//Register Event.
registerOutputEvent(Bot,"canJump","bool");
//Give the event functionallity.
function AIPlayer::canJump(%this,%arg)
	{	
		if(%arg)
			{
				%this.setJumping(1);
				%this.hCantJump = 0;
			}
		else
			{
				%this.setJumping(0);
				%this.hCantJump = 1;
			}
	}

package antiBotJumping
	{
		function AIPlayer::setJumping(%this,%arg)
			{
				if(%this.hCantJump)
					{
						parent::setJumping(%this,0);
					}
				else
					{
						parent::setJumping(%this,%arg);
					}
			}
	};
activatepackage(antiBotJumping);