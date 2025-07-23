// Decompiled with JetBrains decompiler
// Type: ThunderRoad.AnimationFxPlayer
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class AnimationFxPlayer : MonoBehaviour
{
  public FxController[] controllers;

  public event AnimationFxPlayer.PlayEvent OnPlayEvent;

  public void Awake()
  {
    if (this.controllers != null)
      return;
    this.controllers = this.GetComponentsInChildren<FxController>();
  }

  public void Play(int index)
  {
    if (index > this.controllers.Length)
      return;
    this.controllers[index].Play();
    AnimationFxPlayer.PlayEvent onPlayEvent = this.OnPlayEvent;
    if (onPlayEvent == null)
      return;
    onPlayEvent(this, index, this.controllers[index]);
  }

  public delegate void PlayEvent(AnimationFxPlayer player, int index, FxController controller);
}
