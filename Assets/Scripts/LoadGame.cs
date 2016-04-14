using UnityEngine;
using System.Collections;

public class LoadGame : MonoBehaviour {

    public string SceneName;

	public void LoadGameFunction()
    {
        Application.LoadLevel(SceneName);
    }
}
