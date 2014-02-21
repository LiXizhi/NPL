--[[
Title: IPC debugger
Author(s): LiXizhi, inspired by RemDebug 1.0
Date: 2010/5/10
Desc: This is the debug engine that communicates with the remote debug engine worker running in visual studio via IPC. 
Basic functions
- In visual studio, we can launch or attach to a ParaEngine process to debug it. Breakpoints, step into/over/out are supported. 
- Currently, we only support debugging one NPL state (usually the main state). To start the debugging, simply call IPCDebugger.StartDebugEngine(); in the NPL state to be debugged. 
	alternatively, we can start it automatically when loading IPCDebugger.lua, provided the command line parameter "debug" is the current NPL state name, such as "main". The queue name can be specified by "debugqueue", which defaults to "NPLDebug"
- Note: there is no performance penalties when starting a debug engine, it only starts a timer to receive from IPC queue. The IPCdebugger only starts the debug hook whenever visual studio attaches or launched the process. 
Use Lib:
-------------------------------------------------------
NPL.load("(gl)script/ide/Debugger/IPCDebugger.lua");
-- start the debug engine in the "main" state(thread), by loading "IPCDebugger.lua" file and launch with the command line debug="main"
-- "ParaEngineClient.exe" debug="main" bootstrapper="whateverfile"
-- Thenm call start to start debug engine in the calling NPL state.
IPCDebugger.Start();
-- IPCDebugger.WaitForBreak(); -- call this function to wait for break or attach event at a given code location (usually at the beginning). 
------------------------------------------------------
]]
NPL.load("(gl)script/ide/commonlib.lua");
NPL.load("(gl)script/ide/Files.lua");
NPL.load("(gl)script/ide/IPC.lua");

if not IPCDebugger then IPCDebugger = {} end

-- whether to dump IPC messages to log.txt
local debug_debugger = false;

-- polling for incoming debug request message every 100ms. 
IPCDebugger.polling_interval = 100;
local Handlers = {};
IPCDebugger.Handlers = Handlers;
IPCDebugger.IsIPCStarted = nil;
IPCDebugger.input_timer = nil;
IPCDebugger.input_queue_name = nil;

local input_queue;
local output_queue;
local debug_events_enum = {
	DEBUG_OUTPUT = 0,
	BREAKPOINT = 0,
	ATTACHED = 0,
}

--------------------------
-- local fast functions
--------------------------
local tinsert = table.insert
local strfind = string.find
local strsub = string.sub
local strlower = string.lower
local gsub  = string.gsub
--local write = IPCDebugger.WriteDebugOutput
local setcolor = function() end

local IsWindows = true;

local coro_debugger
local events = { BREAK = 1, WATCH = 2, STEP = 3, SET = 4 }
local breakpoints = {}
local watches = {}
local step_into   = false
local step_over   = false
local step_lines  = 0
local step_level  = 0
local stack_level = 0
local trace_level = 0
local trace_calls = false
local trace_returns = false
local trace_lines = false
local ret_file, ret_line, ret_name
local current_thread = 'main'
local started = false
local pause_off = false
local _g      = _G
local cocreate, cowrap = coroutine.create, coroutine.wrap
local pausemsg = 'pause'

-- call this when game is loaded. Please note, if one delete all timers, such as restart a game level, one need to call this function again. 
-- @param bForceStart: if true, we will force start the debugger regardless when the app is started with command line debug="main". 
function IPCDebugger.Start(bForceStart)
	if(not bForceStart) then
		local debugState = ParaEngine.GetAppCommandLineByParam("debug", "");
		if(debugState == "true" or debugState == __rts__:GetName()) then
			IPCDebugger.StartDebugEngine(ParaEngine.GetAppCommandLineByParam("debugqueue", "NPLDebug"));
		end
	else
		IPCDebugger.StartDebugEngine(ParaEngine.GetAppCommandLineByParam("debugqueue", "NPLDebug"));
	end	
end

-- start the debug engine. It will begin waiting for incoming debug request from the debugger UI.
-- Note: start the debug engine only start a 100ms timer that polls the "debugger IPC queue". 
-- So there is no performance impact to the runtime until we explicitly enable debug hook of the NPL runtime. 
-- @param input_queue_name: the input IPC queue name, default to "NPLDebug"
function IPCDebugger.StartDebugEngine(input_queue_name)
	if(IPCDebugger.IsIPCStarted) then
		if(IPCDebugger.input_timer) then
			IPCDebugger.input_timer:Change(IPCDebugger.polling_interval, IPCDebugger.polling_interval)
		end	
		return
	end
	IPCDebugger.IsIPCStarted = true;
	IPCDebugger.input_queue_name = input_queue_name or "NPLDebug";
	input_queue = ParaIPC.CreateGetQueue(IPCDebugger.input_queue_name, 2);
	
	commonlib.log("IPC debugger started in NPL state %s: queue name %s\n", __rts__:GetName(), IPCDebugger.input_queue_name);
	
	-- start timer to process the asynchrounous messages. 
	IPCDebugger.input_timer = IPCDebugger.input_timer or commonlib.Timer:new({callbackFunc = function(timer)
		local out_msg = {};
		while(input_queue:try_receive(out_msg) == 0) do
			if(debug_debugger) then
				log("AsyncDebugMsg:")
				commonlib.echo(out_msg);
			end	
			if(out_msg.method == "debug") then
				local handler = Handlers[out_msg.filename];
				if(type(handler) == "function") then
					handler(out_msg.type, out_msg.param1, out_msg.param2, out_msg.code, out_msg.from)
				end
			end	
		end
	end})
	IPCDebugger.input_timer:Change(IPCDebugger.polling_interval, IPCDebugger.polling_interval)
end

-- waiting for a the external debugger to attach and break this process
-- This is a helper function to force entering debug session at the very beginning of a program. 
-- In most cases, this function is never called. 
function IPCDebugger.WaitForBreak()
	log("IPC debugger is waiting for external debugger to attach.\n");
	local out_msg = {};
	-- break until we receive the "Attach"
	while(input_queue:receive(out_msg) == 0) do
	
		if(debug_debugger) then
			log("AsyncDebugMsg:")
			commonlib.echo(out_msg);
		end	
		if(out_msg.method == "debug") then
			local handler = Handlers[out_msg.filename];
			if(type(handler) == "function") then
				handler(out_msg.type, out_msg.param1, out_msg.param2, out_msg.code, out_msg.from)
			end
			if(out_msg.filename == "Break") then
				log("the external debugger calls break, so we will exit the loop and let the external debugger to take over control.\n");
				break;
			end
		end	
	end
end


-- send a debug event message to the remote debug engine via the output_queue
function IPCDebugger.Write(msg)
	if(output_queue) then
		output_queue:try_send({
			method = "debug",
			from = IPCDebugger.input_queue_name,
			type = msg.type,
			param1 = msg.param1,
			param2 = msg.param2,
			filename = msg.filename,
			code = msg.code,
		});
	end
end

-- this function is met to send message to output window while the program is paused
-- @param msg: the string
function IPCDebugger.WriteDebugOutput(msg)
	if(debug_debugger) then log(msg); end
	IPCDebugger.Write({filename="DebuggerOutput", type=debug_events_enum.DEBUG_OUTPUT, code = msg});
end
local write = IPCDebugger.WriteDebugOutput

function IPCDebugger.Dump(...)
	local msg = table.concat({...})
	if(msg) then
		if(debug_debugger) then log(msg); end
		IPCDebugger.Write({filename="ExpValue", type=debug_events_enum.DEBUG_OUTPUT, code = msg});
	end	
end

-- this funcion is met to send messsage to output window while the program is running. 
-- @param msg: the string
function IPCDebugger.WriteOutput(msg)
	IPCDebugger.Write({filename="Output", type=debug_events_enum.DEBUG_OUTPUT, code = msg});
end

-- send a break point event to the debugger UI. 
function IPCDebugger.WriteBreakPoint(filename, line)
	IPCDebugger.Write({filename="BP", type=debug_events_enum.BREAKPOINT, code = {filename=filename, line=line}});
end

-- read the next debug message. 
-- @return the message table or nil
function IPCDebugger.WaitForDebugEvent(out_msg)
	out_msg = out_msg or {};
	if(input_queue:receive(out_msg) == 0) then
		if(debug_debugger) then
			log("SyncDebugMsg:")
			commonlib.echo(out_msg);
		end
		return out_msg
	end
end

-- async break request
function Handlers.Break(type, param1, param2, msg, from)
	output_queue = IPC.CreateGetQueue(from, 2);
	IPCDebugger.pause();
end

-- async attach a remote IPC debugger to this NPL state and break it  
function Handlers.Attach(type, param1, param2, msg, from)
	-- create the output message queue to communicate with the remote debug engine
	output_queue = IPC.CreateGetQueue(from, 2);
	-- attach debug hook
	IPCDebugger.Attach();
end

-- async detach a remote IPC debugger 
function Handlers.Detach(type, param1, param2, msg, from)
	IPCDebugger.Detach();
end

-- async resume suspend
function Handlers.Suspend(type, param1, param2, msg)
end

-- SetBreakpoint async: this is not recommended way to set breakpoint, call setb when breaked instead.
function Handlers.setb(type, param1, param2, msg)
	local filename, line = IPCDebugger.NormalizeFileName(msg.filename), msg.line;
	if(filename and line) then
		IPCDebugger.set_breakpoint(filename, line);
		IPCDebugger.WriteDebugOutput("Breakpoint async set in file "..filename..' line '..line..'\n')
	end	
end

-- RemoveBreakpoint async: this is not recommended way to remove breakpoint, call delb when breaked instead. 
function Handlers.delb(type, param1, param2, msg)
	IPCDebugger.remove_breakpoint(IPCDebugger.NormalizeFileName(msg.filename), msg.line)
end

function Handlers.Terminate(type, param1, param2, msg)
end

function Handlers.Close(type, param1, param2, msg)
end
	

--like debug.getinfo but copes with no activation record at the given level
--and knows how to get 'field'. 'field' can be the name of any of the
--activation record fields or any of the 'what' names or nil for everything.
--only valid when using the stack level to get info, not a function name.
local function getinfo(level,field)
  level = level + 1  --to get to the same relative level as the caller
  if not field then return debug.getinfo(level) end
  local what
  if field == 'name' or field == 'namewhat' then
    what = 'n'
  elseif field == 'what' or field == 'source' or field == 'linedefined' or field == 'lastlinedefined' or field == 'short_src' then
    what = 'S'
  elseif field == 'currentline' then
    what = 'l'
  elseif field == 'nups' then
    what = 'u'
  elseif field == 'func' then
    what = 'f'
  else
    return debug.getinfo(level,field)
  end
  local ar = debug.getinfo(level,what)
  if ar then return ar[field] else return nil end
end


local function indented( level, ... )
  IPCDebugger.Dump( string.rep('  ',level), table.concat({...}), '\n' )
end

local dumpvisited

local function dumpval( level, name, value, limit )
  local index
  if type(name) == 'number' then
    index = string.format('[%d] = ',name)
  elseif type(name) == 'string'
     and (name == '__VARSLEVEL__' or name == '__ENVIRONMENT__' or name == '__GLOBALS__' or name == '__UPVALUES__' or name == '__LOCALS__') then
    --ignore these, they are debugger generated
    return
  elseif type(name) == 'string' and strfind(name,'^[_%a][_.%w]*$') then
    index = name ..' = '
  else
    index = string.format('[%q] = ',tostring(name))
  end
  if type(value) == 'table' then
    if dumpvisited[value] then
      indented( level, index, string.format('ref%q;',dumpvisited[value]) )
    else
      dumpvisited[value] = tostring(value)
      if (limit or 0) > 0 and level+1 >= limit then
        indented( level, index, dumpvisited[value] )
      else
        indented( level, index, '{  -- ', dumpvisited[value] )
        for n,v in pairs(value) do
          dumpval( level+1, n, v, limit )
        end
        indented( level, '};' )
      end
    end
  else
    if type(value) == 'string' then
      if string.len(value) > 40 then
        indented( level, index, '[[', value, ']];' )
      else
        indented( level, index, string.format('%q',value), ';' )
      end
    else
      indented( level, index, tostring(value), ';' )
    end
  end
end

local function dumpvar( value, limit, name )
  dumpvisited = {}
  dumpval( 0, name or tostring(value), value, limit )
end


--show +/-N lines of a file around line M
local function show(file,line,before,after)

  line   = tonumber(line   or 1)
  before = tonumber(before or 10)
  after  = tonumber(after  or before)

  if not strfind(file,'%.') then file = file..'.lua' end

  local f = io.open(file,'r')
  if not f then
    --{{{  try to find the file in the path
    
    write('searching ...\n')
    -- By LXZ: looks for a file in the ./script/*.* recursively, cache the latest search result 
    -- Print the filenames if there are multiple match, or open the unique file. 
    file = gsub(file, "script/", "");
    local result = commonlib.Files.Find({}, "script", 10, 20, file)
    if(#result == 1) then
		local file = "script/"..result[1].filename;
		write("script/"..file.." is found\n");
		f = io.open(file,'r');
	elseif(#result > 1) then
		write('multiple files found, could be:\n');
		local _, file_result
		for _, file_result in ipairs(result) do
			write("script/"..file_result.filename.."\n");
		end
		return
    end

    --}}}
    if not f then
      write('Cannot find '..file..'\n')
      return
    end
  end

  local i = 0
  for l in f:lines() do
    i = i + 1
    if i >= (line-before) then
      if i > (line+after) then break end
      if i == line then
		setcolor(71)
        write(i..'***\t'..l..'\n')
        setcolor()
      else
        write(i..'\t'..l..'\n')
      end
    end
  end

  f:close()

end


local function gi( i )
  return function() i=i+1 return debug.getinfo(i),i end
end

local function gl( level, j )
	return function() 
		j=j+1;
		-- By LiXizhi 2010.10.16: we must first save to local variable and then return. otherwise luajit2 will generate "level out of range" Runtime error.
		local n, v = debug.getlocal( level, j );
		return n,v
	end
end

local function gu( func, k )
  return function() k=k+1 return debug.getupvalue( func, k ) end
end

local  traceinfo

local function tracestack(l)
  local l = l + 1                        --NB: +1 to get level relative to caller
  traceinfo = {}
  traceinfo.pausemsg = pausemsg
  for ar,i in gi(l) do
    tinsert( traceinfo, ar )
    local names  = {}
    local values = {}
    for n,v in gl(i,0) do
      if strsub(n,1,1) ~= '(' then   --ignore internal control variables
        tinsert( names, n )
        tinsert( values, v )
      end
    end
    if #names > 0 then
      ar.lnames  = names
      ar.lvalues = values
    end
    if ar.func then
      local names  = {}
      local values = {}
      for n,v in gu(ar.func,0) do
        if strsub(n,1,1) ~= '(' then   --ignore internal control variables
          tinsert( names, n )
          tinsert( values, v )
        end
      end
      if #names > 0 then
        ar.unames  = names
        ar.uvalues = values
      end
    end
  end
end

local function trace(set)
  local mark
  for level,ar in ipairs(traceinfo) do
    if level == set then
      mark = '***'
    else
      mark = ''
    end
    write('['..level..']'..mark..'\t'..(ar.name or ar.what)..' in '..ar.short_src..':'..ar.currentline..'\n')
  end
end

local function info() dumpvar( traceinfo, 0, 'traceinfo' ) end


local function set_breakpoint(file, line)
  if not breakpoints[line] then breakpoints[line] = {} end  
  breakpoints[line][file] = true;
end
IPCDebugger.set_breakpoint = set_breakpoint;

local function remove_breakpoint(file, line)
  if breakpoints[line] then breakpoints[line][file] = nil  end
end
IPCDebugger.remove_breakpoint = remove_breakpoint;

-- normalize file name
local function NormalizeFileName(filename)
	if(filename) then
		-- always use lower cased. 
		filename = strlower(filename); 
		-- use forward slash
		filename = string.gsub(filename, "\\", "/");
		-- let us remove 
		filename = string.gsub(filename, "^.*/script/", "script/");
	end
	return filename;
end
IPCDebugger.NormalizeFileName = NormalizeFileName;
		
-- search for exact names
local function has_breakpoint(file, line)
  return breakpoints[line] and breakpoints[line][file]
end

local function capture_vars(ref,level,line)
  --get vars, file and line for the given level relative to debug_hook offset by ref

  local lvl = ref + level                --NB: This includes an offset of +1 for the call to here

  --{{{  capture variables
  
  local ar = debug.getinfo(lvl, "f")
  if not ar then return {},'?',0 end
  
  local vars = {__UPVALUES__={}, __LOCALS__={}}
  local i
  
  local func = ar.func
  if func then
    i = 1
    while true do
      local name, value = debug.getupvalue(func, i)
      if not name then break end
      if strsub(name,1,1) ~= '(' then  --NB: ignoring internal control variables
        vars[name] = value
        vars.__UPVALUES__[i] = name
      end
      i = i + 1
    end
    vars.__ENVIRONMENT__ = getfenv(func)
  end
  
  vars.__GLOBALS__ = getfenv(0)
  
  i = 1
  while true do
    local name, value = debug.getlocal(lvl, i)
    if not name then break end
    if strsub(name,1,1) ~= '(' then    --NB: ignoring internal control variables
      vars[name] = value
      vars.__LOCALS__[i] = name
    end
    i = i + 1
  end
  
  vars.__VARSLEVEL__ = level
  
  if func then
    --NB: Do not do this until finished filling the vars table
    setmetatable(vars, { __index = getfenv(func), __newindex = getfenv(func) })
  end
  
  --NB: Do not read or write the vars table anymore else the metatable functions will get invoked!
  
  --}}}

  local file = getinfo(lvl, "source")
  if strfind(file, "@") == 1 then
    file = strsub(file, 2)
  end
  if IsWindows then file = strlower(file) end

  if not line then
    line = getinfo(lvl, "currentline")
  end

  return vars,file,line

end

local function restore_vars(ref,vars)

  if type(vars) ~= 'table' then return end

  local level = vars.__VARSLEVEL__       --NB: This level is relative to debug_hook offset by ref
  if not level then return end

  level = level + ref                    --NB: This includes an offset of +1 for the call to here

  local i
  local written_vars = {}

  i = 1
  while true do
    local name, value = debug.getlocal(level, i)
    if not name then break end
    if vars[name] and strsub(name,1,1) ~= '(' then     --NB: ignoring internal control variables
      debug.setlocal(level, i, vars[name])
      written_vars[name] = true
    end
    i = i + 1
  end

  local ar = debug.getinfo(level, "f")
  if not ar then return end

  local func = ar.func
  if func then

    i = 1
    while true do
      local name, value = debug.getupvalue(func, i)
      if not name then break end
      if vars[name] and strsub(name,1,1) ~= '(' then   --NB: ignoring internal control variables
        if not written_vars[name] then
          debug.setupvalue(func, i, vars[name])
        end
        written_vars[name] = true
      end
      i = i + 1
    end

  end

end

local function print_trace(level,depth,event,file,line,name)

  --NB: level here is relative to the caller of trace_event, so offset by 2 to get to there
  level = level + 2

  local file = file or getinfo(level,'short_src')
  local line = line or getinfo(level,'currentline')
  local name = name or getinfo(level,'name')

  local prefix = ''
  if current_thread ~= 'main' then prefix = '['..tostring(current_thread)..'] ' end

  write(prefix..
           string.format('%08.2f:%02i.',os.clock(),depth)..
           string.rep('.',depth%32)..
           (file or '')..' ('..(line or '')..') '..
           (name or '')..
           ' ('..event..')\n')

end

local function trace_event(event, line, level)

  if event == 'return' and trace_returns then
    --note the line info for later
    ret_file = getinfo(level+1,'short_src')
    ret_line = getinfo(level+1,'currentline')
    ret_name = getinfo(level+1,'name')
  end

  if event ~= 'line' then return end

  local slevel = stack_level
  local tlevel = trace_level

  if trace_calls and slevel > tlevel then
    --we are now in the function called, so look back 1 level further to find the calling file and line
    print_trace(level+1,slevel-1,'c',nil,nil,getinfo(level+1,'name'))
  end

  if trace_returns and slevel < tlevel then
    print_trace(level,slevel,'r',ret_file,ret_line,ret_name)
  end

  if trace_lines then
    print_trace(level,slevel,'l')
  end

  trace_level = stack_level

end

-- this is the debug hook that is called per line/call/return/break
-- highly optimized to run fast
local function debug_hook(event, line)
	if not started then 
		log("warning: debug_hook is called when debugger is not attached.\n");
		IPCDebugger.Detach();
		return;
	end
	--commonlib.echo({event = event, line = line, stack_level = stack_level, step_into = step_into, step_over = step_over })

	local level = 2;
	-- trace_event(event,line,level)
	if event == "call" then
		stack_level = stack_level + 1
		--echo({"call", stack_level})
	elseif event == "return" or event == "tail return" then
		stack_level = stack_level - 1

		--echo({"return", stack_level})
		--if stack_level < 0 then stack_level = 0 end
	else
		local file = strlower(debug.getinfo(2, "S").source);
		if string.find(file, "@") == 1 then
			file = string.sub(file, 2)
		end

		local ev = events.STEP

		--echo({step_into= step_into, step_over=step_over, stack_level=stack_level, step_level=step_level})

		if step_into or (step_over and stack_level <= step_level)then
			step_into = false
			step_over = false
		elseif(has_breakpoint(file, line)) then
			ev = events.BREAK;
		else
			return  
		end
		local vars, idx = nil, 0;
		-- Enter Break Mode, since there is no watches, we will only capture vars after a break point is met. This greatly improves debug_hook speed at run mode. 
		local vars,file,line = capture_vars(level,1,line)
		
		--local stop, ev, idx = false, events.STEP, 0
		--while true do
		  --for index, value in pairs(watches) do
			--setfenv(value.func, vars)
			--local status, res = pcall(value.func)
			--if status and res then
			  --ev, idx = events.WATCH, index
			  --stop = true
			  --break
			--end
		  --end
		  --if stop then break end
		  --if (step_into)
		  --or (step_over and (stack_level <= step_level or stack_level == 0)) then
			--step_lines = step_lines - 1
			--if step_lines < 1 then
			  --ev, idx = events.STEP, 0
			  --break
			--end
		  --end
		  --if has_breakpoint(file, line) then
			--ev, idx = events.BREAK, 0
			--break
		  --end
		  --return
		--end
		tracestack(level)

		local last_next = 1
		local err, next = coroutine.resume(coro_debugger, ev, vars, file, line, idx)

		while true do
			if next == 'cont' then
				return
			elseif next == 'stop' then
				IPCDebugger.Detach();
				return
			elseif tonumber(next) then --get vars for given level or last level
				write("get vars for given level or last level\n")
				next = tonumber(next)
				if next == 0 then next = last_next end
				last_next = next
				restore_vars(level,vars)
				vars, file, line = capture_vars(level,next)
				err, next = coroutine.resume(coro_debugger, events.SET, vars, file, line, idx)
			else
				write('Unknown command from debugger_loop: '..tostring(next)..'\n')
				write('Stopping debugger\n')
				next = 'stop'
			end
		end
	end
end

-- whenever a stopping event occurs
local function report(ev, vars, file, line, idx_watch)
  local vars = vars or {}
  local file = file or '?'
  local line = line or 0
  local prefix = ''
  if current_thread ~= 'main' then prefix = '['..tostring(current_thread)..'] ' end
  if ev == events.STEP then
    write(prefix.."Paused at file "..file.." line "..line..' ('..stack_level..')\n')
  elseif ev == events.BREAK then
    write(prefix.."Paused at file "..file.." line "..line..' ('..stack_level..') (breakpoint)\n')
  elseif ev == events.WATCH then
    write(prefix.."Paused at file "..file.." line "..line..' ('..stack_level..')'.." (watch expression "..idx_watch.. ": ["..watches[idx_watch].exp.."])\n")
  elseif ev == events.SET then
    --do nothing
  else
    write(prefix.."Error in application: "..file.." line "..line.."\n")
  end
  if ev ~= events.SET then
    if pausemsg and pausemsg ~= '' then 
		if(type(pausemsg) == "string") then
			write('Message: '..pausemsg..'\n') 
		end
	end
    pausemsg = ''
    IPCDebugger.WriteBreakPoint(file, line);
  end
  
  return vars, file, line
end

local function debugger_loop(ev, vars, file, line, idx_watch)
	write("NPL debugger_loop started\n")
	local eval_env, breakfile, breakline = report(ev, vars, file, line, idx_watch)

	local command, params, args;

	while true do
	write("[DEBUG]> ")
	local msg_in = IPCDebugger.WaitForDebugEvent();

	if(not msg_in) then
		command = "exit"
	else
		command = msg_in.filename;
	end

	local line, filename;
	local param1, param2 = msg_in.param1, msg_in.param2;
	params = msg_in.code;
	if(type(params) == "table") then
		line = params.line;
		filename = NormalizeFileName(params.filename);
	end

	if command == "setb" or command=="break" or command=="b" then
		-- set breakpoint
		if filename and line then
			set_breakpoint(filename,line)
			write("Breakpoint set in file "..filename..' line '..line..'\n')
		else
			write("Bad request\n")
		end
	  
	elseif command == "delb" then
		-- delete breakpoint

		if filename and line then
			remove_breakpoint(filename, line)
			write("Breakpoint deleted from file "..filename..' line '..line.."\n")
		else
			write("Bad request\n")
		end

	elseif command == "delallb" then
		-- delete all breakpoints
		breakpoints = {}
		write('All breakpoints deleted\n')
	  
	elseif command == "listb" then
		-- list breakpoints
		for i, v in pairs(breakpoints) do
			for ii, vv in pairs(v) do
				write("Break at: "..i..' in '..ii..'\n')
			end
		end

	elseif command == "setw" then
		-- set watch expression
		if params.exp then
			local func = loadstring("return(" .. params.exp.. ")")
			local newidx = #watches + 1
			watches[newidx] = {func = func, exp = params.exp}
			write("Set watch exp no. " .. newidx..'\n')
		else
			write("Bad request\n")
		end
	  
	elseif command == "delw" then
		-- delete watch expression
		local index = param1
		if index then
			watches[index] = nil
			write("Watch expression deleted\n")
		else
			write("Bad request\n")
		end

	elseif command == "delallw" then
		--  delete all watch expressions
		watches = {}
		write('All watch expressions deleted\n')

	elseif command == "listw" then
		--list watch expressions
		for i, v in pairs(watches) do
			write("Watch exp. " .. i .. ": " .. v.exp..'\n')
		end
	  
	elseif command == "run" or command == "continue" or command == "r" then
		-- run until breakpoint
		step_into = false
		step_over = false
		eval_env, breakfile, breakline = report(coroutine.yield('cont'))

	elseif command == "step" or command == "s" then
		-- step N lines (into functions)
		local N = param1;
		if(N == 0) then N = 1 end
		step_over  = false
		step_into  = true
		step_lines = N
		eval_env, breakfile, breakline = report(coroutine.yield('cont'))

	elseif command == "over" or command=="next" or command=="n" then
		-- step N lines (over functions)
		local N = param1;
		if(N == 0) then N = 1 end
		step_into  = false
		step_over  = true
		step_lines = N
		step_level = stack_level
		eval_env, breakfile, breakline = report(coroutine.yield('cont'))
	  
	elseif command == "out" then
		-- step N lines (out of functions)
		local N = param1;
		if(N == 0) then N = 1 end
		step_into  = false
		step_over  = true
		step_lines = 1
		step_level = stack_level - tonumber(N or 1)
		eval_env, breakfile, breakline = report(coroutine.yield('cont'))

	elseif command == "goto" then
		-- step until reach line
		local N = param1;
		if(N == 0) then N = 1 end
		if N then
			step_over  = false
			step_into  = false
			if has_breakpoint(breakfile,N) then
				eval_env, breakfile, breakline = report(coroutine.yield('cont'))
			else
				local bf = breakfile
				set_breakpoint(breakfile,N)
				eval_env, breakfile, breakline = report(coroutine.yield('cont'))
				if breakfile == bf and breakline == N then 
					remove_breakpoint(breakfile,N) 
				end
			end
		else
			write("Bad request\n")
		end

	elseif command == "set" then
		-- set/show context level
		local level = param1
		if level~=0 then
			eval_env, breakfile, breakline = report(coroutine.yield(level))
		end
		if eval_env.__VARSLEVEL__ then
			write('Level: '..eval_env.__VARSLEVEL__..'\n')
		else
			write('No level set\n')
		end
	  
	elseif command == "vars" then
		-- list context variables
		local depth = param1
		if(depth == 0) then depth = 1 end
		dumpvar(eval_env, depth+1, 'variables')

	elseif command == "glob" then
		-- list global variables
		local depth = param1
		if(depth == 0) then depth = 1 end
		dumpvar(eval_env.__GLOBALS__,depth+1,'globals')

	elseif command == "fenv" then
		-- list function environment variables
		local depth = param1
		if(depth == 0) then depth = 1 end
		dumpvar(eval_env.__ENVIRONMENT__,depth+1,'environment')
	  
	elseif command == "ups" then
		-- list upvalue names
		dumpvar(eval_env.__UPVALUES__,2,'upvalues')

	elseif command == "locs" then
		-- list locals names
		dumpvar(eval_env.__LOCALS__,2,'upvalues')

	elseif command == "what" then
	  -- show where a function is defined
		if args and args ~= '' then
			local v = eval_env
			local n = nil
			for w in string.gmatch(args,"[%w_]+") do
				v = v[w]
				if n then n = n..'.'..w else n = w end
				if not v then break end
			end
			if type(v) == 'function' then
				local def = debug.getinfo(v,'S')
				if def then
					write(def.what..' in '..def.short_src..' '..def.linedefined..'..'..def.lastlinedefined..'\n')
				else
					write('Cannot get info for '..v..'\n')
				end
			else
				write(tostring(v)..' is not a function\n')
			end
		else
			write("Bad request\n")
		end

	elseif command == "dump" or command=="print" or command=="p" then
		--  dump a variable
		local name, depth = params.name, params.depth
		if name ~= '' then
			if depth == '' or depth == 0 then depth = nil end
			depth = tonumber(depth or 1)
			local v = eval_env
			local n = nil
			for w in string.gmatch(name,"[^%.]+") do     --get everything between dots
				if tonumber(w) then
					v = v[tonumber(w)]
				else
					v = v[w]
				end
				if n then n = n..'.'..w else n = w end
				if not v then break end
			end
			dumpvar(v,depth+1,n)
		else
			write("Bad request\n")
		end

	elseif command == "show" or command=="list" or command=="l" then
		--  show file around a line or the current breakpoint
		local line, file, before, after = params.line, params.file, params.before, params.after;
		if before == 0 then before = 10     end
		if after  == 0 then after  = before end
		
		if file ~= '' and file ~= "=stdin" then
			show(file,line,before,after)
		else
			write('Nothing to show\n')
		end

	elseif command == "poff" then
		-- turn pause command off
		pause_off = true
	  
	elseif command == "pon" then
		-- turn pause command on
		pause_off = false
	  
	elseif command == "tron" then
		-- turn tracing on/off
		local option = getargs('S')
		trace_calls   = false
		trace_returns = false
		trace_lines   = false
		if strfind(option,'c') then trace_calls   = true end
		if strfind(option,'r') then trace_returns = true end
		if strfind(option,'l') then trace_lines   = true end

	elseif command == "trace" or command=="bt" or command=="backtrace" then
		-- dump a stack trace
		trace(eval_env.__VARSLEVEL__)

	elseif command == "info" then
		-- dump all debug info captured
		info()
	  
	elseif command == "pause" then
		-- not allowed in here
		write('pause() should only be used in the script you are debugging\n')

	elseif command == "exit" or command=="finish" or command=="f" or command=="Detach" then
		-- exit debugger
		return 'stop'

	elseif command == 'exec' then
	  -- exec code in current location
		local code;
		if(type(params) == "table") then
			code = params.name
		else	
			code = params
		end
		local ok, func = pcall(loadstring,code)
		if func == nil then
			IPCDebugger.Dump("Compile error: "..line..'\n')
		elseif not ok then
			IPCDebugger.Dump("Compile error: "..func..'\n')
		else
			setfenv(func, eval_env)
			local res = {pcall(func)}
			if res[1] then
				if res[2] then
					table.remove(res,1)
					for _,v in ipairs(res) do
						IPCDebugger.Dump(tostring(v))
						IPCDebugger.Dump('\t')
					end
					IPCDebugger.Dump('\n')
				else	
					IPCDebugger.Dump('NPL Expression executed without return value\n');
				end
				--update in the context
				eval_env, breakfile, breakline = report(coroutine.yield(0))
			else
			  IPCDebugger.Dump("Run error: "..res[2]..'\n')
			end
		end
	end
	end

end

--
-- Starts/resumes a debug session
--
function IPCDebugger.pause(x)
  if pause_off then return end               --being told to ignore pauses
  pausemsg = x or 'pause'
  local lines
  local src = getinfo(2,'short_src')
  if src == "stdin" then
    lines = 1   --if in a console session, stop now
  else
    lines = 2   --if in a script, stop when get out of pause()
  end
  if started then
    --we'll stop now 'cos the existing debug hook will grab us
    step_lines = lines
    step_into  = true
  else
    coro_debugger = cocreate(debugger_loop)  --NB: Use original coroutune.create
    --set to stop when get out of pause()
    trace_level = 0
    step_level  = 0
    stack_level = 1
    step_lines = lines
    step_into  = true
    started    = true
    debug.sethook(debug_hook, "lcr")         --NB: this will cause an immediate entry to the debugger_loop
  end
end

-- start the debug hook but does not pause.
function IPCDebugger.Attach()
	log("NPL debugger attached\n")
	IPCDebugger.Write({filename="Attached", type=debug_events_enum.ATTACHED, code = {
		desc = "NPL debugger 1.0 attached\n",
		workingdir = ParaIO.GetCurDirectory(0),
	}});
	
	if(not started) then
		coro_debugger = cocreate(debugger_loop)  --NB: Use original coroutune.create
		
		trace_level = 0
		step_level  = 0
		stack_level = 5 -- this just avoid caller stack to be negative. 
		step_lines = 0;
		started = true;
		
		debug.sethook(debug_hook, "lcr")         --NB: this will cause an immediate entry to the debugger_loop
	end
end

-- stop the debug hook. 
function IPCDebugger.Detach()
	log("NPL debugger detached\n")
	if(started) then
		started = false;
		debug.sethook() 
		
		-- remove all break points
		breakpoints = {};
	end
	-- send back to confirm detach. 
	IPCDebugger.Write({filename="Detach"});
end

--shows the value of the given variable, only really useful
--when the variable is a table
--see dump debug command hints for full semantics
function IPCDebugger.dump(v,depth)
  dumpvar(v,(depth or 1)+1,tostring(v))
end

local _traceback = debug.traceback       --note original function

--override standard function
debug.traceback = function(x)
  local assertmsg = _traceback(x)        --do original function
  IPCDebugger.pause(x)                   --let user have a look at stuff
  return assertmsg                       --carry on
end

-- this one is solely for testing
local function activate()
	if(main_state == nil) then
		-- only for testing
		main_state = 0;
		-- start it when this file is loaded. 
		IPCDebugger.Start();
	else
		local i = 0;
		i = i+1;
		i = i-1;
		local function TestStepInto() 
			i = i+1;
			i = i+1;
		end
		i = i+1;
		-- commonlib.echo(i);
		i = i+1;
		TestStepInto();
		i = i+1;
		TestStepInto();
		-- log(i.." tick \n");	
	end
end
NPL.this(activate)