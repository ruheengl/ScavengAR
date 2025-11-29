using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameStage { Stage1_Clues, Stage2_Hunt, Stage3_Boss, GameOver }
    [SerializeField] private GameStage currentStage = GameStage.Stage1_Clues;

    [Header("Stage 1: Clues")]
    [SerializeField] private int totalClues = 3;
    private int cluesFound = 0;
    private readonly List<string> foundClueNames = new List<string>();

    [Header("Stage 2: Hunt")]
    [SerializeField] private int totalObjects = 5;
    [SerializeField] private float huntTimeLimit = 180f;
    private int objectsFound = 0;
    private float huntTimeRemaining;
    private bool isHuntActive = false;
    private bool huntSuccessful = false;
    private readonly List<string> foundObjectNames = new List<string>();
    [SerializeField] private GameObject[] stage2Targets;

    [Header("Stage 3: Boss")]
    [SerializeField] private int playerHealth = 5;
    [SerializeField] private int bossHealth = 5;
    [SerializeField] private float normalProjectileInterval = 3f;
    [SerializeField] private float normalProjectileSpeed = 1f;
    private int currentPlayerHealth;
    private int currentBossHealth;
    private bool isBossFightActive = false;
    [SerializeField] private GameObject bossTarget;

    [Header("UI References (HUD)")]
    [SerializeField] private GameObject stage1UI;
    [SerializeField] private GameObject stage2UI;
    [SerializeField] private GameObject stage3UI;
    [SerializeField] private TextMeshProUGUI clueCountText;
    [SerializeField] private TextMeshProUGUI objectCountText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button startBossButton;
    [SerializeField] private Button shootButton;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI bossHealthText;
    [SerializeField] private GameObject clueListPanel;

    [Header("Audio")]
    [SerializeField] private AudioClip stageCompleteSound;
    [SerializeField] private AudioClip countdownSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;

    [SerializeField] private AudioClip foundSound;

    private AudioSource audioSource;
    private Coroutine messageRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        InitializeGame();
    }

    private void Update()
    {
        if (isHuntActive && currentStage == GameStage.Stage2_Hunt)
        {
            huntTimeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (huntTimeRemaining <= 0f)
            {
                HuntTimedOut();
            }
        }
    }

    private void InitializeGame()
    {
        SetTargetsActive(stage2Targets, false);
        if (bossTarget != null)
            bossTarget.SetActive(false);

        currentStage = GameStage.Stage1_Clues;
        cluesFound = 0;
        objectsFound = 0;
        huntSuccessful = false;
        foundClueNames.Clear();
        foundObjectNames.Clear();

        if (stage1UI != null) stage1UI.SetActive(false);
        if (stage2UI != null) stage2UI.SetActive(false);
        if (stage3UI != null) stage3UI.SetActive(false);

        if (startBossButton != null)
            startBossButton.gameObject.SetActive(false);
    }

    public void StartStage1FromUI()
    {
        currentStage = GameStage.Stage1_Clues;
        cluesFound = 0;
        foundClueNames.Clear();

        ShowStage1UI();
        UpdateClueUI();

    }

    public void StartStage2FromUI()
    {
        StartStage2();
    }

    public void StartBossFightFromUI(bool isHardMode)
    {
        StartBossFight();
    }

    // Stage 1
    public void ClueFound(string clueName)
    {
        if (currentStage != GameStage.Stage1_Clues) return;
        if (foundClueNames.Contains(clueName)) return;

        cluesFound++;
        PlaySound(foundSound);
        foundClueNames.Add(clueName);
        UpdateClueUI();

        if (cluesFound >= totalClues)
        {
            StartCoroutine(CompleteStage1());
        }
    }

    private IEnumerator CompleteStage1()
    {
        PlaySound(stageCompleteSound);
        yield return new WaitForSeconds(3f);

        if (stage1UI != null) stage1UI.SetActive(false);

        if (UIFlowController.Instance != null)
        {
            UIFlowController.Instance.ShowStage1End();
        }
    }

    // Stage 2
    private void StartStage2()
    {
        SetTargetsActive(stage2Targets, true);
        if (bossTarget != null)
            bossTarget.SetActive(false);

        currentStage = GameStage.Stage2_Hunt;

        ShowStage2UI();
        huntTimeRemaining = huntTimeLimit;
        isHuntActive = true;
        objectsFound = 0;
        foundObjectNames.Clear();

        UpdateObjectUI();
        UpdateTimerUI();
    }

    public void ObjectFound(int objectIndex, string objectName)
    {
        if (currentStage != GameStage.Stage2_Hunt) return;
        if (!isHuntActive) return;
        if (foundObjectNames.Contains(objectName)) return;

        objectsFound++;
        PlaySound(foundSound);
        foundObjectNames.Add(objectName);
        UpdateObjectUI();

        if (objectsFound >= totalObjects)
        {
            HuntCompleted(true);
        }
    }

    private void HuntTimedOut()
    {
        isHuntActive = false;
        HuntCompleted(false);
    }

    private void HuntCompleted(bool success)
    {
        isHuntActive = false;
        huntSuccessful = success;
        PlaySound(stageCompleteSound);

        if (stage2UI != null) stage2UI.SetActive(false);

        if (UIFlowController.Instance != null)
        {
            UIFlowController.Instance.ShowStage2End(success);
        }

        if (startBossButton != null)
        {
            startBossButton.gameObject.SetActive(false);
        }
    }

    // Stage 3
    public void StartBossFight()
    {
        SetTargetsActive(stage2Targets, false);
        if (bossTarget != null)
            bossTarget.SetActive(true);

        currentStage = GameStage.Stage3_Boss;
        ShowStage3UI();

        currentPlayerHealth = playerHealth;
        currentBossHealth = bossHealth;

        if (!huntSuccessful)
        {
            currentBossHealth = bossHealth * 2;
        }

        UpdateHealthUI();
        isBossFightActive = true;

        BossFightManager boss = FindAnyObjectByType<BossFightManager>();
        if (boss != null)
        {
            boss.TryStartBossFightIfTracked();
        }

        if (startBossButton != null)
            startBossButton.gameObject.SetActive(false);

        if (UIFlowController.Instance != null)
        {
            UIFlowController.Instance.SetShootCooldown(GetProjectileInterval());
        }

    }

    public void PlayerHit()
    {
        if (!isBossFightActive) return;

        currentPlayerHealth--;
        UpdateHealthUI();

        if (currentPlayerHealth <= 0)
        {
            GameOver(false);
        }
    }

    public void BossHit()
    {
        if (!isBossFightActive) return;

        currentBossHealth--;
        UpdateHealthUI();

        if (currentBossHealth <= 0)
        {
            GameOver(true);
        }
    }

    private void GameOver(bool victory)
    {
        isBossFightActive = false;
        currentStage = GameStage.GameOver;

        if (victory)
        {
            PlaySound(victorySound);
        }
        else
        {
            PlaySound(defeatSound);
        }

        if (UIFlowController.Instance != null)
        {
            UIFlowController.Instance.ShowGameOver(victory);
        }
    }

    private void ShowStage1UI()
    {
        if (stage1UI != null) stage1UI.SetActive(true);
        if (stage2UI != null) stage2UI.SetActive(false);
        if (stage3UI != null) stage3UI.SetActive(false);
    }

    private void ShowStage2UI()
    {
        if (stage1UI != null) stage1UI.SetActive(false);
        if (stage2UI != null) stage2UI.SetActive(true);
        if (stage3UI != null) stage3UI.SetActive(false);
    }

    private void ShowStage3UI()
    {
        if (stage1UI != null) stage1UI.SetActive(false);
        if (stage2UI != null) stage2UI.SetActive(false);
        if (stage3UI != null) stage3UI.SetActive(true);
    }

    private void UpdateClueUI()
    {
        if (clueCountText != null)
        {
            clueCountText.text = $"Trophies Recovered: {cluesFound}/{totalClues}";
        }
    }

    private void UpdateObjectUI()
    {
        if (objectCountText != null)
        {
            objectCountText.text = $"Gear Found: {objectsFound}/{totalObjects}";
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(huntTimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(huntTimeRemaining % 60f);
            timerText.text = $"Time Left: {minutes:00}:{seconds:00}";

            if (huntTimeRemaining < 30f)
            {
                timerText.color = Color.red;
            }
            if (huntTimeRemaining == 10f)
            {
                PlaySound(countdownSound);
            }

        }
    }

    private void UpdateHealthUI()
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"Player: {currentPlayerHealth}";
        }

        if (bossHealthText != null)
        {
            bossHealthText.text = $"Boss: {currentBossHealth}";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SetTargetsActive(GameObject[] targets, bool active)
    {
        if (targets == null) return;

        foreach (var go in targets)
        {
            if (go != null)
                go.SetActive(active);
        }
    }

    public void ShowCluePanels()
    {
        bool isActive = clueListPanel.activeSelf;
        clueListPanel.SetActive(!isActive);
    }

    public void ResetGameButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsHuntSuccessful() => huntSuccessful;
    public bool IsBossFightActive() => isBossFightActive;
    public float GetProjectileInterval() => huntSuccessful ? normalProjectileInterval : normalProjectileInterval / 2f;
    public float GetProjectileSpeed() => huntSuccessful ? normalProjectileSpeed : normalProjectileSpeed * 2f;
}
