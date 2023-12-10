#ifndef TERRAIN_TO_MESH_SPLATMAP_CGINC
#define TERRAIN_TO_MESH_SPLATMAP_CGINC


//Disabled by default
//#define TERRAIN_TO_MESH_BUILTIN_SAMPLER


//BUG: Unrecognized sampler 'sampler_t2m_layer_0_diffuse'
#if defined(TERRAIN_TO_MESH_SHADERPASS_DEPTHONLY) || defined(TERRAIN_TO_MESH_SHADERPASS_DEPTHNORMALS) || defined(TERRAIN_TO_MESH_SHADERPASS_DEPTHNORMALSONLY)
    #ifndef TERRAIN_TO_MESH_BUILTIN_SAMPLER
    #define TERRAIN_TO_MESH_BUILTIN_SAMPLER
    #endif
#endif


#include "../cginc/Core.cginc"


//Curved World//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void SHG_TerrainToMeshCurvedWorld_float(float3 inVertex, float3 inNormal, float4 inTangent, out float3 outVertex, out float3 outNormal)
{
    float4 vertex = float4(inVertex, 1);
    float3 normal = inNormal;
    float4 tangent =  inTangent;

    //Curved World
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
        #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON            
            CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(vertex, normal, tangent)
        #else
            CURVEDWORLD_TRANSFORM_VERTEX(vertex)
        #endif
    #endif


    outVertex = vertex.xyz;
    outNormal = normal.xyz;
}

void SHG_TerrainToMeshCurvedWorld_half(float3 inVertex, float3 inNormal, float4 inTangent, out float3 outVertex, out float3 outNormal)
{
    SHG_TerrainToMeshCurvedWorld_float(inVertex, inNormal, inTangent, outVertex, outNormal);
}


//Holes//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void SHG_TerrainToMeshCalculateClipValue_float(float4 uv, out float clipValue)
{
     clipValue = TerrainToMeshCalculateClipValue(uv.xy);	
}

void SHG_TerrainToMeshCalculateClipValue_half(float4 uv, out float clipValue)
{
     SHG_TerrainToMeshCalculateClipValue_float(uv, clipValue);	
} 


//Layers//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
void SHG_TerrainToMeshCalculateLayersBlend_float(float4 uv, out float3 albedoValue, out float alphaValue, out float3 normalValue, out float metallicValue, out float smoothnessValue, out float occlusionValue)
{
    #if defined(TERRAIN_TO_MESH_SHADERPASS_SHADOWCASTER) || defined(TERRAIN_TO_MESH_SHADERPASS_DEPTHONLY) || defined(TERRAIN_TO_MESH_SHADERPASS_DEPTHNORMALS)

        albedoValue = 0;
        alphaValue = 1;
        normalValue = float3(0, 0, 1);
        metallicValue = 0;
        smoothnessValue = 0;
        occlusionValue = 0;

    #else

        TerrainToMeshCalculateLayersBlend(uv.xy, albedoValue, alphaValue, normalValue, metallicValue, smoothnessValue, occlusionValue);	

    #endif
}

void SHG_TerrainToMeshCalculateLayersBlend_half(float4 uv, inout float3 albedoValue, inout float alphaValue, inout float3 normalValue, inout float metallicValue, inout float smoothnessValue, out float occlusionValue)
{
    SHG_TerrainToMeshCalculateLayersBlend_float(uv, albedoValue, alphaValue, normalValue, metallicValue, smoothnessValue, occlusionValue);	
}

 
#endif   