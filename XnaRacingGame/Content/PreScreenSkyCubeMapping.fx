// Project: RacingGame, File: PreScreenSkyCubeMapping.fx
// Creation date: 06.11.2005 22:28
// Last modified: 06.11.2005 22:39
// Author: Benjamin Nitschke (abi@exdream.com) (c) 2005
// Note: To test this use FX Composer from NVIDIA!

// Ruffly based on NVidia's preCubeBg.fx, Copyright NVIDIA Corporation 2002, see FX Composer for more details.
string description = "Shows the sky bg with help of a cube map texture, should be called before anything else is rendered.";

// Script for FX Composer, not used by app
float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "preprocess";
	string ScriptOutput = "color";
	
	// We just call a script in the main technique.
	string Script = "Clear=Depth;Technique=SkyCubeMap;ScriptExternal=color;";
> = 1.0;

// Unlike most other shaders we only need viewInverse, ambientColor
// for the scene brightness and diffuseTexture, which is the background
// cube map texture for the sky.
float4x4 viewInverse           : ViewInverse;

float scale
<
	string UIName = "Scale";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 2.0;
	float UIStep = 0.01;
> = 1.0f;

// The ambient color for the sky, should be 1 for normal brightness.
float4 ambientColor : Ambient
<
	string UIName = "Ambient Color";
	string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

// Texture and samplers
texture diffuseTexture : Environment
<
	string UIName = "Diffuse Texture";
	string ResourceName = "R:\\Textures\\SpaceSkyCubeMap.dds";
	string ResourceType = "CUBE";
>;

samplerCUBE diffuseTextureSampler = sampler_state
{
	Texture = <diffuseTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	AddressW  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

//-------------------------------------

struct VertexInput
{
	// We only need 2d screen position, that's all.
	float2 pos      : POSITION;
};

struct VB_OutputPos3DTexCoord
{
	float4 pos      : POSITION;
	float3 texCoord : TEXCOORD0;
};

VB_OutputPos3DTexCoord VS_SkyCubeMap(VertexInput In)
{
	VB_OutputPos3DTexCoord Out;
	Out.pos = float4(In.pos.xy, 0, 1);
	// Multiply x by aspect ratio, else we get a streched image!
	//Out.texCoord = mul(float4(In.pos.x * 1.33f, In.pos.y, scale, 0), viewInverse).xyz; 
	
	// Also negate xy because the cube map is for MDX (left handed) and is upside down
	Out.texCoord = mul(float4(-In.pos, scale, 0), viewInverse).xyz;
	
	// And fix rotation too (we use better x, y ground plane system)
	Out.texCoord = float3(
		-Out.texCoord.x*1.0f,
		-Out.texCoord.z*0.815f,
		-Out.texCoord.y*1.0f);
		
	return Out;
} // VS_SkyCubeMap(..)

float4 PS_SkyCubeMap(VB_OutputPos3DTexCoord In) : COLOR
{
	float4 texCol = ambientColor *
		texCUBE(diffuseTextureSampler, In.texCoord);
	return texCol;
} // PS_SkyCubeMap(.)

technique SkyCubeMap < string Script = "Pass=P0;"; >
{
	pass P0 < string Script = "Draw=Buffer;"; >
	{
		//done in app:
		ZEnable = false;
		VertexShader = compile vs_1_1 VS_SkyCubeMap();
		PixelShader  = compile ps_1_1 PS_SkyCubeMap();
	} // pass P0
} // technique SkyCubeMap
