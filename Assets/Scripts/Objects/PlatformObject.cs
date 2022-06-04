using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformObject : PlacedObject
{
    
    public enum Portal
    {
        Front,
        Back,
        Left,
        Right
    }

    [SerializeField] private PortalPosition FrontProtalPosition;
    [SerializeField] private PortalPosition BackProtalPosition;
    [SerializeField] private PortalPosition LeftProtalPosition;
    [SerializeField] private PortalPosition RightProtalPosition;

    private PortalObject FrontPortalObject;
    private PortalObject BackPortalObject;
    private PortalObject LeftPortalObject;
    private PortalObject RightPortalObject;

    public void PlacePortal(Portal portal, PortalObjectType portalObjectType, Transform parent = null)
    {
        PortalPosition portalPosition = GetPortalPosition(portal);
        Quaternion rotation = Quaternion.identity;
        if (portal == Portal.Front || portal== Portal.Back)
            rotation = Quaternion.identity * Quaternion.Euler(0, 90, 0); ;

        Transform portalObjectTransform = Instantiate(portalObjectType.prefab, portalPosition.transform.position, rotation, parent);

        PortalObject portalPlacedObject = portalObjectTransform.GetComponent<PortalObject>();
        SetPortalObject(portal, portalPlacedObject);
    }

    private PortalPosition GetPortalPosition(Portal edge)
    {
        switch (edge)
        {
            default:
            case Portal.Front: return FrontProtalPosition;
            case Portal.Back: return BackProtalPosition;
            case Portal.Left: return LeftProtalPosition;
            case Portal.Right: return RightProtalPosition;
        }
    }

    private void SetPortalObject(Portal portal, PortalObject portalObject)
    {
        switch (portal)
        {
            default:
            case Portal.Front:
                FrontPortalObject = portalObject;
                break;
            case Portal.Back:
                BackPortalObject = portalObject;
                break;
            case Portal.Left:
                LeftPortalObject = portalObject;
                break;
            case Portal.Right:
                RightPortalObject = portalObject;
                break;
        }
    }

    public PortalObject GetPortalObject(Portal portal)
    {
        switch (portal)
        {
            default:
            case Portal.Front:
                return FrontPortalObject;
            case Portal.Back:
                return BackPortalObject;
            case Portal.Left:
                return LeftPortalObject;
            case Portal.Right:
                return RightPortalObject;
        }
    }

    public PortalObject GetPortalObject(PlacedObjectType.Dir dir)
    {
        switch (dir)
        {
            default:
            case PlacedObjectType.Dir.Down:
                return FrontPortalObject;
            case PlacedObjectType.Dir.Up:
                return BackPortalObject;
            case PlacedObjectType.Dir.Left:
                return LeftPortalObject;
            case PlacedObjectType.Dir.Right:
                return RightPortalObject;
        }
    }

    public string Save()
    {
        return JsonUtility.ToJson(new PortalSaveObject{

            FrontPortalObjectName = FrontPortalObject != null ? FrontPortalObject.GetPortalObjectTypeName() : "",
            BackPortalObjectName = BackPortalObject != null ? BackPortalObject.GetPortalObjectTypeName() : "",
            LeftPortalObjectName = LeftPortalObject != null ? LeftPortalObject.GetPortalObjectTypeName() : "",
            RightPortalObjectName = RightPortalObject != null ? RightPortalObject.GetPortalObjectTypeName() : "",
        });
    }

    public void Load(string saveString, Transform platformTransform)
    {
        PortalSaveObject portalSaveObject = JsonUtility.FromJson<PortalSaveObject>(saveString);

        if (portalSaveObject.FrontPortalObjectName != "")
            PlacePortal(Portal.Front, GroundListAssets.Instance.GetPlacedPortalObjectTypeSOFromName(portalSaveObject.FrontPortalObjectName), platformTransform);
        if (portalSaveObject.BackPortalObjectName != "")
            PlacePortal(Portal.Back, GroundListAssets.Instance.GetPlacedPortalObjectTypeSOFromName(portalSaveObject.BackPortalObjectName), platformTransform);
        if (portalSaveObject.LeftPortalObjectName != "")
            PlacePortal(Portal.Left, GroundListAssets.Instance.GetPlacedPortalObjectTypeSOFromName(portalSaveObject.LeftPortalObjectName), platformTransform);
        if (portalSaveObject.RightPortalObjectName != "")
            PlacePortal(Portal.Right, GroundListAssets.Instance.GetPlacedPortalObjectTypeSOFromName(portalSaveObject.RightPortalObjectName), platformTransform);
    }

    [System.Serializable]
    public class PortalSaveObject
    {
        public string FrontPortalObjectName;
        public string BackPortalObjectName;
        public string LeftPortalObjectName;
        public string RightPortalObjectName;
    }
}
