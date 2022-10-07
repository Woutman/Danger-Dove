using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBarrageController : MonoBehaviour
{
    List<GameObject> launcherList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        int numLaunchers = transform.childCount;
        for (int i = 0; i < numLaunchers; i++)
        {
            launcherList.Add(transform.GetChild(i).gameObject);
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
            launcher.GetComponent<LaserLauncherController>().ResetForPhase();
        }
    }

    // Fire laser from given launcher
    public void FireLaser(int launcherNum, float laserDuration)
    {
        StartCoroutine(launcherList[launcherNum].GetComponent<LaserLauncherController>().FireLaser(laserDuration));
    }
}
