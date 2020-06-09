using System;
using System.Collections;
using System.Collections.Generic;
using GercStudio.USK.Scripts;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace GercStudio.USK.Scripts
{

    public class BodyPartCollider : MonoBehaviour
    {
        [HideInInspector] public EnemyController EnemyController;
        [HideInInspector] public Controller Controller;

        public enum BodyPart
        {
            Head,
            Hands,
            Legs,
            Body
        }

        [HideInInspector] public BodyPart bodyPart;
        [HideInInspector] public bool checkColliders;
        [HideInInspector] public bool gettingDamage;
        [HideInInspector] public string attackType;
        [HideInInspector] public float damageMultiplayer = 2;
        [HideInInspector] public GameObject attacking;
        [HideInInspector] public bool registerDeath;

        private void Update()
        {
            gettingDamage = false;
            attackType = "";
            attacking = null;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Fire"))
            {
                gettingDamage = true;
                attackType = "Fire";
                attacking = other.transform.root.gameObject;
            }

            if (!checkColliders)
                return;

            if (EnemyController && other.CompareTag("Smoke"))
            {
                EnemyController.InSmoke = true;
            }
            
//            if (other.CompareTag("Fire"))
//            {
//                if (EnemyController)
//                {
//                    if (other.transform.root.GetComponent<Controller>())
//                    {
//                        var weaponController = other.transform.root.GetComponent<Controller>().WeaponManager.WeaponController;
//                        if (weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame)
//                        {
//                            EnemyController.EnemyHealth -= weaponController.Attacks[weaponController.currentAttack].weapon_damage * Time.deltaTime;
//
//                            EnemyController.PlayDamageAnimation();
//                            
//                            EnemyController.GetShotFromWeapon(0.1f);
//                        }
//                    }
//                }
//                else if (Controller)
//                {
//                    if (other.transform.root.gameObject.GetComponent<EnemyController>())
//                    {
//                        Controller.PlayerHealth -= other.transform.root.GetComponent<EnemyController>().Attacks[0].Damage * Time.deltaTime;
//                        
//                        if (Controller.PlayerHealth <= 0)
//                        {
//                            Controller.KillerName = "Enemy";
//                        }
//                    }
//                    else if (other.transform.root.gameObject.GetComponent<Controller>())
//                    {
//                        if (other.transform.root.gameObject.GetComponent<Controller>().gameObject.GetInstanceID() == Controller.gameObject.GetInstanceID()) return;
//
//                        switch (Controller.CanKillOthers)
//                        {
//                            case PUNHelper.CanKillOthers.OnlyOpponents:
//                                if(Controller.MyTeam == other.transform.root.gameObject.GetComponent<Controller>().MyTeam && Controller.MyTeam != PUNHelper.Teams.Null)
//                                    return;
//                                break;
//                            case PUNHelper.CanKillOthers.Everyone:
//                                break;
//                            case PUNHelper.CanKillOthers.NoOne:
//                                return;
//                        }
//	                
//                        var weaponController = other.transform.root.gameObject.GetComponent<Controller>().WeaponManager.WeaponController;
//
//                        var deltaTime = Time.deltaTime;
//                        
//                        if (Controller.CharacterSync)
//                            Controller.CharacterSync.UpdateKillAssists(weaponController.Controller.CharacterName);
//
//                        if (Controller.PlayerHealth - weaponController.Attacks[weaponController.currentAttack].weapon_damage * deltaTime <= 0 && !registerDeath)
//                        {
//                            if(weaponController.Controller.CharacterSync)
//                                weaponController.Controller.CharacterSync.AddScore(PlayerPrefs.GetInt("FireKill"), "fire");
//                            
//                            Controller.KillerName = weaponController.Controller.CharacterName;
//                            Controller.KilledWeaponImage = (Texture2D)weaponController.WeaponImage;
//                            
//                            if(Controller.CharacterSync)
//                                Controller.CharacterSync.AddScoreToAssistants();
//
//                            registerDeath = true;
//                        }
//
//                        Controller.PlayerHealth -= weaponController.Attacks[weaponController.currentAttack].weapon_damage * deltaTime;
//                    }
//                }
//            }

            if (EnemyController && other.transform.root.GetComponent<Controller>() && !other.CompareTag("Melee Collider") && !other.CompareTag("Fire"))
            {
                foreach (var player in EnemyController.Players)
                {
                    if (player.player.Equals(other.transform.root.gameObject))
                    {
                        player.HearPlayer = true;
                        EnemyController.IKnowWherePlayerIs = false;
                        player.hearTime += Time.deltaTime;
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Melee Collider"))
            {
                gettingDamage = true;
                attackType = "Melee";
                attacking = other.transform.root.gameObject;
            }

            if(!checkColliders)
                return;

//            if (other.CompareTag("Melee Collider"))
//            {
//                if (EnemyController)
//                {
//                    if (other.transform.root.GetComponent<Controller>() && other.transform.root.GetComponent<Controller>().WeaponManager)
//                    {
//                        var weaponManager = other.transform.root.GetComponent<Controller>().WeaponManager;
//                        var weaponController = weaponManager.WeaponController;
//
//                        if (weaponController && weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee)
//                        {
//                            EnemyController.EnemyHealth -= weaponController.Attacks[weaponController.currentAttack].weapon_damage;
//
//                            EnemyController.GetShotFromWeapon(2);
//                        }
//                        else if (weaponManager.slots[weaponManager.currentSlot].weaponSlotInGame[weaponManager.slots[weaponManager.currentSlot].currentWeaponInSlot].fistAttack)
//                        {
//                            EnemyController.EnemyHealth -= weaponManager.FistDamage;
//
//                            EnemyController.GetShotFromWeapon(3);
//                        }
//                        
//                        EnemyController.PlayDamageAnimation();
//                    }
//                }
//                else if (Controller)
//                {
//                    if (other.transform.root.gameObject.GetComponent<EnemyController>())
//                    {
//                        Controller.PlayerHealth -= other.transform.root.GetComponent<EnemyController>().Attacks[0].Damage;
//                        if (Controller.PlayerHealth <= 0)
//                        {
//                            
//                            Controller.KillerName = "Enemy";
//                        }
//                    }
//                    else if (other.transform.root.gameObject.GetComponent<Controller>())
//                    {
//                        if (other.transform.root.gameObject.GetComponent<Controller>().gameObject.GetInstanceID() == Controller.gameObject.GetInstanceID()) return;
//
//                        switch (Controller.CanKillOthers)
//                        {
//                            case PUNHelper.CanKillOthers.OnlyOpponents:
//                                if(Controller.MyTeam == other.transform.root.gameObject.GetComponent<Controller>().MyTeam && Controller.MyTeam != PUNHelper.Teams.Null)
//                                    return;
//				            
//                                break;
//                            case PUNHelper.CanKillOthers.Everyone:
//                                break;
//                            case PUNHelper.CanKillOthers.NoOne:
//                                return;
//                        }
//
//                        var inventoryManager = other.transform.root.gameObject.GetComponent<Controller>().WeaponManager;
//                        var damage = 0;
//                        var hasWeapon = false;
//
//                        if (inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame.Count > 0)
//                        {
//                            if (inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].fistAttack)
//                            {
//                                damage = (int)inventoryManager.FistDamage;
//                            }
//                            else if (inventoryManager.slots[inventoryManager.currentSlot].weaponSlotInGame[inventoryManager.slots[inventoryManager.currentSlot].currentWeaponInSlot].weapon)
//                            {
//                                var weaponController = inventoryManager.WeaponController;
//                                damage = weaponController.Attacks[weaponController.currentAttack].weapon_damage;
//                                hasWeapon = true;
//                            }
//                        }
//
//                        if (Controller.oneShotOneKill)
//                            damage = (int) Controller.PlayerHealth + 50;
//                        
//                        if (Controller.CharacterSync)
//                            Controller.CharacterSync.UpdateKillAssists(inventoryManager.Controller.CharacterName);
//		            
//                        if (Controller.PlayerHealth - damage <= 0)
//                        {
//                            if(inventoryManager.Controller.CharacterSync)
//                                inventoryManager.Controller.CharacterSync.AddScore(PlayerPrefs.GetInt("MeleeKill"), "melee");
//                            
//                            Controller.KillerName = other.transform.root.GetComponent<Controller>().CharacterName;
//
//                            if (hasWeapon && inventoryManager.WeaponController.WeaponImage)
//                                Controller.KilledWeaponImage = (Texture2D) inventoryManager.WeaponController.WeaponImage;
//                            else
//                            {
//                                if(inventoryManager.FistIcon)
//                                    Controller.KilledWeaponImage = (Texture2D)inventoryManager.FistIcon;
//                            }
//                            
//                            if(Controller.CharacterSync)
//                                Controller.CharacterSync.AddScoreToAssistants();
//                        }
//
//                        Controller.PlayerHealth -= damage;
//                    }
//                }
//            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(!checkColliders)
                return;
            
//            
//            if(Controller && Controller.CharacterSync)
//                if(other.gameObject.GetComponent<CapturePoint>() && other.gameObject.GetComponent<CapturePoint>().type == CapturePoint.Type.Rectangle)
//                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {{"hpc", false}});

            if (EnemyController)
            {
                if (other.CompareTag("Smoke"))
                {
                    EnemyController.InSmoke = false;
                }

                if (other.transform.root.GetComponent<Controller>())
                {
                    foreach (var player in EnemyController.Players)
                    {
                        if (player.player.Equals(other.transform.root.gameObject))
                        {
                            player.HearPlayer = false;
                            player.hearTime = 0;
                        }
                    }
                }
            }
        }
    }
}
