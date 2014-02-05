using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a break statement node in the AST for the Lua code file.
    /// </summary>
    public class Break : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Break"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Break(LexLocation location)
            : base(location)
        {
        }
    }
}
