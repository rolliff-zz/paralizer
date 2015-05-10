 /*************************************************************************
	Date  : 1/17/2015 12:00:00 AM  
	Author: Robert A. Olliff
	Email : robert.olliff@gmail.com

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace RagelP
{
    public static class TokenIteratorExtensions
    {
        static string []qualifiers_ = new string[]{
            "const","volatile"
        };

        
        public static bool IsQualifier(this LinkedListNode<TokenData> td)
        {
            return (td != null &&
                qualifiers_.Contains(td.Value.Raw));
        }
        public static bool IsQualifier(this TokenIterator iter)
        {
            return (iter.Current != null &&
                qualifiers_.Contains(iter.Current.Value.Raw));
        }
    }
    public class TokenIterator
    {
        public static void Trace()
        {
            System.Diagnostics.StackTrace trace = new StackTrace();
            var c = trace.GetFrame(1);
            var name = c.GetMethod().Name;
            var parent = c.GetMethod().DeclaringType.Name;

            Debug.Print("Enter {0}::{1}", parent, name);
        }

        Stack<LinkedListNode<TokenData>> states = new Stack<LinkedListNode<TokenData>>();

        LinkedListNode<TokenData> mark_;
        public void MarkSet()
        {
            mark_ = Current;
        }
        public void MarkClear()
        {
            mark_ = null;
        }
        public void MarkReturn()
        {
            if (mark_ == null)
                throw new InvalidOperationException("Mark has not been set");

            Current = mark_;
        }
        public LinkedListNode<TokenData> Current { get; private set; }
        public LinkedListNode<TokenData> End { get; private set; }
        public TokenData Value { get { return Current.Value; } }
        public string CurrentString
        {
            get
            {
                if(Current != null)
                {
                    if(Current.Value !=null)
                    {
                        return Current.Value.Raw;
                    }
                }
                return null;
            }
        }
        public Boolean DebuggingEnabled { get; set; }
        public String CurrentLine
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                var temp = Current;
                while (temp != null && temp.Value.Raw.Trim(' ') != "\r\n" && temp.Value.Raw.Trim(' ') != "\n")
                    temp = temp.Previous;

                if (temp == null)
                    return String.Empty;
                temp = temp.Next;
                sb.Append(temp.Value.Raw);
                while (temp != null && temp.Value.Raw.Trim(' ') != "\r\n" && temp.Value.Raw.Trim(' ') != "\n")
                {
                    temp = temp.Next;
                    sb.Append(temp.Value.Raw);
                }

                return sb.ToString();
            }
        }


        public TokenIterator(LinkedListNode<TokenData> node, LinkedListNode<TokenData> end)
        {
            DebuggingEnabled = false;
            Current = node;
            End = end;
        }
        public TokenIterator(LinkedList<TokenData> list) :
            this(list.First, list.Last)
        {

        }
        public TokenIterator(LinkedListNode<TokenData> node) :
            this(node, null)
        {

        }

        public void SeekEnd()
        {
            if (Current == null)
                return;
            Current = Current.List.Last;
        }

        public TokenData LookAhead(int distance = 1)
        {
            var tmp = Current;
            for (int n = 0; n < distance; ++n)
            {
                if (tmp == null)
                    return TokenData.Invalid;

                tmp = tmp.Next;
            }
            return tmp.Value;
        }
        public bool ForwardCount(int count)
        {
            int c = 0;
            return ForwardUntil(n =>
            {
                return c++ >= count;
            });
        }

        public bool BackCount(int count)
        {
            int c = 0;
            return BackUntil(n =>
            {
                return c++ >= count;
            });
        }

        public bool ForwardLine()
        {
            if (ForwardUntil("\n"))
            {
                return Next();
            }
            return false;
        }

        public bool BackLine()
        {
            if (BackUntil("\n"))
            {
                return Previous();
            }
            return false;
        }
        public TokenIterator GetScope()
        {
            if (!ForwardUntil(Token.TK_EnterScope))
                throw new Exception("No enter scope");
            var start = Current;
            LinkedListNode<TokenData> end = null;
            int level = 1;
            while (Next())
            {
                if (Current.Value.TokenType == Token.TK_EnterScope)
                {
                    level++;
                }
                else if (Current.Value.TokenType == Token.TK_LeaveScope)
                {
                    level--;
                    if (level == 0)
                    {
                        end = Current;
                        break;
                    }
                }
            }

            if (end == null)
                throw new Exception("no leave scope");
            return new TokenIterator(start, end);
        }
        public void PushState()
        {
            states.Push(Current);
        }
        public void PopState()
        {
            Current = states.Pop();
        }


        // //////////////////////////////////////
        //  Forwards/Backwards base functions
        // //////////////////////////////////////

        public bool BackUntil(Func<LinkedListNode<TokenData>, bool> pred, bool look_only = false)
        {
            int i = 0;
            //Trace();
            return BackUntil(pred, ref i);
        }


        public bool ForwardUntil(Func<LinkedListNode<TokenData>, bool> pred, bool look_only = false)
        {
            int i = 0;
            //Trace();
            return ForwardUntil(pred, ref i);
        }

        public bool BackUntil(Func<LinkedListNode<TokenData>, bool> pred, ref int count, bool look_only = false)
        {
            return Until(pred, (LinkedListNode<TokenData> t) => { return t.Previous; }, ref count);
        }

        public bool ForwardUntil(Func<LinkedListNode<TokenData>, bool> pred, ref int count, bool look_only = false)
        {
            return Until(pred, (LinkedListNode<TokenData> t) => { return t.Next; }, ref count);
        }

        private bool Until(Func<LinkedListNode<TokenData>, bool> pred, Func<LinkedListNode<TokenData>, LinkedListNode<TokenData>> action, ref int count, bool look_only = false)
        {
            var cur = Current;
            int trip = 0;
            bool ret = false;
            for (; cur != End; cur = action(cur), ++trip)
            {

                if (count > 0 && trip == count)
                {
                    break;
                }

                if (pred(cur))
                {
                    if (DebuggingEnabled)
                    {
                        Debug.Print("Match: {0,-16} : {1}", cur.Value.Raw, cur.Value.TokenType.ToString());
                    }
                    if (!look_only)
                        Current = cur;
                    ret = true;
                    break;
                }
                else if (DebuggingEnabled)
                {
                    Debug.Print("Continue: {0,-16} : {1}", cur.Value.Raw, cur.Value.TokenType.ToString());
                }
            }

            count = trip;

            return ret;
        }

        // //////////////////////////////////////
        //  Forwards/Backwards
        // //////////////////////////////////////

        public bool ForwardUntil(Token t)
        {
            return ForwardUntil(n => n.Value.TokenType == t);
        }

        public bool BackUntil(Token t)
        {
            return BackUntil(n => n.Value.TokenType == t);
        }

        public bool ForwardUntil(TokenAttr attr)
        {
            return ForwardUntil(n => n.Value.Attributes.HasFlag(attr));
        }

        public bool BackUntil(TokenAttr attr)
        {
            return BackUntil(n => n.Value.Attributes.HasFlag(attr));
        }

        public bool ForwardUntil(string str)
        {
            return ForwardUntil(n => n.Value.Raw == str);
        }

        public bool BackUntil(string str)
        {
            return BackUntil(n => n.Value.Raw == str);
        }

        // //////////////////////////////////////
        //  Forwards/Backwards with max count
        // //////////////////////////////////////
        public bool ForwardUntil(Token t, ref int count, bool look_only = false)
        {
            return ForwardUntil(n => n.Value.TokenType == t, ref count, look_only);
        }

        public bool BackUntil(Token t, ref int count, bool look_only = false)
        {
            return BackUntil(n => n.Value.TokenType == t, ref count, look_only);
        }

        public bool ForwardUntil(TokenAttr attr, ref int count, bool look_only = false)
        {
            return ForwardUntil(n => n.Value.Attributes.HasFlag(attr), ref count, look_only);
        }

        public bool BackUntil(TokenAttr attr, ref int count, bool look_only = false)
        {
            return BackUntil(n => n.Value.Attributes.HasFlag(attr), ref count, look_only);
        }

        public bool ForwardUntil(string str, ref int count, bool look_only = false)
        {
            return ForwardUntil(n => n.Value.Raw == str, ref count, look_only);
        }

        public bool BackUntil(string str, ref int count, bool look_only = false)
        {
            return BackUntil(n => n.Value.Raw == str, ref count, look_only);
        }

        public void NextOrDie()
        {
            if (!Next())
                throw new UnexpectedEndOfFile();
        }
        public void NextOrDie(Token expected_type)
        {
            var cur = Current;

            if (!Next())            
                throw new SyntaxException(cur.Value, "Expected next to be {0}, but was null", expected_type);
            if (Current.Value.TokenType != expected_type)
                throw new SyntaxException(cur.Value, "Expected next to be {0}, but was {1}", expected_type, Current.Value.TokenType);
        }

        public void NextOrDie(String expected_text)
        {
            var cur = Current;

            if (!Next())
                throw new SyntaxException(cur.Value, "Expected next to be {0}, but was null", expected_text);
            if (Current.Value.Raw != expected_text)
                throw new SyntaxException(cur.Value, "Expected next to be {0}, but was {1}", expected_text, Current.Value.Raw);
        }
        public bool Next()
        {
            //Trace();
            if (Current == null)
                return false;
            if(DebuggingEnabled)
                Debug.WriteLine(Current.Value.ToString());       

            Current = Current.Next;
            return Current != End && Current != null;
        }
        public bool Previous()
        {
            //Trace();
            Current = Current.Previous;
            return Current != null;
        }

        public static string GetString(LinkedListNode<TokenData> start, LinkedListNode<TokenData> end)
        {
            TokenIterator iter = new TokenIterator(start, end);
            StringBuilder sb = new StringBuilder();
            do
            {
                sb.Append(iter.Current.Value.Raw);
            } while (iter.Next());
            return sb.ToString();
        }
    }
}
