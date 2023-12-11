// Perfect Culling (C) 2021 Patrick König
//

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Koenigz.PerfectCulling
{
    public static class PerfectCullingEditorUtil
    {
        public static bool TryGetAssetBakeSize(PerfectCullingBakeData bakeData, out float bakeSizeMb)
        {  
            bakeSizeMb = 0f;
            
            if (bakeData == null)
            {
                return false;
            }
            
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(bakeData);
            
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }

            System.IO.FileInfo fi;

            try
            {
                fi = new FileInfo(assetPath);

                if (!fi.Exists)
                {
                    return false;
                }
            }
            catch (System.Exception)
            {
                return false;
            }

            bakeSizeMb = (float) fi.Length * 1e-6f;

            return true;
        }
    }
}

#endif