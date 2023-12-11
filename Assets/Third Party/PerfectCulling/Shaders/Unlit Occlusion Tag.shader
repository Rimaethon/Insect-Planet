Shader "Koenigz/Unlit Occlusion Tag" 
{
	Properties
	{
		[PerRendererData] _Color("Main Color", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
	}
		
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100
		
		Pass
		{
			Lighting Off
			Cull [_Cull]
			Fog {Mode Off}
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#pragma multi_compile_instancing
				#pragma instancing_options nolightprobe nolightmap
			
				#include "UnityCG.cginc"

				struct v2f
				{
					float4 vertex : SV_POSITION;

					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
				UNITY_INSTANCING_BUFFER_END(Props)

				v2f vert(appdata_full  v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

					o.vertex = UnityObjectToClipPos(v.vertex);
					
					return o;
				}

				float generateNoise(in float2 xy, in float seed)
				{
					return frac(tan(distance(xy * 1.618033988749895, xy) * seed) * xy.x);
				}
			
				float4 frag(v2f i) : COLOR
				{
					UNITY_SETUP_INSTANCE_ID(i);

					float4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

					if (col.a >= 0.5)
					{
				        if (generateNoise(float2(i.vertex.xy + i.vertex.zz), 12) < 0.75)
				        {
				        	discard;
				        }
					}

					// Always render opaque in the end
					col.a = 1.0;

					if (!IsGammaSpace())
					{
						col.r = LinearToGammaSpaceExact(col.r);
						col.g = LinearToGammaSpaceExact(col.g);
						col.b = LinearToGammaSpaceExact(col.b);
					}
					
					return col;
				}
			ENDCG
		}
	}
}