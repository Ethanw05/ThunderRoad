// Decompiled with JetBrains decompiler
// Type: ThunderRoad.WristStats
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

[AddComponentMenu("ThunderRoad/Creatures/Wrist stats")]
public class WristStats : ThunderBehaviour
{
  public float showDistance = 0.31f;
  public float showAngle = 40f;
  public string healthEffectId;
  public string manaEffectId;
  public string focusEffectId;
  [NonSerialized]
  public Creature creature;
  [NonSerialized]
  public bool isShown = true;
  [NonSerialized]
  public EffectData healthEffectData;
  [NonSerialized]
  public EffectData manaEffectData;
  [NonSerialized]
  public EffectData focusEffectData;
  protected EffectInstance healthEffectInstance;
  protected EffectInstance manaEffectInstance;
  protected EffectInstance focusEffectInstance;

  public List<ValueDropdownItem<string>> GetAllEffectID()
  {
    return Catalog.GetDropdownAllID(Category.Effect);
  }

  public void Awake()
  {
    this.creature = this.GetComponentInParent<Creature>();
    EventManager.onPossess += new EventManager.PossessEvent(this.OnPossessionEvent);
  }

  public void OnPossessionEvent(Creature creature, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd || !((UnityEngine.Object) this.creature == (UnityEngine.Object) creature))
      return;
    this.InitEffects();
  }

  public void InitEffects()
  {
    RagdollPart componentInParent = this.GetComponentInParent<RagdollPart>();
    if ((bool) (UnityEngine.Object) componentInParent)
      this.transform.SetParent(componentInParent.meshBone, false);
    if (this.healthEffectData == null && !string.IsNullOrEmpty(this.healthEffectId))
      this.healthEffectData = Catalog.GetData<EffectData>(this.healthEffectId);
    if (this.manaEffectData == null && !string.IsNullOrEmpty(this.manaEffectId))
      this.manaEffectData = Catalog.GetData<EffectData>(this.manaEffectId);
    if (this.focusEffectData == null && !string.IsNullOrEmpty(this.focusEffectId))
      this.focusEffectData = Catalog.GetData<EffectData>(this.focusEffectId);
    if (this.healthEffectInstance == null && this.healthEffectData != null)
    {
      this.healthEffectInstance = this.healthEffectData.Spawn(this.transform);
      foreach (ThunderBehaviour effect in this.healthEffectInstance.effects)
        effect.gameObject.SetLayerRecursively(GameManager.GetLayer(LayerName.Highlighter));
    }
    if (this.manaEffectInstance == null && this.manaEffectData != null)
    {
      this.manaEffectInstance = this.manaEffectData.Spawn(this.transform);
      foreach (ThunderBehaviour effect in this.manaEffectInstance.effects)
        effect.gameObject.SetLayerRecursively(GameManager.GetLayer(LayerName.Highlighter));
    }
    if (this.focusEffectInstance != null || this.focusEffectData == null)
      return;
    this.focusEffectInstance = this.focusEffectData.Spawn(this.transform);
    foreach (ThunderBehaviour effect in this.focusEffectInstance.effects)
      effect.gameObject.SetLayerRecursively(GameManager.GetLayer(LayerName.Highlighter));
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    if (!(bool) (UnityEngine.Object) this.creature.player)
      return;
    float num1 = UnityEngine.Vector3.Distance(this.creature.centerEyes.position, this.transform.position);
    float num2 = UnityEngine.Vector3.Angle(-this.creature.centerEyes.forward, this.transform.forward);
    if (this.isShown && !GameManager.options.showWristStats)
    {
      this.Show(false);
    }
    else
    {
      if ((double) num1 < (double) this.showDistance && (double) num2 < (double) this.showAngle)
      {
        if (!this.isShown)
        {
          this.Show(true);
          this.isShown = true;
        }
      }
      else if (this.isShown)
      {
        this.Show(false);
        this.isShown = false;
      }
      if (!this.isShown)
        return;
      this.healthEffectInstance?.SetIntensity((double) this.creature.maxHealth == double.PositiveInfinity ? 1f : this.creature.currentHealth / this.creature.maxHealth);
      if (!(bool) (UnityEngine.Object) this.creature.mana)
        return;
      this.manaEffectInstance?.SetIntensity((double) this.creature.mana.MaxFocus == double.PositiveInfinity ? 1f : this.creature.mana.currentFocus / this.creature.mana.MaxFocus);
      if (!(bool) (UnityEngine.Object) this.creature.player)
        return;
      this.focusEffectInstance?.SetIntensity(Mathf.Lerp(0.0f, 0.75f, (double) this.creature.mana.MaxFocus == double.PositiveInfinity ? 1f : this.creature.mana.currentFocus / this.creature.mana.MaxFocus));
    }
  }

  public void Show(bool active)
  {
    if (active)
    {
      this.InitEffects();
      if (this.healthEffectData != null)
        this.healthEffectInstance?.Play();
      else
        Debug.LogError((object) "Wrist stats health effect is missing!");
      if (this.manaEffectData != null)
        this.manaEffectInstance?.Play();
      else
        Debug.LogError((object) "Wrist stats mana effect is missing!");
      if (this.focusEffectData != null)
        this.focusEffectInstance?.Play();
      else
        Debug.LogError((object) "Wrist stats focus effect is missing!");
    }
    else
    {
      this.healthEffectInstance?.Stop();
      this.manaEffectInstance?.Stop();
      this.focusEffectInstance?.Stop();
    }
  }
}
