using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private GameObject trophyRoot;

    [SerializeField] private string triggerTag = "MainCamera";

    [SerializeField] private string trophyName = "Timing Trophy";

    private bool hasBeenTriggered = false;
    private Renderer[] trophyRenderers;

    private void Awake()
    {
        if (trophyRoot == null && transform.parent != null)
        {
            trophyRoot = transform.parent.gameObject;
        }

        if (trophyRoot != null)
        {
            trophyRenderers = trophyRoot.GetComponentsInChildren<Renderer>(true);
        }
        else
        {
            Debug.LogWarning($"[TriggerZone] No trophyRoot assigned on {name}");
        }
    }

    private void Start()
    {
        SetTrophyVisible(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;
        if (!other.CompareTag(triggerTag)) return;

        hasBeenTriggered = true;

        SetTrophyVisible(true);

        Debug.Log($"[TriggerZone] Player entered. Trophy shown: {trophyName}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClueFound(trophyName);
        }
    }

    private void SetTrophyVisible(bool visible)
    {
        if (trophyRenderers == null) return;

        foreach (var r in trophyRenderers)
        {
            if (r != null)
            {
                r.enabled = visible;
            }
        }
    }
}
