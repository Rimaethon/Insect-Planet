using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using Koenigz.PerfectCulling;

public static class pc_renderer_vulkan
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeRendererSettings
    {
        public int renderResolutionWidth;
        public int renderResolutionHeight;
        
        public int threadCount;

        public float farClipPlane;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMeshData
    {
        public int vertCount;
        public Vector3[] verts;

        public int indCount;
        public int[] indices;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeRendererTransformation
    {
        public Vector3 boundsCenter;
        public Vector3 boundsSize;
        
        public Matrix4x4 mat4x4;

        public Vector4 color;
    };
    
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMeshRenderers
    {
        public NativeMeshData meshData;

        public int transformationCount;
        public NativeRendererTransformation[] transformations;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string libname);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool FreeLibrary(IntPtr hModule);

    private static IntPtr m_libHandle;

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    delegate bool DelFinishCellBake (int cellIndex, int cellSize, IntPtr data);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DelBeginRender(NativeRendererSettings settings, Vector3[] samplingPositions, int samplingPosCount, NativeMeshRenderers[] nativeMeshRenderers, int nativeMeshRenderersCount, DelFinishCellBake finishCellBake);
    
    private static DelBeginRender BeginRender;
    private static DelFinishCellBake m_finishedCellBake;

    private static Thread m_thread;

    private static bool m_running;
    
    public class pc_renderer_settings
    {
        public string NativeLibPath;
        public Vector3[] SamplingPositions;
        public NativeMeshRenderers[] NativeMeshRenderers;

        public PerfectCullingBakerNativeVulkanWin64 baker;
    }
    
    public static void Render(pc_renderer_settings settings)
    {
#if UNITY_EDITOR
        if (m_thread != null)
        {
            Debug.LogError("Bake still in progress");
            
            return;
        }

        m_running = true;
        
        settings.NativeLibPath = UnityEditor.AssetDatabase.GetAssetPath(PerfectCullingResourcesLocator.Instance.NativeVulkanLib);
        
        // We spawn a new thread to keep the application responsive and display progress.
        m_thread = new Thread(ThreadMain);
        
        m_thread.Start(settings);
#endif
    }

    private static pc_renderer_settings settings;

    private static void ThreadMain(object obj)
    {
        settings = (pc_renderer_settings) obj;
        
        m_libHandle = LoadLibrary(settings.NativeLibPath);
        
        if (m_libHandle == IntPtr.Zero) 
        {
            Debug.LogError($"Failed to load library, code: {Marshal.GetLastWin32Error()}");

            return;
        }
        
        IntPtr funcaddr = GetProcAddress(m_libHandle, "BeginRender");
        
        BeginRender = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DelBeginRender)) as DelBeginRender;
        
        m_finishedCellBake = new DelFinishCellBake(OnFinishedCellBake);

        var rendererSettings = new NativeRendererSettings()
        {
            renderResolutionWidth = PerfectCullingSettings.Instance.bakeCameraResolutionWidth,
            renderResolutionHeight = PerfectCullingSettings.Instance.bakeCameraResolutionHeight,
            
            // WARNING: Each thread allocates its very own resources on the GPU. Increasing the number of threads too much can result in running out of memory on the GPU!
            // Plus we are pretty much GPU bound and utilizing even more threads doesn't seem to yield in better performance past some point. That is why it is capped here.
            threadCount = Mathf.Min(System.Environment.ProcessorCount, 8),
            farClipPlane = 1000.0f,
        };
        
        BeginRender(rendererSettings, settings.SamplingPositions, settings.SamplingPositions.Length, settings.NativeMeshRenderers, settings.NativeMeshRenderers.Length, m_finishedCellBake);
        
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        
        if (!FreeLibrary(m_libHandle))
        {
            Debug.LogError("Failed to free library.");
        }
        
        m_libHandle = IntPtr.Zero;
    }

    static bool OnFinishedCellBake(int cellIndex, int cellSize, IntPtr data)
    {
        // This is called from multiple threads!
        
        var handle = settings.baker.GetHandleAt(cellIndex);

        if (cellSize == 0)
        {
            handle.indices = System.Array.Empty<ushort>();
        }
        else
        {
            int[] inputHashes = new int[cellSize];
            {
                Marshal.Copy(data, inputHashes, 0, cellSize);

                handle.indices = new ushort[cellSize];
                for (int i = 0; i < inputHashes.Length; ++i)
                {
                    int q = inputHashes[i];

                    int b = q / (256 * 256);
                    q -= (b * 256 * 256);
                    int g = q / 256;
                    int r = q % 256;

                    // The value returned might actually overflow so we cannot use q directly
                    int index = (b * 256 * 256) + (g * 256) + r; //r + 256 * (g + 256 * b);

                    handle.indices[i] = (ushort)handle.m_hash[index];
                }
            }
            inputHashes = null;

            System.Array.Sort(handle.indices);
        }

        handle.MarkCompleted();

        return m_running;
    }

    public static void JoinThread()
    {
        if (m_thread == null)
        {
            return;
        }
        
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        
        m_running = false;
        
        m_thread.Join();

        m_thread = null;
    }
}
