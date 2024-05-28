using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using System.IO;
using UnityEngine.Networking;
using System.Linq;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.AppSettings;
using CompasXR.Database.FirebaseManagment;
using Unity.IO.LowLevel.Unsafe;
using Unity.XR.CoreUtils.Datums;

namespace CompasXR.Core
{
    /*
    * CompasXR.Core : Is the Primary namespace for all Classes that
    * controll the primary functionalities of the CompasXR Application.
    */

    public class BuildingPlanDataDictEventArgs : EventArgs
    {
        /*
        * BuildingPlanDataDictEventArgs : Class inherits from EventArgs &
        * it is used to send the BuildingPlanData Class on events.
        */
        public BuildingPlanData BuildingPlanDataItem { get; set; }
    }

    public class TrackingDataDictEventArgs : EventArgs
    {
        /*
        * TrackingDataDictEventArgs : Class inherits from EventArgs &
        * it is used to send the TrackingDataDict on events.
        */
        public Dictionary<string, Node> QRCodeDataDict { get; set; }
    }

    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
    public class JointsDataDictEventArgs : EventArgs
    {
        /*
        * JointsDataDictEventArgs : Class inherits from EventArgs &
        * it is used to send the JointsDataDict on events.
        */
        public Dictionary<string, Data.Joint> JointsDataDict { get; set; }
    }
    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////


    public class UpdateDataItemsDictEventArgs : EventArgs
    {
        /*
        * UpdateDataItemsDictEventArgs : Class inherits from EventArgs &
        * it is used to send the new Step items on event arguments.
        */
        public Step NewValue { get; set; }
        public string Key { get; set; }
    }
    public class UserInfoDataItemsDictEventArgs : EventArgs
    {
        /*
        * UserInfoDataItemsDictEventArgs : Class inherits from EventArgs &
        * it is used to send the UserInfo Class on events.
        */
        public UserCurrentInfo UserInfo { get; set; }
        public string Key { get; set; }
    }

    public class ApplicationSettingsEventArgs : EventArgs
    {
        /*
        * ApplicationSettingsEventArgs : Class inherits from EventArgs &
        * it is used to send the ApplicationSettings Class on events.
        */
        
        public ApplicationSettings Settings { get; set; }
    }

    public class DatabaseManager : MonoBehaviour
    {
        /*
        * DatabaseManager : Class is used to manage the Firebase Realtime Database connection and configuration settings.
        * Additionally it is designed to handle the database events, and allow users to fetch and push data to the database.
        * The primary goal of the DatabaseManager class is to handle data from the Firebase RealtimeDatabase and Storage.
        */
        
        // Firebase database references
        public DatabaseReference dbReferenceAssembly;
        public DatabaseReference dbReferenceBuildingPlan;
        public DatabaseReference dbReferenceSteps;
        public DatabaseReference dbReferenceLastBuiltIndex;
        public DatabaseReference dbReferenceQRCodes;
        public DatabaseReference dbReferenceUsersCurrentSteps;
        public StorageReference dbRefrenceStorageDirectory;
        public DatabaseReference dbRefrenceProject;

        // Data structures to store nodes and steps
        public Dictionary<string, Node> AssemblyDataDict { get; private set; } = new Dictionary<string, Node>();
        public BuildingPlanData BuildingPlanDataItem { get; private set; } = new BuildingPlanData();
        public Dictionary<string, Node> QRCodeDataDict { get; private set; } = new Dictionary<string, Node>();

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public Dictionary<string, Data.Joint> JointsDataDict { get; private set; } = new Dictionary<string, Data.Joint>();
        DatabaseReference dbReferenceJoints;

        public delegate void JointsDataDictEventHandler(object source, JointsDataDictEventArgs e); 
        public event JointsDataDictEventHandler JointsDataDictReceived;

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

        public Dictionary<string, UserCurrentInfo> UserCurrentStepDict { get; private set; } = new Dictionary<string, UserCurrentInfo>();

        //Data Structure to Store Application Settings
        public ApplicationSettings applicationSettings;

        // Define event delegates and events
        public delegate void StoreDataDictEventHandler(object source, BuildingPlanDataDictEventArgs e); 
        public event StoreDataDictEventHandler DatabaseInitializedDict;

        public delegate void TrackingDataDictEventHandler(object source, TrackingDataDictEventArgs e); 
        public event TrackingDataDictEventHandler TrackingDictReceived;

        public delegate void UpdateDataDictEventHandler(object source, UpdateDataItemsDictEventArgs e); 
        public event UpdateDataDictEventHandler DatabaseUpdate;

        public delegate void StoreApplicationSettings(object source, ApplicationSettingsEventArgs e);
        public event StoreApplicationSettings ApplicationSettingUpdate;
        
        public delegate void UpdateUserInfoEventHandler(object source, UserInfoDataItemsDictEventArgs e);
        public event UpdateUserInfoEventHandler UserInfoUpdate;

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        float StartUpTimeStamp;

        //Other Scripts
        public UIFunctionalities UIFunctionalities;

        //In script use objects.
        public bool z_remapped;
        public string TempDatabaseLastBuiltStep;

        public string CurrentPriority = null;

    /////////////////////// Monobehaviour Methods /////////////////////////////////
        void Awake()
        {
            /*
            * Method is triggered on the awake event of the script and sets up object/script dependencies.
            */
            OnAwakeInitilization();
        }
        protected virtual void OnDestroy()
        {
            /*
            * Method is used to trigger the OnDestroy Event.
            * It is designed to trigger the event and send the data to the respective classes.
            */
            dbReferenceUsersCurrentSteps.Child(SystemInfo.deviceUniqueIdentifier).RemoveValueAsync();
            BuildingPlanDataItem.steps.Clear();
            BuildingPlanDataItem.PriorityTreeDictionary.Clear();
            UserCurrentStepDict.Clear();
            AssemblyDataDict.Clear();
            RemoveListners();
        }

    /////////////////////// FETCH AND PUSH DATA /////////////////////////////////
        private void OnAwakeInitilization()
        {
            /*
            * Method is used to initialize the DatabaseManager class on Awake.
            * It is used to find dependencies and set data persistence.
            */
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            StartUpTimeStamp = Time.time;

            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
        }
        public async void FetchSettingsData(DatabaseReference settings_reference)
        {
            /*
            * Method is used to fetch the ApplicationSettings data from the Firebase Realtime Database.
            * It is used to fetch the settings data and trigger an event to send the data to the ApplicationSettings class.
            */
            await settings_reference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("FetchSettingsData: Error fetching data from Firebase");
                    return;
                }
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    DeserializeSettingsData(snapshot);
                }
            });
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public async void FetchData(object source, ApplicationSettingsEventArgs e)
        {
            /*
            * Method is used to fetch the data from the Firebase Realtime Database.
            * It is used to fetch the data and trigger events to send the data to the respective classes.
            */
            dbRefrenceProject = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name);
            dbReferenceAssembly = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("assembly").Child("graph").Child("node");
            dbReferenceBuildingPlan = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("building_plan").Child("data");
            dbReferenceSteps = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("building_plan").Child("data").Child("steps");
            dbReferenceLastBuiltIndex = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("building_plan").Child("data").Child("LastBuiltIndex");
            dbReferenceQRCodes = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("QRFrames").Child("graph").Child("node");
            dbReferenceUsersCurrentSteps = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("UsersCurrentStep");

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            dbReferenceJoints = FirebaseDatabase.DefaultInstance.GetReference(e.Settings.project_name).Child("joints");

            if (e.Settings.storage_folder == "None")
            {
                await FetchRTDDatawithEventHandler(dbReferenceQRCodes, snapshot => DeserializeAssemblyDataSnapshot(snapshot, QRCodeDataDict), "TrackingDict");
                await FetchRTDDatawithEventHandler(dbReferenceAssembly, snapshot => DeserializeAssemblyDataSnapshot(snapshot, AssemblyDataDict));
                await FetchRTDDatawithEventHandler(dbReferenceBuildingPlan, snapshot => DesearializeBuildingPlanDataSnapshot(snapshot), "BuildingPlanDataDict");

                await FetchRTDDatawithEventHandler(dbReferenceJoints, snapshot => DeserializeJointDataSnapshot(snapshot, JointsDataDict), "JointsDataDict");
            }
            else
            {
                z_remapped = e.Settings.z_to_y_remap;
                dbRefrenceStorageDirectory = FirebaseStorage.DefaultInstance.GetReference("obj_storage").Child(e.Settings.storage_folder);

                string basepath = dbRefrenceStorageDirectory.Path;
                string folderPath = basepath.Substring(1);
                string storageBucket = FirebaseManager.Instance.storageBucket;
                string firebaseUrl = $"https://firebasestorage.googleapis.com/v0/b/{storageBucket}/o?prefix={folderPath}/&delimiter=/";
                List<DataHandlers.FileMetadata> files = await DataHandlers.GetFilesInWebFolder(firebaseUrl);
                List<DataHandlers.FileMetadata> filesWithUri = await DataHandlers.GetDownloadUriFromFirebaseStorageWithMetaData(files, true);

                FetchRealTimeandStorageData(filesWithUri);
            }
        }
        private async void FetchRealTimeandStorageData(List<DataHandlers.FileMetadata> files)
        {
            /*
            * Method is used to fetch the data from the Firebase Realtime Database and Firebase Storage.
            * It is used to fetch the data and trigger events to send the data to the respective classes.
            */
            string directoryPath = System.IO.Path.Combine(Application.persistentDataPath, "Object_Storage");
            await DataHandlers.DownloadFilesFromOnlineStorageDirectory(files, directoryPath);

            await FetchRTDDatawithEventHandler(dbReferenceQRCodes, snapshot => DeserializeAssemblyDataSnapshot(snapshot, QRCodeDataDict), "TrackingDict");
            await FetchRTDDatawithEventHandler(dbReferenceAssembly, snapshot => DeserializeAssemblyDataSnapshot(snapshot, AssemblyDataDict));
            await FetchRTDDatawithEventHandler(dbReferenceBuildingPlan, snapshot => DesearializeBuildingPlanDataSnapshot(snapshot), "BuildingPlanDataDict");

            await FetchRTDDatawithEventHandler(dbReferenceJoints, snapshot => DeserializeJointDataSnapshot(snapshot, JointsDataDict), "JointsDataDict");

        }
        public async Task FetchRTDDatawithEventHandler(DatabaseReference dbreference, Action<DataSnapshot> deserilizationMethod, string eventname = null)
        {
            /*
            * Method is used to fetch the data from the Firebase Realtime Database.
            * It is used to fetch the data and trigger events to send the data to the respective classes.
            */
            await DataHandlers.FetchDataFromDatabaseReference(dbreference, deserilizationMethod);

            if (eventname != null && eventname == "BuildingPlanDataDict")
            {
                OnDatabaseInitializedDict(BuildingPlanDataItem); 
            }
            if (eventname != null && eventname == "TrackingDict")
            {
                OnTrackingDataReceived(QRCodeDataDict);
            }

            if (eventname != null && eventname == "JointsDataDict")
            {
                OnJointsDataDictReceived(JointsDataDict);
            }
        }      

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

        public void PushAllDataBuildingPlan(string key)
        {        
            /*
            * Method is used to push the data to the Firebase Realtime Database.
            * It particulararly is used to push all of the BuildingPlanData Information.
            * Additionally it overwrites the individual step with my specefied device ID to indicate I made the change.
            */
            Step specificstep = BuildingPlanDataItem.steps[key];
            specificstep.data.device_id = SystemInfo.deviceUniqueIdentifier;
            string data = JsonConvert.SerializeObject(BuildingPlanDataItem);
            dbReferenceBuildingPlan.SetRawJsonValueAsync(data);
        }

    /////////////////////////// DATA DESERIALIZATION ///////////////////////////////////////
        private void DeserializeSettingsData(DataSnapshot snapshot)
        {
            /*  
            * Method is used to deserialize the ApplicationSettings data from the Firebase Realtime Database.
            */
            string path = Application.persistentDataPath;
            string storageFolderPath = Path.Combine(path, "Object_Storage");
            DataHandlers.DeleteFilesFromDirectory(storageFolderPath);
            DataHandlers.CreateDirectory(storageFolderPath);
            string AppData = snapshot.GetRawJsonValue();

            if (!string.IsNullOrEmpty(AppData))
            {
                Debug.Log("Application Settings:" + AppData);
                applicationSettings = JsonConvert.DeserializeObject<ApplicationSettings>(AppData);
            }
            else
            {
                Debug.LogWarning("You did not set your settings data properly");
            }
        
            OnSettingsUpdate(applicationSettings);
        } 
        private void DeserializeAssemblyDataSnapshot(DataSnapshot snapshot, Dictionary<string, Node> dataDict)
        {
            /*
            * Method is used to deserialize the Assembly Node data from the Firebase Realtime Database.
            * It is designed to take a snapshot of the Node data reference and iterate through them parsing the information.
            */
            dataDict.Clear();
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                string key = childSnapshot.Key;
                var json_data = childSnapshot.GetValue(true);
                Node node_data = Node.Parse(key, json_data);
                if (node_data.IsValidNode())
                {
                    dataDict[key] = node_data;
                    dataDict[key].type_id = key;
                }
                else
                {
                    if (node_data.part.dtype != "compas_timber.connections")
                    {
                        Debug.LogWarning($"DeserializeAssemblyDataSnapshot: Invalid Node structure for key '{key}'. Not added to the dictionary.");
                    }
                }
            }
            Debug.Log($"DeserializeAssemblyDataSnapshot: The number of nodes stored in the Assembly Dict is {dataDict.Count}");
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        private void DeserializeJointDataSnapshot(DataSnapshot snapshot, Dictionary<string, Data.Joint> dataDict)
        {
            /*
            * Method is used to deserialize the Assembly Node data from the Firebase Realtime Database.
            * It is designed to take a snapshot of the Node data reference and iterate through them parsing the information.
            */
            dataDict.Clear();
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                string key = childSnapshot.Key;
                var json_data = childSnapshot.GetValue(true);
                Data.Joint joint_data = Data.Joint.Parse(key, json_data);
                if (joint_data.IsValidJoint())
                {
                    dataDict[key] = joint_data;
                    dataDict[key].Key = key;
                }
                else
                {
                    Debug.LogWarning($"DeserializeJointDataSnapshot: Invalid Joint structure for key '{key}'. Not added to the dictionary.");
                }
            }

            Debug.Log($"DeserializeAssemblyDataSnapshot: The number of Joints stored in Dict is {dataDict.Count}");
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

        private void DesearializeStringItem(DataSnapshot snapshot, ref string tempStringStorage)
        {  
            /*
            * Method is used to deserialize a string item from the Firebase Realtime Database.
            * It is designed to take a snapshot of the a string data reference and parse the information.
            */
            string jsondatastring = snapshot.GetRawJsonValue();
            if (!string.IsNullOrEmpty(jsondatastring))
            {
                tempStringStorage = JsonConvert.DeserializeObject<string>(jsondatastring);
            }
            else
            {
                Debug.LogWarning("DesearializeStringItem: String Item Did not produce a value");
                tempStringStorage = null;
            }
        }
        private void DesearializeBuildingPlanDataSnapshot(DataSnapshot snapshot)
        {
            /*
            * Method is used to deserialize the Building Plan data from the Firebase Realtime Database.
            * It is designed to take a snapshot of the Building Plan data reference and parse the information.
            */
            if (BuildingPlanDataItem.steps != null && BuildingPlanDataItem.LastBuiltIndex != null)
            {
                BuildingPlanDataItem.LastBuiltIndex = null;
                BuildingPlanDataItem.steps.Clear();
            }
            if (BuildingPlanDataItem.PriorityTreeDictionary != null)
            {
                BuildingPlanDataItem.PriorityTreeDictionary.Clear();
            }

            var jsondata = snapshot.GetValue(true);
            BuildingPlanData buildingPlanData = BuildingPlanData.Parse(jsondata);
            Debug.Log($" THIS IS YOUR PRIOIRTY TREE DICTIONARY {JsonConvert.SerializeObject(buildingPlanData.PriorityTreeDictionary)}");
            if (buildingPlanData != null && buildingPlanData.steps != null)
            {
                BuildingPlanDataItem = buildingPlanData;
            }
            else
            {
                Debug.LogWarning("You did not set your building plan data properly");
            }
            
        }

    /////////////////////////// INTERNAL DATA MANAGERS //////////////////////////////////////
        public void FindInitialElement()
        {
            /*
            * Method is used to find the initial element in the Building Plan Data.
            * It is designed to iterate through the Building Plan Data and find the first element that is not built.
            */
            for (int i =0 ; i < BuildingPlanDataItem.steps.Count; i++)
            {
                Step step = BuildingPlanDataItem.steps[i.ToString()];
                if(step.data.is_built == false)
                {
                    UIFunctionalities.SetCurrentPriority(step.data.priority.ToString());
                    UIFunctionalities.SetCurrentStep(i.ToString());
                    break;
                }
            }
        }
        public int OtherUserPriorityChecker(Step step, string stepKey)
        {        
            /*
            * Method is used to check the priority of the step changed by someone else and compare it to the current priority.
            * It is designed to check if the priority is the same as the current priority
            * if it is complete, or if it is incomplete.
            */
            if (CurrentPriority == step.data.priority.ToString())
            {
                List<string> UnbuiltElements = new List<string>();
                List<string> PriorityDataItem = BuildingPlanDataItem.PriorityTreeDictionary[CurrentPriority];

                foreach(string element in PriorityDataItem)
                {
                    Step stepToCheck = BuildingPlanDataItem.steps[element];
                    if(!stepToCheck.data.is_built)
                    {
                        UnbuiltElements.Add(element);
                    }
                }
                if(UnbuiltElements.Count == 0)
                {
                    Debug.Log($"OtherUserPriorityCheck: Current Priority is complete. Unlocking Next Priority.");
                    return 2;
                }
                else
                {
                    Debug.Log($"OtherUserPriorityCheck: Current Priority is not complete. Incomplete Priority");
                    return 0;
                }
            }
            else
            {
                Debug.Log($"OtherUserPriorityCheck: Current Priority is not the same as the priority of the step. Set this priority.");
                return 1;
            }

        }

    /////////////////////////////// EVENT HANDLING ////////////////////////////////////////
        public void AddListeners(object source, EventArgs args)
        {          
            /*
            * Method is used to add event listeners to the Firebase Realtime Database Events.
            * It is designed to listen for changes in the database and trigger events to update the data.
            */
            dbReferenceSteps.ChildAdded += OnStepsChildAdded;
            dbReferenceSteps.ChildChanged += OnStepsChildChanged;
            dbReferenceSteps.ChildRemoved += OnStepsChildRemoved;
            
            dbReferenceUsersCurrentSteps.ChildAdded += OnUserChildAdded; 
            dbReferenceUsersCurrentSteps.ChildChanged += OnUserChildChanged;
            dbReferenceUsersCurrentSteps.ChildRemoved += OnUserChildRemoved;

            dbReferenceLastBuiltIndex.ValueChanged += OnLastBuiltIndexChanged;

            dbRefrenceProject.ChildAdded += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildChanged += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildRemoved += OnProjectInfoChangedUpdate;
        }
        public void RemoveListners()
        {        
            /*
            * Method is used to remove event listeners to the Firebase Realtime Database Events.
            * It is used on restart and other methods in which I cause resubscription to new references.
            */
            dbReferenceSteps.ChildAdded += OnStepsChildAdded;
            dbReferenceSteps.ChildChanged += OnStepsChildChanged;
            dbReferenceSteps.ChildRemoved += OnStepsChildRemoved;
            
            dbReferenceUsersCurrentSteps.ChildAdded += OnUserChildAdded; 
            dbReferenceUsersCurrentSteps.ChildChanged += OnUserChildChanged;
            dbReferenceUsersCurrentSteps.ChildRemoved += OnUserChildRemoved;

            dbReferenceLastBuiltIndex.ValueChanged += OnLastBuiltIndexChanged;

            dbRefrenceProject.ChildAdded += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildChanged += OnProjectInfoChangedUpdate;
            dbRefrenceProject.ChildRemoved += OnProjectInfoChangedUpdate;
        }
        public void OnStepsChildAdded(object sender, Firebase.Database.ChildChangedEventArgs args) 
        {
            /*
            * Method is used to handle the Child Added event from the Firebase Realtime Database.
            * It is designed to take the snapshot of the child added event and parse the information.
            * Additionally it the validity of the step and adds it to the dictoinary if it is.
            */
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            var key = args.Snapshot.Key;
            var childSnapshot = args.Snapshot.GetValue(true);
            Debug.Log($"OnStepsChildAdded: A child added event was triggered for the key: {key} in the steps reference.");

            if (childSnapshot != null)
            {
                Step newValue = Step.Parse(childSnapshot);
            
                if (newValue.IsValidStep())
                {
                    if (BuildingPlanDataItem.steps.ContainsKey(key))
                    {
                        Debug.Log($"OnStepsChildAdded: The key: {key} already exists in the dictionary");
                    }
                    else
                    {
                        Debug.Log($"OnStepsChildAdded: The key: {key} does not exist in the dictionary added to priority tree in {newValue.data.priority.ToString()}");
                        BuildingPlanDataItem.steps.Add(key, newValue);

                        if (BuildingPlanDataItem.PriorityTreeDictionary.ContainsKey(newValue.data.priority.ToString()))
                        {
                            BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()].Add(key);
                        }
                        else
                        {
                            BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()] = new List<string>();
                            BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()].Add(key);
                        }
                        OnDatabaseUpdate(newValue, key);                        
                    }
                }
                else
                {
                    Debug.LogWarning($"OnStepsChildAdded: The Changed key is no longer valid and will not be added to the dictionary");
                }
            }
        } 
        public void OnStepsChildChanged(object sender, Firebase.Database.ChildChangedEventArgs args) 
        {
            if (args.DatabaseError != null) {
            Debug.LogError($"OnStepsChildChanged: Database error: {args.DatabaseError}");
            return;
            }
            if (args.Snapshot == null) {
                Debug.LogWarning("OnStepsChildChanged: Snapshot is null. Ignoring the child change.");
                return;
            }

            string key = args.Snapshot.Key;
            if (key == null) {
                Debug.LogWarning("OnStepsChildChanged: Snapshot key is null. Ignoring the child change.");
                return;
            }

            var childSnapshot = args.Snapshot.GetValue(true);

            if (childSnapshot != null)
            {
                Step newValue = Step.Parse(childSnapshot);
                if (!Step.AreEqualSteps(newValue, BuildingPlanDataItem.steps[key]))
                {    
                    if (newValue.data.device_id != null)
                    {
                        //TODO: This is in the case that we want to switch to pushing one item at a time... however for now it is un needed.
                        if (newValue.data.device_id == SystemInfo.deviceUniqueIdentifier && Step.AreEqualSteps(newValue, BuildingPlanDataItem.steps[key]))
                        {
                            return;
                        }
                        else
                        {    
                            if(newValue.IsValidStep())
                            {
                                if (newValue.data.priority != BuildingPlanDataItem.steps[key].data.priority)
                                {
                                    //Remove old key from nested list, and if the list is empty remove the list from the dictionary.
                                    BuildingPlanDataItem.PriorityTreeDictionary[BuildingPlanDataItem.steps[key].data.priority.ToString()].Remove(key);
                                    if (BuildingPlanDataItem.PriorityTreeDictionary[BuildingPlanDataItem.steps[key].data.priority.ToString()].Count == 0)
                                    {
                                        BuildingPlanDataItem.PriorityTreeDictionary.Remove(BuildingPlanDataItem.steps[key].data.priority.ToString());
                                    }

                                    //Check if the steps priority already in the priority tree dictionary
                                    if (BuildingPlanDataItem.PriorityTreeDictionary.ContainsKey(newValue.data.priority.ToString()))
                                    {
                                        BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()].Add(key);
                                    }
                                    //If not add a new list and add the key to the list
                                    else
                                    {
                                        BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()] = new List<string>();
                                        BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()].Add(key);
                                    }
                                }
                                else
                                {
                                    Debug.Log($"OnStepsChildChanged: The priority of the step {key} did not change");
                                }
                                BuildingPlanDataItem.steps[key] = newValue;
                            }
                            else
                            {
                                Debug.LogWarning($"OnStepsChildChanged: Invalid Node structure for key '{key}'. Not added to the dictionary.");
                            }
                            Debug.Log($"OnStepsChildChanged: This key: {key} changed and will be replaced in the dictionary.");
                            OnDatabaseUpdate(newValue, key);
                        }
                    }
                    //Check: This change happened either manually or from grasshopper. To an object that doesn't have a device id.
                    else
                    {
                        Debug.LogWarning($"OnStepsChildChanged: Device ID is null: the change for key {key} happened from gh or manually.");
                        if(newValue.IsValidStep())
                        {
                            if (newValue.data.priority != BuildingPlanDataItem.steps[key].data.priority)
                            {                            
                                BuildingPlanDataItem.PriorityTreeDictionary[BuildingPlanDataItem.steps[key].data.priority.ToString()].Remove(key);
                                if (BuildingPlanDataItem.PriorityTreeDictionary[BuildingPlanDataItem.steps[key].data.priority.ToString()].Count == 0)
                                {
                                    BuildingPlanDataItem.PriorityTreeDictionary.Remove(BuildingPlanDataItem.steps[key].data.priority.ToString());
                                }

                                //Check if the steps priority already in the priority tree dictionary
                                if (BuildingPlanDataItem.PriorityTreeDictionary.ContainsKey(newValue.data.priority.ToString()))
                                {
                                    BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()].Add(key);
                                }
                                //If not add a new list and add the key to the list
                                else
                                {
                                    BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()] = new List<string>();
                                    BuildingPlanDataItem.PriorityTreeDictionary[newValue.data.priority.ToString()].Add(key);
                                }
                                BuildingPlanDataItem.steps[key] = newValue;
                            }
                            else
                            {
                                Debug.Log($"The priority of the step {key} did not change");
                            }

                        }
                        else
                        {
                            Debug.LogWarning($"Invalid Node structure for key '{key}'. Not added to the dictionary.");
                        }
                        Debug.Log($"OnStepsChildChanged: This key: {key} changed and will be replaced in the dictoinary.");
                        OnDatabaseUpdate(newValue, key);
                    }
                }
            }
        }
        public void OnStepsChildRemoved(object sender, Firebase.Database.ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            string key = args.Snapshot.Key;
            string childSnapshot = args.Snapshot.GetRawJsonValue();

            if (!string.IsNullOrEmpty(childSnapshot))
            {
                if (BuildingPlanDataItem.steps.ContainsKey(key))
                {
                    Step newValue = null;
                    Debug.Log($"OnStepsChildRemoved: The key: {key} exists in the dictionary and is going to be removed");
                    BuildingPlanDataItem.PriorityTreeDictionary[BuildingPlanDataItem.steps[key].data.priority.ToString()].Remove(key);

                    //Check if the priority tree list is empty and remove it from the dictionary if it is.
                    if (BuildingPlanDataItem.PriorityTreeDictionary[BuildingPlanDataItem.steps[key].data.priority.ToString()].Count == 0)
                    {
                        BuildingPlanDataItem.PriorityTreeDictionary.Remove(BuildingPlanDataItem.steps[key].data.priority.ToString());
                    }
                    
                    //Remove the step from the building plan dictionary
                    BuildingPlanDataItem.steps.Remove(key);
                    OnDatabaseUpdate(newValue, key);
                }
                else
                {
                    Debug.Log($"OnStepsChildRemoved: The key: {key} did not exist in the dictionary.");
                }
            }
        }  
        public async void OnLastBuiltIndexChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
        {
            /*
            * Method is used to handle the Last Built Index Changed event from the Firebase Realtime Database.
            * It is designed to take the snapshot of the Last Built Index and parse the information.
            * Additionally it checks the priority of this step and updates my current priority as needed.
            */
            if (args.DatabaseError != null) {
            Debug.LogError($"Database error: {args.DatabaseError}");
            return;
            }

            if (args.Snapshot == null) {
                Debug.LogWarning("Snapshot is null. Ignoring the child change.");
                return;
            }
            
            TempDatabaseLastBuiltStep = null;
            await FetchRTDDatawithEventHandler(dbReferenceLastBuiltIndex, snapshot => DesearializeStringItem(snapshot, ref TempDatabaseLastBuiltStep));
            if (TempDatabaseLastBuiltStep != null)
            {
                if(TempDatabaseLastBuiltStep != BuildingPlanDataItem.LastBuiltIndex)
                {
                    BuildingPlanDataItem.LastBuiltIndex = TempDatabaseLastBuiltStep;
                    Debug.Log($"Last Built Index is now {BuildingPlanDataItem.LastBuiltIndex}");

                    UIFunctionalities.SetLastBuiltText(BuildingPlanDataItem.LastBuiltIndex);

                    // Check Priority to see if its complete.
                    Step step = BuildingPlanDataItem.steps[BuildingPlanDataItem.LastBuiltIndex];
                    int priorityCheck = OtherUserPriorityChecker(step, BuildingPlanDataItem.LastBuiltIndex);

                    if (priorityCheck == 1) //Priority is not the same, and we should set this priority now.
                    {
                        UIFunctionalities.SetCurrentPriority(step.data.priority.ToString());
                    }
                    //All the elements of this priority are complete, and it is not the last item in the building plan.
                    else if (priorityCheck == 2 && Convert.ToInt32(BuildingPlanDataItem.LastBuiltIndex) < BuildingPlanDataItem.steps.Count - 1)
                    {
                        string localCurrentPriority = CurrentPriority;
                        int nextPriority = Convert.ToInt32(localCurrentPriority) + 1;
                        UIFunctionalities.SetCurrentPriority(nextPriority.ToString());

                        //If my CurrentStep Priority is the same as New Current Priority then update UI graphics
                        Step localCurrentStep = BuildingPlanDataItem.steps[UIFunctionalities.CurrentStep];
                        if(localCurrentStep.data.priority.ToString() == CurrentPriority)
                        {    
                            UIFunctionalities.IsBuiltButtonGraphicsControler(localCurrentStep.data.is_built, localCurrentStep.data.priority);
                        }

                    }
                    else
                    {
                        Debug.Log($"OnLastBuiltIndexChanged: priority check returned incomplete priority, and we should not move to the next priority");
                    }
                }
                else
                {
                    Debug.Log("OnLastBuiltIndexChanged: Last Built Index is the same your current Last Built Index");
                }

            }
        }    
        public void OnUserChildAdded(object sender, Firebase.Database.ChildChangedEventArgs args)
        {
            /*
            * Method is used to handle the User Child Added event from the Firebase Realtime Database.
            * It is designed to take the snapshot of the User Current Step and parse the information.
            * Additionally it checks the validity of the step and adds it to the dictoinary if it is.
            */
            if (args.DatabaseError != null) {
            Debug.LogError($"OnUserChildAdded: Database error: {args.DatabaseError}");
            return;
            }
            if (args.Snapshot == null) {
                Debug.LogWarning("OnUserChildAdded: Snapshot is null. Ignoring the child change.");
                return;
            }

            string key = args.Snapshot.Key;
            var childSnapshot = args.Snapshot.GetValue(true);

            if (childSnapshot != null)
            {
                UserCurrentInfo newValue = UserCurrentInfo.Parse(childSnapshot);
                if (newValue != null)
                {
                    if (UserCurrentStepDict.ContainsKey(key))
                    {
                        Debug.Log($"OnUserChildAdded: This User {key} already exists in the dictionary");
                    }
                    else
                    {
                        Debug.Log($"OnUserChildAdded: The key '{key}' does not exist in the dictionary");
                        UserCurrentStepDict.Add(key, newValue);
                        OnUserInfoUpdated(newValue, key);
                    }
                }
            }
        }
        public void OnUserChildChanged(object sender, Firebase.Database.ChildChangedEventArgs args)
        {
            /*
            * Method is used to handle the User Child Changed event from the Firebase Realtime Database.
            * It is designed to take the snapshot of the User Current Information and parse the information.
            * Additionally it triggers event changes to remove or update the Users Object.
            */
            if (args.DatabaseError != null) {
            Debug.LogError($"OnUserChildChanged: Database error: {args.DatabaseError}");
            return;
            }

            if (args.Snapshot == null) {
                Debug.LogWarning("OnUserChildChanged: Snapshot is null. Ignoring the child change.");
                return;
            }

            string key = args.Snapshot.Key;
            var childSnapshot = args.Snapshot.GetValue(true);

            if (childSnapshot != null)
            {
                UserCurrentInfo newValue = UserCurrentInfo.Parse(childSnapshot);
                if (key != SystemInfo.deviceUniqueIdentifier)
                {    
                    if(newValue != null)
                    {
                        UserCurrentStepDict[key] = newValue;
                    }
                    else
                    {
                        Debug.LogWarning($"OnUserChildChanged: User info data for '{key}' is null. Not added to the dictionary.");
                    }
                    OnUserInfoUpdated(newValue, key);
                }
            }
        }
        public void OnUserChildRemoved(object sender, Firebase.Database.ChildChangedEventArgs args)
        {
            /*
            * Method is used to handle the User Child Removed event from the Firebase Realtime Database.
            * It is designed to take the snapshot of the User Current Information and parse the information.
            * Additionally it triggers event changes to remove the Users Object.
            */
            if (args.DatabaseError != null) {
            Debug.LogError($"OnUserChildRemoved: Database error: {args.DatabaseError}");
            return;
            }
            
            string key = args.Snapshot.Key;
            string childSnapshot = args.Snapshot.GetRawJsonValue();
            
            if (!string.IsNullOrEmpty(childSnapshot))
            {
                if (UserCurrentStepDict.ContainsKey(key))
                {
                    UserCurrentInfo newValue = null;
                    Debug.Log($"OnUserChildRemoved: User {key} left and will be removed from the scene.");
                    UserCurrentStepDict.Remove(key);
                    OnUserInfoUpdated(newValue, key);
                }
                else
                {
                    Debug.Log($"OnUserChildRemoved: User {key} doesn't exist in the dictionary.");
                }
            }

        }
        
        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        bool IsWithinTimeWindow(float currentTime, float startTime, float timeWindow)
        {
            /*
            * Method is used to check if the current time is within a time window.
            * It is designed to check if the current time is within the time window.
            */
            return (currentTime - startTime) <= timeWindow;
        }
        public async void OnProjectInfoChangedUpdate(object sender, Firebase.Database.ChildChangedEventArgs args)
        {
            /*
            * Method is used to handle the Project Info Changed event from the Firebase Realtime Database.
            * It is designed to take the snapshot of the Project Information and parse the information.
            * Additionally it triggers event changes to update the data in the respective dictionaries.
            * It is used to handle global changes like assembly, qrcodes, & etc.
            * It will clear my currently stored information and fetch the new data.
            */

            if (args.DatabaseError != null) {
            Debug.LogError($"OnProjectInfoChangedUpdate: Database error: {args.DatabaseError}");
            return;
            }

            if (args.Snapshot == null) {
                Debug.LogWarning("OnProjectInfoChangedUpdate: Snapshot is null. Ignoring the child change.");
                return;
            }

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            if (IsWithinTimeWindow(Time.time, StartUpTimeStamp, 3.0f))
            {
                Debug.Log($"OnProjectInfoChangedUpdate: Time Window is still open. Ignoring the child change on Key {args.Snapshot.Key}.");
                return;
            }

            string key = args.Snapshot.Key;
            var childSnapshot = args.Snapshot.GetValue(true);

            if (childSnapshot != null && key != null)
            {
                if(key == "assembly")
                {
                    Debug.Log("OnProjectInfoChangedUpdate: Assembly Changed");
                    await FetchRTDDatawithEventHandler(dbReferenceAssembly, snapshot => DeserializeAssemblyDataSnapshot(snapshot, AssemblyDataDict));
                }
                else if(key == "QRFrames")
                {
                    Debug.Log("OnProjectInfoChangedUpdate: QRFrames Changed");
                    await FetchRTDDatawithEventHandler(dbReferenceQRCodes, snapshot => DeserializeAssemblyDataSnapshot(snapshot, QRCodeDataDict), "TrackingDict");
                }
                else if(key == "beams")
                {
                    Debug.Log("OnProjectInfoChangedUpdate: Beams Changed");
                }
                else if(key == "joints")
                {
                    InstantiateObjects instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
                    if (JointsDataDict != null)
                    {
                        JointsDataDict.Clear();
                    }
                    if(instantiateObjects != null && instantiateObjects.Joints.transform.childCount > 0)
                    {
                        instantiateObjects.DestroyAllJoints();
                    }
                    else
                    {
                        Debug.LogWarning("OnProjectInfoChangedUpdate: Joints Data Dict is null or Instantiate Objects is null");
                    }

                    await FetchRTDDatawithEventHandler(dbReferenceJoints, snapshot => DeserializeJointDataSnapshot(snapshot, JointsDataDict), "JointsDataDict");
                    Debug.Log("OnProjectInfoChangedUpdate: Joints Changed");
                }
                else if(key == "parts")
                {
                    Debug.Log("OnProjectInfoChangedUpdate: Parts changed");
                }
                else if(key == "building_plan")
                {
                    Debug.Log("OnProjectInfoChangedUpdate: BuildingPlan and should be handled by other listners");
                }
                else if(key == "UsersCurrentStep")
                {
                    Debug.Log("OnProjectInfoChangedUpdate: User Current Step Changed this should be handled by other listners");
                }
                else
                {
                    Debug.LogWarning($"Project Changed: The key: {key} did not match the expected project keys");
                }
            }
        }
        protected virtual void OnDatabaseInitializedDict(BuildingPlanData BuildingPlanDataItem)
        {
            /*
            * Method is used to trigger the Database Initialized Event.
            * It is designed to trigger the event and send the Building Plan Data to the respective classes.
            */
            UnityEngine.Assertions.Assert.IsNotNull(DatabaseInitializedDict, "Database dict is null!");
            Debug.Log("OnDatabaseInitializedDict: Building Plan Data Received");
            DatabaseInitializedDict(this, new BuildingPlanDataDictEventArgs() {BuildingPlanDataItem = BuildingPlanDataItem});
        }
        protected virtual void OnTrackingDataReceived(Dictionary<string, Node> QRCodeDataDict)
        {
            /*
            * Method is used to trigger the Tracking Data Received Event.
            * It is designed to trigger the event and send the Tracking Data to the respective classes.
            */
            UnityEngine.Assertions.Assert.IsNotNull(TrackingDictReceived, "Tracking Dict is null!");
            Debug.Log("OnTrackingDataReceived: Tracking Data Received");
            TrackingDictReceived(this, new TrackingDataDictEventArgs() {QRCodeDataDict = QRCodeDataDict});
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        protected virtual void OnJointsDataDictReceived(Dictionary<string, Data.Joint> inputJointsDataDict)
        {
            /*
            * Method is used to trigger the Tracking Data Received Event.
            * It is designed to trigger the event and send the Tracking Data to the respective classes.
            */
            UnityEngine.Assertions.Assert.IsNotNull(JointsDataDictReceived, "Joints Dict is null!");
            Debug.Log("OnTrackingDataReceived: Tracking Data Received");
            JointsDataDictReceived(this, new JointsDataDictEventArgs() {JointsDataDict = inputJointsDataDict});
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        protected virtual void OnDatabaseUpdate(Step newValue, string key)
        {
            /*
            * Method is used to trigger the Database Update Event.
            * It is designed to trigger the event and send the Step Data to the respective classes.
            */
            UnityEngine.Assertions.Assert.IsNotNull(DatabaseInitializedDict, "new dict is null!");
            DatabaseUpdate(this, new UpdateDataItemsDictEventArgs() {NewValue = newValue, Key = key });
        }
        protected virtual void OnUserInfoUpdated(UserCurrentInfo newValue, string key)
        {
            /*
            * Method is used to trigger the User Info Updated Event.
            * It is designed to trigger the event and send the User Info Data to the respective classes.
            */
            UnityEngine.Assertions.Assert.IsNotNull(UserInfoUpdate, "new dict is null!");
            UserInfoUpdate(this, new UserInfoDataItemsDictEventArgs() {UserInfo = newValue, Key = key });
        }
        protected virtual void OnSettingsUpdate(ApplicationSettings settings)
        {
            /*
            * Method is used to trigger the Settings Update Event.
            * It is designed to trigger the event and send the Settings Data to the ApplicationSettings class.
            */
            ApplicationSettingUpdate(this, new ApplicationSettingsEventArgs(){Settings = settings});
        }
    }

    public static class DataHandlers
    {
        class ListFilesResponse
        {
            /*
            * Class is used to deserialize the JSON response from web request to a online directory.
            */
            public List<object> prefix { get; set; }
            public List<FileMetadata> items { get; set; }
        }

        public class FileMetadata
        {
            /*
            * Class is used to store the metadata of the files in the online directory.
            */
            public string name { get; set; }
            public string bucket { get; set; }
            public string uri { get; set; }
        }
        public static void DeleteFilesFromDirectory(string directoryPath)
        {
            /*
            * Method is used to delete all files in a directory.
            * It is designed to take the directory path and delete all files in the directory.
            */
            string folderpath = directoryPath.Replace('\\', '/');
            if (Directory.Exists(folderpath))
            {
                System.IO.DirectoryInfo di = new DirectoryInfo(folderpath);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                Debug.Log($"DeleteObjectsFromDirectory: Deleted all files in the directory @ {folderpath}");
            }
        }
        public static void CreateDirectory(string directoryPath)
        {
            /*
            * Method is used to create a directory.
            * It is designed to take the directory path and create the directory if it does not exist.
            */
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"CreateDirectory: Created Directory for Object Storage @ {directoryPath}");
            }
            else
            {
                Debug.Log($"CreateDirectory: Directory @ path {directoryPath} already exists.");
            }
        }
        public static void PushStringDataToDatabaseReference(DatabaseReference databaseReference, string data)
        {
            /*
            * Method is used to push a string data to the Firebase Realtime Database databaseReference.
            */
            databaseReference.SetRawJsonValueAsync(data);
        }
        public static string PrintDataFromDatabaseRefrerence(DatabaseReference databaseReference)
        {
            /*
            * Method is used to print the data from the Firebase Realtime Database databaseReference.
            */
            string jsondata = "";
            databaseReference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("error");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;
                    jsondata = dataSnapshot.GetRawJsonValue(); 
                    Debug.Log("all nodes" + jsondata);
                }
            });
            return jsondata;
        }
        public static void CheckLocalPathExistance(string path)
        {       
            /*
            * Method is used to check if a file exists in the local directory.
            * It is designed to take the file path and check if the file exists.
            */
            path = path.Replace('\\', '/');
            if (File.Exists(path))
            {
                Debug.Log($"CheckLocalPathExistance: File Exists @ {path}");
            }
            else
            {
                Debug.Log($"CheckLocalPathExistance: File does not exist @ {path}");
            }

        }
        public static async Task DownloadFilesFromOnlineStorageDirectory(List<DataHandlers.FileMetadata> filesMetadata, string localDirectoryPath) //TODO: Can't Move Because DownloadFile cant move
        {
            /*
            * Method is used to download files from an online storage directory.
            * It is designed to take the files metadata and the local directory path and download the files.
            */
            List<Task> downloadTasks = new List<Task>();
            foreach (var fileMetadata in filesMetadata)
            {
                string downloadUrl = fileMetadata.uri;
                string localFilePath = System.IO.Path.Combine(localDirectoryPath, System.IO.Path.GetFileName(fileMetadata.name));
                CreateDirectory(localDirectoryPath);
                downloadTasks.Add(DownloadFileFromURLToLocal(downloadUrl, localFilePath, true));
            }
            await Task.WhenAll(downloadTasks);
        }
        public static async Task<List<FileMetadata>> GetFilesInWebFolder(string webFolderUrl)
        {
            /*
            * Method is used to get the files in a web folder.
            * It is designed to take the web folder URL and return the files in the folder.
            */
            Debug.Log($"GetFilesInFolder: Web Directory URL : {webFolderUrl}");
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(webFolderUrl))
            using (HttpContent content = response.Content)
            {                
                string responseText = await content.ReadAsStringAsync();
                Debug.Log($"GetFilesInFolder: File list response information: {responseText}");
                ListFilesResponse responseData = JsonConvert.DeserializeObject<ListFilesResponse>(responseText);
                return responseData.items;
            }
        }
        public static async Task FetchDataFromDatabaseReference(DatabaseReference dbreference, Action<DataSnapshot> deserilizationMethod)
        {
            /*
            * Method is used to fetch the data from the Firebase Realtime Database.
            * It is designed to take the database reference and the deserialization method and fetch the data.
            */
            await dbreference.GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("FetchDataFromDatabaseReference: Error fetching data from Firebase");
                    return;
                }

                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (deserilizationMethod != null)
                    {
                        deserilizationMethod(snapshot);
                    }
                }
            });
        }
        public static async Task DownloadFileFromURLToLocal(string downloadUrl, string saveFilePath, bool onScreenMessage)
        {
            /*
            * Method is used to download a file from a URL to a local directory.
            * It is designed to take the download URL, the save file path, and a boolean to display an on screen message.
            */
            using (UnityWebRequest webRequest = UnityWebRequest.Get(downloadUrl))
            {
                webRequest.downloadHandler = new DownloadHandlerFile(saveFilePath);
                await webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    if(onScreenMessage)
                    {    
                        Color panelColor = new Color(1.0f, 1.0f, 1.0f, 0.85f);
                        string filepath = Path.GetFileName(saveFilePath);
                        string message = $"DownloadFileFromURLToLocal: ERROR: Application failed download file for object {filepath}. Please review the associated file and try again.";
                        UserInterface.CreateCenterAlignedSelfDestructiveMessageInstance(
                            "FileDownloadFailedMessage", 450.0f, 725.0f, panelColor,
                            TMPro.TextAlignmentOptions.TopLeft, 25.0f, Color.red,
                            message, 70.0f, 250.0f, Color.red, 25.0f, "Acknowledge",
                            Color.white);
                    }

                    Debug.LogError("DownloadFileFromURLToLocal: File download error: " + webRequest.error);
                }
                else
                {
                    Debug.Log("DownloadFileFromURLToLocal: File successfully downloaded and saved to " + saveFilePath);
                }
            }
        }
        public static async Task<List<DataHandlers.FileMetadata>> GetDownloadUriFromFirebaseStorageWithMetaData(List<DataHandlers.FileMetadata> filesMetadata, bool onScreenMessage)
        {
            /*
            * Method is used to get the download URI from Firebase Storage ONLY with metadata.
            * It is designed to take the files metadata and a boolean to display an on screen message with download failure.
            */
            List<Task> fetchUriTasks = new List<Task>();
            foreach (var fileMetadata in filesMetadata)
            {
                var fileRef = FirebaseStorage.DefaultInstance.GetReference(fileMetadata.name);
                fetchUriTasks.Add(fileRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted)
                    {
                        if(onScreenMessage)
                        {    
                            Color panelColor = new Color(1.0f, 1.0f, 1.0f, 0.85f);
                            string message = $"ERROR: Application unable to fetch URL for {fileMetadata.name}. Please review the associated file and try again.";
                            UserInterface.CreateCenterAlignedSelfDestructiveMessageInstance(
                                "FetchDownloadURIFailedMessage", 450.0f, 725.0f, panelColor,
                                TMPro.TextAlignmentOptions.TopLeft, 25.0f, Color.red,
                                message, 70.0f, 250.0f, Color.red, 25.0f, "Acknowledge",
                                Color.white);
                        }
                        Debug.LogError("GetDownloadUriFromFirebaseStorageWithMetaData: Error fetching download URL from Firebase Storage");
                        return;
                    }
                    if (task.IsCompleted)
                    {
                        Uri downloadUrlUri = task.Result;
                        string downloadUrl = downloadUrlUri.ToString();
                        fileMetadata.uri = downloadUrl;
                    }
                }));
            }
            await Task.WhenAll(fetchUriTasks);
            return filesMetadata;
        }
    }

}