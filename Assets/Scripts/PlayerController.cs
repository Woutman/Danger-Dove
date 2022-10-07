using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float shieldRegen = 5.0f;
    [SerializeField] private float blockTiming = 0.1f;
    [SerializeField] private float stunTime = 2.0f;
    [SerializeField] private float laserDamage = 5.0f;
    [SerializeField] private float rocketDamage = 10.0f;
    [SerializeField] private float shieldDamage = 5.0f;
    [SerializeField] private int bulletBufferSize = 5;
    [SerializeField] private Color colorSuperShieldHit;
    [SerializeField] private Color colorShieldHit;
    [SerializeField] private Color colorShieldDefault;
    [SerializeField] private AnimationCurve shieldHitScaleCurve;
    [SerializeField] private AnimationCurve shieldSuperChargeColorCurve;
    [SerializeField] private AnimationCurve shieldSuperChargeScaleCurve;

    private float xBound = 4.5f;
    public float health = 100.0f;
    public float shieldDurability = 50.0f;

    public bool gameIsRunning = false;
    public bool isAlive = true;
    public bool shieldActive = false;
    public bool superShield = false;
    public bool isStunned = false;
    private bool isSuperCharged = false;

    private Vector3 scaleDefault = new Vector3(1.5f, 1.5f, 1.5f);
    private Vector3 scaleSuperShieldHit = new Vector3(1.75f, 1.75f, 1.75f);
    private Vector3 scaleShieldSuperCharge = new Vector3(15f, 15f, 15f);

    public GameObject shield;
    public GameObject bulletPrefab;
    private List<GameObject> bulletBuffer = new List<GameObject>();
    private AudioSource audioFleshHit;
    private AudioSource audioShieldhHit;
    private AudioSource audioSuperShieldHit;
    private AudioSource audioLaserHit;
    private AudioSource audioDeath;
    private AudioSource audioSuperCharge;
    private Animator animator;
    private ParticleSystem FXBlood;

    // Start is called before the first frame update
    void Start()
    {
        animator = transform.Find("Eagle").GetChild(0).GetComponent<Animator>();
        FXBlood = transform.Find("Eagle").GetChild(0).GetChild(2).GetComponent<ParticleSystem>();

        InitiateBullets();
        InitiateAudio();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameIsRunning)
        {
            CheckForStuns();
            MovePlayer();
            BoundPlayer();
            ActivateShield();
            Shoot();
            CheckForDeath();
        }        
    }

    // Creates and fills buffer of bullets
    private void InitiateBullets()
    {
        for (int i = 0; i < bulletBufferSize; i++)
        {
            bulletBuffer.Add(Instantiate(bulletPrefab));
        }

        foreach (GameObject bullet in bulletBuffer)
        {
            bullet.SetActive(false);
        }
    }

    // Create audio source references
    private void InitiateAudio()
    {
        audioFleshHit = transform.Find("Audio Flesh Hit").GetComponent<AudioSource>();
        audioShieldhHit = transform.Find("Audio Shield Hit").GetComponent<AudioSource>();
        audioSuperShieldHit = transform.Find("Audio Super Shield Hit").GetComponent<AudioSource>();
        audioLaserHit = transform.Find("Audio Laser Hit").GetComponent<AudioSource>();
        audioDeath = transform.Find("Audio Death").GetComponent<AudioSource>();
        audioSuperCharge = transform.Find("Audio Super Charge").GetComponent<AudioSource>();
    }

    // Move player based on horizontal input
    private void MovePlayer()
    {
        if (!isStunned)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            transform.Translate(Vector3.right * horizontalInput * Time.deltaTime * speed);
            Transform eagle = transform.Find("Eagle");
            eagle.rotation = Quaternion.Euler(0, 0, -30 * horizontalInput);
        }
    }

    // Prevent player from leaving the play area
    private void BoundPlayer()
    {
        if (transform.position.x < -xBound)
        {
            transform.position = new Vector3(-xBound, 0, -10);
        }
        else if (transform.position.x > xBound)
        {
            transform.position = new Vector3(xBound, 0, -10);
        }
    }

    // Activate shield around player
    private void ActivateShield()
    {
        if (Input.GetMouseButtonDown(1) && !isStunned && !isSuperCharged)
        {
            shield.SetActive(true);
            shieldActive = true;

            // Activate perfect block window
            StartCoroutine(SuperShieldTimer());
        }
        
        if (!Input.GetMouseButton(1) && !isSuperCharged || isStunned)
        {
            shield.SetActive(false);
            shieldActive = false;

            // Shield returns slowly to equilibrium when inactive
            switch (shieldDurability)
            {
                case < 50.0f:
                    shieldDurability += shieldRegen * Time.deltaTime;
                    break;
                case > 50.0f:
                    shieldDurability -= shieldRegen * Time.deltaTime;
                    break;
            }
        }
    } 

    private IEnumerator SuperShieldTimer()
    {
        superShield = true;
        yield return new WaitForSeconds(blockTiming);
        superShield = false;
    }

    // Shoot projectile towards mouse pointer if shield is not active
    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0) && !shieldActive && !isStunned)
        {
            foreach (GameObject bullet in bulletBuffer)
            {
                if (bullet.activeSelf)
                {
                    continue;
                }

                bullet.transform.SetPositionAndRotation(transform.position, bulletPrefab.transform.rotation);
                bullet.GetComponent<BulletController>().LookAtTarget();
                bullet.SetActive(true);
                break;
            }
        }
    }

    private void CheckForStuns()
    {
        // Stun player when shield durability drops below 0
        if (shieldDurability < 1)
        {
            shieldDurability = 1;
            StartCoroutine(Stun());
        }

        // Stun boss when shield durability goes above 100
        if (shieldDurability > 99)
        {
            shieldDurability = 50;
            StartCoroutine(ShieldSuperCharge());           
        }
    }

    private IEnumerator Stun()
    {
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }

    // Kill player when health drops below zero
    private void CheckForDeath()
    {
        if (health < 1)
        {
            isAlive = false;
            health = 0;
        }
    }

    // Calculates damage and plays sound when a rocket is fired
    public void CalculateDamage()
    {
        // If shield is down, lose health
        if (!shieldActive)
        {
            audioFleshHit.Play();
            FXBlood.Play();
            health -= rocketDamage;
            return;
        }

        // If shield is up lose durability, unless perfectly timed block
        if (superShield)
        {
            audioSuperShieldHit.Play();
            StartCoroutine(ChangeShieldColor(colorSuperShieldHit));
            StartCoroutine(ChangeShieldSize());
            shieldDurability += shieldDamage;
            return;
        }
        else
        {
            audioShieldhHit.Play();
            StartCoroutine(ChangeShieldColor(colorShieldHit));
            StartCoroutine(ChangeShieldSize());
            shieldDurability -= shieldDamage;
            return;
        }   
    }

    // Change shield color
    private IEnumerator ChangeShieldColor(Color color)
    {
        Color shieldColorDefault = shield.GetComponent<Renderer>().material.color;
        shield.GetComponent<Renderer>().material.color = color;
        yield return new WaitForSeconds(0.2f);

        shield.GetComponent<Renderer>().material.color = shieldColorDefault;
    }

    // Change shield size 
    private IEnumerator ChangeShieldSize()
    {
        float count = 0;
        while (count < 1.0f)
        {
            count += Time.deltaTime / 0.2f;
            shield.transform.localScale = Vector3.Lerp(scaleDefault, scaleSuperShieldHit, shieldHitScaleCurve.Evaluate(count));           
            yield return new WaitForEndOfFrame();
        }
    }

    // Animation for when shield reaches 100 durability
    private IEnumerator ShieldSuperCharge()
    {
        isSuperCharged = true;
        shield.SetActive(true);

        audioSuperCharge.Play();
        GameObject.Find("Boss").GetComponent<BossController>().toBeStunned = true;

        Vector3 oldScale = shield.transform.localScale;
        Color oldShieldColor = colorSuperShieldHit;
        Color newShieldColor = new Vector4(oldShieldColor.r, oldShieldColor.g, oldShieldColor.b, 0f);

        float count = 0;
        while (count < 1.0f)
        {
            count += Time.deltaTime / 1.0f;
            shield.transform.localScale = Vector3.Lerp(oldScale, scaleShieldSuperCharge, shieldSuperChargeScaleCurve.Evaluate(count));
            shield.GetComponent<Renderer>().material.color = Color.Lerp(oldShieldColor, newShieldColor, shieldSuperChargeColorCurve.Evaluate(count));
            yield return new WaitForEndOfFrame();
        }

        shield.transform.localScale = scaleDefault;
        shield.GetComponent<Renderer>().material.color = colorShieldDefault;
        shield.SetActive(false);
        isSuperCharged = false;
    }

    // Play Leave animation and set player object to inactive
    public IEnumerator Leave()
    {
        animator.Play("Leave");
        
        yield return new WaitForSeconds(2.5f);

        gameObject.SetActive(false);
    }

    // Play Death animation and set player object to inactive
    public IEnumerator Die()
    {
        animator.Play("Death");
        audioDeath.Play();

        yield return new WaitForSeconds(audioDeath.clip.length);

        gameObject.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        // Lose health while standing in lasers
        if (other.gameObject.CompareTag("Laser"))
        {
            health -= laserDamage * Time.deltaTime;

            if (!audioLaserHit.isPlaying && isAlive)
            {
                audioLaserHit.Play();
            }            

            if (!FXBlood.isPlaying)
            {
                FXBlood.Play();
            }
        }
    }

    // Set player health based on difficulty
    public void SetHealth(int difficulty)
    {
        health = 100 * difficulty;
    }
}
