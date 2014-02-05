using System;
using System.Collections;
using System.Collections.Generic;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom
{
    /// <summary>
    /// Class represents navigator for CodeElements tree.
    /// </summary>
    public class LuaCodeDomNavigator : IEnumerator<SimpleCodeElement>
    {
    	private SimpleCodeElement rootElement;
        protected List<SimpleCodeElement> codeElements;
		protected SimpleCodeElement current;
        
        /// <summary>
		/// Initializes a new instance of the <see cref="LuaCodeDomNavigator"/> class.
        /// </summary>
        /// <param name="rootElement"></param>
        public LuaCodeDomNavigator(CodeElement rootElement)
        {
            if (rootElement == null)
                throw new ArgumentNullException("rootElement");

			IncludeSelf = true;
            SetRootElement((SimpleCodeElement) rootElement);
        }


        /// <summary>
		/// Initializes a new instance of the <see cref="LuaCodeDomNavigator"/> class.
        /// </summary>
        /// <param name="codeModel"></param>
        public LuaCodeDomNavigator(LuaFileCodeModel codeModel)
            : this(codeModel.RootElement)
        {
        }

		/// <summary>
		/// Gets or sets the root element.
		/// </summary>
		/// <value>The root element.</value>
		protected SimpleCodeElement RootElement
		{
			get { return rootElement; }
			set { rootElement = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [include self].
		/// </summary>
		/// <value><c>true</c> if [include self]; otherwise, <c>false</c>.</value>
		public bool IncludeSelf{ get; set; }

		/// <summary>
		/// Gets the result elements.
		/// </summary>
		/// <value>The result elements.</value>
        public List<SimpleCodeElement> ResultElements
        {
            get { return codeElements; }
        }

		/// <summary>
		/// Find elements in members recursively.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="K"></typeparam>
		/// <returns></returns>
        public IEnumerable<SimpleCodeElement> WalkTopLevelMembers<T, K>()
        {
            InitializeRoot();
            WalkTopLevelMembers<T, K>(rootElement);
            return ResultElements;
        }

		/// <summary>
		/// Find elements in members recursively.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
        public IEnumerable<SimpleCodeElement> WalkTopLevelMembers<T>()
        {
            InitializeRoot();
            WalkTopLevelMembers<T>(rootElement);
            return ResultElements;
        }

        /// <summary>
		/// Find elements elements in members recursively.
        /// </summary>
        public virtual IEnumerable<SimpleCodeElement> WalkMembers<T, K>()
        {
            InitializeRoot();
            WalkMembers<T, K>(rootElement);
            return ResultElements;
        }

        /// <summary>
		/// Find elements elements in members recursively.
        /// </summary>
        public virtual IEnumerable<SimpleCodeElement> WalkMembers<T>()
        {
            InitializeRoot();
            WalkMembers<T>(rootElement);
            return ResultElements;
        }

        /// <summary>
        /// Set root element for walking tree.
        /// </summary>
        /// <param name="element"></param>
        public void SetRootElement(SimpleCodeElement element)
        {
            rootElement = element;
        }

        /// <summary>
        /// Initialize class member variables. 
        /// </summary>
        protected virtual void InitializeRoot()
        {
            codeElements = new List<SimpleCodeElement>();
        }

		/// <summary>
		/// Find top-level elements in members recursively.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element">The element.</param>
        protected virtual void WalkTopLevelMembers<T>(CodeElement element)
        {
            if (element == null) return;
			if (element is T)
            {
				if (element != rootElement || (element == rootElement && IncludeSelf))
                codeElements.Add((SimpleCodeElement) element);
            }

            if (element.Children != null)
            {
                foreach (CodeElement childElement in element.Children)
                {
                    if (childElement is T)
                    {
                        codeElements.Add((SimpleCodeElement) childElement);
                    }
                }
            }
        }

		/// <summary>
		/// Find top-level elements in members recursively.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="K"></typeparam>
		/// <param name="element">The element.</param>
        protected virtual void WalkTopLevelMembers<T, K>(CodeElement element)
        {
            if (element == null) return;
            if (element is T || element is K)
            {
				if (element != rootElement || (element == rootElement && IncludeSelf))
                codeElements.Add((SimpleCodeElement)element);
            }

            if (element.Children != null)
            {
                foreach (CodeElement childElement in element.Children)
                {
                    if (childElement is T || childElement is K)
                    {
                        codeElements.Add((SimpleCodeElement)childElement);
                    }
                }
            }
        }

		/// <summary>
		/// Find elements in members recursively.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="K"></typeparam>
		/// <param name="element">The element.</param>
        protected virtual void WalkMembers<T, K>(CodeElement element)
        {
            if (element == null) return;
            if (element is T || element is K)
            {
				if (element != rootElement || (element == rootElement && IncludeSelf))
                codeElements.Add((SimpleCodeElement) element);
            }

            if (element.Children != null)
            {
                foreach (CodeElement childElement in element.Children)
                {
                    WalkMembers<T, K>(childElement);
                }
            }
        }

		/// <summary>
		/// Find elements in members recursively.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="element">The element.</param>
        protected virtual void WalkMembers<T>(CodeElement element)
        {
            if (element == null) return;
			if (element is T)
            {
				if (element != rootElement || (element == rootElement && IncludeSelf))
                codeElements.Add((SimpleCodeElement) element);
            }

            if (element.Children != null)
            {
                foreach (CodeElement childElement in element.Children)
                {
                    WalkMembers<T>(childElement);
                }
            }
        }

		/// <summary>
		/// Gets the parent element.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
        public static CodeElement GetParentElement(ICodeDomElement element)
        {
            if (element == null) return null;
            if (element.ParentElement is LuaCodeFunction || element.ParentElement is LuaCodeClass)
            {
                return element.ParentElement;
            }
            CodeElement parent = GetParent(element.ParentElement as ICodeDomElement);
            return parent;
        }


		/// <summary>
		/// Gets the parent.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <returns></returns>
        private static CodeElement GetParent(ICodeDomElement element)
        {
            if (element == null) return null;
            if (element.ParentElement is LuaCodeFunction
                || element.ParentElement is LuaCodeClass || element.ParentElement == null)
            {
                return element.ParentElement;
            }
            CodeElement parent = GetParent(element.ParentElement as ICodeDomElement);
            return parent;
        }

		/// <summary>
		/// Gets the name of the code elements by.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
        public IEnumerable<SimpleCodeElement> GetCodeElementsByName(string name)
        {
            return GetCodeElementsByName<CodeElement>(name);
        }

		/// <summary>
		/// Gets the name of the code elements by.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name">The name.</param>
		/// <returns></returns>
        public IEnumerable<SimpleCodeElement> GetCodeElementsByName<T>(string name) where T : CodeElement
        {
            var elements = WalkMembers<T>();
            foreach (var element in elements)
            {
                if (element.Name == name)
                {
                    yield return element;
                }
            }
        }

		/// <summary>
		/// Performs application-defined tasks associated with freeing,
		/// releasing, or resetting unmanaged resources.
		/// </summary>
        public void Dispose()
        {
        }

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public bool MoveNext()
        {
            if (current == null)
            {
                current = rootElement;
                return true;
            }
            throw new NotImplementedException();
        }

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception>
        public void Reset()
        {
            current = null;
        }

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <value></value>
		/// <returns>The element in the collection at the current position of the enumerator.</returns>
        object IEnumerator.Current
        {
            get { return Current; }
        }

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <value></value>
		/// <returns>The element in the collection at the current position of the enumerator.</returns>
        public SimpleCodeElement Current
        {
            get { return current; }
        }

    }
}