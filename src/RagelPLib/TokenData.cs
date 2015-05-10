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


namespace RagelP
{
    [Flags]
    public enum TokenAttr
    {
        None = 0,
        MemberDef,
        MemberUse,
        ConstantUsage,
        Static,
        Final,
        FunctionCall,
        Import,
        StatementTerminator,
        AddSec,
        AddSecEx,
        AddSecHi,
    }
    public enum Token
    {
        TK_Unknown = 0,
        TK_Dlit = 256,
        TK_Slit,
        TK_Float,
        TK_Id,
        TK_NameSep,
        TK_Arrow,
        TK_PlusPlus,
        TK_MinusMinus,
        TK_ArrowStar,
        TK_DotStar,
        TK_ShiftLeft,
        TK_ShiftRight,
        TK_IntegerDecimal,
        TK_IntegerOctal,
        TK_IntegerHex,
        TK_EqualsEquals,
        TK_NotEquals,
        TK_AndAnd,
        TK_OrOr,
        TK_MultAssign,
        TK_DivAssign,
        TK_PercentAssign,
        TK_PlusAssign,
        TK_MinusAssign,
        TK_AmpAssign,
        TK_CaretAssign,
        TK_BarAssign,
        TK_DotDotDot,
        TK_Whitespace,
        TK_Comment,
        TK_SingleChar,
        TK_Modifier,
        TK_Keyword,
        TK_Const,
        TK_Target,
        TK_Primitive,
        TK_ConstantDef,
        TK_Package,
        TK_Class,
        TK_Function,
        TK_Start,
        TK_Equals,
        TK_EnterScope,
        TK_LeaveScope,
        TK_Import,
        TK_TsLog,
        TK_AdapterExport,
        TK_EOF,
        TK_TOP,
        TK_BOTTOM,
        TK_Date,
        TK_BeginEnd
    }

    [System.Diagnostics.DebuggerDisplay("{TokenType.ToString()}:{Line}:{Column} {Raw}")]
    public class TokenData : IEquatable<TokenData>
    {
        public static TokenData Invalid = new TokenData { TokenType = Token.TK_Unknown, File = null, Column = 0, Line = 0, Raw = "<invalid>" };
        public TokenData()
        {
            Attributes = TokenAttr.None;
        }
        public Token TokenType { get; set; }
        public String Raw { get; set; }
        public String Package { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public TokenAttr Attributes { get; set; }
        public String File { get; set; }
        //internal MajorScope Scope { get; set; }
        public override string ToString()
        {
            return String.Format("{0}:{1}:{2} {3}", TokenType.ToString(), Line, Column, Raw);
        }

        public override bool Equals(object obj)
        {
            var td = obj as TokenData;
            if (null == td)
                return false;
            return Equals(td);
        }

        public bool Equals(TokenData other)
        {
            var td = other;
            if (null == td)
                return false;

            if (this.Column != td.Column)
                return false;

            if (this.Line != td.Line)
                return false;

            if (this.TokenType != td.TokenType)
                return false;

            if (this.Raw != td.Raw)
                return false;

            return true;
        }
    }

}
