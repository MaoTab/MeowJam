shader_type canvas_item;

uniform float progress : hint_range(0, 1);
uniform bool reverse;
uniform float diamondPixelSize = 10.0;

void fragment() {
	float xFraction = fract(FRAGCOORD.x / diamondPixelSize);
	float yFraction = fract(FRAGCOORD.y / diamondPixelSize);
	float xDistance = abs(xFraction - 0.5);
	float yDistance = abs(yFraction - 0.5);
	if(reverse){
		if (xDistance + yDistance + UV.x + UV.y < progress * 4.0) {
		discard;
		}
	}else{
		if (xDistance + yDistance + UV.x + UV.y > progress * 4.0) {
		discard;
		}
	}
}