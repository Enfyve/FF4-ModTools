sampler2D input : register(s0);
sampler2D blend : register(s1);

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float4 inputColor;
	inputColor = tex2D(input, uv);

	float4 blendColor;
	blendColor = tex2D(blend, uv);

	// R = Base + Blend - 1
	inputColor.r = inputColor.r + blendColor.r - 1;
	inputColor.g = inputColor.g + blendColor.g - 1;
	inputColor.b = inputColor.b + blendColor.b - 1;

	return inputColor;
}
