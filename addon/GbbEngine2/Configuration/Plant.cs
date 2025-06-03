using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;

namespace GbbEngine2.Configuration
{
    public partial class Plant : ObservableObject
    {

        [ObservableProperty]
        private string m_Name = "";

        // ======================================
        // Inverter
        // ======================================

        [ObservableProperty]
        private int m_Number = 1;


        [ObservableProperty]
        private int m_DriverNo;

        [ObservableProperty]
        private int m_IsDisabled = 0;

        [ObservableProperty]
        private string? m_AddressIP;

        [ObservableProperty]
        private int? m_PortNo = 8899;

        [ObservableProperty]
        private long? m_SerialNumber;

        // ======================================
        // GbbOptimizer
        // ======================================

        [ObservableProperty]
        private string? m_GbbOptimizer_PlantId;

        [ObservableProperty]
        private string? m_GbbOptimizer_PlantToken;

        [ObservableProperty]
        private string? m_GbbOptimizer_Mqtt_Address = "gbboptimizerX-mqtt.gbbsoft.pl";

        [ObservableProperty]
        private int? m_GbbOptimizer_Mqtt_Port = 8883;

        // ======================================
        // Services state
        // ======================================

        internal PlantState? PlantState;


        // ======================================
        // Constructor
        // ======================================
        //public ForecastList Forecasts { get; init; }

        public Plant()
        {
            //this.Forecasts = new ForecastList(this);
        }


        //public bool Cerbo_UseDirectConn
        //{
        //    get
        //    {
        //        return !Cerbo_UseVRMConn;
        //    }
        //    set
        //    {
        //        Cerbo_UseVRMConn = !value;
        //    }
        //}

        // ======================================
        const int VERSION = 1;
        const string Key = "xLwh1WTkQX9nH8vIsg6bmS+qCQTTBlVXOvXb3WLhMcw=";
        const string IV = "/O0Hsgp3epBwF0yaawcP37==";

        public void WriteToXML(XmlWriter xml)
        {
            var cipher = GbbLib.Encryption.AES_CreateCipher(Key);

            xml.WriteStartElement("Plant");
            xml.WriteAttributeString("Version", VERSION.ToString());


            xml.WriteAttributeString("Number", Number.ToString());
            xml.WriteAttributeString("Name", Name);
            xml.WriteAttributeString("DriverNo", DriverNo.ToString());
            xml.WriteAttributeString("IsDisabled", IsDisabled.ToString());

            if (AddressIP!=null)
                xml.WriteAttributeString("AddressIP", AddressIP);
            if (PortNo!=null)
                xml.WriteAttributeString("PortNo", PortNo.ToString());
            if (SerialNumber!=null)
                xml.WriteAttributeString("SerialNumber", SerialNumber.ToString());

            if (GbbOptimizer_PlantId != null)
                xml.WriteAttributeString("GbbOptimizer_PlantId", GbbOptimizer_PlantId.ToString());
            if (GbbOptimizer_PlantToken != null)
                xml.WriteAttributeString("GbbOptimizer_PlantToken", GbbOptimizer_PlantToken);
            if (GbbOptimizer_Mqtt_Address != null)
                xml.WriteAttributeString("GbbOptimizer_Mqtt_Address", GbbOptimizer_Mqtt_Address);
            if (GbbOptimizer_Mqtt_Port != null)
                xml.WriteAttributeString("GbbOptimizer_Mqtt_Port", GbbOptimizer_Mqtt_Port.ToString());

            //if (Password != null)
            //    xml.WriteAttributeString("Password", GbbLib.Encryption.AES_Encrypt(cipher, IV, Password));
            //xml.WriteAttributeString("Cerbo_UseVRMConn", XmlConvert.ToString(Cerbo_UseVRMConn));

            //foreach (var itm in Forecasts)
            //    itm.WriteToXML(xml);


            xml.WriteEndElement();
        }

        public static Plant ReadFromXML(XmlReader xml)
        {
            Plant ret = new();

            if (xml.IsStartElement("Plant"))
            {
                var cipher = GbbLib.Encryption.AES_CreateCipher(Key);

                int Version = int.Parse(xml.GetAttribute("Version") ?? "");
                if (Version > VERSION)
                    throw new ApplicationException("Can't read Plant from newer program version!");

                string? s;
                int i;
                long l;

                s = xml.GetAttribute("Number");
                if (s != null && int.TryParse(s, out i))
                    ret.Number = i;

                ret.Name = xml.GetAttribute("Name") ?? "";

                s = xml.GetAttribute("DriverNo");
                if (s != null && int.TryParse(s, out i))
                    ret.DriverNo = i;

                s = xml.GetAttribute("IsDisabled");
                if (s != null && int.TryParse(s, out i))
                    ret.IsDisabled = i;

                ret.AddressIP = xml.GetAttribute("AddressIP");

                s = xml.GetAttribute("PortNo");
                if (s != null && int.TryParse(s, out i))
                    ret.PortNo = i;

                s = xml.GetAttribute("SerialNumber");
                if (s != null && long.TryParse(s, out l))
                    ret.SerialNumber = l;


                ret.GbbOptimizer_PlantId = xml.GetAttribute("GbbVictronWeb_PlantId") ?? xml.GetAttribute("GbbOptimizer_PlantId");
                ret.GbbOptimizer_PlantToken = xml.GetAttribute("GbbVictronWeb_PlantToken") ?? xml.GetAttribute("GbbOptimizer_PlantToken");
                ret.GbbOptimizer_Mqtt_Address = xml.GetAttribute("GbbVictronWeb_Mqtt_Address") ?? xml.GetAttribute("GbbOptimizer_Mqtt_Address");

                s = xml.GetAttribute("GbbVictronWeb_Mqtt_Port") ?? xml.GetAttribute("GbbOptimizer_Mqtt_Port");
                if (s != null && int.TryParse(s, out i))
                    ret.GbbOptimizer_Mqtt_Port = i;

                if (!xml.IsEmptyElement)
                {
                    //List<Schedule> schedules = new List<Schedule>();

                    xml.Read();
                    while (xml.NodeType != XmlNodeType.EndElement && xml.NodeType != XmlNodeType.None)
                    {
                        //if (xml.IsStartElement("Forecast"))
                        //    ret.Forecasts.Add(Forecast.ReadFromXML(xml));
                        //else
                            xml.Skip();

                        xml.MoveToContent();
                    }
                    xml.ReadEndElement();

                    //// sort schcedules
                    //foreach (var itm in schedules.OrderBy(q => q.Number))
                    //    ret.Schedules.Add(itm);

                }
                else
                    xml.Skip();

                // curr profile
                //if (sCurrLoadProfile != null)
                //{
                //    int index = XmlConvert.ToInt32(sCurrLoadProfile);
                //    if (index >= 0 && index < ret.LoadProfiles.Count)
                //        ret.m_CurrLoadProfile = ret.LoadProfiles[index];
                //}


            }
            return ret;
        }
        // =======================================
        // Operations
        // ======================================

        public void OurCheckDataForUI()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ApplicationException("Name can't be empty!");
            }

            //CheckSellingPrices();

        }

    }
}