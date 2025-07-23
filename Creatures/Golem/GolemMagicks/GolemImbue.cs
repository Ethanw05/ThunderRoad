// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemImbue
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Self-imbue config")]
[Serializable]
public class GolemImbue : GolemAbility
{
  public float selfImbueDuration = 10f;
  public float contactDamage = 5f;
  public float timeBetweenDamage = 0.5f;
  public bool ungripOnDamage;
  public bool disarmAllowed;
  public string bodyEffectID;
  public string startEffectID;
  public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();
  [NonSerialized]
  public EffectData startEffectData;
  [NonSerialized]
  public EffectData bodyEffectData;
  protected List<EffectInstance> bodyEffects;
  protected bool addedHandlers;

  public override bool Allow(GolemController golem) => base.Allow(golem);

  public override void Begin(GolemController golem)
  {
    base.Begin(golem);
    if (this.startEffectData == null)
      this.startEffectData = Catalog.GetData<EffectData>(this.startEffectID);
    if (this.bodyEffectData == null)
      this.bodyEffectData = Catalog.GetData<EffectData>(this.bodyEffectID);
    golem.PerformAttackMotion(GolemController.AttackMotion.SelfImbue, new System.Action(((GolemAbility) this).End));
    this.startEffectData?.Spawn(golem.transform).Play();
    if (this.addedHandlers)
      return;
    foreach (Component bodyPart in golem.bodyParts)
      bodyPart.GetOrAddComponent<CollisionListener>().OnCollisionEnterEvent += new CollisionListener.CollisionEvent(this.GolemTouch);
  }

  private void GolemTouch(UnityEngine.Collision other)
  {
    if (this.bodyEffects.IsNullOrEmpty())
      return;
    Creature target = (Creature) null;
    RagdollPart component1 = other.rigidbody?.GetComponent<RagdollPart>();
    if (component1 != null)
    {
      target = component1.ragdoll.creature;
    }
    else
    {
      Creature component2 = other.rigidbody?.GetComponent<Creature>();
      if (component2 != null)
      {
        target = component2;
      }
      else
      {
        Item component3 = other.rigidbody?.GetComponent<Item>();
        if (component3 != null && this.disarmAllowed)
        {
          for (int index = component3.handlers.Count - 1; index >= 0; --index)
            this.Affect(component3.handlers[index].creature);
        }
      }
    }
    if (!((UnityEngine.Object) target != (UnityEngine.Object) null))
      return;
    this.Affect(target);
  }

  public override void AbilityStep(int step)
  {
    base.AbilityStep(step);
    this.golem.StartCoroutine(this.EffectCoroutine());
  }

  private IEnumerator EffectCoroutine()
  {
    GolemImbue golemImbue = this;
    golemImbue.ApplyEffects();
    if (golemImbue.golem.isClimbed)
    {
      for (int index = golemImbue.golem.climbers.Count - 1; index >= 0; --index)
      {
        Creature climber = golemImbue.golem.climbers[index];
        golemImbue.Affect(climber);
      }
    }
    golemImbue.golem.OnDamageDealt += new GolemController.GolemDealDamage(golemImbue.GolemHit);
    float endTime = Time.time + golemImbue.selfImbueDuration;
    while ((double) Time.time < (double) endTime)
    {
      if (golemImbue.golem.isClimbed && !golemImbue.bodyEffects.IsNullOrEmpty())
      {
        for (int index = golemImbue.golem.climbers.Count - 1; index >= 0; --index)
        {
          Creature climber = golemImbue.golem.climbers[index];
          golemImbue.Affect(climber);
        }
      }
      yield return (object) Yielders.EndOfFrame;
    }
    golemImbue.golem.OnDamageDealt -= new GolemController.GolemDealDamage(golemImbue.GolemHit);
    golemImbue.RemoveEffects();
  }

  private void GolemHit(Creature target, float damage)
  {
    if (this.bodyEffects.IsNullOrEmpty())
      return;
    this.Affect(target);
  }

  protected void ApplyEffects()
  {
    if (!this.bodyEffects.IsNullOrEmpty())
      return;
    this.bodyEffects = new List<EffectInstance>();
    HumanBodyBones[] humanBodyBonesArray = new HumanBodyBones[9]
    {
      HumanBodyBones.Spine,
      HumanBodyBones.LeftUpperArm,
      HumanBodyBones.LeftLowerArm,
      HumanBodyBones.RightUpperArm,
      HumanBodyBones.RightLowerArm,
      HumanBodyBones.LeftUpperLeg,
      HumanBodyBones.LeftLowerLeg,
      HumanBodyBones.RightUpperLeg,
      HumanBodyBones.RightLowerLeg
    };
    foreach (HumanBodyBones humanBoneId in humanBodyBonesArray)
      SpawnOnBone(this.golem.animator.GetBoneTransform(humanBoneId));

    void SpawnOnBone(Transform bone)
    {
      EffectInstance effectInstance = this.bodyEffectData.Spawn(bone);
      effectInstance.Play();
      this.bodyEffects.Add(effectInstance);
    }
  }

  protected void RemoveEffects()
  {
    if (this.bodyEffects.IsNullOrEmpty())
      return;
    foreach (EffectInstance bodyEffect in this.bodyEffects)
      bodyEffect.Stop();
    this.bodyEffects = (List<EffectInstance>) null;
  }

  protected void Affect(Creature target)
  {
    if ((double) Time.time < (double) target.lastDamageTime + (double) this.timeBetweenDamage)
      return;
    if (this.ungripOnDamage)
    {
      RagdollHand hand = target.GetHand((Side) UnityEngine.Random.Range(0, 2));
      if (!this.ReleaseHand(hand))
        this.ReleaseHand(hand.otherHand);
    }
    target.Damage(this.contactDamage);
    foreach (Golem.InflictedStatus appliedStatuse in this.appliedStatuses)
      target.Inflict(appliedStatuse.data, (object) this, appliedStatuse.duration, (object) appliedStatuse.parameter);
  }

  private bool ReleaseHand(RagdollHand hand)
  {
    if ((double) Time.time < (double) hand.creature.lastDamageTime + (double) this.timeBetweenDamage || !this.golem.grabbedParts.ContainsKey(hand) && !this.disarmAllowed)
      return false;
    if ((UnityEngine.Object) hand.grabbedHandle != (UnityEngine.Object) null)
      hand.UnGrab(false);
    if (hand.climb?.gripPhysicBody != (PhysicBody) null)
      hand.climb.UnGrip();
    hand.climb?.DisableGripTemp(0.5f);
    return true;
  }
}
