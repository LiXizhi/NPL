using System;
using EnvDTE;
using EnvDTE80;

namespace ParaEngine.Tools.Lua.CodeDom
{
	/// <summary>
	/// Represents event data from FileCodeModelChanged event.
	/// </summary>
	public sealed class FileCodeModelChangedEventArgs : EventArgs 
	{
		/// <summary>
		/// Gets the changed <see cref="CodeElement"/>.
		/// </summary>
		public CodeElement Element{get; set;}

		/// <summary>
		/// Gets the kind of change.
		/// </summary>
		public vsCMChangeKind ChangeKind { get; set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="FileCodeModelChangedEventArgs"/> class.
		/// </summary>
		/// <param name="element">The changed <see cref="CodeElement"/>.</param>
		/// <param name="changeKind">The kind of change.</param>
		public FileCodeModelChangedEventArgs(CodeElement element, vsCMChangeKind changeKind)
		{
			ChangeKind = changeKind;
			Element = element;
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="FileCodeModelChangedEventArgs"/> class.
		/// </summary>
		/// <param name="element">The changed <see cref="CodeElement"/>.</param>
		public FileCodeModelChangedEventArgs(CodeElement element)
			:this(element, vsCMChangeKind.vsCMChangeKindUnknown)
		{
			Element = element;
		}

		public override string ToString()
		{
			return String.Concat("FileCodeModelChangedEventArgs: ", Element);
		}
	}
}