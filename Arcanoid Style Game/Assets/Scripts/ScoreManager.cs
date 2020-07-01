using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

    // Main Score Vars
    private LevelManager levelManager;
    private PowerManager powerManager;
    private SaveManager saveManager;
    public int score = 0;
    public int addedScore = 0;
    //private int xpPoints = 0;
    //private int xpPointsRequired = 1000;
    //public int experience = 1;

    public float ballHits = 0;
    public float bestBallHits = 0;
    public float scoreHits = 0;
    public float bestScoreHits = 0;
    private int returnToPaddleCounter = 0;

    public int ballHitsAdditive = 1;
    public int curveBallAdditive = 4;

    public float scoreMultiplier = 1;
    private float scoreMultiplierNormal = 1;
    private float scoreMultiplierPowerDownx1 = 2;
    private float scoreMultiplierPowerDownx2 = 3;
    private float scoreMultiplierPowerDownx3 = 4;
    
    public int hitBrickPoints = 1;

    //public int collectPowerUpPoints = 100;
    public int collectPowerPointsX1 = 50;
    public int collectPowerPointsX2 = 100;
    public int collectPowerPointsX3 = 200;
    //public int collectPowerDownPoints = 100; //150;

    // Achievements
    public enum Achievements {
        StageClear = 100,
        BreakEverything = 75,
        BuzzerBeater = 250,
        Goalkeeper = 150,
        CurveballExpert = 125,
        UtilityPlayer = 225,
        MakeEveryShotCount = 260,
        KeepTheBallRollingX1 = 60,
        KeepTheBallRollingX2 = 120,
        KeepTheBallRollingX3 = 240,
        SlamDunk = 500,
        CarryTheBall = 600,
        MarathonRunner = 1000,
        GOAT = 2000
    };
    public List<Achievements> achievementsList = new List<Achievements>();

    // buzzerBeater
    float timeTotal = 0;
    float timeLeft = 0;
    private bool buzzerBeater = true;

    // goalkeeper
    public bool goalkeeper = true;

    // utility player
    public int powersTotal = 0;
    public int powersCollected = 0;

    // curveball expert
    public int curvedHits = 0;

    // make every shot count
    public bool makeEveryShotCount = true;

    // goat counter
    private int goatCounter = 0;

    // Multiplier Star
    private bool multiplierStar = false;

    // Text Vars
    //public Text experienceText;
    public Text scoreText;
    public Text multiplierText;
    public Text timerText;
    public Text levelText;
    public Text powerText;
    public Text nextTimerText;

    void Awake () {
        
        // Find objects
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        saveManager = GameObject.FindObjectOfType<SaveManager>();

        // Initialize scoreText
        SetScore(0, 0);

        // Initialize Timer
        InitializeTimer();
    }

    public void ResetScoreHits()
    {
        ballHits = 0;
        scoreHits = 0;
    }

    public void ResetBestScoreHits()
    {
        ballHits = 0;
        scoreHits = 0;
        bestBallHits = 0;
        bestScoreHits = 0;
        addedScore = 0;
    }

    // Method that updates the score value and text with the current value of score
    public int SetScore(int additive, int bonusScoreAdditive)
    {
        // set hits & scoreMultiplier
        ballHits += additive;
        if (ballHits > bestBallHits) { bestBallHits = ballHits; };
        scoreHits += additive + bonusScoreAdditive;
        if (scoreHits > bestScoreHits) { bestScoreHits = scoreHits; };
        SetScoreMultiplier();

        // set score
        int newScore = Mathf.RoundToInt(scoreHits * scoreMultiplier);
        score += newScore;
        addedScore += newScore;

        // set score text
        if (scoreText != null)
            scoreText.text = score.ToString(); //"SCORE " +

        return newScore;
    }

    public void SetScoreMultiplier()
    {
        if ((powerManager.currentPower == PowerManager.Power.adversity)
        || (powerManager.currentPower == PowerManager.Power.fast)
        || (powerManager.currentPower == PowerManager.Power.minimize)
        || (powerManager.currentPower == PowerManager.Power.reverse))
        {
            if (powerManager.powerStrength == PowerManager.Strength.x1) { scoreMultiplier = scoreMultiplierPowerDownx1; }
            else if (powerManager.powerStrength == PowerManager.Strength.x2) { scoreMultiplier = scoreMultiplierPowerDownx2; }
            else if (powerManager.powerStrength == PowerManager.Strength.x3) { scoreMultiplier = scoreMultiplierPowerDownx3; }
        }
        else
        {
            scoreMultiplier = scoreMultiplierNormal;
        }

        scoreMultiplier = Mathf.Round(scoreMultiplier * 100f) / 100f;
        SetMultiplierText();
    }

    public int SetScoreOther(int scoreAdditive)
    {
        int newScore = scoreAdditive; // Mathf.RoundToInt(scoreAdditive * scoreMultiplier);
        score += newScore;
        addedScore += newScore;
        scoreText.text = score.ToString();
        return newScore;
    }

    // Method that updates the multiplier text with the current value of scoreMultiplier
    public void SetMultiplierText()
    {
        multiplierText.text = scoreMultiplier.ToString(); //bestBallHits.ToString(); //ToString("0.00"); //"+ " + 
    }

    public void SetLevelText()
    {
        levelText.text = levelManager.level.ToString(); //"LEVEL = " + 
    }

    public void SetPowerText(string power, string strength)
    {
        if (powerManager.currentPower == 0)
        {
            powerText.GetComponent<Text>().enabled = false;
        }
        else
        {
            powerText.GetComponent<Text>().enabled = true;
            powerText.text = power + " x " + strength;
        }
    }

    // Method that resets the scoreMultiplier to a given value;
    public void ResetMultiplierPowerDown(float scoreMultiplierPercentageReturn)
    {
        scoreMultiplier = 1 + (Mathf.Round((scoreMultiplier * scoreMultiplierPercentageReturn) * 100f) / 100f); // You keep 20% of your current multiplier if you have a power down x1 active every time you hit the paddle
    }

    // Method that updates the multiplier text with the current value of scoreMultiplier
    public void SetTimerText()
    {
        timerText.text = (Mathf.Floor(timeLeft / 60)).ToString("00") + ":" + (Mathf.RoundToInt(timeLeft % 60)).ToString("00");

        /*
        if (timeLeft >= Mathf.RoundToInt(timeTotal * 0.75f))
        {
            timerText.color = new Color32(229, 228, 226, 255); // Platinum
            SetNextTimerText(timeTotal * 0.75f, new Color32(255, 215, 0, 255));
        }
        else if (timeLeft >= Mathf.RoundToInt(timeTotal * 0.50f))
        {
            timerText.color = new Color32(255, 215, 0, 255); // Gold
            SetNextTimerText(timeTotal * 0.50f, new Color32(192, 192, 192, 255));
        }
        else if (timeLeft >= Mathf.RoundToInt(timeTotal * 0.25f))
        {
            timerText.color = new Color32(192, 192, 192, 255); // Silver
            SetNextTimerText(timeTotal * 0.25f, new Color32(205, 127, 50, 255));
        }
        else
        {
            timerText.color = new Color32(205, 127, 50, 255); // Bronze
            SetNextTimerText(0, Color.white);
        }
        */
    }

    void SetNextTimerText( float time, Color color)
    {
        nextTimerText.text = Mathf.Floor(time / 60).ToString("00") + ":" + Mathf.RoundToInt(time % 60).ToString("00");
        nextTimerText.color = color;
    }

    void Countdown()
    {
        timeLeft--;
        if (timeLeft >= 0)
        {
            SetTimerText();
        }
        else
        {
            timerText.text = "";
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Timer Icon"))
            {
                obj.GetComponent<SpriteRenderer>().enabled = false;
            }
            buzzerBeater = false;
            CancelInvoke("Countdown");
        }
    }

    public int GetPowerPoints()
    {
        if (powerManager.powerStrength == PowerManager.Strength.x1)
            return collectPowerPointsX1;
        else if (powerManager.powerStrength == PowerManager.Strength.x2)
            return collectPowerPointsX2;
        else if (powerManager.powerStrength == PowerManager.Strength.x3)
            return collectPowerPointsX3;
        else return 0;
    }

    public void InitializeTimer()
    {
        CancelInvoke("Countdown");
        timeTotal = (FindObjectsOfType<BrickZero>().Length * 2) - (FindObjectsOfType<ExplosiveBrick>().Length * 3); //levelManager.brickHitsTotal * 3;
        timeLeft = timeTotal;
        SetTimerText();
        InvokeRepeating("Countdown", 1, 1);
    }

    public void CheckReturnToPaddle()
    {
        if (ballHits <= 0) { makeEveryShotCount = false; }
        returnToPaddleCounter += 1;

        CheckHitPercentage();
    }

    void CheckHitPercentage()
    {
        double hitPercentage = System.Math.Round(ballHits / levelManager.brickHitsTotal, 2, System.MidpointRounding.ToEven); //System.Math.Round(ballHits / (levelManager.brickHitsTotal + levelManager.unbreakableBrickHitsTotal), 2, System.MidpointRounding.ToEven);
        //Debug.Log("ballHits = " + ballHits);
        //Debug.Log("hitPercentage = " + hitPercentage);
        //Debug.Log("brickHitsTotal = " + levelManager.brickHitsTotal);
        //Debug.Log("unbreakableBrickHitsTotal = " + levelManager.unbreakableBrickHitsTotal);
        if (hitPercentage >= 1) { achievementsList.Add(Achievements.SlamDunk); }
        else if (hitPercentage >= 0.90f) { achievementsList.Add(Achievements.KeepTheBallRollingX3); }
        else if (hitPercentage >= 0.75f) { achievementsList.Add(Achievements.KeepTheBallRollingX2); }
        else if (hitPercentage >= 0.5f) { achievementsList.Add(Achievements.KeepTheBallRollingX1); }
    }

    public void AddCarryTheBall()
    {
        achievementsList.Add(Achievements.CarryTheBall);
    }

    void UpdateSaveManagerScoreData()
    {
        saveManager.endScore = score;
        saveManager.endStage = levelManager.level;

        if (score > saveManager.bestScore) { saveManager.bestScore = score; }
        if (levelManager.level > saveManager.bestStage) { saveManager.bestStage = levelManager.level; }
    }

    public void UpdateAchievements()
    {
        // check and add end stage achievements
        achievementsList.Add(Achievements.StageClear);
        if (buzzerBeater) { achievementsList.Add(Achievements.BuzzerBeater); goatCounter += 1; }
        if (goalkeeper) { achievementsList.Add(Achievements.Goalkeeper); goatCounter += 1; }
        Power[] powersLeft = FindObjectsOfType<Power>();
        if (powersLeft.Length > 0)
        {
            foreach ( Power power in powersLeft)
            {
                if (power.transform.position.y >= 3.75f)
                {
                    powersTotal -= 1;
                }
            }
        }
        if (powersCollected >= 5 && powersCollected >= powersTotal) { achievementsList.Add(Achievements.UtilityPlayer); goatCounter += 1; }
        if (levelManager.unbreakableBrickHitsTotal > 0)
        {
            int unbreakableHitsTotal = 0;

            GameObject[] unbreakables = GameObject.FindGameObjectsWithTag("Unbreakable");

            foreach (GameObject obj in unbreakables)
            {
                unbreakableHitsTotal += obj.GetComponent<UnbreakableBrick>().hits;
            }
            
            if (unbreakableHitsTotal <= 0) { achievementsList.Add(Achievements.BreakEverything); goatCounter += 1; } //{ achievementsList.Add(Achievements.BreakEverything.ToString()); }
        }
        if (curvedHits >= 10) { achievementsList.Add(Achievements.CurveballExpert); goatCounter += 1; }
        if (makeEveryShotCount && returnToPaddleCounter > 1) { achievementsList.Add(Achievements.MakeEveryShotCount); goatCounter += 1; }
        CheckHitPercentage();
        if (goatCounter >= 6) { achievementsList.Add(Achievements.GOAT); }

        // order list by largest score
        achievementsList.Sort((a, b) => -1 * a.CompareTo(b));

        // set score
        foreach (var achievement in achievementsList)
        {
            score += (int)achievement; //(int)Achievements.Parse(typeof(Achievements), achievement);
            addedScore += (int)achievement;
            scoreText.text = score.ToString();
            Debug.Log(achievement + "! +" + (int)achievement);
        }

        // empty achievement list
        achievementsList.Clear();

        // reset achievements
        buzzerBeater = true;
        goalkeeper = true;
        powersCollected = 0;
        powersTotal = 0;
        curvedHits = 0;
        makeEveryShotCount = true;
        returnToPaddleCounter = 0;
        goatCounter = 0;
    }

    public void UpdateSaveManager()
    {
        UpdateSaveManagerScoreData();
        saveManager.UpdateExperience();
        saveManager.Save();
    }
}
