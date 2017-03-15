/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using Microsoft.VisualStudioTools;

namespace NPLTools.Project
{
    public static class LuaConstants
    {
        //Language name
        internal const string LanguageName = "Lua";
        internal const string TextEditorSettingsRegistryKey = LanguageName;
        internal const string FileExtension = ".lua";
        internal const string ProjectFileFilter = "Lua Project File (*.luaproj)\n*.luaproj\nAll Files (*.*)\n*.*\n";
        /// <summary>
        /// The extension for Lua files which represent Windows applications.
        /// </summary>
        internal const string WindowsFileExtension = ".luaw";
#if DEV11_OR_LATER
        internal const string ProjectImageList = "Microsoft.LuaImageList.png";
#else
        internal const string ProjectImageList = "Microsoft.LuaImageList.bmp";
#endif

        internal const string LibraryManagerGuid = "9BC0A61D-7798-4A66-B3A1-ED6F7D5A977A";//"888888e5-b976-4366-9e98-e7bc01f1842c";
        internal const string LibraryManagerServiceGuid = "207739EE-ACF5-4FD1-89D5-BC01CFD61DBD";//"88888859-2f95-416e-9e2b-cac4678e5af7";
        internal const string ProjectFactoryGuid = "5697748A-77EF-44CA-8824-4F5637E5945B";//"888888a0-9f3d-457c-b088-3a5042f75d52";
        internal const string EditorFactoryGuid = "08EEB6F9-CA6E-4848-9C3E-2035ECE337F3";//"888888c4-36f9-4453-90aa-29fa4d2e5706";
        internal const string ProjectNodeGuid = "0698CE68-9E74-4333-8777-1E58A40D6BE8";//"8888881a-afb8-42b1-8398-e60d69ee864d";
        internal const string GeneralPropertyPageGuid = "7559653F-8C34-49AF-B5C3-775C219AE002";//"888888fd-3c4a-40da-aefb-5ac10f5e8b30";
        internal const string DebugPropertyPageGuid = "06DA731E-273C-4CA9-9DD6-B1DB05902913";//"9A46BC86-34CB-4597-83E5-498E3BDBA20A";
        internal const string PublishPropertyPageGuid = "09288510-C7EB-40B7-AE1D-A2A28F0D386F";//"63DF0877-CF53-4975-B200-2B11D669AB00";
        internal const string EditorFactoryPromptForEncodingGuid = "5A83FE43-C408-4BD3-A3BD-88DEA206B140";//"CA887E0B-55C6-4AE9-B5CF-A2EEFBA90A3E";

        internal const string InterpreterItemType = "23843E9F-907C-4E6A-A6AF-381624342C2D";//"32235F49-CF87-4F2C-A986-B38D229976A3";
        internal const string InterpretersPackageItemType = "71470108-1F43-4678-A254-2BB0F368EBD5";//"64D8C685-F085-4E04-B759-3DF715EBA3FA";
        internal static readonly Guid InterpreterItemTypeGuid = new Guid(InterpreterItemType);
        internal static readonly Guid InterpretersPackageItemTypeGuid = new Guid(InterpretersPackageItemType);

        internal const string InterpretersPropertiesGuid = "D95521DF-DE6F-431E-B680-1BB37A6DE758";//"45D3DC23-F419-4744-B55B-B897FAC1F4A2";
        internal const string InterpretersWithBaseInterpreterPropertiesGuid = "13E5C2A8-3830-41F0-9E16-AB7B29B75565";//"F86C3C5B-CF94-4184-91F8-29687D3B9227";
        internal const string InterpretersPackagePropertiesGuid = "DC1054FA-2E88-4620-8A19-BFC62D3AE53F";//"BBF56A45-B037-4CC2-B710-F2CE304CCF32";
        internal const string InterpreterListToolWindowGuid = "68CFEE5E-8D88-4084-B59A-F6B010D45E1F";//"75504045-D02F-44E5-BF60-5F60DF380E8B";

        // Do not change below info without re-requesting PLK:
        internal const string ProjectSystemPackageGuid = "0C92F352-CCBA-44D3-9A96-EAD5058372C5";//"15490272-3C6B-4129-8E1D-795C8B6D8E9F"; //matches PLK

        //IDs of the icons for product registration (see Resources.resx)
        internal const int IconIfForSplashScreen = 300;
        internal const int IconIdForAboutBox = 400;

        internal const int AddEnvironment = 0x4006;
        internal const int AddVirtualEnv = 0x4007;
        internal const int AddExistingVirtualEnv = 0x4008;
        internal const int ActivateEnvironment = 0x4009;
        internal const int InstallLuaPackage = 0x400A;

        internal const int AddSearchPathZipCommandId = 0x4003;
        internal const int AddLuaPathToSearchPathCommandId = 0x4030;

        //Custom (per-project) commands
        internal const int FirstCustomCmdId = 0x4010;
        internal const int LastCustomCmdId = 0x402F;
        internal const int CustomProjectCommandsMenu = 0x2005;

        // Shows up before references
        internal const int InterpretersContainerNodeSortPriority = 200;

        internal const string InterpreterId = "InterpreterId";
        internal const string InterpreterVersion = "InterpreterVersion";

        internal const string LaunchProvider = "LaunchProvider";

        internal const string LuaExtension = "LuaExtension";

        public const string SearchPathSetting = "SearchPath";
        public const string InterpreterPathSetting = "InterpreterPath";
        public const string InterpreterArgumentsSetting = "InterpreterArguments";
        public const string CommandLineArgumentsSetting = "CommandLineArguments";
        public const string StartupFileSetting = "StartupFile";
        public const string IsWindowsApplicationSetting = "IsWindowsApplication";

        /// <summary>
        /// Specifies port to which to open web browser on launch.
        /// </summary>
        public const string WebBrowserPortSetting = "WebBrowserPort";

        /// <summary>
        /// Specifies URL to which to open web browser on launch.
        /// </summary>
        public const string WebBrowserUrlSetting = "WebBrowserUrl";

        //Mixed-mode debugging project property
        public const string EnableNativeCodeDebugging = "EnableNativeCodeDebugging";

        public const string WorkingDirectorySetting = CommonConstants.WorkingDirectory;
        public const string ProjectHomeSetting = CommonConstants.ProjectHome;
    }
}
