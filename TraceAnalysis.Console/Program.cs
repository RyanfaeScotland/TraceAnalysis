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
                TraceFile selectedFile;
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
                    case "D":
                        FindDifferences(DisplayTraceFileMenuAndGetChoice(), DisplayTraceFileMenuAndGetChoice());
                        break;      
                    case "F":
                        selectedFile = DisplayTraceFileMenuAndGetChoice();
                        FindAddressFrequency(selectedFile);
                        break;                                
                    case "H":
                        notesFiles.FirstOrDefault().Value.GenerateHTML();
                        break;
                    case "I":
                        FindInstruction(DisplayTraceFileMenuAndGetChoice(), DisplayPromptAndGetUserInput());
                        break;                    
                    case "L":
                        AnalyseLoop(DisplayTraceFileMenuAndGetChoice());
                        break;
                    case "LOAD":
                        selectedFile = DisplayTraceFileMenuAndGetChoice();
                        selectedFile.LoadTraceFile();
                        break;
                    case "O":
                        selectedFile = DisplayTraceFileMenuAndGetChoice();
                        selectedFile.LoadTraceFile();
                        selectedFile.WriteOutLineInOrder();
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
                    default:
                        System.Console.WriteLine("Invalid selection");
                        break;
                }
            }
        }

        private static string DisplayPromptAndGetUserInput()
        {
            System.Console.WriteLine("Enter a sensible response...");
            return System.Console.ReadLine();
        }

        private static void AnalyseLoop(TraceFile traceFile)
        {
            traceFile.LoadTraceFile();
            AnalysisEngine.AnalyseLoop(traceFile);
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

        private static TraceFile DisplayTraceFileMenuAndGetChoice()
        {
            System.Console.WriteLine("Which Trace file...?");
            DisplayTraceFilesList();
            string line = System.Console.ReadLine();
            System.Console.WriteLine();
            int selection = Int32.Parse(line);
            return selection == -1 ? null : traceFiles[Int32.Parse(line)];
        }

        private static string DisplayMainMenuAndGetChoice()
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Whatcha gonna do...?");            
            System.Console.WriteLine("1 - Display Trace File List");
            System.Console.WriteLine("2 - Display Notes File List");
            System.Console.WriteLine("3 - Display ROMs File List");
            System.Console.WriteLine("D - Find Differences in Trace Files");
            System.Console.WriteLine("F - Find Address Frequency in Trace File(s)");            
            System.Console.WriteLine("H - Generate HTML from notes");
            System.Console.WriteLine("I - Find Instruction in Trace File(s)");
            System.Console.WriteLine("L - Loop Analysis");
            System.Console.WriteLine("LOAD - Load Trace File");
            System.Console.WriteLine("O - Output Trace File in Order Read");
            System.Console.WriteLine("P - Launch Program");
            System.Console.WriteLine("R - Read Trace File Continuously");            
            System.Console.WriteLine("Q - Quit");
            string line = System.Console.ReadLine();
            System.Console.WriteLine();
            return line.ToUpper();
        }

        private static void FindDifferences(TraceFile traceFileA, TraceFile traceFileB)
        {
            if (traceFileA != null && traceFileB != null)
            {
                traceFileA.LoadTraceFile();
                traceFileB.LoadTraceFile();
                AnalysisEngine.FindDifferences(traceFileA, traceFileB);
                AnalysisEngine.FindDifferences(traceFileB, traceFileA);
            }
        }

        private static void FindAddressFrequency(TraceFile traceFile)
        {
            if (traceFile == null)
            {
                foreach (var currentTraceFile in traceFiles)
                {
                    AnalysisEngine.FindAddressFrequency(currentTraceFile);
                }
            }else{
                AnalysisEngine.FindAddressFrequency(traceFile);
            }
        }

        private static void FindInstruction(TraceFile traceFile, string instruction)
        {
            traceFile.LoadTraceFile();
            AnalysisEngine.FindInstruction(traceFile, instruction);
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
