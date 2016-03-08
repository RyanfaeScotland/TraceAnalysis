using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TraceAnalysis.Engine
{
    public class TraceFile
    {
        public string nameAndPath { get; set; }
        public Dictionary<string, string> addressesToLines { get; set; }
        public Dictionary<string, int> addressesFrequency { get; set; }
        public bool stopLoadingTraceFile;
        public TraceFile(string nameAndPath)
        {
            this.nameAndPath = nameAndPath;
            addressesToLines = new Dictionary<string, string>();
            addressesFrequency = new Dictionary<string, int>();
        }

        public void LoadTraceFile()
        {
            var fs = new FileStream(nameAndPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fs))
            {
                Task.Factory.StartNew(() =>
                {
                    stopLoadingTraceFile = false;
                    Console.WriteLine("Reading Trace File, press A to Abort");
                    while (Console.ReadKey().Key != ConsoleKey.A) Thread.Sleep(1);
                    stopLoadingTraceFile = true;
                });

                while (!stopLoadingTraceFile)
                {
                    string line = sr.ReadLine();
                    if (line.Length < 8) continue;
                    string address = line.Substring(0, 8);
                    addressesToLines[address] = line;
                    Thread.Sleep(1);
                }
            }
        }

        public void FindAddressFrequency()
        {
            string address;
            int frequency;
            foreach (var line in File.ReadLines(nameAndPath))
            {
                if (line.Length < 8) continue;
                address = line.Substring(0, 8);
                addressesFrequency[address] = addressesFrequency.TryGetValue(address, out frequency) ? ++frequency : 1;
            }
        }

        public void WriteAddressFrequencyToConsole()
        {
            System.Console.WriteLine(String.Format("Frequency of address access in {0}", nameAndPath));
            foreach (var addressValuePair in addressesFrequency.OrderBy(key => key.Value))
            {
                System.Console.WriteLine(addressValuePair.Key + ": " + addressValuePair.Value);
            }
        }

        public void FindDifferences(TraceFile otherTraceFile)
        {
            var addressesThisFileHasThatOtherDoesNot = addressesToLines.Keys.Except(otherTraceFile.addressesToLines.Keys);
            System.Console.WriteLine("In 1 but not in 2:");
            foreach (var address in addressesThisFileHasThatOtherDoesNot)
            {
                System.Console.WriteLine(address);
            }
        }
    }
}