using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviorA1 : MonoBehaviour
{

    public UnitData unitData;
    // We need rigidBody2D to make a movement 
    public Rigidbody2D rigidbody2d;
    // We need Collider2D to allow collision detection 
    public Collider2D collider2d;

    public GameObject target;

    // Start is called before the first frame update

    void Start()
    {
        if (!rigidbody2d)
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
        }
        if (!collider2d)
        {
            collider2d = GetComponent<Collider2D>();
        }

        SetupUnitData();
    }

    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            DetectEnemy();
        }
        else
        {
            FollowTarget();
        }

    }

    public void SetupUnitData()
    { // Reference Playground HealthSystemAttribute component 
        HealthSystemAttribute healthSystem = GetComponent<HealthSystemAttribute>();
        if (healthSystem)
        {
            healthSystem.maxHealth = (int)unitData.maxHealth;
            healthSystem.health = (int) unitData.health;
        }
        ModifyHealthAttribute modifyHealthAttrb = GetComponent<ModifyHealthAttribute>();
        if (modifyHealthAttrb)
        {
            modifyHealthAttrb.healthChange = (int)-unitData.collisionDamage;
        }

    }

    public void DetectEnemy()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }

    void FollowTarget()
    { // Follow Target 
        transform.position = Vector2.MoveTowards( 
            transform.position, 
            target.transform.position, 
            unitData.speed * Time.deltaTime);
    }

    }
