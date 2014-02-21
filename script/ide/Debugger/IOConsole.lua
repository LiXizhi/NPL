--[[
Title: Console
Author(s): LiXizhi
Date: 2010/3/17
Desc: A simple command line console system for win32 GUI. io and os library will be loaded. 
The console is displayed immediately after loading this file. 
Use Lib:
-------------------------------------------------------
NPL.load("(gl)script/ide/Debugger/IOConsole.lua");
commonlib.console.write("HelloWorld\n")
commonlib.console.SetTextAttribute(1+128);
commonlib.console.write("Intensisfied Blue text\n")
commonlib.console.SetTextAttribute(2+64);
commonlib.console.write("Green on red background\n")
commonlib.console.SetTextAttribute();
commonlib.console.write("default text\n")
------------------------------------------------------
]]
ParaEngine.GetAttributeObject():CallField("AllocConsole");
if(not os or not io) then 
	-- this ensures that os and io libs are loaded. 
	luaopen_profiler();
end	

local console = commonlib.gettable("commonlib.console");

-- the last text attribute. 
local last_text_attr;
local att = ParaEngine.GetAttributeObject();

-- text_attrs
console.text_attrs = {
	FOREGROUND_BLUE      = 1, -- text color contains blue.
	FOREGROUND_GREEN     = 2, -- text color contains green.
	FOREGROUND_RED       = 4, -- text color contains red.
	FOREGROUND_INTENSITY = 8, -- text color is intensified.
	BACKGROUND_BLUE      = 16, -- background color contains blue.
	BACKGROUND_GREEN     = 32, -- background color contains green.
	BACKGROUND_RED       = 64, -- background color contains red.
	BACKGROUND_INTENSITY = 128, -- background color is intensified.
	COMMON_LVB_LEADING_BYTE    = 256, -- Leading Byte of DBCS
	COMMON_LVB_TRAILING_BYTE   = 512, -- Trailing Byte of DBCS
	COMMON_LVB_GRID_HORIZONTAL = 1024, -- DBCS: Grid attribute: top horizontal.
	COMMON_LVB_GRID_LVERTICAL  = 2048, -- DBCS: Grid attribute: left vertical.
	COMMON_LVB_GRID_RVERTICAL  = 4096, -- DBCS: Grid attribute: right vertical.
	COMMON_LVB_REVERSE_VIDEO   = 4000, -- DBCS: Reverse fore/back ground attribute.
	COMMON_LVB_UNDERSCORE      = 16384, -- DBCS: Underscore.
	COMMON_LVB_SBCSDBCS        = 32768, -- SBCS or DBCS flag
}
-- default text_attr. 
default_text_attr = (1+2+4);

-- write to console output
-- e.g. console.write("hello world\n")
console.write = io.write;

-- read from console input
-- e.g. local line = io.read("*line")
console.read = io.read;

-- it will affect all text before it.
-- @param text_attr: such as (console.text_attrs.FOREGROUND_BLUE + console.text_attrs.BACKGROUND_GREEN)
--  if nil, it will default to default text color which is unintensified white on black background. 
function console.SetTextAttribute(text_attr)
	if(text_attr ~= last_text_attr) then
		last_text_attr = text_attr;
		att:SetField("ConsoleTextAttribute", text_attr or default_text_attr);
	end
end
