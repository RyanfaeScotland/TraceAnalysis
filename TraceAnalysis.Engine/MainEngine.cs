using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TraceAnalysis.Engine
{
    public static class AnalysisEngine
    {
        static volatile bool exit = false;

        public static void FindDifferences(Dictionary<string, int> addressesA, Dictionary<string, int> addressesB)
        {
            var keysDictionaryAHasThatBDoesNot = addressesA.Keys.Except(addressesB.Keys);
            System.Console.WriteLine("In 1 but not in 2:");
            foreach (var address in keysDictionaryAHasThatBDoesNot)
            {
                System.Console.WriteLine(address + ": " + addressesA[address]);
            }
            System.Console.WriteLine("Hit a key to continue...");
            System.Console.ReadKey();
        }

        public static void FindOccurances(string fileName, Dictionary<string, int> addresses)
        {
            string address;
            int value;
            foreach (var line in File.ReadLines(fileName))
            {
                if (line.Length < 8) continue;
                address = line.Substring(0, 8);
                addresses[address] = addresses.TryGetValue(address, out value) ? ++value : 1;
            }

            System.Console.WriteLine(fileName);
            foreach (var addressValuePair in addresses.OrderBy(key => key.Value))
            {
                System.Console.WriteLine(addressValuePair.Key + ": " + addressValuePair.Value);
            }
            System.Console.WriteLine("Hit a key to continue...");
            System.Console.ReadKey();
        }

        public static void ReadConstantly(string fileName)
        {
            int counter = 0;

            Dictionary<string, string> addressesToLines = new Dictionary<string, string>();
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fs))
            {
                Task.Factory.StartNew(() =>
                {
                    exit = false;
                    while (Console.ReadKey().Key != ConsoleKey.Q) ;
                    exit = true;
                });

                while (!exit)
                {
                    if (++counter >= 100)
                    {
                        System.Console.WriteLine(String.Format("Constantly reading file: {0}\nHit Q to Quit back to menu.", fileName));
                        counter = 0;
                    }
                    string line = sr.ReadLine();
                    if (String.IsNullOrEmpty(line))
                    {
                        System.Console.WriteLine("Waiting...");
                        Thread.Sleep(1000);                        
                    } else {
                        System.Console.WriteLine("Reading...");
                        if (line.Length < 8) continue;
                        string parsedLine = ParseTraceFileLine(line);                        
                        string address = line.Substring(0, 8);
                        addressesToLines[address] = parsedLine;
                    }
                }
                var result = addressesToLines.OrderBy(i => i.Key);
                File.WriteAllLines("traceResults.log", result.Select(i=>i.Value));
            }
        }

        private static string ParseTraceFileLine(string line)
        {
            if (line.Contains("RTE") || line.Contains("RTS"))
            {
                line = line + System.Environment.NewLine;
            }
            return line;
        }
    }
}
