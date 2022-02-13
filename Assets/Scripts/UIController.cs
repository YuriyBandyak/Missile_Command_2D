using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Klasa do zarz¹dzania interfejsem. Odpowiada za przyciski, przemieszczenie kamery oraz stan gry.
/// </summary>
public class UIController : MonoBehaviour
{
    //stats
    public int playerScore = 0;
    public float showResultsOfLevelTimer = 0;
    private bool isGameOver = false;
    private int restoredCities = 0;

    //references
    public GameObject enemyController;
    public Text scoreText, levelText;

    public GameObject inGameCanvasObject, theEndCanvasObject;
    public Canvas inGameCanvas, endOfLevelCanvas, afterDeathCanvas, mainMenuCanvas;

    public TextMeshProUGUI citiesAmount, citiesScore, missileAmount, missileScore, 
        batteriesAmount, batteriesScore, previousScore, newScore, finalScore;

    public GameObject[] cities = new GameObject[6];
    public GameObject[] batteries = new GameObject[3];

    //Camera
    public GameObject cam;
    private bool isCamMoving = false;
    private Vector3 camStartPos;
    private Vector3 camGamePos = new Vector3(0, -18f, -10f);
    private Vector3 camTargetPos;
    private float camSpeed = 0.1f;

    private void Start()
    {
        camStartPos = cam.transform.position;
        camTargetPos = camStartPos;
        levelText.text = enemyController.GetComponent<EnemyController>().level.ToString();
    }

    private void FixedUpdate()
    {
        if (isGameOver)
        {
            cam.transform.position = Vector3.Lerp(cam.transform.position, camStartPos, camSpeed);
        }
        else
        {
            showResultsOfLevelTimer -= Time.deltaTime;

            if (!isCamMoving && showResultsOfLevelTimer < 0 && !enemyController.gameObject.activeInHierarchy && enemyController.GetComponent<EnemyController>().isLevelEnd)
            {
                MoveCam();
                RestoringCitiesAndBatteries();
                enemyController.GetComponent<EnemyController>().NextLevel();
            }

            if (isCamMoving && cam.transform.position != camTargetPos)
            {
                cam.transform.position = Vector3.Lerp(cam.transform.position, camTargetPos, camSpeed);
                enemyController.SetActive(false);
                inGameCanvasObject.SetActive(false);
                endOfLevelCanvas.gameObject.SetActive(false);
                mainMenuCanvas.gameObject.SetActive(false);
            }

            IsCamFinishMoving();
        }
    }

    private void IsCamFinishMoving()
    {
        if (isCamMoving && cam.transform.position == camTargetPos)
        {
            isCamMoving = false;
            if (cam.transform.position == camGamePos)
            {
                enemyController.SetActive(true);
                inGameCanvasObject.SetActive(true);
                endOfLevelCanvas.gameObject.SetActive(false);
                mainMenuCanvas.gameObject.SetActive(false);
            }
            else if (cam.transform.position == camStartPos && enemyController.GetComponent<EnemyController>().level != 0)
            {
                enemyController.SetActive(false);
                inGameCanvasObject.SetActive(false);
                endOfLevelCanvas.gameObject.SetActive(true);
                mainMenuCanvas.gameObject.SetActive(false);
                showResultsOfLevelTimer = 10;
            }
            else if (cam.transform.position == camStartPos && enemyController.GetComponent<EnemyController>().level == 0)
            {
                enemyController.SetActive(false);
                inGameCanvasObject.SetActive(false);
                endOfLevelCanvas.gameObject.SetActive(false);
                mainMenuCanvas.gameObject.SetActive(true);
            }
        }
    }

    public void MoveCam()
    {
        Debug.Log("MoveCam");
        if (camTargetPos == camStartPos)
        {
            camTargetPos = camGamePos;
        }
        else
        {
            camTargetPos = camStartPos;
        }
        isCamMoving = true;
    }

    public void AddPointsToScore(int points)
    {
        playerScore += points;
        scoreText.text = playerScore.ToString();
    }

    public void ShowResultsOfLevel(int number)
    {
        Debug.Log("NextLevelInUI()");

        levelText.text = number.ToString();
        endOfLevelCanvas.gameObject.SetActive(true);

        int savedCities = 0;
        int savedMissiles = 0;
        int savedBatteries = 0;

        foreach (GameObject city in cities)
        {
            if (city.GetComponent<BoxCollider2D>().enabled) savedCities++;
        }

        foreach (GameObject battery in batteries)
        {
            savedMissiles += battery.GetComponent<TurretsController>().missileAmount;
        }

        foreach (GameObject battery in batteries)
        {
            if (battery.transform.Find("Missile_battery_turret").gameObject.activeInHierarchy) savedBatteries++;
        }

        if (savedCities == 0)
        {
            TheEnd();
            return;
        }

        citiesAmount.SetText(savedCities.ToString());
        citiesScore.SetText((savedCities * 50).ToString());

        missileAmount.SetText(savedMissiles.ToString());
        missileScore.SetText((savedMissiles * 10).ToString());

        batteriesAmount.SetText(savedBatteries.ToString());
        batteriesScore.SetText((savedBatteries * 100).ToString());

        previousScore.SetText(playerScore.ToString());

        playerScore += (savedCities * 50) + (savedMissiles * 10) + (savedBatteries * 100);
        newScore.SetText(playerScore.ToString());

        showResultsOfLevelTimer = 10;

        MoveCam();
    }

    public void RestoringCitiesAndBatteries()
    {
        
        foreach (GameObject battery in batteries)
        {
            battery.GetComponent<TurretsController>().RestoreAndReload();
        }

        int savedCities = 0;
        int canBeRestored = (playerScore / 4000) - restoredCities;

        foreach (GameObject city in cities)
        {
            if (city.GetComponent<BoxCollider2D>().enabled) savedCities++;
        }

        foreach (GameObject city in cities)
        {
            if (canBeRestored > 0 && savedCities != 6)
            {
                if (!city.GetComponent<BoxCollider2D>().enabled)
                {
                    city.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("City");
                    city.GetComponent<BoxCollider2D>().enabled = true;
                    restoredCities++;
                    savedCities++;
                    canBeRestored--;
                }
            }
        }

        if (savedCities != 6 && canBeRestored > 0) RestoringCitiesAndBatteries();
    }

    private void TheEnd()
    {
        Debug.Log("THE END");
        isGameOver = true;
        theEndCanvasObject.SetActive(true);
        enemyController.SetActive(false);
        inGameCanvasObject.SetActive(false);
        endOfLevelCanvas.gameObject.SetActive(false);
        mainMenuCanvas.gameObject.SetActive(false);

        finalScore.SetText(playerScore.ToString());
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void ExitFromGame()
    {
        Application.Quit();
    }
}
