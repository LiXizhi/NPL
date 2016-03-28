using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// Provides the information for word completion and member selection to CodeSense in response to a
    /// parse request.
    /// </summary>
	public class DeclarationAuthoringScope : AuthoringScope
	{
		private readonly List<ICodeSenseDeclarationProvider> declarationProviders;
		private readonly LanguageService languageService;
		private string qualifiedName = String.Empty;
        public string m_quickInfoText;
        public TextSpan m_quickInfoSpan = new TextSpan();
        public string m_goto_filename = null;
        public TextSpan m_goto_textspan = new TextSpan();

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthoringScope"/> class.
        /// </summary>
        public DeclarationAuthoringScope(LanguageService languageService)
		{
			if (languageService == null)
				throw new ArgumentNullException("languageService");

			this.declarationProviders = new List<ICodeSenseDeclarationProvider>();
			this.languageService = languageService;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthoringScope"/> class.
		/// </summary>
		public DeclarationAuthoringScope(LanguageService languageService, IEnumerable<ICodeSenseDeclarationProvider> declarationProviders)
		{
			if (languageService == null)
				throw new ArgumentNullException("languageService");
			if (declarationProviders == null)
				throw new ArgumentNullException("declarationProviders");

			this.declarationProviders = new List<ICodeSenseDeclarationProvider>(declarationProviders);
			this.languageService = languageService;
		}

		/// <summary>
		/// Clears the authoring scope.
		/// </summary>
		public void Clear()
		{
			declarationProviders.Clear();
		}

		/// <summary>
		/// Adds a CodeSense provider to the authoring scope.
		/// </summary>
		/// <param name="declarationProvider">The CodeSense provider to add.</param>
		public void AddProvider(ICodeSenseDeclarationProvider declarationProvider)
		{
			if (declarationProvider == null)
				throw new ArgumentNullException("declarationProvider");

			declarationProviders.Add(declarationProvider);
		}

		/// <summary>
		/// Removes a declaration provider from the authoring scope.
		/// </summary>
		/// <param name="declarationProvider">The declaration provider to remove.</param>
		public void RemoveProvider(ICodeSenseDeclarationProvider declarationProvider)
		{
			if (declarationProvider == null)
				throw new ArgumentNullException("declarationProvider");

			if (declarationProviders.Contains(declarationProvider))
				declarationProviders.Remove(declarationProvider);
		}

		/// <summary>
		/// Sets the name of the qualified.
		/// </summary>
		/// <param name="qualifiedName">Name of the qualified.</param>
		public void SetQualifiedName(string qualifiedName)
		{
			if (qualifiedName == null)
				throw new ArgumentNullException("qualifiedName");

			this.qualifiedName = qualifiedName;
		}

		/// <summary>
		/// Gets the data tip text.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <param name="span">The span.</param>
		/// <returns></returns>
		public override string GetDataTipText(int line, int col, out TextSpan span)
		{
            // span = new TextSpan();
            // return String.Empty;

            //span.iEndLine = line;
            //span.iStartIndex = col-1;
            //span.iEndIndex = col+1;
            //span.iStartLine = line;
            span = m_quickInfoSpan;
            return m_quickInfoText;
		}

		/// <summary>
		/// Gets the declarations.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <param name="info">The info.</param>
		/// <param name="reason">The reason.</param>
		/// <returns></returns>
		public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
		{
			if (reason == ParseReason.CompleteWord)
			{
				// Get the declarations from all providers for the scope, Order by Name and project the declarations into Babel.Declarations
				Declaration[] declarations = declarationProviders.SelectMany(provider => provider.GetCompleteWordDeclarations())
																 .OrderBy(declaration => declaration.Name).Distinct()
																 .ToArray();

				return new LuaDeclarations(languageService, declarations);
			}

			if (reason == ParseReason.MemberSelect ||
				reason == ParseReason.MemberSelectAndHighlightBraces)
			{
				// Get the declarations from all providers for the scope, Order by Name and project the declarations into Babel.Declarations
				Declaration[] declarations = declarationProviders.SelectMany(provider => provider.GetMemberSelectDeclarations(qualifiedName))
															   .OrderBy(declaration => declaration.Name).Distinct(new DeclarationEqualityComparer())
															   .ToArray();

				return new LuaDeclarations(languageService, declarations);
			}

			return new LuaDeclarations(languageService, new Declaration[0]);
		}

		/// <summary>
		/// Gets the methods.
		/// </summary>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public override Methods GetMethods(int line, int col, string name)
		{
			Debug.WriteLine("GetMethods: " + name);

			string fullName = string.IsNullOrEmpty(qualifiedName) ? name : string.Format("{0}{1}{2}",
				qualifiedName, ".", name);

			Method[] methods = declarationProviders.SelectMany(provider => provider.GetMethods(fullName))
												   .ToArray();

			if (methods.Length == 0)
				return null;

			return new LuaMethods(methods);
		}

		/// <summary>
		/// Gotoes the specified CMD.
		/// </summary>
		/// <param name="cmd">The CMD.</param>
		/// <param name="textView">The text view.</param>
		/// <param name="line">The line.</param>
		/// <param name="col">The col.</param>
		/// <param name="span">The span.</param>
		/// <returns></returns>
        public override string Goto(Microsoft.VisualStudio.VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
			span = m_goto_textspan;
            return m_goto_filename;

            // testing: 
            // span.iStartLine = span.iEndLine = 23;
            // span.iStartIndex = 16; span.iEndIndex = 21;
            // return "E:/Temp/NPL/VsNPL/test.lua";
        }

        /// <summary>
        /// Defines methods to support the comparison of objects for equality.
        /// </summary>
        private class DeclarationEqualityComparer : IEqualityComparer<Declaration>
		{
			/// <summary>
			/// Determines whether the specified objects are equal.
			/// </summary>
			/// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
			/// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
			/// <returns>
			/// true if the specified objects are equal; otherwise, false.
			/// </returns>
			public bool Equals(Declaration x, Declaration y)
			{
				return string.Compare(x.Name, y.Name, StringComparison.Ordinal) == 0 &&
				       x.DeclarationType == y.DeclarationType;
			}

			/// <summary>
			/// Returns a hash code for the specified object.
			/// </summary>
			/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
			/// <returns>A hash code for the specified object.</returns>
			/// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
			public int GetHashCode(Declaration obj)
			{
				if(obj!=null)
					return obj.GetHashCode();

				return 0;
			}
		}
	}
}