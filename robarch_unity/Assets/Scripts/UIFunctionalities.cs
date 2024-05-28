using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System;
using System.Linq;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using CompasXR.Core;
using CompasXR.Systems;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.AppSettings;
using CompasXR.Robots;
using CompasXR.Robots.MqttData;
using Unity.VisualScripting;

namespace CompasXR.UI
{
    /*
    * CompasXR.UI : Is the namespace for all Classes that
    * controll the primary functionalities releated to the User Interface in the CompasXR Application.
    * Functionalities, such as UI interaction, UI element creation, and UI element control.
    */
    public class UIFunctionalities : MonoBehaviour
    {
        /*
        * UIFunctionalities : Class is used to manage the User Interface and User Interface elements.
        * This class is designed to handle the all UI elements and are primariarily divided into 3 sections.
        * 1. Primary UI Elements: These are UI elements used to control the primary functionalities of the application (constantly on the screen).
        * 2. Visualizer Menu Elements: These are UI elements used to control the various visualization functionalities of the application.
        * 3. Menu Elements: These are UI elements used to control additional functionalities of the application. Ex. Info, Reload, etc.
        */

        //Other Scripts for inuse objects
        public DatabaseManager databaseManager;
        public InstantiateObjects instantiateObjects;
        public EventManager eventManager;
        public MqttTrajectoryManager mqttTrajectoryManager;
        public TrajectoryVisualizer trajectoryVisualizer;
        public RosConnectionManager rosConnectionManager;
        public ScrollSearchManager scrollSearchManager;

        //Primary UI Objects
        private GameObject VisibilityMenuObject;
        private GameObject MenuButtonObject;
        private GameObject EditorToggleObject;
        public GameObject CanvasObject;
        public GameObject ConstantUIPanelObjects;
        public GameObject NextGeometryButtonObject;
        public GameObject PreviousGeometryButtonObject;
        public GameObject PreviewGeometrySliderObject;
        public Slider PreviewGeometrySlider;
        public GameObject IsBuiltPanelObjects;
        public GameObject IsBuiltButtonObject;
        public GameObject IsbuiltButtonImage;
    

        //On Screen Messages
        public GameObject MessagesParent;
        public GameObject OnScreenErrorMessagePrefab;
        public GameObject OnScreenInfoMessagePrefab;
        private GameObject PriorityIncompleteWarningMessageObject;
        private GameObject PriorityIncorrectWarningMessageObject;
        private GameObject PriorityCompleteMessageObject;
        public GameObject MQTTFailedToConnectMessageObject;
        public GameObject MQTTConnectionLostMessageObject;
        public GameObject ErrorFetchingDownloadUriMessageObject;
        public GameObject ErrorDownloadingObjectMessageObject;
        public GameObject TrajectoryReviewRequestMessageObject;
        public GameObject TrajectoryCancledMessage;
        public GameObject TrajectoryRequestTimeoutMessage;
        public GameObject SearchItemNotFoundWarningMessageObject;
        public GameObject ActiveRobotIsNullWarningMessageObject;
        public GameObject TransactionLockActiveWarningMessageObject;
        public GameObject MqttNotConnectedForRequestWarningMessageObject;
        public GameObject ActiveRobotCouldNotBeFoundWarningMessage;
        public GameObject ActiveRobotUpdatedFromPlannerMessageObject;
        public GameObject TrajectoryResponseIncorrectWarningMessageObject;
        public GameObject ConfigDoesNotMatchURDFStructureWarningMessageObject;
        public GameObject TrajectoryNullWarningMessageObject;

        //Visualizer Menu Objects
        private GameObject VisualzierBackground;
        private GameObject PreviewActorToggleObject;
        public GameObject IDToggleObject;
        public GameObject RobotToggleObject;
        public GameObject ObjectLengthsToggleObject;
        public GameObject JointsToggleObject;
        private GameObject ObjectLengthsUIPanelObjects;
        private Vector3 ObjectLengthsUIPanelPosition;
        private TMP_Text ObjectLengthsText;
        private GameObject ObjectLengthsTags;
        public GameObject ScrollSearchToggleObject;
        private GameObject ScrollSearchObjects;
        public GameObject PriorityViewerToggleObject;
        public GameObject NextPriorityButtonObject;
        public GameObject PreviousPriorityButtonObject;
        public GameObject PriorityViewerBackground;
        public GameObject SelectedPriorityTextObject;
        public TMP_Text SelectedPriorityText;

        //Menu Toggle Button Objects
        private GameObject MenuBackground;
        private GameObject ReloadButtonObject;
        private GameObject InfoToggleObject;
        private GameObject InfoPanelObject;
        public GameObject CommunicationToggleObject;
        private GameObject CommunicationPanelObject;

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public GameObject JointScaleToggleObject;

        //Editor Toggle Objects
        private GameObject EditorBackground;
        private GameObject BuilderEditorButtonObject;
        private GameObject BuildStatusButtonObject;
        
        //Communication Specific Objects
        private TMP_InputField MqttBrokerInputField;
        private TMP_InputField MqttPortInputField;
        private GameObject MqttUpdateConnectionMessage;
        public GameObject MqttConnectionStatusObject;
        public GameObject MqttConnectButtonObject;
        public GameObject RosConnectButtonObject;
        private TMP_InputField RosHostInputField;
        private TMP_InputField RosPortInputField;
        private GameObject RosUpdateConnectionMessage;
        public GameObject RosConnectionStatusObject;

        //Trajectory Review UI Controls
        public GameObject ReviewTrajectoryObjects;
        public GameObject RequestTrajectoryButtonObject;
        public GameObject ApproveTrajectoryButtonObject;
        public GameObject RejectTrajectoryButtonObject;
        public GameObject TrajectoryReviewSliderObject;
        public Slider TrajectoryReviewSlider;
        public GameObject ExecuteTrajectoryButtonObject;

        public TMP_Text ActiveRobotText;
        public TMP_Dropdown RobotSelectionDropdown;
        public GameObject SetActiveRobotToggleObject;

        //Object Colors
        private Color Yellow = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        private Color White = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color TranspWhite = new Color(1.0f, 1.0f, 1.0f, 0.4f);
        private Color TranspGrey = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.4f);

        //Parent Objects for gameObjects
        public GameObject Elements;
        public GameObject QRMarkers;
        public GameObject UserObjects;

        //AR Camera and Touch GameObjects & Occlusion Objects
        public Camera arCamera;
        private GameObject activeGameObject;

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        private GameObject activeJointGameObject;

        private GameObject temporaryObject; 
        private ARRaycastManager rayManager;
        public CompasXR.Systems.OperatingSystem currentOperatingSystem;
        private AROcclusionManager occlusionManager;
        private GameObject OcclusionToggleObject;

        //On Screen Text
        public GameObject CurrentStepTextObject;
        public GameObject EditorSelectedTextObject;
        public TMP_Text CurrentStepText;
        public TMP_Text LastBuiltIndexText;
        public TMP_Text CurrentPriorityText;
        public TMP_Text EditorSelectedText;

        //In script use variables
        public string CurrentStep = null;
        public string SearchedElementStepID;
        public string SelectedPriority = "None";
        public bool IDTagIsOffset = false;
        public bool PriorityTagIsOffset = false;

        /////////////////////////////////// Monobehaviour Methods ///////////////////////////////////////////////////////////        
        void Start()
        {
            /*
            * Start : Method is used to initialize the UI elements and set up the UI Object & Script Dependencies on start.
            */
            OnAwakeInitilization();
        }
        void Update()
        {
            /*
            * Update : Method is used to update the UI elements and check for touch option activation.
            */
            TouchSearchControler();
        }

        /////////////////////////////////// UI Control & OnStart methods ////////////////////////////////////////////////////
        private void OnAwakeInitilization()
        {
            /*
            * OnAwakeInitilization : Method is used to initialize the UI elements and set up the 
            * UI Objects, Script Dependencies, & set up relationships on start.
            */

            //Find Other Scripts
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            eventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
            mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
            trajectoryVisualizer = GameObject.Find("TrajectoryVisualizer").GetComponent<TrajectoryVisualizer>();
            rosConnectionManager = GameObject.Find("RosManager").GetComponent<RosConnectionManager>();
            scrollSearchManager = GameObject.Find("ScrollSearchManager").GetComponent<ScrollSearchManager>();

            //Find Global use GameObjects
            Elements = GameObject.Find("Elements");
            QRMarkers = GameObject.Find("QRMarkers");
            CanvasObject = GameObject.Find("Canvas");
            UserObjects = GameObject.Find("ActiveUserObjects");     

            //Find AR and system management items
            arCamera = GameObject.Find("XR Origin").FindObject("Camera Offset").FindObject("Main Camera").GetComponent<Camera>();
            rayManager = FindObjectOfType<ARRaycastManager>();
            currentOperatingSystem = OperatingSystemManager.GetCurrentOS();

            //Find Constant UI Pannel
            ConstantUIPanelObjects = GameObject.Find("ConstantUIPanel");
        
            //Set up UI Objects and buttons on start
            SetPrimaryUIItemsOnStart();
            SetVisualizerMenuItemsOnStart();
            SetMenuItemsOnStart();
            SetCommunicationItemsOnStart();
        }
        private void SetPrimaryUIItemsOnStart()
        {
            /*
            * SetPrimaryUIItemsOnStart : Method is used to set up the primary UI elements on start.
            * Primary UI elements constitute the UI elements that are constantly on the screen
            * & control basic fundimental functionalities of the application.
            */
            
            //Find OnScreen UI Objects
            UserInterface.FindButtonandSetOnClickAction(ConstantUIPanelObjects, ref NextGeometryButtonObject, "Next_Geometry", NextStepButton);
            UserInterface.FindButtonandSetOnClickAction(ConstantUIPanelObjects, ref PreviousGeometryButtonObject, "Previous_Geometry", PreviousStepButton);
            UserInterface.FindSliderandSetOnValueChangeAction(CanvasObject, ref PreviewGeometrySliderObject, ref PreviewGeometrySlider, "GeometrySlider", PreviewGeometrySliderSetVisibilty);
            IsBuiltPanelObjects = ConstantUIPanelObjects.FindObject("IsBuiltPanel"); 
            UserInterface.FindButtonandSetOnClickAction(IsBuiltPanelObjects, ref IsBuiltButtonObject, "IsBuiltButton", () => ModifyStepBuildStatus(CurrentStep));
            IsbuiltButtonImage = IsBuiltButtonObject.FindObject("Image");
            UserInterface.FindToggleandSetOnValueChangedAction(CanvasObject, ref MenuButtonObject, "Menu_Toggle", ToggleMenu);
            UserInterface.FindToggleandSetOnValueChangedAction(CanvasObject, ref VisibilityMenuObject, "Visibility_Editor", ToggleVisibilityMenu);

            //Find Text Objects
            CurrentStepTextObject = GameObject.Find("Current_Index_Text");
            CurrentStepText = CurrentStepTextObject.GetComponent<TMPro.TMP_Text>();
            GameObject LastBuiltIndexTextObject = GameObject.Find("LastBuiltElement_Text");
            LastBuiltIndexText = LastBuiltIndexTextObject.GetComponent<TMPro.TMP_Text>();
            GameObject CurrentPriorityTextObject = GameObject.Find("CurrentPriority_Text");
            CurrentPriorityText = CurrentPriorityTextObject.GetComponent<TMPro.TMP_Text>();
            EditorSelectedTextObject = CanvasObject.FindObject("Editor_Selected_Text");
            EditorSelectedText = EditorSelectedTextObject.GetComponent<TMPro.TMP_Text>();
            
            //Find Background Images for Toggles
            VisualzierBackground = VisibilityMenuObject.FindObject("Background_Visualizer");
            MenuBackground = MenuButtonObject.FindObject("Background_Menu");

            //Find OnScreeen Message Prefabs
            MessagesParent = CanvasObject.FindObject("OnScreenMessages");
            OnScreenErrorMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenErrorMessagePrefab");
            OnScreenInfoMessagePrefab = MessagesParent.FindObject("Prefabs").FindObject("OnScreenInfoMessagePrefab");

            //OnScreen Messages with custom acknowledgement events.
            ActiveRobotUpdatedFromPlannerMessageObject = MessagesParent.FindObject("Prefabs").FindObject("ActiveRobotUpdatedFromPlannerMessage");
            TrajectoryReviewRequestMessageObject = MessagesParent.FindObject("Prefabs").FindObject("TrajectoryReviewRequestReceivedMessage");
        }
        private void SetVisualizerMenuItemsOnStart()
        {
            /*
            * SetVisualizerMenuItemsOnStart : Method is used to set up the Visualizer Menu UI elements on start.
            * Visualizer Menu UI elements constitute the UI elements that are used to control various visualization
            * functionalities of the application.
            */

            //Find Visualizer Menu Objects
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref PreviewActorToggleObject, "PreviewActorToggle", TogglePreviewActor);
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref IDToggleObject, "ID_Toggle", ToggleID);
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref RobotToggleObject, "RobotToggle", ToggleRobot);
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref ScrollSearchToggleObject, "ScrollSearchToggle", ToggleScrollSearch);
            ScrollSearchObjects = ScrollSearchToggleObject.FindObject("ScrollSearchObjects");

            //Find Robot toggle and Objects
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref PriorityViewerToggleObject, "PriorityViewer", TogglePriority);
            PriorityViewerBackground = PriorityViewerToggleObject.FindObject("BackgroundPriorityViewer");
            SelectedPriorityTextObject = PriorityViewerToggleObject.FindObject("SelectedPriorityText");
            SelectedPriorityText = SelectedPriorityTextObject.GetComponent<TMP_Text>();
            UserInterface.FindButtonandSetOnClickAction(PriorityViewerToggleObject, ref NextPriorityButtonObject, "NextPriorityButton", SetNextPriorityGroup);
            UserInterface.FindButtonandSetOnClickAction(PriorityViewerToggleObject, ref PreviousPriorityButtonObject, "PreviousPriorityButton", SetPreviousPriorityGroup);

            //Find Object Lengths Toggle and Objects
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref ObjectLengthsToggleObject, "ObjectLength_Button", ToggleObjectLengths);
            ObjectLengthsUIPanelObjects = CanvasObject.FindObject("ObjectLengthsPanel");
            ObjectLengthsUIPanelPosition = ObjectLengthsUIPanelObjects.transform.localPosition;
            ObjectLengthsText = ObjectLengthsUIPanelObjects.FindObject("LengthsText").GetComponent<TMP_Text>();
            ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            UserInterface.FindToggleandSetOnValueChangedAction(VisibilityMenuObject, ref JointsToggleObject, "JointsToggle", ToggleJoints);
        }
        private void SetMenuItemsOnStart()
        {
            /*
            * SetMenuItemsOnStart : Method is used to set up the Menu UI elements on start.
            * Menu UI elements constitute the UI elements that are used to control additional functionalities
            * of the application.
            */

            //Find Toggle Objects and set up on value changed actions
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref InfoToggleObject, "Info_Button", ToggleInfo);
            UserInterface.FindButtonandSetOnClickAction(MenuButtonObject, ref ReloadButtonObject, "Reload_Button", ReloadApplication);
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref CommunicationToggleObject, "Communication_Button", ToggleCommunication);
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref EditorToggleObject, "Editor_Toggle", ToggleEditor);
            UserInterface.FindButtonandSetOnClickAction(EditorToggleObject, ref BuilderEditorButtonObject, "Builder_Editor_Button", TouchModifyActor);
            UserInterface.FindButtonandSetOnClickAction(EditorToggleObject, ref BuildStatusButtonObject, "Build_Status_Editor", TouchModifyBuildStatus);
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref CommunicationToggleObject, "Communication_Button", ToggleCommunication);

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            UserInterface.FindToggleandSetOnValueChangedAction(MenuButtonObject, ref JointScaleToggleObject, "JointZoomToggle", ToggleJointScale);

            //Find Panel Objects used for Info and communication
            InfoPanelObject = CanvasObject.FindObject("InfoPanel");
            CommunicationPanelObject = CanvasObject.FindObject("CommunicationPanel");

            //Find Background Images for Toggles
            EditorBackground = EditorToggleObject.FindObject("Background_Editor");
        }
        private void SetCommunicationItemsOnStart()
        {
            /*
            * SetCommunicationItemsOnStart : Method is used to set up the Communication UI elements on start.
            * Communication UI elements constitute the UI elements that are used to control the communication
            * functionalities of the application. Such as Trajectory Review, Connection Management, & Robot Selection.
            */

            //Find Pannel Objects used for connecting to a different MQTT broker
            MqttBrokerInputField = CommunicationPanelObject.FindObject("MqttBrokerInputField").GetComponent<TMP_InputField>();
            MqttPortInputField = CommunicationPanelObject.FindObject("MqttPortInputField").GetComponent<TMP_InputField>();
            MqttUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsMQTTReconnectMessage");
            MqttConnectionStatusObject = CommunicationPanelObject.FindObject("MqttConnectionStatusObject");
            UserInterface.FindButtonandSetOnClickAction(CommunicationPanelObject, ref MqttConnectButtonObject, "MqttConnectButton", UpdateMqttConnectionFromUserInputs);

            //Find Pannel Objects used for connecting to a different ROS host
            RosHostInputField = CommunicationPanelObject.FindObject("ROSHostInputField").GetComponent<TMP_InputField>();
            RosPortInputField = CommunicationPanelObject.FindObject("ROSPortInputField").GetComponent<TMP_InputField>();
            RosUpdateConnectionMessage = CommunicationPanelObject.FindObject("UpdateInputsROSReconnectMessage");
            RosConnectionStatusObject = CommunicationPanelObject.FindObject("ROSConnectionStatusObject");
            UserInterface.FindButtonandSetOnClickAction(CommunicationPanelObject, ref RosConnectButtonObject, "ROSConnectButton", UpdateRosConnectionFromUserInputs);

            //Find Control Objects and set up events
            GameObject TrajectoryControlObjects = GameObject.Find("TrajectoryReviewUIControls");
            ReviewTrajectoryObjects = TrajectoryControlObjects.FindObject("ReviewTrajectoryControls");

            //Find Object, request button and add event listner for on click method
            UserInterface.FindButtonandSetOnClickAction(TrajectoryControlObjects, ref RequestTrajectoryButtonObject, "RequestTrajectoryButton", RequestTrajectoryButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(ReviewTrajectoryObjects, ref ApproveTrajectoryButtonObject, "ApproveTrajectoryButton", ApproveTrajectoryButtonMethod);
            UserInterface.FindButtonandSetOnClickAction(ReviewTrajectoryObjects, ref RejectTrajectoryButtonObject, "RejectTrajectoryButton", RejectTrajectoryButtonMethod);
            UserInterface.FindSliderandSetOnValueChangeAction(ReviewTrajectoryObjects, ref TrajectoryReviewSliderObject, ref TrajectoryReviewSlider, "TrajectoryReviewSlider", TrajectorySliderReviewMethod);
            UserInterface.FindButtonandSetOnClickAction(TrajectoryControlObjects, ref ExecuteTrajectoryButtonObject, "ExecuteTrajectoryButton", ExecuteTrajectoryButtonMethod);

            ActiveRobotText = CanvasObject.FindObject("CurrentRobotText").GetComponent<TMP_Text>();
        }
        public void SetOcclusionFromOS(ref AROcclusionManager occlusionManager, CompasXR.Systems.OperatingSystem currentOperatingSystem)
        {
            /*  
            * Method used to set the occlusion manager based on the current operating system.
            * This method is used to enable occlusion on iOS devices and disable it on other devices.
            */
            if(currentOperatingSystem == CompasXR.Systems.OperatingSystem.iOS)
            {
                occlusionManager = FindObjectOfType<AROcclusionManager>(true);
                occlusionManager.enabled = true;

                Debug.Log("AROcclusion: will be activated because current platform is ios");
            }
            else
            {
                Debug.Log("AROcclusion: will not be activated because current system is not ios");
            }
        }

        /////////////////////////////////////// Primary UI Functions //////////////////////////////////////////////
        public void ToggleVisibilityMenu(Toggle toggle)
        {
            /*
            * Method is used to toggle the visibility of the Visualizer Menu items.
            */
            if (VisualzierBackground != null && PreviewActorToggleObject != null && RobotToggleObject != null && ObjectLengthsToggleObject != null && IDToggleObject != null && PriorityViewerToggleObject != null)
            {    
                if (toggle.isOn)
                {             
                    VisualzierBackground.SetActive(true);
                    PreviewActorToggleObject.SetActive(true);
                    RobotToggleObject.SetActive(true);
                    ObjectLengthsToggleObject.SetActive(true);
                    IDToggleObject.SetActive(true);
                    PriorityViewerToggleObject.SetActive(true);
                    ScrollSearchToggleObject.SetActive(true);

                    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                    JointsToggleObject.SetActive(true);

                    UserInterface.SetUIObjectColor(VisibilityMenuObject, Yellow);

                }
                else
                {
                    VisualzierBackground.SetActive(false);
                    PreviewActorToggleObject.SetActive(false);
                    RobotToggleObject.SetActive(false);
                    ObjectLengthsToggleObject.SetActive(false);
                    IDToggleObject.SetActive(false);
                    PriorityViewerToggleObject.SetActive(false);
                    ScrollSearchToggleObject.SetActive(false);

                    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                    JointsToggleObject.SetActive(false);
                    
                    
                    UserInterface.SetUIObjectColor(VisibilityMenuObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find one of the buttons in the Visualizer Menu.");
            }   
        }
        public void ToggleMenu(Toggle toggle)
        {
            /*
            * Method is used to toggle the visibility of the Menu items.
            */
            if (MenuBackground != null && InfoToggleObject != null && ReloadButtonObject != null && CommunicationToggleObject != null && EditorToggleObject != null)
            {    
                if (toggle.isOn)
                {             
                    MenuBackground.SetActive(true);
                    InfoToggleObject.SetActive(true);
                    ReloadButtonObject.SetActive(true);
                    CommunicationToggleObject.SetActive(true);
                    EditorToggleObject.SetActive(true);
                    
                    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                    JointScaleToggleObject.SetActive(true);

                    UserInterface.SetUIObjectColor(MenuButtonObject, Yellow);

                }
                else
                {
                    if(EditorToggleObject.GetComponent<Toggle>().isOn){
                        EditorToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    if(InfoToggleObject.GetComponent<Toggle>().isOn){
                        InfoToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    if(CommunicationToggleObject.GetComponent<Toggle>().isOn){
                        CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    
                    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                    JointScaleToggleObject.SetActive(false);
                    if(JointScaleToggleObject.GetComponent<Toggle>().isOn){
                        JointScaleToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    MenuBackground.SetActive(false);
                    InfoToggleObject.SetActive(false);
                    ReloadButtonObject.SetActive(false);
                    CommunicationToggleObject.SetActive(false);
                    EditorToggleObject.SetActive(false);
                    UserInterface.SetUIObjectColor(MenuButtonObject, White);
                }
            }
            else
            {
                Debug.LogWarning("Could not find one of the buttons in the Menu.");
            }   
        }
        public void NextStepButton()
        {
            /*
            * Method is used to move to the next step in the building plan.
            */
            if(CurrentStep != null)
            {
                int CurrentStepInt = Convert.ToInt16(CurrentStep);
                if(CurrentStepInt < databaseManager.BuildingPlanDataItem.steps.Count - 1)
                {
                    SetCurrentStep((CurrentStepInt + 1).ToString());
                }  
            }
        }
        public void SetCurrentStep(string key)
        {
            /*
            * Method is used to set the current step in the building plan.
            * This method is really used to do a lot of UI control.
            * It handles the coloring, control, and coordination of many items
            * both in the UI and in AR space.
            */

            //If the current step is not null, remove the arrow from the previous step
            if(CurrentStep != null)
            {
                ObjectInstantiaion.DestroyGameObjectByName($"{CurrentStep} Arrow");
                GameObject previousStepElement = Elements.FindObject(CurrentStep);

                if(previousStepElement != null)
                {
                    Step PreviousStep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
                    string elementID = PreviousStep.data.element_ids[0];
                    instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, PreviousStep, key, previousStepElement.FindObject(elementID + " Geometry"));
                    if (PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                    {
                        instantiateObjects.ColorObjectByPriority(SelectedPriority, PreviousStep.data.priority.ToString(), CurrentStep, previousStepElement.FindObject(elementID + " Geometry"));
                    }
                }
            }

            //Set the current step to the new key
            CurrentStep = key;
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            GameObject element = Elements.FindObject(key);
            if(element != null)
            {
                instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, element.FindObject(step.data.element_ids[0] + " Geometry"));
                Debug.Log($"SetCurrentStep: Current Step is now {CurrentStep}");
            }
            CurrentStepText.text = CurrentStep;
            
            //Write current step information to the database
            instantiateObjects.UserIndicatorInstantiator(ref instantiateObjects.MyUserIndacator, element, CurrentStep, CurrentStep, "ME", 0.25f);
            UserCurrentInfo userCurrentInfo = new UserCurrentInfo();
            userCurrentInfo.currentStep = CurrentStep;
            userCurrentInfo.timeStamp = (System.DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy HH:mm:ss"));
            databaseManager.UserCurrentStepDict[SystemInfo.deviceUniqueIdentifier] = userCurrentInfo;
            DataHandlers.PushStringDataToDatabaseReference(databaseManager.dbReferenceUsersCurrentSteps.Child(SystemInfo.deviceUniqueIdentifier), JsonConvert.SerializeObject(userCurrentInfo));

            //Check other additional UI elements and set them  based on their toggle status
            if(ObjectLengthsToggleObject.GetComponent<Toggle>().isOn)
            {
                instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
            }
            if(RobotToggleObject.GetComponent<Toggle>().isOn)
            {
                SetRequestUIFromKey(CurrentStep);
                if(trajectoryVisualizer.ActiveRobot != null)
                {
                    if(!trajectoryVisualizer.ActiveRobot.activeSelf)
                    {
                        trajectoryVisualizer.ActiveRobot.SetActive(true);
                    }
                    SetRobotObjectFromStep(CurrentStep);
                    SetActiveRobotTextFromKey(CurrentStep);
                }
                else
                {
                    Debug.LogWarning("SetCurrentStep: Active Robot is null.");
                }

                if(trajectoryVisualizer.ActiveTrajectoryParentObject.transform.childCount > 0)
                {
                    trajectoryVisualizer.DestroyActiveTrajectoryChildren();
                }
            }

            if(PreviewGeometrySlider.value != 1)
            {
                PreviewGeometrySliderSetVisibilty(PreviewGeometrySlider.value);
            }

            IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
        }
        public void PreviousStepButton()
        {
            /*
            * Method is used to move to the previous step in the building plan on button press.
            */
            if(CurrentStep != null)
            {
                int CurrentStepInt = Convert.ToInt16(CurrentStep);
                if(CurrentStepInt > 0)
                {
                    SetCurrentStep((CurrentStepInt - 1).ToString());
                }  
            }       

        }
        public void PreviewGeometrySliderSetVisibilty(float value)
        {
            /*
            * Method is used to set the visibility of geometry in the scene based on the slider value.
            */
            if (CurrentStep != null)
            {
                int min = Convert.ToInt16(CurrentStep);
                float SliderValue = value;
                int ElementsTotal = databaseManager.BuildingPlanDataItem.steps.Count;
                float SliderMax = 1;
                float SliderMin = 0;
                float SliderRemaped = HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, min, ElementsTotal); 

                foreach(int index in Enumerable.Range(min, ElementsTotal))
                {
                    string elementName = index.ToString();
                    int InstanceNumber = Convert.ToInt16(elementName);
                    GameObject element = Elements.FindObject(elementName);
                    if (element != null)
                    {
                        if (InstanceNumber > SliderRemaped)
                        {
                            element.SetActive(false); 
                        }
                        else
                        {
                            element.SetActive(true);
                        }
                    }
                }
            }

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            if(JointsToggleObject.GetComponent<Toggle>().isOn)
            {
                instantiateObjects.SetAllJointsVisibilityFromAdjacency();
            }
        }
        public void IsBuiltButtonGraphicsControler(bool builtStatus, int stepPriority)
        {
            /*
            * Method is used to control the graphics of the is built button based on the built status of the step.
            */
            if (IsBuiltPanelObjects.activeSelf)
            {
                if (builtStatus)
                {
                    IsbuiltButtonImage.SetActive(true);
                    IsBuiltButtonObject.GetComponent<Image>().color = TranspGrey;
                }
                else
                {
                    IsbuiltButtonImage.SetActive(false);
                    IsBuiltButtonObject.GetComponent<Image>().color = TranspWhite;
                }
            }
        }
        public bool LocalPriorityChecker(Step step)
        {
            /*
            * Method is used to check the priority of the step and determine if the step can be built.
            * This method is used to check the priority of the step and determine if the step can be built
            * based on the current set priority and the step of the priority attempting to be built.
            */
            if(databaseManager.CurrentPriority == null)
            {
                Debug.LogError("LocalPriorityChecker: Current Priority is null.");
                return false;
            }
            else if (databaseManager.CurrentPriority == step.data.priority.ToString())
            {
                return true;
            }
            else if (Convert.ToInt16(databaseManager.CurrentPriority) > step.data.priority) //TODO: THIS ONLY WORKS BECAUSE WE PUSH EVERYTHING.
            {
                for(int i = Convert.ToInt16(step.data.priority) + 1; i < databaseManager.BuildingPlanDataItem.PriorityTreeDictionary.Count; i++)
                {
                    List<string> PriorityDataItem = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[i.ToString()];
                    foreach(string key in PriorityDataItem)
                    {
                        Step stepToUnbuild = databaseManager.BuildingPlanDataItem.steps[key];
                        if(stepToUnbuild.data.is_built)
                        {                        
                            stepToUnbuild.data.is_built = false;
                        }
                        instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, stepToUnbuild, key, Elements.FindObject(key).FindObject(stepToUnbuild.data.element_ids[0] + " Geometry"));
                    }
                }
                return true;
            }
            else
            {
                if(step.data.priority != Convert.ToInt16(databaseManager.CurrentPriority) + 1)
                {
                    string message = $"WARNING: This elements priority is incorrect. It is priority {step.data.priority.ToString()} and next priority to build is {Convert.ToInt16(databaseManager.CurrentPriority) + 1}";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PriorityIncorrectWarningMessageObject, "PriorityIncorrectWarningMessage", MessagesParent, message, "LocalPriorityChecker: Priority Incorrect Warning");
                    return false;
                }
                else
                {   
                    List<string> UnbuiltElements = new List<string>();
                    List<string> PriorityDataItem = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[databaseManager.CurrentPriority];

                    foreach(string element in PriorityDataItem)
                    {
                        Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[element];
                        if(!stepToCheck.data.is_built)
                        {
                            UnbuiltElements.Add(element);
                        }
                    }
                    if(UnbuiltElements.Count == 0)
                    {
                        Debug.Log($"Priority Check: Current Priority is complete. Unlocking Next Priority.");
                        string message = $"The previous priority {databaseManager.CurrentPriority} is complete you are now moving on to priority {step.data.priority.ToString()}.";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenInfoMessagePrefab, ref PriorityCompleteMessageObject, "PriorityCompleteMessage", MessagesParent, message, "LocalPriorityChecker: Priority Complete Message");
                        SetCurrentPriority(step.data.priority.ToString());

                        if(databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.priority.ToString() == databaseManager.CurrentPriority)
                        {    
                            IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
                        }
                        return false;
                    }
                    else
                    {
                        string message = $"WARNING: This element cannot build because the following elements from Current Priority {databaseManager.CurrentPriority} are not built: {string.Join(", ", UnbuiltElements)}";
                        UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref PriorityIncompleteWarningMessageObject, "PriorityIncompleteWarningMessage", MessagesParent, message, "LocalPriorityChecker: Priority Incomplete Warning");
                        return false;
                    }
                }
            }
        }
        public void ModifyStepBuildStatus(string key)
        {
            /*
            * Method is used to modify the build status of the step based on the key.
            * If the step is being unbuilt, it will unbuild all steps of a higher priority,
            * and it will prevent building of steps with higher priority then the current priority.
            */
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            if (LocalPriorityChecker(step))
            {
                if(step.data.is_built)
                {
                    step.data.is_built = false;
                    int StepInt = Convert.ToInt16(key);
                    for(int i = StepInt; i >= 0; i--)
                    {
                        Step stepToCheck = databaseManager.BuildingPlanDataItem.steps[i.ToString()];
                        if(StepInt == 0)
                        {
                            SetCurrentPriority(stepToCheck.data.priority.ToString());
                            break;   
                        }
                        if(stepToCheck.data.is_built)
                        {
                            databaseManager.BuildingPlanDataItem.LastBuiltIndex = i.ToString();
                            SetLastBuiltText(i.ToString());
                            SetCurrentPriority(stepToCheck.data.priority.ToString());
                            break;
                        }
                    }
                }
                else
                {
                    step.data.is_built = true;
                    databaseManager.BuildingPlanDataItem.LastBuiltIndex = key;
                    SetLastBuiltText(key);
                    SetCurrentPriority(step.data.priority.ToString());
                }

                instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
                if(key == CurrentStep)
                {    
                    IsBuiltButtonGraphicsControler(step.data.is_built, step.data.priority);
                }
                databaseManager.PushAllDataBuildingPlan(key);

                //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                instantiateObjects.ColorAllJointsByBuildState();
            }
            else
            {
                Debug.Log("ModifyStepBuildStatus: Priority Check will not allow this step to be built.");
            }
        }
        public void SetLastBuiltText(string key)
        {
            /*
            * Method is used to set the last built text on the screen.
            */
            LastBuiltIndexText.text = $"Last Built Element : {key}";
        }
        public void SetCurrentPriority(string Priority)
        {        
            /*
            * Method is used to set the current priority of the building plan.
            * This method is used to set the current priority of the building plan
            * and update the UI elements based on the current priority.
            */
            if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn && databaseManager.CurrentPriority != Priority)
            {
                instantiateObjects.ApplyColorBasedOnPriority(Priority);
                instantiateObjects.CreatePriorityViewerItems(Priority, ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.03f, 0.125f, Color.red, instantiateObjects.PriorityViewerPointsObject);
                SelectedPriority = Priority;
                PriorityViewerObjectsGraphicsController(true, Priority);
            }
            databaseManager.CurrentPriority = Priority;

            if(RobotToggleObject.GetComponent<Toggle>().isOn)
            {
                SetRequestUIFromKey(CurrentStep);
            }
            CurrentPriorityText.text = $"Current Priority : {Priority}";
            Debug.Log($"SetCurrentPriority: Current Priority set to {Priority} ");
        }
        public void RobotSelectionDropdownValueChanged(int dropDownValue)
        {
            /*
            * Method is used to set the active robot based on the dropdown value.
            * Additionally it controls UI elements based on the dropdown value.
            */
            Debug.Log($"RobotSelectionDropdownValueChanged: Robot Selection Dropdown Value Changed to {dropDownValue}. Setting Current Active Robot to False.");
            SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
        }

        /////////////////////////////////////// On Screen Message Functions //////////////////////////////////////////////
        public void SignalTrajectoryReviewRequest(string key, string robotName, Action visualizeRobotMethod)
        {
            /*
            * Method is used to signal a trajectory review request from another user.
            * This method is used for a custom review request because it has more requirements then message requests.
            * and set up the UI elements to acknowledge the request.
            */
            Debug.Log($"Trajectory Review Request: Other User is Requesting review of Trajectory for Step {key} .");
            TMP_Text messageComponent = TrajectoryReviewRequestMessageObject.FindObject("MessageText").GetComponent<TMP_Text>();

            string message = $"REQUEST : Trajectory Review requested for step: {key} with Robot: {robotName}.";
            
            if(TransactionLockActiveWarningMessageObject != null && TransactionLockActiveWarningMessageObject.activeSelf)
            {
                TransactionLockActiveWarningMessageObject.SetActive(false);
            }

            if(messageComponent != null && message != null && TrajectoryReviewRequestMessageObject != null)
            {
                UserInterface.SignalOnScreenMessageWithButton(TrajectoryReviewRequestMessageObject, messageComponent, message);
            }
            else
            {
                Debug.LogWarning("Trajectory Review Request Message: Could not find message object or message component.");
            }

            GameObject AcknowledgeButton = TrajectoryReviewRequestMessageObject.FindObject("AcknowledgeButton");
            if (AcknowledgeButton!= null && AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() <= 1)
            {
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => SetCurrentStep(key));
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => 
                {
                    if(!RobotToggleObject.GetComponent<Toggle>().isOn)
                    {
                        RobotToggleObject.GetComponent<Toggle>().isOn = true;
                    }
                });
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => visualizeRobotMethod());
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => TrajectoryServicesUIControler(false, false, true, true, false, false));
            }
        }
        public void SignalActiveRobotUpdateFromPlanner(string key, string robotName, string activeRobotName, Action visualizeRobotMethod)
        {
            /*
            * Method is used to signal an active robot update from another user on message request.
            * This method is used for a custom active robot update because it has more requirements then message requests.
            * and set up the UI elements to acknowledge the request.
            */
            Debug.Log($"SignalActiveRobotUpdateFromPlanner: Other User is Requesting review of Trajectory for Step {key} .");
            TMP_Text messageComponent = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("MessageText").GetComponent<TMP_Text>();
            string message = $"WARNING: You requested for {activeRobotName} but reply Trajectory is for {robotName}. ACTIVE ROBOT UPDATED.";
            int robotSelection = RobotSelectionDropdown.options.FindIndex(option => option.text == robotName);

            if(robotSelection != -1)
            {            
                if(SetActiveRobotToggleObject.GetComponent<Toggle>().isOn)
                {
                    SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = false;
                }
                RobotSelectionDropdown.value = robotSelection;
                SetActiveRobotToggleObject.GetComponent<Toggle>().isOn = true;
            }
            else
            {
                Debug.LogError("SignalActiveRobotUpdateFromPlanner: Could not find robot in dropdown options.");
            }

            if(messageComponent != null && message != null && ActiveRobotUpdatedFromPlannerMessageObject != null)
            {
                UserInterface.SignalOnScreenMessageWithButton(ActiveRobotUpdatedFromPlannerMessageObject, messageComponent, message);
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Could not find message object or message component.");
            }

            GameObject AcknowledgeButton = ActiveRobotUpdatedFromPlannerMessageObject.FindObject("AcknowledgeButton");
            if (AcknowledgeButton!= null && AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() <= 1)
            {
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => visualizeRobotMethod());
                AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => TrajectoryServicesUIControler(false, false, true, true, false, false));
            }
            else
            {
                Debug.LogWarning("SignalActiveRobotUpdateFromPlanner: Something Is messed up with on click event listner.");
            }

        }
        public void SignalMQTTConnectionFailed()
        {
            /*
            * Method is used to signal a MQTT connection failure to the user.
            * and set up the UI elements to acknowledge the request.
            */
            Debug.LogWarning("MQTT: MQTT Connection Failed.");
            if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
            {
                CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
            }
            string message = $"WARNING: MQTT Failed to connect to broker: {mqttTrajectoryManager.brokerAddress} on port: {mqttTrajectoryManager.brokerPort}. Please check your internet and try again.";
            UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MQTTFailedToConnectMessageObject, "MQTTConnectionFailedMessage", MessagesParent, message, "SignalMQTTConnectionFailed: MQTT Connection Failed.");
        }

        /////////////////////////////////////// Communication Buttons //////////////////////////////////////////////
        public void UpdateConnectionStatusText(GameObject connectionStatusObject, bool connectionStatus)
        {
            /*
            * Method is used to update the connection status text based on the connection status of communication protocols.
            */
            TMP_Text connectionStatusText = connectionStatusObject.FindObject("StatusText").GetComponent<TMP_Text>();
            if(RosConnectionStatusObject == null)
            {
                Debug.LogWarning("ConnectionStatusText is null for " + connectionStatusObject.name);
            }

            if(connectionStatus)
            {
                connectionStatusText.text = "CONNECTED";
                connectionStatusText.color = Color.green;
            }
            else
            {
                connectionStatusText.text = "DISCONNECTED";
                connectionStatusText.color = Color.red;
            }
        }
        public void UpdateMqttConnectionFromUserInputs()
        {
            /*
            * Method is used to update the MQTT connection based on the user inputs.
            * This allows the user to input various MQTT broker and port addresses.
            * Allowing for custom and even encrypted connections.
            */
            UserInterface.SetUIObjectColor(MqttConnectButtonObject, White);
            string newMqttBroker = MqttBrokerInputField.text;
            if (string.IsNullOrWhiteSpace(newMqttBroker))
            {
                newMqttBroker = "broker.hivemq.com";
            }

            string newMqttPort = MqttPortInputField.text;
            if (string.IsNullOrWhiteSpace(newMqttPort))
            {
                newMqttPort = "1883";
            }

            if (newMqttBroker != mqttTrajectoryManager.brokerAddress || Convert.ToInt32(newMqttPort) != mqttTrajectoryManager.brokerPort)
            {
                mqttTrajectoryManager.RemoveConnectionEventListners();
                mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();
                mqttTrajectoryManager.brokerAddress = newMqttBroker;
                mqttTrajectoryManager.brokerPort = Convert.ToInt32(newMqttPort);
                mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();
            }
            else
            {
                Debug.Log("MQTT: Broker and Port are the same as the current one. Not updating connection.");
                MqttUpdateConnectionMessage.SetActive(true);
            }
        }
        public void UpdateRosConnectionFromUserInputs()
        {
            Debug.Log($"UpdateRosConnectionFromUserInputs: Attempting ROS Connection to ws://{RosHostInputField.text}:{RosPortInputField.text} from User Inputs.");
            UserInterface.SetUIObjectColor(RosConnectButtonObject, White);
            string rosHostInput = RosHostInputField.text;
            string rosPortInput = RosPortInputField.text;

            if (string.IsNullOrWhiteSpace(rosHostInput) || string.IsNullOrWhiteSpace(rosPortInput))
            {
                rosHostInput = "localhost";
                rosPortInput = "9090";
            }
            string newRosBridgeAddress = $"ws://{rosHostInput}:{rosPortInput}";

            Debug.Log("UpdateRosConnectionFromUserInputs: New ROS Bridge Address: " + newRosBridgeAddress);
            if (newRosBridgeAddress != rosConnectionManager.RosBridgeServerUrl || !rosConnectionManager.IsConnectedToRos)
            {
                if(rosConnectionManager.IsConnectedToRos)
                {
                    rosConnectionManager.RosSocket.Close();
                }
                rosConnectionManager.RosBridgeServerUrl = newRosBridgeAddress;
                rosConnectionManager.ConnectAndWait();
            }
            else
            {
                Debug.Log("UpdateRosConnectionFromUserInputs: ROS Host and Port are the same as our current and we are connected. Not updating connection.");
                RosUpdateConnectionMessage.SetActive(true);
            }
        }
        public void TrajectoryServicesUIControler(bool requestTrajectoryVisability, bool requestTrajectoryInteractable, bool trajectoryReviewVisibility, bool trajectoryReviewInteractable, bool executeTrajectoryVisability, bool executeTrajectoryInteractable)
        {
            /*
            * Method is used to control the UI elements of the trajectory services.
            * This method is used to control the visibility and interactibility of UI elements for trajectory services
            * based on the current service and the current user.
            */
            RequestTrajectoryButtonObject.SetActive(requestTrajectoryVisability);
            RequestTrajectoryButtonObject.GetComponent<Button>().interactable = requestTrajectoryInteractable;

            ReviewTrajectoryObjects.SetActive(trajectoryReviewVisibility);
            ApproveTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;
            RejectTrajectoryButtonObject.GetComponent<Button>().interactable = trajectoryReviewInteractable;

            ExecuteTrajectoryButtonObject.SetActive(executeTrajectoryVisability);
            ExecuteTrajectoryButtonObject.GetComponent<Button>().interactable = executeTrajectoryInteractable;

            if (executeTrajectoryInteractable)
            {
                RejectTrajectoryButtonObject.GetComponent<Button>().interactable = executeTrajectoryInteractable;
            }
            if ( trajectoryReviewVisibility || executeTrajectoryVisability)
            {
                RobotToggleObject.GetComponent<Toggle>().interactable = false;

                //Next and previous button not interactable based on service
                NextGeometryButtonObject.GetComponent<Button>().interactable = false;
                PreviousGeometryButtonObject.GetComponent<Button>().interactable = false;
                
            }
            else if (requestTrajectoryVisability)
            {
                RobotToggleObject.GetComponent<Toggle>().interactable = true;
                NextGeometryButtonObject.GetComponent<Button>().interactable = true;
                PreviousGeometryButtonObject.GetComponent<Button>().interactable = true;
            }
        }
        public void RequestTrajectoryButtonMethod()
        {
            /*
            * Method is used to request a trajectory for the current step.
            * This method is used to request a trajectory for the current step,
            * set UI elements and publish the request on the particular request topic.
            */
            Debug.Log($"RequestTrajectoryButtonMethod: Requesting Trajectory for Step {CurrentStep}");

            if(!mqttTrajectoryManager.mqttClientConnected)
            {
                Debug.Log("RequestTrajectoryButtonMethod : You are not connected to mqtt broker and canot send request.");
                string message = "WARNING: You are not connected to MQTT, therefore cannot send a trajectory request. Please restart the app or set connection info.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref MqttNotConnectedForRequestWarningMessageObject, "MqttNotConnectedOnRequestWarningMessageObject", MessagesParent, message, "RequestTrajectoryButtonMethod: MQTT Connection Incorrect.");
                return;
            }         
            
            if (mqttTrajectoryManager.serviceManager.TrajectoryRequestTransactionLock)
            {
                Debug.Log("RequestTrajectoryButtonMethod : You cannot request because transaction lock is active");
                string message = "WARNING: You are currently prevented from requesting because another active user is awaiting a Trajectory Result.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref OnScreenErrorMessagePrefab, ref TransactionLockActiveWarningMessageObject, "TransactionLockActiveWarningMessage", MessagesParent, message, "RequestTrajectoryButtonMethod: Transaction Lock Active Warning.");
                return;
            }
            else
            {    
                //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                string robotName = databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.robot_name;

                string topicToPublishOn;
                if(robotName == "AA")
                {
                    topicToPublishOn = mqttTrajectoryManager.compasXRTopics.publishers.getTrajectoryRequestTopicAA;
                }
                else
                {
                    topicToPublishOn = mqttTrajectoryManager.compasXRTopics.publishers.getTrajectoryRequestTopicAB;
                }

                if(topicToPublishOn == null)
                {
                    Debug.LogError("RequestTrajectoryButtonMethod: Topic to Publish On is null.");
                    return;
                }

                Debug.Log("Publishing Request to Topic: " + topicToPublishOn);
                
                mqttTrajectoryManager.PublishToTopic(topicToPublishOn, new GetTrajectoryRequest(CurrentStep, robotName).GetData());
                mqttTrajectoryManager.serviceManager.PrimaryUser = true;
                mqttTrajectoryManager.serviceManager.currentService = ServiceManager.CurrentService.GetTrajectory;
                TrajectoryServicesUIControler(true, false, false, false, false, false);
            }
        }
        public void ApproveTrajectoryButtonMethod()
        {
            /*
            * Method is used to approve a trajectory for the current step.
            * This method is used to approve a trajectory for the current step,
            * set UI elements and publish the approval on the particular approval topic.
            */
            Debug.Log($"ApproveTrajectoryButtonMethod: Approving Trajectory for Step {CurrentStep}");
            TrajectoryServicesUIControler(false, false, true, false, false, false);
            
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            string robotName = databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.robot_name; //TODO: Could be also mqttTrajectoryManager.serviceManager.LastGetTrajectoryRequest.elementID (instead of CurrentStep if error occurs)
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, robotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 1).GetData());
        }
        public void RejectTrajectoryButtonMethod()
        {
            /*
            * Method is used to reject a trajectory for the current step.
            * This method is used to reject a trajectory for the current step,
            * set UI elements and publish the rejection on the particular approval topic.
            */
            Debug.Log($"RejectTrajectoryButtonMethod: Rejecting Trajectory for Step {CurrentStep}");
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            string robotName = databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.robot_name; //TODO: Could be also mqttTrajectoryManager.serviceManager.LastGetTrajectoryRequest.elementID (instead of CurrentStep if error occurs)
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, robotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 0).GetData());
            TrajectoryServicesUIControler(false, false, true, false, false, false);
        }
        public void TrajectorySliderReviewMethod(float value)
        {
            if (mqttTrajectoryManager.serviceManager.CurrentTrajectory != null)
            {
                if (mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count > 0)
                {
                    float SliderValue = value;
                    int TrajectoryConfigurationsCount = mqttTrajectoryManager.serviceManager.CurrentTrajectory.Count; 
                    float SliderMax = 1;
                    float SliderMin = 0;
                    float SliderValueRemaped = HelpersExtensions.Remap(SliderValue, SliderMin, SliderMax, 0, TrajectoryConfigurationsCount-1); 
                    Debug.Log($"TrajectorySliderReviewMethod: Slider Value Changed is value {SliderValueRemaped} and the item is {JsonConvert.SerializeObject(mqttTrajectoryManager.serviceManager.CurrentTrajectory[(int)SliderValueRemaped])}"); //TODO:CHECK SLIDER REMAP
                    trajectoryVisualizer.ColorRobotConfigfromSliderInput((int)SliderValueRemaped, instantiateObjects.InactiveRobotMaterial, instantiateObjects.ActiveRobotMaterial,ref trajectoryVisualizer.previousSliderValue);
                }
                else
                {
                    Debug.Log("TrajectorySliderReviewMethod: Current Trajectory Count is 0.");
                }
            }
            else
            {
                Debug.Log("TrajectorySliderReviewMethod: Current Trajectory is null.");
            }
        }
        public void ExecuteTrajectoryButtonMethod()
        {
            /*
            * Method is used to execute a trajectory for the current step.
            * It sets UI elements and publish the execution on the particular approval topic to the CAD.
            */
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            string robotName = databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.robot_name; //TODO: Could be also mqttTrajectoryManager.serviceManager.LastGetTrajectoryRequest.elementID (instead of CurrentStep if error occurs)
            string topicToPublishOn;
            if(robotName == "AA")
            {
                topicToPublishOn = mqttTrajectoryManager.compasXRTopics.publishers.sendTrajectoryTopicAA;
            }
            else
            {
                topicToPublishOn = mqttTrajectoryManager.compasXRTopics.publishers.sendTrajectoryTopicAB;
            }

            if(topicToPublishOn == null)
            {
                Debug.LogError("ExecuteTrajectoryButtonMethod: Topic to Publish On is null.");
                return;
            }

            Debug.Log($"ExecuteTrajectoryButtonMethod: Executing Trajectory for Step {CurrentStep} by publishing to topic {topicToPublishOn}.");
            Dictionary<string, object> sendTrajectoryMessage = new SendTrajectory(CurrentStep, robotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory).GetData();
            mqttTrajectoryManager.PublishToTopic(topicToPublishOn, sendTrajectoryMessage);
            TrajectoryServicesUIControler(false, false, false, false, true, false);
            mqttTrajectoryManager.PublishToTopic(mqttTrajectoryManager.compasXRTopics.publishers.approveTrajectoryTopic, new ApproveTrajectory(CurrentStep, robotName, mqttTrajectoryManager.serviceManager.CurrentTrajectory, 2).GetData());

        }

        ////////////////////////////////////// Visualizer Menu Buttons ////////////////////////////////////////////
        public void TogglePreviewActor(Toggle toggle)
        {
            /*
            * Method is used to toggle the preview actor view in the scene.
            * Additionally it will color all elements in the scene based on their actor.
            */
            Debug.Log("TogglePreviewActor: Preview Builder Toggle Pressed value set to " + toggle.GetComponent<Toggle>().isOn);
            if(toggle.isOn)
            {
                instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.ActorView;
                instantiateObjects.ApplyColorBasedOnActor();
                UserInterface.SetUIObjectColor(PreviewActorToggleObject, Yellow);
            }
            else
            {
                instantiateObjects.visulizationController.VisulizationMode = VisulizationMode.BuiltUnbuilt;
                instantiateObjects.ApplyColorBasedOnAppModes();
                UserInterface.SetUIObjectColor(PreviewActorToggleObject, White);
            }
        }
        public void ToggleID(Toggle toggle)
        {
            /*
            * Method is used to toggle the ID tags in the scene.
            * Additionally it will reposition the ID tags based on priority viewer the toggle value.
            */
            Debug.Log("ToggleID: ID Toggle Pressed value set to " + toggle.GetComponent<Toggle>().isOn);
            if (toggle != null && IDToggleObject != null)
            {
                if(toggle.isOn)
                {
                    ARSpaceTextControler(true, "IdxText", ref IDTagIsOffset, "IdxImage", PriorityViewerToggleObject.GetComponent<Toggle>().isOn, 0.155f); //bool verticlReposition, float distance
                    UserInterface.SetUIObjectColor(IDToggleObject, Yellow);
                }
                else
                {
                    ARSpaceTextControler(false, "IdxText", ref IDTagIsOffset, "IdxImage");
                    if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn && PriorityTagIsOffset)
                    {
                        ARSpaceTextControler(true, "PriorityText", ref PriorityTagIsOffset, "PriorityImage");
                    }
                    UserInterface.SetUIObjectColor(IDToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleID: Could not find ID Toggle or ID Toggle Object.");
            }
        }
        public void ARSpaceTextControler(bool Visibility, string textObjectBaseName, ref bool tagIsOffset, string imageObjectBaseName = null, bool verticalReposition = false, float? verticalOffset = null)
        {
            /*
            * Method is used to control the visibility of AR Space Text in the scene.
            * Additionally it will reposition the AR Space Text based on the visibility and the vertical reposition value.
            */
            if (instantiateObjects != null && instantiateObjects.Elements != null)
            {
                foreach (Transform child in instantiateObjects.Elements.transform)
                {
                    Transform textChild = child.Find(child.name + textObjectBaseName);
                    if (textChild != null)
                    {
                        textChild.gameObject.SetActive(Visibility);
                    }
                    if (verticalReposition)
                    {
                        Vector3 objectposition = textChild.transform.position;
                        Vector3 newPosition = ObjectTransformations.OffsetPositionVectorByDistance(objectposition, verticalOffset.GetValueOrDefault(0.0f), "y");
                        textChild.position = newPosition;
                    }
                    else
                    {
                        HelpersExtensions.ObjectPositionInfo instantiationPosition = textChild.GetComponent<HelpersExtensions.ObjectPositionInfo>();
                        textChild.localPosition = instantiationPosition.position;
                    }
                    if(imageObjectBaseName != null)
                    {
                        Transform imageChild = child.Find(child.name + imageObjectBaseName);
                        if (imageChild != null)
                        {
                            imageChild.gameObject.SetActive(Visibility);
                        }

                        if (verticalReposition)
                        {
                            Vector3 objectposition = imageChild.transform.position;
                            Vector3 newPosition = ObjectTransformations.OffsetPositionVectorByDistance(objectposition, verticalOffset.GetValueOrDefault(0.0f), "y");
                            imageChild.position = newPosition;
                            tagIsOffset = true;
                        }
                        else
                        {
                            HelpersExtensions.ObjectPositionInfo instantiationPosition = imageChild.GetComponent<HelpersExtensions.ObjectPositionInfo>();
                            imageChild.localPosition = instantiationPosition.position;
                            tagIsOffset = false;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("ARSpaceTextControler: InstantiateObjects script or Elements object not set.");
            }
        }
        public void ToggleObjectLengths(Toggle toggle)
        {
            /*
            * Method is used to toggle the object lengths in the scene.
            * Additionally it will calculate the object lengths and set the P1 & P2 elements based on the current step.
            */
            Debug.Log($"ToggleObjectLengths: Object Lengths Toggle set to {toggle.GetComponent<Toggle>().isOn}");
            if (ObjectLengthsUIPanelObjects != null && ObjectLengthsText != null && ObjectLengthsTags != null)
            {    
                if (toggle.isOn)
                {
                    ObjectLengthsUIPanelObjects.SetActive(true);
                    ObjectLengthsTags.FindObject("P1Tag").SetActive(true);
                    ObjectLengthsTags.FindObject("P2Tag").SetActive(true);

                    if (CurrentStep != null)
                    {    
                        instantiateObjects.CalculateandSetLengthPositions(CurrentStep);
                    }
                    else
                    {
                        Debug.LogWarning("ToggleObjectLengths: Current Step is null.");
                    }
                    UserInterface.SetUIObjectColor(ObjectLengthsToggleObject, Yellow);

                }
                else
                {
                    ObjectLengthsUIPanelObjects.SetActive(false);
                    ObjectLengthsTags.FindObject("P1Tag").SetActive(false);
                    ObjectLengthsTags.FindObject("P2Tag").SetActive(false);
                    UserInterface.SetUIObjectColor(ObjectLengthsToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleObjectLengths: Could not find Object Lengths Objects.");
            }
        }
        public void SetObjectLengthsText(float P1distance, float P2distance)
        {
            /*
            * Method is used to set the object lengths text in the scene.
            */
            ObjectLengthsText.text = $"P1 | {(float)Math.Round(P1distance, 2)}     P2 | {(float)Math.Round(P2distance, 2)}";
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void SetObjectLengthsTextFromStoredKey(string key)
        {
            /*
            * Method is used to set the object lengths text in the scene.
            */
            List<float> objectLengths = instantiateObjects.ObjectLengthsDictionary[key];
            float p1Distance = (float)Math.Round(objectLengths[0], 2);
            float p2Distance = (float)Math.Round(objectLengths[1], 2);
            ObjectLengthsText.text = $"P1 | {p1Distance}     P2 | {p2Distance}";
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void ToggleRobot(Toggle toggle)
        {
            /*
            * Method is used to toggle the robot in the scene.
            * Additionally it will set the visibility of the robot and the UI elements based on the toggle value,
            * and step actor.
            */
            Debug.Log($"ToggleRobot: Robot Toggle Pressed value set to {toggle.GetComponent<Toggle>().isOn}");
            if(toggle.isOn && RequestTrajectoryButtonObject != null)
            {
                if(trajectoryVisualizer.ActiveRobot.transform.childCount > 0 && CurrentStep != null)
                {
                    SetRobotObjectFromStep(CurrentStep);
                    SetActiveRobotTextFromKey(CurrentStep);
                }
                else
                {
                    Debug.LogError("ToggleRobot: Active Robot or CurrentStep is null not setting any visibility.");
                }
                if(CurrentStep != null)
                {
                    SetRequestUIFromKey(CurrentStep);
                }
                else
                {
                    Debug.LogWarning("ToggleRobot: Current Step is null.");
                }
                UserInterface.SetUIObjectColor(RobotToggleObject, Yellow);
            }
            else
            {            
                if (RequestTrajectoryButtonObject.activeSelf)
                {
                    TrajectoryServicesUIControler(false, false, false, false, false, false);
                }
                            
                if(trajectoryVisualizer.ActiveRobotObjects.transform.childCount > 0)
                {
                    if(trajectoryVisualizer.ActiveRobot.activeSelf)
                    {
                        trajectoryVisualizer.ActiveRobot.SetActive(false);
                    }
                    else if(trajectoryVisualizer.ActiveTrajectoryParentObject.activeSelf)
                    {
                        trajectoryVisualizer.ActiveTrajectoryParentObject.SetActive(false);
                    }
                }
                UserInterface.SetUIObjectColor(RobotToggleObject, White);
            }
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void SetActiveRobotTextFromKey(string key)
        {
            /*
            * Method is used to set the active robot text in the scene based on the key.
            */

                Step step = databaseManager.BuildingPlanDataItem.steps[key];
         
                if(step.data.actor == "ROBOT")
                {
                    string robotName = step.data.robot_name;
                    ActiveRobotText.gameObject.SetActive(true);
                    ActiveRobotText.text = robotName;

                }
                else
                {
                    ActiveRobotText.gameObject.SetActive(false);
                }
        }
        public void SetRobotObjectFromStep(string key)
        {
            /*
            * Method is used to set the robot object in the scene based on the step information.
            */
            GameObject robotObjectAA = trajectoryVisualizer.ActiveRobot.FindObject("AA");
            GameObject robotObjectAB = trajectoryVisualizer.ActiveRobot.FindObject("AB");
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            string robotName = databaseManager.BuildingPlanDataItem.steps[CurrentStep].data.robot_name;

            GameObject zeroConfigurationRobot = trajectoryVisualizer.ActiveRobot.FindObject("RobotZero");
            if(zeroConfigurationRobot != null)
            {
                if(zeroConfigurationRobot.activeSelf)
                {
                    zeroConfigurationRobot.SetActive(false);
                }
            }

            if(step.data.actor == "ROBOT")
            {
                Frame robotAAFrame = step.data.robot_AA_base_frame;
                if(!robotObjectAA.activeSelf)
                {
                    robotObjectAA.SetActive(true);
                }
                URDFManagement.SetRobotLocalPositionandRotationFromFrame(robotAAFrame, robotObjectAA);

                Frame robotABFrame = step.data.robot_AB_base_frame;
                URDFManagement.SetRobotLocalPositionandRotationFromFrame(robotABFrame, robotObjectAB);
                if(!robotObjectAB.activeSelf)
                {
                    robotObjectAB.SetActive(true);
                }

                if(robotName == "AA")
                {
                    URDFManagement.ColorURDFGameObject(robotObjectAA, instantiateObjects.ActiveRobotMaterial, ref trajectoryVisualizer.URDFRenderComponents);
                    URDFManagement.ColorURDFGameObject(robotObjectAB, instantiateObjects.InactiveRobotMaterial, ref trajectoryVisualizer.URDFRenderComponents);
                }
                else
                {
                    URDFManagement.ColorURDFGameObject(robotObjectAA, instantiateObjects.InactiveRobotMaterial, ref trajectoryVisualizer.URDFRenderComponents);
                    URDFManagement.ColorURDFGameObject(robotObjectAB, instantiateObjects.ActiveRobotMaterial, ref trajectoryVisualizer.URDFRenderComponents);
                }
            }
            else
            {
                if(robotObjectAA.activeSelf)
                {
                    robotObjectAA.SetActive(false);
                }
                if(robotObjectAB.activeSelf)
                {
                    robotObjectAB.SetActive(false);
                }
            }
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void SetRequestUIFromKey(string key)
        {
            /*
            * Method is used to set the robotic UI elements based on the key.
            * Additionally it will set the visibility of the robot and the UI elements based on the key,
            * and step actor.
            */
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            if(step.data.actor == "ROBOT")
            {
                if (!step.data.is_built && step.data.priority.ToString() == databaseManager.CurrentPriority)
                {    
                    TrajectoryServicesUIControler(true, true, false, false, false, false);
                }
                else
                {
                    TrajectoryServicesUIControler(true, false, false, false, false, false);
                }
            }
            else
            {
                // RobotSelectionDropdownObject.SetActive(true);
                // SetActiveRobotToggleObject.SetActive(true);
                TrajectoryServicesUIControler(false, false, false, false, false, false);
            }
        }
        public void TogglePriority(Toggle toggle)
        {
            /*
            * Method is used to toggle the priority viewer in the scene.
            * Additionally it will set the visibility of the priority viewer UI elements and gameObjects based on the toggle value.
            */
            Debug.Log($"TogglePriority: Priority Toggle Pressed the value is now set to {toggle.GetComponent<Toggle>().isOn}");
            if(toggle.isOn && PriorityViewerToggleObject != null)
            {
                if(PreviewGeometrySliderObject.GetComponent<Slider>().value != 1)
                {
                    PreviewGeometrySliderObject.GetComponent<Slider>().value = 1;
                }
                
                ARSpaceTextControler(true, "PriorityText", ref PriorityTagIsOffset, "PriorityImage", IDToggleObject.GetComponent<Toggle>().isOn, 0.155f);
                instantiateObjects.PriorityViewrLineObject.SetActive(true);
                instantiateObjects.PriorityViewerPointsObject.SetActive(true);

                PriorityViewerObjectsGraphicsController(true, databaseManager.CurrentPriority);
                SelectedPriority = databaseManager.CurrentPriority;
                instantiateObjects.CreatePriorityViewerItems(databaseManager.CurrentPriority, ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.02f, 0.10f, Color.red, instantiateObjects.PriorityViewerPointsObject);
                instantiateObjects.ApplyColorBasedOnPriority(databaseManager.CurrentPriority);
                UserInterface.SetUIObjectColor(PriorityViewerToggleObject, Yellow);
            }
            else
            {
                instantiateObjects.ApplyColorBasedOnAppModes();
                instantiateObjects.PriorityViewrLineObject.SetActive(false);
                instantiateObjects.PriorityViewerPointsObject.SetActive(false);
                SelectedPriority = "None";
                ARSpaceTextControler(false, "PriorityText", ref PriorityTagIsOffset, "PriorityImage");

                if(IDToggleObject.GetComponent<Toggle>().isOn && IDTagIsOffset)
                {
                    ARSpaceTextControler(true, "IdxText", ref IDTagIsOffset, "IdxImage");
                }
                PriorityViewerObjectsGraphicsController(false);
                UserInterface.SetUIObjectColor(PriorityViewerToggleObject, White);
            }
        }
        public void PriorityViewerObjectsGraphicsController(bool? isVisible, string selectedPrioritytext=null)
        {
            /*
            * Method is used to control the updating of the priority viewer UI elements.
            */
            if(isVisible.HasValue)
            {
                NextPriorityButtonObject.SetActive(isVisible.Value);
                PreviousPriorityButtonObject.SetActive(isVisible.Value);
                SelectedPriorityTextObject.SetActive(isVisible.Value);
                PriorityViewerBackground.SetActive(isVisible.Value);
            }
            if(selectedPrioritytext != null)
            {
                SelectedPriorityText.text = selectedPrioritytext;
            }
        }
        public void SetNextPriorityGroup()
        {
            /*
            * Method is used to set the next priority group in the priority viewer.
            * Additionally it will color the elements of the next priority group and set the text based on the next priority group.
            */
            Debug.Log("SetNextPriorityGroup: Next Priority Button Pressed");
            if(SelectedPriority != "None")
            {
                int SelectedPriorityInt = Convert.ToInt16(SelectedPriority);
                int newPriorityGroupInt = SelectedPriorityInt + 1;

                if(newPriorityGroupInt <= databaseManager.BuildingPlanDataItem.PriorityTreeDictionary.Count - 1)
                {                
                    instantiateObjects.ApplyColortoPriorityGroup(SelectedPriorityInt.ToString(), newPriorityGroupInt.ToString());
                    instantiateObjects.CreatePriorityViewerItems(newPriorityGroupInt.ToString(), ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.02f, 0.10f, Color.red, instantiateObjects.PriorityViewerPointsObject);
                    instantiateObjects.ApplyColortoPriorityGroup(newPriorityGroupInt.ToString(), newPriorityGroupInt.ToString(), true);
                    PriorityViewerObjectsGraphicsController(true, newPriorityGroupInt.ToString());
                    SelectedPriority = (SelectedPriorityInt + 1).ToString();
                }
                else
                {
                    Debug.Log("SetNextPriorityGroup: We have reached the priority groups limit.");
                }
            }
            else
            {
                Debug.LogWarning("SetNextPriorityGroup: Selected Priority is null.");
            }
        }
        public void SetPreviousPriorityGroup()
        {
            /*
            * Method is used to set the previous priority group in the priority viewer.
            * Additionally it will color the elements of the previous priority group and set the text based on the previous priority group.
            */
            Debug.Log("SetPreviousPriorityGroup: setting the previous Priority group");
            if(SelectedPriority != "None")
            {
                int SelectedPriorityInt = Convert.ToInt16(SelectedPriority);
                int newPriorityGroupInt = SelectedPriorityInt - 1;

                if(newPriorityGroupInt >= 0)
                {
                    instantiateObjects.ApplyColortoPriorityGroup(SelectedPriorityInt.ToString(), newPriorityGroupInt.ToString());
                    instantiateObjects.CreatePriorityViewerItems(newPriorityGroupInt.ToString(), ref instantiateObjects.PriorityViewrLineObject, Color.red, 0.02f, 0.10f, Color.red, instantiateObjects.PriorityViewerPointsObject);
                    instantiateObjects.ApplyColortoPriorityGroup(newPriorityGroupInt.ToString(), newPriorityGroupInt.ToString(), true);
                    PriorityViewerObjectsGraphicsController(true, newPriorityGroupInt.ToString());
                    SelectedPriority = (SelectedPriorityInt - 1).ToString();
                }
                else
                {
                    Debug.Log("SetPreviousPriorityGroup: We have reached the zero priority group.");
                }
            }
            else
            {
                Debug.LogWarning("SetPreviousPriorityGroup: Selected Priority is null.");
            }
        }
        public void ToggleScrollSearch(Toggle toggle)
        {
            /*
            * Method is used to toggle the scroll search in the scene.
            * Additionally it will set the visibility of the scroll search UI elements based on the toggle value.
            */
            if (toggle.isOn)
            {             
                ScrollSearchObjects.SetActive(true);
                scrollSearchManager.CreateCellsFromPrefab(ref scrollSearchManager.cellPrefab, scrollSearchManager.cellSpacing, scrollSearchManager.cellsParent, databaseManager.BuildingPlanDataItem.steps.Count, ref scrollSearchManager.cellsExist);
                UserInterface.SetUIObjectColor(ScrollSearchToggleObject, Yellow);
            }
            else
            {
                ScrollSearchObjects.SetActive(false);
                scrollSearchManager.ResetScrollSearch(ref scrollSearchManager.cellsExist);
                UserInterface.SetUIObjectColor(ScrollSearchToggleObject, White);
            }
        }
        public void ToggleJoints(Toggle toggle)
        {
            /*
            * Method is used to toggle the joints in the scene.
            * Additionally it will set the visibility of the joints UI elements based on the toggle value.
            */
            Debug.Log($"ToggleJoints: Joints Toggle Pressed value set to {toggle.GetComponent<Toggle>().isOn}");
            if(toggle.isOn)
            {
                if(PreviewGeometrySlider.value != 1)
                {
                    instantiateObjects.SetAllJointsVisibilityFromAdjacency();
                }
                else
                {
                    instantiateObjects.SetVisibilityOfAllJoints(true);
                }

                UserInterface.SetUIObjectColor(JointsToggleObject, Yellow);
            }
            else
            {
                instantiateObjects.SetVisibilityOfAllJoints(false);
                UserInterface.SetUIObjectColor(JointsToggleObject, White);
            }
        }

        ////////////////////////////////////////// Menu Buttons ///////////////////////////////////////////////////
        private void ToggleInfo(Toggle toggle)
        {
            /*
            * Method is used to toggle the information panel in the scene.
            */
            if(InfoPanelObject != null)
            {
                Debug.Log($"ToggleInfo: Info Toggle Pressed value is now set to {toggle.GetComponent<Toggle>().isOn}");
                if (toggle.isOn)
                {             
                    if(CommunicationToggleObject.GetComponent<Toggle>().isOn)
                    {
                        CommunicationToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    InfoPanelObject.SetActive(true);
                    UserInterface.SetUIObjectColor(InfoToggleObject, Yellow);
                }
                else
                {
                    InfoPanelObject.SetActive(false);
                    UserInterface.SetUIObjectColor(InfoToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleInfo: Could not find Info Panel.");
            }
        }
        private void ToggleCommunication(Toggle toggle)
        {
            /*
            * Method is used to toggle the communication panel in the scene.
            */
            if(CommunicationPanelObject != null)
            {
                Debug.Log($"ToggleCommunication: Communication Toggle Pressed value is now {toggle.GetComponent<Toggle>().isOn}");
                if (toggle.isOn)
                {             
                    if(InfoToggleObject.GetComponent<Toggle>().isOn)
                    {
                        InfoToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    CommunicationPanelObject.SetActive(true);
                    UpdateConnectionStatusText(MqttConnectionStatusObject, mqttTrajectoryManager.mqttClientConnected);
                    UpdateConnectionStatusText(RosConnectionStatusObject, rosConnectionManager.IsConnectedToRos);
                    UserInterface.SetUIObjectColor(CommunicationToggleObject, Yellow);
                }
                else
                {
                    if(MqttUpdateConnectionMessage.activeSelf)
                    {
                        MqttUpdateConnectionMessage.SetActive(false);
                    }
                    
                    CommunicationPanelObject.SetActive(false);
                    UserInterface.SetUIObjectColor(CommunicationToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleCommunication: Could not find Communication Panel.");
            }
        }
        private void ReloadApplication()
        {
            /*
            * Method is used to reload the application.
            * This method will reset all current information and then pull all information again.
            */
            Debug.Log("ReloadApplication: Attempting to reload all information from the database");
            databaseManager.RemoveListners();
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            instantiateObjects.DestroyAllJoints();     
            databaseManager.JointsDataDict.Clear();
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

            if (Elements.transform.childCount > 0)
            {
                foreach (Transform child in Elements.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            if (QRMarkers.transform.childCount > 0)
            {        
                foreach (Transform child in QRMarkers.transform)
                {
                    child.transform.position = Vector3.zero;
                    child.transform.rotation = Quaternion.identity;
                }
            }

            if (UserObjects.transform.childCount > 0)
            {
                foreach (Transform child in UserObjects.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            databaseManager.BuildingPlanDataItem.steps.Clear();
            databaseManager.AssemblyDataDict.Clear();
            databaseManager.QRCodeDataDict.Clear();
            databaseManager.UserCurrentStepDict.Clear();
            databaseManager.BuildingPlanDataItem.PriorityTreeDictionary.Clear();

            mqttTrajectoryManager.UnsubscribeFromCompasXRTopics();
            mqttTrajectoryManager.RemoveConnectionEventListners();

            databaseManager.FetchSettingsData(eventManager.dbReferenceSettings);
            mqttTrajectoryManager.DisconnectandReconnectAsyncRoutine();

        }
        public void ToggleEditor(Toggle toggle)
        {
            /*
            * Method is used to toggle the editor panel in the scene.
            * Additionally it toggles touch input for objects in space.
            */
            if (EditorBackground != null && BuilderEditorButtonObject != null && BuildStatusButtonObject != null)
            {    
                Debug.Log($"ToggleEditor: Editor Toggle Pressed Value now set to {toggle.GetComponent<Toggle>().isOn}");
                if (toggle.isOn)
                {             
                    EditorBackground.SetActive(true);
                    BuilderEditorButtonObject.SetActive(true);
                    BuildStatusButtonObject.SetActive(true);
                    CurrentStepTextObject.SetActive(false);
                    EditorSelectedTextObject.SetActive(true);

                    if(JointScaleToggleObject.GetComponent<Toggle>().isOn)
                    {
                        JointScaleToggleObject.GetComponent<Toggle>().isOn = false;
                    }

                    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                    ControlAllCollidersInChildren(instantiateObjects.Joints, false);

                    TouchSearchModeController(TouchMode.ElementEditSelection);
                    UserInterface.SetUIObjectColor(EditorToggleObject, Yellow);
                }
                else
                {
                    EditorBackground.SetActive(false);
                    BuilderEditorButtonObject.SetActive(false);
                    BuildStatusButtonObject.SetActive(false);
                    EditorSelectedTextObject.SetActive(false);
                    CurrentStepTextObject.SetActive(true);

                    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                    ControlAllCollidersInChildren(instantiateObjects.Joints, true);

                    TouchSearchModeController(TouchMode.None);

                    if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.ActorView)
                    {
                        instantiateObjects.ApplyColorBasedOnActor();
                    }
                    else if(instantiateObjects.visulizationController.VisulizationMode == VisulizationMode.BuiltUnbuilt)
                    {
                        instantiateObjects.ApplyColorBasedOnBuildState();
                    }
                    else if(PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                    {
                        instantiateObjects.ApplyColorBasedOnPriority(SelectedPriority);
                    }
                    else
                    {
                        Debug.LogWarning("ToggleEditor: Could not find Visulization Mode.");
                    }
                    UserInterface.SetUIObjectColor(EditorToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleEditor: Could not find one of the buttons in the Editor Menu.");
            }
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void ToggleJointScale(Toggle toggle)
        {
            /*
            * Method used to touch scale a joint
            */
            if (JointScaleToggleObject != null)
            {    
                Debug.Log($"ToggleJointScale: Joint Zoom toggle Pressed value is now set to {toggle.GetComponent<Toggle>().isOn}");
                if (toggle.isOn)
                {             
                    //If the editor is on, turn it off
                    if(EditorToggleObject.GetComponent<Toggle>().isOn)
                    {
                        EditorToggleObject.GetComponent<Toggle>().isOn = false;
                    }
                    if(!JointsToggleObject.GetComponent<Toggle>().isOn)
                    {
                        JointsToggleObject.GetComponent<Toggle>().isOn = true;
                    }

                    //Disable the colliders for the elements
                    ControlAllCollidersInChildren(instantiateObjects.Elements, false);

                    //Set the touch mode to joint selection
                    TouchSearchModeController(TouchMode.JointSelection);

                    //Set the color of the joint scale toggle to yellow
                    UserInterface.SetUIObjectColor(JointScaleToggleObject, Yellow);
                }
                else
                {
                    //Set the touch mode to none
                    TouchSearchModeController(TouchMode.None);

                    //Enable the colliders for the elements
                    ControlAllCollidersInChildren(instantiateObjects.Elements, true);

                    //Set the color of the joint scale toggle to white
                    UserInterface.SetUIObjectColor(JointScaleToggleObject, White);
                }
            }
            else
            {
                Debug.LogWarning("ToggleEditor: Could not find one of the buttons in the Editor Menu.");
            }
        }
        public void ToggleAROcclusion(Toggle toggle)
        {
            /*
            * Method is used to toggle the AR Occlusion in the scene.
            */
            if (OcclusionToggleObject != null && occlusionManager != null)
            {
                Debug.Log("ToggleAROcclusion: Occlusion Toggle Pressed value is now set to " + toggle.GetComponent<Toggle>().isOn);
                if (toggle.isOn)
                { 
                    occlusionManager.enabled = true;            
                    UserInterface.SetUIObjectColor(OcclusionToggleObject, Yellow);
                }
                else
                {
                    occlusionManager.enabled = false;
                    UserInterface.SetUIObjectColor(OcclusionToggleObject, White);            
                }
            }
            else
            {
                Debug.LogWarning("ToggleAROcclusion: Could not find Occlusion Toggle Object.");
            }
        }

        ////////////////////////////////////////// Editor Buttons /////////////////////////////////////////////////
        public void TouchSearchModeController(TouchMode modetype)
        {
            /*
            * Method is used to control the touch search mode in the scene.
            * it will set the touch mode based on the input mode type.
            */

            instantiateObjects.visulizationController.TouchMode = modetype;

            if (modetype == TouchMode.ElementEditSelection)
            {
                Debug.Log ("***TouchMode: ELEMENT EDIT MODE***");
            }

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            else if(modetype == TouchMode.JointSelection)
            {
                Debug.Log ("***TouchMode: JOINT SELECTION MODE***");
            }
            else if(modetype == TouchMode.None)
            {
                if(activeGameObject != null)
                {    
                    DestroyBoundingBoxFixElementColor();
                    activeGameObject = null;
                    Debug.Log ("***TouchMode: NONE***");
                }

                if(activeJointGameObject != null)
                {
                    ObjectInstantiaion.DestroyGameObjectByName("ClonedJoint");
                    activeJointGameObject = null;
                    Debug.Log ("***TouchMode: NONE***");
                }
            }
            else
            {
                Debug.LogWarning("TouchSearchModeController: Could not find Touch Mode.");
            }

        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void ControlAllCollidersInChildren(GameObject parent, bool enable)
        {
            /*
            * Method is used to control all colliders in the children of a parent object.
            */
            foreach (Transform child in parent.transform)
            {
                Collider[] collider = child.GetComponentsInChildren<Collider>();
                foreach (Collider col in collider)
                {
                    col.enabled = enable;
                }
            }
        }
        private void TouchSearchControler()
        {
            /*
            Touch search controler is used to control the touch input modes for the application.
            */
            
            if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
            {
                SearchInput();
            }
            else if (instantiateObjects.visulizationController.TouchMode == TouchMode.JointSelection)
            {
                SearchInput();
            }
        }
        private void ColliderControler()
        {
            /*
            * Collider controler is used to control the collider for the elements in the scene.
            * It will enable or disable the collider based on the current step.
            * Additionally it will color elements that are not suppose to be interacted with.
            */
            Step Currentstep = databaseManager.BuildingPlanDataItem.steps[CurrentStep];
            for (int i =0 ; i < databaseManager.BuildingPlanDataItem.steps.Count; i++)
            {
                Step step = databaseManager.BuildingPlanDataItem.steps[i.ToString()];
                GameObject element = Elements.FindObject(i.ToString()).FindObject(step.data.element_ids[0] + " Geometry");
                Collider ElementCollider = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Collider>();
                Renderer ElementRenderer = element.FindObject(step.data.element_ids[0] + " Geometry").GetComponent<Renderer>();

                if(ElementCollider != null)
                {
                    if(step.data.priority == Currentstep.data.priority)
                    {
                        ElementCollider.enabled = true;
                    }
                    else
                    {
                        ElementCollider.enabled = false;
                        ElementRenderer.material = instantiateObjects.LockedObjectMaterial;
                    }
                }
            }
        }
        private GameObject SelectedObject(GameObject activeGameObject = null)
        {
            /*
            * Selected object is used to find the selected object in the scene.
            * It will allow you to select an object both in the editor and on the device.
            */
            if (Application.isEditor)
            {
                Ray ray = arCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitObject;
                if (Physics.Raycast(ray, out hitObject))
                {
                    if (hitObject.collider.tag != "plane")
                    {
                        activeGameObject = hitObject.collider.gameObject;
                        Debug.Log($"SelectedObject: hit object is {activeGameObject.name}");
                    }
                }
            }
            else
            {
                Touch touch = Input.GetTouch(0);
                if (Input.touchCount == 1 && touch.phase == TouchPhase.Ended)
                {
                    List<ARRaycastHit> hits = new List<ARRaycastHit>();
                    rayManager.Raycast(touch.position, hits);
                    if (hits.Count > 0)
                    {
                        Ray ray = arCamera.ScreenPointToRay(touch.position);
                        RaycastHit hitObject;
                        if (Physics.Raycast(ray, out hitObject))
                        {
                            if (hitObject.collider.tag != "plane")
                            {
                                activeGameObject = hitObject.collider.gameObject;
                                Debug.Log($"SelectedObject: hit object is {activeGameObject.name}");
                            }
                        }
                    }
                }
            }
            return activeGameObject;
        }
        private void SearchInput()
        {
            /*
            * Search input is used to control the search input for the application.
            * It will allow you to search for objects in the scene and select them.
            * Additionally it is configured to work in both the editor and on the device.
            */
            if (Application.isEditor)
            {   
                if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }

                    if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
                    {
                        Debug.Log("*** ELEMENT SELECTION MODE : Editor ***");
                        EditMode();
                    }
                    else if (instantiateObjects.visulizationController.TouchMode == TouchMode.JointSelection)
                    {
                        Debug.Log("*** JOINT SELECTION MODE : Editor ***");
                        JointSelectionModeActivation();
                    }

                    else
                    {
                        Debug.Log("Press a button to initialize a mode");
                    }
                }
            }
            else
            {
                SearchTouch();
            }
        }
        private void SearchTouch()
        {
            /*
            * Search touch is used to control the search touch for the application.
            * It allows for the touch selection of objects in space.
            */
            if (Input.touchCount > 0)         
            {
                if (PhysicRayCastBlockedByUi(Input.GetTouch(0).position))
                {
                    if (instantiateObjects.visulizationController.TouchMode == TouchMode.ElementEditSelection)
                    {
                        Debug.Log("*** SearchTouch: ELEMENT SELECTION MODE: Touch ***");
                        EditMode();                     
                    }
                    else if (instantiateObjects.visulizationController.TouchMode == TouchMode.JointSelection)
                    {
                        Debug.Log("*** SearchTouch: JOINT SELECTION MODE: Touch ***");
                        JointSelectionModeActivation();
                    }
                    else
                    {
                        Debug.Log("SearchTouch: Press a button to initialize a mode");
                    }
                }
            }
        }
        private bool PhysicRayCastBlockedByUi(Vector2 touchPosition)
        {
            /*
            * Physics ray cast blocked by UI is used to check if the physics ray cast is blocked by the UI.
            * It will return a boolean value based on the touch position.
            */
            if (HelpersExtensions.IsPointerOverUIObject(touchPosition))
            {
                return false;
            }
            return true;
        }
        private void EditMode()
        {
            /*
            * Edit mode is used to control the edit mode for the application.
            * It will allow you to touch select any object in the screen.
            */
            activeGameObject = SelectedObject();
            
            if (Input.touchCount == 1)
            {
                activeGameObject = SelectedObject();
            }
            if (activeGameObject != null)
            {
                EditorSelectedText.text = activeGameObject.transform.parent.name;
                temporaryObject = activeGameObject;
                string activeGameObjectParentname = activeGameObject.transform.parent.name;
                instantiateObjects.ColorHumanOrRobot(databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.actor, databaseManager.BuildingPlanDataItem.steps[activeGameObjectParentname].data.is_built, activeGameObject);
                addBoundingBox(temporaryObject);
            }
            else
            {
                if (GameObject.Find("BoundingArea") != null)
                {
                    DestroyBoundingBoxFixElementColor();
                }
            }
        }
        private void addBoundingBox(GameObject gameObj)
        {
            /*
            * Add bounding box is used to add a bounding box to the selected object.
            * It will create a bounding box around the object and color the object based on the object.
            */
            DestroyBoundingBoxFixElementColor();

            GameObject boundingArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            boundingArea.name = "BoundingArea";
            boundingArea.GetComponent<Renderer>().material = instantiateObjects.HumanUnbuiltMaterial;
            
            Collider collider = gameObj.GetComponent<Collider>();
            Vector3 center = collider.bounds.center;
            float radius = collider.bounds.extents.magnitude;

            if (boundingArea.GetComponent<Rigidbody>() != null)
            {
                Destroy(boundingArea.GetComponent<BoxCollider>());
            }
            if (boundingArea.GetComponent<Collider>() != null)
            {
                Destroy(boundingArea.GetComponent<BoxCollider>());
            }

            boundingArea.transform.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
            boundingArea.transform.localPosition = center;
            boundingArea.transform.rotation = gameObj.transform.rotation;

            var stepParent = gameObj.transform.parent;
            boundingArea.transform.SetParent(stepParent);
        }
        private void DestroyBoundingBoxFixElementColor()
        {
            /*
            * Destroy bounding box fix element color is used to destroy the bounding box and fix the element color.
            */
            if (GameObject.Find("BoundingArea") != null)
            {
                GameObject Box = GameObject.Find("BoundingArea");
                var element = Box.transform.parent;
                GameObject elementGameobject = Box.transform.parent.gameObject;

                if (element != null && elementGameobject != null)
                {
                    if (CurrentStep != null)
                    {
                        if (element.name != CurrentStep)
                        {
                            Step step = databaseManager.BuildingPlanDataItem.steps[element.name];
                            if(step != null)
                            {
                                instantiateObjects.ObjectColorandTouchEvaluater(instantiateObjects.visulizationController.VisulizationMode, instantiateObjects.visulizationController.TouchMode, step, element.name, elementGameobject.FindObject(step.data.element_ids[0] + " Geometry"));                        
                            }
                            else
                            {
                                Debug.LogWarning("DestroyBoundingBoxFixElementColor: Fix Element Color: Step is null.");
                            }                        
                        }
                    }

                }

                Destroy(GameObject.Find("BoundingArea"));
            }

        }
        public void ModifyStepActor(string key)
        {
            /*
            * Modify step actor is used to modify the actor of the step.
            * It will change the actor from human to robot or robot to human.
            */
            Debug.Log($"ModifyStepActor: Modifying Actor of: {key}");
            Step step = databaseManager.BuildingPlanDataItem.steps[key];

            if(step.data.actor == "HUMAN")
            {
                step.data.actor = "ROBOT";
            }
            else
            {
                step.data.actor = "HUMAN";
            }

            instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, Elements.FindObject(key).FindObject(step.data.element_ids[0] + " Geometry"));
            databaseManager.PushAllDataBuildingPlan(key);
        }
        private void TouchModifyBuildStatus()
        {
            /*
            * Touch modify build status is used to modify the build status of the step.
            * It will change the build status from built to unbuilt or unbuilt to built.
            */
            Debug.Log("TouchModifyBuildStatus: Build Status Button Pressed");
            if (activeGameObject != null)
            {
                ModifyStepBuildStatus(activeGameObject.transform.parent.name);
            }
        }
        private void TouchModifyActor()
        {
            /*
            * Touch modify actor is used to modify the actor of the step.
            * It will change the actor from human to robot or robot to human.
            */
            Debug.Log("TouchModifyActor: Actor Modifier Button Pressed");
            if (activeGameObject != null)
            {
                ModifyStepActor(activeGameObject.transform.parent.name);
            }
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        private void JointSelectionModeActivation()
        {
            /*
            * Edit mode is used to control the edit mode for the application.
            * It will allow you to touch select any object in the screen.
            */

            if(GameObject.Find("ClonedJoint") != null)
            {
                ObjectInstantiaion.DestroyGameObjectByName("ClonedJoint");
            }

            activeJointGameObject = SelectedObject();
            
            if (Input.touchCount == 1)
            {
                activeJointGameObject = SelectedObject();
            }

            if (activeJointGameObject != null)
            {
                temporaryObject = activeJointGameObject;
                CloneAndScaleJointObject(activeJointGameObject);
            }
            else
            {
                if (GameObject.Find("ClonedJoint") != null)
                {
                    ObjectInstantiaion.DestroyGameObjectByName("ClonedJoint");
                }
            }
        }

        private void CloneAndScaleJointObject(GameObject jointToClone)
        {
            /*
            * Clone and scale joint object is used to clone and scale the joint object.
            */
                GameObject clonedJoint = Instantiate(jointToClone, jointToClone.transform.position, jointToClone.transform.rotation);
                clonedJoint.name = "ClonedJoint";
                clonedJoint.transform.SetParent(jointToClone.transform.parent, true);

                //Set the material of the selected joint
                Renderer[] renderers = clonedJoint.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.material = instantiateObjects.SearchedObjectMaterial;
                }
                
                clonedJoint.transform.localScale = new Vector3(3.0f, 3.0f, 3.0f);
        }

    }

    public static class UserInterface
    {
        public static void SetUIObjectColor(GameObject Button, Color color)
        {
            /*
            * Set UI Object Color is used to set the color of the UI object.
            */
            Button.GetComponent<Image>().color = color;
        }
        public static void FindButtonandSetOnClickAction(GameObject searchObject, ref GameObject buttonParentObjectReference, string unityObjectName, UnityAction customAction)
        {
            /*
            * Find Button and Set On Click Action is used to find the button and set the on click action.
            * This method is used throughout the CompasXR Application, and serves as a simple way to set on click actions.
            */
            if (searchObject != null)
            {    
                buttonParentObjectReference = searchObject.FindObject(unityObjectName);
                Button buttonComponent = buttonParentObjectReference.GetComponent<Button>();
                buttonComponent.onClick.AddListener(customAction);
            }
            else
            {
                Debug.LogError($"FindButtonandSetOnClickAction: Could not Set OnClick Action because search object is null for {unityObjectName}");
            }
        }
        public static void FindToggleandSetOnValueChangedAction(GameObject searchObject, ref GameObject toggleParentObjectReference, string unityObjectName, UnityAction<Toggle> customAction)
        {
            /*
            * Find Toggle and Set On Value Changed Action is used to find the toggle and set the on value changed action.
            * This method is used throughout the CompasXR Application, and serves as a simple way to set on value changed actions.
            */
            if (searchObject != null)
            {    
                toggleParentObjectReference = searchObject.FindObject(unityObjectName);
                Toggle toggleComponent = toggleParentObjectReference.GetComponent<Toggle>();
                toggleComponent.onValueChanged.AddListener(value => customAction(toggleComponent));
            }
            else
            {
                Debug.LogError($"Toggle Constructer: Could not Set OnValueChanged Action because search object is null for {unityObjectName}");
            }
        }
        public static void FindSliderandSetOnValueChangeAction(GameObject searchObject, ref GameObject sliderParentObjectReference, ref Slider sliderObjectReference, string unityObjectName, UnityAction<float> customAction)
        {
            /*
            * Find Slider and Set On Value Change Action is used to find the slider and set the on value change action.
            * This method is used throughout the CompasXR Application, and serves as a simple way to set on value change actions.
            */
            if(searchObject != null)
            {
                sliderParentObjectReference = searchObject.FindObject(unityObjectName);
                sliderObjectReference = sliderParentObjectReference.GetComponent<Slider>();
                sliderObjectReference.onValueChanged.AddListener(customAction);
            }
            else
            {
                Debug.LogError($"Slider Constructer: Could not Set OnValueChanged Action because search object is null for {unityObjectName}");
            }
        }
        public static void PrintStringOnClick(string Text)
        {
            /*
            * Print String On Click is used to print a string to the console from a button.
            */
            Debug.Log(Text);
        }
        public static List<TMP_Dropdown.OptionData> SetDropDownOptionsFromStringList(TMP_Dropdown dropDown, List<string> stringList)
        {
            /*
            * Set Drop Down Options From String List is used to set the add a list to a drop down item.
            */
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach(string stringItem in stringList)
            {
                options.Add(new TMP_Dropdown.OptionData(stringItem));
            }
            return options;
        }
        public static TMP_Dropdown.OptionData AddOptionDataToDropdown(string option, TMP_Dropdown dropDown)
        {
            /*
            * Add Option Data To Dropdown is used to add an option to a drop down UI item.
            */            
            TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(option);
            dropDown.options.Add(newOption);
            return newOption;
        }
        public static void SignalOnScreenMessageFromPrefab(ref GameObject prefabReference, ref GameObject messageObjectReference, string activeMessageGameObjectName, GameObject activeMessageParent, string message, string logMessageName)
        {
            /*
            * Signal On Screen Message From Prefab is used to signal an on screen message from a prefab.
            * This method is used to create a message from a prefab and set the message text.
            * Additionally it is dependent on the structure of the prefab.
            */
            Debug.Log($"SignalOnScreenMessageFromPrefab: {logMessageName}: Signal On Screen Message.");
            if(messageObjectReference == null)
            {
                messageObjectReference = GameObject.Instantiate(prefabReference);
                messageObjectReference.transform.SetParent(activeMessageParent.transform, false);
                messageObjectReference.name = activeMessageGameObjectName;
            }
            TMP_Text messageTextComponent = messageObjectReference.FindObject("MessageText").GetComponent<TMP_Text>();

            if(messageTextComponent != null && message != null && messageObjectReference != null)
            {
                SignalOnScreenMessageWithButton(messageObjectReference, messageTextComponent, message);
            }
            else
            {
                Debug.LogWarning($"SignalOnScreenMessageFromPrefab: {logMessageName}: Could not find message object or message component.");
            }
        }
        public static void CreateCenterAlignedSelfDestructiveMessageInstance(string messageGameObjectName, float messageHeight, float messageWidth, Color messagePanelColor, TextAlignmentOptions textAlignment, float textBoarderOffset, Color textColor, string message, float buttonHeight, float buttonWidth, Color buttonColor, float buttonTextBoarderOffset, string buttonText, Color buttonTextColor)
        {
            /*
            * Create an instance of an On Screen Message with a button that will destroy itself when clicked.
            * This instance is used for messages in the library where
            * you cannot find instances of message objects in other classes.
            */

            GameObject newCanvas = new GameObject($"{messageGameObjectName}Canvas");
            Canvas canvas = newCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
	        newCanvas.AddComponent<CanvasScaler>();
	        newCanvas.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject($"{messageGameObjectName}Panel");
	        panel.AddComponent<CanvasRenderer>();
	        Image panelImage = panel.AddComponent<Image>();
	        panelImage.color = messagePanelColor;
	        panel.transform.SetParent(newCanvas.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(messageWidth, messageHeight);
            panelRect.anchoredPosition = new Vector2(0, 0);

            GameObject textObject = new GameObject($"{messageGameObjectName}Text");
	        textObject.transform.SetParent(newCanvas.transform, false);
            TMPro.TextMeshProUGUI messageText = textObject.AddComponent<TMPro.TextMeshProUGUI>();
            RectTransform textRectObject = textObject.GetComponent<RectTransform>();
            float textWidth = messageWidth-textBoarderOffset*2;
            float textHeight = messageHeight-textBoarderOffset*3-buttonHeight;
            textRectObject.sizeDelta = new Vector2(textWidth, textHeight);

            GameObject randomItems = GameObject.Instantiate(textObject, newCanvas.transform, false);
            textRectObject.anchoredPosition = new Vector2(0, (textBoarderOffset*2 + buttonHeight)/2);

            messageText.alignment = textAlignment;
            messageText.color = textColor;
            messageText.text = message;
            messageText.enableAutoSizing = true;
            messageText.fontSizeMin = 1;
            messageText.fontSizeMax = 100;

            GameObject buttonObject = new GameObject($"{messageGameObjectName}Button");
	        buttonObject.transform.SetParent(newCanvas.transform, false);
            float buttonYLocation = (messageHeight/2 - textBoarderOffset - buttonHeight/2)*-1;
            RectTransform buttonRectObject = buttonObject.AddComponent<RectTransform>();
            buttonRectObject.anchoredPosition = new Vector2(0, buttonYLocation);
            buttonRectObject.sizeDelta = new Vector2(buttonWidth, buttonHeight);
            Button buttonComponent = buttonObject.AddComponent<Button>();
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = buttonColor;

            GameObject buttonTextObject = new GameObject($"{messageGameObjectName}ButtonText");
	        buttonTextObject.transform.SetParent(buttonObject.transform, false);
            TMPro.TextMeshProUGUI buttonTextComponent = buttonTextObject.AddComponent<TMPro.TextMeshProUGUI>();
            RectTransform buttontextRectObject = buttonTextComponent.GetComponent<RectTransform>();
            buttontextRectObject.anchoredPosition = new Vector2(0, 0);
            buttontextRectObject.sizeDelta = new Vector2(buttonWidth-buttonTextBoarderOffset, buttonHeight-buttonTextBoarderOffset);

            buttonTextComponent.enableAutoSizing = true;
            buttonTextComponent.fontSizeMin = 1;
            buttonTextComponent.fontSizeMax = 100;
            buttonTextComponent.text = buttonText;
            buttonTextComponent.color = buttonTextColor;
            buttonTextComponent.alignment = TextAlignmentOptions.Center;

            buttonComponent.onClick.AddListener(() => GameObject.Destroy(newCanvas));
        }
        public static void SignalOnScreenMessageFromReference(ref GameObject messageObjectReference, string message, string logMessageName)
        {
            /*
            * Signal On Screen Message From Reference is used to signal an on screen message from a reference.
            * This method is used to create a message from a reference and set the message text.
            * Additionally it is dependent on the structure of the prefab.
            */
            Debug.Log($"SignalOnScreenMessageFromReference: {logMessageName}: Signal On Screen Message.");
            TMP_Text messageTextComponent = messageObjectReference.FindObject("MessageText").GetComponent<TMP_Text>();

            if(messageTextComponent != null && message != null && messageObjectReference != null)
            {
                SignalOnScreenMessageWithButton(messageObjectReference, messageTextComponent, message);
            }
            else
            {
                Debug.LogWarning($"SignalOnScreenMessageFromReference: {logMessageName}: Could not find message object or message component.");
            }
        }
        public static void SignalOnScreenMessageWithButton(GameObject messageGameObject, TMP_Text messageComponent = null, string message = "None")
        {
            /*
            * Signal On Screen Message With Button is used to signal an on screen message with a button.
            * This method is used to set the message text and activate the message object.
            * Additionally it is dependent on the structure of the prefab.
            */
            if (messageGameObject != null)
            {
                if(message != "None" && messageComponent != null)
                {
                    messageComponent.text = message;
                }
                messageGameObject.SetActive(true);
                GameObject AcknowledgeButton = messageGameObject.FindObject("AcknowledgeButton");

                if (AcknowledgeButton.GetComponent<Button>().onClick.GetPersistentEventCount() == 0)
                {
                    AcknowledgeButton.GetComponent<Button>().onClick.AddListener(() => messageGameObject.SetActive(false));
                }
            }
            else
            {
                Debug.LogWarning($"Message: Could not find message object or message component inside of GameObject {messageGameObject.name}.");
            }  
        }

    }
}

