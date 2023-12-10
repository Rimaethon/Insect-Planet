Shader "Hidden/Amazing Assets/Terrain To Mesh/Editor"
{
	Properties 
	{
		_Color("", Color) = (1, 1, 1, 1)
		_MainTex("", 2D) = "white" {}
	}
	 


	CGINCLUDE

	#include "UnityCG.cginc"


	float4 _Color;

	sampler2D _MainTex;
	float4 _MainTex_TexelSize; 

	UNITY_DECLARE_TEX2DARRAY(_TextureArray);
	int _TextureArrayIndex;

	
	float4 fragRGB(v2f_img i) : SV_Target 
	{
		float4 c = tex2D(_MainTex, i.uv);
		
		return float4(c.rgb, 1);
	} 

	float4 fragTextureArrayRGB(v2f_img i) : SV_Target 
	{
		float4 c = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv, _TextureArrayIndex));
		
		return float4(c.rgb, 1);
	} 

	ENDCG 
	

	SubShader    
	{		
		Pass	//0
	    {
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert_img
	    	#pragma fragment fragRGB
			ENDCG

		} //Pass

		Pass	//1
	    {
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
			#pragma vertex vert_img
	    	#pragma fragment fragTextureArrayRGB
			ENDCG

		} //Pass

	} //SubShader
	 
} //Shader
