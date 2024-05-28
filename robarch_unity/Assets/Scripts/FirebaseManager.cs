using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using CompasXR.Systems;


namespace CompasXR.Database.FirebaseManagment
{
    /*
    * CompasXR.Database.FirebaseManagement : A namespace to define and controll various Firebase connection,
    * configuration information, user record, and general database management.
    */

    public sealed class FirebaseManager
    {
        /*
        * FirebaseManager : Sealed class using the Singleton Pattern.
        * This class is used to manage the Firebase configuration settings.
        */

        private static FirebaseManager instance = null;
        private static readonly object padlock = new object();
        public static FirebaseManager Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new FirebaseManager();
                    }
                    return instance;
                }
            }
        }
        public string appId;
        public string apiKey;
        public string databaseUrl;
        public string storageBucket;
        public string projectId;

        FirebaseManager() 
        {
            /*
            * FirebaseManager : Constructor for the FirebaseManager class.
            * This constructor is used to set the configuration settings for Firebase.
            * It contains the required settings for connecting to the base database.
            */
            apiKey = "AIzaSyBL5TXacR15lZaXDm3PUbWuwcvTSN5iLjc";
            databaseUrl = "https://robarchcompasxr-default-rtdb.europe-west1.firebasedatabase.app";
            storageBucket = "robarchcompasxr.appspot.com";
            projectId = "robarchcompasxr";

            CompasXR.Systems.OperatingSystem currentOS = OperatingSystemManager.GetCurrentOS();
            switch (currentOS)
            {
                case CompasXR.Systems.OperatingSystem.iOS:
                    appId = "1:669195035760:ios:a0845b32906b4f487b80b5";
                    break;
                case CompasXR.Systems.OperatingSystem.Android: 
                    appId = "1:669195035760:android:a769c70eac3c04a67b80b5";
                    break;
                default:
                    appId = "1:669195035760:android:a769c70eac3c04a67b80b5";
                    break;
            }
        }


    }
}