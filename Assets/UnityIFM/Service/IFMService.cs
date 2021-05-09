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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Events;

    #region Message Handler Bindings
    [Serializable]
    public class UnityEventIFMMessage : UnityEvent<IFMPacket> { }


    [Serializable]
    public class IFMEventHandler
    {
        public UnityEventIFMMessage ifmMessageHandler = new UnityEventIFMMessage();
        public IFMEventHandler() { }
        public IFMEventHandler( UnityAction<IFMPacket> handler)
        {
            ifmMessageHandler.AddListener(handler);
        }
    }
    #endregion

    public delegate void IFMPacketHandler();

    public class IFMService : MonoBehaviour
    {
        public int port = 49983;
        private readonly IFMQueue receiver = new IFMQueue();
        public static IFMService Instance { get; private set; }     // Provide access to Receiver singleton
        [SerializeField]
        private List<IFMEventHandler> handlers = new List<IFMEventHandler>()
        {
            new IFMEventHandler(),
        };
#pragma warning disable IDE0051 // Remove warning for unused private members
        #region Pseudo-Singleton Setup
        void Awake()
        {
            // Pseudo-Singleton, one receiver per Project
            if(Instance == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else if(Instance != this)
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Coroutine setup
        private void OnEnable()
        {
            receiver.OpenServer(port);
            StartCoroutine("ProcessIFMPackets");
        }

        private void OnDisable()
        {
            receiver.CloseServer();
            StopCoroutine("ProcessIFMPackets");
        }
        #endregion

        #region Coroutine
        private IEnumerator ProcessIFMPackets()
        {
            Debug.Log("Monitoring Packet Queue.");
            while (true)
            {
                while (receiver.HasWaitingPackets())
                {
                    IFMPacket packet = receiver.GetNextPacket();
                    foreach( IFMEventHandler handler in handlers)
                    {
                        try
                        {
                            handler.ifmMessageHandler.Invoke(packet);
                        }
                        catch (Exception e) {
                            Debug.LogError("Message Handler not properly set, check IFMManager: " + e.Message);
                        }
                    }

                }
                yield return new WaitForSeconds(0.01f);
            }
        }
        #endregion

#pragma warning restore IDE0051 // Remove unused private members

        public void DefaultHandler(IFMPacket packet)
        {
            Debug.Log(string.Format("Packet Received {0}", packet.ToString()));
        }
    }
}