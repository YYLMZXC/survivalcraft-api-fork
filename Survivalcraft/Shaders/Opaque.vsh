#ifdef HLSL

float4x4 u_worldViewProjectionMatrix;
float4 u_color;

void main(
	in float3 a_position: POSITION,
#ifdef USE_VERTEXCOLOR
	in float4 a_color: COLOR,
#endif
#ifdef USE_TEXTURE
	in float2 a_texcoord: TEXCOORD,
#endif
#ifdef USE_TEXTURE
	out float2 v_texcoord : TEXCOORD,
#endif
	out float4 v_color : COLOR,
	out float4 sv_position: SV_POSITION
)
{
	// Color
	v_color = u_color;
#ifdef USE_VERTEXCOLOR
	v_color *= a_color;
#endif

	// Texcoord
#ifdef USE_TEXTURE
	v_texcoord = a_texcoord;
#endif

	// Position
	sv_position = mul(float4(a_position.xyz, 1.0), u_worldViewProjectionMatrix);
}

#endif
#ifdef GLSL

// <Semantic Name='POSITION' Attribute='a_position' />
// <Semantic Name='COLOR' Attribute='a_color' />
// <Semantic Name='TEXCOORD' Attribute='a_texcoord' />

uniform mat4 ViewProjectionMatrix;
uniform mat4 LightMatrix;
uniform vec3 LightPosition;
uniform vec2 viewsize;

attribute vec3 a_position;
attribute vec2 a_texcoord;
attribute vec4 a_color;

varying float pmx;
varying vec2 v_texcoord;
varying vec4 v_color;
varying vec2 s_texcoord;

vec2 WorldToScreen(vec3 source, vec2 size,mat4 worldViewProjection){
	vec4 result2 = worldViewProjection * vec4(source,1.0);
	vec3 result = result2.xyz;
	result /= source.x * worldViewProjection[0][3] + source.y * worldViewProjection[1][3] + source.z * worldViewProjection[2][3] + worldViewProjection[3][3];
	result.x = (result.x + 1.0) * 0.5 * size.x;
	result.y = (-result.y + 1.0) * 0.5 * size.y;
	return result.xy/size;
}


void main()
{
	v_color = a_color;
	v_texcoord = a_texcoord;
	pmx = distance(LightPosition,a_position);
	pmx = pmx / distance(LightPosition);
	s_texcoord = WorldToScreen(a_position,viewsize,LightMatrix);//获取太阳视角下的屏幕的纹理坐标
	gl_Position = ViewProjectionMatrix * vec4(a_position, 1.0);
	OPENGL_POSITION_FIX;
}

#endif
