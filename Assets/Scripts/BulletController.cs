using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float speed = 10.0f;
    private float zBound = -0.15f;

    private GameObject boss;
    private Camera cam;
    private AudioSource bossHitAudio;
    private AudioSource bulletDestroyAudio;

    // Start is called before the first frame update
    void Start()
    {
        boss = GameObject.Find("Boss");
        bossHitAudio = boss.transform.Find("Boss Hit Audio").GetComponent<AudioSource>();
        bulletDestroyAudio = GameObject.Find("Player").transform.Find("Audio Bullet Destroy").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
        if (transform.position.z > zBound)
        {
            gameObject.SetActive(false);
        }
    }

    // Aim bullet towards mouse pointer
    public void LookAtTarget()
    {        
        cam = Camera.main;
        Vector3 camPos = cam.transform.position;
        Vector3 mousePos = Input.mousePosition;

        Vector3 aim = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, zBound - camPos.z));
        transform.LookAt(aim);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Ignore collision with player or shield
        if (!other.CompareTag("Player") &&
            !other.CompareTag("Shield"))
        {
            // If bullet hits weakspot when weak, do damage
            if (other.CompareTag("Weakspot") &&
                other.GetComponent<WeakspotController>().isWeak)
            {
                boss.GetComponent<BossController>().health -= 1.0f;
                boss.GetComponent<BossController>().GetHit();
                other.GetComponent<WeakspotController>().isHit = true;
                bossHitAudio.time = 0.0f;
                bossHitAudio.Play();
            }

            bulletDestroyAudio.Play();
            gameObject.SetActive(false);
        }
    }
}
