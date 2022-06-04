using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility_Functions;

public class Grid<TGridObject>
{
    [SerializeField] static int isPlayMode = PlayerPrefs.GetInt("PlayMode");

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int z;
    }
    private int width;
    private TGridObject[,] gridArray;
    private float cellSize;
    private Vector3 originPosition;
    private TextMesh[,] debugTextArray;
    public Grid(int width, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int,TGridObject> createGridObject)
    {
        this.width = width;
        this.cellSize = cellSize;
        this.originPosition = originPosition;
        gridArray = new TGridObject[width,width];
        debugTextArray = new TextMesh[width,width];

        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                gridArray[i, j] = createGridObject(this, i, j);
            }
        }
        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                //if(isPlayMode==1)
                    debugTextArray[i, j] = Utils_Class.CreateWorldText(gridArray[i, j]?.ToString(), GameObject.Find("PositionLocation").transform, GetWorldPosition(i, j) + new Vector3(cellSize, 0, cellSize) * .5f, 8, Color.black, TextAnchor.MiddleCenter);
                //Debug.DrawLine(GetWorldPosition(i,j), GetWorldPosition(i, j + 1), Color.black, 100f);
                //Debug.DrawLine(GetWorldPosition(i,j), GetWorldPosition(i + 1, j), Color.black, 100f);
            }
        }
        //Debug.DrawLine(GetWorldPosition(0, width), GetWorldPosition(width, width), Color.black, 100f);
        //Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, width), Color.black, 100f);
        OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
        {
            debugTextArray[eventArgs.x, eventArgs.z].text = gridArray[eventArgs.x, eventArgs.z]?.ToString();
        };
    }

    public void DestroyGrid()
    {
        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                GameObject.Destroy(debugTextArray[i, j].gameObject);
            }
        }
    }
    public void FaceCameraTextMesh()
    {
        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                debugTextArray[i,j].transform.rotation = Camera.main.transform.rotation;

            }
        }
    }
    public int GetWidth()
    {
        return width;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z) * cellSize + originPosition;
    }

    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        z = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public void SetGridObject(int x, int z, TGridObject value)
    {
        if (x >= 0 && z >= 0 && x < width && z < width)
        {
            gridArray[x, z] = value;
            TriggerGridObjectChanged(x, z);
        }
    }

    public void TriggerGridObjectChanged(int x, int z)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXZ(worldPosition, out int x, out int z);
        SetGridObject(x, z, value);
    }

    public TGridObject GetGridObject(int x, int z)
    {
        if(x >= 0 && z >= 0 && x < width && z < width)
        {
            return gridArray[x, z];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject[,] GetGridArray()
    {
        return gridArray;
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, z;
        GetXZ(worldPosition,out x,out z);
        return GetGridObject(x,z);
    }

    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        int x = gridPosition.x;
        int z = gridPosition.y;

        if (x >= 0 && z >= 0 && x < width && z < width)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
