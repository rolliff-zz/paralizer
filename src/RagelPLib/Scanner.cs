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
    public partial class Scanner : RagelBase
    {
        List<LinkedListNode<TokenData>> _adapterExports = new List<LinkedListNode<TokenData>>();
        
        public List<LinkedListNode<TokenData>> AdapterExports { get { return _adapterExports; } }

        private void klass()
        {
        }
        private LinkedListNode<TokenData> token(Token tok)
        {
            if (tok == Token.TK_Whitespace)
                return null;

            int len = te - ts;
            string s = new string(_data, ts, len);
            TokenData dat = null;
            LinkedListNode<TokenData> ret = null;            
            dat = new TokenData() { TokenType = tok, Raw = s};
            ret = _list.AddLast(dat);


            dat.Column = col;
            dat.Line = line;
            dat.File = CurrentFile;
            

            // do some fixups
            if (tok == Token.TK_SingleChar && s == "{")
                dat.TokenType = Token.TK_EnterScope;
            else if (tok == Token.TK_SingleChar && s == "}")
                dat.TokenType = Token.TK_LeaveScope;
            else if (tok == Token.TK_SingleChar && s == "=")
                dat.TokenType = Token.TK_Equals;

           // Debug.Print(dat.ToString());

            foreach(char c in s.ToCharArray())
            {
                if (c == '\n')
                {
                    line += 1;
                    col = 1;
                }
                else
                {
                    col += 1;
                }
            }
            return ret;
        }
    }
}
