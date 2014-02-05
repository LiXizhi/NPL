using System.Collections.Generic;

namespace ParaEngine.Tools.Lua.AST
{
	/// <summary>
	/// Contains context-related information about nodes in the AST.
	/// Keeping context information for nodes is necessary for proper
	/// intellisense behaviour.
	/// </summary>
	public class NodeContext
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public NodeContext()
		{
			ValidLocalVariableNames=new List<string>();
		}

		/// <summary>
		/// Gets or sets the list of local variable names that are in scope
		/// at this node.
		/// </summary>
		/// <value>The valid local variable names.</value>
		public List<string> ValidLocalVariableNames { get; set; }

		/// <summary>
		/// Gets or sets the parent node of the current node.
		/// </summary>
		/// <value>The parent.</value>
		public Node Parent { get; set; }

		/// <summary>
		/// Checks whether a local variable with the same name is
		/// in scope at the current node.
		/// </summary>
		/// <param name="localName">Name of the local.</param>
		/// <returns>
		/// 	<c>true</c> if [has local variable in scope] [the specified local name]; otherwise, <c>false</c>.
		/// </returns>
		public bool HasLocalVariableInScope(string localName)
		{
			return ValidLocalVariableNames.Contains(localName);
		}
	}
}
