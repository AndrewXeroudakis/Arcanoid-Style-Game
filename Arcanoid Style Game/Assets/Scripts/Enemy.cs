using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public GameObject power;
    private bool createPower = true;
    private int dead = -1;
    private bool changeDirectionMidAir = true;
    private float speed = 75f;
    private Vector2 dir = Vector2.down; //new Vector2(0, 0); //Vector2.down;
    private Rigidbody2D rb;
    private EnemySpawner enemySpawner;
    private PowerManager powerManager;
    List<GameObject> currentBrickCollisions = new List<GameObject>();
    Vector2 ceilingX = new Vector2(0, 0);
    private float distInUnits = 0.5f;
    private float offset;

    void Start () {
        rb = GetComponent<Rigidbody2D>();
        enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        offset = 13.7f % distInUnits;
    }
	
	void FixedUpdate () {
        
        // move
        rb.velocity = dir * speed * Time.fixedDeltaTime;
	}

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            GetComponent<Collider2D>().isTrigger = false;
            dead = 3;
        }
        else if (collision.gameObject.tag == "Border")
        {
            dir = dir * -1;
        }
        else if (collision.gameObject.tag == "Bolt")
        {
            MyDestroy();
        }
        else if (collision.gameObject.tag == "Paddle")
        {
            createPower = false;

            // push paddle
            //Vector2 heading = collision.transform.position - this.transform.position;
            //collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2((heading.normalized.x > 0) ? 1 : -1, 0) * 200f * Time.fixedDeltaTime);

            MyDestroy();
        }
        else if (collision.gameObject.tag == "Breakable" || collision.gameObject.tag == "Unbreakable")
        {
            // stop midAir movement coroutine
            changeDirectionMidAir = false;
            StopCoroutine(WaitToAirMove());

            // check if above enemy
            Vector2 heading = collision.transform.position - this.transform.position;
            if (heading.normalized.y < 0.75)
            {
                currentBrickCollisions.Add(collision.gameObject);
                StartCoroutine(WaitToMove());
                //Debug.Log("New Brick Collision " + currentBrickCollisions.Count);
            }
            else
            {
                dir = Vector2.down;
                currentBrickCollisions.Clear();
                ceilingX = new Vector2(heading.normalized.x, 0);
            }
            
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        // stop midAir movement coroutine
        changeDirectionMidAir = false;
        //StopCoroutine(WaitToAirMove());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Breakable" || collision.gameObject.tag == "Unbreakable")
        {
            currentBrickCollisions.Remove(collision.gameObject);
            StartCoroutine(WaitToMove());
        }
        
    }

    /*void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            MyDestroy();
        }
    }*/

    void ChooseDirection(GameObject collision)
    {
        Vector2 heading = collision.transform.position - this.transform.position;

        if (heading.normalized.y < 0)
        {
            if (heading.normalized.x == 0f) // || dir == Vector2.down)
            {
                int[] numbers = new int[] { -1, 1 };
                int randomIndex = Random.Range(0, 2);
                int randomIntFromNumbers = numbers[randomIndex];
                dir = new Vector2(randomIntFromNumbers, 0);
            }
            else
            {
                if (ceilingX.normalized.x != 0) { dir = new Vector2((ceilingX.normalized.x > 0) ? 1 : -1, 0); ceilingX = new Vector2(0, 0); }
                else { dir = new Vector2((heading.normalized.x > 0) ? 1 : -1, 0); }
            }
        }
        else
        {
            dir = Vector2.up;
        }
        
        //Debug.Log("Current dir " + dir);
        
    }

    IEnumerator WaitToMove()
    {
        yield return new WaitForFixedUpdate();
        
        //Debug.Log("Current Collisions " + currentBrickCollisions.Count);

        if (currentBrickCollisions.Count > 0)// && (setDirection)
        {
            changeDirectionMidAir = false;
            StopCoroutine(WaitToAirMove());

            if (currentBrickCollisions.Count > 1) { currentBrickCollisions.Sort((b, a) => a.transform.position.y.CompareTo(b.transform.position.y)); /*Debug.Log("Multiple Collisions " + currentBrickCollisions.Count);*/ }

            ChooseDirection(currentBrickCollisions[0]);
        }
        else
        {
            //Debug.Log("No Collisions " + currentBrickCollisions.Count);
            dir = Vector2.down;
            StopCoroutine(WaitToAirMove());
            StartCoroutine(WaitToAirMove());
        }
    }

    IEnumerator WaitToAirMove()
    {
        yield return new WaitForSeconds(Random.Range(2f, 4f));
        yield return new WaitForFixedUpdate();
        
        changeDirectionMidAir = true;
    }

    void Update()
    {
        if (changeDirectionMidAir == true 
            && (transform.position.y % distInUnits >= offset && transform.position.y % distInUnits < offset + 0.01)
            && (transform.position.y > 3.75f)
            && (transform.position.y <= 11.5f))
        {
            if (dir == Vector2.down)
            {
                Vector2[] directions = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1) };
                dir = directions[Random.Range(0, directions.Length)];
            }
            else
            {
                dir = Vector2.down;
            }

            changeDirectionMidAir = false;
            StopCoroutine(WaitToAirMove());
            StartCoroutine(WaitToAirMove());
            
            //Debug.Log("Position = " + transform.position.y);
        }

        if (dead > 0)
        {
            dead -= 1;
        }
        else if (dead == 0)
        {
            MyDestroy();
        }
    }

    public void MyDestroy()
    {
        if (createPower)
        {
            if (powerManager.ChooseRandomDrop() == 1)
            {
                //if(powerManager.powerStrength != PowerManager.Strength.x3)
                Instantiate(power, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), transform.rotation); //transform.position + center
                                                                            // Reset weight
                powerManager.CancelInvoke("Countdown");
                powerManager.dropWeights[PowerManager.Drops.dropPower] = powerManager.dropPowerWeight;
                powerManager.startTimer = true;
                //Debug.Log(powerManager.dropWeights[PowerManager.Drops.dropPower]);
            }
        }
        
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        changeDirectionMidAir = false;
        StopCoroutine(WaitToMove());
        StopCoroutine(WaitToAirMove());

        if (enemySpawner.spawning == false)
        {
            enemySpawner.StopCoroutine(enemySpawner.SpawnEnemies());
            enemySpawner.StartCoroutine(enemySpawner.SpawnEnemies());
            enemySpawner.spawning = true;
        }
    }
}
