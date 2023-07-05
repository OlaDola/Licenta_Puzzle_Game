using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraTest : MonoBehaviour
{
    public CameraTest Instance { get; private set; }
    public Camera MainCamera;
    public Camera PreviewCamera;

    public GameObject perspectiveView;
    public OrthoView orthoView;

    public Grid_Show GridSystem;

    private Vector3 previousPosition;

    private Matrix4x4 ortho,
                        perspective;

    [SerializeField]
    private float fov = 60,
                            near = 0f,
                            far = 100f,
                            orthographicSize = 15f;

    private OrthoView.Dir dir;
    private float aspect;

    private Vector3 CameraGridNumberChangedVector;
    private Quaternion CameraGridNumberChangedQuaternion;

    private Vector3 CameraGridNumberChangedVectorPreview;
    private Quaternion CameraGridNumberChangedQuaternionPreview;

    private MatrixBlender blender;
    private bool orthoOn;

    public float rotationSpeed;
    private void Start()
    {
        Instance = this;
        HandleGridNumberChange();
    }

    public void HandleGridNumberChange()
    {
        HandleCameraPosition();

        aspect = (float)Screen.width / (float)Screen.height;
        ortho = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, near, far);
        perspective = Matrix4x4.Perspective(fov, aspect, near, far);
        MainCamera.projectionMatrix = perspective;
        orthoOn = false;
        blender = (MatrixBlender)GetComponent(typeof(MatrixBlender));
        orthoView.Setup(GridSystem.GetMiddlePosition());
    }


    void LateUpdate()
    {
        if (!PauseMenu.isPause)
        {
            GridSystem.grid.FaceCameraTextMesh();
            if (!orthoOn)
                HandlePerspectiveRotaion();
            //if(orthoOn)
            //    HandleOrthoRotaion();
            HandleProjection();
        }
    }

    public bool GetOrthoOn()
    {
        return orthoOn;
    }

    private void HandleCameraPosition()
    {
        CameraGridNumberChangedVector = new Vector3(GridSystem.GetMiddlePosition().x, GridSystem.GetMiddlePosition().x, -((GridSystem.GetMiddlePosition().x) * 3) / 2);
        CameraGridNumberChangedQuaternion = new Quaternion(0.216439605f, 0, 0, 0.976296067f);
        CameraGridNumberChangedVectorPreview = new Vector3(GridSystem.GetMiddlePosition().x, GridSystem.GetMiddlePosition().x, -((GridSystem.GetMiddlePosition().x) * 2) / 3);
        CameraGridNumberChangedQuaternionPreview = new Quaternion(0.216439605f, 0, 0, 0.976296067f);
        MainCamera.transform.DOMove(CameraGridNumberChangedVector, 1f);
        MainCamera.transform.DORotateQuaternion(CameraGridNumberChangedQuaternion, 1f);
        if (PlayerPrefs.GetInt("PlayMode") != 1)
            PreviewCamera.transform.position = CameraGridNumberChangedVectorPreview;
        Debug.Log(MainCamera.transform.position);
        perspectiveView.transform.position = CameraGridNumberChangedVector;
        perspectiveView.transform.rotation = CameraGridNumberChangedQuaternion;
    }

    private void HandlePerspectiveRotaion()
    {
        if ((Input.GetMouseButtonDown(0) && (!GridSystem.GetPlacedObjectType() || GridSystem.IsDemolishActive())) || (Input.GetMouseButtonDown(1) && GridSystem.GetPlacedObjectType()))
        {
            previousPosition = MainCamera.ScreenToViewportPoint(Input.mousePosition);
        }
        if (!orthoOn)
        {
            if ((Input.GetMouseButton(0) && (!GridSystem.GetPlacedObjectType() || GridSystem.IsDemolishActive())) || (Input.GetMouseButton(1) && GridSystem.GetPlacedObjectType()))
            {
                Vector3 direction = previousPosition - MainCamera.ScreenToViewportPoint(Input.mousePosition);

                perspectiveView.transform.RotateAround(GridSystem.GetMiddlePosition(), new Vector3(0, 10, 0), (-direction.x * 180) / rotationSpeed);

                MainCamera.transform.RotateAround(GridSystem.GetMiddlePosition(), new Vector3(0, 10, 0), (-direction.x * 180) / rotationSpeed);

                previousPosition = MainCamera.ScreenToViewportPoint(Input.mousePosition);
            }
        }
    }

    private void HandleOrthoRotaion()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            dir = orthoView.MinOrthoDistDir(MainCamera.transform.position);
            dir = OrthoView.GetNextDir(dir, Input.mouseScrollDelta.y);
            HandlePositionMain(MainCamera, OrthoView.GetDir(dir), 1f);
            HandlePortalConnection(dir);
        }
    }

    private void HandlePortalConnection(OrthoView.Dir dir)
    {
        Debug.Log(dir);
        Vector2Int minPosition;
        Vector2Int maxPosition;

        Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir, List<Vector2Int>>> portalConnectors = GridSystem.GetPortalConnectors();

        Dictionary<Vector2Int, List<Vector2Int>> connectedPlatformMin = new Dictionary<Vector2Int, List<Vector2Int>>();
        foreach (Vector2Int position in portalConnectors.Keys)
        {
            //Debug.Log(position);
            PlacedObject placedObject1 = GridSystem.GetGridList().GetGridObject(position.x, position.y).GetPlacedObject();
            placedObject1.TryGetComponent(out PlatformObject platformObject1);
            List<Vector2Int> minPositions = new List<Vector2Int>();
            foreach (PlacedObjectType.Dir dirPortal in portalConnectors[position].Keys)
            {
                //Debug.Log("dirPortal:" + dirPortal);
                PortalObject portalObject1 = platformObject1.GetPortalObject(dirPortal);
                PortalScript portalScript1 = new PortalScript();
                if (portalObject1 != null)
                    portalScript1 = portalObject1.GetPortalScript();
                minPosition = new Vector2Int(GridSystem.GetGridSize(), GridSystem.GetGridSize());
                maxPosition = new Vector2Int(-1, -1);
                PlacedObject placedObject2 = new PlacedObject();
                PlatformObject platformObject2 = new PlatformObject();
                PortalScript portalScript2 = new PortalScript();
                foreach (Vector2Int positionConected in portalConnectors[position][dirPortal])
                {
                    //Debug.Log("positionConected:" + positionConected);
                    //Debug.Log(dir);

                    switch (dir)
                    {
                        case OrthoView.Dir.Left:
                            if (dirPortal == PlacedObjectType.Dir.Up || dirPortal == PlacedObjectType.Dir.Down)
                            {
                                if (positionConected.x < minPosition.x)
                                {
                                    minPosition = positionConected;
                                    //Debug.Log("MinPosition:" + minPosition);
                                }
                            }
                            break;
                        case OrthoView.Dir.Right:
                            if (dirPortal == PlacedObjectType.Dir.Up || dirPortal == PlacedObjectType.Dir.Down)
                            {
                                if (positionConected.x > maxPosition.x)
                                {
                                    maxPosition = positionConected;
                                    //Debug.Log("MaxPosition:" + maxPosition);
                                }
                            }
                            break;
                        case OrthoView.Dir.Front:
                            if (dirPortal == PlacedObjectType.Dir.Left || dirPortal == PlacedObjectType.Dir.Right)
                            {
                                if (positionConected.y < minPosition.y)
                                {
                                    minPosition = positionConected;
                                    //Debug.Log("MinPosition:" + minPosition);
                                }
                            }
                            break;
                        case OrthoView.Dir.Back:
                            if (dirPortal == PlacedObjectType.Dir.Left || dirPortal == PlacedObjectType.Dir.Right)
                            {
                                if (positionConected.y > maxPosition.y)
                                {
                                    maxPosition = positionConected;
                                    //Debug.Log("MaxPosition:" + maxPosition);
                                }
                            }
                            break;
                    }
                }
                if (minPosition != new Vector2Int(GridSystem.GetGridSize(), GridSystem.GetGridSize()) || maxPosition != new Vector2Int(-1, -1))
                {
                    Vector2Int position2 = new Vector2Int(0, 0);
                    if (dir == OrthoView.Dir.Left || dir == OrthoView.Dir.Right)
                    {
                        switch (dir)
                        {
                            case OrthoView.Dir.Left:
                                Debug.Log("MinPositionLeft:" + minPosition);
                                position2 = minPosition;
                                if (!minPositions.Contains(position2))
                                    minPositions.Add(position2);
                                break;
                            case OrthoView.Dir.Right:
                                Debug.Log("MaxPositionRight:" + maxPosition);
                                position2 = maxPosition;
                                if (!minPositions.Contains(position2))
                                    minPositions.Add(position2);
                                break;
                        }
                    }
                    if (dir == OrthoView.Dir.Front || dir == OrthoView.Dir.Back)
                    {
                        switch (dir)
                        {
                            case OrthoView.Dir.Front:
                                print("MinPositionfront:" + minPosition);
                                position2 = minPosition;
                                if (!minPositions.Contains(position2))
                                    minPositions.Add(position2);
                                break;
                            case OrthoView.Dir.Back:
                                print("MaxPositionBack:" + maxPosition);
                                position2 = maxPosition;
                                if (!minPositions.Contains(position2))
                                    minPositions.Add(position2);
                                break;
                        }
                    }
                }
            }
            connectedPlatformMin.Add(position, minPositions);
        }
        foreach (Vector2Int key in connectedPlatformMin.Keys)
        {
            foreach (Vector2Int value in connectedPlatformMin[key])
                Debug.Log("(" + key + ":-->" + value.ToString());

        }

        foreach (Vector2Int position in portalConnectors.Keys)
        {
            PlacedObject placedObject1 = GridSystem.GetGridList().GetGridObject(position.x, position.y).GetPlacedObject();
            placedObject1.TryGetComponent(out PlatformObject platformObject1);
            foreach (PlacedObjectType.Dir dirPortal in portalConnectors[position].Keys)
            {
                PortalObject portalObject1 = platformObject1.GetPortalObject(dirPortal);
                PortalScript portalScript1 = new PortalScript();
                if (portalObject1 != null)
                {
                    portalScript1 = portalObject1.GetPortalScript();
                }
                PlacedObject placedObject2 = new PlacedObject();
                PlatformObject platformObject2 = new PlatformObject();
                PortalObject portalObject2 = new PortalObject();
                PortalScript portalScript2 = new PortalScript();
                foreach (Vector2Int positionConected in portalConnectors[position][dirPortal])
                {
                    if (connectedPlatformMin[position].Contains(positionConected) && connectedPlatformMin[positionConected].Contains(position))
                    {
                        //print(position + ":" + positionConected);
                        placedObject2 = GridSystem.GetGridList().GetGridObject(positionConected.x, positionConected.y).GetPlacedObject();
                        placedObject2.TryGetComponent(out platformObject2);

                        switch (dirPortal)
                        {
                            case PlacedObjectType.Dir.Down:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Up) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Up);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                            case PlacedObjectType.Dir.Up:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Down) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Down);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                            case PlacedObjectType.Dir.Right:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Left) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Left);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                            case PlacedObjectType.Dir.Left:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Right) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Right);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                        }
                        if (position.x == positionConected.x || position.y == positionConected.y)
                        {
                            if (portalObject1 != null)
                            {
                                portalScript1.linkedPortal = null;
                                print("here");
                                portalObject1.GetComponentInChildren<BoxCollider>().enabled = false;
                            }
                            if (portalObject2 != null)
                            {
                                portalScript2.linkedPortal = null;
                                print("here2");
                                portalObject2.GetComponentInChildren<BoxCollider>().enabled = false;
                            }
                        }
                        else
                        {
                            portalObject1.GetComponentInChildren<BoxCollider>().enabled = true;
                            portalObject2.GetComponentInChildren<BoxCollider>().enabled = true;

                            portalScript2 = portalObject2.GetPortalScript();

                            portalScript1.linkedPortal = portalScript2;
                            portalScript2.linkedPortal = portalScript1;
                            //print("linked:" + position + "<->" + positionConected);

                        }
                    }
                    if (connectedPlatformMin[position].Contains(positionConected) && !connectedPlatformMin[positionConected].Contains(position))
                    {
                        //print(position + ":!" + positionConected);
                        placedObject2 = GridSystem.GetGridList().GetGridObject(positionConected.x, positionConected.y).GetPlacedObject();
                        placedObject2.TryGetComponent(out platformObject2);

                        switch (dirPortal)
                        {
                            case PlacedObjectType.Dir.Down:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Up) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Up);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                            case PlacedObjectType.Dir.Up:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Down) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Down);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                            case PlacedObjectType.Dir.Right:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Left) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Left);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                            case PlacedObjectType.Dir.Left:
                                if (platformObject2.GetPortalObject(PlacedObjectType.Dir.Right) != null)
                                {
                                    portalObject2 = platformObject2.GetPortalObject(PlacedObjectType.Dir.Right);
                                    portalScript2 = portalObject2.GetPortalScript();
                                }
                                break;
                        }
                        if (position.x == positionConected.x || position.y == positionConected.y)
                        {
                            if (portalObject1 != null && connectedPlatformMin[position].Contains(positionConected))
                            {
                                portalScript1.linkedPortal = null;
                                print(position + "->" + positionConected);
                                print("here1");
                                portalObject1.GetComponentInChildren<BoxCollider>().enabled = false;
                            }
                            if (portalObject2 != null && connectedPlatformMin[positionConected].Contains(position))
                            {
                                portalScript2.linkedPortal = null;
                                print(position + "->" + positionConected);
                                print("here2");
                                portalObject2.GetComponentInChildren<BoxCollider>().enabled = false;
                            }
                        }
                        else
                        {
                            portalObject1.GetComponentInChildren<BoxCollider>().enabled = true;
                            portalObject2.GetComponentInChildren<BoxCollider>().enabled = true;

                            portalScript2 = portalObject2.GetPortalScript();

                            portalScript1.linkedPortal = portalScript2;
                            //print("linked:" + position + "->" + positionConected);
                        }
                    }
                }
            }

        }
    }
    private void HandleProjection()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            orthoOn = !orthoOn;
            if (orthoOn)
            {
                MainCamera.transform.DOMove(orthoView.MinOrthoDist(MainCamera.transform.position).position, 1f);
                MainCamera.transform.DORotateQuaternion(orthoView.MinOrthoDist(MainCamera.transform.position).rotation, 1f);
                blender.BlendToMatrix(ortho, 1f);
                HandlePortalConnection(orthoView.MinOrthoDistDir(MainCamera.transform.position));
            }
            else
            {
                MainCamera.transform.DOMove(perspectiveView.transform.position, 1f);
                MainCamera.transform.DORotateQuaternion(perspectiveView.transform.rotation, 1f);
                blender.BlendToMatrix(perspective, 1f);
            }
        }
    }

    private IEnumerator LerpFromTo(Camera Camera, Camera From, Transform To, float duration)
    {
        Vector3 CamPos = From.transform.position;
        Quaternion CamRot = From.transform.rotation;

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            Camera.transform.position = Vector3.Lerp(CamPos, To.position, (Time.time - startTime) / duration);
            if (Vector3.Distance(Camera.transform.position, To.position) < 0.5f)
                Camera.transform.position = To.position;
            Camera.transform.rotation = Quaternion.Slerp(CamRot, To.rotation, (Time.time - startTime) / duration);
            if (Quaternion.Angle(Camera.transform.rotation, To.rotation) < 0.5f)
                Camera.transform.rotation = To.rotation;
            yield return 1;
        }
    }
    private Coroutine HandlePositionMain(Camera From, Transform To, float duration)
        {
            StopAllCoroutines();
            return StartCoroutine(LerpFromTo(MainCamera, From, To, duration));
        }

    private IEnumerator LerpFromToGrid(Camera Camera, Camera From, Vector3 VectorTo, Quaternion QuaternionTo, float duration)
    {
        Vector3 CamPos = From.transform.position;
        Quaternion CamRot = From.transform.rotation;

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            Camera.transform.position = Vector3.Lerp(CamPos, VectorTo, (Time.time - startTime) / duration);
            if (Vector3.Distance(Camera.transform.position, VectorTo) < 0.1f)
                Camera.transform.position = VectorTo;
            Camera.transform.rotation = Quaternion.Slerp(CamRot, QuaternionTo, (Time.time - startTime) / duration);
            if (Quaternion.Angle(Camera.transform.rotation, QuaternionTo) < 0.01f)
                Camera.transform.rotation = QuaternionTo;
            yield return 1;
        }
    }

    private Coroutine HandlePositionGridMain(Camera From, Vector3 VectorTo, Quaternion QuaternionTo, float duration)
    {
        StopAllCoroutines();
        return StartCoroutine(LerpFromToGrid(MainCamera, From, VectorTo, QuaternionTo, duration));
    }
}