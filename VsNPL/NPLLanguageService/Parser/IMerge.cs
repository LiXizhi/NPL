namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// Classes implementing this interface must supply a
    /// method that merges two location objects to return
    /// a new object of the same type.
    /// MPPG-generated parsers have the default location
    /// action equivalent to "@$ = @1.Merge(@N);" where N
    /// is the right-hand-side length of the production.
    /// </summary>
    /// <typeparam name="YYLTYPE"></typeparam>
    public interface IMerge<YYLTYPE>
    {
		/// <summary>
		/// Merges the specified last.
		/// </summary>
		/// <param name="last">The last.</param>
		/// <returns></returns>
        YYLTYPE Merge(YYLTYPE last);
    }
}
