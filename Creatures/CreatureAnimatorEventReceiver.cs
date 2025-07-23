// Decompiled with JetBrains decompiler
// Type: CreatureAnimatorEventReceiver
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

#nullable disable
public class CreatureAnimatorEventReceiver : MonoBehaviour
{
  [NonSerialized]
  public CreatureAbility ability;
  protected Creature creature;

  public Dictionary<string, CreatureAbilityHitbox> hitboxDictionary { get; protected set; } = new Dictionary<string, CreatureAbilityHitbox>();

  public CreatureAbilityHitbox[] hitBoxes { get; protected set; }

  private void Start()
  {
    this.creature = this.GetComponentInParent<Creature>();
    this.hitBoxes = this.creature.GetComponentsInChildren<CreatureAbilityHitbox>(true);
    foreach (CreatureAbilityHitbox hitBox in this.hitBoxes)
      this.hitboxDictionary.Add(hitBox.name, hitBox);
  }

  public void EnableHitbox(string hitboxesString)
  {
    foreach (string str in hitboxesString.Split(",", StringSplitOptions.None))
      this.hitboxDictionary[str.Replace(" ", "")]?.EnableHitBox(this.ability);
  }

  public void DisableHitbox(string hitboxesString)
  {
    foreach (string str in hitboxesString.Split(",", StringSplitOptions.None))
      this.hitboxDictionary[str.Replace(" ", "")]?.DisableHitBox();
  }

  public void DisableAllHitBoxes()
  {
    foreach (CreatureAbilityHitbox hitBox in this.hitBoxes)
      hitBox.DisableHitBox();
  }

  public virtual void PlaySyncedVoiceClip(string id)
  {
  }
}
