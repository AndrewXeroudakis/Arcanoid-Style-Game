using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{

    public static SaveManager saveManager;
    FileStream saveFile;

    public int experience = 1;
    public int xpPointsPrevious = 0;
    public int xpPoints = 0;
    public int xpPointsRequired = 1000;
    public int endScore = 0;
    public int endStage = 1;
    public int bestScore = 0;
    public int bestStage = 1;

    private GameObject expBar;
    private float a;
    private float b;
    private bool experienceBarAnimation = false;

    void Awake()
    {
        if (saveManager == null)
        {
            DontDestroyOnLoad(gameObject);
            saveManager = this;
        }
        else if (saveManager != this)
        {
            Destroy(gameObject);
            return;
        }

        Load();

        SetExperienceDisplay();
    }

    void Update()
    {
        /*if (experienceBarAnimation == true)
        {
            //expBar.GetComponent<Slider>().value = Mathf.Lerp(a, b, 0.0001f);
            if (expBar.GetComponent<Slider>().value < b) { expBar.GetComponent<Slider>().value += 0.01f; }
            else { expBar.GetComponent<Slider>().value = b; experienceBarAnimation = false; Debug.Log("value end = " + expBar.GetComponent<Slider>().value); }
        }*/
    }

    void SetExperienceDisplay()
    {
        Scene scene = SceneManager.GetActiveScene();

        if (scene.name == "Lose")
        {
            GameObject xpText = GameObject.FindGameObjectWithTag("XpPointsText");
            xpText.GetComponent<Text>().text = xpPoints.ToString() + "/" + xpPointsRequired.ToString();

            GameObject scoreText = GameObject.FindGameObjectWithTag("ScoreText");
            GameObject stageText = GameObject.FindGameObjectWithTag("StageText");
            GameObject bScoreText = GameObject.FindGameObjectWithTag("BestScoreText");
            GameObject bStageText = GameObject.FindGameObjectWithTag("BestStageText");

            scoreText.GetComponent<Text>().text = endScore.ToString();
            stageText.GetComponent<Text>().text = endStage.ToString();
            bScoreText.GetComponent<Text>().text = bestScore.ToString();
            bStageText.GetComponent<Text>().text = bestStage.ToString();
        }

        if (scene.name != "Game")
        {
            GameObject expText = GameObject.FindGameObjectWithTag("ExperienceText");
            expText.GetComponent<Text>().text = "level " + experience.ToString();

            expBar = GameObject.FindGameObjectWithTag("ExperienceBar");
            expBar.GetComponent<Slider>().value = (float)System.Math.Round((double)xpPoints / xpPointsRequired, 2, System.MidpointRounding.ToEven);
            /*a = (float)System.Math.Round((double)xpPointsPrevious / xpPointsRequired, 2, System.MidpointRounding.ToEven);
            b = (float)System.Math.Round((double)xpPoints / xpPointsRequired, 2, System.MidpointRounding.ToEven);
            Debug.Log("a = " + a);
            Debug.Log("b = " + b);
            Debug.Log("value start = " + expBar.GetComponent<Slider>().value);
            expBar.GetComponent<Slider>().value = a;
            Debug.Log("value = " + expBar.GetComponent<Slider>().value);
            experienceBarAnimation = true;*/
        }
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(Application.persistentDataPath + "/savedData.dat"))
        { saveFile = File.Open(Application.persistentDataPath + "/savedData.dat", FileMode.Open); }
        else { saveFile = File.Create(Application.persistentDataPath + "/savedData.dat"); }

        SaveData saveData = new SaveData();
        saveData.experience = experience;
        saveData.xpPointsPrevious = xpPointsPrevious;
        saveData.xpPoints = xpPoints;
        saveData.xpPointsRequired = xpPointsRequired;
        saveData.endScore = endScore;
        saveData.endStage = endStage;
        saveData.bestScore = bestScore;
        saveData.bestStage = bestStage;

        bf.Serialize(saveFile, saveData);
        saveFile.Close();
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/savedData.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream saveFile = File.Open(Application.persistentDataPath + "/savedData.dat", FileMode.Open);
            SaveData saveData = (SaveData)bf.Deserialize(saveFile);
            saveFile.Close();

            experience = saveData.experience;
            xpPointsPrevious = saveData.xpPointsPrevious;
            xpPoints = saveData.xpPoints;
            xpPointsRequired = saveData.xpPointsRequired;
            endScore = saveData.endScore;
            endStage = saveData.endStage;
            bestScore = saveData.bestScore;
            bestStage = saveData.bestStage;
        }
    }

    public void UpdateExperience()
    {
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        xpPointsPrevious = xpPoints;
        xpPoints += scoreManager.addedScore;

        while (xpPoints >= xpPointsRequired)
        {
            experience += 1;

            xpPoints = xpPoints - xpPointsRequired;

            xpPointsRequired *= 2;
        }

        //Debug.Log("Level = " + experience);
        //Debug.Log("xpPoints = " + xpPoints);
        //Debug.Log("xpPointsRequired = " + xpPointsRequired);

        //Debug.Log("Score = " + endScore);
        //Debug.Log("Stage = " + endStage);
        //Debug.Log("BestScore = " + bestScore);
        //Debug.Log("BestStage = " + bestStage);
    }
}

[Serializable]
class SaveData
{
    public int experience;
    public int xpPointsPrevious;
    public int xpPoints;
    public int xpPointsRequired;
    public int endScore;
    public int endStage;
    public int bestScore;
    public int bestStage;
}
