shader_type canvas_item;

uniform float rain_amount = 200.0;
uniform float near_rain_length : hint_range(0.001, 1.0) = 0.2;
uniform float far_rain_length : hint_range(0.001, 1.0) = 0.1;
uniform float near_rain_width : hint_range(0.1, 1.0) = 1.0;
uniform float far_rain_width : hint_range(0.1, 1.0) = 0.5;
uniform float near_rain_transparency : hint_range(0.1, 1.0) = 1.0;
uniform float far_rain_transparency : hint_range(0.1, 1.0) = 0.5;
uniform vec4 rain_color : source_color = vec4(0.8, 0.8, 0.8, 1.0);
uniform float base_rain_speed : hint_range(0.01, 1.0) = 0.5;
uniform float additional_rain_speed : hint_range(0.01, 1.0) = 0.5;
uniform float slant : hint_range(-1.0, 1.0) = 0.2;

void fragment() {
// 要控制程序化特效，用这行
// uniform float time = 10000.0;

	float time = 10000.0 + TIME;

	vec2 uv = vec2(0.0);
	float remainder = mod(UV.x - UV.y * slant, 1.0 / rain_amount);
	uv.x = (UV.x - UV.y * slant) - remainder;
	float rn = fract(sin(uv.x * rain_amount));
	uv.y = fract((UV.y + rn));

// 模糊化
//COLOR = mix(COLOR, rain_color, smoothstep(1.0 - (far_rain_length + (near_rain_length - far_rain_length) * rn), 1.0, fract(uv.y - time * (base_rain_speed + additional_rain_speed * rn))) * (far_rain_transparency + (near_rain_transparency - far_rain_transparency) * rn) * step(remainder * rain_amount, far_rain_width + (near_rain_width - far_rain_width) * rn));

// 无模糊
COLOR = mix(COLOR, rain_color, step(1.0 - (far_rain_length + (near_rain_length - far_rain_length) * rn), fract(uv.y - time * (base_rain_speed + additional_rain_speed * rn))) * (far_rain_transparency + (near_rain_transparency - far_rain_transparency) * rn) * step(remainder * rain_amount, far_rain_width + (near_rain_width - far_rain_width) * rn));
}