using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeIn : MonoBehaviour {

    Image rend;
	// Use this for initialization
	void Start () {
        rend = GetComponent<Image>();
        Color c = rend.material.color;
        c.a = 0f;
        rend.material.color = c;
        StartFading();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator FadeImageInAndOut()
    {
        for ( float f= 0f; f <= 1; f+= 0.01f)
        {
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return new WaitForSeconds(0.001f);
        }

        for (float f = 1f; f >= -0.01f; f -= 0.01f)
        {
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return new WaitForSeconds(0.001f);
        }
        SceneManager.LoadScene(1);

        //SceneManager.LoadScene(1);
        //StartCoroutine("FadeImageOut");
    }

    IEnumerator FadeImageIn()
    {
        for (float f = 0f; f <= 1; f += 0.02f)
        {
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator FadeImageOut()
    {
        for (float f = 1f; f >= -0.01f; f -= 0.01f)
        {
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return new WaitForSeconds(0.001f);
        }
        SceneManager.LoadScene(1);
        
    }

    public void StartFading()
    {
        StartCoroutine("FadeImageInAndOut");
    }
}
