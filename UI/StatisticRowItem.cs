using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatisticRowItem : MonoBehaviour
{
    // The statistic to display
    public Preferences.Statistic statistic;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = Preferences.StatisticValue(statistic);
    }
}
