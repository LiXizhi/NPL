using System;
using System.CodeDom.Compiler;
using EnvDTE;

namespace ParaEngine.Tools.Lua.CodeDom
{
	/// <summary>
	/// 
	/// </summary>
    public static class LuaCodeModelFactory
    {
        /// <summary>
        /// Creates FileCodeModel instance.
        /// </summary>
        /// <param name="item">The ProjectItem.</param>
        /// <param name="provider">The CodeDomProvider implementation.</param>
        /// <param name="fileName">The fileName.</param>
        /// <returns></returns>
        public static FileCodeModel CreateFileCodeModel
            (ProjectItem item, CodeDomProvider provider, string fileName)
        {
            if (null == item)
                throw new ArgumentNullException("item");

            return CreateFileCodeModel(item.DTE, item, provider, fileName);
        }


        /// <summary>
        /// Creates FileCodeModel instance.
        /// </summary>
        /// <param name="dte">The DTE instance.</param>
        /// <param name="item">The ProjectItem.</param>
        /// <param name="provider">The CodeDomProvider implementation.</param>
        /// <param name="fileName">The fileName.</param>
        /// <returns></returns>
        public static FileCodeModel CreateFileCodeModel
            (DTE dte, ProjectItem item, CodeDomProvider provider, string fileName)
        {
            if (null == item)
                throw new ArgumentNullException("item");

            return new LuaFileCodeModel(dte, item, provider, fileName);
        }

        /// <summary>
        /// Creates FileCodeModel instance.
        /// </summary>
        /// <param name="project">The Wow Project.</param>
        /// <returns></returns>
        public static CodeModel CreateProjectCodeModel(Project project)
        {
            return new LuaProjectCodeModel(project);
        }
    }
}