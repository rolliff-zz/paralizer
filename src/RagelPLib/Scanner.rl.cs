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
    public partial class Scanner
    {
       
       // int _is_static = 0;
       %%{
	machine CppScanner; 
	write data nofinal;

	# Floating literals.
	fract_const = digit* '.' digit+ | digit+ '.';
	exponent = [eE] [+\-]? digit+;
	float_suffix = [flFL];

    fuck = ([0-9]+ ('st' | 'nd' | 'rd' | 'th'));

	main := |*

	# Single and double literals.
	( 'L'? "'" ( [^'\\\n] | /\\./ )* "'" ) 
		{token( Token.TK_Slit );};
	( 'L'? '"' ( [^"\\] | /\\./ )* '"' ) 
		{token( Token.TK_Dlit );};

#(  ('enum') space+ any* :>> "{" any* :>> "};" ){  klass();};
	
    

    # Identifiers
	( ([a-zA-Z_]|fuck) [a-zA-Z0-9_\.]*('-'?[a-zA-Z0-9_]*)? ) 
		{token( Token.TK_Id );};

    # Paradox Date
    ([0-9]+ '.' [0-9]+ '.' [0-9]+ ('.' [0-9]+)?) 
        { token(Token.TK_Date); };

	# Floating literals.
	( ('-'|'+')? ( fract_const exponent? float_suffix? | digit+ exponent float_suffix? ) )
		{token( Token.TK_Float );};
	
	# Integer decimal. Leading part buffered by float.
	( ('-'|'+')? ( '0' | [1-9] [0-9]* ) [ulUL]{0,3} ) 
		{token( Token.TK_IntegerDecimal );};

	# Integer octal. Leading part buffered by float.
	( '0' [0-9]+ [ulUL]{0,2} ) 
		{token( Token.TK_IntegerOctal );};

	# Integer hex. Leading 0 buffered by float.
	( '0' ( 'x' [0-9a-fA-F]+ [ulUL]{0,2} ) ) 
		{token( Token.TK_IntegerHex );};    

	# Only buffer the second item, first buffered by symbol. */
	'::' {token( Token.TK_NameSep );};
	'==' {token( Token.TK_EqualsEquals );};
	'!=' {token( Token.TK_NotEquals );};
	'&&' {token( Token.TK_AndAnd );};
	'||' {token( Token.TK_OrOr );};
	'*=' {token( Token.TK_MultAssign );};
	'/=' {token( Token.TK_DivAssign );};
	'%=' {token( Token.TK_PercentAssign );};
	'+=' {token( Token.TK_PlusAssign );};
	'-=' {token( Token.TK_MinusAssign );};
	'&=' {token( Token.TK_AmpAssign );};
	'^=' {token( Token.TK_CaretAssign );};
	'|=' {token( Token.TK_BarAssign );};
	'++' {token( Token.TK_PlusPlus );};
	'--' {token( Token.TK_MinusMinus );};
	'->' {token( Token.TK_Arrow );};
	'->*' {token( Token.TK_ArrowStar );};
	'.*' {token( Token.TK_DotStar );};
    '\n' {line++;col=1;};
	# Three char compounds, first item already buffered. */
	'...' {token( Token.TK_DotDotDot );};

	# Single char symbols.
	( punct - [_"'] ) {token( Token.TK_SingleChar );};

    ( '///' space* 'ADAPTER_EXPORT' ){token(Token.TK_AdapterExport);};

	# Comments and whitespace.
    'BEGIN' any* :>> 'END'{token(Token.TK_BeginEnd);};
    'TOP_BEGIN' any* :>> 'TOP_END'{token(Token.TK_TOP);};
    'BOTTOM_BEGIN' any* :>> 'BOTTOM_END'{token(Token.TK_BOTTOM);};
	 '/*' any* :>> '*/'{token(Token.TK_Comment);};
	('//' [^\n]* '\n')--('ADAPTER_EXPORT'){token(Token.TK_Comment);};
    ('#' [^\n]* '\n'){token(Token.TK_Comment);};
    
	( any - (33..126)-'\n'  )+{token(Token.TK_Whitespace);};

	*|;
}%%
      
        //TagData _td;
        LinkedList<TokenData> _list = new LinkedList<TokenData>();
        //LinkedListNode<TokenData> _target = null;

        int cs =0;
	    public void init()
	    {
            %% write init;
            
	    }

	    public override void exec(char[] data, int len )
	    {
            //current_package = pkg;
            _data = data;
		    p = 0;
		    pe = len;
		    int eof = len;
            line = 1;
            col = 0;
		    //string _s;
		    %% write exec;
	    }

       
public void resolve()
{
    //DoResolveInheritance();
}
        public LinkedList<TokenData> list() { return _list; }

	    public LinkedListNode<TokenData> finish( )
        {
            _list.AddLast(new TokenData{ TokenType = Token.TK_EOF });
            return _list.First;
	    }       
    }
}
