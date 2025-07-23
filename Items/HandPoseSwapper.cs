// Decompiled with JetBrains decompiler
// Type: ThunderRoad.HandPoseSwapper
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (HandlePose))]
[AddComponentMenu("ThunderRoad/Hand Pose Swapper")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/HandPoseSwapper")]
public class HandPoseSwapper : MonoBehaviour
{
  public List<string> alternateHandPoseIDs = new List<string>();
  protected HandlePose handlePose;

  public void SetDefaultPoseByIndex(int newIDIndex)
  {
    this.SetDefaultPose(this.alternateHandPoseIDs[newIDIndex]);
  }

  public void SetDefaultPose(string newPoseID)
  {
    this.SetPoses(newPoseID, this.handlePose.targetHandPoseId);
  }

  public void SetTargetPoseByIndex(int newIDIndex)
  {
    this.SetTargetPose(this.alternateHandPoseIDs[newIDIndex]);
  }

  public void SetTargetPose(string newPoseID)
  {
    this.SetPoses(this.handlePose.defaultHandPoseId, newPoseID);
  }

  protected void Start() => this.handlePose = this.GetComponent<HandlePose>();

  protected void SetPoses(string defaultID, string targetID)
  {
    this.handlePose.defaultHandPoseId = defaultID;
    this.handlePose.targetHandPoseId = targetID;
    this.handlePose.LoadHandPosesData();
    if (this.handlePose.handle?.handlers?.Count.GetValueOrDefault() <= 0)
      return;
    foreach (RagdollHand handler in this.handlePose.handle.handlers)
    {
      handler.poser.SetDefaultPose(this.handlePose.defaultHandPoseData);
      handler.poser.SetTargetPose(this.handlePose.targetHandPoseData);
      handler.poser.SetTargetWeight(this.handlePose.targetWeight);
    }
  }
}
