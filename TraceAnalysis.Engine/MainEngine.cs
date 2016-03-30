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
        static volatile bool stopReadingConstantly;

        public static void FindDifferences(TraceFile traceFileA, TraceFile traceFileB)
        {
            traceFileA.FindDifferences(traceFileB);
        }

        public static void FindAddressFrequency(TraceFile traceFile)
        {
            traceFile.FindAddressFrequency();
            traceFile.WriteAddressFrequencyToConsole();
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
                    stopReadingConstantly = false;
                    while (Console.ReadKey().Key != ConsoleKey.Q) ;
                    stopReadingConstantly = true;
                });

                while (!stopReadingConstantly)
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

        public static void FindInstruction(TraceFile traceFile, string instruction)
        {
            List<TraceLine> instructions = traceFile.FindInstruction(instruction);
            foreach (TraceLine line in instructions)
            {
                System.Console.WriteLine(String.Format("{0}: {1}", line.address, line.instruction));
            }
        }

        public static void AnalyseLoop(TraceFile traceFile)
        {
            List<TraceLine> instructions = traceFile.FindInstruction("DBFa");
            TraceLine line = instructions.First();
            System.Console.WriteLine(String.Format("{0}: {1} - D1: {2}", line.address, line.instruction, line.D1));
            for (int lineIndex = 0; lineIndex < traceFile.linesInOrder.Count; lineIndex++ )
            {
                string address = traceFile.linesInOrder[lineIndex].address;
                if (line.instruction.Contains(address.Substring(3).Trim()))
                {
                    System.Console.WriteLine(String.Format("{0}: {1} - A5: {2} A4: {3}", traceFile.addressesToLines[address].address, traceFile.addressesToLines[address].instruction, traceFile.addressesToLines[address].A5, traceFile.addressesToLines[address].A4));
                    while (line.address != address)
                    {
                        address = traceFile.linesInOrder[++lineIndex].address;
                        System.Console.WriteLine(String.Format("{0}: {1} - {2} A4: {3}", traceFile.addressesToLines[address].address, traceFile.addressesToLines[address].instruction, traceFile.addressesToLines[address].A5, traceFile.addressesToLines[address].A4));                        
                    }
                    break;
                }
            }
        }
    }
}
