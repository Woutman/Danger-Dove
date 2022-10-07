using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncherController : MonoBehaviour
{
    [SerializeField] private float fireDelay = 1.0f;

    private Vector3 curScaleIndicator;
    private GameObject fireIndicator;
    private GameObject launcherActive;
    private GameObject launcherInactive;
    private AudioSource fireAudio;
    private ParticleSystem fireParticle;

    // Start is called before the first frame update
    void Start()
    {
        fireIndicator = transform.GetChild(0).gameObject;
        curScaleIndicator = fireIndicator.transform.localScale;

        fireAudio = transform.Find("Audio Fire").GetComponent<AudioSource>();
        fireParticle = transform.Find("FX Fire Particle").GetComponent<ParticleSystem>();

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
        fireIndicator.SetActive(false);
        fireIndicator.transform.localScale = curScaleIndicator;
        launcherInactive.SetActive(true);
        launcherActive.SetActive(false);
    }

    // Activate fire indicator, wait, calculate damages, reset launcher and indicator
    public IEnumerator FireRocket()
    {
        fireIndicator.SetActive(true);
        launcherInactive.SetActive(false);
        launcherActive.SetActive(true);
        StartCoroutine(FireDelay());
        yield return new WaitForSeconds(fireDelay);

        fireIndicator.SetActive(false);
        fireIndicator.transform.localScale = curScaleIndicator;
        launcherActive.SetActive(false);
        launcherInactive.SetActive(true);       
        fireAudio.Play();
        fireParticle.Play();
        GameObject.Find("Player").GetComponent<PlayerController>().CalculateDamage();               
    }

    // Shrink fire indicator over fireDelay time
    private IEnumerator FireDelay()
    {
        Vector3 curScale = curScaleIndicator;
        Vector3 targetScale = new Vector3(0.1f, curScale.y, 0.1f);        

        float counter = 0f;
        while (counter < 1)
        {
            fireIndicator.transform.localScale = Vector3.Lerp(curScale, targetScale, counter);
            counter += Time.deltaTime / fireDelay;
            yield return new WaitForEndOfFrame();
        }        
    }
}
