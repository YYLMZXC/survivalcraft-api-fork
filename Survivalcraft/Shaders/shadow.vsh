#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform mat4 ViewProjectionMatrix;
uniform mat4 LightMatrix;
uniform vec3 LightPosition;
uniform vec2 viewsize;

attribute vec3 a_position;

varying float distance;
varying vec2 v_texcoord;
varying vec2 viewsize2;

void main()
{
	viewsize2=viewsize;
	float llen=length(LightPosition);
	float alen=length(a_position);
	distance = alen / llen;
	vec4 tmp = vec4(a_position.xyz, 1.0) * ViewProjectionMatrix;
	v_texcoord = tmp.xy;
	gl_Position = ViewProjectionMatrix * vec4(a_position.xyz, 1.0);
	OPENGL_POSITION_FIX;
}

#endif
