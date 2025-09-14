namespace MuzDost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    class Program
    {
        public static string downloadDir = "..\\Downloads\\";
        public static string resultDir = "..\\Results\\";



        static void Main(string[] args)
        {
            // creating downloads folder if it doesn't exist in this directory
            if (!Directory.Exists("..\\Downloads")) { Directory.CreateDirectory("..\\Downloads"); }
            if (!Directory.Exists("..\\Results")) { Directory.CreateDirectory("..\\Results"); }

            string procPath = "..\\Config\\ProcessList.json";
            string muzDataPath = "..\\Docs\\MuzikantData.csv";

            // checks the paths and asks user to correct them if they're invalid
            ValidatePaths(ref procPath, ref muzDataPath);

            // loading processes from specified process list
            Dictionary<string, Process> processes = ConfigsManager.Instance.GetProcDict();
            if (processes.Count == 0)
            {
                Console.WriteLine("No processes loaded.");
                return;
            }
            else
            {
                Console.WriteLine("-----");
                foreach (Process process in processes.Values)
                {
                    ConfigsManager.PrintProcess(process);
                }
            }

            // loading contractors from specified contractor list
            Contractor[] contractors = ConfigsManager.LoadContractors(processes);
            if (contractors.Length == 0)
            {
                Console.WriteLine("No contractors loaded.");
                return;
            }

            // processing succesfully loaded non-zero contractor list
            else
            {
                Console.WriteLine($"{contractors.Length} contractors loaded");
                TaskManager taskMngr = new TaskManager();
                taskMngr.Process(contractors, muzDataPath).Wait();
            }
        }

        static void ValidatePaths(ref string procPath, ref string muzDataPath)
        {
            bool validProcPath = File.Exists(procPath);
            bool validMuzDataPath = File.Exists(muzDataPath);
            
            while (!validProcPath || !validMuzDataPath)
            {
                if (!validProcPath)
                {
                    Console.WriteLine("Please enter a valid relative path to ProcessList.json");
                    // null check
                    string? temp = Console.ReadLine();
                    procPath = temp ?? "";
                }
                if (!validMuzDataPath)
                {
                    Console.WriteLine("Please enter a valid relative path to MuzikantData.csv");
                    // null check
                    string? temp = Console.ReadLine();
                    muzDataPath = temp ?? "";
                }
                validProcPath = File.Exists(procPath);
                validMuzDataPath = File.Exists(muzDataPath);
            }
        }
    }
}
