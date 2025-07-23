// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemAbility
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

public abstract class GolemAbility : ScriptableObject
{
  public GolemAbilityType type = GolemAbilityType.Ranged;
  public float weight = 1f;
  public RampageType rampageType;
  public Golem.Tier abilityTier;
  public bool stunOnExit;
  public float stunDuration;
  [NonSerialized]
  public bool active;
  [NonSerialized]
  public GolemController golem;
  protected System.Action endCallback;
  [NonSerialized]
  public GolemController.AttackMotion stepMotion;

  /// <summary>
  /// Whether a headshot should interrupt and <c>End()</c> this ability.
  /// </summary>
  public virtual bool HeadshotInterruptable => false;

  /// <summary>
  /// Whether the golem should ask this ability where to look.
  /// Runs <c>GolemAbility.LookAt()</c> during the golem's <c>LookAtCoroutine</c> if true.
  /// </summary>
  public virtual bool OverrideLook => false;

  /// <summary>
  /// Force-run this ability, cancelling whatever the golem was doing before. Not a particularly safe method.
  /// </summary>
  public virtual void ForceRun() => this.golem.StartAbility(this);

  /// <summary>
  /// If <c>OverrideLook</c> is true, this is called during the golem's <c>LookAtCoroutine</c> instead of the default behaviour.
  /// </summary>
  public virtual void LookAt()
  {
  }

  /// <summary>Whether this ability should be allowed to run.</summary>
  /// <param name="golem"></param>
  /// <returns></returns>
  public virtual bool Allow(GolemController golem)
  {
    if (this.type != GolemAbilityType.Climb && !((UnityEngine.Object) golem.lastAbility != (UnityEngine.Object) this))
      return false;
    return this.abilityTier == Golem.Tier.Any || golem.tier == Golem.Tier.Any || this.abilityTier.HasFlagNoGC(golem.tier);
  }

  public void Begin(GolemController golem, System.Action endCallback = null)
  {
    this.golem = golem;
    this.active = true;
    this.endCallback = endCallback;
    this.Begin(golem);
  }

  /// <summary>
  /// Called when the ability first starts. Load data and tell the golem to play animations here.
  /// </summary>
  /// <param name="golem"></param>
  public virtual void Begin(GolemController golem)
  {
  }

  /// <summary>
  /// Called when the golem's current animation triggers an AbilityStep event.
  /// </summary>
  /// <param name="step"></param>
  public void TryAbilityStep(UnityEngine.AnimationEvent e)
  {
    if (e.stringParameter != this.stepMotion.ToString())
      return;
    this.AbilityStep(e.intParameter);
  }

  /// <summary>
  /// Called when the golem's current animation triggers an AbilityStep event.
  /// </summary>
  /// <param name="step"></param>
  public virtual void AbilityStep(int step)
  {
  }

  /// <summary>
  /// This is called once every Golem update cycle (every 0.5 seconds by default).
  /// </summary>
  /// <param name="delta"></param>
  public virtual void OnCycle(float delta)
  {
  }

  /// <summary>This is called once every ManagedUpdate.</summary>
  public virtual void OnUpdate()
  {
  }

  /// <summary>
  /// This is called when the ability is interrupted by a crystal being broken.
  /// </summary>
  public virtual void Interrupt() => this.End(true);

  /// <summary>
  /// Call this to kindly ask the GolemController to safely end this ability.
  /// </summary>
  public void End(bool early)
  {
    if ((UnityEngine.Object) this.golem.currentAbility == (UnityEngine.Object) this)
      this.golem.EndAbility();
    else
      this.OnEnd();
    if (!this.stunOnExit || early)
      return;
    this.golem.Stun(this.stunDuration);
  }

  public void End() => this.End(false);

  /// <summary>
  /// Called when the ability should end. Put code to end and clean up your ability here,
  /// but do not call this directly - use <c>GolemAbility.End()</c> instead.
  /// </summary>
  public virtual void OnEnd()
  {
    this.active = false;
    System.Action endCallback = this.endCallback;
    if (endCallback == null)
      return;
    endCallback();
  }
}
