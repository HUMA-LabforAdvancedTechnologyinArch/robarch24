using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using CompasXR.Core.Extentions;

namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user record and general database management.
    */

    public class UserManager : MonoBehaviour
    {
        /*
        * UserManager : Class is used to manage the user record and configuration settings.
        * Additionally it is designed to handle the user record events, and allow users to create new user
        */
        private string userID;
        private DatabaseReference dbReference_root;
        public TMPro.TMP_InputField Username;

        public class User
        {
            public Dictionary<string, Device> devices;

            public User()
            {
                /*
                * User : Class is used to manage a device name under a user input.
                */
                devices = new Dictionary<string, Device>();
            }
        }

        [System.Serializable]
        public class Device
        {
            /*
            * Device : Class is used to manage the device record and configuration settings.
            */
            public List<string> dates;

            public Device()
            {
                dates = new List<string>();
            }
        }

        //////////////////////////// Monobehaviour Methods //////////////////////////////
        void Start()
        {
            userID = SystemInfo.deviceUniqueIdentifier;
            dbReference_root = FirebaseDatabase.DefaultInstance.RootReference;
            
            if (dbReference_root == null)
            {
                Debug.LogError("Firebase Database reference is null!");
            }
            else
            {
                print("Firebase Database reference is initialized.");
            }
        }

        //////////////////////////// User Management Methods //////////////////////////////
        public void CreateUser()
        {
            if (dbReference_root == null)
            {
                Debug.LogError("dbReference_root is null! Make sure Firebase is initialized and the Start method is called.");
                return;
            }

            if (Username == null || string.IsNullOrWhiteSpace(Username.text))
            {
                GameObject UsernameInputMessage = GameObject.Find("Canvas").FindObject("UsernameInputMessage");
                UsernameInputMessage.SetActive(true);
                Debug.Log("Username is not assigned or empty!");
                return;
            }
            
            string time = System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss");
            string playerName = Username.text.ToLower();

            dbReference_root.Child("Users").Child(playerName).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error occurred while reading data from the database.");
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {   
                    if (snapshot.Child("devices").Child(userID).Exists)
                    {
                        List<string> currentDates = new List<string>();
                        foreach (var dateSnapshot in snapshot.Child("devices").Child(userID).Child("dates").Children)
                        {
                            string date = dateSnapshot.GetValue(true).ToString();
                            currentDates.Add(date);
                        }
                        currentDates.Add(time);
                        dbReference_root.Child("Users").Child(playerName).Child("devices").Child(userID).Child("dates").SetValueAsync(currentDates).ContinueWithOnMainThread(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Debug.LogError("Failed to update user.");
                            }
                            else if (t.IsCompleted)
                            {
                                Debug.Log("User updated successfully.");
                                HelpersExtensions.ChangeScene("MainGame");
                            }
                        });
                    }
                    else
                    {   
                        dbReference_root.Child("Users").Child(playerName).Child("devices").Child(userID).Child("dates").Child("0").SetValueAsync(time).ContinueWithOnMainThread(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Debug.LogError("Failed to create device.");
                            }
                            else if (t.IsCompleted)
                            {
                                Debug.Log("Device created successfully.");
                                HelpersExtensions.ChangeScene("MainGame");
                            }
                        });
                    }
                }
                else
                {   
                    User newUser = new User();
                    Device newDevice = new Device();
                    newDevice.dates.Add(time);
                    newUser.devices.Add(userID, newDevice);
                    string json = JsonConvert.SerializeObject(newUser);
                    dbReference_root.Child("Users").Child(playerName).SetRawJsonValueAsync(json).ContinueWithOnMainThread(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Debug.LogError("Failed to create user.");
                        }
                        else if (t.IsCompleted)
                        {
                            Debug.Log("User created successfully.");
                            HelpersExtensions.ChangeScene("MainGame");
                        }
                    });
                }
            });
        }
    }
}