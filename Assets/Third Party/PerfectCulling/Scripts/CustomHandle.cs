// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CustomHandle
{
    public interface IResizableByHandle
    {
        Vector3 HandleSized { get; set; }
    }
    
    public class ActualHandle<T, U> where T : UnityEngine.Behaviour, IResizableByHandle
                                    where U : struct
    {
#if UNITY_EDITOR
        public void DrawHandle(T zone)
        {
            Matrix4x4 oldMatrix = Handles.matrix;
            
            Matrix4x4 mat = Matrix4x4.TRS(zone.transform.position, zone.transform.rotation, Vector3.one);

            Handles.matrix = mat;
            
            DrawHandleForDirection(zone, mat, Vector3.up);
            DrawHandleForDirection(zone, mat, -Vector3.up);

            DrawHandleForDirection(zone, mat, Vector3.right);
            DrawHandleForDirection(zone, mat, -Vector3.right);
        
            DrawHandleForDirection(zone, mat, Vector3.forward);
            DrawHandleForDirection(zone, mat, -Vector3.forward);

            Handles.matrix = oldMatrix;
        }
        
        void DrawHandleForDirection(T origin, Matrix4x4 matrix4X4, Vector3 axis)
        {
            Vector3 oldSize = origin.HandleSized;

            Vector3 snap = Vector3.one * 0.5f;
            
            Vector3 GetAxis(Vector3 value) => Vector3.Scale(value, axis);
            
            Transform t = origin.transform;

            Vector3 basePos = (0.5f * Vector3.Scale(origin.HandleSized, axis));
        
            float size = UnityEditor.HandleUtility.GetHandleSize(basePos) * 0.1f;

            UnityEditor.EditorGUI.BeginChangeCheck();
            var fmh_56_73_638344818987494120 = Quaternion.identity; Vector3 hPos3 = UnityEditor.Handles.FreeMoveHandle(basePos,Quaternion.identity, size, snap, UnityEditor.Handles.DotHandleCap);
        
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                string undoName = "Size changed";
                
                UnityEditor.Undo.RecordObject(origin, undoName);
                UnityEditor.Undo.RecordObject(origin.transform, undoName);

                if (axis.x < 0 || axis.y < 0 || axis.z < 0)
                {
                    Vector3 tmp = (GetAxis(basePos) - GetAxis(hPos3));

                    tmp.x = GetValue(tmp.x);
                    tmp.y = GetValue(tmp.y);
                    tmp.z = GetValue(tmp.z);

                    origin.HandleSized = origin.HandleSized - tmp;
                    
                    if (GetAxis(origin.HandleSized).magnitude <= 1)
                    {
                        origin.HandleSized = oldSize;
                    }
                    else
                    {
                        t.Translate(0.5f * tmp);
                    }
                }
                else
                {
                    Vector3 tmp = (GetAxis(hPos3) - GetAxis(basePos));
                   
                    tmp.x = GetValue(tmp.x);
                    tmp.y = GetValue(tmp.y);
                    tmp.z = GetValue(tmp.z);
                    
                    origin.HandleSized = origin.HandleSized + tmp;

                    if (GetAxis(origin.HandleSized).magnitude <= 1)
                    {
                        origin.HandleSized = oldSize;
                    }
                    else
                    {
                        t.Translate(0.5f * tmp);
                    }
                }                
            }
        }

        float GetValue(float x)
        {
            if (typeof(U) == typeof(float))
            {
                return x;
            }
            
            if (typeof(U) == typeof(int))
            {
                return (int) x;
            }

            throw new System.NotImplementedException();
        }
#endif
    }
}