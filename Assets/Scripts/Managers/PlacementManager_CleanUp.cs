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

public class PlacementManager_CleanUp : MonoBehaviour
{
    // Prefabs & materials
    public GameObject groundPlanePrefab;
    public GameObject clickPointPrefab;
    public Material[] materialForWalls;

    // startPoint & endPoint
    private GameObject startPoint;
    private GameObject endPoint;
    private LineRenderer measureLine;

    // Plane, water & wall variables
    private bool planeIsPlaced;
    private float height = 4.0f;
    private GameObject groundPlane;
    private bool wallPlacementEnabled = true;
    private bool toggleVisibilityOfWalls;
    private List<GameObject> listOfLinerenderers;

    // Line renderer
    [SerializeField] GameObject lineRendererPrefab;

    // AR
    [SerializeField] ARSession arSession;
    [SerializeField] ARRaycastManager arRaycastManager;
    [SerializeField] ARPlaneManager arPlaneManager;
    [SerializeField] Camera arCamera;

    // Raycasts
    private List<ARRaycastHit> hitsAR = new List<ARRaycastHit>();
    private RaycastHit hits;
    private bool HasSavedPoint;
    private Vector3 savedPoint;
    private List<GameObject> listOfPlacedObjects;
    private int groundLayerMask = 1 << 8;

    // ProBuilder variables
    private GameObject waterGameObject;
    private int pipeSubdivAxis = 10;
    private int pipeSubdivHeight = 4;
    private float holeSize = 0.5f;
    private float pipeHeight = 0.1f;
    private List<GameObject> listOfWallMeshes;

    // UI slider values

    private textOverlayMultSlider multiplierText;
    private textOverlayRadiusSlider radiusText;

    // Elevation variables
    private float elevation = 0.001f;

    private void Awake()
    {
        // Lists for wall objects
        listOfPlacedObjects = new List<GameObject>();
        listOfWallMeshes = new List<GameObject>();
        listOfLinerenderers = new List<GameObject>();

        // startPoint & endPoint
        startPoint = Instantiate(clickPointPrefab, Vector3.zero, Quaternion.identity);
        endPoint = Instantiate(clickPointPrefab, Vector3.zero, Quaternion.identity);
        startPoint.SetActive(false);
        endPoint.SetActive(false);
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
                    }

                    else if (planeIsPlaced)
                    {
                        Ray ray = arCamera.ScreenPointToRay(touch.position);
                        RaycastHit hitInfo;

                        if (Physics.Raycast(ray, out hitInfo, groundLayerMask))
                        {
                            startPoint.SetActive(true);
                            startPoint.transform.SetPositionAndRotation(hitInfo.point, Quaternion.identity);

                            // Snapping startPoint
                            if (listOfPlacedObjects != null)
                            {
                                foreach (var point in listOfPlacedObjects)
                                {
                                    if (point.transform.position != startPoint.transform.position)
                                    {
                                        float dist = Vector3.Distance(point.transform.position, startPoint.transform.position);
                                        if (dist < 0.1)
                                        {
                                            startPoint.transform.position = point.transform.position;
                                        }
                                    }
                                }
                            }
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
                    // Snapping the endPoint
                    if (listOfPlacedObjects != null)
                    {
                        foreach (var point in listOfPlacedObjects)
                        {
                            if (point.transform.position != endPoint.transform.position)
                            {
                                float dist = Vector3.Distance(point.transform.position, endPoint.transform.position);
                                if (dist < 0.1)
                                {
                                    endPoint.transform.position = point.transform.position;
                                }
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
                    listOfPlacedObjects.Add(startPointObject);
                    listOfPlacedObjects.Add(endPointObject);

                    // Disable temporary line renderer and create a new one
                    measureLine.enabled = false;
                    DrawLineBetweenTwoPoints(startPoint, endPoint);

                    // Then disable the startPoint and endPoint
                    startPoint.SetActive(false);
                    endPoint.SetActive(false);

                    // Create a wall with the startpoint and endpoint as corner vertices
                    CreateQuadFromPoints(startPointObject.transform.position, endPointObject.transform.position);
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

    // Helper functions
    private void DrawLineBetweenTwoPoints(GameObject startPoint, GameObject endPoint)
    {
        var lineRendererGameObject = Instantiate(lineRendererPrefab);
        var lineRenderer = lineRendererGameObject.GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, startPoint.transform.position);
        lineRenderer.SetPosition(1, endPoint.transform.position);
        listOfLinerenderers.Add(lineRendererGameObject);
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

    private void CreateQuadFromPoints(Vector3 firstPoint, Vector3 secondPoint)
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
    }

    private void renderWallMeshes(bool isVisible)
    {
        foreach (GameObject wallMesh in listOfWallMeshes)
        {
            wallMesh.SetActive(isVisible);
        }
    }

    private void renderClickPoints(bool isVisible)
    {
        foreach (GameObject point in listOfPlacedObjects)
        {
            point.SetActive(isVisible);
        }
    }

    private void renderLineRenderers(bool isVisible)
    {
        foreach (GameObject line in listOfLinerenderers)
        {
            line.SetActive(isVisible);
        }
    }

    private void DrawLinesBetweenObjects()
    {
        int lengthOfList = listOfPlacedObjects.Count;
        if (lengthOfList > 1)
        {
            for (int i = 0; i < lengthOfList - 1; i++)
            {
                try
                {
                    var lineRendererGameObject = Instantiate(lineRendererPrefab);
                    var lineRenderer = lineRendererGameObject.GetComponent<LineRenderer>();
                    lineRenderer.SetPosition(0, listOfPlacedObjects[i].transform.position);
                    lineRenderer.SetPosition(1, listOfPlacedObjects[i + 1].transform.position);
                    listOfLinerenderers.Add(lineRendererGameObject);
                }
                catch (Exception)
                {
                    Debug.LogError("Exceptions baby!");
                    throw;
                }
            }
        }
    }

    // UI logic
    public void ResetSession()
    {
        if (waterGameObject != null)
            Destroy(waterGameObject);

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

        HasSavedPoint = false;
        elevation = 0.0f;
        arSession.Reset();
        Debug.Log("Session reset");
    }

    public void RenderWalls()
    {
        renderWallMeshes(true);
        renderClickPoints(true);
        renderLineRenderers(true);
    }
}
