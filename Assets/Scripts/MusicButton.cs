using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class MusicButton : MonoBehaviour
{
    private AudioSource bgm;
    [SerializeField] Image buttonImage;
    private GameObject[] other;
    private Color startColor;
    // Start is called before the first frame update
    void Start()
    {
        other = GameObject.FindGameObjectsWithTag("Music");

        foreach (GameObject oneOther in other)
        {
            bgm = oneOther.GetComponent<AudioSource>();
        }
        startColor = buttonImage.color;
    }

    private void Update()
    {
        if (bgm != null)
        {
            if (bgm.mute)
                buttonImage.color = Color.grey;
            else
                buttonImage.color = startColor;
        }
    }

    public void ToggleMusic()
    {
        if (bgm != null)
        {
            if (!bgm.mute)
            {
                bgm.mute = true;
            }
            else
            {
                bgm.mute = false;
            }
        }

    }
}
