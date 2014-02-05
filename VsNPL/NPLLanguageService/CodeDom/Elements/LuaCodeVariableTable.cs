using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.Parser;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// Represents a table in Lua code file.
    /// </summary>
    [ComVisible(true)]
    public sealed class LuaCodeVariableTable : LuaCodeVariable, ICodeDomElement
    {
        private new TableConstructor astNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeVariableTable"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="parentElement"></param>
        /// <param name="name"></param>
        /// <param name="access"></param>
        /// <param name="variable"></param>
        public LuaCodeVariableTable(DTE dte, CodeElement parentElement, string name, vsCMAccess access,
                                    TableConstructor variable)
            : base(dte, parentElement, name, new LuaCodeTypeRef(
                                                 dte, LuaDeclaredType.Table), access,
                   new Variable(variable.Location) {Identifier = new Identifier(variable.Location)})
        {
            childObjects = new LuaCodeElements(dte, this);
            astNode = variable;
            parent = parentElement;
        }

        /// <summary>
        /// Adds fields to the table object.
        /// </summary>
        /// <param name="variable">Field variable.</param>
        public void AddInitVariable(LuaCodeVariable variable)
        {
            childObjects.AddElement(variable);
        }

        /// <summary>
        ///  Adds fields to the table object.
        /// </summary>
        /// <param name="variableName">Field variable name.</param>
        /// <param name="type">Field variable type.</param>
        /// <param name="variable">Optional. Field variable Node object.</param>
        /// <returns></returns>
        public CodeVariable AddInitVariable(string variableName, LuaType type, Variable variable)
        {
            var result = new LuaCodeVariable(DTE, parent, variableName,
                                             new LuaCodeTypeRef(DTE, LuaDeclaredType.Find(type.ToString())),
                                             vsCMAccess.vsCMAccessPublic, variable);
            AddInitVariable(result);
            return result;
        }

        /// <summary>
        /// Gets or sets the location of table identifier.
        /// </summary>
        public LexLocation IdentifierLocation { get; set; }

        #region ICodeDomElement Members

        /// <summary>
        /// Parent of CodeElement.
        /// </summary>
        CodeElement ICodeDomElement.ParentElement
        {
            get { return ParentElement; }
        }

        /// <summary>
        /// Add child object to CodeElement.
        /// </summary>
        /// <param name="child">The new CodeElement.</param>
        void ICodeDomElement.AddChildObject(CodeElement child)
        {
            childObjects.Add(child);
        }

        /// <summary>
        /// AST object of CodeElement.
        /// </summary>
        Node ICodeDomElement.LuaASTTypeObject
        {
            get { return astNode; }
        }

        #endregion

        #region Overriden Members

        /// <summary>
        /// Gets an enumeration that defines the type of object.
        /// </summary>
        public override vsCMElement Kind
        {
            get { return vsCMElement.vsCMElementMap; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("LuaCodeVariableTable [Name: {0}]", Name);
        }

        /// <summary>
        /// Returns a collection of objects contained within this <see cref="T:EnvDTE.CodeElement" />.
        /// </summary>
        public override CodeElements Children
        {
            get { return childObjects; }
        }

        /// <summary>
        /// Gets/Sets associated AST Node object with CodeElement.
        /// </summary>
        public new TableConstructor LuaASTNodeObject
        {
            get { return astNode; }
            set { astNode = value; }
        }

        /// <summary>
        /// Gets Prototype of CodeElement.
        /// </summary>
        /// <param name="flags">vsCMPrototype flag.</param>
        /// <returns></returns>
        public override string get_Prototype(int flags)
        {
            if (((int) vsCMPrototype.vsCMPrototypeType | (int) vsCMPrototype.vsCMPrototypeParamNames |
                 (int) vsCMPrototype.vsCMPrototypeParamTypes) == flags)
            {
                return GetProtoTypeName();
            }
            switch ((vsCMPrototype) flags)
            {
                case vsCMPrototype.vsCMPrototypeParamTypes:
                    return String.Concat(Name, "{}");
                case vsCMPrototype.vsCMPrototypeFullname:
                    return Name;
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Create unique table name with initializers.
        /// </summary>
        /// <returns></returns>
        private string GetProtoTypeName()
        {
            var stringBuilder = new StringBuilder();
            try
            {
                if (childObjects != null)
                {
                    stringBuilder.Append(Name);
                    stringBuilder.Append("{");
                    childObjects.ForEach(child =>
                                             {
                                                 if (!String.IsNullOrEmpty(child.Name))
                                                     stringBuilder.Append(child.Name);
                                                 else
                                                     stringBuilder.Append("#");
                                                 stringBuilder.Append(" ,");
                                             });
                    stringBuilder.Remove(stringBuilder.Length - 2, 2);
                    stringBuilder.Append("}");
                    return stringBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            return String.Concat(Name, "{}");
        }

        /// <summary>
        /// Gets a text point object that defines the beginning of the code item.
        /// </summary>
        public override TextPoint StartPoint
        {
            get
            {
                TextPoint textPoint = base.StartPoint;
                if (IdentifierLocation != null)
                {
                    textPoint = new LuaTextPoint(textPoint.Parent,
                        IdentifierLocation.sCol, IdentifierLocation.sLin);
                }
                return textPoint;
            }
        }

        /// <summary>
        /// Gets the text point that is the location of the end of the code item.
        /// </summary>
        public override TextPoint EndPoint
        {
            get
            {
                TextPoint textPoint = base.EndPoint;
                if (IdentifierLocation != null)
                {
                    textPoint =  new LuaTextPoint(textPoint.Parent,
                        IdentifierLocation.eCol, IdentifierLocation.eLin);
                }
                return textPoint;
            }
        }

        #endregion
    }
}