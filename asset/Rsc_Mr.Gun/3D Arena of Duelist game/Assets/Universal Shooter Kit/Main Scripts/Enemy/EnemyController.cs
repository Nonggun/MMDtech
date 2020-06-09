// GercStudio
// © 2018-2020

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(AudioSource))]

    public class EnemyController : MonoBehaviour
    {
        public List<AIHelper.Player> Players;

        public EnemyAttack EnemyAttack;

        [Range(1, 100)] public float DistanceToSee = 30;
        [Range(1, 100)] public float AttackDistancePercent = 50;
        [Range(1, 180)] public float HorizontalAngleToSee = 60;
        [Range(1, 180)] public float HeightToSee = 20;
        [Range(1, 500)] public float EnemyHealth = 100;
        [Range(1, 30)] public float Speed = 3.5f;
        [Range(0.1f, 2)] public float SpeedOffset = 1;
        public float headMultiplier = 1;
        public float bodyMultiplier = 1;
        public float handsMultiplier = 1;
        public float legsMultiplier = 1;
        public float damageAnimationTimeout;
        
        public int nextBehaviour;
        public int currentBehaviour;
        public int lastBehaviour;
        public int EnemyTag;

        public bool IKnowWherePlayerIs;
        public bool RootMotionMovement = true;
        public bool isHuman;
        public bool UseCovers;
        public bool UseStates = true;
        public bool UseHealthBar = true;
        public bool InSmoke;
        public bool setCoverPoint;
        public bool HasIndex;
        public bool inGrass;

        public List<Transform> checkPositions;
        public List<Transform> BodyParts = new List<Transform>(11);

        public Transform MovementPoint;
        public Transform CoverPoint;
        public Transform Ragdoll;
        public Transform HealthText;
        public Transform DirectionObject;
        public Transform DirectionObjectParent;
        
        public GameObject Weapon;
        public GameObject currentCover;
        
        public AudioSource FeetAudioSource;
        public AudioSource AudioSource;

        public ProjectSettings projectSettings;

        public List<AIHelper.EnemyAttack> Attacks = new List<AIHelper.EnemyAttack>{new AIHelper.EnemyAttack()};

        public AnimationClip LeftRotationAnimation;
        public AnimationClip RightRotationAnimation;
        public AnimationClip CrouchLeftRotationAnimation;
        public AnimationClip CrouchRightRotationAnimation;
        public AnimationClip IdleAnimation;
        public AnimationClip CrouchIdleAnimation;
        public AnimationClip WalkAnimation;
        public AnimationClip RunAnimation;
        public List<AnimationClip> FindAnimations;
        public List<AnimationClip> DamageAnimations;
        public AnimationClip[] AllSidesMovementAnimations = new AnimationClip[8];

        public AudioClip phrase1; // I saw something
        public AudioClip phrase2; // he left, I'll look for him
        public AudioClip phrase3; // I see him, attack
        public AudioClip phrase4; // I'll go find it
        public AudioClip phrase5; // I must've imagined it then

        public MovementBehavior Behaviour;

        public List<GameObject> targets;

        public AIHelper.EnemyStates State = AIHelper.EnemyStates.Waypoints;

        public Vector3 currentWaypointPosition;
        public Vector3 currentCoverDirection;

        public Image backgroundImg;
        public Image yellowImg;
        public Image redImg;
        public Image healthBarValue;
        public Image healthBarBackground;

        public List<Texture> BloodHoles = new List<Texture>{null};
        
        public Canvas StateCanvas;
        public Canvas HealthCanvas;

        public Material trailMaterial;

        public Animator anim;

        public RuntimeAnimatorController AnimatorController;

        public Helper.AnimationClipOverrides ClipOverrides; 
        public AnimatorOverrideController newController;
        
        private int currentCheckPointNumber;
        private int currentWP;
        private int PreviousPoint;

        #region InspectorParameters

        public int topInspectorTab;
        public int movementTopInspectorTab;
        public int movementBottomInspectorTab;
        public int animsAndSoundInspectorTab;
        public int attackInspectorTab;
        public int currentMovementInspectorTab;
        public int indexInGameManager;
        
        public bool delete;
        public bool rename;
        public bool renameError;
        
        public string curName;

        #endregion
        
        private float currentDamageAnimationTime;
        private float findAfterAttackTimer;
        private float addPointToCheckFunctionCount;
        private float addMovePointFunctionCount;
        private float seeCheckPointTimeout;
        private float reduceTimeout;
        private float findTimeout;
        private float behindCoverTimeout;
        private float randomMovementTimeout;
        private float rateOfAttack;
        private float healthValuePercent;
        private float findPlayersTimer;

        public float currentWarningValue;

        private string[] allSidesMovementOverrideAnimations = 
        {
            "_EnemyMoveForward", "_EnemyMoveForwardLeft", "_EnemyMoveForwardRight", "_EnemyMoveLeft",
            "_EnemyMoveRight", "_EnemyMoveBackwardLeft", "_EnemyMoveBackwardRight", "_EnemyMoveBackward"
        };
        
        private bool checkedPoint;
        private bool playerInSeeArea;
        private bool createPointsForCheck;
        private bool warning;
        private bool attack;
        private bool isMovementPause;
        private bool isNextAction;
        private bool canReduceValue = true;
        private bool setRandomMovementPoint;
        private bool reachedCoverPoint;
        private bool playerOnNavMesh;
        private bool agentIsStopped;
        private bool playAttackPhrase;
        private bool meleeDamage;

        private NavMeshAgent agent;

        void Awake()
        {
            anim = gameObject.GetComponent<Animator>();
            
            anim.runtimeAnimatorController = AnimatorController;

            agent = GetComponent<NavMeshAgent>();
            
            AudioSource = GetComponent<AudioSource>();

            EnemyAttack = gameObject.AddComponent<EnemyAttack>();
            EnemyAttack.EnemyController = this;

            newController = new AnimatorOverrideController(anim.runtimeAnimatorController);
            anim.runtimeAnimatorController = newController;

            ClipOverrides = new Helper.AnimationClipOverrides(newController.overridesCount);
            newController.GetOverrides(ClipOverrides);

            if(WalkAnimation)
                ClipOverrides["_EnemyWalk"] = WalkAnimation;

            if(IdleAnimation)
                ClipOverrides["_EnemyIdle"] = IdleAnimation;
            
            if(CrouchIdleAnimation)
                ClipOverrides["_EnemyCrouchIdle"] = CrouchIdleAnimation;
            
            if(RunAnimation)
                ClipOverrides["_EnemyRun"] = RunAnimation;
            
            if (Attacks[0].HandsIdleAnimation)
                ClipOverrides["_EnemyHandsIdle"] = Attacks[0].HandsIdleAnimation;
            
            if (Attacks[0].HandsAttackAnimation)
                ClipOverrides["_EnemyHandsAttack"] = Attacks[0].HandsAttackAnimation;
            
            if(Attacks[0].HandsReloadAnimation)
                ClipOverrides["_EnemyHandsReload"] = Attacks[0].HandsReloadAnimation;

            ClipOverrides["_EnemyAttack"] = Attacks[0].MeleeAttackAnimations.Find(clip => clip != null);

            for (var i = 0; i < AllSidesMovementAnimations.Length; i++)
            {
                if (AllSidesMovementAnimations[i])
                {
                    ClipOverrides[allSidesMovementOverrideAnimations[i]] = AllSidesMovementAnimations[i];
                }
            }

            newController.ApplyOverrides(ClipOverrides);

            if (anim.avatar && anim.avatar.isHuman && Attacks[0].AttackType != AIHelper.AttackTypes.Melee)
            {
                anim.SetLayerWeight(1,1);
            }
            else if (anim.avatar && !anim.avatar.isHuman || Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
            {
                anim.SetLayerWeight(1, 0);
            }

            if (Weapon && !Weapon.GetComponent<AudioSource>())
                Weapon.AddComponent<AudioSource>();
            
            if (Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
            {
                Attacks[0].UseReload = false;
                anim.SetBool("Melee", true);
            }

            if (Attacks[0].AttackType == AIHelper.AttackTypes.Melee || Attacks[0].AttackType == AIHelper.AttackTypes.Fire)
            {
                UseCovers = false;
            }
            
            if(!UseStates && StateCanvas)
                StateCanvas.gameObject.SetActive(false);
            else if(UseStates && StateCanvas)
                StateCanvas.gameObject.SetActive(true);
            
            Attacks[0].CurrentAmmo = Attacks[0].InventoryAmmo;
            rateOfAttack = Attacks[0].RateOfAttack;
            
            if(!UseHealthBar && HealthCanvas)
                HealthCanvas.gameObject.SetActive(false);
            else if(UseHealthBar && HealthCanvas)
                HealthCanvas.gameObject.SetActive(true);
            
            Helper.ChangeLayersRecursively(transform, "Enemy");
            Helper.ManageBodyColliders(BodyParts, this);
        }

        void Start()
        {
            healthValuePercent = EnemyHealth;

            if (FeetAudioSource)
            {
                FeetAudioSource.hideFlags = HideFlags.HideInHierarchy;
            }
            
            if(Behaviour)
                anim.SetBool("Move", true);
        }

        private void OnAnimatorMove()
        {
            if (!RootMotionMovement)
            {
                agent.speed = Speed;
                return;
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Find") || anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                transform.rotation = anim.rootRotation;
            }
            
            anim.SetFloat("SpeedOffset", SpeedOffset);

            if(Time.deltaTime > 0)
                agent.speed = (anim.deltaPosition / Time.deltaTime).magnitude * SpeedOffset;
        }

        void Update()
        {
            if (agentIsStopped)
            {
                if(agent.isOnNavMesh)
                    agent.isStopped = true;
            }
            else
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk") || anim.GetCurrentAnimatorStateInfo(0).IsName("Run") || 
                    anim.GetCurrentAnimatorStateInfo(0).IsName("Forward") || anim.GetCurrentAnimatorStateInfo(0).IsName("Backward") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsName("Left") || anim.GetCurrentAnimatorStateInfo(0).IsName("Right") 
                    || anim.GetCurrentAnimatorStateInfo(0).IsName("Forward_Left") || anim.GetCurrentAnimatorStateInfo(0).IsName("Forward_Right") || 
                    anim.GetCurrentAnimatorStateInfo(0).IsName("Backward_Left") || anim.GetCurrentAnimatorStateInfo(0).IsName("Backward_Right"))
                {
                    if(agent.isOnNavMesh)
                        agent.isStopped = false;
                }
            }

            GetDamageInBodyColliders();
            
            findPlayersTimer += Time.deltaTime;
            
            if(findPlayersTimer > 2 && Players.Count == 0)
                FindPlayers();

            currentDamageAnimationTime += Time.deltaTime;

            if (Players.Count > 0)
            {
                if(yellowImg)
                    yellowImg.fillAmount = Players[0].warningValue;
                
                if(redImg)
                    redImg.fillAmount = Players[0].attackValue;

                if (Players[0].player)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(new Vector3(Players[0].player.transform.position.x, transform.position.y ,Players[0].player.transform.position.z), out hit, 1000, NavMesh.AllAreas))
                    {
                        playerOnNavMesh = hit.distance <= 3;
                    }
                    else
                    {
                        playerOnNavMesh = false;
                    }
                }
            }
            else
            {
                playerOnNavMesh = false;
                
                if(yellowImg)
                    yellowImg.fillAmount = 0;
                
                if(redImg)
                    redImg.fillAmount = 0;
            }

//            GetComponent<LineRenderer>().SetPositions(agent.path.corners);

            if (healthBarValue)
                healthBarValue.fillAmount = EnemyHealth / healthValuePercent;

            if (StateCanvas)
            {
                StateCanvas.transform.position = anim.GetBoneTransform(HumanBodyBones.Head).position + Vector3.up;
                
                if(Players.Count > 0 && Players[0].player)
                    StateCanvas.transform.LookAt(Players[0].player.GetComponent<Controller>().thisCamera.transform.position);
            }
            
            if (HealthCanvas)
            {
                HealthCanvas.transform.position = anim.GetBoneTransform(HumanBodyBones.Head).position + Vector3.up / 2;
                
                if(Players.Count > 0 && Players[0].player)
                    HealthCanvas.transform.LookAt(Players[0].player.GetComponent<Controller>().thisCamera.transform.position);
            }

            CheckHealth();

            CheckArea();

            ReduceValuesFromWeapon();
            
            switch (State)
            {
                case AIHelper.EnemyStates.Waypoints:
                    WayPointsMoving();
                    break;
                case AIHelper.EnemyStates.Warning:
                    CheckSomePoints();
                    break;
                case AIHelper.EnemyStates.Attack:
                    Attack();
                    break;
                case AIHelper.EnemyStates.FindAfterAttack:
                    FindAfterAttack();
                    break;
            }
            
            if(State != AIHelper.EnemyStates.Attack && playAttackPhrase)
                playAttackPhrase = false;
            
            CoverBehaviour();
            
            RandomMovementBehaviour();

            if (State != AIHelper.EnemyStates.Attack)
            {
                if (setCoverPoint)
                    setCoverPoint = false;

                if (reachedCoverPoint)
                    reachedCoverPoint = false;
                
                if(setRandomMovementPoint)
                    setRandomMovementPoint = false;

                if(currentCover)
                    currentCover = null;
                
                if(CoverPoint)
                    Destroy(CoverPoint.gameObject);
                
                if(MovementPoint)
                    Destroy(MovementPoint.gameObject);
                
                if(anim.GetBool("Attack"))
                    anim.SetBool("Attack", false);
                
                if(anim.GetBool("AllSidesMovement"))
                    anim.SetBool("AllSidesMovement", false);
                
                if (anim.GetBool("Crouch"))
                    anim.SetBool("Crouch", false);
            }
        }

        void LateUpdate()
        {
            if (Players.Count > 0 && Players[0].player)
            {
                if (State == AIHelper.EnemyStates.Attack && Attacks[0].AttackType != AIHelper.AttackTypes.Melee &&
                    Vector3.Distance(transform.position, Players[0].player.transform.position) <= DistanceToSee * AttackDistancePercent / 100 &&
                    !anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch") && !anim.GetCurrentAnimatorStateInfo(0).IsName("Left Rotate 1")
                    && !anim.GetCurrentAnimatorStateInfo(0).IsName("Right Rotate 1"))
                {
                    var body = anim.GetBoneTransform(HumanBodyBones.Spine);
                    var target = Attacks[0].AttackType != AIHelper.AttackTypes.Fire ? 
                        Players[0].player.GetComponent<Controller>().BodyObjects.TopBody.position : 
                        Players[0].player.GetComponent<Controller>().BodyObjects.Head.position;

                    var direction = target - DirectionObject.position;

                    var middleAngleX = Helper.AngleBetween(direction, DirectionObject).x;
                    var middleAngleY = Helper.AngleBetween(direction, DirectionObject).y;

                    body.RotateAround(DirectionObject.position, Vector3.up, -middleAngleY);
                    body.RotateAround(DirectionObject.position, DirectionObject.TransformDirection(Vector3.right), -middleAngleX);
                }
            }
        }
        
        public void PlayDamageAnimation()
        {
            if (currentDamageAnimationTime > damageAnimationTimeout)
            {
                currentDamageAnimationTime = 0;
                
                if (DamageAnimations.Count > 0)
                {
                    var index = Random.Range(0, DamageAnimations.Count - 1);

                    if (DamageAnimations.Contains(DamageAnimations[index]))
                    {
                        ClipOverrides["_EnemyDamage"] = DamageAnimations[index];
                        newController.ApplyOverrides(ClipOverrides);
                        
                        anim.CrossFade("Damage Reaction", 0, 0);
                    }
                }
            }
        }

        void CheckArea()
        {
            if (Players.Count > 0 && Players[0].player && playerOnNavMesh)
            {
                var distance = Vector3.Distance(transform.position, Players[0].player.transform.position);
                Players[0].distanceBetween = distance;

//                if (anim.GetBool("Find"))
//                {
//                    if(DirectionObject.parent != DirectionObjectParent)
//                        DirectionObject.parent = DirectionObjectParent;
//                }
//                else
//                {
//                    if(DirectionObject.parent != transform)
//                        DirectionObject.parent = transform;
//                }

                if (distance < DistanceToSee)
                {
                    if (distance > 5)
                    {
                        if (AIHelper.CheckRaycast(Players[0].player.GetComponent<Controller>().BodyObjects.Hips, DirectionObject, HorizontalAngleToSee, HeightToSee, DistanceToSee, State == AIHelper.EnemyStates.Attack, inGrass))
                        { 
                            CharacterDetection(4);
                        }

                        if (AIHelper.CheckRaycast(Players[0].player.GetComponent<Controller>().BodyObjects.Head, DirectionObject, HorizontalAngleToSee, HeightToSee, DistanceToSee, State == AIHelper.EnemyStates.Attack, inGrass))
                        {
                            CharacterDetection(4);
                        }

                        if (!AIHelper.CheckRaycast(Players[0].player.GetComponent<Controller>().BodyObjects.Hips, DirectionObject, HorizontalAngleToSee, HeightToSee, DistanceToSee, State == AIHelper.EnemyStates.Attack, inGrass) &&
                            !AIHelper.CheckRaycast(Players[0].player.GetComponent<Controller>().BodyObjects.Head, DirectionObject, HorizontalAngleToSee, HeightToSee, DistanceToSee, State == AIHelper.EnemyStates.Attack, inGrass) || InSmoke)
                            Players[0].SeePlayer = false;

//
//                        Debug.DrawLine(DirectionObject.position, Players[0].player.GetComponent<Controller>().BodyObjects.Hips.position,
//                            AIHelper.CheckRaycast(Players[0].player.GetComponent<Controller>().BodyObjects.Hips, DirectionObject, HorizontalAngleToSee, HeightToSee, DistanceToSee, State == AIHelper.EnemyStates.Attack, inGrass) ? Color.red : Color.green);

//                        Debug.DrawLine(DirectionObject.position, Players[0].player.GetComponent<Controller>().BodyObjects.Head.position,
//                            AIHelper.CheckRaycast(Players[0].player.GetComponent<Controller>().BodyObjects.Head, DirectionObject, HorizontalAngleToSee, HeightToSee, DistanceToSee, State == AIHelper.EnemyStates.Attack, inGrass) ? Color.red : Color.green);
                    }
                    else
                    {
                        CharacterDetection(4);
                    }
                }
                else
                {
                    Players[0].SeePlayer = false;
                }

                if (UseStates)
                {
                    if (Players[0].HearPlayer && !warning)
                    {
                        IncreaseWarningValue(2);
                    }
                    else if (Players[0].HearPlayer && warning && !attack)
                    {
                        IncreaseAttackValue(2);
                    }
                    
                    if (!Players[0].SeePlayer && !Players[0].HearPlayer && canReduceValue)
                        reductionValue();
                }
                else
                {
                    if (Players[0].HearPlayer)
                    {
                        warning = true;
                        attack = true;
                        Players[0].attackValue = 1;
                        Players[0].warningValue = 1;
                    }
                    
                    if (!Players[0].SeePlayer && !Players[0].HearPlayer && !IKnowWherePlayerIs)
                    {
                        if (State != AIHelper.EnemyStates.Waypoints)
                        {
                            if (UseStates)
                            {
                                if (phrase5)
                                {
                                    AudioSource.Stop();
                                    AudioSource.PlayOneShot(phrase5);
                                }
                            }
                            else
                            {
                                if (phrase2)
                                {
                                    AudioSource.Stop();
                                    AudioSource.PlayOneShot(phrase2);
                                }
                            }

                            State = AIHelper.EnemyStates.Waypoints;
                            warning = false;
                            attack = false;
                            Players[0].attackValue = 0;
                            Players[0].warningValue = 0;
                            anim.SetBool("Run", false);
                            
                            if(agent.isOnNavMesh)
                                agent.SetDestination(currentWaypointPosition);
                            
                            StopAllCoroutines();
                            StartMovement();
                        }
                    }
                }
                
                
                if (warning && !attack && State != AIHelper.EnemyStates.FindAfterAttack)
                {
                    State = AIHelper.EnemyStates.Warning;
                }
                else if (attack && warning && (Players[0].HearPlayer || Players[0].SeePlayer || IKnowWherePlayerIs))
                {
                    State = AIHelper.EnemyStates.Attack;
                }
            }
            else
            {
                if (State != AIHelper.EnemyStates.Waypoints)
                {
                    State = AIHelper.EnemyStates.Waypoints;
                    Players[0].SeePlayer = false;
                    Players[0].HearPlayer = false;
                    Players[0].attackValue = 0;
                    Players[0].warningValue = 0;
                    IKnowWherePlayerIs = false;
                    anim.SetBool("Run", false);
                    agent.SetDestination(currentWaypointPosition);
                    StopAllCoroutines();
                    StartMovement();
                }
            }
        }

        void CharacterDetection(int speed)
        {
            Players[0].SeePlayer = true;
            IKnowWherePlayerIs = false;
            
            if (UseStates)
            {
                if (!warning)
                    IncreaseWarningValue(speed);
                else
                {
                    if (!attack)
                        IncreaseAttackValue(speed);
                }
            }
            else
            {
                warning = true;
                attack = true;
                Players[0].attackValue = 1;
                Players[0].warningValue = 1;
            }
        }

        IEnumerator AttackVision()
        {
            yield return new WaitForSeconds(5);
            checkPositions.Add(AIHelper.CreatePointToCheck(Players[0].player.transform.position + Vector3.up, "attack"));
            StopCoroutine(AttackVision());
        }


        public void IncreaseAttackValue(float speed)
        {
            if (Players[0].attackValue < 1)
            {
                Players[0].attackValue += Time.deltaTime / speed;

                if (Players[0].attackValue > 1)
                    Players[0].attackValue = 1;
            }
            else
            {
                attack = true;
            }
        }
        
        public void IncreaseWarningValue(float speed)
        {
            if (Players[0].warningValue < 1)
            {
                Players[0].warningValue += Time.deltaTime / speed;

                if (Players[0].warningValue > 1)
                    Players[0].warningValue = 1;
            }
            else
            {
                warning = true;
            }
        }

        public void GetShotFromWeapon(float value)
        {
            if (UseStates)
            {
                switch (State)
                {
                    case AIHelper.EnemyStates.Waypoints:
                        currentWarningValue = Players[0].warningValue + value;
                        break;
                    case AIHelper.EnemyStates.Warning:
                        currentWarningValue = Players[0].attackValue + value;
                        break;
                    case AIHelper.EnemyStates.FindAfterAttack:
                        IKnowWherePlayerIs = true;
                        break;
                }

                if (canReduceValue)
                {
                    canReduceValue = false;
                    reduceTimeout = 0;
                }
            }
            else
            {
                var distance = Vector3.Distance(transform.position, Players[0].player.transform.position);
               
                if (distance > DistanceToSee) return;
                
                warning = true;
                attack = true;
                Players[0].attackValue = 1;
                Players[0].warningValue = 1;

                IKnowWherePlayerIs = true;
            }
        }

        void ReduceValuesFromWeapon()
        {
            reduceTimeout += Time.deltaTime;

            if (reduceTimeout > 3)
                canReduceValue = true;

            if (!canReduceValue)
            {
                switch (State)
                {
                    case AIHelper.EnemyStates.Waypoints:
                        Players[0].warningValue = Mathf.Lerp(Players[0].warningValue, currentWarningValue, 0.5f);

                        if (Players[0].warningValue >= 1)
                        {
                            Players[0].warningValue = 1;
                            currentWarningValue = 0;

                            warning = true;
                        }

                        break;

                    case AIHelper.EnemyStates.Warning:
                        Players[0].attackValue = Mathf.Lerp(Players[0].attackValue, currentWarningValue, 0.5f);

                        if (Players[0].attackValue >= 1)
                        {
                            Players[0].attackValue = 1;
                            IKnowWherePlayerIs = true;
                            attack = true;
                        }

                        break;
                }
            }
        }

        public void GetDamageInBodyColliders()
        {
            var getAnyDamage = false;

            foreach (var bodyPart in BodyParts)
            {
                var bodyColliderScript = bodyPart.gameObject.GetComponent<BodyPartCollider>();

                if (bodyColliderScript.gettingDamage)
                {
                    if (bodyColliderScript.attackType == "Fire")
                    {
                        if (bodyColliderScript.attacking.GetComponent<Controller>())
                        {
                            var weaponController = bodyColliderScript.attacking.GetComponent<Controller>().WeaponManager.WeaponController;
                            if (weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Flame)
                            {
                                EnemyHealth -= weaponController.Attacks[weaponController.currentAttack].weapon_damage * Time.deltaTime;

                                PlayDamageAnimation();

                                GetShotFromWeapon(0.1f);

                                break;
                            }
                        }
                    }
                    else if (bodyColliderScript.attackType == "Melee")
                    {
                        getAnyDamage = true;

                        if (!meleeDamage)
                        {
                            if (bodyColliderScript.attacking.GetComponent<Controller>() && bodyColliderScript.attacking.GetComponent<Controller>().WeaponManager)
                            {
                                var weaponManager = bodyColliderScript.attacking.GetComponent<Controller>().WeaponManager;
                                var weaponController = weaponManager.WeaponController;

                                if (weaponController && weaponController.Attacks[weaponController.currentAttack].AttackType == WeaponsHelper.TypeOfAttack.Melee)
                                {
                                    EnemyHealth -= weaponController.Attacks[weaponController.currentAttack].weapon_damage;
                                    
                                    meleeDamage = true;

                                    GetShotFromWeapon(2);
                                }
                                else if (weaponManager.slots[weaponManager.currentSlot].weaponSlotInGame[weaponManager.slots[weaponManager.currentSlot].currentWeaponInSlot].fistAttack)
                                {
                                    EnemyHealth -= weaponManager.FistDamage;
                                    
                                    meleeDamage = true;

                                    GetShotFromWeapon(3);
                                }

                                PlayDamageAnimation();
                                
                                break;
                            }
                        }
                    }
                }
            }

            if(!getAnyDamage)
                meleeDamage = false;
        }

        void GenerateCheckPoints()
        {
            currentCheckPointNumber = 0;
            checkPositions.Clear();

            checkPositions.Add(AIHelper.CreatePointToCheck(Players[0].player.transform.position + Vector3.up, "warning"));

            Transform point;
            
            addPointToCheckFunctionCount = 0;
            point = AIHelper.GeneratePointOnNavMesh(checkPositions[0].position, transform.forward, DistanceToSee, ref addPointToCheckFunctionCount, false);
            if(point)
                checkPositions.Add(point);
            
                
            addPointToCheckFunctionCount = 0;
            point = AIHelper.GeneratePointOnNavMesh(checkPositions[0].position, transform.right, DistanceToSee, ref addPointToCheckFunctionCount, false);
            if(point)
                checkPositions.Add(point);
            
                
            addPointToCheckFunctionCount = 0;
            point = AIHelper.GeneratePointOnNavMesh(checkPositions[0].position, -transform.forward, DistanceToSee, ref addPointToCheckFunctionCount, false);
            if(point)
                checkPositions.Add(point);

                
            addPointToCheckFunctionCount = 0;
            point = AIHelper.GeneratePointOnNavMesh(checkPositions[0].position, -transform.right, DistanceToSee, ref addPointToCheckFunctionCount, false);
            if(point)
                checkPositions.Add(point);

            
            StartCoroutine(ISawSomething());
        }
        
        IEnumerator ISawSomething()
        {
            StopMovement(); 
            PlayFindAnimation();

            if (phrase1)
            {
                AudioSource.Stop();
                AudioSource.PlayOneShot(phrase1);
            }
            
            yield return new WaitForSeconds(PlayFindAnimation() + 1);
            
            if (attack)
            {
                StopCoroutine(ISawSomething());
            }
            else
            {
                agent.SetDestination(checkPositions[0].position);
                anim.SetBool("Find", false);
                if (phrase4)
                {
                    AudioSource.Stop();
                    AudioSource.PlayOneShot(phrase4);
                }
                StartMovement();
                StopCoroutine(ISawSomething());
            }
        }
        
        
        void reductionValue()
        {
            if (Players[0].attackValue > 0)
            {
                if(State != AIHelper.EnemyStates.Attack && State != AIHelper.EnemyStates.FindAfterAttack)
                    Players[0].attackValue -= Time.deltaTime;
            }
            else
            {
                if (Players[0].warningValue > 0)
                {
                    if(State != AIHelper.EnemyStates.Attack && State != AIHelper.EnemyStates.FindAfterAttack && State != AIHelper.EnemyStates.Warning)
                        Players[0].warningValue -= Time.deltaTime;
                }
            }
        }

        void RandomMovementBehaviour()
        {
            if (MovementPoint && setRandomMovementPoint)
            {
                if (Vector3.Distance(transform.position, new Vector3(agent.destination.x, transform.position.y, agent.destination.z)) <= (Attacks[0].AttackType != AIHelper.AttackTypes.Melee ? 2 : 1))
                {
                    randomMovementTimeout += Time.deltaTime;
                    
                    StopMovement();
                    anim.SetBool("AllSidesMovement", false);

                    if (randomMovementTimeout > (Attacks[0].AttackType != AIHelper.AttackTypes.Melee ? Random.Range(2, 4) : Random.Range(5, 10)))
                    {
                        randomMovementTimeout = 0;
                        setRandomMovementPoint = false;
                        Destroy(MovementPoint.gameObject);
                    }
                }
                else
                {
                    anim.SetBool("AllSidesMovement", true);
                    AIHelper.AllSidesMovement(Helper.AngleBetween(DirectionObject.forward, agent.velocity), anim);
                }
            }
        }

        void CoverBehaviour()
        {
            if (CoverPoint && setCoverPoint && Players.Count > 0 && Players[0].player)
            {
                CheckCoverPoint();
                
                if (Vector3.Distance(transform.position, CoverPoint.position) <= 2)
                {
                    if (!reachedCoverPoint && !anim.GetBool("Crouch"))
                    {
                        ChangeCoverState();
                    }

                    anim.SetBool("AllSidesMovement", false);
                    reachedCoverPoint = true;
                    
                    if(agent.isOnNavMesh)
                        if(!agentIsStopped)
                            StopMovement();

                    behindCoverTimeout += Time.deltaTime;

                    if (behindCoverTimeout > Random.Range(7, 14) && !anim.GetCurrentAnimatorStateInfo(1).IsName("Reload") 
                        || anim.GetCurrentAnimatorStateInfo(1).IsName("Reload") && !anim.GetBool("Crouch"))
                    {
                        behindCoverTimeout = 0;
                        ChangeCoverState();
                    }
                }
                else
                {
                    if (anim.GetBool("Crouch"))
                    {
                        anim.SetBool("Crouch", false);
                    }
                    anim.SetBool("AllSidesMovement", true);
                    AIHelper.AllSidesMovement(Helper.AngleBetween(DirectionObject.forward, agent.velocity), anim);
                }
            }
        }

        public void PlayStepSound(float volume)
        {
            var hit = new RaycastHit();

            var layerMask = ~ (LayerMask.GetMask("Enemy") | LayerMask.GetMask("Grass") | LayerMask.GetMask("Character") | LayerMask.GetMask("Head"));
            
            if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out hit, 100, layerMask))
            {
                var surface = hit.collider.GetComponent<Surface>();
				
                if (FeetAudioSource && surface && surface.EnemyFootstepsSounds.Length > 0)
                    CharacterHelper.PlayStepSound(surface, FeetAudioSource, EnemyTag, volume, "enemy");
            }
        }

        void ChangeCoverState()
        {
            if (!anim.GetBool("Crouch"))
            {
                anim.SetBool("Crouch", true);
                anim.SetBool("Attack", false);
            }
            else
            {
                anim.SetBool("Crouch", false);
                anim.SetBool("Attack", true);
            }
        }

        void CheckCoverPoint()
        {
            var newDirection = currentCover.transform.position - Players[0].player.transform.position;

            if (Mathf.Abs(Helper.AngleBetween(currentCoverDirection, newDirection)) > 60)
            {
                reachedCoverPoint = false;
                setCoverPoint = false;

                if(CoverPoint)
                    Destroy(CoverPoint.gameObject);
            }
        }

        void Attack()
        {
            if (checkPositions.Count > 0)
            {
                foreach (var point in checkPositions.Where(point => point))
                {
                    Destroy(point.gameObject);
                }

                checkPositions.Clear();
                
                createPointsForCheck = false;
                checkedPoint = false;
                agent.updateRotation = true;
                
                anim.SetBool("Find", false);
                
                StopAllCoroutines();
                StopMovement();
            }

            if (!playAttackPhrase)
            {
                if (phrase3)
                {
                    AudioSource.Stop();
                    AudioSource.PlayOneShot(phrase3);
                }
                playAttackPhrase = true;
            }

            if (!Players[0].HearPlayer && !Players[0].SeePlayer && !IKnowWherePlayerIs)
            {
                State = AIHelper.EnemyStates.FindAfterAttack;
                if (phrase2)
                {
                    AudioSource.Stop();
                    AudioSource.PlayOneShot(phrase2);
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, Players[0].player.transform.position) <= DistanceToSee * AttackDistancePercent / 100)
                {
                    if (!setCoverPoint)
                    {
                        setCoverPoint = true;
                        CoverPoint = AIHelper.GetCoverPoint(this);

                        if (CoverPoint && CoverPoint.position != Vector3.zero && UseCovers)
                        {
                            agent.SetDestination(CoverPoint.position);
                            StartMovement();
                            anim.SetBool("Attack", true);

                            setRandomMovementPoint = false;

                            if (MovementPoint)
                                Destroy(MovementPoint.gameObject);
                        }
                        else
                        {
                            if (!setRandomMovementPoint)
                            {
                                setRandomMovementPoint = true;

                                addMovePointFunctionCount = 0;
                                var direction = Vector3.zero;
                                var distance = 10f;

                                if (Attacks[0].AttackType == AIHelper.AttackTypes.Fire || Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
                                {
                                    var angle = Random.Range(-90, 90);
                                    distance = DistanceToSee * AttackDistancePercent / 150;

                                    direction = Quaternion.AngleAxis(angle, Vector3.up) * Players[0].player.GetComponent<Controller>().DirectionObject.forward;
                                }
                                else
                                {
                                    direction = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
                                }

                                var point = Attacks[0].AttackType != AIHelper.AttackTypes.Fire && Attacks[0].AttackType != AIHelper.AttackTypes.Melee ? transform.position : Players[0].player.transform.position;
                                MovementPoint = AIHelper.GeneratePointOnNavMesh(point, direction, distance, ref addMovePointFunctionCount, true);

                                if (MovementPoint)
                                {
                                    if (agent.isOnNavMesh)
                                        agent.SetDestination(MovementPoint.position);

                                    StartMovement();
                                    anim.SetBool("Attack", true);
                                }
                                else
                                {
                                    setRandomMovementPoint = false;
                                    anim.SetBool("AllSidesMovement", false);
                                    StopMovement();
                                }
                            }

                            setCoverPoint = false;
                            reachedCoverPoint = false;

                            if (CoverPoint)
                                Destroy(CoverPoint.gameObject);

                            if (!anim.GetCurrentAnimatorStateInfo(1).IsName("Reload") && !anim.GetBool("Reload"))
                                anim.SetBool("Attack", true);
                        }
                    }

                    var lookPos = Players[0].player.transform.position - transform.position;
                    lookPos.y = 0;
                    var rotation = Quaternion.LookRotation(lookPos);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10);

                    anim.SetFloat("Angle", Helper.AngleBetween(transform.forward, lookPos));

                    // transform.LookAt(Players[0].player.transform.position);
                    //generate attack type here
                }
                else
                {
//                    anim.SetBool("Attack", false);
//                    anim.SetBool("Crouch", false);
//                    anim.SetBool("AllSidesMovement", false);

                    anim.SetBool("Run", true);

                    StartMovement();

                    if (CoverPoint)
                        Destroy(CoverPoint.gameObject);

                    if (MovementPoint)
                        Destroy(MovementPoint.gameObject);

                    setCoverPoint = false;
                    reachedCoverPoint = false;
                    setRandomMovementPoint = false;

                    if (agent.isOnNavMesh)
                        agent.SetDestination(Players[0].player.transform.position);
                }

                rateOfAttack += Time.deltaTime;

                if (Attacks[0].UseReload && Attacks[0].CurrentAmmo <= 0)
                {
                    anim.SetBool("Reload", true);
                    anim.SetBool("Attack", false);
                    Attacks[0].CurrentAmmo = Attacks[0].InventoryAmmo;
                    EnemyAttack.StartCoroutine(EnemyAttack.ReloadTimeout());
                }
                
                if (anim.GetBool("Attack") && anim.GetCurrentAnimatorStateInfo(Attacks[0].AttackType == AIHelper.AttackTypes.Melee ? 0 : 1).IsName("Attack"))
                {
                    if (rateOfAttack >= Attacks[0].RateOfAttack || Attacks[0].AttackType == AIHelper.AttackTypes.Fire)
                    {
                        if (Attacks[0].UseReload && Attacks[0].CurrentAmmo > 0)
                        {
                            rateOfAttack = 0;
                            EnemyAttack.Attack(Attacks[0]);

                            if (!Attacks[0].HandsAttackAnimation.isLooping)
                                anim.Play("Attack", 1, 0);
                        }
                        else if (!Attacks[0].UseReload)
                        {
                            if (Attacks[0].MeleeAttackAnimations.Count > 0)
                            {
                                AnimationClip animationClip;

                                var index = Random.Range(0, Attacks[0].MeleeAttackAnimations.Count);

                                animationClip = Attacks[0].MeleeAttackAnimations.Contains(Attacks[0].MeleeAttackAnimations[index]) ? Attacks[0].MeleeAttackAnimations[index] : Attacks[0].MeleeAttackAnimations.Find(clip => clip != null);

                                ClipOverrides["_EnemyAttack"] = animationClip;
                                newController.ApplyOverrides(ClipOverrides);
                            }
                            
                            rateOfAttack = 0;
                            EnemyAttack.Attack(Attacks[0]);

                            if (Attacks[0].AttackType == AIHelper.AttackTypes.Melee)
                            {
                                if (!Attacks[0].MeleeAttackAnimations[0].isLooping)
                                    anim.Play("Attack", 0, 0);
                            }
                            else
                            {
                                if (!Attacks[0].HandsAttackAnimation.isLooping)
                                    anim.Play("Attack", 1, 0);
                            }

                        }
                    }
                    else
                    {
                        //anim.SetBool("Attack", false);
                    }
                }
//                else
//                {
//                    rateOfAttack = 0;
//                }

//                }
            }
        }

        void FindAfterAttack()
        {
            seeCheckPointTimeout += Time.deltaTime;

            if (createPointsForCheck)
            {
                if (Vector3.Distance(checkPositions[currentCheckPointNumber].position, transform.position) <= 5 && !checkedPoint)
                {
                    StopMovement();
                    StartCoroutine(Think());
                    checkedPoint = true;
                }
                else if(Vector3.Distance(checkPositions[currentCheckPointNumber].position, transform.position) > 5 && seeCheckPointTimeout > 2)
                {
                    if(agentIsStopped)
                    {
                        anim.SetBool("Move", true);
                        anim.SetBool("Run", true);
                        
                        agent.updateRotation = true;
                        agentIsStopped = false;
                    }
                }

                if (Players[0].HearPlayer || Players[0].SeePlayer || IKnowWherePlayerIs)
                {
                    findAfterAttackTimer += Time.deltaTime;

                    if (IKnowWherePlayerIs)
                    {
                        var count = checkPositions.Count;

                        for (var i = 0; i < count; i++)
                        {
                            if (checkPositions[i])
                                Destroy(checkPositions[i].gameObject);
                        }

                        anim.SetBool("Find", false);
                        agent.updateRotation = true;
                        
                        StopAllCoroutines();
                        
                        findAfterAttackTimer = 0;
                        checkPositions.Clear();
                        createPointsForCheck = false;
                        checkedPoint = false;
                        State = AIHelper.EnemyStates.Attack;
                    }
                    else if (findAfterAttackTimer > 3)
                    {
                        var count = checkPositions.Count;

                        for (var i = 0; i < count; i++)
                        {
                            if (checkPositions[i])
                                Destroy(checkPositions[i].gameObject);
                        }
                        
                        anim.SetBool("Find", false);
                        agent.updateRotation = true;
                        
                        StopAllCoroutines();
                        findAfterAttackTimer = 0;
                        checkPositions.Clear();
                        createPointsForCheck = false;
                        checkedPoint = false;
                        State = AIHelper.EnemyStates.Attack;
                    }
                }
                else
                {
                    findAfterAttackTimer = 0;
                }
            }
            else
            {
                anim.SetBool("Find", false);
                agent.updateRotation = true;
                
                StopAllCoroutines();
                checkPositions.Add(AIHelper.CreatePointToCheck(Players[0].player.transform.position + Vector3.up, "attack"));
                currentCheckPointNumber = 0;
                createPointsForCheck = true;
                checkedPoint = false;
                agent.SetDestination(checkPositions[0].position);
                StartCoroutine(AttackVision());
                seeCheckPointTimeout = 0;
            }
        }

        void WayPointsMoving()
        {
            if (Behaviour && Behaviour.points.Count > 0)
            {
                if (!HasIndex)
                {
                    currentWaypointPosition = Behaviour.points[0].point.transform.position;
                    HasIndex = true;
                }

                if (Vector3.Distance(currentWaypointPosition, transform.position) <= 2)
                {
                    if (!isNextAction)
                    {
                        lastBehaviour = currentBehaviour;
                        currentBehaviour = nextBehaviour;
                        ChoiceNextAction();
                        isNextAction = true;
                    }
                }
                else
                {
                    if(agent.isOnNavMesh)
                        agent.SetDestination(currentWaypointPosition);

                    isNextAction = false;
                }
            }
        }

        void ChoiceNextAction()
        {
            switch (Behaviour.points[currentBehaviour].action)
            {
                case Helper.NextPointAction.NextPoint:
                    CalculationNextPointIndex(Helper.NextPointAction.NextPoint);
                    break;
                case Helper.NextPointAction.RandomPoint:
                    CalculationNextPointIndex(Helper.NextPointAction.RandomPoint);
                    break;
                case Helper.NextPointAction.ClosestPoint:
                    CalculationNextPointIndex(Helper.NextPointAction.ClosestPoint);
                    break;
                case Helper.NextPointAction.Stop:
                    StopMovement();
                    break;
            }

            if (Behaviour.points[currentBehaviour].waitTime > 0 && Behaviour.points[currentBehaviour].action != Helper.NextPointAction.Stop)
            {
                isMovementPause = true;
                StopMovement();
            }
            else
            {
                agent.SetDestination(currentWaypointPosition);
            }
        }

        public void CalculationNextPointIndex(Helper.NextPointAction currentAction)
        {
            switch (currentAction)
            {
                case Helper.NextPointAction.NextPoint:
                    nextBehaviour++;
                    if (nextBehaviour >= Behaviour.points.Count)
                        nextBehaviour = 0;
                    break;
                case Helper.NextPointAction.RandomPoint:
                    nextBehaviour = Random.Range(0, Behaviour.points.Count);
                    break;
                case Helper.NextPointAction.ClosestPoint:
                    nextBehaviour = AIHelper.GetNearestPoint(Behaviour.points, transform.position, nextBehaviour, lastBehaviour);
                    break;
            }

            currentWaypointPosition = Behaviour.points[nextBehaviour].point.transform.position;
        }

        public void StopMovement()
        {
            if(agent.isOnNavMesh) agentIsStopped = true;
            
            agent.updateRotation = false;

            anim.SetBool("Move", false);
            anim.SetBool("Run", false);

            if (isMovementPause && State == AIHelper.EnemyStates.Waypoints) 
                StartCoroutine(MovePause(Behaviour.points[currentBehaviour].waitTime));
        }

        void StartMovement()
        {
            anim.SetBool("Attack", false);
            anim.SetBool("Crouch", false);
            anim.SetBool("AllSidesMovement", false);
            anim.SetBool("Move", true);
            agent.updateRotation = true;
            
            if(agent.isOnNavMesh)
                agentIsStopped = false;
        }


        IEnumerator MovePause(float time)
        {
            yield return new WaitForSeconds(time + 2);

            isMovementPause = false;

            agent.updateRotation = true;
            agentIsStopped = false;
            agent.SetDestination(currentWaypointPosition);

            anim.SetBool("Move", true);

            StopCoroutine("MovePause");
        }
        
        IEnumerator StartMovementDelay()
        {
            yield return new WaitForSeconds(1);
            
            anim.SetBool("Attack", false);
            
            anim.SetBool("Crouch", false);
            anim.SetBool("AllSidesMovement", false);
            anim.SetBool("Move", true);
            agent.updateRotation = true;
            agentIsStopped = false;
            StopCoroutine("StartMovementDelay");
        }
        
        IEnumerator Think()
        {
            yield return new WaitForSeconds(PlayFindAnimation());

            if (checkPositions.Count > 0)
            {
                Destroy(checkPositions[currentCheckPointNumber].gameObject);

                if (currentCheckPointNumber < checkPositions.Count - 1)
                {
                    currentCheckPointNumber += 1;
                    agent.SetDestination(checkPositions[currentCheckPointNumber].position);
                }
                else
                {
                    if (State == AIHelper.EnemyStates.Warning)
                    {
                        warning = false;
                        State = AIHelper.EnemyStates.Waypoints;
                        checkPositions.Clear();
                        createPointsForCheck = false;
                        if (phrase5)
                        {
                            AudioSource.Stop();
                            AudioSource.PlayOneShot(phrase5);
                        }
                        agent.SetDestination(currentWaypointPosition);
                    }
                    else if (State == AIHelper.EnemyStates.FindAfterAttack)
                    {
                        var random = Random.Range(0, 2);

                        if (random == 0)
                        {
                            attack = false;
                            checkPositions.Clear();
                            createPointsForCheck = false;
                            anim.SetBool("Run", false);
                            State = AIHelper.EnemyStates.Warning;
                        }
                        else
                        {
                            attack = false;
                            warning = false;
                            State = AIHelper.EnemyStates.Waypoints;
                            if (phrase5)
                            {
                                AudioSource.Stop();
                                AudioSource.PlayOneShot(phrase5);
                            }
                            anim.SetBool("Run", false);
                            checkPositions.Clear();
                            createPointsForCheck = false;
                            agent.SetDestination(currentWaypointPosition);
                        }
                    }
                }
                StartCoroutine(StartMovementDelay());
                anim.SetBool("Find", false);
                agent.updateRotation = true;
                
                agent.updateRotation = true;
            }
            
            checkedPoint = false;
            StopCoroutine(Think());
        }

        float PlayFindAnimation()
        {
            AnimationClip animationClip = null;

            if (FindAnimations.Count > 0)
            {
                var index = Random.Range(0, FindAnimations.Count - 1);

                if (FindAnimations.Contains(FindAnimations[index]))
                {
                    animationClip = FindAnimations[index];

                    ClipOverrides["_EnemyFind"] = animationClip;
                    newController.ApplyOverrides(ClipOverrides);
                }
            }

            agent.updateRotation = false;
            anim.SetBool("Find", true);

            if (animationClip != null) return animationClip.length;

            return 5;
        }

        void CheckSomePoints()
        {
            if (!createPointsForCheck)
            {
                GenerateCheckPoints();
                createPointsForCheck = true;
            }
            else
            {
                if (Vector3.Distance(checkPositions[currentCheckPointNumber].position, transform.position) <= 5 && !checkedPoint)
                {
                    checkedPoint = true;

                    if (agent.updatePosition)
                    {
                        StopMovement();
                        StartCoroutine(Think());
                    }
                }
            }
        }

        public void FindPlayers()
        {
            if(Players.Count > 0)
                Players.Clear();
            
            var foundPlayers = FindObjectsOfType<Controller>();
            
            foreach (var player in foundPlayers)
            {
                Players.Add(new AIHelper.Player {HearPlayer = false, player = player.gameObject});
            }
        }

        void CheckHealth()
        {
            if (EnemyHealth <= 0)
            {
                CreateRagdoll();
            }
        }
        
        void CreateRagdoll()
        {
            var gameManager = FindObjectOfType<GameManager>();

            if (gameManager)
            {
                gameManager.RemoveEnemyFromScene(indexInGameManager);
            }
            
            anim.SetLayerWeight(1,0);

            foreach (var part in BodyParts)
            {
                part.GetComponent<Rigidbody>().isKinematic = false;
            }

            if (Weapon)
            {
                Weapon.transform.parent = null;
                Weapon.AddComponent<Rigidbody>();
            }

            if(StateCanvas)
                StateCanvas.gameObject.SetActive(false);
            
            if(HealthCanvas)
                HealthCanvas.gameObject.SetActive(false);
            
            enabled = false;
            anim.enabled = false;
            agent.enabled = false;

//            Destroy(gameObject, 10);
        }
        
        public void PlayAttackSound()
        {
            if(Weapon)
                Weapon.GetComponent<AudioSource>().PlayOneShot(Attacks[0].AttackAudio);
            else AudioSource.PlayOneShot(Attacks[0].AttackAudio);
        }

        #region Gizmos
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(!DirectionObject)
                return;
            
            
            var dir = DirectionObject.forward;

            var xLeftDir = Quaternion.Euler(0, HorizontalAngleToSee / 2, 0) * dir;
            var xRightDir = Quaternion.Euler(0, -HorizontalAngleToSee / 2, 0) * dir;

            var position = DirectionObject.position;

            Handles.zTest = CompareFunction.Greater;
            if (topInspectorTab == 0)
            {
                Handles.color = new Color32(255, 0, 0, 50);
            }
            else
            {
                Handles.color = new Color32(255, 255, 255, 50);
            }

            DrawArea(position, xRightDir, xLeftDir, dir, DistanceToSee, HeightToSee);

            if (topInspectorTab == 1)
            {
                Handles.color = new Color32(255, 255, 0, 50);
                
                var newHeight = DistanceToSee * AttackDistancePercent / 100 * Mathf.Tan(Mathf.Abs(Helper.AngleBetween((position + xLeftDir * DistanceToSee - transform.up * HeightToSee / 2) - position, DirectionObject).x) * Mathf.Deg2Rad);
                DrawArea(position, xRightDir, xLeftDir, dir, DistanceToSee * AttackDistancePercent / 100, newHeight * 2);
            }
            

            Handles.zTest = CompareFunction.Less;
            if (topInspectorTab == 0)
            {
                Handles.color = new Color32(255, 0, 0, 255);
            }
            else
            {
                Handles.color = new Color32(255, 255, 255, 255);
            }
            
            DrawArea(position, xRightDir, xLeftDir, dir, DistanceToSee, HeightToSee);
            
            if (topInspectorTab == 1)
            {
                Handles.color = new Color32(255, 255, 0, 255);
                
                var newHeight = DistanceToSee * AttackDistancePercent / 100 * Mathf.Tan(Mathf.Abs(Helper.AngleBetween((position + xLeftDir * DistanceToSee - transform.up * HeightToSee / 2) - position, DirectionObject).x) * Mathf.Deg2Rad);
                DrawArea(position, xRightDir, xLeftDir, dir, DistanceToSee * AttackDistancePercent / 100, newHeight * 2);
            }
        }

        void DrawArea(Vector3 position, Vector3 xRightDir, Vector3 xLeftDir, Vector3 dir, float distance, float height)
        {
            Handles.DrawLine(position, position + xLeftDir * distance - transform.up * height / 2);
            Handles.DrawLine(position, position + xRightDir * distance - transform.up * height / 2);

            Handles.DrawWireArc(position - transform.up * height / 2, transform.up, dir, HorizontalAngleToSee / 2, distance);
            Handles.DrawWireArc(position - transform.up * height / 2, transform.up, dir, -HorizontalAngleToSee / 2, distance);

            Handles.DrawLine(position + xLeftDir * distance, position + xLeftDir * distance + transform.up * height / 2);
            Handles.DrawLine(position + xRightDir * distance, position + xRightDir * distance + transform.up * height / 2);

            Handles.DrawLine(position + xLeftDir * distance, position + xLeftDir * distance - transform.up * height / 2);
            Handles.DrawLine(position + xRightDir * distance, position + xRightDir * distance - transform.up * height / 2);

            Handles.DrawLine(position, position + xLeftDir * distance + transform.up * height / 2);
            Handles.DrawLine(position, position + xRightDir * distance + transform.up * height / 2);

            Handles.DrawWireArc(position + transform.up * height / 2, transform.up, dir, HorizontalAngleToSee / 2, distance);
            Handles.DrawWireArc(position + transform.up * height / 2, transform.up, dir, -HorizontalAngleToSee / 2, distance);
        }

#endif
        
        #endregion
    }
}




