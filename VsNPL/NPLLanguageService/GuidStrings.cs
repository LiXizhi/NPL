namespace ParaEngine.Tools.Lua
{
    static class GuidStrings
    {

#if DEBUG
        public const string LuaLanguageServicePackage = "C5AA7DA8-C129-4cc7-ADBD-B71283E03AA7";
        public const string LuaLanguageService = "16AEA62B-0A31-45a5-9207-DD2CD290C145";
        public const string LuaRefactoringService = "80DA149C-E8E1-48e4-8228-DCA3EC4CBB53";
        public const string SourceOutlinerToolWindow = "4A831EC8-29C2-436c-9C80-57E43D8EA883";
        public const string RefactorRenameCommand = "8C6CA9FA-FBC4-4025-A711-7D802CFC8950";
        public const string UndoCommand = "0354DFCF-7FE9-43be-88D5-F047E7A38579";
#else
        public const string LuaLanguageServicePackage = "A53667AE-D839-4bbe-AC6B-0EE2B17E542C";
        public const string LuaLanguageService = "F8C85CC7-E3C2-4efb-AA8E-9F6C38A761A4";
        public const string LuaRefactoringService = "43989338-E85F-434b-91C6-9733A1A1C79F";
        public const string SourceOutlinerToolWindow = "4AFC2A42-DC2F-420f-B1CF-8A94D65C6056";
        public const string RefactorRenameCommand = "FF006832-9AF2-4635-B992-8F4A1DA2A45A";
        public const string UndoCommand = "55D3350E-8EBF-4d74-B015-FA96DF3B68C1";
#endif

    }
}
