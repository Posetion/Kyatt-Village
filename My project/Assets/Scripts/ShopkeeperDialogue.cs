using UnityEngine;
using TMPro;

public class ShopkeeperDialogue : MonoBehaviour
{
    [Header("3D Text References")]
    [Tooltip("Drag the 3D World Space TextMeshPro component that sits above the shopkeeper here.")]
    [SerializeField] private TextMeshPro dialogueText;

    [Header("Dialogue Timing")]
    [Tooltip("How many seconds between dialogue changes.")]
    [SerializeField] private float switchInterval = 5f;

    [Header("Cryptic Blacksmith Dialogues")]
    [Tooltip("Subtle lines where the Blacksmith hints at the curse without revealing you are dead.")]
    [SerializeField] private string[] idleGreetings = new string[]
    {
        "*CLANG!* Back again, wanderer? The forge remembers your footsteps...",
        "The mist in this Kyat village is thick... it makes men forget who they are.",
        "The spirits outside fight without rest. Do not let them consume you.",
        "*CLANG!* Choose one power-up carefully. Nightfall is approaching.",
        "This iron body was forged by the Sorcerer... but my hammer strikes for you.",
        "You carry a heavy shadow, traveler. Pick a weapon for the road.",
        "*CLANG!* Blood spills, the sun sets, and yet we stand here once more...",
        "Gold buys power in these woods. Make your choice before the darkness rolls in."
    };

    [Header("Action Dialogues")]
    [Tooltip("What the Golem says when you buy your power-up.")]
    [SerializeField] private string[] purchaseReactions = new string[]
    {
        "*CLANG!* Power forged! May it grant you another chance.",
        "A fine choice, wanderer. Push forward into the mist.",
        "The forge rests. Go, before the villagers awaken again!",
        "*CLANG!* Take it and carve your way out of this nightmare."
    };

    private float timer = 0f;

    private void OnEnable()
    {
        // Say a random line right away when the shop opens
        SayRandomGreeting();
        timer = 0f;
    }

    private void Update()
    {
        // Uses unscaled delta time so it switches dialogue every 5s 
        // even if time is frozen (Time.timeScale = 0) while shopping
        timer += Time.unscaledDeltaTime;

        if (timer >= switchInterval)
        {
            timer = 0f;
            SayRandomGreeting();
        }
    }

    /// <summary>
    /// Picks a random cryptic greeting and displays it.
    /// </summary>
    public void SayRandomGreeting()
    {
        if (idleGreetings == null || idleGreetings.Length == 0) return;

        int randomIndex = Random.Range(0, idleGreetings.Length);
        SetDialogue(idleGreetings[randomIndex]);
    }

    /// <summary>
    /// Call this from your ShopManager when the player purchases a power-up!
    /// </summary>
    public void SayPurchaseReaction()
    {
        if (purchaseReactions != null && purchaseReactions.Length > 0)
        {
            int randomIndex = Random.Range(0, purchaseReactions.Length);
            SetDialogue(purchaseReactions[randomIndex]);
        }

        // Reset timer so the purchase reaction stays visible for 5 seconds before returning to idle lines
        timer = 0f;
    }

    private void SetDialogue(string message)
    {
        if (dialogueText != null)
        {
            dialogueText.text = message;
        }
        else
        {
            Debug.LogWarning("[ShopkeeperDialogue] 3D TextMeshPro reference is missing!");
        }
    }
}