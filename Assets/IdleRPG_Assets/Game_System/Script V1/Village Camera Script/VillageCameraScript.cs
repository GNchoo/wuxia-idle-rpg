using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SAMPLETEXT.Village.Cam
{
    public class VillageCameraScript : MonoBehaviour
    {
        [Header("Condition Settings")]
        public bool VillageView;
        [Header("Camera Drag Settings")]
        Vector3 touchStart;
        [SerializeField]
        Camera VillageCamera;
        [SerializeField]
        GameObject VillageTabView;

        [Header("Camera Zoom Settings")]
        [SerializeField]
        float zoomOutMin;
        [SerializeField]
        float zoomOutMax;

        [Header("Camera Border Settings")]
        [SerializeField]
        float XLeftBorder;
        [SerializeField]
        float XRightBorder;
        [SerializeField]
        float YTopBorder;
        [SerializeField]
        float YBottomBorder;

        float XLeftZoomValue;
        float XRightZoomValue;
        float YTopZoomValue;
        float YBottomZoomValue;

        [Header("Village Name Settings")]
        [SerializeField]
        GameObject[] VillageNameCollectionObj;
        // Start is called before the first frame update
        void Start()
        {
            //var height = VillageCamera.orthographicSize;
            //var width = height * VillageCamera.aspect;

            ////var minX = GlobalScript.WorldBounds.min.x + width;
            ////var maxX = GlobalScript.WorldBounds.extents.x - width;

            //var minX = BoundManager.ThisBounds.min.x + width;
            //var maxX = BoundManager.ThisBounds.extents.x - width;

            //var minY = BoundManager.ThisBounds.min.y + height;
            //var maxY = BoundManager.ThisBounds.extents.y - height;

            //_CameraBounds = new Bounds();
            //_CameraBounds.SetMinMax(new Vector3(minX, minY, 0.0f), new Vector3(maxX, maxY, 0.0f));
        }

        void CameraMovement()
        {
            //VillageCamera.transform.position = new Vector3(Mathf.Clamp(VillageCamera.transform.position.x, XLeftBorder, XRightBorder), Mathf.Clamp(VillageCamera.transform.position.y, YBottomBorder, YTopBorder),transform.position.z);

            if (VillageTabView.activeSelf == false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    touchStart = VillageCamera.ScreenToWorldPoint(Input.mousePosition);
                }
                if (Input.GetMouseButton(0))
                {
                    Vector3 direction = touchStart - VillageCamera.ScreenToWorldPoint(Input.mousePosition);

                    VillageCamera.transform.position += direction;

                    //if (VillageCamera.orthographicSize == zoomOutMin)
                    //{
                    //    XLeftBorder = .5f;
                    //    XRightBorder = 4f;
                    //    YTopBorder = 3f;
                    //    YBottomBorder = -3f;
                    //}

                    //else if (VillageCamera.orthographicSize > zoomOutMin)
                    //{
                    //    XLeftBorder = 1.5f;
                    //    XRightBorder = 3f;
                    //    YTopBorder = 1f;
                    //    YBottomBorder = -1f;
                    //}
                    //else if (VillageCamera.orthographicSize != zoomOutMax && VillageCamera.orthographicSize != zoomOutMin)
                    //{
                    //    XLeftBorder = 1f;
                    //    XRightBorder = 3.5f;
                    //    YTopBorder = 1.3f;
                    //    YBottomBorder = -1.3f;
                    //}

                    //VillageCamera.transform.position = new Vector3(Mathf.Clamp(VillageCamera.transform.position.x, XLeftBorder, XRightBorder), Mathf.Clamp(VillageCamera.transform.position.y, YBottomBorder, YTopBorder), transform.position.z);


                }
                zoom(Input.GetAxis("Mouse ScrollWheel"));


                if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;

                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                    float difference = currentMagnitude - prevMagnitude;

                    zoom(difference * 0.01f);

                }
                else
                {

                }


            }

        }
        void CameraMovementUpdate()
        {
            //if (VillageCamera.orthographicSize >= 9)
            //{
            //    XLeftBorder = 1.2f;
            //    XRightBorder = 3.3f;

            //}

            //if (VillageCamera.orthographicSize >= 7 && VillageCamera.orthographicSize <= 8)
            //{
            //    XLeftBorder = .5f;
            //    XRightBorder = 3.7f;

            //}

            //if (VillageCamera.orthographicSize < 7)
            //{
            //    XLeftBorder = 0;
            //    XRightBorder = 4.3f;
            //}
            //float Height = 2f * VillageCamera.orthographicSize;
            //float width = Height * VillageCamera.aspect;

            if (VillageTabView.activeSelf == false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    touchStart = VillageCamera.ScreenToWorldPoint(Input.mousePosition);
                }
                if (Input.GetMouseButton(0))
                {
                    Vector3 direction = touchStart - VillageCamera.ScreenToWorldPoint(Input.mousePosition);

                    VillageCamera.transform.position += direction;

                    //VillageCamera.transform.position = GetCameraBounds();

                    //Vector3 Cam = VillageCamera.transform.position;


                    //float aspect = VillageCamera.aspect;
                    //float CamSize = VillageCamera.orthographicSize;

                    //Cam.x = Mathf.Clamp(Cam.x, XLeftBorder + CamSize * aspect, XRightBorder - CamSize * aspect);
                    //Cam.y = Mathf.Clamp(Cam.y, YBottomBorder + CamSize, YTopBorder - CamSize);

                    //VillageCamera.transform.position = Cam;


                    //VillageCamera.transform.position = new Vector3(Mathf.Clamp(VillageCamera.transform.position.x, XLeftBorder / VillageCamera.orthographicSize, XRightBorder / VillageCamera.orthographicSize), Mathf.Clamp(VillageCamera.transform.position.y, YBottomBorder / VillageCamera.orthographicSize, YTopBorder / VillageCamera.orthographicSize), transform.position.z);

                }
                zoom(Input.GetAxis("Mouse ScrollWheel"));

                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    //XLeftZoomValue += .1f;
                    //XRightZoomValue += .1f;

                    //if (XLeftZoomValue >= 1.8f)
                    //{
                    //    XLeftZoomValue = 1.8f;
                    //}

                    //if (XRightZoomValue >= 1.4f)
                    //{
                    //    XRightZoomValue = 1.4f;
                    //}

                    //YTopZoomValue += .08f;
                    //YBottomZoomValue += .08f;

                    //if (YTopZoomValue >= 2.6f)
                    //{
                    //    YTopZoomValue = 2.6f;
                    //}

                    //if (YBottomZoomValue >= 2.6f)
                    //{
                    //    YBottomZoomValue = 2.6f;
                    //}
                    //Debug.Log(XLeftZoomValue);
                }

                if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    //XLeftZoomValue -= .1f;
                    //XRightZoomValue -= .1f;
                    //if (XLeftZoomValue <= 0f)
                    //{
                    //    XLeftZoomValue = 0f;
                    //}

                    //if (XRightZoomValue <= 0)
                    //{
                    //    XRightZoomValue = 0f;
                    //}

                    //YTopZoomValue -= .1f;
                    //YBottomZoomValue -= .1f;

                    //if (YTopZoomValue <= 0f)
                    //{
                    //    YTopZoomValue = 0f;
                    //}

                    //if (YBottomZoomValue <= 0f)
                    //{
                    //    YBottomZoomValue = 0f;
                    //}
                    //Debug.Log(XLeftZoomValue);
                }


                if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;

                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                    float difference = currentMagnitude - prevMagnitude;

                    zoom(difference * 0.01f);


                    if (difference >= 0)
                    {
                        //XLeftZoomValue += .1f;
                        //XRightZoomValue += .1f;

                        //if (XLeftZoomValue >= 1.8f)
                        //{
                        //    XLeftZoomValue = 1.8f;
                        //}

                        //if (XRightZoomValue >= 1.4f)
                        //{
                        //    XRightZoomValue = 1.4f;
                        //}

                        //YTopZoomValue += .1f;
                        //YBottomZoomValue += .1f;

                        //if (YTopZoomValue >= 2.6f)
                        //{
                        //    YTopZoomValue = 2.6f;
                        //}

                        //if (YBottomZoomValue >= 2.6f)
                        //{
                        //    YBottomZoomValue = 2.6f;
                        //}
                    }

                    if (difference < 0)
                    {
                        //XLeftZoomValue -= .1f;
                        //XRightZoomValue -= .1f;
                        //if (XLeftZoomValue <= 0f)
                        //{
                        //    XLeftZoomValue = 0f;
                        //}

                        //if (XRightZoomValue <= 0)
                        //{
                        //    XRightZoomValue = 0f;
                        //}

                        //YTopZoomValue -= .1f;
                        //YBottomZoomValue -= .1f;

                        //if (YTopZoomValue <= 0f)
                        //{
                        //    YTopZoomValue = 0f;
                        //}

                        //if (YBottomZoomValue <= 0f)
                        //{
                        //    YBottomZoomValue = 0f;
                        //}
                    }
                }
                else
                {

                }


                if (VillageCamera.orthographicSize <= zoomOutMin + 1)
                {
                    foreach (GameObject VNCO in VillageNameCollectionObj)
                    {
                        VNCO.gameObject.SetActive(true);
                    }
                }

                if (VillageCamera.orthographicSize > zoomOutMin + 1)
                {
                    foreach (GameObject VNCO in VillageNameCollectionObj)
                    {
                        VNCO.gameObject.SetActive(false);
                    }
                }
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            //CameraMovement();
            CameraMovementUpdate();
        }

        private void LateUpdate()
        {

            //VillageCamera.transform.position = new Vector3(Mathf.Clamp(VillageCamera.transform.position.x, XLeftBorder - XLeftZoomValue, XRightBorder + XRightZoomValue), Mathf.Clamp(VillageCamera.transform.position.y, YBottomBorder - YBottomZoomValue, YTopBorder + YTopZoomValue), transform.position.z);

            ClampCamera();
        }

        void ClampCamera()
        {
            Vector3 clampMovement = VillageCamera.transform.position;

            float CamSize = VillageCamera.orthographicSize;
            float aspectSize = VillageCamera.aspect;

            //clampMovement.x = Mathf.Clamp(clampMovement.x, 8f + CamSize * aspectSize, 112 - CamSize * aspectSize);
            clampMovement.x = Mathf.Clamp(clampMovement.x, XLeftBorder + CamSize * aspectSize, XRightBorder - CamSize * aspectSize);
            clampMovement.y = Mathf.Clamp(clampMovement.y, YBottomBorder + CamSize, YTopBorder - CamSize);

            VillageCamera.transform.position = clampMovement;
        }

        void zoom(float increment)
        {
            VillageCamera.orthographicSize = Mathf.Clamp(VillageCamera.orthographicSize - increment, zoomOutMin, zoomOutMax);
        }
    }

}

