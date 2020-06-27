using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class LevelManager : MonoBehaviour {

    private Ball ball;
    private PowerManager powerManager;
    private ScoreManager scoreManager;
    private MapGenerator mapGenerator;
    private EnemySpawner enemySpawner;
    //private Background background;

    public bool levelStart = false;
    public static int breakableCount = 0;
    public int brickHitsTotal = 0;
    public int unbreakableBrickHitsTotal = 0;

    // Level
    public int level = 0;

    // Level vars
    private int month = 0; // 1 - 3
    public enum Season { SPRING = 1, SUMMER, AUTUMN, WINTER};
    public int season = 0; // 1 - 4
    //private int year = 0;

    // Difficulty
    public float difficultyLinearFactor;
    public float difficultyPowFactor;
    [SerializeField]
    private int difficulty;
    public bool updateLevel = false; // TO BE REMOVED!!!

    // Intensity
    public AnimationCurve intensityCurve;
    public int defaultIntensityLevelCap = 12;
    public int intensityLevelCap = 12;
    //private int levelCounter = 0;
    [SerializeField]
    private float intensity;

    void Start()
    {
        //ball = GameObject.FindObjectOfType<Ball>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();
        //background = GameObject.FindObjectOfType<Background>();

        ResetLevels();
        UpdateLevel();
    }

    void Update()
    {
        if (updateLevel)
        {
            // Set Difficulty
            updateLevel = false;

            UpdateLevel();
        }
    }

    public void LoadLevel(string name){
		//Debug.Log ("New Level load: " + name);
        SceneManager.LoadScene(name);
	}

	public void QuitRequest(){
		//Debug.Log ("Quit requested");
		Application.Quit ();
	}

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BrickDestroyed()
    {
        // Get Brick Count
        GetBrickCount();

        // Check Brick Count
        if (LevelManager.breakableCount <= 0)
        {
            //LoadNextLevel();

            powerManager = GameObject.FindObjectOfType<PowerManager>();
            if (powerManager.powerStrength == PowerManager.Strength.x3) { powerManager.StrengthDown(); }
            if (powerManager.currentPower == PowerManager.Power.dissolve) { powerManager.ResetDissolve(); }

            //Debug.Log(FindObjectsOfType<Ball>().Length);
            //Debug.Log(GameObject.FindGameObjectsWithTag("Ball").Length);
            
            scoreManager = GameObject.FindObjectOfType<ScoreManager>();
            scoreManager.scoreMultiplier = 1;
            scoreManager.SetMultiplierText();
            scoreManager.InitializeTimer();

            //ball = GameObject.FindObjectOfType<Ball>();
            //ball.ResetBall();
            foreach (Ball obj in FindObjectsOfType<Ball>())
            {
                obj.ResetBall();
            }

            WinLevel();
        }
        
    }

    void GetBrickCount()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Breakable");

        breakableCount = objects.Length;
        //Debug.Log(breakableCount);
    }

    void WinLevel()
    {
        scoreManager.UpdateAchievements();
        scoreManager.UpdateSaveManager();
        UpdateLevel();
    }

    public void LoseLevel()
    {
        scoreManager.UpdateSaveManager();
        LoadLevel("Lose");
    }

    void UpdateLevel()
    {
        /*if (powerManager != null) { powerManager.ResetPower(); }
        if (ball != null) { ball.ResetBall(); }
        if (scoreManager != null)
        {
            scoreManager.scoreMultiplier = 1;
            scoreManager.SetMultiplierText();
            scoreManager.InitializeTimer();
        }*/

        levelStart = false;

        // set level
        level++;

        // set month & season
        if (level == 1)
        {
            month = 1;
            season = 1;
        }
        else if (month >= 3)
        {
            month = 1;
            if (season >= 4) { season = 1; }
            else { season++; }
        }
        else { month++; }

        //Debug.Log("Level = "+ level);
        //Debug.Log("Month = " + month);
        //Debug.Log("Season = " + season);

        // set difficulty
        difficulty = Mathf.RoundToInt(Mathf.Pow((float)level, difficultyPowFactor) * difficultyLinearFactor);

        // set intensity
        if (level > intensityLevelCap || level == 1) { intensityLevelCap = (level - 1) + defaultIntensityLevelCap; }
        //intensity = intensityCurve.Evaluate((float)level / (float)intensityLevelCap);
        float intens = 0f;
        switch (season)
        {
            case (int)Season.SPRING:
                {
                    if (month == 1) { intens = 0f; }
                    else if (month == 2) { intens = 0.2f; }
                    else if (month == 3) { intens = 0.4f; }
                }
                break;
            case (int)Season.SUMMER:
                {
                    if (month == 1) { intens = 0.2f; }
                    else if (month == 2) { intens = 0.4f; }
                    else if (month == 3) { intens = 0.6f; }
                }
                break;
            case (int)Season.AUTUMN:
                {
                    if (month == 1) { intens = 0.4f; }
                    else if (month == 2) { intens = 0.6f; }
                    else if (month == 3) { intens = 0.8f; }
                }
                break;
            case (int)Season.WINTER:
                {
                    if (month == 1) { intens = 0.6f; }
                    else if (month == 2) { intens = 0.8f; }
                    else if (month == 3) { intens = 1f; }
                }
                break;
        }
        double d = Math.Round((double)difficulty / level * 0.2, 3, MidpointRounding.AwayFromZero);
        intensity = Mathf.Clamp(intens + (float)d, 0, 1);
        //Debug.Log("intensity = " + intensity);

        // set map generator parameters and generate bricks
        mapGenerator = GameObject.FindObjectOfType<MapGenerator>();
        mapGenerator.SetMapGeneratorParameters(level, month, difficulty, intensity);
        mapGenerator.generateBricks = true;
        mapGenerator.Generate();

        // initialize brickHitsTotal
        int hitsTotal = 0;

        GameObject[] breakables = GameObject.FindGameObjectsWithTag("Breakable");

        foreach (GameObject obj in breakables)
        {
            hitsTotal += obj.GetComponent<BrickZero>().hits;
        }

        brickHitsTotal = hitsTotal;

        // initialize unbreakableBrickHitsTotal
        int unbreakableHitsTotal = 0;

        GameObject[] unbreakables = GameObject.FindGameObjectsWithTag("Unbreakable");

        foreach (GameObject obj in unbreakables)
        {
            unbreakableHitsTotal += obj.GetComponent<UnbreakableBrick>().hits;
        }

        unbreakableBrickHitsTotal = unbreakableHitsTotal;

        // score manager
        scoreManager.InitializeTimer();
        scoreManager.SetLevelText();
        scoreManager.ResetBestScoreHits();
        scoreManager.SetScoreMultiplier();
        //scoreManager.SetMultiplierText();

        // background
        //background.LoadSprites();

        // enemies
        enemySpawner.spawning = false;
        enemySpawner.StopCoroutine(enemySpawner.SpawnEnemies());

        //enemySpawner.enemyMax = Mathf.RoundToInt(intensity * 4);
        if (intensity <= 0.2f) { enemySpawner.enemyMax = 0; }
        else if (intensity > 0.05f && intensity <= 0.6f) { enemySpawner.enemyMax = 1; }
        else if (intensity > 0.6f && intensity <= 0.85f) { enemySpawner.enemyMax = 2; }
        else if (intensity > 0.85f && intensity < 1f) { enemySpawner.enemyMax = 3; }
        else if (intensity >= 1f) { enemySpawner.enemyMax = 4; }
        enemySpawner.StartCoroutine(enemySpawner.SpawnEnemies());
        enemySpawner.spawning = true;
        Debug.Log(enemySpawner.enemyMax);

        // timer icon
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Timer Icon"))
        {
            obj.GetComponent<SpriteRenderer>().enabled = true;
        }

        levelStart = true;
    }

    void ResetLevels()
    {
        level = 0;
        month = 0;
        season = 0;
    }
    
}
