using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Insect_Planet._Scripts.ObjectManagers
{
    public static class Vector3Serializer
    {
        public static byte[] Serialize(Int16[][] int16Arrays)
        {
            List<byte[]> int16DataList = new List<byte[]>();

            foreach (Int16[] int16Array in int16Arrays)
            {
                byte[] xBytes = BitConverter.GetBytes(int16Array[0]);
                byte[] yBytes = BitConverter.GetBytes(int16Array[1]);
                byte[] zBytes = BitConverter.GetBytes(int16Array[2]);

                byte[] vectorData = new byte[6]; // 2 bytes for each x, y, z
                System.Buffer.BlockCopy(xBytes, 0, vectorData, 0, 2);
                System.Buffer.BlockCopy(yBytes, 0, vectorData, 2, 2);
                System.Buffer.BlockCopy(zBytes, 0, vectorData, 4, 2);

                int16DataList.Add(vectorData);
            }

            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, int16DataList.ToArray());
                return stream.ToArray();
            }
        }

        private static Int16[][] Deserialize(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {

                byte[][] int16DataArray = (byte[][])formatter.Deserialize(stream);
                List<Int16[]> int16ArraysList = new List<Int16[]>();

                foreach (byte[] int16Data in int16DataArray)
                {
                    Int16 x = BitConverter.ToInt16(int16Data, 0);
                    Int16 y = BitConverter.ToInt16(int16Data, 2);
                    Int16 z = BitConverter.ToInt16(int16Data, 4);

                    Int16[] int16Array = { x, y, z };
                    int16ArraysList.Add(int16Array);
                }

                return int16ArraysList.ToArray();
            }
        }

        public static void SaveToFile(Int16[][] int16Arrays, string fileName = "nocenter", string subFolder = "AsteroidPositions", string extension = "dat")
        {
            // Construct the full file path
            string filePath = Path.Combine(Application.streamingAssetsPath, subFolder, $"{fileName}.{extension}");

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            byte[] serializedData = Serialize(int16Arrays);
            File.WriteAllBytes(filePath, serializedData);
        }

        public static Int16[][] LoadFromFile(string fileName, string subFolder = "AsteroidPositions", string extension = "dat")
        {
            // Construct the full file path
            string filePath = Path.Combine(Application.streamingAssetsPath, subFolder, $"{fileName}.{extension}");

            byte[] data = File.ReadAllBytes(filePath);
            return Deserialize(data);
        }


    }
}
