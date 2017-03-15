using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace Boyaa
{
    public delegate void CallbackEventInitialize(int iThreadId);
    public delegate void CallbackEventCreateVM(int iThreadId, int vm);
    public delegate void CallbackEventDestroyVM(int iThreadId, int vm);
    public delegate void CallbackEventLoadScript(int iThreadId, string fullPath, int scriptIndex, int iRelative);
    public delegate void CallbackEventBreak(int iThreadId, string fullPath, int line);
    public delegate void CallbackEventSetBreakpoint(int iThreadId, string fullPath, int line, int enabled);
    public delegate void CallbackEventException(int iThreadId, string fullPath, int line, string msg);
    public delegate void CallbackEventLoadError(int iThreadId, string fullPath, int line, string error);
    public delegate void CallbackEventMessage(int iThreadId, int msgType, string fullPath, int line, string msg);
    public delegate void CallbackEventSessionEnd(int iThreadId);
    public delegate void CallbackEventNameVM(int iThreadId, int vm, string vmName);
    class LuaDebug
    {
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventInitialize(CallbackEventInitialize callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventCreateVM(CallbackEventCreateVM callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventDestroyVM(CallbackEventDestroyVM callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventLoadScript(CallbackEventLoadScript callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventBreak(CallbackEventBreak callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventSetBreakpoint(CallbackEventSetBreakpoint callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventException(CallbackEventException callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventLoadError(CallbackEventLoadError callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventMessage(CallbackEventMessage callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventSessionEnd(CallbackEventSessionEnd callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetCallbackEventNameVM(CallbackEventNameVM callbackFunction);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetWriteLog(int iEnable);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int WritePackageLog(string log);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static uint StartProcess(string command,string commandArguments,string workingDirectory,string symbolsDirectory,string scriptsDirectory);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void DebugStart();
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void DebugStop();
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void StepInto();
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void StepOver();
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetBreakpoint(string fullPath, int line);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void DisableBreakpoint(string fullPath,int line);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetNumStackFrames();
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void GetStackFrame(int stackFrameIndex,StringBuilder fullPath,int fullPathLen,StringBuilder fun,int funLen,ref int line);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static bool ExecuteText(int executeId, string text, StringBuilder type, int typeLen, StringBuilder value, int valueLen, ref int expandable);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int EnumChildrenNum(int executeId,string text);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void EnumChildren(int executeId, string text, int subIndex, StringBuilder subText, int subTextLen);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static int GetProjectNumFiles();
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void GetProjectFile(int index, StringBuilder fullPath, int fullPathLen);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void ClearInitBreakpoints();
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void AddInitBreakpoint(string fullPath,int line);
        [DllImport("Decoda.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public extern static void SetStackLevel(int stackLevel);
    }
}
