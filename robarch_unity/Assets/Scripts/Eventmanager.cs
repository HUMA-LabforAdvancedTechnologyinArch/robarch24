using System;
using UnityEngine;
using Firebase.Database;
using CompasXR.Database.FirebaseManagment;
using CompasXR.Robots;

namespace CompasXR.Core
{
    /*
    * CompasXR.Core : Is the Primary namespace for all Classes that
    * controll the primary functionalities of the CompasXR Application.
    */
    public class EventManager : MonoBehaviour
    {
        /*
        * EventManager : Class is used to manage global event listeners and subscriptions and has 2 primary functions.
        * 1. To initialize the application and establish the start up routine for passing information between scripts.
        * 2. To manage the global event listeners and throughout interaction and infomtion change.
        */

        //GameObjects for Script Storage
        public GameObject databaseManagerObject;
        public GameObject instantiateObjectsObject;
        public GameObject checkFirebaseObject;
        public GameObject qrLocalizationObject;
        public GameObject mqttTrajectoryReceiverObject;
        public GameObject trajectoryVisualizerObject;

        //Settings Database Reference
        public DatabaseReference dbReferenceSettings;

        //Other Script Components
        public DatabaseManager databaseManager;

        //////////////////////////// Monobehaviour Methods //////////////////////////////
        void Awake()
        {            
            //Initilization functionalities for the application.
            Caching.ClearCache();
            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            dbReferenceSettings =  FirebaseDatabase.DefaultInstance.GetReference("ApplicationSettings");
            
            //Add script components to objects in the scene
            databaseManager = databaseManagerObject.AddComponent<DatabaseManager>();  
            InstantiateObjects instantiateObjects = instantiateObjectsObject.AddComponent<InstantiateObjects>();
            CheckFirebase checkFirebase = checkFirebaseObject.AddComponent<CheckFirebase>();
            QRLocalization qrLocalization = qrLocalizationObject.GetComponent<QRLocalization>();
            MqttTrajectoryManager mqttTrajectoryReceiver = mqttTrajectoryReceiverObject.GetComponent<MqttTrajectoryManager>();
            TrajectoryVisualizer trajectoryVisualizer = trajectoryVisualizerObject.GetComponent<TrajectoryVisualizer>();
            
            //Establish Global Event Listeners
            checkFirebase.FirebaseInitialized += DBInitializedFetchSettings;
            databaseManager.ApplicationSettingUpdate += databaseManager.FetchData;
            databaseManager.ApplicationSettingUpdate += mqttTrajectoryReceiver.SetCompasXRTopics;
            databaseManager.DatabaseInitializedDict += instantiateObjects.OnDatabaseInitializedDict;

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            databaseManager.JointsDataDictReceived += instantiateObjects.OnJointsInformationReceived;
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

            databaseManager.TrackingDictReceived += qrLocalization.OnTrackingInformationReceived;
            instantiateObjects.PlacedInitialElements += databaseManager.AddListeners;
            databaseManager.DatabaseUpdate += instantiateObjects.OnDatabaseUpdate;
            databaseManager.UserInfoUpdate += instantiateObjects.OnUserInfoUpdate;
        }

        //////////////////////////// Event Methods //////////////////////////////////////
        public void DBInitializedFetchSettings(object sender, EventArgs e)
        {
            /*
            * Method is used to fetch the settings data from the Firebase Database
            * once the connection has been initilized.
            */
            databaseManager.FetchSettingsData(dbReferenceSettings);
        }  

    }
}

