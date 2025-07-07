using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    // 조준선 변경 코드
    public GameObject[] crosshairs;
    private int currentIndex = 0;

    void Start()
    {
        UpdateCrosshairDisplay();
    }

    void Update()
    {
        // 순환하면서 순서대로 이미지 변경
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