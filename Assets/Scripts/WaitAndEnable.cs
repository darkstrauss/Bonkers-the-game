using UnityEngine;
using System.Collections;

public class WaitAndEnable : MonoBehaviour {

    public GameObject RestartButton;
    public GameObject RE;

	// Use this for initialization
	void Start () {
        StartCoroutine(WaitAndRestart());
	}

    IEnumerator WaitAndRestart()
    {
        yield return new WaitForSeconds(2.0f);
        RestartButton.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        RE.SetActive(true);
    }
	
}
