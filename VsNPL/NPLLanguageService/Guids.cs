// Guids.cs
// MUST match guids.h
using System;
using ParaEngine.Tools.Lua;
namespace ParaEngine.NPLLanguageService
{
    static class GuidList
    {
        public const string guidNPLLanguageServicePkgString = "a85e31a8-79a8-4028-b3e0-6e8923873197";
        public const string guidNPLLanguageServiceCmdSetString = "2ff68ea1-b061-467c-a0bb-29929ada812d";
        public const string guidToolWindowPersistanceString = "5288878d-9534-4979-822e-93a1f16e869d";
        public const string guidNPLLanguageServiceEditorFactoryString = "5bcdb184-0ce4-429b-991a-2c3ee429e72a";

        public static readonly Guid guidNPLLanguageServiceCmdSet = new Guid(guidNPLLanguageServiceCmdSetString);
        public static readonly Guid guidNPLLanguageServiceEditorFactory = new Guid(guidNPLLanguageServiceEditorFactoryString);
    };
    
    static class Guids
    {
        public static readonly Guid LuaLanguageServicePackage = new Guid(GuidStrings.LuaLanguageServicePackage);
        public static readonly Guid LuaLanguageService = new Guid(GuidStrings.LuaLanguageService);

        public static readonly Guid SourceOutlinerToolWindow = new Guid(GuidStrings.SourceOutlinerToolWindow);
        public static readonly Guid RefactorRenameCommand = new Guid(GuidStrings.RefactorRenameCommand);

    }
}