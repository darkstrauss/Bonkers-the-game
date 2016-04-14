using UnityEngine;
using System.Collections;

public class MockEnemy : MonoBehaviour
{
    public static float STARTCOMBATDISTANCE = 1f;
    public GameObject player,CBS;
    public bool activeEnemy;
    public float Maxhealth;
    public int Health;
    public int Dmg = 15;
    public bool isAttacking = false;

    private bool attacked = false;



    // Use this for initialization
    void Start()
    {
        player = Camera.main.GetComponent<PlayerMovement>().player;
        CBS = Camera.main.GetComponent<PlayerMovement>().CBS;
    }


    // Update is called once per frame
    void Update()
    {

        // CHeck for distance between player and enemy

        // Check for attacking state

        if (((gameObject.transform.position - player.transform.position).magnitude) <= STARTCOMBATDISTANCE && (attacked == false))
        {
            StartCoroutine(Attackcylce());
            attacked = true;
        }
        if (Health <= 0)
        {
            DEAD();
        }
    }

    IEnumerator Attackcylce()
    {
        CBS.SetActive(true);
        CBS.GetComponent<CombatSystem>().ActiveEnemy = gameObject;
        CBS.GetComponent<CombatSystem>().IncomingAttack();
        yield return new WaitForSeconds(Random.Range(2.0f, 6.0f));
        StartCoroutine(attackAnim());
        StartCoroutine(Attackcylce());
    }

    IEnumerator attackAnim()
    {
        isAttacking = true;
        gameObject.GetComponent<Animator>().Play("Hit");
        yield return new WaitForSeconds(1);
        isAttacking = false;
    }

    public void HealthDown(int amountDOWN)
    {
        Health -= amountDOWN;
    }

    // Funchtion to increase health
    public void HealthUp(int amountUP)
    {
        Health += amountUP;
    }

    private void DEAD()
    {
        gameObject.GetComponent<Animator>().Play("Die");
        CBS.SetActive(false);
        Destroy(gameObject, 3.0f);
    }
}
