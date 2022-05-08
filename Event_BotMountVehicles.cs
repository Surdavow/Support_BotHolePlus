registerOutputEvent("Bot", "canMountVehicles", "bool", 0);

function Player::canMountVehicles(%obj, %bool)
{
	%obj.cannotMountVehicles = !%bool;
}

package Event_BotCanMountVehicles
{
	function minigameCanUse(%objA, %objB)
	{
		if(isObject(%objA) && isObject(%objB))
		{
			if((%objA.getClassName() $= "Player" || %objA.getClassName() $= "AIPlayer") && (%objB.getClassName() $= "WheeledVehicle" || %objB.getClassName() $= "FlyingVehicle" || %objB.getClassName() $= "AIPlayer" && %objB.getDataBlock().rideable))
			{
				if(%objA.cannotMountVehicles)
				{
					return 0;
				}
			}

			if((%objB.getClassName() $= "Player" || %objB.getClassName() $= "AIPlayer") && (%objA.getClassName() $= "WheeledVehicle" || %objA.getClassName() $= "FlyingVehicle" || %objA.getClassName() $= "AIPlayer" && %objA.getDataBlock().rideable))
			{
				if(%objB.cannotMountVehicles)
				{
					return 0;
				}
			}
		}

		parent::minigameCanUse(%objA, %objB);
	}
};
activatePackage(Event_BotCanMountVehicles);