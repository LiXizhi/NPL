using System;
using System.Collections.Generic;
using System.Linq;

namespace ParaEngine.Tools.Lua.AST
{
	/// <summary>
	/// Context for local variable declarations.
	/// </summary>
	public static class DeclarationProviderExtensions
	{
		/// <summary>
		/// Initializes the context.
		/// </summary>
		/// <param name="chunk">The chunk.</param>
		public static void InitializeContext(this Chunk chunk)
		{
			var validLocals = new List<string>();
			chunk.InitializeContextForNode(null, validLocals);
		}

		/// <summary>
		/// Determines whether [has local variable in scope] [the specified node].
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="localName">Name of the local.</param>
		/// <returns>
		/// 	<c>true</c> if [has local variable in scope] [the specified node]; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasLocalVariableInScope(this Node node, string localName)
		{
			if (node.Context == null)
				throw new ApplicationException("Context of node has not been set.");

			return node.Context.HasLocalVariableInScope(localName);
		}

		/// <summary>
		/// Initializes the context for node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="parentNode">The parent node.</param>
		/// <param name="localNames">The local names.</param>
		private static void InitializeContextForNode(this Node node, Node parentNode, ICollection<string> localNames)
		{
			if (node is IDeclarationProvider)
			{
				var declaringNode = (IDeclarationProvider) node;

				foreach (var declaration in declaringNode.GetDeclarations())
				{
					if (declaration.IsLocal && !localNames.Contains(declaration.Name))
						localNames.Add(declaration.Name);
				}
			}

			if (node != null)
			{
				node.Context = CreateNodeContext(localNames, parentNode);

				foreach (var childNode in node.GetChildNodes())
				{
					InitializeContextForNode(childNode, node, localNames.ToList());
				}

				if (node.Next != null)
				{
					InitializeContextForNode(node.Next, node.Parent, localNames.ToList());
				}
			}
		}

		/// <summary>
		/// Creates the node context.
		/// </summary>
		/// <param name="localNames">The local names.</param>
		/// <param name="parentNode">The parent node.</param>
		/// <returns></returns>
		private static NodeContext CreateNodeContext(IEnumerable<string> localNames, Node parentNode)
		{
			var context = new NodeContext
			              	{
			              		ValidLocalVariableNames = localNames.ToList(),
			              		Parent = parentNode
			              	};
			return context;
		}
	}
}