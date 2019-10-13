using System;
using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText = null;
    public void UpdateTimer(TimeSpan time)
    {
        _timerText.text = time.ToString(@"mm\:ss");
    }
}
