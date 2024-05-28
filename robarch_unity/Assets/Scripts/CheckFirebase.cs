using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;

namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user record, and general database management.
    */

    public class CheckFirebase : MonoBehaviour
    {
        /*
        * CheckFirebase : Class is used to check if Firebase is initialized or not.
        * It is linked to an event that is passed to additional scripts to provide initilization confirmation.
        */

        public delegate void FirebaseInitializedEventHandler(object source, EventArgs args);
        public event FirebaseInitializedEventHandler FirebaseInitialized;

        public void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogError(message: $"Failed to initialize Firebase with {task.Exception}");
                    return;
                }

                OnFirebaseInitialized();
                Debug.Log("Invoked");
            });
        }

        protected virtual void OnFirebaseInitialized()
        {
            if(FirebaseInitialized != null)
            {
                FirebaseInitialized(this, EventArgs.Empty);
            }
        } 

    }
}