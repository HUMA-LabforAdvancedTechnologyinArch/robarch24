using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dummiesman;
using TMPro;
using CompasXR.UI;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;
using CompasXR.AppSettings;
using UnityEngine.InputSystem;

namespace CompasXR.Core
{
    /*
    * CompasXR.Core : Is the Primary namespace for all Classes that
    * controll the primary functionalities of the CompasXR Application.
    */
    public class InstantiateObjects : MonoBehaviour
    {
        /*
        * InstantiateObjects : Class is used to manage the instantiation of objects
        * in the AR space, and control the visulization, coloring, & etc. of the objects based on the
        * user input, building plan data, assembly info, and event items.
        */

        //Other Sript Objects
        public DatabaseManager databaseManager;
        public UIFunctionalities UIFunctionalities;
        public ScrollSearchManager scrollSearchManager;

        //Object Materials
        public Material BuiltMaterial;
        public Material UnbuiltMaterial;
        public Material HumanBuiltMaterial;
        public Material HumanUnbuiltMaterial;
        public Material RobotBuiltMaterial;
        public Material RobotUnbuiltMaterial;
        public Material LockedObjectMaterial;
        public Material SearchedObjectMaterial;
        public Material ActiveRobotMaterial;
        public Material InactiveRobotMaterial;
        public Material OutlineMaterial;

        //Parent Objects
        public GameObject QRMarkers; 
        public GameObject Elements;
        public GameObject ActiveUserObjects;

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public GameObject Joints;
        public GameObject JointPrefab;
        public GameObject MirroredJointPrefab;

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////


        //Events
        public delegate void InitialElementsPlaced(object source, EventArgs e);
        public event InitialElementsPlaced PlacedInitialElements;

        //Make Initial Visulization controler
        public ModeControler visulizationController = new ModeControler();

        //Private in script use objects
        private GameObject IdxImage;
        private GameObject PriorityImage;
        public GameObject MyUserIndacator;
        private GameObject OtherUserIndacator;
        public GameObject ObjectLengthsTags;
        public GameObject PriorityViewrLineObject;
        public GameObject PriorityViewerPointsObject;

        //Dictionary for storing the p1 & p2 positions of elements from initial instantiation
        public Dictionary<string, List<float>> ObjectLengthsDictionary = new Dictionary<string, List<float>>();

    /////////////////////////////// Monobehaviour Methods //////////////////////////////////////////
        public void Awake()
        {
            OnAwakeInitilization();
        }

    /////////////////////////////// INSTANTIATE OBJECTS //////////////////////////////////////////
        private void OnAwakeInitilization()
        {
            /*
            * Method is used to initialize all the objects, variables, and dependencies
            * that are required for the instantiation of objects in the AR space.
            */

            //Find Additional Scripts.
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            UIFunctionalities = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            scrollSearchManager = GameObject.Find("ScrollSearchManager").GetComponent<ScrollSearchManager>();

            if (scrollSearchManager == null)
            {
                Debug.LogWarning("ScrollSearchManager is null");
            }

            //Find Parent Object to Store Our Items in.
            Elements = GameObject.Find("Elements");
            QRMarkers = GameObject.Find("QRMarkers");
            ActiveUserObjects = GameObject.Find("ActiveUserObjects");

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            Joints = GameObject.Find("Joints");
            JointPrefab = GameObject.Find("JointObjects").FindObject("JointPrefab");
            MirroredJointPrefab = GameObject.Find("JointObjects").FindObject("MirroredJointPrefab");
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

            //Find Initial Materials
            BuiltMaterial = GameObject.Find("Materials").FindObject("Built").GetComponentInChildren<Renderer>().material;
            UnbuiltMaterial = GameObject.Find("Materials").FindObject("Unbuilt").GetComponentInChildren<Renderer>().material;
            HumanBuiltMaterial = GameObject.Find("Materials").FindObject("HumanBuilt").GetComponentInChildren<Renderer>().material;
            HumanUnbuiltMaterial = GameObject.Find("Materials").FindObject("HumanUnbuilt").GetComponentInChildren<Renderer>().material;
            RobotBuiltMaterial = GameObject.Find("Materials").FindObject("RobotBuilt").GetComponentInChildren<Renderer>().material;
            RobotUnbuiltMaterial = GameObject.Find("Materials").FindObject("RobotUnbuilt").GetComponentInChildren<Renderer>().material;
            LockedObjectMaterial = GameObject.Find("Materials").FindObject("LockedObjects").GetComponentInChildren<Renderer>().material;
            SearchedObjectMaterial = GameObject.Find("Materials").FindObject("SearchedObjects").GetComponentInChildren<Renderer>().material;
            ActiveRobotMaterial = GameObject.Find("Materials").FindObject("ActiveRobot").GetComponentInChildren<Renderer>().material;
            InactiveRobotMaterial = GameObject.Find("Materials").FindObject("InactiveRobot").GetComponentInChildren<Renderer>().material;
            OutlineMaterial = GameObject.Find("Materials").FindObject("OutlineMaterial").GetComponentInChildren<Renderer>().material;
            
            //Find GameObjects fo internal use
            IdxImage = GameObject.Find("ImageTagTemplates").FindObject("Circle");
            PriorityImage = GameObject.Find("ImageTagTemplates").FindObject("Triangle");
            MyUserIndacator = GameObject.Find("UserIndicatorPrefabs").FindObject("MyUserIndicatorPrefab");
            OtherUserIndacator = GameObject.Find("UserIndicatorPrefabs").FindObject("OtherUserIndicatorPrefab");
            ObjectLengthsTags = GameObject.Find("ObjectLengthsTags");
            PriorityViewrLineObject = GameObject.Find("PriorityViewerObjects").FindObject("PriorityViewerLine");
            PriorityViewerPointsObject = GameObject.Find("PriorityViewerObjects").FindObject("PriorityViewerPoints");

        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void placeJointsDict(Dictionary<string, Data.Joint> JointsDataDict)
        {
            /*
            * Method is used to place all the joints in the AR space
            * based on the joints data dictionary.
            */
            if (JointsDataDict != null)
            {
                Debug.Log($"placeJointsDict: Number of key-value pairs in the dictionary = {JointsDataDict.Count}");
                foreach (KeyValuePair<string, Data.Joint> entry in JointsDataDict)
                {
                    if (entry.Value != null)
                    {
                        PlaceJoint(entry.Value);
                    }
                }
            }
            else
            {
                Debug.LogWarning("The dictionary is null");
            }
        }
        public void PlaceJoint(Data.Joint joint)
        {
            /*
            * Method is used to place a joint in the AR space
            * based on the joint data.
            */
            
            GameObject jointToPlace = JointPrefab;
            GameObject baseJoint = Instantiate(jointToPlace, JointPrefab.transform.position, JointPrefab.transform.rotation);

            GameObject emptyJointObject = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(new GameObject(),
             joint.element.frame1.point, joint.element.frame1.xaxis,
             joint.element.frame1.yaxis, false, false);

            GameObject jointHalf1 = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(baseJoint,
             joint.element.frame1.point, joint.element.frame1.xaxis,
             joint.element.frame1.yaxis, false, false);
            jointHalf1.transform.SetParent(emptyJointObject.transform, true);
            jointHalf1.SetActive(true);
            jointHalf1.name = "1";

            GameObject jointHalf2 = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(baseJoint,
             joint.element.frame2.point, joint.element.frame2.xaxis,
             joint.element.frame2.yaxis, false, false);
            jointHalf2.transform.SetParent(emptyJointObject.transform, true);
            jointHalf2.SetActive(true);
            jointHalf2.name = "2";

            emptyJointObject.transform.SetParent(Joints.transform, false);
            emptyJointObject.name = $"Joint_{joint.Key}";
            emptyJointObject.SetActive(UIFunctionalities.JointsToggleObject.GetComponent<Toggle>().isOn);

            BoxCollider jointColider = emptyJointObject.AddComponent<BoxCollider>();
            Renderer prefabRenderer = jointToPlace.GetComponentInChildren<Renderer>();
            float jointPrefabYSize = prefabRenderer.bounds.size.y;
            float jointPrefabXSize = prefabRenderer.bounds.size.x;
            float jointPrefabZSize = prefabRenderer.bounds.size.z;
            jointColider.size = new Vector3(jointPrefabXSize*1.1f, jointPrefabYSize*2, jointPrefabZSize*1.2f);

            if(visulizationController.TouchMode == TouchMode.ElementEditSelection)
            {
                jointColider.enabled = false;
            }
            else
            {
                jointColider.enabled = true;
            }

            ColorJointFromAdjacentStepsBuildStatus(emptyJointObject, joint);

        }
        public void ColorJointFromAdjacentStepsBuildStatus(GameObject joint, Data.Joint jointData)
        {
            /*
            * Method is used to color the joint based on the adjacency of the joint
            * in the AR space.
            */
            
            List<bool> adjacentStepsBuiltStatus = new List<bool>();

            foreach (string key in jointData.adjacency)
            {
                adjacentStepsBuiltStatus.Add(databaseManager.BuildingPlanDataItem.steps[key].data.is_built);
            }

            if (adjacentStepsBuiltStatus.Contains(false))
            {
                Renderer[] renderers = joint.GetComponentsInChildren<Renderer>();
                foreach(Renderer rendererObject in renderers)
                {
                    rendererObject.material = UnbuiltMaterial;
                }
            }
            else
            {
                Renderer[] renderers = joint.GetComponentsInChildren<Renderer>();
                foreach(Renderer rendererObject in renderers)
                {
                    rendererObject.material = BuiltMaterial;
                }
            }

        }
        public void ColorAllJointsByBuildState()
        {
            /*
            * Method is used to color all the joints in the AR space
            * based on the build state of the adjacent steps.
            */
            foreach (Transform child in Joints.transform)
            {
                Data.Joint jointData = databaseManager.JointsDataDict[child.name.Replace("Joint_", "")];
                ColorJointFromAdjacentStepsBuildStatus(child.gameObject, jointData);
            }
        }
        public void DestroyAllJoints()
        {
            /*
            * Method is used to destroy all the joints in the AR space.
            */
            foreach (Transform child in Joints.transform)
            {
                Destroy(child.gameObject);
            }
        }
        public void FindandDeleteJointsFromDeletedStep(string deletedStepKey)
        {
            /*
            * Method is used to find the deleted joints in the AR space
            * and destroy them.
            */

            foreach (var jointItem in databaseManager.JointsDataDict)
            {
                string key = jointItem.Key;
                Data.Joint jointData = jointItem.Value;
                
                if (jointData.adjacency.Contains(deletedStepKey))
                {
                    GameObject jointObject = Joints.FindObject($"Joint_{jointData.Key}");
                    Destroy(jointObject);

                    jointData.adjacency.Remove(deletedStepKey);
                }
            }
        }
        public void SetVisibilityOfAllJoints(bool isVisible)
        {
            /*
            * Method is used to set the visibility of the joints in the AR space.
            */
            foreach (Transform child in Joints.transform)
            {
                child.gameObject.SetActive(isVisible);
            }
        }
        public void SetAllJointsVisibilityFromAdjacency()
        {
            /*
            * Method is used to set the visibility of the joints in the AR space
            * based on the visibility of the adjacent steps.
            */
            foreach (var jointItem in databaseManager.JointsDataDict)
            {
                Data.Joint jointData = jointItem.Value;
                SetJointVisibilityFromAdjacencyVisibility(jointData);
            }
        }
        public void SetJointVisibilityFromAdjacencyVisibility(Data.Joint jointEntry)
        {
            /*
            * Method is used to set the visibility of the joint in the AR space
            * based on the visibility of the adjacent steps.
            */

            List<bool> adjacentStepsVisibility = new List<bool>();
            bool isVisible = false;           
            foreach (string key in jointEntry.adjacency)
            {
                GameObject element = Elements.FindObject(key);
                if (element.activeSelf)
                {
                    adjacentStepsVisibility.Add(true);
                }
                else
                {
                    adjacentStepsVisibility.Add(false);
                }
            }

            if(adjacentStepsVisibility.Contains(true))
            {
                isVisible = true;
            }
            GameObject jointObject = Joints.FindObject($"Joint_{jointEntry.Key}");
            jointObject.SetActive(isVisible);
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void PlaceElementFromStep(string Key, Step step)
        {
            /*
            * PlaceElementFromStep : Method is used to place an element in the AR space
            * based on the step information from the building plan data.
            */

            Debug.Log($"PlaceElement: {step.data.element_ids[0]} from Step: {Key}");

            //Load the correct object based on the step information
            GameObject geometry_object = gameobjectTypeSelector(step);
            if (geometry_object == null)
            {
                Debug.LogError($"PlaceElement: This key:{step.data.element_ids[0]} from Step: {Key} is null");
                return;
            }

            //Check if the object is suppose to be loaded as an .Obj file
            bool isObj = false;
            if (step.data.geometry == "2.ObjFile")
            {
                isObj = true;
            }

            //Instantiate the object in the AR space
            GameObject elementPrefab = ObjectInstantiaion.InstantiateObjectFromRightHandFrameData(geometry_object, step.data.location.point, step.data.location.xaxis, step.data.location.yaxis, isObj, databaseManager.z_remapped);

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            StoreObjectLengthsPositionsOnInstantiation(Key, elementPrefab, ObjectLengthsDictionary);
            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

            elementPrefab.transform.SetParent(Elements.transform, false);
            elementPrefab.name = Key;
            GameObject geometryObject = elementPrefab.FindObject(step.data.element_ids[0] + " Geometry");
            
            //Set AR Text objects for the element
            float heightOffset = getHeightOffsetByStepGeometryType(step, step.data.geometry);
            CreateTextForGameObjectOnInstantiation(elementPrefab, step.data.element_ids[0], heightOffset, $"{Key}", $"{elementPrefab.name}IdxText", 0.5f);
            CreateBackgroundImageForText(ref IdxImage, elementPrefab,  heightOffset, $"{elementPrefab.name}IdxImage", false);
            CreateTextForGameObjectOnInstantiation(elementPrefab, step.data.element_ids[0], heightOffset, $"{step.data.priority}", $"{elementPrefab.name}PriorityText", 0.5f);
            CreateBackgroundImageForText(ref PriorityImage, elementPrefab, heightOffset, $"{elementPrefab.name}PriorityImage", false);

            //Control color and visualization of the object
            ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, step, Key, geometryObject);
            if (UIFunctionalities.IDToggleObject.GetComponent<Toggle>().isOn)
            {
                elementPrefab.FindObject(elementPrefab.name + "IdxText").gameObject.SetActive(true);
                elementPrefab.FindObject(elementPrefab.name + "IdxImage").gameObject.SetActive(true);
            }
            if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
            {
                ColorObjectByPriority(UIFunctionalities.SelectedPriority, step.data.priority.ToString(), Key, geometryObject);
                elementPrefab.FindObject(elementPrefab.name + "PriorityText").gameObject.SetActive(true);
                elementPrefab.FindObject(elementPrefab.name + "PriorityImage").gameObject.SetActive(true);
            }
            if (Key == UIFunctionalities.CurrentStep)
            {
                ColorHumanOrRobot(step.data.actor, step.data.is_built, geometryObject);
                UserIndicatorInstantiator(ref MyUserIndacator, elementPrefab, Key, Key, "ME", 0.25f);
            }

            if (visulizationController.TouchMode == TouchMode.JointSelection)
            {
                Collider[] coliders = elementPrefab.GetComponentsInChildren<Collider>();

                foreach (Collider colider in coliders)
                {
                    colider.enabled = false;
                }
            }
            else
            {
                Collider[] coliders = elementPrefab.GetComponentsInChildren<Collider>();

                foreach (Collider colider in coliders)
                {
                    colider.enabled = true;
                }
            }
        }
        public float getHeightOffsetByStepGeometryType(Step step, string geometryType)
        {
            /*
            * Method is used to calculate the height offset of text and user objects
            * based on the geometry type of the step.
            */
            float heightOffset = 0.0f;
            switch (geometryType)
            {
                case "0.Cylinder":
                    heightOffset = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width * 4.0f;
                    break;
                case "1.Box":
                    heightOffset = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width;
                    break;
                case "2.ObjFile":
                    heightOffset = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width;
                    break;
                default:
                    heightOffset = 0.155f;
                    break;
            }
            return heightOffset;
        }
        public void placeElementsDict(Dictionary<string, Step> BuildingPlanDataDict)
        {
            /*
            * Method is used to place all the elements in the AR space
            * based on the building plan data dictionary.
            */
            if (BuildingPlanDataDict != null)
            {
                Debug.Log($"placeElementsDict: Number of key-value pairs in the dictionary = {BuildingPlanDataDict.Count}");
                foreach (KeyValuePair<string, Step> entry in BuildingPlanDataDict)
                {
                    if (entry.Value != null)
                    {
                        PlaceElementFromStep(entry.Key, entry.Value);
                    }

                }
                OnInitialObjectsPlaced();
            }
            else
            {
                Debug.LogWarning("The dictionary is null");
            }
        }   
        public GameObject gameobjectTypeSelector(Step step)
        {
            /*
            * Method is used to determine the type of gameobject to instantiate
            * based on the geometry type of the step.
            * Cylinder & Box will be recreated on the fly, while .Obj files will be loaded from the storage.
            */

            if (step == null)
            {
                Debug.LogWarning("Step is null. Cannot determine GameObject type.");
                return null;
            }

            GameObject element;

            switch (step.data.geometry)
                {
                    case "0.Cylinder":
                        element = new GameObject();
                        element.transform.position = Vector3.zero;
                        element.transform.rotation = Quaternion.identity;
                        float cylinderRadius = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width;
                        float cylinderHeight = databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.height;
                        Vector3 cylindersize = new Vector3(cylinderRadius*2, cylinderHeight/2, cylinderRadius*2);

                        GameObject cylinderObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        cylinderObject.transform.localScale = Vector3.one;
                        cylinderObject.transform.localScale = cylindersize;
                        cylinderObject.name = step.data.element_ids[0].ToString() + " Geometry";

                        // Access the Renderer component of the cylinder object
                        Renderer renderer = cylinderObject.GetComponent<Renderer>();

                        // Get the bounds of the renderer
                        Bounds bounds = renderer.bounds;

                        // Print the size of the bounds along the y-axis
                        Debug.Log($"gameobjectTypeSelector: Cylinder Created. Length along Y-axis: {bounds.size.y}");


                        BoxCollider cylinderCollider = cylinderObject.AddComponent<BoxCollider>();
                        Vector3 cylinderColliderSize = new Vector3(cylinderCollider.size.x*1.1f, cylinderCollider.size.y*1.2f, cylinderCollider.size.z*1.2f);
                        cylinderCollider.size = cylinderColliderSize;
                        cylinderObject.transform.SetParent(element.transform);
                        break;

                    case "1.Box":                    
                        element = new GameObject();
                        element.transform.position = Vector3.zero;
                        element.transform.rotation = Quaternion.identity;
                        Vector3 cubesize = new Vector3(databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.length, databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.height, databaseManager.AssemblyDataDict[step.data.element_ids[0].ToString()].attributes.width);
                        
                        GameObject boxObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        boxObject.transform.localScale = cubesize;
                        boxObject.name = step.data.element_ids[0].ToString() + " Geometry";

                        BoxCollider boxCollider = boxObject.AddComponent<BoxCollider>();
                        Vector3 boxColliderSize = new Vector3(boxCollider.size.x*1.1f, boxCollider.size.y*1.2f, boxCollider.size.z*1.2f);
                        boxCollider.size = boxColliderSize;
                        boxObject.transform.SetParent(element.transform);
                        break;

                    case "2.ObjFile":

                        string basepath = Application.persistentDataPath;
                        string folderpath = Path.Combine(basepath, "Object_Storage");
                        string filepath = Path.Combine(folderpath, step.data.element_ids[0]+".obj");

                        if (File.Exists(filepath))
                        {
                            element =  new OBJLoader().Load(filepath);
                        }
                        else
                        {
                            element = null;
                            Debug.LogError("gameobjectTypeSelector: ObjPrefab is null");
                        }
                        if (element!=null && element.transform.childCount > 0)
                        {
                            GameObject child_object = element.transform.GetChild(0).gameObject;
                            child_object.name = step.data.element_ids[0].ToString() + " Geometry";
                            BoxCollider collider = child_object.AddComponent<BoxCollider>();
                            Vector3 MeshSize = child_object.GetComponent<MeshRenderer>().bounds.size;
                            Vector3 colliderSize = new Vector3(MeshSize.x*1.1f, MeshSize.y*1.2f, MeshSize.z*1.2f);
                            collider.size = colliderSize;
                        }
                        break;

                    default:
                        Debug.LogWarning($"No element type found for type {step.data.geometry}");
                        return null;
                }

                Debug.Log($"gameobjectTypeSelector: Created Element of type {step.data.geometry}");
                return element;
            
        }
        private void CreateTextForGameObjectOnInstantiation(GameObject gameObject, string assemblyID, float offsetDistance, string text, string textObjectName, float fontSize)
        {              
            /*
            * Method is used to create a 3D text object on the instantiation of the gameobject
            * in the AR space.
            */
            GameObject childobject = gameObject.FindObject(assemblyID + " Geometry");
            Vector3 center = ObjectTransformations.FindGameObjectCenter(childobject);
            Vector3 offsetPosition = ObjectTransformations.OffsetPositionVectorByDistance(center, offsetDistance, "y");

            GameObject TextContainer = ObjectInstantiaion.CreateTextinARSpaceAsGameObject(
                text, textObjectName, fontSize,
                TextAlignmentOptions.Center, Color.white, offsetPosition,
                Quaternion.identity, true, false, gameObject);
        }
        private void CreateBackgroundImageForText(ref GameObject inputImg, GameObject parentObject, float verticalOffset,string imgObjectName, bool isVisible=true, bool isBillboard=true, bool storePositionData=true)
        {            
            /*
            * Method is used to create a background image for the 3D text object
            * in the AR space.
            */
            string elementID = databaseManager.BuildingPlanDataItem.steps[parentObject.name].data.element_ids[0];
            Vector3 centerPosition = ObjectTransformations.FindGameObjectCenter(parentObject.FindObject(elementID + " Geometry"));
            Vector3 offsetPosition = ObjectTransformations.OffsetPositionVectorByDistance(centerPosition, verticalOffset, "y");
            GameObject imgObject = ObjectInstantiaion.InstantiateObjectFromPrefabRefrence(ref inputImg, imgObjectName, offsetPosition, Quaternion.identity, parentObject);

            if (isBillboard)
            {
                HelpersExtensions.Billboard billboard = imgObject.AddComponent<HelpersExtensions.Billboard>();
            }
            if (storePositionData)
            {
                HelpersExtensions.ObjectPositionInfo positionData = imgObject.AddComponent<HelpersExtensions.ObjectPositionInfo>();
                positionData.StorePositionRotationScale(imgObject.transform.localPosition, imgObject.transform.localRotation, imgObject.transform.localScale);
            }
            imgObject.SetActive(isVisible);
        }
        public void UserIndicatorInstantiator(ref GameObject UserIndicator, GameObject parentObject, string stepKey, string namingBase, string inGameText, float fontSize)
        {            
            /*
            * Method is used to instantiate a user indicator object in the AR space
            * based on the step information from the building plan data.
            */
            if (UserIndicator == null)
            {
                Debug.LogError("Could Not find UserIndicator.");
                return;
            }

            Step step = databaseManager.BuildingPlanDataItem.steps[stepKey];
            GameObject element = Elements.FindObject(stepKey);
            GameObject geometryObject = element.FindObject(step.data.element_ids[0] + " Geometry");
            if (geometryObject == null)
            {
                Debug.LogError("Geometry Object not found.");
                return;
            }
            
            float heightOffset = getHeightOffsetByStepGeometryType(step, step.data.geometry);
            Vector3 objectCenter = ObjectTransformations.FindGameObjectCenter(geometryObject);
            Vector3 arrowOffset = ObjectTransformations.OffsetPositionVectorByDistance(objectCenter, heightOffset, "y");
            Quaternion rotationQuaternion = Quaternion.identity;

            GameObject newArrow = null;
            newArrow = ObjectInstantiaion.InstantiateObjectFromPrefabRefrence(ref UserIndicator, namingBase+" Arrow", arrowOffset, rotationQuaternion, parentObject);
            newArrow.AddComponent<HelpersExtensions.Billboard>();

            GameObject IndexTextContainer = ObjectInstantiaion.CreateTextinARSpaceAsGameObject(
                inGameText, $"{namingBase} UserText", fontSize,
                TextAlignmentOptions.Center, Color.white, newArrow.transform.position,
                newArrow.transform.rotation, true, true, newArrow);

            ObjectTransformations.OffsetGameObjectPositionByExistingObjectPosition(IndexTextContainer, newArrow, 0.12f , "y");
            newArrow.SetActive(true);
        }
        public void CreateNewUserObject(string UserInfoname, string itemKey)
        {
            /*
            * Method is used to create a new user object in the AR space
            * based on the user information.
            */
            GameObject userObject = new GameObject(UserInfoname);
            userObject.transform.SetParent(ActiveUserObjects.transform);
            userObject.transform.position = Vector3.zero;
            userObject.transform.rotation = Quaternion.identity;
            UserIndicatorInstantiator(ref OtherUserIndacator, userObject, itemKey, UserInfoname, UserInfoname, 0.15f);
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void StoreObjectLengthsPositionsOnInstantiation(string Key, GameObject gameObject, Dictionary<string, List<float>> ObjectLenthsDictionary)
        {
            /*
                Method is used to store the P1 and P2 positions of the element in the AR space
                Based on world Zero.
            */

            if(ObjectLenthsDictionary.ContainsKey(Key))
            {
                ObjectLenthsDictionary.Remove(Key);
            }
            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2PositionsFromGameObjectStepKeyToWorldZero(gameObject, Key, false);
            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2PositionsFromGameObjectStepKeyToWorldZero(gameObject, Key, true);
            ObjectLengthsTags.FindObject("P1Tag").transform.position = P1Position;
            ObjectLengthsTags.FindObject("P2Tag").transform.position = P2Position;
            float P1distance = Vector3.Distance(P1Position, P1Adjusted);
            float P2distance = Vector3.Distance(P2Position, P2Adjusted);
            ObjectLenthsDictionary.Add(Key, new List<float> {P1distance, P2distance});
        }
        public (Vector3, Vector3) FindP1orP2PositionsFromGameObjectStepKeyToWorldZero(GameObject objectToMeasure, string key, bool isP2)
        {
            /*
            * Method is used to find the P1 or P2 positions of the element in the AR space
            * P1 is the center of the element - half of the height or length of the element
            * P2 is the center of the element + half of the height or length of the element
            */
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            Vector3 center = ObjectTransformations.FindGameObjectCenter(objectToMeasure);

            float offsetDistance;
            Vector3 offsetVector;
            if(step.data.geometry == "0.Cylinder")
            {
                offsetDistance =  databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.height;
                offsetVector = objectToMeasure.transform.up;
            }
            else
            {
                offsetDistance = databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.length;
                offsetVector = objectToMeasure.transform.right;
            }

            Vector3 ptPosition = new Vector3(0, 0, 0);
            if(!isP2)
            {                
                ptPosition = center + offsetVector * (offsetDistance / 2)* -1;
            }
            else
            {
                ptPosition = center + offsetVector * (offsetDistance / 2);
            }

            Vector3 worldZeroPosition = Vector3.zero;
            Vector3 ptPositionAdjusted = new Vector3(0,0,0);
            if (ptPosition != Vector3.zero)
            {
                ptPositionAdjusted = new Vector3(ptPosition.x, worldZeroPosition.y, ptPosition.z);
            }
            else
            {
                Debug.LogError("P1 or P2 Position is null.");
            }

            return (ptPosition, ptPositionAdjusted);
        }
        public (Vector3, Vector3) FindP1orP2Positions(string key, bool isP2)
        {
            /*
            * Method is used to find the P1 or P2 positions of the element in the AR space
            * P1 is the center of the element - half of the height or length of the element
            * P2 is the center of the element + half of the height or length of the element
            */

            GameObject element = Elements.FindObject(key);
            Step step = databaseManager.BuildingPlanDataItem.steps[key];
            Vector3 center = ObjectTransformations.FindGameObjectCenter(element.FindObject(step.data.element_ids[0] + " Geometry"));

            float offsetDistance;
            Vector3 offsetVector;
            if(step.data.geometry == "0.Cylinder")
            {
                offsetDistance =  databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.height;
                offsetVector = element.transform.up;
            }
            else
            {
                offsetDistance = databaseManager.AssemblyDataDict[step.data.element_ids[0]].attributes.length;
                offsetVector = element.transform.right;
            }

            Vector3 ptPosition = new Vector3(0, 0, 0);
            if(!isP2)
            {                
                ptPosition = center + offsetVector * (offsetDistance / 2)* -1;
            }
            else
            {
                ptPosition = center + offsetVector * (offsetDistance / 2);
            }

            Vector3 ElementsPosition = Elements.transform.position;
            Vector3 ptPositionAdjusted = new Vector3(0,0,0);
            if (ptPosition != Vector3.zero)
            {
                ptPositionAdjusted = new Vector3(ptPosition.x, ElementsPosition.y, ptPosition.z);
            }
            else
            {
                Debug.LogError("P1 or P2 Position is null.");
            }

            return (ptPosition, ptPositionAdjusted);
        }
        public void CalculateandSetLengthPositions(string key)
        {
            /*
            * Method is used to calculate the P1 and P2 positions of the element in the AR space
            * and set the line positions and text for the object lengths.
            */

            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2Positions(key, false);
            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2Positions(key, true);
            ObjectLengthsTags.FindObject("P1Tag").transform.position = P1Position;
            ObjectLengthsTags.FindObject("P2Tag").transform.position = P2Position;
            if (ObjectLengthsTags.FindObject("P1Tag").GetComponent<HelpersExtensions.Billboard>() == null)
            {
                ObjectLengthsTags.FindObject("P1Tag").AddComponent<HelpersExtensions.Billboard>();
            }
            if (ObjectLengthsTags.FindObject("P2Tag").GetComponent<HelpersExtensions.Billboard>() == null)
            {
                ObjectLengthsTags.FindObject("P2Tag").AddComponent<HelpersExtensions.Billboard>();
            }

            float P1distance = Vector3.Distance(P1Position, P1Adjusted);
            float P2distance = Vector3.Distance(P2Position, P2Adjusted);
            LineRenderer P1Line = ObjectLengthsTags.FindObject("P1Tag").GetComponent<LineRenderer>();
            P1Line.useWorldSpace = true;
            P1Line.SetPosition(0, P1Position);
            P1Line.SetPosition(1, P1Adjusted);

            LineRenderer P2Line = ObjectLengthsTags.FindObject("P2Tag").GetComponent<LineRenderer>();
            P2Line.useWorldSpace = true;
            P2Line.SetPosition(0, P2Position);
            P2Line.SetPosition(1, P2Adjusted);

            //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
            UIFunctionalities.SetObjectLengthsTextFromStoredKey(key);
            // UIFunctionalities.SetObjectLengthsText(P1distance, P2distance);
        }
        public void UpdateObjectLengthsLines(string currentStep, GameObject p1LineObject, GameObject p2LineObject)
        {
            /*
            * Method is used to update the P1 and P2 positions of the element in the AR space
            * and set the line positions for the object lengths.
            */

            (Vector3 P1Position, Vector3 P1Adjusted) = FindP1orP2Positions(currentStep, false);
            List<Vector3> P1Positions = new List<Vector3> { P1Position, P1Adjusted };
            UpdateLinePositionsByVectorList(P1Positions, p1LineObject);

            (Vector3 P2Position, Vector3 P2Adjusted) = FindP1orP2Positions(currentStep, true);
            List<Vector3> P2Positions = new List<Vector3> { P2Position, P2Adjusted };
            UpdateLinePositionsByVectorList(P2Positions, p2LineObject);

        }
        public void CreatePriorityViewerItems(string selectedPriority, ref GameObject lineObject, Color lineColor, float lineWidth, float ptRadius, Color ptColor, GameObject ptsParentObject)
        {
            /*
            * Method is used to create the priority viewer items in the AR space
            * based on the selected priority.
            */
            List<string> priorityList = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[selectedPriority];
            DrawLinefromKeyswithGameObjectReference(priorityList, ref lineObject, lineColor, lineWidth, true, ptColor, ptsParentObject);
        }
        public void DrawLinefromKeyswithGameObjectReference(List<string> keyslist, ref GameObject lineObject, Color lineColor, float lineWidth, bool createPoints=true, Color? ptColor=null, GameObject ptsParentObject=null)
        {
            /*
            * Method is used to draw a line in the AR space based on the list of keys
            * and create points if desired.
            */
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                Debug.Log("LineRenderer is null. for object: " + lineObject.name);
                lineRenderer = lineObject.AddComponent<LineRenderer>();
            }

            if (ptsParentObject && ptsParentObject.transform.childCount > 0)
            {
                foreach (Transform child in ptsParentObject.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            int listLength = keyslist.Count;
            if (listLength > 1)
            {
                lineRenderer.positionCount = keyslist.Count;

                for (int i = 0; i < keyslist.Count; i++)
                {
                    GameObject element = Elements.FindObject(keyslist[i]);
                    Vector3 center = ObjectTransformations.FindGameObjectCenter(element.FindObject(databaseManager.BuildingPlanDataItem.steps[keyslist[i]].data.element_ids[0] + " Geometry"));
                    lineRenderer.SetPosition(i, center);
                    if (createPoints)
                    {
                        if(ptColor != null)
                        {
                            CreateSphereForPriorityViewerFromGameObject(element, ptColor.Value, keyslist[i] + "Point", ptsParentObject);
                        }
                        else
                        {
                            Debug.Log("DrawLineFromKeys: Point Radius and Color not provided.");
                        }
                    }
                }
                lineObject.SetActive(true);
            }
            else
            {
                if(listLength != 0)
                {                        
                    if(createPoints)
                    {
                        if(ptColor != null)
                        {
                            lineObject.SetActive(false);
                            GameObject element = Elements.FindObject(keyslist[0]);
                            CreateSphereForPriorityViewerFromGameObject(element, ptColor.Value, keyslist[0] + "Point", ptsParentObject);
                        }
                        else
                        {
                            Debug.Log("DrawLineFromKeys: Point Radius and Color not provided.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("DrawLineFromKeys: List length is 0.");
                }
            }

        }
        public void UpdatePriorityLine(string selectedPriority, GameObject lineObject)
        {
            /*  
            * Method is used to update the priority line in the AR space
            * based on the selected priority.
            */
            Debug.Log($"UpdatingPriorityLine: priority {selectedPriority}");
            List<Vector3> priorityObjectPositions = GetPositionsFromPriorityGroup(selectedPriority);
            UpdateLinePositionsByVectorList(priorityObjectPositions, lineObject);
        }
        public void UpdateLinePositionsByVectorList(List<Vector3> posVectorList, GameObject lineObject)
        {
            /*
            * Method is used to update the line positions in the AR space
            * based on the list of vector positions.
            */
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            int listLength = posVectorList.Count;
            if (listLength > 1)
            {

                for (int i = 0; i < posVectorList.Count; i++)
                {
                    lineRenderer.SetPosition(i, posVectorList[i]);
                }
            }
            else
            {
                Debug.LogWarning("UpdateLinePositionsByVectorList: List length is 0.");
            }
        }
        public GameObject CreateSphereForPriorityViewerFromGameObject(GameObject gameObject, Color color, string name=null, GameObject parentObject=null)
        {
            /*
            * Method is used to create a sphere object in the AR space
            * based on the the scale of the input game object.
            */
            Collider collider = gameObject.GetComponentInChildren<Collider>();
            Vector3 center = ObjectTransformations.FindGameObjectCenter(gameObject);
            float radius = collider.bounds.extents.magnitude;

            float scaleFactor;
            if(radius>0.75)
            {
                scaleFactor = 0.1f;
            }
            else if(radius<0.2)
            {
                scaleFactor = 0.3f;
            }
            else
            {
                scaleFactor = 0.15f;
            }

            float scaledRadius = radius * scaleFactor;
            GameObject sphere = CreateSphereAtPosition(center, scaledRadius, color, name, parentObject);
            return sphere;
        }
        public GameObject CreateSphereAtPosition(Vector3 position, float radius, Color color, string name=null, GameObject parentObject=null)
        {
            /*
            * Method is used to create a sphere object in the AR space
            * based on the position, radius, and color.
            */
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = new Vector3(radius, radius, radius);
            sphere.GetComponent<Renderer>().material.color = color;
            if (name != null)
            {
                sphere.name = name;
            }
            if (parentObject != null)
            {
                sphere.transform.SetParent(parentObject.transform);
            }
            return sphere;
        }
        public void DestroyChildrenWithOutGeometryName(GameObject gameObject)
        {
            /*
            * Method is used to destroy the children of the game object without
            * name including "Geometry".
            */

            foreach (Transform child in gameObject.transform)
            {
                if (child.gameObject.name.Contains("Geometry") == false)
                {
                    Destroy(child.gameObject);
                }
            }
        }

    /////////////////////////////// POSITION AND ROTATION ////////////////////////////////////////
        public Quaternion GetQuaternionFromStepKey(string key)
        {
            /*
            * Method is used to get the quaternion from the step key
            * based on the x and y axis of the step data.
            */
            ObjectTransformations.Rotation rotationrh = ObjectTransformations.GetRotationFromRightHand(databaseManager.BuildingPlanDataItem.steps[key].data.location.xaxis, databaseManager.BuildingPlanDataItem.steps[key].data.location.yaxis); 
            ObjectTransformations.Rotation rotationlh = ObjectTransformations.RightHandToLeftHand(rotationrh.x , rotationrh.y);
            Quaternion rotationQuaternion = ObjectTransformations.GetQuaternion(rotationlh.y, rotationlh.z);
            return rotationQuaternion;
        }
        public List<Vector3> GetPositionsFromPriorityGroup(string priorityGroup)
        {
            /*
            * Method is used to get the positions from the game objects in a priority group
            * based on the priority group.
            */
            List<Vector3> positions = new List<Vector3>();
            List<string> keys = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[priorityGroup];
            foreach (string key in keys)
            {
                GameObject element = Elements.FindObject(key);
                Vector3 center = ObjectTransformations.FindGameObjectCenter(element.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0] + " Geometry"));
                positions.Add(center);
            }
            return positions;
        }

    /////////////////////////////// Material and colors ////////////////////////////////////////
        public void ObjectColorandTouchEvaluater(VisulizationMode visualizationMode, TouchMode touchMode, Step step, string key, GameObject geometryObject)
        {
            /*
            * Method is used to determine the color and touch of the object
            * based on the visulization mode and touch mode.
            */
            switch (visualizationMode)
            {
                case VisulizationMode.BuiltUnbuilt:
                    ColorBuiltOrUnbuilt(step.data.is_built, geometryObject);
                    break;
                case VisulizationMode.ActorView:
                    ColorHumanOrRobot(step.data.actor, step.data.is_built, geometryObject);
                    break;
            }
            switch (touchMode)
            {
                case TouchMode.None:
                    break;
                case TouchMode.ElementEditSelection:
                    break;
            }
        }
        public void ColorObjectbyInputMaterial(GameObject gamobj, Material material)
        {
            /*
            * Method is used to color the object by the input material
            * based on the game object.
            */
            Renderer m_renderer= gamobj.GetComponentInChildren<MeshRenderer>();
            m_renderer.material = material; 
        }
        public void ColorBuiltOrUnbuilt(bool built, GameObject gamobj)
        {
            /*
            * Method is used to color the object based on the built status
            */
            Renderer m_renderer= gamobj.GetComponentInChildren<MeshRenderer>();
            if (built)
            {          
                m_renderer.material = BuiltMaterial; 
            }
            else
            {
                m_renderer.material = UnbuiltMaterial;
            }
        }
        public void ColorHumanOrRobot(string actor, bool builtStatus, GameObject gamobj)
        {
            /*
            * Method is used to color the object based on the actor and built status
            */            
            Renderer m_renderer= gamobj.GetComponentInChildren<Renderer>();
            if (actor == "HUMAN")
            {
                if(builtStatus)
                {
                    m_renderer.material = HumanBuiltMaterial;
                }
                else
                {
                    m_renderer.material = HumanUnbuiltMaterial; 
                }
            }
            else
            {
                if(builtStatus)
                {
                    m_renderer.material = RobotBuiltMaterial;
                }
                else
                {
                    m_renderer.material = RobotUnbuiltMaterial;
                }
            }
        }
        public void ColorObjectByPriority(string SelectedPriority, string StepPriority,string Key, GameObject gamobj)
        {
            /*
            * Method is used to color the object based on the selected priority
            * and the step priority.
            */
            Renderer m_renderer= gamobj.GetComponentInChildren<Renderer>();
            if (StepPriority != SelectedPriority)
            {
                m_renderer.material = OutlineMaterial;
            }
            else
            {
                Step step = databaseManager.BuildingPlanDataItem.steps[Key];
                string elementID = step.data.element_ids[0];
                ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, step, Key, gamobj.FindObject(elementID + " Geometry"));
            }
        }
        public void ApplyColorBasedOnBuildState()
        {
            /*
            * Method is used to apply color to objects based on their build state
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (KeyValuePair<string, Step> entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");

                    if (gameObject != null && geometryObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorBuiltOrUnbuilt(entry.Value.data.is_built, geometryObject);

                        //Check if other visibility options are on and need to be colored additionally.
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, geometryObject);
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                }
            }
        }
        public void ApplyColorBasedOnActor()
        {
            /*
            * Method is used to apply color to objects based on the actor
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");

                    if (gameObject != null && geometryObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ColorHumanOrRobot(entry.Value.data.actor, entry.Value.data.is_built, geometryObject);

                        //Check if other visibility options are on and need to be colored additionally.
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, geometryObject);
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                }
            }
        }
        public void ApplyColorBasedOnPriority(string SelectedPriority)
        {
            /*
            * Method is used to apply color to objects based on the selected priority
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (var entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");
                    if (gameObject != null && geometryObject != null)
                    {
                        if (entry.Key != UIFunctionalities.CurrentStep)
                        {
                            ColorObjectByPriority(SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry"));
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find object with key: {entry.Key}");
                    }
                }
            }
        }
        public void ApplyColortoPriorityGroup(string selectedPriorityGroup, string newPriorityGroup, bool newPriority=false)
        {
            /*
            * Method is used to apply color to objects based on the selected priority group
            */
            List<string> priorityList = databaseManager.BuildingPlanDataItem.PriorityTreeDictionary[selectedPriorityGroup];
            foreach (string key in priorityList)
            {
                GameObject gameObject = GameObject.Find(key);
                GameObject geometryObject = gameObject.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0] + " Geometry");

                if (gameObject != null && geometryObject != null)
                {
                    if (key != UIFunctionalities.CurrentStep)
                    {
                        if (newPriority)
                        {
                            ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, databaseManager.BuildingPlanDataItem.steps[key], key, gameObject.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0]));
                            if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && key == scrollSearchManager.selectedCellStepIndex)
                            {
                                ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                            }
                        }
                        else
                        {
                            ColorObjectByPriority(newPriorityGroup, databaseManager.BuildingPlanDataItem.steps[key].data.priority.ToString(), key, gameObject.FindObject(databaseManager.BuildingPlanDataItem.steps[key].data.element_ids[0]));
                            if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && key == scrollSearchManager.selectedCellStepIndex)
                            {
                                ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Could not find object with key: {key}");
                }
            }
        }
        public void ApplyColorBasedOnAppModes()
        {
            /*
            * Method is used to apply color to objects based on the app modes
            */
            if (databaseManager.BuildingPlanDataItem.steps != null)
            {
                foreach (KeyValuePair<string, Step> entry in databaseManager.BuildingPlanDataItem.steps)
                {
                    GameObject gameObject = GameObject.Find(entry.Key);
                    GameObject geometryObject = gameObject.FindObject(entry.Value.data.element_ids[0] + " Geometry");

                    if (gameObject != null && geometryObject != null && gameObject.name != UIFunctionalities.CurrentStep)
                    {
                        ObjectColorandTouchEvaluater(visulizationController.VisulizationMode, visulizationController.TouchMode, entry.Value, entry.Key, geometryObject);
                        if (UIFunctionalities.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            ColorObjectByPriority(UIFunctionalities.SelectedPriority, entry.Value.data.priority.ToString(), entry.Key, geometryObject);
                        }
                        if (UIFunctionalities.ScrollSearchToggleObject.GetComponent<Toggle>().isOn && entry.Key == scrollSearchManager.selectedCellStepIndex)
                        {
                            ColorObjectbyInputMaterial(geometryObject, SearchedObjectMaterial);
                        }
                    }
                }
            }
        }

    /////////////////////////////// EVENT HANDLING ////////////////////////////////////////
        public void OnDatabaseInitializedDict(object source, BuildingPlanDataDictEventArgs e)
        {
            /*
            * Method is used to handle the event when the database is initialized
            */
            Debug.Log("OnDatabaseInitializedDict: Database is loaded." + " " + "Number of Steps in the BuildingPlan " + e.BuildingPlanDataItem.steps.Count);
            placeElementsDict(e.BuildingPlanDataItem.steps);
        }
        public void OnDatabaseUpdate(object source, UpdateDataItemsDictEventArgs eventArgs)
        {
            /*
            * Method is used to handle the event when the database is updated
            */
            Debug.Log("OnDatabaseUpdate:" + " " + "Key of Step updated = " + eventArgs.Key);
            if (eventArgs.NewValue == null)
            {
                ObjectInstantiaion.DestroyGameObjectByName(eventArgs.Key);

                //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                FindandDeleteJointsFromDeletedStep(eventArgs.Key);

                //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                //Remove Object Measurements from the deleted step
                if(ObjectLengthsDictionary.ContainsKey(eventArgs.Key))
                {
                    ObjectLengthsDictionary.Remove(eventArgs.Key);
                }
            }
            else
            {
                InstantiateChangedKeys(eventArgs.NewValue, eventArgs.Key);

                //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
                ColorAllJointsByBuildState();
            }

        }
        public void OnUserInfoUpdate(object source, UserInfoDataItemsDictEventArgs eventArgs)
        {
            /*
            * Method is used to handle the event when the user info is updated
            */
            if (eventArgs.UserInfo == null)
            {
                Debug.Log($"OnUserInfoUpdate: User Info is null {eventArgs.Key} will be removed");
                ObjectInstantiaion.DestroyGameObjectByName(eventArgs.Key);
            }
            else
            {
                if (GameObject.Find(eventArgs.Key) != null)
                {
                    Debug.Log($"OnUserInfoUpdate: User {eventArgs.Key} updated their current step.");
                    ObjectInstantiaion.DestroyGameObjectByName(eventArgs.Key + " Arrow");
                    UserIndicatorInstantiator(ref OtherUserIndacator, GameObject.Find(eventArgs.Key), eventArgs.UserInfo.currentStep, eventArgs.Key, eventArgs.Key, 0.15f);
                }
                else
                {
                    Debug.Log($"OnUserInfoUpdate: New user joined and {eventArgs.Key} now join the assembly party :)");
                    CreateNewUserObject(eventArgs.Key, eventArgs.UserInfo.currentStep);
                }
            }
        }
        private void InstantiateChangedKeys(Step newValue, string key)
        {
            /*
            * Method is used to instantiate the changed keys on database events
            */
            if (GameObject.Find(key) != null)
            {
                Debug.Log("InstantiateChangedKeys: Deleting old object with key: " + key);
                GameObject oldObject = GameObject.Find(key);
                Destroy(oldObject);
            }
            else
            {
                Debug.Log( $"InstantiateChangedKeys: Could Not find Object with key: {key}");
            }
            PlaceElementFromStep(key, newValue);
        }
        protected virtual void OnInitialObjectsPlaced()
        {
            /*
            * Method is used to raise the event when the initial objects are placed
            */
            PlacedInitialElements(this, EventArgs.Empty);
            databaseManager.FindInitialElement();
        }

        //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
        public void OnJointsInformationReceived(object source, JointsDataDictEventArgs e)
        {
            /*
            * Method is used to handle the event when joints data has been received
            */
            Debug.Log("OnJointsInformationReceived: Database is loaded." + " " + "total number of joints = " + e.JointsDataDict.Count);
            placeJointsDict(e.JointsDataDict);
        }

    }

    public static class ObjectInstantiaion
    {
        /*
        * ObjectInstantiation class is used to facilitate the placement of objects in the AR space
        * Class is used to handle the object instantiation in the AR space
        * It contains methods for the creation of 3D objects, text, and etc. in the AR space
        */
        public static GameObject CreateTextinARSpaceAsGameObject(string text, string gameObjectName, float fontSize, TextAlignmentOptions textAlignment, Color textColor, Vector3 position, Quaternion rotation, bool isBillboard, bool isVisible, GameObject parentObject=null, bool storePositionData=true)
        {
            GameObject textContainer = new GameObject(gameObjectName);
            textContainer.transform.position = position;
            textContainer.transform.rotation = rotation;

            TextMeshPro textMesh = textContainer.AddComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.autoSizeTextContainer = true;
            textMesh.alignment = textAlignment;
            textMesh.color = textColor;

            if (isBillboard)
            {
                textContainer.AddComponent<HelpersExtensions.Billboard>();
            }
            if (parentObject != null)
            {
                textContainer.transform.SetParent(parentObject.transform);
            }
            if (storePositionData)
            {
                HelpersExtensions.ObjectPositionInfo positionData = textContainer.AddComponent<HelpersExtensions.ObjectPositionInfo>();
                positionData.StorePositionRotationScale(textContainer.transform.localPosition, textContainer.transform.localRotation, textContainer.transform.localScale);
            }
            textContainer.SetActive(isVisible);
            return textContainer;
        }
        public static GameObject InstantiateObjectFromPrefabRefrence(ref GameObject prefabReference, string gameObjectName, Vector3 position, Quaternion rotation, GameObject parentObject=null)
        {
            /*
            * Method is used to instantiate the object from the prefab reference
            */
            GameObject instantiatedObject = GameObject.Instantiate(prefabReference, position, rotation);
            instantiatedObject.name = gameObjectName;
            if (parentObject != null)
            {
                instantiatedObject.transform.SetParent(parentObject.transform);
            }
            return instantiatedObject;
        }
        public static GameObject InstantiateObjectFromRightHandFrameData(GameObject gameObject, float[] pointData, float[] xAxisData, float[] yAxisData, bool isObj, bool z_remapped)
        {
            /*
            * Method is used to instantiate the object from the right hand frame data
            * based on the point, x-axis, y-axis, and z-axis data.
            * This method serves as a simplified version of the placeElement method. And only requires a frame.
            * It loads the object, instantiates it at the correct place and then destroys the loaded object.
            */
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(pointData);
            ObjectTransformations.Rotation rotationData = ObjectTransformations.GetRotationFromRightHand(xAxisData, yAxisData);
            Quaternion rotationQuaternion;

            if(isObj)
            {
                rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForObj(rotationData, z_remapped);
            }
            else
            {
                rotationQuaternion = ObjectTransformations.GetQuaternionFromFrameDataForUnityObject(rotationData);
            }

            if(rotationQuaternion == null)
            {
                Debug.LogError("placeElement: Cannot assign object rotation because it is null");
            }

            GameObject elementPrefab = GameObject.Instantiate(gameObject, positionData, rotationQuaternion);
            if (gameObject != null)
            {
                GameObject.Destroy(gameObject);
            }
            return elementPrefab;
        }
        public static void DestroyGameObjectByName(string gameObjectName)
        {
            /*
            * Destroy the gameobject by input gameObjectName
            */

            if (GameObject.Find(gameObjectName) != null)
            {
                GameObject oldObject = GameObject.Find(gameObjectName);
                GameObject.Destroy(oldObject);
            }
            else
            {
                Debug.LogWarning( $"DestroyGameObjectByName: Could Not find Object with key: {gameObjectName}");
            }
        }
    }
}