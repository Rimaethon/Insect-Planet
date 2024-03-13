using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace Rimaethon._Scripts.ObjectManagers
{
    [ExecuteInEditMode]
    public class AsteroidManager : MonoBehaviour
    {
        [SerializeField] private List<Mesh> objectMeshes;
        [SerializeField] private Material objectMaterial;
        [SerializeField] private GameObject meshRendererPrefab;
        private List<ComputeBuffer> argsBuffers;
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        private void Start()
        {
            argsBuffers = new List<ComputeBuffer>();

            LoadInstances();
            Application.targetFrameRate =-1;
        }


        private ComputeBuffer GetArgsBuffer(uint count, Mesh mesh)
        {
            args[0] = mesh.GetIndexCount(0);
            args[1] = count;
            args[2] = mesh.GetIndexStart(0);
            args[3] = mesh.GetBaseVertex(0);

            ComputeBuffer buffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            buffer.SetData(args);
            return buffer;
        }



        private void LoadInstances()
        {

            string subFolderPath = Path.Combine(Application.streamingAssetsPath, "AsteroidPositions");
            string[] fileNames = Directory.GetFiles(subFolderPath, "*.dat");
            float rotationX =0;

            foreach (var fileName in fileNames)
            {
                Vector3 position = ParseCoordinates(Path.GetFileNameWithoutExtension(Path.GetFileName(fileName)));
                float rotationY = ParseCoordinates(Path.GetFileNameWithoutExtension(Path.GetFileName(fileName))).w;

                GameObject meshRenderer= Instantiate(meshRendererPrefab, position,quaternion.identity);
                if (rotationY>=180)
                {
                    rotationX = math.lerp(0, 30, (rotationY - 180) / 180);
                }
                else
                {
                    rotationX = math.lerp(30, -30, (rotationY) / 180);
                }

                //It sets a x rotation between -30 and 30 degrees, depending on the y rotation which makes the box  look at to the planet
                 meshRenderer.transform.Rotate(-rotationX,360f-rotationY,0);

                InstancedMeshRenderer instancedMeshRenderer = meshRenderer.GetComponent<InstancedMeshRenderer>();
                instancedMeshRenderer.fileName=Path.GetFileNameWithoutExtension(Path.GetFileName(fileName));

            }
        }


        private Vector4 ParseCoordinates(string coordinates)
        {
            string[] values = coordinates.Split(',');

            if (values.Length != 4) return Vector4.zero;
            float y=0, z=0, w=0;

            if (float.TryParse(values[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(values[1], NumberStyles.Float, CultureInfo.InvariantCulture, out y) &&
                float.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out z))
                float.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out w);
            {
                return new Vector4(x, y, z,w);

            }
        }



    }
}
