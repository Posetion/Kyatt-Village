using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleUI : MonoBehaviour
{
    public Button playButton;
    public Button settingsButton;
    
    [Header("Buttons")]
    [SerializeField] private Button closeSettingsButton; // Added SerializeField for the close button
    [SerializeField] private Button resetDataButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject permanentUpgradesPanel;

    [Header("Permanent Upgrades")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button closeUpgradesButton;
    [SerializeField] private Button movementSpeedUpgradeButton;
    [SerializeField] private Button attackDamageUpgradeButton;
    [SerializeField] private Button maximumHealthUpgradeButton;
    [SerializeField] private Button defenseUpgradeButton;
    [SerializeField] private TMP_Text permanentUpgradePointsText;
    [SerializeField] private TMP_Text permanentUpgradeLevelsText;

    void Start()
    {
        FindSettingsReferences();
        FindPermanentUpgradeReferences();
        ShowMainPanel();

        if (playButton != null && playButton.onClick.GetPersistentEventCount() == 0)
        {
            playButton.onClick.AddListener(OnPlayButton);
        }

        AddButtonListener(settingsButton, ShowSettings);
        AddButtonListener(closeSettingsButton, BackToMain);
        AddButtonListener(resetDataButton, ResetRunData);

        AddButtonListener(continueButton, ContinueToGame);
        AddButtonListener(closeUpgradesButton, BackToMain);
        AddButtonListener(movementSpeedUpgradeButton, () => TryBuyUpgrade(PermanentUpgradeType.MovementSpeed));
        AddButtonListener(attackDamageUpgradeButton, () => TryBuyUpgrade(PermanentUpgradeType.AttackDamage));
        AddButtonListener(maximumHealthUpgradeButton, () => TryBuyUpgrade(PermanentUpgradeType.MaximumHealth));
        AddButtonListener(defenseUpgradeButton, () => TryBuyUpgrade(PermanentUpgradeType.Defense));
        RefreshPermanentUpgradePanel();
    }

    public void OnPlayButton()
    {
        ShowPermanentUpgradesPanel();
    }

    public void ShowPermanentUpgradesPanel()
    {
        if (permanentUpgradesPanel == null)
        {
            Debug.LogError("[TitleUI] Permanent upgrade panel is missing from the Title canvas.");
            return;
        }

        SetMainMenuButtonsVisible(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        permanentUpgradesPanel.SetActive(true);
        RefreshPermanentUpgradePanel();
    }

    public void ContinueToGame()
    {
        if (UiManager.instance != null)
        {
            UiManager.instance.OnContinueButton();
            return;
        }

        Time.timeScale = 1f;
        if (SceneController.Instance != null)
        {
            SceneController.Instance.StartNewRun();
        }
        else
        {
            Debug.LogError("[TitleUI] Cannot start the game because SceneController.Instance is missing.");
        }
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(true);
            SetMainMenuButtonsVisible(true);
        }
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (permanentUpgradesPanel != null) permanentUpgradesPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("[TitleUI] Settings panel is missing from the Title canvas.");
            return;
        }

        SetMainMenuButtonsVisible(false);
        settingsPanel.SetActive(true);
        if (permanentUpgradesPanel != null) permanentUpgradesPanel.SetActive(false);
    }

    public void ResetRunData()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetAllProgress();
        }

        if (SceneController.Instance != null)
        {
            SceneController.Instance.ResetRooms();
        }

        BackToMain();
    }

    public void BackToMain()
    {
        ShowMainPanel();
    }

    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void FindPermanentUpgradeReferences()
    {
        Transform panel = transform.Find("Panel/PowerupPanel");
        if (panel == null) return;

        permanentUpgradesPanel = panel.gameObject;
        Transform content = panel.Find("Panel (1)");
        if (content == null) return;

        continueButton = FindButton(content, "Button 1", continueButton);
        closeUpgradesButton = FindButton(content, "Close", closeUpgradesButton);
        movementSpeedUpgradeButton = FindButton(content, "MovementSpeedButton", movementSpeedUpgradeButton);
        attackDamageUpgradeButton = FindButton(content, "AttackDamageButton", attackDamageUpgradeButton);
        maximumHealthUpgradeButton = FindButton(content, "MaximumHealthButton", maximumHealthUpgradeButton);
        defenseUpgradeButton = FindButton(content, "DefenseButton", defenseUpgradeButton);

        Transform points = content.Find("UpgradePointsText");
        if (points != null) permanentUpgradePointsText = points.GetComponent<TMP_Text>();

        Transform levels = content.Find("UpgradeLevelsText");
        if (levels != null) permanentUpgradeLevelsText = levels.GetComponent<TMP_Text>();
    }

    private void FindSettingsReferences()
    {
        if (playButton == null)
        {
            Transform start = transform.Find("Panel/start");
            if (start != null) playButton = start.GetComponent<Button>();
        }

        if (settingsButton == null)
        {
            Transform button = transform.Find("Panel/Setting");
            if (button != null) settingsButton = button.GetComponent<Button>();
        }

        if (settingsPanel == null)
        {
            Transform panel = transform.Find("Setting");
            if (panel != null) settingsPanel = panel.gameObject;
        }

        if (settingsPanel == null)
        {
            Debug.LogError("[TitleUI] Settings panel is missing from the Title canvas.");
            return;
        }

        Transform content = settingsPanel.transform.Find("Panel");
        if (content == null)
        {
            Debug.LogError("[TitleUI] Settings panel content is missing from the Title canvas.");
            return;
        }

        closeSettingsButton = FindButton(content, "Close", closeSettingsButton);
        resetDataButton = FindButton(content, "Reset data", resetDataButton);
    }

    private Button FindButton(Transform parent, string childName, Button current)
    {
        if (current != null) return current;
        Transform child = parent.Find(childName);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private void AddButtonListener(Button button, UnityEngine.Events.UnityAction listener)
    {
        if (button == null) return;

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentListenerState(i) != UnityEngine.Events.UnityEventCallState.Off &&
                button.onClick.GetPersistentTarget(i) == listener.Target &&
                button.onClick.GetPersistentMethodName(i) == listener.Method.Name)
            {
                return;
            }
        }

        button.onClick.AddListener(listener);
    }

    private void SetMainMenuButtonsVisible(bool visible)
    {
        if (playButton != null) playButton.gameObject.SetActive(visible);
        if (settingsButton != null) settingsButton.gameObject.SetActive(visible);
        Transform exitButton = transform.Find("Panel/Exit");
        if (exitButton != null) exitButton.gameObject.SetActive(visible);
    }

    private void TryBuyUpgrade(PermanentUpgradeType upgrade)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[TitleUI] Cannot purchase an upgrade because GameManager.Instance is missing.");
            return;
        }

        GameManager.Instance.TryPurchasePermanentUpgrade(upgrade);
        RefreshPermanentUpgradePanel();
    }

    private void RefreshPermanentUpgradePanel()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null) return;

        if (permanentUpgradePointsText != null)
        {
            permanentUpgradePointsText.text = $"Upgrade Points: {manager.PermanentUpgradePoints}";
        }

        if (permanentUpgradeLevelsText != null)
        {
            permanentUpgradeLevelsText.text =
                "Permanent upgrades apply at the start of each run.\n" +
                "Earn 1 point each time you clear and leave a map.";
        }

        UpdateUpgradeButton(movementSpeedUpgradeButton, manager, PermanentUpgradeType.MovementSpeed, "Speed", "+5% speed");
        UpdateUpgradeButton(attackDamageUpgradeButton, manager, PermanentUpgradeType.AttackDamage, "Damage", "+10% damage");
        UpdateUpgradeButton(maximumHealthUpgradeButton, manager, PermanentUpgradeType.MaximumHealth, "Health", "+10% max health");
        UpdateUpgradeButton(defenseUpgradeButton, manager, PermanentUpgradeType.Defense, "Defense", "+1 defense");
    }

    private void UpdateUpgradeButton(
        Button button,
        GameManager manager,
        PermanentUpgradeType upgrade,
        string displayName,
        string bonus)
    {
        if (button == null) return;

        int level = manager.GetPermanentUpgradeLevel(upgrade);
        int cost = manager.GetPermanentUpgradeCost(upgrade);
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = $"{displayName} {level}/10\n{bonus} | Cost: {cost}";
        }

        button.interactable = manager.CanPurchasePermanentUpgrade(upgrade);
    }

#if UNITY_EDITOR
    public void SetupTitleSceneButtonsForEditor()
    {
        FindSettingsReferences();
        if (settingsButton == null || settingsPanel == null || closeSettingsButton == null || resetDataButton == null)
        {
            Debug.LogError("[TitleUI] Could not find all required Title settings controls.");
            return;
        }

        SetPersistentListener(settingsButton, ShowSettings);
        SetPersistentListener(resetDataButton, ResetRunData);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);
    }

    private void SetPersistentListener(Button button, UnityEngine.Events.UnityAction listener)
    {
        UnityEditor.Undo.RecordObject(button, "Wire Title Button Callback");
        int listenerCount = button.onClick.GetPersistentEventCount();
        if (listenerCount == 0)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, listener);
        }
        else
        {
            UnityEditor.Events.UnityEventTools.RegisterPersistentListener(button.onClick, 0, listener);
            for (int i = button.onClick.GetPersistentEventCount() - 1; i > 0; i--)
            {
                UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);
            }
        }

        button.onClick.SetPersistentListenerState(0, UnityEngine.Events.UnityEventCallState.RuntimeOnly);
        UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(button);
    }

    private static void RemovePersistentListeners(Button button)
    {
        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);
        }
    }

    public void SetupPermanentUpgradePanelForEditor()
    {
        Transform content = transform.Find("Panel/PowerupPanel/Panel (1)");
        if (content == null)
        {
            Debug.LogError("[TitleUI] Could not find the existing permanent-upgrade panel content.");
            return;
        }

        Button template = content.Find("Button 1")?.GetComponent<Button>();
        if (template == null)
        {
            Debug.LogError("[TitleUI] Could not find the existing Continue button to use as a style template.");
            return;
        }

        TMP_Text templateText = template.GetComponentInChildren<TMP_Text>(true);
        if (templateText == null)
        {
            Debug.LogError("[TitleUI] The Continue button has no TextMeshPro label to use as a style template.");
            return;
        }

        CreateUpgradePanelText(content, templateText, "UpgradeHeader", "PERMANENT UPGRADES", 340f, 64f, 42f);
        CreateUpgradePanelText(content, templateText, "UpgradePointsText", "Upgrade Points: 0", 260f, 54f, 32f);
        CreateUpgradePanelText(
            content,
            templateText,
            "UpgradeLevelsText",
            "Upgrade costs rise by 1 point per level.\nEarn 1 point for each map you clear.",
            180f,
            90f,
            25f);

        CreateUpgradePanelButton(content, template, "MovementSpeedButton", new Vector2(-190f, 55f));
        CreateUpgradePanelButton(content, template, "AttackDamageButton", new Vector2(190f, 55f));
        CreateUpgradePanelButton(content, template, "MaximumHealthButton", new Vector2(-190f, -90f));
        CreateUpgradePanelButton(content, template, "DefenseButton", new Vector2(190f, -90f));

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gameObject.scene);
        FindPermanentUpgradeReferences();
        RefreshPermanentUpgradePanel();
    }

    private void CreateUpgradePanelText(
        Transform parent,
        TMP_Text template,
        string objectName,
        string initialText,
        float yPosition,
        float height,
        float fontSize)
    {
        Transform existing = parent.Find(objectName);
        GameObject textObject = existing != null
            ? existing.gameObject
            : new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMPro.TextMeshProUGUI text = textObject.GetComponent<TMPro.TextMeshProUGUI>();
        text.font = template.font;
        text.fontSharedMaterial = template.fontSharedMaterial;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.raycastTarget = false;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, yPosition);
        rect.sizeDelta = new Vector2(760f, height);
        text.text = initialText;
    }

    private void CreateUpgradePanelButton(
        Transform parent,
        Button template,
        string objectName,
        Vector2 position)
    {
        Transform existing = parent.Find(objectName);
        GameObject buttonObject = existing != null
            ? existing.gameObject
            : Instantiate(template.gameObject, parent, false);
        buttonObject.name = objectName;

        TMP_Text label = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            if (objectName == "MovementSpeedButton") label.text = "Speed 0/10\n+5% speed";
            else if (objectName == "AttackDamageButton") label.text = "Damage 0/10\n+10% damage";
            else if (objectName == "MaximumHealthButton") label.text = "Health 0/10\n+10% max health";
            else label.text = "Defense 0/10\n+1 defense";
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(350f, 110f);

        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            RemovePersistentListeners(button);
            button.onClick.RemoveAllListeners();
            button.interactable = false;
        }
    }
#endif
}