using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.Parser;
using System;
using System.Diagnostics;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Represents a declaration that the nodes in the AST can provide to the CodeSense engine.
    /// </summary>
    [DebuggerDisplay("Name = {Name}, Type = {Type}, DeclarationType = {DeclarationType}, IsLocal = {IsLocal}")]
    public class Declaration : IEquatable<Declaration>
    {
		/// <summary>
		/// Gets or sets the name declared.
		/// </summary>
		/// <value>The name.</value>
        public string Name { get; set; }

		/// <summary>
		/// Gets or sets whether the declaration is local.
		/// </summary>
		/// <value><c>true</c> if this instance is local; otherwise, <c>false</c>.</value>
        public bool IsLocal { get; set; }

		/// <summary>
		/// Gets whether the declaration is global.
		/// </summary>
		/// <value><c>true</c> if this instance is global; otherwise, <c>false</c>.</value>
    	public bool IsGlobal
    	{
			get { return !IsLocal; }
    	}

        /// <summary>
        /// in which file this declaration is defined
        /// </summary>
        public string FilenameDefinedIn { get; set; }

        /// <summary>
        /// in which line and columns the declaration is defined. 
        /// </summary>
        public LexLocation TextspanDefinedIn { get; set; }

        /// <summary>
        /// Gets or sets the type of declaration.
        /// </summary>
        /// <value>The type of the declaration.</value>
        public DeclarationType DeclarationType { get; set; }

		/// <summary>
		/// Gets or sets the description of the declaration.
		/// </summary>
		/// <value>The description.</value>
        public string Description { get; set; }

		/// <summary>
		/// Gets or sets the type of the declared variable or field, if available.
		/// </summary>
		/// <value>The type.</value>
        public string Type { get; set; }

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
        public bool Equals(Declaration other)
        {
            if (other == null)
                return false;

            return this.Name.Equals(other.Name);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Declaration);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() * 27;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("{0}:{1}", DeclarationType.ToString()[0], Name);
        }
    }
}
