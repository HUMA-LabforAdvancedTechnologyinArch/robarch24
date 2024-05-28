using UnityEngine;
using Firebase;
using Firebase.Extensions;
using CompasXR.Core.Extentions;


namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user user record and general database management.
    */

    public class FirebaseInitializer : MonoBehaviour
    {
        /*
        * FirebaseInitializer : Class is used to initialize Firebase with the provided configuration settings.
        * Additionally it is designed to change scenes on successful initilization.
        */
        
        public MqttFirebaseConfigManager mqttConfigManager;

        //////////////////////////// Monobehaviour Methods //////////////////////////////
        public void Start()
        {
            mqttConfigManager = FindObjectOfType<MqttFirebaseConfigManager>();
            if (mqttConfigManager == null)
            {
                Debug.LogError("MqttConfigManager not found in the scene.");
            }
        }

        //////////////////////////// Firebase Initilization Methods///////////////////////
        public void InitializeFirebase()
        {
            /*
            * InitializeFirebase : Method is used to initialize Firebase with the provided configuration settings.
            */
            AppOptions options = new AppOptions
            {
                AppId = FirebaseManager.Instance.appId,
                ApiKey = FirebaseManager.Instance.apiKey,
                DatabaseUrl = new System.Uri(FirebaseManager.Instance.databaseUrl),
                StorageBucket = FirebaseManager.Instance.storageBucket,
                ProjectId = FirebaseManager.Instance.projectId,
            };

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                Firebase.DependencyStatus dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    FirebaseApp app = FirebaseApp.Create(options);

                    if (app != null)
                    {
                        Debug.Log("InitializeFirebase: Firebase Initialized Successfully");
                        mqttConfigManager.Disconnect();
                        HelpersExtensions.ChangeScene("Login");
                    }
                    else
                    {
                        Debug.LogError("InitializeFirebase: Failed to create Firebase app. Please check your configuration.");
                    }
                }
                else
                {
                    Debug.LogError($"InitializeFirebase: Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

    }
}