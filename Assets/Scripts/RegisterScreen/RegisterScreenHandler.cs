using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterScreenHandler : MonoBehaviour {

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RegisButtonClick()
    {
        SceneManager.LoadScene(1);
    }
}
