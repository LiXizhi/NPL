-- Author: Li,Xizhi
-- Date: 2006-5
-- Desc: This sample code demostrate character simulation in ParaEngine. two NPCs and GirlPC1 character is created at position near (191, 101)
-- user can click on NPC0 and NPC1. NPCs will wave at and face to the nearest player when it perceives it.
-- NPL.load("(gl)script/samples/AI_script.lua");

local PlayerX = 191
local PlayerY = 101

--create asset or resources used in this scene
ParaAsset.LoadParaX("HeroAsset", "Character/Human/Female/HumanFemale.x");

local player;
local playerChar;

--create two NPCs.
local i=0;
for i=0, 1 do
	
	player = ParaScene.CreateCharacter ("NPC"..i, "HeroAsset", "", true, 0.35, 3.14159, 1.0);
	player:SetPosition(PlayerX, 0, PlayerY+i);
	player:SnapToTerrainSurface(0);
	
	player:SetSentientField(0, true);  -- make this sentient to group ID 0
	player:SetGroupID(1); -- set group 1
	player:SetPerceptiveRadius(5);
	player.onperceived = ";NPC_onperceived();";	-- called when object perceived other objects in its perceptive radius..
	player.onclick = ";NPC_onclick();";	-- called when user clicked
	player.onframemove = ";NPC_onframemove();"; -- called when object is sentient
	
	ParaScene.Attach(player);
	playerChar = player:ToCharacter();
	playerChar:LoadStoredModel(213);
	playerChar:AssignAIController("face", "true");
	
	-- a sequence can be read from file or database; here I just hard-coded them.
	local s = player:ToCharacter():GetSeqController();
	s:BeginAddKeys();
	s:Lable("start");
	s:PlayAnim("EmotePoint");
	s:Wait(3);
	s:Turn(0);
	s:WalkTo(10,0,0);
	s:Wait(3);
	s:Turn(-1.57);
	s:RunTo(0,0,10);
	s:Turn(3.14);
	--s:Pause();
	s:WalkTo(-5,0,0);s:Jump();s:RunTo(-5,0,0);
	s:Turn(1.57);
	s:MoveTo(0,0,-10);
	s:Goto("start");
	s:Exec(";NPC_SEQ_EXEC_Test()");
	s:EndAddKeys();
end

-- create the GirlPC
player = ParaScene.CreateCharacter ("girlPC1", "HeroAsset", "", true, 0.35, 3.9, 1.0);
player:SetPosition(PlayerX-3, 0, PlayerY+3);
player:SnapToTerrainSurface(0);
player:SetGroupID(0); -- set group 0
player:SetPerceptiveRadius(30);
ParaScene.Attach(player);
playerChar = player:ToCharacter();
playerChar:LoadStoredModel(213);
playerChar:SetFocus();

function NPC_SEQ_EXEC_Test()
	local player = ParaScene.GetObject(sensor_name);
	if(player:IsValid() == true) then 
		local s = player:ToCharacter():GetSeqController();
		s:Suspend();
	end
end

function NPC_onperceived()
	-- do your AI code here.
	local player = ParaScene.GetObject(sensor_name);
	local bResume=true;
	if(player:IsValid() == true) then 
		
		local nCount = player:GetNumOfPerceivedObject();
		for i=0,nCount-1 do
			local gameobj = player:GetPerceivedObject(i);
			if(gameobj:DistanceTo(player) < 5) then
				local s = player:ToCharacter():GetSeqController();
				s:Suspend();
				player:ToCharacter():PlayAnimation("EmoteWave");
				s:Turn(gameobj:GetFacing()+3.14);
				bResume = false;
				break;
			end
		end
	end
	if(bResume == true) then
		local s = player:ToCharacter():GetSeqController();
		s:Resume();
	end
end

function NPC_onframemove()
	-- do your AI code here.
end

function NPC_onclick()
	if(mouse_button=="left") then
		_guihelper.MessageBox("Hello: I am "..sensor_name);
	end
end