using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBarrageController : MonoBehaviour
{
    private int numLaunchers = 3;

    private List<GameObject> launcherList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numLaunchers; i++)
        {
            launcherList.Add(transform.GetChild(0).GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Resets game object for phase transition
    public void ResetForPhase()
    {
        StopAllCoroutines();
        foreach (GameObject launcher in launcherList)
        {
            launcher.GetComponent<RocketLauncherController>().ResetForPhase();
        }
    }

    // Fire rocket from given launcher
    public void FireRocket(int launcherNum)
    {
        RocketLauncherController controller = launcherList[launcherNum].GetComponent<RocketLauncherController>();
        StartCoroutine(controller.FireRocket());
    }
}
