using UnityEngine;
using Vuforia;

public class ObjectTracker : MonoBehaviour
{
    [Header("Object Info")]
    [SerializeField] private string objectName = "Water Bottle";
    [SerializeField] private int objectIndex = 0;

    [Header("Visual Feedback")]
    [SerializeField] private GameObject checkmarkPrefab;

    private ObserverBehaviour observerBehaviour;
    private bool hasBeenFound = false;
    private GameObject checkmarkInstance;
    private bool wasTracking = false;

    private void Start()
    {
        observerBehaviour = GetComponent<ObserverBehaviour>();
    }

    private void Update()
    {
        if (observerBehaviour == null) return;

        bool isTracking =
            observerBehaviour.TargetStatus.Status == Status.TRACKED ||
            observerBehaviour.TargetStatus.Status == Status.EXTENDED_TRACKED;

        if (isTracking && !wasTracking)
        {
            OnTargetFound();
        }
        else if (!isTracking && wasTracking)
        {
            OnTargetLost();
        }

        wasTracking = isTracking;
    }

    private void OnTargetFound()
    {
        if (!hasBeenFound)
        {
            hasBeenFound = true;
            ObjectFoundFirstTime();
        }

        Debug.Log($"[ObjectTracker] Tracking: {objectName}");
    }

    private void OnTargetLost()
    {
        Debug.Log($"[ObjectTracker] Lost tracking: {objectName}");
    }

    private void ObjectFoundFirstTime()
    {
        Debug.Log($"[ObjectTracker] Found for first time: {objectName}");

        if (checkmarkPrefab != null && checkmarkInstance == null)
        {
            checkmarkInstance = Instantiate(
                checkmarkPrefab,
                transform.position + Vector3.up * 0.2f,
                Quaternion.identity,
                transform);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ObjectFound(objectIndex, objectName);
        }
    }

    public bool IsFound() => hasBeenFound;
}
