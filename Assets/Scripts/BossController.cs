using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private float bossStunTime = 3.0f;
    [SerializeField] private float bossStunTimeTwo = 2.5f;
    [SerializeField] private float bossStunTimeThree = 2.0f;
    [SerializeField] private float startHealth = 100.0f;
    [SerializeField] private float phaseTwoFraction = 0.7f;
    [SerializeField] private float phaseThreeFraction = 0.5f;
    [SerializeField] private float laserDurTotal = 1.0f;
    [SerializeField] private AnimationCurve stunCurve;
    [SerializeField] private AnimationCurve hitCurve;
    [SerializeField] private GameObject bars;
    [SerializeField] private GameObject youWinText;

    private float bounceSpeed = 0.0005f;
    private float introSpeed;
    private float rocketDurTotal = 1.0f;
    private float weakspotDurTotal = 1.0f;
    private float difficulty = 1.0f;
    private int phase = -1;
    private int numExplosions;

    private bool phaseTransition = false;
    private bool phaseActive = false;
    private bool isStunned = false;
    private bool isHit = false;    
    private bool hasStarted = false;

    public float health;
    public bool gameIsRunning = false;
    public bool toBeStunned = false;
    public bool isAlive = true;

    private Vector3 introPos = new Vector3(0, 12, 0);
    private Vector3 startPos = new Vector3(0, 0.3f, 0);

    private List<GameObject> weakspots = new List<GameObject>();
    private List<GameObject> rocketBarrages = new List<GameObject>();
    private GameObject laserBarrage;
    private AudioSource phaseOutAudio;
    private AudioSource phaseInAudio;
    private AudioSource stunAudio;
    private AudioSource introAudio;
    private Transform explosionGroup;
    private PlayerController playerController;
    List<ParticleSystem> explosionParticles = new List<ParticleSystem>();
    List<AudioSource> explosionAudioSources = new List<AudioSource>();

    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        InitiateTools();
        InitiateFXandAudio();
        health = startHealth;        
        introSpeed = (introPos.y - startPos.y) / (introAudio.clip.length - 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        // Intro sequence
        if (phase == -1 && gameIsRunning)
        {
            if (!hasStarted)
            {
                transform.position = introPos;
                hasStarted = true;
            }

            transform.Translate(Vector3.down * Time.deltaTime * introSpeed);

            if (!introAudio.isPlaying)
            {
                introAudio.Play();
            }

            if (transform.position.y < startPos.y)
            {
                transform.position = startPos;
                phase++;
            }
        }

        // Passive bobbing up and down
        if (phaseActive && !isHit)
        {
            transform.Translate(Vector3.up * Mathf.Sin(Time.time) * bounceSpeed);
        }
        
        // Start of boss routines
        if (phase == 0 && !phaseActive && !phaseTransition && !isStunned)
        {
            bars.SetActive(true);
            StartCoroutine(PhaseRoutine0());
            phaseActive = true;
        }

        if (phase == 1 && !phaseActive && !phaseTransition && !isStunned)
        {
            StartCoroutine(PhaseRoutine1());
            phaseActive = true;
        }

        if ((health < startHealth * phaseTwoFraction) && !phaseTransition &&
            (phase == 0 || phase == 1))
        {
            StopAllCoroutines();
            StartCoroutine(TransitionPhase());
            phase = 2;
        }

        if (phase == 2 && !phaseActive && !phaseTransition && !isStunned)
        {
            StartCoroutine(PhaseRoutine2());
            phaseActive = true;
        }

        if ((health < startHealth * phaseThreeFraction) && !phaseTransition &&
            phase == 2)
        {
            StopAllCoroutines();
            StartCoroutine(TransitionPhase());
            phase = 3;
        }

        if (phase == 3 && !phaseActive && !phaseTransition && !isStunned)
        {
            StartCoroutine(PhaseRoutine3());
            phaseActive = true;
        }

        // Check for death
        if (health < 1 && isAlive)
        {
            isAlive = false;
            health = 0;
        }

        // Check for stun
        if (toBeStunned && !isStunned)
        {
            StunMethod();
        }
    }

    // Create and populate lists of boss tools
    private void InitiateTools()
    {
        int numWeakspots = GameObject.Find("Weakspots").transform.childCount;

        for (int i = 0; i < numWeakspots; i++)
        {
            weakspots.Add(GameObject.Find("Weakspots").transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < 2; i++)
        {
            rocketBarrages.Add(GameObject.Find("Launchers").transform.GetChild(i).gameObject);
        }

        laserBarrage = GameObject.Find("Laser Barrage");
    }

    // Create references to boss audio and SFX
    private void InitiateFXandAudio()
    {
        phaseOutAudio = transform.Find("Phase Out Audio").GetComponent<AudioSource>();
        phaseInAudio = transform.Find("Phase In Audio").GetComponent<AudioSource>();
        stunAudio = transform.Find("Stun Audio").GetComponent<AudioSource>();
        introAudio = transform.Find("Intro Audio").GetComponent<AudioSource>();

        explosionGroup = transform.Find("Explosions");
        numExplosions = explosionGroup.childCount;

        for (int i = 0; i < numExplosions; i++)
        {
            explosionParticles.Add(explosionGroup.GetChild(i).gameObject.GetComponent<ParticleSystem>());
            if (i < numExplosions - 1)
            {
                explosionAudioSources.Add(explosionGroup.GetChild(i).GetChild(1).gameObject.GetComponent<AudioSource>());
            }
            else
            {
                explosionAudioSources.Add(explosionGroup.GetChild(i).GetChild(2).gameObject.GetComponent<AudioSource>());
                explosionAudioSources.Add(explosionGroup.GetChild(i).GetChild(3).gameObject.GetComponent<AudioSource>());
            }
        }
    }

    // Reset all weapons and weakspots
    public void ResetTools()
    {
        foreach (GameObject weakspot in weakspots)
        {
            weakspot.GetComponent<WeakspotController>().ResetForPhase();
        }

        foreach (GameObject rocketBarrage in rocketBarrages)
        {
            rocketBarrage.GetComponent<RocketBarrageController>().ResetForPhase();
        }

        laserBarrage.GetComponent<LaserBarrageController>().ResetForPhase();
    }

    // Opens given weakspots
    private void OpenWeakspots(List<int> toOpen)
    {
        foreach (int weakspotNum in toOpen)
        {
            StartCoroutine(weakspots[weakspotNum].GetComponent<WeakspotController>().OpenWeakspot(difficulty));
        }
    }

    // Fires given rockets from given barrages 
    private void FireRockets(List<(int, int)> rocketsToFire)
    {
        foreach ((int barrage, int rocket) in rocketsToFire)
        {
            rocketBarrages[barrage].GetComponent<RocketBarrageController>().FireRocket(rocket);
        }
    }

    // Fires given lasers
    private void FireLasers(List<int> lasersToFire, float laserDuration)
    {
        foreach (int laser in lasersToFire)
        {
            laserBarrage.GetComponent<LaserBarrageController>().FireLaser(laser, laserDuration);
        }
    }

    // Perform cleanup and transition to next boss phase
    private IEnumerator TransitionPhase()
    {
        phaseTransition = true;
        phaseActive = false;
        isStunned = false;
        phaseOutAudio.Play();
        StartCoroutine(StunAnimation());

        ResetTools();

        yield return new WaitForSeconds(phaseOutAudio.clip.length);

        phaseInAudio.Play();
        yield return new WaitForSeconds(phaseInAudio.clip.length);

        phaseTransition = false;
    }

    // Routine for intro phase
    private IEnumerator PhaseRoutine0()
    {
        // Wait before game start
        yield return new WaitForSeconds(3);

        StartCoroutine(WeakspotRoutine1());
        yield return new WaitForSeconds(weakspotDurTotal * 7);

        StartCoroutine(RocketRoutine1());
        yield return new WaitForSeconds(weakspotDurTotal * 1);
        StartCoroutine(WeakspotRoutine1());
        yield return new WaitForSeconds(weakspotDurTotal * 7);

        phase++;
        phaseActive = false;
    }

    // Routine for Phase One
    private IEnumerator PhaseRoutine1()
    {
        while (health > startHealth * phaseTwoFraction)
        {
            ResetTools();
            StartCoroutine(RocketRoutine1());
            StartCoroutine(LaserRoutineOutInInOut());
            yield return new WaitForSeconds(weakspotDurTotal * 1);

            StartCoroutine(WeakspotRoutine1());
            yield return new WaitForSeconds(weakspotDurTotal * 7);
        }        
    }


    // Routine for Phase Two
    private IEnumerator PhaseRoutine2()
    {
        bossStunTime = bossStunTimeTwo;
        while (health > startHealth * phaseThreeFraction)
        {
            ResetTools();
            StartCoroutine(RocketRoutine2());
            StartCoroutine(LaserRoutineLeftRightRightLeft());
            yield return new WaitForSeconds(weakspotDurTotal * 1);

            StartCoroutine(WeakspotRoutine1());
            yield return new WaitForSeconds(weakspotDurTotal * 7);
        }         
    }

    // Routine for Phase Three
    private IEnumerator PhaseRoutine3()
    {
        difficulty = 0.33f;
        bossStunTime = bossStunTimeThree;
        List<string> laserRoutines = new List<string> { "LaserRoutineOutInInOut",
                                                        "LaserRoutineLeftRightRightLeft",
                                                        "LaserRoutineEvenOdd" };
        while (health > 0)
        {
            ResetTools();
            StartCoroutine(RocketRoutine2());
            // Start random laser routine each cycle
            StartCoroutine(laserRoutines[Random.Range(0, 3)]);
            yield return new WaitForSeconds(weakspotDurTotal * 1);

            StartCoroutine(WeakspotRoutine2());
            yield return new WaitForSeconds(weakspotDurTotal * 7);
        }
    }

    private IEnumerator WeakspotRoutine1()
    {
        List<int> notes = new List<int> { 2, 3, 1, 2, 2, 3, 0, 2, 2, 3, 1, 2, 2, 3, 0, 2 };
        foreach (int note in notes)
        {
            OpenWeakspots(new List<int> { note });
            yield return new WaitForSeconds(weakspotDurTotal / 2);
        }
    }

    private IEnumerator WeakspotRoutine2()
    {
        List<int> notes = new List<int> { 2, 4, 3, 4, 1, 4, 2, 4, 2, 4, 3, 4, 0, 4, 2, 4, 2, 4, 3, 4, 1, 4, 2, 4, 2, 4, 3, 4, 0, 4, 2, 4 };
        foreach (int note in notes)
        {
            OpenWeakspots(new List<int> { note });
            yield return new WaitForSeconds(weakspotDurTotal / 4);
        }
    }

    private IEnumerator LaserRoutineOutInInOut()
    {
        FireLasers(new List<int> { 0, 6 }, 3 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 1, 5 }, 2 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 2, 4 }, laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal * 2);

        FireLasers(new List<int> { 3 }, 3 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 2, 4 }, 2 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 1, 5 }, laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);
    }

    private IEnumerator LaserRoutineLeftRightRightLeft()
    {
        FireLasers(new List<int> { 0, 1 }, 3 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 2, 3 }, 2 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 4, 5 }, laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal * 2);

        FireLasers(new List<int> { 5, 6 }, 3 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 3, 4 }, 2 * laserDurTotal - 0.02f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 1, 2 }, laserDurTotal - 0.05f);
        yield return new WaitForSeconds(laserDurTotal);
    }

    private IEnumerator LaserRoutineEvenOdd()
    {
        FireLasers(new List<int> { 1, 3, 5 }, laserDurTotal * 0.75f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> {0, 2, 4, 6 }, laserDurTotal * 0.75f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 1, 3, 5 }, laserDurTotal);
        yield return new WaitForSeconds(laserDurTotal * 2);

        FireLasers(new List<int> {0, 2, 4, 6 }, laserDurTotal * 0.75f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> { 1, 3, 5 }, laserDurTotal * 0.75f);
        yield return new WaitForSeconds(laserDurTotal);

        FireLasers(new List<int> {0, 2, 4, 6 }, laserDurTotal);
        yield return new WaitForSeconds(laserDurTotal);
    }

    private IEnumerator RocketRoutine1()
    {
        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 2);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 0.25f);

        FireRockets(new List<(int, int)> { (1, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 0.75f);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 2);
    }

    private IEnumerator RocketRoutine2()
    {
        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal);

        FireRockets(new List<(int, int)> { (0, 2) });
        yield return new WaitForSeconds(rocketDurTotal * 0.25f);

        FireRockets(new List<(int, int)> { (1, 0) });
        yield return new WaitForSeconds(rocketDurTotal * 0.5f);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 0.25f);

        FireRockets(new List<(int, int)> { (1, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 2);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 0.5f);

        FireRockets(new List<(int, int)> { (1, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 0.5f);

        FireRockets(new List<(int, int)> { (0, 2) });
        yield return new WaitForSeconds(rocketDurTotal * 0.25f);

        FireRockets(new List<(int, int)> { (1, 0) });
        yield return new WaitForSeconds(rocketDurTotal * 0.5f);

        FireRockets(new List<(int, int)> { (0, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 0.25f);

        FireRockets(new List<(int, int)> { (1, 1) });
        yield return new WaitForSeconds(rocketDurTotal * 2);
    }

    private void StunMethod()
    {
        isStunned = true;
        toBeStunned = false;
        phaseActive = false;

        StopAllCoroutines();
        StartCoroutine(Stun());
    }

    private IEnumerator Stun()
    {
        ResetTools();
        yield return new WaitForSeconds(0.5f);

        stunAudio.Play();
        StartCoroutine(StunAnimation());

        foreach (GameObject weakspot in weakspots)
        {
            weakspot.GetComponent<WeakspotController>().OpenWeakspotIndef();
        }

        yield return new WaitForSeconds(bossStunTime);

        foreach (GameObject weakspot in weakspots)
        {
            weakspot.GetComponent<WeakspotController>().CloseWeakspot();
        }

        phaseInAudio.Play();
        yield return new WaitForSeconds(phaseInAudio.clip.length);

        isStunned = false;
    }

    private IEnumerator StunAnimation()
    {
        Vector3 newPos = new Vector3(startPos.x - 0.75f, startPos.y, startPos.z);

        float count = 0;
        while (count < 1.0f)
        {
            count += Time.deltaTime / 0.5f;
            transform.position = Vector3.Lerp(startPos, newPos, stunCurve.Evaluate(count));
            yield return new WaitForEndOfFrame();
        }
    }

    public void GetHit()
    {
        StartCoroutine(HitAnimation());
    }

    private IEnumerator HitAnimation()
    {
        isHit = true;
        Vector3 oldPos = transform.position;
        Vector3 newPos = new Vector3(oldPos.x - 0.1f, oldPos.y, oldPos.z);

        float count = 0;
        while (count < 1.0f)
        {
            count += Time.deltaTime / 0.25f;
            transform.position = Vector3.Lerp(oldPos, newPos, hitCurve.Evaluate(count));
            yield return new WaitForEndOfFrame();
        }

        isHit = false;
    }

    // Play death animation of boss
    public IEnumerator Die()
    {
        phaseOutAudio.Play();
        StartCoroutine(StunAnimation());
        yield return new WaitForSeconds(2.0f);

        explosionParticles[0].Play();
        explosionAudioSources[0].Play();
        yield return new WaitForSeconds(0.2f);
        weakspots[2].SetActive(false);
        yield return new WaitForSeconds(0.8f);

        explosionParticles[1].Play();
        explosionAudioSources[1].Play();
        yield return new WaitForSeconds(0.2f);
        weakspots[3].SetActive(false);
        yield return new WaitForSeconds(0.3f);

        explosionParticles[2].Play();
        explosionAudioSources[2].Play();
        yield return new WaitForSeconds(0.2f);
        weakspots[1].SetActive(false);
        yield return new WaitForSeconds(0.3f);

        explosionParticles[3].Play();
        explosionAudioSources[3].Play();
        yield return new WaitForSeconds(0.2f);
        weakspots[0].SetActive(false);
        yield return new WaitForSeconds(0.3f);

        explosionParticles[4].Play();
        explosionAudioSources[4].Play();
        yield return new WaitForSeconds(0.2f);
        weakspots[4].SetActive(false);
        yield return new WaitForSeconds(1.8f);

        explosionParticles[numExplosions - 1].Play();
        explosionAudioSources[numExplosions - 1].Play();
        explosionAudioSources[numExplosions - 1].Play();
        yield return new WaitForSeconds(0.2f);

        transform.Find("Weakspots").gameObject.SetActive(false);
        transform.Find("Launchers").gameObject.SetActive(false);
        transform.Find("Body").gameObject.SetActive(false);

        youWinText.SetActive(true);
        StartCoroutine(playerController.Leave());
    }

    public void Stop()
    {
        StopAllCoroutines();        
    }
}
