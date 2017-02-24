﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLProject
{
    static class Guids
    {
        public const string guidNPLProjectPkgString =
            "E2113C2D-E364-41B5-B1FF-71FAD9691D2E";
        public const string guidSimpleProjectCmdSetString =
            "DD092ECF-972B-471B-AA9B-20845E1DFE4C";
        public const string guidSimpleProjectFactoryString =
            "AFCF7665-3223-4967-A133-AD0F059C8014";

        public static readonly Guid guidSimpleProjectCmdSet =
            new Guid(guidSimpleProjectCmdSetString);
        public static readonly Guid guidSimpleProjectFactory =
            new Guid(guidSimpleProjectFactoryString);
    }
}