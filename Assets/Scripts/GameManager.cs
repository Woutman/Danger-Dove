using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject youWinText;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject preTitleScreen;
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject introScreen;
    [SerializeField] private GameObject bars;
    [SerializeField] private Button mediumButton;
    private GameObject player;
    private GameObject boss;
    private BossController bossController;
    private PlayerController playerController;
    private Slider healthBar;
    private Slider shieldBar;
    private Slider bossBar;
    private AudioSource titleMusic;

    private bool introIsActive = true;
    public bool gameIsRunning = true;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerController>();

        boss = GameObject.Find("Boss");
        bossController = boss.GetComponent<BossController>();

        titleMusic = GameObject.Find("Title Music").GetComponent<AudioSource>();

        InitiateSliders();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && titleScreen.activeSelf)
        {
            titleScreen.SetActive(false);
            introScreen.SetActive(true);
            mediumButton.Select();
        }

        if (Input.anyKeyDown && preTitleScreen.activeSelf)
        {
            preTitleScreen.SetActive(false);
            titleScreen.SetActive(true);
            titleMusic.Play();
        }       

        if (!introIsActive && gameIsRunning)
        {
            UpdateSliders();

            if (!boss.GetComponent<BossController>().isAlive)
            {
                gameIsRunning = false;
                playerController.gameIsRunning = false;
                bossController.Stop();
                bossController.ResetTools();

                StartCoroutine(bossController.Die());                
            }

            if (!player.GetComponent<PlayerController>().isAlive)
            {
                gameIsRunning = false;
                playerController.gameIsRunning = false;
                gameOverText.SetActive(true);
                bossController.Stop();
                bossController.ResetTools();

                StartCoroutine(playerController.Die());
            }
        }       
    }

    // Initiates counters
    private void InitiateSliders()
    {
        healthBar = GameObject.Find("Health Bar").GetComponent<Slider>();
        healthBar.value = playerController.health;
        shieldBar = GameObject.Find("Shield Bar").GetComponent<Slider>();
        shieldBar.value = playerController.shieldDurability;
        bossBar = GameObject.Find("Boss Bar").GetComponent<Slider>();
        bossBar.value = bossController.health;
        bars.SetActive(false);
    }

    // Update counters
    private void UpdateSliders()
    {
        healthBar.value = playerController.health;
        shieldBar.value = playerController.shieldDurability;
        bossBar.value = bossController.health;
    }

    // Start game proper
    public void StartGame()
    {
        introScreen.SetActive(false);
        StartCoroutine(FadeOutIntro());
    }

    //Reload game
    public void Restartgame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator FadeOutIntro()
    {
        float count = 0;
        float curVol = titleMusic.volume;
        playerController.gameIsRunning = true;

        while (count < 3.0f)
        {
            count += Time.deltaTime;
            titleMusic.volume = Mathf.Lerp(curVol, 0.0f, count / 3.0f);            
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1.0f);
        introIsActive = false;
        bossController.gameIsRunning = true;             
    }
}
