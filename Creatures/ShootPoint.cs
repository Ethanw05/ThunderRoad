// Decompiled with JetBrains decompiler
// Type: ThunderRoad.ShootPoint
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/ShootPoint.html")]
public class ShootPoint : ThunderBehaviour
{
  public static List<ShootPoint> list = new List<ShootPoint>();
  [Range(0.0f, 360f)]
  [Tooltip("Depicts the angle of which the NPC will go to the shootpoint. If the player is outside of the radius, the NPC will choose another path.")]
  public float allowedAngle = 60f;
  [NonSerialized]
  public UnityEngine.Vector3 navPosition;
  [NonSerialized]
  public Creature currentCreature;

  protected override void ManagedOnEnable()
  {
    base.ManagedOnEnable();
    ShootPoint.list.Add(this);
    NavMeshHit hit;
    if (NavMesh.SamplePosition(this.transform.position, out hit, 100f, -1) && (double) this.transform.position.y > (double) hit.position.y)
      this.navPosition = hit.position;
    else
      this.navPosition = this.transform.position;
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.LateUpdate;

  protected internal override void ManagedLateUpdate()
  {
    Creature currentCreature = this.currentCreature;
    if ((currentCreature != null ? (currentCreature.state == Creature.State.Dead ? 1 : 0) : 0) == 0)
      return;
    this.currentCreature = (Creature) null;
  }

  protected override void ManagedOnDisable()
  {
    base.ManagedOnDisable();
    ShootPoint.list.Remove(this);
  }

  public void OnDrawGizmos()
  {
    if ((double) this.allowedAngle > 0.0 && (double) this.allowedAngle < 360.0)
    {
      Gizmos.color = UnityEngine.Color.green;
      Gizmos.DrawRay(this.transform.position, Quaternion.AngleAxis(this.allowedAngle * 0.5f, UnityEngine.Vector3.up) * this.transform.forward);
      Gizmos.DrawRay(this.transform.position, Quaternion.AngleAxis((float) (-(double) this.allowedAngle * 0.5), UnityEngine.Vector3.up) * this.transform.forward);
    }
    NavMeshHit hit;
    if (NavMesh.SamplePosition(this.transform.position, out hit, 100f, -1) && (double) this.transform.position.y > (double) hit.position.y)
    {
      Gizmos.color = UnityEngine.Color.gray;
      Gizmos.DrawSphere(this.transform.position, 0.15f);
      Gizmos.color = UnityEngine.Color.green;
      Gizmos.DrawLine(this.transform.position, hit.position);
    }
    else
    {
      Gizmos.color = UnityEngine.Color.red;
      Gizmos.DrawSphere(this.transform.position, 0.15f);
    }
  }
}
