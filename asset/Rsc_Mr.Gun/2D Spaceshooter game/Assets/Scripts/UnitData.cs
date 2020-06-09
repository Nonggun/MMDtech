using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitData
{
    public string unitName;
    public float health;
    public float maxHealth;
    public float speed;
    public float collisionDamage;
    public float baseDamage;

    public UnitData()
    {
        unitName = "Default";
        health = 1.0f;
        maxHealth = 1.0f;
        speed = 1.0f;
        collisionDamage = 1.0f;
        baseDamage = 1.0f;
    }
    public static UnitData CreateBaseUnit()
    {
        return new UnitData();
    }

}