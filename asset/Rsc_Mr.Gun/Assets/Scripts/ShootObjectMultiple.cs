using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootObjectMultiple : MonoBehaviour
{
    public GameObject prefabToSpawn;
    private int playerId;
    // The key to press to create the objects/projectiles 
    // Only if you want to do manual control 
    public KeyCode keyToPress = KeyCode.Space;
    // The rate of creation, as long as the key is pressed 
    public float creationRate = .5f;
    // The speed at which the object are shot along the Y axis 
    public float shootSpeed = 5f;
    // Relative direction to each shooter point 
    public Vector2 shootDirection = new Vector2(1f, 1f);

    public bool relativeToRotation = true;
    public bool EnableShootByKey;
    public List<Transform> shooterPoints;
    private float timeOfLastSpawn;

    // Start is called before the first frame update
    void Start()
    {
        timeOfLastSpawn = -creationRate;

    }

    // Update is called once per frame
    void Update()
    {
        if (EnableShootByKey)
        {
            if (Input.GetKey(keyToPress))
            {
                ShootObject();
            }
        }

    }

    public void ShootObject()
    {
        
        // Check the cooldown 
        if (Time.time >= timeOfLastSpawn + creationRate)
        {
            foreach (Transform shooterTransform in shooterPoints)
            {
                Vector2 actualBulletDirection = (relativeToRotation) ? (Vector2)(Quaternion.Euler(0, 0, shooterTransform.eulerAngles.z) * shootDirection) : shootDirection;
                GameObject newObject = Instantiate<GameObject>(prefabToSpawn);
                newObject.transform.position = shooterTransform.position;
                newObject.transform.eulerAngles = new Vector3(0f, 0f, Utils.Angle(actualBulletDirection)); newObject.tag = "Bullet";

                // push the created objects, but only if they have a Rigidbody2D 
                Rigidbody2D rigidbody2D = newObject.GetComponent<Rigidbody2D>();
                if (rigidbody2D != null)
                {
                    // Impulse means immediate 
                    rigidbody2D.AddForce(actualBulletDirection * shootSpeed, ForceMode2D.Impulse);
                    // add a Bullet component if the prefab doesn't already have one, and assign the player ID
                    BulletAttribute b = newObject.GetComponent<BulletAttribute>();
                    if (b == null)
                    {
                        b = newObject.AddComponent<BulletAttribute>();
                    }
                    b.playerId = playerId;
                    }
            }
            // Set the time we created object for cooldown reference timeOfLastSpawn = Time.time;
        }
    }
}
