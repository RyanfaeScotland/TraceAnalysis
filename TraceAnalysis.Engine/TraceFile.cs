using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis.Engine
{
    public class TraceFile
    {
        public string name { get; set; }
        public Dictionary<string, int> traceLog { get; set; }
        
        public TraceFile(string name)
        {
            this.name = name;
            traceLog = new Dictionary<string, int>();
        }
    }
}
