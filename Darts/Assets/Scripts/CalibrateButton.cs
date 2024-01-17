using UnityEngine;
using Vuforia;

public class CalibrateButton : MonoBehaviour {
    public DartboardManager dartboardManager;
    public VectorCalculation vectorCalculation;
    private bool isCalibrated = false;

    void Start() {
        GetComponent<VirtualButtonBehaviour>().RegisterOnButtonPressed(OnButtonPressed);
    }

    public void OnButtonPressed(VirtualButtonBehaviour vb) {
        if (!isCalibrated) {
            isCalibrated = true;
            vectorCalculation.Calibrate();
            dartboardManager.StartGame();
        }
    }
}