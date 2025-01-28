shader_type spatial;

uniform float radius = 0.5;
uniform vec4 color = vec4(1.0, 0.0, 0.0, 1.0);

void vertex() {
}

void fragment() {
    //vec2 uv = FRAGCOORD.xy / VIEWPORT_SIZE;
    //vec2 center = vec2(0.5, 0.5);
    //float dist = length(uv - center);
//
    //if (dist > radius) {
        //discard;
    //}

    ALBEDO = color.rgb;
    ALPHA = color.a;
}

