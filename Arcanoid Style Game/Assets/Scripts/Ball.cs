
//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour
{

    //public AudioClip HitBrick;
    //public float speed = 16;
    public bool hasStarted = false;
    private bool r = true;
    private float ballToPaddle = 4.05f;

    // Speed variables
    public float currentSpeedMultiplier;
    public float slowSpeedMultiplier1 = 8f; //8.5f; 
    public float slowSpeedMultiplier2 = 7f; //7.5f;
    public float slowSpeedMultiplier3 = 6f; //5.5f;
    public float normalSpeedMultiplier = 9f;
    public float fastSpeedMultiplier1 = 10f; //9.5f; 
    public float fastSpeedMultiplier2 = 11f; //10.5f; 
    public float fastSpeedMultiplier3 = 12f; //12.5f;

    // Curve Ball
    public bool isCurveBall = false;
    float curveForce = 0;

    // Dissolve
    public Ball ball;
    private bool dis;

    
    private PowerManager powerManager;
    private ScoreManager scoreManager;
    private Paddle paddle;
    //private Vector3 paddleToBallVector;
    //private CircleCollider2D myCollider;

    // Start timer
    public Text startTimerText;
    private bool startTimer = false;
    private int startTimerSeconds = 4;

    // Catch
    private Vector3 paddleToBallVectorCatch;
    private Vector2 dir;
    private bool catchActive = false;

    // Dissolve
    private float dissolveOffset;
    private float dissolveInitialAngle = 25f;
    private int dissolveNext = 1;
    private float dissolveNextAngle;

    // Adversity
    private float adversityX3SpeedMultiplier = 12; //13;

    //Reverse
    private int bounceTimer = 0;
    private int bounceTimerDefault = 3;

    // Children
    GameObject fastBall;

    // Set Rigidbody to rb
    private Rigidbody2D rb;
    //private float fastBallOffset = 0.2f;

    public Vector2 CurrentDirection { get { return rb.velocity.normalized; } }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //myCollider = GetComponent<CircleCollider2D>();
        dissolveOffset = GetComponent<CircleCollider2D>().radius * 4;
        dissolveNextAngle = dissolveInitialAngle;
        fastBall = GameObject.Find("Fast Ball");

        // Find Objects
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        paddle = GameObject.FindObjectOfType<Paddle>();

        // Set paddleToBallVector
        //paddleToBallVector = new Vector3(4.5f, 2.8f, -2); //new Vector3(12, 2, -2) - new Vector3(12, 1.5f, -2); // this.transform.position - paddle.transform.position;
        
        ResetBall();
    }

    // Use this for initialization
    void Start()
    {
        // Set velocity
        //rb.velocity = Vector2.up * speed;
        //ResetBall();
    }

    public void ResetBall()
    {
        hasStarted = false;
        startTimer = true;
        currentSpeedMultiplier = normalSpeedMultiplier;
        rb.velocity *= 0;
        Physics2D.IgnoreLayerCollision(8, 9, false);
        r = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasStarted)
        {
            // Set velocity to 0
            //rb.velocity *= 0;

            // Follow paddle
            //this.transform.position = paddle.transform.position + paddleToBallVector;
            if (r)
            {
                this.transform.position = new Vector3(paddle.transform.position.x, ballToPaddle, -2);
                GetComponent<RelativeJoint2D>().enabled = true;

                r = false;
            }
            this.transform.position = new Vector3(Mathf.Clamp(this.transform.position.x, paddle.transform.position.x - paddle.paddleHalfWidth, paddle.transform.position.x + paddle.paddleHalfWidth), ballToPaddle, -2);

            // Set timer
            if (startTimer)
            {
                startTimerSeconds = 4;
                InvokeRepeating("StartTimerCountdown", 1, 1);
                SetStartTimerText();
                startTimer = false;
            }

            // Check for mouse input
            if ((paddle.PlayerInput(Paddle.Controls.clickStart)) || startTimerSeconds <= 0)  //Input.GetMouseButton(0)
            {
                // destroy joint
                GetComponent<RelativeJoint2D>().enabled = false;

                // shoot ball
                startTimerSeconds = 0;
                SetStartTimerText();
                ShootBall(new Vector2(HitFactor(transform.position, paddle.transform.position, paddle.paddleHalfWidth*2), 1).normalized);

                // reapply power
                if (powerManager.currentPower == PowerManager.Power.dissolve
                || powerManager.currentPower == PowerManager.Power.fast
                || powerManager.currentPower == PowerManager.Power.slow)
                { powerManager.ApplyPower(); }

                if (powerManager.currentPower == PowerManager.Power.catchPower)
                {
                    powerManager.ApplyPower();
                    float x = HitFactor(transform.position, paddle.transform.position, paddle.paddleHalfWidth * 2);
                    dir = new Vector2(x, 1).normalized;
                    paddleToBallVectorCatch = new Vector3(this.transform.position.x, ballToPaddle, -2) - paddle.transform.position;
                    catchActive = true;
                }
            }
        }


        // Change State - Catch
        if (powerManager.currentPower == PowerManager.Power.catchPower && catchActive)
        {
            // Set movement to 0
            currentSpeedMultiplier = 0;

            // Follow paddle
            this.transform.position = paddle.transform.position + paddleToBallVectorCatch;

            // Check for mouse input
            if (paddle.PlayerInput(Paddle.Controls.clickStart)) //Input.GetMouseButton(0)
            {
                CatchPowerShootBall();
            }
        }

    }

    void FixedUpdate()
    {
        if (hasStarted == true && powerManager.currentPower == PowerManager.Power.reverse && powerManager.powerStrength == PowerManager.Strength.x3)
        {
            if (bounceTimer <= 0)
            {
                bounceTimer = 0;
                rb.velocity = new Vector2(Input.acceleration.x, -Input.acceleration.z) * currentSpeedMultiplier;
            }
            else
            {
                bounceTimer -= 1;
            }
        }
        else
        {
            // fix horizontal movement
            Vector2 fixHor = rb.velocity.normalized;
            float ang = 0.5f;

            if ((rb.velocity.normalized.y > 0 && rb.velocity.normalized.x > 0) || (rb.velocity.normalized.y > 0 && rb.velocity.normalized.x < 0)) { fixHor = new Vector2(rb.velocity.normalized.x, Mathf.Clamp(rb.velocity.normalized.y, ang, 1f)); }
            else if ((rb.velocity.normalized.y <= 0 && rb.velocity.normalized.x >= 0) || (rb.velocity.normalized.y <= 0 && rb.velocity.normalized.x <= 0)) { fixHor = new Vector2(rb.velocity.normalized.x, Mathf.Clamp(rb.velocity.normalized.y, -1f, -ang)); }

            // set velocity
            if (hasStarted && powerManager.currentPower != PowerManager.Power.adversity) { rb.velocity = fixHor * currentSpeedMultiplier; } //rb.velocity.normalized * currentSpeedMultiplier;

            if (hasStarted && powerManager.currentPower == PowerManager.Power.adversity && powerManager.powerStrength == PowerManager.Strength.x3) { rb.velocity = fixHor * adversityX3SpeedMultiplier; } //rb.velocity.normalized * adversityX3SpeedMultiplier;

            // add curve
            if (isCurveBall) { rb.AddForce(new Vector2(curveForce, 0)); }
        }
    }

    public void UpdateSpeed(PowerManager.Power currentPower, PowerManager.Strength powerStrength)
    {
        // Change State - Slow / Fast
        if (currentPower == PowerManager.Power.slow)
        {
            if (powerStrength == PowerManager.Strength.x1)
                currentSpeedMultiplier = slowSpeedMultiplier1;
            else if (powerStrength == PowerManager.Strength.x2)
                currentSpeedMultiplier = slowSpeedMultiplier2;
            else if (powerStrength == PowerManager.Strength.x3)
                currentSpeedMultiplier = slowSpeedMultiplier3;
        }
        else if (currentPower == PowerManager.Power.fast)
        {
            if (powerStrength == PowerManager.Strength.x1)
                currentSpeedMultiplier = fastSpeedMultiplier1;
            else if (powerStrength == PowerManager.Strength.x2)
                currentSpeedMultiplier = fastSpeedMultiplier2;
            else if (powerStrength == PowerManager.Strength.x3)
            {
                currentSpeedMultiplier = fastSpeedMultiplier3;
            }
        }
        else
        {
            currentSpeedMultiplier = normalSpeedMultiplier;
        }

        rb.velocity = rb.velocity.normalized * currentSpeedMultiplier;

        //Debug.Log(rb.velocity + " " + rb.velocity.magnitude);
    }

    public void UpdateSpeed(Vector2 direction)
    {
        rb.velocity = direction * currentSpeedMultiplier;
    }

    public void ShootBall(Vector2 direction)
    {
        rb.velocity = direction * currentSpeedMultiplier;
        CancelInvoke("StartTimerCountdown");
        //SetStartTimerText();
        hasStarted = true;
    }

    public void SetStartTimerText()
    {
        //startTimerText.text = (Mathf.Floor(startTimeLeft / 60)).ToString("00") + ":" + (Mathf.RoundToInt(startTimeLeft % 60)).ToString("00");
        if (startTimerSeconds > 1) { startTimerText.text = (startTimerSeconds - 1).ToString(); }
        else if (startTimerSeconds == 1) { startTimerText.text = "break !"; }
        else if (startTimerSeconds <= 0) { startTimerText.text = ""; }
    }

        public void CatchPowerShootBall()
    {
        currentSpeedMultiplier = normalSpeedMultiplier;
        rb.velocity = dir * currentSpeedMultiplier;
        catchActive = false;
    }

    // On ANY collision
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (hasStarted)
        {
            bounceTimer = bounceTimerDefault;

            // Check if Paddle
            if (collision.gameObject.name == "Paddle")
            {
                // Reset Score Multiplier
                /*if (powerManager.currentPower != PowerManager.Power.minimize &&
                    powerManager.currentPower != PowerManager.Power.fast &&
                    powerManager.currentPower != PowerManager.Power.reverse &&
                    powerManager.currentPower != PowerManager.Power.adversity)
                { scoreManager.scoreMultiplier = 1; }
                else { scoreManager.ResetMultiplierPowerDown(scoreManager.SetCurrentScoreMultiplierPercentageReturn()); }
                scoreManager.SetMultiplierText();*/

                // set makeEveryShotCount
                scoreManager.CheckReturnToPaddle();

                //Reset Ball Hits/Check for Achievement
                scoreManager.ResetScoreHits();

                // Calculate Hit Factor
                float x = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.x);

                // Calculate direction, set length to 1
                dir = new Vector2(x, 1).normalized;

                // Set Velocity
                if (powerManager.currentPower == PowerManager.Power.catchPower)
                {
                    rb.velocity = dir * 0;
                    paddleToBallVectorCatch = new Vector3(this.transform.position.x, ballToPaddle, -2) - paddle.transform.position;
                    catchActive = true;
                }
                else
                {
                    rb.velocity = dir * currentSpeedMultiplier;

                    // enable curve
                    if (paddle.isOnBorder == false)
                    {
                        float targetDist = 0;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
                        {
                            if (powerManager.currentPower == PowerManager.Power.reverse) { targetDist = -1 * (Camera.main.ScreenToWorldPoint(Input.mousePosition).x + paddle.transform.position.x - paddle.gameWorldWidth); }
                            else { targetDist = Camera.main.ScreenToWorldPoint(Input.mousePosition).x - paddle.transform.position.x; }
                                
                            if (Mathf.Abs(targetDist) >= 0.45f)
                            { curveForce = (targetDist) * currentSpeedMultiplier; isCurveBall = true; };
                            
                        }
#else
                    {
                        if (powerManager.currentPower == PowerManager.Power.reverse)
                        {
                            if (powerManager.powerStrength == PowerManager.Strength.x1)
                            {
                                targetDist = -1 * (Camera.main.ScreenToWorldPoint(paddle.touchOrigin).x + paddle.transform.position.x - paddle.gameWorldWidth);
                            }
                            else if (powerManager.powerStrength == PowerManager.Strength.x2)
                            {
                                targetDist = Input.acceleration.x;
                            }
                        }
                        else { targetDist = Camera.main.ScreenToWorldPoint(paddle.touchOrigin).x - paddle.transform.position.x; }

                        if (Mathf.Abs(targetDist) >= 0.225f)
                        { curveForce = (targetDist) * currentSpeedMultiplier; isCurveBall = true; };
                    }
#endif
                    }

                    //Debug.Log("Curve" + isCurveBall);
                    //Debug.Log(targetDist);
                    //Debug.Log(rb.velocity.normalized);
                }
            }
            else
            {
                isCurveBall = false;
            }

            // Fix movement "bug" on border collision
            if (collision.gameObject.tag == "Border")
            {
                // Vertical movement fix
                if (rb.velocity.normalized == new Vector2(0, 1) || rb.velocity.normalized == new Vector2(0, -1))
                {
                    Debug.Log("Vertical Fix");
                    float yDistance = rb.position.y - transform.position.y;
                    float border = Mathf.Sign(this.transform.position.x - collision.transform.position.x);
                    rb.velocity = new Vector2(border * 1f, rb.velocity.y + yDistance + Mathf.Sign(yDistance));
                }

                // Horizontal movement fix
                //if (rb.velocity.normalized == new Vector2(1, 0) || rb.velocity.normalized == new Vector2(-1, 0))
                /*{

                    Debug.Log("Horizontal Fix");
                    float xDistance = rb.position.x - transform.position.x;
                    rb.velocity = new Vector2(rb.velocity.x + xDistance + Mathf.Sign(xDistance), -1.5f);
                }*/
            }

            // Adversity
            if (powerManager.currentPower == PowerManager.Power.adversity)
            {
                if (powerManager.powerStrength == PowerManager.Strength.x1)
                {
                    float randomAngle = Random.Range(-25f, 25f);
                    float randomSpeedMultiplier = Random.Range(0.75f, 1.15f);
                    
                    Vector2 dir = rb.velocity.normalized;
                    Vector2 newDir = Quaternion.AngleAxis(randomAngle, Vector3.forward) * (Vector3)dir;
                    rb.velocity = newDir * currentSpeedMultiplier * randomSpeedMultiplier;
                }
                else if (powerManager.powerStrength == PowerManager.Strength.x2)
                {
                    float randomAngle = Random.Range(-50f, 50f);
                    float randomSpeedMultiplier = Random.Range(0.75f, 1.25f);

                    Vector2 dir = rb.velocity.normalized;
                    Vector2 newDir = Quaternion.AngleAxis(randomAngle, Vector3.forward) * (Vector3)dir;
                    rb.velocity = newDir * currentSpeedMultiplier * randomSpeedMultiplier;
                }
                else if (powerManager.powerStrength == PowerManager.Strength.x3)
                {
                    GameObject brick = GameObject.FindWithTag("Breakable");
                    if (brick != null)
                    {
                        // Check if Unbreakable Brick is on the way
                        bool free = true;
                        RaycastHit2D hit = Physics2D.Raycast(this.transform.position, brick.GetComponent<Renderer>().bounds.center);
                        GameObject hitted = hit.collider.gameObject;
                        if (hitted.tag == "Unbreakable") // (hitted is UnbreakableBrick) *ALSO NOT //(hitted.GetType() == typeof(UnbreakableBrick)) *NOT WORKING
                        {
                            free = false;
                        }

                        // Set new Vector
                        if (free)
                        {
                            Vector2 brickDir = (brick.GetComponent<Renderer>().bounds.center - this.transform.position).normalized;
                            rb.velocity = brickDir * adversityX3SpeedMultiplier;
                        }
                        else
                        {
                            // Change brick target
                            GameObject[] bricks = GameObject.FindGameObjectsWithTag("Breakable");
                            int randomIndex = Random.Range(0, bricks.Length);
                            Vector2 brickDir = (bricks[randomIndex].GetComponent<Renderer>().bounds.center - this.transform.position).normalized;
                            rb.velocity = brickDir * adversityX3SpeedMultiplier;
                        }
                    }
                }
            }

            // Play Sound
            //AudioSource.PlayClipAtPoint(HitBrick, transform.position);
        }
        //Debug.Log(rb.velocity + " " + rb.velocity.magnitude);

    }

    float HitFactor(Vector2 ballPos, Vector2 paddlePos, float paddleWidth)
    {
        return (ballPos.x - paddlePos.x) / paddleWidth;
    }

    void Countdown()
    {
        startTimerSeconds--;
    }

    void StartTimerCountdown()
    {
        startTimerSeconds--;
        SetStartTimerText();
    }

    public void EnableFastBall()
    {
        fastBall = GameObject.Find("Fast Ball");
        fastBall.GetComponent<Collider2D>().enabled = true;
        Physics2D.IgnoreLayerCollision(8, 9, true);
    }

    public void DisableFastBall()
    {
        fastBall = GameObject.Find("Fast Ball");
        fastBall.GetComponent<Collider2D>().enabled = false;
        Physics2D.IgnoreLayerCollision(8, 9, false);
    }

    public void StartDissolve(int count)
    {
        //this.GetComponent<SpriteRenderer>().color = Color.yellow;
        dissolveNextAngle = dissolveInitialAngle;
        Dissolve(count);
    }
    
    private void Dissolve(int count)
    {
        if (count > 0)
        {
            Dissolve();
            Dissolve(--count);
        }
    }

    private void Dissolve()
    {
        Ball newBall = Instantiate(ball, transform.position, transform.rotation);
        newBall.ShootBall(rb.velocity.normalized);
        newBall.PlaceAndDirect(dissolveNext * dissolveNextAngle);
        dissolveNext = -dissolveNext;
        if (dissolveNext > 0)
            dissolveNextAngle = dissolveNextAngle + dissolveNext * dissolveInitialAngle;
    }

    private void PlaceAndDirect(float angle)
    {
        Vector2 dir = rb.velocity.normalized;
        Vector2 newDir = Quaternion.AngleAxis(angle, Vector3.forward) * (Vector3)dir;
        transform.position = (Vector2)transform.position - dir * this.dissolveOffset + newDir * dissolveOffset;
        rb.velocity = newDir * currentSpeedMultiplier;
        
        CancelInvoke("StartTimerCountdown");
        hasStarted = true;
    }

}