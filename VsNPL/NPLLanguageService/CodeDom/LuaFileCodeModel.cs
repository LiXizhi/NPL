using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.TextManager.Interop;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.CodeDom
{
    /// <summary>
    /// Provides a FileCodeModel based upon the representation of the program obtained via AST Nodes.
    /// 
    /// There are 3 ways a document can be edited that the code model needs to handle:
    ///     1. The user edits the document inside of VS.  Here we don't need
    ///        to update the code model until the next call back to manipulate it.
    ///     2. A script runs which uses EditPoint's to update the text of the document.  
    ///     3. The user uses the FileCodeModel to add members to the document.
    /// 
    /// </summary>
    [ComVisible(true)]
    public sealed class LuaFileCodeModel : SimpleCodeElement, FileCodeModel2
    {
        public const string ENTRYFUNCTION_NAME = "@LuaEntryFunction";

        private readonly ProjectItem parent; // the project item for which we represent the FileCodeModel on
        private readonly CodeDomProvider provider;
    	private Chunk chunk;
        private bool isDirty;
        private TextPoint currentEditingPoint;
        private bool modelInitialized;
        private CodeElement codeElementFromPointObject;
        private LuaCodeClass rootElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaFileCodeModel"/> class.
        /// </summary>
        /// <param name="dte">Global DTE instance.</param>
        /// <param name="provider">CodeDomProvider implementation.</param>
        /// <param name="parent">Parent ProjectItem of FileCodeModel.</param>
        /// <param name="filename">Code file name.</param>
        public LuaFileCodeModel(DTE dte, ProjectItem parent, CodeDomProvider provider, string filename)
            : base(dte, filename)
        {
            this.parent = parent;
            this.provider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaFileCodeModel"/> class.
        /// </summary>
        /// <param name="dte">Global DTE instance.</param>
        /// <param name="buffer">IVsTextLines instance.</param>
        /// <param name="provider">CodeDomProvider implementation.</param>
        /// <param name="moniker">File moniker.</param>
        public LuaFileCodeModel(DTE dte, IVsTextLines buffer, CodeDomProvider provider, string moniker)
            : base(dte, moniker)
        {
            TextBuffer = buffer;
            this.provider = provider;
        }

		/// <summary>
		/// Gets a value indicating whether this instance is dirty.
		/// </summary>
		/// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
    	public bool IsDirty
    	{
    		get { return isDirty; }
    	}

    	/// <summary>
    	/// Gets the text buffer.
    	/// </summary>
    	/// <value>The text buffer.</value>
    	public IVsTextLines TextBuffer { get; private set; }

    	/// <summary>
        /// Gets the root element of FileCodeModel.
        /// </summary>
        public LuaCodeClass RootElement
        {
            get { return rootElement; }
            set
            {
                rootElement = value;
                if (rootElement != null)
                {
                    rootElement.Parent = this;
                }
            }
        }

		/// <summary>
		/// Gets or Sets a value indicating whether this <see cref="LuaFileCodeModel"/> is dirty.
		/// </summary>
		/// <value><c>true</c> if dirty; otherwise, <c>false</c>.</value>
		public bool Dirty
		{
			get { return modelInitialized; }
			set { ModelInitialized = !value; }
		}

        /// <summary>
        /// Indicates FileCodemodel initialization status.
        /// </summary>
        public bool ModelInitialized
        {
            get { return modelInitialized; }
			private set { modelInitialized = value; }
        }

        #region FileCodeModel Members

        /// <summary>
        /// Gets all the CodeElements that live in the FileCodeModel.
        /// (returns the root element of code tree)
        /// </summary>
        public CodeElements CodeElements
        {
            get
            {
                var res = new LuaCodeElements(DTE, this);
				if (RootElement != null)
					res.AddElement(RootElement);
                return res;
            }
        }

        #region FileCodeModel2 Members

		/// <summary>
		/// Used when the code model is built to determine whether the parse operation completed or encountered an error.
		/// </summary>
		/// <value></value>
		/// <returns>A <see cref="T:EnvDTE80.vsCMParseStatus"/> enumeration.</returns>
        public vsCMParseStatus ParseStatus
        {
            get { throw new NotImplementedException(); }
        }

		/// <summary>
		/// Gets a value indicating whether a batch code model updates is currently open.
		/// </summary>
		/// <value></value>
		/// <returns>true if a batch of code model updates is currently open; otherwise, false.</returns>
        public bool IsBatchOpen
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets all the CodeElements that live in the Lua FileCodeModel.
        /// </summary>
        public CodeElements LuaCodeElements
        {
            get
            {
                var res = new LuaCodeElements(DTE, this);
                var navigator = new LuaCodeDomNavigator(this);
                foreach (var codeElement in navigator.WalkTopLevelMembers<LuaCodeFunction, LuaCodeVariable>())
                {
                    if (codeElement.Name != ENTRYFUNCTION_NAME)
                    {
                        res.AddElement(codeElement);
                    }
					else
                    {
						var subNavigator = new LuaCodeDomNavigator(codeElement) {IncludeSelf = false};
                    	foreach (var codeSubElement in subNavigator.WalkMembers<LuaCodeFunction>())
						{
							//if (codeSubElement.Name != ENTRYFUNCTION_NAME)
								res.AddElement(codeSubElement);
						}
                    }
                }
                return res;
            }
        }

        /// <summary>
        /// Gets the parent ProjectItem reference.
        /// </summary>
        public ProjectItem Parent
        {
            get { return parent; }
        }

		/// <summary>
		/// Gets the text point that is the location of the end of the code item.
		/// </summary>
		/// <value></value>
        public override TextPoint EndPoint
        {
            get { return ((TextDocument) ProjectItem.Document.Object("TextDocument")).EndPoint; }
        }

		/// <summary>
		/// Gets a text point object that defines the beginning of the code item.
		/// </summary>
		/// <value></value>
        public override TextPoint StartPoint
        {
            get { return ((TextDocument) ProjectItem.Document.Object("TextDocument")).StartPoint; }
        }

		/// <summary>
		/// Returns a collection of objects contained within this <see cref="T:EnvDTE.CodeElement"/>.
		/// </summary>
		/// <value></value>
        public override CodeElements Children
        {
            get { return CodeElements; }
        }

		/// <summary>
		/// Gets the <see cref="T:EnvDTE.CodeElements"/> collection containing the CodeElement
		/// that supports this property.
		/// </summary>
		/// <value></value>
        public override CodeElements Collection
        {
            get { return null; }
        }

		/// <summary>
		/// Gets a fully-qualified name of the specified code element.
		/// </summary>
		/// <value></value>
        public override string FullName
        {
            get { return DTE.FullName; }
        }

		/// <summary>
		/// Gets an enumeration that defines the type of object.
		/// </summary>
		/// <value></value>
        public override vsCMElement Kind
        {
            get { return vsCMElement.vsCMElementModule; }
        }

		/// <summary>
		/// Gets the <see cref="T:EnvDTE.ProjectItem"/> object associated
		/// with the <see cref="T:EnvDTE.CodeElement"/> object.
		/// </summary>
		/// <value></value>
        public override ProjectItem ProjectItem
        {
            get { return parent; }
        }


		/// <summary>
		/// Adds a function to the top-level element.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="function">The function.</param>
		/// <returns></returns>
        public CodeFunction AddFunction(string objectName, LuaDeclaredType returnType, FunctionDeclaration function)
        {
            CodeFunction codeFunction = RootElement.AddFunction(objectName, vsCMFunction.vsCMFunctionFunction,
                                                                returnType, Missing.Value,
                                                                vsCMAccess.vsCMAccessPublic, function);
            return codeFunction;
        }

		/// <summary>
		/// Adds a variable to the top-level CodeElement.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="variableType">Type of the variable.</param>
		/// <param name="variableNode">The variable node.</param>
		/// <param name="access">The access.</param>
		/// <returns></returns>
        public CodeVariable AddVariable(string objectName, object variableType, object variableNode, vsCMAccess access)
        {
            return RootElement.AddVariable(objectName, variableType, null, access, variableNode);
        }

		/// <summary>
		/// Adds a function to the top-level CodeElement.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="objectKind">Kind of the object.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="function">The function.</param>
		/// <param name="access">The access.</param>
		/// <returns></returns>
        public CodeFunction AddFunction(string objectName, vsCMFunction objectKind, object returnType, object function,
                                        vsCMAccess access)
        {
            CodeFunction codeFunction = AddFunction(objectName, returnType as LuaDeclaredType,
                                                    function as FunctionDeclaration);
            codeFunction.Access = access;
            return codeFunction;
        }

		/// <summary>
		/// Ensures that all current code model events have been raised
		/// and the model has finished being generated.
		/// </summary>
        public void Synchronize()
        {
			modelInitialized = false;
			
			
        }

		/// <summary>
		/// Used to receive specific <see cref="T:EnvDTE.CodeElement"/>.
		/// </summary>
		/// <param name="ID">The string used to identify the element.</param>
		/// <returns>
		/// A <see cref="T:EnvDTE.CodeElement"/> object.
		/// </returns>
        public CodeElement ElementFromID(string ID)
        {
            throw new NotImplementedException();
        }

        #region Not Supported members

        /// <summary>
        /// Adds a class to the top-level (empty) namespace 
        /// </summary>
        /// <param name="objectName">The name of the class to add</param>
        /// <param name="Position">The position where the class should be added (1 based)</param>
        /// <param name="Bases">The bases the class dervies from</param>
        /// <param name="ImplementedInterfaces">the interfaces the class implements</param>
        /// <param name="Access">The classes protection level</param>
        public CodeClass AddClass(string objectName, object Position, object Bases, object ImplementedInterfaces,
                                  vsCMAccess Access)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the namespace.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="position">The position.</param>
		/// <returns></returns>
        public CodeNamespace AddNamespace(string objectName, object position)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Removes the specified element.
		/// </summary>
		/// <param name="element">The element.</param>
        public void Remove(object element)
        {
            throw new NotSupportedException("LuaFileCodeModel does not support this feature.");
        }

		/// <summary>
		/// Adds the import.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="Position">The position.</param>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
        public CodeImport AddImport(string name, object Position, string alias)
        {
            throw new NotSupportedException("LuaFileCodeModel does not support this feature.");
        }

		/// <summary>
		/// Begins the batch.
		/// </summary>
        public void BeginBatch()
        {
            throw new NotSupportedException("LuaFileCodeModel does not support this feature.");
        }

		/// <summary>
		/// Ends the batch.
		/// </summary>
        public void EndBatch()
        {
            throw new NotSupportedException("LuaFileCodeModel does not support this feature.");
        }

        #endregion

		/// <summary>
		/// Adds the delegate.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="type">The type.</param>
		/// <param name="position">The position.</param>
		/// <param name="access">The access.</param>
		/// <returns></returns>
        public CodeDelegate AddDelegate(string objectName, object type, object position, vsCMAccess access)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the enum.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="position">The position.</param>
		/// <param name="bases">The bases.</param>
		/// <param name="access">The access.</param>
		/// <returns></returns>
        public CodeEnum AddEnum(string objectName, object position, object bases, vsCMAccess access)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the interface.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="position">The position.</param>
		/// <param name="bases">The bases.</param>
		/// <param name="access">The access.</param>
		/// <returns></returns>
		public CodeInterface AddInterface(string objectName, object position, object bases, vsCMAccess access)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the struct.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="position">The position.</param>
		/// <param name="bases">The bases.</param>
		/// <param name="implementedInterfaces">The implemented interfaces.</param>
		/// <param name="access">The access.</param>
		/// <returns></returns>
		public CodeStruct AddStruct(string objectName, object position, object bases, object implementedInterfaces, vsCMAccess access)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Adds the attribute.
		/// </summary>
		/// <param name="objectName">Name of the object.</param>
		/// <param name="value">The value.</param>
		/// <param name="position">The position.</param>
		/// <returns></returns>
		public CodeAttribute AddAttribute(string objectName, string value, object position)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="T:EnvDTE.TextPoint" /> object that marks the beginning 
        /// of the code element definition.
        /// </summary>
        /// <param name="part">Optional. A <see cref="T:EnvDTE.vsCMPart" /> constant specifying 
        /// the portion of the code to retrieve.</param>
        /// <returns>A <see cref="T:EnvDTE.TextPoint" /> object.</returns>
        public override TextPoint GetEndPoint(vsCMPart part)
        {
            return EndPoint;
        }

        /// <summary>
        /// Returns a <see cref="T:EnvDTE.TextPoint" /> object that marks the end 
        /// of the code element definition.
        /// </summary>
        /// <param name="part">Optional. A <see cref="T:EnvDTE.vsCMPart" /> constant specifying 
        /// the portion of the code to retrieve.</param>
        /// <returns>A <see cref="T:EnvDTE.TextPoint" /> object.</returns>
        public override TextPoint GetStartPoint(vsCMPart part)
        {
            return StartPoint;
        }

        #endregion

        /// <summary>
        /// Called when text is added to the page via either the user editing the page or 
        /// via using EditPoint's from FileCodeModel.  We go through and update any items
        /// after the current item so their current positions are correct.  We also mark the
        /// model as being dirty so that before we hand out any CodeElement's we can re-parse, but
        /// we avoid the expensive reparse if the user is simply doing multiple edits.  
        /// 
        /// Finally, when things go idle, we will also re-parse the document so we need not delay
        /// it until the user actually wants to do something.
        /// </summary>
        public void OnLineChanged(object sender, TextLineChange[] changes, int last)
        {
            isDirty = true;
            modelInitialized = false;
        }

        /// <summary>
        /// Called when the editor is idle.  This is an ideal time to re-parse.
        /// </summary>
        public static void OnIdle(IVsTextLines lines)
        {
            //Reparse();
        }

        /// <summary>
        /// performs lazy initialization to ensure our current code model is up-to-date.
        /// 
        /// If we haven't yet created our CodeDom backing we'll create it for the 1st time.  If we've
        /// created our backing, but some elements have been changed that we haven't yet reparsed
        /// then we'll reparse & merge any of the appropriate changes.
        /// </summary>
        public void Initialize(Chunk chunkNode)
        {
            chunk = chunkNode;
            var translator = new ASTTranslator(DTE, this);
            RootElement = translator.Translate(chunkNode);
            modelInitialized = true;
        }


        /// <summary>
        /// Gets CodeElement by actual EditPoint.
        /// </summary>
        /// <returns></returns>
        public CodeElement GetElementByEditPoint()
        {
            var textDocument = (TextDocument) ProjectItem.Document.Object("TextDocument");
            var point = new LuaTextPoint(textDocument,
                                         textDocument.Selection.ActivePoint.LineCharOffset,
                                         textDocument.Selection.ActivePoint.Line);
            var element = GetElementByEditPoint(point);
            return element;
        }

        /// <summary>
        /// Gets CodeElement by specified TextPoint.
        /// </summary>
        /// <returns></returns>
        public CodeElement GetElementByEditPoint(TextPoint point)
        {
            var element = CodeElementFromPoint(point, vsCMElement.vsCMElementClass);
			//if (element != null)
			//{
			//    LexLocation loc = ((ICodeDomElement) element).LuaASTTypeObject.Location;
			//    Trace.WriteLine(String.Format("Element[{4}] Location: StartCol-{0} StartLine-{1} EndCol-{2} EndLine-{3}", loc.sCol, loc.sLin, loc.eCol, loc.eLin, element));
			//}
            return element;
        }

        /// <summary>
        /// Given a point and an element type to search for returns the element of that type at that point
        /// or null if no element is found.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public CodeElement CodeElementFromPoint(TextPoint point, vsCMElement scope)
        {
            codeElementFromPointObject = null;
            currentEditingPoint = point;

            WalkMembers(RootElement);

            //Get FunctionCall if result is Identifier
            if (codeElementFromPointObject is LuaCodeElement<Identifier>
                && ((ICodeDomElement) codeElementFromPointObject).ParentElement is LuaCodeElement<FunctionCall>)
            {
                var funcCall =
                    ((ICodeDomElement) codeElementFromPointObject).ParentElement as LuaCodeElement<FunctionCall>;
                if (funcCall != null)
                    if (funcCall.Children != null && codeElementFromPointObject == funcCall.Children.Item(1))
                    {
                        codeElementFromPointObject = funcCall;
                    }
            }
            return codeElementFromPointObject;
        }

		/// <summary>
		/// Find FunctionCall elements in members recursively.
		/// </summary>
		/// <param name="element">The element.</param>
        private void WalkMembers(CodeElement element)
        {
            try
            {
                if (element == null) return;

                if (element is LuaCodeStatement)
                {
                    var statement = element as LuaCodeStatement;
                    foreach (CodeElement childElement in statement.Statements)
                    {
                        WalkMembers(childElement);
                    }
                }
                else if (IsInRange(((ICodeDomElement) element).LuaASTTypeObject, currentEditingPoint, element))
                {
                    codeElementFromPointObject = element;
                }

                if (element.Children != null)
                {
                    foreach (CodeElement childElement in element.Children)
                    {
                        WalkMembers(childElement);
                    }
                }

				//Check parameters of function
                if (element is LuaCodeFunction)
                {
					if (codeElementFromPointObject != null) return;
                    var parameters = ((LuaCodeFunction) element).Parameters;
                    if (parameters != null)
                    {
                        foreach (CodeElement childElement in parameters)
                        {
                            WalkMembers(childElement);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

		/// <summary>
		/// Check codeObject location.
		/// </summary>
		/// <param name="codeObject">The code object.</param>
		/// <param name="point">The point.</param>
		/// <param name="element">The element.</param>
		/// <returns>
		/// 	<c>true</c> if [is in range] [the specified code object]; otherwise, <c>false</c>.
		/// </returns>
        private bool IsInRange(Node codeObject, TextPoint point, CodeElement element)
        {
            if (codeObject is Chunk || codeObject == null) return false;

            int startLine = codeObject.Location.sLin;
            int endLine = codeObject.Location.eLin;

            // match in the middle of a block
            //if (line < point.Line && point.Line < endLine)
            //{
            //    return true;
            //}

			if (startLine == point.Line || endLine == startLine || element is LuaCodeVariableTable)
            {
                // single line match, make sure the columns are right
                int startCol = codeObject.Location.sCol + 1;
                int endCol = codeObject.Location.eCol + 1;

                if (codeObject is FunctionDeclaration)
                {
                    var endPoint = LuaCodeDomHelper.GetFunctionDeclarationFromSource
                        (codeObject as FunctionDeclaration,
                         (TextDocument) parent.Document.Object("TextDocument"));
                    endLine = startLine;
                    endCol = endPoint == null ? startCol : endPoint.DisplayColumn;
                }
                if (element is LuaCodeVariableTable)
                {
                    var table = element as LuaCodeVariableTable;
                    if (table.IdentifierLocation != null)
                    {
                        startLine = table.IdentifierLocation.sLin;
                        startCol = table.IdentifierLocation.sCol;
                        endCol = table.IdentifierLocation.eCol;
                    }
                }
                if (startLine == point.Line && startCol <= point.LineCharOffset &&
                    point.LineCharOffset < endCol)
                {
                    return true;
                }
                if (endLine != startLine && endLine == point.Line &&
                    endCol <= point.LineCharOffset && point.LineCharOffset < endCol)
                {
                    return true;
                }
            }

            return false;
        }

		/// <summary>
		/// Merge Changes into VS source code.
		/// </summary>
		/// <param name="oldName">The old name.</param>
		/// <param name="elements">The elements.</param>
		/// <returns></returns>
		public bool MergeChanges(string oldName, IEnumerable<CodeElement> elements)
		{
			return MergeChanges(oldName, elements, true);
		}


		/// <summary>
		/// Merge Changes into VS source code.
		/// </summary>
		/// <param name="oldName">The old name.</param>
		/// <param name="elements">The elements.</param>
		/// <param name="useUndoManager">if set to <c>true</c> [use undo manager].</param>
		/// <returns></returns>
        public bool MergeChanges(string oldName, IEnumerable<CodeElement> elements, bool useUndoManager)
        {
            var result = false;
            if (elements == null || string.IsNullOrEmpty(oldName)) return result;
            if (provider == null) throw new InvalidOperationException("CodeDomProvider cannot be null!");

            IFileCodeMerger merger = ((ILuaCodeDomProvider) (provider)).CreateFileCodeMerger(ProjectItem);
            result = merger != null;

            if (result)
            {
                try
                {
					if (useUndoManager)
						DTE.UndoContext.Open("Undo Code Merge", false);

                    var codeElements = new List<CodeElement>(elements);
                    //Reverses the order of the changed CodeElements because
                    //modification of beginning of line could change TextPoint location.
                    codeElements.Reverse();
                    result = false;
                    codeElements.ForEach<CodeElement>(element =>
                                                          {
                                                              var item = element as ICodeDomElement;
                                                              LexLocation location;
                                                              if (item != null && item.LuaASTTypeObject != null)
                                                              {
                                                                  if (element is LuaCodeVariableTable)
                                                                  {
                                                                      location =
                                                                          ((LuaCodeVariableTable) element).
                                                                              IdentifierLocation;
                                                                  }
                                                                  else
                                                                  {
                                                                      location = item.LuaASTTypeObject.Location;
                                                                  }
                                                                  //Perform merges.
                                                                  if (element is LuaCodeFunction)
                                                                  {
                                                                      result = merger.RenameFunction(location.sLin,
                                                                                                     location.sCol + 1,
                                                                                                     location.eLin,
                                                                                                     location.eCol + 1,
                                                                                                     oldName,
                                                                                                     element.Name);
                                                                  }
                                                                  else
                                                                  {
                                                                      result = merger.Replace(location.sLin,
                                                                                              location.sCol + 1,
                                                                                              location.eLin,
                                                                                              location.eCol + 1,
                                                                                              oldName, element.Name);
                                                                  }
                                                              }
                                                              result = result && true;
                                                          }
                        );
                    if (result) merger.Commit();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    throw;
                }
                finally
                {
					if (useUndoManager)
						DTE.UndoContext.Close();
                    modelInitialized = !result;
                }
            }
            return result;
        }

    }
}