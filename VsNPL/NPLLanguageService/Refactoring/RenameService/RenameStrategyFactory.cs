using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.Refactoring.RenameService;

namespace ParaEngine.Tools.Lua.Refactoring.RenameService
{
    /// <summary>
    /// Create concrete implementation of IRenameStrategy
    /// </summary>
    internal static class RenameStrategyFactory
    {

        /// <summary>
        /// Creates IRenameStrategy by elementType.
        /// </summary>
        /// <param name="elementType">Type of CodeElement.</param>
        /// <returns>IRenameStrategy implementation.</returns>
        public static IRenameStrategy Create(vsCMElement elementType)
        {
            IRenameStrategy result = null;
            switch (elementType)
            {
                case vsCMElement.vsCMElementFunctionInvokeStmt:
                case vsCMElement.vsCMElementFunction: 
                    { result = new FunctionRenameStrategy(); break; }
                case vsCMElement.vsCMElementVariable:
                case vsCMElement.vsCMElementMap:
                case vsCMElement.vsCMElementLocalDeclStmt:
                case vsCMElement.vsCMElementDefineStmt:
                case vsCMElement.vsCMElementDeclareDecl: 
                    { result = new VariableRenameStrategy(); break; }
            }
            return result;
        }

        /// <summary>
        /// Creates IConflictResolver by elementType.
        /// </summary>
        /// <param name="elementType">Type of CodeElement.</param>
        /// <param name="fileCodeModel">LuaFileCodeModel instance.</param>
        /// <returns>IConflictResolver implementation.</returns>
        public static IConflictResolver CreateConflictResolver(vsCMElement elementType, LuaFileCodeModel fileCodeModel)
        {
            IConflictResolver result = null;
            switch (elementType)
            {
                case vsCMElement.vsCMElementFunctionInvokeStmt:
                case vsCMElement.vsCMElementFunction:
                    {
                        result = new FunctionConflictResolver(fileCodeModel);
                        break;
                    }
            }
            return result;
        }
    }
}