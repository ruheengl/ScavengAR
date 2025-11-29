using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIFlowController : MonoBehaviour
{
    public static UIFlowController Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject coverPanel;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject stage1Panel;
    [SerializeField] private GameObject stage1EndPanel;
    [SerializeField] private GameObject stage2Panel;
    [SerializeField] private GameObject stage2WinEndPanel;
    [SerializeField] private GameObject stage2LossEndPanel;
    [SerializeField] private GameObject stage3EasyPanel;
    [SerializeField] private GameObject stage3HardPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Cover Panel Settings")]
    [SerializeField] private float coverDuration = 2f;

    [Header("Boss Fight UI")]
    [SerializeField] private Button shootButton;
    [SerializeField] private Image shootButtonFillImage;
    [SerializeField] private float shootCooldown = 3f;
    [SerializeField] private BossFightManager bossFightManager;

    private bool canShoot = false;
    private float cooldownTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ShowOnly(coverPanel);
        StartCoroutine(CoverThenStory());

        if (shootButton != null)
        {
            shootButton.onClick.AddListener(OnShootButtonPressed);
            shootButton.gameObject.SetActive(false);
            shootButton.interactable = false;
        }

        if (shootButtonFillImage != null)
        {
            shootButtonFillImage.fillAmount = 0f;
        }

        if (bossFightManager == null)
        {
            bossFightManager = FindAnyObjectByType<BossFightManager>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (!canShoot && GameManager.Instance.IsBossFightActive())
        {
            cooldownTimer += Time.deltaTime;

            if (shootButtonFillImage != null)
            {
                float progress = Mathf.Clamp01(cooldownTimer / shootCooldown);
                shootButtonFillImage.fillAmount = progress;
            }

            if (cooldownTimer >= shootCooldown)
            {
                canShoot = true;
                cooldownTimer = 0f;

                if (shootButton != null)
                {
                    shootButton.interactable = true;
                }
            }
        }
    }

    private IEnumerator CoverThenStory()
    {
        yield return new WaitForSeconds(coverDuration);
        ShowOnly(storyPanel);
    }

    private void ShowOnly(GameObject panelToShow)
    {
        GameObject[] allPanels =
        {
            coverPanel, storyPanel, stage1Panel, stage1EndPanel,
            stage2Panel, stage2WinEndPanel, stage2LossEndPanel,
            stage3EasyPanel, stage3HardPanel, victoryPanel, gameOverPanel
        };

        foreach (var p in allPanels)
        {
            if (p != null) p.SetActive(false);
        }

        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }

    private void HideAllPanels()
    {
        ShowOnly(null);
    }

    public void OnStoryNext()
    {
        ShowOnly(stage1Panel);
    }

    public void OnStage1Start()
    {
        HideAllPanels();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartStage1FromUI();
        }
    }

    public void ShowStage1End()
    {
        ShowOnly(stage1EndPanel);
    }

    public void OnStage1EndNext()
    {
        ShowOnly(stage2Panel);
    }

    public void OnStage2Start()
    {
        HideAllPanels();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartStage2FromUI();
        }
    }

    public void ShowStage2End(bool win)
    {
        if (win)
        {
            ShowOnly(stage2WinEndPanel);
        }
        else
        {
            ShowOnly(stage2LossEndPanel);
        }
    }

    public void OnStage2WinNext()
    {
        ShowOnly(stage3EasyPanel);
    }

    public void OnStage2LossNext()
    {
        ShowOnly(stage3HardPanel);
    }

    public void OnStage3EasyStart()
    {
        StartBossFromPanel(false);
    }

    public void OnStage3HardStart()
    {
        StartBossFromPanel(true);
    }

    private void StartBossFromPanel(bool isHardMode)
    {
        HideAllPanels();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartBossFightFromUI(isHardMode);
        }

        if (shootButton != null)
        {
            shootButton.gameObject.SetActive(true);
            shootButton.interactable = false;
        }

        canShoot = false;
        cooldownTimer = 0f;

        if (shootButtonFillImage != null)
        {
            shootButtonFillImage.fillAmount = 0f;
        }
    }

    public void ShowGameOver(bool playerWon)
    {
        if (shootButton != null)
        {
            shootButton.gameObject.SetActive(false);
        }

        if (playerWon)
        {
            ShowOnly(victoryPanel);
        }
        else
        {
            ShowOnly(gameOverPanel);
        }
    }

    private void OnShootButtonPressed()
    {
        if (!canShoot || GameManager.Instance == null || !GameManager.Instance.IsBossFightActive())
            return;

        if (bossFightManager != null)
        {
            bossFightManager.PlayerShoot();
        }

        canShoot = false;
        cooldownTimer = 0f;

        if (shootButton != null)
        {
            shootButton.interactable = false;
        }

        if (shootButtonFillImage != null)
        {
            shootButtonFillImage.fillAmount = 0f;
        }
    }

    public void SetShootCooldown(float newCooldown)
    {
        shootCooldown = Mathf.Max(0.1f, newCooldown);
    }
}
