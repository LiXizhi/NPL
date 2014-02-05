using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    public class WhileLoop : Loop
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="WhileLoop"/> class.
		/// </summary>
		/// <param name="location">The location of the node.</param>
        public WhileLoop(LexLocation location)
            : base(location)
        {
        }
    }
}
