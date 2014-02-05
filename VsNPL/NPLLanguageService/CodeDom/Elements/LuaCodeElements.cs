using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EnvDTE;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// A collection of objects representing code constructs in a Lua source file.
    /// </summary>
    [ComVisible(true)]
    public class LuaCodeElements : List<CodeElement>, CodeElements
    {
        private readonly DTE dte;
        private readonly object parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeElements"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="parent"></param>
        public LuaCodeElements(DTE dte, object parent)
        {
            this.dte = dte;
            this.parent = parent;
        }

        #region Public Members

        /// <summary>
        /// Adds an object to the end of the Collection.
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(CodeElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            Add(element);
        }

        /// <summary>
        /// Inserts an element into the Collection at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="element"></param>
        public void InsertElement(int index, CodeElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            Insert(index, element);
        }

        /// <summary>
        /// Removes the first occurence of a specific object from the Collection.
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(CodeElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            Remove(element);
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Converts position of a specified element to index.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private int PositionToIndex(object element)
        {
            var cde = element as CodeElement;
        	if (cde != null)
        		return IndexOf(cde);

        	var pos = (int)element;
        	if (pos == -1)
        		return Count;

        	return pos - 1;
        }

        #endregion

        #region CodeElements Members

        /// <summary>
        /// Gets a value indicating the number of objects in the <see cref="T:EnvDTE.CodeElements" />
        ///  collection.
        /// </summary>
        public new int Count
        {
            get { return base.Count; }
        }

        /// <summary>
        /// Creates a programmatic identifier that does not collide with other identifiers in the scope
        ///  and that follows the current language naming rules.
        /// </summary>
        /// <param name="prefix">Required. The prefix string or whole name to check to see whether or not it is unique for the collection of code elements.</param>
        /// <param name="newName">Optional. If supplied, this returns with a guaranteed unique name.</param>
        /// <returns>A Boolean value indicating true if the name is a unique identifier; otherwise returns false.</returns>
        public bool CreateUniqueID(string prefix, ref string newName)
        {
            newName = Guid.NewGuid().ToString();
            return true;
        }

        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        public DTE DTE
        {
            get { return dte; }
        }

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeElements" /> collection.
        /// </summary>
        public object Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the Collection.
        /// </summary>
        /// <returns></returns>
        public new System.Collections.IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        /// <summary>
        /// Returns a <see cref="T:EnvDTE.CodeElement" /> object in a <see cref="T:EnvDTE.CodeElements" />
        ///  collection.
        /// </summary>
        /// <param name="index">Required. The index of the <see cref="T:EnvDTE.CodeElement" /> object to return. </param>
        /// <returns>A <see cref="T:EnvDTE.CodeElement" /> object.</returns>
        public CodeElement Item(object index)
        {
            return this[PositionToIndex(index)];
        }

        /// <summary>
        /// Microsoft Internal Use Only.
        /// </summary>
        /// <param name="Element"></param>
        public void Reserved1(object Element)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}