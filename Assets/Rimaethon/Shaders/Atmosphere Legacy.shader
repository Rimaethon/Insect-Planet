// Upgrade NOTE: upgraded instancing buffer 'ExoPlanetsLegacyAtmosphere' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Exo-Planets/Legacy/Atmosphere"
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
	CustomEditor "AtmosphereEditor"
	
	
}
/*ASEBEGIN
Version=17700
-1680;203;1680;989;2848.936;661.4293;1;True;False
Node;AmplifyShaderEditor.CommentaryNode;68;-6088.463,496.3987;Inherit;False;1011.591;371.1455;Light Source Vector from script;5;74;72;71;70;69;Light Source Vector;1,0.6068678,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;69;-6055.354,561.9867;Inherit;False;304;234;Input is set via LightSource.cs;1;73;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;73;-6004.354,622.9867;Float;False;InstancedProperty;_LightSourceAtmo;_LightSourceAtmo;3;0;Create;True;0;0;False;0;1,0,0;1,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;74;-5707.623,700.1371;Float;False;Constant;_Float2;Float 2;36;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;72;-5682.854,559.4769;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;50;-4680.73,173.2502;Inherit;False;904.2393;398.7203;Hand made fresnel;4;54;53;52;51;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;71;-5491.754,676.8129;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;53;-4637.879,410.4162;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;54;-4646.811,227.59;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;52;-4361.339,300.0197;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;6;-3180.747,-422.277;Inherit;False;3207.77;1197.935;Atmosphere Emissive + Vertex Offset;5;39;29;12;8;7;Atmosphere ;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;75;-4697.25,688.8053;Inherit;False;891.5458;346.1274;Angle Between Light Source and Camera ;4;56;58;57;55;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;70;-5327.764,618.2386;Float;False;LightSourceVector;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;60;-4750.514,1080.29;Inherit;False;910.5537;332.3206;Custom mask for atmohsphere;7;67;66;65;64;63;62;61;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-4056.39,299.6369;Float;False;FresnelMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;7;-3140.926,-372.2125;Inherit;False;2693.273;464.8405;Atmosphere Controls ;21;46;44;43;41;40;33;32;31;30;27;26;25;23;20;19;14;13;10;9;22;89;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;55;-4689.704,744.1694;Inherit;False;70;LightSourceVector;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;58;-4574.129,806.3435;Float;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;62;-4733.814,1153.813;Inherit;False;70;LightSourceVector;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;67;-4683.504,1253.657;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;66;-4439.404,1177.509;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;57;-4327.965,804.6321;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-4298.373,1359.534;Float;False;Constant;_Float0;Float 0;39;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;8;-3119.957,162.7416;Inherit;False;1530.697;503.61;Fine tuning of Dir mask;1;11;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;44;-3114.256,-191.2965;Inherit;False;51;FresnelMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;10;-3129.466,-102.9742;Inherit;False;346;136;Material Property;1;47;;0,1,0.3411765,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;63;-4302.564,1285.694;Float;False;Constant;_Float13;Float 13;38;0;Create;True;0;0;False;0;-0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;11;-3109.086,224.476;Inherit;False;1495.317;370.623;Mask controls for Atmosphere;10;45;38;37;36;35;34;28;18;17;16;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-4030.039,731.5725;Float;False;ViewDotLight;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;65;-4283.793,1156.53;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;19;-3105.167,-295.3785;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-3122.556,-58.76915;Float;False;Property;_ExteriorSize;Exterior Size;1;0;Create;True;0;0;False;0;0.3;0.27;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-3035.766,287.3685;Inherit;False;56;ViewDotLight;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;64;-4079.184,1153.743;Float;False;AtmosphereLightMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;40;-2720.747,-171.5484;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;3;False;4;FLOAT;3.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;33;-2954.086,-295.4645;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;16;-3037.586,512.9203;Inherit;False;64;AtmosphereLightMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-2626.716,440.1117;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;30;-2761.607,-303.6852;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;4.15;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;38;-2647.626,264.0287;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;10;False;3;FLOAT;0;False;4;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-2416.567,491.3265;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;32;-2489.747,-305.4586;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.01;False;3;FLOAT;0;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;9;-2269.376,-96.2545;Inherit;False;346;136;Material Property;1;48;;0,1,0.3411765,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;37;-2312.056,288.4633;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2212.876,434.6556;Float;False;Constant;_Float15;Float 15;38;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-2248.167,-285.7418;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-2221.876,520.6566;Float;False;Constant;_Float18;Float 18;39;0;Create;True;0;0;False;0;20;16.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;20;-2079.216,-324.6403;Float;False;Constant;_Color2;Color 2;39;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;48;-2213.876,-45.80626;Float;False;Property;_ExteriorIntensity;Exterior Intensity;0;0;Create;True;0;0;False;0;0.25;0.27;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;76;-1673.249,1131.954;Inherit;False;579;239;Material Property;2;78;77;;0,1,0.3426006,1;0;0
Node;AmplifyShaderEditor.SmoothstepOpNode;45;-2056.657,283.4095;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;78;-1629.619,1183.058;Float;False;Property;_AtmosphereColor;Atmosphere Color;4;1;[HDR];Create;True;0;0;False;0;0.3764706,1.027451,1.498039,0;1.272658,1.272658,1.272658,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;43;-1846.796,-319.3746;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;28;-1827.147,282.5306;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-1368.6,1199.989;Float;False;BaseColorAtmospheres;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GammaToLinearNode;27;-1675.996,-175.9703;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-1419.416,-159.0758;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-1408.756,-301.2477;Inherit;False;77;BaseColorAtmospheres;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-1108.266,-180.2535;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;12;-988.067,155.4272;Inherit;False;554.1529;385.2613;Vertex offset;4;42;24;21;15;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;46;-930.2867,-339.5035;Float;False;Constant;_Color1;Color 1;39;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PosVertexDataNode;24;-947.2964,391.8656;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;22;-924.6205,-132.067;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;21;-937.0767,210.0453;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;3;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;42;-760.3267,335.0463;Inherit;False;1;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;41;-720.147,-172.8551;Float;False;Property;_EnableAtmosphere;Enable Atmosphere;2;0;Create;True;0;0;False;0;1;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LinearToGammaNode;14;-454.6664,-73.61974;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-626.2661,250.7963;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;39;-346.6215,254.68;Float;False;AtmosphereSize;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;29;-263.4467,-165.9215;Float;False;AtmosphereColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LinearToGammaNode;89;-1661.746,-356.9898;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;87;76.83124,-267.7058;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GammaToLinearNode;13;-456.6664,-250.6197;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;88;181.2893,-128.8983;Float;False;True;-1;2;AtmosphereEditor;100;1;Exo-Planets/Legacy/Atmosphere;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;4;1;False;-1;1;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;1;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;2;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Overlay=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;0
Node;AmplifyShaderEditor.CommentaryNode;59;-4330.793,1118.53;Inherit;False;245;297.0039;Mask Softening;0;;1,1,1,1;0;0
WireConnection;72;0;73;0
WireConnection;71;0;72;0
WireConnection;71;1;74;0
WireConnection;52;0;54;0
WireConnection;52;1;53;0
WireConnection;70;0;71;0
WireConnection;51;0;52;0
WireConnection;66;0;62;0
WireConnection;66;1;67;0
WireConnection;57;0;55;0
WireConnection;57;1;58;0
WireConnection;56;0;57;0
WireConnection;65;0;66;0
WireConnection;65;1;63;0
WireConnection;65;2;61;0
WireConnection;19;0;44;0
WireConnection;64;0;65;0
WireConnection;40;0;47;0
WireConnection;33;0;19;0
WireConnection;17;0;36;0
WireConnection;30;0;33;0
WireConnection;30;1;40;0
WireConnection;38;0;36;0
WireConnection;34;0;17;0
WireConnection;34;1;16;0
WireConnection;32;0;30;0
WireConnection;37;0;38;0
WireConnection;37;1;34;0
WireConnection;31;0;32;0
WireConnection;45;0;37;0
WireConnection;45;1;18;0
WireConnection;45;2;35;0
WireConnection;43;0;20;0
WireConnection;43;1;31;0
WireConnection;43;2;48;0
WireConnection;28;0;45;0
WireConnection;77;0;78;0
WireConnection;27;0;43;0
WireConnection;25;0;28;0
WireConnection;25;1;27;0
WireConnection;23;0;26;0
WireConnection;23;1;25;0
WireConnection;22;0;23;0
WireConnection;21;0;47;0
WireConnection;42;0;24;0
WireConnection;41;0;46;0
WireConnection;41;1;22;0
WireConnection;14;0;41;0
WireConnection;15;0;21;0
WireConnection;15;1;42;0
WireConnection;39;0;15;0
WireConnection;29;0;14;0
WireConnection;89;0;43;0
WireConnection;13;0;41;0
WireConnection;88;0;29;0
WireConnection;88;1;39;0
ASEEND*/
//CHKSM=F54C20DD94825AEE8F6048276DCF1B5383A869A3