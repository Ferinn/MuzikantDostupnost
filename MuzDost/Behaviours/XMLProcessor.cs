namespace MuzDost
{
    using System;
    using System.Text;
    using System.Xml;
    class XMLProcessor
    {
        public XMLProcessor()
        {
            Console.WriteLine("XMLReader initialised!");
            settings = new XmlReaderSettings();
            settings.Async = true;
        }

        private XmlReaderSettings settings;

        public async Task<Dictionary<string, ConElement>> ProcessXML(Contractor contractor, string xmlDataPath, Process process)
        {
            Console.WriteLine($"Began processing XML at {xmlDataPath}");
            Dictionary<string, ConElement> conElements = new Dictionary<string, ConElement>();
            int subRootElementIndex = 0;
            using (XmlReader xr = XmlReader.Create(new StreamReader(xmlDataPath, ConfigsManager.GetProcEncoding(process)), settings))
            {
                try
                {
                    while (await xr.ReadAsync())
                    {
                        contractor.skippedRows.Add(true);
                        conElements = await ScanAndLoad(conElements, xr, process);
                        subRootElementIndex++;
                    }
                    foreach (ConElement element in conElements.Values)
                    {
                        Console.WriteLine($"{contractor.name} has: {element.indexedValues.Count} entries for element: {element.name}.");
                    }
                    Console.WriteLine($"Sucessfully finished processing XML at {xmlDataPath}");
                    return conElements;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occured while processing XML at: {xmlDataPath}, at {subRootElementIndex}\n" + ex);
                    return new Dictionary<string, ConElement>();
                }
            }
        }

        private async Task<Dictionary<string, ConElement>> ScanAndLoad(Dictionary<string, ConElement> conElements, XmlReader xr, Process process)
        {
            if (xr.NodeType == XmlNodeType.Element)
            {
                string[] searchedElements = ConfigsManager.GetConElements(process);
                for (int i = 0; i < searchedElements.Length; i++)
                {
                    if (String.Equals(xr.Name, searchedElements[i], StringComparison.OrdinalIgnoreCase))
                    {
                        string uniformName = searchedElements[i];
                        if (!conElements.ContainsKey(uniformName))
                        {
                            conElements.Add(uniformName, new ConElement(uniformName));
                        }

                        string value = xr.Value == string.Empty ? await ExtractValue(xr, uniformName) : xr.Value;
                        conElements[uniformName].indexedValues.Add(value);
                    }
                }
            }
            return conElements;
        }

        private async Task<string> ExtractValue(XmlReader xr, string elementName)
        {
            while (await xr.ReadAsync())
            {
                if (xr.Name.ToLower() == elementName)
                {
                    return string.Empty;
                }
                if (xr.NodeType == XmlNodeType.Text)
                {
                    return xr.Value;
                }
                if (xr.NodeType == XmlNodeType.CDATA)
                {
                    return xr.Value.Replace(";", "");
                }
            }
            return string.Empty;
        }
    }
}