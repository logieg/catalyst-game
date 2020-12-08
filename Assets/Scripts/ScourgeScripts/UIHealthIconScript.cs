using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Handles the dynamic rendering of heart icons in the UI to represent the player's health (meant to be attached to the UI canvas object directly)
public class UIHealthIconScript : MonoBehaviour
{
    public GameObject player;
    public GameObject heartIconPrefab;
    public Sprite fullHeartIcon;
    public Sprite emptyHeartIcon;
    public int offsetX = 136,
        offsetY = -20;
    public int size = 30;

    private PlayerScript2 playerScript;
    private GameObject[] icons;
    private int cachedHealth,           // For performance, so that Update() doesn't change the sprite of the health icons every frame
        cachedMaxHealth;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = player.GetComponent<PlayerScript2>();
        CreateIcons();
    }

    // Update is called once per frame
    void Update()
    {
        // Check cached health values so we don't replace the sprites every frame
        if (cachedMaxHealth != playerScript.maxHealth)
            CreateIcons();
        if (cachedHealth != playerScript.GetHealth())
        {
            cachedHealth = playerScript.GetHealth();
            for (int i = 0; i < playerScript.maxHealth; i++)
            {
                // Update each icon to be full or empty based on the player's current health
                if (i < cachedHealth)
                    icons[i].GetComponent<Image>().sprite = fullHeartIcon;
                else
                    icons[i].GetComponent<Image>().sprite = emptyHeartIcon;
            }
        }
    }

    // Initialize the health icons based on the player's max health
    public void CreateIcons()
    {
        // If there are icons already in existence, destroy them
        if (icons != null && icons.Length > 0)
        {
            foreach (GameObject o in icons)
                Destroy(o);
        }

        // Create the heart icons and attach them as children of this canvas
        icons = new GameObject[playerScript.maxHealth];
        for (int i = 0; i < playerScript.maxHealth; i++)
        {
            icons[i] = Instantiate(heartIconPrefab, transform);
            RectTransform uiPosition = icons[i].GetComponent<RectTransform>();
            uiPosition.anchoredPosition = new Vector2(offsetX + (size + 6) * i, offsetY);
        }
        cachedMaxHealth = playerScript.maxHealth;
        cachedHealth = -50; // Force a refresh of the icons to ensure they accurately display the player's health
    }
}
