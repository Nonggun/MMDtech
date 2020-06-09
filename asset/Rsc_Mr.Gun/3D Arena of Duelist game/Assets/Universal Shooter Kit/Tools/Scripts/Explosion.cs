// GercStudio
// © 2018-2019

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GercStudio.USK.Scripts
{

    public class Explosion : MonoBehaviour
    {
        [HideInInspector] public float Radius = 2;
        [HideInInspector] public float Force = 100;
        [HideInInspector] public float Time = 1;

        [HideInInspector] public int damage;
        [HideInInspector] public int instanceId;

//        [HideInInspector] public PUNHelper.Teams OwnerTeam;
//        [HideInInspector] public string OwnerName;
        [HideInInspector] public bool ApplyForce;
        [HideInInspector] public Controller Owner;

        [HideInInspector] public Texture WeaponImage;

        private List<int> charactersIds = new List<int>{-1};

        private bool anyDamage;

        void Start()
        {
            ExplosionProcess();
//            Transform[] allChildren = GetComponentsInChildren<Transform>();
//            foreach (Transform child in allChildren)
//            {
//                if (child.GetInstanceID() != GetInstanceID())
//                {
//                    child.parent = null;
//                    child.gameObject.AddComponent<DestroyObject>().destroy_time = 5;
//                }
//            }
        }

        void ExplosionProcess()
        {
            var hitColliders = Physics.OverlapSphere(transform.position, Radius);
            
            foreach (var collider in hitColliders)
            {
                if (collider.transform.root.GetComponent<EnemyController>())
                {
                    var enemyScript = collider.transform.root.GetComponent<EnemyController>();
                    enemyScript.EnemyHealth -= damage;
                    enemyScript.GetShotFromWeapon(1.5f);
                    enemyScript.PlayDamageAnimation();
                    
                    break;
                }
                
                if (collider.GetComponent<Rigidbody>() && ApplyForce && !collider.transform.root.GetComponent<Controller>())
                    collider.GetComponent<Rigidbody>().AddExplosionForce(Force * 50, transform.position, Radius, 0.0f);
                
                if (collider.transform.root.GetComponent<Controller>())
                {
                    if (charactersIds.All(id => id != collider.transform.root.gameObject.GetInstanceID()))
                    {
                        charactersIds.Add(collider.transform.root.gameObject.GetInstanceID());

                        var controller = collider.transform.root.GetComponent<Controller>();

                        if (Owner)
                        {
                            switch (Owner.CanKillOthers)
                            {
                                case PUNHelper.CanKillOthers.OnlyOpponents:

                                    if (controller.MyTeam != Owner.MyTeam || controller.MyTeam == Owner.MyTeam && Owner.MyTeam == PUNHelper.Teams.Null)
                                    {
                                        if (controller.PlayerHealth - damage <= 0 && Owner.CharacterSync && controller != Owner)
                                        {
                                            Owner.CharacterSync.AddScore(PlayerPrefs.GetInt("ExplosionKill"), "explosion");
                                        }

                                        controller.ExplosionDamage(damage, Owner.CharacterName, WeaponImage ? WeaponImage : null, controller.oneShotOneKill);
                                    }

                                    break;

                                case PUNHelper.CanKillOthers.Everyone:

                                    if (controller.MyTeam != Owner.MyTeam || controller.MyTeam == Owner.MyTeam && Owner.MyTeam == PUNHelper.Teams.Null)
                                    {
                                        if (controller.PlayerHealth - damage <= 0 && Owner.CharacterSync && controller != Owner)
                                        {
                                            Owner.CharacterSync.AddScore(PlayerPrefs.GetInt("ExplosionKill"), "explosion");
                                        }
                                    }

                                    controller.ExplosionDamage(damage, Owner.CharacterName, WeaponImage ? WeaponImage : null, controller.oneShotOneKill);

                                    break;

                                case PUNHelper.CanKillOthers.NoOne:
                                    break;
                            }
                        }
                    }
                }

                if (collider.GetComponent<FlyingProjectile>() && collider.gameObject.GetInstanceID() != instanceId)
                {
                    collider.GetComponent<FlyingProjectile>().Explosion();
                    break;
                }
            }

            //Destroy(gameObject, Time);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }

}



