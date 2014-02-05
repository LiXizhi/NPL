using System;
using EnvDTE;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
    public class InvalidCodeElementException : Exception
    {
		[NonSerialized]
        private readonly CodeElement element;

		/// <summary>
		/// Gets the element.
		/// </summary>
		/// <value>The element.</value>
        internal CodeElement Element
        {
            get { return element; }
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidCodeElementException"/> class.
		/// </summary>
        public InvalidCodeElementException()
		{
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidCodeElementException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
        public InvalidCodeElementException(string message)
            : base(message)
        {
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidCodeElementException"/> class.
		/// </summary>
		/// <param name="element">The element.</param>
        internal InvalidCodeElementException(CodeElement element)
            : this(String.Empty, element)
        {
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidCodeElementException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="element">The element.</param>
        internal InvalidCodeElementException(string message, CodeElement element)
            : base(message)
        {
            this.element = element;
        }
		/// <summary>
		/// Initializes a new instance of the <see cref="InvalidCodeElementException"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="innerException">The inner exception.</param>
        public InvalidCodeElementException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}