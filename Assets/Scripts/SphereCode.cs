using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SphereCode : MonoBehaviour {

    public GameObject Parent;
    public int SphereID;
    private SphereParentCode spCode;
    private GUILayer UILayer;
    public bool EnterBool = false;
    private bool Touchthis = false;

    // Use this for initialization
    void Start ()
    {
        UILayer = GetComponentInParent<GUILayer>();
        spCode = GetComponentInParent<SphereParentCode>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!EnterBool && !Touchthis)
        {
            foreach (Touch touch in Input.touches)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject == gameObject.transform)
                    {
                        Touchthis = true;
                    }
                }
            }
        }
    }

    public void OnEnter()
    {
        if ((Camera.main.GetComponent<PlayerMovement>().CBS.GetComponent<CombatSystem>().Stamina >= 50f) && (Input.GetMouseButton(0) || (Touchthis)) && !EnterBool)
        {
            spCode.AddSphereID(SphereID);
            EnterBool = true;
            GetComponent<Button>().image.color = new Color(0f, 1f, 1f, 0.8f);
        } 
        else if ((Camera.main.GetComponent<PlayerMovement>().CBS.GetComponent<CombatSystem>().Stamina <= 50f) && (Input.GetMouseButton(0) || (Touchthis)) && !EnterBool)
        {
            GetComponent<Button>().image.color = new Color( 0f, 0f, 1f ,0.8f);
            StartCoroutine(Recolor());
        }      
    }

    public void OnExit()
    {
        if (EnterBool == true)
        {
            GetComponent<Button>().image.color = new Color(1f, 0f, 0f, 0.8f);
            StartCoroutine(ResestBool());
        }
             
    }

    IEnumerator ResestBool()
    {
        yield return new WaitForSeconds(0.1f);
        EnterBool = false;
        Touchthis = false;
    }

    IEnumerator Recolor()
    {
        while((Camera.main.GetComponent<PlayerMovement>().CBS.GetComponent<CombatSystem>().Stamina <= 50f))
        {
            yield return new WaitForSeconds(0.1f);
        }
        gameObject.GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.8f);
    }
}
