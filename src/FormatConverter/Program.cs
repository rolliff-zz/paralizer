using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FormatConverter
{
    class Program
    {
        static void ImportAll()
        {
            var the_path = @"H:\software\HOI3\Hearts of Iron III\tfh\mod\BlackICE\units";
            var tech_path = @"H:\software\HOI3\Hearts of Iron III\tfh\mod\BlackICE\technologies";
            var test = @"H:\software\HOI3\Hearts of Iron III\tfh\mod\BlackICE\map\terrain.txt";
            var leaders = @"H:\software\HOI3\Hearts of Iron III\tfh\mod\BlackICE\history\leaders\GER.txt";
            var traits = @"H:\software\HOI3\Hearts of Iron III\tfh\mod\BlackICE\common\traits.txt";
            var leaders_all = @"H:\software\HOI3\Hearts of Iron III\tfh\mod\BlackICE\history\leaders\GER";

            var dib = new DataImportSqlite();
            DataImportBase.FilterDelegate f = delegate(JToken token, string id, Dictionary<string, JToken> things)
            {
                things[id] = (token.First as JProperty).Value;

                return false;
            };

            DataImportBase.FilterDelegate tech = delegate(JToken token, string id, Dictionary<string, JToken> things)
            {
                foreach (var cat in token)
                {
                    things[(cat as JProperty).Name] = (cat as JProperty).Value;
                }
                return false;
            };

            DataImportBase.FilterDelegate terrain = delegate(JToken token, string id, Dictionary<string, JToken> things)
            {
                var cats = token["categories"];
                foreach (var cat in cats)
                {
                    things[(cat as JProperty).Name] = (cat as JProperty).Value;
                }

                return false;
            };
            DataImportBase.FilterDelegate leaders_fn = delegate(JToken token, string id, Dictionary<string, JToken> things)
            {
                foreach (JProperty dude in (token as JObject).Properties())
                {
                    things[dude.Name] = dude.Value;
                }

                return false;
            };
            DataImportBase.FilterDelegate traits_fn = delegate(JToken token, string id, Dictionary<string, JToken> things)
            {
                foreach (JProperty dude in (token as JObject).Properties())
                {
                    things[dude.Name] = dude.Value;
                }

                return false;
            };
            //dib.ImportDirectory("leaders", leaders_all, leaders_fn);
            //dib.ImportFile("traits", traits, traits_fn);
            dib.ImportFile("leaders", leaders, leaders_fn);
            dib.ImportDirectory("units", the_path, f);
            dib.ImportFile("terrain", test, terrain);
            dib.ImportDirectory("tech", tech_path, tech);
        }
        static void Test()
        {
            Console.WriteLine("Open");
            var db = new DataImportSqlite();
            string filename = @"Germany1937_02_12_15.hoi3";
            Console.WriteLine("Savegame");
            var save = db.Get("save",filename);

            string filenamebase = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\BlackICE\save games\Germany1937_02_12_15.hoi3";
            //File.WriteAllText(filenamebase + ".min.json", doc.ToString());
            Console.WriteLine("Tech");
            var techs = db.Get("tech", null);

            //File.WriteAllText(filenamebase + ".tech.json", tech.ToString());

            Console.WriteLine("Units");
  
            JObject units_all = db.Get("units",null);


            var units = (
                from JProperty x in units_all
                where 
                    !x.Name.StartsWith("guard") &&
                    (x.Value)["type"].Value<string>() == "land" &&
                    (x.Value.SelectToken("unit_group") == null || x.Value.SelectToken("unit_group").Value<string>() != "division_HQ_unit_type") &&
                    (x.Value.SelectToken("usable_by") == null || x.Value.SelectToken("usable_by[0]") == null || (string)x.Value.SelectToken("usable_by[0]") == "GER") &&
                    ((int)x.Value.SelectToken("build_time") <= 220)

                select x
            ).ToList();

           

            string fmt = "{0,-35} | {1,-10} | {2,-10} | {3,-10} | {4,-10} | {5,-10} | {6,-10} | {7,-10} | {8,-10} | {9,-10} | {10,-10}";
            
            var save_tech = (save["technology"]);

            foreach (var u in units)
            {
                
                
                foreach (var tech in techs)
                {
                    var current_save_tech = save_tech[tech.Key];
                    var level = (int)current_save_tech.SelectToken("$[0]");

                    var crap = (
                        from JProperty applicable_tech in tech.Value
                        where applicable_tech.Name == u.Name
                        select applicable_tech).SingleOrDefault();

                    if (crap == null)
                        continue;

                    Debug.WriteLine("\tapply {0} from {1} ({2})" , crap.Value.Count(), tech.Key, level);
                    foreach (object c in crap.Value)
                    {
                        if (c is JProperty)
                        {
                            var o = (c as JProperty);
                            if (o.Value.Type == JTokenType.Float)
                            {
                                var d = u.Value.Value<decimal>(o.Name);
                                d += ((decimal)o.Value * level);
                                u.Value[o.Name] = d;
                                //Debug.WriteLine("\t\t modify {0,-20} : {1}", o.Name, (decimal)o.Value * level);
                            }
                            else if (o.Value.Type == JTokenType.Integer)
                            {

                                var d = u.Value.Value<int>(o.Name);
                                d += ((int)o.Value * level);
                                u.Value[o.Name] = d;
                                //Debug.WriteLine("\t\t modify {0,-20} : {1}", o.Name, (int)o.Value * level);
                            }
                            else
                            {
                                //Debug.WriteLine("\t\t modify {0,-20} : {1}", o.Name, o.Value);
                            }
                        }
                        else if(c is JObject)
                        {
                            foreach(var o in (c as JObject).Properties())
                            {
                                if (o.Value.Type == JTokenType.Float)
                                {
                                    Debug.WriteLine("\t\t modify {0,-20} : {1}", o.Name, (decimal)o.Value * level);
                                }
                                else if (o.Value.Type == JTokenType.Integer)
                                {
                                    Debug.WriteLine("\t\t modify {0,-20} : {1}", o.Name, o.Value<int>() * level);
                                }
                                else
                                {
                                    Debug.WriteLine("\t\t modify {0,-20} : {1}", o.Name, o.Value);
                                }
                            }
                        }
                    }

                }

                
            }

             var units2 = (
                from JProperty x in units
                select new 
                {
                    name = x.Name,
                    build_cost_ic = (int)x.Value.SelectToken("$.build_cost_ic"),
                    build_cost_mp = (int)x.Value.SelectToken("$.build_cost_manpower"),
                    build_time = (int)x.Value.SelectToken("$.build_time"),
                    tp = (string)x.Value.SelectToken("$.type"),
                    HA = (int)x.Value.SelectToken("$.hard_attack"),
                    SA = (int)x.Value.SelectToken("$.soft_attack"),
                    max_strength = (int)x.Value.SelectToken("$.max_strength"),
                    default_organization = (int)x.Value.SelectToken("$.default_organisation"),
                    default_morale = (decimal)x.Value.SelectToken("$.default_morale"),
                    repair_cost = (int)x.Value.SelectToken("$.repair_cost_multiplier"),
                    speed = (int)x.Value.SelectToken("$.maximum_speed"),
                    defensiveness = (decimal)x.Value.SelectToken("$.defensiveness"),
                    toughness = (decimal)x.Value.SelectToken("$.toughness"),
                    softness = (decimal)x.Value.SelectToken("$.softness"),
                    air_defence = (int)x.Value.SelectToken("$.air_defence"),
                    sup = (int)x.Value.SelectToken("$.suppression"),
                    aa = (int?)x.Value.SelectToken("$.air_attack"),
                    ap = (int?)x.Value.SelectToken("$.ap_attack"),
                    river = (int?)x.Value.SelectToken("$.river.attack"),
                    woods = (int?)x.Value.SelectToken("$.woods.attack"),
                    forest = (int?)x.Value.SelectToken("$.forest.attack"),
                    d_river = (int?)x.Value.SelectToken("$.river.defence"),
                    d_woods = (int?)x.Value.SelectToken("$.woods.defence"),
                    d_forest = (int?)x.Value.SelectToken("$.forest.defence"),
                    supplies = (int)x.Value.SelectToken("$.supply_consumption"),
                    fuel = (int?)x.Value.SelectToken("$.fuel_consumption"),
                    combat_width = (int)x.Value.SelectToken("$.combat_width"),
                    officers = (int)x.Value.SelectToken("$.officers")

                }
                ).ToList();

            Debug.WriteLine(fmt, "Name", "Str", "Org", "Morale", "Officers", "Softness", "IC", "Manpower", "Time", "SA", "HA");
            foreach(var u in units2)
            {
                Debug.WriteLine(fmt, u.name, u.max_strength, u.default_organization, u.default_morale, u.officers, u.softness, u.build_cost_ic, u.build_cost_mp, u.build_time, u.SA, u.HA);
            }
            Console.WriteLine("Adjust for Technology");
            
            foreach (var tech in techs)
            {
                Console.WriteLine("--> For " + tech.Key);
                var current_save_tech = save_tech[tech.Key];
                var level = (int)current_save_tech.SelectToken("$[0]");
                
                var applicable_pairs = (
                    from JProperty applicable_tech in tech.Value
                    join applicable_unit in units on applicable_tech.Name equals applicable_unit.Name
                    select new { applicable_tech, applicable_unit }
                ).ToList();

                
                foreach (var pair in applicable_pairs)
                {
                    var values = (
                        from JProperty L in (pair.applicable_tech.Value as JObject).Properties()
                        join JProperty R in (pair.applicable_unit.Value as JObject).Properties() on L.Name equals R.Name
                        select new { Left = L, Right = R }).ToList();

                    var prop = (pair.applicable_tech.Parent.Parent as JProperty);
                    save_tech[prop.Name][0].Value<int>();

                    foreach (var modified_item in values)
                    {
                        Debug.Print(modified_item.Left.Name + " : " + modified_item.Left.Name);
                    }

                }
            }
            Console.WriteLine("Done");

        }
        static void Savegame()
        {
            //string n = "ñ";

            //var b = Encoding.UTF8.GetBytes(n);
            //var x = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, b);
            //return;


            string filename = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\BlackICE\save games\Germany1937_02_12_15.hoi3";

            using (var s = new StreamReader(filename, Paralizer.ParalizerCore.MyDefaultEncoding, true))
            {
                Console.WriteLine("Sanitizing");
                var str = Paralizer.ParalizerCore.Sanitize(s);
                File.WriteAllText(filename + ".test", str, Paralizer.ParalizerCore.MyDefaultEncoding);
            }

            Console.WriteLine("Reading");
            var obj = Paralizer.ParalizerCore.FromParadoxFile(filename + ".test");

            Console.WriteLine("Json");
            var json = Paralizer.ParalizerCore.ToJson(obj);
            File.WriteAllText(filename + ".json", json.ToString());
            var db = new DataImportSqlite();
            db.Put("save", Path.GetFileName(filename), json["GER"]);
        }
        static void Main(string[] args)
        {
            Test();
            //Savegame();
            //new Unitizer().Unitify();
            Console.WriteLine("fuck");
            return;
//            try
//            {
//                if (args.Length < 3)
//                    throw new ArgumentException("not enough args");
//                string[] provs = new string[] { "9421", "5602", "5546", "9330",
//"5352", "5354", "5353", "5320",
//"5441", "5479", "5500", "5501", "5458",
//"9272", "5395", "5374", "5351",
//"5350", "5373", "5412", "5428",
//"5324", "5356", "5377", "5375", "5376"};

//                string fmt = args[0];
//                string src = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\BlackICE\save games\prov.txt";//args[1];

//                string dst = args[2];
                //using (StreamWriter prov2 = new StreamWriter(@"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\BlackICE\save games\prov2.txt"))
                //{
                //    var text2 = File.ReadAllText(@"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\BlackICE\save games\prov.txt");
                //    var matches = Regex.Matches(text2, @"^(?<id>\d+)=(?<crap>.*?)controller=""GER""(?<crap3>.*?)infra=(?<crap2>.*?){(?<value>.*?)}.*?^(?<id2>\d+)=", RegexOptions.Multiline | RegexOptions.Singleline);
                //    foreach (Match m in matches)
                //    {
                        
                //        var str = Regex.Replace
                //            (m.Value, @".*?infra=(?<crap2>.*?)\{\s*?(?<value1>\d+\.\d+)\s*?(?<value2>\d+\.\d+)\s*?\}","infra=\r\n\t{\r\n 10.000 10.000\t}");
                //        Debug.WriteLine(m.Groups["value"].Value + " ==> " + str);
                //    }
                //}
                //return;
            //    if (fmt == "json")
            //    {
            //        var data = Paralizer.ParalizerCore.FromParadoxFile(src);
            //        foreach (var obj in data.Children.Where(n=> provs.Contains(n.Name)))
            //        {
            //            var list = obj.Children.Where(n => n.Name == "controller").SingleOrDefault();
            //            if (list == null)
            //                continue;
                        
            //            list.Content = "GER";

            //            //if (!list.Content.ToString().Contains("GER"))
            //            //    continue;

            //            //list = obj.Children.Where(n => n.Name == "infra").SingleOrDefault();
            //            //if (list == null)
            //            //    continue;



            //            //Debug.WriteLine(obj.Name + " : " + list.Content);
            //            //foreach (var val in list.Children)
            //            //{
            //            //    Debug.WriteLine("\t" + val.Content.ToString());
            //            //    val.Content = 10.000;
            //            //}
            //        }

            //        Paralizer.ParalizerCore.ToParadox(data, @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\BlackICE\save games\prov2.txt");
            //        //var json = Paralizer.ParalizerCore.ToJson(data);

            //        //File.WriteAllText(dst, json.ToString());
            //    }
            //    else if (fmt == "pdox")
            //    {
            //        var text = File.ReadAllText(src);
            //        var json = JObject.Parse(text);
                     
            //        var data = Paralizer.ParalizerCore.FromJson(json);

            //        Paralizer.ParalizerCore.ToParadox(data, dst);

            //    }
            //    else
            //    {
            //        throw new ArgumentException("invalid format specified");
            //    }

            //}
            //catch (ArgumentException a)
            //{
            //    Console.WriteLine(a);
            //    Usage();
            //}

        }

        private static void Usage()
        {
            string usage = @"FormatConverter fmt src dst
    fmt Target format, 'json' or 'pdox'
    src File containing data which uses the format opposite of 'fmt'
    dst File into which to place converted data 
";
            Console.WriteLine(usage);
        }
    }
}
