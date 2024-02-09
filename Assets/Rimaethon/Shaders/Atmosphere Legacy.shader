
Shader "Rimaethon/Atmosphere"
{
	Properties
	{
		_ExteriorIntensity("Exterior Intensity", Range( 0 , 1)) = 0.25
		_ExteriorSize("Exterior Size", Range( 0.1 , 1)) = 0.3
		[Toggle]_EnableAtmosphere("Enable Atmosphere", Float) = 1
		_LightSourceAtmo("_LightSourceAtmo", Vector) = (1,0,0,0)
		[HDR]_AtmosphereColor("Atmosphere Color", Color) = (0.3764706,1.027451,1.498039,0)

	}

	SubShader
	{


		Tags { "RenderType"="Overlay" }
	LOD 100

		CGINCLUDE
		     #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
		ENDCG
		Blend One One
		Cull Front
		ColorMask RGBA
		ZWrite Off
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
			#include "UnityStandardBRDF.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float3 ase_normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_texcoord1 : TEXCOORD1;
			};

			uniform float _ExteriorSize;
			uniform float _EnableAtmosphere;
			uniform float4 _AtmosphereColor;
			uniform float _ExteriorIntensity;
			UNITY_INSTANCING_BUFFER_START(ExoPlanetsLegacyAtmosphere)
				UNITY_DEFINE_INSTANCED_PROP(float3, _LightSourceAtmo)
#define _LightSourceAtmo_arr ExoPlanetsLegacyAtmosphere
			UNITY_INSTANCING_BUFFER_END(ExoPlanetsLegacyAtmosphere)


			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 AtmosphereSize39 = ( (0.0 + (_ExteriorSize - 0.0) * (1.0 - 0.0) / (3.0 - 0.0)) * ( v.vertex.xyz * 1 ) );

				float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.ase_texcoord.xyz = ase_worldPos;
				float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;


				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.w = 0;
				o.ase_texcoord1.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = AtmosphereSize39;
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
				float4 color46 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
				float4 BaseColorAtmospheres77 = _AtmosphereColor;
				float3 _LightSourceAtmo_Instance = UNITY_ACCESS_INSTANCED_PROP(_LightSourceAtmo_arr, _LightSourceAtmo);
				float3 normalizeResult72 = normalize( _LightSourceAtmo_Instance );
				float3 LightSourceVector70 = ( normalizeResult72 / 1.0 );
				float3 ase_worldPos = i.ase_texcoord.xyz;
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
				ase_worldViewDir = Unity_SafeNormalize( ase_worldViewDir );
				float dotResult57 = dot( LightSourceVector70 , ase_worldViewDir );
				float ViewDotLight56 = dotResult57;
				float3 ase_worldNormal = i.ase_texcoord1.xyz;
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float dotResult66 = dot( LightSourceVector70 , normalizedWorldNormal );
				float smoothstepResult65 = smoothstep( -0.4 , 1.0 , dotResult66);
				float AtmosphereLightMask64 = smoothstepResult65;
				float smoothstepResult45 = smoothstep( 0.0 , 20.0 , ( (0.0 + (ViewDotLight56 - 0.0) * (0.1 - 0.0) / (10.0 - 0.0)) + ( ( ViewDotLight56 * 0.0 ) + AtmosphereLightMask64 ) ));
				float4 color20 = IsGammaSpace() ? float4(0,0,0,0) : float4(0,0,0,0);
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult52 = dot( ase_worldViewDir , ase_worldNormal );
				float FresnelMask51 = dotResult52;
				float4 temp_cast_0 = (( (0.0 + (pow( pow( -FresnelMask51 , 1.5 ) , (3.0 + (_ExteriorSize - 0.0) * (3.5 - 3.0) / (1.0 - 0.0)) ) - 0.0) * (10.0 - 0.0) / (0.01 - 0.0)) * 1.0 )).xxxx;
				float4 lerpResult43 = lerp( color20 , temp_cast_0 , _ExteriorIntensity);
				float3 gammaToLinear27 = GammaToLinearSpace( lerpResult43.rgb );
				float4 clampResult22 = clamp( ( BaseColorAtmospheres77 * float4( ( (0.0 + (smoothstepResult45 - 0.0) * (10.0 - 0.0) / (1.0 - 0.0)) * gammaToLinear27 ) , 0.0 ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float3 linearToGamma14 = LinearToGammaSpace( (( _EnableAtmosphere )?( clampResult22 ):( color46 )).rgb );
				float3 AtmosphereColor29 = linearToGamma14;


				finalColor = float4( AtmosphereColor29 , 0.0 );
				return finalColor;
			}
			ENDCG
		}
	}
}
