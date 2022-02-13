using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tworzenie rakiet, zmiana poziomów.
/// </summary>
public class EnemyController : MonoBehaviour
{
    public int level = 1;
    public bool isLevelEnd = false;

    private static float missilesAmountForLevel = 15;
    private static float frequencyOfSpawnMissiles = 1.5f;
    private static float timeForLevelToEnd = 5;

    private static float xRangeForMissiles = 8.5f;

    private float missileTimer = 0;
    private float endTimer = 0;

    private float missilesAmountAtCurrentLevel = 0;

    //references
    private GameObject MissilesSpawnZone;
    private GameObject missilePrefab;
    public UIController uiController;

    private void Start()
    {
        MissilesSpawnZone = transform.Find("MissilesSpawnZone").gameObject;
        missilePrefab = Resources.Load<GameObject>("Missile");
    }

    private void FixedUpdate()
    {
        if (missileTimer <= 0 && endTimer < 0 && !isLevelEnd)
        {
            SpawnMissiles();
            missileTimer = frequencyOfSpawnMissiles;
        }

        missileTimer -= Time.deltaTime;
        endTimer -= Time.deltaTime;

        if (missilesAmountAtCurrentLevel >= missilesAmountForLevel && endTimer <= 0 && isLevelEnd)
        {
            ShowResults();
        }
        else if(missilesAmountAtCurrentLevel >= missilesAmountForLevel && !isLevelEnd)
        {
            isLevelEnd = true;
            endTimer = timeForLevelToEnd;
        }
    }

    private void SpawnMissiles()
    {
        float xPos;
        float rotation;
        xPos = Random.Range(-xRangeForMissiles, xRangeForMissiles);
        if (xPos < 0)
        {
            rotation = Random.Range(-45, -135 + (45 * (xPos / -xRangeForMissiles)));
        }
        else
        {
            rotation = Random.Range(-45 - (45 * (xPos / xRangeForMissiles)), -135);
        }

        GameObject newMissile = Instantiate(missilePrefab, new Vector3(xPos, MissilesSpawnZone.transform.position.y, -1), Quaternion.Euler(0, 0, rotation));
        Vector3 direction = Quaternion.AngleAxis(rotation, Vector3.forward) * Vector2.right;
        newMissile.GetComponent<Missile>().SetStats(direction, Missile.ObjectOwner.Enemy);
        newMissile.GetComponent<Missile>().SetUIController(uiController);

        missilesAmountAtCurrentLevel++;
    }

    public void NextLevel()
    {
        level++;
        if (level <= 10)
        {
            missilesAmountAtCurrentLevel = 0;
            missilesAmountForLevel = 15 + level;
            frequencyOfSpawnMissiles = 1.5f - level / 10;
        }
        else
        {
            missilesAmountAtCurrentLevel = 0;
            missilesAmountForLevel = 15 + 10;
            frequencyOfSpawnMissiles = 1.5f - 10 / 10;
        }
        endTimer = 0;
        isLevelEnd = false;
    }

    private void ShowResults()
    {
        Debug.Log("ShowResults() from EC");
        uiController.ShowResultsOfLevel(level);
        this.gameObject.SetActive(false);
    }
}
