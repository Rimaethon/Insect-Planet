// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public static class PerfectCullingConstants
    {
        /// <summary>
        /// The maximum amount of renderers supported.
        /// 
        /// NOTICE:
        /// Changing this value might not be sufficient to support additional renderers. In many places ushort is used to save memory.
        /// Furthermore you should rather consider using overlapping volumes and/or reducing the number of individual renderers!
        /// </summary>
        public const int MaxRenderers = ushort.MaxValue;
        
        /// <summary>
        /// How many positions are sampled before they are read back from the GPU.
        ///
        /// WARNING:
        /// Increasing this number can speed up the baking process but could result in the GPU and/or CPU to run out of memory.
        /// On the other side decreasing this number could help if you are already hitting memory limits.
        ///
        /// This is ignored when using the native renderer.
        /// 
        /// </summary>
        public const int SampleBatchCount = 2048;

        /// <summary>
        /// The baking camera only renders this layers.
        /// However any renderer that is not supposed to be in the snapshot is automatically disabled as well.
        /// Concluding that this should "just work" and doesn't need adjustments.
        /// </summary>
        public const int CamBakeLayer = 30;

        /// <summary>
        /// Allows to set Renderers to only shadow casting instead of disabling them completely.
        /// </summary>
        public const PerfectCullingRenderToggleMode ToggleRenderMode = PerfectCullingRenderToggleMode.ToggleForceRenderingOff;

        /// <summary>
        /// Sanity checks the data. Comes with a bit of overhead but you can be sure the data makes sense.
        /// </summary>
        public const bool SafetyChecks = true;
        
        /// <summary>
        /// Supported Renderer types
        /// </summary>
        public static readonly HashSet<System.Type> SupportedRendererTypes = new HashSet<System.Type>()
        {
            typeof(MeshRenderer),
            typeof(SkinnedMeshRenderer)
        };

        /// <summary>
        /// Internally used. Don't change.
        /// </summary>
        public static Color ClearColor = Color.black;

        /// <summary>
        /// Scene reload is necessary for correct function but can be useful to disable it for debugging purposes.
        /// </summary>
        public static bool AllowSceneReload = true;
        
        /// <summary>
        /// Path for multi scene temp scene
        /// </summary>
        public static readonly string MultiSceneTempPath = @"Assets/PerfectCulling_Temp.unity";

        /// <summary>
        /// Sub-folder inside Resources folder. Required so we can restrict the amount of assets we load to only Perfect Culling related assets!
        /// </summary>
        public static readonly string ResourcesFolder = "Perfect Culling";

        /// <summary>
        /// Controls opaqueness of volume visualization inside the Editor
        /// </summary>
        public const float VolumeInsideAlpha = 0.05f;

        /// <summary>
        /// Limits the search range for finding non-empty cells when EmptyCellCullBehaviour is set to FindClosestNonEmptyCell.
        /// This is done for performance reasons because we evaluate this at run-time and need to scan all three dimensions.
        /// * Range of 3 requires 3 * 3 * 3 = 27 iterations.
        /// * Range of 4 requires 4 * 4 * 4 = 64 iterations.
        /// </summary>
        public const int MaxNonEmptyCellSearchRange = 3;

        /// <summary>
        /// Reduces the overhead in PerfectCullingBitStream but is still being tested thus could be unstable.
        /// 
        /// Will eventually become the default behaviour after additional testing confirmed that it is stable.
        /// </summary>
        public const bool ReadMultipleBitsInBitStream = false;
    }
}