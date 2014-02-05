using System;
using System.Collections.Generic;
using System.Linq;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
    /// <summary>
    /// Represents a function (either as a statement (declaration) or as an expression (assignment) node
    /// in the AST for the Lua code file.
    /// </summary>
    public class FunctionDeclaration : Node, IScopedDeclarationProvider, IDeclarationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Function"/> class.
        /// </summary>
        /// <param name="location">The location of the node.</param>
        public FunctionDeclaration(LexLocation location)
            : base(location)
        {
        }

		/// <summary>
		/// Gets or sets the name of the function.
		/// </summary>
		/// <value>The name.</value>
        public string Name { get; set; }

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
		/// Gets or sets whether the function was declared local in its scope.
		/// </summary>
		/// <value><c>true</c> if this instance is local; otherwise, <c>false</c>.</value>
        public bool IsLocal { get; set; }

		/// <summary>
		/// Gets whether this node denotes a scope.
		/// </summary>
		/// <value></value>
        public override bool IsScope
        {
            get { return true; }
        }


		/// <summary>
		/// Gets an enumeration of declarations that the node declares.
		/// </summary>
		/// <returns>An enumeration of declarations.</returns>
        public IEnumerable<Declaration> GetDeclarations()
        {
            var method = new Method
                            {
                                Name = this.Name,
                                DeclarationType = DeclarationType.Function,
                                IsLocal = this.IsLocal
                            };

            // Initialize parameters
            if (ParameterList != null)
            {
                method.Parameters = ParameterList.IdentifierList.OfType<Identifier>()
                                                                .Select(identifier => new Parameter
                                                                                      {
                                                                                          DeclarationType = DeclarationType.Parameter,
                                                                                          Name = identifier.Name,
                                                                                          IsLocal = true
                                                                                      })
                                                                .ToArray();
            }

            yield return method;
        }

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
            return String.Format("FunctionDeclaration [Name = {0}, IsLocal = {1}]", this.Name, this.IsLocal);
        }
	}
}
