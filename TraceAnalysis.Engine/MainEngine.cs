using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraceAnalysis.Engine
{
    public static class AnalysisEngine
    {
        public static void FindDifferences(Dictionary<string, int> addressesA, Dictionary<string, int> addressesB)
        {
            var keysDictionaryAHasThatBDoesNot = addressesA.Keys.Except(addressesB.Keys);
            System.Console.WriteLine("In 1 but not in 2:");
            foreach (var address in keysDictionaryAHasThatBDoesNot)
            {
                System.Console.WriteLine(address + ": " + addressesA[address]);
            }
            System.Console.WriteLine("Hit a key to continue...");
            System.Console.ReadKey();
        }

        public static void FindOccurances(string fileName, Dictionary<string, int> addresses)
        {
            string address;
            int value;
            foreach (var line in File.ReadLines(fileName))
            {
                if (line.Length < 8) continue;
                address = line.Substring(0, 8);
                addresses[address] = addresses.TryGetValue(address, out value) ? ++value : 1;
            }

            System.Console.WriteLine(fileName);
            foreach (var addressValuePair in addresses.OrderBy(key => key.Value))
            {
                System.Console.WriteLine(addressValuePair.Key + ": " + addressValuePair.Value);
            }
            System.Console.WriteLine("Hit a key to continue...");
            System.Console.ReadKey();
        }
    
        
    }
}
