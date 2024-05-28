using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CompasXR.Core;
using CompasXR.Core.Data;
using CompasXR.Core.Extentions;

namespace CompasXR.UI
{
    /*
    * CompasXR.UI : Is the namespace for all Classes that
    * controll the primary functionalities releated to the User Interface in the CompasXR Application.
    * Functionalities, such as UI interaction, UI element creation, and UI element control.
    */
    public class ScrollSearchManager : MonoBehaviour
    {
        //Other Scripts and global Objects for in script use
        public DatabaseManager databaseManager;
        public InstantiateObjects instantiateObjects;
        public UIFunctionalities uiFunctionalites;
        public GameObject Elements;

        //Public Variables
        public RectTransform scrollablePanel;
        public RectTransform container;
        public GameObject cellsParent;
        public List<GameObject> cells;
        public RectTransform center;
        public GameObject ScrollSearchObjects;
        public GameObject cellPrefab;

        //Private Variables
        public bool cellsExist = false;
        public float[] cellDistances;
        private bool dragging = false;
        public int cellSpacing = -20;
        private int closestCellIndex;
        private int? selectedCellIndex = null;
        public string selectedCellStepIndex;

        /////////////////////////////////////////////// Monobehaviour Methods //////////////////////////////////////////////////////////             
        void Start()
        {
            OnStartInitilization();
        }
        void Update()
        {
            ScrollSearchControler(ref cellsExist);
        }

        /////////////////////////////////////////////// Initilization and Control Methods ///////////////////////////////////////////////       
        private void OnStartInitilization()
        {
            /*
            * Method is used to initialize all the required variables and objects for the ScrollSearchManager script.
            * The method is called in the Start() method of the Monobehaviour.
            */

            //Find Other script Objects
            databaseManager = GameObject.Find("DatabaseManager").GetComponent<DatabaseManager>();
            instantiateObjects = GameObject.Find("Instantiate").GetComponent<InstantiateObjects>();
            uiFunctionalites = GameObject.Find("UIFunctionalities").GetComponent<UIFunctionalities>();
            Elements = GameObject.Find("Elements");

            //Find Objects
            GameObject Canvas = GameObject.Find("Canvas");
            GameObject VisiblityEditor = Canvas.FindObject("Visibility_Editor");
            GameObject ScrollSearchToggle = VisiblityEditor.FindObject("ScrollSearchToggle");
            ScrollSearchObjects = ScrollSearchToggle.FindObject("ScrollSearchObjects");
            cellPrefab = ScrollSearchObjects.FindObject("CellPrefab");
            cellsParent = ScrollSearchObjects.FindObject("Container");

        }
        public void ScrollSearchControler(ref bool cellsExist)
        {
            /*
            * Method is used to control the scrolling and search functionality of the ScrollSearchManager script.
            * The method is called in the Update() method of the Monobehaviour.
            */
            if (cellsExist)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    cellDistances[i] = Mathf.Abs(center.transform.position.y - cells[i].transform.position.y);
                }
                float minDistance = Mathf.Min(cellDistances);

                for (int a = 0; a < cells.Count; a++)
                {
                    if (minDistance == cellDistances[a])
                    {
                        closestCellIndex = a;
                    }
                }

                if (!dragging)
                {
                    LerpToCell(closestCellIndex * - cellSpacing);
                    ScrollSearchObjectColor(ref closestCellIndex, ref selectedCellIndex, ref selectedCellStepIndex, ref cells);
                }
            }
        }

        /////////////////////////////////////////////// OnScreen Object Management /////////////////////////////////////////////////////   
        public void CreateCellsFromPrefab(ref GameObject prefabAsset, int cellSpacing, GameObject cellsParent, int cellCount, ref bool cellsExist)
        {
            /*
            * Method is used to create a list of cells from a prefab asset.
            */
            cellDistances = new float[cellCount];
            for (int i = 0; i < cellCount; i++)
            {
                GameObject cell = Instantiate(prefabAsset, cellsParent.transform);
                cell.SetActive(true);
                Vector2 newCellPosition = new Vector2(0 , i * cellSpacing);
                cell.GetComponent<RectTransform>().anchoredPosition = newCellPosition;
                cell.GetComponentInChildren<TMP_Text>().text = i.ToString();
                cell.name = $"Cell {i}";
                cells.Add(cell);
            }
            cellsExist = true;
        }
        public void ResetScrollSearch(ref bool cellsExist)
        {
            /*
            * Method is used to reset the scroll search functionality.
            */
            cellsExist = false;
            DestroyCellInfo(ref cells, ref cellDistances);

            if (selectedCellStepIndex != null)
            {
                Step step = databaseManager.BuildingPlanDataItem.steps[selectedCellStepIndex];
                GameObject objectToColor = Elements.FindObject(selectedCellStepIndex).FindObject(step.data.element_ids[0] + " Geometry");
                if (objectToColor != null)
                {
                    instantiateObjects.ObjectColorandTouchEvaluater(
                        instantiateObjects.visulizationController.VisulizationMode,
                        instantiateObjects.visulizationController.TouchMode,
                        step, selectedCellStepIndex, objectToColor);
                }

                if (uiFunctionalites.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                {
                    instantiateObjects.ColorObjectByPriority(uiFunctionalites.SelectedPriority, step.data.priority.ToString(), selectedCellStepIndex, objectToColor);
                }
            }
            selectedCellIndex = null;
        }
        public void DestroyCellInfo(ref List<GameObject> cells, ref float[] cellDistances)
        {
            /*
            * Method is used to destroy all the cells and cell information.
            */
            foreach (GameObject cell in cells)
            {
                Destroy(cell);
            }
            cells.Clear();
            cellDistances = new float[0];
        }
        void LerpToCell(int position)
        {
            /*
            * Method is used to lerp the scrollable panel to a specific cell position.
            * used to inturpolate the position of the scrollable panel to the position of the cell.
            */
            float newY = Mathf.Lerp(container.anchoredPosition.y, position, Time.deltaTime * 10f);
            Vector2 newPosition = new Vector2(container.anchoredPosition.x, newY);
            container.anchoredPosition = newPosition;
        }

        /////////////////////////////////////////////// Spatial Object Management /////////////////////////////////////////////////////
        public void ScrollSearchObjectColor(ref int closestCellIndex, ref int? selectedCellIndex, ref string selectedCellStepIndex, ref List<GameObject> cells)
        {
            /*
            * Method is used to control coloring the object that is being searched for in the scroll search functionality.
            */

            if (closestCellIndex != selectedCellIndex)
            {
                if (selectedCellIndex != null && selectedCellStepIndex != null)
                {
                    Step step = databaseManager.BuildingPlanDataItem.steps[selectedCellStepIndex];
                    GameObject objectToColor = Elements.FindObject(selectedCellStepIndex).FindObject(step.data.element_ids[0] + " Geometry");
                    if(objectToColor == null)
                    {
                        Debug.Log("ScrollSearchController: ObjectToColor is null.");
                    }

                    if (selectedCellStepIndex != uiFunctionalites.CurrentStep)
                    {
                        instantiateObjects.ObjectColorandTouchEvaluater(
                            instantiateObjects.visulizationController.VisulizationMode,
                            instantiateObjects.visulizationController.TouchMode,
                            step, selectedCellStepIndex, objectToColor);

                        if (uiFunctionalites.PriorityViewerToggleObject.GetComponent<Toggle>().isOn)
                        {
                            instantiateObjects.ColorObjectByPriority(uiFunctionalites.SelectedPriority, step.data.priority.ToString(), selectedCellStepIndex, objectToColor);
                        }
                    }
                    else
                    {
                        instantiateObjects.ColorHumanOrRobot(step.data.actor, step.data.is_built, objectToColor);
                    }
                    
                }

                selectedCellIndex = closestCellIndex;
                selectedCellStepIndex = GetTextItemFromGameObject(cells[closestCellIndex]);
                Step newStep = databaseManager.BuildingPlanDataItem.steps[selectedCellStepIndex];
                GameObject newObjectToColor = Elements.FindObject(selectedCellStepIndex).FindObject(newStep.data.element_ids[0] + " Geometry");

                if (newObjectToColor != null)
                {
                    Debug.Log($"ScrollSearchController: Coloring Object {selectedCellStepIndex} by searched color.");
                    instantiateObjects.ColorObjectbyInputMaterial(newObjectToColor, instantiateObjects.SearchedObjectMaterial);
                }
                else
                {
                    string message = $"WARNING: The item {selectedCellStepIndex} could not be found. Please retype information and try search again.";
                    UserInterface.SignalOnScreenMessageFromPrefab(ref uiFunctionalites.OnScreenErrorMessagePrefab, ref uiFunctionalites.SearchItemNotFoundWarningMessageObject, "SearchItemNotFoundWarningMessage", uiFunctionalites.MessagesParent, message, "ScrollSearchController: Could not find searched item.");
                }
            }
        }
        public string GetTextItemFromGameObject(GameObject gameObject)
        {
            /*
            * Method is used to get the text item from a game object.
            */
            return gameObject.GetComponentInChildren<TMP_Text>().text;
        }

        /////////////////////////////////////////////// Scrolling Object Event Methods  ////////////////////////////////////////////////
        public void StartDrag()
        {
            /*
            * Method is used to indicate the drag is active.
            */
            dragging = true;
        }
        public void EndDrag()
        {
            /*
            * Method is used to indicate the drag is inactive.
            */
            dragging = false;
        }

    }
}
