function AIPlayer::followPlayer(%obj, %list, %int, %bool, %client) // There is already a function for following the player, but the bot attacks them
{
	%obj.hFollowType = %list;
	%obj.hKeepFollowing = %bool;

	if(%int == -1 && %list == 4)
		return;

	if(%list == 0)
		%player = getBrickGroupFromObject(%obj.spawnBrick).client.player;
	else if(%list == 1)
	{
		if(%obj.hSearchRadius == -2)
			%radius = 2000000000;
		else
			%radius = %obj.hSearchRadius;
	
		initContainerRadiusSearch(%obj.getPosition(), %radius, $TypeMasks::PlayerObjectType);

		while((%target = containerSearchNext()) != 0)
		{
			if(%target.getClassName() $= "Player")
			{
				%player = %target.getID();
				break;
			}
		}
	}
	else if(%list == 2 && !%obj.hKeepFollowing && %obj.hFollowingNoDamage == 0)
	{
		if(%obj.hSearchRadius == -2)
			%radius = 2000000000;
		else
			%radius = %obj.hSearchRadius;
	
		initContainerRadiusSearch(%obj.getPosition(), %radius, $TypeMasks::PlayerObjectType);

		while((%target = containerSearchNext()) != 0)
			if(%target.getClassName() $= "Player")
				%player = %target.getID();
	}
	else if(%list == 3 && isObject(%client) && %client != 0)
		%player = %client.player;
	else if(%list == 4 && %int != -1)
		%player = findClientByBL_ID(%int).player;
	else
		return;

	if(!isObject(%player))
		return;

	if(%player.client.name $= "")
		return;

	if(%player.getClassName() !$= "Player") // Gotta make sure it's a player
		return;

	if(%player.client.isDead && !%bool) // If hKeepFollowing is true, then the bot will follow the player when they respawn
		return;

	%obj.hFollowingNoDamage = %player.client; // hFollowing is already a variable

	if(!%player.client.isDead)
	{
		%obj.setAimObject(%player);
		%obj.setMoveObject(%player);
	}
}

package BotFollowPlayerPackage
{
	function AIPlayer::hLoop(%obj)
	{
		Parent::hLoop(%obj);

		if(!isObject(%obj.hFollowingNoDamage))
			%obj.hFollowingNoDamage = 0;

		if(%obj.hFollowingNoDamage == 0 && %obj.hFollowType != 1 && %obj.hFollowType != 2)
			return;

		if(%obj.hFollowingNoDamage == 0 && (%obj.hFollowType == 1 || %obj.hFollowType == 2))
		{
			%obj.followPlayer(%obj.hFollowType, %obj.hFollowingNoDamage.bl_id, %obj.hKeepFollowing, %obj.hFollowingNoDamage);
			return;
		}

		%following = %obj.hFollowingNoDamage.player;

		if(!isObject(%following) && !%obj.hKeepFollowing)
			%obj.hFollowingNoDamage = 0;

		if(%following.client.isDead && !%obj.hKeepFollowing)
			%obj.hFollowingNoDamage = 0;

		%obj.followPlayer(%obj.hFollowType, %obj.hFollowingNoDamage.bl_id, %obj.hKeepFollowing, %obj.hFollowingNoDamage);
	}

	function GameConnection::onDeath(%this, %killerPlayer, %killerClient, %damageType, %damageLoc)
	{
		Parent::onDeath(%this, %killerPlayer, %killerClient, %damageType, %damageLoc);
		%this.isDead = true;
	}

	function GameConnection::spawnPlayer(%this, %spawn)
	{
		%this.isDead = false;
		return Parent::spawnPlayer(%this, %spawn);
	}
};
activatePackage(BotFollowPlayerPackage);

registerOutputEvent(Bot, "FollowPlayer", "list Owner 0 Nearest 1 Farthest 2 Activator 3 Custom 4\tint -1 999999 -1\tbool", 1);