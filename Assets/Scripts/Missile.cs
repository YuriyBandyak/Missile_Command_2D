using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa rakietowa. Zaimplementowane funkcje przemieszczenia, wybuchu i zderzeñ.
/// </summary>
public class Missile : MonoBehaviour
{
    //enum
    public enum ObjectOwner { Player, Enemy }
    public enum MissileType { EnemyMissile, PlayerMainMissile, PlayerSupportiveMissile}

    //stats
    [SerializeField]
    private MissileType type = MissileType.EnemyMissile;
    private ObjectOwner owner = ObjectOwner.Player;
    private Vector3 targetPosition;
    private Vector3 direction;
    private bool isMoving = true;
    private float currentVelocity = 0;
    private float currentDeep = 0;

    public static float missileVelocityMain = 5f;
    public static float missileVelocitySupp = 4f;
    public static float missileVelocityEnemy = 3f;
    public static float playerMissilesDeep = -1;
    public static float enemyMissilesDeep = -3;

    //references
    private GameObject explosionPrefab;
    private UIController uiController;

    public Missile(Vector3 targetPosition, Vector3 direction, ObjectOwner owner)
    {
        this.targetPosition = targetPosition;
        this.direction = direction;
        this.owner = owner;
    }

    public void SetStats(Vector3 targetPosition, Vector3 direction, ObjectOwner owner, MissileType type)
    {
        this.targetPosition = targetPosition;
        this.direction = direction;
        this.owner = owner;
        this.type = type;
        switch (type)
        {
            case MissileType.EnemyMissile:
                currentVelocity = missileVelocityEnemy;
                currentDeep = enemyMissilesDeep;
                break;
            case MissileType.PlayerMainMissile:
                currentVelocity = missileVelocityMain;
                break;
            case MissileType.PlayerSupportiveMissile:
                currentVelocity = missileVelocitySupp;
                break;
        }
    }

    public void SetStats(Vector3 direction, ObjectOwner owner)
    {
        this.direction = direction;
        this.owner = owner;
        if (owner == ObjectOwner.Enemy) type = MissileType.EnemyMissile;
        switch (type)
        {
            case MissileType.EnemyMissile:
                currentVelocity = missileVelocityEnemy;
                currentDeep = enemyMissilesDeep;
                break;
            case MissileType.PlayerMainMissile:
                currentVelocity = missileVelocityMain;
                break;
            case MissileType.PlayerSupportiveMissile:
                currentVelocity = missileVelocitySupp;
                break;
        }
    }

    public void SetUIController(UIController controller)
    {
        this.uiController = controller;
    }

    private void Start()
    {
        explosionPrefab = Resources.Load<GameObject>("Explosion");
        if (currentDeep ==0 )currentDeep = playerMissilesDeep;
        isMoving = true;
    }

    private void FixedUpdate()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (isMoving)
        {
            transform.position += direction * currentVelocity * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, transform.position.y, currentDeep);
        }
        
        if (Vector2.Distance(transform.position, targetPosition) < .5f && GetComponent<SpriteRenderer>().enabled)
        {
            ExplodeMissile(this);
        }
    }

    private void ExplodeMissile(Missile missile)
    {
        GameObject newExplosion;
        if (missile.targetPosition != null && missile.targetPosition != Vector3.zero)
        {
            newExplosion = Instantiate(explosionPrefab, missile.targetPosition, Quaternion.identity);
        }
        else
        {
            newExplosion = Instantiate(explosionPrefab, missile.transform.position, Quaternion.identity);
        }
        Object.Destroy(this.gameObject);
        Object.Destroy(newExplosion, 0.5f);
    }

    private void ExplodeMissile(Vector3 position)
    {
        GameObject newExplosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        Object.Destroy(newExplosion, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == ObjectOwner.Enemy)
        {
            switch (collision.tag)
            {
                case "Missile_battery":
                    collision.GetComponent<TurretsController>().DestroyBattery();
                    ExplodeMissile(this);
                    break;
                case "City":
                    collision.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Destroyed_City_Sprite");
                    collision.GetComponent<BoxCollider2D>().enabled = false;
                    Object.Destroy(this.gameObject);
                    ExplodeMissile(collision.transform.position);
                    break;
                case "Explosions":
                    Destroy(this.gameObject);

                    if (owner == ObjectOwner.Enemy)
                    {
                        uiController.AddPointsToScore(15);
                    }
                    break;
            }
        }

        switch (collision.gameObject.name)
        {
            case "Terrain":
                ExplodeMissile(this);
                break;

            case "Missile":
                if (this.owner == ObjectOwner.Player && collision.gameObject.GetComponent<Missile>().owner != this.owner)
                {
                    uiController.AddPointsToScore(15);
                    Destroy(collision.gameObject);
                    Destroy(this.gameObject);
                    ExplodeMissile(transform.position);
                }
                break;
            case "Missile(Clone)":
                if (this.owner == ObjectOwner.Player && collision.gameObject.GetComponent<Missile>().owner != this.owner)
                {
                    uiController.AddPointsToScore(15);
                    Destroy(collision.gameObject);
                    Destroy(this.gameObject);
                    ExplodeMissile(transform.position);
                }
                break;
        }
    }
}
