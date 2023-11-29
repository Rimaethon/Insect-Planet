// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Skybox/CubemapClampedHDR" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    //_Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
    _HDRIntensityThreshold ("HDR Intensity Threshold", Range(0, 10)) = 2.0 // Adjust the range as needed


}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"

        samplerCUBE _Tex;
        half4 _Tex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;
        half _HDRIntensityThreshold;

       

        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.texcoord = v.vertex.xyz;
            return o;
        }

       fixed4 frag (v2f i) : SV_Target
{
    half4 tex = texCUBE (_Tex, i.texcoord);
    half3 c = DecodeHDR (tex, _Tex_HDR);
    c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
    c *= _Exposure;

    // Use the inspector-exposed property for HDR intensity threshold
    c = saturate(min(c, _HDRIntensityThreshold-0.5));

    return half4(c, 1);
}

        ENDCG
    }
}


Fallback Off

}