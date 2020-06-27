using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderBottom : MonoBehaviour {

    public int lives;

    private Ball ball;
    private PowerManager powerManager;
    private ScoreManager scoreManager;

    private void Start()
    {
        ball = GameObject.FindObjectOfType<Ball>();
        powerManager = GameObject.FindObjectOfType<PowerManager>();
        scoreManager = GameObject.FindObjectOfType<ScoreManager>();
    }

    // Access Level Manager
    public LevelManager levelManager;

    // When colliding with ball
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            if (powerManager.currentPower == PowerManager.Power.dissolve)
            {
                GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");

                if (balls.Length <= 1)
                {
                    LoseLife();
                }
                else
                {
                    Destroy(collision.gameObject);
                }
            }
            else
            {
                LoseLife();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<Enemy>().MyDestroy();
        }
    }

    void LoseLife()
    {
        // Reduce Lives
        if (powerManager.currentPower == PowerManager.Power.player && powerManager.powerStrength == PowerManager.Strength.x3) { }
        else { lives--; }
        scoreManager.goalkeeper = false;

        // Reset powerManager.currentPower
        powerManager.ResetPower();

        // Reset score multiplier
        scoreManager.SetScoreMultiplier();
        scoreManager.ResetScoreHits();

        // Check Lives
        if (lives < 0)
        {
            levelManager.LoseLevel();
        }
        else
        {
            ball = GameObject.FindObjectOfType<Ball>();
            ball.ResetBall();
        }
    }
}
