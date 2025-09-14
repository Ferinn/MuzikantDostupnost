namespace MuzDost
{
    public struct MuzElement
    {
        public MuzElement(string _name)
        {
            name = _name;
            valueToCode = new Dictionary<string, int>();
        }

        public string name {get; private set;}
        public Dictionary<string, int> valueToCode;
    }

    public struct ConElement
    {
        public ConElement(string _name)
        {
            name = _name;
            indexedValues = new List<string>();
        }

        public string name {get; private set;}
        public List<string> indexedValues;
    }
}