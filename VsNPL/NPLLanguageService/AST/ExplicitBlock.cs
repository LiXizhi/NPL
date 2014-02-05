using System;
using System.Collections.Generic;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents an explicit block in the AST for a Lua code file.
    /// </summary>
    public class ExplicitBlock : Node
    {
        private string name = "~Block_" + Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplicitBlock"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public ExplicitBlock(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the qualifying name of the scope.
		/// </summary>
		/// <value>The name.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

		/// <summary>
		/// Gets whether this node denotes a scope.
		/// </summary>
		/// <value></value>
        public override bool IsScope
        {
            get { return true; }
        }

		/// <summary>
		/// Gets or sets the actual block inside.
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
