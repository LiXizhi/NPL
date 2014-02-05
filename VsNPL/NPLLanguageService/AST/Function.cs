using System;
using System.Collections.Generic;
using System.Linq;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    public class Function : Node, IScopedDeclarationProvider
    {
        private string name = "~Function_" + Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public Function(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets the qualifying name of the scope.
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
		/// Gets or sets the parameter list.
		/// </summary>
		/// <value>The parameter list.</value>
        public ParameterList ParameterList { get; set; }

		/// <summary>
		/// Gets or sets the function body.
		/// </summary>
		/// <value>The body.</value>
        public Block Body { get; set; }

        /// <summary>
        /// Gets an enumeration of declarations that should be scoped by the same node that declares it.
        /// </summary>
        /// <returns>An enumeration of declarations.</returns>
        public IEnumerable<Declaration> GetScopedDeclarations()
        {
            if (ParameterList != null && ParameterList.IdentifierList != null)
            {
                return ParameterList.IdentifierList.OfType<Identifier>()
                                                   .Select(identifier => DeclarationFactory.CreateDeclaration(DeclarationType.Parameter, true, identifier));
            }

            return new Declaration[0];
        }

        /// <summary>
        /// Gets the child nodes of the node.
        /// </summary>
        /// <returns>An enumerable collection of nodes.</returns>
        public override IEnumerable<Node> GetChildNodes()
        {
            yield return ParameterList;
            yield return Body;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("Function [Name = {0}]", this.Name);
        }
	}
}
