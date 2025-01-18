shader_type canvas_item;

const mat4 bayer_matrix = mat4(
    vec4(00.0 / 16.0, 12.0 / 16.0, 03.0 / 16.0, 15.0 / 16.0),
    vec4(08.0 / 16.0, 04.0 / 16.0, 11.0 / 16.0, 07.0 / 16.0),
    vec4(02.0 / 16.0, 14.0 / 16.0, 01.0 / 16.0, 13.0 / 16.0),
    vec4(10.0 / 16.0, 06.0 / 16.0, 09.0 / 16.0, 05.0 / 16.0));

float get_dither_value(vec2 pixel) {
    int x = int(pixel.x - 4.0 * floor(pixel.x / 4.0));
    int y = int(pixel.y - 4.0 * floor(pixel.y / 4.0));
    return bayer_matrix[x][y];
}

uniform vec2 pixel_scale = vec2(1.0);
uniform float dither_pixel_size = 1.0;
uniform vec2 dither_pixel_offset = vec2(0.0);

vec2 get_pixel_center(sampler2D texture_albedo, vec2 uv) {
    return floor(uv * vec2(textureSize(texture_albedo, 0)) / dither_pixel_size + dither_pixel_offset) + 0.5;
}

uniform sampler2D dither_palette : source_color, filter_nearest;

vec4 get_dither_color(float dither_value, vec4 color) {
    vec2 palette_size = vec2(textureSize(dither_palette, 0));
    float color_count = floor((palette_size.y + 1.0) / 16.0);

    float y = (clamp(color.g * 16.0, 0.5, 15.5) + floor(dither_value * color_count) * 16.0) / palette_size.y;
    float x = clamp(floor(color.r * 16.0), 0.0, 15.0) / 16.0 + clamp(color.b * 16.0, 0.5, 15.5) / 256.0;

    return vec4(texture(dither_palette, vec2(x, y)).rgb, 1.0);
}

uniform vec2 alpha_dither_pixel_offset = vec2(0.0);

void fragment() {
    vec4 color = texture(TEXTURE, UV) * COLOR;
    vec2 pixel = get_pixel_center(TEXTURE, UV);
    pixel.r *= pixel_scale.r;
    pixel.g *= pixel_scale.g;
    float dither_value_alpha = get_dither_value(pixel + alpha_dither_pixel_offset);
    if (color.a <= dither_value_alpha) {
        discard;
    }
    float dither_value = get_dither_value(pixel);
    COLOR = get_dither_color(dither_value, color) * COLOR;
}
