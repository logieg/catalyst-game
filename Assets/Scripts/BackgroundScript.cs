using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to handle background behavior like parallax
public class BackgroundScript : MonoBehaviour
{
    public Camera followCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = new Vector3(followCamera.transform.position.x, followCamera.transform.position.y, transform.position.z);
    }
}
