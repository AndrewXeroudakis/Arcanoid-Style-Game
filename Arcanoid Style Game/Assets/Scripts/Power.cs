using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Power : MonoBehaviour {

    private int myPower;
    private float speed = 4;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    public Sprite slowSprite;
    public Sprite catchSprite;
    public Sprite dissolveSprite;
    public Sprite enlargeSprite;
    public Sprite paddleSprite;
    public Sprite laserSprite;
    public Sprite breakerSprite;
    public Sprite xplosionSprite;
    public Sprite minimizeSprite;
    public Sprite fastSprite;
    public Sprite reverseSprite;
    public Sprite adversitySprite;

    // Objects to find
    private PowerManager powerManager;
    private ScoreManager scoreManager;

    void Awake()
    {
        // Find Objects
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        scoreManager.powersTotal += 1;
        //Debug.Log("powersTotal = " + scoreManager.powersTotal);

        // Set Power
        //powerManager.currentPower = (PowerManager.Power)myPower; //PowerManager.Power.slow;
        //int[] numbers = new int[] { 11, 11};
        //int randomIndex = Random.Range(0, 2);
        //int randomIntFromNumbers = numbers[randomIndex]; //randomIntFromNumbers;
        //rb = GetComponent<Rigidbody2D>();
        //rb.velocity *= 0;
        myPower = powerManager.ChooseRandomPower(); //Random.Range(1, 12);

        // Set Animation
        //GetComponent<Animator>().SetInteger("PowerNumber", myPower);

        // Set Sprite
        sr = GetComponent<SpriteRenderer>();
        if (myPower == (int)PowerManager.Power.adversity) { sr.sprite = adversitySprite; }
        else if (myPower == (int)PowerManager.Power.reverse) { sr.sprite = reverseSprite; }
        else if (myPower == (int)PowerManager.Power.fast) { sr.sprite = fastSprite; }
        else if (myPower == (int)PowerManager.Power.minimize) { sr.sprite = minimizeSprite; }
        else if (myPower == (int)PowerManager.Power.xplosion) { sr.sprite = xplosionSprite; }
        else if (myPower == (int)PowerManager.Power.breaker) { sr.sprite = breakerSprite; }
        else if (myPower == (int)PowerManager.Power.laser) { sr.sprite = laserSprite; }
        else if (myPower == (int)PowerManager.Power.player) { sr.sprite = paddleSprite; }
        else if (myPower == (int)PowerManager.Power.enlarge) { sr.sprite = enlargeSprite; }
        else if (myPower == (int)PowerManager.Power.dissolve) { sr.sprite = dissolveSprite; }
        else if (myPower == (int)PowerManager.Power.catchPower) { sr.sprite = catchSprite; }
        else if (myPower == (int)PowerManager.Power.slow) { sr.sprite = slowSprite; }
    }

    void Update()
    {
        // set velocity
        transform.Translate(Vector3.down * Time.deltaTime * speed);
    }

    // Check Collision withPaddle
    void OnTriggerEnter2D(Collider2D other)
    {
        //Check collision name
        //Debug.Log("collision name = " + other.gameObject.name);
        if (other.gameObject.name == "Paddle")
        {
            PowerManager.Power newPower = (PowerManager.Power)myPower;
            powerManager.ReceivedPower(transform, newPower);
            //if (powerManager.currentPower == PowerManager.Power.laser) { Paddle paddle = GameObject.FindObjectOfType<Paddle>(); paddle.ResetLaser(); }

            MyDestroy();
        }
    }

    void MyDestroy()
    {
        Destroy(gameObject);
    }
}
