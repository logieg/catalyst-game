using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Simple script to handle changing the text of a textbox (which is attached to this object as the first child)
public class MessageTextScript : MonoBehaviour
{
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = transform.GetChild(0).gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() { }

    public void SetText(string str)
    {
        text.text = str;
    }
}
