using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using TMPro;

namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user user record and general database management.
    */

    public class MqttFirebaseConfigManager : M2MqttUnityClient
    {

        /*
        * MqttFirebaseConfigManager : Class is used to manage the MQTT connection and configuration settings.
        * Additionally it is designed to handle the MQTT message events, and allow users to subscribe custom topics
        * for sending FirebaseConfiguration Information.
        */

        [Header("MQTT Settings")]
        [Tooltip("Set the topic to publish")]
        public string nameController = "Controller 1";
        private FirebaseConfigSettings saveFirebaseConfigSettingsScript;
        public GameObject greenScreenPanel;
        private float flashDuration = 1.0f;
        private string m_msg;
        public string msg
        {
            get { return m_msg; }
            set
            {
                if (m_msg == value) return;
                m_msg = value;
                OnMessageArrived?.Invoke(m_msg);
            }
        }
        public event OnMessageArrivedDelegate OnMessageArrived;
        public delegate void OnMessageArrivedDelegate(string newMsg);
        private bool m_isConnected;
        public bool isConnected
        {
            get { return m_isConnected; }
            set
            {
                if (m_isConnected == value) return;
                m_isConnected = value;
                OnConnectionSucceeded?.Invoke(isConnected);
            }
        }
        public event OnConnectionSucceededDelegate OnConnectionSucceeded;
        public delegate void OnConnectionSucceededDelegate(bool isConnected);
        private List<string> eventMessages = new List<string>();
        private string currentTopic = "";

    //////////////////////////// Monobehaviour Methods //////////////////////////////
        protected override void Start()
        {
            base.Start();
            GameObject firebaseManager = GameObject.Find("Firebase_Manager");
            if (firebaseManager != null)
            {
                saveFirebaseConfigSettingsScript = firebaseManager.GetComponent<FirebaseConfigSettings>();
            }
            else
            {
                Debug.LogError("MqttFirebaseConfigManager: Firebase Manager GameObject not found in the scene.");
            }
            Connect();
        }  
        protected override void Update()
        {
            base.Update();
        }
        public void OnDestroy()
        {
            Disconnect();
        }
        public void OnConnectButtonClicked()
        {
            /*
            * OnConnectButtonClicked : Method is used to subscribe to a custom topic 
            * for sending Firebase Configuration Information.
            */

            if (client != null && client.IsConnected)
            {
                string topicToSubscribe = saveFirebaseConfigSettingsScript.topicSubscribeInput.text;
                if (!string.IsNullOrEmpty(topicToSubscribe) && topicToSubscribe != currentTopic)
                {
                    UnsubscribeCurrentTopic();
                    currentTopic = topicToSubscribe;
                    SubscribeToTopic();
                    FlashGreenScreen();
                }
            }
        else
            {
                Debug.LogError("OnConnectButtonClicked: MQTT client is not connected.");
            }
        }

    //////////////////////////// MQTT Connection Methods ////////////////////////////
        private void SubscribeToTopic()
        {
            /*
            * SubscribeToTopic : Method is used to subscribe to a unique user input topic.
            */
            string topicToSubscribe = saveFirebaseConfigSettingsScript.topicSubscribeInput.text;
            if (!string.IsNullOrEmpty(topicToSubscribe))
            {
                client.Subscribe(new string[] { topicToSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                Debug.Log("SubscribeToTopic: Subscribed to topic: " + topicToSubscribe);
            }
            else
            {
                Debug.LogError("SubscribeToTopic: Topic to subscribe is empty.");
            }
        }
        private void UnsubscribeCurrentTopic()
        {
            /*
            * SubscribeToTopic : Method is used to subscribe to a unique user input topic.
            */
            string topicToUnsubscribe = saveFirebaseConfigSettingsScript.topicSubscribeInput.text;
            if (!string.IsNullOrEmpty(topicToUnsubscribe))
            {
                client.Unsubscribe(new string[] { topicToUnsubscribe });
                Debug.Log("UnsubscribeCurrentTopic: Unsubscribed from topic: " + topicToUnsubscribe);
            }
            
        }
        private void FlashGreenScreen()
        {
            /*
            * FlashGreenScreen : Method is used to flash the green screen panel for a short duration.
            */
            greenScreenPanel.SetActive(true);
            Invoke("HideGreenScreen", flashDuration);
        }
        private void HideGreenScreen()
        {
            /*
            * HideGreenScreen : Method is used to hide the green screen panel.
            */
            greenScreenPanel.SetActive(false);
        }
        protected override void DecodeMessage(string topic, byte[] message)
        {
            /*
            * DecodeMessage : Method is used to decode the message received from the MQTT broker.
            */
            msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg + " from topic: " + topic);
            OnMessageArrivedHandler(msg);
            StoreMessage(msg);
        }
        public void OnMessageArrivedHandler(string newMsg)
        {
            /*
            * OnMessageArrivedHandler : Method is used to handle the message received from the MQTT broker.
            */
            Dictionary<string, string> resultDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(newMsg);
            FirebaseManager.Instance.appId = resultDataDict["appId"];
            FirebaseManager.Instance.apiKey = resultDataDict["apiKey"];
            FirebaseManager.Instance.databaseUrl = resultDataDict["databaseUrl"];
            FirebaseManager.Instance.storageBucket = resultDataDict["storageBucket"];
            FirebaseManager.Instance.projectId = resultDataDict["projectId"];
            saveFirebaseConfigSettingsScript.UpdateInputFields();

        }
        private void StoreMessage(string eventMsg)
        {
            /*
            * StoreMessage : Method is used to store up to 50 messages from the MQTT Broker.
            */
            if (eventMessages.Count > 50) eventMessages.Clear();
            eventMessages.Add(eventMsg);
        }
    }
}

