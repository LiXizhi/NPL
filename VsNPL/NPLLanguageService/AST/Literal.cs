using System;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a literal node in the AST of a Lua code file.
    /// </summary>
    public class Literal : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Literal(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the type of the literal.
		/// </summary>
		/// <value>The type.</value>
        public LuaType Type { get; set; }

		/// <summary>
		/// Gets or sets the value of the literal.
		/// </summary>
		/// <value>The value.</value>
        public string Value { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("Literal [Type: {0}, Value: {1}]", this.Type, this.Value);
        }
    }
}
