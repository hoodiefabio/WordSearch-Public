using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Letters : MonoBehaviour
{
    public bool utilized = false;
    public bool identified = false;
    public bool startWord = false;  
    public bool endWord = false;
    public int index = 0;
    public TextMesh letter;
    public int gridX, gridY;
    public Transform lineTarget;
    private AudioSource select;
    public static Letters over = null;

    void Start()
    {
        GetComponent<Renderer>().materials[0].color = SearchGenerator.Instance.defaultTint;
        lineTarget = this.gameObject.transform;
        select = SearchGenerator.Instance.selectSound;
    }

    void Update()
    {

        if (Input.GetMouseButtonUp(0))
        {
            utilized = false;
            if (GetComponent<Renderer>().materials[0].color != SearchGenerator.Instance.defaultTint)
            {
                GetComponent<Renderer>().materials[0].color = SearchGenerator.Instance.defaultTint;
            }
        }
    }
    void OnMouseEnter()
    {
        over = this;
        select.PlayOneShot(select.clip);
    }
    void OnMouseExit()
    {
        if (over == this)
        {
            over = null;
        }
    }
}
