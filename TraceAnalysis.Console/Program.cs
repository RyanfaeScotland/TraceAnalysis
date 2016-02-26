using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TraceAnalysis.Engine;

//xcopy "C:\Users\Ryan\Games\Emulators\Mega Drive\Roms\Toejam\Toejam.xml" "$(SolutionDir)TraceAnalysis.Console\bin\Debug" /y

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

            EnterMainProgramLoop();

            Exit();
        }

        private static void EnterMainProgramLoop()
        {
            while (!quit)
            {
                string menuSelection = DisplayMenuAndGetChoice();

                switch (menuSelection)
                {
                    case "Q":
                        quit = true;
                        break;
                    case "1":
                        DisplayTraceFilesList();
                        break;
                    case "2":
                        DisplayNotesFilesList();
                        break;
                    case "3":
                        DisplayROMsFilesList();
                        break;
                    case "4":
                        FindOccurances();
                        break;            
                    case "5":
                        FindDifferences();
                        break;                        
                    default:
                        System.Console.WriteLine("Invalid selection");
                        break;
                }
            }
        }

        private static string DisplayMenuAndGetChoice()
        {
            System.Console.WriteLine("Whatcha gonna do...?");            
            System.Console.WriteLine("1 - Display Trace File List");
            System.Console.WriteLine("2 - Display Notes File List");
            System.Console.WriteLine("3 - Display ROMs File List");
            System.Console.WriteLine("4 - Find Occurances in Trace Files");
            System.Console.WriteLine("5 - Find Differences in Trace Files");
            System.Console.WriteLine("Q - Quit");
            ConsoleKeyInfo key = System.Console.ReadKey();
            System.Console.WriteLine();
            return key.KeyChar.ToString().ToUpper();
        }

        private static void FindDifferences()
        {
            if (traceFiles.Count >= 2)
            {
                AnalysisEngine.FindDifferences(traceFiles.ElementAt(0).Value.traceLog, traceFiles.ElementAt(1).Value.traceLog);
                AnalysisEngine.FindDifferences(traceFiles.ElementAt(1).Value.traceLog, traceFiles.ElementAt(0).Value.traceLog);
            }
        }

        private static void FindOccurances()
        {
            foreach (var fileNameAndSource in traceFiles)
            {
                AnalysisEngine.FindOccurances(fileNameAndSource.Key, traceFiles[fileNameAndSource.Key].traceLog);
            }
        }

        private static void DisplayTraceFilesList()
        {
            foreach (var fileNameAndSource in traceFiles)
            {
                System.Console.WriteLine("Trace File: " + traceFiles[fileNameAndSource.Key].name);
            }
        }

        private static void DisplayNotesFilesList()
        {
            foreach (var fileNameAndSource in notesFiles)
            {
                System.Console.WriteLine("Notes File: " + notesFiles[fileNameAndSource.Key].name);
            }
        }

        private static void DisplayROMsFilesList()
        {
            foreach (var fileNameAndSource in notesFiles)
            {
                System.Console.WriteLine("ROM File: " + notesFiles[fileNameAndSource.Key].GetRomFileName());
            }
        }

        private static void Exit()
        {
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
