// Perfect Culling (C) 2021 Patrick König
//

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public class PerfectCullingClustering
    {
        private const int MeshLimit = 65000;
        
        public class RendererData
        {
            public Renderer Renderer;
            public MeshFilter MeshFilter;
            public int VertexCount;
        }

        public class ClusterData
        {
            public List<RendererData> Renderers;
            public Bounds Bounds;
            public int VertexCount;
        }

        private static ClusterData CreateCluster(Renderer renderer)
        {
            MeshFilter mf = renderer.GetComponent<MeshFilter>();

            if (mf == null || mf.sharedMesh == null)
            {
                return null;
            }
            
            RendererData rd = new RendererData()
            {
                Renderer = renderer,
                MeshFilter = mf,
                VertexCount = mf.sharedMesh.vertexCount
            };

            return new ClusterData()
            {
                Renderers = new List<RendererData>() { rd },
                Bounds = renderer.bounds,
                VertexCount = rd.VertexCount
            };
        }

        public static List<ClusterData> Cluster(List<Renderer> renderers)
        {
            List<ClusterData> clusters = new List<ClusterData>();

            // Fill
            foreach (var rend in renderers)
            {
                ClusterData c = CreateCluster(rend);
                
                if (c == null)
                {
                    continue;
                }
                
                clusters.Add(c);
            }

            Bounds totalBounds = clusters[0].Bounds;

            for (int i = 1; i < clusters.Count; ++i)
            {
                totalBounds.Encapsulate(clusters[i].Bounds);
            }

            bool dirty = true;
            
            while (clusters.Count > 2 && dirty)
            //for (int IT = 0; IT < 16; ++IT)
            {
                dirty = false;
                
                for (int j = 0; j < clusters.Count; ++j)
                {
                    ClusterData a = clusters[j];
                    
                    int bestIndex = -1;
                    Bounds bestBounds = default;

                    // Start with i = 1
                    for (int i = 0; i < clusters.Count; ++i)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        
                        ClusterData b = clusters[i];

                        int mergedVertexCount = a.VertexCount + b.VertexCount;

                        if (mergedVertexCount >= MeshLimit)
                        {
                            continue;
                        }

                        if ((a.Bounds.center - b.Bounds.center).sqrMagnitude > 10f * 10f)
                        {
                            // Too large bounds difference
                            continue;
                        }

                        Bounds aEnlargedBounds = new Bounds(a.Bounds.center, 35f * Vector3.one);

                        if (!aEnlargedBounds.Intersects(b.Bounds))
                        {
                            continue;
                        }

                        bestIndex = i;

                        bestBounds = a.Bounds;
                        bestBounds.Encapsulate(b.Bounds);

                        dirty = true;

                        break;
                    }

                    if (bestIndex == -1)
                    {
                        continue;
                    }

                    ClusterData bestCluster = clusters[bestIndex];

#if false
                    // Swap() and Pop()
                    
                    int posEnd1 = clusters.Count - 2;
                    int posEnd2 = clusters.Count - 1;

                    ClusterData end1 = clusters[posEnd1];
                    ClusterData end2 = clusters[posEnd2];

                    clusters[bestIndex] = end1;
                    clusters[j] = end2;
                    
                    clusters[posEnd1] = null;
                    clusters[posEnd2] = null;
                        
                    clusters.RemoveRange(clusters.Count - 2, 2);
#else
                    var x2 = clusters[j];

                    clusters.RemoveAt(bestIndex);
                    clusters.RemoveAt(j > bestIndex ? j - 1 : j);
#endif

                    clusters.Add(new ClusterData()
                    {
                        Renderers = a.Renderers.Union(bestCluster.Renderers).ToList(),
                        Bounds = bestBounds,
                        VertexCount = a.VertexCount + bestCluster.VertexCount
                    });
                }
            }

            return clusters;
        }
    }
}