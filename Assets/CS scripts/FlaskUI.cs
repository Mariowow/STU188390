using UnityEngine;
using TMPro;

public class FlaskUI : MonoBehaviour
{
    public TextMeshProUGUI flaskCountText;   // text showing "current / max" charges
    public TextMeshProUGUI flaskLabelText;   // optional label (e.g. "Flasks")
    private Flask flask;                       // reference to the flask manager

    private void Start()
    {
        // Grab the global flask singleton
        flask = Flask.instance;
    }

    private void Update()
    {
        // Each frame, update the display to match the current charge count
        if (flask != null && flaskCountText != null)
        {
            flaskCountText.text = flask.currentFlasks + " / " + flask.maxFlasks;
        }
    }
}