using System.Collections.Generic;
namespace ParaEngine.Tools.Lua
{
    public class Method : Declaration
    {
        /// <summary>
        /// Gets or sets the parameters of the method.
        /// </summary>
        public IList<Parameter> Parameters { get; set; }
    }
}
