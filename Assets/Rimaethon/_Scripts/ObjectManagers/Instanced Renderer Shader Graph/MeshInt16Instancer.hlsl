#include <UnityShaderVariables.cginc>
#ifndef MESH_SHADER_INCLUDED
#define MESH_SHADER_INCLUDED

StructuredBuffer<float3> _PerInstanceData;

float4x4 worldMatrixInvesrse;

void instancingSetup() {
    #ifndef SHADERGRAPH_PREVIEW

    float3 instanceData = _PerInstanceData[unity_InstanceID];
    float4x4 worldMatrixConverted = float4x4(
        float4(1, 0, 0, instanceData.x),
        float4(0, 1, 0, instanceData.y),
        float4(0, 0, 1, instanceData.z),
        float4(0, 0, 0, 1)
    );
    float4x4 worldMatrixInverse = transpose(worldMatrixConverted);
    unity_ObjectToWorld = mul(unity_ObjectToWorld, worldMatrixConverted);
    unity_WorldToObject = mul(unity_WorldToObject, worldMatrixInverse);
    #endif
}

void GetInstanceID_float(out float Out) {
    Out = 0;
    #ifndef SHADERGRAPH_PREVIEW
    #if UNITY_ANY_INSTANCING_ENABLED
    Out = unity_InstanceID;
    #endif
    #endif
}

void Instancer_float(float3 Position, out float3 Out) {
    Out = Position;
}

#endif
