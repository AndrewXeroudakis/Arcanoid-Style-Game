using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    //public float imageSpeedDefault;
    //public Sprite[] sprites;
    // autoPlay: Is true when autoPlay is checked
    public Bolt bolt;
    public GameObject explosion;
    public bool autoPlay = false;
    // Instance of Ball
    private Ball ball;
    private PowerManager powerManager;
    private AudioManager audioManager;
    //private ScoreManager scoreManager;
    //private Animator myAnimator;
    private BoxCollider2D myCollider;
    //private float mouseXMin = 1f; //6.5f;
    //private float mouseXMax = 8f; //17.5f;
    //private float mouseNormalXMin = 1f; //6.5f;
    //private float mouseNormalXMax = 8f; //17.5f;
    public float gameWorldWidth = 0;
    public float paddleHalfWidth = 0;
    private float rightBorder = 0;
    public bool isOnBorder = false;
    private float touchYLimit = 4.25f;
    //private float mouseEnlargeXMin = 6.75f;
    //private float mouseEnlargeXMax = 17.25f;
    //private float mouseMinimizeXMin = 6.25f;
    //private float mouseMinimizeXMax = 17.75f;

    // Controls
    public enum Controls { clickStart = 0, clickHold, clickEnd };

    // Touch Controls
    public Vector2 touchOrigin = new Vector2(Screen.width / 2, 0); //-Vector2.one; // Offscreen point

    // Minimize - Black Hole
    private float pullRadius = 12;
    private float pullForce = 6000;

    // Laser
    bool shootingLaser = false;
    private float rateOfFire;
    private float rateOfFireX1 = 0.6f;
    private float rateOfFireX2 = 0.45f;
    private float rateOfFireX3 = 0.3f;
    bool overheat = false;
    private int overheatShootCounter = 6;
    private int overheatShootLimit = 6;
    private float overheatCooldown = 3f;

    //private float prevMousePos;
    //public float mouseSensitivity = 0.75f;

    private void Start()
    {
        ball = GameObject.FindObjectOfType<Ball>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        //scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        //myAnimator = GetComponent<Animator>();
        myCollider = GetComponent<BoxCollider2D>();
        paddleHalfWidth = (myCollider.size.x / 2) * transform.localScale.x;
        gameWorldWidth = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x;
        rightBorder = gameWorldWidth - paddleHalfWidth;
        //prevMousePos = Input.mousePosition.x;
        touchOrigin = new Vector2(Screen.width / 2, 0);
    }

    private void FixedUpdate()
    {
        // Black Hole
        if (powerManager.currentPower == PowerManager.Power.minimize && powerManager.powerStrength == PowerManager.Strength.x3)
        {
            Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, pullRadius);

            foreach (Collider2D obj in objects)
            {
                if (obj.tag == "Power" || obj.tag == "Ball")
                {
                    // calculate direction from target to me
                    Vector3 forceDirection = transform.position - obj.transform.position;

                    // apply force on target towards me
                    obj.GetComponent<Rigidbody2D>().AddForce(forceDirection.normalized / pullRadius * pullForce * Time.fixedDeltaTime);
                }
            }
        }
    }

    // Update
    void Update()
    {
        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        {
            MoveWithMouse();
                
        }
#else
        {
            if (powerManager.currentPower == PowerManager.Power.reverse && powerManager.powerStrength == PowerManager.Strength.x2) { MoveWithAccelerometer(); }
            else if (powerManager.currentPower == PowerManager.Power.reverse && powerManager.powerStrength == PowerManager.Strength.x3) { AutoPlay(); }
            else { MoveWithTouch(); }
        }
#endif
        
        
        // Change State - Enlarge / Minimize
        if (powerManager.currentPower == PowerManager.Power.enlarge)
        {
            // Check Strength and enlarge
            if (powerManager.powerStrength == PowerManager.Strength.x1)
            {
                transform.localScale = new Vector3(1.5f, 1, 0);
            }
            else if (powerManager.powerStrength == PowerManager.Strength.x2)
            {
                transform.localScale = new Vector3(2f, 1, 0);
            }
            else if (powerManager.powerStrength == PowerManager.Strength.x3)
            {
                transform.localScale = new Vector3(4f, 1, 0);
            }

            ResetCollider();
            paddleHalfWidth = (myCollider.size.x / 2) * transform.localScale.x;
            rightBorder = gameWorldWidth - paddleHalfWidth;

            /*
            // Set Animation
            myAnimator.SetTrigger("Enlarge");

            //Set Collider
            myCollider.size = new Vector2(2.5f, 0.44f);

            // Set Mouse Limits
            mouseXMin = mouseEnlargeXMin;
            mouseXMax = mouseEnlargeXMax;
            */

        }
        else if (powerManager.currentPower == PowerManager.Power.minimize)
        {
            // Check Strength and minimize
            if (powerManager.powerStrength == PowerManager.Strength.x1)
            {
                transform.localScale = new Vector3(0.75f, 1, 0);
            }
            else if (powerManager.powerStrength == PowerManager.Strength.x2)
            {
                transform.localScale = new Vector3(0.5f, 1, 0);
            }
            else if (powerManager.powerStrength == PowerManager.Strength.x3)
            {
                transform.localScale = new Vector3(0.2f, 1, 0);
            }

            ResetCollider();
            paddleHalfWidth = (myCollider.size.x / 2) * transform.localScale.x;
            rightBorder = gameWorldWidth - paddleHalfWidth;

            /*
            // Set Animation
            myAnimator.SetTrigger("Minimize");

            //Set Collider
            myCollider.size = new Vector2(1.5f, 0.44f);

            // Set Mouse Limits
            mouseXMin = mouseMinimizeXMin;
            mouseXMax = mouseMinimizeXMax;
            */
        }
        else
        {
            // Set size
            if (transform.localScale.x != 1 )
            {
                transform.localScale = new Vector3(1, 1, 0);

                ResetCollider();
                paddleHalfWidth = (myCollider.size.x / 2) * transform.localScale.x;
                rightBorder = gameWorldWidth - paddleHalfWidth;
            }
            
            // Set Animation
            //myAnimator.SetTrigger("Normal");

            //Set Collider
            //myCollider.size = new Vector2 (2f, 0.44f);

            // Set Mouse Limits
            //mouseXMin = mouseNormalXMin;
            //mouseXMax = mouseNormalXMax;
        }

        // Change State - Laser
        if (powerManager.currentPower == PowerManager.Power.laser)
        {
            // Check for mouse input
            if (PlayerInput(Controls.clickHold) && overheat == false) //Input.GetMouseButton(0)
            {
                // Check Strength and set Rate Of Fire
                if (powerManager.powerStrength == PowerManager.Strength.x1)
                {
                    rateOfFire = rateOfFireX1;
                }
                else if (powerManager.powerStrength == PowerManager.Strength.x2)
                {
                    rateOfFire = rateOfFireX2;
                }
                else if (powerManager.powerStrength == PowerManager.Strength.x3)
                {
                    rateOfFire = rateOfFireX3;
                }

                if (!shootingLaser && ball.hasStarted) { InvokeRepeating("ShootLaser", 0, rateOfFire); shootingLaser = true; }
            }
            else
            {
                ResetLaser();
            }

            if (PlayerInput(Controls.clickEnd) && !overheat) //Input.GetMouseButtonUp(0)
            {
                if (overheatShootCounter == overheatShootLimit) { Invoke("ResetOverheat", 1); }
                overheatShootCounter--;
                if (overheatShootCounter <= 0) { overheat = true; CancelInvoke("ResetOverheat"); Invoke("ResetOverheat", overheatCooldown); }
                //Debug.Log(overheatShootCounter);
            }
        }
        else
        {
            if (shootingLaser)
            {
                ResetLaser();
            }
        }

        // Change State - Xplosion
        if (powerManager.currentPower == PowerManager.Power.xplosion)
        {
            // destroy unbreakables
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Unbreakable"))
            {
                Vector3 center = (obj.GetComponent<SpriteRenderer>().bounds.center) / 18;
                BrickZero brick = obj.GetComponent<BrickZero>();
                if (brick != null)
                {
                    Instantiate(explosion, obj.transform.position + center, obj.transform.rotation);
                    brick.MyDestroy();
                }
            }

            // destroy breakables
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Breakable"))
            {
                Vector3 center = (obj.GetComponent<SpriteRenderer>().bounds.center) / 18;
                BrickZero brick = obj.GetComponent<BrickZero>();
                if (brick != null)
                {
                    Instantiate(explosion, obj.transform.position + center, obj.transform.rotation);
                    brick.MyDestroy();
                }
            }
            
            powerManager.ResetPower();
        }
    }

    void MoveWithMouse()
    {
        // Instanciate Vector3 to modify the new position values
        Vector3 paddlePos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        // Set x pos to mouse x
        if (powerManager.currentPower == PowerManager.Power.reverse)
        {
            float mousePosInBlocks = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - gameWorldWidth;
            paddlePos.x = Mathf.Clamp(-mousePosInBlocks, paddleHalfWidth, rightBorder);
        }
        else
        {
            float mousePosInBlocks = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            paddlePos.x = Mathf.Clamp(mousePosInBlocks, paddleHalfWidth, rightBorder);
        }

        // check borders
        if (paddlePos.x <= paddleHalfWidth || paddlePos.x >= rightBorder) { isOnBorder = true; }
        else { isOnBorder = false; }

        // Set position
        this.transform.position = Vector2.Lerp(transform.position, paddlePos, 0.20f);

        /*
        float mousePos = Input.mousePosition.x;
        float movement = (powerManager.currentPower == PowerManager.Power.reverse ? -1 : 1) * (mousePos - prevMousePos) * mouseSensitivity * Time.deltaTime;
        paddlePos.x += movement;
        paddlePos.x = Mathf.Clamp(paddlePos.x, mouseXMin, mouseXMax);
        prevMousePos = mousePos;
        */

        // Set position
        //this.transform.position = paddlePos;
    }

    void MoveWithTouch()
    {
        // Instanciate Vector3 to modify the new position values
        Vector3 paddlePos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);

        //set touchOrigin
        if (Input.touchCount > 0)
        {
            Touch myTouch = Input.touches[0];

            if (Camera.main.ScreenToWorldPoint(myTouch.position).y <= touchYLimit)
            {
                if (myTouch.phase != TouchPhase.Ended && myTouch.phase != TouchPhase.Canceled) //(myTouch.phase == TouchPhase.Began)
                                                                                               //if (myTouch.phase == TouchPhase.Ended)
                {
                    touchOrigin = myTouch.position;
                }
            }
        }

        // Set x pos to touch x
        if (powerManager.currentPower == PowerManager.Power.reverse)
        {
            float mousePosInBlocks = Camera.main.ScreenToWorldPoint(touchOrigin).x - gameWorldWidth;
            paddlePos.x = Mathf.Clamp(-mousePosInBlocks, paddleHalfWidth, rightBorder);
        }
        else
        {
            float mousePosInBlocks = Camera.main.ScreenToWorldPoint(touchOrigin).x;
            paddlePos.x = Mathf.Clamp(mousePosInBlocks, paddleHalfWidth, rightBorder);
        }

        // check borders
        if (paddlePos.x <= paddleHalfWidth || paddlePos.x >= rightBorder) { isOnBorder = true; }
        else { isOnBorder = false; }

        // Set position
        this.transform.position = Vector2.Lerp(transform.position, paddlePos, 0.20f); // 0.70f

        /*
        float mousePos = Input.mousePosition.x;
        float movement = (powerManager.currentPower == PowerManager.Power.reverse ? -1 : 1) * (mousePos - prevMousePos) * mouseSensitivity * Time.deltaTime;
        paddlePos.x += movement;
        paddlePos.x = Mathf.Clamp(paddlePos.x, mouseXMin, mouseXMax);
        prevMousePos = mousePos;
        */

        // Set position
        //this.transform.position = paddlePos;
        //this.transform.position = Vector2.Lerp(transform.position, paddlePos, 0.35f);
    }

    void MoveWithAccelerometer()
    {
        // Get accelerometer values
        float accelX = Input.acceleration.x;

        // Instanciate Vector3 to modify the new position values
        Vector3 paddlePos = new Vector3(Mathf.Clamp(this.transform.position.x + accelX, paddleHalfWidth, rightBorder), this.transform.position.y, this.transform.position.z);
        
        // check borders
        if (paddlePos.x <= paddleHalfWidth || paddlePos.x >= rightBorder) { isOnBorder = true; }
        else { isOnBorder = false; }

        // Set position
        this.transform.position = Vector2.Lerp(transform.position, paddlePos, 0.45f);
    }

    void AutoPlay()
    {
        Vector3 paddlePos = new Vector3(Mathf.Clamp(ball.transform.position.x, paddleHalfWidth, rightBorder), this.transform.position.y, this.transform.position.z);
        this.transform.position = Vector2.Lerp(transform.position, paddlePos, 0.80f);

        /*Vector3 paddlePos = new Vector3(0.5f, this.transform.position.y, 0f);
        Vector3 ballPos = ball.transform.position;
        paddlePos.x = Mathf.Clamp(ballPos.x, 0.5f, 15.5f);
        this.transform.position = paddlePos;*/
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ball.hasStarted) { audioManager.PlaySoundAtPoint("HitPaddle", transform.position); }
    }

    public void SetBall(Ball newBall)
    {
        ball = newBall;
    }

    void ShootLaser()
    {
        // Check Strength
        if (powerManager.powerStrength == PowerManager.Strength.x1)
        {
            Vector3 offset = new Vector3(0, 0.75f, 0f);
            Instantiate(bolt, transform.position + offset, transform.rotation);
        }
        else if (powerManager.powerStrength == PowerManager.Strength.x2)
        {
            Vector3 offset1 = new Vector3(0.45f, 0.75f, 0f);
            Vector3 offset2 = new Vector3(-0.45f, 0.75f, 0f);
            Instantiate(bolt, transform.position + offset1, transform.rotation);
            Instantiate(bolt, transform.position + offset2, transform.rotation);
        }
        else if (powerManager.powerStrength == PowerManager.Strength.x3)
        {
            Vector3 offset1 = new Vector3(0.25f, 0.75f, 0f);
            Vector3 offset2 = new Vector3(-0.25f, 0.75f, 0f);
            Vector3 offset3 = new Vector3(0.55f, 0.75f, 0f);
            Vector3 offset4 = new Vector3(-0.55f, 0.75f, 0f);
            Instantiate(bolt, transform.position + offset1, transform.rotation);
            Instantiate(bolt, transform.position + offset2, transform.rotation);
            Instantiate(bolt, transform.position + offset3, transform.rotation);
            Instantiate(bolt, transform.position + offset4, transform.rotation);
        }
    }

    public void ResetLaser()
    {
        CancelInvoke("ShootLaser");
        shootingLaser = false;
        rateOfFire = rateOfFireX1;
    }

    public void DestroyBolts()
    {
        // destroy previous bolts
        foreach (GameObject bolt in GameObject.FindGameObjectsWithTag("Bolt"))
        {
            Destroy(bolt);
        }
    }

    void ResetOverheat()
    {
        overheatShootCounter = overheatShootLimit;
        if (overheat) { overheat = false; }
    }

    public bool PlayerInput(Controls input)
    {
        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        {
            switch (input)
            {
                case Controls.clickStart:
                    {
                        return Input.GetMouseButtonDown(0);
                    };
                case Controls.clickHold:
                    {
                        return Input.GetMouseButton(0);
                    };
                case Controls.clickEnd:
                    {
                        return Input.GetMouseButtonUp(0);
                    };
            }
        }
#else
        {

            if (Input.touchCount > 0)
            {
                Touch myTouch = Input.touches[0];

                if (Camera.main.ScreenToWorldPoint(myTouch.position).y <= touchYLimit)
                {
                    switch (input)
                    {
                        case Controls.clickStart:
                            {
                                //return myTouch.phase == TouchPhase.Began;
                                //return myTouch.phase != TouchPhase.Ended && myTouch.phase != TouchPhase.Canceled;
                                return myTouch.phase == TouchPhase.Ended;
                            };
                        case Controls.clickHold:
                            {
                                return myTouch.phase != TouchPhase.Ended && myTouch.phase != TouchPhase.Canceled;
                            };
                        case Controls.clickEnd:
                            {
                                return myTouch.phase == TouchPhase.Ended;
                            };
                    }
                }
            }
        }
#endif

        return false;
    }

    IEnumerator ResetCollider()
    {
        Destroy(myCollider);
        yield return 0;
        myCollider = gameObject.AddComponent<BoxCollider2D>() as BoxCollider2D;
    }

    
}
