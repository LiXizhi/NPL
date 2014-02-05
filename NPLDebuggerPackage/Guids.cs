// Guids.cs
// MUST match guids.h
using System;

namespace ParaEngine.NPLDebuggerPackage
{
    static class GuidList
    {
        public const string guidNPLDebuggerPackagePkgString = "b63940d7-a819-4793-9de8-f55a8085deb6";
        public const string guidNPLDebuggerPackageCmdSetString = "2e66d0c0-f21e-4149-8102-9c3684f4d6a8";
        public const string guidToolWindowPersistanceString = "f8112e24-1f21-4e55-825e-5509c34b6d26";

        public static readonly Guid guidNPLDebuggerPackageCmdSet = new Guid(guidNPLDebuggerPackageCmdSetString);

        public static NPLDebuggerPackage gPackage;
    };
}