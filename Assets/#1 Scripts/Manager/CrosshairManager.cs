using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public GameObject[] crosshairs;
    private int currentIndex = 0;

    void Start()
    {
        UpdateCrosshairDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            currentIndex = (currentIndex + 1) % crosshairs.Length;
            UpdateCrosshairDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            currentIndex = (currentIndex - 1 + crosshairs.Length) % crosshairs.Length;
            UpdateCrosshairDisplay();
        }
    }

    void UpdateCrosshairDisplay()
    {
        for (int i = 0; i < crosshairs.Length; i++)
        {
            crosshairs[i].SetActive(i == currentIndex);
        }
    }
}