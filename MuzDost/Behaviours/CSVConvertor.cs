namespace MuzDost
{
    using System.Globalization;
    using System.Xml.Linq;
    using CsvHelper;
    using CsvHelper.Configuration;
    using System.IO;

    class CSVConvertor
    {
        public async Task<bool> ConvertAsync(string csvSource, string xmlTarget, Process process)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = process.Separator,
                    Encoding = ConfigsManager.GetProcEncoding(process),
                    BadDataFound = null,
                    MissingFieldFound = null
                };

                var xmlDoc = new XElement("XML-File",
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance")
                );

                using (var fileStream = new FileStream(csvSource, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                using (var reader = new StreamReader(fileStream, config.Encoding))
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<dynamic>();

                    await Task.Run(() =>
                    {
                        foreach (var record in records)
                        {
                            var row = new XElement("row");
                            var usedXmlNames = new HashSet<string>();

                            var dict = (IDictionary<string, object>)record;
                            object? value;

                            foreach (Pair pair in process.Pairs)
                            {
                                string fieldName = pair.csvAlias;
                                string xmlElementName = pair.conAttribute;

                                if (!usedXmlNames.Add(xmlElementName)) continue;

                                dict.TryGetValue(fieldName, out value);

                                row.Add(new XElement(xmlElementName, value ?? ""));
                            }

                            if (!string.IsNullOrWhiteSpace(process.Price.csvName) &&
                                usedXmlNames.Add(process.Price.xmlName))
                            {
                                dict.TryGetValue(process.Price.csvName, out value);
                                row.Add(new XElement(process.Price.xmlName, value ?? ""));
                            }

                            if (!string.IsNullOrWhiteSpace(process.Availability.csvName) &&
                                usedXmlNames.Add(process.Availability.xmlName))
                            {
                                dict.TryGetValue(process.Availability.csvName, out value);
                                row.Add(new XElement(process.Availability.xmlName, value ?? ""));
                            }

                            xmlDoc.Add(row);
                        }
                    });
                }

                var xmlDocument = new XDocument(xmlDoc);


                using (var stream = new FileStream(xmlTarget, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                {
                    xmlDocument.Save(writer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"While saving the conversion of {xmlTarget} CSV, the following error occurred:\n{ex.Message}");
                return false;
            }
            return true;
        }
    }
}