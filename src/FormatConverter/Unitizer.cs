using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paralizer;

namespace FormatConverter
{
    class Unitizer
    {

       
        
        internal void Unitify()
        {
            IndentedTextWriter log = new IndentedTextWriter(new StreamWriter(Console.OpenStandardOutput()));
            JObject FatObject = new JObject();
            var files = Directory.GetFiles(@"H:\software\HOI3\Hearts of Iron III\tfh\mod\BlackICE\units", "*.txt", SearchOption.TopDirectoryOnly);
            
            foreach (var file in files)
            {
                Debug.WriteLine("Parsing", file);
                try
                {
                    var data = Paralizer.ParalizerCore.FromParadoxFile(file);
                    var n = Path.ChangeExtension(file, "json");
                    var json = Paralizer.ParalizerCore.ToJson(data);
                    FatObject.Add(json.First);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("!!! {0}", e.Message);
                    Debug.WriteLine("!!!");
                }
            }
            var units = UnitProcessor.ProcessUnits(FatObject);
            StringBuilder sb = new StringBuilder();
            foreach (var u in units)
            {
                List<string> row = new List<string>();

                Debug.Assert(u.terrain_modifiers.Count == Unit.terra.Count());

                row.Add(u.title);
                row.Add(u.type);
                foreach (var v in u.terrain_modifiers)
                {
                    row.Add(v.attack.ToString());
                    row.Add(v.movement.ToString());
                    row.Add(v.defence.ToString());
                }

                sb.AppendLine(String.Join(",", row));
            }
            File.WriteAllText(@"H:\software\hoi3\units_research\all.csv", sb.ToString());
        }
    }
}
