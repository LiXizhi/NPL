using System;
using System.Runtime.InteropServices;
using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// Represents a code element or construct in a Lua source file.
    /// </summary>
    /// <typeparam name="LuaASTNode">AST Node type.</typeparam>
    [ComVisible(true)]
    public class LuaCodeElement<LuaASTNode> : SimpleCodeElement, ICodeDomElement
        where LuaASTNode : Node
    {
        protected LuaASTNode astNode;
        protected LexLocation location;
        protected CodeElement parent;
        protected LuaCodeElements childObjects;
        protected vsCMElement vsCMElementType = vsCMElement.vsCMElementOther;


        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeElement{LuaASTNode}<LuaASTNode>"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="name"></param>
        /// <param name="astNode"></param>
        public LuaCodeElement(DTE dte, string name, Node astNode)
            : this(dte, name, null, astNode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeElement{LuaASTNode}<LuaASTNode>"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="name"></param>
        /// <param name="parentElement"></param>
        /// <param name="node"></param>
        public LuaCodeElement(DTE dte, string name, CodeElement parentElement, Node node)
            : base(dte, name)
        {
            parent = parentElement;

            astNode = (LuaASTNode) node;

            SetElementType(node);
        }

        #region Common protected helpers

        /// <summary>
        /// Commit changes made by code merging.
        /// </summary>
        protected void CommitChanges()
        {
            //Not used.
        }

        #endregion

        #region Protected helpers

        /// <summary>
        /// Set vsCMElement for CodeElement.
        /// </summary>
        /// <param name="node">AST Node object.</param>
        protected virtual void SetElementType(Node node)
        {
            if (node == null) return;
            if (node is Variable) vsCMElementType = vsCMElement.vsCMElementDeclareDecl;
            else if (node is FunctionDeclaration) vsCMElementType = vsCMElement.vsCMElementFunction;
            else if (node is FunctionCall || node is Return)
                vsCMElementType = vsCMElement.vsCMElementFunctionInvokeStmt;
            else if (node is LocalDeclaration) vsCMElementType = vsCMElement.vsCMElementLocalDeclStmt;
            else if (node is Break) vsCMElementType = vsCMElement.vsCMElementOptionStmt;
            else if (node is Identifier || node is Literal)
                vsCMElementType = vsCMElement.vsCMElementDefineStmt;
            else if (node is If || node is ElseIfBlock || node is ThenBlock
                     || node is Loop || node is ForLoop || node is WhileLoop ||
                     node is RepeatUntilLoop)
                vsCMElementType = vsCMElement.vsCMElementImplementsStmt;
            else if (node is Chunk) vsCMElementType = vsCMElement.vsCMElementClass;
        }

        #endregion

        #region Overriden Members

        /// <summary>
        /// Gets the text point that is the location of the end of the code item.
        /// </summary>
        public override TextPoint EndPoint
        {
            get
            {
                if (null == ProjectItem)
                {
                    return null;
                }
                if (null == ProjectItem.Document)
                {
                    ProjectItem.Open(Guid.Empty.ToString("B"));
                }
                return new LuaTextPoint(
                    (TextDocument)ProjectItem.Document.Object("TextDocument"),
                    astNode.Location.eCol, astNode.Location.eLin);
            }
        }

        /// <summary>
        /// Gets a text point object that defines the beginning of the code item.
        /// </summary>
        public override TextPoint StartPoint
        {
            get
            {
                return new LuaTextPoint(
                    (TextDocument)ProjectItem.Document.Object("TextDocument"),
                    astNode.Location.sCol, astNode.Location.sLin);
            }
        }

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

        /// <summary>
        /// Returns a collection of objects contained within this <see cref="T:EnvDTE.CodeElement" />.
        /// </summary>
        public override CodeElements Children
        {
            get { return childObjects; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.CodeElements" /> collection containing the CodeElement 
        /// that supports this property.
        /// </summary>
        public override CodeElements Collection
        {
            get { return null; }
        }

        /// <summary>
        /// Gets an enumeration that defines the type of object.
        /// </summary>
        public override vsCMElement Kind
        {
            get { return vsCMElementType; }
        }

        /// <summary>
        /// Gets a fully-qualified name of the specified code element.
        /// </summary>
        public override string FullName
        {
            get { return Name; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.ProjectItem" /> object associated 
        /// with the <see cref="T:EnvDTE.CodeElement" /> object.
        /// </summary>
        public override ProjectItem ProjectItem
        {
            get { return parent.ProjectItem; }
        }

        #endregion

        #region ICodeDomElement Members

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public virtual CodeElement ParentElement
        {
            get { return parent; }
        }

        /// <summary>
        /// AST object of CodeElement.
        /// </summary>
        Node ICodeDomElement.LuaASTTypeObject
        {
            get { return astNode; }
        }

        /// <summary>
        /// Add child object to CodeElement.
        /// </summary>
        /// <param name="child">The new CodeElement.</param>
        void ICodeDomElement.AddChildObject(CodeElement child)
        {
            if (childObjects == null)
            {
                childObjects = new LuaCodeElements(DTE, parent);
            }
            childObjects.AddElement(child);
        }

        #endregion


        /// <summary>
        /// Gets/Sets associated AST Node object with CodeElement.
        /// </summary>
        public LuaASTNode LuaASTNodeObject
        {
            get { return astNode; }
            set { astNode = value; }
        }

    }
}