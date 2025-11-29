using UnityEngine;

public class RoomAligner : MonoBehaviour
{
    public Transform roomRoot;

    public Transform arCamera;

    public float cameraHeight = 1.5f;

    private void Start()
    {
        if (roomRoot == null || arCamera == null)
        {
            Debug.LogWarning("[RoomAligner] roomRoot or arCamera not assigned.");
            return;
        }

        roomRoot.position = arCamera.position + new Vector3(0f, -cameraHeight, 0f);

        Vector3 forward = arCamera.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }
        forward.Normalize();

        roomRoot.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }
}
