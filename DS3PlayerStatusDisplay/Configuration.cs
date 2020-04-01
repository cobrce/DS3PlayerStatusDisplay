using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DS3Stamina
{
    internal class Configuration
    {
        public string Ip = "127.0.0.1";
        public int Port = 3636;
        public int NumberOfLeds = 80;
        public int Offset = -12;
        public bool Reverse = true;

        static string ConfigPath = Path.Combine(Path.GetDirectoryName(typeof(Configuration).Assembly.Location), "config.json");

        public Configuration()
        {

        }

        public override string ToString()
        {
            return $"IP : {Ip}\nPort : {Port}\nNumber of leds : {NumberOfLeds}\nOffset : {Offset}\nReverse : {Reverse}";
        }

        public static Configuration Load()
        {
            if (File.Exists(ConfigPath))
                try
                {
                    return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigPath));
                }
                catch { }
            var cfg = new Configuration();
            cfg.Save();
            return cfg;
        }

        public bool Save()
        {
            try
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this));
            }
            catch
            {
                return false;
            }
            return true;
        }

        internal bool ConfigParam(string param, string value)
        {
            switch (param.Substring(2).ToLower())
            {
                case "ip":
                    Ip = value;
                    return true;
                case "port":
                    if (int.TryParse(value, out int port))
                    {
                        Port = port;
                        return true;
                    }
                    break;
                case "leds":
                    if (int.TryParse(value, out int leds))
                    {
                        NumberOfLeds = leds;
                        return true;
                    }
                    break;
                case "offset":
                    if (int.TryParse(value, out int offset))
                    {
                        Offset = offset;
                        return true;
                    }
                    break;
                case "reverse":
                    if (int.TryParse(value, out int iRev))
                    {
                        this.Reverse = iRev > 0;
                        return true;
                    }
                    if (bool.TryParse(value, out bool bRev))
                    {
                        this.Reverse = bRev;
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}