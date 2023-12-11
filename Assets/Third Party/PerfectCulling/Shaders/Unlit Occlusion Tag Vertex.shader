Shader "Koenigz/Unlit Occlusion Tag Vertex" 
{
	Properties
	{
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
				#pragma target 2.0

				#include "UnityCG.cginc"

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					nointerpolation float4 color    : COLOR;
				};
		
				v2f vert(appdata_full v)
				{
					v2f o;
					
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.color = v.color;
					
					return o;
				}

				float generateNoise(in float2 xy, in float seed)
				{
					return frac(tan(distance(xy * 1.618033988749895, xy) * seed) * xy.x);
				}
			
				float4 frag(v2f i) : COLOR
				{
					float4 col = i.color;

					if (col.a >= 0.5)
					{
				        if (generateNoise(float2(i.vertex.xy + i.vertex.zz), 12) < 0.75)
				        {
				        	discard;
				        }
					}

					// Always render opaque in the end
					col.a = 1.0;

					// No color space conversion necessary
					
					return col;
				}
			ENDCG
		}
	}
}