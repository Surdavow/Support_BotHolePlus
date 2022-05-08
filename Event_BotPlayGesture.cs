//Bot Play Gesture From Brick
//By: Gothboy77 BLID 14329
//-----------------------------
//Create Brick Gesture List
function createBrickGestureList()
{
	%count = -1;
	%list[%count++] = "Ani_activate2 2";
	%list[%count++] = "Ani_rotccw 3";
	%list[%count++] = "Ani_undo 4";
	%list[%count++] = "Ani_wrench 5";
	%list[%count++] = "Ani_talk 6";
	%list[%count++] = "Ani_head 7";
	%list[%count++] = "Ani_headReset 8";

	%list[%count++] = "Mov_activate 1";
	%list[%count++] = "Mov_attack 9";
	%list[%count++] = "Mov_sit 10";
	%list[%count++] = "Mov_crouch 11";
	%list[%count++] = "Mov_jump 12";
	%list[%count++] = "Mov_jet 13";

	%list[%count++] = "Emo_alarm 14";
	%list[%count++] = "Emo_love 15";
	%list[%count++] = "Emo_hate 16";
	%list[%count++] = "Emo_confusion 17";
	%list[%count++] = "Emo_bricks 18";

	%list[%count++] = "Arm_ArmUp 19";
	%list[%count++] = "Arm_BothArmsUp 20";
	%list[%count++] = "Arm_ArmsDown 21";

	%total = 0;
	
	%catList = "";
	
	for(%a = 0; %a <= %count; %a++)
	{
		%gesture = %list[ %a ];
		
		%nGest = getWord( %gesture, 1 );
		
		%preGest = strreplace(%gesture,"_"," ");
		%preGest = getWord(%preGest,1);
		
		$hBotGest++;
		$holeBotGestures[ %nGest ] = %preGest;
		
		%catList = %catList SPC %gesture;
	}
	//Define the list
	%fList = "Root 0" @ %catList;
	//Register New Event
	registerOutputEvent(fxDTSBrick, "botPlayGesture", list SPC %fList, 0);
}
//Create the same default list, but for the brick
createBrickGestureList();
//Core Function
function FxDTSBrick::botPlayGesture(%this,%list)
{
	//The brick needs a bot
	if(!isObject(%this.hBot))
		return;
	//Get variables
	%obj = %this.hBot;
	%gest = $holeBotGestures[%list];
	//Check List
	if(%list == 0)
	{
		%obj.playThread(3,root);
		%obj.hResetMoveTriggers();
		return;
	}
	//Arm Thread
	if(%gest $= "ArmUp")
	{
		%obj.hDefaultThread = "armReadyRight";
		%obj.playThread(1, "armReadyRight");
		return;
	}
	if(%gest $= "BothArmsUp")
	{
		%obj.hDefaultThread = "ArmReadyBoth";
		%obj.playThread(1, "armReadyBoth");
		return;
	}
	if(%gest $= "ArmsDown")
	{
		%obj.hDefaultThread = 0;
		%obj.playThread(1,"root");
		return;
	}
	//Head Thread (stop laughing because it rhymes)
	if(%gest $= "head" )
	{
		%obj.hRandomHeadTurn();
		return;
	}
	if(%gest $= "headReset" )
	{
		%obj.hResetHeadTurn();
		return;
	}
	//Other Actions
	if(%gest $= "sit")
	{
		serverCmdSit(%obj);
		return;
	}
	if(%gest $= "crouch")
	{
		%obj.setCrouching(1);
		return;
	}
	if(%gest $= "jump")
	{
		%obj.hJump(200);
		return;
	}
	if(%gest $= "jet")
	{
		%obj.hJet(2000);
	}
	if(%gest $= "attack")
	{
		%obj.setImageTrigger(0,1);
		%image = %obj.getMountedImage( 0 );
		
		cancel( %obj.hAttackChargeSchedule );
		
		if( %image.isChargeWeapon )	
			%obj.hAttackChargeSchedule = %obj.scheduleNoQuota( 1000, setImageTrigger, 0, 0 );
		else if( %image.melee )
			%obj.hAttackChargeSchedule = %obj.scheduleNoQuota( 32, setImageTrigger, 0, 0 );
		else
			%obj.setImageTrigger( 0, 0 );
			
		return;
	}
	//Emotes
	if(%gest $= "love")
	{
		%obj.emote("loveImage");
		return;
	}
	if(%gest $= "alarm")
	{
		%obj.emote("alarmProjectile");
		return;
	}
	if(%gest $= "confusion")
	{
		%obj.emote("wtfImage");
		return;
	}
	if(%gest $= "hate")
	{
		%obj.emote("hateImage");
		return;
	}
	if(%gest $= "bricks")
	{
		%obj.emote("BSDProjectile");
		return;
	}
	if(%gest $= "activate")
	{
		%obj.activateStuff();
		return;
	}
	//DO IT NOW!
	%obj.playThread(3,%gest);
}