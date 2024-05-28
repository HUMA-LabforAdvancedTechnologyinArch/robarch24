using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CompasXR.Core.Data
{   
    /*
    * CompasXR.Core.Data : A namespace to define and controll various data structures and data processing methods.
    * This namespace is used to define the data structures that corelate to Compas data structures
    */

   ///////////// Class for Handeling Data conversion Inconsistencies /////////////// 

    [System.Serializable]
    public static class DataConverters
    {
        /*
        * DataConverters : A class to handle data conversion inconsistencies between different data types
        * and casting information to info required for deserilization.
        */

        public static float[] ConvertDatatoFloatArray(object data)
        {
            if (data is List<object>)
            {
                List<object> dataList = data as List<object>;
                return dataList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is float[])
            {
                return (float[])data;
            }
            else if (data is List<System.Double>)
            {
                List<System.Double> doubleList = data as List<System.Double>;
                return doubleList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is System.Single[])
            {
                return new float[] { (float)data };
            }
            else if (data is List<System.Single>)
            {
                List<System.Single> singleList = data as List<System.Single>;
                return singleList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is System.Double[])
            {
                System.Double[] doubleArray = data as System.Double[];
                return doubleArray.Select(Convert.ToSingle).ToArray();
            }
            else if (data is JArray)
            {
                JArray dataArray = data as JArray;
                return dataArray.Select(token => (float)token).ToArray();
            }
            else
            {
                Debug.LogError("DataParser: Data is not a List<Object>, List<System.Double>, System.Double Array, System.Single Array, List<System.Single>,  float Array, or JArray.");
                return null;
            }
        }
    } 
    

   /////////////Classes for Assembly Desearialization./////////////// 
    [System.Serializable]
    public class Node
    {
        /*
        * Node : A class to define the structure of a node in the assembly data structure.
        * This class is used to define the structure of a node in the assembly data structure.
        * It is based off the Compas data structure for a node.
        */
        public Part part { get; set; }
        public string type_data { get; set; }
        public string type_id { get; set; }
        public Attributes attributes { get; set; }
        public static Node Parse(string key, object jsondata)
        {
            /*
            * Method to create an instance of a the Node class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            Node node = FromData(jsonDataDict, key);
            return node;
        }
        public static Node FromData(Dictionary<string, object> jsonDataDict, string key)
        {
            /*
            * Method to create an instance of a the Node class from a dictionary.
            */
            Node node = new Node();
            node.part = new Part();
            node.attributes = new Attributes();
            node.type_id = key;
            DtypeGeometryDesctiptionSelector(node, jsonDataDict);
            return node;
        }
        private static void DtypeGeometryDesctiptionSelector(Node node, Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to select the correct desearialization method based on the dtype of the part in the assembly.
            * It is used to parse the data from the dictionary and set the values of the node class.
            */

            Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
            Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
            string dtype = (string)partDict["dtype"];

            switch (dtype)
            {
                case "compas.geometry/Cylinder":

                    node.part.frame = Frame.Parse(dataDict["frame"]);
                    node.part.dtype = dtype;

                    float height = Convert.ToSingle(dataDict["height"]);
                    float radius = Convert.ToSingle(dataDict["radius"]);
                    node.attributes.length = radius;
                    node.attributes.width = radius;
                    node.attributes.height = height;

                    break;

                case "compas.geometry/Box":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(dataDict["frame"]);

                    float xsize = Convert.ToSingle(dataDict["xsize"]);
                    float ysize = Convert.ToSingle(dataDict["ysize"]);
                    float zsize = Convert.ToSingle(dataDict["zsize"]);
                    node.attributes.length = xsize;
                    node.attributes.width = ysize;
                    node.attributes.height = zsize;

                    break;

                case "compas.geometry/Frame":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.FromData(dataDict);

                    //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas.datastructures/Mesh":

                    node.part.dtype = dtype;

                    Dictionary<string, object> frameDict;
                    if (jsonDataDict.TryGetValue("frame", out object frameObject))
                    {
                        frameDict = jsonDataDict["frame"] as Dictionary<string, object>;
                        Dictionary<string, object> frameDataDict = frameDict["data"] as Dictionary<string, object>;
                        node.part.frame = Frame.FromData(frameDataDict);
                    }
                    else
                    {
                        node.part.frame = Frame.RhinoWorldXY();
                    }

                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas_timber.parts/Beam":

                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(dataDict["frame"]);

                    float objLength = Convert.ToSingle(dataDict["length"]);
                    float objWidth = Convert.ToSingle(dataDict["width"]);
                    float objHeight = Convert.ToSingle(dataDict["height"]);
                    node.attributes.length = objLength;
                    node.attributes.width = objWidth;
                    node.attributes.height = objHeight;

                    break;

                case string connectionType when connectionType.StartsWith("compas_timber.connections"):
                    //TODO: Set dtype to only compas_timber.connections so It can be checked in valid node without .StartsWith
                    node.part.dtype = "compas_timber.connections";
                    break;

                case "compas.datastructures/Part":
                    PartDesctiptionSelector(node, jsonDataDict);
                    break;

                default:
                    Debug.LogError($"DtypeGeometryDesctiptionSelector: No Deserilization type for dtype {dtype}.");
                    break;
            }
        }
        private static void PartDesctiptionSelector(Node node, Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to select the correct desearialization method based on the dtype of the part in the assembly.
            * It is used to parse the data from the dictionary and set the values of the node class.
            * This method is specifically used to parse the data for the part dtype "compas.datastructures/Part".
            */
            Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
            Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
            Dictionary<string, object> attributesDict = dataDict["attributes"] as Dictionary<string, object>;
            Dictionary<string, object> shapeDict = attributesDict["shape"] as Dictionary<string, object>;
            Dictionary<string, object> shapeDataDict = shapeDict["data"] as Dictionary<string, object>;
            string dtype = (string)shapeDict["dtype"];

            switch (dtype)
            {
                case "compas.geometry/Cylinder":

                    node.part.frame = Frame.Parse(shapeDataDict["frame"]);
                    node.part.dtype = dtype;

                    float height = Convert.ToSingle(shapeDataDict["height"]);
                    float radius = Convert.ToSingle(shapeDataDict["radius"]);
                    node.attributes.length = radius;
                    node.attributes.width = radius;
                    node.attributes.height = height;

                    break;

                case "compas.geometry/Box":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(shapeDataDict["frame"]);

                    float xsize = Convert.ToSingle(shapeDataDict["xsize"]);
                    float ysize = Convert.ToSingle(shapeDataDict["ysize"]);
                    float zsize = Convert.ToSingle(shapeDataDict["zsize"]);
                    node.attributes.length = xsize;
                    node.attributes.width = ysize;
                    node.attributes.height = zsize;

                    break;
                
                case "compas.datastructures/Mesh":

                    node.part.dtype = dtype;

                    Dictionary<string, object> frameDict;
                    if (jsonDataDict.TryGetValue("frame", out object frameObject))
                    {
                        frameDict = jsonDataDict["frame"] as Dictionary<string, object>;
                        Dictionary<string, object> frameDataDict = frameDict["data"] as Dictionary<string, object>;
                        node.part.frame = Frame.FromData(frameDataDict);
                    }
                    else
                    {
                        node.part.frame = Frame.RhinoWorldXY();
                    }

                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas.geometry/Frame":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.FromData(shapeDataDict);

                    //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    if (attributesDict.TryGetValue("name", out object name))
                    {
                        string nameString = name.ToString();
                        if(nameString.StartsWith("QR_"))
                        {
                            node.part.dtype = "compas_xr/QRCode";
                        }
                    }

                    break;

                default:
                    Debug.LogError($"PartDesctiptionSelector: No Part Deserilization type for dtype {dtype}.");
                    break;
                
            }
        }
        public bool IsValidNode()
        {   
            /*
            * Method to check if the node contains all valid information.
            */
            if (!string.IsNullOrEmpty(type_id) &&
                !string.IsNullOrEmpty(part.dtype) &&
                part != null &&
                part.frame != null)
            {
                if (part.dtype == "compas_timber.connections")
                {
                    Debug.Log("This is a timbers Joint and should be ignored");
                    return false;
                }
                else if (part.dtype != "compas.geometry/Frame" || 
                        part.dtype != "compas.datastructures/Mesh" ||
                        part.dtype != "compas_xr/QRCode")
                {
                    if (attributes != null &&
                        attributes?.length != null &&
                        attributes?.width != null &&
                        attributes?.height != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

    }

    [System.Serializable]
    public class Part
    {
        /*
        * Part : A class to define the structure of a part in the assembly data structure.
        * It is based off the Compas data structure for a part, but not a direct corelation.
        */
        public Frame frame { get; set; }
        public string dtype { get; set; }

    }

    [System.Serializable]
    public class Attributes
    {
        /*
        * Attributes : A class to define the attributes of the node item.
        */
        public float length { get; set; }
        public float width { get; set; }
        public float height { get; set; }
    } 

    [System.Serializable]
    public class Frame
    {
        /*
        * Frame : A class to define the structure of a frame in the assembly data structure.
        * It is based off the Compas data structure for a frame.
        */
        public float[] point { get; set; }
        public float[] xaxis { get; set; }
        public float[] yaxis { get; set; }

        public static Frame Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Frame class from a json string.
            */
            Dictionary<string, object> frameDataDict = jsondata as Dictionary<string, object>;;
            return FromData(frameDataDict);
        }
        public static Frame FromData(Dictionary<string, object> frameDataDict)
        {            
            /*
            * Method to create an instance of a the Frame class from a dictionary.
            */
            Frame frame = new Frame();
            float[] point = DataConverters.ConvertDatatoFloatArray(frameDataDict["point"]);
            float[] xaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["xaxis"]);
            float[] yaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["yaxis"]);

            if (point == null || xaxis == null || yaxis == null)
            {
                Debug.LogError("FrameParse: One or more arrays is null.");
            }
            else
            {
                frame.point = point;
                frame.xaxis = xaxis;
                frame.yaxis = yaxis;
            }

            return frame;
        }
        public Dictionary<string, object> GetData()
        {
            /*
            * Method to return the frame data as a dictionary.
            */
            return new Dictionary<string, object>
            {
                { "point", point },
                { "xaxis", xaxis },
                { "yaxis", yaxis }
            };
        }
        public static Frame RhinoWorldXY()
        {
            /*
            * Returns a frame that represents the world XY plane in Rhino coordinates.
            */
            Frame frame = new Frame();
            frame.point = new float[] { 0.0f, 0.0f, 0.0f };
            frame.xaxis = new float[] { 1.0f, 0.0f, 0.0f };
            frame.yaxis = new float[] { 0.0f, 1.0f, 0.0f };
            return frame;
        }

    }

    /////////////// Classes For Building Plan Desearialization///////////////////
    
    [System.Serializable]
    public class BuildingPlanData
    {
        /*
        * BuildingPlanData : A class to define the structure of a building plan in the assembly data structure.
        * It is based off the Compas data structure for a building plan.
        * The building plan contains a dictionary of steps required for assembly, and the last built index.
        */
        public string LastBuiltIndex { get; set; }
        public Dictionary<string, Step> steps { get; set; }
        public Dictionary<string, List<string>> PriorityTreeDictionary { get; set; }
        public static BuildingPlanData Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the BuildingPlanData class from a json
            * Returns BuildingPlanData Class
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            BuildingPlanData buildingPlanData = BuildingPlanData.FromData(jsonDataDict);
            return buildingPlanData;
        }
        public static BuildingPlanData FromData(Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to create an instance of a the BuildingPlanData class from a dictionary.
            * Returns BuildingPlan Class instance & PriorityTreeDictionary
            */
            BuildingPlanData buidingPlanData = new BuildingPlanData();
            buidingPlanData.steps = new Dictionary<string, Step>();
            buidingPlanData.PriorityTreeDictionary = new Dictionary<string, List<string>>();
            if (jsonDataDict.TryGetValue("LastBuiltIndex", out object last_built_index))
            {
                Debug.Log($"Last Built Index Fetched From database: {last_built_index.ToString()}");
                buidingPlanData.LastBuiltIndex = last_built_index.ToString();
            }
            else
            {
                buidingPlanData.LastBuiltIndex = null;
            }
            List<object> stepsList = jsonDataDict["steps"] as List<object>;
            for(int i =0 ; i < stepsList.Count; i++)
            {
                string key = i.ToString();
                var json_data = stepsList[i];
                Step step_data = Step.Parse(json_data);
                
                if (step_data.IsValidStep())
                {
                    buidingPlanData.steps[key] = step_data;
                    Debug.Log($"FromData: BuildingPlan Step {key} successfully added to the building plan dictionary");

                    if (buidingPlanData.PriorityTreeDictionary.ContainsKey(step_data.data.priority.ToString()))
                    {
                        buidingPlanData.PriorityTreeDictionary[step_data.data.priority.ToString()].Add(key);
                    }
                    else
                    {
                        buidingPlanData.PriorityTreeDictionary[step_data.data.priority.ToString()] = new List<string>();
                        buidingPlanData.PriorityTreeDictionary[step_data.data.priority.ToString()].Add(key);
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
                }
            }
            return buidingPlanData;
        }
    }

    [System.Serializable]
    public class Step
    {
        /*
        * Step : A class to define the structure of a step in the building plan data structure.
        * It is based off the Compas data structure for a step.
        * The step contains the data required for a single step of the building process
        */
        public Data data { get; set; }
        public string dtype { get; set; }
        public string guid { get; set; }
        public static Step Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Step class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Step FromData(Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to create an instance of a the Step class from a dictionary.
            */
            Step step = new Step();
            step.dtype = (string)jsonDataDict["dtype"];
            step.guid = (string)jsonDataDict["guid"];

            Dictionary<string, object> dataDict = jsonDataDict["data"] as Dictionary<string, object>;
            step.data = Data.FromData(dataDict);
            return step;
        }
        public static bool AreEqualSteps(Step step ,Step NewStep)
        {
            /*
            * Method to compare two steps and check if they are equal.
            */
            if (step != null &&
                NewStep != null &&
                step.data.device_id == NewStep.data.device_id &&
                step.data.element_ids == step.data.element_ids &&
                step.data.actor == NewStep.data.actor &&
                step.data.location.point.SequenceEqual(NewStep.data.location.point) &&
                step.data.location.xaxis.SequenceEqual(NewStep.data.location.xaxis) &&
                step.data.location.yaxis.SequenceEqual(NewStep.data.location.yaxis) &&
                step.data.geometry == NewStep.data.geometry &&
                step.data.instructions.SequenceEqual(NewStep.data.instructions) &&
                step.data.is_built == NewStep.data.is_built &&
                step.data.is_planned == NewStep.data.is_planned &&
                step.data.elements_held.SequenceEqual(NewStep.data.elements_held) &&
                step.data.priority == NewStep.data.priority)
            {
                return true;
            }
            return false;
        }
        public bool IsValidStep()
        {
            /*
            * Method to check if the step contains all valid information.
            */
            if (data != null &&
                data.element_ids != null &&
                !string.IsNullOrEmpty(data.actor) &&
                data.location != null &&
                data.geometry != null &&
                data.instructions != null &&
                data.is_built != null &&
                data.is_planned != null &&
                data.elements_held != null &&
                data.priority != null)
            {
                return true;
            }
            return false;
        }

    }

    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////
    [System.Serializable]
    public class Data
    {
        /*
        * Data : A class to define the structure of a data in the building plan data structure.
        * data contains the information required for coordinating the building process
        */
        public string device_id { get; set; }
        public string[] element_ids { get; set; }
        public string actor { get; set; }
        public Frame location { get; set; }
        public string geometry { get; set; }
        public string[] instructions { get; set; }
        public bool is_built { get; set; }
        public bool is_planned { get; set; }
        public int[] elements_held { get; set; }
        public int priority { get; set; }
        public Frame robot_AA_base_frame { get; set; }
        public Frame robot_AB_base_frame { get; set; }
        public string robot_name { get; set; }

        public static Data Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Data class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Data FromData(Dictionary<string, object> dataDict)
        {
            /*
            * Method to create an instance of a the Data class from a dictionary.
            */
            Data data = new Data();
            Dictionary<string, object> locationDataDict = dataDict["location"] as Dictionary<string, object>;
            data.location = Frame.FromData(locationDataDict);

            if (dataDict.TryGetValue("device_id", out object device_id))
            {
                data.device_id = device_id.ToString();
            }
            else
            {
                data.device_id = null;
            }
            data.actor = (string)dataDict["actor"];
            data.geometry = (string)dataDict["geometry"];
            data.is_built = (bool)dataDict["is_built"];
            data.is_planned = (bool)dataDict["is_planned"];
            data.priority = (int)(long)dataDict["priority"];

            List<object> element_ids = dataDict["element_ids"] as List<object>;
            List<object> instructions = dataDict["instructions"] as List<object>;
            List<object> elements_held = dataDict["elements_held"] as List<object>;
            if (element_ids != null &&
                instructions != null &&
                elements_held != null)
            {
                data.elements_held = elements_held.Select(Convert.ToInt32).ToArray();
                data.element_ids = element_ids.Select(x => x.ToString()).ToArray();
                data.instructions = instructions.Select(x => x.ToString()).ToArray();
            }
            else
            {
                Debug.Log("FromData (Data): One of the lists is null or improperly casted.");
            }

            //PARSING ADDITIONAL ROBARCH DATA
            Dictionary<string, object> robotAABaseframeDataDict = dataDict["robot_AA_base_frame"] as Dictionary<string, object>;
            data.robot_AA_base_frame = Frame.FromData(robotAABaseframeDataDict);
            Dictionary<string, object> robotABBaseframeDataDict = dataDict["robot_AB_base_frame"] as Dictionary<string, object>;
            data.robot_AB_base_frame = Frame.FromData(robotABBaseframeDataDict);
            data.robot_name = (string)dataDict["robot_name"];
            return data;
        }

    }

    [System.Serializable]
    public class Joint
    {
        /*
        * Joint : A class to define the structure of a joint in the building plan data structure.
        * Joint contains the information required for coordinating the building process
        */
        public List<string> adjacency { get; set; }
        public Element element { get; set; }
        public bool is_mirrored { get; set; }
        public string Key { get; set; }
        public static Joint Parse(string key, object jsondata)
        {
            /*
            * Method to create an instance of a the Joint class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(key, jsonDataDict);
        }
        public static Joint FromData(string key, Dictionary<string, object> dataDict)
        {
            /*
            * Method to create an instance of a the Joint class from a dictionary.
            */
            Joint joint = new Joint();
            joint.Key = key;

            //Parse adjacency and convert to string list
            List<object> adjacencyList = dataDict["adjacency"] as List<object>;
            joint.adjacency = adjacencyList.Select(x => x.ToString()).ToList();

            //Parse element
            joint.element = Element.Parse(dataDict["element"]);
            return joint;
        }
        public bool IsValidJoint()
        {
            /*
            * Method to check if the joint contains all valid information.
            */
            if (adjacency != null &&
                element != null &&
                is_mirrored != null)
            {
                return true;
            }
            return false;
        }

    }

    [System.Serializable]
    public class Element
    {
        /*
        * Element : A class to define the structure of a element in the joints.
        * Element contains the information required for coordinating joint positions the building process
        */
        public Frame frame1 { get; set; }
        public Frame frame2 { get; set; }
        public static Element Parse(object jsondata)
        {
            /*
            * Method to create an instance of a the Element class from a json string.
            */
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Element FromData(Dictionary<string, object> jsonDataDict)
        {
            /*
            * Method to create an instance of a the Element class from a dictionary.
            */
            Element element = new Element();
            element.frame1 = Frame.FromData(jsonDataDict["frame1"] as Dictionary<string, object>);
            element.frame2 = Frame.FromData(jsonDataDict["frame2"] as Dictionary<string, object>);
            return element;
        }
    }
    //TODO: Extended for RobArch2024/////////////////////////////////////////////////////////////////////////////////

    ////////////////Classes for User Current Informatoin/////////////////////
    
    [System.Serializable]
    public class UserCurrentInfo
    {
        /*
        * UserCurrentInfo : A class to define the structure of a user current information in the building plan data structure.
        * UserCurrentInfo contains the information required for tracking multiple users across the building process
        */
        public string currentStep { get; set; }
        public string timeStamp { get; set; }
        public static UserCurrentInfo Parse(object jsondata)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static UserCurrentInfo FromData(Dictionary<string, object> jsonDataDict)
        {
            //Create class instances of node elements
            UserCurrentInfo userCurrentInfo = new UserCurrentInfo();
            userCurrentInfo.currentStep = (string)jsonDataDict["currentStep"];
            userCurrentInfo.timeStamp = (string)jsonDataDict["timeStamp"];
            return userCurrentInfo;
        }

    }
}