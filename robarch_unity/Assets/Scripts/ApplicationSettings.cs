    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    namespace CompasXR.AppSettings
    {
        /*
        * CompasXR.AppSettings Namespace contains all classes related to control
        * & management of internal app functionalities and methods.
        */

        [System.Serializable]
        public class ApplicationSettings
        {

            /*
            * CompasXR.AppSettings Namespace contains all classes related to direct control of 
            * internal app functionalities and methods.
            */
            public string project_name {get; set;}
            public string storage_folder {get; set;}
            public bool z_to_y_remap {get; set;}
        }
    }