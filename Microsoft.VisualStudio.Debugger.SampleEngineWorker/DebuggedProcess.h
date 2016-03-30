#pragma once

#pragma managed(off)
#include "PETypes.h"
#include "InterprocessQueue.hpp"
#include "NPLInterface.hpp"

#pragma managed(on)

#include "AddressDictionary.h"
#include "BreakpointData.h"
#include "SymbolEngine.h"
#include "VariableInformation.h"

BEGIN_NAMESPACE

// Forward-refs
interface class ISampleEngineCallback;
ref class DebuggedModule;
ref class DebuggedThread;
ref class ModuleResolver;
ref class X86ThreadContext;

// If the engine is launching or attaching.
enum DEBUG_METHOD
{
	Launch, 
	Attach
};

// The type of the last stopping event.
enum STOPPING_EVENT_KIND
{
	Invalid,
	StartDebugging,
	Breakpoint,
	Exception,
	StepComplete,
	AyncBreakComplete,
	LoadComplete
};

// added for NPL
enum enum_STEPKIND{
	STEP_INTO = 0,	STEP_OVER = 1,	STEP_OUT = 2,	STEP_BACKWARDS = 3,
};
// added for NPL
enum enum_STEPUNIT
{
	STEP_STATEMENT = 0,
	STEP_LINE = 1,
	STEP_INSTRUCTION = 2,
};

[Flags]
public enum class ResumeEventPumpFlags
{
	ResumeForStepOrExecute = 0x1,
	ResumeWithExceptionHandled = 0x2
};

public ref class StackInfo sealed
{
public:
	StackInfo(unsigned int nAddress, String^ sName) {
		m_nAddress = nAddress;
		m_sName = sName;
	}
	unsigned int m_nAddress;
	String^ m_sName;
};

// Constants for exception types that are interesting to the sample engine.
// BreakpointExceptionCode is when the debuggee executes and int3. 
// SingleStepExceptionCode is sent when the processor is in trace mode and has completed
// a single-step
const DWORD BreakpointExceptionCode = 0x80000003;
const DWORD SingleStepExceptionCode = 0x80000004;

// DebuggeedProcess represents a process being debugged to the back-end portion of the debug engine sample.
public ref class DebuggedProcess sealed
{
private:
	// immutable fields
	const DEBUG_METHOD m_debugMethod;
	const HANDLE m_hProcess;
	const DWORD m_dwPollThreadId;
	initonly ISampleEngineCallback^ m_callback;
	initonly ModuleResolver^ m_resolver;
	SymbolEngine* m_pSymbolEngine;

	DebuggedThread^ m_entrypointThread;
	DebuggedModule^ m_entrypointModule;

	// LOCKING ORDER:
	// m_threadIdMap must be taken before m_breakpointMap
	// other locks are unordered

	// Only updated on the main thread
	int m_suspendCount;
	
	// Module & Thread and kept in a map and a list. The map is used for indexing,
	// the list is used to return these things in their load order. These can be read on
	// the poll thread without a lock, but should be locked for poll thread updates &
	// non-poll thread reads. Except for cleanup, all updates occur on the poll thread.
	initonly AddressDictionary<DebuggedModule^>^ m_moduleAddressMap;
	initonly Collections::Generic::LinkedList<DebuggedModule^>^ m_moduleList;

	initonly Collections::Generic::Dictionary<DWORD, DebuggedThread^>^ m_threadIdMap;
	initonly Collections::Generic::LinkedList<DebuggedThread^>^ m_threadList;

	// This map can be updated on any thread at any time. It needs to be locked to
	// read or write.
	initonly Collections::Generic::Dictionary<DWORD_PTR, BreakpointData^>^ m_breakpointMap;

	// These fields are only updated on the poll thread
	DEBUG_EVENT& m_lastDebugEvent;
	STOPPING_EVENT_KIND m_lastStoppingEvent;

	// True if the debug loop is currently pumping.
	bool m_fIsPumpingDebugEvents;

	// True if the sample engine has seen the entrypoint breakpoint.
	bool m_fSeenEntrypointBreakpoint;

	// True if the sample engine is expecting an asymbreak event.
	bool m_fExpectingAsyncBreak;

	bool m_fExpectingBreakpointSingleStep;
	BreakpointData^ m_singleStepBreakpoint;

#pragma region NPL Symbol API
private:
	// id, string map is used by NPL debugger only. 
	// mapping from file id to full path of file name
	static Collections::Generic::Dictionary<unsigned int, String^>^ m_id_to_string = gcnew Collections::Generic::Dictionary<unsigned int, String^>();
	// mapping from both relative and full path to file id. 
	static Collections::Generic::Dictionary<String^, unsigned int>^ m_string_to_id = gcnew Collections::Generic::Dictionary<String^, unsigned int>();
	
	String^ GetRelativeFilePath(String^ filePath)
	{
		filePath = filePath->Replace("\\", "/");
		filePath = filePath->ToLower();
		// Add lower cased and stripping working directory path as well. This is relative path used by NPL runtime. 
		int nIndex = filePath->IndexOf(m_workingDir);
		if(nIndex == 0)
		{
			return filePath->Substring(m_workingDir->Length);
		}
		return filePath;
	}

	unsigned int GetIdByString(String^ str){
		if(String::IsNullOrEmpty(str))
			return 0;
		unsigned int out = 0;
		str = str->ToLower();
		if(m_string_to_id->TryGetValue(str, out)){
			return out;
		}
		else{
			// add both full path and relative to working dir path to string map. 
			static unsigned int s_last_file_id = 0;
			String^ fullpath = str;
			
			str = str->Replace("\\", "/");
			bool isFullPath = str->IndexOf(":") > 0;
			if (isFullPath)
			{
				// add relative path used by NPL runtime as well. 
				int nIndex = str->IndexOf(m_workingDir);
				if (nIndex == 0)
				{
					// stripping working directory
					str = str->Substring(m_workingDir->Length);
					m_string_to_id[str] = s_last_file_id;
				}
				else
				{
					// get relative path by finding the first "script/" or "source/" or "src/"
					nIndex = str->IndexOf("script/");
					if (nIndex < 0)
					{
						nIndex = str->IndexOf("source/");
						if (nIndex < 0)
							nIndex = str->IndexOf("src/");
					}
					if (nIndex >= 0)
					{
						// add relative path. 
						str = str->Substring(nIndex);
						m_string_to_id[str] = s_last_file_id;
					}
				}
			}
			// add full path as used by visual studio to locate file path
			m_id_to_string[s_last_file_id] = fullpath;
			m_string_to_id[fullpath] = s_last_file_id;
			out = s_last_file_id;
			s_last_file_id = s_last_file_id + 1;
		}
		return out;
	}
	String^ GetStringById( unsigned int id) {
		String^ out = "";
		if(m_id_to_string->TryGetValue(id, out)){
			return out;
		}
		return out;
	}

	// return an unsigned int address from filename and line number
	// this will roughly simulate a symbol server and compatible with native debugger address
	unsigned int GetAddressByFileLine(String^ filename, int line){
		return (line)*10000 + GetIdByString(filename);
	}
	
	// get filename and line number from unsigned int address
	// this will roughly simulate a symbol server and compatible with native debugger address
	void GetFileLineByAddress(unsigned int address, String^% filename, int% line){
		line = (int)(address/10000);
		filename = GetStringById(address % 10000);
	}
	void NPL_Suspend();
	void NPL_Resume();
	void NPL_SetBreakPoint(unsigned int addr);
	void NPL_RemoveBreakPoint(unsigned int addr);
	
	bool TranslateNPLMsgToDebugEvent(LPDEBUG_EVENT lpDebugEvent, ParaEngine::InterProcessMessage& msg_in);
	bool WaitForNPLDebugEvent( LPDEBUG_EVENT lpDebugEvent, DWORD dwMilliseconds );
	BOOL ContinueNPLDebugEvent( DWORD dwProcessId, DWORD dwThreadId, DWORD dwContinueStatus );

	bool DispatchNPLDebugEvent(bool& fContinue);

/** whether the debugged NPL runtime has entered debug loop (in break mode waiting for next debug message). */
	bool IsNPLInBreakMode()
	{
		return !((m_lastStoppingEvent == Invalid) || (m_lastStoppingEvent == LoadComplete) || (m_lastStoppingEvent == StartDebugging));
	}

	//bool NPLAttachProcess();
	bool NPLDetachProcess();
	unsigned int m_curBreakpointAddress;

	Collections::Generic::List<StackInfo^>^ m_curStackInfos = gcnew Collections::Generic::List<StackInfo^>();
	
	// lower cased forward slash /, that ends with /
	String^ m_workingDir;
	// whether we are expecting a step into/over/out breakpoints from NPL 
	bool m_bExpectingStepBreakpoint;
	bool m_bNPLProcDetachRequested;
	
public:
	bool NPL_EvaluateExpressionSync(String^ sExpression, String^% sOutputValue);

	void SetWorkingDir(String^ workingDir) { 
		m_workingDir = workingDir; 
		m_workingDir = m_workingDir->Replace("\\", "/");
		if(!m_workingDir->EndsWith("/"))
		{
			m_workingDir += "/";
		}
		m_workingDir = m_workingDir->ToLower();
	}

	/** Step over, into, out debugging commands 
	* @param nStepKind	STEP_INTO = 0,	STEP_OVER = 1,	STEP_OUT = 2,	STEP_BACKWARDS = 3,
	* @param nStepUnit 	STEP_STATEMENT = 0,	STEP_LINE = 1,	STEP_INSTRUCTION = 2,
	*/
	int Step(DebuggedThread^ thread, int nStepKind, int nStepUnit);
	

#pragma endregion NPL Symbol API


public:

	// The pid of the process
	initonly int Id;

	// The name of the process
	initonly String^ Name;

	// The start address of the process (normally in the CRT)
	initonly DWORD_PTR StartAddress;

	// Resume pumping debug events
	void ResumeEventPump();

	// Async-Break
	void Break();
	void Suspend();
	void Resume();
	void ResumeFromLaunch();
	X86ThreadContext^ GetThreadContext(IntPtr hThread);
	DebuggedModule^ ResolveAddress(DWORD_PTR address);

	void SetBreakpoint(DWORD_PTR address, Object^ client);
	void RemoveBreakpoint(DWORD_PTR address, Object^ client);

	cli::array<byte>^ ReadMemory(DWORD_PTR base, DWORD size);
	unsigned int ReadMemoryUInt(DWORD_PTR base);
	void WriteMemory(DWORD_PTR base, cli::array<byte>^ data);
	void Detach();
	void Terminate();
	void Close();
	void Continue(DebuggedThread^ thread);
	void Execute(DebuggedThread^ thread);
	cli::array<DebuggedThread^>^ GetThreads();
	cli::array<DebuggedModule^>^ GetModules();

	// Initiate an x86 stack walk on this thread.
	void DoStackWalk(DebuggedThread^ thread);

	void WaitForAndDispatchDebugEvent(ResumeEventPumpFlags flags);

	property DWORD PollThreadId
	{
		DWORD get() { return m_dwPollThreadId; }
	}

	property bool IsStopped
	{
		bool get()
		{
			return m_lastDebugEvent.dwDebugEventCode != 0 ||
				m_suspendCount > 0;
		}
	}

	property bool IsPumpingDebugEvents
	{
		bool get()
		{
			return m_fIsPumpingDebugEvents;
		}
	}

public:
	// Symbol handler methods which allow the upper layers to obtain symbol information.
	bool GetSourceInformation(unsigned int ip, String^% documentName, String^% functionName, unsigned int% dwLine, unsigned int% numParameters, unsigned int% numLocals);
	void GetFunctionArgumentsByIP(unsigned int ip, unsigned int bp, cli::array<VariableInformation^>^ arguments);
	void GetFunctionLocalsByIP(unsigned int ip, unsigned int bp, cli::array<VariableInformation^>^ locals);

	cli::array<unsigned int>^ GetAddressesForSourceLocation(String^ moduleName, String^ documentName, DWORD dwStartLine, DWORD dwStartCol);

internal:
	DebuggedProcess(DEBUG_METHOD method, ISampleEngineCallback^ callback, HANDLE hProcess, int processId, String^ name);

private:
	~DebuggedProcess();
	!DebuggedProcess();
	bool WaitForDebugEvent();
	bool WaitForDebugEvent(DWORD dwTimeout);
	void ContinueDebugEvent(bool fExceptionHandled);
	bool DispatchDebugEvent();
	DebuggedModule^ CreateModule(const LOAD_DLL_DEBUG_INFO& debugEvent);
	DebuggedThread^ CreateThread(DWORD threadId, HANDLE hThread, DWORD_PTR startAddress);

	bool HandleAsyncBreakException(const EXCEPTION_DEBUG_INFO* exceptionDebugInfo);
	bool HandleBreakpointException(const EXCEPTION_DEBUG_INFO* exceptionDebugInfo);
	bool HandleBreakpointSingleStepException(DWORD dwThreadId, const EXCEPTION_DEBUG_INFO* exceptionDebugInfo);

	void GetFunctionVariablesByIP(unsigned int ip, unsigned int bp, DWORD dwDataKind, cli::array<VariableInformation^>^ variables);

	DWORD GetImageSizeFromPEHeader(HANDLE hProcess, LPVOID lpDllBase);	

	bool IsExpectingAsyncBreak()
	{
		return m_fExpectingAsyncBreak;
	}

	bool IsBreakpointException(DWORD dwExceptionCode)
	{
		return (dwExceptionCode == BreakpointExceptionCode);
	}

	bool IsSingleStepException(DWORD dwExceptionCode)
	{
		return (dwExceptionCode == SingleStepExceptionCode);
	}

	bool LastDebugEventWasBreakpoint()
	{
		if (m_lastStoppingEvent == Breakpoint && m_lastDebugEvent.dwDebugEventCode == EXCEPTION_DEBUG_EVENT)
		{
			const EXCEPTION_DEBUG_INFO* exceptionDebugInfo = &(m_lastDebugEvent.u.Exception);

			return IsBreakpointException(exceptionDebugInfo->ExceptionRecord.ExceptionCode);
		}

		return false;
	}

	BreakpointData^ FindBreakpointAtAddress(DWORD_PTR address);
	void RecoverFromBreakpoint();

	void EnableSingleStep(DWORD dwThreadId);

	void RewindInstructionPointer(DWORD dwThreadId, DWORD dwNumBytes);
};

END_NAMESPACE
