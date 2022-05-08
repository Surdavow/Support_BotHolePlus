function fxDtsBrick::killBot(%this)
{
	if(isObject(%b = %this.hBot))
	{
		if(%b.getState() !$= "Dead")
		{
		%b.kill();
		}	
	}
}
registerOutputEvent("fxDtsBrick", "KillBot");