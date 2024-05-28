using System;
using System.Threading;
using RosSharp.RosBridgeClient.Protocols;
using UnityEngine;
using RosSharp.RosBridgeClient;
using UnityEngine.UI;
using CompasXR.UI;

namespace CompasXR.Robots
{
    /*
    * CompasXR.Robots : Is the namespace for all Classes that
    * controll the primary functionalities releated to the use of robots in the CompasXR Application.
    * Functionalities, such as robot communication, robot visualization, and robot interaction.
    */

    public class RosConnectionManager : MonoBehaviour
    {
        /*
        * RosConnectionManager : Class is used to manage the connection to the ROSBridge Server and has 2 primary functions.
        * 1. To establish the connection to the ROSBridge Server and manage the connection status.
        * 2. To manage the global event listeners and throughout interaction and infomtion change.
        */
        private UIFunctionalities uiFunctionalities;
        public int SecondsTimeout = 10;
        public RosSocket RosSocket { get; private set; }
        public RosSocket.SerializerEnum Serializer;
        public Protocol protocol;
        public string RosBridgeServerUrl = "ws://192.168.0.1:9090";
        public ManualResetEvent IsConnected { get; private set; }
        public bool IsConnectedToRos { get { return IsConnected.WaitOne(0); } }

        //////////////////////////// Monobehaviour Methods //////////////////////////////
        public virtual void Awake()
        {
            IsConnected = new ManualResetEvent(false);
            
            //Find UIFunctionalities Script
            uiFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();

        }
        private void OnApplicationQuit()
        {
            if(RosSocket != null && IsConnectedToRos)
            {
                RosSocket.Close();
            }
        }

        //////////////////////////// Connection Methods //////////////////////////////
        public void ConnectAndWait()
        {
            RosSocket = ConnectToRos(protocol, RosBridgeServerUrl, OnConnected, OnClosed, Serializer);

            if (!IsConnected.WaitOne(SecondsTimeout * 1000))
                Debug.LogWarning("Failed to connect to RosBridge at: " + RosBridgeServerUrl);
        }
        public static RosSocket ConnectToRos(Protocol protocolType, string serverUrl, EventHandler onConnected = null, EventHandler onClosed = null, RosSocket.SerializerEnum serializer = RosSocket.SerializerEnum.Microsoft)
        {
            IProtocol protocol = ProtocolInitializer.GetProtocol(protocolType, serverUrl);
            protocol.OnConnected += onConnected;
            protocol.OnClosed += onClosed;

            return new RosSocket(protocol, serializer);
        }
        private void OnConnected(object sender, EventArgs e)
        {
            IsConnected.Set();
            Debug.Log("Connected to RosBridge: " + RosBridgeServerUrl);

            //Set UI Object Color to green & connected if the communication toggle is on.
            if (uiFunctionalities.CommunicationToggleObject.GetComponent<Toggle>().isOn)
            {
                uiFunctionalities.UpdateConnectionStatusText(uiFunctionalities.RosConnectionStatusObject, true);
            }
        }
        private void OnClosed(object sender, EventArgs e)
        {
            IsConnected.Reset();
            Debug.Log("Disconnected from RosBridge: " + RosBridgeServerUrl);

            //Set UI Object Color to red and disconnected if the communication toggle is on.
            if (uiFunctionalities.CommunicationToggleObject.GetComponent<Toggle>().isOn)
            {
                uiFunctionalities.UpdateConnectionStatusText(uiFunctionalities.RosConnectionStatusObject, false);
            }
        }
    }
}