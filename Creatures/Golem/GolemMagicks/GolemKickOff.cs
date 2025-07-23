// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemKickOff
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Kick off config")]
public class GolemKickOff : GolemAbility
{
  public GolemController.AttackMotion motion;
  public bool launchClimbers = true;
  public float launchSpeed = 2f;
  public bool launchVertical;
  private bool kickActive;

  public override void Begin(GolemController golem)
  {
    base.Begin(golem);
    golem.PerformAttackMotion(this.motion, new System.Action(((GolemAbility) this).End));
    this.kickActive = true;
  }

  public override void AbilityStep(int step)
  {
    base.AbilityStep(step);
    if (!this.kickActive || !this.golem.isClimbed)
      return;
    foreach (Creature forceUngripClimbers in this.golem.ForceUngripClimbersEnumerable(true))
    {
      if (this.launchClimbers)
      {
        UnityEngine.Vector3 vector3;
        UnityEngine.Vector3 normalized;
        if (!this.launchVertical)
        {
          vector3 = forceUngripClimbers.transform.position.ToXZ() - this.golem.transform.position.ToXZ();
          normalized = vector3.normalized;
        }
        else
        {
          vector3 = forceUngripClimbers.transform.position - this.golem.transform.position;
          normalized = vector3.normalized;
        }
        double launchSpeed = (double) this.launchSpeed;
        UnityEngine.Vector3 force = normalized * (float) launchSpeed;
        forceUngripClimbers.AddForce(force, ForceMode.VelocityChange, 1f, (CollisionHandler) null);
      }
    }
  }

  public override void Interrupt()
  {
    base.Interrupt();
    this.kickActive = false;
  }

  public override void OnEnd()
  {
    base.OnEnd();
    this.kickActive = false;
  }
}
