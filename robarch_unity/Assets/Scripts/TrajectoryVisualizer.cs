using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.Urdf;
using CompasXR.Core;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.Robots.MqttData;
using Newtonsoft.Json;

namespace CompasXR.Robots
{
    /*
    * CompasXR.Robots : Is the namespace for all Classes that
    * controll the primary functionalities releated to the use of robots in the CompasXR Application.
    * Functionalities, such as robot communication, robot visualization, and robot interaction.
    */
    public class TrajectoryVisualizer : MonoBehaviour
    {
        /*
        The TrajectoryVisualizer class is responsible for managing the active robot in the scene,
        instantiating and visualizing robot trajectories, and controling placing active robot objects in the scene.
        */

        //Other script objects
        private InstantiateObjects instantiateObjects;
        private MqttTrajectoryManager mqttTrajectoryManager;
        private UIFunctionalities uiFunctionalities;

        //GameObjects for storing the active robot objects in the scene
        public GameObject ActiveRobotObjects;
        public GameObject ActiveRobot;
        public GameObject ActiveTrajectoryParentObject;
        private GameObject BuiltInRobotsParent;

        //Dictionary for storing URDFLinkNames associated with JointNames. Updated by recursive method from updating robot.
        public Dictionary<string, string> URDFLinkNames = new Dictionary<string, string>();
        public int? previousSliderValue;
        public Dictionary<string, string> URDFRenderComponents = new Dictionary<string, string>();  //TODO: IF THE URDF STRUCTURE IS DIFFERENT THIS WILL CAUSE A PROBLEM

        //List of available robots
        public List<string> RobotURDFList = new List<string> {"UR3", "UR5", "UR10e", "ETHZurichRFL"};
            
        ////////////////////////////////////////// Monobehaviour Methods ////////////////////////////////////////////////////////
        void Start()
        {
            OnStartInitilization();
        }

        ////////////////////////////////////////// Initilization & Selection //////////////////////////////////////////////////////
        private void OnStartInitilization()
        {
            /*
            OnStartInitilization is called at the start of the script,
            and is responsible for finding and setting the necessary dependencies to objects that exist in the scene.
            */
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            uiFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            mqttTrajectoryManager = GameObject.Find("MQTTTrajectoryManager").GetComponent<MqttTrajectoryManager>();
            BuiltInRobotsParent = GameObject.Find("RobotPrefabs");
            ActiveRobotObjects = GameObject.Find("ActiveRobotObjects");

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            Dictionary<string, float> initialConfigDict = new Dictionary<string, float>();
            initialConfigDict.Add("liftkit_joint", 0.050000000000000003f);
            initialConfigDict.Add("elbow_joint", 2.629f);
            initialConfigDict.Add("wrist_3_joint", -2.117f);
            initialConfigDict.Add("shoulder_pan_joint", -2.117f);
            initialConfigDict.Add("shoulder_lift_joint", -1.736f);
            initialConfigDict.Add("wrist_1_joint", -2.4649999999999999f);
            initialConfigDict.Add("wrist_2_joint", -1.571f);

            Dictionary<string, string> linkNamesStorageDict = new Dictionary<string, string>();
            linkNamesStorageDict.Add("liftkit_joint", "liftkit_600mm");
            linkNamesStorageDict.Add("shoulder_pan_joint", "shoulder_link");
            linkNamesStorageDict.Add("shoulder_lift_joint", "upper_arm_link");
            linkNamesStorageDict.Add("elbow_joint", "forearm_link");
            linkNamesStorageDict.Add("wrist_1_joint", "wrist_1_link");
            linkNamesStorageDict.Add("wrist_2_joint", "wrist_2_link");
            linkNamesStorageDict.Add("wrist_3_joint", "wrist_3_link");

            SetRobArchActiveRobotsOnStart(BuiltInRobotsParent, initialConfigDict, linkNamesStorageDict);
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        private void SetRobArchActiveRobotsOnStart(GameObject BuiltInRobotsParent, Dictionary<string, float> initialConfigDict, Dictionary<string, string> linkNamesStorageDict)
        {
            /*
                SetActiveRobot is responsible for setting the active robots in the scene.
            */
            GameObject baseUrdfObject = BuiltInRobotsParent.FindObject("MobileRobot");
            // GameObject robotAB = BuiltInRobotsParent.FindObject("AB");

            if(baseUrdfObject != null)
            {
                SetActiveRobot(baseUrdfObject, "AA", true, ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectoryParentObject, instantiateObjects.InactiveRobotMaterial, false, initialConfigDict, linkNamesStorageDict);
                URDFManagement.SetRobotConfigfromJointsDict(initialConfigDict, ActiveRobot, linkNamesStorageDict);
            }
            else
            {
                Debug.Log($"SetActiveRobot: Robot AA not found in the BuiltInRobotsParent.");
            }

            if(baseUrdfObject != null)
            {
                SetActiveRobot(baseUrdfObject, "AB", true, ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectoryParentObject, instantiateObjects.InactiveRobotMaterial, false, initialConfigDict, linkNamesStorageDict);
                URDFManagement.SetRobotConfigfromJointsDict(initialConfigDict, ActiveRobot, linkNamesStorageDict);
            }

            if(baseUrdfObject != null)
            {
                SetActiveRobot(baseUrdfObject, "RobotZero", true, ActiveRobotObjects, ref ActiveRobot, ref ActiveTrajectoryParentObject, instantiateObjects.InactiveRobotMaterial, false, initialConfigDict, linkNamesStorageDict);
            }
            else
            {
                Debug.Log($"SetActiveRobot: Robot AB not found in the BuiltInRobotsParent.");
            }
        }
        private void SetActiveRobot(GameObject selectedRobot, string robotName, bool yRotation, GameObject ActiveRobotObjectsParent, ref GameObject ActiveRobot, ref GameObject ActiveTrajectoryParentObject, Material material, bool visibility, Dictionary<string, float> initialConfigDict, Dictionary<string, string> linkNamesStorageDict)
        {
            /*
            SetActiveRobot is responsible for setting the active robot in the scene.
            */
            // GameObject selectedRobot = BuiltInRobotsParent.FindObject(robotName);

            if(selectedRobot != null)
            {

                GameObject temporaryRobotParent = Instantiate(new GameObject(), ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
                temporaryRobotParent.name = robotName;
                GameObject temporaryRobot = Instantiate(selectedRobot, ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
                temporaryRobot.name = $"{selectedRobot.name}Child";
                temporaryRobot.transform.SetParent(temporaryRobotParent.transform);
                if(yRotation)
                {
                    temporaryRobot.transform.Rotate(0, 90, 0);
                }

                if(ActiveRobot==null)
                {
                    ActiveRobot = Instantiate(new GameObject(), ActiveRobotObjectsParent.transform.position, ActiveRobotObjectsParent.transform.rotation);
                    ActiveRobot.name = "ActiveRobot";
                    ActiveRobot.transform.SetParent(ActiveRobotObjectsParent.transform);
                }

                if(ActiveTrajectoryParentObject == null)
                {
                    ActiveTrajectoryParentObject = Instantiate(new GameObject(), ActiveRobot.transform.position, ActiveRobot.transform.rotation);
                    ActiveTrajectoryParentObject.name = "ActiveTrajectory";
                    ActiveTrajectoryParentObject.transform.SetParent(ActiveRobotObjectsParent.transform);
                }

                temporaryRobotParent.transform.SetParent(ActiveRobot.transform);
                URDFManagement.ColorURDFGameObject(temporaryRobot, material, ref URDFRenderComponents);
                if(robotName != "RobotZero")
                {
                    URDFManagement.SetRobotConfigfromJointsDict(initialConfigDict, temporaryRobot, linkNamesStorageDict);
                }
                temporaryRobotParent.SetActive(visibility);
                temporaryRobot.SetActive(true);
            }
            else
            {
                Debug.Log($"SetActiveRobot: Robot {robotName} not found in the BuiltInRobotsParent.");

                string message = "WARNING: Active Robot could not be found. Confirm with planner which Robot is in use, or load robot.";
                UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ActiveRobotCouldNotBeFoundWarningMessage, "ActiveRobotCouldNotBeFoundWarningMessage", uiFunctionalities.MessagesParent, message, $"SetActiveRobot: Robot {robotName} could not be found");
            }
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////// Robot Object Management ////////////////////////////////////////////////////////
        public void InstantiateRobotTrajectoryFromJointsDict(GetTrajectoryResult result, List<Dictionary<string, float>> TrajectoryConfigs, Frame robotBaseFrame, string trajectoryID, GameObject robotToConfigure, Dictionary<string, string> URDFLinks, GameObject parentObject, bool visibility)
        {
            /*
            InstantiateRobotTrajectoryFromJointsDict is responsible for instantiating the robot trajectory in the scene.
            */

            Debug.Log($"InstantiateRobotTrajectory: For {trajectoryID} with {TrajectoryConfigs.Count} configurations.");
            
            if (TrajectoryConfigs.Count > 0 && robotToConfigure != null || parentObject != null)
            {
                int trajectoryCount = TrajectoryConfigs.Count;
                for (int i = 0; i < trajectoryCount; i++)
                {
                    Debug.Log($"InstantiateRobotTrajectory: Config {i} with {TrajectoryConfigs[i].Count} joints.");

                    GameObject temporaryRobot = Instantiate(robotToConfigure, robotToConfigure.transform.position, robotToConfigure.transform.rotation);
                    temporaryRobot.name = $"Config {i}";

                    Debug.Log($"InstantiateRobotTrajectory: Config {i} Setting joint values: {JsonConvert.SerializeObject(TrajectoryConfigs[i])}.");
                    SetRobotConfigfromDictWrapper(TrajectoryConfigs[i], $"Config {i}", temporaryRobot, ref URDFLinkNames);
                    temporaryRobot.transform.SetParent(parentObject.transform);
                    
                    URDFManagement.SetRobotLocalPositionandRotationFromFrame(robotBaseFrame, temporaryRobot);
                    temporaryRobot.SetActive(visibility);
                }

                if(result.PickAndPlace)
                {    
                    StartCoroutine(AttachElementAfterDelay(result, parentObject, 1.0f));
                }
            }
            else
            {
                if(parentObject != null)
                {
                    Debug.LogError("ParentObject is not equal to null.");
                }
                if(URDFLinks.Count == 0)
                {
                    Debug.LogError("URDFLinks is empty.");
                }
                if(TrajectoryConfigs.Count == 0)
                {
                    Debug.LogError("Trajectory is empty.");
                }
                if(robotToConfigure == null)
                {
                    Debug.LogError("RobotToConfigure is null.");
                }
                Debug.LogError("InstantiateRobotTrajectory: Trajectory is empty, robotToConfigure is null, or joint_names is empty.");
            }
            
        }
        public void VisualizeRobotTrajectoryFromResultMessage(GetTrajectoryResult result, Dictionary<string,string> URDFLinkNames, GameObject robotToConfigure, GameObject parentObject, bool visibility)
        {
            /*
            VisualizeRobotTrajectoryFromJointsDict is responsible for visualizing the robot trajectory in the scene.
            */
            Debug.Log($"VisualizeRobotTrajectory: For {result.TrajectoryID} with {result.Trajectory} configurations.");

            //This is a fix, when the object gets instantiated from the child is off, and this fixes the one time that this happens.
            foreach (Transform child in ActiveRobot.transform)
            {
                if(!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                }
            }
            ActiveRobot.SetActive(false);
            URDFManagement.ColorURDFGameObject(robotToConfigure, instantiateObjects.InactiveRobotMaterial, ref URDFRenderComponents);
            InstantiateRobotTrajectoryFromJointsDict(result, result.Trajectory, result.RobotBaseFrame, result.TrajectoryID, robotToConfigure, URDFLinkNames, parentObject, visibility);     
        }
        IEnumerator AttachElementAfterDelay(GetTrajectoryResult result, GameObject parentObject, float delay = 0.1f)
        {
            yield return new WaitForSeconds(delay);
            AttachElementToTrajectoryEndEffectorLinks(result.ElementID, parentObject.name, result.RobotName, result.EndEffectorLinkName, result.PickIndex.Value, result.Trajectory.Count);
        }
        public void AttachElementToTrajectoryEndEffectorLinks(string stepID, string trajectoryParentName, string robotName, string endEffectorLinkName, int pickIndex, int trajectoryCount)
        {
            /*
            AttachElementToTrajectoryEndEffectorLinks is responsible for attaching an element to the end effector link in the trajectory GameObject.
            */

            int lastConfigIndex = trajectoryCount - 1;
            GameObject stepElement = GameObject.Find(stepID);

            GameObject TrajectoryParent = GameObject.Find(trajectoryParentName);
            GameObject endEffectorLink = TrajectoryParent.FindObject($"Config {lastConfigIndex}").FindObject(endEffectorLinkName);
            GameObject newStepElement = Instantiate(stepElement);

            //Remove all children from the stepElement
            Renderer stepChildRenderer = newStepElement.GetComponentInChildren<MeshRenderer>();
            stepChildRenderer.material = instantiateObjects.InactiveRobotMaterial;
            instantiateObjects.DestroyChildrenWithOutGeometryName(newStepElement);
            newStepElement.name = $"AttachedElement{lastConfigIndex}";

            newStepElement.transform.SetParent(endEffectorLink.transform, true);
            newStepElement.transform.position = stepElement.transform.position;
            newStepElement.transform.rotation = stepElement.transform.rotation;

            Vector3 position = newStepElement.transform.localPosition;
            Quaternion rotation = newStepElement.transform.localRotation;

            for (int i = lastConfigIndex - 1; i >= pickIndex; i--)
            {
                GameObject currentEndEffectorLink = TrajectoryParent.FindObject($"Config {i}").FindObject(endEffectorLinkName);
                GameObject attachedStepElement = Instantiate(newStepElement);
                attachedStepElement.transform.SetParent(currentEndEffectorLink.transform, true);
                instantiateObjects.DestroyChildrenWithOutGeometryName(attachedStepElement);
                attachedStepElement.name = $"AttachedElement{i}";
                attachedStepElement.transform.localPosition = position;
                attachedStepElement.transform.localRotation = rotation;
            }

        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void AttachStickObjectDirectlyToEndEffector(string stepID, string trajectoryParentName, string robotName, string endEffectorLinkName, int pickIndex, int trajectoryCount)
        {
            /*
            AttachElementToTrajectoryEndEffectorLinks is responsible for attaching an element 
            ACHTUNG: This only works for the geometry used for the RobArch Assembly.
            */

            int lastConfigIndex = trajectoryCount - 1;
            GameObject stepElement = GameObject.Find(stepID);

            GameObject TrajectoryParent = GameObject.Find(trajectoryParentName);
            GameObject endEffectorLink = TrajectoryParent.FindObject($"Config {lastConfigIndex}").FindObject(endEffectorLinkName);
            GameObject newStepElement = Instantiate(stepElement);

            //Remove all children from the stepElement
            Renderer stepChildRenderer = newStepElement.GetComponentInChildren<MeshRenderer>();
            stepChildRenderer.material = instantiateObjects.InactiveRobotMaterial;
            instantiateObjects.DestroyChildrenWithOutGeometryName(newStepElement);
            newStepElement.name = $"AttachedElement{lastConfigIndex}";

            newStepElement.transform.SetParent(endEffectorLink.transform, true);
            newStepElement.transform.localPosition = new Vector3(endEffectorLink.transform.localPosition.x, endEffectorLink.transform.localPosition.y + 0.094f, endEffectorLink.transform.localPosition.z);
            Quaternion endEffectorRemappedRotation = Quaternion.Euler(endEffectorLink.transform.rotation.x, endEffectorLink.transform.rotation.y, endEffectorLink.transform.localRotation.z);
            newStepElement.transform.localRotation = endEffectorRemappedRotation * Quaternion.Euler(0, 0, 90);


            Vector3 position = newStepElement.transform.localPosition;
            Quaternion rotation = newStepElement.transform.localRotation;

            for (int i = lastConfigIndex - 1; i >= pickIndex; i--)
            {
                GameObject currentEndEffectorLink = TrajectoryParent.FindObject($"Config {i}").FindObject(endEffectorLinkName);
                GameObject attachedStepElement = Instantiate(newStepElement);
                attachedStepElement.transform.SetParent(currentEndEffectorLink.transform, true);
                instantiateObjects.DestroyChildrenWithOutGeometryName(attachedStepElement);
                attachedStepElement.name = $"AttachedElement{i}";
                attachedStepElement.transform.localPosition = position;
                attachedStepElement.transform.localRotation = rotation;
            }

        }
        public void DestroyActiveRobotObjects()
        {
            /*
            DestroyActiveRobotObjects is responsible for destroying the active robot objects in the scene.
            */

            if(ActiveRobot != null)
            {
                Destroy(ActiveRobot);
            }
            if(ActiveTrajectoryParentObject != null)
            {
                Destroy(ActiveTrajectoryParentObject);
            }
        }
        public void DestroyActiveTrajectoryChildren()
        {
            /*
            DestroyActiveTrajectoryChildren is responsible for destroying child objects in the trajectory parent.
            */
            if(ActiveTrajectoryParentObject != null)
            {
                foreach (Transform child in ActiveTrajectoryParentObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        public void SetRobotConfigfromDictWrapper(Dictionary<string, float> config, string configName, GameObject robotToConfigure, ref Dictionary<string, string> urdfLinkNames)
        {
            /*
            SetRobotConfigfromDictWrapper is responsible for setting the robot configuration from a dictionary.
            */

            Debug.Log($"SetRobotConfigfromDictWrapper: Visulizing robot configuration for gameObject {robotToConfigure.name}.");
            
            if (urdfLinkNames.Count == 0)
            {
                Debug.Log("I Entered Here");
                URDFManagement.FindLinkNamesFromJointNames(robotToConfigure.transform, config, ref urdfLinkNames);
            }
            if(URDFManagement.ConfigJointsEqualURDFLinks(config, urdfLinkNames))
            {
                URDFManagement.SetRobotConfigfromJointsDict(config, robotToConfigure, urdfLinkNames);
            }
            else
            {
                if(uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject == null)
                {
                    string message = $"WARNING: {configName} structure does not match the URDF structure and will not be visualized.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject, "ConfigDoesNotMatchURDFStructureWarningMessage", uiFunctionalities.MessagesParent, message, "SetRobotConfigfromDictWrapper: Config does not match URDF");
                }
                else if(uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject.activeSelf == false)
                {
                    string message = $"WARNING: {configName} structure does not match the URDF structure and will not be visualized.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalities.OnScreenErrorMessagePrefab, ref uiFunctionalities.ConfigDoesNotMatchURDFStructureWarningMessageObject, "ConfigDoesNotMatchURDFStructureWarningMessage", uiFunctionalities.MessagesParent, message, "SetRobotConfigfromDictWrapper: Config does not match URDF");
                }

                Debug.LogWarning($"SetRobotConfigfromDictWrapper: Config dict {config.Count} (Count) and LinkNames dict {urdfLinkNames.Count} (Count) for search do not match.");
            }
        }
        public void ColorRobotConfigfromSliderInput(int sliderValue, Material inactiveMaterial, Material activeMaterial, ref int? previousSliderValue)
        {
            /*
            ColorRobotConfigfromSlider is responsible for coloring the robot configuration from the slider input for trajectory review.
            */
            Debug.Log($"ColorRobotConfigfromSlider: Coloring robot config {sliderValue} for active trajectory.");
            if(previousSliderValue != null)
            {
                GameObject previousRobotGameObject = ActiveTrajectoryParentObject.FindObject($"Config {previousSliderValue}");
                URDFManagement.ColorURDFGameObject(previousRobotGameObject, inactiveMaterial, ref URDFRenderComponents);

                //Attached GameObject
                GameObject previousAttachedGameObject = previousRobotGameObject.FindObject($"AttachedElement{previousSliderValue}");
                if(previousAttachedGameObject != null)
                {
                    previousAttachedGameObject.GetComponentInChildren<Renderer>().material = inactiveMaterial;
                }
            }

            GameObject robotGameObject = ActiveTrajectoryParentObject.FindObject($"Config {sliderValue}");
            if (robotGameObject == null)
            {
                Debug.Log($"ColorRobotConfigfromSlider: Robot GameObject not found for Config {sliderValue}.");
            }
            URDFManagement.ColorURDFGameObject(robotGameObject, activeMaterial, ref URDFRenderComponents);

            //Attached GameObject
            GameObject attachedGameObject = robotGameObject.FindObject($"AttachedElement{sliderValue}");
            if(attachedGameObject != null)
            {
                attachedGameObject.GetComponentInChildren<Renderer>().material = activeMaterial;
            }

            previousSliderValue = sliderValue;
        }

    }

    public static class URDFManagement
    {
        /*
        * URDFManagement : Is a static class that contains methods for managing URDF objects in the scene.
        * URDFManagement is responsible for finding, setting, coloring, and visualizing robot configurations in the scene.
        */
        public static void SetRobotConfigfromList(List<float> config, GameObject URDFGameObject, List<string> jointNames)
        {
            /*
            SetRobotConfigfromList is responsible for setting the robot configuration to the URDF from a list of joint values.
            */
            Debug.Log($"SetRobotConfigFromList: Visulizing robot configuration for gameObject {URDFGameObject.name}.");
            int configCount = config.Count;

            for (int i = 0; i < configCount; i++)
            {
                GameObject joint = URDFGameObject.FindObject(jointNames[i]);
                if (joint)
                {
                    JointStateWriter jointStateWriter = joint.GetComponent<JointStateWriter>();
                    UrdfJoint urdfJoint = joint.GetComponent<UrdfJoint>();
                    if (!jointStateWriter)
                    {
                        jointStateWriter = joint.AddComponent<JointStateWriter>();    
                    }
                    
                    jointStateWriter.Write(config[i]);
                }  
                else
                {
                    Debug.Log($"SetRobotConfigfromList: Joint {joint.name} not found in the robotToConfigure.");
                }
            }

        }
        public static void SetRobotConfigfromJointsDict(Dictionary<string, float> config, GameObject URDFGameObject, Dictionary<string, string> linkNamesStorageDict)
        {
            /*
            SetRobotConfigfromJointsDict is responsible for setting the robot configuration to the URDF from a dictionary of joint values.
            */
            Debug.Log($"SetRobotConfigFromDict: Visulizing robot configuration for gameObject {URDFGameObject.name}.");    

            foreach (KeyValuePair<string, float> jointDescription in config)
            {
                string jointName = jointDescription.Key;
                float jointValue = jointDescription.Value;
                string urdfLinkName = linkNamesStorageDict[jointName];
                GameObject urdfLinkObject = URDFGameObject.FindObject(urdfLinkName);
                Debug.Log($"SetRobotConfigFromDict: ACTUALLY WRITING: Setting joint {jointName} to value {jointValue} on link {urdfLinkName}.");

                if (urdfLinkObject)
                {
                    Debug.Log("SetRobotConfigFromDict: Found URDF Link Object. nameed: " + urdfLinkObject.name);
                    Debug.Log("SetRobotConfigFromDict: Setting information for : " + jointName + " with value: " + jointValue + " on link: " + urdfLinkName);
                    JointStateWriter jointStateWriter = urdfLinkObject.GetComponent<JointStateWriter>();
                    if (!jointStateWriter)
                    {
                        jointStateWriter = urdfLinkObject.AddComponent<JointStateWriter>();    
                    }
                    jointStateWriter.Write(jointValue);
                    // Debug.Log($"Joint Values after writing: {urdfLinkObject.GetComponent<UrdfJoint>().Values}.");
                }  
                else
                {
                    Debug.LogWarning($"SetRobotConfigfromDict: URDF Link {urdfLinkName} not found in the robotToConfigure.");
                }
            }

        }
        public static void FindAllMeshRenderersInURDFGameObject(Transform currentTransform, Dictionary<string,string> URDFRenderComponents)
        {
            /*
            * FindAllMeshRenderersInURDFGameObject is responsible for finding all MeshRenderers in the URDF GameObject.
            * The method is called recursively to search through all children of the URDF GameObject due to its nested structure.
            */

            Debug.Log($"FindMeshRenderers: Searching for Mesh Renderer in {currentTransform.gameObject.name}.");
            MeshRenderer meshRenderer = currentTransform.GetComponentInChildren<MeshRenderer>();

            if (meshRenderer != null)
            {
                int instanceID = meshRenderer.GetInstanceID();
                if (!URDFRenderComponents.ContainsKey(meshRenderer.gameObject.name))
                {
                    // meshRenderer.gameObject.name = meshRenderer.gameObject.name + $"_{instanceID.ToString()}";
                    URDFRenderComponents.Add(meshRenderer.gameObject.name, meshRenderer.gameObject.name);
                    // URDFRenderComponents.Add(instanceID.ToString(), meshRenderer.gameObject.name);
                }
            }
            if (currentTransform.childCount > 0)
            {
                foreach (Transform child in currentTransform)
                {
                    FindAllMeshRenderersInURDFGameObject(child, URDFRenderComponents);
                }
            }
        }
        public static void SetRobotLocalPositionandRotationFromFrame(Frame robotBaseFrame, GameObject robotToPosition)
        {
            /*
            * SetRobotPosition is responsible for setting the robot position and rotation from the robot baseframe from a RightHanded Plane.
            */

            Debug.Log($"SetRobotPosition: Setting the robot {robotToPosition.name} to position and rotation from robot baseframe.");
            
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(robotBaseFrame.point);
            ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(robotBaseFrame.xaxis, robotBaseFrame.yaxis);
            Quaternion rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForUnityObject(rotationData);
            robotToPosition.transform.localPosition = positionData;
            robotToPosition.transform.localRotation = rotationQuaternion;
        }
        public static void ColorURDFGameObject(GameObject RobotParent, Material material, ref Dictionary<string, string> URDFRenderComponentsStorageDict)
        {
            /*
            * ColorURDFGameObject is responsible for coloring the URDF GameObject with a material.
            * If the URDFRenderComponentsStorageDict is empty, the method will search through the URDF GameObject to find all MeshRenderers.
            */
            if (URDFRenderComponentsStorageDict.Count == 0)
            {
                foreach (Transform child in RobotParent.transform)
                {
                    URDFManagement.FindAllMeshRenderersInURDFGameObject(child, URDFRenderComponentsStorageDict);
                }
            }

            foreach (KeyValuePair<string, string> component in URDFRenderComponentsStorageDict)
            {
                string gameObjectName = component.Value;
                GameObject gameObject = RobotParent.FindObject(gameObjectName);

                if (gameObject)
                {
                    MeshRenderer meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer)
                    {
                        meshRenderer.material = material;
                    }
                    else
                    {
                        Debug.Log($"ColorRobot: MeshRenderer not found for {gameObject} when searching through URDF list.");
                    }
                }
            }
        }
        public static void FindLinkNamesFromJointNames(Transform currentTransform, Dictionary<string, float> config, ref Dictionary<string,string> URDFLinkNamesStorageDict)
        {
            /*
            * FindLinkNamesFromJointNames is responsible for finding the URDF Link names from the Joint names in the URDF GameObject.
            * The method is called recursively to search through all children of the URDF GameObject due to its nested structure.
            */
            UrdfJoint urdfJoint = currentTransform.GetComponent<UrdfJoint>();
            if (urdfJoint != null)
            {
                if(config.ContainsKey(urdfJoint.JointName) && !URDFLinkNamesStorageDict.ContainsKey(urdfJoint.JointName))
                {
                    Debug.Log($"FindLinkNames: Value associated with {urdfJoint.JointName} is {config[urdfJoint.JointName]} on link {currentTransform.gameObject.name}.");
                    Debug.Log($"FindLinkNames: Found UrdfJointName {urdfJoint.JointName} in URDF on GameObject {currentTransform.gameObject.name}.");
                    URDFLinkNamesStorageDict.Add(urdfJoint.JointName, currentTransform.gameObject.name);
                }
            }
            if (currentTransform.childCount > 0)
            {
                foreach (Transform child in currentTransform)
                {
                    FindLinkNamesFromJointNames(child, config, ref URDFLinkNamesStorageDict);
                }
            }
            else
            {
                Debug.Log($"FindLinkNames: No UrdfJoint found in URDF on GameObject {currentTransform.gameObject.name}");
            }

        }
        public static bool ConfigJointsEqualURDFLinks(Dictionary<string, float> config, Dictionary<string,string> URDFLinkNamesDict)
        {
            
            /*
            * ConfigJointsEqualURDFLinks is responsible for checking if the joint names in the config dictionary match the URDF Link names.
            */
            bool isEqual = true;
            foreach (KeyValuePair<string, float> joint in config)
            {
                string jointName = joint.Key;
                if(URDFLinkNamesDict.ContainsKey(jointName))
                {
                    Debug.Log($"ConfigJointsEqualURDFLinks: Found joint {jointName} in URDFLinkNames.");
                }
                else
                {
                    Debug.Log($"ConfigJointsEqualURDFLinks: Joint {jointName} not found in URDFLinkNames.");
                    isEqual = false;
                }
            }
            return isEqual;
        }

    }
}
