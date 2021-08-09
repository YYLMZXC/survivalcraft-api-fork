#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform mat4 ViewProjectionMatrix;
uniform vec3 LightPosition;
uniform vec2 viewsize;

attribute vec3 a_position;

varying float pmx;
varying vec2 s_texcoord;

vec2 WorldToScreen(vec3 source, vec2 size,mat4 worldViewProjection){
	vec4 result2 = worldViewProjection * vec4(source,1.0);
	vec3 result = result2.xyz;
	result /= source.x * worldViewProjection[0][3] + source.y * worldViewProjection[1][3] + source.z * worldViewProjection[2][3] + worldViewProjection[3][3];
	result.x = (result.x + 1.0) * 0.5 * size.x;
	result.y = (result.y + 1.0) * 0.5 * size.y;
	return result.xy/size;
}


void main()
{
	pmx = distance(LightPosition,a_position);
	pmx = pmx / distance(LightPosition);
	s_texcoord = WorldToScreen(a_position,viewsize,ViewProjectionMatrix);//获取太阳视角下的屏幕的纹理坐标
	gl_Position = ViewProjectionMatrix * vec4(a_position, 1.0);
	OPENGL_POSITION_FIX;
}

#endif
