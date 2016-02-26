using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TraceAnalysis.Engine;
using System.Diagnostics;

namespace TraceAnalysis.Console
{
    class Program
    {
        private static string configFile = "config.xml";
        
        private static Dictionary<string, TraceFile> traceFiles = new Dictionary<string, TraceFile>();
        private static Dictionary<string, NotesFile> notesFiles = new Dictionary<string, NotesFile>();

        private static bool quit = false;
        private static void Main(string[] args)
        {
            ParseArgs(args);

            LoadConfig(configFile);

            while (!quit) { 
            System.Console.WriteLine("Whatcha gonna do...?");
            System.Console.WriteLine("Q - Quit");
            System.Console.WriteLine("Anything else - Everything else");
            ConsoleKeyInfo key = System.Console.ReadKey();

                switch (key.KeyChar.ToString().ToUpper())
                {
                    case "Q":
                        quit = true;
                        break;
                    default:
                        foreach (var fileNameAndSource in notesFiles)
                        {
                            notesFiles[fileNameAndSource.Key].GetRomFileName();
                        }

                        System.Console.WriteLine("Config complete - Hit a key to process...");
                        System.Console.ReadKey();

                        foreach (var fileNameAndSource in traceFiles)
                        {
                            AnalysisEngine.FindOccurances(fileNameAndSource.Key,
                                traceFiles[fileNameAndSource.Key].traceLog);
                        }

                        if (traceFiles.Count >= 2)
                        {
                            AnalysisEngine.FindDifferences(traceFiles.ElementAt(0).Value.traceLog,
                                traceFiles.ElementAt(1).Value.traceLog);
                            AnalysisEngine.FindDifferences(traceFiles.ElementAt(1).Value.traceLog,
                                traceFiles.ElementAt(0).Value.traceLog);
                        }

                        System.Console.WriteLine("FIN");
                        break;
                }
            }
            System.Environment.Exit(0);
        }

        private static void LoadConfig(string configFile)
        {
            var xml = XDocument.Load(configFile);
            var fileNames = from c in xml.Root.Descendants("TraceFiles").Elements()
                            select c.Value;
            foreach (var fileName in fileNames)
            {
                System.Console.WriteLine("Primed Trace File: " + fileName);
                traceFiles.Add(fileName, new TraceFile(fileName));
            }

            fileNames = from c in xml.Root.Descendants("NotesFiles").Elements()
                            select c.Value;
            foreach (var fileName in fileNames)
            {
                System.Console.WriteLine("Primed Notes File: " + fileName);
                notesFiles.Add(fileName, new NotesFile(fileName));
            }
        }

        private static void ParseArgs(string[] args)
        {
            return;
        }
    }
}
