// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Footstep
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Footstep.html")]
public class Footstep : ThunderBehaviour
{
  /// <summary>
  /// Id of the material to retrieve the sounds from (in catalog).
  /// </summary>
  [Tooltip("Id of the material to retrieve the sounds from (in catalog).")]
  public string materialId = nameof (Footstep);
  /// <summary>
  /// If true, on each step we trigger a raycast sampling above and under the foot.
  /// We only keep the higher collider and play its effects. We discard the others.
  /// </summary>
  [Tooltip("Enables on each step a ray sampling above and under the foot. Plays only the effects of the highest collider. (Needs to be true for water planes)")]
  public bool usePerFootRaycastCheck = true;
  /// <summary>
  /// Velocity thresholds, used in an inverse lerp when walking and running.
  /// </summary>
  [Tooltip("Velocity thresholds, used in an inverse lerp when walking and running.")]
  public UnityEngine.Vector2 minMaxStandingVelocity = new UnityEngine.Vector2(0.0f, 8f);
  /// <summary>
  /// Velocity thresholds, used in an inverse lerp when falling.
  /// </summary>
  [Tooltip("Velocity thresholds, used in an inverse lerp when falling.")]
  public UnityEngine.Vector2 minMaxFallingVelocity = new UnityEngine.Vector2(0.1f, 10f);
  /// <summary>
  /// Factor used to tweak the intensity of the footsteps when falling.
  /// </summary>
  [Tooltip("Factor used to tweak the intensity of the footsteps when falling.")]
  public float fallingIntensityFactor = 1.25f;
  /// <summary>
  /// Factor used to tweak the intensity of the footsteps when crouching.
  /// </summary>
  [Tooltip("Factor used to tweak the intensity of the footsteps when crouching.")]
  public float crouchingIntensityFactor = 0.35f;
  /// <summary>
  /// Cool-down delay in second between two steps. Different timers are used for each foot.
  /// </summary>
  [Tooltip("Cool-down delay in second between two steps. Different timers are used for each foot.")]
  public float stepMinDelay = 0.2f;
  /// <summary>Cool-down delay in second between two falls.</summary>
  [Tooltip("Cool-down delay in second between two falls.")]
  public float fallMinDelay = 0.2f;
  /// <summary>
  /// Height used to detect footsteps, it's added to the locomotion ground point (in meters).
  /// </summary>
  [Tooltip("Height used to detect footsteps, it's added to the locomotion ground point (in meters).")]
  public float footstepDetectionHeightThreshold = 0.045f;
  /// <summary>
  /// Height used to detect footsteps while running, it's added to the locomotion ground point (in meters).
  /// </summary>
  [Tooltip("Height used to detect footsteps while running, it's added to the locomotion ground point (in meters).")]
  public float footstepDetectionRunningHeightThreshold = 0.08f;
  /// <summary>
  /// RaycastHit buffer, used to sample the floor material per foot.
  /// </summary>
  private readonly RaycastHit[] hits = new RaycastHit[4];
  private int waterMaterialHash = Animator.StringToHash("Water (Instance)");
  protected MaterialData materialData;
  public Creature creature;
  protected Locomotion creatureLocomotion;
  private int defaultLayer;
  private int locomotionOnlyLayer;
  private float lastStepTimeRight;
  private float lastStepTimeLeft;
  private float lastFallTime;
  private Transform footLeft;
  private Transform footRight;
  private bool hasSteppedLeft;
  private bool hasSteppedRight;
  public bool quietLanding;
  private Dictionary<object, float> playerStepIntensityMultipliers = new Dictionary<object, float>();

  public static event Footstep.FootStepEvent OnFootStepEvent;

  public float stepIntensityMultiplier { get; private set; } = 1f;

  public event Footstep.StepEvent OnStep;

  private void Awake()
  {
    this.materialData = Catalog.GetData<MaterialData>(this.materialId);
    this.defaultLayer = GameManager.GetLayer(LayerName.Default);
    this.locomotionOnlyLayer = GameManager.GetLayer(LayerName.LocomotionOnly);
    EventManager.onPossess += new EventManager.PossessEvent(this.OnPossessionEvent);
    EventManager.onUnpossess += new EventManager.PossessEvent(this.OnUnpossessionEvent);
    this.OnStep += new Footstep.StepEvent(this.PlayStepEffects);
  }

  private void Start()
  {
    this.creature = this.GetComponentInParent<Creature>();
    if (!((Object) this.creature != (Object) null))
      return;
    this.creatureLocomotion = this.creature.locomotion;
    this.footLeft = this.creature.GetFoot(Side.Left).toesAnchor;
    this.footRight = this.creature.GetFoot(Side.Right).toesAnchor;
  }

  private void OnDestroy()
  {
    this.OnStep -= new Footstep.StepEvent(this.PlayStepEffects);
    if ((bool) (Object) this.creatureLocomotion)
      this.creatureLocomotion.OnGroundEvent -= new Locomotion.GroundEvent(this.OnLocomotionGroundEvent);
    EventManager.onPossess -= new EventManager.PossessEvent(this.OnPossessionEvent);
    EventManager.onUnpossess -= new EventManager.PossessEvent(this.OnUnpossessionEvent);
  }

  public void AddStepVolumeMultiplier(object handler, float multiplier)
  {
    this.playerStepIntensityMultipliers[handler] = multiplier;
    this.UpdateMultiplier();
  }

  public void RemoveStepVolumeMultiplier(object handler)
  {
    this.playerStepIntensityMultipliers.Remove(handler);
    this.UpdateMultiplier();
  }

  private void UpdateMultiplier()
  {
    this.stepIntensityMultiplier = 1f;
    foreach (KeyValuePair<object, float> intensityMultiplier in this.playerStepIntensityMultipliers)
    {
      this.stepIntensityMultiplier *= intensityMultiplier.Value;
      if ((double) this.stepIntensityMultiplier == 0.0)
        break;
    }
  }

  /// <summary>
  /// Caches the player's creature and locomotion.
  /// Updates on ground event binding.
  /// Caches the creature's feet.
  /// </summary>
  /// <param name="creature">Player's creature</param>
  /// <param name="eventTime"></param>
  private void OnPossessionEvent(Creature creature, EventTime eventTime)
  {
    if (!this.gameObject.activeInHierarchy || eventTime != EventTime.OnEnd || !((Object) creature == (Object) this.creature))
      return;
    this.creatureLocomotion.OnGroundEvent -= new Locomotion.GroundEvent(this.OnLocomotionGroundEvent);
    this.creatureLocomotion = creature.player.locomotion;
    this.creatureLocomotion.OnGroundEvent += new Locomotion.GroundEvent(this.OnLocomotionGroundEvent);
    this.footLeft = creature.GetFoot(Side.Left).toesAnchor;
    this.footRight = creature.GetFoot(Side.Right).toesAnchor;
  }

  /// <summary>
  /// Un-cache the player's creature and locomotion. Updates on ground event binding
  /// </summary>
  /// <param name="creature">Player's creature</param>
  /// <param name="eventTime"></param>
  private void OnUnpossessionEvent(Creature creature, EventTime eventTime)
  {
    if (!this.gameObject.activeInHierarchy || eventTime != EventTime.OnEnd || !((Object) creature == (Object) this.creature))
      return;
    this.creatureLocomotion.OnGroundEvent -= new Locomotion.GroundEvent(this.OnLocomotionGroundEvent);
    this.creatureLocomotion = creature.locomotion;
    this.creatureLocomotion.OnGroundEvent += new Locomotion.GroundEvent(this.OnLocomotionGroundEvent);
  }

  /// <summary>
  /// When the locomotion is grounded (ie. falling), this method gets called.
  /// It forces the feet to play their sounds.
  /// </summary>
  /// <param name="groundPoint">Position of the fall</param>
  /// <param name="velocity">Velocity of the fall</param>
  /// <param name="groundCollider">Collider of the ground object</param>
  private void OnLocomotionGroundEvent(
    Locomotion locomotion,
    UnityEngine.Vector3 groundPoint,
    UnityEngine.Vector3 velocity,
    Collider groundCollider)
  {
    float time = Time.time;
    if ((double) time < (double) this.lastFallTime + (double) this.fallMinDelay || this.quietLanding && this.creature.currentLocomotion.isCrouched)
      return;
    this.CheckFootLeft(this.footLeft.position, true);
    this.CheckFootRight(this.footRight.position, true);
    this.lastFallTime = time;
  }

  /// <summary>
  /// Picks effects from the groundCollider's material, and play on the given ground point.
  /// </summary>
  /// <param name="groundPoint">Where to spawn the effects (world)</param>
  /// <param name="groundCollider">GroundCollider the creature is currently standing on</param>
  /// <param name="speed">Speed of the effects to play</param>
  /// <param name="intensity">Intensity of the effects to play</param>
  private void PlayOnGroundEffects(
    UnityEngine.Vector3 groundPoint,
    Collider groundCollider,
    float speed,
    float intensity)
  {
    MaterialData.Collision collision = this.materialData.GetCollision(Animator.StringToHash(groundCollider.material.name));
    EffectInstance effectInstance = EffectInstance.Spawn(collision == null ? this.materialData.defaultEffects : collision.effects, groundPoint, Quaternion.LookRotation(UnityEngine.Vector3.up), intensity * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f), speed * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f));
    effectInstance.SetNoise(true);
    effectInstance.source = (object) this.creature;
    effectInstance.Play();
  }

  /// <summary>
  /// Converts the creature locomotion velocity to a [0 ; 1] float, using its magnitude.
  /// The value is clamped between 2 thresholds values (different when falling).
  /// </summary>
  /// <returns>A velocity magnitude clamped between 2 thresholds values, different when falling.</returns>
  private float GetFootSpeedRatio(bool falling = false)
  {
    if (!(bool) (Object) this.creatureLocomotion)
      return 0.0f;
    float num = 1f;
    if (falling)
      num = this.fallingIntensityFactor;
    else if (this.creatureLocomotion.isCrouched)
      num = this.crouchingIntensityFactor;
    UnityEngine.Vector2 vector2 = falling ? this.minMaxFallingVelocity : this.minMaxStandingVelocity;
    return Mathf.InverseLerp(vector2.x, vector2.y, this.creatureLocomotion.velocity.magnitude) * num;
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  /// <summary>
  /// Keep track of the steps cool-downs and calls related stepping methods
  /// </summary>
  protected internal override void ManagedUpdate()
  {
    if ((Object) this.footLeft == (Object) null || (Object) this.footRight == (Object) null)
      return;
    double time = (double) Time.time;
    if (time >= (double) this.lastStepTimeLeft + (double) this.stepMinDelay)
      this.CheckFootLeft(this.footLeft.position);
    if (time < (double) this.lastStepTimeRight + (double) this.stepMinDelay)
      return;
    this.CheckFootRight(this.footRight.position);
  }

  /// <summary>Checks if the left foot has stepped.</summary>
  /// <param name="footLeftPosition">Position of the left foot.</param>
  /// <param name="forceStep">Force the sound to play even when on cooldown (used on fall).</param>
  private void CheckFootLeft(UnityEngine.Vector3 footLeftPosition, bool forceStep = false)
  {
    float num = this.creatureLocomotion.IsRunning ? this.footstepDetectionRunningHeightThreshold : this.footstepDetectionHeightThreshold;
    bool flag = (double) footLeftPosition.y - (double) this.creatureLocomotion.groundHit.point.y <= (double) num;
    if (!this.hasSteppedLeft)
    {
      if (!(flag | forceStep))
        return;
      this.hasSteppedLeft = true;
      this.lastStepTimeLeft = Time.time;
      Footstep.StepEvent onStep = this.OnStep;
      if (onStep == null)
        return;
      onStep(footLeftPosition, Side.Left, this.GetFootSpeedRatio(forceStep));
    }
    else if (forceStep)
    {
      this.hasSteppedLeft = true;
      this.lastStepTimeLeft = Time.time;
      Footstep.StepEvent onStep = this.OnStep;
      if (onStep == null)
        return;
      onStep(footLeftPosition, Side.Left, this.GetFootSpeedRatio(true));
    }
    else
    {
      if (flag)
        return;
      this.hasSteppedLeft = false;
    }
  }

  /// <summary>Checks if the right foot has stepped</summary>
  /// <param name="footRightPosition">position of the right foot</param>
  /// <param name="forceStep">Force the sound to play even when on cooldown (used on fall)</param>
  private void CheckFootRight(UnityEngine.Vector3 footRightPosition, bool forceStep = false)
  {
    float num = this.creatureLocomotion.IsRunning ? this.footstepDetectionRunningHeightThreshold : this.footstepDetectionHeightThreshold;
    bool flag = (double) footRightPosition.y - (double) this.creatureLocomotion.groundHit.point.y <= (double) num;
    if (!this.hasSteppedRight)
    {
      if (!(flag | forceStep))
        return;
      this.hasSteppedRight = true;
      this.lastStepTimeRight = Time.time;
      Footstep.StepEvent onStep = this.OnStep;
      if (onStep == null)
        return;
      onStep(footRightPosition, Side.Right, this.GetFootSpeedRatio(forceStep));
    }
    else if (forceStep)
    {
      this.hasSteppedRight = true;
      this.lastStepTimeRight = Time.time;
      Footstep.StepEvent onStep = this.OnStep;
      if (onStep == null)
        return;
      onStep(footRightPosition, Side.Right, this.GetFootSpeedRatio(true));
    }
    else
    {
      if (flag)
        return;
      this.hasSteppedRight = false;
    }
  }

  /// <summary>Choose and play footsteps effects.</summary>
  /// <param name="position">Where to spawn the effect (world)</param>
  /// <param name="side">Side of the foot currently stepping</param>
  /// <param name="velocity">Velocity of the creature</param>
  private void PlayStepEffects(UnityEngine.Vector3 position, Side side, float velocity)
  {
    if (!(bool) (Object) Player.local || !(bool) (Object) Player.local.creature)
      return;
    UnityEngine.Vector3 vector3 = position;
    Footstep.FootStepEvent onFootStepEvent = Footstep.OnFootStepEvent;
    if (onFootStepEvent != null)
      onFootStepEvent(this, vector3, velocity * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f), velocity * (this.creature.isPlayer ? this.stepIntensityMultiplier : 1f), this.creature.waterHandler.inWater);
    if (this.creature.waterHandler.inWater)
    {
      vector3.y = this.creature.waterHandler.waterSurfacePosition.y;
      EffectInstance effectInstance = Catalog.gameData.water.footstepEffectData.Spawn(vector3, Quaternion.LookRotation(UnityEngine.Vector3.up), (Transform) null, (CollisionInstance) null, true, (ColliderGroup) null, false, this.creature.waterHandler.submergedRatio, velocity);
      effectInstance.SetNoise(true);
      effectInstance.source = (object) this.creature;
      effectInstance.Play();
    }
    else
    {
      if (this.materialData == null || (Object) this.creatureLocomotion == (Object) null || this.usePerFootRaycastCheck && this.PerFootRaycast(position, side, velocity))
        return;
      Collider collider = this.creatureLocomotion.groundHit.collider;
      if ((Object) collider == (Object) null)
        return;
      int layer = collider.gameObject.layer;
      if (layer != this.defaultLayer && layer != this.locomotionOnlyLayer)
        return;
      this.PlayOnGroundEffects(vector3, collider, velocity, velocity);
    }
  }

  /// <summary>
  /// Casts a ray on the position, sampling above and under the foot.
  /// Plays only the effects of the highest collider.
  /// </summary>
  /// <param name="position">Position of the ray to cast</param>
  /// <param name="side">Side of the foot</param>
  /// <param name="velocity">Velocity of the player</param>
  /// <returns>True if it found and played an effect, false otherwise</returns>
  private bool PerFootRaycast(UnityEngine.Vector3 position, Side side, float velocity)
  {
    float num1 = (float) ((this.creatureLocomotion.IsRunning ? (double) this.footstepDetectionRunningHeightThreshold : (double) this.footstepDetectionHeightThreshold) * 2.5);
    int num2 = Physics.SphereCastNonAlloc(position + UnityEngine.Vector3.up * num1, 0.05f, UnityEngine.Vector3.down, this.hits, num1 + num1 / 10f, (int) ThunderRoadSettings.current.groundLayer, QueryTriggerInteraction.Collide);
    if (num2 <= 0)
      return false;
    float num3 = this.hits[0].point.y;
    int index1 = 0;
    for (int index2 = 1; index2 < num2; ++index2)
    {
      if (this.hits[index2].collider.gameObject.layer == this.defaultLayer || this.hits[index2].collider.gameObject.layer == this.locomotionOnlyLayer)
      {
        float y = this.hits[index2].point.y;
        if ((double) y > (double) num3)
        {
          num3 = y;
          index1 = index2;
        }
      }
    }
    RaycastHit hit = this.hits[index1];
    bool flag = this.waterMaterialHash == Animator.StringToHash(hit.collider.material.name);
    this.PlayOnGroundEffects(position, hit.collider, velocity, flag ? 0.1f : velocity);
    return true;
  }

  private void OnDrawGizmosSelected()
  {
    UnityEngine.Vector3 vector3_1 = this.transform.position;
    UnityEngine.Vector3 vector3_2 = UnityEngine.Vector3.up;
    if ((bool) (Object) this.creatureLocomotion)
    {
      vector3_1 = this.creatureLocomotion.groundHit.point;
      vector3_2 = this.creatureLocomotion.groundHit.normal;
    }
    UnityEngine.Vector3 position1 = this.transform.position;
    UnityEngine.Vector3 position2 = this.transform.position;
    if ((bool) (Object) this.footLeft)
      position1 = this.footLeft.position;
    if ((bool) (Object) this.footRight)
      position2 = this.footRight.position;
    Gizmos.color = new UnityEngine.Color(1f, 1f, 1f, 0.3f);
    Matrix4x4 matrix1 = Gizmos.matrix;
    Gizmos.matrix = Matrix4x4.TRS(vector3_1 + vector3_2 * this.footstepDetectionHeightThreshold, Quaternion.identity, new UnityEngine.Vector3(1f, 0.0f, 1f));
    Gizmos.DrawWireSphere(UnityEngine.Vector3.zero, 0.2f);
    Gizmos.DrawWireSphere(UnityEngine.Vector3.zero, 0.8f);
    Gizmos.matrix = matrix1;
    Matrix4x4 matrix2 = Gizmos.matrix;
    Gizmos.color = new UnityEngine.Color(1f, 0.65f, 0.7f);
    Gizmos.matrix = Matrix4x4.TRS(vector3_1 + vector3_2 * this.footstepDetectionRunningHeightThreshold, Quaternion.identity, new UnityEngine.Vector3(1f, 0.0f, 1f));
    Gizmos.DrawWireSphere(UnityEngine.Vector3.zero, 0.1f);
    Gizmos.DrawWireSphere(UnityEngine.Vector3.zero, 0.7f);
    Gizmos.matrix = matrix2;
    Gizmos.color = this.hasSteppedLeft ? UnityEngine.Color.red : UnityEngine.Color.yellow;
    Gizmos.DrawLine(position1, new UnityEngine.Vector3(position1.x, vector3_1.y, position1.z));
    Gizmos.color = this.hasSteppedRight ? UnityEngine.Color.red : UnityEngine.Color.yellow;
    Gizmos.DrawLine(position2, new UnityEngine.Vector3(position2.x, vector3_1.y, position2.z));
    Gizmos.color = UnityEngine.Color.blue;
    Gizmos.DrawWireSphere(position1, 0.015f);
    Gizmos.color = UnityEngine.Color.blue;
    Gizmos.DrawWireSphere(position2, 0.015f);
    float num = this.footstepDetectionHeightThreshold * 2f;
    Gizmos.color = new UnityEngine.Color(1f, 1f, 1f, 0.3f);
    Gizmos.DrawLine(position1 + UnityEngine.Vector3.up * num, position1 + UnityEngine.Vector3.down * (num + num / 10f));
    Gizmos.DrawLine(position2 + UnityEngine.Vector3.up * num, position2 + UnityEngine.Vector3.down * (num + num / 10f));
  }

  public delegate void FootStepEvent(
    Footstep footstep,
    UnityEngine.Vector3 position,
    float intensity,
    float speed,
    bool inWater);

  public delegate void StepEvent(UnityEngine.Vector3 position, Side side, float velocity);
}
