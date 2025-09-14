using System.Text;
using System.Diagnostics;

namespace MuzDost
{
    class MuzData
    {
        public MuzData(string muzDataPath)
        {

            //                                      static elementCount
            muzData = new Dictionary<string, MuzElement>(elementCount);

            Stopwatch timer = Stopwatch.StartNew();
            // loading and parsing muzikant data, with win1250 encoding
            LoadData(muzDataPath, ConfigsManager.validEncodings[1]);
            timer.Stop();
            Console.WriteLine($"Muzikant Data loaded in {timer.Elapsed}");
        }

        //        elementName/columnName | muzikantCode, value
        private Dictionary<string, MuzElement> muzData;
        private static int elementCount = 8;

        public MuzElement GetElement(string name)
        {
            MuzElement element;
            muzData.TryGetValue(name, out element);

            return element;
        }

        private void LoadData(string muzDataPath, Encoding enc1250)
        {
            int debug_RowIndex = 0;
            string debug_Row = "";
            int debug_elementIndex = 0;
            try
            {
                using (StreamReader sr = new StreamReader(muzDataPath, enc1250))
                {
                    // tokenizing header
                    string? temp = sr.ReadLine();
                    string[] elementNames = temp == null ? new string[0] : temp.Split(';');

                    // initialising muElements
                    MuzElement[] muzElements = new MuzElement[8];
                    for (int i = 0; i < elementNames.Length; i++)
                    {
                        muzElements[i] = new MuzElement(elementNames[i]);
                        muzData.Add(muzElements[i].name, muzElements[i]);
                    }

                    Console.WriteLine("muzElements initiliased!");
                    // iterating through rows
                    while (!sr.EndOfStream)
                    {
                        // handling edge case where row doesn't start with muzCode
                        int rowShift = 0;

                        // checking for null read
                        string? tempRow = sr.ReadLine();
                        string[] row = tempRow == null ? new string[0] : tempRow.Split(';');

                        if (row == null || row[0] == "")
                        {
                            debug_RowIndex++;
                            continue;
                        }

                        debug_Row = tempRow ?? "";
                        int muzCode = 0;
                        bool hasCode = true;
                        while (!int.TryParse(row[rowShift], out muzCode))
                        {
                            rowShift++;
                            if (rowShift >= row.Length)
                            {
                                hasCode = false;
                                break;
                            }
                        }
                        if (!hasCode)
                        {
                            debug_RowIndex++;
                            continue;
                        }
                        
                        // seperating row data into correct muzElements
                        for (int columnIndex = 0; columnIndex < elementNames.Length; columnIndex++)
                        {
                            // skipping muzCode
                            int inRowIndex = (columnIndex + rowShift) % row.Length;
                            debug_elementIndex = inRowIndex;

                            // checking that the value has not been added yet
                            if (!muzElements[columnIndex].valueToCode.ContainsKey(row[inRowIndex]) &&
                                row[inRowIndex] != "")
                            {
                                // adding value to 'valueToCodes'
                                muzElements[columnIndex].valueToCode.Add(row[inRowIndex], muzCode);
                            }
                        }

                        debug_RowIndex++;
                        debug_elementIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"While loading and parsing muzikant data at path: {muzDataPath}, with reader at rowIndex: {debug_RowIndex}, elementIndex: {debug_elementIndex}, row:\n"
                + $"{debug_Row}\n"
                + $"the following error occured: {ex.Message}");
                return;
            }
        }
    }
}