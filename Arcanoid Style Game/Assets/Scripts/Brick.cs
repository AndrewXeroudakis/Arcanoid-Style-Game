using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : BrickZero {

    public GameObject power;
    public GameObject scoreText;
    public AudioClip HitBrick;

    // Explosive Brick
    public GameObject explosion;
    public float explosionRadius = 1.0f;

    //public Sprite[] hitSprites;
    //public int hits; // 8 - 1 

    // Sprites
    public Sprite summerHit1;
    public Sprite summerHit2;
    public Sprite summerHit3;
    /*public Sprite summerUnbreakable;
    public Sprite summerExplosive;
    public Sprite autumnHit1;
    public Sprite autumnHit2;
    public Sprite autumnHit3;
    public Sprite autumnUnbreakable;
    public Sprite autumnExplosive;
    public Sprite winterHit1;
    public Sprite winterHit2;
    public Sprite winterHit3;
    public Sprite winterUnbreakable;
    public Sprite winterExplosive;*/
    /*public Sprite springHit1;
    public Sprite springHit2;
    public Sprite springHit3;
    public Sprite springUnbreakable;
    public Sprite springExplosive;*/

    private SpriteRenderer spriteRenderer;
    private LevelManager levelManager; // protected
    private PowerManager powerManager;
    private ScoreManager scoreManager;
    //private bool isBreakable;
    private GameObject collidingScorer;

    void Awake()
    {
        levelManager = GameObject.FindObjectOfType<LevelManager>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
    }

    private void Start()
    {
        LevelManager.breakableCount++;
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        LoadSprites();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collidingScorer = collision.gameObject;
        AudioSource.PlayClipAtPoint(HitBrick, transform.position);
        HandleHits();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Check if Bolt
        if (collider.gameObject.tag == "Bolt")
        {
            collidingScorer = collider.gameObject;
            AudioSource.PlayClipAtPoint(HitBrick, transform.position);
            HandleHits();
        }
        if(collider.name == "Fast Ball")
        {
            collidingScorer = collider.gameObject;
            MyDestroy();
        }
    }

    // HandleHits Method
    void HandleHits()
    {
        // Check Power
        if (powerManager.currentPower == PowerManager.Power.breaker)
        {
            // Check Strength and add damage
            if (powerManager.powerStrength == PowerManager.Strength.x1)
            {
                hits -= 2; //4;
            }
            else if (powerManager.powerStrength == PowerManager.Strength.x2)
            {
                hits -= 3; //8;
            }
            else if (powerManager.powerStrength == PowerManager.Strength.x3)
            {
                hits -= 12;
                Explode();
            }
        }
        else
        {
            hits--;
        }

        // Check Hits
        if (hits <= 0)
        {
            MyDestroy();
        }
        else
        {
            // Add new score and score text
            HandleScore();
            LoadSprites();
        }

        /*hits--;
        if (powerManager.currentPower == PowerManager.Power.breaker || hits <= 0)
        {
            MyDestroy();
            //Destroy(gameObject);
        }
        else
        {
            // Add new score and score text
            int newScore = scoreManager.SetScore(scoreManager.hitBrickPoints, scoreManager.standardMultiplierAdditive);
            Instantiate(scoreText, transform.position, transform.rotation).GetComponent<TextMesh>().text = "+" + newScore.ToString();
            LoadSprites();
        }*/
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

        /*
        Vector3 direction = transform.position - collidingScorer.transform.position;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0) { } // collision is to the right
            else { } // collision is to the left
        }
        else
        {
            if (direction.y > 0) { } // collision is up
            else {  } // collision is down
        }*/
    }

    /*void SimulateWin()
    {
        levelManager.LoadNextLevel();
    }*/

    // LoadSprites Method
    private void LoadSprites()
    {
        if (hits >= 3) { this.spriteRenderer.sprite = summerHit3; }
        else if (hits == 2) { this.spriteRenderer.sprite = summerHit2; }
        else if (hits <= 1) { this.spriteRenderer.sprite = summerHit1; }

        //int spriteIndex = timesHit - 1;
        /*switch (levelManager.season)
        {
            case (int)LevelManager.Season.SUMMER:
                {
                    if (hits >= 3) { this.spriteRenderer.sprite = summerHit3; }
                    else if (hits == 2) { this.spriteRenderer.sprite = summerHit2; }
                    else if (hits <= 1) { this.spriteRenderer.sprite = summerHit1; }
                }
                break;
            case (int)LevelManager.Season.AUTUMN:
                {
                    if (hits >= 3) { this.spriteRenderer.sprite = autumnHit3; }
                    else if (hits == 2) { this.spriteRenderer.sprite = autumnHit2; }
                    else if (hits <= 1) { this.spriteRenderer.sprite = autumnHit1; }
                }
                break;
            case (int)LevelManager.Season.WINTER:
                {
                    if (hits >= 3) { this.spriteRenderer.sprite = winterHit3; }
                    else if (hits == 2) { this.spriteRenderer.sprite = winterHit2; }
                    else if (hits <= 1) { this.spriteRenderer.sprite = winterHit1; }
                }
                break;
            case (int)LevelManager.Season.SPRING:
                {
                    if (hits >= 3) { this.spriteRenderer.sprite = autumnHit3; }
                    else if (hits == 2) { this.spriteRenderer.sprite = autumnHit2; }
                    else if (hits <= 1) { this.spriteRenderer.sprite = autumnHit1; }
                }
                break;
        }*/

        /*
        if (hits >= 4) { this.spriteRenderer.color = Color.magenta; }
        else if (hits == 3) { this.spriteRenderer.color = Color.red; }
        else if (hits == 2) { this.spriteRenderer.color = Color.yellow; }
        else if (hits <= 1) { this.spriteRenderer.color = Color.green; }*/


        /*if (hits >= 8) { this.spriteRenderer.sprite = hit8; }
        else if (hits == 7) { this.spriteRenderer.sprite = hit7; }
        else if (hits == 6) { this.spriteRenderer.sprite = hit6; }
        else if (hits == 5) { this.spriteRenderer.sprite = hit5; }
        else if (hits == 4) { this.spriteRenderer.sprite = hit4; }
        else if (hits == 3) { this.spriteRenderer.sprite = hit3; }
        else if (hits == 2) { this.spriteRenderer.sprite = hit2; }
        else if (hits <= 1) { this.spriteRenderer.sprite = hit1; }*/

        // Set sprite
        /*if (hitSprites[spriteIndex])
        {
            this.GetComponent<SpriteRenderer>().sprite = hitSprites[spriteIndex];
        }*/
    }

    public override void MyDestroy()
    {
        HandleScore();
        //levelManager.BrickDestroyed();
        //Vector3 center = (GetComponent<SpriteRenderer>().bounds.center) / 18;
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

    private void OnDestroy()
    {
        if (levelManager.levelStart)
        {
            levelManager.BrickDestroyed();
        }
    }

    public void Explode()
    {
        //Debug.Log("Explode");
        //Vector3 center = (GetComponent<SpriteRenderer>().bounds.center); /// 18;
        //AudioSource.PlayClipAtPoint(crack, transform.position);
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
