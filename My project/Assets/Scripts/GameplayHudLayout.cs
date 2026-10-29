using UnityEngine;

[ExecuteAlways]
public class GameplayHudLayout : MonoBehaviour
{
    [Header("Top-left positions (Canvas units)")]
    [SerializeField] private Vector2 settingButtonPosition = new Vector2(46f, 46f);
    [SerializeField] private Vector2 heartsPosition = new Vector2(210f, 46f);
    [SerializeField] private Vector2 coinPosition = new Vector2(210f, 128f);

    [Header("Optional sizes (0 keeps the current size)")]
    [SerializeField] private Vector2 settingButtonSize = new Vector2(150f, 150f);
    [SerializeField] private Vector2 heartsSize = new Vector2(650f, 70f);
    [SerializeField] private Vector2 coinSize = Vector2.zero;

    [SerializeField] private RectTransform settingButton;
    [SerializeField] private RectTransform hearts;
    [SerializeField] private RectTransform coinContainer;

    private void OnEnable()
    {
        ApplyLayout();
    }

    private void OnValidate()
    {
        ApplyLayout();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
            ApplyLayout();
    }

    public void ApplyLayout()
    {
        if (settingButton == null)
            settingButton = FindRect("settingButton");
        if (hearts == null)
            hearts = FindRect("Hearts");
        if (coinContainer == null)
            coinContainer = FindRect("Coinpart");

        Place(settingButton, settingButtonPosition, settingButtonSize);
        Place(hearts, heartsPosition, heartsSize);
        Place(coinContainer, coinPosition, coinSize);
    }

    private RectTransform FindRect(string objectName)
    {
        foreach (RectTransform rect in GetComponentsInChildren<RectTransform>(true))
        {
            if (rect.name == objectName)
                return rect;
        }
        return null;
    }

    private static void Place(RectTransform rect, Vector2 positionFromTopLeft, Vector2 size)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(positionFromTopLeft.x, -positionFromTopLeft.y);

        if (size.x > 0f)
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        if (size.y > 0f)
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }
}
