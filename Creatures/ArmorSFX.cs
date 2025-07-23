// Decompiled with JetBrains decompiler
// Type: ThunderRoad.ArmorSFX
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class ArmorSFX : ThunderBehaviour
{
  private Creature creature;
  private Footstep footStep;
  private float timeSinceLastPlayed;
  private float blockTime = 0.1f;
  private int effectPrio;
  private EffectData armorSFXData;

  private void Awake()
  {
    this.creature = this.GetComponent<Creature>();
    this.creature.OnDataLoaded += new Creature.SimpleDelegate(this.OnCreatureDataLoaded);
    this.footStep = this.creature.GetComponentInChildren<Footstep>();
    this.footStep.OnStep += new Footstep.StepEvent(this.PlayFootStepArmorFX);
    this.creature.OnThisCreatureAttackEvent += new Creature.ThisCreatureAttackEvent(this.OnThisCreatureAttackEvent);
  }

  private void OnDestroy()
  {
    this.creature.OnThisCreatureAttackEvent -= new Creature.ThisCreatureAttackEvent(this.OnThisCreatureAttackEvent);
    this.footStep.OnStep -= new Footstep.StepEvent(this.PlayFootStepArmorFX);
  }

  private void OnCreatureDataLoaded() => this.CalculateEffectData();

  private void OnThisCreatureAttackEvent(
    Creature targetCreature,
    Transform targetTransform,
    BrainModuleAttack.AttackType type,
    BrainModuleAttack.AttackStage stage)
  {
    if (stage != BrainModuleAttack.AttackStage.Attack && stage != BrainModuleAttack.AttackStage.WindUp && stage != BrainModuleAttack.AttackStage.FollowThrough)
      return;
    float intensity = 1f;
    switch (stage)
    {
      case BrainModuleAttack.AttackStage.WindUp:
        intensity = 1f;
        break;
      case BrainModuleAttack.AttackStage.Attack:
        intensity = 1f;
        break;
      case BrainModuleAttack.AttackStage.FollowThrough:
        intensity = 0.5f;
        break;
    }
    this.PlaySFX(this.creature.transform.position, intensity);
  }

  public void PlayFootStepArmorFX(UnityEngine.Vector3 position, Side side, float velocity)
  {
    if (this.effectPrio <= 1)
      return;
    this.PlaySFX(position, velocity);
  }

  private void PlaySFX(UnityEngine.Vector3 position, float intensity)
  {
    if (!(bool) (UnityEngine.Object) Player.local || this.effectPrio < 1 || this.armorSFXData == null || (double) Time.time - (double) this.timeSinceLastPlayed < (double) this.blockTime)
      return;
    EffectInstance effectInstance = EffectInstance.Spawn(this.armorSFXData, position, Quaternion.LookRotation(UnityEngine.Vector3.forward), intensity, 0.0f, (Transform) null, (CollisionInstance) null, true, (ColliderGroup) null, true);
    effectInstance.SetNoise(true);
    effectInstance.source = (object) this.creature;
    effectInstance.Play();
    this.timeSinceLastPlayed = Time.time;
  }

  public ItemContent[] GetCorePartContents()
  {
    List<string> stringList = new List<string>()
    {
      "Torso",
      "Legs",
      "Feet"
    };
    List<ItemContent> itemContentList = new List<ItemContent>();
    foreach (ItemContent itemContent in this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content => content.HasState<ContentStateWorn>())))
    {
      ItemModuleWardrobe module;
      ItemModuleWardrobe.CreatureWardrobe wardrobe;
      if (itemContent.data.TryGetModule<ItemModuleWardrobe>(out module) && module.TryGetWardrobe(this.creature, out wardrobe))
      {
        if (itemContent.data.id == "SantaHat")
        {
          itemContentList.Add(itemContent);
        }
        else
        {
          for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
          {
            string channel = wardrobe.manikinWardrobeData.channels[index];
            int layer = wardrobe.manikinWardrobeData.layers[index];
            if ((!(channel == "Torso") || layer != 0) && stringList.Contains(channel))
            {
              itemContentList.Add(itemContent);
              break;
            }
          }
        }
      }
    }
    return itemContentList.ToArray();
  }

  public void CalculateEffectData()
  {
    ItemContent[] corePartContents = this.GetCorePartContents();
    int num = -1;
    string str = "";
    this.armorSFXData = (EffectData) null;
    foreach (ContainerContent<ItemData, ItemContent> containerContent in corePartContents)
    {
      ItemModuleWardrobe module;
      if (containerContent.data.TryGetModule<ItemModuleWardrobe>(out module) && !module.armorSoundEffectID.IsNullOrEmptyOrWhitespace() && module.armorSoundEffectPriority > num)
      {
        num = module.armorSoundEffectPriority;
        str = module.armorSoundEffectID;
      }
    }
    this.effectPrio = num;
    if (str.IsNullOrEmptyOrWhitespace())
      return;
    this.armorSFXData = Catalog.GetData<EffectData>(str);
  }
}
