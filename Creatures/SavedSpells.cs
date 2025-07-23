// Decompiled with JetBrains decompiler
// Type: ThunderRoad.SavedSpells
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using ThunderRoad.Skill.SpellPower;

#nullable disable
namespace ThunderRoad;

public struct SavedSpells(
  SpellCastData left,
  SpellCastData right,
  SpellTelekinesis tkLeft,
  SpellTelekinesis tkRight)
{
  public SpellCastData left = left;
  public SpellCastData right = right;
  public SpellTelekinesis tkLeft = tkLeft;
  public SpellTelekinesis tkRight = tkRight;
}
