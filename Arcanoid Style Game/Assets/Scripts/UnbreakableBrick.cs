using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnbreakableBrick : BrickZero {

    public AudioClip HitUnbreakable;
    public GameObject power;
    public GameObject scoreText;
    private GameObject collidingScorer;

    private ScoreManager scoreManager;
    private PowerManager powerManager;
    //private SpriteRenderer spriteRenderer;

    void Awake()
    {
        hits = 1;
    }

    void Start()
    {
        // Get Objects
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
    }

    /*private void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        LoadSpritesUnbreakable();
    }*/

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collidingScorer = collision.gameObject;

        AudioSource.PlayClipAtPoint(HitUnbreakable, transform.position);
        //GetComponent<Animator>().SetTrigger("OnHit");

        // Check Power
        if (powerManager.currentPower == PowerManager.Power.breaker)
        {
            //if (powerManager.powerStrength == PowerManager.Strength.x3) { Explode(); }
            MyDestroy();
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Check if Bolt
        if (collider.gameObject.tag == "Bolt")
        {
            collidingScorer = collider.gameObject;
            AudioSource.PlayClipAtPoint(HitUnbreakable, transform.position);
            //GetComponent<Animator>().SetTrigger("OnHit");
        }
    }

    void HandleScore()
    {
        // check curve ball && bolt
        bool curveBall = false;
        bool bolt = false;
        if (collidingScorer != null)
        {
            if (collidingScorer.tag == "Ball" || collidingScorer.tag == "Ball") { if (collidingScorer.GetComponent<Ball>().isCurveBall == true) { curveBall = true; } }
            else if (collidingScorer.tag == "Bolt") { bolt = true; }
        }

        // set score
        int newScore = 0;
        if (bolt)
        {
            newScore = scoreManager.SetScoreOther(scoreManager.hitBrickPoints);
        }
        else
        {
            if (curveBall)
            {
                newScore = scoreManager.SetScore(scoreManager.ballHitsAdditive, scoreManager.curveBallAdditive);
                scoreManager.curvedHits += 1;
            }
            else
            {
                newScore = scoreManager.SetScore(scoreManager.ballHitsAdditive, 0);
            }
        }

        // instantiate score text
        GameObject sT = Instantiate(scoreText, transform.position, transform.rotation);

        // set score text position
        if (collidingScorer != null)
        {
            Vector3 direction = transform.position - collidingScorer.transform.position;
            if (direction.y >= 0) { sT.transform.position -= new Vector3(0, 0.5f, 0); sT.GetComponent<ScoreText>().offset.y = -1; } // collision is up
            else { sT.transform.position += new Vector3(0, 0.5f, 0); sT.GetComponent<ScoreText>().offset.y = 1; } // collision is down
        }
        else
        {
            sT.transform.position += new Vector3(0, 0.5f, 0); sT.GetComponent<ScoreText>().offset.y = 1;
        }

        // set score text text
        sT.GetComponent<TextMesh>().text = "+" + newScore.ToString();
    }

    public override void MyDestroy()
    {
        // Add new score and score text
        HandleScore();

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

        Destroy(gameObject);
    }

    /*void LoadSpritesUnbreakable()
    {
        switch (levelManager.season)
        {
            case (int)LevelManager.Season.SUMMER:
                {
                    this.spriteRenderer.sprite = summerUnbreakable;
                }
                break;
            case (int)LevelManager.Season.AUTUMN:
                {
                    this.spriteRenderer.sprite = autumnUnbreakable;
                }
                break;
            case (int)LevelManager.Season.WINTER:
                {
                    this.spriteRenderer.sprite = autumnUnbreakable; //winterUnbreakable;
                }
                break;
            case (int)LevelManager.Season.SPRING:
                {
                    this.spriteRenderer.sprite = autumnUnbreakable; //springUnbreakable;
                }
                break;
        }
    }*/

}
