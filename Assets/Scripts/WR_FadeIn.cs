using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WR_FadeIn : MonoBehaviour {

    Image rend;
    // Use this for initialization
    void Start()
    {
        rend = GetComponent<Image>();
        Color c = rend.material.color;
        c.a = 0f;
        rend.material.color = c;
        StartFading();
    }

    // Update is called once per frame
    void Update () {
		
	}

    IEnumerator FadeImageIn()
    {
        for (float f = 0f; f <= 1; f += 0.01f)
        {
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return new WaitForSeconds(0.001f);
        }
    }

    public void StartFading()
    {
        StartCoroutine("FadeImageIn");
    }

}
