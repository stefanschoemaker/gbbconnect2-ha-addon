using CommunityToolkit.Mvvm.ComponentModel;
using GbbLibSmall;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GbbEngine2.Configuration
{
    public partial class Parameters : ObservableObject
    {
        public const string APP_VERSION = "1.2.3";
        public static string APP_ENVIRONMENT = "Library";

        // ======================================
        public PlantList Plants { get; set; } = new();

        //[ObservableProperty]
        //private Plant? m_CurrPlant;

        [ObservableProperty]
        private bool m_Server_AutoStart;

        [ObservableProperty]
        private bool m_IsVerboseLog;

        [ObservableProperty]
        private bool m_IsDriverLog;

        [ObservableProperty]
        private bool m_IsDriverLog2;


        [ObservableProperty]
        private bool m_ClearOldLogs=true;



        // ======================================
        // Save and Load
        // ======================================

        const int VERSION = 1;
        public void WriteToXML(XmlWriter xml)
        {
            xml.WriteStartElement("Parameters");
            xml.WriteAttributeString("Version", VERSION.ToString());

            xml.WriteAttributeString("Server_AutoStart", Server_AutoStart ? "1" : "0");
            xml.WriteAttributeString("IsVerboseLog", IsVerboseLog ? "1" : "0");
            xml.WriteAttributeString("IsDriverLog", IsDriverLog ? "1" : "0");
            xml.WriteAttributeString("IsDriverLog2", IsDriverLog2 ? "1" : "0");
            xml.WriteAttributeString("ClearOldLogs", ClearOldLogs ? "1" : "0");


            //if (CurrPlant!= null)
            //    xml.WriteAttributeString("CurrPlant", XmlConvert.ToString(Plants.IndexOf(CurrPlant)));

            foreach (var itm in Plants)
                itm.WriteToXML(xml);

            xml.WriteEndElement();
        }

        public static Parameters ReadFromXML(XmlReader xml)
        {
            Parameters ret = new();

            if (xml.IsStartElement("Parameters"))
            {
                int Version = int.Parse(xml.GetAttribute("Version") ?? "");
                if (Version > VERSION)
                    throw new ApplicationException("Can't read Parameters from newer program version!");

                string? s;

                s = xml.GetAttribute("Server_AutoStart");
                if (s != null)
                    ret.Server_AutoStart = s=="1";

                s = xml.GetAttribute("IsVerboseLog");
                if (s != null)
                    ret.IsVerboseLog= s=="1";

                s = xml.GetAttribute("IsDriverLog");
                if (s != null)
                    ret.IsDriverLog= s=="1";

                s = xml.GetAttribute("IsDriverLog2");
                if (s != null)
                    ret.IsDriverLog2= s=="1";

                s = xml.GetAttribute("ClearOldLogs");
                if (s != null)
                    ret.ClearOldLogs= s=="1";


                //// for later
                //s = xml.GetAttribute("CurrLoadProfile");


                if (!xml.IsEmptyElement)
                {
                    xml.Read();
                    while (xml.NodeType != XmlNodeType.EndElement && xml.NodeType != XmlNodeType.None)
                    {
                        if (xml.IsStartElement("Plant"))
                            ret.Plants.Add(Plant.ReadFromXML(xml));
                        else
                            xml.Skip();

                        xml.MoveToContent();
                    }
                    xml.ReadEndElement();
                }
                else
                    xml.Skip();

                //// curr plant
                //if (s != null)
                //{
                //    int index = XmlConvert.ToInt32(s);
                //    if (index >= 0 && index < ret.Plants.Count)
                //        ret.CurrPlant = ret.Plants[index];
                //}


            }
            return ret;
        }
        // ======================================
        public void Save()
        {
            Save(Parameters_GetFileName());
        }
        
        public void Save(string FileName) // SaveLocaly
        {
            var tmpFileName = FileName + ".tmp";
            if (System.IO.File.Exists(tmpFileName))
                System.IO.File.Delete(tmpFileName);

            // Create
            var param = new System.Xml.XmlWriterSettings();
            param.Indent = true;
            using var xml = System.Xml.XmlWriter.Create(tmpFileName, param);

            xml.WriteStartDocument();
            WriteToXML(xml);
            xml.WriteEndDocument();

            xml.Close();

            System.IO.File.Move(tmpFileName, FileName, true);

        }



        public static Parameters Load(string FileName)
        {


            Parameters ret;

            if (System.IO.File.Exists(FileName))
            {
                // Parse
                var param = new System.Xml.XmlReaderSettings();
                using var xml = System.Xml.XmlReader.Create(FileName, param);

                ret = ReadFromXML(xml);

            }
            else
            {
                ret = new();
            }


            // init plants
            if (ret.Plants.Count == 0)
            {
                var p = new Plant();
                p.Name = "My Main Plant";
                ret.Plants.Add(p);
            }

            //// curr plant
            //if (ret.CurrPlant == null && ret.Plants.Count > 0)
            //    ret.CurrPlant = ret.Plants[0];

            //// create forecasts
            //foreach (var itm in ret.Plants)
            //    itm.OurRecalcForecast();



            return ret;
        }

        // ======================================
        // Base directory
        // ======================================

        /// <summary>
        /// Base directory for program public data
        /// </summary>
        /// <returns></returns>
        public static string OurGetUserBaseDirectory()
        {
            var Dir=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Dir = Path.Combine(Dir, "GbbConnect2");
            Directory.CreateDirectory(Dir);
            return Dir;
        }

        public static string Parameters_GetFileName()
        {
            string FileName;
            
            var Dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            FileName = Path.Combine(Dir, "Parameters.xml");
            if (File.Exists(FileName))
                return FileName;

            return System.IO.Path.Combine(OurGetUserBaseDirectory(), "Parameters.xml");
        }

        // ======================================
        // Clear log
        // ======================================

        /// <summary>
        /// delete all logs created 2 months ago
        /// </summary>
        public void DoClearOldLogs(IOurLog log)
        {
            if (ClearOldLogs)
            {
                DateTime d = DateTime.Now.AddMonths(-2);

                // directory for log
                string DirName = Path.Combine(OurGetUserBaseDirectory(), "Log");
                Directory.CreateDirectory(DirName);

                var l = Directory.GetFiles(DirName, $"{d:yyyy-MM}*.txt");
                if (l.Length>0)
                    log.OurLog(LogLevel.Information, $"ClearOldLogs: {l.Length} files from month: {d:yyyy-MM}");

                foreach (var File in l)
                {
                        System.IO.File.Delete(File);
                }   

            }

        }


    }
}
