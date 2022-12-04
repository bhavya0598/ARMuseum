using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
///
/// If a raycast hits a trackable, the <see cref="planetPrefabs"/> is instantiated
/// and moved to the hit position.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject[] planets;

    UnityEvent placementUpdate;

    [SerializeField]
    GameObject visualObject;

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject[] planetPrefabs
    {
        get { return planets; }
        set { planets = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedPlanets { get; private set; }


    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;

    ARPlaneManager m_ARPlaneManager;

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_ARPlaneManager= GetComponent<ARPlaneManager>();

        if (placementUpdate == null)
            placementUpdate = new UnityEvent();

        placementUpdate.AddListener(DiableVisual);
    }

    void Update()
    {
        if (!TryGetFirstTouchPosition(out Vector2 tocuhPosition))
            return;

        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            if (m_RaycastManager.Raycast(tocuhPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = s_Hits[0].pose;

                foreach (var plane in m_ARPlaneManager.trackables)
                { 
                    plane.gameObject.SetActive(false);
                }
                m_ARPlaneManager.enabled = false;
                spawnedPlanets = Instantiate(planets[Random.Range(0, planets.Length)], hitPose.position, hitPose.rotation);
                placementUpdate.Invoke();
            }
        }
    }

    bool TryGetFirstTouchPosition(out Vector2 firstTouchPosition)
    {
        if (Input.touchCount > 0)
        {
            firstTouchPosition = Input.GetTouch(0).position;
            return true;
        }

        firstTouchPosition = default;
        return false;
    }

    public void DiableVisual()
    {
        visualObject.SetActive(false);
    }
}