namespace ParaEngine.Tools.Lua.Parser
{
    // Abstract base class for MPLEX scanners
    public abstract class ScanBase : AScanner<LexValue, LexLocation>
    {
        protected abstract int CurrentSc { get; set; }
        //
        // Override the virtual EolState property if the scanner state is more
        // complicated then a simple copy of the current start state ordinal
        //
        public virtual int EolState { get { return CurrentSc; } set { CurrentSc = value; } }
    }


    /// <summary>
    /// Abstract scanner class that MPPG expects its scanners to extend.
    /// </summary>
    /// <typeparam name="YYSTYPE"></typeparam>
    /// <typeparam name="YYLTYPE"></typeparam>
    public abstract class AScanner<YYSTYPE,YYLTYPE> 
        where YYSTYPE : struct
        where YYLTYPE : IMerge<YYLTYPE>
    {
        public YYSTYPE yylval;              // lexical value: set by scanner
        public YYLTYPE yylloc;              // location value: set by scanner

		/// <summary>
		/// Yylexes this instance.
		/// </summary>
		/// <returns></returns>
        public abstract int yylex();

		/// <summary>
		/// Yyerrors the specified format.
		/// </summary>
		/// <param name="format">The format.</param>
		/// <param name="args">The args.</param>
        public virtual void yyerror(string format, params object[] args) {}
    }
}