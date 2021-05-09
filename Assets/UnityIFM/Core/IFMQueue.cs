
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
    using System.Collections.Generic;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    // Used to queue up packets for main thread. Makes sure that Rendering thread is completely disconnected from the UDP Queue.
    public class IFMQueue
    {
        private readonly Queue<IFMPacket> queue = new Queue<IFMPacket>();
        private IFMServer server;
        public void OpenServer(int port = 49983)
        {

#if UNITY_EDITOR
            if (PlayerSettings.runInBackground == false)
            {
                Debug.Log("Suggest setting PlayerSettings runInBackground = true");
            }
#endif
            CloseServer();

            server = new IFMServer(port);
            server.PacketReceivedEvent += OnPacketReceived;
        }

        public void CloseServer()
        {
            if (server != null)
            {
                server.Close();
                server = null;
            }
        }

        void OnPacketReceived(IFMPacket packet)
        {
            lock (queue)
            {
                queue.Enqueue(packet);
            }
        }

        public bool HasWaitingPackets()
        {
            lock (queue)
            {
                return 0 < queue.Count;
            }
        }

        public IFMPacket GetNextPacket()
        {
            lock (queue)
            {
                return queue.Dequeue();
            }
        }

        public void Clear()
        {
            lock(queue)
            {
                queue.Clear();
                queue.TrimExcess();
            }
        }
    }

}