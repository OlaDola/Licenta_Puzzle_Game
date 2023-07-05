using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{

    public PortalScript linkedPortal;
    readonly List<PortalTraveller> trackedTravellers = new List<PortalTraveller>();

    private BoxCollider boxCollider;

    [SerializeField]
    private Vector3 platformCoordinates;

    [SerializeField] CameraTest cameraTest;
    [SerializeField] Grid_Show gridSystem;

    private void Start()
    {
        cameraTest = GameObject.Find("Main Camera").GetComponent<CameraTest>();
        gridSystem = GameObject.Find("Grid").GetComponent<Grid_Show>();
        Transform portalPivot = gameObject.transform.parent;
        platformCoordinates = portalPivot.parent.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (cameraTest.GetOrthoOn() && linkedPortal != null)
            HandleTravellers();
    }

    void HandleTravellers()
    {
        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            Vector3 OffsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(OffsetFromPortal, transform.right));

            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.right));

            gridSystem.grid.GetXZ(travellerT.position, out int x, out int z);
            Vector2Int playerGridPosition = new Vector2Int(x, z);

            gridSystem.grid.GetXZ(platformCoordinates, out x, out z);
            Vector2Int portalGridPosition = new Vector2Int(x, z);

            if (playerGridPosition != portalGridPosition)
            { 
                if (portalSide != portalSideOld)
                {
                    traveller.Teleport(transform, linkedPortal.transform.transform, m.GetColumn(3), m.rotation);

                    linkedPortal.OnTravellerEnterPortal(traveller);
                    trackedTravellers.RemoveAt(i);
                    i--;
                }
                else
                {
                    traveller.previousOffsetFromPortal = OffsetFromPortal;
                } 
            }
        }

        //if(playerIsOverlapping)
        //{
        //    Vector3 portalToPlayer = player.position - transform.position;
        //    float dotProduct = Vector3.Dot(transform.up, portalToPlayer);

        //    if (dotProduct < 0f)
        //    {
        //        float rotationDiff = -Quaternion.Angle(transform.rotation, reciever.rotation);
        //        rotationDiff += 180;

        //        player.Rotate(Vector3.up, rotationDiff);

        //        Vector3 positionOffset = Quaternion.Euler(0f, rotationDiff, 0f) * portalToPlayer;
        //        player.position = reciever.position + positionOffset;

        //        playerIsOverlapping = false;
        //    }

        //}

    }

    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add(traveller);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller)
        {
            OnTravellerEnterPortal(traveller);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();

        if (traveller && trackedTravellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
        }
    }
}