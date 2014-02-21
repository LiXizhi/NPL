--[[
Author: LiXizhi
Date: 2010/7/14
Desc: Profiler for finding the performance hot spot.
---++ Using NPL Profiler
Please see TestNPLProfiler.lua for examples. 

These test functions can be used in test code to evalucate code performance.
It can also be efficiently injected into production code to programmatically turn on 
and check code performance. When perf is not enabled, there is almost zero penalty incurred from the perf code.

---++ Perf Result
| max_fps(inner) | the inner loop function FPS. It will use the min_value of all outer loops. This is most accurate, since there might not be CPU context switch or script GC. |
| fps(inner) | average inner loop function FPS for all runs. |
| fps | average outer loop function FPS for all runs. |
| cur_value | last value in outer loop. |
| count | total outer loop count, i.e. begin/end pairs | 
| min_value | min outer loop time | 
| max_value | max outer loop time | 
-----------------------------------------------
NPL.load("(gl)script/ide/Debugger/NPLProfiler.lua");
local npl_profiler = commonlib.gettable("commonlib.npl_profiler");

-- turn on perf
npl_profiler.perf_show();

-- method1: one can put following perf function pair in critical function. 
npl_profiler.perf_begin("test")
-- code here
npl_profiler.perf_end("test")

-- method2: one can evaluate how fast some NPC functions by writing
local j;
for j=1, 10 do 
	npl_profiler.perf_begin("test1")
	do
		-- code here will be perfed
		local i;
		for i = 1, 10000 do
			local player = ParaScene.GetPlayer();
			local x, y, z = player:GetPosition();
		end
	end
	npl_profiler.perf_end("test1")
end
-- dump perf result of test1 to log
LOG.info(npl_profiler.perf_get("test1"));


-- method3: to simplify method2
npl_profiler.perf_func("test2", function() 
	local player = ParaScene.GetPlayer();
	local x, y, z = player:GetPosition();
end, 10, 10000);

--INFO:GetGlobal:A.B.C:max_fps(inner):5000004,avg(inner):0.000000250,fps(inner):4000000cur_value=0.002000000, avg_value=0.002500000, count=10, fps=400, cfps=400, min_value=0.0020000, max_value=0.0030000
--INFO:LOG.info:max_fps(inner):66667,avg(inner):0.000015667,fps(inner):63830cur_value=0.016000003, avg_value=0.015666664, count=3, fps=64, cfps=64, min_value=0.0150000, max_value=0.0160000
--INFO:baseline:i=i+1:max_fps(inner):10000008,avg(inner):0.000000150,fps(inner):6666667cur_value=0.001999999, avg_value=0.001500000, count=10, fps=667, cfps=667, min_value=0.0010000, max_value=0.0020000
--INFO:ParaScene.GetPlayer+GetPosition:max_fps(inner):1428573,avg(inner):0.000000960,fps(inner):1041667cur_value=0.007000014, avg_value=0.009600000, count=10, fps=104, cfps=104, min_value=0.0070000, max_value=0.0180000

-- this will dump all stats by passing nil
LOG.info(npl_profiler.perf_get());


-- Following will toggle profiler on/off (discard old data)
commonlib.npl_profiler.perf_enable();
commonlib.npl_profiler.perf_show(nil, true);
-----------------------------------------------
]]

local npl_profiler = commonlib.gettable("commonlib.npl_profiler");
local getAccurateTime = ParaGlobal.getAccurateTime;
local table_insert = table.insert;

local LOG = LOG;

-- disable by default until perf_enable or perf_show is called
local perf_enabled = false;
-- performance stats
local perf_stats = {};

-- clear all stat
function npl_profiler.perf_reset()
	perf_stats = {};
end

-- temporarily enable or disable perf. Please note this function must not be called between perf_begin and perf_end
-- @param bEnable: boolean or nil. if nil it mean toggle
function npl_profiler.perf_enable(bEnable)
	if(bEnable == nil) then
		perf_enabled = not perf_enabled;
	else
		perf_enabled = bEnable;
	end
	if(perf_enabled) then
		LOG.info("------------- profiler started--------------")
		LOG.show("_perf_enabled", true)
	else
		npl_profiler.perf_reset();
		LOG.show("_perf_enabled", false)
	end
end

-- begin performance of a given code block
-- It must be called in pairs in perf_begin(x), perf_end(x). 
-- nested calls with the same name are supported, where only the outer is calculated. 
-- @param bRecursive: true to handle nested calls, default to nil which does not handle. Both begin/end function should handle the same recursive calls.
function npl_profiler.perf_begin(name, bRecursive)
	if(not perf_enabled) then
		return;
	end
	local stat = perf_stats[name]
	if(not stat) then
		local curTime = getAccurateTime();
		stat = {
			start_time = curTime,
			end_time = 0,
			last_begin = curTime,
			min_value = 9999,
			max_value = 0,
			avg_value = 0,
			count = 0,
			begin_count = 1,
			count = 0,
			last_value = 0, 
			history = {},
		}
		perf_stats[name] = stat;
	else
		if(not bRecursive or stat.begin_count == 0) then
			local curTime = getAccurateTime();
			stat.last_begin = curTime;
		end
		if(bRecursive) then
			stat.begin_count = stat.begin_count + 1;
		else
			stat.begin_count = 1;
		end
	end
end

-- end performance of a given code block
-- It must be called in pairs in perf_begin(x), perf_end(x). 
-- nested calls with the same name are supported, where only the outer is calculated. 
-- @param bRecursive: true to handle nested calls, default to nil which does not handle
function npl_profiler.perf_end(name, bRecursive)
	if(not perf_enabled) then
		return;
	end
	local stat = perf_stats[name]
	if(stat) then
		stat.begin_count = stat.begin_count - 1;
				
		if(stat.begin_count == 0)then
			local curTime = getAccurateTime();
			local delta = curTime - stat.last_begin;
			if(delta < stat.min_value) then
				stat.min_value = delta;
			end
			if(delta > stat.max_value) then
				stat.max_value = delta;
			end
			
			stat.last_value = delta;
			local last_count = stat.count
			-- now calculate last count
			stat.avg_value = (stat.avg_value*last_count + delta) / (last_count+1);
			stat.count = last_count + 1;
			stat.end_time = curTime;
			-- table_insert(stat.history, delta);
		elseif(stat.begin_count < 0)then
			LOG.applog("error: perf_end invoked without matching perf_begin")
		end
	else    
		LOG.applog("error: perf_end invoked without calling perf_begin")
	end
end

-- get all collected info of a given name
function npl_profiler.perf_get(name)
	if(name) then
		return perf_stats[name];
	else
		return perf_stats;
	end
end

-- get perf string
function npl_profiler.perf_getstring(name, bShort)
	local stat = perf_stats[name];
	if(stat) then
		local str
		if(bShort) then
			str = string.format("cur=%.7f,avg=%.7f,cnt=%d,fps=%.0f,cfps=%.2f,min=%.7f,max=%.7f", 
					stat.last_value, stat.avg_value, stat.count, 1/stat.avg_value, stat.count/(stat.end_time-stat.start_time), stat.min_value, stat.max_value);
		else
			str = string.format("cur_value=%.9f, avg_value=%.9f, count=%d, fps=%.0f, cfps=%.0f, min_value=%.7f, max_value=%.7f", 
					stat.last_value, stat.avg_value, stat.count, 1/stat.avg_value, stat.count/(stat.end_time-stat.start_time), stat.min_value, stat.max_value);
			if(stat.loop_count) then
				str = string.format("max_fps(inner):%.0f,avg(inner):%.9f,fps(inner):%.0f", 
					1/stat.min_value*stat.loop_count, stat.avg_value/stat.loop_count, 1/stat.avg_value*stat.loop_count)..str;
			end
		end
		return str;
	end
end

-- this function is used to perf a given function by using a loop nOutLoopTimes*nInnerLoopTimes times
-- @param name: 
function npl_profiler.perf_func(name, func_callback, nOutLoopTimes, nInnerLoopTimes)
	local j;
	nOutLoopTimes = nOutLoopTimes or 1
	nInnerLoopTimes = nInnerLoopTimes or 1
	for j=1, nOutLoopTimes do 
		npl_profiler.perf_begin(name)
		local i;
		for i = 1, nInnerLoopTimes do
			func_callback();
		end
		npl_profiler.perf_end(name)
	end
	npl_profiler.perf_get(name).loop_count = nInnerLoopTimes;
end

local perf_timer;

-- show all perfs in head on UI. It will auto enable perf if not yet. 
-- @param nRefreshRate: default to 1000 milliseconds
-- @param bNoAutoStart: true to disable auto start. 
function npl_profiler.perf_show(nRefreshRate, bNoAutoStart)
	NPL.load("(gl)script/ide/timer.lua");
	if(not bNoAutoStart and not perf_enabled) then
		npl_profiler.perf_enable(true);
	end
	perf_timer = perf_timer or commonlib.Timer:new({callbackFunc = function(timer)
		local name, stat;
		for name, stat in pairs(perf_stats) do
			LOG.show(name, npl_profiler.perf_getstring(name, true));
		end
	end})
	perf_timer:Change(0, nRefreshRate or 1000);
end

-- dump all result to log file
function npl_profiler.perf_dump_result()
	LOG.info("dumping perf result--------------")
	local name, stat;
	for name, stat in pairs(perf_stats) do
		LOG.info(name..":"..npl_profiler.perf_getstring(name));
	end
end