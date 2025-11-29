using UnityEngine;
using Vuforia;
using System.Collections;

public class BossFightManager : MonoBehaviour
{
    [Header("Boss Object Target")]
    [SerializeField] private ObserverBehaviour bossObserver;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject bossProjectilePrefab;
    [SerializeField] private GameObject playerProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip bossShootSound;
    [SerializeField] private AudioClip playerShootSound;

    private Transform bossTransform;
    private Transform playerTransform;
    private AudioSource audioSource;
    private bool isBossTracked = false;
    private bool wasTracking = false;
    private GameObject bossVisualInstance;
    private Coroutine bossAttackCoroutine;

    private void Start()
    {
        playerTransform = Camera.main.transform;
        bossTransform = transform;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.5f;
    }

    private void OnDisable()
    {
        if (bossAttackCoroutine != null)
        {
            StopCoroutine(bossAttackCoroutine);
            bossAttackCoroutine = null;
        }
        isBossTracked = false;
        wasTracking = false;
    }

    private void Update()
    {

        bool isTracking =
            bossObserver.TargetStatus.Status == Status.TRACKED ||
            bossObserver.TargetStatus.Status == Status.EXTENDED_TRACKED;

        if (isTracking && !wasTracking)
        {
            OnBossFound();
        }
        else if (!isTracking && wasTracking)
        {
            OnBossLost();
        }

        wasTracking = isTracking;
    }

    private void OnBossFound()
    {
        if (!isBossTracked && GameManager.Instance != null && GameManager.Instance.IsBossFightActive())
        {
            isBossTracked = true;
            StartBossFight();
        }
    }

    private void OnBossLost()
    {
        isBossTracked = false;
        Debug.Log("[BossFightManager] Boss target lost.");
    }

    private void StartBossFight()
    {
        Debug.Log("[BossFightManager] Boss fight started!");

        if (bossAttackCoroutine == null)
        {
            bossAttackCoroutine = StartCoroutine(BossAttackRoutine());
        }
    }

    private IEnumerator BossAttackRoutine()
    {
        while (GameManager.Instance != null && GameManager.Instance.IsBossFightActive())
        {
            float interval = GameManager.Instance.GetProjectileInterval();
            yield return new WaitForSeconds(interval);

            if (isBossTracked)
            {
                BossShoot();
            }
        }

        bossAttackCoroutine = null;
    }

    private void BossShoot()
    {

        Vector3 spawnPos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + Vector3.up * 0.2f;

        GameObject projectile = Instantiate(bossProjectilePrefab, spawnPos, Quaternion.identity);

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<Projectile>();
        }

        Vector3 targetPos = playerTransform.position;
        Vector3 direction = (targetPos - spawnPos).normalized;
        float speed = GameManager.Instance.GetProjectileSpeed();

        projScript.Initialize(direction, speed, true);

        PlaySound(bossShootSound);
        Debug.Log("[BossFightManager] Boss shoots projectile!");
    }

    public void PlayerShoot()
    {
        if (GameManager.Instance == null ||
            !GameManager.Instance.IsBossFightActive() ||
            !isBossTracked)
            return;

        Vector3 spawnPos = playerTransform.position + playerTransform.forward * 0.5f;
        GameObject projectile = Instantiate(playerProjectilePrefab, spawnPos, Quaternion.identity);

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<Projectile>();
        }

        Vector3 bossPos = bossTransform.position;
        Vector3 direction = (bossPos - spawnPos).normalized;
        float speed = GameManager.Instance.GetProjectileSpeed();

        projScript.Initialize(direction, speed, false);

        PlaySound(playerShootSound);
        Debug.Log("[BossFightManager] Player shoots projectile!");
    }

    public void OnBossHit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.BossHit();
        }
    }

    public void OnPlayerHit()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerHit();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public Transform GetBossTransform()
    {
        return bossTransform;
    }

    public void TryStartBossFightIfTracked()
    {
        if (bossObserver == null ||
            GameManager.Instance == null ||
            !GameManager.Instance.IsBossFightActive() ||
            isBossTracked)
            return;

        bool isTracking =
            bossObserver.TargetStatus.Status == Status.TRACKED ||
            bossObserver.TargetStatus.Status == Status.EXTENDED_TRACKED;

        if (isTracking)
        {
            isBossTracked = true;
            StartBossFight();
        }
    }
}
