using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PowerUpDisplayContainer : MonoBehaviour
{
    [Header("Fixed Location Slots")]
    [Tooltip("Drag your pre-created UI GameObjects/Transforms where icons should appear in order.")]
    [SerializeField] private List<Transform> iconLocationSlots = new List<Transform>();

    [Header("Optional Settings")]
    [Tooltip("Optional prefab containing an Image + TextMeshProUGUI for stack count.")]
    [SerializeField] private GameObject iconDisplayPrefab;

    private void OnEnable()
    {
        RefreshDisplay();
    }

    private void Start()
    {
        // Ensure it updates on start in case OnEnable fired before GameManager was ready
        StartCoroutine(DelayedRefresh());
    }

    private IEnumerator DelayedRefresh()
    {
        // Wait one frame to ensure GameManager singleton and data are fully settled in the new scene
        yield return null;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        Debug.Log("[PowerUpDisplayContainer] RefreshDisplay() called.");

        if (GameManager.Instance == null)
        {
            Debug.LogError("[PowerUpDisplayContainer] Refresh failed: GameManager.Instance is NULL!");
            return;
        }

        Debug.Log($"[PowerUpDisplayContainer] GameManager found. Total collected icons in history: {GameManager.Instance.collectedPowerUpIcons.Count}");

        // Safety check: if slots are empty, log a warning to help debug
        if (iconLocationSlots == null || iconLocationSlots.Count == 0)
        {
            Debug.LogWarning("[PowerUpDisplayContainer] No icon location slots assigned in the Inspector!");
            return;
        }

        // 1. Clear any existing icons spawned inside the location slots
        foreach (Transform slot in iconLocationSlots)
        {
            if (slot == null) continue;

            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }

        // 2. Count occurrences of each unique sprite icon
        Dictionary<Sprite, int> iconCounts = new Dictionary<Sprite, int>();

        foreach (Sprite iconSprite in GameManager.Instance.collectedPowerUpIcons)
        {
            if (iconSprite == null)
            {
                Debug.LogWarning("[PowerUpDisplayContainer] Found a NULL sprite inside GameManager's collectedPowerUpIcons list!");
                continue;
            }

            if (iconCounts.ContainsKey(iconSprite))
            {
                iconCounts[iconSprite]++;
            }
            else
            {
                iconCounts[iconSprite] = 1;
            }
        }

        Debug.Log($"[PowerUpDisplayContainer] Unique power-ups to display: {iconCounts.Count}");

        // 3. Populate slots at exact locations
        int slotIndex = 0;

        foreach (KeyValuePair<Sprite, int> pair in iconCounts)
        {
            if (slotIndex >= iconLocationSlots.Count)
            {
                Debug.LogWarning("[PowerUpDisplayContainer] More unique power-ups than available location slots! Increase your slots array size.");
                break;
            }

            Transform targetSlot = iconLocationSlots[slotIndex];
            if (targetSlot == null)
            {
                Debug.LogWarning($"[PowerUpDisplayContainer] Slot at index {slotIndex} is NULL!");
                slotIndex++;
                continue;
            }

            Sprite iconSprite = pair.Key;
            int count = pair.Value;

            GameObject newIconObj;

            if (iconDisplayPrefab != null)
            {
                newIconObj = Instantiate(iconDisplayPrefab, targetSlot);
                Debug.Log($"[PowerUpDisplayContainer] Instantiated icon prefab for sprite '{iconSprite.name}' (Count: {count}) into slot {slotIndex}");
            }
            else
            {
                newIconObj = CreateIconAtLocation(iconSprite, count, targetSlot);
                Debug.Log($"[PowerUpDisplayContainer] Procedurally created icon object for sprite '{iconSprite.name}' (Count: {count}) into slot {slotIndex}");
            }

            // Reset transform alignment
            RectTransform rect = newIconObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            // Set Sprite
            Image img = newIconObj.GetComponent<Image>();
            if (img == null) img = newIconObj.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.sprite = iconSprite;
                img.preserveAspect = true;
            }
            else
            {
                Debug.LogError($"[PowerUpDisplayContainer] Spawned icon object '{newIconObj.name}' has NO Image component!");
            }

            // Find all Text components inside the spawned object
            TMP_Text[] countTexts = newIconObj.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text countText in countTexts)
            {
                if (count > 1)
                {
                    countText.text = $"x{count}";
                    countText.gameObject.SetActive(true);
                }
                else
                {
                    // Forcefully clear and hide it if count is only 1!
                    countText.text = "";
                    countText.gameObject.SetActive(false);
                }
            }

            slotIndex++;
        }
    }

    private GameObject CreateIconAtLocation(Sprite sprite, int count, Transform parentLocation)
    {
        GameObject iconObj = new GameObject("PowerUpIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(parentLocation, false);

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        
        RectTransform parentRect = parentLocation.GetComponent<RectTransform>();
        if (parentRect != null)
        {
            iconRect.sizeDelta = parentRect.rect.size != Vector2.zero ? parentRect.rect.size : new Vector2(48f, 48f);
        }
        else
        {
            iconRect.sizeDelta = new Vector2(48f, 48f);
        }

        Image img = iconObj.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;

        if (count > 1)
        {
            GameObject textObj = new GameObject("CountText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(iconObj.transform, false);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(1f, 0f);
            textRect.anchorMax = new Vector2(1f, 0f);
            textRect.pivot = new Vector2(1f, 0f);
            textRect.anchoredPosition = new Vector2(4f, -2f);
            textRect.sizeDelta = new Vector2(40f, 25f);

            TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = $"x{count}";
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.yellow;
            tmp.alignment = TextAlignmentOptions.BottomRight;

            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = Color.black;
        }

        return iconObj;
    }
}