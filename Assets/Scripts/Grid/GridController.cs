using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public static GridController Instance { get; private set; }

    [SerializeField] private Transform planeTransform;
    private GameObject planeGameObject;

    public Grid<GridObject> grid;

    public int gridSize;
    public float gridCellSize;
    private Vector3 middlePosition;

    private void Awake()
    {
        Instance = this;
        grid = new Grid<GridObject>(gridSize, gridCellSize, Vector3.zero, (Grid<GridObject> g, int x, int z) => new GridObject(g, x, z));
        middlePosition = new Vector3(gridCellSize * gridSize / 2, 0, gridCellSize * gridSize / 2);
        planeTransform.localScale = new Vector3(gridCellSize * gridSize, 0, gridCellSize * gridSize);
        planeTransform = Instantiate(planeTransform, middlePosition, Quaternion.identity);
        planeTransform.name = "Plane";
    }

    public Vector3 GetMiddlePosition()
    {
        return middlePosition;
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

    public void PlacePlatform(Vector3 position, PlacedObjectType placedObjectType)
    {
        grid.GetXZ(position, out int x, out int z);

        Vector2Int placedObjectOrigin = new Vector2Int(x, z);
        TryPlacedPlatform(grid, placedObjectOrigin, placedObjectType);
    }

    private void TryPlacedPlatform(Grid<GridObject> grid, Vector2Int gridCoordLocation, PlacedObjectType placedObjectType)
    {
        List<Vector2Int> gridPositionList = placedObjectType.GetGridPositionList(gridCoordLocation, PlacedObjectType.Dir.Down);
        bool canBuild = true;

        foreach (Vector2Int gridPosition in gridPositionList)
        {
            bool isValidPosition = grid.IsValidGridPosition(gridPosition);
            if (!isValidPosition)
            {
                canBuild = false;
                break;
            }

            if (!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
            {
                canBuild = false;
                break;
            
            }
        }
        if (canBuild)
        {
            Vector2Int rotationOffset = placedObjectType.GetRotationOffset(PlacedObjectType.Dir.Down);
            Vector3 placedObjectWorld = grid.GetWorldPosition(gridCoordLocation.x, gridCoordLocation.y) +
                new Vector3(rotationOffset.x, 0, rotationOffset.y) * gridCellSize;

            PlacedObject placedObject = PlacedObject.Create(placedObjectWorld, gridCoordLocation, PlacedObjectType.Dir.Down, placedObjectType);
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
            }
        }
    }

    public void DestroyPlatform(Vector3 position)
    {
        grid.GetXZ(position, out int x, out int z);

        Vector2Int placedObjectOrigin = new Vector2Int(x, z);
        TryDestroyPlatform(grid, placedObjectOrigin);
    }

    private void TryDestroyPlatform(Grid<GridObject> grid, Vector2Int gridCoordLocation)
    {
        PlacedObject placedObject = grid.GetGridObject(gridCoordLocation.x, gridCoordLocation.y).GetPlacedObject();

        if (placedObject != null)
        {
            placedObject.DestroySelf();

            List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
            }
        }
    }
}

