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
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public delegate void PacketReceivedEventHandler(IFMPacket packet);
    public class IFMServer
    {
        #region Delegates
        public event PacketReceivedEventHandler PacketReceivedEvent;
        #endregion

        #region Member variables
        private UdpClient udpClient;
        private Thread thread;
        private readonly int retryTimeout = 2000;
        private readonly int localPort = 49983;
        private readonly int sleepMilliseconds = 5;
        private bool connected;

        // Connection handshake with app as defined in https://www.ifacialmocap.com/for-developer/
        private readonly byte[] handShake = Encoding.ASCII.GetBytes("iFacialMocap_sahuasouryya9218sauhuiayeta91555dy3719".ToCharArray());
        #endregion

        #region Constructors
        public IFMServer(int port = 49983) {
            PacketReceivedEvent += delegate (IFMPacket p) { };
            localPort = port;
            Connect();
        }
        #endregion

        #region Public interfaces
        public void Connect()
        {
            // If Client Exists cLose it.
            if (udpClient != null) Close();
            try
            {
                connected = false;
                //receiver.meshTargetList = controller.GetComponentsInChildren<SkinnedMeshRenderer>();
                udpClient = new UdpClient(localPort);
                udpClient.Client.ReceiveTimeout = 1000;

                thread = new Thread(new ThreadStart(ReceivePool));
                thread.Start();
            } catch(Exception e){
                throw e;
            }
        }

        public void Close()
        {
            if (thread != null) thread.Abort();
            thread = null;
            udpClient.Close();
            udpClient = null;
        }
        #endregion


        /**
         * Gets any recent data from udpClient on port 49983.
         * - If data is received from a remote IP and not previously connected, send handshake confirmation to client
         * - Process data in Packet
         * - When succesfully Parsed add to  queue.
         */
        private void ReceiveCycle()
        {
            try
            {
                IPEndPoint remoteIP = null;
                byte[] data = udpClient.Receive(ref remoteIP);

                // TODO: Allow for multiple inputs?
                if (remoteIP != null)
                {
                    if (!connected)
                    {
                        Debug.Log("IFMServer: Got connection from IFM compatible app Confirming connection");
                        udpClient.Send(handShake, handShake.Length, remoteIP.Address.ToString(), 49983);
                        connected = true;
                    }

                    string message = Encoding.ASCII.GetString(data);
                    IFMPacket packet = IFMPacket.Parse(message);

                    PacketReceivedEvent(packet);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                Debug.Log("IFMServer: IMF Receiver shutting down");
                connected = false;
            }catch(SocketException e)
            {
                if (connected)
                {
                    Debug.LogWarning("IFMServer: SocketException occurred: " + e.Message);
                }
                connected = false;
                // Wait 5 seconds before trying again
                Thread.Sleep(retryTimeout);
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("IFMServer:  Can't create server at port {0} - {1}", localPort, e));
            }
        }

        // Cycles infinitely until closure on a separate thread.
        void ReceivePool()
        {
            while (true)
            {
                ReceiveCycle();
                if (udpClient.Available == 0)
                    Thread.Sleep(sleepMilliseconds);
            }
        }
    }
}