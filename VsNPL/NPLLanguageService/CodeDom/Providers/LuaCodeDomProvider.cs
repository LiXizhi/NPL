using System;
using System.CodeDom.Compiler;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Definitions;

namespace ParaEngine.Tools.Lua.CodeDom.Providers
{
    /// <summary>
    /// Provides access to IFileCodeMerger.
    /// </summary>
    public class LuaCodeDomProvider : CodeDomProvider, ILuaCodeDomProvider
    {
        private readonly ProjectItem projectItem;
        private readonly DTE dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeDomProvider"/> class.
        /// </summary>
        /// <param name="item"></param>
        public LuaCodeDomProvider(ProjectItem item)
        {
            projectItem = item;
            dte = projectItem.DTE;
        }

        /// <summary>
        /// Gets DTE instance.
        /// </summary>
        public DTE DTE
        {
            get { return dte; }
        }

        #region Obsolete overrides

		/// <summary>
		/// When overridden in a derived class, creates a new code compiler.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.CodeDom.Compiler.ICodeCompiler"/> that can be used for compilation of <see cref="N:System.CodeDom"/> based source code representations.
		/// </returns>
        [Obsolete]
        public override ICodeCompiler CreateCompiler()
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// When overridden in a derived class, creates a new code generator.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.CodeDom.Compiler.ICodeGenerator"/> that can be used to generate <see cref="N:System.CodeDom"/> based source code representations.
		/// </returns>
        [Obsolete]
        public override ICodeGenerator CreateGenerator()
        {
            throw new NotImplementedException();
        }

		/// <summary>
		/// When overridden in a derived class, creates a new code parser.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.CodeDom.Compiler.ICodeParser"/> that can be used to parse source code. The base implementation always returns null.
		/// </returns>
        [Obsolete]
        public override ICodeParser CreateParser()
        {
            return base.CreateParser();
        }

        #endregion

		/// <summary>
		/// Creates the file code merger.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		public IFileCodeMerger CreateFileCodeMerger(ProjectItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IFileCodeMerger fileCodeMerger;
			string activeFileName = DTE.ActiveDocument.ProjectItem.get_FileNames(1);
			bool useDocumentMerger = GetTextDocument() != null;

			if (useDocumentMerger || item.get_FileNames(1) == activeFileName)
				fileCodeMerger = new LuaFileCodeMerger(item);
			else
				fileCodeMerger = new LuaExternalFileCodeMerger(item);

			return fileCodeMerger;

		}

    	/// <summary>
        /// Creates FileCodeMerger for current ProjectItem.
        /// </summary>
        /// <returns></returns>
        public IFileCodeMerger CreateFileCodeMerger()
        {
			return CreateFileCodeMerger(DTE.ActiveDocument.ProjectItem);
        }

		/// <summary>
		/// Returns TextDocument object if exists.
		/// </summary>
		/// <param name="item">The <see cref="ProjectItem"/>.</param>
		/// <returns></returns>
		private static TextDocument GetTextDocument(ProjectItem item)
		{
			if (item.Document == null) return null;
			return (TextDocument)item.Document.Object("TextDocument");
		}

        /// <summary>
        /// Returns TextDocument object if exists.
        /// </summary>
        /// <returns></returns>
        private TextDocument GetTextDocument()
        {
        	return GetTextDocument(projectItem);
        }
    }
}