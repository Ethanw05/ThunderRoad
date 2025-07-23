// Decompiled with JetBrains decompiler
// Type: ThunderRoad.VelocityTracker
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class VelocityTracker : ThunderBehaviour
{
  public UnityEngine.Vector3 velocity;
  public UnityEngine.Vector3 angularVelocity;
  private UnityEngine.Vector3 lastPos;
  private UnityEngine.Vector3 lastRot;

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  public UnityEngine.Vector3 position
  {
    get => this.transform.position;
    set => this.transform.position = value;
  }

  public Quaternion rotation
  {
    get => this.transform.rotation;
    set => this.transform.rotation = value;
  }

  public static implicit operator Transform(VelocityTracker tracker) => tracker.transform;

  protected override void ManagedOnEnable()
  {
    base.ManagedOnEnable();
    this.lastPos = this.transform.position;
    this.lastRot = this.transform.eulerAngles;
  }

  protected internal override void ManagedUpdate()
  {
    base.ManagedUpdate();
    this.velocity = (this.transform.position - this.lastPos) / Time.fixedDeltaTime;
    this.angularVelocity = (this.transform.eulerAngles - this.lastRot) / Time.fixedDeltaTime;
    this.lastPos = this.transform.position;
    this.lastRot = this.transform.eulerAngles;
  }
}
