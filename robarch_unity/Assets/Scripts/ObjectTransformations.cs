using UnityEngine;
using CompasXR.Core.Data;
using System.Collections.Generic;
namespace CompasXR.Core
{
    public static class ObjectTransformations
    {
        /*
        * ObjectTransformations : Class is used for conversions of GameObjects positions and rotations based on input data.
        * The primary functions are used to convert object position and rotation data from Rhino to Unity and vice versa.
        * Additionally it contains methods for translating GameObjects based on Image Target data, GameObjects, and vectors.
        */
        public struct Rotation
        {
            /*
            * Rotation : Struct is used to define the rotation of an object in 3D space.
            * The struct contains three Vector3 objects for the x, y, and z axis.
            */
            public Vector3 x;
            public Vector3 y;
            public Vector3 z;
        }
        public static Quaternion GetQuaternionFromFrameDataForObj(Rotation rotation, bool z_to_y_remapped)
        {   
            /*
            * Method used to calculate a quaternion from RightHand frame data for .obj loaded objects.
            * The method takes a Rotation struct and a boolean value to determine
            */
            Rotation rotationLh = RightHandToLeftHand(rotation.x , rotation.y);
            Rotation Zrotation = ZRotation(rotationLh);
            Rotation ObjectRotation;
            if (!z_to_y_remapped == true)
            {
                ObjectRotation = XRotation(Zrotation);
            }
            else
            {
                ObjectRotation = Zrotation;
            }
            Quaternion rotationQuaternion = GetQuaternion(ObjectRotation.y, ObjectRotation.z);
            return rotationQuaternion;
        } 
        public static Quaternion GetQuaternionFromFrameDataForUnityObject(Rotation rotation)
        {   
            /*
            * Method used to calculate a quaternion from  from RightHand frame data Unity created Objects.
            * The method takes a Rotation struct and returns a Quaternion object.
            */
            Rotation rotationLh = RightHandToLeftHand(rotation.x , rotation.y);
            Quaternion rotationQuaternion = GetQuaternion(rotationLh.y, rotationLh.z);
            return rotationQuaternion;
        } 
        public static Vector3 GetPositionFromRightHand(float[] pointlist)
        {
            /*
            * Method used to convert a float array of point data from RightHand to a Vector3 position in Unity space.
            */
            Vector3 position = new Vector3(pointlist[0], pointlist[2], pointlist[1]);
            return position;
        }
        public static Rotation GetRotationFromRightHand(float[] x_vecdata, float [] y_vecdata)
        {
            /*
            * Method used to convert float arrays of x and y axis data from RightHand to vectors that are the right hand coordinate space.
            */
            Vector3 x_vec_right = new Vector3(x_vecdata[0], x_vecdata[1], x_vecdata[2]);
            Vector3 y_vec_right  = new Vector3(y_vecdata[0], y_vecdata[1], y_vecdata[2]);
            Rotation rotationRH;
            rotationRH.x = x_vec_right;
            rotationRH.y = y_vec_right;
            rotationRH.z = Vector3.Cross(x_vec_right, y_vec_right);
            return rotationRH;
        } 
        public static Rotation RightHandToLeftHand(Vector3 x_vec_right, Vector3 y_vec_right)
        {
            /*
            * Method used to convert RightHand Vectors to LeftHand Vectors.
            */
            Vector3 x_vec = new Vector3(x_vec_right[0], x_vec_right[2], x_vec_right[1]);
            Vector3 z_vec = new Vector3(y_vec_right[0], y_vec_right[2], y_vec_right[1]);
            Vector3 y_vec = Vector3.Cross(z_vec, x_vec);
            Rotation rotationLh;
            rotationLh.x = x_vec;
            rotationLh.z = z_vec;
            rotationLh.y = y_vec;
            return rotationLh;
        }
        public static Quaternion GetQuaternion(Vector3 y_vec, Vector3 z_vec)
        {
            /*
            * Method used to calculate a quaternion from y and z axis vectors.
            */
            Quaternion rotation = Quaternion.LookRotation(z_vec, y_vec);
            return rotation;
        }
        public static Rotation ZRotation(Rotation ObjectRotation)
        {
            /*
            * Method used to rotate a Rotation struct 180 degrees around the Z axis.
            * The method takes a Rotation struct and returns a new Rotation struct.
            * The method is used for obj import correction.
            */
            Vector3 x_vec = ObjectRotation.x;
            Vector3 z_vec = ObjectRotation.z;
            Vector3 y_vec = ObjectRotation.y;
            
            Quaternion z_rotation = Quaternion.AngleAxis(180, z_vec);
            x_vec = z_rotation * x_vec;
            y_vec = z_rotation * y_vec;
            z_vec = z_rotation * z_vec;

            Rotation ZXrotation;
            ZXrotation.x = x_vec;
            ZXrotation.y = y_vec;
            ZXrotation.z = z_vec;

            return ZXrotation;
        }
        public static Rotation XRotation(Rotation ObjectRotation)
        {
            /*
            * Method used to rotate a Rotation struct 90 degrees around the X axis.
            * The method takes a Rotation struct and returns a new Rotation struct.
            * The method is used for obj import correction.
            */
            Vector3 x_vec = ObjectRotation.x;
            Vector3 z_vec = ObjectRotation.z;
            Vector3 y_vec = ObjectRotation.y;

            Quaternion rotation_x = Quaternion.AngleAxis(90f, x_vec);
            x_vec = rotation_x * x_vec;
            y_vec = rotation_x * y_vec;
            z_vec = rotation_x * z_vec;

            Rotation ZXrotation;
            ZXrotation.x = x_vec;
            ZXrotation.y = y_vec;
            ZXrotation.z = z_vec;

            return ZXrotation;
        }
        public static Vector3 FindGameObjectCenter(GameObject gameObject)
        {
            /*
            * Method used to find the center of a GameObject.
            * The method takes a GameObject and returns a Vector3 position.
            */
            Renderer renderer = gameObject.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("Renderer not found in the parent object.");
                return Vector3.zero;
            }
            Vector3 center = renderer.bounds.center;
            return center;
        }
        public static Vector3 OffsetPositionVectorByDistance(Vector3 position, float offsetDistance, string axis)
        {
            /*
            * Method used to offset a Vector3 position by a distance along a specified axis.
            */
            switch (axis)
            {
                case "x":
                {
                    Vector3 offsetPosition = new Vector3(position.x + offsetDistance, position.y, position.z);
                    return offsetPosition;
                }
                case "y":
                {
                    Vector3 offsetPosition = new Vector3(position.x, position.y + offsetDistance, position.z);
                    return offsetPosition;
                }
                case "z":
                {
                    Vector3 offsetPosition = new Vector3(position.x, position.y, position.z + offsetDistance);
                    return offsetPosition;
                }
            }
            return Vector3.zero;
        }
        public static void OffsetGameObjectPositionByExistingObjectPosition(GameObject gameObject, GameObject existingObject, float offsetDistance, string axis)
        {
            /*
            * Method used to offset a GameObject position by another gameObjects vector.
            */
            Vector3 center = FindGameObjectCenter(existingObject);
            switch (axis)
            {
                case "x":
                {
                    Vector3 offsetPosition = new Vector3(center.x + offsetDistance, center.y, center.z);
                    gameObject.transform.position = offsetPosition;
                    break;
                }
                case "y":
                {
                    Vector3 offsetPosition = new Vector3(center.x, center.y + offsetDistance, center.z);
                    gameObject.transform.position = offsetPosition;
                    break;
                }
                case "z":
                {
                    Vector3 offsetPosition = new Vector3(center.x, center.y, center.z + offsetDistance);
                    gameObject.transform.position = offsetPosition;
                    break;
                }
            }
        }
        public static Frame ConvertGameObjectToRightHandFrameData(GameObject gameObject)
        {
            /*
            * Method used to convert a GameObject to a Frame data object.
            * The method takes a GameObject and returns a Frame object calculated from the objects position & rotation.
            */
            (float[] pointData, float[] xaxisData, float[] yaxisData) = FromUnityToRhinoConversion(gameObject);
            Frame frame = new Frame();
            frame.point = pointData;
            frame.xaxis = xaxisData;
            frame.yaxis = yaxisData;
            return frame;
        }
        public static (float[], float[], float[]) FromUnityToRhinoConversion(GameObject gameObject)
        {
            /*
            * Method used to convert a GameObjects position and rotation from Unity to Rhino.
            * The method takes a GameObject and returns three float arrays for the point, xaxis, and yaxis.
            */
            float[] position = GetPositionFromLeftHand(gameObject);
            Rotation rotation = GetRotationFromLeftHand(gameObject);
            (float[] x_vecdata, float[] y_vecdata) = LeftHandToRightHand(rotation.x, rotation.z);
            return (position, x_vecdata, y_vecdata);
        }
        public static float[] GetPositionFromLeftHand(GameObject gameObject)
        {
            /*
            * Method used to convert a GameObjects position from LeftHand to RightHand.
            * The method takes a GameObject and returns a float array of the position.
            */
            Vector3 objectPosition = gameObject.transform.position;
            float [] objectPositionArray = new float[3] {objectPosition.x, objectPosition.y,  objectPosition.z};
            float[] convertedPosition = new float [3] {objectPositionArray[0], objectPositionArray[2], objectPositionArray[1]};
            return convertedPosition;
        }
        public static Rotation GetRotationFromLeftHand(GameObject gameObject)
        {
            /*
            * Method used to convert a GameObjects rotation from LeftHand to RightHand.
            * The method takes a GameObject and returns a Rotation struct.
            */
            Vector3 objectWorldZ = gameObject.transform.TransformDirection(gameObject.transform.forward);
            Vector3 objectWorldX = gameObject.transform.TransformDirection(gameObject.transform.right);

            float[] x_vecdata = new float[3] {objectWorldX.x, objectWorldX.y, objectWorldX.z};
            float[] z_vecdata = new float[3] {objectWorldZ.x, objectWorldZ.y, objectWorldZ.z};

            Vector3 x_vec_left = new Vector3(x_vecdata[0], x_vecdata[1], x_vecdata[2]);
            Vector3 z_vec_left  = new Vector3(z_vecdata[0], z_vecdata[1], z_vecdata[2]);
            
            Rotation rotationLH;
            
            rotationLH.x = x_vec_left;
            rotationLH.y = Vector3.Cross(z_vec_left, x_vec_left);
            rotationLH.z = z_vec_left;
            
            return rotationLH;
        }
        public static (float[], float[]) LeftHandToRightHand(Vector3 x_vec_left, Vector3 z_vec_left)
        {        
            /*
            * Method used to convert LeftHand vectors to RightHand vectors.
            * The method takes two Vector3 objects and returns two float arrays.
            */
            Vector3 x_vec = new Vector3(x_vec_left[0], x_vec_left[2], x_vec_left[1]);
            Vector3 y_vec = new Vector3(z_vec_left[0], z_vec_left[2], z_vec_left[1]);
            Vector3 z_vec = Vector3.Cross(y_vec, x_vec);
            float[] x_vecdata = new float[3] {x_vec_left[0], x_vec_left[2], x_vec_left[1]};
            float[] y_vecdata = new float[3] {z_vec_left[0], z_vec_left[2], z_vec_left[1]};
            return (x_vecdata, y_vecdata);
        }      
        public static Vector3 TranslateGameObjectsPositionFromImageTarget(GameObject gameObject, Vector3 rightHandPositionData, Quaternion calculatedObjectQuaternion)
        {
            /*
            * Method used to translate a GameObjects position from an Image Target.
            * The method takes a GameObject, a Vector3 position, and a Quaternion object and returns a Vector3 position.
            */
            Vector3 pos = gameObject.transform.position + (gameObject.transform.rotation * Quaternion.Inverse(calculatedObjectQuaternion) * -rightHandPositionData);
            return pos;
        }
        public static Quaternion TranslateGameObjectRotationFromImageTarget(GameObject imageTargetGameObject, float[] targetXAxis, float[] targetYAxis)
        {
            /*
            * Method used to translate a GameObjects rotation from an Image Target.
            * The method takes a GameObject, and two float arrays for the x and y axis and returns a Quaternion object.
            */
            Rotation rotationData = GetRotationFromRightHand(targetXAxis, targetYAxis);
            Quaternion rotationQuaternion = GetQuaternionFromFrameDataForUnityObject(rotationData);
            Quaternion rot = imageTargetGameObject.transform.rotation * Quaternion.Inverse(rotationQuaternion);
            return rot;
        }
        public static void TranslateGameObjectByImageTarget(GameObject gameObject, GameObject imageTargetGameObject, float[] targetPoint, float[] targetXAxis, float[] targetYAxis)
        {
            /*
            * Method used to translate a GameObjects position and rotation from an Image Target.
            * The method takes a GameObject, an Image Target GameObject, and three float arrays for the point, xaxis, and yaxis.
            */
            Rotation rotationData = GetRotationFromRightHand(targetXAxis, targetYAxis);
            Quaternion rotationQuaternion = GetQuaternionFromFrameDataForUnityObject(rotationData);
            Quaternion rot = imageTargetGameObject.transform.rotation * Quaternion.Inverse(rotationQuaternion);
            Vector3 positionData = ObjectTransformations.GetPositionFromRightHand(targetPoint);
            Vector3 pos = ObjectTransformations.TranslateGameObjectsPositionFromImageTarget(imageTargetGameObject, positionData, rotationQuaternion);
            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;
        }
        public static void TranslateGameObjectListByImageTarget(List<GameObject> gameObjectsList, GameObject imageTargetGameObject, float[] targetPoint, float[] targetXAxis, float[] targetYAxis)
        {
            /*
            * Method used to translate a list of GameObjects positions and rotations from an Image Target.
            * The method takes a List of GameObjects, an Image Target GameObject, and three float arrays for the point, xaxis, and yaxis.
            */
            foreach (GameObject gameObject in gameObjectsList)
            {
                TranslateGameObjectByImageTarget(gameObject, imageTargetGameObject, targetPoint, targetXAxis, targetYAxis);
            }
        }

    }
}
