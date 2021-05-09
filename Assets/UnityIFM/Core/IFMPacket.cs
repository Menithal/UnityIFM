

/**  
* 
* UnityIFM - Simplified iFacialMocap Unity Interface
* 
* Copyright 2021 Matti 'Menithal' Lahtinen
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* 
**/

namespace UnityIFM
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class BlendshapeData
    {
        public string Name { get; private set; }
        public float Value { get; private set; }

        public BlendshapeData(string name, string value)
        {
            this.Name = name;
            this.Value = float.Parse(value);
        }

        public BlendshapeData(string[] tuple)
        {
            // Protocol shorthands Left and Right to save on packet size if names are long.
            this.Name = tuple[0].Replace("_L", "Left").Replace("_R", "Right");
            this.Value = float.Parse(tuple[1]);
        }

        public void SetBlendshapeForRenderers(SkinnedMeshRenderer[] renderers)
        {
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                int index = IFMUtility.GetBlendshapeIndex(renderer, this.Name);
                if (index >= 0) renderer.SetBlendShapeWeight(index, this.Value);
            }
        }
    }
    public class IFMTransform
    {
        public Vector3 Position { get; private set; }
        public Vector3 EulerRotation { get; private set; }
        static Vector3[] GetVector3FromPayload(String data)
        {
            string[] payload = data.Split(',');

            int count = payload.Length;

            if (count % 3 != 0)
            {
                throw new Exception("payload does not consist of vector3");
            }

            int vectorCount = count / 3;
            Vector3[] vectors = new Vector3[vectorCount];

            for (int i = 0; i < vectorCount; i++)
            {
                vectors[i] = new Vector3( float.Parse(payload[i*3]), float.Parse(payload[i*3 + 1]), float.Parse(payload[i*3 + 2]));
            }

            return vectors;

        }

        public void SetWithPayload(string vectorList)
        {
            Vector3[] payload = GetVector3FromPayload(vectorList);

            // *Matching Rotate with Unity Forward -Z Reverse up and downs.
            payload[0].y *= -1;
            payload[0].z *= -1;
            EulerRotation = payload[0];


            if (payload.Length > 1)
            {
                Position = payload[1];
            }
        }
    }

    // Based on iFacialMocap Protocol spec https://www.ifacialmocap.com/for-developer/
    // Compiled into a Model object and static Unpacker
    public class IFMPacket
    {

        public List<BlendshapeData> Blendshapes { get; private set; }
        public IFMTransform Head { get; private set; }
        public IFMTransform RightEye { get; private set; }
        public IFMTransform LeftEye { get; private set; }

        public IFMPacket()
        {
            Blendshapes = new List<BlendshapeData>();
            Head = new IFMTransform();
            RightEye = new IFMTransform();
            LeftEye = new IFMTransform();
        }

        public static IFMPacket Parse(String payloadString)
        {
            IFMPacket packet = new IFMPacket();


            string[] payload = payloadString.Split('=');

            if (payload.Length != 2) throw IMFException("Got Invalid Length of IMFPacket");

            string[] blendshapes = payload[0].Split('|');
            string[] transforms = payload[1].Split('|');

            foreach (string blendshapeTuples in blendshapes)
            {
                if (blendshapeTuples.Length > 0)
                {
                    string[] tuple = blendshapeTuples.Split('-');
                    if (tuple.Length == 2)
                    {

                        packet.Blendshapes.Add(new BlendshapeData(tuple));
                    }
                    else
                    {
                        throw IMFException("Got Invalid tuple for Blendshapes");
                    }
                }
            }

            foreach (string transformTuples in transforms)
            {
                if (transformTuples.Length > 0)
                {
                    string[] tuple = transformTuples.Split('#');

                    string bone = tuple[0];

                    switch (bone)
                    {
                        case "head":
                            packet.Head.SetWithPayload(tuple[1]);
                            break;
                        case "leftEye":
                            packet.LeftEye.SetWithPayload(tuple[1]);
                            break;
                        case "rightEye":
                            packet.RightEye.SetWithPayload(tuple[1]);
                            break;
                        default:
                            break;
                    }
                }
            }

            return packet;
        }

        private static Exception IMFException(string v)
        {
            throw new Exception("IMFException: " + v);
        }
    }

}