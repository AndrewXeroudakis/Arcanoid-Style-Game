
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerManager : MonoBehaviour {

    public enum Power { normal, slow, catchPower, dissolve, enlarge, player, laser, breaker, xplosion, minimize, fast, reverse, adversity };
    public enum Strength { x1 = 1, x2, x3 };
    public Power currentPower = Power.normal;
    public Strength powerStrength = Strength.x1;

    // Power Weights, high number means more occurrance : their total should be 100
    // Power Ups : total 60
    public int slowWeight = 10;
    public int catchPowerWeight = 10;
    public int dissolveWeight = 10; //8;
    public int enlargeWeight = 10;
    public int playerWeight = 10; //5;
    public int laserWeight = 10; //8;
    public int breakerWeight = 10; //8;
    public int xplosionWeight = 5; //1;
    // Power Downs : total 40
    public int minimizeWeight = 10;
    public int fastWeight = 10;
    public int reverseWeight = 10;
    public int adversityWeight = 10;
    // Vars
    int[] powerWeights;
    int powerWeightTotal;
    // Structure of Powers
    struct Powers
    {
        public const int slow = 0;
        public const int catchPower = 1;
        public const int dissolve = 2;
        public const int enlarge = 3;
        public const int player = 4;
        public const int laser = 5;
        public const int breaker = 6;
        public const int xplosion = 7;
        public const int minimize = 8;
        public const int fast = 9;
        public const int reverse = 10;
        public const int adversity = 11;
    }

    // Drop Power Weights, high number means more occurrance : their total should be 100
    public int dropPowerWeight = 10;
    public int dropNothingWeight = 90;
    // Vars
    public int[] dropWeights;
    int dropWeightTotal;
    // Structure of Drops
    public struct Drops
    {
        public const int dropNothing = 0;
        public const int dropPower = 1;
    }
    // Drop Timer Vars
    public bool startTimer = true;
    private int secondsTotal = 3; //30;
    private int secondsLeft = 0;

    private ScoreManager scoreManager;
    private Paddle paddle;
    public ScoreText scoreText;
    public Ball ball;
    public Text strengthX3TimerText;
    private float strengthX3Duration = 10f;
    private float strengthX3DurationLeft = 10f;

    private float dissolveX3Interval = 2f;
    private int dissolveMaxBalls = 6;

    void Awake()
    {
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        paddle = GameObject.FindObjectOfType<Paddle>();

        ResetStrengthX3Countdown();

        // Set powerWeights array
        powerWeights = new int[13]; // 13 is the total number of powers
        // Fill powerWeights array
        powerWeights[Powers.slow] = slowWeight;
        powerWeights[Powers.catchPower] = catchPowerWeight;
        powerWeights[Powers.dissolve] = dissolveWeight;
        powerWeights[Powers.enlarge] = enlargeWeight;
        powerWeights[Powers.player] = playerWeight;
        powerWeights[Powers.laser] = laserWeight;
        powerWeights[Powers.breaker] = breakerWeight;
        powerWeights[Powers.xplosion] = xplosionWeight;
        powerWeights[Powers.minimize] = minimizeWeight;
        powerWeights[Powers.fast] = fastWeight;
        powerWeights[Powers.reverse] = reverseWeight;
        powerWeights[Powers.adversity] = adversityWeight;
        // Set powerWeightTotal
        powerWeightTotal = 0;
        foreach (int w in powerWeights)
        {
            powerWeightTotal += w;
        }

        // Set dropWeights array
        dropWeights = new int[2]; // 2 is the total number of drops
        // Fill dropWeights array
        dropWeights[Drops.dropNothing] = dropNothingWeight;
        dropWeights[Drops.dropPower] = dropPowerWeight;
        // Set dropWeightTotal
        dropWeightTotal = 0;
        foreach (int w in dropWeights)
        {
            dropWeightTotal += w;
        }
    }

    void Update()
    {
        // Timer
        if (startTimer)
        {
            secondsLeft = secondsTotal;
            InvokeRepeating("Countdown", 1, 1);
            startTimer = false;
        }

        //Strength x3 timer text follow paddle position
        strengthX3TimerText.transform.position = paddle.transform.position + new Vector3(0, -1, 0);
    }

    public void ResetPower()
    {
        if (currentPower == Power.dissolve) { ResetDissolve(); }
        else if (currentPower == Power.laser) { FindObjectOfType<Paddle>().DestroyBolts(); }
        else if (currentPower == Power.fast || currentPower == Power.slow) { ball = FindObjectOfType<Ball>(); ball.DisableFastBall(); ball.UpdateSpeed(Power.normal, Strength.x1); }
        else if (currentPower == Power.catchPower) { FindObjectOfType<Ball>().CatchPowerShootBall(); }
        
        currentPower = Power.normal;
        powerStrength = Strength.x1;

        //CancelInvoke("StrengthDown");
        ResetStrengthX3Countdown();
        CancelInvoke("DissolveX3");

        // set scoreManager
        scoreManager.SetPowerText(((Power)currentPower).ToString(), ((int)powerStrength).ToString());
        scoreManager.SetScoreMultiplier();
    }

    // ChooseRandomPower : Method that returns a number from 0 - 12 based on some chance calculations
    public int ChooseRandomPower()
    {
        return RandomWeighted(powerWeights, powerWeightTotal) + 1; // The "+1" is so that the number will align with the enum Power
    }

    // ChooseRandomDrop : Method that returns a number from 0 - 1 based on some chance calculations
    public int ChooseRandomDrop()
    {
        return RandomWeighted(dropWeights, dropWeightTotal);
    }

    // RandomWeighted : Method that returns a random weighted integer
    int RandomWeighted(int[] weights, int weightTotal)
    {
        int result = 0, total = 0;
        int randVal = Random.Range(0, weightTotal);
        for (result = 0; result < weights.Length; result++)
        {
            total += weights[result];
            if (total > randVal) break;
        }
        return result;
    }

    void Countdown()
    {
        secondsLeft--;
        if (secondsLeft <= 0)
        {
            CancelInvoke("Countdown");
            // Every 3 seconds it adds 10 weight to the original drop power weight until it reaches 270(3*90) which is 75% chance.
            //if (dropWeights[Drops.dropPower] < dropWeights[Drops.dropNothing]*3) // stops at 75% drop power chance.
            {
                // Add weight
                dropWeights[Drops.dropPower] += dropPowerWeight;
                //Debug.Log(dropWeights[Drops.dropPower]);
                // Reset dropWeightTotal
                dropWeightTotal = 0;
                foreach (int w in dropWeights)
                {
                    dropWeightTotal += w;
                }
                startTimer = true;
            }
        }
        //Debug.Log(secondsLeft);
    }

    void StrengthX3Countdown()
    {
        strengthX3DurationLeft--;
        strengthX3TimerText.text = strengthX3DurationLeft.ToString();
        if (strengthX3DurationLeft <= 0)
        {
            StrengthDown();
        }
    }

    void ResetStrengthX3Countdown()
    {
        CancelInvoke("StrengthX3Countdown");
        strengthX3DurationLeft = strengthX3Duration;
        strengthX3TimerText.text = strengthX3DurationLeft.ToString();
        if (powerStrength == Strength.x3) { strengthX3TimerText.GetComponent<Text>().enabled = true; }
        else { strengthX3TimerText.GetComponent<Text>().enabled = false; }
    }

    public void StrengthDown()
    {
        Debug.Log("StrengthDown");

        //CancelInvoke("StrengthDown");
        CancelInvoke("DissolveX3");

        if (this.powerStrength == Strength.x3)
            powerStrength = Strength.x2;

        ResetStrengthX3Countdown();

        if (currentPower == Power.dissolve)
        {
            DestroyExtraBalls(dissolveMaxBalls);
        }
        else if (currentPower == Power.fast || currentPower == Power.slow)
        {
            ball = FindObjectOfType<Ball>();
            ball.DisableFastBall();
            ball.UpdateSpeed(currentPower, powerStrength);
        }

        // set scoreManager
        scoreManager.SetPowerText(((Power)currentPower).ToString(), ((int)powerStrength).ToString());
        scoreManager.SetScoreMultiplier();
    }

    public void ReceivedPower(Transform powerTransform, Power newPower)
    {
        //Strength prevPowerStrength = powerStrength;

        scoreManager.powersCollected += 1;
        //Debug.Log("powersCollected = " + scoreManager.powersCollected);

        // reset laser
        if (currentPower == PowerManager.Power.laser) { FindObjectOfType<Paddle>().ResetLaser(); }

        // Check new power
        if (powerStrength == Strength.x3)
        {
            if (newPower == currentPower)
            {
                ExtendPower();
            }
        }
        else
        { 
            if (newPower == currentPower)
            {
                IncreaseStrength();
            }
            else
            {
                ResetPower();
            }

            // Set currentPower
            currentPower = newPower;

            // Apply Power
            //if (prevPowerStrength < Strength.x3) // Να φυγει αυτη η γραμμη, Apply Power θα κανει παντα, το dissolve θα πρεπει να φτιαχνει οσες μπαλες λειπουν αν ειναι στο max strength
            ball = FindObjectOfType<Ball>();
            if (currentPower != Power.dissolve || ball.hasStarted == true) { ApplyPower(); }
        }

        // set scoreManager
        scoreManager.SetPowerText(((Power)currentPower).ToString(), ((int)powerStrength).ToString());
        scoreManager.SetScoreMultiplier();

        // Add new score and score text
        int newScore = 0;
        if (powerStrength == Strength.x3 && newPower != currentPower) { newScore = scoreManager.SetScoreOther(scoreManager.collectPowerPointsX1); }
        else { newScore = scoreManager.SetScoreOther(scoreManager.GetPowerPoints()); }        
        ScoreText sT = Instantiate(scoreText, powerTransform.position, powerTransform.rotation);
        sT.transform.position += new Vector3(0, 0.25f, 0);
        sT.GetComponent<ScoreText>().offset.y = 1;
        sT.GetComponent<TextMesh>().text = "+" + newScore.ToString();
    }

    private void ExtendPower()
    {
        //CancelInvoke("StrengthDown");
        //Invoke("StrengthDown", strengthX3Duration);
        ResetStrengthX3Countdown();
        InvokeRepeating("StrengthX3Countdown", 1, 1);
        if (currentPower != Power.dissolve || ball.hasStarted == true) { ApplyPower(); }
    }

    public void ApplyPower()
    {
        if (currentPower == Power.fast || currentPower == Power.slow)
        {
            ball = GameObject.FindObjectOfType<Ball>();
            ball.UpdateSpeed(currentPower, powerStrength);
            if(currentPower == Power.fast && this.powerStrength == Strength.x3)
                ball.EnableFastBall();
        }
        else if(currentPower == Power.dissolve)
        {
            // Find Highest ball
            Ball ballHighest = GameObject.FindObjectsOfType<Ball>().OrderByDescending(go => go.transform.position.y).First();
            
            switch (powerStrength)
            {
                case PowerManager.Strength.x1:
                    {
                        ballHighest.StartDissolve(2);
                    }
                    break;
                case PowerManager.Strength.x2:
                    {
                        ballHighest.StartDissolve(3);
                    }
                    break;
                case PowerManager.Strength.x3:
                    {
                        //Debug.Log("Strength 3 dissolve...");
                        ballHighest.StartDissolve(4);
                        if(!IsInvoking("DissolveX3"))
                        //CancelInvoke()
                            InvokeRepeating("DissolveX3", dissolveX3Interval + 0.01f, dissolveX3Interval);
                    }
                    break;
            }
            //Time.timeScale = 0.05f;

            /*
            Ball[] balls = FindObjectsOfType<Ball>();
            foreach(Ball ball in balls)
            {
                ball.Dissolve();
            }
            */
        }
        else if (currentPower == Power.player)
        {
            BorderBottom borderBodom = GameObject.FindObjectOfType<BorderBottom>();

            int maxLives = 9;

            if (borderBodom.lives < maxLives)
            {
                // Check Strength and enlarge
                if (powerStrength == PowerManager.Strength.x1)
                {
                    Mathf.Clamp(borderBodom.lives++, 0, maxLives);
                }
                else if (powerStrength == PowerManager.Strength.x2)
                {
                    Mathf.Clamp(borderBodom.lives += 2, 0, maxLives);
                }
                else if (powerStrength == PowerManager.Strength.x3)
                {
                    Mathf.Clamp(borderBodom.lives += 3, 0, maxLives);
                }
            }
        }
    }


    private void DissolveX3()
    {
        //Debug.Log("DissolveX3 Repeat...");
        Ball[] balls = FindObjectsOfType<Ball>().OrderByDescending(go => go.transform.position.y).ToArray();
        balls[0].StartDissolve(1);
    }

    private void IncreaseStrength()
    {
        if (powerStrength != Strength.x3)
            powerStrength++;
        if (powerStrength == Strength.x3)
        {
            ResetStrengthX3Countdown();
            InvokeRepeating("StrengthX3Countdown", 1, 1);
            //Invoke("StrengthDown", strengthX3Duration);
            if (currentPower == Power.reverse) { scoreManager.AddCarryTheBall(); }
        }
    }

    public void ResetDissolve()
    {
        DestroyExtraBalls(1);
    }

    private void DestroyExtraBalls(int maxBalls)
    {
        // Create array of balls
        Ball[] balls = FindObjectsOfType<Ball>();

        // Check if more than one balls exist
        if (balls.Length > 1)
        {
            // Order array by highest y
            balls = balls.OrderByDescending(go => go.transform.position.y).ToArray();

            // Destroy other balls
            for (int i = maxBalls; i < balls.Length; i++)
            {
                Destroy(balls[i].gameObject);
            }
        }

        // Reset paddle ball
        paddle.SetBall(balls[0]);
    }
}
