using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private bool isBossProjectile;
    private bool hasHit = false;

    [SerializeField] private float lifetime = 10f;
    [SerializeField] private GameObject impactEffectPrefab;

    private float spawnTime;
    private BossFightManager bossFightManager;

    private void Start()
    {
        spawnTime = Time.time;
        bossFightManager = FindAnyObjectByType<BossFightManager>();

        SphereCollider collider = gameObject.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
            collider.isTrigger = true;
        }

        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public void Initialize(Vector3 dir, float spd, bool fromBoss)
    {
        direction = dir.normalized;
        speed = spd;
        isBossProjectile = fromBoss;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Time.time - spawnTime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (isBossProjectile)
        {
            if (other.CompareTag("MainCamera"))
            {
                HitPlayer();
            }
        }
        else
        {
            if (other.GetComponent<BossFightManager>() != null)
            {
                HitBoss();
            }
        }

        // if (!hasHit && other.gameObject.layer == LayerMask.NameToLayer("Default"))
        // {
        //     DestroyProjectile();
        // }
    }

    private void HitPlayer()
    {
        hasHit = true;

        if (bossFightManager != null)
        {
            bossFightManager.OnPlayerHit();
        }

        SpawnImpactEffect();
        Destroy(gameObject);
    }

    private void HitBoss()
    {
        hasHit = true;

        if (bossFightManager != null)
        {
            bossFightManager.OnBossHit();
        }

        SpawnImpactEffect();
        Destroy(gameObject);
    }

    private void DestroyProjectile()
    {
        hasHit = true;
        SpawnImpactEffect();
        Destroy(gameObject);
    }

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
}
