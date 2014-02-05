namespace ParaEngine.Tools.Lua.Parser
{
    public interface IColorScan
    {
		/// <summary>
		/// Sets the source.
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="offset">The offset.</param>
        void SetSource(string source, int offset);

		/// <summary>
		/// Gets the next.
		/// </summary>
		/// <param name="state">The state.</param>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		/// <returns></returns>
        int GetNext(ref int state, out int start, out int end);
    }
}
