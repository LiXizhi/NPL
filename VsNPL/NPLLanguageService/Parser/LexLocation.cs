namespace ParaEngine.Tools.Lua.Parser
{
    /// <summary>
    /// This is the default class that carries location
    /// information from the scanner to the parser.
    /// If you don't declare "%YYLTYPE Foo" the parser
    /// will expect to deal with this type.
    /// </summary>
    public class LexLocation : IMerge<LexLocation>
    {
        public int sLin; // start line
        public int sCol; // start column
        public int eLin; // end line
        public int eCol; // end column

		/// <summary>
		/// Initializes a new instance of the <see cref="LexLocation"/> class.
		/// </summary>
        public LexLocation()
        { }

		/// <summary>
		/// Initializes a new instance of the <see cref="LexLocation"/> class.
		/// </summary>
		/// <param name="sl">The sl.</param>
		/// <param name="sc">The sc.</param>
		/// <param name="el">The el.</param>
		/// <param name="ec">The ec.</param>
        public LexLocation(int sl, int sc, int el, int ec)
        { sLin = sl; sCol = sc; eLin = el; eCol = ec; }

		/// <summary>
		/// Merges the specified last.
		/// </summary>
		/// <param name="last">The last.</param>
		/// <returns></returns>
        public LexLocation Merge(LexLocation last)
        { return new LexLocation(this.sLin, this.sCol, last.eLin, last.eCol); }
    }
}
