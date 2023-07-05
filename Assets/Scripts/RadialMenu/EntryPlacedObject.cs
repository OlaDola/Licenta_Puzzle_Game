using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryPlacedObject : RadialMenuEntry
{

    [SerializeField] PlacedObjectType placedObject;

    public override void DoFunction(Vector3 position)
    {
        GridController.Instance.PlacePlatform(position, placedObject);
    }
}
