// Decompiled with JetBrains decompiler
// Type: ThunderRoad.AirHelper
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class AirHelper : ThunderBehaviour
{
  public float minHeight = 1f;
  public Locomotion locomotion;
  public bool inAir;

  public event AirHelper.AirEvent OnAirEvent;

  public event AirHelper.AirEvent OnGroundEvent;

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  public bool Climbing
  {
    get
    {
      RagdollHandClimb climb1 = Player.currentCreature.handLeft.climb;
      if (climb1 != null && climb1.isGripping && climb1.gripItem == null)
        return true;
      RagdollHandClimb climb2 = Player.currentCreature.handRight.climb;
      return climb2 != null && climb2.isGripping && climb2.gripItem == null;
    }
  }

  protected internal override void ManagedUpdate()
  {
    base.ManagedUpdate();
    this.locomotion = Player.local?.locomotion;
    if ((Object) this.locomotion == (Object) null || (Object) Player.currentCreature?.handLeft == (Object) null)
      return;
    bool climbing = this.Climbing;
    if (this.inAir)
    {
      if (((!this.locomotion.isGrounded ? 0 : (!this.locomotion.isJumping ? 1 : 0)) | (climbing ? 1 : 0)) == 0)
        return;
      this.Trigger(false);
    }
    else
    {
      if (this.locomotion.isGrounded && !this.locomotion.isJumping || this.locomotion.SphereCastGround(this.minHeight, out RaycastHit _, out float _) || climbing)
        return;
      this.Trigger(true);
    }
  }

  public void Trigger(bool active)
  {
    this.inAir = active;
    Creature creature = this.locomotion.player?.creature ?? this.locomotion.creature;
    if (!(bool) (Object) creature)
      return;
    if (active)
    {
      AirHelper.AirEvent onAirEvent = this.OnAirEvent;
      if (onAirEvent == null)
        return;
      onAirEvent(creature);
    }
    else
    {
      AirHelper.AirEvent onGroundEvent = this.OnGroundEvent;
      if (onGroundEvent == null)
        return;
      onGroundEvent(creature);
    }
  }

  public delegate void AirEvent(Creature creature);
}
