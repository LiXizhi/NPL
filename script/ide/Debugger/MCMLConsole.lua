--[[
Title: mcml based UI Console
Author(s): LiXizhi
Date: 2010/8/30
Desc: A simple mcml browser based console system for debugging mostly server side applications.
e.g. Game Server can be started with a mcml console. 
Use Lib:
-------------------------------------------------------
NPL.load("(gl)script/ide/Debugger/MCMLConsole.lua");
commonlib.mcml_console.show(bShow, mcml_url);
------------------------------------------------------
]]
NPL.load("(gl)script/ide/IDE.lua");
NPL.load("(gl)script/ide/Debugger/IPCDebugger.lua");
System = commonlib.inherit(Map3DSystem);

local mcml_console = commonlib.gettable("commonlib.mcml_console");

-- toggle show of mcml based console, it will fill the entire window. 
-- @param bShow: nil to toggle. 
-- @param url: the url to display. this can be nil. 
function mcml_console.show(bShow, url)
	-- load game server console
	local ctl = CommonCtrl.GetControl("MCMLBrowserWnd.mcmlconsole");
	if(not ctl) then
		NPL.load("(gl)script/ide/WindowFrame.lua");
		NPL.load("(gl)script/kids/3DMapSystemAnimation/AnimationManager.lua");
		NPL.load("(gl)script/kids/3DMapSystemApp/mcml/BrowserWnd.lua");
		-- NPL.load("(gl)script/kids/3DMapSystemApp/AppManager.lua");
		-- use the taurus theme
		NPL.load("(gl)script/apps/Taurus/DefaultTheme.lua");
		Taurus_LoadDefaultTheme();
		
		LOG.std(nil, "system", "console", "mcml console is loaded")

		ctl = Map3DSystem.mcml.BrowserWnd:new{
			name = "MCMLBrowserWnd.mcmlconsole",
			alignment = "_fi",
			left=0, top=0,
			width = 0,
			height = 0,
			parent = nil,
			DisplayNavBar = true,
			window = nil
		};
	else
		ctl.parent = _parent;
	end
	ctl:Show(bShow)
	if(url) then
		ctl:Goto(url);
	end
end