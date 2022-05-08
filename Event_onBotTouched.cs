package on_bot_touched_package
{
	function armor::onCollision( %this, %obj, %col, %pos, %vel )
	{
		parent::onCollision( %this, %obj, %col, %pos, %vel );

		if ( !isObject( %cl = %obj.client ) )
		{
			return;
		}

		if ( !isObject( %bk = %col.spawnBrick ) )
		{
			return;
		}

		if ( !%col.isHoleBot )
		{
			return;
		}

		if ( $Sim::Time - %obj.lastBotTouchTime < 0.19 )
		{
			return;
		}

		%obj.lastBotTouchTime = $Sim::Time;
		%bk.onBotTouched( %cl );
	}
};

activatePackage( "on_bot_touched_package" );
registerInputEvent( "fxDTSBrick", "onBotTouched", "Self fxDTSBrick" TAB "Bot Bot" TAB "Player Player" TAB "Client GameConnection" TAB "MiniGame MiniGame" );

function fxDTSBrick::onBotTouched( %this, %cl )
{
	$InputTarget_["Self"] = %this;
	$InputTarget_["Bot"] = %this.hBot;
	$InputTarget_["Player"] = %cl.player;
	$InputTarget_["Client"] = %cl;
	$InputTarget_["MiniGame"] = getMiniGameFromObject( %this );

	%this.processInputEvent( "onBotTouched", %cl );
}