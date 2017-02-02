%namespace ParaEngine.Tools.Lua.Parser
%using System.Diagnostics
%using Microsoft.VisualStudio.TextManager.Interop
%using ParaEngine.Tools.Lua.AST
%YYSTYPE LexValue
%partial

%union 
{
	public Node node;
    public string str;
    public int number;
    
    public override string ToString()
    {
		if (!String.IsNullOrEmpty(str))
			return str;
		if (node != null)
			return node.ToString();

		return number.ToString();			
    }
}

%{
    private ErrorHandler handler = null;
    public void SetHandler(ErrorHandler handler) { this.handler = handler; }
%}

%token <str> KWAND KWBREAK KWDO KWEND KWELSE KWELSEIF KWFOR KWFALSE KWFUNCTION KWIF
%token <str> KWIN KWLOCAL KWNOT KWNIL KWOR KWREPEAT KWRETURN KWTHEN KWTRUE KWUNTIL KWWHILE KWDEF

%token LPAREN RPAREN PLUSLBRACE LBRACE RBRACE LBRACKET RBRACKET SEMICOLON COMMA DOTDOT

%token <str> EQUAL PLUS MINUS ASTERISK SLASH PERCENT CARET POUND ELLIPSIS
%token <str> EQ NEQ GT GTE LT LTE

%token <str> IDENTIFIER NUMBER STRING DOT COLON
%type <str> UnaryOperator BinaryOperator

%type <str> String
%type <str> FunctionName DottedName
%type <node> Block
%type <node> StatementList Statement LastStatement 
%type <node> VariableList Variable 
%type <node> ExpressionList23 ExpressionList Expression
%type <node> Function FunctionCall PrefixExpression Arguments
%type <node> TableConstructor
%type <node> FieldList Field
%type <node> IdentifierList
%type <node> ParameterList
%type <node> ThenBlock ElseIfBlockList
%type <node> TokenList Token RawTokenList RawToken DefParameterList

%left  ASTERISK SLASH PERCENT
%left  PLUS MINUS
%left  GT GTE LT LTE EQ NEQ
%right DOTDOT
%right KWNOT POUND
%right CARET

%token maxParseToken 
%token LEX_WHITE LEX_COMMENT LEX_ERROR

%%

/* See http://lua-users.org/wiki/LuaGrammar */

Chunk
    : Block
    {
		Chunk = new Chunk(@$) { Block = (Block)$1 };
    }
    ;
    
Semicolon
	: SEMICOLON
	| /* empty */
	;

Block 
	: StatementList
	{
		$$ = new Block(@$) { StatementList = $1 };
	}
	| StatementList LastStatement Semicolon
	{
		@$ = Merge(@1, @2);
		$$ = new Block(@$) { StatementList = AppendNode($1, $2) };
	}
	;
	
StatementList
	: /* empty */
	| StatementList Statement Semicolon
	{
		@$ = Merge(@1, @2);
		$$ = AppendNode($1, $2);
	}
	;
	
Statement
	: VariableList EQUAL ExpressionList
	{
		$$ = new Assignment(@$) { VariableList = $1, ExpressionList = $3 };
	}
	| FunctionCall
	{
		$$ = $1;
	}
	| FunctionCall LBRACE Block RBRACE
	{
		$$ = new FunctionExpression(@$) { FunctionCall = (FunctionCall)$1, Block = (Block)$3 };

		Region(@2, @4);
	}
	| KWIF Expression KWTHEN ThenBlock KWEND
	{
		$$ = new If(@$) { Expression = $2, ThenBlock = (ThenBlock)$4 };
	}
	| KWDO Block KWEND
	{
		$$ = new ExplicitBlock(@$) { Block = (Block)$2 };
		
		Region(@2, @3);
	}
	| KWWHILE Expression KWDO Block KWEND
	{
		$$ = new WhileLoop(@$) { Expression = $2, Block = (Block)$4 };
	}
	| KWREPEAT Block KWUNTIL Expression
	{
		$$ = new RepeatUntilLoop(@$) { Expression = $4, Block = (Block)$2 };
	}
	| KWFOR IDENTIFIER EQUAL ExpressionList23 KWDO Block KWEND
	{
		$$ = new ForLoop(@$) { Identifier = new Identifier(@2) { Name = $2 }, Expression = $4, Block = (Block)$6 };
	}
	| KWFOR IdentifierList KWIN ExpressionList KWDO Block KWEND
	{
		$$ = new ForLoop(@$) { IdentifierList = (Identifier)$2, Expression = $4, Block = (Block)$6 };
	}
	| KWFUNCTION FunctionName ParameterList Block KWEND
	{
		$$ = new FunctionDeclaration(@$) { Name = $2, ParameterList = $3 as ParameterList, Body = (Block)$4 };

		Region(@3, @5);
	}
	| KWLOCAL KWFUNCTION FunctionName ParameterList Block KWEND
	{
		$$ = new FunctionDeclaration(@$) { Name = $3, ParameterList = $4 as ParameterList, Body = (Block)$5, IsLocal = true };
		
		Region(@4, @6);
	}
	| KWLOCAL IdentifierList
	{
		$$ = new LocalDeclaration(@$) { IdentifierList = (Identifier)$2 };
	}
	| KWLOCAL IdentifierList EQUAL ExpressionList
	{
		$$ = new Assignment(@$) { VariableList = $2, ExpressionList = $4, IsLocal = true };
	}
	| KWDEF DefParameterList LBRACE TokenList RBRACE
	{
		$$ = new DefBlock(@$){ TokenList = new Node(@4) };

		Region(@3, @5);
	}
	| error
	{
	}
	;

DefParameterList
	: LPAREN String RPAREN
	{
		Match(@1, @3);
	}
	| LPAREN String error { Error(@1, "Unmatched parentheses."); }
	| LPAREN String COMMA IdentifierList RPAREN
	{
		$$ = new ParameterList(@$) { IdentifierList = (Identifier)$4 };
		
		Match(@1, @5);
	}
	| LPAREN String COMMA IdentifierList error { Error(@1, "Unmatched parentheses."); }
	| LPAREN String COMMA ELLIPSIS RPAREN
	{
		Match(@1, @5);
	}
	| LPAREN String COMMA ELLIPSIS error { Error(@1, "Unmatched parentheses."); }
	;

TokenList
	: /* empty */
	| TokenList Token
	{
		$$ = AppendNode($1, $2);
	}
	;

Token
	: KWAND | KWBREAK | KWDO | KWEND | KWELSE | KWELSEIF | KWFOR | KWFALSE | KWFUNCTION | KWIF
	  | KWIN | KWLOCAL | KWNOT | KWNIL | KWOR | KWREPEAT | KWRETURN | KWTHEN | KWTRUE | KWUNTIL 
	  | KWWHILE | KWDEF | LPAREN | RPAREN | LBRACKET | RBRACKET | SEMICOLON 
	  | COMMA | DOTDOT | EQUAL | PLUS | MINUS | ASTERISK | SLASH | PERCENT | CARET | POUND 
	  | ELLIPSIS | EQ | NEQ | GT | GTE | LT | LTE | IDENTIFIER | NUMBER | STRING | DOT | COLON
	{ $$ = new Node(@$); }
	| PLUSLBRACE RawTokenList RBRACE
	{ $$ = new Node(@2); }
	| TableConstructor
	{ $$ = new Node(@$); }
	;

RawTokenList
	: /* empty */
	| RawTokenList RawToken
	{
		$$ = AppendNode($1, $2);
	}
	;

RawToken
	: KWAND | KWBREAK | KWDO | KWEND | KWELSE | KWELSEIF | KWFOR | KWFALSE | KWFUNCTION | KWIF
	  | KWIN | KWLOCAL | KWNOT | KWNIL | KWOR | KWREPEAT | KWRETURN | KWTHEN | KWTRUE | KWUNTIL 
	  | KWWHILE | KWDEF | LPAREN | RPAREN | LBRACKET | RBRACKET | SEMICOLON 
	  | COMMA | DOTDOT | EQUAL | PLUS | MINUS | ASTERISK | SLASH | PERCENT | CARET | POUND 
	  | ELLIPSIS | EQ | NEQ | GT | GTE | LT | LTE | IDENTIFIER | NUMBER | STRING | DOT | COLON
	{ $$ = new Node(@1); }
	| TableConstructor
	{ $$ = new Node(@$); }
	;

ThenBlock
	: Block ElseIfBlockList
	{
		$$ = new ThenBlock(Merge(@1, @2)) { Block = (Block)$1, ElseIfBlockList = (ElseIfBlock)$2 };
	}
	| Block ElseIfBlockList KWELSE Block
	{
		$$ = new ThenBlock(Merge(@1, @4)) { Block = (Block)$1, ElseIfBlockList = (ElseIfBlock)$2, ElseBlock = (Block)$4 };
	}
	;
	
ElseIfBlockList
	: /* empty */
	| ElseIfBlockList KWELSEIF Expression KWTHEN Block
	{
		$$ = AppendNode($1, new ElseIfBlock(Merge(@2, @5)) { Expression = $3, Block = (Block)$5 });
	}
	;

LastStatement
	: KWBREAK
	{
		$$ = new Break(@$);
	}
	| KWRETURN
	{
		$$ = new Return(@$);
	}
	| KWRETURN ExpressionList
	{
		$$ = new Return(@$) { ExpressionList = $2 };
	}
	;
	
FunctionName
	: DottedName { $$ = $1; }
	| DottedName COLON IDENTIFIER 
	{ 
		$$ = $1 + ':' + $3; 
		
		QualifyName(@2, @3, $3);
	}
	| DottedName COLON error
	{
	}
	;
	
DottedName
	: IDENTIFIER 
	{
		StartName(@1, $1);
		
		$$ = $1; 
	}
	| DottedName DOT IDENTIFIER 
	{ 
		$$ = $1 + '.' + $3; 
		
		QualifyName(@2, @3, $3);
	}
	| DottedName DOT error
	{
		$$ = $1;
	}
	;
	
IdentifierList
	: IDENTIFIER
	{
		StartName(@1, $1);
		
		$$ = new Identifier(@$) { Name = $1 };
	}
	| IdentifierList COMMA IDENTIFIER
	{
		StartName(@3, $3);
		
		$$ = AppendNode($1, new Identifier(@3) { Name = $3 });
	}
	;

ExpressionList
	: Expression
	{
		$$ = $1;
	}
	| ExpressionList COMMA { Parameter(@2); } Expression
	{
		$$ = AppendNode($1, $4);		
	}
	;
	
ExpressionList23
	: Expression COMMA Expression
	{
		$$ = AppendNode($1, $3);
	}
	| Expression COMMA Expression COMMA Expression
	{
		$$ = AppendNodes($1, $3, $5);
	}
	;
	
UnaryOperator
	: KWNOT
	| POUND
	| MINUS
	;
	 
BinaryOperator
	: KWOR | KWAND
	| GT | GTE | LT | LTE | EQ | NEQ | DOTDOT 
	| PLUS | MINUS | ASTERISK | SLASH | PERCENT | CARET
	;

String
	: STRING
	{
		$$ = $1;
	}
	| String STRING
	{
		$$ = String.Concat($1, $2);
	}
	;
	
Expression
	: KWNIL
	{
		$$ = new Literal(@$) { Type = LuaType.Nil, Value = $1 };
	}
	| KWTRUE 
	{
		$$ = new Literal(@$) { Type = LuaType.Boolean, Value = $1 };
	}
	| KWFALSE 
	{
		$$ = new Literal(@$) { Type = LuaType.Boolean, Value = $1 };
	}
	| NUMBER 
	{
		$$ = new Literal(@$) { Type = LuaType.Number, Value = $1 };
	}
	| String
	{
		$$ = new Literal(@$) { Type = LuaType.String, Value = $1 };
	}
	| ELLIPSIS 
	{
	}
	| Function
	{
		$$ = $1;
	}
	| PrefixExpression
	{
		$$ = $1;
	}
	| TableConstructor
	{
		$$ = $1;
	}
	| UnaryOperator Expression
	{
		$$ = new UnaryExpression(@$) { Operator = $1, Expression = $2 };
	}
	| Expression BinaryOperator Expression
	{
		$$ = new BinaryExpression(@$) { Operator = $2, LeftExpression = $1, RightExpression = $3 };
	}
	;
	
VariableList
	: Variable
	{
		$$ = $1;
	}
	| VariableList COMMA Variable
	{
		$$ = AppendNode($1, $3);
	}
	;
	
Variable
	: IDENTIFIER
	{
		StartName(@1, $1);
		
		$$ = new Identifier(@$) { Name = $1 };
	}
	| PrefixExpression LBRACKET Expression RBRACKET
	{
		Match(@2, @4);
		
		$$ = new Variable(@$) { PrefixExpression = $1, Expression = $3 };
	}
	| PrefixExpression DOT IDENTIFIER
	{
		// If the PrefixExpression is an Identifier, we can just append the scoped name
		if ($1 is Identifier)
		{
			Identifier identifier = (Identifier)$1;
			identifier.Name = identifier.Name + '.' + $3;
			$$ = identifier;
			
			QualifyName(@2, @3, $3);
		}
		else
		{
			$$ = new Variable(@$) { PrefixExpression = $1, Identifier = new Identifier(@3) { Name = $3 } };
		}
	}
	| PrefixExpression DOT error
	{
		// TODO: Should we return something?
	}
	;
	
PrefixExpression
	: Variable
	{
		$$ = $1;
	}
	| FunctionCall
	{
		$$ = $1;
	}
	| LPAREN Expression RPAREN
	{
		Match(@1, @3);
		
		$$ = $2;
	}
	;
	
FunctionCall
	: PrefixExpression Arguments
	{
		$$ = new FunctionCall(@$) { PrefixExpression = $1, Arguments = $2 };
	}
	| PrefixExpression COLON IDENTIFIER Arguments
	{
		QualifyName(@2, @3, $3);
		
		$$ = new FunctionCall(@$) { PrefixExpression = $1, Identifier = new Identifier(@3) { Name = $3 }, Arguments = $4 };
	}
	;
	
Arguments
	: StartArg error
	{
	}
	| StartArg EndArg
	{
		Match(@1, @2);
	}
	| StartArg ExpressionList EndArg
	{
		$$ = $2;
		
		Match(@1, @3);
	}
	| StartArg ExpressionList error
	{
		EndParameters(@3);
		
		Error(@3, "Unmatched parentheses.");
	}
	| TableConstructor
	{
		$$ = $1;
	}
	| STRING
	{
		$$ = new Literal(@$) { Type = LuaType.String, Value = $1 };
	}
	;

StartArg
	: LPAREN
	{
		StartParameters(@1);
	}
	;
	
EndArg
	: RPAREN
	{
		EndParameters(@1);
	}
	;
		
Function
	: KWFUNCTION ParameterList Block KWEND
	{
		$$ = new Function(@$) { ParameterList = $2 as ParameterList, Body = (Block)$3 };
		
		Region(@2, @4);
	}
	;
	
ParameterList
	: LPAREN RPAREN
	{
		Match(@1, @2);
	}
	| LPAREN error { Error(@1, "Unmatched parentheses."); }
	| LPAREN IdentifierList RPAREN
	{
		$$ = new ParameterList(@$) { IdentifierList = (Identifier)$2 };
		
		Match(@1, @3);
	}
	| LPAREN IdentifierList error { Error(@1, "Unmatched parentheses."); }
	| LPAREN ELLIPSIS RPAREN
	{
		Match(@1, @3);
	}
	| LPAREN ELLIPSIS error { Error(@1, "Unmatched parentheses."); }
	| LPAREN IdentifierList COMMA ELLIPSIS RPAREN
    {
		$$ = new ParameterList(@$) { IdentifierList = (Identifier)$2 };
		
		Match(@1, @5);
	}
	| LPAREN IdentifierList COMMA ELLIPSIS error { Error(@1, "Unmatched parentheses."); }
	;
	
FieldSeparator
	: COMMA
	| SEMICOLON
	;
	
TableConstructor
	: LBRACE RBRACE
	{
		$$ = new TableConstructor(@$);
		
		Match(@1, @2);
	}
	| LBRACE FieldList RBRACE
	{
		$$ = new TableConstructor(@$) { FieldList = (Field)$2 };
		
		Match(@1, @3);
		Region(@1, @3);
	}
	| LBRACE FieldList FieldSeparator RBRACE
	{
		$$ = new TableConstructor(@$) { FieldList = (Field)$2 };
		
		Match(@1, @4);
		Region(@1, @4);
	}
	;
	
FieldList
	: Field
	{
		$$ = $1;
	}
	| FieldList FieldSeparator Field
	{
		$$ = AppendNode($1, $3);
	}
	;
	
Field
	: Expression
	{
		$$ = new Field(@$) { Expression  = $1 };
	}
	| IDENTIFIER EQUAL Expression
	{
		$$ = new Field(@$) { Identifier = new Identifier(@1) { Name = $1 }, Expression = $3 };		
	}
	| LBRACKET Expression RBRACKET EQUAL Expression
	{
		$$ = new Field(@$) { LeftExpression = $2, Expression = $5 };		
		
		Match(@1, @3);
	}
	;
%%
