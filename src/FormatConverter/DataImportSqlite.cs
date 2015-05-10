using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FormatConverter
{
    class DataImportSqlite : DataImportBase
    {
        static readonly string table = @"CREATE TABLE [j_file] (
[id] INTEGER  NOT NULL PRIMARY KEY,
[timestamp] TIMESTAMP  NULL,
[dim] VARCHAR(255)  NULL,
[file] VARCHAR(255)  NULL,
[doc] BLOB  NULL
)";
        static readonly string get_query_dim = @"SELECT doc from [j_file] WHERE dim=:p1";
        static readonly string get_query_file = @"SELECT doc from [j_file] WHERE file=:p1";
        static readonly string get_query = @"SELECT doc from [j_file] WHERE dim=:p1 AND file=:p2";

        SQLiteConnection m_conn;
        protected override void Init()
        {
            //try { File.Delete("E:\\tmp.s3db"); }
            //catch { }
            var c = new SQLiteConnectionStringBuilder();
            c.DataSource = "E:\\tmp.s3db";
            //c.DataSource = ":memory:";
            c.Version = 3;
            
            
            m_conn = new SQLiteConnection(c.ToString());
            m_conn.Open();
            
        }

        public JObject Get(string dim, string file)
        {
            var cmd = m_conn.CreateCommand();
            
            if (String.IsNullOrEmpty(dim))
            {
                cmd.CommandText = get_query_file;
                cmd.Parameters.Add(new SQLiteParameter("p2", file));
            }
            else if (String.IsNullOrEmpty(file))
            {
                JObject ret = new JObject();
                cmd.CommandText = "SELECT file from [j_file] where dim=:p1";
                cmd.Parameters.Add(new SQLiteParameter("p1", dim));
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    file = reader.GetString(0);
                    var cmd2 = m_conn.CreateCommand();
                    cmd2.CommandText = get_query;
                    cmd2.Parameters.Add(new SQLiteParameter("p1", dim));
                    cmd2.Parameters.Add(new SQLiteParameter("p2", file));
                    var res2 = cmd2.ExecuteScalar();
                    ret[file] = JObject.Parse(Paralizer.ParalizerCore.MyDefaultEncoding.GetString(res2 as byte[]));
                }
                return ret;
            }
            else
            {
                cmd.CommandText = get_query;
                cmd.Parameters.Add(new SQLiteParameter("p1", dim));
                cmd.Parameters.Add(new SQLiteParameter("p2", file));
            }

            var res1 = cmd.ExecuteScalar() as byte[];
            var res = Paralizer.ParalizerCore.MyDefaultEncoding.GetString(res1);

            return JObject.Parse(res);
        }

        public void Put(string dim, string file, JToken doc)
        {
            Submit(dim, file, doc);
        }
        public void InitDatabase()
        {
            var cmd = m_conn.CreateCommand();
            cmd.CommandText = table;
            cmd.ExecuteNonQuery();
        }
        protected override void Submit(string dim, string id, Newtonsoft.Json.Linq.JToken json)
        {
            string Q = @"INSERT INTO j_file (timestamp, dim, file, doc) VALUES (:time,:dim, :fuck,:shit)";
            
            SQLiteCommand cmd = m_conn.CreateCommand();
            cmd.CommandText = Q;

            cmd.Parameters.Add(new SQLiteParameter("time", DateTime.Now));
            cmd.Parameters.Add(new SQLiteParameter("fuck", id));
            cmd.Parameters.Add(new SQLiteParameter("shit", json.ToString()));
            cmd.Parameters.Add(new SQLiteParameter("dim", dim));
            cmd.ExecuteNonQuery();  
            
        }

        internal object Query(string query)
        {
            throw new NotImplementedException();
        }
    }
}
