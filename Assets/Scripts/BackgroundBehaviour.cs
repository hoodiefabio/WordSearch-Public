using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundBehaviour : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    public float fallSpeed = 100f;

    void Update()
    {
        //makes letters fall
        transform.position = new Vector3(transform.position.x, transform.position.y - (fallSpeed * Time.deltaTime), transform.position.z);
        //slowely rotates letters
        transform.Rotate(0.0f, 0.0f, 0.1f, Space.World);

        //loops letters falling
        if(transform.position.y < -(Screen.height/2))
        {
            transform.position = new Vector3(transform.position.x, (Screen.height + Random.Range(100,200)), transform.position.z);
        }
    }

}
