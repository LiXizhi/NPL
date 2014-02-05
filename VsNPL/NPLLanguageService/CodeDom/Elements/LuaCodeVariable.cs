using System;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// An object defining a variable construct in a Lua source file.
    /// </summary>
    [ComVisible(true)]
    public class LuaCodeVariable : LuaCodeElement<Variable>, CodeVariable2, IDeclarationCodeElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeVariable"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="parentElement"></param>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="access"></param>
        /// <param name="variable"></param>
        public LuaCodeVariable(DTE dte, CodeElement parentElement, string name,
                               CodeTypeRef type, vsCMAccess access, Variable variable)
            : base(dte, name, variable)
        {
            parent = parentElement;
            Type = ObjectToTypeRef(type);
            Access = access;
        }

        #region CodeVariable Members

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
            get { return parent.Children; }
        }

        /// <summary>
        /// Gets the <see cref="T:EnvDTE.ProjectItem" /> object associated with the 
        /// <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public override ProjectItem ProjectItem
        {
            get { return parent.ProjectItem; }
        }

        /// <summary>
        /// Creates a new attribute code construct and inserts the code in the correct location.
        /// </summary>
        /// <returns>A <see cref="T:EnvDTE.CodeAttribute" /> object.</returns>
        /// <param name="attributeName">Required. The name of the new attribute.</param>
        /// <param name="Value">Required. The value of the attribute, which may be a list of parameters for a parameterized property, separated by commas .</param>
        /// <param name="Position">Optional. Default = 0. The code element after which to add the new element. If the value is a <see cref="T:EnvDTE.CodeElement" />, then the new element is added immediately after it.If the value is a Long data type, then <see cref="M:EnvDTE80.CodeVariable2.AddAttribute(System.String,System.String,System.Object)" /> indicates the element after which to add the new element.Because collections begin their count at 1, passing 0 indicates that the new element should be placed at the beginning of the collection. A value of -1 means the element should be placed at the end.</param>
        public CodeAttribute AddAttribute(string attributeName, string Value, object Position)
        {
            throw new NotSupportedException("Lua does not support this feature.");
        }

        /// <summary>
        /// Gets a collection of all attributes for the parent object.
        /// </summary>
        public CodeElements Attributes
        {
            get { return null; }
        }

        /// <summary>
        /// Sets or gets the access attributes of this item.
        /// </summary>
        public vsCMAccess Access { get; set; }

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
        /// Gets or sets an object defining the initialization code for an element.
        /// </summary>
        public object InitExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the item is a constant.
        /// </summary>
        public bool IsConstant
        {
            get { return false; }
            set { throw new NotSupportedException("Lua does not support this feature."); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the item is statically defined, 
        /// that is, if the item is common to all instances of this object type or only to 
        /// this object specifically.
        /// </summary>
        public bool IsShared
        {
            get { return false; }
            set { throw new NotSupportedException("Lua does not support this feature."); }
        }

        #region CodeVariable2 Members

        /// <summary>
        /// Sets or gets when the variable is eligible to be changed.
        /// </summary>
        public vsCMConstKind ConstKind
        {
            get { return vsCMConstKind.vsCMConstKindNone; }
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

        /// <summary>
        /// Gets or sets an object representing the programmatic type.
        /// </summary>
        public CodeTypeRef Type { get; set; }

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public object Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets Prototype of CodeElement
        /// </summary>
        /// <param name="flags">vsCMPrototype flag.</param>
        /// <returns></returns>
        public virtual string get_Prototype(int flags)
        {
            switch ((vsCMPrototype)flags)
            {
                case vsCMPrototype.vsCMPrototypeFullname:
                    return Name;
                default:
                    return String.Empty;
            }
        }

        #endregion

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public override CodeElement ParentElement
        {
            get { return parent; }
        }


		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
        public override string ToString()
        {
            return String.Format("LuaCodeVariable [Name: {0}, Value: {1}]", Name, InitExpression);
        }
    }
}