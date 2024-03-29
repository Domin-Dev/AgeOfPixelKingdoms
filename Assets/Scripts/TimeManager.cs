using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    Button button;
    TextMeshProUGUI text;
    float[] speeds = { 1, 2, 4};
    int currentSpeed;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => { SetSpeed(); Sounds.instance.PlaySound(5); });
        GameObject.FindWithTag("Pause").GetComponent<Button>().onClick.AddListener(() => { Pause(); });
        currentSpeed = 0;
        text = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void SetSpeed()
    {
        if(Time.timeScale == 0) GameAssets.Instance.pause.gameObject.SetActive(false);


        if (currentSpeed + 1 < speeds.Length)
        {
            currentSpeed++;
        }
        else
        {
            currentSpeed = 0;
        }
        Time.timeScale = speeds[currentSpeed];
        text.text = speeds[currentSpeed].ToString() + "X";
    }

    private void Pause()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = speeds[currentSpeed];
            GameAssets.Instance.pause.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 0;
            GameAssets.Instance.pause.gameObject.SetActive(true);
        }
    }
    private void OnDestroy()
    {
        Time.timeScale = 1;

    }
}