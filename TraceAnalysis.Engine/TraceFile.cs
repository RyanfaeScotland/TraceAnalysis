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
        public Dictionary<string, TraceLine> addressesToLines { get; set; }
        public Dictionary<string, int> addressesFrequency { get; set; }
        public bool stopLoadingTraceFile;
        public TraceFile(string nameAndPath)
        {
            this.nameAndPath = nameAndPath;
            addressesToLines = new Dictionary<string, TraceLine>();
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
                    if (line.Length < 14) continue;
                    string address = line.Substring(0, 8);
                    addressesToLines[address] = new TraceLine(line);
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

    public class TraceLine
    {
        string address;
        string opcode;
        string instruction;
        string A0, A1, A2, A3, A4, A5, A6, A7;
        string D0, D1, D2, D3, D4, D5, D6, D7;
        string flags;

        public TraceLine(string rawLine)
        {
            //00:0202  4A B9  TST.L   ($00A10008)              A0=00000000 A1=00000000 A2=00000000 A3=00000000 A4=00000000 A5=00000000 A6=00000000 A7=00FF8000 D0=00000000 D1=00000000 D2=00000000 D3=00000000 D4=00000000 D5=00000000 D6=00000000 D7=00000000 xnzvc
            address = rawLine.Substring(0, 7);
            opcode = rawLine.Substring(9, 5);
            instruction = rawLine.Substring(16, 25).Trim();
            A0 = rawLine.Substring(rawLine.IndexOf("A0") + 3, 8);
            A1 = rawLine.Substring(rawLine.IndexOf("A1") + 3, 8);
            A2 = rawLine.Substring(rawLine.IndexOf("A2") + 3, 8);
            A3 = rawLine.Substring(rawLine.IndexOf("A3") + 3, 8);
            A4 = rawLine.Substring(rawLine.IndexOf("A4") + 3, 8);
            A5 = rawLine.Substring(rawLine.IndexOf("A5") + 3, 8);
            A6 = rawLine.Substring(rawLine.IndexOf("A6") + 3, 8);
            A7 = rawLine.Substring(rawLine.IndexOf("A7") + 3, 8);
            D0 = rawLine.Substring(rawLine.IndexOf("D0") + 3, 8);
            D1 = rawLine.Substring(rawLine.IndexOf("D1") + 3, 8);
            D2 = rawLine.Substring(rawLine.IndexOf("D2") + 3, 8);
            D3 = rawLine.Substring(rawLine.IndexOf("D3") + 3, 8);
            D4 = rawLine.Substring(rawLine.IndexOf("D4") + 3, 8);
            D5 = rawLine.Substring(rawLine.IndexOf("D5") + 3, 8);
            D6 = rawLine.Substring(rawLine.IndexOf("D6") + 3, 8);
            D7 = rawLine.Substring(rawLine.IndexOf("D7") + 3, 8);
            flags = rawLine.Substring(rawLine.Length - 5, 5);
        }
    }
}