shader_type spatial;

void vertex() {
}

void fragment() {
    vec2 uv = UV - 0.5;
    float d = length(uv);
    float circle = smoothstep(0.5, 0.49, d);
    vec4 color = vec4(vec3(circle), 1.0);
	if(circle < 0.49){
		discard;
	}
	ALBEDO = color.rgb * vec3(1.0, 0, 0);
	ALPHA = color.a;
}
