namespace MuzDost
{
    public class Contractor
    {
        public Contractor(string _name, Process _process)
        {
            name = _name;
            xmlDataPath = "";
            success = false;
            process = _process;
            conElements = new Dictionary<string, ConElement>();
            skippedRows = new List<bool>();
        }
        public string name;
        public string xmlDataPath;
        public bool success;

        public Process process;
        Dictionary<string, ConElement> conElements;
        public ConElement prices;
        public List<bool> skippedRows;

        public async Task LoadData()
        {
            XMLProcessor XMLProcessor = new XMLProcessor();
            conElements = await XMLProcessor.ProcessXML(this, xmlDataPath, process);
            
            if (process.Price.xmlName != "")
            {
                conElements.TryGetValue(process.Price.xmlName, out prices);
            }
            try
            {
                conElements.Remove(process.Price.xmlName);
            }
            catch{}
            
            PrintSamples();
        }

        public ConElement GetElement(string name)
        {
            ConElement element;
            conElements.TryGetValue(name, out element);

            return element;
        }

        private void PrintSamples()
        {
            Console.WriteLine("-----");
            foreach (ConElement conElement in conElements.Values)
            {
                Console.WriteLine($"{name}'s {conElement.name}:");
                for (int i = 0; i < 10 && i < conElement.indexedValues.Count; i++)
                {
                    Console.WriteLine(conElement.indexedValues[i]);
                }
                Console.WriteLine("-----");
            }
        }
    }
}