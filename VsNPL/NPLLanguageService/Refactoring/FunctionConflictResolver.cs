using System.Collections.Generic;
using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom;
using ParaEngine.Tools.Lua.CodeDom.Elements;

namespace ParaEngine.Tools.Lua.Refactoring
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class FunctionConflictResolver : ConflictResolverBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionConflictResolver"/> class.
        /// </summary>
        /// <param name="fileCodeModel">The LuaFileCodemodel.</param>
        public FunctionConflictResolver(LuaFileCodeModel fileCodeModel)
            : base(fileCodeModel)
        {
        }

		/// <summary>
		/// Determines whether this instance has conflict.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance has conflict; otherwise, <c>false</c>.
		/// </returns>
        public override bool HasConflict()
        {
            return CodeConflictType != ConflictType.None && CodeConflictType != ConflictType.Unknown; ;
        }

		/// <summary>
		/// Finds the conflicts.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="newName">The new name.</param>
		/// <returns></returns>
        public override IEnumerable<CodeElement> FindConflicts(CodeElement element, string newName)
        {
            CodeConflictType = ConflictType.None;
            var elements = new List<CodeElement>();
            var navigator = new LuaCodeDomNavigator(fileCodeModel);
            var results = navigator.WalkTopLevelMembers<LuaCodeFunction>();
            results.ForEach(item =>
                                {
                                    if (element != item && newName == item.Name)
                                    {
                                        elements.Add(item);
                                        CodeConflictType = ConflictType.Function;
                                    }
                                });

            return elements;
        }
    }
}