using UnityEngine;
using TMPro;

public class CindersUI : MonoBehaviour
{
    public TextMeshProUGUI cindersAmount;       // the on-screen text showing the cinder count
    private CinderManager cinderManager;        // reference to the currency manager

    private void Start()
    {
        // Find the cinder manager in the scene
        cinderManager = FindAnyObjectByType<CinderManager>();
    }

    private void Update()
    {
        // Every frame, sync the displayed text with the current cinder total
        if (cinderManager != null && cindersAmount != null)
        {
            cindersAmount.text = cinderManager.currentCinders.ToString();
        }
    }
}