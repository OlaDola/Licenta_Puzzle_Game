using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedObject : MonoBehaviour
{
    private PlacedObjectType placedObjectType;
    private Vector2Int origin;
    private PlacedObjectType.Dir dir;

    public static PlacedObject Create(Vector3 worldPosition, Vector2Int origin, PlacedObjectType.Dir dir, PlacedObjectType placedObjectType)
    {
        Transform placedObjectTransform = Instantiate(placedObjectType.prefab,worldPosition,Quaternion.Euler(0,placedObjectType.GetRotationAngle(dir),0));

        PlacedObject placedObject = placedObjectTransform.GetComponent<PlacedObject>();
        placedObject.Setup(placedObjectType, origin, dir);

        return placedObject;

    }

    private void Setup(PlacedObjectType placedObjectType, Vector2Int origin, PlacedObjectType.Dir dir)
    {
        this.placedObjectType = placedObjectType;
        this.origin = origin;
        this.dir = dir;
    }

    public List<Vector2Int> GetGridPositionList()
    {
        return placedObjectType.GetGridPositionList(origin, dir);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public SaveObject GetSaveObject()
    {
        return new SaveObject
        {
            placedObjectTypeName = placedObjectType.name,
            origin = origin,
            dir = dir,
            platformPlacedObjectSave = (this is PlatformObject) ? ((PlatformObject)this).Save() : "",
        };
    }

    public override string ToString()
    {
        return placedObjectType.nameString;
    }

    [System.Serializable]
    public class SaveObject
    {

        public string placedObjectTypeName;
        public Vector2Int origin;
        public PlacedObjectType.Dir dir;
        public string platformPlacedObjectSave;
    }
}
