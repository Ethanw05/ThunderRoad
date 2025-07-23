// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Gravity
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

/// <summary>
/// This component applies custom gravity to a collisionHandlers physic body
/// </summary>
public class Gravity : ThunderBehaviour
{
  public CollisionHandler collisionHandler;
  public float gravityMultiplier;

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.FixedUpdate;

  protected internal override void ManagedFixedUpdate()
  {
    if (!(bool) (Object) this.collisionHandler || !this.collisionHandler.active)
      return;
    this.collisionHandler.physicBody.AddForce(this.gravityMultiplier * Physics.gravity, ForceMode.Acceleration);
  }
}
