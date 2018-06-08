using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LoginScreenScript : MonoBehaviour {

	public void EnterWaittingRoom()
    {
        SceneManager.LoadScene(2);
    }
}
