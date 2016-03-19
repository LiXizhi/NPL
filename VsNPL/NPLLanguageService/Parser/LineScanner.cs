/***************************************************************************
Title: Scanner
Author: LiXizhi@yeah.net
Date: 2016/3/18
Desc: syntax highlighting for NPL,lua and mixed NPL and html page. 
***************************************************************************/

using Microsoft.VisualStudio.Package;
using ParaEngine.Tools.Lua.Parser;
using System.Text.RegularExpressions;

namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// What kind of source code are we scanning
    /// </summary>
    public enum PageLexerState
    {
        // at the beginning of each file, the state is 0; then it must be in either of following states.
        PageState_Uknown = 0,
        PageState_NPL = 1,
        PageState_HTML = 2,
        PageState_NPL_IN_HTML = 3,
    }

    public enum NPLTokens
    {
        LEX_COMMENT_LIGHT = 256,
        LEX_NPL_BEGINCODE,
        LEX_NPL_ENDCODE,
        LEX_NPL_HTML_ATTR_VALUE
    }

    /// <summary>
    /// LineScanner wraps the GPLEX scanner to provide the IScanner interface
    /// required by the Managed Package Framework. This includes mapping tokens
    /// to color definitions.
    /// </summary>
    public class LineScanner : IScanner
    {
    	readonly Lexer.Scanner lex;
        Regex reg_begin_npl_code = new Regex(@"<[\%\?]n?p?l?=?");
        Regex reg_end_npl_code = new Regex(@"[\%\?]>\r?\n?");
        
        const int MAX_LINE_CHARS = 999;
        /// <summary>
        /// Initializes a new instance of the <see cref="LineScanner"/> class.
        /// </summary>
        public LineScanner()
        {
            this.lex = new ParaEngine.Tools.Lua.Lexer.Scanner();
        }

        #region private
        private PageLexerState GetPageState(int cacheState)
        {
            return (PageLexerState)(cacheState >> 8);
        }

        private int GetNPLLexerState(int cacheState)
        {
            return cacheState & 0x0f;
        }

        private int GetHTMLLexerState(int cacheState)
        {
            return (cacheState & 0xff) >> 4;
        }

        private int ComputeCacheState(int nplLexState, int htmlLexState, PageLexerState pageState)
        {
            return nplLexState | ((int)(htmlLexState) << 4) | ((int)(pageState) << 8);
        }
        private void FillTokenInfo(int token, TokenInfo tokenInfo, int start, int end)
        {
            Configuration.TokenDefinition definition = Configuration.GetDefinition(token);
            tokenInfo.StartIndex = start;
            tokenInfo.EndIndex = end;
            tokenInfo.Color = definition.TokenColor;
            tokenInfo.Type = definition.TokenType;
            tokenInfo.Trigger = definition.TokenTriggers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nNum">-1 to read to end</param>
        /// <returns></returns>
        private int ReadSome(int nNum = -1)
        {
            int c = 0;
            int nCount = 0;
            do
            {
                c = lex.buffer.Read();
                nCount++;
            } while ((nNum<0 || nCount < nNum) && c != Lexer.ScanBuff.EOF); // c != '\n' &&
            return nCount;
        }

        private int ReadTo(int nPos = -1)
        {
            int c = 0;
            int nCount = 0;
            while ((nPos < 0 || lex.buffer.ReadPos < nPos) && c != Lexer.ScanBuff.EOF)
            {
                c = lex.buffer.Read();
                nCount++;
            }
            return nCount;
        }

        private int GetChar()
        {
            string s = lex.buffer.GetString(lex.buffer.ReadPos, lex.buffer.ReadPos+1);
            return (s.Length == 0) ? '\n' : s[0];
        }

        #endregion


        private PageLexerState TrySetInitialPageState(PageLexerState page_state)
        {
            if (page_state == PageLexerState.PageState_Uknown)
            {
                // this is the beginning of file
                page_state = PageLexerState.PageState_NPL;
                string sText = lex.yytext;
                int nPos = lex.buffer.Pos;
                int nReadPos = lex.buffer.ReadPos;
                string sChar = lex.buffer.GetString(nReadPos, nReadPos + 1);
                string sNextChar = lex.buffer.GetString(nPos, nPos + 1);
                // int nChar = lex.buffer.Peek();
                if (sChar == "<")
                {
                    // if file begins with `<`, it is regarded as a .page file with mixed HTML and NPL code.
                    page_state = PageLexerState.PageState_HTML;
                }
            }
            return page_state;
        }

        private int HTMLLexGetNext(ref int state, out int start, out int end)
        {
            end = start = lex.buffer.ReadPos;
            int token = (int)NPLTokens.LEX_COMMENT_LIGHT;
            while (true)
            {
                int thisChar = GetChar();
                if(thisChar == '\n')
                {
                    lex.buffer.Read();
                    end = lex.buffer.ReadPos;
                    token = (int)Tokens.EOF;
                    break;
                }
                bool hasReadText = (lex.buffer.ReadPos > start);
                if (state == 0)
                {
                    // inside pure text
                    if (thisChar == '\"' || thisChar == '\'')
                    {
                        if (!hasReadText)
                        {
                            state = thisChar == '\"' ? 1 : 2;
                            token = (int)NPLTokens.LEX_NPL_HTML_ATTR_VALUE;
                        }
                        break;
                    }
                    else
                        lex.buffer.Read();
                }
                else if (state == 1 || state == 2)
                {
                    // inside `"` or `'` quotation
                    lex.buffer.Read();
                    token = (int)NPLTokens.LEX_NPL_HTML_ATTR_VALUE;
                    if ( (state == 1 && thisChar == '\"') || (state == 2 && (thisChar == '\'')))
                    {
                        state = 0;
                        break;
                    }
                }
                else
                {
                    // should never be here
                    state = 0;
                    lex.buffer.Read();
                }
            }
            if (lex.buffer.ReadPos <= start)
            { 
                lex.buffer.Read();
            }
            end = lex.buffer.ReadPos > start ? lex.buffer.ReadPos-1 : start;
            return token;
        }

        /// <summary>
        /// Scans the token and provide info about it.
        /// </summary>
        /// <param name="tokenInfo">The token info.</param>
        /// <param name="nStateCache">The state. visual studio cache it per line, so that it can begin parsing from any line 
        /// by passing the cached state of the previous line. 
        /// Initial value(0), Inside comment (1), inside string(2).
        /// </param>
        /// <returns></returns>
        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int nStateCache)
        {
            PageLexerState page_state = GetPageState(nStateCache);
            int npl_lex_state = GetNPLLexerState(nStateCache);
            int html_lex_state = GetHTMLLexerState(nStateCache);
            page_state = TrySetInitialPageState(page_state);

            int token = (int)Tokens.EOF;
            int start = lex.buffer.ReadPos;
            int end = 0;

            if (page_state == PageLexerState.PageState_NPL)
            {
                token = lex.GetNext(ref npl_lex_state, out start, out end);
            }
            else if (page_state == PageLexerState.PageState_HTML)
            {
                // in HTML mode, look for "<[%%%?]n?p?l?=?"
                string sAllText = lex.buffer.GetString(lex.buffer.ReadPos, MAX_LINE_CHARS);
                if (sAllText.Length > 0)
                {
                    Match m = reg_begin_npl_code.Match(sAllText);
                    if (m.Success)
                    {
                        if (m.Index == 0)
                        {
                            end = start + m.Index + m.Length - 1;
                            page_state = PageLexerState.PageState_NPL_IN_HTML;
                            npl_lex_state = 0;
                            token = (int)NPLTokens.LEX_NPL_BEGINCODE;
                            ReadTo(end + 1);
                        }
                        else
                        {
                            int nLastStart = start;
                            int nStartTagIndex = start + m.Index;
                            token = HTMLLexGetNext(ref html_lex_state, out start, out end);
                            if (end >= nStartTagIndex)
                            {
                                start = nLastStart;
                                end = nStartTagIndex;
                                lex.buffer.Pos = end + 1;
                                token = (int)NPLTokens.LEX_COMMENT_LIGHT;
                            }
                        }
                    }
                    else
                    {
                        token = HTMLLexGetNext(ref html_lex_state, out start, out end);
                    }
                    if (start < end && token == (int)Tokens.EOF)
                    {
                        token = (int)NPLTokens.LEX_COMMENT_LIGHT;
                    }
                }
            }
            else if (page_state == PageLexerState.PageState_NPL_IN_HTML)
            {
                // in embedded NPL mode, look for "[%%%?]>\r?\n?"
                // TODO: optimize performance to only search once per line 
                string sAllText = lex.buffer.GetString(lex.buffer.ReadPos, MAX_LINE_CHARS);
                Match m = reg_end_npl_code.Match(sAllText);
                if (m.Success)
                {
                    if(m.Index == 0)
                    {
                        end = start + m.Index + m.Length - 1;
                        ReadTo(end+1);
                        page_state = PageLexerState.PageState_HTML;
                        npl_lex_state = 0;
                        token = (int)NPLTokens.LEX_NPL_ENDCODE;
                    }
                    else
                    {
                        int nEndTagStart = start + m.Index;
                        token = lex.GetNext(ref npl_lex_state, out start, out end);
                        if(start >= nEndTagStart)
                        {
                            start = nEndTagStart;
                            end = nEndTagStart + m.Length - 1;
                            ReadTo(end+1);
                            page_state = PageLexerState.PageState_HTML;
                            npl_lex_state = 0;
                            token = (int)NPLTokens.LEX_NPL_ENDCODE;
                        }
                    }
                }
                else
                {
                    token = lex.GetNext(ref npl_lex_state, out start, out end);
                }
            }

            nStateCache = ComputeCacheState(npl_lex_state, html_lex_state, page_state);

            // !EOL and !EOF
            if (token != (int)Tokens.EOF)
            {
                FillTokenInfo(token, tokenInfo, start, end);
                return true;
            }
            return false;
        }

		/// <summary>
		/// Sets the line source.
		/// </summary>
		/// <param name="source">The source: per line. </param>
		/// <param name="offset">The offset.</param>
        public void SetSource(string source, int offset)
        {
            lex.SetSource(source, offset);
        }
    }
}