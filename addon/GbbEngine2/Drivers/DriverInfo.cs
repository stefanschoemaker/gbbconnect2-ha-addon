namespace GbbEngine2.Drivers
{
    public class DriverInfo
    {
        public enum Drivers
        {
            i000_SolarmanV5 = 0,
            i001_ModbusTCP = 1,
            i999_Random = 999,
        }

        public int DriverNo { get; set; }

        public string Name { get; set; } = "";

        
        public static List<DriverInfo> OurGetDriveInfos()
        {
            List<DriverInfo> ret = new();

            ret.Add(new DriverInfo() { DriverNo = (int)Drivers.i000_SolarmanV5, Name = "SolarmanV5 (wifi-dongle)" });
            ret.Add(new DriverInfo() { DriverNo = (int)Drivers.i001_ModbusTCP, Name = "ModbusTCP (wired-dongle) (BETA)" });
#if DEBUG
            ret.Add(new DriverInfo() { DriverNo = (int)Drivers.i999_Random, Name = "Random" });
#endif

            return ret;
        }

    }
}
