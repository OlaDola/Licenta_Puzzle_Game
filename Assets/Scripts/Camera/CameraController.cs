
using UnityEngine;

namespace Dimension
{

    public class CameraController : MonoBehaviour
    {

        Vector3 GridSystem;

        public Transform perspectiveView;
        public Transform[] orthographicViews;
        public float transitionSpeed = 0.5f;

        [SerializeField] protected Transform currentView;

        [SerializeField] Transform previousFinalView;

        public static Transform currentViewPublic;

        public float rotationSpeed;

        public Camera MainCamera;

        public bool ChangeProjection = false;
        public bool canChangeProjection = true;
        public bool canChangeFromOrtho = true;
        public bool ChangeFromOrtho = false;

        private bool ChangingProj = false;
        private bool ChangingOrtho = false;
        private float currentT = 0.0f;

        [SerializeField] int i;

        private Vector3 previousPosition;


        private void Awake()
        {
            currentView = perspectiveView;
            previousFinalView = currentView;

            currentViewPublic = currentView;

        }


        private void Update()
        {

            //GridSystem = GameObject.Find("GridBuildingSystem").GetComponent<GridBuildingSystem>().TempGridOrigin;
            if (ChangingProj && canChangeProjection)
            {
                ChangeProjection = false;
            }
            else if (ChangingOrtho && canChangeFromOrtho)
            {
                ChangeFromOrtho = false;
            }
            else if (ChangeProjection && canChangeProjection)
            {
                ChangingProj = true;
                currentT = 0.0f;
            }
            else if (canChangeFromOrtho && ChangeFromOrtho)
            {
                ChangingOrtho = true;
                currentT = 0.0f;
            }

            if (!ChangingProj && !ChangingOrtho)
            {
                if (Input.GetKeyDown(KeyCode.Tab) && canChangeProjection)
                {

                    if (GetComponent<Camera>().orthographic)
                    {
                        currentView = previousFinalView;
                    }
                    else
                    {
                        previousFinalView = perspectiveView;

                        MinDistance();
                    }

                    ChangeProjection = true;
                }

                if (GetComponent<Camera>().orthographic && canChangeFromOrtho)
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        ChangeLeft();
                        ChangeFromOrtho = true;
                    }
                    else if (Input.GetKeyDown(KeyCode.E))
                    {
                        ChangeRight();
                        ChangeFromOrtho = true;
                    }
                }

                if (!(GetComponent<Camera>().orthographic))
                {
                    //First click gets the current possition of the camera
                    if (Input.GetMouseButtonDown(0))
                    {
                        previousPosition = MainCamera.ScreenToViewportPoint(Input.mousePosition);
                    }

                    //While holding the left mouse button continue to get possition of the camera and rotate around the Y Axis
                    if (Input.GetMouseButton(0))
                    {
                        Vector3 direction = previousPosition - MainCamera.ScreenToViewportPoint(Input.mousePosition);

                        perspectiveView.RotateAround(GridSystem, new Vector3(0, 3, 0), (-direction.x * 180) / rotationSpeed);

                        MainCamera.transform.RotateAround(GridSystem, new Vector3(0, 3, 0), (-direction.x * 180) / rotationSpeed);

                        previousPosition = MainCamera.ScreenToViewportPoint(Input.mousePosition);
                    }
                }
            }

            currentViewPublic = currentView;

        }

        private void ChangeRight()
        {
            i++;

            i %= 4;

            currentView = orthographicViews[i];
        }

        private void ChangeLeft()
        {
            if (i == 0)
            {
                i = 4;
            }

            i--;

            currentView = orthographicViews[i];
        }

        void LateUpdate()
        {

            if (!ChangingProj && !ChangingOrtho)
            {
                return;
            }

            var currentlyOrthographic = GetComponent<Camera>().orthographic;
            Matrix4x4 orthoMat, persMat;
            if (ChangingProj)
            {

                if (currentlyOrthographic)
                {

                    orthoMat = GetComponent<Camera>().projectionMatrix;

                    GetComponent<Camera>().orthographic = false;
                    GetComponent<Camera>().ResetProjectionMatrix();

                    persMat = GetComponent<Camera>().projectionMatrix;
                }
                else
                {
                    persMat = GetComponent<Camera>().projectionMatrix;

                    GetComponent<Camera>().orthographic = true;
                    GetComponent<Camera>().ResetProjectionMatrix();

                    orthoMat = GetComponent<Camera>().projectionMatrix;
                }


                GetComponent<Camera>().orthographic = currentlyOrthographic;

                currentT += Time.deltaTime / transitionSpeed;

                if (currentT < 1.0f)
                {
                    if (currentlyOrthographic)
                    {
                        ChangePosition();
                        GetComponent<Camera>().projectionMatrix = MatrixLerp(orthoMat, persMat, currentT * currentT);
                    }
                    else
                    {
                        ChangePosition();
                        GetComponent<Camera>().projectionMatrix = MatrixLerp(persMat, orthoMat, Mathf.Sqrt(currentT));
                    }
                }
                else
                {
                    ChangingProj = false;
                    GetComponent<Camera>().orthographic = !currentlyOrthographic;
                    GetComponent<Camera>().ResetProjectionMatrix();
                }
            }
            else if (ChangingOrtho)
            {
                currentT += (Time.deltaTime / transitionSpeed);

                if (currentT < 1.0f)
                {
                    ChangePosition();
                }
                else
                {
                    ChangingOrtho = false;
                }
            }
        }

        private void MinDistance()
        {
            i = 0;
            float distance = float.MaxValue;

            for (int j = 0; j < orthographicViews.Length; j++)
            {
                if (Mathf.Abs(Vector3.Distance(previousFinalView.position, orthographicViews[j].position)) < distance)
                {
                    distance = Mathf.Abs(Vector3.Distance(previousFinalView.position, orthographicViews[j].position));

                    currentView = orthographicViews[j];
                    i = j;
                }
            }
        }

        private void ChangePosition()
        {

            Vector3 pos = MainCamera.transform.position;
            Quaternion rot = MainCamera.transform.rotation;

            MainCamera.transform.position = Vector3.Slerp(pos, currentView.transform.position, Time.deltaTime * transitionSpeed);
            MainCamera.transform.rotation = Quaternion.Slerp(rot, currentView.transform.rotation, Time.deltaTime * transitionSpeed);
        }


        private Matrix4x4 MatrixLerp(Matrix4x4 from, Matrix4x4 to, float t)
        {
            t = Mathf.Clamp(t, 0.0f, 1.0f);

            var newMatrix = new Matrix4x4();
            newMatrix.SetRow(0, Vector4.Lerp(from.GetRow(0), to.GetRow(0), t));
            newMatrix.SetRow(1, Vector4.Lerp(from.GetRow(1), to.GetRow(1), t));
            newMatrix.SetRow(2, Vector4.Lerp(from.GetRow(2), to.GetRow(2), t));
            newMatrix.SetRow(3, Vector4.Lerp(from.GetRow(3), to.GetRow(3), t));

            //Debug.Log(from);

            return newMatrix;
        }
    }
}
