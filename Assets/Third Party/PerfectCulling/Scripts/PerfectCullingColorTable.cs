// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    [PreferBinarySerialization]
    public class PerfectCullingColorTable : ScriptableObject
    {
        private static PerfectCullingColorTable m_instance;

        public static PerfectCullingColorTable Instance => PerfectCullingResourcesLocator.Instance.ColorTable;
        
        public Color32[] Colors => m_colors;
        
        [SerializeField] private Color32[] m_colors;
        
        [ContextMenu("Generate")]
        void Generate()
        {
            List<Color32> colors = new List<Color32>(PerfectCullingConstants.MaxRenderers);

            for (int g = 0; g <= byte.MaxValue; ++g)
            {
                for (int b = 0; b <= byte.MaxValue; ++b)
                {
                    // We only use two components so we don't need to use a 3D texture.
                    Color32 col = new Color32(0, (byte) g, (byte) b, byte.MaxValue);
                    
                    if (col == PerfectCullingConstants.ClearColor)
                    {
                        continue;
                    }

                    colors.Add(col);
                }
            }
            
            // Randomize order
            int count = colors.Count;  
            
            while (count > 1) 
            {  
                count--;  
                
                int index = UnityEngine.Random.Range(0, count + 1);  
                
                (colors[index], colors[count]) = (colors[count], colors[index]);
            }
            
            m_colors = colors.ToArray();
        }
    }
}