using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Very brief animation script that uses material texture offset to animate through a "spritesheet" for the sword swipe
public class SwordSwipeAnimation : MonoBehaviour
{
    private Material mat;
    public float frameTime = 0.05f; // The amount of delay between animation frames

    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
        StartCoroutine(Animate());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Animate()
    {
        yield return new WaitForSeconds(frameTime);
        mat.mainTextureOffset = new Vector2(0, 0.5f);
        yield return new WaitForSeconds(frameTime);
        mat.mainTextureOffset = new Vector2(0, 0.25f);
        yield return new WaitForSeconds(frameTime);
        mat.mainTextureOffset = new Vector2(0, 0.0f);
        yield return new WaitForSeconds(frameTime);
        Destroy(gameObject);
    }
}
