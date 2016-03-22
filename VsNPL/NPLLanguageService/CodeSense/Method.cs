using System.Collections.Generic;
using System.Text;

namespace ParaEngine.Tools.Lua
{
    public class Method : Declaration
    {
        /// <summary>
        /// Gets or sets the parameters of the method.
        /// </summary>
        public IList<Parameter> Parameters { get; set; }

        /// <summary>
        /// NPL syntax formatting here, used by quick info
        /// </summary>
        /// <returns></returns>
        public string GetQuickInfo(string methodPrefix)
        {
            StringBuilder output = new StringBuilder();
            if(!string.IsNullOrEmpty(this.Description))
            {
                output.AppendFormat("{0}\n", this.Description);
            }
            output.AppendFormat("{0}{1}(", methodPrefix==null ? "" : methodPrefix, this.Name);
            if (Parameters != null)
            {
                bool bFirstParam = true;
                foreach (var param in Parameters)
                {
                    if (bFirstParam)
                        bFirstParam = false;
                    else
                        output.Append(", ");
                    if (param.Optional)
                        output.Append("[");
                    output.Append(param.Name);
                    if (!string.IsNullOrEmpty(param.Type))
                    {
                        output.AppendFormat(":{0}", param.Type);
                    }
                    if (param.Optional)
                        output.Append("]");
                }
            }
            output.Append(")");
            return output.ToString();
        }

        
    }
}
