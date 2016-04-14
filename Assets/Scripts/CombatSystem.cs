using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatSystem : MonoBehaviour
{
    public float dTime;
    public bool isActive;
    public float Stamina, StaminaIncrease;
    public int Health;
    public GameObject DodgeCircleL, DodgeCircleR;
    public GameObject ActiveEnemy;
    public GameObject PlayerHealth, EnemyHealth, PlayerStamina;
    public GameObject Sound1, Sound2, Sound3, Sound4, Sound5, Sound6;
    private List<GameObject> Sounds = new List<GameObject>();
    private int CurrentSound = 1;



    private IEnumerator coroutine1, coroutine2, staminaCO1;

    private void Awake()
    {
        ResetCo();
    }
    // Use this for initialization
    void Start()
    {
        Health = 100;
        Sounds.Add(Sound1);
        Sounds.Add(Sound2);
        Sounds.Add(Sound3);
        Sounds.Add(Sound4);
        Sounds.Add(Sound5);
        Sounds.Add(Sound6);
    }

    void OnEnable()
    {
        Camera.main.GetComponent<PlayerMovement>().inCombat = true;
        isActive = true;
        Camera.main.GetComponent<PlayerMovement>().CancelMovement();
        StartCoroutine(staminaCO1);
        
        // Pause game
        // Update avaiblibe strikes
    }

    void OnDisable()
    {
        Camera.main.GetComponent<PlayerMovement>().inCombat = false;
        isActive = false;
        ResetCo();
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            GameOver();
        }
      
        DisplayStamina();
        DisplayHealth();
    }

    private void DisplayStamina()
    {
        if (Stamina >= 1)
            PlayerStamina.transform.localScale = new Vector3(Stamina / 200, 1f, 1f);
        if (Stamina <= 1)
            PlayerStamina.transform.localScale = new Vector3(0.0f, 1f, 1f);
    }

    private void DisplayHealth()
    {
        
        PlayerHealth.transform.localScale = new Vector3 (1f, Health/100f , 1f);
        EnemyHealth.transform.localScale = new Vector3(1f, 0.0f + ActiveEnemy.GetComponent<MockEnemy>().Health / ActiveEnemy.GetComponent<MockEnemy>().Maxhealth , 1f);
        // Display Health in someway
    }

    // Reset IEnumerators for use later
    private void ResetCo()
    {
        coroutine1 = DodgeVisualIE(dTime, DodgeCircleL);
        coroutine2 = DodgeVisualIE(dTime, DodgeCircleR);
        staminaCO1 = STIncrease();

        DodgeCircleL.SetActive(false);
        DodgeCircleR.SetActive(false);
    }

    // Start when a new attack comes in
    public void IncomingAttack()
    {
        

        DodgeCircleL.transform.localScale = new Vector3(4f, 4f, 4f);
        DodgeCircleR.transform.localScale = new Vector3(4f, 4f, 4f);

        DodgeCircleL.SetActive(true);
        DodgeCircleR.SetActive(true);

        StartCoroutine(coroutine1);
        StartCoroutine(coroutine2);

        
    }

    // Used to cancel coroutines and dodge incoming attack
    public void DodgeVisual()
    {
        StopCoroutine(coroutine1);
        StopCoroutine(coroutine2);

        DodgeCircleL.SetActive(false);
        DodgeCircleR.SetActive(false);

        ResetCo();
    }

    // Function to decrease health
    public void HealthDown(int amountDOWN)
    {
        Health -= amountDOWN;
    }

    // Funchtion to increase health
    public void HealthUp(int amountUP)
    {
        Health += amountUP;
    }

    // Decrease enemyhealth
    public void EnemyHealthDown(int amountDOWN)
    {
        if (ActiveEnemy != null)
        {
            ActiveEnemy.GetComponent<MockEnemy>().HealthDown(amountDOWN);
        }
    }

    // Funchtion to increase enemyhealth
    public void EnemyHealthUp(int amountUP)
    {
        ActiveEnemy.GetComponent<MockEnemy>().HealthUp(amountUP);
    }

    // Dislpay game over screen + score etc
    private void GameOver()
    {
        //Play animtion
        Destroy(gameObject);
        // Show Drag back to cell scene
        Application.LoadLevel("GameOver");
        
    }

    IEnumerator DodgeVisualIE(float time, GameObject rORl)
    {
        Vector3 originalScale = rORl.transform.localScale;
        Vector3 destinationScale = new Vector3(1.5f, 1.5f, 1.5f);

        float currentTime = 0.0f;

        while (currentTime <= time)
        {
            rORl.transform.localScale = Vector3.Lerp(originalScale, destinationScale, Mathf.SmoothStep(0, 1, (currentTime / time)));
            currentTime += Time.deltaTime;
            yield return null;
        }
        rORl.SetActive(false);
        HealthDown(ActiveEnemy.GetComponent<MockEnemy>().Dmg);
        Sounds[CurrentSound].GetComponent<AudioSource>().Play();
        CurrentSound++;
        if(CurrentSound == 6)
        {
            CurrentSound = 1;
        }
        DodgeVisual();
    }

    IEnumerator STIncrease()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (Stamina <= 200)
            {
                Stamina += StaminaIncrease;
            }
        }
    }

    IEnumerator loadGameOver()
    {
        yield return new WaitForSeconds(2.0f);

    }
}
