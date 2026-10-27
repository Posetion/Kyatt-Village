using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BuffData : MonoBehaviour
{
    [Header("Buff Info")]
    public string buffName;
    [TextArea] public string buffDescription;
    public Sprite buffIcon;

    [Header("UI References Inside Prefab")]
    public TMP_Text nameText;
    public TMP_Text descText;
    public Image iconImage;
    
    // Optional: Only used if displayed in a context that requires a button click
    public Button selectButton; 

    public void InitializeUI()
    {
        if (nameText != null) nameText.text = buffName;
        if (descText != null) descText.text = buffDescription;
        if (iconImage != null && buffIcon != null) iconImage.sprite = buffIcon;
    }

    // Virtual method so specific buff types (like attack boost, speed boost) can override what they do
    public virtual void ApplyBuff(GameObject player)
    {
        Debug.Log($"Applied buff: {buffName} to player!");
        // Add your actual stat modification logic here or via events
    }
}