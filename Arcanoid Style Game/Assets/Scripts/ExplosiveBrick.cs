using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBrick : BrickZero {

    public GameObject power;
    public GameObject scoreText;
    private GameObject collidingScorer;

    public GameObject explosion;
    public float explosionRadius = 1.0f;

    private LevelManager levelManager;
    private ScoreManager scoreManager;
    private PowerManager powerManager;

    //private SpriteRenderer spriteRenderer;

    /*void Start()
    {
        LevelManager.breakableCount++;
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
    }*/

    private void Start()
    {
        //spriteRenderer = this.GetComponent<SpriteRenderer>();
        //LoadSpritesExplosive();
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        hits = 1;
        LevelManager.breakableCount++;
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collidingScorer = collision.gameObject;
        MyDestroy();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Check if Bolt
        if (collider.gameObject.tag == "Bolt")
        {
            collidingScorer = collider.gameObject;
            MyDestroy();
        }
        if (collider.name == "Fast Ball")
        {
            collidingScorer = collider.gameObject;
            MyDestroy();
        }
    }

    /*private void DestroyBricks(Vector3 center)
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position + center, explosionRadius);

        foreach (Collider2D obj in objects)
        {
            if (obj.Equals(this.gameObject.GetComponent<Collider2D>()))
                continue;
            Brick brick = obj.GetComponent<Brick>();
            if(brick != null)
            {
                Instantiate(explosion, obj.transform.position + center, obj.transform.rotation);
                brick.MyDestroy();
            }
        }
    }*/

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
        HandleScore();

        //GetComponent<Collider2D>().enabled = false;

        //Explode();

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

    /*void LoadSpritesExplosive()
    {
        switch (levelManager.season)
        {
            case (int)LevelManager.Season.SUMMER:
                {
                    this.spriteRenderer.sprite = summerExplosive;
                }
                break;
            case (int)LevelManager.Season.AUTUMN:
                {
                    this.spriteRenderer.sprite = autumnExplosive;
                }
                break;
            case (int)LevelManager.Season.WINTER:
                {
                    this.spriteRenderer.sprite = autumnExplosive; //winterExplosive;
                }
                break;
            case (int)LevelManager.Season.SPRING:
                {
                    this.spriteRenderer.sprite = autumnExplosive; //springExplosive;
                }
                break;
        }
    }*/

    /*private void Explode()
    {
        //Debug.Log("Explode");
        Vector3 center = (GetComponent<SpriteRenderer>().bounds.center) / 18;
        //AudioSource.PlayClipAtPoint(crack, transform.position);
        Instantiate(explosion, transform.position + center, transform.rotation);
        DestroyBricks(center);
    }*/

    private void OnDestroy()
    {
        if (levelManager.levelStart)
        {
            Explode();
            levelManager.BrickDestroyed();
        }
    }

    public void Explode()
    {
        //Debug.Log("Explode");
        //Vector3 center = (GetComponent<SpriteRenderer>().bounds.center); /// 18;
        //AudioSource.PlayClipAtPoint(crack, transform.position);
        GetComponent<Collider2D>().enabled = false;
        Instantiate(explosion, transform.position, transform.rotation); //transform.position + center
        DestroyBricks(transform.position); //center
    }

    public void DestroyBricks(Vector3 center)
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(center, explosionRadius); //transform.position + center

        foreach (Collider2D obj in objects)
        {
            if (obj.Equals(this.gameObject.GetComponent<Collider2D>()))
                continue;
            BrickZero brick = obj.GetComponent<BrickZero>();
            if (brick != null)
            {
                Instantiate(explosion, obj.transform.position, obj.transform.rotation); //obj.transform.position + center
                brick.MyDestroy();
            }
        }
    }
}
