 /*************************************************************************
	Date  : 1/17/2015 12:00:00 AM  
	Author: Robert A. Olliff
	Email : robert.olliff@gmail.com

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RagelP;
using Newtonsoft.Json.Linq;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;

namespace Paralizer
{
    /// <summary>
    /// The strategy used to parse this bizarre data format is,
    /// "What would this look like in JSON?" and
    /// "How could that JSON be converted back to the original?"
    /// </summary>
    /// 
    /// 
    /// @see Paralizer.ObjectType
    /// 
    /// To answer the first question, we first have to figure out how
    /// the Paradox format works.
    /// 
    /// Propeties:
    /// 
    /// @code
    ///     name = value
    /// @endcode
    /// 
    /// Name can be a String, Number, or Date (Dates discussed later) 
    /// Value can be String, Number, Date, Object, Array
    /// Objects
    ///  Objects begin with a '{' token and end with a '}' on the same level
    ///  Objects contain zero or more Properies(s)
    ///  When an object has two or more Properties which have the same Name, the Properties are
    ///  collapsed into a single Property with the same name. The Value of the Property
    ///  is an Array of the collapsed Property Values.
    ///  
    /// @code
    /// 
    /// // Object
    /// victory_conditions={
    ///  
    ///    // Duplicate Property 1
    ///    faction={               
    ///     operation_sealion 0.1.0.0 operation_barbarossa 0.1.0.0 axis }
    ///  
    ///    // Duplicate Property 2
    ///    faction={
    ///        operation_overlord 0.1.0.0 operation_torch 0.1.0.0 allies }
    /// 
    ///    // Duplicate Property 3
    ///    faction={
    ///        operation_bagration 0.1.0.0 operation_august_storm 0.1.0.0 comintern }
    ///  
    ///    game_finished=no
    /// }
    /// @endcode
    ///
    /// written in JSON, this would look like:
    /// @code
    /// "victory_conditions" : {
    ///     "faction" :
    ///     [
    ///         // Value for Dupe1,
    ///         // Value for Dupe2,
    ///         // Value for Dupe3
    ///     ],
    ///     "game_finished" : false
    /// }
    ///     
    /// @endcode
    /// Or, even better:
    /// @code
    /// "victory_conditions" : {
    ///     "faction" : {
    ///         "axis" : {
    ///             "operation_whatever" : "0.1.0.0"
    ///         },
    ///         "allies" : {
    ///             "operation_turtleneck" : "0.1.0.0"
    ///         },
    ///         // etc
    ///     },
    ///     "game_finished" : false
    /// }
    /// @endcode
    /// That last example, although ideal, would require predefined knowledge of what a "faction"
    /// object is; which is beyond the scope of this parser.
    /// 
    /// In the code for this project, an array of Values which were formerly the values of duplicate propeties
    /// are referred to as "DuplicatesArray"'s, which will help us when converting from/to JSON/Paradox.

    ///
    /// Grammar Notes
    /// 
    /// My ANTLR is rusty, but a grammer might look something like:
    ///
    /// program  := (object|property)*
    /// object   := Identifier Equals Begin (object|property|member)* End
    /// member   := String|Integer|Float|Date
    /// property := Identifier Equals member
    ///
    /// Identifier   := [\w_][\w_\d]*? Whitespace
    /// Whitespace   := \s+
    /// String       := .*?"|[^#=\{\}\s]+
    /// Boolean      := (yes|no|true|false)
    /// Integer      := -?\d+
    /// Float        := -?(\d+\.\d*|\d*\.\d+)
    /// Date         := \d\d\d\d\.\d+\.\d+\.\d+
    /// Equals       := =
    /// Begin        := \{
    /// End          := \}        
    /// 
    /// 
        
    ///
    /// Simple Array
    /// ===============
    /// 
    /// simple_array=
    /// {
    ///     0 43 636 12345 
    /// }
    /// 
    /// faction=
    /// {
    ///     operation_sealion 0.1.0.0 operation_barbarossa 0.1.0.0 
    /// }
    /// 
    /// Mighty Morphin' Array
    /// =================
    /// 
    /// When parsing a paradox file, you find that an object will suddenly decide to become an array.
    /// For example,
    /*! 
    victory_conditions={
        faction={
            operation_sealion 0.1.0.0 operation_barbarossa 0.1.0.0 axis }
        faction={
            operation_overlord 0.1.0.0 operation_torch 0.1.0.0 allies }
        faction={
            operation_bagration 0.1.0.0 operation_august_storm 0.1.0.0 comintern }
        game_finished=no
    }
     */
    /// victory_conditions is an object, it seems to contain other objects.
    /// faction is an object, no wait, it's an array. All of its elements are nameless.
    /// Ok fine, it's array. Now, the next member of victory_conditions is... another faction.
    /// So that faction object we just found is not a member of victory_conditions, but rather
    /// an element in an array of 'faction' objects. The array, named faction, is the real member
    /// of victory_conditions.
    /// 
    /// 
    /// associative objects (assjects) 
    /// These have properties and other objects (Things that equal other things)
    /// assject=
    /// {
    ///     # object
    ///     id=
    ///     {
    ///         id=7000
    ///         type=40
    ///     }
    ///     
    ///     # property
    ///     owner="NOR"
    /// }
    internal class WeirdFormatParser
    {
        static IndentedTextWriter logger = new IndentedTextWriter(new StreamWriter(new FileStream("weird_format_parser.log", FileMode.Create)));
        private WeirdFormatParser() { }

        public static ParadoxDataElement ParseParadox(TextReader src)
        {
            return ParseParadox(src.ReadToEnd());
        }

        public static ParadoxDataElement ParseParadox(string src)
        {            
            var v = new RagelP.Scanner();
            v.init();

            Regex expression = new Regex("#(?<crap>.*?)$", RegexOptions.Multiline);

            string data = expression.Replace(src, "");
            v.exec(data);
            
            return ParseParadox(new TokenIterator(v.finish().List));
        }

        private static ParadoxDataElement ParseParadox(TokenIterator iter)
        {   
            JObject root = new JObject();
            var Root = new ParadoxDataElement
            {
                Type = ObjectType.Root
            };

            WeirdFormatParser parser = new WeirdFormatParser();
            parser.Iterate(Root, iter);

            return Root;
        }

        private bool InitializeDataObject(ParadoxDataElement obj, TokenIterator iter)
        {
            switch (iter.Value.TokenType)
            {
                case Token.TK_EnterScope:
                    obj.Type = ObjectType.Associative;
                    iter.NextOrDie();
                    Iterate(obj, iter);
                    break;
                case Token.TK_Float:
                    obj.Content = decimal.Parse(iter.CurrentString);
                    obj.Type = ObjectType.Float;
                    break;
                case Token.TK_IntegerDecimal:
                    obj.Content = int.Parse(iter.CurrentString);
                    obj.Type = ObjectType.Integer;
                    break;
                case Token.TK_Dlit:
                    obj.Content = iter.CurrentString.Trim('"');
                    obj.Type = ObjectType.String;
                    break;
                case Token.TK_Date:
                    obj.Content = iter.CurrentString;
                    obj.Type = ObjectType.Date;
                    break;
                case Token.TK_Id:
                    obj.Content = iter.CurrentString;
                    obj.Type = ObjectType.Name;
                    break;
                default:
                    {
                        if (iter.CurrentString == "yes")
                        {
                            obj.Content = true;
                            obj.Type = ObjectType.Boolean;

                        }
                        else if (iter.CurrentString == "no")
                        {
                            obj.Content = false;
                            obj.Type = ObjectType.Boolean;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
            }
            return true;
        }
        
        static Token[] LeftHandTypes = new Token[] { Token.TK_Id, Token.TK_Date, Token.TK_IntegerDecimal };

        private void Iterate(ParadoxDataElement obj, TokenIterator iter)
        {
            do
            {
                if (LeftHandTypes.Contains(iter.Value.TokenType) && iter.LookAhead().TokenType == Token.TK_Equals)
                {
                    // The enter scope case handle this
                    if (iter.LookAhead(2).TokenType == Token.TK_EnterScope)
                        continue;

                    var id = new ParadoxDataElement { Name = iter.CurrentString, Location = iter.Value };

                    // Skip LH name
                    iter.NextOrDie();

                    // Skip '='
                    iter.NextOrDie();

                    if (InitializeDataObject(id, iter))
                        obj.AddObject(id);
                }
                else 
                if (iter.Value.TokenType == Token.TK_EnterScope)
                {
                    
                    if (iter.Current.Previous.Value.TokenType == Token.TK_Equals)
                    {
                        var name_token = iter.Current.Previous.Previous.Value;

                        if (!LeftHandTypes.Contains(name_token.TokenType))
                            throw new LexerException("Expected identifier but found {0}", name_token.ToString());

                        var o = new ParadoxDataElement { 
                            Name = name_token.Raw, 
                            Location = name_token
                        };

                        if (InitializeDataObject(o, iter))
                            obj.AddObject(o);
                    }
                    else if (
                        iter.Current.Previous.Value.TokenType == Token.TK_EnterScope ||
                        iter.Current.Previous.Value.TokenType == Token.TK_LeaveScope
                        )
                    {                        
                        var anon = new ParadoxDataElement { Name = "<anonymous>" };
                        anon.Type = ObjectType.AssociativeAnonymous;
                        iter.NextOrDie();
                        Iterate(anon, iter);
                        obj.AddObject(anon);
                    }
                }
                else if (iter.Value.TokenType == Token.TK_LeaveScope)
                {
                    logger.Indent--;
                    FinishHim(obj);
                    break;
                }
                else if (iter.Value.TokenType == Token.TK_Equals)
                {
                    // Do Nothing
                }
                else
                {
                    var id = new ParadoxDataElement { Name = null };
                    if (InitializeDataObject(id, iter))
                        obj.AddElement(id);
                }

            } while (iter.Next());
        }

        private void FinishHim(ParadoxDataElement obj)
        {
            //int total = obj.Children.Count();
            //int with_names = obj.Children.Count(n => n.Name != null);
            //if (with_names != 0 && with_names != total)
            //{
            //    var unnamed = obj.Children.Where(n => n.Name == null).ToList();

            //}

            //if (with_names == 0)
            //{
            //    logger.WriteLine("Morphing to array {0}", obj.Name);
            //    obj.Type = ObjectType.Array;
            //}
        }
    }
}
