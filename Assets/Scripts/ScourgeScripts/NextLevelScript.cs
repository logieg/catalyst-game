using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Simple script for loading the next level
public class NextLevelScript : MonoBehaviour
{
    // The name of the level to load
    public string levelName;
    public GameObject staticSaver;       // To reset the player's spawn position for the next level

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Transitioning level to: " + levelName);
            staticSaver.GetComponent<StaticSaverScript>().ChangeSpawn(Vector3.zero);
            SceneManager.LoadScene(levelName);
        }
    }
}
