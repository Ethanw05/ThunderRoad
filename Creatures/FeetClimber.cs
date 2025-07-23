// Decompiled with JetBrains decompiler
// Type: ThunderRoad.FeetClimber
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/FeetClimber.html")]
[AddComponentMenu("ThunderRoad/Creatures/Feet climber")]
public class FeetClimber : ThunderBehaviour
{
  public float footSpeed = 4f;
  public float sweepAngle = -70f;
  public float sweepMinDelay = 0.5f;
  public float sweepMaxVerticalAngle = 30f;
  public float sweepMaxHorizontalAngle = 30f;
  public float sphereCastRadius = 0.05f;
  public float moveOutWeight = 0.2f;
  public float legLenghtMultiplier = 1.3f;
  public float minFootSpacing = 0.2f;
  public float legToHeadMaxAngle = 30f;
  public float footMaxAngle = 45f;
  public bool showDebug;
  public FeetClimber.Foot footLeft;
  public FeetClimber.Foot footRight;
  public float stepAngle;
  public RaycastHit[] sphereCastHits;
  public int sphereCastHitCount;
  protected Creature creature;
  protected float lastSweep;
  protected bool initialized;

  protected void Awake()
  {
    this.creature = this.GetComponentInParent<Creature>();
    this.transform.rotation = this.transform.rotation * Quaternion.AngleAxis(-this.sweepAngle, UnityEngine.Vector3.right);
  }

  public void Init()
  {
    this.stepAngle = UnityEngine.Vector3.Angle(UnityEngine.Vector3.forward, new UnityEngine.Vector3(0.0f, this.sphereCastRadius * 2f, this.GetSweepDistance()));
    this.sphereCastHits = new RaycastHit[Mathf.CeilToInt((float) ((double) this.sweepMaxVerticalAngle / (double) this.stepAngle + 1.0)) * 2 * (Mathf.CeilToInt(this.sweepMaxHorizontalAngle / this.stepAngle) * 2)];
    this.footLeft = new FeetClimber.Foot(this, Side.Left);
    this.footRight = new FeetClimber.Foot(this, Side.Right);
    this.initialized = true;
  }

  public float GetSweepDistance() => this.creature.morphology.legsLength * this.legLenghtMultiplier;

  public FeetClimber.Foot GetFoot(Side side) => side != Side.Left ? this.footRight : this.footLeft;

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    if (!this.initialized || !this.creature.isPlayer)
      return;
    if (this.creature.currentLocomotion.isGrounded)
    {
      if (this.footLeft.enabled && !this.creature.player.footLeft.footTracked)
        this.footLeft.StopPose();
      if (this.footRight.enabled && !this.creature.player.footRight.footTracked)
        this.footRight.StopPose();
      if (this.footLeft.enabled || this.footRight.enabled)
        return;
      this.creature.animator.SetBool(Creature.hashFalling, false);
    }
    else
    {
      this.transform.position = this.creature.ragdoll.rootPart.transform.position;
      if (this.creature.handLeft.climb.isGripping || this.creature.handRight.climb.isGripping || this.creature.handLeft.collisionHandler.isColliding || this.creature.handRight.collisionHandler.isColliding)
      {
        if (!this.footLeft.enabled && !this.creature.player.footLeft.footTracked)
          this.footLeft.StartPose();
        if (!this.footRight.enabled && !this.creature.player.footRight.footTracked)
          this.footRight.StartPose();
      }
      this.footLeft.Update();
      this.footRight.Update();
    }
  }

  protected void CastSweep()
  {
    if ((double) Time.time - (double) this.lastSweep < (double) this.sweepMinDelay)
      return;
    this.sphereCastHitCount = 0;
    for (float horizontalAngle = 0.0f; (double) horizontalAngle < (double) this.sweepMaxHorizontalAngle; horizontalAngle += this.stepAngle)
    {
      this.SphereCastVertical(horizontalAngle);
      this.SphereCastVertical(-horizontalAngle);
    }
    this.footLeft.hitValid = false;
    this.footRight.hitValid = false;
    float num1 = float.PositiveInfinity;
    for (int index = 0; index < this.sphereCastHitCount; ++index)
    {
      if ((double) this.transform.InverseTransformPoint(this.sphereCastHits[index].point).x < 0.0)
      {
        float num2 = UnityEngine.Vector3.Angle(this.sphereCastHits[index].normal, this.transform.position - this.sphereCastHits[index].point);
        if ((double) num2 < (double) this.footMaxAngle && (double) num2 < (double) num1)
        {
          num1 = num2;
          this.footLeft.raycastHit = this.sphereCastHits[index];
          this.footLeft.hitValid = true;
        }
      }
    }
    float num3 = float.PositiveInfinity;
    for (int index = 0; index < this.sphereCastHitCount; ++index)
    {
      if ((double) this.transform.InverseTransformPoint(this.sphereCastHits[index].point).x > 0.0 && (!this.footLeft.hitValid || (double) UnityEngine.Vector3.Distance(this.sphereCastHits[index].point, this.footLeft.raycastHit.point) > (double) this.minFootSpacing))
      {
        float num4 = UnityEngine.Vector3.Angle(this.sphereCastHits[index].normal, this.transform.position - this.sphereCastHits[index].point);
        if ((double) num4 < (double) this.footMaxAngle && (double) num4 < (double) num3)
        {
          num3 = num4;
          this.footRight.raycastHit = this.sphereCastHits[index];
          this.footRight.hitValid = true;
        }
      }
    }
    this.lastSweep = Time.time;
  }

  protected void SphereCastVertical(float horizontalAngle)
  {
    RaycastHit hitInfo;
    if (this.SphereCastAngle(0.0f, horizontalAngle, out hitInfo))
    {
      this.sphereCastHits[this.sphereCastHitCount] = hitInfo;
      ++this.sphereCastHitCount;
    }
    float verticalAngle = 0.0f;
    while ((double) verticalAngle < (double) this.sweepMaxVerticalAngle)
    {
      verticalAngle += this.stepAngle;
      if (this.SphereCastAngle(-verticalAngle, horizontalAngle, out hitInfo))
      {
        this.sphereCastHits[this.sphereCastHitCount] = hitInfo;
        ++this.sphereCastHitCount;
      }
      if (this.SphereCastAngle(verticalAngle, horizontalAngle, out hitInfo))
      {
        this.sphereCastHits[this.sphereCastHitCount] = hitInfo;
        ++this.sphereCastHitCount;
      }
    }
  }

  protected bool SphereCastAngle(
    float verticalAngle,
    float horizontalAngle,
    out RaycastHit hitInfo)
  {
    return Physics.SphereCast(new Ray(this.transform.position, this.transform.rotation * (Quaternion.AngleAxis(verticalAngle, UnityEngine.Vector3.right) * Quaternion.AngleAxis(horizontalAngle, UnityEngine.Vector3.up) * UnityEngine.Vector3.forward)), this.sphereCastRadius, out hitInfo, this.GetSweepDistance() - this.sphereCastRadius, (int) ThunderRoadSettings.current.groundLayer);
  }

  protected void OnDrawGizmos()
  {
    for (int index = 0; index < this.sphereCastHitCount; ++index)
    {
      if (this.footRight.hitValid && this.sphereCastHits[index].point == this.footRight.raycastHit.point)
      {
        Gizmos.color = UnityEngine.Color.green;
        Gizmos.DrawWireSphere(this.sphereCastHits[index].point, this.minFootSpacing);
        Gizmos.DrawRay(this.sphereCastHits[index].point, this.sphereCastHits[index].normal * 0.02f);
        Gizmos.DrawLine(this.transform.position, this.sphereCastHits[index].point);
      }
      if (this.footLeft.hitValid && this.sphereCastHits[index].point == this.footLeft.raycastHit.point)
      {
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawWireSphere(this.sphereCastHits[index].point, this.minFootSpacing);
        Gizmos.DrawRay(this.sphereCastHits[index].point, this.sphereCastHits[index].normal * 0.02f);
        Gizmos.DrawLine(this.transform.position, this.sphereCastHits[index].point);
      }
      if (this.showDebug)
      {
        Gizmos.color = UnityEngine.Color.gray;
        Gizmos.DrawRay(this.sphereCastHits[index].point, this.sphereCastHits[index].normal * 0.02f);
      }
    }
  }

  [Serializable]
  public class Foot
  {
    protected FeetClimber feetClimber;
    public bool enabled;
    public Side side;
    public FeetClimber.Foot.State state;
    public bool hitValid;
    public RaycastHit raycastHit;
    public Transform grip;
    public Transform gripIkAnchor;
    protected float weight;

    public Foot(FeetClimber feetClimber, Side side)
    {
      this.feetClimber = feetClimber;
      this.side = side;
      this.grip = new GameObject("FootGrip" + side.ToString()).transform;
      this.grip.SetParent(feetClimber.transform);
      this.gripIkAnchor = new GameObject("IkAnchor").transform;
      this.gripIkAnchor.SetParent(this.grip);
    }

    public void StartPose()
    {
      this.grip.SetParent((Transform) null);
      this.state = FeetClimber.Foot.State.Idle;
      this.weight = 0.0f;
      if (this.feetClimber.creature.isPlayer)
      {
        this.gripIkAnchor.localPosition = this.feetClimber.creature.GetFoot(this.side).grip.transform.InverseTransformPointUnscaled(this.feetClimber.creature.GetFoot(this.side).toesAnchor.position);
        this.gripIkAnchor.localRotation = Quaternion.Inverse(this.feetClimber.creature.GetFoot(this.side).grip.transform.rotation) * this.feetClimber.creature.GetFoot(this.side).toesAnchor.rotation;
      }
      else
      {
        this.gripIkAnchor.localPosition = this.feetClimber.creature.GetFoot(this.side).grip.transform.InverseTransformPointUnscaled(this.feetClimber.creature.GetFoot(this.side).transform.position);
        this.gripIkAnchor.localRotation = Quaternion.Inverse(this.feetClimber.creature.GetFoot(this.side).grip.transform.rotation) * this.feetClimber.creature.GetFoot(this.side).transform.rotation;
      }
      this.feetClimber.creature.animator.SetBool(Creature.hashFalling, true);
      this.feetClimber.creature.ragdoll.ik.SetFootAnchor(this.side, this.gripIkAnchor);
      this.feetClimber.creature.ragdoll.ik.SetFootWeight(this.side, this.weight, this.weight);
      this.enabled = true;
    }

    public FeetClimber.Foot GetOtherFoot()
    {
      return this.side != Side.Left ? this.feetClimber.footLeft : this.feetClimber.footRight;
    }

    public void Update()
    {
      if (!this.enabled)
        return;
      if (this.state == FeetClimber.Foot.State.Idle)
      {
        this.feetClimber.CastSweep();
        if (this.hitValid && (this.GetOtherFoot().state != FeetClimber.Foot.State.Posed && this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn || this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn && (double) UnityEngine.Vector3.Distance(this.raycastHit.point, this.GetOtherFoot().grip.position) > (double) this.feetClimber.minFootSpacing))
          this.state = FeetClimber.Foot.State.MovingIn;
        this.weight = Mathf.MoveTowards(this.weight, 0.0f, Time.deltaTime * this.feetClimber.footSpeed);
      }
      else if (this.state == FeetClimber.Foot.State.MovingOut)
      {
        this.feetClimber.CastSweep();
        if (this.hitValid)
        {
          this.weight = Mathf.MoveTowards(this.weight, this.feetClimber.moveOutWeight, Time.deltaTime * this.feetClimber.footSpeed);
          if ((double) this.weight == (double) this.feetClimber.moveOutWeight && (this.GetOtherFoot().state != FeetClimber.Foot.State.Posed && this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn || this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn && (double) UnityEngine.Vector3.Distance(this.raycastHit.point, this.GetOtherFoot().grip.position) > (double) this.feetClimber.minFootSpacing))
            this.state = FeetClimber.Foot.State.MovingIn;
        }
        else
          this.state = FeetClimber.Foot.State.Idle;
      }
      else if (this.state == FeetClimber.Foot.State.MovingIn)
      {
        if (this.grip.position != this.raycastHit.point)
        {
          this.grip.position = this.raycastHit.point;
          this.grip.rotation = Quaternion.LookRotation(this.raycastHit.normal, this.feetClimber.transform.up);
          this.grip.rotation = Quaternion.LookRotation(this.grip.transform.up, this.grip.transform.forward);
          if ((double) Math.Abs(UnityEngine.Vector3.SignedAngle(UnityEngine.Vector3.up, this.grip.up, this.grip.forward)) > 45.0)
            this.grip.rotation *= Quaternion.AngleAxis(45f, UnityEngine.Vector3.right);
        }
        if (this.PoseValid(this.grip.position))
        {
          this.weight = Mathf.MoveTowards(this.weight, 1f, Time.deltaTime * this.feetClimber.footSpeed);
          if (Math.Abs((double) this.weight - 1.0) < 0.001)
            this.state = FeetClimber.Foot.State.Posed;
        }
        else
          this.state = FeetClimber.Foot.State.MovingOut;
      }
      else if (this.state == FeetClimber.Foot.State.Posed && !this.PoseValid(this.grip.position))
        this.state = FeetClimber.Foot.State.MovingOut;
      this.feetClimber.creature.ragdoll.ik.SetFootWeight(this.side, this.weight, this.weight);
    }

    protected bool PoseValid(UnityEngine.Vector3 position)
    {
      return (double) UnityEngine.Vector3.Distance(position, this.feetClimber.transform.position) <= (double) this.feetClimber.creature.morphology.legsLength && (this.side != Side.Right || (double) this.feetClimber.transform.InverseTransformPoint(position).x >= 0.0 && (double) UnityEngine.Vector3.Angle(this.feetClimber.creature.ragdoll.headPart.bone.animation.transform.position - this.feetClimber.transform.position, this.feetClimber.creature.footRight.lowerLegBone.position - this.feetClimber.creature.footRight.upperLegBone.position) >= (double) this.feetClimber.legToHeadMaxAngle) && (this.side != Side.Left || (double) this.feetClimber.transform.InverseTransformPoint(position).x <= 0.0 && (double) UnityEngine.Vector3.Angle(this.feetClimber.creature.ragdoll.headPart.bone.animation.transform.position - this.feetClimber.transform.position, this.feetClimber.creature.footLeft.lowerLegBone.position - this.feetClimber.creature.footLeft.upperLegBone.position) >= (double) this.feetClimber.legToHeadMaxAngle);
    }

    public void StopPose()
    {
      this.grip.SetParent(this.feetClimber.transform);
      if (this.enabled)
      {
        this.feetClimber.creature.ragdoll.ik.SetFootAnchor(this.side, (Transform) null);
        this.weight = 0.0f;
      }
      this.enabled = false;
    }

    public enum State
    {
      Idle,
      MovingOut,
      MovingIn,
      Posed,
    }
  }
}
