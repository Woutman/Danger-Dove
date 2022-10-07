using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserLauncherController : MonoBehaviour
{
    [SerializeField] private float laserFireDelay = 1.0f;
    
    private GameObject laser;
    private GameObject launcherActive;
    private GameObject launcherInactive;
    private AudioSource laserAudio;

    // Start is called before the first frame update
    void Start()
    {
        laser = transform.GetChild(0).gameObject;
        laserAudio = GameObject.Find("Boss").transform.Find("Laser Audio").GetComponent<AudioSource>();
        launcherActive = transform.Find("Launcher Active").gameObject;
        launcherInactive = transform.Find("Launcher Inactive").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Resets game object for phase transition
    public void ResetForPhase()
    {
        StopAllCoroutines();
        laser.SetActive(false);
        launcherInactive.SetActive(true);
        launcherActive.SetActive(false);        
    }

    // Change color, wait, fire laser, wait, reset laser and color
    public IEnumerator FireLaser(float laserDuration)
    {
        launcherInactive.SetActive(false);
        launcherActive.SetActive(true);
        yield return new WaitForSeconds(laserFireDelay);

        laser.SetActive(true);
        laserAudio.Play();
        yield return new WaitForSeconds(laserDuration);

        laser.SetActive(false);
        launcherInactive.SetActive(true);
        launcherActive.SetActive(false);
    }
}
