#ifndef TERRAIN_TO_MESH_VARIABLES_CGINC
#define TERRAIN_TO_MESH_VARIABLES_CGINC


#if defined(TERRAIN_TO_MESH_BUILTIN_SAMPLER)
    SamplerState Sampler_Linear_Repeat;
    SamplerState Sampler_Linear_Clamp;

    #define BUILTIN_SAMPLER_REPEAT   Sampler_Linear_Repeat
    #define BUILTIN_SAMPLER_CLAMP    Sampler_Linear_Clamp
#endif


    //Layer Count/////////////////////////////////////////////////////////////////////////////
    int _T2M_Layer_Count;

    //Holes///////////////////////////////////////////////////////////////////////////////////
    #if defined(_ALPHATEST_ON)
        TEXTURE2D(_T2M_HolesMap); 
        T2M_DECLARE_SAMPLER_STATE(sampler_T2M_HolesMap)
    #endif

     
    #if defined(_T2M_TEXTURE_SAMPLE_TYPE_ARRAY)
     
        TEXTURE2D_ARRAY(_T2M_SplatMaps2DArray);     SAMPLER(sampler_T2M_SplatMaps2DArray);
        TEXTURE2D_ARRAY(_T2M_DiffuseMaps2DArray);   SAMPLER(sampler_T2M_DiffuseMaps2DArray);
        TEXTURE2D_ARRAY(_T2M_NormalMaps2DArray);    SAMPLER(sampler_T2M_NormalMaps2DArray);
        TEXTURE2D_ARRAY(_T2M_MaskMaps2DArray);      SAMPLER(sampler_T2M_MaskMaps2DArray);

    #else

        //SamplerState
        T2M_DECLARE_SAMPLER_STATE(sampler_T2M_SplatMap_0)
        T2M_DECLARE_SAMPLER_STATE(sampler_T2M_Layer_0_Diffuse)
        
         
        //Splatmaps///////////////////////////////////////////////////////////////////////////////
        TEXTURE2D(_T2M_SplatMap_0); 

        #if defined(NEED_SPLAT_MAP_1)
            TEXTURE2D(_T2M_SplatMap_1);
        #endif 

        #if defined(NEED_SPLAT_MAP_2)
            TEXTURE2D(_T2M_SplatMap_2);
        #endif

        #if defined(NEED_SPLAT_MAP_3)
            TEXTURE2D(_T2M_SplatMap_3);
        #endif


        //Layers//////////////////////////////////////////////////////////////////////////////////
        TEXTURE2D(_T2M_Layer_0_Diffuse); 
        #if defined(_T2M_LAYER_0_NORMAL)
            T2M_DECALRE_NORMAL(0)
        #endif
        #if defined(_T2M_LAYER_0_MASK) 
            T2M_DECALRE_MASK(0)
        #endif

    
        TEXTURE2D(_T2M_Layer_1_Diffuse); 
        #if defined(_T2M_LAYER_1_NORMAL)
            T2M_DECALRE_NORMAL(1)
        #endif
        #if defined(_T2M_LAYER_1_MASK) 
            T2M_DECALRE_MASK(1)
        #endif

        #ifdef NEED_PAINT_MAP_2
            TEXTURE2D(_T2M_Layer_2_Diffuse); 
            #if defined(_T2M_LAYER_2_NORMAL)
                T2M_DECALRE_NORMAL(2)
            #endif
            #if defined(_T2M_LAYER_2_MASK) 
                T2M_DECALRE_MASK(2)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_3
            TEXTURE2D(_T2M_Layer_3_Diffuse); 
            #if defined(_T2M_LAYER_3_NORMAL)
                T2M_DECALRE_NORMAL(3)
            #endif
            #if defined(_T2M_LAYER_3_MASK) 
                T2M_DECALRE_MASK(3)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_4
            TEXTURE2D(_T2M_Layer_4_Diffuse); 
            #if defined(_T2M_LAYER_4_NORMAL)
                T2M_DECALRE_NORMAL(4)
            #endif
            #if defined(_T2M_LAYER_4_MASK) 
                T2M_DECALRE_MASK(4)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_5
            TEXTURE2D(_T2M_Layer_5_Diffuse); 
            #if defined(_T2M_LAYER_5_NORMAL)
                T2M_DECALRE_NORMAL(5)
            #endif
            #if defined(_T2M_LAYER_5_MASK) 
                T2M_DECALRE_MASK(5)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_6
            TEXTURE2D(_T2M_Layer_6_Diffuse); 
            #if defined(_T2M_LAYER_6_NORMAL)
                T2M_DECALRE_NORMAL(6)
            #endif
            #if defined(_T2M_LAYER_6_MASK) 
                T2M_DECALRE_MASK(6)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_7
            TEXTURE2D(_T2M_Layer_7_Diffuse); 
            #if defined(_T2M_LAYER_7_NORMAL)
                T2M_DECALRE_NORMAL(7)
            #endif
            #if defined(_T2M_LAYER_7_MASK) 
                T2M_DECALRE_MASK(7)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_8
            TEXTURE2D(_T2M_Layer_8_Diffuse); 
            #if defined(_T2M_LAYER_8_NORMAL)
                T2M_DECALRE_NORMAL(8)
            #endif
            #if defined(_T2M_LAYER_8_MASK) 
                T2M_DECALRE_MASK(8)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_9
            TEXTURE2D(_T2M_Layer_9_Diffuse); 
            #if defined(_T2M_LAYER_9_NORMAL)
                T2M_DECALRE_NORMAL(9)
            #endif
            #if defined(_T2M_LAYER_9_MASK) 
                T2M_DECALRE_MASK(9)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_10
            TEXTURE2D(_T2M_Layer_10_Diffuse); 
            #if defined(_T2M_LAYER_10_NORMAL)
                T2M_DECALRE_NORMAL(10)
            #endif
            #if defined(_T2M_LAYER_10_MASK) 
                T2M_DECALRE_MASK(10)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_11
            TEXTURE2D(_T2M_Layer_11_Diffuse); 
            #if defined(_T2M_LAYER_11_NORMAL)
                T2M_DECALRE_NORMAL(11)
            #endif
            #if defined(_T2M_LAYER_11_MASK) 
                T2M_DECALRE_MASK(11)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_12
            TEXTURE2D(_T2M_Layer_12_Diffuse); 
            #if defined(_T2M_LAYER_12_NORMAL)
                T2M_DECALRE_NORMAL(12)
            #endif
            #if defined(_T2M_LAYER_12_MASK) 
                T2M_DECALRE_MASK(12)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_13
            TEXTURE2D(_T2M_Layer_13_Diffuse); 
            #if defined(_T2M_LAYER_13_NORMAL)
                T2M_DECALRE_NORMAL(13)
            #endif
            #if defined(_T2M_LAYER_13_MASK) 
                T2M_DECALRE_MASK(13)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_14
            TEXTURE2D(_T2M_Layer_14_Diffuse); 
            #if defined(_T2M_LAYER_14_NORMAL)
                T2M_DECALRE_NORMAL(14)
            #endif
            #if defined(_T2M_LAYER_14_MASK) 
                T2M_DECALRE_MASK(14)
            #endif
        #endif

        #ifdef NEED_PAINT_MAP_15
            TEXTURE2D(_T2M_Layer_15_Diffuse); 
            #if defined(_T2M_LAYER_15_NORMAL)
                T2M_DECALRE_NORMAL(15)
            #endif
            #if defined(_T2M_LAYER_15_MASK) 
                T2M_DECALRE_MASK(15)
            #endif
        #endif

    #endif


#endif  //TERRAIN_TO_MESH_VARIABLES_CGINC