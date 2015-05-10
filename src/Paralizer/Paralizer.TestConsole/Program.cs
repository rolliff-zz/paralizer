using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Paralizer.TestConsole
{
    class Program
    {
        class WatchStopper
        {
            Stopwatch w;
            string name;
            public WatchStopper()
            {        
                w = new Stopwatch();
                w.Start();
            }

            public TimeSpan Elapsed(bool restart)
            {
                w.Stop();
                var v = w.Elapsed;
                if (restart)
                {
                    w.Restart();
                }

                return v;
            }

            public void PrintElapsed(string message, bool restart=true)
            {
                var v = Elapsed(restart);
                Console.WriteLine("{0} Elapsed for :{1}", v.ToString(), message);
            }
        }
        string file = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\save games\Germany1963_11_29_03.hoi3";
        void RealMain(string[] args)
        {
            WatchStopper ws = new WatchStopper();            
            var data = ParalizerCore.FromParadoxFile(file);
            ws.PrintElapsed("Parsing");

            var json = ParalizerCore.ToJson(data);
            ws.PrintElapsed("Stringing");

            
            foreach (JProperty node in json)
            {
                if (Regex.IsMatch(node.Name, "[A-Z]{3}"))
                {
                    Console.WriteLine(node.Name);
                    Console.WriteLine("\t" + (node.Value as JObject)["ai"]["personality"].ToString(Newtonsoft.Json.Formatting.None));
                }
            }

            ws.PrintElapsed("Barfing");

        }
        static void Main(string[] args)
        {
            new Program().RealMain(args);
        }
    }
}
