using System;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents an identifier node in the AST for the Lua code file.
    /// </summary>
    public class Identifier : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Identifier(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the name of the identifier.
		/// </summary>
		/// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("Identifier [Name = {0}]", this.Name);
        }
    }
}
