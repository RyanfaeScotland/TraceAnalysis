using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using TraceAnalysis.Engine;

namespace TraceAnalysis.Console
{
    class Program
    {
        private static string configFile = "config.xml";
        
        private static Dictionary<string, TraceFile> traceFiles = new Dictionary<string, TraceFile>();
        private static Dictionary<string, NotesFile> notesFiles = new Dictionary<string, NotesFile>();
        private static Dictionary<string, string> launchablePrograms = new Dictionary<string, string>();
        private static bool quit;
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
                string menuSelection = DisplayMainMenuAndGetChoice();

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
                        FindAddressFrequency(null);
                        break;            
                    case "5":
                        FindDifferences();
                        break;      
                    case "H":
                        notesFiles.FirstOrDefault().Value.GenerateHTML();
                        break;
                    case "L":
                        string programName = DisplayProgramMenuAndGetChoice();
                        if (!programName.Equals("Unknown"))
                        {
                            LaunchProgram(launchablePrograms[programName]);
                        }
                        break;
                    case "R":
                        AnalysisEngine.ReadConstantly(@"C:\Users\Ryan\Games\Emulators\Mega Drive\Gens r57shell Mod\trace.log");
                        break;
                    case "T":
                        FindAddressFrequency("ToejamStartTrace.log");
                        break;  
                    case "U":
                        FindInstruction("ToejamStartTrace.log", "DBFa");
                        break;
                    default:
                        System.Console.WriteLine("Invalid selection");
                        break;
                }
            }
        }

        private static string DisplayProgramMenuAndGetChoice()
        {
            System.Console.WriteLine("What program...?");
            System.Console.WriteLine("1 - Notepad++");
            System.Console.WriteLine("2 - 68K");
            System.Console.WriteLine("3 - Gens");
            System.Console.WriteLine("4 - HxD");
            ConsoleKeyInfo key = System.Console.ReadKey();
            System.Console.WriteLine();
            switch (key.KeyChar.ToString().ToUpper())
            {
                case "1":
                    return "Notepad++";
                case "2":
                    return "68K";
                case "3":
                    return "Gens";
                case "4":
                    return "HxD";
                default:
                    return "Unknown";
            }
        }

        private static string DisplayMainMenuAndGetChoice()
        {
            System.Console.WriteLine("Whatcha gonna do...?");            
            System.Console.WriteLine("1 - Display Trace File List");
            System.Console.WriteLine("2 - Display Notes File List");
            System.Console.WriteLine("3 - Display ROMs File List");
            System.Console.WriteLine("4 - Find Address Frequency in Trace Files");
            System.Console.WriteLine("5 - Find Differences in Trace Files");
            System.Console.WriteLine("H - Generate HTML from notes");
            System.Console.WriteLine("L - Launch Program");
            System.Console.WriteLine("R - Read Trace File Continuously");
            System.Console.WriteLine("T - Find Address Frequency in ToejamStartTrace.log");
            System.Console.WriteLine("U - Find DBFa Instruction in ToejamStartTrace.log");
            System.Console.WriteLine("Q - Quit");
            ConsoleKeyInfo key = System.Console.ReadKey();
            System.Console.WriteLine();
            return key.KeyChar.ToString().ToUpper();
        }

        private static void FindDifferences()
        {
            if (traceFiles.Count >= 2)
            {
                traceFiles.ElementAt(0).Value.LoadTraceFile();
                traceFiles.ElementAt(1).Value.LoadTraceFile();
                AnalysisEngine.FindDifferences(traceFiles.ElementAt(0).Value, traceFiles.ElementAt(1).Value);
                AnalysisEngine.FindDifferences(traceFiles.ElementAt(1).Value, traceFiles.ElementAt(0).Value);
            }
        }

        private static void FindAddressFrequency(string traceFileName)
        {
            if (String.IsNullOrEmpty(traceFileName)){
                foreach (var fileNameAndSource in traceFiles)
                {
                    AnalysisEngine.FindAddressFrequency(traceFiles[fileNameAndSource.Key]);
                }
            }else{
                AnalysisEngine.FindAddressFrequency(traceFiles[traceFileName]);
            }
        }

        private static void FindInstruction(string traceFileName, string instruction)
        {
            traceFiles[traceFileName].LoadTraceFile();
            AnalysisEngine.FindInstruction(traceFiles[traceFileName], instruction);
        }

        private static void DisplayTraceFilesList()
        {
            foreach (var fileNameAndSource in traceFiles)
            {
                System.Console.WriteLine("Trace File: " + traceFiles[fileNameAndSource.Key].nameAndPath);
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

        private static void LaunchProgram(string programPath)
        {
            // Prepare the process to run
            ProcessStartInfo start = new ProcessStartInfo();
            // Enter in the command line arguments, everything you would enter after the executable name itself
            //start.Arguments = arguments;

            // Enter the executable to run, including the complete path
            start.FileName = programPath;
            // Do you want to show a console window?
            //start.WindowStyle = ProcessWindowStyle.Hidden;
            //start.CreateNoWindow = true;
            //int exitCode;

            // Run the external process & wait for it to finish
            //using (Process proc = Process.Start(start))
            //{
            //    proc.WaitForExit();

            //    // Retrieve the app's exit code
            //    exitCode = proc.ExitCode;
            //}
            Process.Start(start);
        }

        private static void Exit()
        {
            System.Environment.Exit(0);
        }

        private static void LoadConfig(string configFile)
        {
            var xml = XDocument.Load(configFile);

            //Load trace files
            var fileNames = from c in xml.Root.Descendants("TraceFiles").Elements()
                            select c.Value;
            foreach (var fileName in fileNames)
            {
                System.Console.WriteLine("Primed Trace File: " + fileName);
                traceFiles.Add(fileName, new TraceFile(fileName));
            }

            //Load notes files
            fileNames = from c in xml.Root.Descendants("NotesFiles").Elements()
                            select c.Value;
            foreach (var fileName in fileNames)
            {
                System.Console.WriteLine("Primed Notes File: " + fileName);
                notesFiles.Add(fileName, new NotesFile(fileName));
            }

            //Load runnable programs
            var elements = from c in xml.Root.Descendants("Programs").Elements()
                        select c;
            foreach (var program in elements)
            {
                string programId = program.Attribute("id").Value;
                System.Console.WriteLine("Primed Program: " + programId);
                launchablePrograms.Add(programId, program.Value);
            }
        }

        private static void ParseArgs(string[] args)
        {
            return;
        }
    }
}
