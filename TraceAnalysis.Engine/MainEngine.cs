﻿using System;
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
    }
}
