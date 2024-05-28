using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using RosSharp.RosBridgeClient.MessageTypes.Rosapi;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using Newtonsoft.Json;
using RosSharp.Urdf;
using CompasXR.Core.Data;
using Google.MiniJSON;


namespace CompasXR.Robots.MqttData
{
    /*
        WARNING: These classes define standard message formats for Compas XR MQTT communication.

        DISCLAIMER: The structure of these classes is tightly coupled with the Compas XR MQTT communication protocol.
        Any modifications to these classes must be synchronized with corresponding changes in the compas_xr/mqtt/messages.py file.
        GitHub Repository: https://github.com/gramaziokohler/compas_xr/blob/messages_module/src/compas_xr/mqtt/messages.py

        USAGE GUIDELINES:
        - These classes set the format for messages exchanged in Compas XR MQTT communication.
        - Avoid making changes to these classes without coordinating updates in the associated messages.py file.
        - Refer to the GitHub repository for the latest information and updates related to the Compas XR MQTT protocol.

        IMPORTANT: Modifications without proper coordination may lead to communication issues between components using this protocol.

        PLEASE NOTE: The information provided here serves as a guide to maintain compatibility and consistency with the Compas XR system.
        For detailed protocol specifications and discussions, refer to the GitHub repository mentioned above.

        CONTRIBUTORS: Ensure that any changes made to these classes are well-documented and discussed within the development team.
    */
   
    /////////////////////////////////////////// Classes for Topic Publishers and Subscribers ///////////////////////////////////////////
    
    [System.Serializable]
    public class CompasXRTopics
    {
        /*
        * CompasXRTopics : Class is used to manage the MQTT topics for Compas XR communication.
        * It is designed to store the publishers and subscribers for the specific project.
        */
        public Publishers publishers { get; set; }
        public Subscribers subscribers { get; set; }

        public CompasXRTopics(string projectName)
        {
            publishers = new Publishers(projectName);
            subscribers = new Subscribers(projectName);
        }
    }

    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class Publishers
    {
        /*
        * Publishers : Class is used to manage the MQTT publishers for Compas XR communication.
        * It is designed to store the specific topics to publish to.
        */
        public string getTrajectoryRequestTopicAA { get; set; }
        public string getTrajectoryRequestTopicAB { get; set; }

        public string approveTrajectoryTopic { get; set; }
        public string approvalCounterRequestTopic { get; set; }

        public string sendTrajectoryTopicAA { get; set; }
        public string sendTrajectoryTopicAB { get; set; }

        public string approvalCounterResultTopic { get; set; }

        public Publishers(string projectName)
        {
            getTrajectoryRequestTopicAA = $"compas_xr/get_trajectory_request/{projectName}/AA";
            getTrajectoryRequestTopicAB = $"compas_xr/get_trajectory_request/{projectName}/AB";

            approvalCounterRequestTopic = $"compas_xr/approval_counter_request/{projectName}";
            approvalCounterResultTopic = $"compas_xr/approval_counter_result/{projectName}";
            approveTrajectoryTopic = $"compas_xr/approve_trajectory/{projectName}";

            sendTrajectoryTopicAA = $"compas_xr/send_trajectory/{projectName}/AA";
            sendTrajectoryTopicAB = $"compas_xr/send_trajectory/{projectName}/AB";
        }

    }

    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class Subscribers
    {
        /*
        * Subscribers : Class is used to manage the MQTT subscribers for Compas XR communication.
        * It is designed to store the specific topics to subscribe to.
        */
        public string getTrajectoryRequestTopicAA { get; set; }
        public string getTrajectoryRequestTopicAB { get; set; }

        public string getTrajectoryResultTopicAA { get; set; }
        public string getTrajectoryResultTopicAB { get; set; }


        public string approveTrajectoryTopic { get; set; }
        public string approvalCounterRequestTopic { get; set; }
        public string approvalCounterResultTopic { get; set; }

        //Constructer for subscribers that takes an input project name
        public Subscribers(string projectName)
        {
            getTrajectoryRequestTopicAA = $"compas_xr/get_trajectory_request/{projectName}/AA";
            getTrajectoryRequestTopicAB = $"compas_xr/get_trajectory_request/{projectName}/AB";

            getTrajectoryResultTopicAA = $"compas_xr/get_trajectory_result/{projectName}/AA";
            getTrajectoryResultTopicAB = $"compas_xr/get_trajectory_result/{projectName}/AB";

            approveTrajectoryTopic = $"compas_xr/approve_trajectory/{projectName}";
            approvalCounterRequestTopic = $"compas_xr/approval_counter_request/{projectName}";
            approvalCounterResultTopic = $"compas_xr/approval_counter_result/{projectName}";
        }
    }

    /////////////////////////////////////////// Classes for Compas XR Services Management ///////////////////////////////////////////////
    
    [System.Serializable]
    public class ServiceManager
    {
        /*
        * ServiceManager : Class is used to manage the services for Compas XR communication.
        * It is designed to store the current service state and manage each individual users
        * connectoin to the trajectory approval services user and approval counters.
        */
        public SimpleCounter UserCount { get; set; }
        public SimpleCounter ApprovalCount { get; set; }
        public bool PrimaryUser { get; set; }
        public string ActiveRobotName { get; set; }
        public List<Dictionary<string, float>> CurrentTrajectory { get; set; } 
        public CurrentService currentService { get; set; }
        public bool TrajectoryRequestTransactionLock { get; set; }
        public GetTrajectoryRequest LastGetTrajectoryRequestMessage { get; set; }
        public GetTrajectoryResult LastGetTrajectoryResultMessage { get; set; }
        public CancellationTokenSource ApprovalTimeOutCancelationToken { get; set; }
        public CancellationTokenSource GetTrajectoryRequestTimeOutCancelationToken { get; set; }
        public bool IsDirtyTrajectory { get; set; }
        public Header IsDirtyGetTrajectoryRequestHeader { get; set; }
        public ServiceManager()
        {
            UserCount = new SimpleCounter();
            ApprovalCount = new SimpleCounter();
            PrimaryUser = false;
            CurrentTrajectory = null;
            currentService = CurrentService.None;
            LastGetTrajectoryRequestMessage = null;
            LastGetTrajectoryResultMessage = null;
            IsDirtyGetTrajectoryRequestHeader = null;
            IsDirtyTrajectory = false;            
            TrajectoryRequestTransactionLock = false;
        }
        public enum CurrentService
        {
            None = 0,
            GetTrajectory = 1,
            ApproveTrajectory = 2,
            ExacuteTrajectory = 3,
        }
    }

    [System.Serializable]
    public class SimpleCounter
    {
        /*
        * SimpleCounter : Class is used to manage the simple counter for Compas XR communication.
        * It is designed to work like a traditional counter and store the current value of the counter
        * and manage the increment and reset operations.
        */
        private int _value;
        private readonly object _lock = new object();
        public SimpleCounter(int start = 0)
        {
            _value = start;
        }

        public int Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
        }

        public int Increment(int num = 1)
        {
            /*
            * Method is used to increment the counter value by the given number.
            */
            lock (_lock)
            {
                _value += num;
                return _value;
            }
        }

        public int Reset()
        {
            lock (_lock)
            {
                /*
                * Method is used to reset the counter to zero.
                */
                _value = 0;
                return _value;
            }
        }
    }
    /////////////////////////////////////////// Classes for Compas XR Custom Messages //////////////////////////////////////////////////
    
    [System.Serializable]
    public class SequenceCounter
    {
        /*
        * SequenceCounter : Class is used to manage the sequence counter for Compas XR Header class.
        * It is designed to work like a traditional counter and store the current value of the counter
        * and manage the increment and update operations.
        * Increments with each message.
        */
        private static readonly int ROLLOVER_THRESHOLD = int.MaxValue;
        private int _value;
        private readonly object _lock = new object();

        public SequenceCounter(int start = 0)
        {
            _value = start;
        }

        public int Increment(int num = 1)
        {
            /*
            * Method is used to increment the counter value by the given number.
            */
            lock (_lock)
            {
                _value += num;
                if (_value > ROLLOVER_THRESHOLD)
                {
                    _value = 1;
                }
                return _value;
            }
        }
        public int UpdateFromMsg(int responseValue)
        {
            /*
            * Method is used to update the counter value based on the received message.
            * This is to allow users to correct their sequence counter on app restarts.
            */
            lock (_lock)
            {
                if(responseValue > _value)
                {
                    _value = responseValue;
                }
                return _value;
            }
        }
    }

    [System.Serializable]
    public class ResponseID
    {
        /*
        * ResponseID : Class is used to manage the response counter for Compas XR Header class.
        * It is designed to work like a traditional counter and store the current value of the counter
        * and manage the increment and update operations.
        * Increments Only with each new request.
        */
        private static readonly int ROLLOVER_THRESHOLD = int.MaxValue;
        private int _value;
        private readonly object _lock = new object();

        public ResponseID(int start = 0)
        {
            _value = start;
        }
        
        public int Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
        }
        public int Increment(int num = 1)
        {
            /*
            * Method is used to increment the counter value by the given number.
            */
            lock (_lock)
            {
                _value += num;
                if (_value > ROLLOVER_THRESHOLD)
                {
                    _value = 1;
                }
                return _value;
            }
        }
        public int UpdateFromMsg(int responseValue)
        {
            /*
            * Method is used to update the counter value based on the received message.
            * This is to allow users to correct their sequence counter on app restarts.
            */
            lock (_lock)
            {
                if(responseValue > _value)
                {
                    _value = responseValue;
                }
                return _value;
            }
        }
    }


    [System.Serializable]
    public class Header
    {
        /*
        * Header : Class is used to manage the header for Compas XR communication.
        * It is designed to store the sequence ID, response ID, device ID, and timestamp for each message.
        */
        private static SequenceCounter _sharedSequenceCounter;
        private static ResponseID _sharedResponseIDCounter;
        public int SequenceID { get; private set; }
        public int ResponseID { get; private set; }
        public string DeviceID { get; private set; }
        public string TimeStamp { get; private set; }
        public Header(bool incrementResponseID = false, int? sequenceID=null, int? responseID=null, string deviceID=null, string timeStamp=null)
        {   
            if(sequenceID.HasValue && responseID.HasValue && deviceID != null && timeStamp != null)
            {    
                SequenceID = sequenceID.Value;
                ResponseID = responseID.Value;
                DeviceID = deviceID;
                TimeStamp = timeStamp;
            }
            else
            {
                SequenceID = EnsureSequenceID();
                ResponseID = EnsureResponseID(incrementResponseID);
                DeviceID = GetDeviceID();
                TimeStamp = GetTimeStamp();
            }
        } 
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the header data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "sequence_id", SequenceID },
                { "response_id", ResponseID },
                { "device_id", DeviceID },
                { "time_stamp", TimeStamp }
            };
        }
        private static int EnsureSequenceID()
        {
            /*
            * Method is used to ensure the Header has a shared instance of the SequenceCounter
            * and that it is incremented and returned.
            */
            if (_sharedSequenceCounter == null)
            {
                _sharedSequenceCounter = new SequenceCounter();
                return _sharedSequenceCounter.Increment();
            }
            else
            {
                return _sharedSequenceCounter.Increment();
            }
        }
        private static int EnsureResponseID(bool incrementResponseID = false)
        {
            /*
            * Method is used to ensure the Header has a shared instance of the ResponseID
            * and that it is incremented and returned.
            */
            if (_sharedResponseIDCounter == null)
            {
                _sharedResponseIDCounter = new ResponseID();
                int responseIDValue = _sharedResponseIDCounter.Value;
                return responseIDValue;
            }
            else
            {
                if (incrementResponseID)
                {
                    _sharedResponseIDCounter.Increment();
                }
                int responseIDValue = _sharedResponseIDCounter.Value;
                return responseIDValue;
            }
        }
        private static void _updateSharedSequenceIDCounter(int sequenceID)
        {
            /*
            * Method is used to update the shared sequence counter based on the received information.
            */
            if (_sharedSequenceCounter != null)
            {
                _sharedSequenceCounter.UpdateFromMsg(sequenceID);
            }
            else
            {
                _sharedSequenceCounter = new SequenceCounter(sequenceID);
            }
        }
        private static void _updateSharedResponseIDCounter(int responseID)
        {
            /*
            * Method is used to update the shared response counter based on the received information.
            */
            if (_sharedResponseIDCounter != null)
            {
                _sharedResponseIDCounter.UpdateFromMsg(responseID);
            }
            else
            {
                _sharedResponseIDCounter = new ResponseID(responseID);
            }
        }
        private static string GetDeviceID()
        {
            /*
            * Method is used to retrieve the device ID for the current device.
            */
            return SystemInfo.deviceUniqueIdentifier;
        }
        private static string GetTimeStamp()
        {
            /*
            * Method is used to retrieve the current timestamp.
            */
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        public static Header Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var sequenceID = Convert.ToInt32(jsonObject["sequence_id"]);
            var responseID = Convert.ToInt32(jsonObject["response_id"]);
            var deviceID = jsonObject["device_id"].ToString();
            var timeStamp = jsonObject["time_stamp"].ToString();

            //Update message counters based on the received information if needed
            _updateSharedSequenceIDCounter(sequenceID);
            _updateSharedResponseIDCounter(responseID);
            return new Header(false, sequenceID, responseID, deviceID, timeStamp);
        }
    }

    [System.Serializable]
    public class GetTrajectoryRequest
    {
        /*
        * GetTrajectoryRequest : Class is used to manage the GetTrajectoryRequest message for Compas XR communication.
        * It is designed to store the element ID, robot name, and header for the message.
        * It is sent to the CAD when a user requests a trajectory.
        */
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string RobotName { get; private set; }
        public string TrajectoryID { get; private set; }
        public GetTrajectoryRequest(string elementID, string robotName, Header header=null)
        {
            Header = header ?? new Header(true);
            ElementID = elementID;
            RobotName = robotName;
            TrajectoryID = $"trajectory_id_{elementID}";
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "robot_name", RobotName },
                { "trajectory_id", TrajectoryID }
            };
        }
        public static GetTrajectoryRequest Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var elementID = jsonObject["element_id"].ToString();
            var robotName = jsonObject["robot_name"].ToString();
            return new GetTrajectoryRequest(elementID, robotName, header);
        }
    }

    [System.Serializable]
    public class GetTrajectoryResult
    {
        /*
        * GetTrajectoryResult : Class is used to manage the GetTrajectoryResult message for Compas XR communication.
        * It is designed to store the element ID, robot name, robot base frame, trajectory, and header for the message.
        * It is sent from the CAD to the user to indicate weather a trajectory has been calculated or not.
        */
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string RobotName { get; private set; }
        public Frame RobotBaseFrame { get; private set; }
        public string TrajectoryID { get; private set; }
        public bool PickAndPlace { get; private set; }
        public int? PickIndex { get; private set; }
        public string? EndEffectorLinkName { get; private set; }
        public List<Dictionary<string, float>> Trajectory { get; private set; } 
        public GetTrajectoryResult(string elementID, string robotName, Frame robotBaseFrame, List<Dictionary<string, float>> trajectory, bool pickAndPlace=false, int? pickIndex=null, string? endEffectorLinkName = null, Header header=null) 
        {
            Header = header ?? new Header();
            ElementID = elementID;
            RobotName = robotName;
            RobotBaseFrame = robotBaseFrame;
            TrajectoryID = $"trajectory_id_{elementID}";
            Trajectory = trajectory;
            PickAndPlace = pickAndPlace;
            PickIndex = pickIndex; //TODO: MAKE THIS A CONFIG?
            EndEffectorLinkName = endEffectorLinkName;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the GetTrajectoryResult data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "robot_name", RobotName },
                { "robot_base_frame", RobotBaseFrame.GetData() },
                { "trajectory_id", TrajectoryID },
                { "trajectory", Trajectory },
                { "pick_and_place", PickAndPlace },
                { "pick_index", PickIndex },
                { "end_effector_link_name", EndEffectorLinkName }
            };
        }
        public static GetTrajectoryResult Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);
            var elementID = jsonObject["element_id"].ToString();
            var robotName = jsonObject["robot_name"].ToString();

            var robotBaseFrameDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(jsonObject["robot_base_frame"]));
            var robotBaseFrameDataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(robotBaseFrameDict["data"]));
            Frame robotBaseFrame = Frame.FromData(robotBaseFrameDataDict);
            if (robotBaseFrame == null)
            {
                throw new Exception("Robot Base Frame is null");
            }
            if (jsonObject["trajectory"] == null)
            {
                throw new Exception("Trajectory is null");
            }
            var trajectory = JsonConvert.DeserializeObject<List<Dictionary<string, float>>>(jsonObject["trajectory"].ToString());

            var pickAndPlace = Convert.ToBoolean(jsonObject["pick_and_place"]);
            if (pickAndPlace)
            {
                var pickIndex = Convert.ToInt32(jsonObject["pick_index"]);
                var endEffectorLinkName = jsonObject["end_effector_link_name"].ToString();
                return new GetTrajectoryResult(elementID, robotName, robotBaseFrame, trajectory, pickAndPlace, pickIndex, endEffectorLinkName, header);
            }
            else
            {
                int? pickIndex = null;
                string? endEffectorLinkName = null;
                return new GetTrajectoryResult(elementID, robotName, robotBaseFrame, trajectory, pickAndPlace=false, pickIndex, endEffectorLinkName, header);
            }
        }
    }

    [System.Serializable]
    public class ApproveTrajectory
    {
        /*
        * ApproveTrajectory : Class is used to manage the ApproveTrajectory message for Compas XR communication.
        * It is designed to store the element ID, robot name, trajectory, and approval status for the message.
        * It is sent from User to User to signify each users approval status of the message.
        * Approval Status: 0 = Rejected, 1 = Approved, 2 = Consensus (All users have approved), & 3 = Cancled (Used for Timeouts).
        */
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string RobotName { get; private set; }
        public string TrajectoryID { get; private set; }
        public List<Dictionary<string, float>> Trajectory { get; private set; }
        public int ApprovalStatus { get; private set; }
        public ApproveTrajectory(string elementID, string robotName, List<Dictionary<string, float>> trajectory, int approvalStatus, Header header=null)
        {
            Header = header ?? new Header();
            ElementID = elementID;
            RobotName = robotName;
            TrajectoryID = $"trajectory_id_{elementID}";
            Trajectory = trajectory;
            ApprovalStatus = approvalStatus;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the ApproveTrajectory data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "robot_name", RobotName },
                { "trajectory_id", TrajectoryID },
                { "trajectory", Trajectory },
                { "approval_status", ApprovalStatus }
            };
        }
        public static ApproveTrajectory Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var elementID = jsonObject["element_id"].ToString();
            var robotName = jsonObject["robot_name"].ToString();
            var approvalStatus = Convert.ToInt16(jsonObject["approval_status"]);
            var trajectory = JsonConvert.DeserializeObject<List<Dictionary<string, float>>>(jsonObject["trajectory"].ToString());
            return new ApproveTrajectory(elementID, robotName, trajectory, approvalStatus, header);
        }
    }

    [System.Serializable]
    public class ApprovalCounterRequest
    {
        /*
        * ApprovalCounterRequest : Class is used to manage the ApprovalCounterRequest message for Compas XR communication.
        * It is designed to store the element ID, trajectory ID, and header for the message.
        * It is sent from the Primary users to all other users to control the amount of approvals needed to proceed.
        */
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }
        public ApprovalCounterRequest(string elementID, Header header=null)
        {
            Header = header ?? new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the ApprovalCounterRequest data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID }
            };
        }
        public static ApprovalCounterRequest Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var elementID = jsonObject["element_id"].ToString();
            return new ApprovalCounterRequest(elementID, header);
        }
    }

    [System.Serializable]
    public class ApprovalCounterResult
    {
        /*
        * ApprovalCounterResult : Class is used to manage the ApprovalCounterResult message for Compas XR communication.
        * It is designed to store the element ID, trajectory ID, and header for the message.
        * It is sent from the users to the Primary user to signify they are required for approval.
        */
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }
        public ApprovalCounterResult(string elementID, Header header=null)
        {
            Header = header ?? new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the ApprovalCounterResult data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID }
            };
        }
        public static ApprovalCounterResult Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var elementID = jsonObject["element_id"].ToString();
            return new ApprovalCounterResult(elementID, header);
        }
    }
    [System.Serializable]
    public class SendTrajectory
    {
        /*
        * SendTrajectory : Class is used to manage the SendTrajectory message for Compas XR communication.
        * It is designed to store the element ID, robot name, trajectory, and header for the message.
        * It is sent from the Primary user to the CAD to signify sending of the Trajectory to the Robot.
        */
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string Robotname { get; private set; }
        public string TrajectoryID { get; private set; }
        public List<Dictionary<string, float>> Trajectory { get; private set; } 
        public SendTrajectory(string elementID, string robotName, List<Dictionary<string, float>> trajectory, Header header=null) 
        {
            Header = header ?? new Header();
            ElementID = elementID;
            Robotname = robotName;
            TrajectoryID = $"trajectory_id_{elementID}";
            Trajectory = trajectory;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method is used to retrieve the SendTrajectory data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "robot_name", Robotname },
                { "trajectory_id", TrajectoryID },
                { "trajectory", Trajectory }
            };
        }
        public static SendTrajectory Parse(string jsonString)
        {
            /*
            * Method is used to parse an instance of the class from a JSON string.
            */
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            var elementID = jsonObject["element_id"].ToString();
            var robotName = jsonObject["robot_name"].ToString();
            var trajectory = JsonConvert.DeserializeObject<List<Dictionary<string, float>>>(jsonObject["trajectory"].ToString());
            return new SendTrajectory(elementID, robotName, trajectory, header);
        }
    }
}


