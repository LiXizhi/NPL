using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLTools.Project
{
    static class Guids
    {
        public const string guidNPLProjectPkgString =
            "E2113C2D-E364-41B5-B1FF-71FAD9691D2E";
        public const string guidNPLProjectCmdSetString =
            "DD092ECF-972B-471B-AA9B-20845E1DFE4C";
        public const string guidNPLProjectFactoryString =
            "AFCF7665-3223-4967-A133-AD0F059C8014";
        public const string ProjectNodeGuid =
            "C0A64257-C203-4F7B-923D-0679CDCA1EA2";
        public static readonly Guid guidNPLProjectCmdSet =
            new Guid(guidNPLProjectCmdSetString);
        public static readonly Guid guidNPLProjectFactory =
            new Guid(guidNPLProjectFactoryString);
    }
}
