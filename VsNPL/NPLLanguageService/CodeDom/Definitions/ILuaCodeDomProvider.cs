using EnvDTE;
using ParaEngine.Tools.Lua.CodeDom.Definitions;

namespace ParaEngine.Tools.Lua.CodeDom.Definitions
{
    /// <summary>
    /// Defines interface for LuaCodeDomProvider.
    /// </summary>
    public interface ILuaCodeDomProvider
    {
        /// <summary>
        /// Gets the top-level extensibility object.
        /// </summary>
        DTE DTE { get; }

        /// <summary>
        /// Create FileCodeMerger for ProjectItem.
        /// </summary>
        /// <returns></returns>
        IFileCodeMerger CreateFileCodeMerger();

		/// <summary>
		/// Creates the file code merger.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
    	IFileCodeMerger CreateFileCodeMerger(ProjectItem item);
    }
}