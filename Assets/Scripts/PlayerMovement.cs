using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dimension;

public class PlayerMovement : PortalTraveller
{
    [SerializeField] Grid_Show GridSystem;

    [SerializeField] CameraTest cameraTest;

    [SerializeField] bool IsOrtho;

    [SerializeField] private float moveSpeed;

    public Rigidbody rb;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    private bool isMoving;
    [SerializeField]
    private Vector3 origPos, targetPos;

    [SerializeField]
    private Transform directionCameraPosition;

    private Vector3 offset;

    // Start is called before the first frame update
    private void Start()
    {
        GridSystem = GameObject.Find("Grid").GetComponent<Grid_Show>();
        cameraTest = GameObject.Find("Main Camera").GetComponent<CameraTest>();
        float gridSizeCell = GridSystem.gridCellSize;
        offset = new Vector3(gridSizeCell / 2, 2f, gridSizeCell / 2);
        moveSpeed = GridSystem.gridCellSize*2.5f;
        Debug.Log(moveSpeed);
    }
    // Update is called once per frame
    void Update()
    {
        IsOrtho = cameraTest.GetOrthoOn();

        // Gets the value of the virtual axis identified by the given name 
        int xDir = (int)Input.GetAxisRaw("Horizontal");
        int zDir;
        if (!IsOrtho)
            zDir = (int)Input.GetAxisRaw("Vertical");
        else
            zDir = 0;


        // Sets a new direction in 3D Space based on the input)
        Vector3 targetDirection = new Vector3(xDir, 0f, zDir);

        // Changes current direction based on camera direction
        directionCameraPosition = cameraTest.orthoView.MinOrthoDist(Camera.main.transform.position);

        targetDirection = directionCameraPosition.TransformDirection(targetDirection);
        targetDirection.y = 0.0f;

        //If we want to move and we arent currently moving
        if(targetDirection != Vector3.zero && !isMoving)
        {
            if (ValidPosition(new Vector2Int((int)targetDirection.x, (int)targetDirection.z)))
            { 
                StartCoroutine(MovePlayer(targetDirection, 0.3f, rb));
            }
            print("targetDirection:" + targetDirection);
        }
    }

    private bool ValidPosition(Vector2Int direction)
    {
        Dictionary<Vector2Int, Dictionary<PlacedObjectType.Dir, List<Vector2Int>>> portalConnectors = GridSystem.GetPortalConnectors();

        origPos = transform.position;
        GridSystem.grid.GetXZ(origPos, out int x, out int z);
        Vector2Int Position = new Vector2Int(x, z);
        Vector2Int newPosition = Position + direction;
        print("newPosition:" + newPosition);
        if (GridSystem.grid.IsValidGridPosition(newPosition))
        {
            foreach (PlacedObjectType.Dir dirPortal in portalConnectors[Position].Keys)
            {
                foreach (Vector2Int positionConected in portalConnectors[Position][dirPortal])
                {
                    
                    if (positionConected == newPosition)
                        return true;
                    else
                    {
                        print("positionConected:" + positionConected);
                        print("positionConected.y-newPosition.y:" + (positionConected.y - newPosition.y));
                        print("positionConected.x - newPosition.x:" + (positionConected.x - newPosition.x));
                        if ((positionConected.x == newPosition.x && direction.y==0) || (positionConected.y == newPosition.y && direction.x == 0))
                        {
                            //print("positionConected:" + positionConected);
                            if (cameraTest.GetOrthoOn())
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
   
    private void HandleGridSnapping()
    {
        GridSystem.grid.GetXZ(transform.position, out int x, out int z);
        transform.position = GridSystem.grid.GetWorldPosition(x, z) + offset;
    }

    private IEnumerator MovePlayer(Vector3 direction, float duration, Rigidbody rb)
    {
        isMoving = true;

        float startTime = Time.time;
        while (Time.time - startTime < duration)
        {
            rb.velocity = (direction * GridSystem.gridCellSize)/ duration;
            
            yield return 1;
        }
        rb.velocity = Vector3.zero;
        HandleGridSnapping();
        isMoving = false;
    }
}
