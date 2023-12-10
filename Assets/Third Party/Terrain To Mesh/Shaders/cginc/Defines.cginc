#ifndef TERRAIN_TO_MESH_DEFINES_CGINC
#define TERRAIN_TO_MESH_DEFINES_CGINC



#if defined(_T2M_LAYER_COUNT_3) 

    #define NEED_PAINT_MAP_2

#elif defined(_T2M_LAYER_COUNT_4)

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3

#elif defined(_T2M_LAYER_COUNT_5)

    #define NEED_SPLAT_MAP_1

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4

#elif defined(_T2M_LAYER_COUNT_6)

    #define NEED_SPLAT_MAP_1

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5

#elif defined(_T2M_LAYER_COUNT_7)

    #define NEED_SPLAT_MAP_1

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6

#elif defined(_T2M_LAYER_COUNT_8)

    #define NEED_SPLAT_MAP_1

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7

#elif defined(_T2M_LAYER_COUNT_9)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7
    #define NEED_PAINT_MAP_8

#elif defined(_T2M_LAYER_COUNT_10)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7
    #define NEED_PAINT_MAP_8
    #define NEED_PAINT_MAP_9

#elif defined(_T2M_LAYER_COUNT_11)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7
    #define NEED_PAINT_MAP_8
    #define NEED_PAINT_MAP_9
    #define NEED_PAINT_MAP_10

#elif defined(_T2M_LAYER_COUNT_12)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7
    #define NEED_PAINT_MAP_8
    #define NEED_PAINT_MAP_9
    #define NEED_PAINT_MAP_10
    #define NEED_PAINT_MAP_11

#elif defined(_T2M_LAYER_COUNT_13)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2
    #define NEED_SPLAT_MAP_3

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7
    #define NEED_PAINT_MAP_8
    #define NEED_PAINT_MAP_9
    #define NEED_PAINT_MAP_10
    #define NEED_PAINT_MAP_11
    #define NEED_PAINT_MAP_12

#elif defined(_T2M_LAYER_COUNT_14)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2
    #define NEED_SPLAT_MAP_3

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7
    #define NEED_PAINT_MAP_8
    #define NEED_PAINT_MAP_9
    #define NEED_PAINT_MAP_10
    #define NEED_PAINT_MAP_11
    #define NEED_PAINT_MAP_12
    #define NEED_PAINT_MAP_13

#elif defined(_T2M_LAYER_COUNT_15)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2
    #define NEED_SPLAT_MAP_3

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6
    #define NEED_PAINT_MAP_7
    #define NEED_PAINT_MAP_8
    #define NEED_PAINT_MAP_9
    #define NEED_PAINT_MAP_10
    #define NEED_PAINT_MAP_11
    #define NEED_PAINT_MAP_12
    #define NEED_PAINT_MAP_13
    #define NEED_PAINT_MAP_14

#elif defined(_T2M_LAYER_COUNT_16)

    #define NEED_SPLAT_MAP_1
    #define NEED_SPLAT_MAP_2
    #define NEED_SPLAT_MAP_3

    #define NEED_PAINT_MAP_2
    #define NEED_PAINT_MAP_3
    #define NEED_PAINT_MAP_4
    #define NEED_PAINT_MAP_5
    #define NEED_PAINT_MAP_6 
    #define NEED_PAINT_MAP_7 
    #define NEED_PAINT_MAP_8
    #define NEED_PAINT_MAP_9
    #define NEED_PAINT_MAP_10
    #define NEED_PAINT_MAP_11
    #define NEED_PAINT_MAP_12
    #define NEED_PAINT_MAP_13
    #define NEED_PAINT_MAP_14
    #define NEED_PAINT_MAP_15

#endif


#if defined(_T2M_TEXTURE_SAMPLE_TYPE_ARRAY)
    
    #define T2M_DECALRE_NORMAL(l)               
    #define T2M_DECALRE_MASK(l)                 
    #define T2M_DECLARE_SAMPLER_STATE(s)

    #define T2M_UNPACK_SPLATMAP(uv,index)                 SAMPLE_TEXTURE2D_ARRAY(_T2M_SplatMaps2DArray, sampler_T2M_SplatMaps2DArray, uv, index);
	#define T2M_UNPACK_PAINTMAP(uv,index,sum,splat)	      float4 paintColor##index = (_T2M_Layer_##index##_MapsUsage.x > 0.5 ? SAMPLE_TEXTURE2D_ARRAY(_T2M_DiffuseMaps2DArray, sampler_T2M_DiffuseMaps2DArray, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw, paintMapUsageIndex) : float4(1, 1, 1, 1));  paintMapUsageIndex += _T2M_Layer_##index##_MapsUsage.x > 0.5 ? 1 : 0;  sum += paintColor##index * _T2M_Layer_##index##_ColorTint * splat;
	#define T2M_UNPACK_NORMAL_MAP(index,uv,sum,splat)     sum += TerrainToMeshNormalStrength(UnpackNormal(SAMPLE_TEXTURE2D_ARRAY(_T2M_NormalMaps2DArray, sampler_T2M_NormalMaps2DArray, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw, normalMapUsageIndex)), _T2M_Layer_##index##_NormalScale) * splat;	normalMapUsageIndex += 1;
	#define T2M_UNPACK_MASK(index,uv,sum,splat)           sum += TerrainToMeshRemap(SAMPLE_TEXTURE2D_ARRAY(_T2M_MaskMaps2DArray, sampler_T2M_MaskMaps2DArray, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw, maskMapUsageIndex), _T2M_Layer_##index##_MaskMapRemapMin, _T2M_Layer_##index##_MaskMapRemapMax) * splat; maskMapUsageIndex += 1;

#else

    #define T2M_DECALRE_NORMAL(l)                         TEXTURE2D(_T2M_Layer_##l##_NormalMap); 
    #define T2M_DECALRE_MASK(l)                           TEXTURE2D(_T2M_Layer_##l##_Mask);

    #if defined(TERRAIN_TO_MESH_BUILTIN_SAMPLER)
        #define T2M_DECLARE_SAMPLER_STATE(s)
    #else
        #define T2M_DECLARE_SAMPLER_STATE(s)              SAMPLER(s);
    #endif


    #if defined(TERRAIN_TO_MESH_BUILTIN_SAMPLER)
        #define T2M_UNPACK_SPLATMAP(uv,index)             _T2M_SplatMap_##index.Sample(BUILTIN_SAMPLER_CLAMP, uv);
	    #define T2M_UNPACK_PAINTMAP(uv,index,sum,splat)	  float4 paintColor##index = _T2M_Layer_##index##_Diffuse.Sample(BUILTIN_SAMPLER_REPEAT, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw);	sum += paintColor##index * _T2M_Layer_##index##_ColorTint * splat;
	    #define T2M_UNPACK_NORMAL_MAP(index,uv,sum,splat) sum += TerrainToMeshNormalStrength(UnpackNormal(_T2M_Layer_##index##_NormalMap.Sample(BUILTIN_SAMPLER_REPEAT, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw)), _T2M_Layer_##index##_NormalScale) * splat;
	    #define T2M_UNPACK_MASK(index,uv,sum,splat)       sum += TerrainToMeshRemap(_T2M_Layer_##index##_Mask.Sample(BUILTIN_SAMPLER_REPEAT, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw), _T2M_Layer_##index##_MaskMapRemapMin, _T2M_Layer_##index##_MaskMapRemapMax) * splat;
    #else
        #define T2M_UNPACK_SPLATMAP(uv,index)             SAMPLE_TEXTURE2D(_T2M_SplatMap_##index, sampler_T2M_SplatMap_0, uv);
	    #define T2M_UNPACK_PAINTMAP(uv,index,sum,splat)	  float4 paintColor##index = SAMPLE_TEXTURE2D(_T2M_Layer_##index##_Diffuse, sampler_T2M_Layer_0_Diffuse, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw);	sum += paintColor##index * _T2M_Layer_##index##_ColorTint * splat;
	    #define T2M_UNPACK_NORMAL_MAP(index,uv,sum,splat) sum += TerrainToMeshNormalStrength(UnpackNormal(SAMPLE_TEXTURE2D(_T2M_Layer_##index##_NormalMap, sampler_T2M_Layer_0_Diffuse, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw)), _T2M_Layer_##index##_NormalScale) * splat;
	    #define T2M_UNPACK_MASK(index,uv,sum,splat)       sum += TerrainToMeshRemap(SAMPLE_TEXTURE2D(_T2M_Layer_##index##_Mask, sampler_T2M_Layer_0_Diffuse, uv * _T2M_Layer_##index##_uvScaleOffset.xy + _T2M_Layer_##index##_uvScaleOffset.zw), _T2M_Layer_##index##_MaskMapRemapMin, _T2M_Layer_##index##_MaskMapRemapMax) * splat;
    #endif

#endif


#if defined(TERRAIN_TO_MESH_BUILTIN_SAMPLER)
    #define T2M_UNPACK_HOLESMAP(uv)                       _T2M_HolesMap.Sample(BUILTIN_SAMPLER_CLAMP, uv);
#else
    #define T2M_UNPACK_HOLESMAP(uv)                       SAMPLE_TEXTURE2D(_T2M_HolesMap, sampler_T2M_HolesMap, uv);
#endif


#define T2M_UNPACK_METALLIC_OCCLUSION_SMOOTHNESS(index,sum,splat)   sum += float4(_T2M_Layer_##index##_MetallicOcclusionSmoothness.rgb, lerp(_T2M_Layer_##index##_MetallicOcclusionSmoothness.a, paintColor##index.a, _T2M_Layer_##index##_SmoothnessFromDiffuseAlpha)) * splat;



#endif