using UnityEngine;
using System.Collections;

public class LoadPanel : MonoBehaviour {

    public GameObject Panel;
    public GameObject CurrentPanel;

    public void loadLevel()
    {
        Panel.SetActive(true);
        CurrentPanel.SetActive(false);
    }
}
