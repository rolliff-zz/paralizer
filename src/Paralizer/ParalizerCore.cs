 /*************************************************************************
	Date  : 1/17/2015 12:00:00 AM  
	Author: Robert A. Olliff
	Email : robert.olliff@gmail.com

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RagelP;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Paralizer
{
    /// <summary>
    /// Paradox Serializer
    /// </summary>
    public class ParalizerCore
    {
        // @todo use log4net
        static IndentedTextWriter logger = new IndentedTextWriter(new StreamWriter(new FileStream("paralizer_core.log",FileMode.Create)));
        private ParalizerCore()
        {
            
        }
        /// <summary>
        /// JSON and Paradox disagree on what constitutes a valid key name. JSON doesn't like dots.
        ///  Technically, you can do this in JSON:
        ///     "1942.1.1.0" : { junk: true }
        ///  But is illadvised and will cause problems in components like MongoDB and JavaScript.
        /// So, we fix this by converting dots to lo-dash. 
        /// </summary>
        private static Regex paradox_date = new Regex("\\d\\d\\d\\d\\.\\d+\\.\\d+\\.\\d+", RegexOptions.None);

        private static Regex jsonized_paradox_date = new Regex("\\d\\d\\d\\d_\\d+_\\d+_\\d+", RegexOptions.None);

        private StringBuilder builder = new StringBuilder();
        private IndentedTextWriter writer;

        private string GetStringResult()
        {
            return builder.ToString();
        }

        public static readonly Encoding MyDefaultEncoding = Encoding.GetEncoding(1252);

        public static void TestEncoding()
        {
            string input = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\test_input.txt";
            string output = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\test_output.txt";

            string tmp;
            using (var s = new StreamReader(input, MyDefaultEncoding, true))
                tmp = s.ReadToEnd();

            using (var s2 = (new StreamWriter(new FileStream(output, FileMode.Create), MyDefaultEncoding)))
                s2.Write(tmp);
        }

        public static void TestEncoding2()
        {
            string input = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\test_input.hoi3";
            string output = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\autosave_out1.hoi3";
            string output2 = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\autosave_out2.hoi3";
            string output3 = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\autosave_out3.hoi3";
            string output4 = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\autosave_out4.hoi3";
            
            string json_output = @"C:\Users\Robby\Documents\Paradox Interactive\Hearts of Iron III\TRP\save games\test_output.js";


            Debug.WriteLine("Test 1, text to text");
            {
                string tmp;
                using (var s = new StreamReader(input, MyDefaultEncoding, true))
                    tmp = s.ReadToEnd();

                using (var s2 = (new StreamWriter(new FileStream(output, FileMode.Create), MyDefaultEncoding)))
                    s2.Write(tmp);
            }
            Debug.WriteLine("Test 2, from text, to text");
            var obj = FromParadoxFile(input);            
            ToParadox(obj, output2);

            Debug.WriteLine("Test 3, from text, to json, to text");

            var json = ToJson(obj);
            File.WriteAllText(output3, json.ToString());

            Debug.WriteLine("Test 4, from text, to json, from json, to text");
            obj = FromJson(json);
            ToParadox(obj, output4);
        }

        public static string Sanitize(TextReader input)
        {
            string n = input.ReadToEnd();
            //var b = Encoding.UTF8.GetBytes(n);
            //var x = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, b);
            //return Encoding.ASCII.GetString(x);
            

            StringBuilder sb = new StringBuilder(n.Length * 2);
            List<char> repl = new List<char>() { '\'', 'ñ' };
            unsafe 
            {
                fixed(char* p = n)
                {
                    char* s = p;
                    int count = n.Length-1;

                    do
                    {
                        if (s[count] == '=')
                        {
                            sb.Append(s[count]);
                            count--;
                            while (count >= 0 && !Char.IsWhiteSpace(s[count]))
                            {

                                if(s[count] >= 'À' || s[count] == '\'')
                                {
                                    var temp = new string(Convert.ToInt32((char)s[count]).ToString("X").Reverse().ToArray());
                                    sb.Append("_X");
                                    sb.Append(temp);
                                    sb.Append("X_");
                                    count--;
                                    break;
                                }
                                
                                sb.Append(s[count]);
                                count--;
                            }
                        }
                        else
                        {
                            sb.Append(s[count]);
                            count--;
                        }
                    }while(count>=0);

                }
            }
            return new string(sb.ToString().Reverse().ToArray());
        }
        
        /// <summary>
        /// Write the obejct to a string
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="dest"></param>
        public static void ToParadox(ParadoxDataElement obj, TextWriter dest)
        {            
            ParalizerCore thing = new ParalizerCore();
            if (dest is IndentedTextWriter)
                throw new InvalidOperationException("Don't pass an IndentedTextWriter, whatever you pass will be wrapped in one. Too bad");

            thing.writer = new IndentedTextWriter(dest);

            if (obj.Type == ObjectType.Root)
                thing.ParalizeObject(obj);
            else
                thing.ParalizeMember(obj);

            thing.writer.Flush();
            thing.writer.Close();
        }

        /// <summary>
        /// Write the object to a file
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="filename"></param>
        public static void ToParadox(ParadoxDataElement obj, string filename)
        {
            try
            {
                using (var s = (new StreamWriter(new FileStream(filename, FileMode.Create), MyDefaultEncoding)))
                    ToParadox(obj, s);
            }
            catch (IOException x)
            {
                var f = Path.GetFileNameWithoutExtension(filename);
                filename = filename.Replace(f, f + "2");
                using (var s = (new StreamWriter(new FileStream(filename, FileMode.Create), MyDefaultEncoding)))
                    ToParadox(obj, s);
            }
        }

        /// <summary>
        /// Return the object in it's string form
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToParadox(ParadoxDataElement obj)
        {
            var s = (new StringWriter());
            ToParadox(obj, s);
            return s.ToString();
        }

        public static ParadoxDataElement FromParadox(TextReader src)
        {
            return WeirdFormatParser.ParseParadox(src);
        }

        public static ParadoxDataElement FromParadoxFile(string path)
        {
            using(var s = new StreamReader(path,MyDefaultEncoding,true))
                return WeirdFormatParser.ParseParadox(s);
        }
        public static ParadoxDataElement FromJson(JToken json)
        {
            var Root = new ParadoxDataElement
            {
                Type = ObjectType.Root,
                Name = "<root>"
            };

            ParalizerCore thing = new ParalizerCore();

            thing.Iterate(Root, json);

            return Root;
        }

        public static JToken ToJson(ParadoxDataElement obj)
        {
            ParalizerCore thing = new ParalizerCore();
            if (obj.Type == ObjectType.Root)
                return thing.SerializeObject(obj);
            else
            {
                JObject o = new JObject();
                o[obj.Name] = thing.ToJToken(obj);
                return o;
            }
        }

        #region ParadoxDataElement/ParadoxWeirdFormat

        private void ParalizeMember(ParadoxDataElement obj)
        {
            switch (obj.Type)
            {
                case ObjectType.Date:
                case ObjectType.Name:
                case ObjectType.Float:
                case ObjectType.Integer:
                case ObjectType.Boolean:
                    var content = obj.Content.ToString();
                    if (obj.Type == ObjectType.Boolean)
                    {
                        if (((bool)obj.Content))
                        {
                            content = "yes";
                        }
                        else
                        {
                            content = "no";
                        }
                    }

                    if (String.IsNullOrEmpty(obj.Name))
                        writer.Write("{0} ", content);
                    else
                        writer.WriteLine("{0}={1}", obj.Name, content);
                    break;

                case ObjectType.String:
                    if (String.IsNullOrEmpty(obj.Name))
                        writer.Write("\"{0}\" ", obj.Content.ToString());
                    else
                        writer.WriteLine("{0}=\"{1}\"", obj.Name, obj.Content.ToString());
                    break;
                case ObjectType.DuplicatesArray:
                    foreach (var v in obj.Children)
                    {
                        ParalizeMember(v);
                    }
                    break;
                case ObjectType.Array:
                case ObjectType.Associative:
                case ObjectType.AssociativeAnonymous:

                    var str = obj.Name;
                    if (jsonized_paradox_date.IsMatch(obj.Name))
                        str = obj.Name.Replace('_', '.');

                    if(obj.Type == ObjectType.Array && obj.Children.Count() < 4)
                        writer.Write("{0}={{", str);
                    else if(obj.Type == ObjectType.AssociativeAnonymous)
                        writer.WriteLine("{{", str);
                    else
                        writer.WriteLine("{0}={{", str);
                    writer.Indent++;
                    ParalizeObject(obj);
                    writer.Indent--;
                    writer.WriteLine("}");
                    break;
                default:
                    throw new LexerException("Impossible!");
            }

        }

        private void ParalizeArray(ParadoxDataElement src)
        {
            foreach (var v in src.Children)
            {
                ParalizeMember(v);
            }
        }

        private void ParalizeObject(ParadoxDataElement src)
        {
            foreach (var v in src.Children)
            {
                ParalizeMember(v);
            }
        }


        private bool ParseJson(ParadoxDataElement obj, JToken value)
        {
            switch (value.Type)
            {
                // Name the current object and dig deeper for the content
                case JTokenType.Property:
                    obj.Name = (value as JProperty).Name;

                    if (jsonized_paradox_date.IsMatch(obj.Name))
                    {
                        obj.Name = obj.Name.Replace('_', '.');
                    }

                    ParseJson(obj, (value as JProperty).Value);
                    break;

                case JTokenType.Object:
                    // Iterate the object items and add them as children
                    logger.Indent++;
                    
                    obj.Type = ObjectType.Associative;
                    Iterate(obj, value);

                    logger.Indent--;
                    break;
                case JTokenType.Array:

                    if (IsSimpleArray(value as JArray))
                    {
                        obj.Type = ObjectType.Array;
                        // Add elements to current object
                        logger.Indent++;
                        foreach (var v in value.Values())
                        {
                            ParadoxDataElement o = new ParadoxDataElement { Name = null };
                            ParseJson(o, v);
                            obj.AddElement(o);
                        }
                        logger.Indent--;
                    }
                    else
                    {
                        obj.Type = ObjectType.DuplicatesArray;
                        logger.Indent++;
                        Iterate(obj, value, obj.Name);
                        logger.Indent--;
                    }
                    break;
                case JTokenType.Float:
                    obj.Content = value.Value<decimal>();
                    obj.Type = ObjectType.Float;
                    break;
                case JTokenType.Integer:
                    obj.Content = value.Value<int>();
                    obj.Type = ObjectType.Integer;
                    break;
                case JTokenType.String:
                    if (jsonized_paradox_date.IsMatch(value.Value<string>()))
                    {
                        obj.Content = value.Value<string>();
                        obj.Type = ObjectType.Date;
                    }
                    else
                    {
                        obj.Content = value.Value<string>();
                        obj.Type = ObjectType.String;
                    }
                    break;
                case JTokenType.Boolean:
                    obj.Content = value.Value<bool>();
                    obj.Type = ObjectType.Boolean;
                    break;
                default:
                    break;
            }
            return true;
        }

        private bool IsSimpleArray(JArray jArray)
        {
            foreach (var v in jArray)
            {
                if (v.Type == JTokenType.Property || v.Type == JTokenType.Object || v.Type == JTokenType.Array)
                    return false;
            }
            return true;
        }

        private void Iterate(ParadoxDataElement obj, JToken root, string name = null)
        {
            foreach (var value in root.Children())
            {
                ParadoxDataElement current = new ParadoxDataElement { Name = name };
                ParseJson(current, value);
                if (current.Type == ObjectType.DuplicatesArray)
                {
                    List<ParadoxDataElement> tmp = new List<ParadoxDataElement>(current.Children);
                    foreach (var o in tmp)
                        o.TransferOwnership(obj);
                }
                else if (!String.IsNullOrEmpty(current.Name))
                {
                    logger.WriteLine("{0}.AddObject<{2}>({1})", obj.Name, current.Name, current.Type);
                    obj.AddObject(current);
                }
                else
                {
                    logger.WriteLine("{0}.AddElement<{2}>({1})", obj.Name, current.Name, current.Type);
                    obj.AddElement(current);
                }
            }
        }

        #endregion

        #region JSON/ParadoxDataElement

        private ParadoxDataElement FromProperty(JProperty prop)
        {
            string name = prop.Name;
            var obj = new ParadoxDataElement();
            if (jsonized_paradox_date.IsMatch(name))
            {
                name = name.Replace("_", ".");
            }

            obj.Name = name;
            FromJToken(obj, prop.Value);
            return obj;
        }
        private void FromJToken(ParadoxDataElement obj, JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Array:
                    obj.Type = ObjectType.Array;
                    SerializeJsonArray(obj, token as JArray);
                    break;
                case JTokenType.Object:
                    obj.Type = ObjectType.Associative;
                    SerializeJsonObject(obj, token as JObject);
                    break;
                case JTokenType.Boolean:
                    obj.Content = token.Value<bool>();
                    obj.Type = ObjectType.Boolean;
                    break;
                case JTokenType.Float:
                    obj.Content = token.Value<decimal>();
                    obj.Type = ObjectType.Float;
                    break;
                case JTokenType.Integer:
                    obj.Content = token.Value<int>();
                    obj.Type = ObjectType.Integer;
                    break;
                case JTokenType.Property:
                    if ((token as JProperty).Name == "_meta_")
                        return;

                    if ((token as JProperty).Name == "_inline_array_")
                    {
                        foreach (var element in (token as JProperty).Value)
                        {
                            ParadoxDataElement obj2 = new ParadoxDataElement();
                            FromJToken(obj2, element);
                            obj.AddObject(obj2);

                        }
                        return;
                    }

                    obj.AddObject(FromProperty(token as JProperty));
                    break;
                case JTokenType.String:
                    obj.Content = token.Value<string>();
                    obj.Type = ObjectType.String;
                    break;

                case JTokenType.Date:
                case JTokenType.Null:
                default:
                    throw new LexerException("Unknown json type {0}", token.Type.ToString());
            }
        }

        private ParadoxDataElement SerializeJsonObject(ParadoxDataElement obj, JObject json)
        {
            foreach (var item in json.Children())
                FromJToken(obj, item);
            return obj;
        }

        private ParadoxDataElement SerializeJsonArray(ParadoxDataElement obj, JArray json)
        {
            foreach (var item in json.Children())
                FromJToken(obj, item);
            return obj;
        }

        private JToken ToJToken(ParadoxDataElement obj)
        {
            switch (obj.Type)
            {
                case ObjectType.DuplicatesArray:
                case ObjectType.Array:
                    return SerializeArray(obj);
                case ObjectType.InlineArray:
                    return new JProperty("_inline_array_", SerializeArray(obj));
                case ObjectType.Associative:
                case ObjectType.AssociativeAnonymous:
                    return SerializeObject(obj);
                case ObjectType.Date:
                    return (string)obj.Content;
                case ObjectType.Float:
                    return (decimal)obj.Content;
                case ObjectType.Integer:
                    return ((int)obj.Content);
                case ObjectType.Boolean:
                    return ((bool)obj.Content);
                case ObjectType.Name:
                case ObjectType.String:
                    return ((string)obj.Content);
            }
            throw new LexerException("Impossible!");
        }

        private JArray SerializeArray(ParadoxDataElement src)
        {
            JArray a = new JArray();
            foreach (var v in src.Children)
            {
                a.Add(ToJToken(v));
            }
            return a;
        }

        private JObject SerializeObject(ParadoxDataElement src)
        {
            JObject obj = new JObject();
            //obj["_meta_"] = src.Metadata();
            foreach (var v in src.Children)
            {

                if (v.Type == ObjectType.InlineArray)
                {
                    obj.Add(ToJToken(v));

                }
                else
                {
                    var str = v.Name;
                    if (paradox_date.IsMatch(v.Name))
                        str = v.Name.Replace(".", "_");

                    obj[str] = ToJToken(v);
                }
            }
            return obj;
        }
        #endregion
    }
}
