--[[
Title: Inter Process Communication
Author(s): LiXizhi
Date: 2010/4/27
Desc: it wraps the raw NPL IPC interface and provide asynchronous and synchronous programming API. 
Note1: A very important thing to note is that you generally can not use the same queue for read and write. A queue object is either readonly or writeonly. 
We need to create 2 queue object with the same queue name to read/write from the queue. 
Note2: sometimes a message queue can be corrupted, and we need to remove and create again in order to reuse the queue name. So to specify queue_usage 0. 
Use Lib:
-------------------------------------------------------
NPL.load("(gl)script/ide/IPC.lua");

-- Example 1: synchronous call with blocking API, useful for remote Debug Engine, etc.
-- Please note ipc_queue_reader and ipc_queue_writer can exist in different process and we also show two ways to create queue.
local ipc_queue_reader = IPC.CreateGetQueue("MyQueue", 0)
-- please note, ParaIPC.CreateGetQueue or ParaIPC.ParaIPCQueue will create another queue object with the same queue name, otherwise the code will not work in the same process. 
local ipc_queue_writer = ParaIPC.CreateGetQueue("MyQueue", 2)

-- send a message to the queue
ipc_queue_writer:try_send({
	method = "NPL1", -- string [optional] default to "NPL"
	from = "writer", -- string [optional] who is sending this message.
	type = 11, -- number [optional] default to 0. 
	param1 = 12, -- number [optional] default to 0. 
	param2 = 13, -- number [optional] default to 0. 
	filename = "", -- string [optional] the file name 
	code = {data=123,}, -- string or table [optional], if method is "NPL", code should be a pure table or nil.
	priority = 1, -- number [optional] default to 0. Message priority
})

-- read a message from the queue. This is a blocking call. 
local out_msg = {};
ipc_queue_reader:receive(out_msg);
commonlib.echo(out_msg);

-- Example 2: using asynchrounous calls which resembles NPL.activate
	
-- start the IPC server that listens for queue "MyIPCServer"
local server_queue = IPC.StartNPLQueueListener("MyIPCServer", 2, 500, {["script/ide/IPC.lua"] = "script/ide/IPC.lua", ["ipc_shortname"] = "script/ide/IPC.lua"});

-- In remote or local process, we can activate a file like below. 
IPC.activate("MyIPCServer", nil, "script/ide/IPC.lua", {data=1});
IPC.activate("MyIPCServer", "from_name", "ipc_shortname", {data=2});
-------------------------------------------------------
]]
NPL.load("(gl)script/ide/timer.lua");

if(not IPC) then IPC={}; end

local queues = {};

-- create get a queue using our manager in this NPL state only. Duplicated calls with the same queue_name will create the same object 
-- Alternatively, we can use ParaIPC.CreateGetQueue(queue_name, usage), where the queue object is shared by all NPL states. 
-- @param usage: nil defaults to 2, which is open or create. 0 means create only(it will remove previous queue before open a new one)
function IPC.CreateGetQueue(queue_name, usage)
	local ipc_queue = queues[queue_name];
	if(ipc_queue) then
		return ipc_queue;
	else
		-- 2 means create and open
		ipc_queue = ParaIPC.ParaIPCQueue(queue_name, usage or 2);
		queues[queue_name] = ipc_queue;
	end
	return ipc_queue;
end

local ipc_listeners = {};
-- start listen to a NPL queue, so that anyone using IPC.activate() function will be processed by this calling process. 
-- This is like start a IPC server and listen for request.
-- Internally, it will start a timer with polling_interval, and check for incoming messages, and dispatch message by activating NPL file as in msg.filename and msg.code
-- @param queue_name: the name of the queue. 
-- @param queue_usage: nil defaults to 2, which is open or create. 0 means create only(it will remove previous queue before open a new one)
-- @param polling_interval: timer interval to check messages in milliseconds. if nil it is fastest. 
-- @param trusted_filemap: nil or a table of key,value pairs {filename=filename}. we will only activate file that exist in this file map.
--   if nil, we will allow any filename, including file with any npl state header, such as "(abc)efg:test.lua"
--   we can give abbrieviated filename. such as {["ipc"] = "script/ide/IPC.lua", ["script/ide/IPC.lua"] = "script/ide/IPC.lua"}
-- @return ipc_queue object
function IPC.StartNPLQueueListener(queue_name, queue_usage, polling_interval, trusted_filemap)
	
	polling_interval = polling_interval or 33
	ipc_listeners[queue_name] = ipc_listeners[queue_name] or {};
	local ipc_listener = ipc_listeners[queue_name];
	
	-- create an anonymous queue object to listen for incoming messages, if not. 
	local ipc_queue = ipc_listener.ipc_queue;
	if(not ipc_queue) then
		ipc_queue = ParaIPC.ParaIPCQueue(queue_name, queue_usage or 2);
		ipc_listener.ipc_queue = ipc_queue; 
	end		
	
	-- start timer to process the message. 
	ipc_listener.mytimer = ipc_listener.mytimer or commonlib.Timer:new({callbackFunc = function(timer)
		local out_msg = {};
		while(ipc_queue:try_receive(out_msg) == 0) do
			--if(out_msg.method == "NPL") then
				local filename;
				if(not trusted_filemap) then
					filename = out_msg.filename;
				else
					filename = trusted_filemap[out_msg.filename];
				end
				if(filename) then
					NPL.activate(filename, out_msg.code);
				end
			--end
		end
	end})
	ipc_listener.mytimer:Change(polling_interval, polling_interval)
	return ipc_queue;
end

-- using this function to activate a remote or local file via interprocess message. 
-- @param queue_name: IPC queue name
-- @param from: the sender's queue name. this is optional. But it hints the receiver to which queue it should send back reply. 
-- @param filename: the file name in the remote or local process who has called IPC.StartNPLQueueListener
-- @param priority: if nil, default to 0. 
-- @return 0 if succeed. 
function IPC.activate(queue_name, from, filename, msg_table, priority)
	local ipc_queue = IPC.CreateGetQueue(queue_name);
	if(ipc_queue) then
		return ipc_queue:try_send({
				method = "NPL", 
				from = from, 
				filename = filename,
				code = msg_table,
				priority = priority,
			})
	end
end

local function activate()
	log("IPC.lua received:");
	commonlib.echo(msg);
end
NPL.this(activate);