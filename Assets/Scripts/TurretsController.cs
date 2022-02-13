using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa do sterowania wie¿yczkami. Odpowiada za ich obracanie, stan oraz tworzenie rakiet.
/// </summary>
public class TurretsController : MonoBehaviour
{
    //enum
    public enum TurretType { Main, Supportive};
    public enum TurretNumber { One, Two, Three};

    //stats
    [SerializeField]
    private TurretType turType;
    [SerializeField]
    private TurretNumber turNumber;
    private Vector2 mousePosition;
    public bool isDestroyed = false;
    private bool canShoot = false;
    public int missileAmount;
    private float cannotShootTimer = 0;

    private static int missilesForLevel = 15;
    
    //references
    private GameObject turretObject;
    private GameObject missilePrefab;
    public UIController uiController;

    private void Start()
    {
        turretObject = transform.Find("Missile_battery_turret").gameObject;
        missilePrefab = Resources.Load<GameObject>("Missile");
        missileAmount = missilesForLevel;
    }

    private void Update()
    {
        cannotShootTimer -= Time.deltaTime;

        MousePosition();

        if (!isDestroyed && 
            missileAmount > 0 &&
            canShoot &&
            cannotShootTimer < 0)
        {
            InputToAction();
        }   
    }

    /// <summary>
    /// Sprawdza pozycje myszy i obraca wie¿yczkê do niej
    /// </summary>
    private void MousePosition()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseToTurret = mousePosition - new Vector2(turretObject.transform.position.x, turretObject.transform.position.y);
        float angle = -Vector2.SignedAngle(mouseToTurret, new Vector2(1, 0));

        if (angle <=0 && angle >= -90)
        {
            angle = 0;
            canShoot = false;
        }
        else if(angle >= -180 && angle <= -90)
        {
            angle = 180;
            canShoot = false;
        }
        else
        {
            canShoot = true;
        }

        if (!isDestroyed &&
            missileAmount > 0 &&
            canShoot &&
            cannotShootTimer < 0)
        {
            turretObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
    }

    private void InputToAction()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && turNumber == TurretNumber.One)
        {
            Shoot(Missile.MissileType.PlayerSupportiveMissile);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && turNumber == TurretNumber.Two)
        {
            Shoot(Missile.MissileType.PlayerMainMissile);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && turNumber == TurretNumber.Three)
        {
            Shoot(Missile.MissileType.PlayerSupportiveMissile);
        }
    }

    private void Shoot(Missile.MissileType type)
    {
        Vector3 direction = (mousePosition - new Vector2(turretObject.transform.position.x, turretObject.transform.position.y)).normalized;

        GameObject newMissile = Instantiate(missilePrefab, turretObject.transform.position, turretObject.transform.rotation);
        newMissile.GetComponent<Missile>().SetStats(mousePosition, direction, Missile.ObjectOwner.Player, type);
        newMissile.GetComponent<Missile>().SetUIController(uiController);

        missileAmount--;
        cannotShootTimer = 0.5f;
    }

    public void DestroyBattery()
    {
        transform.Find("Missile_battery_turret").gameObject.SetActive(false);
        GetComponent<TurretsController>().isDestroyed = true;
        GetComponent<BoxCollider2D>().enabled = false;
    }

    public void RestoreAndReload()
    {
        missileAmount = missilesForLevel;
        transform.Find("Missile_battery_turret").gameObject.SetActive(true);
        transform.GetComponent<TurretsController>().isDestroyed = false;
        GetComponent<BoxCollider2D>().enabled = true;
    }
}
