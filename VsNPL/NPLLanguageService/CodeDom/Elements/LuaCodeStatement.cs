using System.Runtime.InteropServices;
using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// Represents a statement in Lua source file.
    /// </summary>
    [ComVisible(true)]
    public sealed class LuaCodeStatement : LuaCodeElement<Node>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeStatement"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="name"></param>
        /// <param name="astNode"></param>
        public LuaCodeStatement(DTE dte, string name, Node astNode)
            : this(dte, name, null, astNode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaCodeStatement"/> class.
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="astNode"></param>
        public LuaCodeStatement(DTE dte, string name, CodeElement parent, Node astNode)
            : base(dte, name, parent, astNode)
        {
            childObjects = new LuaCodeElements(DTE, ParentElement);
        }

        /// <summary>
        /// Holds operator value from Expression.
        /// </summary>
        public object UserData { get; set; }

        /// <summary>
        /// Gets child elements in Statements.
        /// </summary>
        public LuaCodeElements Statements
        {
            get
            {
                return childObjects;
            }
        }

        /// <summary>
        /// Gets an enumeration that defines the type of object.
        /// </summary>
        public override vsCMElement Kind
        {
            get { return vsCMElement.vsCMElementDefineStmt; }
        }
    }
}