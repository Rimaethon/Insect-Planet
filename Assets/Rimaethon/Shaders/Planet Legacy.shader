// Upgrade NOTE: upgraded instancing buffer 'ExoPlanetsLegacyPlanet' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Exo-Planets/Legacy/Planet"
{
	Properties
	{
[Range(0.1, 10.0)] _TonemappingIntensity("Tonemapping Intensity", Float) = 1.0



		[NoScaleOffset]_ColorTexture("Color Texture", 2D) = "gray" {}
		[Toggle]_EnableClouds("Enable Clouds", Float) = 1
		_PolarMask("Polar Mask", 2D) = "white" {}
		[NoScaleOffset]_NecessaryWaterMask("Necessary Water Mask", 2D) = "black" {}
		[NoScaleOffset]_CloudsTexture("CloudsTexture", 2D) = "black" {}
		_CitiesTexture("Cities Texture", 2D) = "black" {}
		[HDR]_ColorA("Color + A", Color) = (4.541205,4.541205,4.541205,0.3607843)
		_CloudSpeed("Cloud Speed", Float) = 1
		_ShadowsYOffset("Shadows Y Offset", Range( -0.02 , 0.02)) = -0.005
		_ShadowsXOffset("Shadows X Offset", Range( -0.02 , 0.02)) = -0.005
		_ShadowsSharpness("Shadows Sharpness", Range( 0 , 10)) = 2.5
		[Toggle]_EnableCities("Enable Cities", Float) = 1
		[HDR]_Citiescolor("Cities color", Color) = (7.906699,2.649365,1.200494,0)
		[HDR]_AtmosphereColor("Atmosphere Color", Color) = (0.3764706,1.027451,1.498039,0)
		_InteriorSize("Interior Size", Range( -2 , 10)) = 0.3
		_IlluminationSmoothness("Illumination Smoothness", Float) = 4
		_InteriorIntensity("Interior Intensity", Range( 0 , 1)) = 0.65
		[Toggle]_EnableAtmosphere("Enable Atmosphere", Float) = 1
		_IlluminationAmbient("Illumination Ambient", Color) = (0.09019608,0.06666667,0.1490196,0)
		_LightSource("_LightSource", Vector) = (1,0,0,0)
		_CitiesDetail("Cities Detail", Range( 1 , 20)) = 4
		_EnumFloat("_EnumFloat", Float) = 0
		_WaterColor("Water Color", Color) = (0.282353,0.4431373,0.5176471,0)
		_SpecularIntensity("Specular Intensity", Range( 0 , 1)) = 1
		_Normals("Normals", 2D) = "bump" {}
		_NormalsIntensity("Normals Intensity", Range( 0 , 2)) = 1
		[Toggle]_EnableWater("Enable Water", Float) = 1
		_CloudsNormals("Clouds Normals", 2D) = "bump" {}
		_ShadowColorA("Shadow Color + A", Color) = (0.09803922,0.2313726,0.4117647,1)
		_ReliefIntensity("ReliefIntensity", Float) = 2.001
		_ReliefSmoothness("Relief Smoothness", Range( 0 , 5)) = 2
		_IlluminationBoost("Illumination Boost", Float) = 1
		_SkyblendA("Sky blend (A)", Color) = (0.491436,0.5812334,0.748,0)

		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		     #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
		ENDCG
		Blend Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#include "UnityStandardUtils.cginc"
			#include "UnityStandardBRDF.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				float3 ase_normal : NORMAL;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
			};

			uniform sampler2D _ColorTexture;
			uniform float4 _WaterColor;
			uniform float _EnableWater;
			uniform sampler2D _NecessaryWaterMask;
			uniform float4 _ShadowColorA;
			uniform float _EnableClouds;
			uniform sampler2D _CloudsTexture;
			uniform float _ShadowsXOffset;
			uniform float _ShadowsYOffset;
			uniform float _CloudSpeed;
			uniform float _ShadowsSharpness;
			uniform float4 _ColorA;
			uniform sampler2D _PolarMask;
			uniform float _EnumFloat;
			uniform float4 _IlluminationAmbient;
			uniform float _ReliefIntensity;
			uniform sampler2D _CloudsNormals;
			uniform float _ReliefSmoothness;
			uniform float _NormalsIntensity;
			uniform sampler2D _Normals;
			uniform float _IlluminationSmoothness;
			uniform float _EnableCities;
			uniform sampler2D _CitiesTexture;
			uniform float _CitiesDetail;
			uniform float4 _Citiescolor;
			uniform float _EnableAtmosphere;
			uniform float _InteriorSize;
			uniform float _InteriorIntensity;
			uniform float4 _AtmosphereColor;
			uniform float _SpecularIntensity;
			uniform float4 _SkyblendA;
			uniform float _IlluminationBoost;
			UNITY_INSTANCING_BUFFER_START(ExoPlanetsLegacyPlanet)
				UNITY_DEFINE_INSTANCED_PROP(float4, _PolarMask_ST)
#define _PolarMask_ST_arr ExoPlanetsLegacyPlanet
				UNITY_DEFINE_INSTANCED_PROP(float3, _LightSource)
#define _LightSource_arr ExoPlanetsLegacyPlanet
			UNITY_INSTANCING_BUFFER_END(ExoPlanetsLegacyPlanet)

			float _TonemappingIntensity;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldTangent = UnityObjectToWorldDir(v.ase_tangent);
				o.ase_texcoord1.xyz = ase_worldTangent;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord2.xyz = ase_worldNormal;
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord3.xyz = ase_worldBitangent;
				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.ase_texcoord4.xyz = ase_worldPos;
				
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				o.ase_texcoord1.w = 0;
				o.ase_texcoord2.w = 0;
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				float2 uv_ColorTexture144 = i.ase_texcoord.xy;
				float4 color293 = IsGammaSpace() ? float4(0.75,0.75,0.75,0) : float4(0.5225216,0.5225216,0.5225216,0);
				float4 BaseColor156 = ( tex2D( _ColorTexture, uv_ColorTexture144 ) * color293 );
				float4 WaterColor307 = _WaterColor;
				float4 blendOpSrc359 = BaseColor156;
				float4 blendOpDest359 = WaterColor307;
				float WaterTransparency277 = _WaterColor.a;
				float4 lerpResult175 = lerp( BaseColor156 , ( saturate( (( blendOpDest359 > 0.5 ) ? ( 1.0 - 2.0 * ( 1.0 - blendOpDest359 ) * ( 1.0 - blendOpSrc359 ) ) : ( 2.0 * blendOpDest359 * blendOpSrc359 ) ) )) , WaterTransparency277);
				float4 lerpResult216 = lerp( lerpResult175 , WaterColor307 , WaterTransparency277);
				float4 BaseAndWater292 = lerpResult216;
				float2 uv_NecessaryWaterMask331 = i.ase_texcoord.xy;
				float clampResult304 = clamp( ( (( _EnableWater )?( tex2D( _NecessaryWaterMask, uv_NecessaryWaterMask331 ).b ):( 0.0 )) * 10.0 ) , 0.0 , 1.0 );
				float ContinentalMasks248 = clampResult304;
				float4 lerpResult181 = lerp( BaseColor156 , BaseAndWater292 , ContinentalMasks248);
				float3 desaturateInitialColor126 = lerpResult181.rgb;
				float desaturateDot126 = dot( desaturateInitialColor126, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar126 = lerp( desaturateInitialColor126, desaturateDot126.xxx, 0.3 );
				float2 appendResult152 = (float2(_ShadowsXOffset , _ShadowsYOffset));
				float2 uv0325 = i.ase_texcoord.xy * float2( 1,1 ) + appendResult152;
				float temp_output_371_0 = ( _CloudSpeed / 80.0 );
				float4 appendResult317 = (float4(temp_output_371_0 , 0.0 , 0.0 , 0.0));
				float4 UVcloudShadows194 = ( float4( uv0325, 0.0 , 0.0 ) + ( appendResult317 * _Time.x ) );
				float4 tex2DNode153 = tex2Dlod( _CloudsTexture, float4( UVcloudShadows194.xy, 0, _ShadowsSharpness) );
				float temp_output_378_0 = ( tex2DNode153.b * 2.0 );
				float CloudsAlpha302 = _ColorA.a;
				float clampResult221 = clamp( ( (( _EnableClouds )?( 1.0 ):( 0.0 )) * temp_output_378_0 * CloudsAlpha302 ) , 0.0 , 1.0 );
				float CloudsShadows107 = clampResult221;
				float4 _PolarMask_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_PolarMask_ST_arr, _PolarMask_ST);
				float2 uv_PolarMask = i.ase_texcoord.xy * _PolarMask_ST_Instance.xy + _PolarMask_ST_Instance.zw;
				float4 PolarMask103 = tex2D( _PolarMask, uv_PolarMask );
				float4 lerpResult169 = lerp( float4( desaturateVar126 , 0.0 ) , _ShadowColorA , ( ( CloudsShadows107 * PolarMask103 ) * _ShadowColorA.a ));
				float4 CloudsColor193 = ( 0.75 * _ColorA );
				float2 uv0142 = i.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float4 appendResult330 = (float4(temp_output_371_0 , 0.0 , 0.0 , 0.0));
				float4 UVClouds308 = ( float4( uv0142, 0.0 , 0.0 ) + ( appendResult330 * _Time.x ) );
				float lerpResult323 = lerp( tex2D( _CloudsTexture, UVClouds308.xy ).g , _EnumFloat , 0.0);
				float saferPower337 = max( lerpResult323 , 0.0001 );
				float Clouds255 = ( (0.0 + (pow( saferPower337 , 0.5 ) - 0.0) * (CloudsAlpha302 - 0.0) / (1.0 - 0.0)) * (( _EnableClouds )?( 1.0 ):( 0.0 )) );
				float4 lerpResult108 = lerp( lerpResult169 , ( 0.5 * CloudsColor193 ) , ( PolarMask103 * Clouds255 ));
				float4 AmbientColor220 = _IlluminationAmbient;
				float4 color163 = IsGammaSpace() ? float4(0.5294118,0.2701871,0.2038235,0) : float4(0.2422812,0.05933543,0.0343086,0);
				float3 FlatNormal217 = float3(0,0,1);
				float3 lerpResult416 = lerp( FlatNormal217 , UnpackScaleNormal( tex2Dlod( _CloudsNormals, float4( UVClouds308.xy, 0, _ReliefSmoothness) ), ( ( _ReliefIntensity * CloudsAlpha302 ) * 0.5 ) ) , tex2D( _PolarMask, uv_PolarMask ).rgb);
				float2 uv0243 = i.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float3 lerpResult300 = lerp( UnpackScaleNormal( tex2D( _Normals, uv0243 ), ( (0.0 + (_NormalsIntensity - 0.0) * (1.0 - 0.0) / (2.0 - 0.0)) * 0.25 ) ) , FlatNormal217 , ContinentalMasks248);
				float clampResult269 = clamp( ( 3.0 * Clouds255 ) , 0.0 , 1.0 );
				float CloudsOcclusion368 = ( 1.0 - clampResult269 );
				float3 lerpResult196 = lerp( lerpResult416 , lerpResult300 , CloudsOcclusion368);
				float3 Normals229 = lerpResult196;
				float3 ase_worldTangent = i.ase_texcoord1.xyz;
				float3 ase_worldNormal = i.ase_texcoord2.xyz;
				float3 ase_worldBitangent = i.ase_texcoord3.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 tanNormal168 = Normals229;
				float3 worldNormal168 = normalize( float3(dot(tanToWorld0,tanNormal168), dot(tanToWorld1,tanNormal168), dot(tanToWorld2,tanNormal168)) );
				float3 _LightSource_Instance = UNITY_ACCESS_INSTANCED_PROP(_LightSource_arr, _LightSource);
				float3 normalizeResult400 = normalize( _LightSource_Instance );
				float3 LightSourceVector398 = ( normalizeResult400 / 1.0 );
				float dotResult239 = dot( worldNormal168 , LightSourceVector398 );
				float smoothstepResult145 = smoothstep( 0.0 , 1.0 , ( dotResult239 + 0.5 ));
				float BaselLightMask340 = smoothstepResult145;
				float temp_output_353_0 = pow( BaselLightMask340 , _IlluminationSmoothness );
				float clampResult350 = clamp( ( ( ( temp_output_353_0 + 0.0 ) * ( 1.0 - BaselLightMask340 ) ) * 10.0 ) , 0.0 , 1.0 );
				float4 lerpResult303 = lerp( AmbientColor220 , color163 , clampResult350);
				float4 temp_cast_8 = (temp_output_353_0).xxxx;
				float4 lerpResult347 = lerp( lerpResult303 , temp_cast_8 , temp_output_353_0);
				float4 NightDayMask233 = lerpResult347;
				float4 temp_cast_9 = (0.0).xxxx;
				float2 temp_cast_10 = (_CitiesDetail).xx;
				float2 uv0333 = i.ase_texcoord.xy * temp_cast_10 + float2( 0,0 );
				float2 uv0271 = i.ase_texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float clampResult148 = clamp( ContinentalMasks248 , 0.0 , 1.0 );
				float3 desaturateInitialColor265 = ( 1.0 - ( NightDayMask233 * 5.0 ) ).rgb;
				float desaturateDot265 = dot( desaturateInitialColor265, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar265 = lerp( desaturateInitialColor265, desaturateDot265.xxx, 1.0 );
				float3 clampResult314 = clamp( desaturateVar265 , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float3 ase_worldPos = i.ase_texcoord4.xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult382 = dot( ase_worldViewDir , ase_worldNormal );
				float FresnelMask381 = dotResult382;
				float saferPower122 = max( FresnelMask381 , 0.0001 );
				float4 Cities130 = (( _EnableCities )?( ( ( float4( ( ( ( ( ( tex2D( _CitiesTexture, uv0333 ).r * ( 1.0 - tex2D( _CitiesTexture, uv0271 ).a ) ) * ( 1.0 - clampResult148 ) ) * clampResult314 ) * pow( saferPower122 , 4.0 ) ) * CloudsOcclusion368 ) , 0.0 ) * _Citiescolor ) * 1.0 ) ):( temp_cast_9 ));
				float4 color109 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 normalizeResult157 = normalize( ( ase_worldViewDir + LightSourceVector398 ) );
				float3 SpecularDir226 = normalizeResult157;
				float3 saferPower294 = max( SpecularDir226 , 0.0001 );
				float fresnelNdotV342 = dot( normalize( normalizedWorldNormal ), ase_worldViewDir );
				float fresnelNode342 = ( 0.0 + 1.0 * pow( max( 1.0 - fresnelNdotV342 , 0.0001 ), ( ( 1.0 - ( pow( saferPower294 , 3.0 ) + -1.0 ) ) + _InteriorSize ).x ) );
				float3 temp_cast_14 = (fresnelNode342).xxx;
				float3 temp_cast_15 = (fresnelNode342).xxx;
				float3 linearToGamma264 = LinearToGammaSpace( temp_cast_15 );
				float4 BaseColorAtmospheres407 = _AtmosphereColor;
				float dotResult395 = dot( LightSourceVector398 , normalizedWorldNormal );
				float smoothstepResult394 = smoothstep( -0.4 , 1.0 , dotResult395);
				float AtmosphereLightMask393 = smoothstepResult394;
				float clampResult111 = clamp( AtmosphereLightMask393 , 0.0 , 1.0 );
				float smoothstepResult199 = smoothstep( 0.0 , 1.0 , pow( clampResult111 , 1.5 ));
				float4 clampResult213 = clamp( ( ( float4( ( ( linearToGamma264 * ( _InteriorIntensity + ( 1.0 - (0.0 + (_InteriorSize - -2.0) * (1.0 - 0.0) / (10.0 - -2.0)) ) ) ) * _InteriorIntensity ) , 0.0 ) * BaseColorAtmospheres407 ) * smoothstepResult199 ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float3 gammaToLinear218 = GammaToLinearSpace( (( _EnableAtmosphere )?( clampResult213 ):( color109 )).rgb );
				float3 SubAtmosphere403 = gammaToLinear218;
				float4 blendOpSrc190 = ( ( lerpResult108 * NightDayMask233 ) + ( Cities130 * PolarMask103 ) );
				float4 blendOpDest190 = float4( SubAtmosphere403 , 0.0 );
				float dotResult357 = dot( ase_worldNormal , SpecularDir226 );
				float clampResult322 = clamp( dotResult357 , 0.0 , 1.0 );
				float saferPower171 = max( clampResult322 , 0.0001 );
				float temp_output_171_0 = pow( saferPower171 , 2.0 );
				float saferPower137 = max( temp_output_171_0 , 0.0001 );
				ase_worldViewDir = Unity_SafeNormalize( ase_worldViewDir );
				float dotResult388 = dot( LightSourceVector398 , ase_worldViewDir );
				float ViewDotLight387 = dotResult388;
				float clampResult356 = clamp( ( ViewDotLight387 + 0.1 ) , 0.0 , 1.0 );
				float lerpResult273 = lerp( 200.0 , 2000.0 , clampResult356);
				float3 temp_cast_20 = (( pow( saferPower137 , ( lerpResult273 * (0.0 + (0.5 - 0.0) * (1.0 - 0.0) / (30.0 - 0.0)) ) ) * 0.5 )).xxx;
				float3 temp_cast_21 = (( pow( saferPower137 , ( lerpResult273 * (0.0 + (0.5 - 0.0) * (1.0 - 0.0) / (30.0 - 0.0)) ) ) * 0.5 )).xxx;
				float3 gammaToLinear118 = GammaToLinearSpace( temp_cast_21 );
				float clampResult102 = clamp( ViewDotLight387 , 0.0 , 1.0 );
				float lerpResult242 = lerp( 0.25 , _SpecularIntensity , clampResult102);
				float4 temp_output_160_0 = ( (( _EnableWater )?( tex2D( _NecessaryWaterMask, uv_NecessaryWaterMask331 ).b ):( 0.0 )) * ( ( ( float4( gammaToLinear118 , 0.0 ) * ( temp_output_171_0 * WaterColor307 ) ) * CloudsOcclusion368 ) * ( lerpResult242 * 50.0 ) ) );
				float4 Specular155 = temp_output_160_0;
				float4 lerpResult421 = lerp( ( ( 1.0 - ( 1.0 - blendOpSrc190 ) * ( 1.0 - blendOpDest190 ) ) + ( Specular155 * PolarMask103 ) ) , _SkyblendA , _SkyblendA.a);
				float4 SecondPassInput211 = ( lerpResult421 * _IlluminationBoost );
				
				
				finalColor = SecondPassInput211;

		
			
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "PlanetEditor"
	
	
}
