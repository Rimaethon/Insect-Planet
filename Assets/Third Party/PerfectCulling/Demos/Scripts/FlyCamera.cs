using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Koenigz.PerfectCulling.Demos
{
    public class FlyCamera : MonoBehaviour
    {
        [Range(30, 150)]
        [SerializeField] float MouseSensitivity = 90;
        
        float m_rotationX = 0.0f;
        float m_rotationY = 0.0f;

        void LateUpdate()
        {
            float dt = Time.deltaTime;
            
            float speed = 15f;
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed *= 2f;
            }

            transform.localPosition += transform.forward * Input.GetAxis("Vertical") * speed * dt +
                                       transform.right * Input.GetAxis("Horizontal") * speed * dt;

            m_rotationX += Input.GetAxis("Mouse X") * MouseSensitivity * dt;
            m_rotationY += Input.GetAxis("Mouse Y") * MouseSensitivity * dt;

            m_rotationY = Mathf.Clamp(m_rotationY, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(m_rotationX, Vector3.up) *
                                      Quaternion.AngleAxis(m_rotationY, Vector3.left);
        }
    }
}