using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudioTools.Project;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using System.ComponentModel;
namespace NPLTools.Project
{
    [ComVisible(true)]
    [Guid("F30D83A9-D13E-4FF2-A7BD-3685618EFA89")]
    class NPLProjectNodeProperties : ProjectNodeProperties
    {
        public NPLProjectNodeProperties(ProjectNode node) : base(node)
        {
        }

        [Category("General")]
        [DisplayName("Path")]
        [Description("description")]
        public string NodeExeArguments
        { get; set;
        }
    }
}
