 /*************************************************************************
	Date  : 1/17/2015 12:00:00 AM  
	Author: Robert A. Olliff
	Email : robert.olliff@gmail.com

	This file probably has code in it and does stuff.
 ************************************************************************ */
//END STUPID COMMENT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagelP
{
    public static class ExpectExtensions
    {
        
    }
    public class LexerException : Exception
    {
        public LexerException(string mt, params object[] args) :
            base(String.Format(mt, args))
        {
        }
    }

    public class SyntaxException : LexerException
    {
        public SyntaxException(TokenData tok, string mt, params object[] args) :
            base("Error? " + tok.ToString() + ": " + mt, args)
        {
        }
    }

    public class UnexpectedEndOfFile : LexerException
    {
        public UnexpectedEndOfFile() : base("Seems the damned thing just ended out of nowhere. Fix it.") { }
    }
}
