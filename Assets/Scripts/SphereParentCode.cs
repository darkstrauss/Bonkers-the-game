using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using LitJson;

public class SphereParentCode : MonoBehaviour
{
    private string jSonString;
    private JsonData strikeData;

    public List<string> UsableStrikes = new List<string>();
    public Text Output;

    private List<int> Strike1 = new List<int>();
    private List<int> Strike2 = new List<int>();
    private List<int> Strike3 = new List<int>();
    private List<int> Strike4 = new List<int>();

    private List<string> InputStrikeList = new List<string>();
    private List<string> OutputStrikeList = new List<string>();
    private int TotalDMG;

    private string IDString1, IDString2, IDString3, IDString4;
    private bool striking, strikeFound;
    private int currentStrike = 1;
    private int VisualCombo = 1;
    private int Soundplay = 1;

    public GameObject Button1;
    public GameObject Button2;
    public GameObject Button3;
    public GameObject Button4;
    public GameObject Button5;
    public GameObject Button6;
    public GameObject Button7;
    public GameObject Button8;
    public GameObject Button9;

    public GameObject ApCircleL;
    public GameObject ApCircleR;
    public GameObject CBS;

    private GameObject player;
    private Animator playerAnimator;

    public GameObject AttackSound1, AttackSound2, AttackSound3;

    private List<GameObject> Buttons = new List<GameObject>();


    // Use this for initialization
    private void Start()
    {
        CBS = Camera.main.GetComponent<PlayerMovement>().CBS;
        player = Camera.main.GetComponent<PlayerMovement>().player;
        playerAnimator = player.GetComponent<Animator>();

        strikeFound = false;
        UsableStrikes.Add("Horizontal");
        UsableStrikes.Add("Vertical");
        UsableStrikes.Add("Diagonal");
        UsableStrikes.Add("Jab");
        UsableStrikes.Add("Quick Punch");
        UsableStrikes.Add("Hook");
        UsableStrikes.Add("One-Two Punch");
        UsableStrikes.Add("Wombo Combo");
        UsableStrikes.Add("9");
        UsableStrikes.Add("10");
        UsableStrikes.Add("11");
        UsableStrikes.Add("12");
        UsableStrikes.Add("13");
        UsableStrikes.Add("14");
        UsableStrikes.Add("15");
        UsableStrikes.Add("16");
        UsableStrikes.Add("LastSpiral");

        Buttons.Add(Button1);
        Buttons.Add(Button2);
        Buttons.Add(Button3);
        Buttons.Add(Button4);
        Buttons.Add(Button5);
        Buttons.Add(Button6);
        Buttons.Add(Button7);
        Buttons.Add(Button8);
        Buttons.Add(Button9);

        //Strike1.Add(11);
        //Strike2.Add(22);
        //Strike3.Add(33);
        //Strike4.Add(44);

        TextAsset something = Resources.Load("jsonTest") as TextAsset;

        jSonString = something.text;
        strikeData = JsonMapper.ToObject(jSonString);

        // Strikedata = Converted file
        // Horizontal = subset
        // Number is array index
        // Number = subArray index

        try
        {
            string temp = strikeData["Horizontal"][0]["pos"][3].ToString();
        }
        catch (System.Exception)
        {
            Debug.LogError("Json fail failed");
            Output.text = "Failed to load Json";
            throw;
        }
        IDString1 = "";
        IDString2 = "";
        IDString3 = "";
        IDString4 = "";
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void AddSphereID(int IDinput)
    {
        striking = true;
        if (currentStrike == 1)
            Strike1.Add(IDinput);
        if (currentStrike == 2)
            Strike2.Add(IDinput);
        if (currentStrike == 3)
            Strike3.Add(IDinput);
        if (currentStrike == 4)
            Strike4.Add(IDinput);
    }

    public void EndStrike()
    {
        striking = false;
        currentStrike++;
        if (CBS.GetComponent<CombatSystem>().Stamina >= 50)
        {
            if (Strike1.Count > 0)
            {
                CBS.GetComponent<CombatSystem>().Stamina -= 50f;
            }

            // Start right awya with dmg calculation if no more strikes are possible
            if (CBS.GetComponent<CombatSystem>().Stamina <= 50f)
            {
                ReturnSphereIDs();
            }
            else
            {
                // Wait for more strikes to be input
                StopCoroutine(WaitForStrike());
                StartCoroutine(WaitForStrike());
            }
        }
        StartCoroutine(ChangeButtonColor());
    }

    public void ReturnSphereIDs()
    {
        if (Strike1 != null && Strike1.Count > 0)
            foreach (var item in Strike1)
            {
                IDString1 += item + "";
            }
        if (Strike2 != null && Strike2.Count > 0)
            foreach (var item in Strike2)
            {
                IDString2 += item + "";
            }
        if (Strike3 != null && Strike3.Count > 0)
            foreach (var item in Strike3)
            {
                IDString3 += item + "";
            }
        if (Strike4 != null && Strike4.Count > 0)
            foreach (var item in Strike4)
            {
                IDString4 += item + "";
            }
        //Debug.Log("returing ID's Strike 1:" + IDString1);
        //Debug.Log("returing ID's Strike 2:" + IDString2);
        //Debug.Log("returing ID's Strike 3:" + IDString3);
        //Debug.Log("returing ID's Strike 4:" + IDString4);

        InputStrikeList.Add(IDString1);
        InputStrikeList.Add(IDString2);
        InputStrikeList.Add(IDString3);
        InputStrikeList.Add(IDString4);

        foreach (string strike in InputStrikeList)
        {
            foreach (string item in UsableStrikes)
            {
                for (int i = 0; i < strikeData[item][0]["pos"].Count; i++)
                {
                    // Debug.Log(item + "Pass" + i);
                    // Debug.Log(strikeData[item][0]["pos"][i].ToString());
                    if (strike == strikeData[item][0]["pos"][i].ToString())
                    {
                        strikeFound = true;
                        TotalDMG += (int.Parse(strikeData[item][1]["AttackStats"][2].ToString()));
                        OutputStrikeList.Add(strikeData[item][1]["AttackStats"][0].ToString());
                        
                        break;
                    }

                }
                if (strikeFound)
                    break;
            }
            if (!strikeFound && strike != "")
            {
                Debug.Log("Faulty Input" + strike);
            }
            strikeFound = false;
        }

        // Combo Call
        ComboCall();


        // Outputcall
        OutputFun();

        // Reseting Variables
        Reset();
    }

    void ComboCall()
    {
        bool combo1 = false;
        bool combo2 = false;
        bool combo3 = false;


        if (IDString2 != "")
        {
            if (Strike1[Strike1.Count - 1] == (Strike2[0]))
            {
                VisualCombo++;
                Output.text = ("Combo " + VisualCombo + "X");
                foreach (string item in UsableStrikes)
                {
                    for (int i = 0; i < strikeData[item][0]["pos"].Count; i++)
                    {
                        if (IDString2 == strikeData[item][0]["pos"][i].ToString())
                        {
                            TotalDMG *= (int.Parse(strikeData[item][1]["AttackStats"][1].ToString()));
                            combo1 = true;
                            break;
                        }
                        if (combo1)
                            break;
                    }
                    if (combo1)
                        break;
                }

            }
            else
            {
                VisualCombo = 1;
                Output.text = ("Combo " + VisualCombo + "X");
            }

            if (IDString3 != "")
            {
                if (Strike2[Strike2.Count - 1] == (Strike3[0]))
                {
                    VisualCombo++;
                    Output.text = ("Combo " + VisualCombo + "X");
                    foreach (string item in UsableStrikes)
                    {
                        for (int i = 0; i < strikeData[item][0]["pos"].Count; i++)
                        {
                            if (IDString3 == strikeData[item][0]["pos"][i].ToString())
                            {
                                TotalDMG *= (int.Parse(strikeData[item][1]["AttackStats"][1].ToString()));
                                combo2 = true;
                                break;
                            }
                            if (combo2)
                                break;
                        }
                        if (combo2)
                            break;
                    }
                }
                else
                {
                    VisualCombo = 1;
                    Output.text = ("Combo " + VisualCombo + "X");
                } 
            }

            if (IDString4 != "")
            {
                if (Strike3[Strike3.Count - 1] == (Strike4[0]))
                {
                    VisualCombo++;
                    Output.text = ("Combo " + VisualCombo + "X");
                    foreach (string item in UsableStrikes)
                    {
                        for (int i = 0; i < strikeData[item][0]["pos"].Count; i++)
                        {
                            if (IDString4 == strikeData[item][0]["pos"][i].ToString())
                            {
                                TotalDMG *= (int.Parse(strikeData[item][1]["AttackStats"][2].ToString()));
                                combo3 = true;
                                break;
                            }
                            if (combo3)
                                break;
                        }
                        if (combo3)
                            break;
                    }
                }
                else
                {
                    VisualCombo = 1;
                    Output.text = ("Combo " + VisualCombo + "X");
                }
            }

        }
        else
        {
            VisualCombo = 1;
            Output.text = ("Combo " + VisualCombo + "X");
        }
    }

    private void DisplayInput()
    {
        foreach (string item in OutputStrikeList)
        {
            // Add Something to display input attacks at the top of the screen
        }
    }

    private void Reset()
    {
        Camera.main.GetComponent<PlayerMovement>().player.GetComponent<Animator>().Play("Idle");
        InputStrikeList.Clear();
        OutputStrikeList.Clear();
        Strike1.Clear();
        Strike2.Clear();
        Strike3.Clear();
        Strike4.Clear();
        IDString1 = "";
        IDString2 = "";
        IDString3 = "";
        IDString4 = "";
        currentStrike = 1;
        TotalDMG = 0;
    }

    public void OutputFun()
    {
        CBS.GetComponent<CombatSystem>().EnemyHealthDown(TotalDMG);
        playerAnimator.Play("Spin Attack");
        if (Soundplay == 1)
        {
            AttackSound1.GetComponent<AudioSource>().Play();
            Soundplay++;
        }
        else if (Soundplay == 2)
        {
            AttackSound2.GetComponent<AudioSource>().Play();
            Soundplay++;
        }
        else if( Soundplay == 3)
        {
            AttackSound3.GetComponent<AudioSource>().Play();
            Soundplay = 1;
        }
    }


    IEnumerator WaitForStrike()
    {
        yield return new WaitForSeconds(0.5f);
        if (striking == false)
        {
            ReturnSphereIDs();
        }
    }

    IEnumerator ChangeButtonColor()
    {
        yield return new WaitForSeconds(.2f);
        foreach (GameObject button in Buttons)
        {
            button.GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.8f);
            button.GetComponent<SphereCode>().EnterBool = false;
        }

    }

    private void OnDisable()
    {
        Reset();
    }

    private void OnEnable()
    {
        Reset();
    }
}
