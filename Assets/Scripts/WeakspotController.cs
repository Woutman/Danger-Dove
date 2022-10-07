using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakspotController : MonoBehaviour
{
    [SerializeField] private float weakDuration = 1.0f;

    public bool isWeak = false;
    public bool isHit = false;

    private AudioSource weakspotAudio;
    private GameObject eye;
    private GameObject eyeWeak;

    // Start is called before the first frame update
    void Start()
    {
        weakspotAudio = transform.Find("Weakspot Audio").GetComponent<AudioSource>();
        eye = transform.GetChild(1).gameObject;
        eyeWeak = transform.Find("Eye Weak").gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Change color, set weakness, wait, reset color and weakness
    public IEnumerator OpenWeakspot(float difficulty)
    {
        eyeWeak.SetActive(true);
        eye.SetActive(false);
        weakspotAudio.Play();
        isWeak = true;
        StartCoroutine(CheckForHit());
        yield return new WaitForSeconds(weakDuration * difficulty);

        eyeWeak.SetActive(false);
        eye.SetActive(true);
        isWeak = false;
        
    }

    // Open weakspot without timer and hit limit
    public void OpenWeakspotIndef()
    {
        eyeWeak.SetActive(true);
        eye.SetActive(false);
        isWeak = true;
    }

    // Close weakspot
    public void CloseWeakspot()
    {
        eyeWeak.SetActive(false);
        eye.SetActive(true);
        isWeak = false;
    }

    // Resets game object for phase transition
    public void ResetForPhase()
    {
        StopAllCoroutines();
        eyeWeak.SetActive(false);
        eye.SetActive(true);
        isWeak = false;
        isHit = false;
    }

    // If hit, close weakspot early
    private IEnumerator CheckForHit()
    {
        while (isWeak)
        {
            if (isHit)
            {
                eyeWeak.SetActive(false);
                eye.SetActive(true);
                isWeak = false;
                isHit = false;
                StopCoroutine("OpenWeakspot");                
                yield break;
            }
                        
            yield return new WaitForEndOfFrame();
        }
    }
}
