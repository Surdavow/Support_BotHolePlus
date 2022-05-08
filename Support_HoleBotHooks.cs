//-------------------------Hole Bot Hooks Mod--------------------------//
//About:---------------------------------------------------------------//																
//Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis aliquam//
//eros doesn't this look fancy molestie neque consecetur. In ultricies //
//magna ac ipsum tristique elementum. Charlie Sheen Suspendisse Winning//
//ultricies elementum ligula, vel viverra enim semper eu.			   //
//---------------------------------------------------------------------//
//                                                                     //
//By:------------------------------------------------------------------//
//--Robbinson Block----------------------------------------------------//
//--"Someone outta mod the hole bots to make the code less monolithic."//
//---------------------------------------------------------------------//
//----------"... Screw it."--------------------------------------------//
//---------------------------------------------------------------------//
//
//Hole Bot Hooks Mod Goals
//------------------
//Dependent on Bot_Holes
//Easy plugin support.
//Decreased performance(?) for extensibility
//Possibly conflicts with some custom hole bot mods (though likely not)
//

//
// The ScriptObject our new-and-improved bots store their variables in.
//

function HoleBotVarArray_New(%bot)
{
    //I'm not convinced it's worth it to "globalize" the variables this way instead of just recreating them when necessary, but oh well. Now you can hook these too!
    %object = new ScriptObject();
    MissionCleanup.add(%object);
    %object.data = %bot.getDataBlock();
    %object.mount = %bot.getObjectMount();
    %object.wander = %bot.hWander;
    %object.gridWander = %bot.hWander;
    %object.search = %bot.hSearch;
    %object.strafe = %bot.strafe;
    %object.tickrate = %bot.data.hTickRate;
    %object.spastic = %bot.hSpasticLook;
    %object.idleAnim = %bot.hIdleAnimation;
    %object.AFKScale = %bot.hAFKOmeter;
    %object.idle = %bot.hIdle;
    %object.scale = getWord(%bot.getScale(), 0);
    %object.minigame = getMiniGameFromObject(%bot);
    %object.isHost = %bot.spawnBrick.getGroup().client == %object.minigame.owner;
    %object.brickGroup = %bot.spawnBrick.getGroup();
    //Circumstancial variables.
    %object.avoid = 0;
    %object.noStrafe = 0;
    %object.onlyJump = 0;
    %object.isLookingAtPlayer = 0;
    %object.alreadyLooked = 0;
    
    if(!%obj.vars.AFKScale)
    {
        %object.AFKScale = 1;
    }
    return %object;
}

//
// Modified Rodrigo code.
//

package Support_MoreBotHooks
{
    //Main bot loop, called every tick, tick is usually every 3 seconds, every bot uses this function in some way.
    function AIPlayer::hLoop(%obj)
    {
        if(%obj.hooks_hLoop)
        {
            call(%obj.hooks_hLoop, %obj);
            return;
        }
        %datablockHook = %obj.getDataBlock().hooks_hLoop;
        if(%datablockHook)
        {
            call(%datablockHook, %obj);
        }
        else if(%obj.hooks_hLoop | %datablockHook == -1)
        {
            return;
        }
        else
        {
            HoleBot_hLoop(%obj);
        }
    }
    //
    // Bonus package: Deletes the bot's variable object when it dies. The garbage collector likely does this already, but I want to be sure.
    //
	function Armor::onRemove(%this, %obj)
	{
		parent::onRemove(%this, %obj);
		if((%obj.isHoleBot || %obj.getClassname() $= "AIPlayer") && isObject(%obj.vars))
        {
            %obj.vars.delete();
        }
    }
};
activatePackage(Support_MoreBotHooks);

//
// Robb's functions that account for hooks.
//

function HoleBot_hLoop(%obj)
{
	//if we somehow got here and you or your spawnbrick doesn't exist anymore, then poof
	if(!isObject(%obj.spawnBrick))
	{
		%obj.delete();
		return;
	}	
	//Check if we exist, our spawnbrick exists, we're currently active and not dead, we need to exist
	if(!isObject(%obj.spawnbrick) || %obj.isDisabled() || !%obj.hLoopActive)
    {
        return;
    }

    HoleBot_init(%obj);

    //Lay out all the options we have so we can alter them as the function progresses
	%obj.inHoleLoop = 1;
    %obj.vars = HoleBotVarArray_New(%obj);
	
	// if we're in water set our move tolerance to be a bit higher
    HoleBot_waterMovementTolerance(%obj);

	// let's figure out when to get out of the vehicle
    HoleBot_ifMount(%obj);

	%obj.playThread(3,root);
    HoleBot_ifTurret(%obj);
		
	//If we have certain idle actions activate this function gives you back your weapon/fixes your hands
    HoleBot_ifIsSpazzing(%obj);

	//If no tickrate create one, useful for when bots are changed into other datablocks ie horse
	if(!%obj.vars.tickrate)
	{
		%obj.vars.tickrate = 3000;
	}
	// %customLoop = %obj.getDatablock().hCustomLoop;
	
	//if our user isn't in the server, teleport back to spawn and stop doing things
	if(%obj.spawnBrick.itemPosition != 1 && !hBrickClientCheck(%obj.vars.brickGroup))
	{
		%obj.delete();
		return;
	}
	
	//If spawnClose is set, check if we've seen a player, if we don't see one for 1 tick then poof ourselves
    if(HoleBot_ifSpawnClose(%obj) == 1)
    {
        return;
    }
	
	//Again clear movement and reattach jump/crouch images, there is probably a better way to do this
    HoleBot_movementReset(%obj);
	
	// check if we're a jet datablock and we're falling to our death
	HoleBot_jetCheck(%obj);
	
	if(%obj.vars.search)
	{	
		//Radius Check
        HoleBot_ifSearch_Radius(%obj);
		//Fov Check
		HoleBot_ifSearch_FOV(%obj);
	}

	//If there's a custom script tag on the datablock, then call the function
	// if(%customLoop)
    //Lol, I wouldn't have had to make this support script if Rotondo followed through with this idea.
	%obj.getDataBlock().onBotLoop(%obj);

	//If we have a brick target that takes precedence over wandering
    if(HoleBot_ifPathBrick(%obj) == 1)
    {
        %obj.hSched = %obj.scheduleNoQuota(%obj.vars.tickrate + getRandom(0, 750), hLoop);
        return;
    }
    
	%wander_result = HoleBot_ifWander(%obj);
    if(%wander_result == 1)
    {
        //We're returning to our spawn.
        %obj.hSched = %obj.scheduleNoQuota(%obj.vars.tickrate + getRandom(0, 1000), hLoop);
        return;
    }
    else if(%wander_result == 2)
    {
        //We have a target, trigger a moment of realization.
        %obj.hSched = %obj.scheduleNoQuota(%obj.vars.tickrate / 2, hLoop);
        return;
    }
	
	// head turn is out of all
	// random head turn, only if hIdleLookAtOthers is enabled && !%obj.hHeadTurn 
    HoleBot_randomHeadTurn(%obj);
	
	// randomly reset the being sprayed flag so the bot doesn't just spray all the time
    HoleBot_sprayPaintReset(%obj);
	
	HoleBot_ifIdle(%obj);
	
	//%obj.hIsBeingSprayed = 0;
	
	//Pop quiz time, did anyone notice I misspelt emote a few lines back.. er not that it's really a word
    HoleBot_wanderAvoidObstacle(%obj);
	
	//Spazz click check, blocklanders do it why not bots?
    HoleBot_wanderSpazzClick(%obj);
	
	//hmm well apparently emote is an official word, or at least far as I can tell, well it's in the dictionary anyway
	%obj.hSched = %obj.scheduleNoQuota(%obj.vars.tickrate + getRandom(0, 1000), hLoop);
	return;
}

//
// HOW TO USE HOOKS:
//
// Hooks can be specified in two ways: in the datablock or on the armor.
//
// Datablock:
// datablock PlayerData(HoleBot : PlayerStandardArmor)
// {
//      hooks_waterMovementTolerance = "myFunction";
// };
//
// Armor:
// %obj.hooks_waterMovementTolerance = "myFunction";
//
// When a hook is specified these ways, the function with that name will be called and the bot's object passed through as an argument.
// (Note: the hooks "HoleBot_tooFarFromSpawn" and "HoleBot_guardAtSpawn" also include the return distance as an argument, after the bot's object.)
//
// If both the datablock and armor have a call to the same hook, the armor will be prioritized.
// If neither the datablock or armor calls have a function specified but either are set to -1, that portion of code will not be executed.
// If a hook has no value at all (if it hasn't been defined,) the bot will simply go with the default behavior.
//
// Also important is that the local variables that used to reside in hLoop are now replaced with variables in a ScriptObject, "%obj.vars", that is created when the bot spawns.
// Some of the local variables, like "%canJet" and "%minigameHost", were not ported over due to redundancy. (Honestly, a ton of others could go too. Might do that later.)

function HoleBot_init(%obj)
{
    if(%obj.hooks_init)
    {
        call(%obj.hooks_init, %obj);
    }
    if(%obj.vars.data.hooks_init !$= "")
    {
        call(%obj.vars.data.hooks_init, %obj);
    }
}


function HoleBot_waterMovementTolerance(%obj)
{
    if(%obj.hooks_waterMovementTolerance !$= "")
    {
        //The bot's armor has a hook stored. This is always prioritized over a hook applied to the bot's datablock.
        call(%obj.hooks_waterMovementTolerance, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_waterMovementTolerance !$= "";
    if(%datablockHook)
    {
        //No armor hook, but there is one on the datablock.
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_waterMovementTolerance | %datablockHook == -1)
    {
        //If neither have a hook but either are set to -1, stop the function from running.
        return;
    }
    else
    {
        //Rotondo's code.
        %mount = %obj.vars.mount;
        if(%mount && !(%mount.getType() & $TypeMasks::PlayerObjectType))
        {
            %obj.setMoveTolerance(12);
        }
        else if(%obj.getWaterCoverage() != 0 || %mount)
        {
            %obj.setMoveTolerance(3);
        }
        else
        {
            %obj.setMoveTolerance(0.25);
        }
    }
}

function HoleBot_ifMount(%obj)
{
    if(%obj.hooks_ifMount !$= "")
    {
        call(%obj.hooks_ifMount, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifMount;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifMount | %datablockHook == -1)
    {
        return;
    }
    else
    {
        %mount = %obj.vars.mount;
        if(%mount)
        {
            %driver = %mount.getControllingObject();
            
            if(!%driver && !getRandom(0, 2))
            {
                %mount.mountObject(%obj, 0);
            }
            else if(!%driver || checkHoleBotTeams(%obj, %driver))
            {
                %obj.dismount();
            }
            else if(%obj.vars.idle && %driver != %obj && !getRandom(0, 4))
            {
                if(getRandom(0, 1))
                {
                    serverCmdNextSeat(%obj);
                }
                else
                {
                    serverCmdPrevSeat(%obj);
                }
            }
        }
    }
}

function HoleBot_ifTurret(%obj)
{
    if(%obj.hooks_ifTurret !$= "")
    {
        call(%obj.hooks_ifTurret, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifTurret;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifTurret | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.vars.data.isTurret)
        {
            %obj.hShoot = 1;
            %obj.mountImage(hTurretImage, 0);
        }
    }
}

function HoleBot_ifIsSpazzing(%obj)
{
    if(%obj.hooks_ifIsSpazzing !$= "")
    {
        call(%obj.hooks_ifIsSpazzing, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifIsSpazzing;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifIsSpazzing | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.hIsSpazzing)
        {
            if(isObject(%obj.hLastWeapon))
            {
                %obj.setWeapon(%obj.hLastWeapon);
                fixArmReady(%obj);
            }
            else
            {
                %obj.unMountImage(0);
                fixArmReady(%obj);
            }
            %obj.hIsSpazzing = 0;
            if(strLen(%obj.hDefaultThread))
            {
                %obj.playThread(1,%obj.hDefaultThread);
            }
        }
        else if(isObject(%obj.getMountedImage(0)))
        {
            %obj.hLastWeapon = %obj.getMountedImage(0);
        }
    }
}

function HoleBot_ifSpawnClose(%obj)
{
    if(%obj.hooks_ifSpawnClose !$= "")
    {
        call(%obj.hooks_ifSpawnClose, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifSpawnClose;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifSpawnClose | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.vars.data.hSpawnClose)
        {
            if(!doHoleSpawnDetect(%obj))
            {
                %obj.hCSpawnD++;
            }
            else
            {
                %obj.hCSpawnD = 0;
            }

            if(%obj.hCSpawnD >= 1) //egh:hLoop
            {
                %spawnBrick = %obj.vars.spawnBrick;
                %spawnBrick.unSpawnHoleBot();
                cancel(%spawnBrick.hSpawnDetectSchedule);
                %spawnBrick.hSpawnDetectSchedule = %spawnBrick.scheduleNoQuota(5000, spawnHoleBot);
                return 1;
            }
        }
    }
}

function HoleBot_movementReset(%obj)
{
    if(%obj.hooks_movementReset !$= "")
    {
        call(%obj.hooks_movementReset, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_movementReset;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_movementReset | %datablockHook == -1)
    {
        return;
    }
    else
    {
        %obj.clearMoveY();
        %obj.clearMoveX();
        %obj.hResetMoveTriggers();
        %obj.maxYawSpeed = 10;
        %obj.maxPitchSpeed = 10;
    }
}

function HoleBot_jetCheck(%obj)
{
    if(%obj.hooks_jetCheck !$= "")
    {
        call(%obj.hooks_jetCheck, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_jetCheck;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_jetCheck | %datablockHook == -1)
    {
        return;
    }
    else
    {
        %obj.hJetCheck();
    }
}

//Description: If the bot is set to search for players in a set radius, this is called.
//Return: 1 or 0, depending on whether or not the bot found a player it can target.
function HoleBot_ifSearch_Radius(%obj)
{
    if(%obj.hooks_ifSearch_Radius !$= "")
    {
        call(%obj.hooks_ifSearch_Radius, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifSearch_Radius;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifSearch_Radius | %datablockHook == -1)
    {
        return;
    }
    else
    {
        //Radius Check
		if(isObject(%obj.vars.minigame) && %obj.hSearchFOV != 1)
		{
			if(%obj.vars.isHost || %obj.vars.minigame.useAllPlayersBricks)
			{
				%target = %obj.hFindClosestPlayer();
				if(isObject(%target) && hLOSCheck(%obj, %target))
				{
					//if we've found someone set wander to 0 so we don't do any idle actions or wander around
					%obj.vars.wander = 0;
					%obj.vars.idle = 0;
					%obj.hFollowPlayer(%target, 1);
				}
				else if(%obj.hIgnore != %obj.hFollowing && %obj.hSawPlayerLastTick && isObject(%obj.hFollowing) && !%obj.hFollowing.isDisabled() && checkHoleBotTeams(%obj, %obj.hFollowing, 1))
				{
					if(%obj.hIsRunning)
                    {
						%chaseDist = 64;
                    }
					else
                    {
						%chaseDist = 128;
                    }
					if(vectorDist(%obj.getPosition(), %obj.hFollowing.getPosition()) <= %chaseDist * %obj.vars.scale)
					{
						//We saw someone last tick so we should go to where we last saw him/towards him. Might redo the way this works
					    %obj.vars.wander = 0;
					    %obj.vars.idle = 0;
						%obj.hFollowPlayer(%obj.hFollowing, 1);
					}
				}
			}
		}
    }
}

function HoleBot_ifSearch_FOV(%obj)
{
    if(%obj.hooks_ifSearch_FOV !$= "")
    {
        call(%obj.hooks_ifSearch_FOV, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifSearch_FOV;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifSearch_FOV | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.vars.minigame && %obj.hSearchFOV)
		{
			if(%obj.vars.isHost || %obj.vars.minigame.useAllPlayersBricks)
			{
				if(%obj.hSawPlayerLastTick && isObject(%obj.hFollowing) && checkHoleBotTeams(%obj,%obj.hFollowing, 1) && !%obj.hFollowing.isDisabled() && %obj.hIgnore != %obj.hFollowing)
				{
					if(%obj.hIsRunning)
                    {
                        %chaseDist = 64;
                    }
					else
                    {
						%chaseDist = 128;
                    }

					if(vectorDist(%obj.getPosition(), %obj.hFollowing.getPosition()) <= %chaseDist * %obj.vars.scale)
					{
						//We saw someone last tick so we should go to where we last saw him/towards him. Might redo the way this works
                        %obj.vars.wander = 0;
                        %obj.vars.idle = 0;
						%obj.hFollowPlayer(%obj.hFollowing, 1);
					}
				}

				if(%obj.hDoHoleFOVCheck(%obj.hFOVRadius, 1, 1))
				{
                    %obj.vars.wander = 0;
                    %obj.vars.idle = 0;
				}
				else
				{
					cancel(%obj.hFOVSchedule);
					%obj.hFOVSchedule = %obj.scheduleNoQuota(%obj.vars.tickrate / 2, hDoHoleFOVCheck, %obj.hFOVRadius, 0, 1);
				}
			}
		}
    }
}

function HoleBot_ifPathBrick(%obj)
{
    if(%obj.hooks_ifPathBrick !$= "")
    {
        call(%obj.hooks_ifPathBrick, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifPathBrick;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifPathBrick | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(isObject(%obj.hPathBrick) && %obj.hState !$= "Following")
        {	
            //Again ghetto clear movement things
            %obj.setMoveObject("");
            %obj.clearMoveY();
            %obj.clearMoveX();
            %obj.clearAim();
            %obj.hResetHeadTurn();

            %obj.hState = "Pathing";
            
            %tPos = getWords(%obj.hPathBrick.getPosition(), 0, 1) SPC 0;
            %pos = getWords(%obj.getPosition(), 0, 1) SPC 0;
            
            if(%obj.vars.data.canJet)
            {
                %brickZ = %obj.hPathBrick.hGetPosZ();
                %objZ = %obj.hGetPosZ();
                
                if(%brickZ > %objZ + 18)
                {
                    %obj.hLongJet();
                }
                else if(%obj.hHasJetted || %brickZ > %objZ + 2)
                {
                    %obj.hJetCheck(1);
                }
                    
                %obj.maxYawSpeed = 20;
                if(%obj.isJetting() && vectorDist(%pos, %tPos) <= 6 * %obj.vars.scale)
                {
                    %obj.setMoveSpeed(0.1);
                }
            }
                
            // setMoveSlowdown should govern this as well
            %obj.setMoveDestination(getWords(%obj.hPathBrick.getPosition(), 0, 1) SPC getWord(%obj.getPosition(), 2));
            
            //If people disable %obj.vars.wander then we assume they don't want the bot to be doing much, so don't avoid obstacles
            if(%obj.hAvoidObstacles && %obj.vars.wander)
            {
                %obj.hAvoidObstacle();
            }
            return 1;
        }
    }
}

function HoleBot_ifWander(%obj)
{
    if(%obj.hooks_ifWander !$= "")
    {
        call(%obj.hooks_ifWander, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifWander;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifWander | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.vars.wander)
        {
            %obj.hIsRunning = 0;
            //Er again, but still...
            %obj.setMoveObject("");
            %obj.clearMoveY();
            %obj.clearMoveX();
            %obj.setImageTrigger(0, 0);
            %obj.hResetHeadTurn();
            
            //Again converting to brick units, easier for players
            %returnDist = brickToMetric(%obj.hSpawnDist);
            %avoid = 0;
            
            //if 0 we assume they want to always return to brick
            // if returnDistance is zero then our bot will always try to get back to spawn since the distance between bot and spawn is never truly zero
            if(%returnDist <= 0)
            {
                %returnDist = 1.6;
            }
                
            %returnDist = %returnDist * %obj.vars.scale;
            
            if(%returnDist < 1)
            {
                %returnDist = 1;
            }

            if(HoleBot_tooFarFromSpawn(%obj, %returnDist) == 1)
            {
                //Returning to spawn, signal that to hLoop.
                return 1;
            }
            
            //If spawn distance is set to 0 we don't randomly wander. We assume the user wants them to act as guard bots
            HoleBot_guardAtSpawn(%obj, %returnDist);
            
            HoleBot_wanderRandomJet(%obj);
            
            //If we just saw a target, better act fast and irrational
            if(HoleBot_wanderTargetCheck(%obj) == 1)
            {
                return 2;
            }

            //Already did return check and lost target check, set to wander
            %obj.hState = "Wandering";
            
            //Grid walking
            HoleBot_gridWander(%obj);
            //Smooth wandering
            HoleBot_smoothWander(%obj);
            //More conventional bot movement, mixing between the two creates a good variety of movement
            HoleBot_mixedWander(%obj);
        }
    }
}

function HoleBot_tooFarFromSpawn(%obj, %returnDist)
{
    if(%obj.hooks_tooFarFromSpawn !$= "")
    {
        call(%obj.hooks_tooFarFromSpawn, %obj, %returnDist);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_tooFarFromSpawn;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj, %returnDist);
    }
    else if(%obj.hooks_tooFarFromSpawn | %datablockHook == -1)
    {
        return;
    }
    else
    {
        // perhaps we should make this trigger onReachBrick when we get back?
		//We're too far let's go back.. it's safe there
		if(%obj.hReturnToSpawn && vectorDist(%obj.getPosition(), %obj.spawnBrick.getPosition()) > %returnDist)
		{	
			// if %obj.hSpawnDist is zero that means we will never random wander, so let's set a flag that will reset our rotation to our spawnbrick when we arrive
			if(%obj.hSpawnDist == 0)
            {
                %obj.hIsGuard = 1;
            }
			
			%brickPos = %obj.spawnBrick.getPosition();
			%brickZ = hGetZ(%brickPos);
			%objZ = %obj.hGetPosZ();
			
			if(vectorDist(%pos, %brickPos) <= 6 * %obj.vars.scale)
            {
                %obj.setMoveSpeed(0.2);
            }
			
			if(%obj.vars.data.canJet)
			{
				if(%brickZ > %objZ + 18)
                {
                    %obj.hLongJet();
                }
				else if(%obj.hHasJetted || %brickZ > %objZ + 2)
                {
                    %obj.hJetCheck(1);
                }
					
				if(%obj.isJetting() && vectorDist(%pos, %brickPos) <= 6 * %obj.vars.scale)
                {
                    %obj.setMoveSpeed(0.1);
                }
			}

			%obj.clearAim();
			%obj.hFollowing = "";
			%obj.setMoveDestination(%brickPos);
			%obj.hState = "Returning";
			%obj.hAvoidObstacle();

			return 1;
		}
    }
}

function HoleBot_guardAtSpawn(%obj, %returnDist)
{
    if(%obj.hooks_guardAtSpawn !$= "")
    {
        call(%obj.hooks_guardAtSpawn, %obj, %returnDist);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_guardAtSpawn;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj, %returnDist);
    }
    else if(%obj.hooks_guardAtSpawn | %datablockHook == -1)
    {
        return;
    }
    else
    {
    	if(%obj.hReturnToSpawn && %obj.hIsGuard)
		{
			%spawnBrick = %obj.vars.spawnBrick;
			// check that we've fully returned to spawn
			if(vectorDist(%obj.getPosition(), %spawnBrick.getPosition()) <= %returnDist)
			{
				// ok we've returned so let's look forward and turn off hIsGuard	
				%obj.maxYawSpeed = getRandom(5, 10);
				%obj.maxPitchSpeed = getRandom(5, 10);
				
				%obj.setAimVector(%spawnBrick.getForwardVector());
				%obj.hIsGuard = 0;
				
				// call onBotReachBrick event
				%spawnBrick.onBotReachBrick(%obj);
			}
		}
    }
}

function HoleBot_wanderRandomJet(%obj)
{
    if(%obj.hooks_wanderRandomJet !$= "")
    {
        call(%obj.hooks_wanderRandomJet, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_wanderRandomJet;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_wanderRandomJet | %datablockHook == -1)
    {
        return;
    }
    else
    {
    	// randomly jet
		if(%obj.hSpawnDist != 0 && !getRandom(0, 4) && !%obj.hIsFalling())
		{
			%obj.hJump(100);
			// randomly do a long safe jump, or a short jump
			if(!getRandom(0, 2))
            {
                %obj.hJet(%tickrate - 500);
            }
			else
            {
                %obj.hJet(1000 + getRandom(0, 500));
            }
		}
    }
}

function HoleBot_wanderTargetCheck(%obj)
{
    if(%obj.hooks_wanderTargetCheck !$= "")
    {
        call(%obj.hooks_wanderTargetCheck, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_wanderTargetCheck;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_wanderTargetCheck | %datablockHook == -1)
    {
        return;
    }
    else
    {
    	if(%obj.hState $= "Following" && %obj.hSpawnDist != 0)
		{
			if(isObject(%obj.hFollowing) && %obj.hFollowing.getState() $= "Dead")
			{
				//do some sort of victory dance maybe?
				if(%obj.hEmote)
				{
					%obj.emote(loveImage);
				}
				%obj.hFollowing = 0;
			}
			else
			{
				%obj.clearAim();
				%xRand = getRandom(-15, 15);
				%yRand = getRandom(-15, 15);
				
				%obj.setMoveDestination(vectorAdd(%pos,%xRand SPC %yRand SPC 0));
				%obj.hAvoidObstacle();
			
				%obj.hState = "Wandering";
				if(%obj.hEmote)
				{
					%obj.emote(wtfImage);
				}
				%obj.hFollowing = "";
				return 1;
			}
		}
    }
}

function HoleBot_gridWander(%obj)
{
    if(%obj.hooks_gridWander !$= "")
    {
        call(%obj.hooks_gridWander, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_gridWander;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_gridWander | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.hGridWander && getRandom(-1, mFloatLength(1 * %obj.vars.AFKScale, 0)) <= 0 && %obj.hSpawnDist != 0)
		{
			%obj.vars.avoid = 1;
			%obj.vars.noStrafe = 0;
			%obj.vars.onlyJump = 1;
			%obj.vars.idle = 0;

			%obj.clearAim();
			%obj.hFollowing = "";
			%sPos = lockToGrid(%obj, %obj.getPosition());

			if(getRandom(0, 1))
			{
				%rand = getRandom(-4, 4) * 2 * %obj.vars.scale;
				%tPos = vectorAdd(%sPos, %rand SPC "0 0");
			}
			else
			{
				%rand = getRandom(-4, 4) * 2 * %obj.vars.scale;
				%tPos = vectorAdd(%sPos, "0" SPC %rand SPC "0");
			}
			%obj.setMoveDestination(%tPos);

			if(getRandom(0, 4) > 0)
            {
                %obj.scheduleNoQuota(%obj.vars.tickrate / 2, "hDetectWall");
            }
		}
    }
}

function HoleBot_smoothWander(%obj)
{
    if(%obj.hooks_smoothWander !$= "")
    {
        call(%obj.hooks_smoothWander, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_smoothWander;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_smoothWander | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.hSmoothWander && %obj.hSpawnDist && !%obj.hGridWander && !getRandom(0, mFloatLength(2 * %obj.vars.AFKScale, 0)))
		{
			%obj.vars.avoid = 1;
			%obj.vars.idle = 0;
			
			// randomly crouch
			if(!getRandom(0, 3))
            {
                %obj.setCrouching(1);
            }
			else if(!getRandom(0, 1))
            {
                %obj.setCrouching(0);
            }
			%obj.smoothWander();

			// randomly go up or down in water
			if(%obj.getWaterCoverage())
			{
				if(getRandom(1, 4) == 1)
				{
					%obj.hRandomMoveTrigger();
				}
			}

			//3/4 chance that the bot will face away from a wall
			if(getRandom(0,3) > 0)
            {
				%obj.scheduleNoQuota(%obj.vars.tickrate / 2, "hDetectWall");
            }
		}
    }
}

function HoleBot_mixedWander(%obj)
{
    if(%obj.hooks_mixedWander !$= "")
    {
        call(%obj.hooks_mixedWander, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_mixedWander;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_mixedWander | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(!%obj.hGridWander && !getRandom(0, mFloatLength(2 * %obj.vars.AFKScale, 0)) && %obj.hSpawnDist != 0)
		{
			%obj.vars.avoid = 1;
			%obj.vars.idle = 0;
			//Strafing is disbaled from the avoidance function to prevent the bot from walking around a point
			%obj.vars.noStrafe = 1;
			
			//Random crouching, works surprisingly well
			if(!getRandom(0, 3))
            {
                %obj.setCrouching(1);
            }
			else if(!getRandom(0, 1))
            {
                %obj.setCrouching(0);
            }

			%obj.maxYawSpeed = getRandom(3, 10);
			%obj.maxPitchSpeed = getRandom(3, 10);
			%obj.clearAim();
			%xRand = getRandom(-7, 7);
			%yRand = getRandom(-7, 7);
			%obj.hFollowing = "";

			%rFinal = vectorAdd(%pos,%xRand SPC %yRand SPC 0);
			%obj.setMoveDestination(%rFinal);
				
			%dif = vectorScale(vectorSub(%rFinal, %pos), 8);
			%aim = vectorAdd(%pos, getWords(%dif, 0, 1) SPC getRandom(-8, 8));

			// Have to look into this, appears to be a function to make the bot randomly look in a direction while moving
			// but would only be called under weird circumstance ##
			if(%xRand && %yRand)
			{
				%obj.setAimLocation(%aim);
			}

			if(getRandom(0, 4) > 0)
            {
                %obj.scheduleNoQuota(%obj.vars.tickrate / 2, "hDetectWall");
            }
			
			//Randomly go up or down in water
			if(%obj.getWaterCoverage())
			{
				if(getRandom(1, 4) == 1)
				{
					%obj.hRandomMoveTrigger();
				}
			}
		}
    }
}

function HoleBot_randomHeadTurn(%obj)
{
    if(%obj.hooks_randomHeadTurn !$= "")
    {
        call(%obj.hooks_randomHeadTurn, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_randomHeadTurn;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_randomHeadTurn | %datablockHook == -1)
    {
        return;
    }
    else
    {
    	if(%obj.vars.idle && !%obj.vars.isTurret && %obj.hIdleLookAtOthers && !getRandom(0, mFloatLength(4 * %obj.vars.AFKScale, 0)))
        {
            %obj.hRandomHeadTurn();
        }
        else
        {
            %obj.hResetHeadTurn();
        }
    }
}

function HoleBot_sprayPaintReset(%obj)
{
    if(%obj.hooks_sprayPaintReset !$= "")
    {
        call(%obj.hooks_sprayPaintReset, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_sprayPaintReset;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_sprayPaintReset | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(!getRandom(0, 2))
        {
            %obj.hIsBeingSprayed = 0;
        }
    }
}

function HoleBot_ifIdle(%obj)
{
    if(%obj.hooks_ifIdle !$= "")
    {
        call(%obj.hooks_ifIdle, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_ifIdle;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_ifIdle | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(%obj.vars.idle || (%obj.hIsBeingSprayed && %obj.hIdle && %obj.hIdleSpam))
        {
            %obj.hIsRunning = 0;
            //Ok if we're here then we're going to stand still and screw around for a bit
            %obj.vars.avoid = 1;
            %obj.vars.noStrafe = 1;
                
            //If we're screwing around on the grid we don't want to have the chance of being in "stuck mode", we should always stay on the grid based from the spawnbrick
            if(%gridWander)
            {
                %obj.vars.onlyJump = 1;
            }

            %obj.hFollowing = "";
            %obj.maxYawSpeed = 10;
            %obj.maxPitchSpeed = 10;
            %obj.vars.isLookingAtPlayer = 0;
            
            //Randomly look at other bots
            //Ok we're not going to randomly look at another bot, let's randomly look around anyway... maybe
            HoleBot_randomLook(%obj);
                
            if(!%obj.vars.isLookingAtPlayer)
            {
                %obj.hIsbeingSprayed = 0;
            }
                
            //If idle spam is enabled but idle look is not, do this
            HoleBot_toolSpam(%obj);

            //Since we're already screwing around maybe we should sit or spam an emoete
            HoleBot_emoteSpam(%obj);
            %obj.hState = "Wandering";
        }
    }
}

function HoleBot_randomLook(%obj)
{
    if(%obj.hooks_randomLook !$= "")
    {
        call(%obj.hooks_randomLook, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_randomLook;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_randomLook | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(!%obj.vars.data.isTurret && %obj.hIdleLookAtOthers && (!getRandom(0, mFloatLength(2 * %obj.vars.AFKScale, 0)) || %obj.hIsBeingSprayed) && isObject((%targ = %obj.hReturnCloseBlockhead())))
        {
            if(%obj.hIsBeingSprayed)
            {
                %targ = %obj.hIsBeingSprayed;
            }
                
            %obj.vars.isLookingAtPlayer = 1;
            %obj.setAimObject(%targ);
            %obj.vars.alreadyLooked = 1;
            //Randomly follow the other bot/player
            if(!getRandom(0, mFloatLength(1 * %obj.vars.AFKScale, 0)))
            {
                if(%obj.hSpawnDist != 0 && %obj.vars.wander && !%obj.hGridWander)
                {
                    %obj.setMoveY(%obj.hMaxMoveSpeed * 0.75);
                }
            }
        }
        else
        {
            if(getRandom(-3, mFloatLength(2 * %obj.vars.AFKScale, 0)) <= 0 && %obj.hSpasticLook)
            {
                %obj.hLookSpastically();
            }

            if(getRandom(-3, mFloatLength(2 * %obj.vars.AFKScale, 0)) <= 0 && %obj.hSpasticLook)
            {
                %spazLookSchedule = %obj.scheduleNoQuota(%obj.vars.tickrate / 4 + getRandom(0, 500), "hLookSpastically");
            }
        }
    }
}

function HoleBot_toolSpam(%obj)
{
    if(%obj.hooks_toolSpam !$= "")
    {
        call(%obj.hooks_toolSpam, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_toolSpam;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_toolSpam | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(!%obj.vars.data.isTurret && %obj.hIdleSpam && (!getRandom(0, mFloatLength(2 * %obj.vars.AFKScale, 0)) || %obj.hIsBeingSprayed))
        {
            %spamChoice = getRandom(0, 4);
                    
            if(%obj.hIsBeingSprayed)
            {
                %spamChoice = 0;
                %obj.hIsBeingSprayed = 0;
            }
        
            if(isObject(%obj.getMountedImage(0)))
            {
                %obj.unMountImage(0);
                fixArmReady(%obj);
            }
            %obj.unMountImage(1);

            if(%spamChoice == 0)
            {
                serverCmdUseSprayCan(%obj,getRandom(0, 27));
            }
            else if(%spamChoice == 1)
            {
                %obj.mountImage(hammerImage, 0);
            }
            else if(%spamChoice == 2) 
            {
                %obj.mountImage(printGunImage, 0);
                %multiShoot = 1;
            }
            else if(%spamChoice == 3) 
            {
                %obj.mountImage(brickImage, 0);
                %multiShoot = 1;
            }
            else if(%spamChoice == 4) 
            {
                %obj.mountImage(wrenchImage, 0);
                %multiShoot = 1;
            }
            if(%multiShoot)
            {
                %shootTimes = getRandom(1, 4);
                %nIterate = %tickRate / %shootTimes;
                for(%a = 0; %a < %shootTimes; %a++)
                {
                    %obj.hShotSched = %obj.scheduleNoQuota(%a * %nIterate, hShootPrediction, "none", %obj.vars.tickrate, 4, 1);
                }
            }
            else
            {
                %obj.setImageTrigger(0, 1);
            }
                
            fixArmReady(%obj);
            //Set is spazzing to 1 so that we know not to spam click later
            %obj.hIsSpazzing = 1;
        }
    }
}

function HoleBot_emoteSpam(%obj)
{
    if(%obj.hooks_emoteSpam !$= "")
    {
        call(%obj.hooks_emoteSpam, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_emoteSpam;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_emoteSpam | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(!%obj.vars.data.isTurret && %obj.hIdleAnimation && !getRandom(0, mFloatLength(1 * %obj.vars.AFKScale, 0)))
        {
            %anim = $holeBotIdleGestures[getRandom(0, $hBotIdGest)];
            if(!getRandom(0, 5))
            {
                serverCmdSit(%obj);
            }
            if(%anim $= "crouch")
            {
                %obj.setCrouching(1);
            }
            else if(%anim $= "sit")
            {
                serverCmdSit(%obj);
            }
            else
            {
                %obj.playthread(3, %anim);

                if(%anim $= "love")
                {
                    %obj.emote("loveImage");
                }
                if(%anim $= "wtf")
                {
                    %obj.emote("wtfImage");
                }
                if(%anim $= "alarm")
                {
                    %obj.emote("alarmProjectile");
                }

                //If we get the bricks emote, it's only right that we look at the ground like all 90% of blocklanders do when they pick blocks
                if(%anim $= "Bricks")
                {
                    if(!getRandom(0, 2))
                    {
                        cancel(%spazLookSchedule);
                        %pos = %obj.getPosition();
                        %vec = %obj.getForwardVector() * 2;
                        %loc = vectorAdd(%vec,"0 0 -1");
                        %obj.setAimLocation(vectorAdd(%pos, %loc));
                    }
                    %obj.scheduleNoQuota(300, emote, "BSDProjectile");
                }
            }
        }
    }
}

function HoleBot_wanderAvoidObstacle(%obj)
{
    if(%obj.hooks_wanderAvoidObstacle !$= "")
    {
        call(%obj.hooks_wanderAvoidObstacle, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_wanderAvoidObstacle;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_wanderAvoidObstacle | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(!%obj.hSpawnDist == 0 && !%obj.vars.data.isTurret && %obj.vars.avoid && %obj.vars.wander)
        {
            %obj.hAvoidObstacle(%obj.vars.noStrafe, %obj.vars.onlyJump);
        }
    }
}

function HoleBot_wanderSpazzClick(%obj)
{
    if(%obj.hooks_wanderSpazzClick !$= "")
    {
        call(%obj.hooks_wanderSpazzClick, %obj);
        return;
    }
    %datablockHook = %obj.vars.data.hooks_wanderSpazzClick;
    if(%datablockHook !$= "")
    {
        call(%datablockHook, %obj);
    }
    else if(%obj.hooks_wanderSpazzClick | %datablockHook == -1)
    {
        return;
    }
    else
    {
        if(!%obj.vars.data.isTurret && %obj.vars.wander && %obj.hIdle && %obj.hIdleSpam && !getRandom(0, mFloatLength(5 * %obj.vars.AFKScale, 0))) 
        {
            if(isObject(%obj.getMountedImage(0)))
            {
                %obj.unMountImage(0);
                if(isObject(%obj.getMountedImage(1)))
                {
                    %obj.unMountImage(1);
                }
            }
            fixArmReady(%obj);
            %obj.hIsSpazzing = 1;
            %obj.hSpazzClick();
        }
    }
}

//for more information or to request a new hook bother robb via discord or blf