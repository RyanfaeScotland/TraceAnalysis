using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TraceAnalysis.Engine
{
    public class NotesFile
    {
        public string name { get; set; }
//        public Dictionary<string, int> traceLog { get; set; }
        
        public NotesFile(string name)
        {
            this.name = name;
//            traceLog = new Dictionary<string, int>();
        }

        public void GetRomFileName()
        {
            var xml = XDocument.Load(name);
            var gameNames = from c in xml.Root.Descendants("rom").Attributes("id")
                            select c.Value;
            foreach (var gameName in gameNames)
            {
                System.Console.WriteLine("Rom File: " + gameName);
            }
        }
    }
}
