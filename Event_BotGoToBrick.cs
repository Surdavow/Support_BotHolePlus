function fxDTSBrick::botGotoBrick(%obj,%target)
{	
	if(isObject(%obj.hBot))
	{
	%obj.hBot.hClearMovement();
	%count = getWordCount(%target);
	
	%brick = getWord(%target,getrandom(0,%count-1));

	%tBrick = hReturnNamedBrick(%obj.hBot.spawnBrick.getGroup(),%brick);
	if(isObject(%tBrick))
	{
		%obj.hBot.hPathBrick = %tBrick;
		//%obj.setMoveDestination(%tBrick.getTransform());
		cancel( %obj.hBot.hSched );
      %obj.hBot.hSched = 0;
		%obj.hBot.hLoop();
		
		%obj.hBot.hSkipOnReachLoop = 1;
	}
	}
}
registerOutputEvent(fxDTSBrick,"BotGoToBrick","string 20 100");

function fxDTSBrick::botAttemptGotoBrick(%obj,%target)
{	
	if(isObject(%obj.hBot) && !%obj.hBot.hPathBrick)
	{
	%obj.hBot.hClearMovement();
	%count = getWordCount(%target);
	
	%brick = getWord(%target,getrandom(0,%count-1));

	%tBrick = hReturnNamedBrick(%obj.hBot.spawnBrick.getGroup(),%brick);
	if(isObject(%tBrick))
	{
		%obj.hBot.hPathBrick = %tBrick;
		//%obj.setMoveDestination(%tBrick.getTransform());
		cancel( %obj.hBot.hSched );
      %obj.hBot.hSched = 0;
		%obj.hBot.hLoop();
		
		%obj.hBot.hSkipOnReachLoop = 1;
	}
	}
}
registerOutputEvent(fxDTSBrick,"BotAttemptGoToBrick","string 20 100");