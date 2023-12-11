using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Threading;
using Koenigz.PerfectCulling;

public static class pc_renderer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeMeshData
    {
        public int vertCount;
        public Vector3[] verts;

        public int indCount;
        public int[] indices;

        public int colorCount;
        public Color[] colors;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeRendererTransformation
    {
        public Vector3 boundsCenter;
        public Vector3 boundsSize;
        
        public Matrix4x4 mat4x4;
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

    static IntPtr m_libHandle;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void DelBeginRender(Vector3[] samplingPositions, int samplingPosCount, NativeMeshRenderers[] nativeMeshRenderers, int nativeMeshRenderersCount);
    static DelBeginRender BeginRender;

    private static Thread m_thread;

    public class pc_renderer_settings
    {
        public string NativeLibPath;
        public Vector3[] SamplingPositions;
        public NativeMeshRenderers[] NativeMeshRenderers;
    }
    
    public static void Render(pc_renderer_settings settings)
    {
#if UNITY_EDITOR
        if (m_thread != null)
        {
            Debug.LogError("Bake still in progress");
            
            return;
        }
        
        settings.NativeLibPath = UnityEditor.AssetDatabase.GetAssetPath(PerfectCullingResourcesLocator.Instance.NativeLib);
        
        // We spawn a new thread to keep the application responsive and display progress.
        m_thread = new Thread(ThreadMain);
        
        m_thread.Start(settings);
#endif
    }

    private static void ThreadMain(object obj)
    {
        pc_renderer_settings settings = (pc_renderer_settings) obj;
        
        m_libHandle = LoadLibrary(settings.NativeLibPath);
        
        if (m_libHandle == IntPtr.Zero) 
        {
            Debug.LogError($"Failed to load library, code: {Marshal.GetLastWin32Error()}");

            return;
        }
        
        IntPtr funcaddr = GetProcAddress(m_libHandle, "BeginRender");
        
        BeginRender = Marshal.GetDelegateForFunctionPointer(funcaddr, typeof(DelBeginRender)) as DelBeginRender;
        
        BeginRender(settings.SamplingPositions, settings.SamplingPositions.Length, settings.NativeMeshRenderers, settings.NativeMeshRenderers.Length);
        
        if (!FreeLibrary(m_libHandle))
        {
            Debug.LogError("Failed to free library.");
        }
        
        m_libHandle = IntPtr.Zero;
    }

    public static void JoinThread()
    {
        if (m_thread == null)
        {
            return;
        }
        
        m_thread.Join();

        m_thread = null;
    }
}