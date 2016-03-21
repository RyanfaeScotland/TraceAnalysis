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
        public List<TraceLine> linesInOrder { get; set; }
        public Dictionary<string, int> addressesFrequency { get; set; }
        public bool stopLoadingTraceFile, fileLoaded;
        
        public TraceFile(string nameAndPath)
        {
            this.nameAndPath = nameAndPath;
            addressesToLines = new Dictionary<string, TraceLine>();
            linesInOrder = new List<TraceLine>();
            addressesFrequency = new Dictionary<string, int>();
            fileLoaded = false;
        }

        public void LoadTraceFile()
        {
            if (fileLoaded)
            {
                return;
            }
            var fs = new FileStream(nameAndPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var sr = new StreamReader(fs))
            {
                var task = Task.Factory.StartNew(() =>
                {
                    stopLoadingTraceFile = false;
                    Console.WriteLine("Reading Trace File, press A to Abort");
                    while (Console.ReadKey().Key != ConsoleKey.A && !fileLoaded) Thread.Sleep(1);
                    stopLoadingTraceFile = true;
                });

                while (!stopLoadingTraceFile)
                {
                    string line = sr.ReadLine();
                    if (line == null)
                    {
                        fileLoaded = true;
                        break;
                    }
                    if (line.Length < 14) 
                    {
                        continue; 
                    }
                    string address = line.Substring(0, 8);
                    addressesToLines[address] = new TraceLine(line);
                    linesInOrder.Add(new TraceLine(line));
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

        public List<TraceLine> FindInstruction(string instruction)
        {
            List<TraceLine> instructionLines = new List<TraceLine>();
            foreach (TraceLine line in addressesToLines.Values)
            {
                if (line.instruction.StartsWith(instruction))
                {
                    instructionLines.Add(line);
                }
            }
            return instructionLines;
        }

        public void WriteOutLineInOrder()
        {
            for (int index = 0; index < linesInOrder.Count; index++)
            {
                System.Console.WriteLine("{0}: {1}", linesInOrder[index].address, linesInOrder[index].instruction);
            }
        }
    
    }

    public class TraceLine
    {
        public string address;
        public string opcode;
        public string instruction;
        public string A0, A1, A2, A3, A4, A5, A6, A7;
        public string D0, D1, D2, D3, D4, D5, D6, D7;
        public string flags;

        public TraceLine(string rawLine)
        {
            //00:0202  4A B9  TST.L   ($00A10008)              A0=00000000 A1=00000000 A2=00000000 A3=00000000 A4=00000000 A5=00000000 A6=00000000 A7=00FF8000 D0=00000000 D1=00000000 D2=00000000 D3=00000000 D4=00000000 D5=00000000 D6=00000000 D7=00000000 xnzvc
            address = rawLine.Substring(0, 7);
            opcode = rawLine.Substring(9, 5);
            instruction = rawLine.Substring(16, 33).Trim();
            A0 = rawLine.Substring(rawLine.LastIndexOf("A0") + 3, 8);
            A1 = rawLine.Substring(rawLine.LastIndexOf("A1") + 3, 8);
            A2 = rawLine.Substring(rawLine.LastIndexOf("A2") + 3, 8);
            A3 = rawLine.Substring(rawLine.LastIndexOf("A3") + 3, 8);
            A4 = rawLine.Substring(rawLine.LastIndexOf("A4") + 3, 8);
            A5 = rawLine.Substring(rawLine.LastIndexOf("A5") + 3, 8);
            A6 = rawLine.Substring(rawLine.LastIndexOf("A6") + 3, 8);
            A7 = rawLine.Substring(rawLine.LastIndexOf("A7") + 3, 8);
            D0 = rawLine.Substring(rawLine.LastIndexOf("D0") + 3, 8);
            D1 = rawLine.Substring(rawLine.LastIndexOf("D1") + 3, 8);
            D2 = rawLine.Substring(rawLine.LastIndexOf("D2") + 3, 8);
            D3 = rawLine.Substring(rawLine.LastIndexOf("D3") + 3, 8);
            D4 = rawLine.Substring(rawLine.LastIndexOf("D4") + 3, 8);
            D5 = rawLine.Substring(rawLine.LastIndexOf("D5") + 3, 8);
            D6 = rawLine.Substring(rawLine.LastIndexOf("D6") + 3, 8);
            D7 = rawLine.Substring(rawLine.LastIndexOf("D7") + 3, 8);
            flags = rawLine.Substring(rawLine.Length - 5, 5);
        }
    }
}