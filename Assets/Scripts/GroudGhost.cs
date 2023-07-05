using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GroudGhost : MonoBehaviour
{
    private Transform visual;
    private PlacedObjectType placedObjectType;
    [SerializeField] Grid_Show GridSystem;
    [SerializeField] Transform Empty;
    [SerializeField] bool CanBuild;
    private void Start()
    {
        RefreshVisual();

        Grid_Show.Instance.OnSelectedChanged += Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(object sender, System.EventArgs e)
    {
        RefreshVisual();
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = Grid_Show.Instance.GetMouseWorldSnapped();
        targetPosition.y = 1f;
        transform.DOMove(targetPosition, Time.deltaTime * 15f);
        //transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
        transform.DORotateQuaternion(Grid_Show.Instance.GetPlacedObjectRotation(), Time.deltaTime * 15f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, Grid_Show.Instance.GetPlacedObjectRotation(), Time.deltaTime * 15f);
        if(Grid_Show.Instance.GetPlacedObjectType()!=null)
        { 
            TargetCanBuild(targetPosition);
            if (CanBuild)
                visual.gameObject.SetActive(true);
            else
                visual.gameObject.SetActive(false);
        }
    }

    private void TargetCanBuild(Vector3 targetPosition)
    {
        int dir = Grid_Show.Instance.GetPlacedObjectType().GetRotationAngle(Grid_Show.Instance.GetDir());
        GridSystem.grid.GetXZ(targetPosition, out int x, out int z);
        Vector2Int target;
        switch (dir)
        {
            default:
            case 0: target = new Vector2Int(x, z); break;
            case 90: target = new Vector2Int(x, z-1); break;
            case 180: target = new Vector2Int(x-1, z-1); break;
            case 270: target = new Vector2Int(x-1, z); break;
        }
        CanBuild = GridSystem.grid.GetGridObject(target.x, target.y).CanBuild();
    }

    private void RefreshVisual()
    {
        if(visual != null)
        {
            Destroy(visual.gameObject);
            visual = null;
        }
        placedObjectType = Grid_Show.Instance.GetPlacedObjectType();
        if (placedObjectType != null)
        {
            visual = Instantiate(placedObjectType.visual, Vector3.zero, Quaternion.identity); 
            visual.parent = transform;
            visual.localPosition = Vector3.zero;
            visual.localEulerAngles = Vector3.zero;
            SetLayerRecursive(visual.gameObject, 11);
        }
    }

    private void SetLayerRecursive(GameObject targetGameObject, int layer)
    {
        targetGameObject.layer = layer;
        foreach (Transform child in targetGameObject.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}
