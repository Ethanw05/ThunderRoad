// Decompiled with JetBrains decompiler
// Type: ThunderRoad.IkController
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class IkController : ThunderBehaviour
{
  protected Creature creature;
  public bool fullbody;
  public bool turnBodyByHeadAndHands;
  [Header("Head")]
  public bool headEnabled;
  public Transform headTarget;
  public Transform eyesTarget;
  [Header("Hips")]
  public bool hipsEnabled;
  public Transform hipsTarget;
  [Header("Hand Left")]
  public bool handLeftEnabled;
  public Transform handLeftTarget;
  public Transform fingerLeftThumbTarget;
  public Transform fingerLeftIndexTarget;
  public Transform fingerLeftMiddleTarget;
  public Transform fingerLeftRingTarget;
  public Transform fingerLeftLittleTarget;
  [Header("Hand Right")]
  public bool handRightEnabled;
  public Transform handRightTarget;
  public Transform fingerRightThumbTarget;
  public Transform fingerRightIndexTarget;
  public Transform fingerRightMiddleTarget;
  public Transform fingerRightRingTarget;
  public Transform fingerRightLittleTarget;
  [Header("Foot Left")]
  public bool footLeftEnabled;
  public Transform footLeftTarget;
  public Transform kneeLeftHint;
  public bool kneeLeftEnabled;
  [Header("Foot Right")]
  public bool footRightEnabled;
  public Transform footRightTarget;
  public Transform kneeRightHint;
  public bool kneeRightEnabled;
  [Header("Shoulder Left")]
  public bool shoulderLeftEnabled;
  public Transform shoulderLeftTarget;
  [Header("Shoulder Right")]
  public bool shoulderRightEnabled;
  public Transform shoulderRightTarget;

  public event IkController.LateUpdateDelegate OnPreIKUpdateEvent;

  public event IkController.LateUpdateDelegate OnPostIKUpdateEvent;

  public bool initialized { get; protected set; }

  protected virtual void Awake() => this.creature = this.GetComponentInParent<Creature>();

  public virtual void PreIKUpdate()
  {
    if (this.OnPreIKUpdateEvent == null)
      return;
    this.OnPreIKUpdateEvent();
  }

  public virtual void PostIKUpdate()
  {
    if (this.OnPostIKUpdateEvent == null)
      return;
    this.OnPostIKUpdateEvent();
  }

  private void OnAnimatorMove() => this.creature.AnimatorMoveUpdate();

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    if (!this.initialized || !this.creature.initialized)
      return;
    if ((bool) (UnityEngine.Object) this.creature.player)
      this.creature.SetAnimatorHeightRatio(this.creature.transform.InverseTransformPoint(this.creature.player.head.anchor.position).y / this.creature.morphology.headHeight);
    if (!this.turnBodyByHeadAndHands)
      return;
    try
    {
      this.UpdateBodyRotation();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) ex);
    }
  }

  public virtual void UpdateBodyRotation()
  {
    UnityEngine.Vector3 xz = this.creature.centerEyes.forward.ToXZ();
    bool flag = (bool) (UnityEngine.Object) this.creature.currentLocomotion && (double) this.creature.currentLocomotion.velocity.magnitude > (double) this.creature.handToBodyRotationMaxVelocity && (double) UnityEngine.Vector3.Angle(this.creature.currentLocomotion.velocity.normalized, xz) < (double) this.creature.handToBodyRotationMaxAngle;
    UnityEngine.Vector3 to1 = new UnityEngine.Vector3(this.creature.ragdoll.ik.handRightTarget.position.x, this.creature.transform.position.y, this.creature.ragdoll.ik.handRightTarget.position.z) - this.creature.transform.position;
    UnityEngine.Vector3 to2 = new UnityEngine.Vector3(this.creature.ragdoll.ik.handLeftTarget.position.x, this.creature.transform.position.y, this.creature.ragdoll.ik.handLeftTarget.position.z) - this.creature.transform.position;
    float input = UnityEngine.Vector3.Angle(this.creature.transform.forward, xz);
    float num1 = flag ? 1f : Utils.CalculateRatio(input, this.creature.headMinAngle, this.creature.headMaxAngle, 0.0f, 1f);
    float num2 = (float) ((double) UnityEngine.Vector3.SignedAngle(this.creature.transform.right, to1, this.creature.transform.up) * (flag ? 0.0 : (double) this.creature.handRight.GetArmLenghtRatio(true)) * (1.0 - (double) num1));
    float num3 = (float) ((double) UnityEngine.Vector3.SignedAngle(-this.creature.transform.right, to2, this.creature.transform.up) * (flag ? 0.0 : (double) this.creature.handLeft.GetArmLenghtRatio(true)) * (1.0 - (double) num1));
    float num4 = UnityEngine.Vector3.SignedAngle(this.creature.transform.forward, xz, this.creature.transform.up) * num1;
    this.creature.transform.localEulerAngles = new UnityEngine.Vector3(0.0f, this.creature.transform.localEulerAngles.y + (this.creature.turnRelativeToHand ? num2 + num3 + num4 : num4) * this.creature.turnSpeed * Time.deltaTime, 0.0f);
  }

  public virtual void Setup() => throw new NotImplementedException();

  public virtual float GetLocomotionWeight() => throw new NotImplementedException();

  public virtual void SetLocomotionWeight(float weight) => throw new NotImplementedException();

  public virtual void AddLocomotionDeltaPosition(UnityEngine.Vector3 delta)
  {
    throw new NotImplementedException();
  }

  public virtual void AddLocomotionDeltaRotation(Quaternion delta, UnityEngine.Vector3 pivot)
  {
    throw new NotImplementedException();
  }

  public virtual void SetFullbody(bool active) => this.fullbody = active;

  public virtual float GetLookAtWeight() => throw new NotImplementedException();

  public virtual void SetLookAtTarget(Transform anchor) => throw new NotImplementedException();

  public virtual void SetLookAtWeight(float weight) => throw new NotImplementedException();

  public virtual void SetLookAtBodyWeight(float weight, float clamp)
  {
    throw new NotImplementedException();
  }

  public virtual void SetLookAtHeadWeight(float weight, float clamp)
  {
    throw new NotImplementedException();
  }

  public virtual void SetLookAtEyesWeight(float weight, float clamp)
  {
    throw new NotImplementedException();
  }

  public virtual void SetHeadAnchor(Transform anchor) => throw new NotImplementedException();

  public virtual void SetHeadState(bool positionEnabled, bool rotationEnabled)
  {
    throw new NotImplementedException();
  }

  public virtual void SetHeadWeight(float positionWeight, float rotationWeight)
  {
    throw new NotImplementedException();
  }

  public virtual float GetHeadWeight() => throw new NotImplementedException();

  public virtual void SetHipsAnchor(Transform anchor) => throw new NotImplementedException();

  public virtual void SetHipsState(bool active) => throw new NotImplementedException();

  public virtual void SetHipsWeight(float active) => throw new NotImplementedException();

  public virtual float GetHipsWeight() => throw new NotImplementedException();

  public virtual void SetShoulderAnchor(Side side, Transform anchor)
  {
    throw new NotImplementedException();
  }

  public virtual void SetShoulderState(Side side, bool positionEnabled, bool rotationEnabled)
  {
    throw new NotImplementedException();
  }

  public virtual void SetShoulderWeight(Side side, float positionWeight, float rotationWeight)
  {
    throw new NotImplementedException();
  }

  public virtual void SetHandAnchor(Side side, Transform anchor)
  {
    this.SetHandAnchor(side, anchor, Quaternion.identity);
  }

  public virtual void SetHandAnchor(Side side, Transform anchor, Quaternion palmRotation)
  {
    throw new NotImplementedException();
  }

  public virtual void SetHandState(Side side, bool positionEnabled, bool rotationEnabled)
  {
    throw new NotImplementedException();
  }

  public virtual void SetHandWeight(Side side, float positionWeight, float rotationWeight)
  {
    throw new NotImplementedException();
  }

  public virtual float GetHandPositionWeight(Side side) => throw new NotImplementedException();

  public virtual float GetHandRotationWeight(Side side) => throw new NotImplementedException();

  public virtual void SetFootAnchor(Side side, Transform anchor)
  {
    this.SetFootAnchor(side, anchor, Quaternion.identity);
  }

  public virtual void SetFootAnchor(Side side, Transform anchor, Quaternion toesRotation)
  {
    throw new NotImplementedException();
  }

  public virtual void SetFootState(Side side, bool active) => throw new NotImplementedException();

  public virtual void SetFootWeight(Side side, float positionWeight, float rotationWeight)
  {
    throw new NotImplementedException();
  }

  public virtual void SetFootPull(Side side, float value) => throw new NotImplementedException();

  public virtual IkController.FootBoneTarget GetFootBoneTarget()
  {
    throw new NotImplementedException();
  }

  public virtual void SetKneeAnchor(Side side, Transform anchor)
  {
    throw new NotImplementedException();
  }

  public virtual void SetKneeState(Side side, bool active) => throw new NotImplementedException();

  public virtual void SetKneeWeight(Side side, float weight) => throw new NotImplementedException();

  public delegate void LateUpdateDelegate();

  public enum FootBoneTarget
  {
    Ankle,
    Toes,
  }
}
