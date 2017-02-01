using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Lexer;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua
{
    internal class NPLFormatHelper
    {
        // token struct for formatting
        struct FormatToken
        {
            public int token;
            public int startIndex;
            public int endIndex;

            public FormatToken(int token, int start, int end)
            {
                this.token = token;
                this.startIndex = start;
                this.endIndex = end;
            }
        }
        
        /// <summary>
        /// @see also: https://github.com/Trass3r/AsmHighlighter/blob/master/AsmHighlighter/AsmHighlighterFormatHelper.cs
        /// @see also: https://github.com/samizzo/nshader/blob/master/NShaderVS/NShaderFormatHelper.cs
        /// https://msdn.microsoft.com/en-us/library/bb164633.aspx
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <param name="span"></param>
        /// <param name="tabSize"></param>
        /// <returns></returns>
        internal static List<EditSpan> ReformatCode(IVsTextLines pBuffer, int[] indents, bool[] comments, bool[] longStrings, TextSpan span)
        {
            Scanner lex = new Scanner();
            List<EditSpan> changeList = new List<EditSpan>();
            string line = "";
            for (int i = span.iStartLine; i <= span.iEndLine; ++i)
            {
                if (longStrings[i])
                    continue;

                int startIndex = 0;
                int endIndex = 0;
                pBuffer.GetLengthOfLine(i, out endIndex);
                pBuffer.GetLineText(i, startIndex, i, endIndex, out line);

                //rules of formatting
                //rule 1: insert space before and after binary operator if there not any
                //rule 2: insert space after comma, semicolon if there not any
                //rule 3: indentation increase inside block
                //rule 4: multiple spaces replaced by a single space
                //rule 5: no spaces after left parentheses("(") and before right parentheses(")")
                //rule 6: no spaces between identifier and left parentheses("(")
                //rule 7: no spaces before and after colon ":"
                int state = 0, start = 0, end = 0;
                int firstSpaceEnd = -1;
                lex.SetSource(line, 0);

                int token = lex.GetNext(ref state, out start, out end);
                if ((Tokens)token == Tokens.LEX_WHITE)   // skip spaces at front of the line
                {
                    firstSpaceEnd = end;
                    token = lex.GetNext(ref state, out start, out end);
                }
                
                // set indentation
                if( !(firstSpaceEnd == -1 && indents[i] == 0))
                {
                    string indentation = "";
                    for (int j = 0; j < indents[i]; ++j)
                        indentation = "\t" + indentation;
                    TextSpan firstSpaceSpan = new TextSpan();
                    firstSpaceSpan.iStartLine = i;
                    firstSpaceSpan.iEndLine = i;
                    firstSpaceSpan.iStartIndex = 0;
                    firstSpaceSpan.iEndIndex = firstSpaceEnd + 1;
                    changeList.Add(new EditSpan(firstSpaceSpan, indentation));
                }

                if (comments[i])
                    continue;

                FormatToken currentToken = new FormatToken((int)token, start, end);
                FormatToken lastToken = new FormatToken((int)Tokens.EOF, start - 1, start - 1);
                while (currentToken.token != (int)Tokens.EOF)
                {
                    token = lex.GetNext(ref state, out start, out end);
                    // fix issue of last unknow space
                    if (start > end) break;
                    FormatToken nextToken = new FormatToken((int)token, start, end);

                    if (currentToken.token == (int)Tokens.LEX_WHITE)    // spaces
                    {
                        string SpaceorEmpty = " ";
                        if (nextToken.token == (int)Tokens.RPAREN ||        // if meet right paren, remove spaces
                            (nextToken.token == (int)Tokens.LPAREN &&    // rule 6
                            lastToken.token != (int)Tokens.KWFUNCTION) ||
                            nextToken.token == (int)Tokens.LBRACKET ||
                            nextToken.token == (int)Tokens.COLON ||
                            lastToken.token == (int)Tokens.COLON)    
                            SpaceorEmpty = "";
                        TextSpan spaceEdit = new TextSpan();
                        spaceEdit.iStartLine = i;
                        spaceEdit.iEndLine = i;
                        spaceEdit.iStartIndex = currentToken.startIndex;
                        spaceEdit.iEndIndex = currentToken.endIndex + 1;
                        changeList.Add(new EditSpan(spaceEdit, SpaceorEmpty));
                    }
                    else if (currentToken.token == (int)Tokens.COMMA ||
                        currentToken.token == (int)Tokens.SEMICOLON)    // comma, semicolon
                    {
                        if (nextToken.token != (int)Tokens.LEX_WHITE &&
                            nextToken.token != (int)Tokens.EOF)
                        {
                            string space = " ";
                            TextSpan spaceEdit = new TextSpan();
                            spaceEdit.iStartLine = i;
                            spaceEdit.iEndLine = i;
                            spaceEdit.iStartIndex = currentToken.endIndex + 1;
                            spaceEdit.iEndIndex = currentToken.endIndex + 1;
                            changeList.Add(new EditSpan(spaceEdit, space));
                        }    
                    }
                    else if(currentToken.token == (int)Tokens.MINUS ||  // binary operators
                        currentToken.token == (int)Tokens.PLUS ||
                        currentToken.token == (int)Tokens.ASTERISK ||
                        currentToken.token == (int)Tokens.SLASH ||
                        currentToken.token == (int)Tokens.EQUAL)
                    {
                        if(lastToken.token != (int)Tokens.LEX_WHITE && 
                            lastToken.token != (int)Tokens.EOF)
                        {
                            string space = " ";
                            TextSpan spaceEdit = new TextSpan();
                            spaceEdit.iStartLine = i;
                            spaceEdit.iEndLine = i;
                            spaceEdit.iStartIndex = currentToken.startIndex;
                            spaceEdit.iEndIndex = currentToken.startIndex;
                            changeList.Add(new EditSpan(spaceEdit, space));
                        }

                        if(nextToken.token != (int)Tokens.LEX_WHITE &&
                            nextToken.token != (int)Tokens.EOF)
                        {
                            string space = " ";
                            TextSpan spaceEdit = new TextSpan();
                            spaceEdit.iStartLine = i;
                            spaceEdit.iEndLine = i;
                            spaceEdit.iStartIndex = currentToken.endIndex + 1;
                            spaceEdit.iEndIndex = currentToken.endIndex + 1;
                            changeList.Add(new EditSpan(spaceEdit, space));
                        }
                    }
                    else if(currentToken.token == (int)Tokens.LPAREN && 
                        nextToken.token == (int)Tokens.LEX_WHITE)
                    {
                        string empty = "";
                        TextSpan emptyEdit = new TextSpan();
                        emptyEdit.iStartLine = i;
                        emptyEdit.iEndLine = i;
                        emptyEdit.iStartIndex = nextToken.startIndex;
                        emptyEdit.iEndIndex = nextToken.endIndex + 1;
                        changeList.Add(new EditSpan(emptyEdit, empty));

                        // get new nextToken
                        token = lex.GetNext(ref state, out start, out end);
                        // fix issue of last unknow space
                        if (start > end) break;
                        nextToken = new FormatToken(token, start, end);
                    }
   
                    lastToken = currentToken;
                    currentToken = nextToken;        
                }
            }

            return changeList;
        }
    }
}