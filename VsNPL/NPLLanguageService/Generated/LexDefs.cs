/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/


namespace ParaEngine.Tools.Lua.Parser
{
    //
    // These are the dummy declarations for stand-alone lex applications
    // normally these declarations would come from the parser.
    // 
    public interface IErrorHandler
    {
        int ErrNum { get; }
        int WrnNum { get; }
        void AddError(string msg, int lin, int col, int len, int severity);
    }
}
