--[[
Title: https://github.com/LiXizhi/NPLRuntime/wiki
Author: LiXizhi
Date: 2016/3/16
Desc: Example multiple thread used in wiki

The `log.txt` will look like something as below
```
2016-03-16 6:22:00 PM|T1|info|MultiThread|hello world from thread T1
2016-03-16 6:22:01 PM|T3|info|MultiThread|hello world from thread T3
2016-03-16 6:22:03 PM|T2|info|MultiThread|hello world from thread T2
2016-03-16 6:22:03 PM|T5|info|MultiThread|hello world from thread T5
2016-03-16 6:22:04 PM|T4|info|MultiThread|hello world from thread T4
```
-----------------------------------------------
NPL.activate("(gl)script/test/TestMultithread.lua");
-----------------------------------------------
]]
NPL.load("(gl)script/ide/commonlib.lua");

local L = function(text) return text end;

local function Start()
	local self = nil;
	for i=1, 0x05 do
		local thead_name = "T"..i;
		NPL.CreateRuntimeState(thead_name, 0):Start();
		NPL.activate(format("(%s)script/test/TestMultithread.lua", thead_name), {
			text = L"hello world", 
			sleep_time = math.random()*5,
		});
	end
end

local isStarted;
local function activate()
   if(msg and msg.text) then
      -- sleep random seconds to simulate heavy task
	  ParaEngine.Sleep(msg.sleep_time);
	  LOG.std(nil, "info", "MultiThread", "%s from thread %s", msg.text,  __rts__:GetName());
   elseif(not isStarted) then
      -- initialize on first call 
      isStarted = true;
      Start();
   end
end

NPL.this(activate);