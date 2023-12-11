// Perfect Culling (C) 2021 Patrick König
//

using UnityEngine;

namespace Koenigz.PerfectCulling
{
    [CreateAssetMenu(menuName = "Perfect Culling/PerfectCullingSettings")]
    public class PerfectCullingSettings : ScriptableObject
    {
        private static PerfectCullingSettings m_instance;

        public static PerfectCullingSettings Instance => PerfectCullingResourcesLocator.Instance.Settings;
        
        [Tooltip("PerfectCulling provides you with a native rendering library for Windows that can bake occlusion data even faster.")]
        public bool useUnityForRendering = false;

        [Tooltip("Performs a GPU readback and evaluates the image on the CPU. This is slow and you should stay away unless your hardware is very restricted when it comes to Compute Shaders.")]
        public bool useUnityForRenderingCpuCompute = false;
        
        [Tooltip("Native Vulkan Renderer (Experimental)")]
        public bool useNativeVulkanForRendering = false;
        
        [Tooltip("Disabling this option will render transparent objects opaque.")]
        public bool renderTransparency = true;

        [Range(16, 2048)]
        [Tooltip("Resolution for the actual rendering of a single camera perspective (1 out of 6). Increase this if you are experiencing distant object popping. Decrease this if you are memory constrained on your baking system.")]
        public int bakeCameraResolution = 1024;

        [Tooltip("The average sampling speed in milliseconds. Used to estimate bake times.")]
        public float bakeAverageSamplingSpeedMs = 4f;

        [Tooltip("Automatically updates the bake speed for more precise estimates.")]
        public bool autoUpdateBakeAverageSamplingSpeedMs = false;

        // Split this up just in case
        public int bakeCameraResolutionWidth => bakeCameraResolution;
        public int bakeCameraResolutionHeight => bakeCameraResolution;
    }
}