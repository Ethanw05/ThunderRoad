// Decompiled with JetBrains decompiler
// Type: ThunderRoad.KinematicVelocityTrackerExtensions
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public static class KinematicVelocityTrackerExtensions
{
  public static VelocityTracker GetVelocityTracker(this Rigidbody rigidbody)
  {
    VelocityTracker component;
    return !rigidbody.gameObject.TryGetOrAddComponent<VelocityTracker>(out component) ? (VelocityTracker) null : component;
  }
}
