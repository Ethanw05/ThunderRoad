// Decompiled with JetBrains decompiler
// Type: ThunderRoad.DragArea
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace ThunderRoad;

public class DragArea : ThunderBehaviour
{
  [Header("Velocity")]
  [Tooltip("Type of velocity to use. Estimated mode does not need any rigidbody")]
  public DragArea.VelocityType velocityType;
  [Tooltip("Rigidbody to get the velocity from. Only used when velocityType = 'FromRigidbody'.")]
  public Rigidbody rbToGetVelocityFrom;
  [Tooltip("Number of frames used to estimate the velocity. Only used when velocityType = 'Estimated'.")]
  public int velocityAverageFrames = 5;
  [Header("Coefficients")]
  [Tooltip("Multiplies the drag by this value")]
  public float dragCoef = 1f;
  [Tooltip("Percentage of drag to convert into lift")]
  public float dragToLiftRatio = 0.5f;
  [Tooltip("Eases the drag across the normal. When drag is in the same direction than the normal, we sample at 1. when it's perpendicular, we sample at 0.")]
  public AnimationCurve dragAngleEasing = AnimationCurve.Linear(0.0f, 1f, 1f, 1f);
  [Header("Bodies")]
  [Tooltip("Rigidbodies that will be dragged by the area.")]
  public List<PhysicBody> bodiesToDrag;
  [Tooltip("Rigidbodies that will be lifted by the area.")]
  public List<PhysicBody> bodiesToLift;
  [Tooltip("Will drag the creature grabbing this item (locomotion)")]
  public bool dragGrabbingCreatureBody;
  [Tooltip("Will lift the creature grabbing this item (locomotion)")]
  public bool liftGrabbingCreatureBody;
  [Tooltip("Will use the creature locomotion position as the velocity origin")]
  public bool useCreatureLocomotionAsVelocityOrigin;
  [Header("Surface")]
  [Tooltip("Area surface as a plane")]
  public UnityEngine.Vector2 surfaceDimension;
  [Tooltip("Point to add the drag forces at")]
  public Transform center;
  [Tooltip("Origin used to estimate the velocity. Only used when velocityType = 'Estimated'")]
  public Transform velocityOrigin;
  [Tooltip("Is the area two sided? If not, drag will only apply in one way")]
  public bool twoSided = true;
  [Tooltip("Is the area on an Item? If yes, then item callbacks will be used (grab, un-grab, etc.)")]
  [Header("Item")]
  public bool listenToItemCallbacks = true;
  [Header("Misc")]
  [Tooltip("Prevent drag & lift to be computed when the creature turns (causing fast and abrupt changes of position)")]
  public bool preventUpdatesWhenCreatureTurns;
  [Tooltip("Check if the area enters and exit from the water")]
  public bool checkForWater = true;
  [NonSerialized]
  public DragArea.Fluid currentFluid;
  public UnityEvent onStart;
  public UnityEvent onStop;
  [Header("This event is fired (every physic frames) when the area moves in any direction")]
  public UnityEvent<float> onAreaMoves;
  [Header("This event is fired (only once) when we first detect a motion that causes drag")]
  public UnityEvent<float> onAreaStartDragging;
  [Header("This event is fired (every physic frames) when we detect a motion that causes drag")]
  public UnityEvent<float> onAreaDrags;
  [Header("This event is fired (every physic frames) when we detect a motion that is not causing drag")]
  public UnityEvent<float> onAreaPulls;
  [Header("This event is fired (only once) when we stop detecting a motion that causes drag")]
  public UnityEvent<float> onAreaStopDragging;
  [Header("Item related events")]
  public UnityEvent onItemSnaps;
  public UnityEvent<Handle, RagdollHand> onItemIsGrabbed;
  public UnityEvent<Handle, RagdollHand> onItemIsUnGrabbed;
  private bool dragging;
  private Coroutine routine;
  private int sampleCount;
  private UnityEngine.Vector3[] velocitySamples;
  private Item item;
  private Creature creature;
  private WaterHandler waterHandler;
  private Transform defaultVelocityOrigin;

  public UnityEngine.Vector3 Normal => this.transform.forward;

  public float Area => this.surfaceDimension.x * this.surfaceDimension.y;

  public UnityEngine.Vector3 Velocity => this.GetVelocity();

  public UnityEngine.Vector3 Drag => this.GetDrag(this.Velocity, this.currentFluid);

  public UnityEngine.Vector3 Lift => this.GetLift();

  private void Awake()
  {
    this.SetFluidAir();
    if (this.listenToItemCallbacks)
      this.CheckIfIsItemAndCacheValues();
    this.waterHandler = new WaterHandler(false, true);
    this.waterHandler.OnWaterEnter += new WaterHandler.SimpleDelegate(this.OnWaterEnter);
    this.waterHandler.OnWaterExit += new WaterHandler.SimpleDelegate(this.OnWaterExit);
  }

  private void Start() => this.onStart?.Invoke();

  /// <summary>
  /// Caches the parent item, and hooks itself to its callbacks
  /// </summary>
  private void CheckIfIsItemAndCacheValues()
  {
    this.item = this.GetComponentInParent<Item>();
    if (!(bool) (UnityEngine.Object) this.item)
      return;
    this.item.OnGrabEvent += new Item.GrabDelegate(this.OnObjectGrabbed);
    this.item.OnUngrabEvent += new Item.ReleaseDelegate(this.OnObjectReleased);
    this.item.OnSnapEvent += new Item.HolderDelegate(this.OnSnap);
  }

  /// <summary>Updates the water handler</summary>
  private void UpdateWater()
  {
    UnityEngine.Vector3 position = this.center.position;
    this.waterHandler.Update(position, position.y - this.surfaceDimension.y / 2f, position.y + this.surfaceDimension.y / 2f, Mathf.Max(this.surfaceDimension.x, this.surfaceDimension.y) / 2f, this.Velocity);
  }

  /// <summary>
  /// When the water handler detects that the area is immersed, set the fluid to water.
  /// </summary>
  private void OnWaterEnter() => this.SetFluidWater();

  /// <summary>
  /// When the water handler detects that the area is not immersed anymore, set the fluid back to default.
  /// </summary>
  private void OnWaterExit() => this.SetFluidAir();

  /// <summary>Called when the parent item snaps to an holder.</summary>
  /// <param name="holder"></param>
  private void OnSnap(Holder holder) => this.onItemSnaps?.Invoke();

  /// <summary>
  /// Called when the parent item is grabbed.
  /// Caches the grabbing creature to the different values (if used).
  /// </summary>
  /// <param name="handle"></param>
  /// <param name="ragdollhand"></param>
  private void OnObjectGrabbed(Handle handle, RagdollHand ragdollhand)
  {
    this.onItemIsGrabbed?.Invoke(handle, ragdollhand);
    this.creature = ragdollhand.creature;
    PhysicBody physicBody = (bool) (UnityEngine.Object) this.creature.player ? this.creature.player.locomotion.physicBody : this.creature.locomotion.physicBody;
    if (this.dragGrabbingCreatureBody && !this.bodiesToDrag.Contains(physicBody))
      this.bodiesToDrag.Add(physicBody);
    if (this.liftGrabbingCreatureBody && !this.bodiesToLift.Contains(physicBody))
      this.bodiesToLift.Add(physicBody);
    this.defaultVelocityOrigin = this.velocityOrigin;
    if (!this.useCreatureLocomotionAsVelocityOrigin)
      return;
    this.velocityOrigin = physicBody.transform;
  }

  /// <summary>
  /// Called when the parent item is released.
  /// un-caches the grabbing creature to the different values (if used).
  /// </summary>
  /// <param name="handle"></param>
  /// <param name="ragdollhand"></param>
  private void OnObjectReleased(Handle handle, RagdollHand ragdollhand, bool throwing)
  {
    this.onItemIsUnGrabbed?.Invoke(handle, ragdollhand);
    this.creature = ragdollhand.creature;
    PhysicBody physicBody = (bool) (UnityEngine.Object) this.creature.player ? this.creature.player.locomotion.physicBody : this.creature.locomotion.physicBody;
    if (this.dragGrabbingCreatureBody)
      this.bodiesToDrag.Remove(physicBody);
    if (this.liftGrabbingCreatureBody)
      this.bodiesToLift.Remove(physicBody);
    if (!this.useCreatureLocomotionAsVelocityOrigin)
      return;
    this.velocityOrigin = this.defaultVelocityOrigin;
  }

  protected override void ManagedOnEnable()
  {
    if (this.velocityType == DragArea.VelocityType.Estimated)
      this.BeginEstimatingVelocity();
    this.onStart?.Invoke();
  }

  protected override void ManagedOnDisable()
  {
    if (this.velocityType == DragArea.VelocityType.Estimated)
      this.FinishEstimatingVelocity();
    this.onStop?.Invoke();
    this.waterHandler.Reset();
  }

  private void OnDestroy()
  {
    this.onStop?.Invoke();
    if ((bool) (UnityEngine.Object) this.item)
    {
      this.item.OnGrabEvent -= new Item.GrabDelegate(this.OnObjectGrabbed);
      this.item.OnUngrabEvent -= new Item.ReleaseDelegate(this.OnObjectReleased);
      this.item.OnSnapEvent -= new Item.HolderDelegate(this.OnSnap);
    }
    this.waterHandler.OnWaterEnter -= new WaterHandler.SimpleDelegate(this.OnWaterEnter);
    this.waterHandler.OnWaterExit -= new WaterHandler.SimpleDelegate(this.OnWaterExit);
  }

  public override ManagedLoops EnabledManagedLoops
  {
    get => ManagedLoops.FixedUpdate | ManagedLoops.Update;
  }

  protected internal override void ManagedUpdate()
  {
    if (!this.checkForWater)
      return;
    this.UpdateWater();
  }

  protected internal override void ManagedFixedUpdate()
  {
    UnityEngine.Vector3 drag = this.Drag;
    if (this.bodiesToDrag != null)
    {
      for (int index = 0; index < this.bodiesToDrag.Count; ++index)
        this.bodiesToDrag[index].AddForceAtPosition(drag, this.center.position, ForceMode.Force);
    }
    UnityEngine.Vector3 lift = this.Lift;
    if (this.bodiesToLift != null)
    {
      for (int index = 0; index < this.bodiesToLift.Count; ++index)
        this.bodiesToLift[index].AddForceAtPosition(lift, this.center.position, ForceMode.Force);
    }
    if (this.preventUpdatesWhenCreatureTurns && this.CheckIfCreatureIsTurning() || this.onAreaMoves == null && this.onAreaStartDragging == null && this.onAreaDrags == null && this.onAreaPulls == null && this.onAreaStopDragging == null)
      return;
    float magnitude = this.Velocity.magnitude;
    this.onAreaMoves?.Invoke(magnitude);
    if ((double) Mathf.Abs(drag.magnitude) > 0.10000000149011612)
    {
      if (!this.dragging)
      {
        this.onAreaStartDragging?.Invoke(magnitude);
        this.dragging = true;
      }
      else
        this.onAreaDrags?.Invoke(magnitude);
    }
    else if (this.dragging)
    {
      this.dragging = false;
      this.onAreaStopDragging?.Invoke(magnitude);
    }
    else
      this.onAreaPulls?.Invoke(magnitude);
  }

  /// <summary>
  /// Checks if the player is currently turning with the thumbstick
  /// </summary>
  /// <returns>True if the player is turning, false otherwise</returns>
  private bool CheckIfCreatureIsTurning()
  {
    return (bool) (UnityEngine.Object) this.creature && (bool) (UnityEngine.Object) this.creature.locomotion && ((double) this.creature.locomotion.turnSmoothDirection != 0.0 || (double) this.creature.locomotion.turnSnapDirection != 0.0 || (double) this.creature.locomotion.turnSmoothSnapDirection != 0.0);
  }

  /// <summary>Get the velocity of the area using the velocityType.</summary>
  /// <returns>The velocity of the area, using the velocityType</returns>
  public UnityEngine.Vector3 GetVelocity()
  {
    switch (this.velocityType)
    {
      case DragArea.VelocityType.Estimated:
        return this.GetVelocityEstimate();
      case DragArea.VelocityType.FromRigidbody:
        return !(bool) (UnityEngine.Object) this.rbToGetVelocityFrom ? UnityEngine.Vector3.zero : this.rbToGetVelocityFrom.GetPointVelocity(this.center.position);
      default:
        return UnityEngine.Vector3.zero;
    }
  }

  /// <summary>
  /// Estimate and convert the drag as a convenient float that depends on the angle between the area velocity and the area normal.
  /// Eased by the dragAngleEasing curve.
  /// </summary>
  /// <param name="velocity">Velocity of the area in the current fluid.</param>
  /// <returns>The current drag "amount".</returns>
  public float GetDragForce(UnityEngine.Vector3 velocity)
  {
    float num = this.dragAngleEasing.Evaluate(Mathf.Abs(UnityEngine.Vector3.Dot(velocity.normalized, -this.Normal)));
    return UnityEngine.Vector3.Dot(velocity, -this.Normal) * num;
  }

  /// <summary>
  /// Used to quantify the drag or resistance of an object in a fluid environment, such as air or water.
  /// This is a big simplification of real life drag, for gameplay purposes.
  /// </summary>
  /// <param name="velocity">Velocity of the area.</param>
  /// <param name="fluid">Current fluid the area is in</param>
  /// <returns>The estimated drag force in the direction of the area normal.</returns>
  public UnityEngine.Vector3 GetDrag(UnityEngine.Vector3 velocity, DragArea.Fluid fluid)
  {
    UnityEngine.Vector3 velocity1 = Mathf.Pow(this.Area, 2f) * fluid.MassDensity * fluid.FlowVelocity(velocity);
    return !this.twoSided && (double) UnityEngine.Vector3.Dot(this.Normal, velocity) < 0.0 ? UnityEngine.Vector3.zero : this.Normal * this.GetDragForce(velocity1) * this.dragCoef;
  }

  /// <summary>
  /// Estimate the lift, hugely simplified to be the drag force multiplied by a ratio.
  /// This allows to move creatures and objects in the drag direction (ie. the inverted area velocity)
  /// </summary>
  /// <returns>The lift force.</returns>
  public UnityEngine.Vector3 GetLift() => this.Drag * this.dragToLiftRatio;

  /// <summary>Set the current fluid to air (defined in gamedata)</summary>
  public void SetFluidAir() => this.SetFluid(Catalog.gameData.airFluidData);

  /// <summary>Set the current fluid to water (defined in gamedata)</summary>
  public void SetFluidWater() => this.SetFluid(Catalog.gameData.waterFluidData);

  /// <summary>Set the current fluid to the one given.</summary>
  /// <param name="fluid">Fluid to set.</param>
  public void SetFluid(DragArea.Fluid fluid) => this.currentFluid = fluid;

  /// <summary>
  /// Reset velocity samples and start estimating the velocity of the hand.
  /// </summary>
  public void BeginEstimatingVelocity()
  {
    this.FinishEstimatingVelocity();
    this.velocitySamples = new UnityEngine.Vector3[this.velocityAverageFrames];
    this.routine = this.StartCoroutine(this.EstimateVelocityCoroutine());
  }

  /// <summary>Stops the velocity estimation routine.</summary>
  public void FinishEstimatingVelocity()
  {
    if (this.routine == null)
      return;
    this.StopCoroutine(this.routine);
    this.routine = (Coroutine) null;
  }

  /// <summary>
  /// Compute the acceleration estimation from the taken samples
  /// </summary>
  /// <returns>Acceleration estimation</returns>
  public UnityEngine.Vector3 GetAccelerationEstimate()
  {
    if (this.velocitySamples == null)
      return UnityEngine.Vector3.zero;
    UnityEngine.Vector3 zero = UnityEngine.Vector3.zero;
    for (int index = 2 + this.sampleCount - this.velocitySamples.Length; index < this.sampleCount; ++index)
    {
      if (index >= 2)
      {
        int num1 = index - 2;
        int num2 = index - 1;
        UnityEngine.Vector3 velocitySample1 = this.velocitySamples[num1 % this.velocitySamples.Length];
        UnityEngine.Vector3 velocitySample2 = this.velocitySamples[num2 % this.velocitySamples.Length];
        zero += velocitySample2 - velocitySample1;
      }
    }
    return zero * (1f / Time.deltaTime);
  }

  /// <summary>
  /// Compute the velocity estimation from the taken samples
  /// </summary>
  /// <returns>Velocity estimation</returns>
  public UnityEngine.Vector3 GetVelocityEstimate()
  {
    if (this.velocitySamples == null)
      return UnityEngine.Vector3.zero;
    UnityEngine.Vector3 zero = UnityEngine.Vector3.zero;
    int num = Mathf.Min(this.sampleCount, this.velocitySamples.Length);
    if (num == 0)
      return zero;
    for (int index = 0; index < num; ++index)
      zero += this.velocitySamples[index];
    return zero * (1f / (float) num);
  }

  /// <summary>Routine that samples and estimate linear velocity.</summary>
  /// <returns></returns>
  private IEnumerator EstimateVelocityCoroutine()
  {
    this.sampleCount = 0;
    UnityEngine.Vector3 position1 = this.center.position;
    UnityEngine.Vector3 previousPosition = (bool) (UnityEngine.Object) this.velocityOrigin ? position1 - this.velocityOrigin.position : position1;
    while (true)
    {
      yield return (object) new WaitForEndOfFrame();
      float num = 1f / Time.deltaTime;
      int index = this.sampleCount % this.velocitySamples.Length;
      ++this.sampleCount;
      UnityEngine.Vector3 position2 = this.center.position;
      UnityEngine.Vector3 vector3 = (bool) (UnityEngine.Object) this.velocityOrigin ? this.velocityOrigin.position : UnityEngine.Vector3.zero;
      this.velocitySamples[index] = num * (position2 - vector3 - previousPosition);
      previousPosition = position2 - vector3;
    }
  }

  private void OnDrawGizmosSelected()
  {
    Matrix4x4 matrix = Gizmos.matrix;
    Transform transform = this.transform;
    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new UnityEngine.Vector3(this.surfaceDimension.x, this.surfaceDimension.y, 0.0f));
    Gizmos.color = new UnityEngine.Color(1f, 0.3f, 0.1f, 0.1f);
    Gizmos.DrawCube(UnityEngine.Vector3.zero + UnityEngine.Vector3.forward / 100f, UnityEngine.Vector3.one);
    Gizmos.color = new UnityEngine.Color(1f, 0.3f, 0.1f);
    Gizmos.DrawWireCube(UnityEngine.Vector3.zero + UnityEngine.Vector3.forward / 100f, UnityEngine.Vector3.one);
    Gizmos.matrix = matrix;
    Gizmos.color = new UnityEngine.Color(1f, 0.3f, 0.1f);
    DragArea.ArrowGizmo(this.center.position, this.Normal, 0.075f, 0.05f);
    Gizmos.color = new UnityEngine.Color(0.1f, 0.3f, 1f);
    if (this.twoSided)
      DragArea.ArrowGizmo(this.center.position, -this.Normal, 0.075f, 0.05f);
    Gizmos.color = new UnityEngine.Color(1f, 0.3f, 0.1f);
    DragArea.ArrowGizmo(transform.position, this.Drag, this.Drag.magnitude, 0.1f);
    Gizmos.color = new UnityEngine.Color(0.1f, 0.3f, 1f);
    DragArea.ArrowGizmo(transform.position, this.Lift, this.Lift.magnitude, 0.1f);
    Gizmos.color = new UnityEngine.Color(0.35f, 0.5f, 1f);
    UnityEngine.Vector3 position = this.center.position;
    Gizmos.DrawLine(position + this.center.up * this.surfaceDimension.y / 2f, position - this.center.up * this.surfaceDimension.y / 2f);
    float num = 10f;
    for (int index = 0; (double) index < (double) num; ++index)
    {
      UnityEngine.Vector3 vector3 = transform.position - this.transform.right * this.surfaceDimension.x / 2f;
      float a1 = this.dragAngleEasing.Evaluate((float) index / num);
      float a2 = this.dragAngleEasing.Evaluate((float) (1.0 - (double) index / (double) num));
      Gizmos.matrix = Matrix4x4.TRS(vector3 + this.transform.right * ((float) index * (this.surfaceDimension.x / 2f / num)), transform.rotation, new UnityEngine.Vector3(this.surfaceDimension.x / num, this.surfaceDimension.y, 0.0f));
      Gizmos.color = new UnityEngine.Color(1f, 0.3f, 0.1f, a1);
      Gizmos.DrawCube(UnityEngine.Vector3.zero + UnityEngine.Vector3.forward / 100f, UnityEngine.Vector3.one);
      Gizmos.matrix = Matrix4x4.TRS(vector3 + this.transform.right * (float) ((double) this.surfaceDimension.x / 2.0 + (double) index * ((double) this.surfaceDimension.x / 2.0 / (double) num)), transform.rotation, new UnityEngine.Vector3(this.surfaceDimension.x / num, this.surfaceDimension.y, 0.0f));
      Gizmos.color = new UnityEngine.Color(1f, 0.3f, 0.1f, a2);
      Gizmos.DrawCube(UnityEngine.Vector3.zero + UnityEngine.Vector3.forward / 100f, UnityEngine.Vector3.one);
    }
    Gizmos.matrix = matrix;
  }

  private static void ArrowGizmo(
    UnityEngine.Vector3 pos,
    UnityEngine.Vector3 normal,
    float magnitude,
    float arrowHeadLength = 0.25f,
    float arrowHeadAngle = 20f)
  {
    if ((double) magnitude <= 9.9999997473787516E-05 || (double) normal.sqrMagnitude <= 9.9999997473787516E-05)
      return;
    UnityEngine.Vector3 vector3_1 = normal * magnitude;
    Gizmos.DrawRay(pos, vector3_1);
    UnityEngine.Vector3 vector3_2 = Quaternion.LookRotation(vector3_1) * Quaternion.Euler(0.0f, 180f + arrowHeadAngle, 0.0f) * new UnityEngine.Vector3(0.0f, 0.0f, 1f);
    UnityEngine.Vector3 vector3_3 = Quaternion.LookRotation(vector3_1) * Quaternion.Euler(0.0f, 180f - arrowHeadAngle, 0.0f) * new UnityEngine.Vector3(0.0f, 0.0f, 1f);
    Gizmos.DrawRay(pos + vector3_1, vector3_2 * arrowHeadLength);
    Gizmos.DrawRay(pos + vector3_1, vector3_3 * arrowHeadLength);
  }

  /// <summary>
  /// Struct used to define fluids (air, water, etc.).
  /// The higher the density, the higher the drag.
  /// Flow is made to simulate world space "current".
  /// </summary>
  [Serializable]
  public struct Fluid(float density, UnityEngine.Vector3 flow)
  {
    public float density = density;
    public UnityEngine.Vector3 flow = flow;

    /// <summary>
    /// Over simplification of the real world mass densities.
    /// Simplified to return the density only.
    /// </summary>
    public float MassDensity => this.density;

    public UnityEngine.Vector3 FlowVelocity(UnityEngine.Vector3 velocity) => velocity + this.flow;
  }

  public enum VelocityType
  {
    Estimated,
    FromRigidbody,
  }
}
