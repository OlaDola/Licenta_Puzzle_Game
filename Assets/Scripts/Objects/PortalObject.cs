using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalObject : MonoBehaviour
{

    [SerializeField] private PortalObjectType portalObjectType;

    public PortalScript GetPortalScript()
    {
        return gameObject.GetComponentInChildren<PortalScript>();
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public string GetPortalObjectTypeName()
    {
        return portalObjectType.name;
    }
}
