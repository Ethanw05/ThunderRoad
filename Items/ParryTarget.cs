// Decompiled with JetBrains decompiler
// Type: ThunderRoad.ParryTarget
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ParryTarget.html")]
public class ParryTarget : ThunderBehaviour
{
  [Tooltip("Depicts the length of the ParryTarget. With this, AI will know how long your weapon is, and be able to parry it.\n\nCan be adjusted via button to adjust the edge-to-edge gizmo.")]
  public float length = 0.25f;
  public static List<ParryTarget> list = new List<ParryTarget>();
  public Collider testCollider;
  public Side testSide;
  public Item item;
  public Creature owner;

  public UnityEngine.Vector3 GetLineStart()
  {
    return this.transform.position + this.transform.up * this.length;
  }

  public UnityEngine.Vector3 GetLineEnd()
  {
    return this.transform.position + -this.transform.up * this.length;
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = UnityEngine.Color.white;
    Gizmos.DrawLine(this.GetLineStart(), this.GetLineEnd());
  }

  protected new virtual void OnDisable()
  {
    if (!ParryTarget.list.Contains(this))
      return;
    ParryTarget.list.Remove(this);
  }

  protected void Awake()
  {
    this.item = this.GetComponentInParent<Item>();
    this.item.OnGrabEvent += new Item.GrabDelegate(this.OnGrab);
    this.item.OnUngrabEvent += new Item.ReleaseDelegate(this.OnRelease);
    this.item.OnFlyEndEvent += new Item.ThrowingDelegate(this.OnThrowingEnd);
    this.item.OnSnapEvent += new Item.HolderDelegate(this.OnSnap);
  }

  protected void OnGrab(Handle handle, RagdollHand ragdollHand)
  {
    this.owner = handle.item.mainHandler.ragdoll.creature;
    if (ParryTarget.list.Contains(this))
      return;
    ParryTarget.list.Add(this);
  }

  protected void OnRelease(Handle handle, RagdollHand ragdollHand, bool throwing)
  {
    if (throwing)
      return;
    this.owner = (Creature) null;
    if (!ParryTarget.list.Contains(this))
      return;
    ParryTarget.list.Remove(this);
  }

  protected void OnThrowingEnd(Item _)
  {
    this.owner = (Creature) null;
    if (!ParryTarget.list.Contains(this))
      return;
    ParryTarget.list.Remove(this);
  }

  protected void OnSnap(Holder handle)
  {
    if (!ParryTarget.list.Contains(this))
      return;
    ParryTarget.list.Remove(this);
  }

  public ParryTarget.ParryInfo GetParryPositionAndRotation(Collider defenseCollider)
  {
    ParryTarget.ParryInfo positionAndRotation;
    positionAndRotation.parryTarget = this;
    positionAndRotation.onHitCourse = false;
    positionAndRotation.windingUp = false;
    Utils.ClosestPointOnSurface(defenseCollider, this.GetLineStart(), this.GetLineEnd(), out positionAndRotation.colliderPoint, out positionAndRotation.targetPoint, out positionAndRotation.insideCollider);
    positionAndRotation.eventualColliderPoint = positionAndRotation.colliderPoint;
    positionAndRotation.targetPointVelocity = this.item.GetItemPointVelocity(positionAndRotation.targetPoint, this.item.physicBody.isKinematic);
    positionAndRotation.noHitCourseColliderPoint = positionAndRotation.colliderPoint;
    RaycastHit hitInfo1;
    if (!positionAndRotation.insideCollider && positionAndRotation.targetPointVelocity != UnityEngine.Vector3.zero && defenseCollider.Raycast(new Ray(positionAndRotation.targetPoint, positionAndRotation.targetPointVelocity.normalized), out hitInfo1, 10f))
    {
      positionAndRotation.onHitCourse = true;
      positionAndRotation.colliderPoint = hitInfo1.point;
    }
    UnityEngine.Vector3 vector3;
    if (!positionAndRotation.onHitCourse && positionAndRotation.targetPointVelocity != UnityEngine.Vector3.zero)
    {
      UnityEngine.Vector3 normalized1 = positionAndRotation.targetPointVelocity.normalized;
      vector3 = positionAndRotation.parryTarget.item.physicBody.velocity;
      UnityEngine.Vector3 normalized2 = vector3.normalized;
      if ((double) UnityEngine.Vector3.Dot(normalized1, normalized2) > 0.800000011920929)
      {
        UnityEngine.Vector3 normalized3 = positionAndRotation.targetPointVelocity.normalized;
        vector3 = positionAndRotation.colliderPoint - positionAndRotation.targetPoint;
        UnityEngine.Vector3 normalized4 = vector3.normalized;
        RaycastHit hitInfo2;
        if ((double) UnityEngine.Vector3.Dot(normalized3, normalized4) < 0.0 && defenseCollider.Raycast(new Ray(positionAndRotation.targetPoint, -positionAndRotation.targetPointVelocity.normalized), out hitInfo2, 10f))
        {
          positionAndRotation.windingUp = true;
          positionAndRotation.eventualColliderPoint = hitInfo2.point;
        }
      }
    }
    ref ParryTarget.ParryInfo local = ref positionAndRotation;
    vector3 = positionAndRotation.insideCollider ? positionAndRotation.colliderPoint - positionAndRotation.targetPoint : positionAndRotation.targetPoint - positionAndRotation.colliderPoint;
    UnityEngine.Vector3 normalized = vector3.normalized;
    local.dir = normalized;
    positionAndRotation.distance = UnityEngine.Vector3.Distance(positionAndRotation.colliderPoint, positionAndRotation.targetPoint);
    positionAndRotation.noHitCourseDistance = UnityEngine.Vector3.Distance(positionAndRotation.noHitCourseColliderPoint, positionAndRotation.targetPoint);
    return positionAndRotation;
  }

  private void OnDrawGizmos()
  {
    if (!(bool) (Object) this.testCollider)
      return;
    ParryTarget.ParryInfo positionAndRotation = this.GetParryPositionAndRotation(this.testCollider);
    Gizmos.color = UnityEngine.Color.yellow;
    Gizmos.DrawLine(positionAndRotation.colliderPoint, positionAndRotation.targetPoint);
    Gizmos.color = positionAndRotation.insideCollider ? UnityEngine.Color.red : UnityEngine.Color.green;
    Gizmos.DrawWireSphere(positionAndRotation.colliderPoint, 0.01f);
    Gizmos.DrawWireSphere(positionAndRotation.targetPoint, 0.01f);
  }

  public struct ParryInfo
  {
    public ParryTarget parryTarget;
    public UnityEngine.Vector3 colliderPoint;
    public UnityEngine.Vector3 eventualColliderPoint;
    public UnityEngine.Vector3 targetPoint;
    public UnityEngine.Vector3 targetPointVelocity;
    public bool onHitCourse;
    public bool windingUp;
    public UnityEngine.Vector3 noHitCourseColliderPoint;
    public float noHitCourseDistance;
    public bool insideCollider;
    public UnityEngine.Vector3 dir;
    public float distance;
  }
}
