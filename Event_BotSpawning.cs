function fxDtsBrick::AttemptSpawnBot(%this)
{
	if(!isObject(%this.hBot))
		%this.spawnHoleBot();
}
registerOutputEvent("fxDtsBrick", "AttemptSpawnBot");

function fxDtsBrick::AttemptDespawnBot(%this, %effect)
{
	cancel(%this.hModS);
	%this.hModS = 0;

	if(isObject(%b = %this.hBot))
	{
		if(%effect)
			%b.spawnProjectile("audio2d","deathProjectile","0 0 0", 1);

		%b.delete();
		%this.hBot = 0;
	}
}
registerOutputEvent("fxDtsBrick", "AttemptDespawnBot", "bool");

function AIPlayer::Despawn(%this, %effect)
{
	if(isObject(%br = %this.spawnBrick))
	{
		cancel(%br.hModS);
		%br.hModS = 0;
		%br.hBot = 0;
	}

	if(%effect)
		%this.removeBody();
	else
		%this.delete();
}
registerOutputEvent("Bot", "Despawn", "bool");