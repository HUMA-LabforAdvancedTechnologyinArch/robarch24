using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Threading;
using CompasXR.Core;
using CompasXR.UI;
using CompasXR.Robots.MqttData;
using CompasXR.Core.Extentions;

namespace CompasXR.Robots
{
    /*
    * CompasXR.Robots : Is the namespace for all Classes that
    * controll the primary functionalities releated to the use of robots in the CompasXR Application.
    * Functionalities, such as robot communication, robot visualization, and robot interaction.
    */
    public class MqttTrajectoryManager : M2MqttUnityClient
    {
        /*
        * MqttTrajectoryManager : Class is used to manage the MQTT connection and configuration settings.
        * Additionally it is designed to handle the MQTT message events, and allow users to subscribe custom topics
        * The MQTTTrajectory manager manages all responses and conditions based on received messges for trajectory visualization.
        */
        [Header("MQTT Settings")]
        [Tooltip("Set the topic to publish")]
        public string controllerName = "MQTT Trajectory Controller";
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
        private List<string> eventMessages = new List<string>();
        public CompasXRTopics compasXRTopics;
        public ServiceManager serviceManager = new ServiceManager();
        public UIFunctionalities UIFunctionalities;
        public DatabaseManager databaseManager;
        public TrajectoryVisualizer trajectoryVisualizer;

        //////////////////////////////////////////// Monobehaviour Methods ////////////////////////////////////////////
        protected override void Start()
        {
            base.Start();
            OnStartorRestartInitilization();
        }
        protected override void Update()
        {
            base.Update();
        }
        public void OnDestroy()
        {
            UnsubscribeFromCompasXRTopics();
            RemoveConnectionEventListners();
            Disconnect();
        }

        //////////////////////////////////////////// General Methods ////////////////////////////////////////////
        public void OnStartorRestartInitilization(bool Restart = false)
        {
            /*
            * Method is used to initialize the MQTT connection, find dependencies, and add listners
            * for subscriptions on connected.
            */
            if (!Restart)
            {
                UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
                databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
                trajectoryVisualizer = GameObject.Find("TrajectoryVisualizer").GetComponent<TrajectoryVisualizer>();
            }
            Connect();
            AddConnectionEventListners();
        }

        //////////////////////////////////////////// Connection Managers ////////////////////////////////////////
        protected override void OnConnected()
        {
            /*
            * Method is used to signal that the MQTT connection has been established.
            */
            base.OnConnected();
            Debug.Log($"MQTT: Connected to broker: {brokerAddress} on Port: {brokerPort}.");
            if (UIFunctionalities.CommunicationToggleObject.GetComponent<Toggle>().isOn)
            {
                UserInterface.SetUIObjectColor(UIFunctionalities.MqttConnectButtonObject, Color.green);
                UIFunctionalities.UpdateConnectionStatusText(UIFunctionalities.MqttConnectionStatusObject, true);
            }
        }
        protected override void OnDisconnected()
        {
            /*
            * Method is used to signal that the MQTT connection has been disconnected.
            */
            base.OnDisconnected();
            Debug.Log("MQTT: DISCONNECTED.");
            if (UIFunctionalities.CommunicationToggleObject.GetComponent<Toggle>().isOn)
            {
                UIFunctionalities.UpdateConnectionStatusText(UIFunctionalities.MqttConnectionStatusObject, false);
            }
        }
        protected override void OnConnectionLost()
        {
            /*
            * Method is used to signal that the MQTT connection has been lost.
            */
            base.OnConnectionLost();
            string message = "WARNING: MQTT connection has been lost. Please check your internet connection and restart the application.";
            UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref  UIFunctionalities.MQTTConnectionLostMessageObject, "MQTTConnectionLostMessage", UIFunctionalities.MessagesParent, message, "OnConnectionLost: MQTT Connection Lost");
            Debug.Log("MQTT: CONNECTION LOST");
        }
        public async void DisconnectandReconnectAsyncRoutine()
        {
            /*
            * Method is used to disconnect from the MQTT broker and reconnect after a short delay.
            */
            Disconnect();
            StartCoroutine(ReconnectAfterDisconect());
        }
        private IEnumerator ReconnectAfterDisconect()
        {
            /*
            * Method is used to reconnect to the MQTT broker after a short delay.
            */
            yield return new WaitUntil(() => !mqttClientConnected);
            yield return new WaitForSeconds(0.5f);
            OnStartorRestartInitilization(true);
        }

        //////////////////////////////////////////// Topic Managers /////////////////////////////////////////////
        public void SetCompasXRTopics(object source, ApplicationSettingsEventArgs e)
        {
            /*
            * Method is used to set the custom Compas XR Topics based on the Application Settings.
            * format: compas_xr/project_name/message_name
            */
            compasXRTopics = new CompasXRTopics(e.Settings.project_name);
        }
        private void SubscribeToTopic(string topicToSubscribe)
        {
            /*
            * Method is used to subscribe to a custom topic.
            */
            if (!string.IsNullOrEmpty(topicToSubscribe) && client != null)
            {
                Debug.Log("MQTT: SubscribeToTopic: Subscribing to topic: " + topicToSubscribe);
                client.Subscribe(new string[] { topicToSubscribe }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            else
            {
                Debug.LogError("MQTT: Topic to subscribe is empty or client is null.");
            }
        }
        private void UnsubscribeFromTopic(string topicToUnsubscribe)
        {
            /*
            * Method is used to unsubscribe from a custom topic.
            */
            if (!string.IsNullOrEmpty(topicToUnsubscribe) && client != null)
            {
                client.Unsubscribe(new string[] { topicToUnsubscribe });
                Debug.Log("MQTT: UnsubscribeFromTopic: Unsubscribed from topic: " + topicToUnsubscribe);
            }
            else
            {
                Debug.LogWarning("MQTT: Topic to unsubscribe is empty or client is null.");
            }
        }
        public void PublishToTopic(string publishingTopic,  Dictionary<string, object> message)
        {   
            /*
            * Method is used to publish a message to a custom topic.
            */
            if (client != null && client.IsConnected)
            {
                string messagePublish = JsonConvert.SerializeObject(message);
                client.Publish(publishingTopic, System.Text.Encoding.UTF8.GetBytes(messagePublish), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
            }
            else
            {
                Debug.LogWarning("MQTT: PublishToTopic: Client is null or not connected. Cannot publish message.");
            }
        }

        //TODO: Extended For RobArch2024 ////////////////////////////////////////////////////////////////////////
        public void SubscribeToCompasXRTopics()
        {
            /*
            * Method is used to subscribe to the custom Compas XR Topics.
            */
            Debug.Log("MQTT: SubscribeToCompasXRTopics: Subscribing to Compas XR Topics");
            SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryRequestTopicAA);
            SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryResultTopicAB);
            SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryResultTopicAA);
            SubscribeToTopic(compasXRTopics.subscribers.getTrajectoryRequestTopicAB);
            SubscribeToTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
            SubscribeToTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
        }
        public void UnsubscribeFromCompasXRTopics()
        {
            /*
            * Method is used to unsubscribe from the custom Compas XR Topics.
            */
            UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryRequestTopicAA);
            UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryRequestTopicAB);
            UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryResultTopicAA);
            UnsubscribeFromTopic(compasXRTopics.subscribers.getTrajectoryResultTopicAB);
            UnsubscribeFromTopic(compasXRTopics.subscribers.approveTrajectoryTopic);
            UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterRequestTopic);
        }  

        //////////////////////////////////////////// Message Managers ////////////////////////////////////////////
        protected override void DecodeMessage(string topic, byte[] message)
        {
            /*
            * Method is used to decode the message received from the MQTT broker.
            * The method will decode the message and call the appropriate message handler based on the topic.
            */
            msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("MQTT: DecodeMessage: Received: " + msg + " from topic: " + topic);
            CompasXRIncomingMessageHandler(topic, msg);
            StoreMessage(msg);
        }
        private void CompasXRIncomingMessageHandler(string topic, string message)
        {
            /*
            * Method is used to handle the incoming messages from the MQTT broker based on the topic.
            */
            if (topic == compasXRTopics.subscribers.getTrajectoryRequestTopicAA ||
            topic == compasXRTopics.subscribers.getTrajectoryRequestTopicAB)
            {
                Debug.Log("MQTT: GetTrajectoryRequest Message Handeling");
                GetTrajectoryRequest getTrajectoryRequestmessage = GetTrajectoryRequest.Parse(message);
                GetTrajectoryRequestReceivedMessageHandler(getTrajectoryRequestmessage);
            }
            else if (topic == compasXRTopics.subscribers.getTrajectoryResultTopicAA ||
            topic == compasXRTopics.subscribers.getTrajectoryResultTopicAB)
            {
                Debug.Log("MQTT: GetTrajectoryResult Message Handeling");
                GetTrajectoryResult getTrajectoryResultmessage = GetTrajectoryResult.Parse(message);
                GetTrajectoryResultReceivedMessageHandler(getTrajectoryResultmessage); 
            }
            else if (topic == compasXRTopics.subscribers.approveTrajectoryTopic)
            {
                Debug.Log("MQTT: ApproveTrajectory Message Handeling");
                ApproveTrajectory trajectoryApprovalMessage = ApproveTrajectory.Parse(message);
                ApproveTrajectoryMessageReceivedHandler(trajectoryApprovalMessage);
            }
            else if (topic == compasXRTopics.subscribers.approvalCounterRequestTopic)
            {
                ApprovalCounterRequest approvalCounterRequestMessage = ApprovalCounterRequest.Parse(message);
                ApprovalCounterRequestMessageReceivedHandler(approvalCounterRequestMessage);
            }
            else if (topic == compasXRTopics.subscribers.approvalCounterResultTopic)
            {
                ApprovalCounterResult approvalCounterResultMessage = ApprovalCounterResult.Parse(message);
                ApprovalCounterResultMessageReceivedHandler(approvalCounterResultMessage);
            }
            else
            {
                Debug.LogWarning("MQTT: No message handler for topic: " + topic);
            }

        }
        private void GetTrajectoryRequestReceivedMessageHandler(GetTrajectoryRequest getTrajectoryRequestmessage)
        {
            /*
            * Method is used to handle the GetTrajectoryRequest message received from the MQTT broker.
            */
            serviceManager.LastGetTrajectoryRequestMessage = getTrajectoryRequestmessage;
            serviceManager.GetTrajectoryRequestTimeOutCancelationToken = new CancellationTokenSource();
            _= TrajectoryRequestTimeOut(getTrajectoryRequestmessage.ElementID, 240f, serviceManager.GetTrajectoryRequestTimeOutCancelationToken.Token);

            if (getTrajectoryRequestmessage.Header.DeviceID != SystemInfo.deviceUniqueIdentifier)
            {
                Debug.Log($"MQTT: GetTrajectoryRequest from user {getTrajectoryRequestmessage.Header.DeviceID}");
                serviceManager.TrajectoryRequestTransactionLock = true;
            }
            else
            {
                Debug.Log("MQTT: GetTrajectoryRequest this request came from me");
            }
        }
        private void GetTrajectoryResultReceivedMessageHandler(GetTrajectoryResult getTrajectoryResultmessage)
        {
            //Check if the message is dirty and should be ignored
            if(serviceManager.IsDirtyTrajectory)
            {
                if(serviceManager.IsDirtyGetTrajectoryRequestHeader.ResponseID == getTrajectoryResultmessage.Header.ResponseID &&
                serviceManager.IsDirtyGetTrajectoryRequestHeader.SequenceID + 1 == getTrajectoryResultmessage.Header.SequenceID)
                {
                    Debug.Log("MQTT: GetTrajectoryResult: This Message is dirty & Should be Ignored.");
                    return;
                }
            }

            if(serviceManager.GetTrajectoryRequestTimeOutCancelationToken != null)
            {
                Debug.Log("GetTrajectoryResultReceivedMessageHandler: The request time out should be cancled, because the result was received.");
                serviceManager.GetTrajectoryRequestTimeOutCancelationToken.Cancel();
            }

            serviceManager.LastGetTrajectoryResultMessage = getTrajectoryResultmessage;

            if(serviceManager.LastGetTrajectoryRequestMessage != null)
            {
                //First Check if the message is the same as the last request message and if the trajectory count is greater then zero
                if(getTrajectoryResultmessage.Header.ResponseID != serviceManager.LastGetTrajectoryRequestMessage.Header.ResponseID 
                || getTrajectoryResultmessage.Header.SequenceID != serviceManager.LastGetTrajectoryRequestMessage.Header.SequenceID + 1
                || getTrajectoryResultmessage.ElementID != serviceManager.LastGetTrajectoryRequestMessage.ElementID)
                {
                    if(serviceManager.PrimaryUser)
                    {                    
                        Debug.LogWarning("MQTT: GetTrajectoryResult (PrimaryUser): ResponseID, SequenceID, or ElementID do not match the last GetTrajectoryRequestMessage. No action taken.");

                        string message = "WARNING: Trajectory Response did not match expectations. Returning to Request Service.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryResponseIncorrectWarningMessageObject, "TrajectoryResponseIncorrectWarningMessage", UIFunctionalities.MessagesParent, message, "GetTrajectoryResultReceivedMessageHandler: Message Structure incorrect.");

                        serviceManager.PrimaryUser = false;
                        serviceManager.currentService = ServiceManager.CurrentService.None;
                        UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                        return;
                    }
                    else
                    {
                        serviceManager.TrajectoryRequestTransactionLock = false;
                        if(UIFunctionalities.RobotToggleObject.GetComponent<Toggle>().isOn)
                        {
                            UIFunctionalities.SetRequestUIFromKey(UIFunctionalities.CurrentStep);
                        }

                        Debug.LogWarning("MQTT: GetTrajectoryResult (!PrimaryUser): ResponseID, SequenceID, or ElementID do not match the last GetTrajectoryRequestMessage. Ignoring Message.");
                        return;
                    }
                }
                else
                {    
                    
                    //Check if the count is greater then Zero and start the time out dependant on if I am primary user or not.
                    if(getTrajectoryResultmessage.Trajectory.Count > 0)
                    {
                        serviceManager.ApprovalTimeOutCancelationToken = new CancellationTokenSource();
                        float duration = 120; //DURATION FOR PRIMARY USER WAITING FOR APPROVALS.... NEEDS TO BE ADJUSTED W/ FABRICATION TIME.
                        if(!serviceManager.PrimaryUser)
                        {
                            duration = 240; //DURATION FOR NON PRIMARY USER WAITING FOR CONSENSUS.... NEEDS TO BE ADJUSTED W/ FABRICATION TIME.
                        }
                        _= TrajectoryApprovalTimeout(getTrajectoryResultmessage.ElementID, duration, serviceManager.ApprovalTimeOutCancelationToken.Token);
                    }
                    else
                    {
                        Debug.Log("MQTT: GetTrajectoryResult: Trajectory count is zero. No time out started.");
                    }
                    
                    //If I am not the primary user checks
                    if (!serviceManager.PrimaryUser)
                    {
                        if (getTrajectoryResultmessage.Trajectory.Count > 0)
                        {
                            serviceManager.TrajectoryRequestTransactionLock = false;
                            GameObject robotToConfigure = trajectoryVisualizer.ActiveRobot.FindObject("RobotZero");

                            if(robotToConfigure == null)
                            {
                                Debug.LogError("GetTrajectoryResult (!PrimaryUser): Robot to configure is null. No action taken.");
                                return;
                            }

                            UIFunctionalities.SignalTrajectoryReviewRequest(
                                getTrajectoryResultmessage.ElementID,
                                getTrajectoryResultmessage.RobotName,
                                () => trajectoryVisualizer.VisualizeRobotTrajectoryFromResultMessage(
                                    getTrajectoryResultmessage,
                                    trajectoryVisualizer.URDFLinkNames,
                                    robotToConfigure,
                                    trajectoryVisualizer.ActiveTrajectoryParentObject,
                                    true));

                            serviceManager.CurrentTrajectory = getTrajectoryResultmessage.Trajectory;
                            serviceManager.currentService = ServiceManager.CurrentService.ApproveTrajectory;

                            Debug.Log("GetTrajectoryResult (!PrimaryUser): Trajectory count is greater then zero. I am moving on to trajectory review.");
                        }
                        else
                        {
                            serviceManager.TrajectoryRequestTransactionLock = false;
                            if(UIFunctionalities.RobotToggleObject.GetComponent<Toggle>().isOn)
                            {
                                UIFunctionalities.SetRequestUIFromKey(UIFunctionalities.CurrentStep);
                            }

                            Debug.Log("GetTrajectoryResult (!PrimaryUser): Trajectory count is zero. I am free to request.");
                        }
                    }
                    
                    else
                    {           
                        //I am the primary user
                        if (getTrajectoryResultmessage.Trajectory.Count > 0)
                        {
                            SubscribeToTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                            UIFunctionalities.TrajectoryServicesUIControler(false, false, true, true, false, false);
                            serviceManager.CurrentTrajectory = getTrajectoryResultmessage.Trajectory;
                            serviceManager.currentService = ServiceManager.CurrentService.ApproveTrajectory;

                            //TODO: Extended For RobArch2024
                            GameObject robotToConfigure = trajectoryVisualizer.ActiveRobot.FindObject("RobotZero");

                            trajectoryVisualizer.VisualizeRobotTrajectoryFromResultMessage(getTrajectoryResultmessage, 
                                trajectoryVisualizer.URDFLinkNames, robotToConfigure, trajectoryVisualizer.ActiveTrajectoryParentObject, true);

                            Debug.Log("GetTrajectoryResult (PrimaryUser): Trajectory count is greater then zero. I am moving on to trajectory approval Service.");
                            PublishToTopic(compasXRTopics.publishers.approvalCounterRequestTopic, new ApprovalCounterRequest(UIFunctionalities.CurrentStep).GetData());
                        }
                        //If the trajectory count is zero reset Service Manger and Return to Request Trajectory Service
                        else
                        {
                            Debug.Log("MQTT: GetTrajectoryResult (PrimaryUser): Trajectory count is zero resetting Service Manager and returning to Request Trajectory Service.");
                            serviceManager.PrimaryUser = false;
                            serviceManager.currentService = ServiceManager.CurrentService.None;
                            UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                            string message = "WARNING: The robotic controler replied with a Null trajectory. You will be returned to trajectory request.";
                            UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref UIFunctionalities.TrajectoryNullWarningMessageObject, "TrajectoryNullWarningMessage", UIFunctionalities.MessagesParent, message, "GetTrajectoryResultReceivedMessageHandler: Received trajectory is null");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("MQTT: GetTrajectoryResult LastGetTrajectoryRequestMessage is null. A request must be made before this code works.");
            }
        }
        private void ApproveTrajectoryMessageReceivedHandler(ApproveTrajectory trajectoryApprovalMessage)
        {
            /*
            * Method is used to handle the ApproveTrajectory message received from the MQTT broker.
            * The method will handle the approval status and take appropriate actions based on the status.
            * ApprovalStatus: 0 = Trajectory Rejected, 1 = Trajectory Approved, 2 = Consensus, 3 = Cancelation
            */

            //ApproveTrajectoryMessage ApprovalStatus Trajectory rejected message received
            if (trajectoryApprovalMessage.ApprovalStatus == 0)
            {
                if (serviceManager.PrimaryUser)
                {
                    UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                    serviceManager.PrimaryUser = false;
                }

                if(serviceManager.ApprovalTimeOutCancelationToken != null)
                {
                    Debug.Log("ApproveTrajectoryMessageReceivedHandler: Time Out Cancled from Rejection Message.");
                    serviceManager.ApprovalTimeOutCancelationToken.Cancel();
                }
                
                if(trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                }
                else
                {
                    Debug.LogWarning("ApproveTrajectoryMessageReceivedHandler: ActiveTrajectoryParentObject is null or has no children.");
                }

                if(trajectoryVisualizer.ActiveRobot != null && !trajectoryVisualizer.ActiveRobot.activeSelf)
                {
                    trajectoryVisualizer.ActiveRobot.SetActive(true);
                    UIFunctionalities.SetRobotObjectFromStep(UIFunctionalities.CurrentStep);
                }

                serviceManager.ApprovalCount.Reset();
                serviceManager.UserCount.Reset();
                serviceManager.CurrentTrajectory = null;
                serviceManager.currentService = ServiceManager.CurrentService.None;
                UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
            }
            //ApproveTrajectoryMessage ApprovalStatus Trajectory approved message received
            else if (trajectoryApprovalMessage.ApprovalStatus == 1)
            {
                Debug.Log($"MQTT: ApproveTrajectory User {trajectoryApprovalMessage.Header.DeviceID} approved trajectory {trajectoryApprovalMessage.TrajectoryID}");
                if (serviceManager.PrimaryUser)
                {
                    serviceManager.ApprovalCount.Increment();
                    Debug.Log($"MQTT: Message Handeling Counters UserCount == {serviceManager.UserCount.Value} and ApprovalCount == {serviceManager.ApprovalCount.Value}");
                    if (serviceManager.ApprovalCount.Value == serviceManager.UserCount.Value)
                    {
                        Debug.Log("MQTT: ApprovalCount == UserCount. Moving to Service 3 as Primary User.");
                        UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                        serviceManager.currentService = ServiceManager.CurrentService.ExacuteTrajectory;
                        UIFunctionalities.TrajectoryServicesUIControler(false, false, true, false, true, true);
                    }
                    else
                    {
                        Debug.Log($"MQTT: Message Handeling Counters UserCount == {serviceManager.UserCount.Value} and ApprovalCount == {serviceManager.ApprovalCount.Value}");
                    }
                }
            }
            //ApproveTrajectoryMessage ApprovalStatus Consensus message received
            else if (trajectoryApprovalMessage.ApprovalStatus == 2)
            {
                Debug.Log($"MQTT: ApproveTrajectory Consensus message received for trajectory {trajectoryApprovalMessage.TrajectoryID}");

                if (serviceManager.PrimaryUser)
                {
                    UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                    serviceManager.PrimaryUser = false;
                }

                if(serviceManager.ApprovalTimeOutCancelationToken != null)
                {
                    Debug.Log("ApproveTrajectoryMessageReceivedHandler: Time Out Cancled from Consensus Message.");
                    serviceManager.ApprovalTimeOutCancelationToken.Cancel();
                }

                serviceManager.ApprovalCount.Reset();
                serviceManager.UserCount.Reset();
                serviceManager.CurrentTrajectory = null;
                serviceManager.currentService = ServiceManager.CurrentService.None;
                UIFunctionalities.TrajectoryServicesUIControler(true, false, false, false, false, false);
            }            
            //ApproveTrajectoryMessage ApprovalStatus Cancelation message received
            else if (trajectoryApprovalMessage.ApprovalStatus == 3)
            {
                Debug.Log($"MQTT: ApproveTrajectory Cancelation message received for trajectory {trajectoryApprovalMessage.TrajectoryID}");
                if(serviceManager.ApprovalTimeOutCancelationToken != null)
                {
                    Debug.Log("ApproveTrajectoryMessageReceivedHandler: Canceling Trajectory Approval from Cancelation Message.");
                    serviceManager.ApprovalTimeOutCancelationToken.Cancel();
                }
                if (!serviceManager.PrimaryUser)
                {
                    serviceManager.ApprovalCount.Reset();
                    serviceManager.UserCount.Reset();
                    serviceManager.CurrentTrajectory = null;
                    serviceManager.currentService = ServiceManager.CurrentService.None;
                    if(trajectoryVisualizer.ActiveTrajectoryParentObject != null && trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                    {
                        trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                    }
                    if(trajectoryVisualizer.ActiveRobot != null && !trajectoryVisualizer.ActiveRobot.activeSelf)
                    {
                        trajectoryVisualizer.ActiveRobot.SetActive(true);
                    }

                    if (trajectoryApprovalMessage.Header.DeviceID != SystemInfo.deviceUniqueIdentifier)
                    {
                        string message = "WARNING : The trajectory approval has been canceled by another user. Returning to Request Trajectory Service.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref  UIFunctionalities.TrajectoryCancledMessage, "TrajectoryCancledMessage", UIFunctionalities.MessagesParent, message, "ApproveTrajectoryMessageReceivedHandler: Trajectory Cancled by another user.");
                    }
                    UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
                else
                {
                    Debug.Log("MQTT: ApproveTrajectory Cancelation message received for trajectory, but I am the primary user. No action taken.");
                }
            }
            //ApprovalStatus not recognized.
            else
            {
                Debug.LogWarning("MQTT: Approval Status is not recognized. Approval Status: " + trajectoryApprovalMessage.ApprovalStatus);
            }
        }
        private void ApprovalCounterRequestMessageReceivedHandler(ApprovalCounterRequest approvalCounterRequestMessage)
        {
            /*
            * Method is used to handle the ApprovalCounterRequest message received from the MQTT broker.
            * The method will publish the ApprovalCounterResult message to the MQTT broker.
            */
            Debug.Log($"MQTT: ApprovalCounterRequset Message Received from User {approvalCounterRequestMessage.Header.DeviceID}");
            PublishToTopic(compasXRTopics.publishers.approvalCounterResultTopic, new ApprovalCounterResult(approvalCounterRequestMessage.ElementID).GetData());
        }
        private void ApprovalCounterResultMessageReceivedHandler(ApprovalCounterResult approvalCounterResultMessage)
        {
            /*
            * Method is used to handle the ApprovalCounterResult message received from the MQTT broker.
            * The method will increment the UserCount if the Primary User has approved the trajectory.
            */
            Debug.Log($"MQTT: ApprovalCounterResult Message Received from User{approvalCounterResultMessage.Header.DeviceID} for step {approvalCounterResultMessage.ElementID}");
            if (serviceManager.PrimaryUser)
            {
                serviceManager.UserCount.Increment();
            }
        }
        private void StoreMessage(string eventMsg)
        {
            /*
            * Method is used to store the last 50 messages received from the MQTT broker.
            */
            if (eventMessages.Count > 50) eventMessages.Clear();
            eventMessages.Add(eventMsg);
        }
        async Task TrajectoryApprovalTimeout(string elementID, float timeDurationSeconds, CancellationToken cancellationToken)
        {
            /*
            * Method is used to handle the Trajectory Approval Time Out.
            * The method will wait for the time duration and take appropriate actions based on the time out
            * if it is not cancled before timeout is reached.
            */
            Debug.Log($"MQTT: TrajectoryApprovalTimeout: Started with a duration of {timeDurationSeconds} seconds.");
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeDurationSeconds), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (serviceManager.CurrentTrajectory != null)
                {
                    if (serviceManager.PrimaryUser && serviceManager.currentService.Equals(ServiceManager.CurrentService.ExacuteTrajectory))
                    {
                        Debug.Log("MQTT: TrajectoryApprovalTimeout: Primary User has already moved on to Service 3.");
                        return;
                    }
                    else
                    {
                        Debug.Log("MQTT: TrajectoryApprovalTimeout: Primary User has not moved on to service 3 or Other user reached time out : Services will be reset.");
                        string robot_name = databaseManager.BuildingPlanDataItem.steps[elementID].data.robot_name;
                        PublishToTopic(compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(elementID, robot_name, serviceManager.CurrentTrajectory, 3).GetData());
                        if (serviceManager.PrimaryUser)
                        {
                            UnsubscribeFromTopic(compasXRTopics.subscribers.approvalCounterResultTopic);
                        }

                        serviceManager.PrimaryUser = false;
                        serviceManager.currentService = ServiceManager.CurrentService.None;
                        serviceManager.ApprovalCount.Reset();
                        serviceManager.UserCount.Reset();

                        string message = "WARNING : Trajectory Approval has timed out. Returning to Request Trajectory Service.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref  UIFunctionalities.TrajectoryCancledMessage, "TrajectoryCancledMessage", UIFunctionalities.MessagesParent, message, "TrajectoryApprovalTimeout: Trajectory Approval Cancled by Timeout.");
                        UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                    }
                }
                else
                {
                    Debug.Log("MQTT: TrajectoryApprovalTimeout: Current Trajectory is null and this method should not have been called. Either Cancelation did not happen properly, or there is a null trajectory on timeout.");
                }

            }

            catch (TaskCanceledException)
            {
                Debug.Log("MQTT: TrajectoryApprovalTimeout: Task was cancled before the time out duration meaning everything proved to be successful or was purposfully cancled.");
            }
        }
        async Task TrajectoryRequestTimeOut(string elementID, float timeDurationSeconds, CancellationToken cancellationToken)
        {
            /*
            * Method is used to handle the Trajectory Request Time Out.
            * The method will wait for the time duration and take appropriate actions based on the time out
            * if it is not cancled before timeout is reached.
            */
            Debug.Log($"MQTT: TrajectoryRequestTimeOut: For element {elementID} Started with a duration of {timeDurationSeconds} seconds.");
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(timeDurationSeconds), cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                if (serviceManager.PrimaryUser)
                {
                    serviceManager.PrimaryUser = false;
                    serviceManager.currentService = ServiceManager.CurrentService.None;
                    serviceManager.ApprovalCount.Reset();
                    serviceManager.UserCount.Reset();

                    string message = "WARNING : Current Trajectory Request has timed out. Returning to Request Trajectory Service.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref UIFunctionalities.OnScreenErrorMessagePrefab, ref  UIFunctionalities.TrajectoryRequestTimeoutMessage, "TrajectoryRequestTimeoutMessage", UIFunctionalities.MessagesParent, message, "TrajectoryRequestTimeoutMessage: Trajectory Request Cancled by Timeout.");
                    UIFunctionalities.TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
                else
                {
                    Debug.Log("MQTT: TrajectoryRequestTimeOut: Current Primary User has not received a trajectory result yet transaction lock removed.");
                    serviceManager.TrajectoryRequestTransactionLock = false;
                }

                serviceManager.IsDirtyTrajectory = true;
                serviceManager.IsDirtyGetTrajectoryRequestHeader = serviceManager.LastGetTrajectoryRequestMessage.Header;
            }

            catch (TaskCanceledException)
            {
                Debug.Log("MQTT: TrajectoryRequestTimeOut: Task was cancled before the time out duration");
            }
        }

        //////////////////////////////////////////// Event Handlers ////////////////////////////////////////////
        public void AddConnectionEventListners()
        {
            /*
            * Method is used to add event listners for the MQTT connection options.
            */
            ConnectionSucceeded += SubscribeToCompasXRTopics;
            ConnectionFailed += UIFunctionalities.SignalMQTTConnectionFailed;
        }
        public void RemoveConnectionEventListners()
        {
            /*
            * Method is used to remove event listners for the MQTT connection options.
            */
            ConnectionSucceeded -= SubscribeToCompasXRTopics;
            ConnectionFailed -= UIFunctionalities.SignalMQTTConnectionFailed;
        }

    }
}
