using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicOverflow : MonoBehaviour
{
    private AudioSource bgm;
    private GameObject[] other;
    private bool NotFirst = false;
    public bool muted = false;
    private void Awake()
    {
        other = GameObject.FindGameObjectsWithTag("Music");

        foreach (GameObject oneOther in other)
        {
            if (oneOther.scene.buildIndex == -1)
            {
                NotFirst = true;
            }
        }

        if (NotFirst == true)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(transform.gameObject);
        bgm = GetComponent<AudioSource>();
    }

    public void PlayMusic()
    {
        if (bgm.isPlaying) return;
        bgm.Play();
    }

    public void ToggleMusic()
    {
        if (!muted)
        {
            bgm.Pause();
            muted = true;
        }
        else bgm.UnPause();
        
    }
}
