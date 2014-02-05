using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.CodeDom.Elements
{
    /// <summary>
    /// Creates various LuaCodeElements.
    /// </summary>
    public static class LuaCodeElementFactory
    {

		/// <summary>
		/// Creates the variable.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="access">The access.</param>
		/// <param name="variable">The variable.</param>
		/// <returns></returns>
        public static LuaCodeVariable CreateVariable(
            DTE dte, CodeElement parent, string name,
            LuaType type, vsCMAccess access, Variable variable)
        {
            var result = new LuaCodeVariable(dte, parent, name,
                                             new LuaCodeTypeRef(dte, LuaDeclaredType.Find(type.ToString())),
                                             access, variable);
            return result;
        }

		/// <summary>
		/// Creates the variable.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="isLocal">if set to <c>true</c> [is local].</param>
		/// <param name="variable">The variable.</param>
		/// <returns></returns>
        public static LuaCodeVariable CreateVariable(
            DTE dte, CodeElement parent, string name,
            LuaType type, bool isLocal, Variable variable)
        {
            return CreateVariable(dte, parent, name,
                                  type, isLocal ? vsCMAccess.vsCMAccessPrivate : vsCMAccess.vsCMAccessProject, variable);
        }

		/// <summary>
		/// Creates the lua code variable table.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="access">The access.</param>
		/// <param name="variable">The variable.</param>
		/// <returns></returns>
        public static LuaCodeVariable CreateLuaCodeVariableTable(
            DTE dte, CodeElement parent, string name,
            LuaType type, vsCMAccess access, TableConstructor variable)
        {
            var result = new LuaCodeVariableTable(dte, parent, name,
                                                  access, variable);
            return result;
        }

		/// <summary>
		/// Creates the lua code variable table.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="isLocal">if set to <c>true</c> [is local].</param>
		/// <param name="variable">The variable.</param>
		/// <returns></returns>
        public static LuaCodeVariable CreateLuaCodeVariableTable(
            DTE dte, CodeElement parent, string name,
            LuaType type, bool isLocal, TableConstructor variable)
        {
            return CreateLuaCodeVariableTable(dte, parent, name,
                                              type,
                                              isLocal ? vsCMAccess.vsCMAccessPrivate : vsCMAccess.vsCMAccessProject,
                                              variable);
        }

		/// <summary>
		/// Creates the lua code element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dte">The DTE.</param>
		/// <param name="name">The name.</param>
		/// <param name="node">The node.</param>
		/// <returns></returns>
        public static LuaCodeElement<T> CreateLuaCodeElement<T>(
            DTE dte, string name, Node node) where T : Node
        {
            LuaCodeElement<T> element = CreateLuaCodeElement<T>(dte, name, null, node);
            return element;
        }

		/// <summary>
		/// Creates the lua code element.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dte">The DTE.</param>
		/// <param name="name">The name.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="node">The node.</param>
		/// <returns></returns>
        public static LuaCodeElement<T> CreateLuaCodeElement<T>(
            DTE dte, string name, CodeElement parent, Node node) where T : Node
        {
            var element = new LuaCodeElement<T>(dte, name, parent, node);
            return element;
        }

		/// <summary>
		/// Creates the lua code statement.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="name">The name.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="node">The node.</param>
		/// <returns></returns>
        public static LuaCodeStatement CreateLuaCodeStatement(DTE dte, string name, CodeElement parent, Node node)
        {
            var element = new LuaCodeStatement(dte, name, parent, node);
            return element;
        }

		/// <summary>
		/// Creates the lua code function.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="access">The access.</param>
		/// <param name="function">The function.</param>
		/// <returns></returns>
        public static LuaCodeFunction CreateLuaCodeFunction(
            DTE dte, CodeElement parent, string name, LuaType returnType, vsCMAccess access,
            FunctionDeclaration function)
        {
            return CreateLuaCodeFunction(dte, parent, name, returnType, access, vsCMFunction.vsCMFunctionFunction,
                                         function);
        }

		/// <summary>
		/// Creates the lua code function.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="isLocal">if set to <c>true</c> [is local].</param>
		/// <param name="function">The function.</param>
		/// <returns></returns>
        public static LuaCodeFunction CreateLuaCodeFunction(
            DTE dte, CodeElement parent, string name, LuaType returnType, bool isLocal, FunctionDeclaration function)
        {
            return CreateLuaCodeFunction(dte, parent, name, returnType,
                                         isLocal ? vsCMAccess.vsCMAccessPrivate : vsCMAccess.vsCMAccessProject,
                                         vsCMFunction.vsCMFunctionFunction, function);
        }

		/// <summary>
		/// Creates the lua code function.
		/// </summary>
		/// <param name="dte">The DTE.</param>
		/// <param name="parent">The parent.</param>
		/// <param name="name">The name.</param>
		/// <param name="returnType">Type of the return.</param>
		/// <param name="access">The access.</param>
		/// <param name="kind">The kind.</param>
		/// <param name="function">The function.</param>
		/// <returns></returns>
        public static LuaCodeFunction CreateLuaCodeFunction(
            DTE dte, CodeElement parent, string name, LuaType returnType, vsCMAccess access, vsCMFunction kind, FunctionDeclaration function)
        {
            var result = new LuaCodeFunction(dte, parent, name, kind,
                                             new LuaCodeTypeRef(dte, LuaDeclaredType.Find(returnType.ToString())),
                                             access, function);
            return result;
        }
    }
}