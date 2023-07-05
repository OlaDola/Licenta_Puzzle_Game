using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPlacedObject : RadialMenuEntry
{
    public override void DoFunction(Vector3 position)
    {
        GridController.Instance.DestroyPlatform(position);
    }
}
