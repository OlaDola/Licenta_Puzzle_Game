using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility_Functions;
using Baracuda.Monitoring.API;
using Baracuda.Monitoring;

public class Grid_Show : MonoBehaviour
{
    public static bool isSaved;
    public static bool isEmpty;

    public static Grid_Show Instance { get; private set; }
    public event EventHandler OnSelectedChanged;
    public event EventHandler GridNumberChanged;
    public event EventHandler OnObjectPlaced;

    [SerializeField] CameraTest cameraTest;
    [SerializeField] ScreenShotHandler screenShotHandler;

    public enum PlaceObjectType
    {
        GroundObject
    }
    private int numberOfPlatforms;

    private PlaceObjectType placeObjectType;

    [SerializeField] private Transform planeTransform;
    private GameObject planeGameObject;

    [SerializeField] private List<PlacedObjectType> placedObjectsTypeList = null;
    private PlacedObjectType placedObjectType;

    [SerializeField] static int isPlayMode;

    public Grid<GridObject> grid;
    private PlacedObjectType.Dir dir = PlacedObjectType.Dir.Down;

    private List<Grid<GridObject>> gridList;

    public int gridSize;
    public float gridCellSize;

    private Vector3 middlePosition;

    private Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir,Dictionary<string, int>>> placedPositions;

    //[Monitor]
    private Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir, List<Vector2Int>>> portalConnectors;

    private bool isDemolishActive;
    private bool ChangeGridNumber;
    public PortalObjectType floorEdgeObjectTypeSO;

    private void Awake()
    {
        numberOfPlatforms = 0;
        MonitoringManager.RegisterTarget(this);
        Instance = this;
        gridList = new List<Grid<GridObject>>();
        ChangeGridNumber = true;
        grid = new Grid<GridObject>(gridSize, gridCellSize, new Vector3(0, 0, 0), (Grid<GridObject> g, int x, int z) => new GridObject(g, x, z));
        gridList.Add(grid);
        middlePosition = new Vector3(gridCellSize * gridSize / 2, 0, gridCellSize * gridSize / 2);
        planeTransform.localScale = new Vector3(gridCellSize * gridSize, 0, gridCellSize * gridSize);
        planeTransform = Instantiate(planeTransform, middlePosition, Quaternion.identity);
        planeTransform.name = "Plane";
        placedObjectType = null;
        ChangeGridNumber = false;
        placedPositions = new Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir, Dictionary<string, int>>>();
        portalConnectors = new Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir,List<Vector2Int>>>();
        
        isPlayMode = PlayerPrefs.GetInt("PlayMode");
        if (isPlayMode == 1)
        {
            if (PlayerPrefs.HasKey("LevelSystemSave"))
                Load(PlayerPrefs.GetString("LevelSystemSave"));
            GameObject.Find("PositionLocation").SetActive(false);
        }
        isEmpty = true;
    }

    public PlacedObjectType.Dir GetDir()
    {
        return dir;
    }
    public Vector3 GetMiddlePosition()
    {
        return middlePosition;
    }

    public Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir, List<Vector2Int>>> GetPortalConnectors()
    {
        return portalConnectors;
    }

    public Grid<GridObject> GetGridList()
    {
        return gridList[0];
    }

    public int GetGridSize()
    {
        return gridSize;
    }
    public class GridObject
    {
        private Grid<GridObject> grid;
        private int x, z;
        private PlacedObject placedObject;

        public GridObject(Grid<GridObject> grid, int x, int z)
        {
            this.grid = grid;
            this.x = x;
            this.z = z;
        }

        public void SetPlacedObject(PlacedObject placedObject)
        {
            this.placedObject = placedObject;
            grid.TriggerGridObjectChanged(x, z);
        }

        public void ClearPlacedObject()
        {
            placedObject = null;
            grid.TriggerGridObjectChanged(x, z);
        }

        public PlacedObject GetPlacedObject()
        {
            return placedObject;
        }

        public bool CanBuild()
        {
            return placedObject == null;
        }

        public override string ToString()
        {
            return x + ", " + z + "\n" + placedObject;
        }
    }
    private void Update()
    {
        if (!PauseMenu.isPause)
        {
            HandleGrid();
            HandleGroundObjectPlacement();
            HandleSingleDemolish();
            if (Input.GetKeyDown(KeyCode.X))
            {
                HandleAllDemolish();
            }
        }

        /*if (!cameraTest.GetOrthoOn())
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                dir = PlacedObjectType.GetNextDir(dir, Input.mouseScrollDelta.y);
                Utils_Class.CreateWorldTextPopup("" + dir, Mouse_3D.GetMouseWorldPosition());
            }
        }*/
    }

    private void HandleGrid()
    {
        if (ChangeGridNumber)
        {
            DeselectObjectType();
            placedPositions = new Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir, Dictionary<string, int>>>();
            portalConnectors = new Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir, List<Vector2Int>>>();
            SaveObject gridObjects = SaveChangeGridNumber();
            HandleAllDemolish();
            gridList.Remove(grid);
            grid.DestroyGrid();
            GameObject.Destroy(planeTransform.gameObject);
            grid = new Grid<GridObject>(gridSize, gridCellSize, new Vector3(0, 0, 0), (Grid<GridObject> g, int x, int z) => new GridObject(g, x, z));
            gridList.Add(grid);
            LoadChangeGridNumber(gridObjects);
            middlePosition = new Vector3(gridCellSize * gridSize / 2, 0, gridCellSize * gridSize / 2);
            planeTransform.localScale = new Vector3(gridCellSize * gridSize, 0, gridCellSize * gridSize);
            planeTransform = Instantiate(planeTransform, middlePosition, Quaternion.identity);
            planeTransform.name = "Plane";
            placedObjectType = null;
            ChangeGridNumber = false;
            cameraTest.HandleGridNumberChange();
            RefreshGridSizeNumber();
            isSaved = false;
        } 
    }

    public void HandlePortalPlacement()
    {
        foreach(Vector2Int position1 in placedPositions.Keys)
        {
            Dictionary<PlacedObjectType.Dir, Dictionary<string, int>> position1Dic = placedPositions[position1];
            foreach (Vector2Int position2 in placedPositions.Keys) 
            {
                Dictionary<PlacedObjectType.Dir, Dictionary<string, int>> position2Dic = placedPositions[position2];
                
                foreach (PlacedObjectType.Dir dirDic1 in position1Dic.Keys)
                {
                    Dictionary<string, int> portalsDic1 = position1Dic[dirDic1];

                    foreach (PlacedObjectType.Dir dirDic2 in position2Dic.Keys)
                    {

                        if (dirDic1 != dirDic2)
                        {
                            Dictionary<string, int> portalsDic2 = position2Dic[dirDic2];

                            if (portalsDic1.ContainsKey("y") && portalsDic2.ContainsKey("y") && portalsDic1["y"] == portalsDic2["y"])
                            {
                                Dictionary<PlacedObjectType.Dir, List<Vector2Int>> connectedPlatforms1 = portalConnectors[position1];
                                Dictionary<PlacedObjectType.Dir, List<Vector2Int>> connectedPlatforms2 = portalConnectors[position2];

                                List<Vector2Int> connectedLocations1;
                                if (connectedPlatforms1.ContainsKey(dirDic1))
                                    connectedLocations1 = connectedPlatforms1[dirDic1];
                                else
                                    connectedLocations1 = new List<Vector2Int>();
                                List<Vector2Int> connectedLocations2;
                                if (connectedPlatforms2.ContainsKey(dirDic2))
                                    connectedLocations2 = connectedPlatforms2[dirDic2];
                                else
                                    connectedLocations2 = new List<Vector2Int>();
                                if (position1.x != position2.x && position1.y != position2.y)
                                {
                                    if (portalsDic1["Portal"] == 0)
                                    {
                                        PlacedObject placedObject = gridList[0].GetGridObject(position1.x, position1.y).GetPlacedObject();
                                        placedObject.TryGetComponent(out PlatformObject platformObject);
                                        switch (dirDic1)
                                        {
                                            case PlacedObjectType.Dir.Up:
                                                platformObject.PlacePortal(PlatformObject.Portal.Back, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                            case PlacedObjectType.Dir.Down:
                                                platformObject.PlacePortal(PlatformObject.Portal.Front, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                        }
                                    }
                                    if (portalsDic2["Portal"] == 0)
                                    {
                                        PlacedObject placedObject = gridList[0].GetGridObject(position2.x, position2.y).GetPlacedObject();
                                        placedObject.TryGetComponent(out PlatformObject platformObject);
                                        switch (dirDic2)
                                        {
                                            case PlacedObjectType.Dir.Up:
                                                platformObject.PlacePortal(PlatformObject.Portal.Back, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                            case PlacedObjectType.Dir.Down:
                                                platformObject.PlacePortal(PlatformObject.Portal.Front, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                        }
                                    }
                                }
                                if (!connectedLocations1.Contains(position2))
                                {
                                    connectedLocations1.Add(position2);
                                    if (position1.x != position2.x && position1.y != position2.y)
                                        placedPositions[position1][dirDic1]["Portal"]++;
                                }
                                if (!connectedLocations2.Contains(position1))
                                {
                                    connectedLocations2.Add(position1);
                                    if (position1.x != position2.x && position1.y != position2.y)
                                        placedPositions[position2][dirDic2]["Portal"]++;
                                }
                                connectedPlatforms1[dirDic1] = connectedLocations1;
                                connectedPlatforms2[dirDic2] = connectedLocations2;
                                portalConnectors[position1] = connectedPlatforms1;
                                portalConnectors[position2] = connectedPlatforms2;

                            }
                            if (portalsDic1.ContainsKey("x") && portalsDic2.ContainsKey("x") && portalsDic1["x"] == portalsDic2["x"])
                            {
                                Dictionary<PlacedObjectType.Dir, List<Vector2Int>> connectedPlatforms1 = portalConnectors[position1];
                                Dictionary<PlacedObjectType.Dir, List<Vector2Int>> connectedPlatforms2 = portalConnectors[position2];

                                List<Vector2Int> connectedLocations1;
                                if (connectedPlatforms1.ContainsKey(dirDic1))
                                    connectedLocations1 = connectedPlatforms1[dirDic1];
                                else
                                    connectedLocations1 = new List<Vector2Int>();
                                List<Vector2Int> connectedLocations2;
                                if (connectedPlatforms2.ContainsKey(dirDic2))
                                    connectedLocations2 = connectedPlatforms2[dirDic2];
                                else
                                    connectedLocations2 = new List<Vector2Int>();


                                if (position1.x != position2.x && position1.y != position2.y)
                                {
                                    if (portalsDic1["Portal"] == 0)
                                    {
                                        PlacedObject placedObject = gridList[0].GetGridObject(position1.x, position1.y).GetPlacedObject();
                                        placedObject.TryGetComponent(out PlatformObject platformObject);
                                        switch (dirDic1)
                                        {
                                            case PlacedObjectType.Dir.Left:
                                                platformObject.PlacePortal(PlatformObject.Portal.Left, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                            case PlacedObjectType.Dir.Right:
                                                platformObject.PlacePortal(PlatformObject.Portal.Right, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                        }
                                    }
                                    if (portalsDic2["Portal"] == 0)
                                    {
                                        PlacedObject placedObject = gridList[0].GetGridObject(position2.x, position2.y).GetPlacedObject();
                                        placedObject.TryGetComponent(out PlatformObject platformObject);
                                        switch (dirDic2)
                                        {
                                            case PlacedObjectType.Dir.Left:
                                                platformObject.PlacePortal(PlatformObject.Portal.Left, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                            case PlacedObjectType.Dir.Right:
                                                platformObject.PlacePortal(PlatformObject.Portal.Right, floorEdgeObjectTypeSO, placedObject.transform);
                                                break;
                                        }
                                    }
                                }

                                if (!connectedLocations1.Contains(position2))
                                {
                                    connectedLocations1.Add(position2);
                                    if (position1.x != position2.x && position1.y != position2.y)
                                        placedPositions[position1][dirDic1]["Portal"]++;
                                }
                                if (!connectedLocations2.Contains(position1))
                                {
                                    connectedLocations2.Add(position1);
                                    if (position1.x != position2.x && position1.y != position2.y)
                                        placedPositions[position2][dirDic2]["Portal"]++;
                                }

                                connectedPlatforms1[dirDic1] = connectedLocations1;
                                connectedPlatforms2[dirDic2] = connectedLocations2;
                                portalConnectors[position1] = connectedPlatforms1;
                                portalConnectors[position2] = connectedPlatforms2;

                            }
                        }
                    }
                }
            }
        }
        //foreach (Vector2Int platforms in portalConnectors.Keys)
        //{
        //    Debug.Log("Platform:" + platforms);
        //    foreach (PlacedObjectType.Dir connectedPlatforms in portalConnectors[platforms].Keys)
        //    {
        //        Debug.Log("Connected " + connectedPlatforms + ":");
        //        foreach (Vector2Int position in portalConnectors[platforms][connectedPlatforms])
        //        {
        //            Debug.Log(position);
        //        }
        //    }
        //}
    }

    public void HandlePortalRemoval(Vector2Int portalPosition)
    {
        Dictionary<PlacedObjectType.Dir, List<Vector2Int>> portalsToDelete = portalConnectors[portalPosition];
        foreach (PlacedObjectType.Dir dir in portalsToDelete.Keys) 
        {
            foreach (Vector2Int position in portalsToDelete[dir])
            {
                PlacedObject placedObject = gridList[0].GetGridObject(position.x, position.y).GetPlacedObject();
                //Debug.Log(position);
                placedObject.TryGetComponent(out PlatformObject platformObject);
                switch (dir)
                {
                    case PlacedObjectType.Dir.Left:
                        placedPositions[position][PlacedObjectType.Dir.Right]["Portal"] -= 1;
                        if (placedPositions[position][PlacedObjectType.Dir.Right]["Portal"] == 0)
                        {
                            PortalObject portalObjectR = platformObject.GetPortalObject(PlatformObject.Portal.Right);
                            if(portalObjectR != null)
                                portalObjectR.DestroySelf();
                        }
                        break;
                    case PlacedObjectType.Dir.Right:
                        placedPositions[position][PlacedObjectType.Dir.Left]["Portal"] -= 1;
                        if (placedPositions[position][PlacedObjectType.Dir.Left]["Portal"] == 0)
                        {
                            PortalObject portalObjectL = platformObject.GetPortalObject(PlatformObject.Portal.Left);
                            if (portalObjectL != null)
                                portalObjectL.DestroySelf();
                        }
                        break;
                    case PlacedObjectType.Dir.Up:
                        placedPositions[position][PlacedObjectType.Dir.Down]["Portal"] -= 1;
                        if (placedPositions[position][PlacedObjectType.Dir.Down]["Portal"] == 0)
                        {
                            PortalObject portalObjectD = platformObject.GetPortalObject(PlatformObject.Portal.Front);
                            if (portalObjectD != null)
                                portalObjectD.DestroySelf();
                        }
                        break;
                    case PlacedObjectType.Dir.Down:
                        placedPositions[position][PlacedObjectType.Dir.Up]["Portal"] -= 1;
                        if (placedPositions[position][PlacedObjectType.Dir.Up]["Portal"] == 0)
                        {
                            PortalObject portalObjectU = platformObject.GetPortalObject(PlatformObject.Portal.Back);
                            if (portalObjectU != null)
                                portalObjectU.DestroySelf();
                        }
                        break;
                }
            }
        }
    }
    private void HandleSingleDemolish()
    {
        if (isDemolishActive && Input.GetMouseButtonDown(0) && !Utils_Class.IsPointerOverUI())
        {
            Vector3 mousePosition = Mouse_3D.GetMouseWorldPosition();
            grid.GetXZ(mousePosition, out int x, out int z);
            PlacedObject placedObject = grid.GetGridObject(mousePosition).GetPlacedObject();
            if (placedObject != null)
            {
                HandlePortalRemoval(new Vector2Int(x, z));
                RemovePlatformPortalPosition(new Vector2Int(x, z));
                //Demolish
                placedObject.DestroySelf();

                List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();
                foreach(Vector2Int gridPosition in gridPositionList)
                {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
                }
                numberOfPlatforms--;
                isSaved = false;
                isEmpty = (numberOfPlatforms == 0);
            }
        }
    }

    private void HandleAllDemolish()
    {
        PlacedObject placedObject = null;
        foreach(GridObject gridObject in grid.GetGridArray())
        {
            placedObject = gridObject.GetPlacedObject();
            
            if (placedObject != null)
            {
                //Demolish
                placedObject.DestroySelf();

                List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();
                foreach (Vector2Int gridPosition in gridPositionList)
                {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
                }
            }
        }
        placedPositions.Clear();
        portalConnectors.Clear();
        numberOfPlatforms = 0;
        isEmpty = true;
    }

    private void HandleGroundObjectPlacement()
    {
        if (placeObjectType == PlaceObjectType.GroundObject)
        {
            if(!Input.GetMouseButton(1))
            {
                if (Input.GetMouseButtonDown(0) && placedObjectType != null && !Utils_Class.IsPointerOverUI())
                {
                    Vector3 mousePosition = Mouse_3D.GetMouseWorldPosition();
                    grid.GetXZ(mousePosition, out int x, out int z);

                    Vector2Int placedObjectOrigin = new Vector2Int(x, z);

                    if (TryPlacedObject(placedObjectOrigin, placedObjectType, dir, out PlacedObject placedObject))
                    {
                    }
                    else
                    {
                    }
                }
            }
        }
    }

    private void AddPlatformPortalPosition(Vector2Int placedObjectPosition)
    {
        Dictionary<PlacedObjectType.Dir, List<Vector2Int>> connectedPlatforms;
        if (portalConnectors.ContainsKey(placedObjectPosition))
             connectedPlatforms = portalConnectors[placedObjectPosition];
        else
             connectedPlatforms = new Dictionary<PlacedObjectType.Dir, List<Vector2Int>>();
        Dictionary<PlacedObjectType.Dir, Dictionary<string, int>> Platform = new Dictionary<PlacedObjectType.Dir, Dictionary<string, int>>();
        if (placedObjectPosition.y - 1 >= 0)
        {
            Dictionary<string, int> Down = new Dictionary<string, int>();
            Down.Add("Portal", 0);
            Down.Add("y", placedObjectPosition.y - 1);
            Platform.Add(PlacedObjectType.Dir.Down, Down);
        }
        if (placedObjectPosition.y + 1 < gridSize)
        {
            Dictionary<string, int> Up = new Dictionary<string, int>();
            Up.Add("Portal", 0);
            Up.Add("y", placedObjectPosition.y);
            Platform.Add(PlacedObjectType.Dir.Up, Up);
        }
        if(placedObjectPosition.x - 1 >= 0)
        {
            Dictionary<string, int> Left = new Dictionary<string, int>();
            Left.Add("Portal", 0);
            Left.Add("x", placedObjectPosition.x - 1);
            Platform.Add(PlacedObjectType.Dir.Left, Left);
        }
        if (placedObjectPosition.x + 1 < gridSize)
        {
            Dictionary<string, int> Right = new Dictionary<string, int>();
            Right.Add("Portal", 0);
            Right.Add("x", placedObjectPosition.x);
            Platform.Add(PlacedObjectType.Dir.Right, Right);
        }
        placedPositions.Add(placedObjectPosition, Platform);
        portalConnectors.Add(placedObjectPosition, connectedPlatforms);
    }

    public void RemovePlatformPortalPosition(Vector2Int placedObjectPosition)
    {
        placedPositions.Remove(placedObjectPosition);
        portalConnectors.Remove(placedObjectPosition);
        List<PlacedObjectType.Dir> dirToDelete = new List<PlacedObjectType.Dir>();
        foreach(Vector2Int position in portalConnectors.Keys)
        {
            Debug.Log(position);
            foreach(PlacedObjectType.Dir dir in portalConnectors[position].Keys)
            {
                Debug.Log(dir);
                if (portalConnectors[position][dir].Contains(placedObjectPosition))
                {
                    Debug.Log(position);
                    portalConnectors[position][dir].Remove(placedObjectPosition);
                }
                if (portalConnectors[position][dir].Count == 0)
                    dirToDelete.Add(dir);
            }
            foreach(PlacedObjectType.Dir dir in dirToDelete)
            {
                if (portalConnectors[position].ContainsKey(dir))
                    portalConnectors[position].Remove(dir);
            }
        }
    }


    public void Decrement()
    {
        gridSize--;
        if (gridSize < 2)
            gridSize = 2;
        ChangeGridNumber = true;
        RefreshGridSizeNumber();
    }

    public void Increment()
    {
        gridSize++;
        if (gridSize > 9)
            gridSize = 9;
        ChangeGridNumber = true;
        RefreshGridSizeNumber();
    }

    public void DeselectObjectType()
    {
        placedObjectType = null;

        isDemolishActive = false;

        RefreshSelectedObjectType();
    }

    public bool TryPlacedObject(Vector2Int placedObjectOrigin, PlacedObjectType placedObjectType, PlacedObjectType.Dir dir, out PlacedObject placedObject)
    {
        return TryPlacedObject(grid, placedObjectOrigin, placedObjectType, dir, out placedObject);
    }

    public bool TryPlacedObject(Grid<GridObject> grid, Vector2Int placedObjectOrigin, PlacedObjectType placedObjectType, PlacedObjectType.Dir dir, out PlacedObject placedObject)
    {
        List<Vector2Int> gridPositionList = placedObjectType.GetGridPositionList(placedObjectOrigin, dir);
        bool canBuild = true;

        foreach(Vector2Int gridPosition in gridPositionList)
        {
            bool isValidPosition = grid.IsValidGridPosition(gridPosition);
            if (!isValidPosition)
            {
                canBuild = false;
                break;
            }

            if(!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
            {
                canBuild = false;
                break;
            }
        }

        if (canBuild)
        {
            Vector2Int rotationOffset = placedObjectType.GetRotationOffset(dir);
            Vector3 placedObjectWorld = grid.GetWorldPosition(placedObjectOrigin.x, placedObjectOrigin.y) +
                new Vector3(rotationOffset.x, 0, rotationOffset.y) * gridCellSize;

            placedObject = PlacedObject.Create(placedObjectWorld, placedObjectOrigin, dir, placedObjectType);
            Vector2Int placedObjectPosition = new Vector2Int(placedObjectOrigin.x, placedObjectOrigin.y);
            AddPlatformPortalPosition(placedObjectPosition);
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
            }
            OnObjectPlaced?.Invoke(placedObject, EventArgs.Empty);
            HandlePortalPlacement();
            numberOfPlatforms++;
            isEmpty = false;
            isSaved = false;
            return true;
        }
        else
        {
            Utils_Class.CreateWorldTextPopup("Cannot build here!", Mouse_3D.GetMouseWorldPosition());
            placedObject = null;
            return false;
        }
    }

    private void RefreshSelectedObjectType()
    {
        OnSelectedChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshGridSizeNumber()
    {
        GridNumberChanged?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 GetMouseWorldSnapped()
    {
        Vector3 mousePosition = Mouse_3D.GetMouseWorldPosition();
        grid.GetXZ(mousePosition,out int x,out int z);
        
        if(placedObjectType != null)
        {
            Vector2Int rotationOffset = placedObjectType.GetRotationOffset(dir);
            Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * gridCellSize;
            return placedObjectWorldPosition;
        }
        else
        {
            return mousePosition;
        }
    }

    public void SetDemolishActive()
    {
        placedObjectType = null;
        isDemolishActive = true;
        RefreshSelectedObjectType();
    }

    public bool IsDemolishActive()
    {
        return isDemolishActive;
    }

    public bool GetChangedGridNumber()
    {
        return ChangeGridNumber;
    }

    public void SetSelectedPlacedObject(PlacedObjectType placedObjectType)
    {
        this.placedObjectType = placedObjectType;
        isDemolishActive = false;
        RefreshSelectedObjectType();
    }

    public Quaternion GetPlacedObjectRotation()
    {
        if (placedObjectType != null)
        {
            return Quaternion.Euler(0, placedObjectType.GetRotationAngle(dir), 0);
        }
        else
        {
            return Quaternion.identity;
        }
    }

    public Boolean CanBuild(Vector2Int gridPosition)
    {
        return grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild();
    }

    public void SelectPlacedObjectType(PlacedObjectType placedObjectType)
    {
        placeObjectType = PlaceObjectType.GroundObject;
        isDemolishActive = false;
        this.placedObjectType = placedObjectType;
        RefreshSelectedObjectType();
    }

    public PlacedObjectType GetPlacedObjectType()
    {
        return placedObjectType;
    }

    public PlaceObjectType GetPlaceObjectType()
    {
        return placeObjectType;
    }

    [Serializable]
    public class SaveObject
    {
        public int gridSize;
        public PlacedObjectSaveObjectArray[] placedObjectSaveObjectArrayArray;

    }

    [Serializable]
    public class PlacedObjectSaveObjectArray
    {

        public PlacedObject.SaveObject[] placedObjectSaveObjectArray;

    }

    public class PlacedObjectPositions
    {
        public bool[,] placedPosition;
    }

    private void SavePositions()
    {
        bool[,] placedPositionArray = new bool[gridSize,gridSize];
    }

    public void Save(string levelName)
    {
        List<PlacedObjectSaveObjectArray> placedObjectSaveObjectArraysList = new List<PlacedObjectSaveObjectArray>();

        foreach (Grid<GridObject> grid in gridList)
        {
            List<PlacedObject.SaveObject> saveObjectList = new List<PlacedObject.SaveObject>();
            List<PlacedObject> savedPlacedObjectList = new List<PlacedObject>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetWidth(); y++)
                {
                    PlacedObject placedObject = grid.GetGridObject(x, y).GetPlacedObject();
                    if(placedObject != null && !savedPlacedObjectList.Contains(placedObject))
                    {
                        savedPlacedObjectList.Add(placedObject);
                        saveObjectList.Add(placedObject.GetSaveObject());
                    }
                }
            }

            PlacedObjectSaveObjectArray placedObjectSaveObjectArray = new PlacedObjectSaveObjectArray { placedObjectSaveObjectArray = saveObjectList.ToArray() };
            placedObjectSaveObjectArraysList.Add(placedObjectSaveObjectArray);
        }

        SaveObject saveObject = new SaveObject
        {
            gridSize = gridSize,
            placedObjectSaveObjectArrayArray = placedObjectSaveObjectArraysList.ToArray()
        };

        string json = JsonUtility.ToJson(saveObject);

        PlayerPrefs.SetString("LevelSystemSave", levelName);
        SaveSystem.SaveJson(levelName, json, true);
        ScreenShotHandler.ScreenShottake_Static(levelName);
        isSaved = true;
        Debug.Log("Saved!");
    }

    public void Load(string levelName)
    {
        StartCoroutine(LoadFunction(levelName));
    }

    private IEnumerator LoadFunction(string levelName)
    {
        
        HandleAllDemolish();
        string json = SaveSystem.Load(levelName);

        SaveObject saveObject = JsonUtility.FromJson<SaveObject>(json);
        gridSize = saveObject.gridSize;
        ChangeGridNumber = true;
        HandleGrid();
        for (int i = 0; i < gridList.Count; i++)
        {
            Grid<GridObject> grid = gridList[i];

            foreach(PlacedObject.SaveObject placedObjectSaveObject in saveObject.placedObjectSaveObjectArrayArray[i].placedObjectSaveObjectArray)
            {
                yield return new WaitForSeconds(0.2f);
                PlacedObjectType placedObjectType = GroundListAssets.Instance.GetPlacedGroundObjectTypeSOFromName(placedObjectSaveObject.placedObjectTypeName);
                TryPlacedObject(grid, placedObjectSaveObject.origin, placedObjectType, placedObjectSaveObject.dir, out PlacedObject placedObject);
                //if(placedObject is PlatformObject)
                //{
                //    PlatformObject platformObject = (PlatformObject)placedObject;
                //    platformObject.Load(placedObjectSaveObject.platformPlacedObjectSave, platformObject.transform);
                //}
            }
        }
        foreach (Vector2Int platforms in portalConnectors.Keys)
        {
            Debug.Log("Platform:" + platforms);
            foreach (PlacedObjectType.Dir connectedPlatforms in portalConnectors[platforms].Keys)
            {
                Debug.Log("Connected " + connectedPlatforms + ":");
                foreach (Vector2Int position in portalConnectors[platforms][connectedPlatforms])
                {
                    Debug.Log(position);
                }
            }
        }
        isSaved = true;
        isEmpty = false;
        Debug.Log("Load!");
    }

    private SaveObject SaveChangeGridNumber()
    {
        List<PlacedObjectSaveObjectArray> placedObjectSaveObjectArraysList = new List<PlacedObjectSaveObjectArray>();

        foreach (Grid<GridObject> grid in gridList)
        {
            List<PlacedObject.SaveObject> saveObjectList = new List<PlacedObject.SaveObject>();
            List<PlacedObject> savedPlacedObjectList = new List<PlacedObject>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetWidth(); y++)
                {
                    PlacedObject placedObject = grid.GetGridObject(x, y).GetPlacedObject();
                    if (placedObject != null && !savedPlacedObjectList.Contains(placedObject))
                    {
                        savedPlacedObjectList.Add(placedObject);
                        saveObjectList.Add(placedObject.GetSaveObject());
                    }
                }
            }

            PlacedObjectSaveObjectArray placedObjectSaveObjectArray = new PlacedObjectSaveObjectArray { placedObjectSaveObjectArray = saveObjectList.ToArray() };
            placedObjectSaveObjectArraysList.Add(placedObjectSaveObjectArray);
        }

        SaveObject saveObject = new SaveObject
        {
            gridSize = gridSize,
            placedObjectSaveObjectArrayArray = placedObjectSaveObjectArraysList.ToArray()
        };

        return saveObject;
    }

    private void LoadChangeGridNumber(SaveObject savedObjects)
    {   
        for (int i = 0; i < gridList.Count; i++)
        {
            Grid<GridObject> grid = gridList[i];

            foreach (PlacedObject.SaveObject placedObjectSaveObject in savedObjects.placedObjectSaveObjectArrayArray[i].placedObjectSaveObjectArray)
            {
                PlacedObjectType placedObjectType = GroundListAssets.Instance.GetPlacedGroundObjectTypeSOFromName(placedObjectSaveObject.placedObjectTypeName);
                TryPlacedObject(grid, placedObjectSaveObject.origin, placedObjectType, placedObjectSaveObject.dir, out PlacedObject placedObject);
                if (placedObject is PlatformObject)
                {
                    PlatformObject platformObject = (PlatformObject)placedObject;
                    platformObject.Load(placedObjectSaveObject.platformPlacedObjectSave, platformObject.transform);
                }
            }
        }
    }
}
