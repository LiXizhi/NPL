using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EnvDTE;
using ParaEngine.Tools.Lua.AST;
using ParaEngine.Tools.Lua.CodeDom.Definitions;
using ParaEngine.Tools.Lua.CodeDom.Elements;
using ParaEngine.Tools.Lua.Refactoring;
using ParaEngine.Tools.Lua.Refactoring.RenameService;
using ParaEngine.NPLLanguageService;

namespace ParaEngine.Tools.Lua
{
    /// <summary>
    /// 
    /// </summary>
    [Guid(GuidStrings.LuaRefactoringService)]
    public class LuaRefactoringService : IRefactoringService
    {
        private readonly IServiceContainer container;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaRefactoringService"/> class.
        /// </summary>
        /// <param name="container">ServiceContainer instance.</param>
        public LuaRefactoringService(IServiceContainer container)
        {
            this.container = container;
        }

        /// <summary>
        /// Gets ServiceContainer instance.
        /// </summary>
        public IServiceContainer ServiceContainer
        {
            get { return container; }
        }

        #region IRefactoringService Members

        /// <summary>
        /// Rename element in scope of parentElement.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="parentElement">Containing element.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <param name="newName">New name of element.</param>
        public IRenameResult Rename(CodeElement element, CodeElement parentElement, vsCMElement elementType,
                                    string oldName, string newName)
        {
            var renameContext = new CodeElementRenameContext
                (RenameStrategyFactory.Create(elementType));
            IRenameResult result = renameContext.CodeElementRename
                .RenameSymbols(element, (LuaCodeClass) parentElement, elementType, oldName, newName);
            return result;
        }

        #endregion

        /// <summary>
        /// Check renaming element.
        /// </summary>
        /// <param name="element">Element to rename.</param>
        /// <param name="elementType">Type of element.</param>
        /// <param name="oldName">Old name of element.</param>
        /// <returns>True if success, otherwise False</returns>
        public bool CanRenameSymbol(CodeElement element, vsCMElement elementType, string oldName)
        {
            var codeDomElement = element as ICodeDomElement;
            bool result = true;

            if (codeDomElement != null)
            {
                if (elementType == vsCMElement.vsCMElementImplementsStmt ||
                    elementType == vsCMElement.vsCMElementOptionStmt)
                    result = false;
                else if (codeDomElement.LuaASTTypeObject is Variable &&
                         ((ICodeDomElement) codeDomElement.ParentElement).LuaASTTypeObject is TableConstructor)
                    result = false;
                else if (elementType == vsCMElement.vsCMElementFunctionInvokeStmt &&
                         codeDomElement.LuaASTTypeObject is Break)
                    result = false;
                else if (elementType == vsCMElement.vsCMElementDefineStmt &&
                         codeDomElement.LuaASTTypeObject is Literal)
                    result = false;
                else if (codeDomElement.LuaASTTypeObject is Literal)
                    result = false;
                else if (element is LuaCodeStatement)
                    result = false;
            }
            //Check Lua reserved keywords
            if (result && (elementType == vsCMElement.vsCMElementFunctionInvokeStmt
                || elementType == vsCMElement.vsCMElementDeclareDecl
                || elementType == vsCMElement.vsCMElementDefineStmt))
            {
                result = !reservedKeyWords.Contains(element.Name);
				Debug.WriteLine(result ? Resources.RenameAllowedMessage : Resources.SymbolReservedMessage); 
            }
            return result;
        }

        #region ReservedKeyWords

        /// <summary>
        /// Contains Lua reserved keywords
        /// </summary>
        private readonly List<string> reservedKeyWords = new List<string>(
            new[]
                {
                    "_G", "coroutine.resume", "io.open", "math.pow",
                    "_VERSION", "coroutine.running", "io.output", "math.rad",
                    "assert", "coroutine.status", "io.popen", "math.random",
                    "collectgarbage", "coroutine.wrap", "io.read", "math.randomseed",
                    "dofile", "coroutine.yield", "io.stderr", "math.sin",
                    "error", "debug.debug", "io.stdin", "math.sinh",
                    "getfenv", "debug.getfenv", "io.stdout", "math.sqrt",
                    "getmetatable", "debug.gethook", "io.tmpfile", "math.tan",
                    "ipairs", "debug.getinfo", "io.type", "math.tanh",
                    "load", "debug.getlocal", "io.write", "os.clock",
                    "loadfile", "debug.getmetatable", "math.abs", "os.date",
                    "loadstring", "debug.getregistry", "math.acos", "os.difftime",
                    "module", "debug.getupvalue", "math.asin", "os.execute",
                    "next", "debug.setfenv", "math.atan", "os.exit",
                    "pairs", "debug.sethook", "math.atan2", "os.getenv",
                    "pcall", "debug.setlocal", "math.ceil", "os.remove",
                    "print", "debug.setmetatable", "math.cos", "os.rename",
                    "rawequal", "debug.setupvalue", "math.cosh", "os.setlocale",
                    "rawget", "debug.traceback", "math.deg", "os.time",
                    "rawset", "file:close", "math.exp", "os.tmpname",
                    "require", "file:flush", "math.floor", "package.cpath",
                    "select", "file:lines", "math.fmod", "package.loaded",
                    "setfenv", "file:read", "math.frexp", "package.loaders",
                    "setmetatable", "file:seek", "math.huge", "package.loadlib",
                    "tonumber", "file:setvbuf", "math.ldexp", "package.path",
                    "tostring", "file:write", "math.log", "package.preload",
                    "type", "io.close", "math.log10", "package.seeall",
                    "unpack", "io.flush", "math.max", "string.byte",
                    "xpcall", "io.input", "math.min", "string.char",
                    "coroutine.create", "io.lines", "math.modf", "string.dump",
                    "math.pi", "string.find", "string.format", "string.gmatch", "string.gsub", "string.len",
                    "string.lower", "string.match", "string.rep", "string.reverse", "string.sub",
                    "string.upper", "table.concat", "table.insert", "table.maxn", "table.remove",
                    "table.sort",
                });

        #endregion
    }
}