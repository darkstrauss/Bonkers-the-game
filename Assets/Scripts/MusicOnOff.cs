using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MusicOnOff : MonoBehaviour {

    private bool Mute = false;

    public SpriteState pressed = new SpriteState();
    public SpriteState unpressed = new SpriteState();
    public Button btnMain;


    void Start()
    {

    }

    public void Musiconoff ()
    {
        Mute = !Mute;
        Camera.main.GetComponent<AudioSource>().mute = Mute;
        if (Mute)
        {
            btnMain = gameObject.GetComponent<Button>();

            btnMain.spriteState = pressed;
        }
        if ( !Mute)
        {
            btnMain = gameObject.GetComponent<Button>();

            btnMain.spriteState = unpressed;
        }
    }
}
