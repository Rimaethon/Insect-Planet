// Perfect Culling (C) 2021 Patrick König
//

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Koenigz.PerfectCulling
{
    public abstract class PerfectCullingBakeData : ScriptableObject
    {
#pragma warning disable 414
        [SerializeField] public int bakeDataVersion = 1;

        [SerializeField] public bool bakeCompleted = false;

        [SerializeField] public int bakeHash = 0;
        
        [SerializeField] public string strBakeDate;
        [SerializeField] public long bakeDurationMilliseconds;
#pragma warning restore 414
        
        public virtual void SetRawData(int index, ushort[] indices, bool validateData = true) => throw new System.NotImplementedException();

        public virtual void SampleAtIndex(int index, List<ushort> indices) => throw new System.NotImplementedException();
        public virtual int SearchIndexForClosestNonEmptyCell(int index) => throw new System.NotImplementedException();

        public virtual void PrepareForBake(PerfectCullingBakingBehaviour bakingBehaviour) => throw new System.NotImplementedException();
        
        public virtual void CompleteBake() => throw new System.NotImplementedException();
        
        public virtual void DrawInspectorGUI() => throw new System.NotImplementedException();
    }
}