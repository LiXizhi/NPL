using System;
using EnvDTE;
using Microsoft.VisualStudio.Designer.Interfaces;
using ParaEngine.Tools.Lua.CodeDom.Providers;

namespace ParaEngine.Tools.Lua.CodeDom.Providers
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class VSMDLuaCodeDomProvider : IVSMDCodeDomProvider, IDisposable
    {
        private LuaCodeDomProvider codeDomProvider;
    	private readonly IServiceProvider serviceProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="VSMDLuaCodeDomProvider"/> class.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
    	public VSMDLuaCodeDomProvider(IServiceProvider serviceProvider)
    	{
    		this.serviceProvider = serviceProvider;
    	}

    	/// <summary>
        /// Initializes a new instance of the <see cref="VSMDLuaCodeDomProvider"/> class.
        /// </summary>
        /// <param name="provider">LuaCodeDomProvider instance.</param>
        public VSMDLuaCodeDomProvider(LuaCodeDomProvider provider)
        {
            codeDomProvider = provider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VSMDLuaCodeDomProvider"/> class.
        /// </summary>
        /// <param name="projectItem">ProjectItem instance.</param>
        public VSMDLuaCodeDomProvider(ProjectItem projectItem)
        {
            codeDomProvider = new LuaCodeDomProvider(projectItem);
        }

        #region IVSMDCodeDomProvider Members

		/// <summary>
		/// Gets the code DOM provider.
		/// </summary>
		/// <value>The code DOM provider.</value>
        object IVSMDCodeDomProvider.CodeDomProvider
        {
            get
            {
				if(codeDomProvider == null)
				{
					var DTE = serviceProvider.GetService(typeof(DTE)) as DTE;
					codeDomProvider = new LuaCodeDomProvider(DTE.ActiveDocument.ProjectItem);
				}
            	 return codeDomProvider;
            }
        }

        #endregion

        #region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing,
		/// releasing, or resetting unmanaged resources.
		/// </summary>
        void IDisposable.Dispose()
        {
			if(codeDomProvider != null) codeDomProvider.Dispose();
        }

        #endregion
    }
}