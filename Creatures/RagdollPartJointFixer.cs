// Decompiled with JetBrains decompiler
// Type: ThunderRoad.RagdollPartJointFixer
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class RagdollPartJointFixer : ThunderBehaviour
{
  private bool _initialized;
  private RagdollPart part;
  private int collidingCount;
  private static int nextTimeSliceId;

  public bool initialized => this._initialized;

  protected override int SliceOverNumFrames => 2;

  protected override int GetNextTimeSliceId => RagdollPartJointFixer.nextTimeSliceId++;

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  public void SetPart(RagdollPart part)
  {
    this.part = part;
    this._initialized = true;
  }

  private void OnCollisionEnter(UnityEngine.Collision collision) => ++this.collidingCount;

  private void OnCollisionExit(UnityEngine.Collision collision) => --this.collidingCount;

  protected internal override void ManagedUpdate()
  {
    if (!this.initialized || !this.part.initialized || !this.part.hasParent || this.part.isSliced || this.collidingCount <= 0 || !Ragdoll.IsPhysicalState(this.part.ragdoll.state) || this.part.ragdoll.handlers.Count != 0 || (Object) Player.currentCreature?.ragdoll == (Object) this.part.ragdoll || (Object) this.part.characterJoint == (Object) null)
      return;
    UnityEngine.Vector3 position = this.transform.position;
    UnityEngine.Vector3 vector3_1 = this.part.parentPart.transform.TransformPoint(this.part.characterJoint.connectedAnchor);
    if (position.PointInRadius(vector3_1, 0.01f))
      return;
    UnityEngine.Vector3 vector3_2 = position - vector3_1;
    RaycastHit hitInfo;
    if (!Physics.Raycast(vector3_1, vector3_2.normalized, out hitInfo, vector3_2.magnitude, this.gameObject.layer | (int) ThunderRoadSettings.current.groundLayer, QueryTriggerInteraction.Ignore) || !(hitInfo.collider.GetPhysicBody() != this.part.physicBody))
      return;
    this.transform.position = hitInfo.point + hitInfo.normal.normalized * 0.1f;
  }
}
