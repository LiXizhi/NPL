using System;
using System.Runtime.InteropServices;
using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// An object defining a parameter to a function, property, and so on, in a Lua source file.
    /// </summary>
    [ComVisible(true)]
    public class LuaCodeParameter : LuaCodeElement<Identifier>, CodeParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeParameter"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="name"></param>
        /// <param name="parentElement"></param>
        /// <param name="node"></param>
        public LuaCodeParameter(DTE dte, string name, CodeElement parentElement, Node node)
            : base(dte, name, parentElement, node)
        {
        }

        /// <summary>
        /// Creates a new attribute code construct and inserts the code in the correct location.
        /// </summary>
        /// <param name="name">Required. The name of the new attribute.</param>
        /// <param name="Value">Required. The value of the attribute, which may be a comma-separated list of parameters for a parameterized property.</param>
        /// <param name="Position">Optional. Default = 0. The code element after which to add the new element. If the value is a <see cref="T:EnvDTE.CodeElement" />, then the new element is added immediately after it.If the value is a Long data type, then <see cref="M:EnvDTE.CodeParameter.AddAttribute(System.String,System.String,System.Object)" /> indicates the element after which to add the new element.Because collections begin their count at 1, passing 0 indicates that the new element should be placed at the beginning of the collection. A value of -1 means that the element should be placed at the end. </param>
        /// <returns>A <see cref="T:EnvDTE.CodeAttribute" /> object.</returns>
        public CodeAttribute AddAttribute(string name, string Value, object Position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the immediate parent object of a <see cref="T:EnvDTE.CodeVariable" /> object.
        /// </summary>
        public CodeElement Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets or sets an object representing the programmatic type.
        /// </summary>
        public CodeTypeRef Type { get; set; }


        /// <summary>
        /// Gets a collection of all attributes for the parent object.
        /// </summary>
        public CodeElements Attributes
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the document comment for the current code model element.
        /// </summary>
        public string DocComment
        {
            get { return String.Empty; }
            set { throw new NotImplementedException(); }
        }
    }
}