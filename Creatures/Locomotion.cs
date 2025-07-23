// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Locomotion
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (Rigidbody))]
[AddComponentMenu("ThunderRoad/Locomotion")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Locomotion.html")]
public class Locomotion : ThunderBehaviour
{
  [Header("Ground movement")]
  public bool allowMove = true;
  public bool allowTurn = true;
  public bool allowJump = true;
  public bool allowCrouch = true;
  public AnimationCurve moveForceMultiplierByAngleCurve;
  public bool testMove;
  public float forwardSpeed = 0.2f;
  public float backwardSpeed = 0.2f;
  public float strafeSpeed = 0.2f;
  public float runSpeedAdd = 0.1f;
  public float crouchSpeed = 0.1f;
  public FloatHandler globalMoveSpeedMultiplier;
  protected float forwardSpeedMultiplier = 1f;
  protected float backwardSpeedMultiplier = 1f;
  protected float strafeSpeedMultiplier = 1f;
  protected float runSpeedMultiplier = 1f;
  protected float crouchSpeedMultiplier = 1f;
  protected float jumpForceMultiplier = 1f;
  public float forwardAngle = 10f;
  public float backwardAngle = 10f;
  public float runDot = 0.75f;
  public bool runEnabled = true;
  public float crouchHeightRatio = 0.8f;
  public float turnSpeed = 1f;
  private float snapTurnTime;
  private bool smoothSnapTurn;
  [Header("Jump / Fall")]
  public float horizontalAirSpeed = 0.02f;
  public float verticalAirSpeed;
  public float waterSpeed = 0.08f;
  public ForceMode airForceMode = ForceMode.VelocityChange;
  public float jumpGroundForce = 0.3f;
  public float jumpClimbVerticalMultiplier = 0.8f;
  public float jumpClimbVerticalMaxVelocityRatio = 20f;
  public float jumpClimbHorizontalMultiplier = 1f;
  public float jumpMaxDuration = 0.6f;
  [Header("Turn")]
  public float turnSmoothDirection;
  public float turnSnapDirection;
  public float turnSmoothSnapDirection;
  public UnityEngine.Vector3 moveDirection;
  public ForceMode verticalForceMode = ForceMode.VelocityChange;
  [Header("Colliders")]
  public float colliderRadius = 0.3f;
  public float colliderShrinkMinRadius = 0.05f;
  public float colliderGrowDuration = 2f;
  public float colliderHeight = 1f;
  [Tooltip("Only enable this on ")]
  public bool collideWithPlayer = true;
  [Header("Ground detection")]
  public float groundDetectionDistance = 0.05f;
  public PhysicMaterial colliderGroundMaterial;
  public float groundDrag = 3f;
  protected float groundDragMultiplier = 1f;
  public PhysicMaterial colliderFlyMaterial;
  public float flyDrag = 1f;
  protected float flyDragMultiplier = 1f;
  [NonSerialized]
  public PhysicBody physicBody;
  [NonSerialized]
  public CapsuleCollider capsuleCollider;
  protected float orgMass;
  protected float orgDrag;
  protected float orgAngularDrag;
  protected float orgSleepThreshold;
  public float customGravity;
  [NonSerialized]
  public UnityEngine.Vector3 prevVelocity;
  [NonSerialized]
  public UnityEngine.Vector3 prevPosition;
  [NonSerialized]
  public Quaternion prevRotation;
  [NonSerialized]
  public UnityEngine.Vector3 velocity;
  [NonSerialized]
  public float horizontalSpeed;
  [NonSerialized]
  public float verticalSpeed;
  [NonSerialized]
  public float angularSpeed;
  [NonSerialized]
  public bool isCrouched;
  [NonSerialized]
  public LayerMask groundMask;
  [NonSerialized]
  public bool isGrounded;
  [NonSerialized]
  public RaycastHit groundHit;
  [NonSerialized]
  public float groundAngle;
  private UnityEngine.Vector3 accelerationCurrentSpeed;
  [NonSerialized]
  public bool isJumping;
  protected UnityEngine.Vector3 jumpForce;
  protected float jumpTime;
  [NonSerialized]
  public bool jumpCharging;
  protected float jumpChargingTime;
  [NonSerialized]
  public float snapTurnDelay = 0.25f;
  [NonSerialized]
  public bool colliderIsShrinking;
  [NonSerialized]
  public Player player;
  [NonSerialized]
  public Creature creature;
  protected bool initialized;
  protected bool startGroundCheck = true;
  private Collider ignoredPlayerCreatureLocomotion;
  protected Coroutine stopShrinkColliderCoroutine;
  private Dictionary<Creature, List<Collider>> ignoredLocomotionOnlyColliders = new Dictionary<Creature, List<Collider>>();
  public List<Locomotion.SpeedModifier> speedModifiers;
  public List<Locomotion.PhysicModifier> physicModifiers;

  public bool locomotionError { get; private set; }

  public event Locomotion.GroundEvent OnGroundEvent;

  public event Locomotion.CrouchEvent OnCrouchEvent;

  public event Locomotion.CollisionEvent OnCollisionEnterEvent;

  public event Locomotion.FlyEvent OnFlyEvent;

  public event System.Action OnJumpEvent;

  public bool IsRunning => this.isGrounded && (double) this.prevVelocity.sqrMagnitude > 20.0;

  protected void Awake()
  {
    this.physicBody = this.gameObject.GetPhysicBody();
    this.physicBody.freezeRotation = true;
    this.physicBody.isKinematic = true;
    this.capsuleCollider = this.gameObject.AddComponent<CapsuleCollider>();
    this.capsuleCollider.radius = this.colliderRadius;
    this.capsuleCollider.height = this.colliderHeight;
    this.capsuleCollider.center = new UnityEngine.Vector3(0.0f, Mathf.Max(this.colliderHeight / 2f, this.colliderRadius), 0.0f);
    this.capsuleCollider.material = this.colliderGroundMaterial;
    this.player = this.GetComponent<Player>();
    this.creature = this.GetComponent<Creature>();
    this.globalMoveSpeedMultiplier = new FloatHandler();
    if ((bool) (UnityEngine.Object) this.player)
      this.gameObject.layer = GameManager.GetLayer(LayerName.PlayerLocomotion);
    else
      this.gameObject.layer = GameManager.GetLayer(LayerName.BodyLocomotion);
  }

  protected override void ManagedOnEnable()
  {
    if (!this.initialized)
      return;
    this.globalMoveSpeedMultiplier = new FloatHandler();
    this.physicBody.velocity = UnityEngine.Vector3.zero;
    this.physicBody.angularVelocity = UnityEngine.Vector3.zero;
    this.moveDirection = UnityEngine.Vector3.zero;
    this.horizontalSpeed = 0.0f;
    this.verticalSpeed = 0.0f;
    this.physicBody.isKinematic = false;
    this.capsuleCollider.isTrigger = false;
    this.MoveStop();
  }

  protected override void ManagedOnDisable()
  {
    this.physicBody.isKinematic = true;
    this.capsuleCollider.isTrigger = true;
    this.horizontalSpeed = 0.0f;
    this.verticalSpeed = 0.0f;
    this.velocity = UnityEngine.Vector3.zero;
    this.angularSpeed = 0.0f;
    this.moveDirection = UnityEngine.Vector3.zero;
  }

  public void Init()
  {
    this.groundMask = ThunderRoadSettings.current.groundLayer;
    if ((bool) (UnityEngine.Object) this.creature)
    {
      if (this.creature.data.overrideGroundMask)
        this.groundMask = this.creature.data.groundMask;
      this.physicBody.mass = this.creature.data.locomotionMass;
      this.forwardSpeed = this.creature.data.locomotionForwardSpeed;
      this.backwardSpeed = this.creature.data.locomotionBackwardSpeed;
      this.strafeSpeed = this.creature.data.locomotionStrafeSpeed;
      this.runSpeedAdd = this.creature.data.locomotionRunSpeedAdd;
      this.crouchSpeed = this.creature.data.locomotionCrouchSpeed;
      this.horizontalAirSpeed = this.creature.data.locomotionAirSpeed;
      this.jumpGroundForce = this.creature.data.locomotionJumpForce;
      this.jumpClimbVerticalMultiplier = this.creature.data.locomotionJumpClimbVerticalMultiplier;
      this.jumpClimbHorizontalMultiplier = this.creature.data.locomotionJumpClimbHorizontalMultiplier;
      this.jumpClimbVerticalMaxVelocityRatio = this.creature.data.jumpClimbVerticalMaxVelocityRatio;
      this.groundDrag = this.creature.data.locomotionGroundDrag;
      this.flyDrag = this.creature.data.locomotionFlyDrag;
      this.jumpMaxDuration = this.creature.data.locomotionJumpMaxDuration;
    }
    if (this.enabled)
    {
      this.physicBody.isKinematic = false;
      this.capsuleCollider.isTrigger = false;
    }
    this.initialized = true;
    this.startGroundCheck = true;
    this.orgMass = this.physicBody.mass;
    this.orgDrag = this.physicBody.drag;
    this.orgAngularDrag = this.physicBody.angularDrag;
    this.orgSleepThreshold = this.physicBody.sleepThreshold;
  }

  public override ManagedLoops EnabledManagedLoops
  {
    get => ManagedLoops.FixedUpdate | ManagedLoops.Update;
  }

  protected internal override void ManagedUpdate()
  {
    if (!this.collideWithPlayer && (UnityEngine.Object) this.ignoredPlayerCreatureLocomotion != (UnityEngine.Object) Player.currentCreature?.locomotion?.capsuleCollider)
    {
      if ((UnityEngine.Object) this.ignoredPlayerCreatureLocomotion != (UnityEngine.Object) null)
        Physics.IgnoreCollision((Collider) this.capsuleCollider, this.ignoredPlayerCreatureLocomotion, false);
      this.ignoredPlayerCreatureLocomotion = (Collider) Player.currentCreature?.locomotion?.capsuleCollider;
      if ((UnityEngine.Object) this.ignoredPlayerCreatureLocomotion != (UnityEngine.Object) null)
      {
        Physics.IgnoreCollision((Collider) this.capsuleCollider, this.ignoredPlayerCreatureLocomotion, true);
        Physics.IgnoreCollision((Collider) this.capsuleCollider, (Collider) Player.currentCreature?.currentLocomotion?.capsuleCollider, true);
        foreach (Collider componentsInChild in Player.local.globalOffsetTransform.GetComponentsInChildren<Collider>())
          Physics.IgnoreCollision((Collider) this.capsuleCollider, componentsInChild, true);
      }
    }
    this.UpdateGrounded(this.startGroundCheck);
    if (this.startGroundCheck)
      this.startGroundCheck = false;
    this.CrouchCheck();
    this.TestMove(this.transform, UnityEngine.Vector3.up);
    float deltaTime = Time.deltaTime;
    if (this.physicBody.isKinematic)
    {
      if ((double) Time.deltaTime > 0.0)
        this.velocity = (this.transform.position - this.prevPosition) / deltaTime;
      this.horizontalSpeed = new UnityEngine.Vector3(this.velocity.x, 0.0f, this.velocity.z).magnitude;
      this.verticalSpeed = this.velocity.y;
      this.prevPosition = this.transform.position;
    }
    else
    {
      this.velocity = this.prevVelocity;
      this.horizontalSpeed = new UnityEngine.Vector3(this.velocity.x, 0.0f, this.velocity.z).magnitude;
      this.verticalSpeed = this.velocity.y;
      this.prevVelocity = this.physicBody.velocity;
    }
    if ((double) deltaTime > 0.0)
      this.angularSpeed = Mathf.DeltaAngle(0.0f, (this.transform.rotation * Quaternion.Inverse(this.prevRotation)).eulerAngles.y) / deltaTime;
    this.prevRotation = this.transform.rotation;
  }

  public void UpdateGrounded(bool forceInvokeFlyGround = false)
  {
    UnityEngine.Vector3 up = UnityEngine.Vector3.up;
    if (!(bool) (UnityEngine.Object) Level.master)
      return;
    UnityEngine.Vector3 lossyScale = this.transform.lossyScale;
    if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration))
    {
      foreach (Creature creature in Creature.allActive)
      {
        if (creature.isKilled && creature.ragdoll.state == Ragdoll.State.Inert)
          creature.ragdoll.SetPartsLayer(LayerName.Ragdoll);
      }
    }
    if (Physics.SphereCast(this.capsuleCollider.transform.TransformPoint(this.capsuleCollider.center), this.capsuleCollider.radius * 0.99f * lossyScale.y, -up, out this.groundHit, 1000f, (int) this.groundMask, QueryTriggerInteraction.Ignore))
    {
      this.groundAngle = UnityEngine.Vector3.Angle(up, this.groundHit.normal);
      if ((double) Mathf.Clamp(this.groundHit.distance - (float) ((double) this.capsuleCollider.height / 2.0 - (double) this.capsuleCollider.radius * 0.99000000953674316) * lossyScale.y, 0.0f, float.PositiveInfinity) > (double) this.groundDetectionDistance)
      {
        if (forceInvokeFlyGround || this.isGrounded)
        {
          this.groundHit.normal = up;
          this.OnFly(forceInvokeFlyGround);
        }
      }
      else if (forceInvokeFlyGround || !this.isGrounded)
        this.OnGround(this.groundHit.point, this.velocity, (Collider) this.capsuleCollider, forceInvokeFlyGround);
    }
    else if (forceInvokeFlyGround || this.isGrounded)
    {
      this.groundHit.distance = 1000f;
      this.groundHit.normal = up;
      this.groundAngle = 0.0f;
      this.OnFly(forceInvokeFlyGround);
    }
    if (GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration))
      return;
    foreach (Creature creature in Creature.allActive)
    {
      if (creature.isKilled && creature.ragdoll.state == Ragdoll.State.Inert)
        creature.ragdoll.RefreshPartsLayer();
    }
  }

  private void TestMove(Transform t, UnityEngine.Vector3 vector3Up)
  {
    if (!this.testMove)
      return;
    this.moveDirection = Quaternion.Euler(0.0f, t.rotation.eulerAngles.y, 0.0f) * new UnityEngine.Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")) * this.forwardSpeed;
    float axis = Input.GetAxis("Rotation");
    if ((double) axis <= 0.10000000149011612 && (double) axis >= -0.10000000149011612)
      return;
    t.RotateAround(t.position, vector3Up, (float) ((double) axis * (double) this.turnSpeed * 3.0));
  }

  protected void CrouchCheck()
  {
    if (!(bool) (UnityEngine.Object) this.player || !this.allowCrouch || (UnityEngine.Object) this.player.creature == (UnityEngine.Object) null)
      return;
    if ((double) this.player.creature.GetAnimatorHeightRatio() > (double) this.crouchHeightRatio)
    {
      if (!this.isCrouched)
        return;
      this.OnCrouch(false);
    }
    else
    {
      if (this.isCrouched)
        return;
      this.OnCrouch(true);
    }
  }

  protected void OnCrouch(bool isCrouching)
  {
    this.isCrouched = isCrouching;
    Locomotion.CrouchEvent onCrouchEvent = this.OnCrouchEvent;
    if (onCrouchEvent == null)
      return;
    onCrouchEvent(isCrouching);
  }

  protected void OnGround(
    UnityEngine.Vector3 groundPoint,
    UnityEngine.Vector3 velocity,
    Collider groundCollider,
    bool silent = false)
  {
    this.isGrounded = true;
    this.capsuleCollider.material = this.colliderGroundMaterial;
    this.physicBody.drag = this.groundDrag * this.groundDragMultiplier;
    if (silent)
      return;
    Locomotion.GroundEvent onGroundEvent = this.OnGroundEvent;
    if (onGroundEvent == null)
      return;
    onGroundEvent(this, groundPoint, velocity, groundCollider);
  }

  protected void OnFly(bool silent = false)
  {
    this.isGrounded = false;
    this.capsuleCollider.material = this.colliderFlyMaterial;
    this.physicBody.drag = this.flyDrag * this.flyDragMultiplier;
    if (silent)
      return;
    Locomotion.FlyEvent onFlyEvent = this.OnFlyEvent;
    if (onFlyEvent == null)
      return;
    onFlyEvent(this);
  }

  public void SetCapsuleCollider(float height)
  {
    this.capsuleCollider.height = height;
    if ((double) height < (double) this.capsuleCollider.radius)
      this.capsuleCollider.radius = height;
    this.capsuleCollider.center = new UnityEngine.Vector3(this.capsuleCollider.center.x, Mathf.Max(this.capsuleCollider.height / 2f, this.capsuleCollider.radius), this.capsuleCollider.center.z);
  }

  private void OnCollisionEnter(UnityEngine.Collision collision)
  {
    if (collision.collider.gameObject.layer == GameManager.GetLayer(LayerName.LocomotionOnly))
    {
      Creature otherCreature = collision.collider.GetComponentInParent<Creature>();
      if ((UnityEngine.Object) otherCreature != (UnityEngine.Object) null)
      {
        Physics.IgnoreCollision((Collider) this.capsuleCollider, collision.collider);
        List<Collider> colliderList;
        if (this.ignoredLocomotionOnlyColliders.TryGetValue(otherCreature, out colliderList))
        {
          colliderList.Add(collision.collider);
          return;
        }
        this.ignoredLocomotionOnlyColliders.Add(otherCreature, new List<Collider>()
        {
          collision.collider
        });
        otherCreature.OnDespawnEvent += new Creature.DespawnEvent(UnignoreCollision);
        return;
      }

      void UnignoreCollision(EventTime eventTime)
      {
        List<Collider> colliderList;
        if (this.ignoredLocomotionOnlyColliders.TryGetValue(otherCreature, out colliderList))
        {
          foreach (Collider collider2 in colliderList)
            Physics.IgnoreCollision((Collider) this.capsuleCollider, collider2, false);
        }
        otherCreature.OnDespawnEvent -= new Creature.DespawnEvent(UnignoreCollision);
      }
    }
    Locomotion.CollisionEvent collisionEnterEvent = this.OnCollisionEnterEvent;
    if (collisionEnterEvent == null)
      return;
    collisionEnterEvent(collision);
  }

  public void MoveWeighted(
    UnityEngine.Vector3 direction,
    Transform bodyTransform,
    float heightRatio,
    float moveSpeedRatio = 1f,
    float runSpeedRatio = 0.0f,
    float acceleration = 0.0f)
  {
    if (!this.allowMove)
      return;
    if ((bool) (UnityEngine.Object) this.player)
      heightRatio = this.player.creature.GetAnimatorHeightRatio();
    float num1 = this.horizontalAirSpeed;
    if (this.isGrounded)
    {
      if ((double) heightRatio > (double) this.crouchHeightRatio)
      {
        UnityEngine.Vector3 normalized1 = bodyTransform.forward.ToXZ().normalized;
        UnityEngine.Vector3 normalized2 = direction.normalized;
        float num2 = Mathf.Clamp01(UnityEngine.Vector3.Dot(normalized1, normalized2));
        float num3 = Mathf.Clamp01(UnityEngine.Vector3.Dot(-normalized1, normalized2));
        float num4 = Mathf.Abs(UnityEngine.Vector3.Dot(bodyTransform.right.ToXZ().normalized, normalized2));
        double num5 = (double) this.forwardSpeed * (double) this.forwardSpeedMultiplier * (double) num2 * (double) moveSpeedRatio;
        float num6 = this.backwardSpeed * this.backwardSpeedMultiplier * num3 * moveSpeedRatio;
        float num7 = this.strafeSpeed * this.strafeSpeedMultiplier * num4 * moveSpeedRatio;
        float num8 = (double) num2 > (double) this.runDot ? this.runSpeedAdd * this.runSpeedMultiplier * runSpeedRatio : 0.0f;
        double num9 = (double) num6;
        num1 = ((float) (num5 + num9) + num7 + num8) * Mathf.Clamp01(this.transform.lossyScale.y);
      }
      else
        num1 = this.crouchSpeed * this.crouchSpeedMultiplier * Mathf.Clamp01(this.transform.lossyScale.y);
    }
    Player player = this.player;
    int num10;
    if (player == null)
    {
      Creature creature = this.creature;
      num10 = creature != null ? (creature.isSwimming ? 1 : 0) : 0;
    }
    else
      num10 = player.creature.isSwimming ? 1 : 0;
    if (num10 != 0)
      num1 = this.waterSpeed;
    this.moveDirection.x = direction.x * num1;
    this.moveDirection.z = direction.z * num1;
    if ((double) acceleration <= 0.0)
      return;
    UnityEngine.Vector3 vector3 = UnityEngine.Vector3.SmoothDamp(this.velocity, this.moveDirection, ref this.accelerationCurrentSpeed, acceleration);
    this.moveDirection.x = vector3.x;
    this.moveDirection.z = vector3.z;
  }

  public void MoveVertical(float directionY)
  {
    if (!this.isGrounded && (double) this.verticalAirSpeed > 0.0)
      this.moveDirection.y = directionY * this.verticalAirSpeed;
    else
      this.moveDirection.y = 0.0f;
  }

  public void MoveStop() => this.moveDirection = UnityEngine.Vector3.zero;

  protected internal override void ManagedFixedUpdate()
  {
    if (this.allowTurn)
    {
      float angle = 0.0f;
      if ((double) this.turnSmoothDirection != 0.0)
        angle = this.turnSmoothDirection * this.turnSpeed * TimeManager.GetTimeStepMultiplier();
      else if ((double) this.turnSnapDirection != 0.0 && (double) Time.time - (double) this.snapTurnTime > (double) this.snapTurnDelay)
      {
        angle = (float) ((double) this.turnSnapDirection * (double) this.turnSpeed * ((double) this.snapTurnDelay / (double) Time.fixedDeltaTime)) * TimeManager.GetTimeStepMultiplier();
        this.snapTurnTime = Time.time;
      }
      else if ((double) this.turnSmoothSnapDirection == 0.0)
        this.smoothSnapTurn = false;
      else if ((double) Time.time - (double) this.snapTurnTime > (double) this.snapTurnDelay)
      {
        this.smoothSnapTurn = !this.smoothSnapTurn;
        this.snapTurnTime = Time.time;
      }
      else if (this.smoothSnapTurn)
        angle = (float) ((double) this.turnSmoothSnapDirection * (double) this.turnSpeed * 2.0) * TimeManager.GetTimeStepMultiplier();
      if ((double) angle != 0.0)
        this.transform.RotateAround(this.transform.TransformPoint(this.capsuleCollider.center), this.transform.up, angle);
    }
    if (this.allowMove)
    {
      this.locomotionError = true;
      if (!float.IsNaN(this.moveDirection.x) && !float.IsNaN(this.moveDirection.z))
      {
        if ((double) this.moveDirection.x != 0.0 || (double) this.moveDirection.z != 0.0)
        {
          UnityEngine.Vector3 vector3 = new UnityEngine.Vector3(this.moveDirection.x, 0.0f, this.moveDirection.z) * TimeManager.GetTimeStepMultiplier() * (float) (ValueHandler<float>) this.globalMoveSpeedMultiplier;
          float num1 = this.moveForceMultiplierByAngleCurve.Evaluate(this.groundAngle);
          float num2 = (UnityEngine.Object) Player.currentCreature?.currentLocomotion == (UnityEngine.Object) this ? 1f : Mathf.Clamp01(1f / num1);
          UnityEngine.Vector3 force = UnityEngine.Vector3.ProjectOnPlane(vector3, this.isGrounded ? this.groundHit.normal : UnityEngine.Vector3.up).normalized * (vector3.magnitude * (!this.isGrounded ? 1f : ((double) UnityEngine.Vector3.Angle(vector3, this.groundHit.normal) > 90.0 ? num1 : num2)));
          if (!float.IsNaN(force.x) && !float.IsNaN(force.y) && !float.IsNaN(force.z))
          {
            this.locomotionError = false;
            this.physicBody.AddForce(force, this.isGrounded ? ForceMode.VelocityChange : this.airForceMode);
          }
        }
        if ((double) this.moveDirection.y != 0.0)
        {
          this.locomotionError = false;
          this.physicBody.AddForce(new UnityEngine.Vector3(0.0f, this.moveDirection.y, 0.0f), this.verticalForceMode);
        }
      }
    }
    if (this.isJumping)
    {
      if ((double) this.jumpTime > 0.0)
      {
        this.physicBody.AddForce(new UnityEngine.Vector3(Utils.CalculateRatio(this.jumpTime, 0.0f, this.jumpMaxDuration, 0.0f, this.jumpForce.x), Utils.CalculateRatio(this.jumpTime, 0.0f, this.jumpMaxDuration, 0.0f, this.jumpForce.y), Utils.CalculateRatio(this.jumpTime, 0.0f, this.jumpMaxDuration, 0.0f, this.jumpForce.z)) * TimeManager.GetTimeStepMultiplier() * this.jumpForceMultiplier, ForceMode.VelocityChange);
        this.jumpTime -= Time.deltaTime;
      }
      else
      {
        this.jumpTime = 0.0f;
        this.isJumping = false;
      }
    }
    if ((double) this.customGravity == 0.0)
      return;
    this.physicBody.AddForce(this.customGravity * Physics.gravity, ForceMode.Acceleration);
  }

  public void Move(UnityEngine.Vector3 direction)
  {
    if (!this.allowMove)
      return;
    if (this.isGrounded)
    {
      this.moveDirection.x = direction.x * this.forwardSpeed * this.forwardSpeedMultiplier;
      this.moveDirection.z = direction.z * this.forwardSpeed * this.forwardSpeedMultiplier;
    }
    else
    {
      this.moveDirection.x = direction.x * this.horizontalAirSpeed;
      this.moveDirection.z = direction.z * this.horizontalAirSpeed;
    }
  }

  public void Jump(bool active)
  {
    if (!this.allowJump)
      return;
    if (!active)
      this.isJumping = false;
    else if (this.isGrounded)
    {
      if (this.isJumping)
        return;
      this.jumpForce = new UnityEngine.Vector3(0.0f, this.jumpGroundForce, 0.0f);
      this.jumpTime = this.jumpMaxDuration;
      this.isJumping = true;
      if ((bool) (UnityEngine.Object) this.player && (bool) (UnityEngine.Object) this.player.creature && this.player.creature.data.jumpEffectData != null)
      {
        EffectInstance effectInstance = this.player.creature.data.jumpEffectData.Spawn(this.player.creature.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
        effectInstance.source = (object) Player.currentCreature;
        effectInstance.Play();
      }
      System.Action onJumpEvent1 = this.OnJumpEvent;
      if (onJumpEvent1 != null)
        onJumpEvent1();
      if (!(bool) (UnityEngine.Object) this.player || !(bool) (UnityEngine.Object) this.player.creature)
        return;
      System.Action onJumpEvent2 = this.player.creature.locomotion.OnJumpEvent;
      if (onJumpEvent2 == null)
        return;
      onJumpEvent2();
    }
    else
    {
      if (!(bool) (UnityEngine.Object) this.player || !(bool) (UnityEngine.Object) this.player.creature || this.isJumping)
        return;
      bool flag = false;
      if ((bool) (UnityEngine.Object) this.player.creature.handLeft.grabbedHandle && (this.player.creature.handLeft.grabbedHandle.data.forceClimbing || (bool) (UnityEngine.Object) this.player.creature.handLeft.grabbedHandle.item && this.player.creature.handLeft.grabbedHandle.item.data.grabAndGripClimb))
      {
        this.player.creature.handLeft.UnGrab(true);
        flag = true;
      }
      if ((bool) (UnityEngine.Object) this.player.creature.handRight.grabbedHandle && (this.player.creature.handRight.grabbedHandle.data.forceClimbing || (bool) (UnityEngine.Object) this.player.creature.handRight.grabbedHandle.item && this.player.creature.handRight.grabbedHandle.item.data.grabAndGripClimb))
      {
        this.player.creature.handRight.UnGrab(true);
        flag = true;
      }
      if (this.player.creature.handLeft.climb.isGripping && (bool) (UnityEngine.Object) this.player.creature.handLeft.climb.gripItem && this.player.creature.handLeft.climb.gripItem.data.grabAndGripClimb)
        flag = true;
      if (this.player.creature.handRight.climb.isGripping && (bool) (UnityEngine.Object) this.player.creature.handRight.climb.gripItem && this.player.creature.handRight.climb.gripItem.data.grabAndGripClimb)
        flag = true;
      if (this.player.creature.handLeft.climb.isGripping && (this.player.creature.handLeft.climb.gripPhysicBody.isKinematic || this.player.creature.handLeft.climb.gripPhysicBody.gameObject.CompareTag("AllowJumpClimb")))
        flag = true;
      if (this.player.creature.handRight.climb.isGripping && (this.player.creature.handRight.climb.gripPhysicBody.isKinematic || this.player.creature.handRight.climb.gripPhysicBody.gameObject.CompareTag("AllowJumpClimb")))
        flag = true;
      if (this.player.creature.climber.footLeft.state == FeetClimber.Foot.State.Posed)
      {
        flag = true;
        this.player.creature.climber.footLeft.state = FeetClimber.Foot.State.Idle;
      }
      if (this.player.creature.climber.footRight.state == FeetClimber.Foot.State.Posed)
      {
        flag = true;
        this.player.creature.climber.footRight.state = FeetClimber.Foot.State.Idle;
      }
      if (!flag)
        return;
      this.jumpForce = new UnityEngine.Vector3(this.player.head.cam.transform.forward.normalized.x * this.jumpGroundForce * this.jumpClimbHorizontalMultiplier, this.jumpGroundForce * this.jumpClimbVerticalMultiplier * Mathf.InverseLerp(this.jumpClimbVerticalMaxVelocityRatio, 0.0f, this.physicBody.velocity.y), this.player.head.cam.transform.forward.normalized.z * this.jumpGroundForce * this.jumpClimbHorizontalMultiplier);
      this.jumpTime = this.jumpMaxDuration;
      this.isJumping = true;
      if ((bool) (UnityEngine.Object) this.player && (bool) (UnityEngine.Object) this.player.creature && this.player.creature.data.jumpEffectData != null)
      {
        EffectInstance effectInstance = this.player.creature.data.jumpEffectData.Spawn(this.player.creature.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
        effectInstance.source = (object) Player.currentCreature;
        effectInstance.Play();
      }
      System.Action onJumpEvent3 = this.OnJumpEvent;
      if (onJumpEvent3 != null)
        onJumpEvent3();
      if (!(bool) (UnityEngine.Object) this.player || !(bool) (UnityEngine.Object) this.player.creature)
        return;
      System.Action onJumpEvent4 = this.player.creature.locomotion.OnJumpEvent;
      if (onJumpEvent4 == null)
        return;
      onJumpEvent4();
    }
  }

  public void Turn(float speed, Locomotion.TurnMode turnMode)
  {
    switch (turnMode)
    {
      case Locomotion.TurnMode.Instant:
        this.turnSnapDirection = speed;
        break;
      case Locomotion.TurnMode.Snap:
        this.turnSmoothSnapDirection = speed;
        break;
      case Locomotion.TurnMode.Smooth:
        this.turnSmoothDirection = speed;
        break;
    }
  }

  public void StartShrinkCollider()
  {
    if (this.stopShrinkColliderCoroutine != null)
      this.StopCoroutine(this.stopShrinkColliderCoroutine);
    this.capsuleCollider.radius = this.colliderShrinkMinRadius;
    this.colliderIsShrinking = true;
  }

  public void StopShrinkCollider()
  {
    if ((double) this.capsuleCollider.radius < (double) this.colliderRadius)
    {
      if (this.stopShrinkColliderCoroutine != null)
        this.StopCoroutine(this.stopShrinkColliderCoroutine);
      this.stopShrinkColliderCoroutine = this.StartCoroutine(this.StopShrinkColliderCoroutine());
    }
    this.colliderIsShrinking = false;
  }

  protected IEnumerator StopShrinkColliderCoroutine()
  {
    float time = 0.0f;
    while ((double) time < (double) this.colliderGrowDuration)
    {
      this.capsuleCollider.radius = Mathf.Lerp(this.colliderShrinkMinRadius, this.colliderRadius, Mathf.InverseLerp(0.0f, this.colliderGrowDuration, time));
      time += Time.deltaTime;
      yield return (object) null;
    }
  }

  public bool SphereCastGround(
    float castLenght,
    out RaycastHit raycastHit,
    out float groundDistance)
  {
    groundDistance = 0.0f;
    UnityEngine.Vector3 vector3 = this.transform.TransformPoint(this.capsuleCollider.center);
    Ray ray = new Ray(new UnityEngine.Vector3(vector3.x, this.transform.position.y + this.capsuleCollider.height / 2f, vector3.z), -this.transform.up);
    bool flag = false;
    if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration))
    {
      foreach (Creature creature in Creature.allActive)
      {
        if (creature.isKilled && creature.ragdoll.state == Ragdoll.State.Inert)
          creature.ragdoll.SetPartsLayer(LayerName.Ragdoll);
      }
    }
    if (Physics.SphereCast(ray, this.capsuleCollider.radius * 0.99f, out raycastHit, castLenght, (int) this.groundMask, QueryTriggerInteraction.Ignore))
    {
      groundDistance = raycastHit.distance - this.capsuleCollider.height / 2f;
      flag = true;
    }
    if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration))
    {
      foreach (Creature creature in Creature.allActive)
      {
        if (creature.isKilled && creature.ragdoll.state == Ragdoll.State.Inert)
          creature.ragdoll.RefreshPartsLayer();
      }
    }
    return flag;
  }

  public void SetAllSpeedModifiers(object handler, float multiplier)
  {
    this.SetSpeedModifier(handler, multiplier, multiplier, multiplier, multiplier, crouchSpeedModifier: multiplier);
  }

  public void SetSpeedModifier(
    object handler,
    float forwardSpeedMultiplier = 1f,
    float backwardSpeedMultiplier = 1f,
    float strafeSpeedMultiplier = 1f,
    float runSpeedMultiplier = 1f,
    float jumpForceMultiplier = 1f,
    float crouchSpeedModifier = 1f)
  {
    Locomotion.SpeedModifier speedModifier1 = (Locomotion.SpeedModifier) null;
    int count = this.speedModifiers.Count;
    for (int index = 0; index < count; ++index)
    {
      Locomotion.SpeedModifier speedModifier2 = this.speedModifiers[index];
      if (speedModifier2.handler == handler)
      {
        speedModifier1 = speedModifier2;
        break;
      }
    }
    if (speedModifier1 != null)
    {
      speedModifier1.forwardSpeedMultiplier = forwardSpeedMultiplier;
      speedModifier1.backwardSpeedMultiplier = backwardSpeedMultiplier;
      speedModifier1.strafeSpeedMultiplier = strafeSpeedMultiplier;
      speedModifier1.runSpeedMultiplier = runSpeedMultiplier;
      speedModifier1.jumpForceMultiplier = jumpForceMultiplier;
      speedModifier1.crouchSpeedModifier = crouchSpeedModifier;
    }
    else
      this.speedModifiers.Add(new Locomotion.SpeedModifier(handler, forwardSpeedMultiplier, backwardSpeedMultiplier, strafeSpeedMultiplier, runSpeedMultiplier, jumpForceMultiplier, crouchSpeedModifier));
    this.RefreshSpeedModifiers();
  }

  public void RemoveSpeedModifier(object handler)
  {
    for (int index = 0; index < this.speedModifiers.Count; ++index)
    {
      if (this.speedModifiers[index].handler == handler)
      {
        this.speedModifiers.RemoveAtIgnoreOrder<Locomotion.SpeedModifier>(index);
        --index;
      }
    }
    this.RefreshSpeedModifiers();
  }

  public void ClearSpeedModifiers()
  {
    this.speedModifiers.Clear();
    this.RefreshSpeedModifiers();
  }

  public void RefreshSpeedModifiers()
  {
    if (this.speedModifiers.Count == 0)
    {
      this.forwardSpeedMultiplier = 1f;
      this.backwardSpeedMultiplier = 1f;
      this.strafeSpeedMultiplier = 1f;
      this.runSpeedMultiplier = 1f;
      this.jumpForceMultiplier = 1f;
    }
    else
    {
      float num1 = 1f;
      float num2 = 1f;
      float num3 = 1f;
      float num4 = 1f;
      float num5 = 1f;
      foreach (Locomotion.SpeedModifier speedModifier in this.speedModifiers)
      {
        num1 *= speedModifier.forwardSpeedMultiplier;
        num2 *= speedModifier.backwardSpeedMultiplier;
        num3 *= speedModifier.strafeSpeedMultiplier;
        num4 *= speedModifier.runSpeedMultiplier;
        num5 *= speedModifier.jumpForceMultiplier;
      }
      this.forwardSpeedMultiplier = num1;
      this.backwardSpeedMultiplier = num2;
      this.strafeSpeedMultiplier = num3;
      this.runSpeedMultiplier = num4;
      this.jumpForceMultiplier = num5;
    }
  }

  public void SetPhysicModifier(
    object handler,
    float? gravityMultiplier = null,
    float massMultiplier = -1f,
    float dragMultiplier = -1f,
    int duplicateId = -1)
  {
    if (!gravityMultiplier.HasValue && (double) massMultiplier == -1.0 && (double) dragMultiplier == -1.0)
      return;
    Locomotion.PhysicModifier physicModifier1 = (Locomotion.PhysicModifier) null;
    int count = this.physicModifiers.Count;
    for (int index = 0; index < count; ++index)
    {
      Locomotion.PhysicModifier physicModifier2 = this.physicModifiers[index];
      if (physicModifier2.handler == handler)
      {
        physicModifier1 = physicModifier2;
        break;
      }
    }
    if (physicModifier1 != null)
    {
      physicModifier1.gravityMultiplier = gravityMultiplier;
      physicModifier1.massMultiplier = massMultiplier;
      physicModifier1.dragMultiplier = dragMultiplier;
    }
    else
      this.physicModifiers.Add(new Locomotion.PhysicModifier(handler, gravityMultiplier, massMultiplier, dragMultiplier, duplicateId));
    this.RefreshPhysicModifiers();
  }

  public void RemovePhysicModifier(object handler)
  {
    for (int index = 0; index < this.physicModifiers.Count; ++index)
    {
      if (this.physicModifiers[index].handler == handler)
      {
        this.physicModifiers.RemoveAtIgnoreOrder<Locomotion.PhysicModifier>(index);
        --index;
      }
    }
    this.RefreshPhysicModifiers();
  }

  public void ClearPhysicModifiers()
  {
    for (int index = 0; index < this.physicModifiers.Count; ++index)
    {
      if (this.physicModifiers[index].effectInstance != null)
        this.physicModifiers[index].effectInstance.End();
    }
    this.physicModifiers.Clear();
    this.RefreshPhysicModifiers();
  }

  public void RefreshPhysicModifiers()
  {
    if (!(bool) this.physicBody)
      return;
    if (this.physicModifiers.Count == 0)
    {
      this.physicBody.mass = !(bool) (UnityEngine.Object) this.player || !(bool) (UnityEngine.Object) this.player.creature ? (!(bool) (UnityEngine.Object) this.creature ? this.orgMass : this.creature.data.locomotionMass) : this.player.creature.data.locomotionMass;
      this.physicBody.useGravity = true;
      this.customGravity = 0.0f;
      this.groundDragMultiplier = 1f;
      this.flyDragMultiplier = 1f;
    }
    else
    {
      float num1 = 1f;
      float num2 = 1f;
      float num3 = 1f;
      HashSet<int> intSet = new HashSet<int>();
      foreach (Locomotion.PhysicModifier physicModifier in this.physicModifiers)
      {
        if (physicModifier.duplicateId == -1 || intSet.Add(physicModifier.duplicateId))
        {
          if (physicModifier.gravityMultiplier.HasValue)
            num1 *= physicModifier.gravityMultiplier.Value;
          if ((double) physicModifier.massMultiplier > 0.0)
            num2 *= physicModifier.massMultiplier;
          if ((double) physicModifier.dragMultiplier >= 0.0)
            num3 *= physicModifier.dragMultiplier;
        }
      }
      if ((double) num1 == 1.0)
      {
        this.customGravity = 0.0f;
        this.physicBody.useGravity = true;
      }
      else if ((double) num1 <= 0.0)
      {
        this.customGravity = 0.0f;
        this.physicBody.useGravity = false;
      }
      else
      {
        this.customGravity = num1;
        this.physicBody.useGravity = false;
      }
      this.physicBody.mass = !(bool) (UnityEngine.Object) this.player || !(bool) (UnityEngine.Object) this.player.creature ? (!(bool) (UnityEngine.Object) this.creature ? num2 * this.orgMass : num2 * this.creature.data.locomotionMass) : num2 * this.player.creature.data.locomotionMass;
      this.groundDragMultiplier = num3;
      this.flyDragMultiplier = num3;
    }
    PhysicBody physicBody = this.physicBody;
    double num4;
    if (this.isGrounded)
    {
      bool? isSwimming = this.player?.creature?.isSwimming;
      int num5;
      if (!isSwimming.HasValue)
      {
        Creature creature = this.creature;
        num5 = creature != null ? (creature.isSwimming ? 1 : 0) : 0;
      }
      else
        num5 = isSwimming.GetValueOrDefault() ? 1 : 0;
      if (num5 == 0)
      {
        num4 = (double) this.groundDrag * (double) this.groundDragMultiplier;
        goto label_28;
      }
    }
    num4 = (double) this.flyDrag * (double) this.flyDragMultiplier;
label_28:
    physicBody.drag = (float) num4;
  }

  public enum GroundDetection
  {
    Raycast,
    Collision,
  }

  public enum TurnMode
  {
    Instant,
    Snap,
    Smooth,
    Disabled,
  }

  public enum CrouchMode
  {
    Disabled,
    Hold,
    Toggle,
  }

  public delegate void GroundEvent(
    Locomotion locomotion,
    UnityEngine.Vector3 groundPoint,
    UnityEngine.Vector3 velocity,
    Collider groundCollider);

  public delegate void CrouchEvent(bool isCrouching);

  public delegate void CollisionEvent(UnityEngine.Collision collision);

  public delegate void FlyEvent(Locomotion locomotion);

  [Serializable]
  public class SpeedModifier
  {
    [NonSerialized]
    public object handler;
    public float forwardSpeedMultiplier;
    public float backwardSpeedMultiplier;
    public float strafeSpeedMultiplier;
    public float runSpeedMultiplier;
    public float jumpForceMultiplier;
    public float crouchSpeedModifier;

    public SpeedModifier(
      object handler,
      float forwardSpeedMultiplier,
      float backwardSpeedMultiplier,
      float strafeSpeedMultiplier,
      float runSpeedMultiplier,
      float jumpForceMultiplier,
      float crouchSpeedModifier)
    {
      this.handler = handler;
      this.forwardSpeedMultiplier = forwardSpeedMultiplier;
      this.backwardSpeedMultiplier = backwardSpeedMultiplier;
      this.strafeSpeedMultiplier = strafeSpeedMultiplier;
      this.runSpeedMultiplier = runSpeedMultiplier;
      this.jumpForceMultiplier = jumpForceMultiplier;
      this.crouchSpeedModifier = crouchSpeedModifier;
    }
  }

  [Serializable]
  public class PhysicModifier
  {
    [NonSerialized]
    public object handler;
    public float? gravityMultiplier;
    public float massMultiplier;
    public float dragMultiplier;
    public int duplicateId;
    [NonSerialized]
    public EffectInstance effectInstance;

    public PhysicModifier(
      object handler,
      float? gravityMultiplier,
      float massMultiplier,
      float dragMultiplier,
      int duplicateId = -1)
    {
      this.handler = handler;
      this.gravityMultiplier = gravityMultiplier;
      this.massMultiplier = massMultiplier;
      this.dragMultiplier = dragMultiplier;
      this.duplicateId = duplicateId;
    }
  }
}
