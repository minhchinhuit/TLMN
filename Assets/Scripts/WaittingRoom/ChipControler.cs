using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChipControler : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.back *10* Random.Range(0.0f, 0.05f));
    }
}
