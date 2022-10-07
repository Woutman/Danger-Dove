using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Blink : MonoBehaviour
{
    private float nextPeriod = 1.0f;
    private float period = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextPeriod)
        {
            nextPeriod = Time.time + period;
            if (GetComponent<TextMeshProUGUI>().enabled)
            {
                GetComponent<TextMeshProUGUI>().enabled = false;
                return;
            }

            if (!GetComponent<TextMeshProUGUI>().enabled)
            {
                GetComponent<TextMeshProUGUI>().enabled = true;
                return;
            }
        }
    }
}
