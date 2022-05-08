registerOutputEvent("fxDTSBrick", BotSpawnLoot, "datablock ItemData" TAB "datablock ItemData" TAB "datablock ItemData\tstring 100 32", 1);
function fxDTSBrick::botSpawnLoot(%this,%item0,%item1,%item2,%chance,%cl)
{
	%obj = %this.hBot;//For efficiency, make %obj = %this.hBot for the function below to work.
	AIPlayer::SpawnLoot(%obj,%item0,%item1,%item2,%chance,%cl);//There's no need to check for the %obj's object from above, the SpawnLoot function checks anyways.

}

registerOutputEvent("Bot", SpawnLoot, "datablock ItemData" TAB "datablock ItemData" TAB "datablock ItemData\tstring 100 32", 1);
function AIPlayer::SpawnLoot( %obj,%item0,%item1,%item2,%chance,%cl )
{
	%velxmin = -4;//The variables used for random velocity, velocity's minimum and maximum for x, z, and impulse.
	%velxmax = 4;

	%velzmin = 2;
	%velzmax = 6;

	%impulsemin = 1;
	%impulsemax = 4;

		if(isObject(%obj))//Check if the object exists, thing being the bot or the brick's hole bot from botSpawnLoot
	{
		if(getRandom(0,100) <= %chance)//Chance
		{
			if(isObject(%item0))//Now check if the selected item datablock exists, if this is NONE or "" this won't continue
			{
				%loot0 = new item()
				{
	 				dataBlock = %item0;
					position = %obj.getHackPosition();
					dropped = 1;
					canPickup = 1;
					client = %cl;
				};

				if(isObject(%cl.minigame))//Check for the client's minigame, else you wont be able to pickup the item in the minigame
				%loot0.minigame = %cl.minigame;
			
				if(isObject(%loot0))//Perhaps checking if the loot exists is redundant, but I guess I'll add this regardless.
				{
					missionCleanup.add(%loot0);

					//Super long line incoming, that's for setting the item's velocity.
					%loot0.applyimpulse(%loot0.getposition(),vectoradd(vectorscale(%obj.getforwardvector(),getRandom(%impulsemin,%impulsemax)),getRandom(%velxmin,%velxmax) @ " 0 " @ getRandom(%velzmin,%velzmax)));
					%loot0.fadeSched = %loot0.schedule(8000,fadeOut);
					%loot0.delSched = %loot0.schedule(8200,delete);
				}
			}

			if(getRandom(0,100) <= %chance)//Repeated ifStatement, I'm also including the chance variable for variation.
			{
				if(isObject(%item1))
				{
					%loot1 = new item()
					{
	 					dataBlock = %item1;
						position = %obj.getHackPosition();
						dropped = 1;
						canPickup = 1;
						client = %cl;
					};

					if(isObject(%cl.minigame))
					%loot1.minigame = %cl.minigame;
			
					if(isObject(%loot1))
					{
						missionCleanup.add(%loot1);

						%loot1.applyimpulse(%loot1.getposition(),vectoradd(vectorscale(%obj.getforwardvector(),getRandom(%impulsemin,%impulsemax)),getRandom(%velxmin,%velxmax) @ " 0 " @ getRandom(%velzmin,%velzmax)));
						%loot1.fadeSched = %loot1.schedule(8000,fadeOut);
						%loot1.delSched = %loot1.schedule(8200,delete);
					}
				}
			}

			if(getRandom(0,100) <= %chance)//Repeated ifStatement
			{
				if(isObject(%item2))
				{
					%loot2 = new item()
					{
	 					dataBlock = %item2;
						position = %obj.getHackPosition();
						dropped = 1;
						canPickup = 1;
						client = %cl;
					};

					if(isObject(%cl.minigame))
					%loo2.minigame = %cl.minigame;
			
					if(isObject(%loot2))
					{
						missionCleanup.add(%loot2);

						%loot2.applyimpulse(%loot2.getposition(),vectoradd(vectorscale(%obj.getforwardvector(),getRandom(%impulsemin,%impulsemax)),getRandom(%velxmin,%velxmax) @ " 0 " @ getRandom(%velzmin,%velzmax)));
						%loot2.fadeSched = %loot2.schedule(8000,fadeOut);
						%loot2.delSched = %loot2.schedule(8200,delete);
					}
				}//Time to finish this off
			}
		}
	}
}	