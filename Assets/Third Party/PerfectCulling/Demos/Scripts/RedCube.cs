using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling.Demos
{
    public class RedCube : MonoBehaviour
    {
        private void OnEnable()
        {
            // Camera.current can be null because OnEnable is also called in different situations and maybe OnBecameVisible and OnBecameInvisible are an option as well though.
            Debug.Log($"I'm the RedCube script and I was just enabled! Camera: {(Camera.current == null ? "null" : Camera.current.name)}", gameObject);
        }

        private void OnDisable()
        {
            // Camera.current can be null because OnEnable is also called in different situations and maybe OnBecameVisible and OnBecameInvisible are an option as well though.
            Debug.Log($"I'm the RedCube script and I was just disabled! Camera: {(Camera.current == null ? "null" : Camera.current.name)}", gameObject);
        }
    }
}