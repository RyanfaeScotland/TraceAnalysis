using System.Linq;
using System.Xml.Linq;

namespace TraceAnalysis.Engine
{
    public class NotesFile
    {
        public string name { get; set; }
        
        public NotesFile(string name)
        {
            this.name = name;
        }

        public string GetRomFileName()
        {
            var xml = XDocument.Load(name);
            var gameNames = from c in xml.Root.Descendants("rom").Attributes("id")
                            select c.Value;
            return gameNames.FirstOrDefault();            
        }
    }
}
