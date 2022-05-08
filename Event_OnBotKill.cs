package Event_onBotKill
{
	function Armor::Damage(%this,%obj,%sourceObject,%position,%damage,%damageType,%damageLoc)
	{
		%target = %sourceObject.sourceObject;

		if(isObject(%obj) && isObject(%target) && %obj != %target)
		{
		if(%target.getClassName() $= "AIPlayer" && %target.getState() !$= "dead")
		{
			if(%target.isHoleBot)
			{
				%obj.onBotKill = %target;
			}
		}
		}
		Parent::Damage(%this,%obj,%sourceObject,%position,%damage,%damageType,%damageLoc);
	}

	function Armor::onDisabled(%this,%obj,%sourceObject,%position,%damage,%damageType,%damageLoc)
	{
		%killer = %obj.onBotKill;

		if(%obj.getState() $= "Dead" && isObject(%killer) && %obj != %killer)
		{
			if(isObject(%brick = %killer.spawnBrick) && getSimTime() - %obj.onBotKill >= 33)
			{
				$InputTarget_["Self"]   = %brick;
				$InputTarget_["Bot"] = %killer;
				$InputTarget_["MiniGame"] = getMiniGameFromObject(%killer);

				if(%obj.getClassName() $= "Player")
				{
				$InputTarget_["Killed_Player"] = %obj;
				$InputTarget_["Killed_Client"] = %obj.client;
				%brick.processInputEvent("onBotKill", %obj.client);
				}

				if(%obj.getClassName() $= "AIPlayer")
				{
				$InputTarget_["Killed_Bot"] = %obj;
				%brick.processInputEvent("onBotKill");
				}

				
				%obj.onBotKill = getSimTime(); //Prevent duplicates of the event
			}
		}

		Parent::onDisabled(%this,%obj,%sourceObject,%position,%damage,%damageType,%damageLoc);
	}
};

activatePackage(Event_onBotKill);
registerInputEvent("fxDTSBrick", "onBotKill", "Self fxDTSBrick" TAB "Bot Bot" TAB "Killed_Bot Bot" TAB "Killed_Player Player" TAB "Killed_Client GameConnection" TAB "MiniGame MiniGame");
