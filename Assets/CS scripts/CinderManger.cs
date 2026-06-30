using UnityEngine;
using TMPro;

public class CinderManager : MonoBehaviour
{
    public static CinderManager instance;          // singleton: one global currency manager
    public int currentCinders = 0;                 // player's current cinder total
    private GameObject floatingTextPrefab;          // "+X" popup spawned on gain
    private Canvas canvas;                           // UI canvas the popup is parented to

    private void Awake()
    {
        // Singleton setup: keep the first instance, destroy any duplicates
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Load the floating text prefab from Resources and find the UI canvas
        floatingTextPrefab = Resources.Load<GameObject>("Prefabs/FloatingCinders");
        canvas = FindAnyObjectByType<Canvas>();
    }

    public void AddCinders(int amount)
    {
        // Add to the total and show the gain popup
        currentCinders += amount;
        Debug.Log("Cinders gained! Total: " + currentCinders);
        ShowFloatingText(amount);
    }

    private void ShowFloatingText(int amount)
    {
        // Skip if prefab or canvas is missing
        if (floatingTextPrefab == null || canvas == null)
            return;

        // Spawn the popup on the canvas, position it, and set its text
        GameObject floatingText = Instantiate(floatingTextPrefab, canvas.transform);
        floatingText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-130, -60);
        floatingText.GetComponentInChildren<TextMeshProUGUI>().text = "+" + amount;
    }
}