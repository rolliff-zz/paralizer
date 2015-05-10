using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ORA = Oracle.DataAccess.Client;
namespace FormatConverter
{
    class DataImportBase
    {
        public bool IsDebug { get; set; }
        ORA.OracleConnection conn = new ORA.OracleConnection();
        public delegate bool FilterDelegate(JToken token, string id, Dictionary<string, JToken> things);

        public DataImportBase()
        {
            Init();
        }
        virtual protected void Init()
        {
            Environment.SetEnvironmentVariable("ORAHOME", @"R:\app\Robby\product\12.1.0\dbhome_1");
            while (true)
            {
                Console.WriteLine("Attempting...");
                try
                {
                    conn.ConnectionString = "user id=HOI3;password=oracle;data source=orcl";
                    conn.Open();

                }
                catch (ORA.OracleException x)
                {
                    if (x.ErrorCode == 1031)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                }
                break;
            }
            IsDebug = false;
        }
        public void ImportDirectory(string id, string dir, FilterDelegate filter =null)
        {
            JObject FatObject = new JObject();
            var files = Directory.GetFiles(dir, "*.txt", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                Debug.WriteLine("Parsing", file);
                try
                {
                    Dictionary<string, JToken> things = new Dictionary<string, JToken>();
                    var json = ReadFile(file);
                    var real_id = Path.GetFileNameWithoutExtension(file);
                    if (filter != null)
                    {
                        if (filter(json, real_id,things))
                            continue;
                    }
                    foreach(var thing in things)
                        Submit(id, thing.Key, thing.Value);
                    
                }
                catch (Exception e)
                {
                    Debug.Print("!!! {0}", e.Message);
                    Debug.WriteLine("!!!");
                }
            }
            
            //Submit(id, FatObject);

        }
        public void ImportFile(string id, string file, FilterDelegate filter)
        {
            Debug.WriteLine("Parsing", file);
            try
            {
                Dictionary<string, JToken> things = new Dictionary<string, JToken>();
                var json = ReadFile(file);
                var real_id = Path.GetFileNameWithoutExtension(file);
                if (filter != null)
                {
                    filter(json, real_id, things);
                        
                }
                foreach (var thing in things)
                    Submit(id, thing.Key, thing.Value);
            }
            catch (Exception e)
            {
                Debug.Print("!!! {0}", e.Message);
                Debug.WriteLine("!!!");
            }

        }

        protected virtual void Submit(string dim, string id, JToken json)
        {
            string Q = @"INSERT INTO j_file VALUES (SYS_GUID(),SYSTIMESTAMP,:fuck,:shit,:dim)";

            ORA.OracleCommand cmd = conn.CreateCommand();
            cmd.CommandText = Q;
            
            cmd.Parameters.Add(new ORA.OracleParameter("fuck", id));
            cmd.Parameters.Add(new ORA.OracleParameter("shit", json.ToString()));
            cmd.Parameters.Add(new ORA.OracleParameter("dim", dim));
            cmd.ExecuteNonQuery();  
        }
        JToken ReadFile(string file)
        {
            var data = Paralizer.ParalizerCore.FromParadoxFile(file);
            var json = Paralizer.ParalizerCore.ToJson(data);

            if (IsDebug)
            {
                var n = Path.ChangeExtension(file, "json");
                File.WriteAllText(n, json.ToString());
            }
            return json;
        }
    }
}
