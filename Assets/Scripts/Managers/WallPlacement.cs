/**
 *  Clean-up for re-use in MainScene 17/10
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using System;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class WallPlacement : MonoBehaviour
{
    // min wall klass - tänker jag rätt här?
    private class Wall
    {
        public GameObject startPoint;
        public GameObject endPoint;
        public GameObject quad;
        public GameObject line;

        public Wall(GameObject sPoint, GameObject ePoint, GameObject wall, GameObject lineRenderer)
        {
            startPoint = sPoint;
            endPoint = ePoint;
            quad = wall;
            line = lineRenderer;
        }
    }

    private List<Wall> walls = new List<Wall>();

    // Prefabs & materials
    [SerializeField] GameObject groundPlanePrefab;
    [SerializeField] GameObject clickPointPrefab;
    //[SerializeField] GameObject measuringStickPrefab;
    [SerializeField] Material[] materialForWalls;

    // Line renderer
    [SerializeField] GameObject lineRendererPrefab;

    // AR
    [SerializeField] ARSession arSession;
    [SerializeField] ARSessionOrigin arSessionOrigin;
    [SerializeField] ARRaycastManager arRaycastManager;
    [SerializeField] ARPlaneManager arPlaneManager;
    [SerializeField] Camera arCamera;

    // startPoint & endPoint
    private GameObject startPoint;
    private GameObject endPoint;
    private LineRenderer measureLine;
    //private GameObject measuringstick;

    // Plane, water & wall variables
    private bool planeIsPlaced;
    private float height = 4.0f;
    private GameObject groundPlane;
    private bool wallPlacementEnabled = true;
    private List<GameObject> listOfLinerenderers;
    private List<GameObject> listOfWallMeshes;

    // Raycasts
    private List<ARRaycastHit> hitsAR = new List<ARRaycastHit>();
    private List<GameObject> listOfPlacedObjects;
    private int groundLayerMask = 1 << 8;

    private void Awake()
    {
        // To be removed
        listOfPlacedObjects = new List<GameObject>();
        listOfWallMeshes = new List<GameObject>();
        listOfLinerenderers = new List<GameObject>();

        // startPoint & endPoint
        startPoint = Instantiate(clickPointPrefab, Vector3.zero, Quaternion.identity);
        endPoint = Instantiate(clickPointPrefab, Vector3.zero, Quaternion.identity);
        //measuringstick = Instantiate(measuringStickPrefab, Vector3.zero, Quaternion.identity);
        startPoint.SetActive(false);
        endPoint.SetActive(false);
        //measuringstick.SetActive(false);
        measureLine = GetComponent<LineRenderer>();
        measureLine.enabled = false;
    }

    private void Start()
    {
        #if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        #endif
    }

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject(0))
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                if (TouchPhase.Began == touch.phase)
                {
                    if (!planeIsPlaced)
                    {
                        if (arRaycastManager.Raycast(touch.position, hitsAR, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                        {
                            Debug.Log("Placed the plane");
                            var hitPose = hitsAR[0].pose;
                            groundPlane = Instantiate(groundPlanePrefab, hitPose.position, hitPose.rotation);
                            planeIsPlaced = true;
                            TogglePlaneDetection();
                        }

                        //if(planeIsPlaced)
                        //{
                        //    // Measuring stick placed 1m in front of the camera
                        //    var positionStick = new Vector3(arCamera.transform.position.x, groundPlane.transform.position.y, arCamera.transform.position.z + 1.0f);
                        //    arSessionOrigin.MakeContentAppearAt(measuringstick.transform, positionStick, Quaternion.identity);
                        //    measuringstick.GetComponent<textOverlay>().SetText("Waterlevel: \n" + "text not set yet");

                        //    // enable measuring stick
                        //    measuringstick.SetActive(true);
                        //}
                    }

                    else if (planeIsPlaced)
                    {
                        Ray ray = arCamera.ScreenPointToRay(touch.position);
                        RaycastHit hitInfo;

                        if (Physics.Raycast(ray, out hitInfo, groundLayerMask))
                        {
                            if(wallPlacementEnabled)
                            {
                                startPoint.SetActive(true);
                                startPoint.transform.SetPositionAndRotation(hitInfo.point, Quaternion.identity);

                                if (walls.Count > 0)
                                {
                                    foreach (var obj in walls)
                                    {
                                        float dist = 0;
                                        dist = Vector3.Distance(obj.endPoint.transform.position, startPoint.transform.position);
                                        if (dist < 0.1)
                                        {
                                            startPoint.transform.position = obj.endPoint.transform.position;
                                        }

                                        dist = Vector3.Distance(obj.startPoint.transform.position, startPoint.transform.position);
                                        if (dist < 0.1)
                                        {
                                            startPoint.transform.position = obj.startPoint.transform.position;
                                        }
                                    }
                                }
                            }

                            //else if(measuringstick.activeSelf)
                            //{
                            //    Debug.Log("What is active selfe of measuring stick: " + measuringstick.activeSelf);
                            //    measuringstick.transform.SetPositionAndRotation(hitInfo.point, Quaternion.identity);
                            //}
                        }
                    }
                }

                else if (TouchPhase.Moved == touch.phase && wallPlacementEnabled)
                {
                    Ray ray = arCamera.ScreenPointToRay(touch.position);
                    RaycastHit hitInfo;

                    if (Physics.Raycast(ray, out hitInfo, groundLayerMask))
                    {

                        endPoint.SetActive(true);
                        endPoint.transform.SetPositionAndRotation(hitInfo.point, Quaternion.identity);
                    }
                }

                else if (TouchPhase.Ended == touch.phase && wallPlacementEnabled && startPoint.activeSelf && endPoint.activeSelf)
                {
                    if (walls.Count > 0)
                    {
                        foreach (var obj in walls)
                        {
                            float dist = 0;
                            dist = Vector3.Distance(obj.endPoint.transform.position, endPoint.transform.position);
                            if (dist < 0.1)
                            {
                                endPoint.transform.position = obj.endPoint.transform.position;
                            }

                            dist = Vector3.Distance(obj.startPoint.transform.position, endPoint.transform.position);
                            if (dist < 0.1)
                            {
                                endPoint.transform.position = obj.startPoint.transform.position;
                            }
                        }
                    }

                    // De-activates objects/lines smaller than 20 cm
                    if (Vector3.Distance(startPoint.transform.position, endPoint.transform.position) < 0.2f)
                    {
                        startPoint.SetActive(false);
                        endPoint.SetActive(false);
                        measureLine.enabled = false;
                        return;
                    }

                    // Create the start and endpoint
                    var startPointObject = Instantiate(clickPointPrefab, startPoint.transform.position, Quaternion.identity);
                    var endPointObject = Instantiate(clickPointPrefab, endPoint.transform.position, Quaternion.identity);

                    // Disable temporary line renderer and create a new one
                    measureLine.enabled = false;
                    var lRenderer = DrawLineBetweenTwoPoints(startPoint, endPoint);

                    // Create a wall with the startpoint and endpoint as corner vertices
                    var wall = CreateQuadFromPoints(startPointObject.transform.position, endPointObject.transform.position);

                    // Then disable the startPoint and endPoint
                    startPoint.SetActive(false);
                    endPoint.SetActive(false);

                    Wall currentWall = new Wall(startPointObject, endPointObject, wall, lRenderer);
                    walls.Add(currentWall);
                    
                }
            }
        }

        // Draws a line while placing the endpoint
        if (startPoint.activeSelf && endPoint.activeSelf)
        {
            measureLine.enabled = true;
            measureLine.SetPosition(0, startPoint.transform.position);
            measureLine.SetPosition(1, endPoint.transform.position);
        }
    }

    public void RemovePreviousWall()
    {
        Debug.Log("Entered remove wall funciton");
        var length = walls.Count;
        if (length >= 1)
        {
            Wall wallToRemove = walls[length - 1];
            Destroy(wallToRemove.startPoint);
            Destroy(wallToRemove.endPoint);
            Destroy(wallToRemove.quad);
            Destroy(wallToRemove.line);
            //listOfPlacedObjects.RemoveAt(length - 1);
            //listOfPlacedObjects.RemoveAt(length - 2);
            walls.RemoveAt(length - 1);
            Debug.Log("Length of walls: " + walls.Count);
        }
    }

    public Transform GetGroundPlaneTransform()
    {
        if (groundPlane != null && planeIsPlaced)
        {
            return groundPlane.transform;
        }
        else
        {
            return null;
        }
    }

    // Helper functions
    private GameObject DrawLineBetweenTwoPoints(GameObject startPoint, GameObject endPoint)
    {
        var lineRendererGameObject = Instantiate(lineRendererPrefab);
        var lineRenderer = lineRendererGameObject.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, startPoint.transform.position);
        lineRenderer.SetPosition(1, endPoint.transform.position);
        listOfLinerenderers.Add(lineRendererGameObject);

        return lineRendererGameObject;
    }

    public void ToggleWallPlacement()
    {
        wallPlacementEnabled = !wallPlacementEnabled;
    }

    private void TogglePlaneDetection()
    {
        arPlaneManager.enabled = !arPlaneManager.enabled;

        // Go though each plane
        foreach (ARPlane plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(arPlaneManager.enabled);
        }
    }

    private GameObject CreateQuadFromPoints(Vector3 firstPoint, Vector3 secondPoint)
    {
        Debug.Log("CreateQuadeFromPoints");

        GameObject newMeshObject = new GameObject("wall");
        MeshFilter newMeshFilter = newMeshObject.AddComponent<MeshFilter>();
        newMeshObject.AddComponent<MeshRenderer>();

        // ge varje mesh ett material - 0: Occlusion
        newMeshObject.GetComponent<Renderer>().material = materialForWalls[0];
        Mesh newMesh = new Mesh();

        Vector3 heightVector = new Vector3(0, height, 0);

        newMesh.vertices = new Vector3[]
        {
            firstPoint,
            secondPoint,
            firstPoint + heightVector,
            secondPoint + heightVector
        };

        newMesh.triangles = new int[]
        {
            0,2,1,1,2,3,
        };

        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        newMesh.RecalculateBounds();

        newMeshFilter.mesh = newMesh;

        // At first the meshes aren't visible
        newMeshObject.SetActive(false);

        // Add the mesh to the list
        listOfWallMeshes.Add(newMeshObject);

        // returning the quad
        return newMeshObject;
    }

    // UI logic
    public void ResetSession()
    {
        // Destroy the placed objects if any
        for (int i = 0; i < listOfPlacedObjects.Count; i++)
        {
            Destroy(listOfPlacedObjects[i].gameObject);
        }
        listOfPlacedObjects.Clear();

        // Destroy the gameobjects holding the wall meshes
        for (int i = 0; i < listOfWallMeshes.Count; i++)
        {
            Destroy(listOfWallMeshes[i].gameObject);
        }
        listOfWallMeshes.Clear();

        for (int i = 0; i < listOfLinerenderers.Count; i++)
        {
            Destroy(listOfLinerenderers[i].gameObject);
        }
        listOfLinerenderers.Clear();

        planeIsPlaced = false;
        //arSession.Reset();
        Debug.Log("Session reset");
    }

    public void RenderWalls()
    {
        foreach(var wall in walls)
        {
            wall.startPoint.SetActive(false);
            wall.endPoint.SetActive(false);
            wall.quad.SetActive(true);
            wall.line.SetActive(false);
        }
    }
}
