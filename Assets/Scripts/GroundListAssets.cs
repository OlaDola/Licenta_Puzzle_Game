using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundListAssets : MonoBehaviour
{
    public static GroundListAssets Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    
    public PlacedObjectType[] groundList;
    public PlacedObjectType GroundPlatform;
    public PlacedObjectType EndPlatform;

    public PortalObjectType[] portalList;
    public PortalObjectType Portal;

    public PlacedObjectType GetPlacedGroundObjectTypeSOFromName(string placedObjectTypeName)
    {
        foreach (PlacedObjectType placedObjectType in groundList)
        {
            if (placedObjectType.name == placedObjectTypeName)
            {
                return placedObjectType;
            }
        }
        return null;
    }

    public PortalObjectType GetPlacedPortalObjectTypeSOFromName(string placedObjectTypeName)
    {
        foreach (PortalObjectType placedPortalObjectType in portalList)
        {
            if (placedPortalObjectType.name == placedObjectTypeName)
            {
                return placedPortalObjectType;
            }
        }
        return null;
    }
}
