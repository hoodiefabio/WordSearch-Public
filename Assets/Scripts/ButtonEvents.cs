using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonEvents : MonoBehaviour
{
    [SerializeField] bool onMenu;

    public void StartGame(int index)
    {
        StartCoroutine("LoadLevel", index);
    }

    IEnumerator LoadLevel(int level)
    {
        SceneManager.LoadSceneAsync(level);
        yield return null;
    }

    public void Replay()
    {
        StartCoroutine("LoadLevel", SceneManager.GetActiveScene().buildIndex);
    }

   public void ToMenu()
    {
        StartCoroutine("LoadLevel", 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!onMenu)
            {
                ToMenu();
            }
            else Application.Quit();
        }
    }
}
