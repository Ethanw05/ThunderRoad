// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemController
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using RootMotion;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Modules;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.VFX;

#nullable disable
namespace ThunderRoad;

public class GolemController : ThunderEntity
{
  [Header("References")]
  public Animator animator;
  public CharacterController characterController;
  public GolemAnimatorEvent animatorEvent;
  public List<CollisionListener> headListeners = new List<CollisionListener>();
  public List<Rigidbody> bodyParts = new List<Rigidbody>();
  public List<Collider> bodyPartColliders = new List<Collider>();
  public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
  public List<Collider> ignoreCollisionColliders = new List<Collider>();
  public List<Transform> magicSprayPoints = new List<Transform>();
  [Header("Global")]
  public bool awakeOnStart;
  public float moveSpeedMultiplier = 1.1f;
  public float hitDamageMultiplier = 1f;
  public float hitForceMultiplier = 1.1f;
  public Renderer headRenderer;
  [Header("Head look")]
  public MultiAimConstraint headAimConstraint;
  public Transform headIktarget;
  public float headLookSpeed = 3f;
  public LookMode lookMode;
  public LayerMask sightLayer = (LayerMask) 1;
  public float headLookSpeedMultiplier = 1f;
  public Transform eyeTransform;
  [Header("Face plate")]
  public Rigidbody facePlateBody;
  public SimpleBreakable facePlateBreakable;
  public ConfigurableJoint facePlateJoint;
  public float facePlateUnlockAngle = 5f;
  public float facePlateOpenAngle = 110f;
  [Header("Head crystal")]
  public ConfigurableJoint headCrystalJoint;
  public VisualEffect headCrystalLinkVfx;
  public ParticleSystem headCrystalParticle;
  public FxController headCrystalEffectController;
  public AudioSource headCrystalAudioSourceLoop;
  public AudioSource headCrystalTearingAudioSource;
  public AnimationCurve headCrystalLoopAudioPitchCurve;
  public AnimationCurve headCrystalTearingAudioPitchCurve;
  public AnimationCurve headCrystalTearingAudioVolumeCurve;
  public Rigidbody headCrystalBody;
  public Handle headCrystalHandle;
  public float headCrystalGrabMass = 5f;
  public float headCrystalGrabDrag = 100f;
  public float headCrystalShutdownDuration = 8f;
  public float headCrystalTearingDistance = 1f;
  [Header("Death")]
  public UnityEngine.Vector2 stunCheckCapsuleHeights = new UnityEngine.Vector2(1f, 2f);
  public UnityEngine.Vector3 radiusMinMaxCapsuleCast = new UnityEngine.Vector3(1.5f, 3.5f, 6f);
  public AudioSource killAudioSource;
  public ParticleSystem killparticle;
  public float killExplosionForce = 5f;
  public float killExplosionRadius = 5f;
  public float killExplosionUpward = 0.5f;
  public ForceMode killExplosionForceMode = ForceMode.VelocityChange;
  public Transform killExplosionSourceTransform;
  public List<Transform> colliderResizeOnDeath;
  public UnityEvent wakeEvent;
  public UnityEvent startStunEvent;
  public UnityEvent endStunEvent;
  public UnityEvent crystalBreakEvent;
  public UnityEvent defeatEvent;
  public UnityEvent killEvent;
  [Header("Swing")]
  public UnityEngine.Vector2 swingVelocity = new UnityEngine.Vector2(2f, 5f);
  public string swingEffectId = "GolemSwingArm";
  protected EffectData swingEffectData;
  protected EffectInstance[] swingEffects = new EffectInstance[2];
  public VelocityTracker[] swingTrackers = new VelocityTracker[2];
  public Transform handLeft;
  public Transform handRight;
  public Rigidbody[] armRigidbodies = new Rigidbody[2];
  [Header("Powers")]
  public Golem.Tier tier;
  public List<GolemAbility> abilities = new List<GolemAbility>();
  private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
  [Range(0.0f, 2f)]
  protected int wakeMotion;
  public static int awakeHash;
  public static int wakeMotionHash;
  public static int moveSpeedMultiplierHash;
  public static int isBusyHash;
  public static int moveHash;
  public static int inMovementHash;
  public static int locomotionMultHash;
  public static int staggerHash;
  public static int staggerLateralHash;
  public static int staggerAxialHash;
  public static int resistPushHash;
  public static int attackHash;
  public static int attackMotionHash;
  public static int deployHash;
  public static int deployStartedHash;
  public static int isDeployedHash;
  public static int stunHash;
  public static int stunDirectionHash;
  public static int stunStartedHash;
  public static int isStunnedHash;
  public static int inAttackMotionHash;
  [NonSerialized]
  public UnityEngine.Color targetHeadEmissionColor = UnityEngine.Color.black;
  [NonSerialized]
  public FloatHandler speed = new FloatHandler();
  [NonSerialized]
  public GolemSpawner spawner;
  [NonSerialized]
  public GolemAbility currentAbility;
  [NonSerialized]
  public Transform attackTarget;
  protected float orgCharacterRadius;
  protected UnityEngine.Vector3 gravityVelocity;
  protected Coroutine headLookCoroutine;
  protected float orgHeadCrystalMass;
  protected float orgHeadCrystalDrag;
  protected float meleeAttackStartTime;
  protected float stunEndTime;
  protected bool waitStunApproach;
  protected Coroutine deployRoutine;
  protected Coroutine attackEndCoroutine;
  protected System.Action replaceAttackCoroutineAction;

  public static GolemController.AttackSide GetAttackSide(GolemController.AttackMotion attack)
  {
    GolemController.AttackSide attackSide;
    switch (attack)
    {
      case GolemController.AttackMotion.Rampage:
        attackSide = GolemController.AttackSide.Both;
        break;
      case GolemController.AttackMotion.SwingRight:
        attackSide = GolemController.AttackSide.Left;
        break;
      case GolemController.AttackMotion.SwingLeft:
        attackSide = GolemController.AttackSide.Right;
        break;
      case GolemController.AttackMotion.ComboSwing:
        attackSide = GolemController.AttackSide.Both;
        break;
      case GolemController.AttackMotion.ComboSwingAndSlam:
        attackSide = GolemController.AttackSide.Both;
        break;
      case GolemController.AttackMotion.SwingBehindRight:
        attackSide = GolemController.AttackSide.Left;
        break;
      case GolemController.AttackMotion.SwingBehindLeft:
        attackSide = GolemController.AttackSide.Right;
        break;
      case GolemController.AttackMotion.SwingBehindRightTurnBack:
        attackSide = GolemController.AttackSide.Left;
        break;
      case GolemController.AttackMotion.SwingBehindLeftTurnBack:
        attackSide = GolemController.AttackSide.Right;
        break;
      case GolemController.AttackMotion.SwingLeftStep:
        attackSide = GolemController.AttackSide.Right;
        break;
      case GolemController.AttackMotion.SwingRightStep:
        attackSide = GolemController.AttackSide.Left;
        break;
      case GolemController.AttackMotion.Slam:
        attackSide = GolemController.AttackSide.Both;
        break;
      case GolemController.AttackMotion.Stampede:
        attackSide = GolemController.AttackSide.Both;
        break;
      case GolemController.AttackMotion.Breakdance:
        attackSide = GolemController.AttackSide.Both;
        break;
      case GolemController.AttackMotion.SlamLeftTurn90:
        attackSide = GolemController.AttackSide.Right;
        break;
      case GolemController.AttackMotion.SlamRightTurn90:
        attackSide = GolemController.AttackSide.Left;
        break;
      case GolemController.AttackMotion.SwingLeftTurn90:
        attackSide = GolemController.AttackSide.Right;
        break;
      case GolemController.AttackMotion.SwingRightTurn90:
        attackSide = GolemController.AttackSide.Left;
        break;
      case GolemController.AttackMotion.Spray:
        attackSide = GolemController.AttackSide.None;
        break;
      case GolemController.AttackMotion.SprayDance:
        attackSide = GolemController.AttackSide.Left;
        break;
      case GolemController.AttackMotion.Throw:
        attackSide = GolemController.AttackSide.None;
        break;
      case GolemController.AttackMotion.Beam:
        attackSide = GolemController.AttackSide.None;
        break;
      case GolemController.AttackMotion.SelfImbue:
        attackSide = GolemController.AttackSide.None;
        break;
      case GolemController.AttackMotion.RadialBurst:
        attackSide = GolemController.AttackSide.None;
        break;
      case GolemController.AttackMotion.ShakeOff:
        attackSide = GolemController.AttackSide.Both;
        break;
      default:
        attackSide = GolemController.AttackSide.None;
        break;
    }
    return attackSide;
  }

  public static GolemController.AttackSide GetAttackSide(Side side)
  {
    GolemController.AttackSide attackSide;
    switch (side)
    {
      case Side.Right:
        attackSide = GolemController.AttackSide.Right;
        break;
      case Side.Left:
        attackSide = GolemController.AttackSide.Left;
        break;
      default:
        attackSide = GolemController.AttackSide.None;
        break;
    }
    return attackSide;
  }

  public bool animatorIsRoot { get; protected set; }

  public GolemController.State state { get; protected set; }

  public bool isDefeated { get; protected set; }

  public bool isKilled { get; protected set; }

  public bool isLooking { get; protected set; }

  public Transform lookingTarget { get; protected set; }

  public bool isAwake
  {
    get => this.animator.GetBool(GolemController.awakeHash);
    set => this.animator.SetBool(GolemController.awakeHash, value);
  }

  public bool isBusy
  {
    get => this.animator.GetBool(GolemController.isBusyHash);
    set => this.animator.SetBool(GolemController.isBusyHash, value);
  }

  public bool inMovement => this.animator.GetBool(GolemController.inMovementHash);

  public bool inAttackMotion
  {
    get => this.animator.GetBool(GolemController.inAttackMotionHash);
    set => this.animator.SetBool(GolemController.inAttackMotionHash, value);
  }

  public bool isDeployed
  {
    get => this.animator.GetBool(GolemController.isDeployedHash);
    protected set => this.animator.SetBool(GolemController.isDeployedHash, value);
  }

  public bool deployInProgress
  {
    get
    {
      return this.animator.GetBool(GolemController.isDeployedHash) || this.animator.GetBool(GolemController.deployStartedHash);
    }
  }

  public bool isStunned => this.animator.GetBool(GolemController.isStunnedHash);

  public bool stunInProgress
  {
    get
    {
      return this.animator.GetBool(GolemController.isStunnedHash) || this.animator.GetBool(GolemController.stunStartedHash);
    }
  }

  public bool isActiveState
  {
    get => this.isBusy || this.inAttackMotion || this.deployInProgress || this.stunInProgress;
  }

  public bool isClimbed
  {
    get
    {
      if (this.grabbedParts.Count <= 0)
      {
        Rigidbody rigidBody1 = Player.currentCreature?.handLeft?.climb?.gripPhysicBody?.rigidBody;
        if (rigidBody1 == null || !this.bodyParts.Contains(rigidBody1))
        {
          Rigidbody rigidBody2 = Player.currentCreature?.handRight?.climb?.gripPhysicBody?.rigidBody;
          return rigidBody2 != null && this.bodyParts.Contains(rigidBody2);
        }
      }
      return true;
    }
  }

  public UnityEngine.Color HeadEmissionColor
  {
    get => this.headRenderer.material.GetColor(GolemController.EmissionColor);
    set => this.headRenderer.material.SetColor(GolemController.EmissionColor, value);
  }

  public event GolemController.GolemStateChange OnGolemStateChange;

  public event GolemController.GolemAttackEvent OnGolemAttackEvent;

  public event GolemController.GolemRampageEvent OnGolemRampage;

  public event GolemController.GolemStaggerEvent OnGolemStagger;

  public event GolemController.GolemStunEvent OnGolemStun;

  public event GolemController.GolemInterrupt OnGolemInterrupted;

  public event GolemController.GolemInterrupt OnGolemHeadshotInterrupt;

  public event GolemController.GolemDealDamage OnDamageDealt;

  protected List<ValueDropdownItem<string>> GetAllEffectID()
  {
    return Catalog.GetDropdownAllID(Category.Effect);
  }

  private void GetBodyParts()
  {
    this.bodyParts = new List<Rigidbody>();
    foreach (HumanBodyBones humanBoneId in Enum.GetValues(typeof (HumanBodyBones)))
    {
      foreach (Rigidbody componentsInChild in this.animator.GetComponentsInChildren<Rigidbody>())
      {
        if ((UnityEngine.Object) this.animator.GetBoneTransform(humanBoneId) == (UnityEngine.Object) componentsInChild.transform)
        {
          componentsInChild.isKinematic = true;
          this.bodyParts.Add(componentsInChild);
        }
      }
    }
  }

  private void GetBodyPartColliders()
  {
    this.bodyPartColliders = new List<Collider>();
    foreach (Rigidbody bodyPart in this.bodyParts)
    {
      foreach (Collider componentsInChild in bodyPart.GetComponentsInChildren<Collider>())
      {
        if ((UnityEngine.Object) componentsInChild.attachedRigidbody == (UnityEngine.Object) bodyPart && componentsInChild.name.EndsWith("_Collider"))
          this.bodyPartColliders.Add(componentsInChild);
      }
    }
  }

  public virtual void LookAt(Transform target)
  {
    if ((UnityEngine.Object) target == (UnityEngine.Object) null)
    {
      if (!this.isLooking)
        return;
      this.headIktarget.SetParent(this.transform);
      this.headIktarget.position = this.headAimConstraint.data.constrainedObject.position + this.headAimConstraint.data.constrainedObject.forward * 1f;
      this.lookingTarget = (Transform) null;
      this.isLooking = false;
    }
    else
    {
      if (!((UnityEngine.Object) target != (UnityEngine.Object) this.lookingTarget))
        return;
      this.headIktarget.position = this.headAimConstraint.data.constrainedObject.position + this.headAimConstraint.data.constrainedObject.forward * UnityEngine.Vector3.Distance(this.headAimConstraint.data.constrainedObject.position, target.position);
      this.lookingTarget = target;
      if (this.isLooking)
        return;
      this.headIktarget.SetParent((Transform) null);
      this.isLooking = true;
      this.headAimConstraint.weight = 1f;
      if (this.headLookCoroutine != null)
        this.StopCoroutine(this.headLookCoroutine);
      this.headLookCoroutine = this.StartCoroutine(this.LookAtCoroutine());
    }
  }

  public virtual void UnlockFacePlate(bool open)
  {
    this.facePlateJoint.angularXMotion = open ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
    this.facePlateJoint.angularYMotion = open ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked;
    this.headCrystalBody.isKinematic = !open;
    this.facePlateBody.isKinematic = !open;
    this.headCrystalLinkVfx.enabled = open;
    if (open)
    {
      this.facePlateBreakable.enabled = true;
      this.facePlateBreakable.allowedDamageTypes = ~SimpleBreakable.DamageType.None;
      this.facePlateBreakable.Restore();
      this.facePlateJoint.lowAngularXLimit = this.facePlateJoint.lowAngularXLimit with
      {
        limit = -this.facePlateUnlockAngle
      };
      this.headCrystalBody.mass = this.headCrystalGrabMass;
      this.headCrystalBody.drag = this.headCrystalGrabDrag;
      this.headCrystalBody.transform.SetParent((Transform) null);
      this.headCrystalParticle.Play();
      this.headCrystalLinkVfx.Play();
      this.headCrystalAudioSourceLoop.Play();
      this.SetHeadCrystalEffect(0.0f);
    }
    else
    {
      this.facePlateBreakable.enabled = false;
      this.facePlateBreakable.allowedDamageTypes = SimpleBreakable.DamageType.None;
      this.headCrystalBody.mass = this.orgHeadCrystalMass;
      this.headCrystalBody.drag = this.orgHeadCrystalDrag;
      this.headCrystalBody.transform.SetParent(this.animator.GetBoneTransform(HumanBodyBones.Head));
      this.headCrystalParticle.Stop();
      this.headCrystalLinkVfx.Stop();
      this.headCrystalAudioSourceLoop.Stop();
      this.headCrystalTearingAudioSource.Stop();
    }
  }

  public virtual void Kill() => this.StartCoroutine(this.KillCoroutine());

  public virtual IEnumerator KillCoroutine()
  {
    GolemController monoBehaviour = this;
    monoBehaviour.characterController.enabled = false;
    monoBehaviour.ChangeState(GolemController.State.Dead);
    monoBehaviour.RefreshGrabbed(true);
    foreach (SkinnedMeshRenderer skinnedMeshRenderer in monoBehaviour.skinnedMeshRenderers)
    {
      if ((bool) (UnityEngine.Object) skinnedMeshRenderer)
        skinnedMeshRenderer.updateWhenOffscreen = true;
    }
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.ProgressiveAction(0.3f, new Action<float>(monoBehaviour.\u003CKillCoroutine\u003Eb__183_0));
    foreach (Collider bodyPartCollider in monoBehaviour.bodyPartColliders)
    {
      if (bodyPartCollider is MeshCollider meshCollider && !meshCollider.convex)
      {
        meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.UseFastMidphase;
        meshCollider.convex = true;
        yield return (object) Yielders.EndOfFrame;
      }
    }
    foreach (Rigidbody bodyPart in monoBehaviour.bodyParts)
      bodyPart.isKinematic = false;
    foreach (Rigidbody bodyPart in monoBehaviour.bodyParts)
    {
      if ((double) monoBehaviour.killExplosionForce > 0.0)
        bodyPart.AddExplosionForce(monoBehaviour.killExplosionForce, monoBehaviour.killExplosionSourceTransform.position, monoBehaviour.killExplosionRadius, monoBehaviour.killExplosionUpward, monoBehaviour.killExplosionForceMode);
    }
    monoBehaviour.headCrystalBody.mass = monoBehaviour.orgHeadCrystalMass;
    monoBehaviour.headCrystalBody.drag = monoBehaviour.orgHeadCrystalDrag;
    monoBehaviour.headCrystalBody.ResetInertiaTensor();
    UnityEngine.Object.Destroy((UnityEngine.Object) monoBehaviour.headCrystalJoint);
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(2f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_1));
    monoBehaviour.headCrystalLinkVfx.Stop();
    monoBehaviour.headCrystalLinkVfx.gameObject.SetActive(false);
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.ProgressiveAction(monoBehaviour.headCrystalShutdownDuration, new Action<float>(monoBehaviour.\u003CKillCoroutine\u003Eb__183_2));
    monoBehaviour.animator.GetBoneTransform(HumanBodyBones.Head).GetComponent<Rigidbody>().AddExplosionForce(monoBehaviour.killExplosionForce, monoBehaviour.killExplosionSourceTransform.position, monoBehaviour.killExplosionRadius, monoBehaviour.killExplosionUpward, monoBehaviour.killExplosionForceMode);
    monoBehaviour.animator.GetBoneTransform(HumanBodyBones.Spine).GetComponent<Rigidbody>().AddExplosionForce(monoBehaviour.killExplosionForce, monoBehaviour.killExplosionSourceTransform.position, monoBehaviour.killExplosionRadius, monoBehaviour.killExplosionUpward, monoBehaviour.killExplosionForceMode);
    monoBehaviour.animator.GetBoneTransform(HumanBodyBones.UpperChest).GetComponent<Rigidbody>().AddExplosionForce(monoBehaviour.killExplosionForce, monoBehaviour.killExplosionSourceTransform.position, monoBehaviour.killExplosionRadius, monoBehaviour.killExplosionUpward, monoBehaviour.killExplosionForceMode);
    monoBehaviour.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).GetComponent<Rigidbody>().AddExplosionForce(monoBehaviour.killExplosionForce, monoBehaviour.killExplosionSourceTransform.position, monoBehaviour.killExplosionRadius, monoBehaviour.killExplosionUpward, monoBehaviour.killExplosionForceMode);
    monoBehaviour.animator.GetBoneTransform(HumanBodyBones.RightUpperArm).GetComponent<Rigidbody>().AddExplosionForce(monoBehaviour.killExplosionForce, monoBehaviour.killExplosionSourceTransform.position, monoBehaviour.killExplosionRadius, monoBehaviour.killExplosionUpward, monoBehaviour.killExplosionForceMode);
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(0.3f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_3));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(0.4f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_4));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(0.5f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_5));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(0.6f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_6));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(0.7f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_7));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(0.8f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_8));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(0.9f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_9));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(1f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_10));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(1.1f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_11));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(1.2f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_12));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(1.3f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_13));
    // ISSUE: reference to a compiler-generated method
    monoBehaviour.DelayedAction(1.5f, new System.Action(monoBehaviour.\u003CKillCoroutine\u003Eb__183_14));
    if ((bool) (UnityEngine.Object) monoBehaviour.headCrystalBody.transform.parent)
      monoBehaviour.headCrystalBody.gameObject.SetActive(false);
    monoBehaviour.animator.enabled = false;
    monoBehaviour.SetMove(false);
    monoBehaviour.headCrystalAudioSourceLoop.Stop();
    monoBehaviour.headCrystalTearingAudioSource.Stop();
    monoBehaviour.killparticle.transform.SetParent(monoBehaviour.transform);
    monoBehaviour.killparticle.Play();
    monoBehaviour.killAudioSource.Play();
    monoBehaviour.isKilled = true;
  }

  public virtual void Resurrect()
  {
    this.characterController.enabled = true;
    foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.skinnedMeshRenderers)
      skinnedMeshRenderer.updateWhenOffscreen = false;
    foreach (Rigidbody bodyPart in this.bodyParts)
      bodyPart.isKinematic = true;
    if ((bool) (UnityEngine.Object) this.headCrystalBody.transform.parent)
      this.headCrystalBody.gameObject.SetActive(true);
    this.animator.enabled = true;
    this.isKilled = false;
  }

  public virtual void SetAwake(bool awake)
  {
    this.isAwake = awake;
    this.animator.SetInteger(GolemController.wakeMotionHash, this.wakeMotion);
    if ((UnityEngine.Object) this.spawner != (UnityEngine.Object) null)
      this.spawner.StartWakeSequence(this.wakeMotion);
    if (!awake)
      return;
    this.StartCoroutine(this.WakeCoroutine());
  }

  public void Stun(float duration)
  {
    this.Stun(duration, (System.Action) null, (System.Action) null, (System.Action) null);
  }

  public virtual void StopStun()
  {
    if (this.isDefeated && this.stunInProgress)
      return;
    this.animator.SetBool(GolemController.stunHash, false);
  }

  public virtual void StaggerImpact(UnityEngine.Vector3 point)
  {
    UnityEngine.Vector3 vector3 = this.transform.InverseTransformDirection((this.transform.position.ToXZ() - point.ToXZ()).normalized);
    this.Stagger(new UnityEngine.Vector2(vector3.x, vector3.z));
  }

  public virtual void Stagger(float lateral, float axial)
  {
    this.Stagger(new UnityEngine.Vector2(lateral, axial));
  }

  public virtual void Stagger(UnityEngine.Vector2 direction)
  {
    if (this.stunInProgress || this.isDefeated || !this.isAwake)
      return;
    this.isBusy = true;
    this.EndAbility();
    this.StopDeploy();
    this.animator.SetTrigger(GolemController.staggerHash);
    direction = direction.normalized;
    this.animator.SetFloat(GolemController.staggerAxialHash, direction.y);
    this.animator.SetFloat(GolemController.staggerLateralHash, direction.x);
    GolemController.GolemStaggerEvent onGolemStagger = this.OnGolemStagger;
    if (onGolemStagger == null)
      return;
    onGolemStagger(direction);
  }

  public virtual void ResistPush(bool active)
  {
    this.isBusy = true;
    this.animator.SetBool(GolemController.resistPushHash, active);
  }

  public virtual void PerformAttackMotion(
    GolemController.AttackMotion meleeAttack,
    System.Action onMeleeEnd = null)
  {
    if (meleeAttack != GolemController.AttackMotion.Rampage && (UnityEngine.Object) this.currentAbility != (UnityEngine.Object) null)
      this.currentAbility.stepMotion = meleeAttack;
    this.animatorEvent.RightPlant((UnityEngine.AnimationEvent) null);
    this.animatorEvent.LeftPlant((UnityEngine.AnimationEvent) null);
    this.meleeAttackStartTime = Time.time;
    this.inAttackMotion = this.isBusy = true;
    this.lastAttackMotion = meleeAttack;
    this.animator.SetInteger(GolemController.attackMotionHash, (int) meleeAttack);
    this.animator.SetTrigger(GolemController.attackHash);
    if (this.attackEndCoroutine != null)
      this.replaceAttackCoroutineAction = (System.Action) (() => this.attackEndCoroutine = this.StartCoroutine(this.AttackMotionCoroutine(onMeleeEnd)));
    else
      this.attackEndCoroutine = this.StartCoroutine(this.AttackMotionCoroutine(onMeleeEnd));
    if (!((UnityEngine.Object) this.currentAbility == (UnityEngine.Object) null))
      return;
    GolemController.GolemAttackEvent golemAttackEvent = this.OnGolemAttackEvent;
    if (golemAttackEvent == null)
      return;
    golemAttackEvent(meleeAttack, (GolemAbility) null);
  }

  public virtual void UseAbility(int index) => this.StartAbility(this.abilities[index]);

  public void StartAbility(GolemAbility ability, System.Action endCallback = null)
  {
    if (this.stunInProgress)
      return;
    this.EndAbility();
    if ((UnityEngine.Object) ability == (UnityEngine.Object) null)
      return;
    this.isBusy = true;
    this.lastAbilityTime = Time.time;
    this.currentAbility = ability;
    this.currentAbility.Begin(this, endCallback);
    this.OnGolemInterrupted += new GolemController.GolemInterrupt(this.currentAbility.Interrupt);
    GolemController.GolemAttackEvent golemAttackEvent = this.OnGolemAttackEvent;
    if (golemAttackEvent == null)
      return;
    golemAttackEvent(ability is GolemBeam ? GolemController.AttackMotion.Beam : this.lastAttackMotion, ability);
  }

  public void EndAbility()
  {
    if ((UnityEngine.Object) this.currentAbility == (UnityEngine.Object) null)
      return;
    this.OnGolemInterrupted -= new GolemController.GolemInterrupt(this.currentAbility.Interrupt);
    this.currentAbility.OnEnd();
    this.currentAbility = (GolemAbility) null;
    this.lookMode = LookMode.Follow;
  }

  public void SetMoveSpeedMultiplier(float value)
  {
    this.animator.SetFloat(GolemController.moveSpeedMultiplierHash, value);
  }

  protected virtual void OnValidate()
  {
    if (!(bool) (UnityEngine.Object) this.animator)
      this.animator = this.GetComponentInChildren<Animator>();
    if (!(bool) (UnityEngine.Object) this.animatorEvent)
      this.animatorEvent = this.GetComponentInChildren<GolemAnimatorEvent>();
    if (!(bool) (UnityEngine.Object) this.headAimConstraint)
      this.headAimConstraint = this.GetComponentInChildren<MultiAimConstraint>();
    if (!(bool) (UnityEngine.Object) this.characterController)
      this.characterController = this.GetComponentInParent<CharacterController>();
    if (!this.skinnedMeshRenderers.IsNullOrEmpty())
      this.skinnedMeshRenderers.RemoveAll((Predicate<SkinnedMeshRenderer>) (x => (UnityEngine.Object) x == (UnityEngine.Object) null));
    if (this.skinnedMeshRenderers.IsNullOrEmpty())
      this.skinnedMeshRenderers = new List<SkinnedMeshRenderer>((IEnumerable<SkinnedMeshRenderer>) this.GetComponentsInChildren<SkinnedMeshRenderer>());
    if (this.bodyParts.Count == 0)
      this.GetBodyParts();
    if (!Application.isPlaying)
      return;
    this.SetMoveSpeedMultiplier(this.moveSpeedMultiplier);
  }

  public GolemAbility lastAbility { get; protected set; }

  public GolemController.AttackMotion lastAttackMotion { get; protected set; }

  public float lastAbilityTime { get; protected set; }

  public Dictionary<RagdollHand, Rigidbody> grabbedParts { get; protected set; } = new Dictionary<RagdollHand, Rigidbody>();

  public List<Creature> climbers { get; protected set; } = new List<Creature>();

  public RagdollHand mainGrabbedHand { get; protected set; }

  public Transform GetHand(Side side) => side != Side.Left ? this.handRight : this.handLeft;

  protected virtual void Awake()
  {
    this.Load((EntityData) null);
    this.statusImmune = true;
    this.facePlateBreakable.enabled = false;
    this.facePlateBreakable.allowedDamageTypes = SimpleBreakable.DamageType.None;
    this.facePlateBreakable.onBreak.AddListener(new UnityAction(this.OpenFacePlate));
    this.orgHeadCrystalMass = this.headCrystalBody.mass;
    this.orgHeadCrystalDrag = this.headCrystalBody.drag;
    this.orgCharacterRadius = this.characterController.radius;
    this.headCrystalJoint.autoConfigureConnectedAnchor = false;
    this.HeadCrystalHandlers(true);
    this.animatorIsRoot = (UnityEngine.Object) this.animator.gameObject == (UnityEngine.Object) this.gameObject;
    this.InitAnimationParametersHashes();
    this.UnlockFacePlate(false);
    this.SetMoveSpeedMultiplier(this.moveSpeedMultiplier);
    CrystalHuntProgressionModule module;
    if (GameModeManager.instance.currentGameMode.TryGetModule<CrystalHuntProgressionModule>(out module))
    {
      this.wakeMotion = (module.progressionLevel - 1) % 3;
      if (this.wakeMotion < 0)
        this.wakeMotion = 0;
    }
    else
      this.wakeMotion = UnityEngine.Random.Range(0, 3);
    this.SetAwake(this.awakeOnStart);
    this.speed.OnChangeEvent += (ValueHandler<float>.ChangeEvent) ((_, newValue) => this.animator.speed = newValue);
    this.OnGolemStagger += (GolemController.GolemStaggerEvent) (direction =>
    {
      GolemController.GolemInterrupt golemInterrupted = this.OnGolemInterrupted;
      if (golemInterrupted == null)
        return;
      golemInterrupted();
    });
    this.OnGolemStun += (GolemController.GolemStunEvent) (duration =>
    {
      GolemController.GolemInterrupt golemInterrupted = this.OnGolemInterrupted;
      if (golemInterrupted == null)
        return;
      golemInterrupted();
    });
    this.OnGolemRampage += (GolemController.GolemRampageEvent) (() =>
    {
      GolemController.GolemInterrupt golemInterrupted = this.OnGolemInterrupted;
      if (golemInterrupted == null)
        return;
      golemInterrupted();
    });
    foreach (CollisionListener headListener in this.headListeners)
      headListener.OnCollisionEnterEvent += new CollisionListener.CollisionEvent(this.OnHeadCollision);
    this.swingEffectData = Catalog.GetData<EffectData>(this.swingEffectId);
    this.headCrystalHandle.onDataLoaded += new Action<HandleData>(this.OnHandleDataLoaded);
  }

  private void OnHandleDataLoaded(HandleData handleData)
  {
    this.headCrystalHandle.SetTelekinesis(false);
    this.headCrystalHandle.SetTouch(false);
  }

  private void OnHeadCollision(UnityEngine.Collision other)
  {
    if ((UnityEngine.Object) this.currentAbility == (UnityEngine.Object) null || !this.currentAbility.HeadshotInterruptable || this.isDefeated || this.isKilled || (UnityEngine.Object) other.rigidbody?.GetComponentInParent<CollisionHandler>() == (UnityEngine.Object) null || (double) other.relativeVelocity.magnitude < 3.0)
      return;
    this.StaggerImpact(other.GetContact(0).point);
    GolemController.GolemInterrupt headshotInterrupt = this.OnGolemHeadshotInterrupt;
    if (headshotInterrupt == null)
      return;
    headshotInterrupt();
  }

  /// <summary>
  /// Register listeners to all the ladder handle events to enable player climbing
  /// </summary>
  public void RegisterGrabEvents()
  {
    RagdollHandClimb.OnClimberGrip += new RagdollHandClimb.GripEvent(this.ClimberGrip);
    RagdollHandClimb.OnClimberRelease += new RagdollHandClimb.GripEvent(this.ClimberRelease);
    Handle[] componentsInChildren = this.GetComponentsInChildren<Handle>();
    for (int index = 0; index < componentsInChildren.Length; ++index)
    {
      if (!((UnityEngine.Object) componentsInChildren[index].item != (UnityEngine.Object) null))
      {
        componentsInChildren[index].Grabbed += new Handle.GrabEvent(this.OnHandleGrab);
        componentsInChildren[index].UnGrabbed += new Handle.GrabEvent(this.OnHandleUnGrab);
      }
    }
  }

  private void ClimberGrip(RagdollHandClimb climber)
  {
    if (!this.bodyParts.Contains(climber.gripPhysicBody.rigidBody))
      return;
    this.OnClimbHandAdd(climber.gripPhysicBody.rigidBody, climber.ragdollHand);
  }

  private void ClimberRelease(RagdollHandClimb climber)
  {
    if (!this.grabbedParts.ContainsKey(climber.ragdollHand))
      return;
    this.OnClimbHandRemove(climber.ragdollHand);
  }

  private void OnHandleGrab(RagdollHand hand, Handle handle, EventTime time)
  {
    if (time == EventTime.OnStart)
      return;
    this.OnClimbHandAdd(handle.physicBody.rigidBody, hand);
  }

  private void OnHandleUnGrab(RagdollHand hand, Handle handle, EventTime time)
  {
    if (time == EventTime.OnStart)
      return;
    this.OnClimbHandRemove(hand);
  }

  public void OnClimbHandAdd(Rigidbody part, RagdollHand hand)
  {
    if ((UnityEngine.Object) this.mainGrabbedHand == (UnityEngine.Object) null && (bool) (UnityEngine.Object) hand.playerHand)
      this.mainGrabbedHand = hand;
    this.grabbedParts[hand] = part;
    if (!this.climbers.Contains(hand.creature))
      this.climbers.Add(hand.creature);
    if ((bool) (UnityEngine.Object) hand.playerHand)
      hand.playerHand.link.SetAllJointModifiers((object) this, 5f);
    this.RefreshGrabbed();
  }

  public void OnClimbHandRemove(RagdollHand hand)
  {
    if ((bool) (UnityEngine.Object) hand.playerHand && (UnityEngine.Object) this.mainGrabbedHand == (UnityEngine.Object) hand)
      this.mainGrabbedHand = this.grabbedParts.ContainsKey(this.mainGrabbedHand.otherHand) ? this.mainGrabbedHand.otherHand : (RagdollHand) null;
    if ((bool) (UnityEngine.Object) hand.playerHand)
      hand.playerHand.link.RemoveJointModifier((object) this);
    this.grabbedParts.Remove(hand);
    if (!this.grabbedParts.ContainsKey(hand.otherHand))
      this.climbers.Remove(hand.creature);
    this.RefreshGrabbed();
  }

  public void RefreshGrabbed(bool killStart = false)
  {
    bool flag1 = ((this.isDefeated ? 1 : (this.isKilled ? 1 : 0)) | (killStart ? 1 : 0)) != 0;
    bool flag2 = this.grabbedParts.Count > 0 && !flag1;
    if (flag2)
    {
      Player.local.SetFrameOfReference(this.grabbedParts[this.mainGrabbedHand].transform);
      Player.local.crouching = true;
      if (!Player.local.locomotion.colliderIsShrinking)
        Player.local.locomotion.StartShrinkCollider();
    }
    else
    {
      Player.local.SetFrameOfReference(!(bool) (UnityEngine.Object) this.mainGrabbedHand || flag1 ? (Transform) null : this.grabbedParts[this.mainGrabbedHand].transform);
      Player.local.crouching = false;
      if (Player.local.locomotion.colliderIsShrinking)
        Player.local.locomotion.StopShrinkCollider();
    }
    Player.local.autoAlign = !flag2;
    Player.currentCreature.climber.enabled = !flag2;
    if (flag2)
      Player.local.locomotion.groundMask = Player.local.locomotion.groundMask.RemoveFromMask(LayerName.PlayerLocomotionObject.ToString());
    else
      Player.local.locomotion.groundMask = Player.local.locomotion.groundMask.AddToMask(LayerName.PlayerLocomotionObject.ToString());
  }

  public void ForceUngripClimbers(bool climbedOnly, bool bothHands = false)
  {
    foreach (Creature forceUngripClimbers in this.ForceUngripClimbersEnumerable(climbedOnly, bothHands))
      ;
    this.RefreshGrabbed();
  }

  public IEnumerable<Creature> ForceUngripClimbersEnumerable(bool climbedOnly, bool bothHands = false)
  {
    for (int i = this.climbers.Count - 1; i >= 0; --i)
    {
      Creature climber = this.climbers[i];
      if (bothHands)
      {
        UngrabHand(climber.handRight);
        UngrabHand(climber.handLeft);
      }
      else
      {
        RagdollHand hand = climber.GetHand((Side) UnityEngine.Random.Range(0, 2));
        if (!UngrabHand(hand))
          UngrabHand(hand.otherHand);
      }
      if (!this.climbers.Contains(climber))
        yield return climber;
    }

    bool UngrabHand(RagdollHand hand)
    {
      if (climbedOnly && !this.grabbedParts.ContainsKey(hand))
        return false;
      if ((UnityEngine.Object) hand.grabbedHandle != (UnityEngine.Object) null)
        hand.UnGrab(false);
      if (hand.climb?.gripPhysicBody != (PhysicBody) null)
        hand.climb.UnGrip();
      hand.climb?.DisableGripTemp(0.5f);
      return true;
    }
  }

  public void InvokeDamageDealt(Creature target, float damage)
  {
    GolemController.GolemDealDamage onDamageDealt = this.OnDamageDealt;
    if (onDamageDealt == null)
      return;
    onDamageDealt(target, damage);
  }

  private void InitAnimationParametersHashes()
  {
    GolemController.awakeHash = Animator.StringToHash("Awake");
    GolemController.wakeMotionHash = Animator.StringToHash("WakeMotion");
    GolemController.locomotionMultHash = Animator.StringToHash("LocomotionMult");
    GolemController.isBusyHash = Animator.StringToHash("IsBusy");
    GolemController.moveHash = Animator.StringToHash("Move");
    GolemController.inMovementHash = Animator.StringToHash("InMovement");
    GolemController.staggerHash = Animator.StringToHash("Stagger");
    GolemController.staggerAxialHash = Animator.StringToHash("StaggerAxial");
    GolemController.staggerLateralHash = Animator.StringToHash("StaggerLateral");
    GolemController.resistPushHash = Animator.StringToHash("Resist");
    GolemController.attackHash = Animator.StringToHash("Attack");
    GolemController.attackMotionHash = Animator.StringToHash("AttackMotion");
    GolemController.inAttackMotionHash = Animator.StringToHash("InAttackMotion");
    GolemController.deployHash = Animator.StringToHash("Deploy");
    GolemController.deployStartedHash = Animator.StringToHash("DeployStarted");
    GolemController.isDeployedHash = Animator.StringToHash("IsDeployed");
    GolemController.stunHash = Animator.StringToHash("Stun");
    GolemController.stunDirectionHash = Animator.StringToHash("StunDirection");
    GolemController.stunStartedHash = Animator.StringToHash("StunStarted");
    GolemController.isStunnedHash = Animator.StringToHash("IsStunned");
    GolemController.moveSpeedMultiplierHash = Animator.StringToHash("MoveSpeedMultiplier");
  }

  protected override void Start()
  {
    base.Start();
    this.RegisterGrabEvents();
    foreach (Collider collisionCollider1 in this.ignoreCollisionColliders)
    {
      foreach (Collider collisionCollider2 in this.ignoreCollisionColliders)
      {
        if (!((UnityEngine.Object) collisionCollider1 == (UnityEngine.Object) collisionCollider2))
          Physics.IgnoreCollision(collisionCollider1, collisionCollider2, true);
      }
    }
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  public virtual void OnAnimatorMove()
  {
    if (this.animatorIsRoot)
    {
      this.animator.ApplyBuiltinRootMotion();
    }
    else
    {
      UnityEngine.Vector3 deltaPosition = this.animator.deltaPosition;
      if (this.characterController.enabled)
      {
        UnityEngine.Vector3 motion;
        if (this.characterController.isGrounded)
        {
          this.gravityVelocity = UnityEngine.Vector3.zero;
          motion = deltaPosition + Physics.gravity * Time.deltaTime;
        }
        else
        {
          this.gravityVelocity += Physics.gravity * Time.deltaTime * Time.deltaTime;
          motion = deltaPosition + this.gravityVelocity;
        }
        int num = (int) this.characterController.Move(motion);
      }
      else
        this.transform.position += deltaPosition;
      this.transform.rotation *= this.animator.deltaRotation;
    }
  }

  protected IEnumerator LookAtCoroutine()
  {
    while (this.isLooking)
    {
      if (!(bool) (UnityEngine.Object) this.lookingTarget)
      {
        this.LookAt((Transform) null);
        break;
      }
      GolemAbility currentAbility = this.currentAbility;
      if ((currentAbility != null ? (currentAbility.OverrideLook ? 1 : 0) : 0) != 0)
        this.currentAbility.LookAt();
      else if (this.lookMode == LookMode.Follow)
        this.headIktarget.transform.position = UnityEngine.Vector3.Lerp(this.headIktarget.transform.position, this.lookingTarget.position, this.headLookSpeed * this.headLookSpeedMultiplier * Time.deltaTime);
      yield return (object) null;
    }
    while ((double) this.headAimConstraint.weight > 0.0)
    {
      this.headAimConstraint.weight = Mathf.Clamp01(this.headAimConstraint.weight - Time.deltaTime);
      yield return (object) null;
    }
    this.headIktarget.position = UnityEngine.Vector3.zero;
    this.headLookCoroutine = (Coroutine) null;
  }

  public virtual void OpenFacePlate()
  {
    this.headCrystalHandle.SetTelekinesis(true);
    this.headCrystalHandle.SetTouch(true);
    this.facePlateJoint.lowAngularXLimit = this.facePlateJoint.lowAngularXLimit with
    {
      limit = -this.facePlateOpenAngle
    };
  }

  protected virtual void OnHeadCrystalGrabbed(
    RagdollHand ragdollHand,
    Handle handle,
    EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    this.LookAt(ragdollHand.playerHand.grip.transform);
    this.headCrystalJoint.angularXDrive = new JointDrive();
    this.headCrystalJoint.angularYZDrive = new JointDrive();
    this.headCrystalTearingAudioSource.Play();
    this.spawner?.onCrystalGrabbed.Invoke();
    this.InvokeRepeating("UpdateHeadCrystalTearing", 0.1f, 0.1f);
  }

  protected virtual void OnHeadCrystalTkGrabbed(Handle handle, SpellTelekinesis telekinesis)
  {
    this.LookAt(telekinesis.spellCaster.ragdollHand.grip.transform);
    this.headCrystalJoint.angularXDrive = new JointDrive();
    this.headCrystalJoint.angularYZDrive = new JointDrive();
    this.headCrystalTearingAudioSource.Play();
    this.spawner?.onCrystalGrabbed.Invoke();
    this.InvokeRepeating("UpdateHeadCrystalTearing", 0.1f, 0.1f);
  }

  protected void UpdateHeadCrystalTearing()
  {
    try
    {
      float radiusDistanceRatio;
      if (this.headCrystalLinkVfx.transform.position.PointInRadius(this.headCrystalHandle.transform.position, this.headCrystalTearingDistance, out radiusDistanceRatio))
      {
        this.SetHeadCrystalEffect(1f - radiusDistanceRatio);
      }
      else
      {
        this.HeadCrystalHandlers(false);
        this.Kill();
        this.CancelInvoke(nameof (UpdateHeadCrystalTearing));
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Exception when updating head crystal tearing: {ex}");
    }
  }

  private void HeadCrystalHandlers(bool add)
  {
    this.headCrystalHandle.Grabbed -= new Handle.GrabEvent(this.OnHeadCrystalGrabbed);
    this.headCrystalHandle.UnGrabbed -= new Handle.GrabEvent(this.OnHeadCrystalUnGrabbed);
    this.headCrystalHandle.TkGrabbed -= new Handle.TkEvent(this.OnHeadCrystalTkGrabbed);
    this.headCrystalHandle.TkUnGrabbed -= new Handle.TkEvent(this.OnHeadCrystalTkUnGrabbed);
    if (!add)
      return;
    this.headCrystalHandle.Grabbed += new Handle.GrabEvent(this.OnHeadCrystalGrabbed);
    this.headCrystalHandle.UnGrabbed += new Handle.GrabEvent(this.OnHeadCrystalUnGrabbed);
    this.headCrystalHandle.TkGrabbed += new Handle.TkEvent(this.OnHeadCrystalTkGrabbed);
    this.headCrystalHandle.TkUnGrabbed += new Handle.TkEvent(this.OnHeadCrystalTkUnGrabbed);
  }

  protected void SetHeadCrystalEffect(float value)
  {
    this.headCrystalAudioSourceLoop.pitch = this.headCrystalLoopAudioPitchCurve.Evaluate(value);
    this.headCrystalTearingAudioSource.volume = this.headCrystalTearingAudioVolumeCurve.Evaluate(value);
    this.headCrystalTearingAudioSource.pitch = this.headCrystalTearingAudioPitchCurve.Evaluate(value);
    this.headCrystalEffectController.SetIntensity(Mathf.Lerp(0.5f, 1f, value));
  }

  protected virtual void OnHeadCrystalUnGrabbed(
    RagdollHand ragdollHand,
    Handle handle,
    EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    this.headCrystalTearingAudioSource.Stop();
    this.CancelInvoke("UpdateHeadCrystalTearing");
    this.LookAt((Transform) null);
    this.spawner?.onCrystalUnGrabbed.Invoke();
    this.SetHeadCrystalEffect(0.0f);
  }

  protected virtual void OnHeadCrystalTkUnGrabbed(Handle handle, SpellTelekinesis telekinesis)
  {
    this.headCrystalTearingAudioSource.Stop();
    this.CancelInvoke("UpdateHeadCrystalTearing");
    this.LookAt((Transform) null);
    this.spawner?.onCrystalUnGrabbed.Invoke();
    this.SetHeadCrystalEffect(0.0f);
  }

  protected void ChangeState(GolemController.State newState)
  {
    if (newState == this.state)
      return;
    GolemController.GolemStateChange golemStateChange = this.OnGolemStateChange;
    if (golemStateChange != null)
      golemStateChange(newState);
    if (newState == GolemController.State.WakingUp)
      this.wakeEvent?.Invoke();
    if (newState == GolemController.State.Stunned && newState != this.state)
      this.startStunEvent?.Invoke();
    if (this.state == GolemController.State.Stunned && newState != this.state)
      this.endStunEvent?.Invoke();
    if (newState == GolemController.State.Defeated)
      this.defeatEvent?.Invoke();
    if (newState == GolemController.State.Dead)
      this.killEvent?.Invoke();
    this.state = newState;
  }

  protected internal override void ManagedUpdate()
  {
    base.ManagedUpdate();
    this.HeadEmissionColor = UnityEngine.Color.Lerp(this.HeadEmissionColor, this.targetHeadEmissionColor, Time.deltaTime * 10f);
  }

  protected virtual IEnumerator WakeCoroutine()
  {
    this.ChangeState(GolemController.State.WakingUp);
    while (this.isBusy)
      yield return (object) null;
    this.characterController.radius = this.orgCharacterRadius;
    this.characterController.enableOverlapRecovery = true;
    this.ChangeState(GolemController.State.Active);
    this.spawner?.onGolemAwaken?.Invoke();
  }

  public virtual void SetMove(bool active)
  {
    this.animator.SetBool(GolemController.moveHash, active);
  }

  public virtual void Stun(
    float duration = 0.0f,
    System.Action onStunStart = null,
    System.Action onStunnedBegin = null,
    System.Action onStunnedEnd = null)
  {
    this.isBusy = true;
    this.EndAbility();
    this.animator.SetBool(GolemController.stunHash, true);
    float num1 = 0.0f;
    int num2 = -2;
    for (int index = -1; index < 2; ++index)
    {
      RaycastHit hitInfo;
      float num3 = Mathf.InverseLerp(this.radiusMinMaxCapsuleCast.y, this.radiusMinMaxCapsuleCast.z, Physics.CapsuleCast(this.transform.position + new UnityEngine.Vector3(0.0f, this.stunCheckCapsuleHeights.x, 0.0f), this.transform.position + new UnityEngine.Vector3(0.0f, this.stunCheckCapsuleHeights.y, 0.0f), this.radiusMinMaxCapsuleCast.x, Quaternion.Euler(0.0f, (float) index * 120f, 0.0f) * this.transform.forward, out hitInfo, this.radiusMinMaxCapsuleCast.z, Common.MakeLayerMask(LayerName.Default, LayerName.LocomotionOnly)) ? hitInfo.distance : this.radiusMinMaxCapsuleCast.z);
      if ((double) UnityEngine.Random.Range(0.0f, num1 + num3) > (double) num1)
        num2 = index;
      num1 += num3;
    }
    if (num2 == -2)
      num2 = UnityEngine.Random.Range(-1, 2);
    this.animator.SetInteger(GolemController.stunDirectionHash, num2);
    this.StartCoroutine(this.StunCoroutine(duration, onStunStart, onStunnedBegin, onStunnedEnd));
  }

  protected IEnumerator StunCoroutine(
    float duration,
    System.Action onStunStart,
    System.Action onStunnedBegin,
    System.Action onStunnedEnd)
  {
    while (!this.animator.GetBool(GolemController.stunStartedHash))
      yield return (object) null;
    if (this.state != GolemController.State.Defeated && this.state != GolemController.State.Dead)
      this.ChangeState(GolemController.State.Stunned);
    System.Action action1 = onStunStart;
    if (action1 != null)
      action1();
    GolemController.GolemStunEvent onGolemStun = this.OnGolemStun;
    if (onGolemStun != null)
      onGolemStun(duration);
    while (!this.isStunned)
      yield return (object) null;
    this.waitStunApproach = true;
    this.stunEndTime = Time.time + duration;
    System.Action action2 = onStunnedBegin;
    if (action2 != null)
      action2();
    if (this.isDefeated)
      this.RefreshGrabbed();
    if ((double) duration > 0.0)
    {
      while (this.animator.GetBool(GolemController.stunHash) && (double) Time.time < (double) this.stunEndTime)
      {
        if (this.isDefeated)
          yield break;
        yield return (object) null;
      }
      this.StopStun();
    }
    while (this.isStunned)
      yield return (object) null;
    System.Action action3 = onStunnedEnd;
    if (action3 != null)
      action3();
    if (this.state == GolemController.State.Stunned)
      this.ChangeState(GolemController.State.Active);
  }

  protected void InvokeRampageState()
  {
    GolemController.GolemRampageEvent onGolemRampage = this.OnGolemRampage;
    if (onGolemRampage != null)
      onGolemRampage();
    this.ChangeState(GolemController.State.Rampage);
  }

  protected IEnumerator AttackMotionCoroutine(System.Action onAttackEnd)
  {
    this.replaceAttackCoroutineAction = (System.Action) null;
    yield return (object) Yielders.EndOfFrame;
    while (this.replaceAttackCoroutineAction == null && this.inAttackMotion)
      yield return (object) null;
    System.Action action = onAttackEnd;
    if (action != null)
      action();
    if (this.state == GolemController.State.Rampage && !this.inAttackMotion)
      this.ChangeState(GolemController.State.Active);
    this.attackEndCoroutine = (Coroutine) null;
    System.Action attackCoroutineAction = this.replaceAttackCoroutineAction;
    if (attackCoroutineAction != null)
      attackCoroutineAction();
  }

  public bool TryGetCurrentAttackMotion(out GolemController.AttackMotion meleeAttack)
  {
    meleeAttack = this.lastAttackMotion;
    return this.inAttackMotion;
  }

  public bool WithinForwardCone(Transform target, float maxDistance, float maxAngle)
  {
    return this.WithinForwardCone(target, maxDistance, out float _, maxAngle, out float _);
  }

  public bool WithinForwardCone(
    Transform target,
    float maxDistance,
    out float dist,
    float maxAngle)
  {
    return this.WithinForwardCone(target, maxDistance, out dist, maxAngle, out float _);
  }

  public bool WithinForwardCone(
    Transform target,
    float maxDistance,
    float maxAngle,
    out float angle)
  {
    return this.WithinForwardCone(target, maxDistance, out float _, maxAngle, out angle);
  }

  public bool WithinForwardCone(
    Transform target,
    float maxDistance,
    out float dist,
    float maxAngle,
    out float angle)
  {
    dist = 0.0f;
    angle = 0.0f;
    if ((UnityEngine.Object) target == (UnityEngine.Object) null)
      return false;
    angle = UnityEngine.Vector3.Angle(this.transform.forward.ToXZ().normalized, (target.position - this.transform.position).ToXZ().normalized);
    if ((double) angle > (double) maxAngle)
      return false;
    dist = UnityEngine.Vector3.Distance(this.eyeTransform.transform.position, target.position);
    return (double) dist <= (double) maxDistance;
  }

  public bool IsSightable(Transform target, float maxDistance, float maxAngle)
  {
    float dist;
    return this.WithinForwardCone(target, maxDistance, out dist, maxAngle) && !Physics.Raycast(this.eyeTransform.position, target.position - this.eyeTransform.position, dist, (int) this.sightLayer);
  }

  public void UpdateSwingEffects()
  {
    GolemController.AttackMotion meleeAttack;
    if (this.TryGetCurrentAttackMotion(out meleeAttack))
    {
      if (GolemController.GetAttackSide(meleeAttack).HasFlag((Enum) GolemController.AttackSide.Left))
        this.UpdateSwingEffect(Side.Left);
      if (!GolemController.GetAttackSide(meleeAttack).HasFlag((Enum) GolemController.AttackSide.Right))
        return;
      this.UpdateSwingEffect(Side.Right);
    }
    else
    {
      this.StopSwingEffect(Side.Left);
      this.StopSwingEffect(Side.Right);
    }
  }

  private void UpdateSwingEffect(Side side)
  {
    EffectInstance effectInstance = this.swingEffects[(int) side];
    VelocityTracker swingTracker = this.swingTrackers[(int) side];
    Rigidbody armRigidbody = this.armRigidbodies[(int) side];
    UnityEngine.Vector3 velocity = swingTracker.velocity;
    if ((effectInstance != null || (double) velocity.magnitude <= (double) this.swingVelocity.y ? (effectInstance == null ? 0 : ((double) velocity.magnitude > (double) this.swingVelocity.x ? 1 : 0)) : 1) != 0)
    {
      if (effectInstance == null)
      {
        effectInstance = this.swingEffects[(int) side] = this.swingEffectData.Spawn((Transform) swingTracker, (CollisionInstance) null, true, (ColliderGroup) null, false, 0.0f, 1f);
        effectInstance.Play();
      }
      UnityEngine.Vector3 vector3 = armRigidbody.transform.right * (side == Side.Left ? -1f : 1f);
      swingTracker.rotation = Quaternion.LookRotation(UnityEngine.Vector3.ProjectOnPlane(velocity, vector3), vector3);
      effectInstance.SetIntensity(Mathf.InverseLerp(0.0f, 15f, velocity.magnitude));
    }
    else
    {
      if (effectInstance == null)
        return;
      effectInstance.End();
      this.swingEffects[(int) side] = (EffectInstance) null;
    }
  }

  protected void StopSwingEffect(Side side)
  {
    if (this.swingEffects[(int) side] == null)
      return;
    this.swingEffects[(int) side]?.End();
    this.swingEffects[(int) side] = (EffectInstance) null;
  }

  public virtual void Deploy(
    float duration = 0.0f,
    System.Action onDeployStart = null,
    System.Action onDeployedBegin = null,
    System.Action onDeployedEnd = null)
  {
    this.isBusy = true;
    this.animator.SetBool(GolemController.deployHash, true);
    if (this.deployRoutine != null)
      this.StopCoroutine(this.deployRoutine);
    this.deployRoutine = this.StartCoroutine(this.DeployCoroutine(duration, onDeployStart, onDeployedBegin, onDeployedEnd));
  }

  protected IEnumerator DeployCoroutine(
    float duration,
    System.Action onDeployStart,
    System.Action onDeployedBegin,
    System.Action onDeployedEnd)
  {
    while (!this.animator.GetBool(GolemController.deployStartedHash))
      yield return (object) null;
    if (this.animator.GetBool(GolemController.deployHash))
    {
      System.Action action1 = onDeployStart;
      if (action1 != null)
        action1();
      while (!this.isDeployed)
      {
        if (!this.animator.GetBool(GolemController.deployHash))
          yield break;
        yield return (object) null;
      }
      System.Action action2 = onDeployedBegin;
      if (action2 != null)
        action2();
      if ((double) duration > 0.0)
      {
        float time = 0.0f;
        while (this.animator.GetBool(GolemController.deployHash) && (double) time < (double) duration)
        {
          time += Time.deltaTime;
          yield return (object) null;
        }
        this.animator.SetBool(GolemController.deployHash, false);
      }
      while (this.isDeployed)
        yield return (object) null;
      System.Action action3 = onDeployedEnd;
      if (action3 != null)
        action3();
      this.deployRoutine = (Coroutine) null;
    }
  }

  public virtual void StopDeploy()
  {
    if (this.deployRoutine != null)
      this.StopCoroutine(this.deployRoutine);
    this.isDeployed = false;
    this.animator.SetBool(GolemController.deployStartedHash, false);
    this.animator.SetBool(GolemController.deployHash, false);
    this.RunAfter((System.Action) (() => this.isDeployed = false), 2.5f);
  }

  public enum AttackMotion
  {
    Rampage,
    SwingRight,
    SwingLeft,
    ComboSwing,
    ComboSwingAndSlam,
    SwingBehindRight,
    SwingBehindLeft,
    SwingBehindRightTurnBack,
    SwingBehindLeftTurnBack,
    SwingLeftStep,
    SwingRightStep,
    Slam,
    Stampede,
    Breakdance,
    SlamLeftTurn90,
    SlamRightTurn90,
    SwingLeftTurn90,
    SwingRightTurn90,
    Spray,
    SprayDance,
    Throw,
    Beam,
    SelfImbue,
    RadialBurst,
    ShakeOff,
    LightShake,
  }

  [Flags]
  public enum AttackSide
  {
    None = 0,
    Left = 1,
    Right = 2,
    Both = Right | Left, // 0x00000003
  }

  public delegate void GolemStateChange(GolemController.State newState);

  public delegate void GolemAttackEvent(GolemController.AttackMotion motion, GolemAbility ability);

  public delegate void GolemRampageEvent();

  public delegate void GolemStaggerEvent(UnityEngine.Vector2 direction);

  public delegate void GolemStunEvent(float duration);

  public delegate void GolemInterrupt();

  public delegate void GolemDealDamage(Creature target, float damage);

  public enum State
  {
    Inactive,
    Active,
    Stunned,
    Rampage,
    Defeated,
    Dead,
    WakingUp,
  }
}
