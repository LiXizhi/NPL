using System;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using EnvDTE80;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
	/// <summary>
	/// An object defining a function construct in a Lua source file.
	/// </summary>
    [ComVisible(true)]
    public class LuaCodeFunction : LuaCodeElement<FunctionDeclaration>, CodeFunction2, IDeclarationCodeElement
    {
        private readonly LuaCodeElements parameters;

		/// <summary>
		/// Initializes a new instance of the <see cref="LuaCodeFunction"/> class.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parentElement">The parent element.</param>
		/// <param name="name">The name.</param>
		/// <param name="kind">The kind.</param>
		/// <param name="codeType">Type of the code.</param>
		/// <param name="access">The access.</param>
		/// <param name="function">The function.</param>
        public LuaCodeFunction(DTE dte, CodeElement parentElement, string name, vsCMFunction kind,
                               object codeType, vsCMAccess access, FunctionDeclaration function)
            : base(dte, name, function)
        {
            parent = parentElement;
            FunctionBody = new LuaCodeElements(dte, this);
            parameters = new LuaCodeElements(dte, this);
            FunctionType = kind;
            Access = access;
            Type = ObjectToTypeRef(codeType);
        }


        /// <summary>
        /// Gets LuaCodeElements in body of function.
        /// </summary>
        public LuaCodeElements FunctionBody { get; private set; }

        /// <summary>
        /// Create unique function name with parameters.
        /// </summary>
        /// <param name="includeParamTypes"></param>
        /// <returns></returns>
        private string GetProtoTypeName(bool includeParamTypes)
        {
            var stringBuilder = new StringBuilder();
            if (parameters.Count > 0)
            {
                stringBuilder.Append(Name);
                stringBuilder.Append("(");
                parameters.ForEach(param =>
                                       {
                                           if (includeParamTypes)
                                               if (((LuaCodeParameter) param).Type != null)
                                                   stringBuilder.Append(
                                                       String.Concat(((LuaCodeParameter) param).Type.AsFullName, " "));
                                           stringBuilder.Append(param.Name);
                                           stringBuilder.Append(" ,");
                                       });
                stringBuilder.Remove(stringBuilder.Length - 2, 2);
                stringBuilder.Append(")");
            }
            else
            {
                stringBuilder.Append(String.Format("{0}()", Name));
            }

            return stringBuilder.ToString();
        }

        #region CodeFunction Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName">Required. The name of the parameter.</param>
        /// <param name="type">Required. Identifier from AST ParameterList</param>
        /// <param name="position">Optional. Default = 0. The code element after which to add the new element. If the value is a CodeElement, then the new element is added immediately after it.
        /// 
        /// If the value is a Long, then AddParameter indicates the element after which to add the new element.
        /// 
        /// Because collections begin their count at 1, passing 0 indicates that the new element should be placed at the beginning of the collection. A value of -1 means the element should be placed at the end. 
        /// </param>
        /// <returns>A CodeParameter object. </returns>
        public CodeParameter AddParameter(string parameterName, object type, object position)
        {
            var parameter = new LuaCodeParameter(DTE, parameterName, this, (Identifier) type);
            CommitChanges();
            parameters.AddElement(parameter);
            return null;
        }

        /// <summary>
        /// Gets a collection of parameters for this item.
        /// </summary>
        public CodeElements Parameters
        {
            get { return parameters; }
        }

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeFunction" /> object.
        /// </summary>
        public object Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Sets or gets the access modifier of this item.
        /// </summary>
        public vsCMAccess Access { get; set; }

        /// <summary>
        /// Removes a parameter from the argument list.
        /// </summary>
        /// <param name="element">Required. A CodeElement object or the name of one in the collection.</param>
        public void RemoveParameter(object element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            parameters.RemoveElement(element as CodeElement);
        }

        /// <summary>
        /// Sets or gets an object representing the programmatic type.
        /// </summary>
        public CodeTypeRef Type { get; set; }


        /// <summary>
        /// Gets Prototype of CodeElement
        /// </summary>
        /// <param name="flags">vsCMPrototype flag.</param>
        /// <returns></returns>
        public string get_Prototype(int flags)
        {
            if (((int) vsCMPrototype.vsCMPrototypeType | (int) vsCMPrototype.vsCMPrototypeParamNames |
                 (int) vsCMPrototype.vsCMPrototypeParamTypes) == flags)
            {
                return GetProtoTypeName(true);
            }
            switch ((vsCMPrototype) flags)
            {
                case vsCMPrototype.vsCMPrototypeFullname:
                    return Name;
                case vsCMPrototype.vsCMPrototypeParamTypes:
                    return GetProtoTypeName(false);
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the comment associated with the code element.
        /// </summary>
        public string Comment
        {
            get { return String.Empty; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the document comment for the current code model element.
        /// </summary>
        public string DocComment
        {
            get { return String.Empty; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets an enumeration describing how a function is used.
        /// </summary>
        public vsCMFunction FunctionType { get; set; }

        /// <summary>
        /// Gets an enumeration describing how a function is used.
        /// </summary>
        public vsCMFunction FunctionKind
        {
            get { return FunctionType; }
        }

        #region Not supported CodeFunction Members

		/// <summary>
		/// Adds the attribute.
		/// </summary>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="Value">The value.</param>
		/// <param name="Position">The position.</param>
		/// <returns></returns>
        public CodeAttribute AddAttribute(string attributeName, string Value, object Position)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

		/// <summary>
		/// Gets or sets the attributes.
		/// </summary>
		/// <value>The attributes.</value>
        public CodeElements Attributes
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
            private set { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets a value indicating whether this instance is overloaded.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is overloaded; otherwise, <c>false</c>.
		/// </value>
        public bool IsOverloaded
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets the overloads.
		/// </summary>
		/// <value>The overloads.</value>
        public CodeElements Overloads
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets or sets a value indicating whether this instance can override.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can override; otherwise, <c>false</c>.
		/// </value>
        public bool CanOverride
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
            set { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets or sets a value indicating whether [must implement].
		/// </summary>
		/// <value><c>true</c> if [must implement]; otherwise, <c>false</c>.</value>
        public bool MustImplement
        {
            get { throw new NotSupportedException("Lua does not support this feature."); }
            set { throw new NotSupportedException("Lua does not support this feature."); }
        }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is shared.
		/// </summary>
		/// <value><c>true</c> if this instance is shared; otherwise, <c>false</c>.</value>
        public bool IsShared
        {
            get { return false; }
            set { throw new NotSupportedException("Lua does not support this feature."); }
        }

        #endregion

        #region CodeFunction2 Members

        /// <summary>
        /// Sets or gets whether a CodeFunction object represents a parent class function 
        /// that may be overridden, a child class function that is replacing the inherited behavior,
        ///  or whether the function cannot be overridden.
        /// </summary>
        public vsCMOverrideKind OverrideKind
        {
            get { return vsCMOverrideKind.vsCMOverrideKindNone; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the current class is a generic.
        /// </summary>
        public bool IsGeneric
        {
            get { return false; }
        }

        #endregion

        #endregion

        #region Overriden Memebers

        /// <summary>
        /// Gets a text point object that defines the beginning of the code item.
        /// </summary>
        public override TextPoint StartPoint
        {
            get
            {
                TextDocument document = null;
                if (null != ProjectItem)
                {
                    if (null == ProjectItem.Document)
                    {
                        ProjectItem.Open(Guid.Empty.ToString("B"));
                    }
                    document = (TextDocument) ProjectItem.Document.Object("TextDocument");
                }
                // functions always start at 1 so people inserting new lines
                return new LuaTextPoint(document, 1, astNode.Location.sLin);
            }
        }

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public override CodeElement ParentElement
        {
            get { return parent; }
        }

        /// <summary>
        /// Returns a collection of objects contained within this <see cref="T:EnvDTE.CodeElement" />.
        /// </summary>
        public override CodeElements Children
        {
            get { return FunctionBody; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.CodeElements" /> collection containing the CodeElement 
        /// that supports this property.
        /// </summary>
        public override CodeElements Collection
        {
            get { return parent.Children; }
        }

        /// <summary>
        /// Gets an enumeration that defines the type of object.
        /// </summary>
        public override vsCMElement Kind
        {
            get { return vsCMElementType; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.ProjectItem" /> object associated 
        /// with the <see cref="T:EnvDTE.CodeElement" /> object.
        /// </summary>
        public override ProjectItem ProjectItem
        {
            get { return parent.ProjectItem; }
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="T:EnvDTE.CodeType" /> object can be 
        /// obtained from this object.
        /// </summary>
        public override bool IsCodeType
        {
            get { return false; }
        }

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
        public override string ToString()
        {
            return String.Format("LuaCodeFunction: {0}", Name);
        }

        
        #endregion
    }
}