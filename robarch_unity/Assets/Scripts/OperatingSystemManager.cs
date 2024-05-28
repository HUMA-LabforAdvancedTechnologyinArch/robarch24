using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CompasXR.Systems
{
    /*
    * CompasXR.Systems : A namespace to define and controll various system
    * level settings and configurations.
    */

    public class OperatingSystemManager : MonoBehaviour
    {
        /*
        * OperatingSystemManager : Class is used to get the Operating System Info.
        * Contains enum for operating systems Android, iOS, and Unknown.
        */
        public static OperatingSystem GetCurrentOS()
        {
            #if UNITY_ANDROID
            Debug.Log("Operating System: Android");
            return OperatingSystem.Android;
            #elif UNITY_IOS
            Debug.Log("Operating System: iOS");
            return OperatingSystem.iOS;
            #else
            Debug.Log("Operating System: Unknown");
            return OperatingSystem.Unknown; 
            #endif
        }
    }
    public enum OperatingSystem
    {
        Android,
        iOS,
        Unknown
    }

}
