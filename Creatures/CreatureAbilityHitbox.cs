// Decompiled with JetBrains decompiler
// Type: ThunderRoad.CreatureAbilityHitbox
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class CreatureAbilityHitbox : MonoBehaviour
{
  public bool forcer = true;
  public bool damager = true;
  public List<CreatureAbilityHitbox.InflictedStatus> applyStatuses = new List<CreatureAbilityHitbox.InflictedStatus>();
  [NonSerialized]
  public Collider hitCollider;
  protected Creature creature;
  protected CreatureAbility ability;
  protected static Dictionary<Creature, List<PhysicBody>> affectedBodies = new Dictionary<Creature, List<PhysicBody>>();
  protected static Dictionary<Creature, List<Creature>> damagedCreatures = new Dictionary<Creature, List<Creature>>();

  public bool active => this.hitCollider.enabled;

  private void Start()
  {
    this.hitCollider = this.GetComponent<Collider>();
    this.hitCollider.enabled = false;
    this.creature = this.GetComponentInParent<Creature>();
  }

  private void CheckInitCreatureAffectedList<T>(Dictionary<Creature, T> dict) where T : new()
  {
    // ISSUE: explicit non-virtual call
    if ((dict != null ? (!__nonvirtual (dict.ContainsKey(this.creature)) ? 1 : 0) : 1) == 0)
      return;
    if (dict == null)
      dict = new Dictionary<Creature, T>();
    dict.Add(this.creature, new T());
  }

  public void EnableHitBox(CreatureAbility ability)
  {
    this.ability = ability;
    this.hitCollider.enabled = true;
    this.CheckInitCreatureAffectedList<List<PhysicBody>>(CreatureAbilityHitbox.affectedBodies);
    this.CheckInitCreatureAffectedList<List<Creature>>(CreatureAbilityHitbox.damagedCreatures);
  }

  public void DisableHitBox()
  {
    this.ability = (CreatureAbility) null;
    this.hitCollider.enabled = false;
    CreatureAbilityHitbox.affectedBodies.Clear();
    CreatureAbilityHitbox.damagedCreatures.Clear();
  }

  private void OnTriggerEnter(Collider other)
  {
    PhysicBody physicBody = other.GetPhysicBody();
    if ((object) physicBody == null)
      return;
    Item componentInParent1 = physicBody.gameObject.GetComponentInParent<Item>();
    if (componentInParent1 != null)
    {
      if (!((UnityEngine.Object) componentInParent1.breakable != (UnityEngine.Object) null) || !this.ability.breakBreakables)
        return;
      componentInParent1.breakable.Break();
    }
    else
    {
      Creature hitCreature = (Creature) null;
      RagdollPart component;
      if (physicBody.gameObject.TryGetComponent<RagdollPart>(out component))
      {
        hitCreature = component.ragdoll.creature;
      }
      else
      {
        Creature componentInParent2 = physicBody.gameObject.GetComponentInParent<Creature>();
        if (componentInParent2 != null)
          hitCreature = componentInParent2;
      }
      if ((UnityEngine.Object) hitCreature == (UnityEngine.Object) null || (UnityEngine.Object) hitCreature == (UnityEngine.Object) this.creature)
        return;
      if (this.damager && !CreatureAbilityHitbox.damagedCreatures[this.creature].Contains(hitCreature))
      {
        hitCreature.Damage(this.ability.contactDamage);
        CreatureAbilityHitbox.damagedCreatures[this.creature].Add(hitCreature);
      }
      foreach (CreatureAbilityHitbox.InflictedStatus applyStatuse in this.applyStatuses)
      {
        if (!hitCreature.HasStatus(applyStatuse.data))
          hitCreature.Inflict(applyStatuse.data, (object) this, applyStatuse.duration);
      }
      if (!this.forcer)
        return;
      UnityEngine.Vector3 vector3 = (hitCreature.transform.position - this.hitCollider.bounds.center).normalized * this.ability.contactForce;
      if (hitCreature.isPlayer)
      {
        if (CreatureAbilityHitbox.affectedBodies[this.creature].Contains(hitCreature.currentLocomotion.physicBody))
          return;
        if (this.ability.forceUngrip)
        {
          Ungrab(Side.Left);
          Ungrab(Side.Right);
        }
        hitCreature.currentLocomotion.physicBody.AddForce(hitCreature.locomotion.isGrounded ? vector3.ToXZ() : vector3, ForceMode.VelocityChange);
        CreatureAbilityHitbox.affectedBodies[this.creature].Add(hitCreature.currentLocomotion.physicBody);
      }
      else
      {
        hitCreature.ragdoll.SetState(Ragdoll.State.Destabilized);
        foreach (RagdollPart part in hitCreature.ragdoll.parts)
        {
          if (!CreatureAbilityHitbox.affectedBodies[this.creature].Contains(part.physicBody))
          {
            part.physicBody.AddForce(vector3, ForceMode.VelocityChange);
            CreatureAbilityHitbox.affectedBodies[this.creature].Add(part.physicBody);
          }
        }
      }

      void Ungrab(Side side)
      {
        RagdollHand hand = hitCreature.GetHand(side);
        if ((UnityEngine.Object) hand == (UnityEngine.Object) null)
          return;
        if (hand.climb != null)
          hand.climb.UnGrip();
        Handle grabbedHandle = hand.grabbedHandle;
        if (!((grabbedHandle != null ? (UnityEngine.Object) grabbedHandle.GetPhysicBodyInParent()?.gameObject?.GetComponent<RagdollPart>()?.ragdoll : (UnityEngine.Object) null) == (UnityEngine.Object) this.ability?.creature?.ragdoll))
          return;
        hand.UnGrab(false);
      }
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if (!this.ability.multiHit)
      return;
    PhysicBody physicBody = other.GetPhysicBody();
    if ((object) physicBody == null || !CreatureAbilityHitbox.affectedBodies[this.creature].Contains(physicBody))
      return;
    CreatureAbilityHitbox.affectedBodies[this.creature].Remove(physicBody);
    Creature key = (Creature) null;
    RagdollPart component;
    if (physicBody.gameObject.TryGetComponent<RagdollPart>(out component))
    {
      key = component.ragdoll.creature;
    }
    else
    {
      Creature componentInParent = physicBody.gameObject.GetComponentInParent<Creature>();
      if (componentInParent != null)
        key = componentInParent;
    }
    if (!((UnityEngine.Object) key != (UnityEngine.Object) null) || CreatureAbilityHitbox.affectedBodies[this.creature].Contains(key.currentLocomotion.physicBody))
      return;
    foreach (RagdollPart part in this.creature.ragdoll.parts)
    {
      if (CreatureAbilityHitbox.affectedBodies[this.creature].Contains(part.physicBody))
        return;
    }
    CreatureAbilityHitbox.damagedCreatures.Remove(key);
  }

  [Serializable]
  public class InflictedStatus
  {
    public string data;
    public float duration = 3f;

    private List<ValueDropdownItem<string>> GetAllStatuses
    {
      get => Catalog.GetDropdownAllID<StatusData>();
    }
  }
}
