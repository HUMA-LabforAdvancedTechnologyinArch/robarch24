using UnityEngine;

namespace CompasXR.AppSettings
{

    /*
    CompasXR.AppSettings Namespace contains all classes related to control
    & management of internal app functionalities and methods.
    */

    public class ModeControler
    {
        /*
        The Mode Controler class is contains two enums 
        to allow for one place to controll both touch and color.
        TouchMode controls users ability to touch select objects in space. 
        VisulizationMode controls coloring of the objects in space.
        */

        public VisulizationMode VisulizationMode { get; set; }
        public TouchMode TouchMode { get; set; }
        
        public ModeControler()
        {
            VisulizationMode = VisulizationMode.BuiltUnbuilt;
            TouchMode = TouchMode.None;
        }
    }

    public enum VisulizationMode
    {
        BuiltUnbuilt = 0,
        ActorView = 1,
    }
    
    public enum TouchMode
    {
        None = 0,
        ElementEditSelection = 1,
        JointSelection = 2,

    }


}

