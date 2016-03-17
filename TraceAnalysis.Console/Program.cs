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
        
        private static List<TraceFile> traceFiles = new List<TraceFile>();
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
                    case "QUIT":
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
                    case "F":
                        menuSelection = DisplayTraceFileMenuAndGetChoice();
                        FindAddressFrequency(menuSelection);
                        break;            
                    case "5":
                        menuSelection = DisplayTraceFileMenuAndGetChoice();
                        menuSelection = menuSelection + "," + DisplayTraceFileMenuAndGetChoice();
                        FindDifferences(menuSelection);
                        break;      
                    case "H":
                        notesFiles.FirstOrDefault().Value.GenerateHTML();
                        break;
                    case "L":
                        AnalysisLoop("ToejamStartTrace.log");
                        break;
                    case "P":
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

        private static void AnalysisLoop(string traceFileName)
        {
            traceFiles[traceFileName].LoadTraceFile();
            AnalysisEngine.AnalysisLoop(traceFiles[traceFileName]);
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

        private static string DisplayTraceFileMenuAndGetChoice()
        {
            System.Console.WriteLine("Which Trace file...?");
            DisplayTraceFilesList();
            string line = System.Console.ReadLine();
            System.Console.WriteLine();
            return line;
        }

        private static string DisplayMainMenuAndGetChoice()
        {
            System.Console.WriteLine("Whatcha gonna do...?");            
            System.Console.WriteLine("1 - Display Trace File List");
            System.Console.WriteLine("2 - Display Notes File List");
            System.Console.WriteLine("3 - Display ROMs File List");
            System.Console.WriteLine("F - Find Address Frequency in Trace Files");
            System.Console.WriteLine("5 - Find Differences in Trace Files");
            System.Console.WriteLine("H - Generate HTML from notes");
            System.Console.WriteLine("L - Loop Analysis");
            System.Console.WriteLine("P - Launch Program");
            System.Console.WriteLine("R - Read Trace File Continuously");
            System.Console.WriteLine("T - Find Address Frequency in ToejamStartTrace.log");
            System.Console.WriteLine("U - Find DBFa Instruction in ToejamStartTrace.log");
            System.Console.WriteLine("Q - Quit");
            string line = System.Console.ReadLine();
            System.Console.WriteLine();
            return line.ToUpper();
        }

        private static void FindDifferences(string commaSepetatedFileChoices)
        {
            string[] traceFileIndexes = commaSepetatedFileChoices.Split(',');
            if (traceFileIndexes.Length >= 2)
            {
                traceFiles.ElementAt(Int32.Parse(traceFileIndexes[0])).LoadTraceFile();
                traceFiles.ElementAt(Int32.Parse(traceFileIndexes[1])).LoadTraceFile();
                AnalysisEngine.FindDifferences(traceFiles.ElementAt(Int32.Parse(traceFileIndexes[0])), traceFiles.ElementAt(Int32.Parse(traceFileIndexes[1])));
                AnalysisEngine.FindDifferences(traceFiles.ElementAt(Int32.Parse(traceFileIndexes[1])), traceFiles.ElementAt(Int32.Parse(traceFileIndexes[0])));
            }
        }

        private static void FindAddressFrequency(string traceFileIndex)
        {
            if (String.IsNullOrEmpty(traceFileIndex))
            {
                foreach (var traceFile in traceFiles)
                {
                    AnalysisEngine.FindAddressFrequency(traceFile);
                }
            }else{
                AnalysisEngine.FindAddressFrequency(traceFiles[Int32.Parse(traceFileIndex)]);
            }
        }

        private static void FindInstruction(string traceFileIndex, string instruction)
        {
            traceFiles[Int32.Parse(traceFileIndex)].LoadTraceFile();
            AnalysisEngine.FindInstruction(traceFiles[Int32.Parse(traceFileIndex)], instruction);
        }

        private static void DisplayTraceFilesList()
        {
            int position = 0;
            foreach (var traceFile in traceFiles)
            {
                System.Console.WriteLine(String.Format("{0} - {1}", position++, traceFile.nameAndPath));
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
                traceFiles.Add(new TraceFile(fileName));
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
