using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GbbEngine2.Configuration
{
    internal class PlantState
    {
        public Plant? Plant;

        // ======================================
        // Plant State

        public DateTime? LastLog_Date { get; set; }
        public long? LastLog_Pos { get; set; }


        // ======================================
        // additional properties

        public MQTTnet.IMqttClient? MqttClient;

        



        // ======================================
        // Save/Load State
        // ======================================
        internal object SaveFileLoc = new();

        public void OurSaveState()
        {
            // options for json serialization
            var SerOpt = new JsonSerializerOptions();
            SerOpt.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;


            // save
            var FileName = OurGetFileName(Plant!);
            var json = JsonSerializer.Serialize(this, SerOpt);
            lock (SaveFileLoc)
            {
                File.WriteAllText(FileName, json);
            }
        }

        public static PlantState OurLoadState(Plant plant)
        {
            PlantState? ret = null;

            var Filename = OurGetFileName(plant);
            if (File.Exists(Filename))
            {
                try
                {
                    var s = File.ReadAllText(Filename);
                    ret = JsonSerializer.Deserialize<PlantState>(s, new JsonSerializerOptions() { IncludeFields = false });
                }
                catch
                {
                }
            }

            if (ret == null)
                ret = new PlantState();
            ret.Plant = plant;

            return ret;
        }


        /// <summary>
        /// File fo PlantStates: "c:\Users\[user]\AppData\Roaming\Gbb Software\GbbConnect\PlantStates\[Number].json"
        /// </summary>
        /// <param name="plant"></param>
        /// <returns></returns>
        private static string OurGetFileName(Plant plant)
        {
            // directory
            string FileName = Path.Combine(Parameters.OurGetUserBaseDirectory(), "PlantStates");
            Directory.CreateDirectory(FileName);

            // file name
            FileName = Path.Combine(FileName, $"{plant.Number:00000}.json");

            return FileName;
        }


    }
}
