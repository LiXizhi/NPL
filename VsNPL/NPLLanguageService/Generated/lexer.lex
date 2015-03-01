%using ParaEngine.Tools.Lua.Parser;

%namespace ParaEngine.Tools.Lua.Lexer

%x BLOCKCOMMENT,DQSTRING,SQSTRING,LBSTRING

%{
	 // Variable holding the current string literal's block level
	 static int blockLevel = 0;

     int GetIdToken(string txt)
     {
        switch (txt[0])
        {
			case 'a':
				if (txt.Equals("and")) return (int)Tokens.KWAND;
				break;
			case 'b':
				if (txt.Equals("break")) return (int)Tokens.KWBREAK;
				break;
			case 'd':
				if (txt.Equals("do")) return (int)Tokens.KWDO;
				break;
			case 'e':
				if (txt.Equals("end")) return (int)Tokens.KWEND;
				else if (txt.Equals("else")) return (int)Tokens.KWELSE;
				else if (txt.Equals("elseif")) return (int)Tokens.KWELSEIF;
				break;
            case 'f':
                if (txt.Equals("for")) return (int)Tokens.KWFOR;
                else if (txt.Equals("false")) return (int)Tokens.KWFALSE;
                else if (txt.Equals("function")) return (int)Tokens.KWFUNCTION;
                break;
            case 'i':
				if (txt.Equals("if")) return (int)Tokens.KWIF;
				else if (txt.Equals("in")) return (int)Tokens.KWIN;
				break;
            case 'l':
				if (txt.Equals("local")) return (int)Tokens.KWLOCAL;
				break;
            case 'n':
				if (txt.Equals("nil")) return (int)Tokens.KWNIL;
				else if (txt.Equals("not")) return (int)Tokens.KWNOT;
				break;
			case 'o':
				if (txt.Equals("or")) return (int)Tokens.KWOR;
				break;
            case 'r':
				if (txt.Equals("repeat")) return (int)Tokens.KWREPEAT;
				else if (txt.Equals("return")) return (int)Tokens.KWRETURN;
				break;
            case 't':
				if (txt.Equals("then")) return (int)Tokens.KWTHEN;
				else if (txt.Equals("true")) return (int)Tokens.KWTRUE;
				break;
            case 'u':
				if (txt.Equals("until")) return (int)Tokens.KWUNTIL;
				break;
            case 'w':
				if (txt.Equals("while")) return (int)Tokens.KWWHILE;
				break;
            default: 
                break;
        }
        
        return (int)Tokens.IDENTIFIER;
   }
       
   internal void LoadYylval()
   {
	   // Load the token text as the string member of the 'union'
       yylval.str = tokTxt;
       
       // Initialize the current location in yylloc
       yylloc = new LexLocation(tokLin, tokCol, tokLin, tokECol);
   }

   public override void yyerror(string s, params object[] a)
   {
       if (handler != null) 
		  handler.AddError(s, tokLin, tokCol, tokLin, tokECol);
   }
%}

Anything				.*
ABOpenBrkt				[^\[=]
ABCloseBrkt				[^\]]
AllowSingleOpenBrkt		\[{0,1}
AllowSingleCloseBrkt	\]{0,1}

BlkStart		    \[=*\[
BlkEnd				\]=*\]

CmntStart			--
CmntEnd				\n

LongCmntStart		--{BlkStart}

Whitespace      [ \t\r\f\v\n]

%%

[a-zA-Z_][a-zA-Z0-9_]*    { return GetIdToken(yytext); }
[0-9]*([.][0-9])?[0-9]*   { return (int)Tokens.NUMBER; }
0[xX][0-9a-fA-F]+		  { return (int)Tokens.NUMBER; }
/* {String}				  { return (int)Tokens.STRING; } */
\+                        { return (int)Tokens.PLUS;    }
\-                        { return (int)Tokens.MINUS;    }
\*                        { return (int)Tokens.ASTERISK;    }
\/                        { return (int)Tokens.SLASH;    }
\%                        { return (int)Tokens.PERCENT;    }
\^                        { return (int)Tokens.CARET;    }
\#                        { return (int)Tokens.POUND;    }
=                         { return (int)Tokens.EQUAL;    }
==                        { return (int)Tokens.EQ;  }
~=						  { return (int)Tokens.NEQ; }
\<=                       { return (int)Tokens.LTE;    }
\>=                       { return (int)Tokens.GTE;    }
\<                        { return (int)Tokens.LT;     }
\>                        { return (int)Tokens.GT; }

\(                        { return (int)Tokens.LPAREN;    }
\)                        { return (int)Tokens.RPAREN;    }
\{                        { return (int)Tokens.LBRACE;    }
\}                        { return (int)Tokens.RBRACE;    }
\[                        { return (int)Tokens.LBRACKET;    }
\]                        { return (int)Tokens.RBRACKET;    }
;                         { return (int)Tokens.SEMICOLON;    }
:                         { return (int)Tokens.COLON;    }
,                         { return (int)Tokens.COMMA;    }
\.                        { return (int)Tokens.DOT;    }
\.\.					  { return (int)Tokens.DOTDOT; }
\.\.\.					  { return (int)Tokens.ELLIPSIS; }

/* Single-line comments */
{CmntStart}{CmntEnd}												{ return (int)Tokens.LEX_COMMENT; }
{CmntStart}{AllowSingleOpenBrkt}{ABOpenBrkt}{Anything}{CmntEnd}		{ return (int)Tokens.LEX_COMMENT; }

/* Block comments */
{LongCmntStart}											{ BEGIN(BLOCKCOMMENT); blockLevel = yytext.Length - 4; return (int)Tokens.LEX_COMMENT; }
<BLOCKCOMMENT>{AllowSingleCloseBrkt}{ABCloseBrkt}*		{ return (int)Tokens.LEX_COMMENT; }
<BLOCKCOMMENT>{BlkEnd}									{ if (blockLevel == yytext.Length - 2) { BEGIN(INITIAL); } return (int)Tokens.LEX_COMMENT; }

/* Single-quoted string literals */
'														{ BEGIN(SQSTRING); return (int)Tokens.STRING; }
<SQSTRING>[^'\\\n]*										|
<SQSTRING>\\['abfnrtv\\\n]	    						|
<SQSTRING>\\[0-9]{1,3}									{ return (int)Tokens.STRING; }
<SQSTRING>\n											|
<SQSTRING>'												{ BEGIN(INITIAL); return (int)Tokens.STRING; }
<SQSTRING>\\[^'abfnrtv\\\n0-9]{0,1}						{ yyerror("Illegal escape sequence."); return (int)Tokens.STRING; }

/* Double-quoted string literals */
\"														{ BEGIN(DQSTRING); return (int)Tokens.STRING; }
<DQSTRING>[^\"\\\n]*									|
<DQSTRING>\\[\"abfnrtv\\\n]								|
<DQSTRING>\\[0-9]{1,3}									{ return (int)Tokens.STRING; }
<DQSTRING>\n											|
<DQSTRING>\"											{ BEGIN(INITIAL); return (int)Tokens.STRING; }
<DQSTRING>\\[^\"abfnrtv\\\n0-9]{0,1}					{ yyerror("Illegal escape sequence."); return (int)Tokens.STRING; }

/* Long-bracket string literals */
{BlkStart}												{ BEGIN(LBSTRING); blockLevel = yytext.Length - 2;	return (int)Tokens.STRING; }
<LBSTRING>]{0,1}[^\]]*									{ return (int)Tokens.STRING; }
<LBSTRING>{BlkEnd}										{ if (blockLevel == yytext.Length - 2) { BEGIN(INITIAL); } 
														  return (int)Tokens.STRING; }

{Whitespace}+       { return (int)Tokens.LEX_WHITE; }
\n                  { return (int)Tokens.LEX_WHITE; }
.                   { yyerror("Illegal character.");
				      return (int)Tokens.LEX_ERROR; }

%{
						  LoadYylval();
%}

%%

/* .... */
