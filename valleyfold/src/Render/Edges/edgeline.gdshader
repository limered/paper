shader_type spatial;
render_mode unshaded, depth_test_disabled;

uniform vec4 line_color : source_color = vec4(1.0, 0, 0, 1.0);

void vertex() {
}

void fragment() {
	ALBEDO = line_color.rgb;
	ALPHA = line_color.a;
}
