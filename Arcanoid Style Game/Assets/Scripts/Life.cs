using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Life : MonoBehaviour {

    public int thisLife;

    private BorderBottom bb;
    private SpriteRenderer sR;

    // Use this for initialization
    void Start () {
        bb = GameObject.FindObjectOfType<BorderBottom>();
        sR = GetComponent<SpriteRenderer>();
    }
	
	// Update is called once per frame
	void Update () {

        if (bb.lives < thisLife)
        {
            sR.enabled = false;
        }
        else
        {
            sR.enabled = true;
        }
	}
}
