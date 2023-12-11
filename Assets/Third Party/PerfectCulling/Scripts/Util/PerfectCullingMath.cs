// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    /// <summary>
    /// Frequently used math operations.
    /// </summary>
    public static class PerfectCullingMath
    {
        /// <summary>
        /// Flattens XYZ index. This will overflow for a large number of cells due to the cellCount.xyz float components!
        /// </summary>
        public static int FlattenXYZ(int x, int y, int z, Vector3 cellCount)
        {
            return Mathf.CeilToInt((z * cellCount.x * cellCount.y) + (y * cellCount.x) + x);
        }
        
        /// <summary>
        /// Flattens XYZ index. This uses double internally and will not overflow but will be slower. Only intended for baking!
        /// </summary>
        public static int FlattenXYZDouble(int x, int y, int z, Vector3 cellCount)
        {
            return (int)System.Math.Ceiling((z * (double)cellCount.x * (double)cellCount.y) + (y * (double)cellCount.x) + x);
        }

        public static void UnflattenToXYZ(int index, out int x, out int y, out int z, Vector3 cellCount)
        {
            // Flooring by casting to int
            x = index % (int)cellCount.x;
            y = (index / (int)cellCount.x) % (int)cellCount.y;
            z = index / ((int)cellCount.x * (int)cellCount.y);
        }

        public static bool IsXYZInBounds(int x, int y, int z, Vector3 cellCount)
        {
            if (x < 0 || y < 0 || z < 0)
            {
                return false;
            }

            if (x >= cellCount.x || y >= cellCount.y || z >= cellCount.z)
            {
                return false;
            }
            
            return true;
        }

        public static int CalculateNumberOfCells(Vector3 scale, Vector3 cellSize)
        {
            return Mathf.CeilToInt(scale.x / cellSize.x) * Mathf.CeilToInt(scale.y / cellSize.y) * Mathf.CeilToInt(scale.z / cellSize.z);
        }

        public static Vector3 CalculateCellCount(Vector3 scale, Vector3 cellSize)
        {
            Vector3 cellCount = new Vector3(
                Mathf.CeilToInt(scale.x / cellSize.x),
                Mathf.CeilToInt(scale.y / cellSize.y), 
                Mathf.CeilToInt(scale.z / cellSize.z));

            return cellCount;
        }
        
        public static int GetIndexForWorldPos(Vector3 worldPos, Vector3 gridOrigin, Quaternion gridCurrentOrientation, Vector3 gridScale, Quaternion gridBakeOrientation, Vector3 cellCount, Vector3 cellSize, out bool isOutOfBounds)
        {
            // Take into account the baking rotation and the rotation of the volume.
            // We basically rotate around this object as the pivot point by the difference in rotation.
            worldPos = ((Quaternion.Inverse(gridCurrentOrientation) * gridBakeOrientation) * (worldPos - gridOrigin)) + gridOrigin;

            // Apply original bake orientation
            worldPos = Quaternion.Inverse(gridBakeOrientation) * worldPos;
            
            // First remove the origin offset from the position
            Vector3 localPosition = (worldPos - gridOrigin) + 0.5f * gridScale;//transform.lossyScale;
            
            // Convert to grid position
            int unclampedX = (int)(localPosition.x / cellSize.x);
            int unclampedY = (int)(localPosition.y / cellSize.y);
            int unclampedZ = (int)(localPosition.z / cellSize.z);
            
            int clampedX = (int)Mathf.Clamp(unclampedX, 0, cellCount.x - 1);
            int clampedY = (int)Mathf.Clamp(unclampedY, 0, cellCount.y - 1);
            int clampedZ = (int)Mathf.Clamp(unclampedZ, 0, cellCount.z - 1);

            // TODO: We need to use the threshold here because we jump into the next cell too early
            if (   Mathf.Abs(unclampedX - clampedX) > 2 
                || Mathf.Abs(unclampedY - clampedY) > 2
                || Mathf.Abs(unclampedZ - clampedZ) > 2)
            {
                isOutOfBounds = true;
            }
            else
            {
                isOutOfBounds = false;
            }

            return PerfectCullingMath.FlattenXYZ(clampedX, clampedY, clampedZ, cellCount);
        }
    }
}