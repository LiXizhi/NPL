using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    public class RepeatUntilLoop : Loop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatUntilLoop"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public RepeatUntilLoop(LexLocation location)
            : base(location)
        {
        }
    }
}
