using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : MonoBehaviour {

    public float speed;

	// Use this for initialization
	void Start () {
        

    }
	
	// Update is called once per frame
	void Update () {

        GetComponent<Rigidbody2D>().velocity = Vector2.up * speed;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Check if Bolt
        if (collider.gameObject.tag != "Bolt" && collider.gameObject.tag != "Ball" && collider.gameObject.tag != "Power")
        {
            MyDestroy();
        }
    }

    void MyDestroy()
    {
        Destroy(gameObject);
    }
}
