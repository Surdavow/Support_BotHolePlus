registerOutputEvent("Bot", "setNameText", "string 200 300", 1);
registerOutputEvent("Bot", "setNameDistance", "int 0 100", 1);
registerOutputEvent("Bot", "setNameColor", "int 0 255" TAB "int 0 255" TAB "int 0 255");

function AIPlayer::setNameText(%this, %text, %client) {
	%this.setShapeName(%text, 8564862);
	%this.setShapeNameDistance(100);
}

function AIPlayer::setNameDistance(%this, %int, %client) {
	%this.setShapeNameDistance(%int);
}

function AIPlayer::setNameColor(%this, %r, %g, %b, %client) {
	%this.setShapeNameColor(%r / 255 SPC %g / 255 SPC %b / 255);
}