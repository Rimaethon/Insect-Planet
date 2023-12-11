// Perfect Culling (C) 2021 Patrick König
//

namespace Koenigz.PerfectCulling
{
    public enum PerfectCullingRenderToggleMode
    {
        /// <summary>
        /// Toggles the Renderer component itself. This got some overhead and thus is the least efficient option.
        /// </summary>
        ToggleRendererComponent,
        
        /// <summary>
        /// This tells the Renderer to only render shadows. Likely just culls the Renderer on lower end hardware. This option avoids shadow popping.
        /// </summary>
        ToggleShadowcastMode,
        
        /// <summary>
        /// Most efficient way of disabling a renderer completely as it just tells Unity to skip over it. Only available in Unity 2019 or newer.
        /// </summary>
        ToggleForceRenderingOff
    }
}