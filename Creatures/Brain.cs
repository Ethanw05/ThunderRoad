// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Brain
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (NavMeshAgent))]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Brain.html")]
[AddComponentMenu("ThunderRoad/Creatures/Brain")]
public class Brain : ThunderBehaviour
{
  [NonSerialized]
  public bool canDamage;
  [NonSerialized]
  public bool isElectrocuted;
  [NonSerialized]
  public bool isDying;
  [NonSerialized]
  public bool isAttacking;
  [NonSerialized]
  public bool isShooting;
  [NonSerialized]
  public bool isCasting;
  [NonSerialized]
  public bool isChoke;
  [NonSerialized]
  public bool isMuffled;
  [NonSerialized]
  public bool isCarried;
  [NonSerialized]
  public bool isDodging;
  [NonSerialized]
  public bool isDefending;
  [NonSerialized]
  public bool isIncapacitated;
  [NonSerialized]
  public bool isUnconscious;
  [NonSerialized]
  public bool onNavmesh;
  public bool isManuallyControlled;
  [NonSerialized]
  public Creature creature;
  [NonSerialized]
  public NavMeshAgent navMeshAgent;
  public static int hashAim;
  public static int hashBlock;
  public static int hashHit;
  public static int hashHitDirX;
  public static int hashHitDirY;
  public static int hashElectrocute;
  public static int hashChoke;
  public static int hashCarry;
  public static int hashIsCastingLeft;
  public static int hashIsCastingRight;
  public static int hashCast;
  public static int hashCastCurve;
  public static int hashCastSide;
  public static int hashCastTime;
  public static int hashIsReloading;
  public static int hashIsShooting;
  public static int hashShoot;
  public static int hashReload;
  public static int hashGrabRock;
  public static int hashThrowOverhand;
  public static int hashParryMagic;
  public static int hashHitType;
  public static int hashInjured;
  public static int hashDodge;
  public static int hashDodgeType;
  public static int hashDodgeSpeed;
  public static int hashSwapHands;
  public static int hashGrappleEscape;
  public static bool hashInitialized;
  [NonSerialized]
  public Creature currentTarget;
  public float? targetAcquireTime;
  [NonSerialized]
  public Brain.Stagger currentStagger;
  [NonSerialized]
  public Brain.State state;
  [NonSerialized]
  public BrainData instance;
  [NonSerialized]
  public List<Zone> slowZones;
  [NonSerialized]
  public List<object> noStandupModifiers = new List<object>();

  public Creature lastTarget { get; protected set; }

  public event Action<Brain.State> OnStateChangeEvent;

  public event Brain.PushEvent OnPushEvent;

  public event Brain.AttackEvent OnAttackEvent;

  private void OnValidate()
  {
    if (!this.gameObject.activeInHierarchy || Application.isPlaying)
      return;
    this.navMeshAgent = this.GetComponent<NavMeshAgent>();
    if (!(bool) (UnityEngine.Object) this.navMeshAgent)
      return;
    this.navMeshAgent.enabled = false;
  }

  public void OnCreatureEnable()
  {
    if (this.instance == null)
      return;
    this.instance.Start();
    this.instance.Update(true);
  }

  public void OnCreatureDisable() => this.instance?.Stop();

  protected virtual void Awake()
  {
    if (!Brain.hashInitialized)
    {
      Brain.hashAim = Animator.StringToHash("Aim");
      Brain.hashBlock = Animator.StringToHash("Block");
      Brain.hashHitDirX = Animator.StringToHash("HitDirX");
      Brain.hashHitDirY = Animator.StringToHash("HitDirY");
      Brain.hashIsReloading = Animator.StringToHash("IsReloading");
      Brain.hashIsShooting = Animator.StringToHash("IsShooting");
      Brain.hashElectrocute = Animator.StringToHash("Electrocute");
      Brain.hashChoke = Animator.StringToHash("Choke");
      Brain.hashCarry = Animator.StringToHash("Carry");
      Brain.hashReload = Animator.StringToHash("Reload");
      Brain.hashGrabRock = Animator.StringToHash("GrabRock");
      Brain.hashThrowOverhand = Animator.StringToHash("ThrowOverhand");
      Brain.hashShoot = Animator.StringToHash("Shoot");
      Brain.hashHit = Animator.StringToHash("Hit");
      Brain.hashIsCastingLeft = Animator.StringToHash("IsCastingLeft");
      Brain.hashIsCastingRight = Animator.StringToHash("IsCastingRight");
      Brain.hashCast = Animator.StringToHash("Cast");
      Brain.hashCastSide = Animator.StringToHash("CastSide");
      Brain.hashCastTime = Animator.StringToHash("CastTime");
      Brain.hashParryMagic = Animator.StringToHash("ParryMagic");
      Brain.hashHitType = Animator.StringToHash("HitType");
      Brain.hashInjured = Animator.StringToHash("Injured");
      Brain.hashDodge = Animator.StringToHash("Dodge");
      Brain.hashDodgeType = Animator.StringToHash("DodgeType");
      Brain.hashDodgeSpeed = Animator.StringToHash("DodgeSpeed");
      Brain.hashCastCurve = Animator.StringToHash("CastCurve");
      Brain.hashSwapHands = Animator.StringToHash("SwapLeftToRight");
      Brain.hashGrappleEscape = Animator.StringToHash("GrappleEscape");
      Brain.hashInitialized = true;
    }
    this.navMeshAgent = this.GetComponent<NavMeshAgent>();
    this.navMeshAgent.updatePosition = false;
    this.navMeshAgent.updateRotation = false;
    this.navMeshAgent.speed = 0.0f;
    this.navMeshAgent.stoppingDistance = 0.0f;
    this.navMeshAgent.autoBraking = false;
    this.slowZones = new List<Zone>();
  }

  public void Init(Creature creature)
  {
    this.creature = creature;
    creature.ragdoll.OnStateChange += new Ragdoll.StateChange(this.OnRagdollStateChange);
    creature.OnZoneEvent += new Creature.ZoneEvent(this.OnZoneEvent);
    creature.OnDespawnEvent += new Creature.DespawnEvent(this.OnCreatureDespawn);
  }

  public virtual IEnumerator LoadCoroutine(string brainId)
  {
    BrainData outputData;
    if (Catalog.TryGetData<BrainData>(brainId, out outputData))
    {
      this.instance?.Unload();
      this.instance = outputData.TakeFromPool();
      if (this.instance == null)
      {
        Task<BrainData> task = outputData.CloneJsonAsync<BrainData>();
        yield return (object) new WaitUntil((Func<bool>) (() => task.IsCompleted));
        this.instance = task.Result;
      }
      if (this.instance != null)
      {
        yield return (object) this.instance.LoadCoroutine(this.creature);
        this.instance.Start();
      }
      else
        Debug.LogError((object) "BrainData instance is null");
    }
  }

  public virtual void Load(string brainId)
  {
    BrainData outputData;
    if (!Catalog.TryGetData<BrainData>(brainId, out outputData))
      return;
    this.instance?.Unload();
    this.instance = outputData.TakeFromPool();
    if (this.instance == null)
      this.instance = outputData.Instantiate();
    if (this.instance != null)
    {
      this.instance.Load(this.creature);
      this.instance.Start();
    }
    else
      Debug.LogError((object) "BrainData instance is null");
  }

  public virtual void Stop()
  {
    this.isIncapacitated = false;
    this.isUnconscious = false;
    if (this.instance == null)
      return;
    this.instance.Stop();
    this.ClearNoStandUpModifiers();
  }

  public void ResetBrain()
  {
    this.instance?.tree?.Reset();
    this.isMuffled = false;
  }

  public override ManagedLoops EnabledManagedLoops
  {
    get => ManagedLoops.FixedUpdate | ManagedLoops.Update | ManagedLoops.LateUpdate;
  }

  protected internal override void ManagedFixedUpdate() => this.instance?.FixedUpdate();

  protected internal override void ManagedUpdate()
  {
    this.isDodging = this.creature.animator.GetInteger(Brain.hashDodgeType) > 0;
    int num = (UnityEngine.Object) this.currentTarget != (UnityEngine.Object) null ? 1 : 0;
    if (num != 0 && !this.targetAcquireTime.HasValue || (UnityEngine.Object) this.currentTarget != (UnityEngine.Object) this.lastTarget)
    {
      this.targetAcquireTime = new float?(Time.time);
      this.lastTarget = this.currentTarget;
    }
    if (num == 0 && this.targetAcquireTime.HasValue)
      this.targetAcquireTime = new float?();
    if (this.navMeshAgent.enabled)
      this.navMeshAgent.nextPosition = this.creature.transform.position;
    this.instance?.Update();
  }

  protected internal override void ManagedLateUpdate() => this.instance?.LateUpdate();

  public void SetState(Brain.State newState)
  {
    if (this.state == newState)
      return;
    Action<Brain.State> stateChangeEvent = this.OnStateChangeEvent;
    if (stateChangeEvent != null)
      stateChangeEvent(newState);
    EventManager.InvokeCreatureBrainStateChange(this.creature, newState);
    this.state = newState;
  }

  public void InvokePushEvent(Creature.PushType type, Brain.Stagger stagger)
  {
    Brain.PushEvent onPushEvent = this.OnPushEvent;
    if (onPushEvent == null)
      return;
    onPushEvent(type, stagger);
  }

  public void InvokeAttackEvent(Brain.AttackType attackType, bool strong, Creature target)
  {
    Brain.AttackEvent onAttackEvent = this.OnAttackEvent;
    if (onAttackEvent == null)
      return;
    onAttackEvent(attackType, strong, target);
  }

  protected void OnCreatureDespawn(EventTime eventTime) => this.instance?.Unload();

  protected virtual void OnRagdollStateChange(
    Ragdoll.State previousState,
    Ragdoll.State newState,
    Ragdoll.PhysicStateChange physicStateChange,
    EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd || !this.creature.initialized || !this.creature.loaded)
      return;
    this.navMeshAgent.enabled = newState == Ragdoll.State.Standing || newState == Ragdoll.State.Kinematic || newState == Ragdoll.State.NoPhysic;
  }

  protected void OnZoneEvent(Zone zone, bool enter)
  {
    if (!zone.navSpeedModifier)
      return;
    if (enter)
    {
      if (this.slowZones.Contains(zone))
        return;
      this.slowZones.Add(zone);
    }
    else
    {
      if (!this.slowZones.Contains(zone))
        return;
      this.slowZones.Remove(zone);
    }
  }

  public virtual int GetFreeAvoidancePriority(int avoidancePriority)
  {
    foreach (Creature creature in Creature.allActive)
    {
      if (creature.state != Creature.State.Dead && (bool) (UnityEngine.Object) creature.brain && creature.brain.navMeshAgent.avoidancePriority == avoidancePriority)
        avoidancePriority = this.GetFreeAvoidancePriority(avoidancePriority + 1);
    }
    return avoidancePriority;
  }

  public float GetHorizontalDistance(Transform target)
  {
    return this.GetHorizontalDistance(target.position);
  }

  public float GetHorizontalDistance(UnityEngine.Vector3 targetPosition)
  {
    return UnityEngine.Vector3.Distance(this.creature.transform.position.ToXZ(), targetPosition.ToXZ());
  }

  public float GetDistance(Transform target) => this.GetDistance(target.position);

  public float GetDistance(UnityEngine.Vector3 targetPosition)
  {
    return UnityEngine.Vector3.Distance(this.creature.transform.position, targetPosition);
  }

  public float GetHeight(Transform target)
  {
    return target.position.y - this.creature.transform.position.y;
  }

  public float GetHorizontalAngle(Transform target)
  {
    return UnityEngine.Vector3.Angle(this.creature.transform.forward.ToXZ(), target.position.ToXZ() - this.creature.transform.position.ToXZ());
  }

  public bool CanSight(
    UnityEngine.Vector3 targetPosition,
    float sightThickness,
    float minDistance,
    float maxDistance,
    bool showDebugLines = false)
  {
    return Brain.InSightRange(this.creature.centerEyes.position, targetPosition, sightThickness, minDistance, maxDistance, showDebugLines);
  }

  public bool CanSight(Creature targetCreature, bool useDetectionMaxDistance = false, bool showDebugLines = false)
  {
    return this.CanSight(targetCreature, out RagdollPart _, useDetectionMaxDistance, showDebugLines);
  }

  public bool CanSight(
    Creature targetCreature,
    out RagdollPart sightedRagdollPart,
    bool useDetectionMaxDistance = false,
    bool showDebugLines = false)
  {
    BrainModuleSightable module = targetCreature.brain.instance?.GetModule<BrainModuleSightable>();
    if (module != null && module.IsSightable(this.creature.centerEyes.position, module.sightThickness, 0.0f, useDetectionMaxDistance ? module.sightDetectionMaxDistance : module.sightMaxDistance, out sightedRagdollPart, showDebugLines))
      return true;
    sightedRagdollPart = (RagdollPart) null;
    return false;
  }

  public bool CanSight(
    Creature targetCreature,
    float sightThickness,
    float minDistance,
    float maxDistance,
    bool showDebugLines = false)
  {
    return this.CanSight(targetCreature, sightThickness, minDistance, maxDistance, out RagdollPart _, showDebugLines);
  }

  public bool CanSight(
    Creature targetCreature,
    float sightThickness,
    float minDistance,
    float maxDistance,
    out RagdollPart sightedRagdollPart,
    bool showDebugLines = false)
  {
    BrainModuleSightable module = targetCreature.brain.instance?.GetModule<BrainModuleSightable>();
    if (module != null && module.IsSightable(this.creature.centerEyes.position, sightThickness, minDistance, maxDistance, out sightedRagdollPart, showDebugLines))
      return true;
    sightedRagdollPart = (RagdollPart) null;
    return false;
  }

  public static bool InSightRange(
    UnityEngine.Vector3 fromPosition,
    UnityEngine.Vector3 toPosition,
    float sightThickness,
    float minDistance,
    float maxDistance,
    bool showDebugLines = false)
  {
    float num = UnityEngine.Vector3.Distance(fromPosition, toPosition);
    if ((double) num < (double) minDistance || (double) num > (double) maxDistance)
      return false;
    UnityEngine.Vector3 normalized = (toPosition - fromPosition).normalized;
    LayerMask layerMask = (LayerMask) (1 << GameManager.GetLayer(LayerName.Default) | 1 << GameManager.GetLayer(LayerName.NoLocomotion));
    RaycastHit hitInfo;
    if (((double) sightThickness > 0.0 ? (Physics.SphereCast(fromPosition, sightThickness, normalized, out hitInfo, maxDistance, (int) layerMask) ? 1 : 0) : (Physics.Raycast(fromPosition, normalized, out hitInfo, maxDistance, (int) layerMask) ? 1 : 0)) != 0)
    {
      if ((double) hitInfo.distance < (double) num)
      {
        if (showDebugLines)
          Debug.DrawLine(fromPosition, hitInfo.point, UnityEngine.Color.red);
        return false;
      }
      if (showDebugLines)
        Debug.DrawRay(fromPosition, normalized * num, UnityEngine.Color.green);
      return true;
    }
    if (showDebugLines)
      Debug.DrawRay(fromPosition, normalized * num, UnityEngine.Color.green);
    return true;
  }

  public void AddNoStandUpModifier(object handler)
  {
    if (this.noStandupModifiers.Contains(handler))
      return;
    this.noStandupModifiers.Add(handler);
  }

  public void RemoveNoStandUpModifier(object handler)
  {
    if (!this.noStandupModifiers.Contains(handler))
      return;
    this.noStandupModifiers.Remove(handler);
  }

  public void ClearNoStandUpModifiers() => this.noStandupModifiers.Clear();

  protected void OnDrawGizmosSelected() => this.instance?.OnDrawGizmosSelected();

  protected void OnDrawGizmos() => this.instance?.OnDrawGizmos();

  public enum State
  {
    Idle,
    Follow,
    Patrol,
    Investigate,
    Alert,
    Combat,
    Grappled,
    Custom,
  }

  public delegate void PushEvent(Creature.PushType type, Brain.Stagger stagger);

  public delegate void AttackEvent(Brain.AttackType attackType, bool strong, Creature target);

  public enum AttackType
  {
    Melee,
    Bow,
    Cast,
    Throw,
  }

  public enum Stagger
  {
    None,
    LightAndMedium,
    Full,
  }
}
