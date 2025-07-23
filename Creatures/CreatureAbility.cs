// Decompiled with JetBrains decompiler
// Type: ThunderRoad.CreatureAbility
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[Serializable]
public class CreatureAbility : ScriptableObject
{
  public bool multiHit = true;
  public float contactDamage;
  public bool breakBreakables;
  public float contactForce;
  public bool forceUngrip;

  public Creature creature { get; protected set; }

  public virtual void Setup(Creature creature) => this.creature = creature;
}
