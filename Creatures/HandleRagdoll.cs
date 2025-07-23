// Decompiled with JetBrains decompiler
// Type: ThunderRoad.HandleRagdoll
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Creatures/Handle ragdoll")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/HandleRagdoll")]
public class HandleRagdoll : Handle
{
  public static float combatScaleChangeTime = 0.75f;
  public bool canBeEscaped;
  public bool wasTkGrabbed;
  public int grappleEscapeParameterValue;
  public float escapeDelay;
  [NonSerialized]
  public HandleRagdollData handleRagdollData;
  public static bool holdGripRagdoll = false;
  [NonSerialized]
  public float initialTouchRadius;
  [NonSerialized]
  public RagdollPart ragdollPart;
  [NonSerialized]
  public Transform bodyAnchor;
  public bool isBackGrab;
  protected Brain.State lastBrainState;
  protected Coroutine combatScaleCoroutine;
  private bool stabilityReach;
  private UnityEngine.Vector3 grabDir = UnityEngine.Vector3.zero;

  protected override void Awake()
  {
    this.initialTouchRadius = this.touchRadius;
    this.ragdollPart = this.GetComponentInParent<RagdollPart>();
    base.Awake();
  }

  protected override void Start()
  {
    base.Start();
    this.ragdollPart.ragdoll.creature.brain.OnStateChangeEvent += new System.Action<Brain.State>(this.BrainStateChange);
    if (!this.handleRagdollData.allowTelekinesis || this.handleRagdollData.tkActiveCondition == HandleRagdollData.TkCondition.Always)
      return;
    this.ragdollPart.ragdoll.creature.OnDespawnEvent -= new Creature.DespawnEvent(this.CreatureDespawned);
    this.ragdollPart.ragdoll.creature.OnDespawnEvent += new Creature.DespawnEvent(this.CreatureDespawned);
    if (this.handleRagdollData.tkActiveCondition == HandleRagdollData.TkCondition.CreatureDead)
    {
      this.ragdollPart.ragdoll.creature.OnKillEvent -= new Creature.KillEvent(this.CreatureKilled);
      this.ragdollPart.ragdoll.creature.OnKillEvent += new Creature.KillEvent(this.CreatureKilled);
    }
    if (this.handleRagdollData.tkActiveCondition != HandleRagdollData.TkCondition.PartSliced)
      return;
    this.ragdollPart.ragdoll.OnSliceEvent -= new Ragdoll.SliceEvent(this.RagdollSliced);
    this.ragdollPart.ragdoll.OnSliceEvent += new Ragdoll.SliceEvent(this.RagdollSliced);
  }

  public override void Load(InteractableData interactableData)
  {
    if (!(interactableData is HandleRagdollData))
    {
      Debug.LogError((object) "Trying to load wrong data type");
    }
    else
    {
      base.Load((InteractableData) (interactableData as HandleRagdollData));
      this.handleRagdollData = this.data as HandleRagdollData;
      if (!this.handleRagdollData.allowTelekinesis)
        return;
      this.SetTelekinesis(this.handleRagdollData.tkActiveCondition == HandleRagdollData.TkCondition.Always);
    }
  }

  private void CreatureDespawned(EventTime eventTime) => this.SetTelekinesis(false);

  private void CreatureKilled(CollisionInstance collisionInstance, EventTime eventTime)
  {
    this.SetTelekinesis(true);
  }

  private void RagdollSliced(RagdollPart ragdollPart, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd || !this.ragdollPart.isSliced)
      return;
    this.SetTelekinesis(true);
  }

  protected override bool HoldGripToGrab()
  {
    return this.handleRagdollData.forceHoldGripToGrab || HandleRagdoll.holdGripRagdoll;
  }

  public override ManagedLoops EnabledManagedLoops
  {
    get => base.EnabledManagedLoops | ManagedLoops.Update;
  }

  protected internal override void ManagedUpdate()
  {
    if (!this.ragdollPart.initialized)
      return;
    if ((this.ragdollPart.ragdoll.isGrabbed || this.ragdollPart.ragdoll.isTkGrabbed) && (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive || this.ragdollPart.ragdoll.standingUp) && (this.handlers.Count > 0 || this.IsTkGrabbed))
    {
      if (this.handleRagdollData.CheckForceLiftCondition(this.ragdollPart.ragdoll.creature))
      {
        if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
          this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
      }
      else
      {
        if (this.handleRagdollData.bodyTurnDirection != HandleRagdollData.BodyTurnDirection.None)
        {
          foreach (RagdollHand handler in this.handlers)
          {
            UnityEngine.Vector3 vector3 = handler.bone.animation.position.ToXZ() - handler.gripInfo.transform.position.ToXZ();
            UnityEngine.Vector3 to = handler.upperArmPart.transform.position.ToXZ() - this.ragdollPart.ragdoll.creature.transform.position.ToXZ();
            float num = 0.0f;
            if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.HandDirection)
              num = UnityEngine.Vector3.SignedAngle(vector3, to, this.ragdollPart.ragdoll.creature.transform.up);
            else if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.PartDirection)
              num = UnityEngine.Vector3.SignedAngle(this.isBackGrab ? -this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ() : this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ(), to, this.ragdollPart.ragdoll.creature.transform.up);
            else if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.GrabberPosition)
              num = (double) this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(handler.creature.transform.position).z <= 0.0 ? UnityEngine.Vector3.SignedAngle(-this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ(), to, this.ragdollPart.ragdoll.creature.transform.up) : UnityEngine.Vector3.SignedAngle(this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ(), to, this.ragdollPart.ragdoll.creature.transform.up);
            else if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.ClosestCardinal)
              num = UnityEngine.Vector3.SignedAngle(this.ragdollPart.ragdoll.creature.transform.TransformDirection(Utils.ClosestDirection(this.ragdollPart.ragdoll.creature.transform.InverseTransformDirection(vector3), this.handleRagdollData.cardinal)), to, this.ragdollPart.ragdoll.creature.transform.up);
            this.ragdollPart.ragdoll.creature.transform.Rotate(this.ragdollPart.ragdoll.creature.transform.up, num * this.ragdollPart.ragdoll.creature.turnSpeed * Time.deltaTime);
          }
        }
        if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
        {
          if (this.handleRagdollData.moveStep)
          {
            if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Head)
              this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.ragdoll.headPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
            else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Root)
              this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.ragdoll.rootPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
            else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Target)
              this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.ragdoll.targetPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
            else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Self)
              this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
          }
          if (this.handleRagdollData.changeHeight)
          {
            float animationBoneHeight = this.ragdollPart.bone.GetAnimationBoneHeight();
            this.ragdollPart.ragdoll.creature.SetAnimatorHeightRatio(this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(this.bodyAnchor.position).y / animationBoneHeight);
          }
          if (this.handleRagdollData.liftBehaviour != HandleRagdollData.LiftBehaviour.None)
          {
            RagdollPart ragdollPart = this.ragdollPart.ragdoll.rootPart;
            if (this.handleRagdollData.liftPartReference == HandleRagdollData.Part.Head)
              ragdollPart = this.ragdollPart.ragdoll.headPart;
            else if (this.handleRagdollData.liftPartReference == HandleRagdollData.Part.Target)
              ragdollPart = this.ragdollPart.ragdoll.targetPart;
            else if (this.handleRagdollData.liftPartReference == HandleRagdollData.Part.Self)
              ragdollPart = this.ragdollPart;
            if ((double) this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(ragdollPart.transform.position).y > (double) ragdollPart.bone.orgCreatureLocalPosition.y + (double) this.handleRagdollData.liftOffset)
            {
              if (this.handleRagdollData.liftBehaviour == HandleRagdollData.LiftBehaviour.Fall)
                this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
              else if (this.handleRagdollData.liftBehaviour == HandleRagdollData.LiftBehaviour.Ungrab && this.handlers.Count > 0)
                this.handlers[0].UnGrab(false);
            }
          }
          if ((double) this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(this.ragdollPart.ragdoll.headPart.transform.position).y < (double) this.ragdollPart.ragdoll.rootPart.bone.orgCreatureLocalPosition.y)
            this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
        }
      }
    }
    if (!this.handleRagdollData.liftSpinningFix || !this.ragdollPart.ragdoll.isGrabbed || this.ragdollPart.ragdoll.isTkGrabbed || this.ragdollPart.ragdoll.handlers.Count != 1)
      return;
    PhysicBody physicBody = this.ragdollPart.ragdoll.rootPart.physicBody;
    if (this.ragdollPart.ragdoll.state != Ragdoll.State.Destabilized || this.ragdollPart.ragdoll.creature.fallState != Creature.FallState.Falling)
    {
      physicBody.constraints = RigidbodyConstraints.None;
    }
    else
    {
      UnityEngine.Vector3 vector3 = UnityEngine.Vector3.ProjectOnPlane(this.ragdollPart.ragdoll.handlers[0].playerHand.grip.gameObject.transform.rotation * this.grabDir, UnityEngine.Vector3.up);
      Debug.DrawLine(physicBody.transform.position, physicBody.transform.position + this.grabDir * 4f, UnityEngine.Color.black);
      Debug.DrawLine(physicBody.transform.position, physicBody.transform.position + vector3 * 4f, UnityEngine.Color.white);
      if ((double) Mathf.Max(0.0f, physicBody.velocity.magnitude) >= 2.0)
        return;
      physicBody.constraints = RigidbodyConstraints.FreezeRotationX;
      float num = UnityEngine.Vector3.Angle(UnityEngine.Vector3.ProjectOnPlane(physicBody.transform.forward, UnityEngine.Vector3.up), UnityEngine.Vector3.ProjectOnPlane(vector3, UnityEngine.Vector3.up));
      physicBody.transform.rotation = Quaternion.Slerp(physicBody.transform.rotation, Quaternion.LookRotation(vector3), Time.deltaTime * Mathf.Min(num / 180f * this.handleRagdollData.liftSpinningFacingPower, this.handleRagdollData.liftSpinningFacingMaxSpeed));
    }
  }

  private void BrainStateChange(Brain.State state)
  {
    if (this.lastBrainState == Brain.State.Combat || state == Brain.State.Combat)
    {
      if (this.combatScaleCoroutine != null)
        this.ragdollPart.ragdoll.creature.StopCoroutine(this.combatScaleCoroutine);
      this.combatScaleCoroutine = this.ragdollPart.ragdoll.creature.StartCoroutine(this.CombatScale(state == Brain.State.Combat));
    }
    this.lastBrainState = state;
  }

  protected IEnumerator CombatScale(bool combat)
  {
    HandleRagdoll handleRagdoll = this;
    float touchRadiusMultiplier = handleRagdoll.touchRadius / handleRagdoll.initialTouchRadius;
    float lerpStart = combat ? 1f : handleRagdoll.handleRagdollData.scaleDuringCombat;
    float lerpEnd = combat ? handleRagdoll.handleRagdollData.scaleDuringCombat : 1f;
    float changeEnd = Time.time + HandleRagdoll.combatScaleChangeTime * Mathf.InverseLerp(lerpStart, lerpEnd, touchRadiusMultiplier);
    while ((double) Time.time < (double) changeEnd)
    {
      touchRadiusMultiplier = Mathf.MoveTowards(touchRadiusMultiplier, lerpEnd, Mathf.Abs(lerpEnd - lerpStart) * (Time.deltaTime / HandleRagdoll.combatScaleChangeTime));
      handleRagdoll.SetTouchRadius(handleRagdoll.initialTouchRadius * touchRadiusMultiplier);
      yield return (object) Yielders.FixedUpdate;
    }
    handleRagdoll.SetTouchRadius(handleRagdoll.initialTouchRadius * lerpEnd);
    handleRagdoll.combatScaleCoroutine = (Coroutine) null;
  }

  protected void ResetStep()
  {
    if (this.ragdollPart.ragdoll.creature.state != Creature.State.Alive || !this.handleRagdollData.moveStep)
      return;
    if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Head)
      this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.ragdoll.headPart.transform.position;
    else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Root)
      this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.ragdoll.rootPart.transform.position;
    else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Target)
    {
      this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.ragdoll.targetPart.transform.position;
    }
    else
    {
      if (this.handleRagdollData.stepPartReference != HandleRagdollData.Part.Self)
        return;
      this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.transform.position;
    }
  }

  public override void OnTelekinesisGrab(SpellTelekinesis spellTelekinesis)
  {
    if (this.ragdollPart.ragdoll.state == Ragdoll.State.NoPhysic || this.ragdollPart.ragdoll.state == Ragdoll.State.Kinematic)
      this.ragdollPart.ragdoll.SetState(Ragdoll.State.Standing);
    this.telekinesisHandlers.Add(spellTelekinesis.spellCaster);
    this.ragdollPart.ragdoll.tkHandlers.Add(spellTelekinesis.spellCaster);
    if (this.handleRagdollData.useIK)
    {
      this.bodyAnchor = spellTelekinesis.grip.transform;
      if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
      {
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadAnchor(spellTelekinesis.grip.transform);
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadState(true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadWeight(this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
      else if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
      {
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Left, spellTelekinesis.grip.transform);
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandState(Side.Left, true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandWeight(Side.Left, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
      else if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
      {
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Right, spellTelekinesis.grip.transform);
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandState(Side.Right, true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandWeight(Side.Right, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
    }
    else if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
      this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, partTypes: RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand);
    else if (this.ragdollPart.type == RagdollPart.Type.RightArm)
      this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, partTypes: RagdollPart.Type.RightArm | RagdollPart.Type.RightHand);
    if (this.ragdollPart.ragdoll.tkHandlers.Count > 1 && spellTelekinesis.allowDismemberment && GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment))
      this.ragdollPart.ragdoll.EnableCharJointBreakForce(spellTelekinesis.dismembermentBreakForceMultiplier);
    this.ResetStep();
    this.RefreshJointAndCollision();
    this.ragdollPart.ragdoll.isTkGrabbed = true;
    this.ragdollPart.ragdoll.InvokeTelekinesisGrabEvent(spellTelekinesis, this);
    this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
    this.ragdollPart.ragdoll.creature.lastInteractionCreature = spellTelekinesis.spellCaster.ragdollHand.creature;
  }

  public override void OnTelekinesisRelease(
    SpellTelekinesis spellTelekinesis,
    bool tryThrow,
    out bool throwing,
    bool grabbing)
  {
    throwing = false;
    this.telekinesisHandlers.Remove(spellTelekinesis.spellCaster);
    this.ragdollPart.ragdoll.tkHandlers.Remove(spellTelekinesis.spellCaster);
    if (this.handleRagdollData.useIK)
    {
      if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadAnchor((Transform) null);
      if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Left, (Transform) null);
      if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
        this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Right, (Transform) null);
      this.bodyAnchor = (Transform) null;
    }
    this.ragdollPart.ragdoll.RefreshPartJointAndCollision();
    if (this.ragdollPart.ragdoll.charJointBreakEnabled && this.ragdollPart.ragdoll.tkHandlers.Count < 2)
      this.ragdollPart.ragdoll.DisableCharJointBreakForce();
    if (this.ragdollPart.ragdoll.tkHandlers.Count == 0)
    {
      this.ragdollPart.ragdoll.isTkGrabbed = false;
      this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
      this.ragdollPart.ragdoll.creature.lastInteractionCreature = spellTelekinesis.spellCaster.ragdollHand.creature;
      if (tryThrow)
      {
        UnityEngine.Vector3 vector3 = Player.local.transform.rotation * PlayerControl.GetHand(spellTelekinesis.spellCaster.ragdollHand.side).GetHandVelocity();
        if ((double) vector3.magnitude > (double) SpellCaster.throwMinHandVelocity)
        {
          if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
          {
            if (spellTelekinesis.forceDestabilizeOnThrow && !this.ragdollPart.ragdoll.creature.isKilled)
              this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
            this.ragdollPart.ragdoll.creature.TryPush(Creature.PushType.Grab, vector3.normalized, spellTelekinesis.grabThrowLevel, this.ragdollPart.type);
          }
          if (this.ragdollPart.isSliced)
          {
            this.ragdollPart.physicBody.AddForce(vector3.normalized * spellTelekinesis.pushRagdollForce, ForceMode.VelocityChange);
          }
          else
          {
            foreach (RagdollPart part in this.ragdollPart.ragdoll.parts)
            {
              if ((UnityEngine.Object) part == (UnityEngine.Object) this.ragdollPart)
                part.physicBody.AddForce(vector3.normalized * spellTelekinesis.pushRagdollForce, ForceMode.VelocityChange);
              else if (!part.isSliced)
                part.physicBody.AddForce(vector3.normalized * spellTelekinesis.pushRagdollOtherPartsForce, ForceMode.VelocityChange);
            }
          }
          if (spellTelekinesis.clearFloatingOnThrow)
            this.ragdollPart.ragdoll.creature.Clear("Floating");
          throwing = true;
        }
      }
      this.ragdollPart.ragdoll.InvokeTelekinesisReleaseEvent(spellTelekinesis, this, true);
    }
    else
      this.ragdollPart.ragdoll.InvokeTelekinesisReleaseEvent(spellTelekinesis, this, false);
  }

  public override void OnGrab(
    RagdollHand ragdollHand,
    float axisPosition,
    HandlePose orientation,
    bool teleportToHand = false)
  {
    this.ragdollPart.ragdoll.CancelGetUp();
    this.wasTkGrabbed = false;
    base.OnGrab(ragdollHand, axisPosition, orientation, teleportToHand);
    if (!(bool) (UnityEngine.Object) this.bodyAnchor)
      this.bodyAnchor = new GameObject("BodyAnchor" + this.name).transform;
    this.bodyAnchor.SetParent(ragdollHand.playerHand.grip.transform);
    this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.transform.position);
    this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.transform.rotation) * orientation.transform.rotation;
    if (this.handleRagdollData.useIK)
    {
      if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
      {
        if (this.ragdollPart.type == RagdollPart.Type.Neck)
        {
          this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.ragdoll.headPart.transform.position);
          this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.ragdoll.headPart.transform.rotation) * orientation.transform.rotation;
        }
        this.ragdollPart.ragdoll.ik.SetHeadAnchor(this.bodyAnchor);
        this.ragdollPart.ragdoll.ik.SetHeadState(true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.ik.SetHeadWeight(this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
      else if ((UnityEngine.Object) this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftShoulder) == (UnityEngine.Object) this.ragdollPart.bone.animation)
      {
        this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.ragdoll.leftUpperArmPart.transform.position);
        this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.ragdoll.leftUpperArmPart.transform.rotation) * orientation.transform.rotation;
        this.ragdollPart.ragdoll.ik.SetShoulderAnchor(Side.Left, this.bodyAnchor);
        this.ragdollPart.ragdoll.ik.SetShoulderState(Side.Left, true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.ik.SetShoulderWeight(Side.Left, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
      else if ((UnityEngine.Object) this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightShoulder) == (UnityEngine.Object) this.ragdollPart.bone.animation)
      {
        this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.ragdoll.rightUpperArmPart.transform.position);
        this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.ragdoll.rightUpperArmPart.transform.rotation) * orientation.transform.rotation;
        this.ragdollPart.ragdoll.ik.SetShoulderAnchor(Side.Right, this.bodyAnchor);
        this.ragdollPart.ragdoll.ik.SetShoulderState(Side.Right, true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.ik.SetShoulderWeight(Side.Right, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
      else if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
      {
        if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
        {
          Transform bodyAnchor1 = this.bodyAnchor;
          Transform transform = orientation.transform;
          RagdollHand handLeft1 = this.ragdollPart.ragdoll.creature.handLeft;
          UnityEngine.Vector3 position = handLeft1 != null ? handLeft1.transform.position : this.transform.position;
          UnityEngine.Vector3 vector3 = transform.InverseTransformPointUnscaled(position);
          bodyAnchor1.localPosition = vector3;
          Transform bodyAnchor2 = this.bodyAnchor;
          RagdollHand handLeft2 = this.ragdollPart.ragdoll.creature.handLeft;
          Quaternion quaternion = Quaternion.Inverse(handLeft2 != null ? handLeft2.transform.rotation : this.transform.rotation) * orientation.transform.rotation;
          bodyAnchor2.localRotation = quaternion;
        }
        this.ragdollPart.ragdoll.ik.SetHandAnchor(Side.Left, this.bodyAnchor);
        this.ragdollPart.ragdoll.ik.SetHandState(Side.Left, true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.ik.SetHandWeight(Side.Left, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
      else if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
      {
        if (this.ragdollPart.type == RagdollPart.Type.RightArm)
        {
          Transform bodyAnchor3 = this.bodyAnchor;
          Transform transform = orientation.transform;
          RagdollHand handRight1 = this.ragdollPart.ragdoll.creature.handRight;
          UnityEngine.Vector3 position = handRight1 != null ? handRight1.transform.position : this.transform.position;
          UnityEngine.Vector3 vector3 = transform.InverseTransformPointUnscaled(position);
          bodyAnchor3.localPosition = vector3;
          Transform bodyAnchor4 = this.bodyAnchor;
          RagdollHand handRight2 = this.ragdollPart.ragdoll.creature.handRight;
          Quaternion quaternion = Quaternion.Inverse(handRight2 != null ? handRight2.transform.rotation : this.transform.rotation) * orientation.transform.rotation;
          bodyAnchor4.localRotation = quaternion;
        }
        this.ragdollPart.ragdoll.ik.SetHandAnchor(Side.Right, this.bodyAnchor);
        this.ragdollPart.ragdoll.ik.SetHandState(Side.Right, true, this.handleRagdollData.allowRotationIK);
        this.ragdollPart.ragdoll.ik.SetHandWeight(Side.Right, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0.0f);
      }
    }
    else if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
      this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, partTypes: RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand);
    else if (this.ragdollPart.type == RagdollPart.Type.RightArm)
      this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, partTypes: RagdollPart.Type.RightArm | RagdollPart.Type.RightHand);
    if (this.handleRagdollData.liftSpinningFix)
    {
      PhysicBody physicBody = this.ragdollPart.ragdoll.rootPart.physicBody;
      if (physicBody != (PhysicBody) null)
        this.grabDir = physicBody.transform.forward;
    }
    foreach (Collider collider in ragdollHand.colliderGroup.colliders)
      this.ragdollPart.ragdoll.IgnoreCollision(collider, true);
    this.isBackGrab = (double) UnityEngine.Vector3.Angle(this.transform.forward.ToXZ(), orientation.transform.forward.ToXZ()) <= 90.0;
    this.ragdollPart.isGrabbed = true;
    this.ragdollPart.ragdoll.creature.locomotion.StartShrinkCollider();
    if (this.ragdollPart.type == RagdollPart.Type.LeftHand || this.ragdollPart.type == RagdollPart.Type.LeftArm)
    {
      if ((bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle && (bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item)
        this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item.RefreshCollision();
    }
    else if ((this.ragdollPart.type == RagdollPart.Type.RightHand || this.ragdollPart.type == RagdollPart.Type.RightArm) && (bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handRight.grabbedHandle && (bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item)
      this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item.RefreshCollision();
    this.ResetStep();
    this.RefreshJointAndCollision();
    this.ragdollPart.ragdoll.isGrabbed = true;
    this.ragdollPart.ragdoll.handlers.Add(ragdollHand);
    this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
    this.ragdollPart.ragdoll.creature.lastInteractionCreature = ragdollHand.creature;
    this.ragdollPart.ragdoll.InvokeGrabEvent(ragdollHand, this);
  }

  private void CreateStabilizationJoint(RagdollHand ragdollHand)
  {
    this.ragdollPart.ragdoll.AddStabilizationJoint(this.gameObject, new Ragdoll.StabilizationJointSettings()
    {
      angularXDrive = new JointDrive()
      {
        positionSpring = this.handleRagdollData.stabilizationPositionSpring,
        positionDamper = this.handleRagdollData.stabilizationPositionDamper,
        maximumForce = this.handleRagdollData.stabilizationPositionMaxForce
      },
      axis = new UnityEngine.Vector3(0.0f, 1f, 0.0f),
      isKinematic = true
    });
  }

  public override void OnUnGrab(RagdollHand ragdollHand, bool throwing)
  {
    base.OnUnGrab(ragdollHand, throwing);
    this.ragdollPart.ragdoll.handlers.Remove(ragdollHand);
    if (this.ragdollPart.type == RagdollPart.Type.LeftHand || this.ragdollPart.type == RagdollPart.Type.LeftArm)
    {
      if ((bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handLeft?.grabbedHandle && (bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item)
        this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item.RefreshCollision();
    }
    else if ((this.ragdollPart.type == RagdollPart.Type.RightHand || this.ragdollPart.type == RagdollPart.Type.RightArm) && (bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handRight?.grabbedHandle && (bool) (UnityEngine.Object) this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item)
      this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item.RefreshCollision();
    if (this.handleRagdollData.useIK && (bool) (UnityEngine.Object) this.bodyAnchor)
    {
      if (this.handlers.Count == 0)
      {
        if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
          this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadAnchor((Transform) null);
        else if ((UnityEngine.Object) this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftShoulder) == (UnityEngine.Object) this.ragdollPart.bone.animation)
          this.ragdollPart.ragdoll.creature.ragdoll.ik.SetShoulderAnchor(Side.Left, (Transform) null);
        else if ((UnityEngine.Object) this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightShoulder) == (UnityEngine.Object) this.ragdollPart.bone.animation)
          this.ragdollPart.ragdoll.creature.ragdoll.ik.SetShoulderAnchor(Side.Right, (Transform) null);
        else if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
          this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Left, (Transform) null);
        else if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
          this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Right, (Transform) null);
        this.bodyAnchor.SetParent(this.transform);
        this.bodyAnchor.position = this.ragdollPart.transform.position;
        this.bodyAnchor.rotation = this.ragdollPart.transform.rotation;
      }
      else
      {
        foreach (RagdollHand handler in this.handlers)
        {
          if ((UnityEngine.Object) handler.otherHand == (UnityEngine.Object) ragdollHand)
          {
            this.bodyAnchor.SetParent(handler.playerHand.grip.transform, true);
            break;
          }
        }
      }
    }
    else if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
      this.ragdollPart.ragdoll.ResetPinForce(partTypes: RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand);
    else if (this.ragdollPart.type == RagdollPart.Type.RightArm)
      this.ragdollPart.ragdoll.ResetPinForce(partTypes: RagdollPart.Type.RightArm | RagdollPart.Type.RightHand);
    this.ragdollPart.ragdoll.RefreshPartJointAndCollision();
    foreach (Collider collider in ragdollHand.colliderGroup.colliders)
      this.ragdollPart.ragdoll.IgnoreCollision(collider, false);
    if (this.handlers.Count == 0)
    {
      this.ragdollPart.isGrabbed = false;
      this.isBackGrab = false;
    }
    this.ragdollPart.ragdoll.RemoveStabilizationJoint(this.gameObject);
    this.ragdollPart.physicBody.constraints = RigidbodyConstraints.None;
    if (this.ragdollPart.ragdoll.handlers.Count == 0)
    {
      this.ragdollPart.ragdoll.isGrabbed = false;
      if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
      {
        if (!this.ragdollPart.ragdoll.standingUp)
          this.ragdollPart.ragdoll.SetState(Ragdoll.State.Standing);
        this.ragdollPart.ragdoll.creature.locomotion.StopShrinkCollider();
      }
      this.ragdollPart.ragdoll.RefreshPartsLayer();
      this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
      this.ragdollPart.ragdoll.creature.lastInteractionCreature = ragdollHand.creature;
      this.ragdollPart.ragdoll.InvokeUngrabEvent(ragdollHand, this, true);
    }
    else
      this.ragdollPart.ragdoll.InvokeUngrabEvent(ragdollHand, this, false);
  }

  public void RefreshJointAndCollision()
  {
    if (this.handlers.Count <= 0 && !this.IsTkGrabbed)
      return;
    if (this.handleRagdollData.overrideCharJointLimitsOnParts.Count > 0)
    {
      foreach (RagdollPart part in this.ragdollPart.ragdoll.parts)
      {
        if (this.handleRagdollData.overrideCharJointLimitsOnParts.Contains(part.type))
        {
          if (!(bool) (UnityEngine.Object) part.characterJoint)
            return;
          SoftJointLimit softJointLimit = part.characterJoint.swing1Limit with
          {
            limit = this.handleRagdollData.swing1Limit
          };
          part.characterJoint.swing1Limit = softJointLimit;
          softJointLimit = part.characterJoint.lowTwistLimit with
          {
            limit = this.handleRagdollData.lowTwistLimit
          };
          part.characterJoint.lowTwistLimit = softJointLimit;
          softJointLimit = part.characterJoint.highTwistLimit with
          {
            limit = this.handleRagdollData.highTwistLimit
          };
          part.characterJoint.highTwistLimit = softJointLimit;
        }
      }
    }
    if (this.handleRagdollData.activateCollisionOnParts.Count <= 0)
      return;
    foreach (RagdollPart part in this.ragdollPart.ragdoll.parts)
    {
      if (this.handleRagdollData.activateCollisionOnParts.Contains(part.type))
        part.collisionHandler.active = true;
    }
  }
}
