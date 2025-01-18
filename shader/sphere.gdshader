shader_type canvas_item;

uniform float radius : hint_range(0.0, 1.0) = 0.0; // 控制裁剪的半径
uniform bool reverse = false; // 用于控制是裁剪还是反向恢复
uniform vec4 modulation_color : source_color; // 新增的调制颜色

void fragment()
{
    // 获取纹理坐标中心
    vec2 center = vec2(0.5, 0.5);

    // 计算当前像素到中心的距离
    float dist = distance(UV, center);

    // 获取当前像素的纹理颜色
    vec4 tex_color = texture(TEXTURE, UV);

    if (reverse)
    {
        // 如果是反向，距离大于半径则显示像素
        if (dist > radius)
        {
            COLOR = tex_color * modulation_color; // 应用调制颜色
        }
        else
        {
            discard; // 丢弃像素，使其透明
        }
    }
    else
    {
        // 正常裁剪模式，距离小于半径则显示像素
        if (dist < radius)
        {
            COLOR = tex_color * modulation_color * COLOR; // 应用调制颜色
        }
        else
        {
            discard; // 丢弃像素，使其透明
        }
    }
}