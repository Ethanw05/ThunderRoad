// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemSpray
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

[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Magic spray config")]
[Serializable]
public class GolemSpray : GolemAbility
{
  public string spraySkillID;
  public List<string> spraySources = new List<string>();
  public GolemController.AttackMotion sprayMotion = GolemController.AttackMotion.Spray;
  public float sprayAngle = 90f;
  [NonSerialized]
  public IGolemSprayable spraySkillData;
  protected System.Action endSpray;

  public List<ValueDropdownItem<string>> GetValidSkills()
  {
    List<ValueDropdownItem<string>> validSkills = new List<ValueDropdownItem<string>>()
    {
      new ValueDropdownItem<string>("None", "")
    };
    if (!Catalog.IsJsonLoaded())
      return validSkills;
    foreach (SkillData data in Catalog.GetDataList<SkillData>())
    {
      if (data is IGolemSprayable)
        validSkills.Add(new ValueDropdownItem<string>(data.id, data.id));
    }
    return validSkills;
  }

  public List<Transform> sprayPoints { get; protected set; }

  public override bool Allow(GolemController golem)
  {
    return base.Allow(golem) && golem.lastAttackMotion != this.sprayMotion && !(golem.lastAbility is GolemSpray) && (double) Mathf.Abs(UnityEngine.Vector3.SignedAngle(golem.transform.forward, (golem.attackTarget.position.ToXZ() - golem.transform.position.ToXZ()).normalized, golem.transform.up)) < (double) this.sprayAngle / 2.0;
  }

  public override void Begin(GolemController golem)
  {
    base.Begin(golem);
    SkillData data = Catalog.GetData<SkillData>(this.spraySkillID);
    if (!(data is IGolemSprayable))
    {
      Debug.LogError((object) "Can't use a spell which is not configured as a golem sprayable!");
    }
    else
    {
      this.spraySkillData = data as IGolemSprayable;
      this.sprayPoints = new List<Transform>();
      foreach (Transform magicSprayPoint in golem.magicSprayPoints)
      {
        if (this.spraySources.Contains(magicSprayPoint.name))
          this.sprayPoints.Add(magicSprayPoint);
      }
      golem.PerformAttackMotion(this.sprayMotion, new System.Action(((GolemAbility) this).End));
    }
  }

  public override void AbilityStep(int step)
  {
    base.AbilityStep(step);
    if (step != 1)
    {
      if (step != 2)
        return;
      this.EndSpray();
    }
    else
      this.StartSpray();
  }

  public override void Interrupt()
  {
    base.Interrupt();
    this.EndSpray();
  }

  public override void OnEnd()
  {
    base.OnEnd();
    this.EndSpray();
  }

  protected void StartSpray() => this.spraySkillData.GolemSprayStart(this, out this.endSpray);

  protected void EndSpray()
  {
    System.Action endSpray = this.endSpray;
    if (endSpray != null)
      endSpray();
    this.endSpray = (System.Action) null;
  }
}
