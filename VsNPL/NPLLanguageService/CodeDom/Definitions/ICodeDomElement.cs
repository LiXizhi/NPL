using EnvDTE;
using ParaEngine.Tools.Lua.AST;

namespace ParaEngine.Tools.Lua.CodeDom.Definitions
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICodeDomElement
    {
        /// <summary>
        /// Parent of CodeElement.
        /// </summary>
        CodeElement ParentElement { get; }

        /// <summary>
        /// AST object of CodeElement.
        /// </summary>
        Node LuaASTTypeObject { get; }

        /// <summary>
        /// Add child object to CodeElement.
        /// </summary>
        /// <param name="child">The new CodeElement.</param>
        void AddChildObject(CodeElement child);
    }
}