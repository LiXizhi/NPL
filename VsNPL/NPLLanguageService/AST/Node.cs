using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.AST
{
	/// <summary>
	/// A node in the abstract syntax tree parsed by the Lua Language Service.
	/// </summary>
	public class Node : IEnumerable<Node>
	{
		private readonly LexLocation location;

		/// <summary>
		/// Initializes a new instance of the <see cref="Node"/> class.
		/// </summary>
		/// <param name="location">The location of the node.</param>
		public Node(LexLocation location)
		{
			this.location = location;
		}

		/// <summary>
		/// Gets the location of the node in the parsed file.
		/// </summary>
		/// <value>The location.</value>
		public LexLocation Location
		{
			get { return location; }
		}

		/// <summary>
		/// Gets or sets the next sibling node.
		/// </summary>
		/// <value>The next.</value>
		public Node Next { get; set; }

		/// <summary>
		/// Gets whether this node denotes a scope.
		/// </summary>
		/// <value><c>true</c> if this instance is scope; otherwise, <c>false</c>.</value>
		public virtual bool IsScope
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the child nodes of the node.
		/// </summary>
		/// <returns>An enumerable collection of nodes.</returns>
		public virtual IEnumerable<Node> GetChildNodes()
		{
			yield break;
		}

		/// <summary>
		/// Writes a dump of the node tree using Trace.
		/// </summary>
		public void WriteTraceDump()
		{
			// Dump ourselves, write location information, if we have any
			if (location != null)
				Trace.WriteLine(String.Format("{0} (Line: {1}, Column: {2}, EndLine: {3}, EndColumn: {4})", this, location.sLin,
				                              location.sCol, location.eLin, location.eCol));
			else
				Trace.WriteLine(this);

			// Indent and dump the child nodes
			Trace.Indent();

			foreach (var node in GetChildNodes())
			{
				if (node != null)
					node.WriteTraceDump();
			}

			// Unindent back
			Trace.Unindent();

			// Dump the next node
			if (Next != null)
				Next.WriteTraceDump();
		}

		/// <summary>
		/// Gets or sets the context of the node.
		/// </summary>
		/// <value>The context.</value>
		public NodeContext Context { get; set; }

		/// <summary>
		/// Retrieves the parent node from the context.
		/// </summary>
		/// <value>The parent.</value>
		public Node Parent
		{
			get
			{
				if (Context == null)
					throw new ApplicationException("Context of node has not been set.");

				return Context.Parent;
			}
		}

		/// <summary>
		/// Creates a representation of the node including child nodes and next
		/// node. The representation is more or less the same as the trace dump.
		/// </summary>
		/// <returns>String that describes node.</returns>
		public string GetStringRepresentation()
		{
			var sb = new StringBuilder();

			// Dump ourselves, write location information, if we have any
			string toStr = this.ToString();

			if (toStr.Contains("Name = ~") || toStr.Contains("Name: ~"))
				toStr = toStr.Substring(0, toStr.IndexOf('[')).Trim();

			if (location != null)
			{
				sb.AppendLine(String.Format("{0} (Line: {1}, Column: {2}, EndLine: {3}, EndColumn: {4})", toStr, location.sLin,
				                            location.sCol, location.eLin, location.eCol));
			}
			else
			{
				sb.AppendLine(toStr);
			}

			foreach (var node in this.GetChildNodes())
			{
				if (node != null)
				{
					sb.Append(node.GetStringRepresentation());
					sb.AppendLine();
				}
			}

			if (Next != null)
			{
				sb.Append(Next.GetStringRepresentation());
				sb.AppendLine();
			}

			return sb.ToString().Trim();
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return this.GetType().Name;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<Node> GetEnumerator()
		{
			Node node = this;

			while (node != null)
			{
				yield return node;
				node = node.Next;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}