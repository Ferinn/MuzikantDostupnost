using System.Text.Json.Serialization;

namespace MuzDost
{
    public class ProcessConfig
    {
        [JsonInclude] public List<Process> Processes = new List<Process>();
    }

    public struct Process
    {
        public string Name {get; set;}
        public string UrlAdress {get; set;}
        public string LocalAdress {get; set;}
        public int Encoding {get; set;}
        //0 => UTF-8
        //1 => winlatin-1250
        public DataElement Price {get; set;}
            public int PriceToAvailType {get; set;}
            //0 => 0-999 = externí sklad4,
            //0 => 1000-4999 = externí sklad,
            //0 => 5000+ = kudihou
            //---
            //1 => 0-1999 = externí sklad4,
            //1 => 2000-7999 = externí sklad,
            //1 => 8000+ = kudihou
        public DataElement Availability {get; set;}
            public int AvailType {get; set;}
            //0 => stockAmount
            //1 => true, false
            //2 => ANO, NE
            //3 => delivery date
            //TBD
        public int IsXML {get; set;}
        // 1 = source is XML
        // 0 = source is CSV
            public string Separator {get; set;}
        public Pair[] Pairs {get; set;}
    }
        public struct Pair
        {
            public string conAttribute {get; set;}
            public string muzAttribute {get; set;}
            public string csvAlias {get; set;}
        }

        public struct DataElement
        {
            public string xmlName {get; set;}
            public string csvName {get; set;}
        }
}