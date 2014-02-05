using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents the topmost node in the AST.
    /// </summary>
    public class Chunk : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Chunk"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Chunk(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the node representing the block of statements in the chunk.
		/// </summary>
		/// <value>The block.</value>
        public Block Block { get; set; }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return Block;
        }
    }
}
