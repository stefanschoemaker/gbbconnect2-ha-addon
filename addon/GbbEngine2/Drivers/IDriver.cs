using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GbbEngine2.Drivers
{
    public interface IDriver
    {
        public Task<byte[]> ReadHoldingRegister(byte unit, ushort startAddress, ushort numInputs);
        public Task WriteMultipleRegister(byte unit, ushort startAddress, byte[] values);

        public Task<byte[]> SendDataToDevice(byte[] data);  

        public void Dispose();

    }


}
