using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading;
using GbbLibSmall;

namespace GbbEngine2.Drivers.SolarmanV5
{
    public partial class ModbusTcpDriver : IDisposable , IDriver
    {

        private const int WAIT_AFTER_DISCONNECT = 500; // ms

        public int Timeout { get; set; } = 1000; // 1 sec

        Configuration.Parameters Parameters;
        GbbLibSmall.IOurLog? OurLog;

        private string IpAddress;
        private int PortNumber;
        private long SerialNumber;

        private Socket? Socket;

        public ModbusTcpDriver( Configuration.Parameters _Parameters, string? _IPAddress, int? _PortNumber, long? _SerialNumber, IOurLog? ourLog)
        {
            if (_IPAddress == null)
                throw new ApplicationException("GbbConnect2: Missing IP Address");
            if (_PortNumber == null)
                throw new ApplicationException("GbbConnect2: Missing Port Number");
            if (_SerialNumber== null)
                throw new ApplicationException("GbbConnect2: Missing SerialNumber");

            Parameters = _Parameters;
            IpAddress = _IPAddress;
            PortNumber = _PortNumber.Value;
            SerialNumber = _SerialNumber.Value;
            OurLog = ourLog;

            //Connect();
        }

        public void Connect()
        {
            ArgumentNullException.ThrowIfNull(IpAddress);

            if (Socket != null && Socket.Connected)
            {
                Socket.Close();
                Socket.Dispose();
                Socket = null;
            }

            IPAddress? _ip;
            if (IPAddress.TryParse(IpAddress, out _ip) == false)
            {
                IPHostEntry hst = Dns.GetHostEntry(IpAddress);
                _ip= hst.AddressList[0];
            }
            Socket = new Socket(_ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket.SendTimeout = Timeout;
            Socket.ReceiveTimeout = Timeout;
            Socket.NoDelay = true;
            Socket.Connect(new IPEndPoint(_ip, PortNumber));


        }

        // ======================================
        // IDisposable
        // ======================================
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Socket != null)
                    {
                        try { Socket.Shutdown(SocketShutdown.Both); }
                        catch { }
                        Socket.Close();
                        Socket.Dispose();
                        Socket = null;  
                    }
                }
                disposed = true;
            }
        }

        // ======================================
        //
        //

        public async Task<byte[]> ReadHoldingRegister(byte unit, ushort startAddress, ushort numInputs)
        {
            if (numInputs > 125)
                throw new ApplicationException("Too much registers to read!");
            return await WriteSyncData(ModBus.CreateReadHeader(unit, startAddress, numInputs, 3), false, null, startAddress);
        }

        public async Task WriteMultipleRegister(byte unit, ushort startAddress, byte[] values)
        {
            if (values.Length > 250)
                throw new ApplicationException("Too much registers to write!");

            ushort numBytes = Convert.ToUInt16(values.Length);
            if (numBytes % 2 > 0) numBytes++;
            var AddressCount = numBytes / 2;

            // list of changed registers
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < AddressCount; i++)
            {
                sb.Append(startAddress + i);
                sb.Append('=');
                sb.Append(values[2 * i] * 256 + values[2 * i + 1]);
                sb.Append(", ");
            }

            byte[] data = ModBus.CreateWriteHeader(unit, startAddress, Convert.ToUInt16(AddressCount), Convert.ToUInt16(numBytes), 16);
            Array.Copy(values, 0, data, 7, values.Length);
            var crc = ModBus.GetCRC(data);
            data[data.Length - 2] = crc[0];
            data[data.Length - 1] = crc[1];
            await WriteSyncData(data, true, sb.ToString(), startAddress);
        }


        // ------------------------------------------------------------------------
        // Write data and and wait for response
        private const int WAIT_READ_TIME_MS = 100; // ms
        private const int WAIT_WRITE_TIME_MS = 3000; // 3s, 8s
        private DateTime? LastSend;

        private async Task<byte[]> WriteSyncData(byte[] write_data, bool iswrite, string? LogSufix, int StartAddress)
        {
            ArgumentNullException.ThrowIfNull(Socket);

            if (Socket.Connected)
            {
                //tb.AppendText($"{DateTime.Now}: Send ModBus: {BitConverter.ToString(write_data)}\r\n");

                // up to 50ms delay
                if (LastSend != null)
                {
                    int DelayMs;
                    if (iswrite)
                        DelayMs = WAIT_WRITE_TIME_MS;
                    else
                        DelayMs = WAIT_READ_TIME_MS;

                    var ms = (int)(LastSend.Value.AddMilliseconds(DelayMs) - DateTime.Now).TotalMilliseconds;
                    if (ms > 0)
                    {
                        if (Parameters.IsDriverLog && OurLog != null)
                        {
                            OurLog.OurLog(LogLevel.Information, $"Delay: {ms}ms");
                        }
                        await Task.Delay(ms);
                    }
                }

                byte[] Buf = await SendDataToDevice(write_data);

                // Check Modbus CRC
                var crc = ModBus.GetCRC(Buf);
                if (crc[0] != Buf[Buf.Length - 2]
                 || crc[1] != Buf[Buf.Length - 1])
                {
                    if (Parameters.IsDriverLog && OurLog != null)
                        OurLog.OurLog(LogLevel.Information, $"Received ModBus: {BitConverter.ToString(Buf)}");
                    throw new ApplicationException("SolarmanV5: Wrong CRC!");
                }

                // get function
                byte function = Buf[1];
                byte[] ret;


                // ------------------------------------------------------------
                // Response data is slave exception
                if (function > 128)
                {
                    if (Parameters.IsDriverLog && OurLog != null)
                        OurLog.OurLog(LogLevel.Information, $"Received ModBus: {BitConverter.ToString(Buf)}");

                    function -= 128;
                    throw new ApplicationException($"Error response: function: {function}, error={Buf[2]}");
                }
                // ------------------------------------------------------------
                // Write response data
                else if ((function >= 5) && (function != 23))
                {
                    if (Parameters.IsDriverLog && OurLog != null)
                        OurLog.OurLog(LogLevel.Information, $"Received ModBus: {BitConverter.ToString(Buf)}");

                    //data = new byte[2];
                    //Array.Copy(Buf, 3, data, 0, 2);
                    ret = Buf;
                }
                // ------------------------------------------------------------
                // Read response data
                else
                {
                    var len = Buf[2];
                    ret = new byte[len];
                    Array.Copy(Buf, 3, ret, 0, len);


                    if (Parameters.IsDriverLog && OurLog != null)
                    {
                        // list of changed registers
                        var no = len / 2;
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < no; i++)
                        {
                            sb.Append(StartAddress + i);
                            sb.Append('=');
                            sb.Append(ret[2 * i] * 256 + ret[2 * i + 1]);
                            sb.Append(", ");
                        }
                        OurLog.OurLog(LogLevel.Information, $"Received ModBus: {sb.ToString()}: {BitConverter.ToString(Buf)}");
                    }
                }

                LastSend = DateTime.Now;

                return ret;
            }
            else
                throw new ApplicationException("Connection closed!");
        }

        
        // ======================================
        // Sequence Number
        //

        private UInt16? SequenceNumber;

        private byte[] GetNextSequenceNumber()
        {
            if (SequenceNumber == null)
                SequenceNumber = (UInt16)new System.Random().Next(1, 65535);
            else
                SequenceNumber = (UInt16)((SequenceNumber +1 ) & 0xffff);

            return BitConverter.GetBytes(SequenceNumber.Value);
        }



        public async Task<byte[]> SendDataToDevice(byte[] data)
        {
            return await SendDataToDevice(data, null);
        }   

        public async Task<byte[]> SendDataToDevice(byte[] modBusData, string? LogSufix)
        {
            ArgumentNullException.ThrowIfNull(Socket);

            byte[] modBusTcp = new byte[modBusData.Length + 6 - 2];

            // TransactionId
            var TranId = GetNextSequenceNumber();
            modBusTcp[0] = TranId[0];
            modBusTcp[1] = TranId[1];

            // ProtocolId
            modBusTcp[2] = 0;
            modBusTcp[3] = 0;

            // Length
            var len = (UInt16)(modBusData.Length-2);
            modBusTcp[4] = (byte)(len>>8);
            modBusTcp[5] = (byte)(len & 0xff);

            // copy modBusData without CRC
            Array.Copy(modBusData, 0, modBusTcp, 6, modBusData.Length-2);

            // Send
            if (Parameters.IsDriverLog && OurLog != null)
            {
                if (LogSufix != null)
                    OurLog.OurLog(LogLevel.Information, $"Send ModBusTCP: {LogSufix}: {BitConverter.ToString(modBusTcp)} ");
                else
                    OurLog.OurLog(LogLevel.Information, $"Send ModBusTCP: {BitConverter.ToString(modBusTcp)}");
            }
            var InBuf  = await InternalSend(modBusTcp);

            // check TransactionId
            if (InBuf.Length < 10)
                throw new ApplicationException($"ModBusTCP: Response too short! Length={InBuf.Length}");
            if (InBuf[0] != TranId[0]
                || InBuf[1] != TranId[1])
                throw new ApplicationException("ModBusTCP: Wrong TransactionId!");

            // check error
            if (InBuf[8] > 127)
            {
                string error = "??";
                switch(InBuf[9])
                {
                    case 0x01: error = "Illegal Function"; break;
                    case 0x02: error = "Illegal Data Address"; break;
                    case 0x03: error = "Illegal Data Value"; break;
                    case 0x04: error = "Slave Device Failure"; break;
                    case 0x05: error = "Acknowledge"; break;
                    case 0x06: error = "Slave Device Busy"; break;
                    case 0x08: error = "Memory Parity Error"; break;
                    case 0x0A: error = "Gateway Path Unavailable"; break;
                    case 0x0B: error = "Gateway Target Device Failed to Respond"; break;
                }
                throw new ApplicationException($"Error response: {InBuf[9]}={error}");
            }

            // get len
            var len2 = (UInt16)(InBuf[4] << 8 | InBuf[5]);

            // get ModBus data
            byte[] ret = new byte[len2+2];
            Array.Copy(InBuf, 6, ret, 0, len2);
            
            // add CRC
            var crc = ModBus.GetCRC(ret);
            ret[len2] = crc[0];
            ret[len2+1] = crc[1];

            return ret;
        }

        private readonly static SemaphoreSlim _lock = new (1, 1);

        private async Task<byte[]> InternalSend(byte[] OutBuf)
        {
            ArgumentNullException.ThrowIfNull(Socket);

            await _lock.WaitAsync();
            try
            {
                for (int j = 1; j <= 11; j++)
                {
                    try
                    {
                        // send
                        int bytesSent = await Socket.SendAsync(OutBuf);

                        // receive
                        byte[] buffer = new byte[1024];
                        int bytesReceived = Socket.Receive(buffer); // not-async to wait only 5 sec
                        if (bytesReceived == 0)
                            throw new ApplicationException("Connection Lost (received 0 bytes)");
                        byte[] InBuf = new byte[bytesReceived];
                        Buffer.BlockCopy(buffer, 0, InBuf, 0, bytesReceived);

                        // log
                        if (Parameters.IsDriverLog2 && OurLog != null)
                        {
                            OurLog.OurLog(LogLevel.Information, $"Received ModBusTCP: {BitConverter.ToString(InBuf)}");
                        }
                        return InBuf;
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        if (Parameters.IsDriverLog && OurLog != null)
                            OurLog.OurLog(LogLevel.Information, $"Send error: {ex.Message}");

                        if (j == 11)
                            throw;

                        if (Parameters.IsDriverLog && OurLog != null)
                            OurLog.OurLog(LogLevel.Information, $"Retry: {j}");
                        Socket.Close();
                        Thread.Sleep(WAIT_AFTER_DISCONNECT);
                        Connect();
                    }
                }
                return []; // never return
            }
            finally
            {
                // Always release the lock, error or success.
                _lock.Release();
            }


        }

    }


}
