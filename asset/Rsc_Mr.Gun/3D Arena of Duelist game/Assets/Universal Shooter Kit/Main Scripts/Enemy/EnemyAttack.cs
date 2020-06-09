// GercStudio
// © 2018-2019

using System.Collections;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    public class EnemyAttack : MonoBehaviour
    {
        private bool attackAudioPlay;

        private RaycastHit Hit;
        
        public EnemyController EnemyController;

        private AudioSource _audio;

        private AIHelper.EnemyAttack _attack;

        public bool FlashEffectTimeout;

        private void Start()
        {
            _audio = GetComponent<AudioSource>();
            
            _attack = EnemyController.Attacks[0];
        }

        void Update()
        {
            if (_attack != null)
            {
                if (_attack.AttackType == AIHelper.AttackTypes.Fire && !EnemyController.anim.GetBool("Attack"))
                    
                {
                    if (_audio.isPlaying)
                    {
                        attackAudioPlay = false;
                        _audio.Stop();
                    }

                    if (_attack.DamageColliders.Count > 0)
                    {
                        foreach (var damageCollider in _attack.DamageColliders.Where(collider => collider))
                        {
                            if (!damageCollider.isTrigger)
                                damageCollider.isTrigger = true;
                                
                            if (damageCollider.enabled)
                                damageCollider.enabled = false;
                        }
                    }
                }

                if (_attack.AttackType == AIHelper.AttackTypes.Melee && !EnemyController.anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
                {
                    if (_attack.DamageColliders.Count > 0)
                    {
                        foreach (var damageCollider in _attack.DamageColliders.Where(collider => collider))
                        {
                            if (!damageCollider.isTrigger)
                                damageCollider.isTrigger = true;
                                
                            if (damageCollider.enabled)
                                damageCollider.enabled = false;
                        }
                    }
                }
            }
        }

        public IEnumerator ReloadTimeout()
        {
            yield return new WaitForSeconds(0.5f);
            EnemyController.anim.SetBool("Reload", false);
            StopCoroutine(ReloadTimeout());
        }
        
        public IEnumerator FlashGrenadeEffect()
        {
            yield return new WaitForSeconds(3);
            EnemyController.InSmoke = false;
            FlashEffectTimeout = false;
            StopCoroutine(FlashGrenadeEffect());
        }

        public void Attack(AIHelper.EnemyAttack Attack)
        {
            switch (Attack.AttackType)
            {
                case AIHelper.AttackTypes.Bullets:
                    BulletsAttack(Attack);
                    break;
                case AIHelper.AttackTypes.Rockets:
                    RocketsAttack(Attack);
                    break;
                case AIHelper.AttackTypes.Fire:
                    FireAttack(Attack);
                    break;
                case AIHelper.AttackTypes.Melee:
//                    MeleeAttack(Attack);
                    break;
            }
        }

        void RocketsAttack(AIHelper.EnemyAttack Attack)
        {
            if (Attack.AttackAudio)
                _audio.PlayOneShot(Attack.AttackAudio);
            
            if (Attack.AttackSpawnPoints.Count > 0)
            {
                if (Attack.UseReload)
                {
                    Attack.CurrentAmmo -= 1;
                }

                for (var i = 0; i < Attack.AttackSpawnPoints.Count; i++)
                {
                    if (Attack.AttackSpawnPoints[i])
                    {
                        var rocket = Instantiate(Attack.Rocket, Attack.AttackSpawnPoints[i].position, Attack.AttackSpawnPoints[i].rotation);
                        rocket.SetActive(true);

                        var RocketScript = rocket.AddComponent<FlyingProjectile>();

                        RocketScript.isRocket = true;
                        RocketScript.ApplyForce = true;
                        RocketScript.Speed = 20;
                        RocketScript.isEnemy = true;
                        //RocketScript.OwnerName = "Enemy";
                        RocketScript.damage = Attack.Damage;
                        RocketScript.isRaycast = true;
                        
                        if(Attack.Explosion)
                            RocketScript.explosion = Attack.Explosion.transform;
                        
                        RocketScript.TargetPoint = EnemyController.Players[0].player.GetComponent<Controller>().BodyObjects.TopBody.position +
                                                    new Vector3(Random.Range(-Attack.Scatter, Attack.Scatter), Random.Range(-Attack.Scatter, Attack.Scatter), 0);

                    }
                }
            }

        }
        
        void BulletsAttack(AIHelper.EnemyAttack Attack)
        {
            if (Attack.AttackAudio)
                _audio.PlayOneShot(Attack.AttackAudio);
            
            if (Attack.AttackSpawnPoints.Count > 0)
            {
                if (Attack.UseReload)
                {
                    Attack.CurrentAmmo -= 1;
                }
                
                for (var i = 0; i < Attack.AttackSpawnPoints.Count; i++)
                {
                    if (Attack.AttackSpawnPoints[i])
                    {
                        var Direction = Attack.AttackSpawnPoints[i].TransformDirection(Vector3.forward + new Vector3(Random.Range(-Attack.Scatter, Attack.Scatter), Random.Range(-Attack.Scatter, Attack.Scatter), 0));
                       
                        if (Attack.MuzzleFlash)
                        {
                            var Flash = Instantiate(Attack.MuzzleFlash, Attack.AttackSpawnPoints[i].position, Attack.AttackSpawnPoints[i].rotation);
                            Flash.transform.parent = Attack.AttackSpawnPoints[i].transform;
                            Flash.gameObject.AddComponent<DestroyObject>().DestroyTime = 0.17f;
                        }

                        
                        if (Physics.Linecast(Attack.AttackSpawnPoints[i].position, 
                            EnemyController.Players[0].player.GetComponent<Controller>().BodyObjects.TopBody.position + new Vector3(Random.Range(-Attack.Scatter, Attack.Scatter), Random.Range(-Attack.Scatter, Attack.Scatter), 0), out Hit))
                        {
                            var HitRotation = Quaternion.FromToRotation(Vector3.up, Hit.normal);
                            
                            var tracer = new GameObject("Tracer");
                            tracer.transform.position = Attack.AttackSpawnPoints[i].position;
                            tracer.transform.rotation = Attack.AttackSpawnPoints[i].rotation;
                            
                            WeaponsHelper.AddTrail(tracer, Hit.point, EnemyController.trailMaterial, 0.1f);

                            if (Hit.collider.transform.root.GetComponent<Controller>())
                            {
                                if (Hit.collider.transform.root.GetComponent<Controller>())
                                {
                                    var controller = Hit.collider.transform.root.GetComponent<Controller>();
                                    if (controller.WeaponManager.BloodProjector)
                                    {
                                        WeaponsHelper.CreateBlood(controller.WeaponManager.BloodProjector, Hit.point - Direction.normalized * 0.15f, Quaternion.LookRotation(Direction), Hit.transform, controller.BloodHoles);
                                    }

                                    controller.Damage(Attack.Damage, "Enemy", null, false);
                                }
                            }


                            if (Hit.transform.GetComponent<Rigidbody>())
                                Hit.transform.GetComponent<Rigidbody>().AddForceAtPosition(Direction * 500, Hit.point);

                            if (Hit.collider.GetComponent<Surface>())
                            {
                                var surface = Hit.collider.GetComponent<Surface>();
                                if (surface.Material)
                                {
                                    if (surface.Sparks & surface.Hit)
                                    {
                                        Instantiate(surface.Sparks, Hit.point + (Hit.normal * 0.01f), HitRotation);
                                        var hitGO = Instantiate(surface.Hit, Hit.point + (Hit.normal * 0.001f), HitRotation).transform;
                                        if (surface.HitAudio)
                                        {
                                            hitGO.gameObject.AddComponent<AudioSource>();
                                            hitGO.gameObject.GetComponent<AudioSource>().clip = surface.HitAudio;
                                            hitGO.gameObject.GetComponent<AudioSource>().PlayOneShot(hitGO.gameObject.GetComponent<AudioSource>().clip);
                                        }
                                        hitGO.parent = Hit.transform;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("(Enemy) <color=red>Missing components [AttackSpawnPoint]</Color>. Add it, otherwise the enemy won't shoot.", gameObject);
                    }
                }
            }
            else
            {
                Debug.LogError("(Enemy) <color=red>Missing components</color> [AttackSpawnPoint]. Add it, otherwise the enemy won't shoot.", gameObject);
            }
        }

        public void FireAttack(AIHelper.EnemyAttack Attack)
        {
            if (!attackAudioPlay)
            {
                attackAudioPlay = true;
                _audio.clip = Attack.AttackAudio;
                _audio.Play();
            }
            
            if (Attack.AttackSpawnPoints.Count > 0)
            {
                Attack.CurrentAmmo -= 1 * Time.deltaTime;
                for (var i = 0; i < Attack.AttackSpawnPoints.Count; i++)
                {
                    if (Attack.Fire)
                    {
                        var fire = Instantiate(Attack.Fire, Attack.AttackSpawnPoints[i].position, Attack.AttackSpawnPoints[i].rotation);
                        fire.gameObject.hideFlags = HideFlags.HideInHierarchy;
                        fire.gameObject.SetActive(true);
                    }

                    if (Attack.DamageColliders[i] && !Attack.DamageColliders[i].enabled)
                        Attack.DamageColliders[i].enabled = true;
                }
            }
        }

        public void MeleeColliders(string status)
        {
            var attack = EnemyController.Attacks[0];

            if (attack.DamageColliders.Count > 0)
            {
                foreach (var collider in attack.DamageColliders.Where(collider => collider))
                {
                    collider.enabled = status == "on";
                }
            }
        }
    }

}





 

		




