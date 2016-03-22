/***************************************************************************
Title: Scanner
Author: LiXizhi@yeah.net
Date: 2016/3/21
Desc: full-text scanner for NPL,lua and mixed NPL and html page. 
***************************************************************************/

using Microsoft.VisualStudio.Package;
using ParaEngine.Tools.Lua.Parser;
using System.Text.RegularExpressions;
using System;
using ParaEngine.Tools.Lua.Lexer;

namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// LineScanner wraps the GPLEX scanner to provide the IScanner interface
    /// required by the Managed Package Framework. This includes mapping tokens
    /// to color definitions.
    /// </summary>
    public class LuaScanner : ScanBase
    {
        readonly Lexer.Scanner lex;
        Regex reg_begin_npl_code = new Regex(@"<[\%\?]n?p?l?=?");
        Regex reg_end_npl_code = new Regex(@"[\%\?]>\r?\n?");

        string[] m_lines;
        int m_nCurrentLine = 0;
        
        PageLexerState page_state = PageLexerState.PageState_Uknown;
        int npl_lex_state = 0;
        const int MAX_CHARS = 99999999;
        bool m_bSuppressError = false;
        public bool IsSuppressError
        {
            get { return m_bSuppressError; }
            set { m_bSuppressError = value; }
        }

        public IErrorHandler Handler
        {
            get { return lex.Handler; }
            set { lex.Handler = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LineScanner"/> class.
        /// </summary>
        public LuaScanner()
        {
            this.lex = new ParaEngine.Tools.Lua.Lexer.Scanner();
            yylval = lex.yylval;
            yylloc = lex.yylloc;
            Reset();
        }

        public void Reset()
        {
            page_state = PageLexerState.PageState_Uknown;
            npl_lex_state = 0;
            m_nCurrentLine = 0;
            m_lines = null;
            m_bSuppressError = false;
        }

        #region private helpers
        protected override int CurrentSc{
            get { return lex.EolState; }
            set { lex.EolState = value; }
        }

        protected void SetOutputToken()
        {
            yylval = lex.yylval;
            yylloc = lex.yylloc;
        }

        protected void SetOutputToken(int start, int end)
        {
            yylval = lex.yylval;
            yylloc = lex.yylloc;
            if (yylloc != null)
            {
                yylloc.sCol = start;
                yylloc.eCol = end;
                yylloc.sLin = m_nCurrentLine;
                yylloc.eLin = m_nCurrentLine;
            }
        }

        /// <summary>
        /// return the last token read
        /// </summary>
        /// <param name="nPos"></param>
        /// <returns></returns>
        private int ReadTo(int nPos = -1)
        {
            int c = 0;
            int nCount = 0;
            int start, end;
            m_bSuppressError = true;
            while ((nPos < 0 || lex.buffer.ReadPos < nPos) && c != ScanBuff.EOF)
            {
                c = lex.GetNext(ref npl_lex_state, out start, out end);
                nCount++;
            }
            m_bSuppressError = false;
            return nCount > 0 ? c : ScanBuff.EOF;
        }

        private int GetChar()
        {
            string s = lex.buffer.GetString(lex.buffer.ReadPos, lex.buffer.ReadPos + 1);
            return (s.Length == 0) ? '\n' : s[0];
        }


        private PageLexerState TrySetInitialPageState(PageLexerState page_state, string source)
        {
            if (page_state == PageLexerState.PageState_Uknown)
            {
                // this is the beginning of file
                page_state = PageLexerState.PageState_NPL;
                if(source.Length > 0 && source[0] == '<')
                {
                    // if file begins with `<`, it is regarded as a .page file with mixed HTML and NPL code.
                    page_state = PageLexerState.PageState_HTML;
                }
            }
            return page_state;
        }
        
        #endregion

        /// <summary>
        /// Set the complete source.
        /// </summary>
        /// <param name="source">The source: per line. </param>
        /// <param name="offset">The offset.</param>
        public void SetSource(string source, int offset)
        {
            Reset();

            page_state = TrySetInitialPageState(page_state, source);
            if (page_state == PageLexerState.PageState_NPL)
            {
                lex.SetSource(source, offset);
                SetOutputToken();
                return;
            }
            else
            {
                bool bDisableWebPageErrors = true;
                if(bDisableWebPageErrors)
                {
                    // this is naive way of skipping errors and parse HTML as NPL. 
                    page_state = PageLexerState.PageState_NPL;
                    m_bSuppressError = true;
                    lex.SetSource(source, offset);
                    SetOutputToken();
                }
                else
                {
                    // TODO: this is advanced way: but right now, I can not get the lexer to work 
                    // without modifying the auto-generated source code. 
                    // Perhaps, the only way is to pre-process all HTML to white space; otherwise lex Scanner source must be modified. 

                    // for mixed HTML/npl code, we will split into lines and parse line by line
                    m_lines = Regex.Split(source, "\r?\n");
                    m_nCurrentLine = 0;
                    NextLine();
                }
            }
        }

        /// <summary>
        /// replace html code with space. 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public string PreprocessPageSource(string source)
        {
            return source;
        }

        protected void NextLine()
        {
            if(m_nCurrentLine < m_lines.Length)
            {
                SetLineSource(m_lines[m_nCurrentLine], 0);
            }
            m_nCurrentLine++;
        }

        protected void SetLineSource(string source, int offset)
        {
            lex.SetSource(source+"\n", offset);
            SetOutputToken();
        }

        protected bool CheckEndOfText()
        {
            return (m_nCurrentLine > m_lines.Length);
        }

        public override int yylex()
        {
            int token = (int)Tokens.EOF;
            
            if (page_state == PageLexerState.PageState_NPL)
            {
                token = this.lex.yylex();
                SetOutputToken();
            }
            else
            {
                int start = lex.buffer.ReadPos;
                int end = 0;

                if (CheckEndOfText())
                    return this.lex.yylex();

                if (page_state == PageLexerState.PageState_HTML)
                {
                    // in HTML mode, look for "<[%%%?]n?p?l?=?"
                    string sAllText = lex.buffer.GetString(lex.buffer.ReadPos, MAX_CHARS);
                    if (sAllText.Length > 0)
                    {
                        Match m = reg_begin_npl_code.Match(sAllText);
                        if (m.Success)
                        {
                            end = start + m.Index + m.Length - 1;
                            page_state = PageLexerState.PageState_NPL_IN_HTML;
                            token = ReadTo(end + 1);
                        }
                        else
                        {
                            token = ReadTo(-1);
                        }
                    }
                    
                    if (token == (int)Tokens.EOF)
                        NextLine();
                    
                    return yylex();
                }
                else if (page_state == PageLexerState.PageState_NPL_IN_HTML)
                {
                    // in embedded NPL mode, look for "[%%%?]>\r?\n?"
                    bool bRestart = false;
                    string sAllText = lex.buffer.GetString(lex.buffer.ReadPos, MAX_CHARS);
                    Match m = reg_end_npl_code.Match(sAllText);
                    if (m.Success)
                    {
                        if (m.Index == 0)
                        {
                            end = start + m.Index + m.Length - 1;
                            token = ReadTo(end+1);
                            page_state = PageLexerState.PageState_HTML;
                            bRestart = true;
                        }
                        else
                        {
                            int nEndTagStart = start + m.Index;
                            token = lex.GetNext(ref npl_lex_state, out start, out end);
                            if (start >= nEndTagStart)
                            {
                                start = nEndTagStart;
                                end = nEndTagStart + m.Length - 1;
                                token = ReadTo(end+1);
                                page_state = PageLexerState.PageState_HTML;
                                bRestart = true;
                            }
                        }
                    }
                    else
                    {
                        token = sAllText.Length == 0 ? (int)Tokens.EOF : lex.GetNext(ref npl_lex_state, out start, out end);
                        if (token == (int)Tokens.EOF)
                        {
                            NextLine();
                            bRestart = true;
                        }
                    }
                    if (bRestart)
                    {
                        return yylex();
                    }
                    SetOutputToken(start, end);
                }
            }
            return token;
        }

        /// <summary>
		/// Yyerrors the specified format.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="args">The args.</param>
        public override void yyerror(string format, params object[] args)
        {
            if (page_state != PageLexerState.PageState_HTML && !m_bSuppressError)
                lex.yyerror(format, args);
        }
    }
}