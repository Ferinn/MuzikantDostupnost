namespace MuzDost
{
    using System.Text;
    using System.Text.Json;
    public sealed class ConfigsManager
    {
        // singleton blabber
        private static readonly Lazy<ConfigsManager> lazy =
            new Lazy<ConfigsManager>(() => new ConfigsManager());

        public static ConfigsManager Instance { get { return lazy.Value; } }
        // end of singleton blabber

        // existence of a private constructor is required for correct lazy singleton behaviour 
        private ConfigsManager()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            validEncodings[0] = Encoding.UTF8;
            validEncodings[1] = Encoding.GetEncoding(1250);

            // temp hard coded path
            procConfig = LoadProcesses("");
        }

        // encodings are registered in the constructor
        public static Encoding[] validEncodings = new Encoding[2];

        public ProcessConfig procConfig;

        // Contractors
        public static Contractor[] LoadContractors(Dictionary<string, Process> processes)
        {
            try
            {
                List<Contractor> contractors = new List<Contractor>();
                foreach (Process process in processes.Values)
                {
                    Contractor contractor = new Contractor(process.Name, process);
                    contractors.Add(contractor);
                }
                
                contractors.TrimExcess();
                return contractors.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Program encountered exception: {ex.Message}");
                return new Contractor[0];
            }
        }

        // Processes
            private ProcessConfig LoadProcesses(string procListPath)
            {
                try
                {
                    string json = File.ReadAllText("..\\Config\\ProcessList.json");
                    ProcessConfig? procConfig = JsonSerializer.Deserialize<ProcessConfig>(json);

                    procConfig = procConfig ?? new ProcessConfig();

                    if (procConfig.Processes.Count == 0)
                    {
                        Console.WriteLine("Failed to deserialize processes!");
                    }
                    return procConfig;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new ProcessConfig();
                }
            }

            public Dictionary<string, Process> GetProcDict()
            {
                Dictionary<string, Process> procDict = new Dictionary<string, Process>();
                foreach (Process process in procConfig.Processes)
                {
                    procDict.Add(process.Name, process);
                }

                return procDict;
            }

            public static Encoding GetProcEncoding(Process process)
            {
                return validEncodings[process.Encoding];
            }

            public static string[] GetConElements(Process process)
            {
                List<string> conElements = process.Pairs.Select(pair => pair.conAttribute).ToList();
                if (process.Price.xmlName != "") {conElements.Add(process.Price.xmlName);}
                conElements.Add(process.Availability.xmlName);

                return conElements.ToArray();
            }

            public static string[] GetMuzElements(Process process)
            {
                return process.Pairs.Select(pair => pair.muzAttribute).ToArray();
            }

            public static void PrintProcess(Process process)
            {
                Console.WriteLine($"{process.Name}'s process contains the following pairs:");
                for (int i = 0; i < process.Pairs.Length; i++)
                {
                    Console.WriteLine(process.Pairs[i].conAttribute + " " + process.Pairs[i].muzAttribute);
                }
                Console.WriteLine("-----");
            }
    }
}