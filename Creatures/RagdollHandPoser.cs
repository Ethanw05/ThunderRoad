// Decompiled with JetBrains decompiler
// Type: ThunderRoad.RagdollHandPoser
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (RagdollHand))]
[AddComponentMenu("ThunderRoad/Creatures/Ragdoll hand poser")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RagdollHandPoser")]
public class RagdollHandPoser : ThunderBehaviour
{
  public RagdollHand ragdollHand;
  [CatalogPicker(new Category[] {Category.HandPose})]
  public string defaultHandPoseId = "DefaultOpen";
  [Range(0.0f, 1f)]
  public float targetWeight;
  [CatalogPicker(new Category[] {Category.HandPose})]
  public string targetHandPoseId = "DefaultClose";
  public bool globalRatio = true;
  [Range(0.0f, 1f)]
  public float thumbCloseWeight;
  [Range(0.0f, 1f)]
  public float indexCloseWeight;
  [Range(0.0f, 1f)]
  public float middleCloseWeight;
  [Range(0.0f, 1f)]
  public float ringCloseWeight;
  [Range(0.0f, 1f)]
  public float littleCloseWeight;
  public bool allowThumbTracking = true;
  public bool allowIndexTracking = true;
  public bool allowMiddleTracking = true;
  public bool allowRingTracking = true;
  public bool allowLittleTracking = true;
  public HandPoseData.Pose.MirrorParams mirrorParams;
  [NonSerialized]
  public HandPoseData defaultHandPoseData;
  [NonSerialized]
  public HandPoseData.Pose.Fingers defaultHandPoseFingers;
  [NonSerialized]
  public HandPoseData targetHandPoseData;
  [NonSerialized]
  public HandPoseData.Pose.Fingers targetHandPoseFingers;
  [NonSerialized]
  public bool hasTargetHandPose;
  public bool poseComplete;

  public void UpdatePoseThumb(float targetWeight)
  {
    this.thumbCloseWeight = targetWeight;
    if (this.hasTargetHandPose)
      this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb, this.targetHandPoseFingers.thumb, targetWeight);
    else
      this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb);
  }

  public void UpdatePoseIndex(float targetWeight)
  {
    this.indexCloseWeight = targetWeight;
    if (this.hasTargetHandPose)
      this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index, this.targetHandPoseFingers.index, targetWeight);
    else
      this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index);
  }

  public void UpdatePoseMiddle(float targetWeight)
  {
    this.middleCloseWeight = targetWeight;
    if (this.hasTargetHandPose)
      this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle, this.targetHandPoseFingers.middle, targetWeight);
    else
      this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle);
  }

  public void UpdatePoseRing(float targetWeight)
  {
    this.ringCloseWeight = targetWeight;
    if (this.hasTargetHandPose)
      this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring, this.targetHandPoseFingers.ring, targetWeight);
    else
      this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring);
  }

  public void UpdatePoseLittle(float targetWeight)
  {
    this.littleCloseWeight = targetWeight;
    if (this.hasTargetHandPose)
      this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little, this.targetHandPoseFingers.little, targetWeight);
    else
      this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little);
  }

  public void UpdatePose(HandPoseData.FingerType finger, float weight)
  {
    this.UpdateFinger(this.ragdollHand.GetFinger(finger), this.defaultHandPoseFingers.GetFinger(finger));
  }

  public virtual void UpdateFinger(
    RagdollHand.Finger finger,
    HandPoseData.Pose.Finger defaultHandPoseFingers,
    HandPoseData.Pose.Finger targetHandPoseFingers,
    float targetWeight)
  {
    HandPoseData.Pose.Finger.Bone proximal = defaultHandPoseFingers.proximal;
    UnityEngine.Vector3 position = UnityEngine.Vector3.Lerp(proximal.localPosition, targetHandPoseFingers.proximal.localPosition, targetWeight);
    Quaternion localRotation1 = Quaternion.Lerp(proximal.localRotation, targetHandPoseFingers.proximal.localRotation, targetWeight);
    finger.proximal.collider.transform.SetPositionAndRotation(this.ragdollHand.transform.TransformPoint(position), this.ragdollHand.transform.TransformRotation(localRotation1));
    UnityEngine.Vector3 localPosition1 = UnityEngine.Vector3.Lerp(defaultHandPoseFingers.intermediate.localPosition, targetHandPoseFingers.intermediate.localPosition, targetWeight);
    Quaternion localRotation2 = Quaternion.Lerp(defaultHandPoseFingers.intermediate.localRotation, targetHandPoseFingers.intermediate.localRotation, targetWeight);
    finger.intermediate.collider.transform.SetLocalPositionAndRotation(localPosition1, localRotation2);
    UnityEngine.Vector3 localPosition2 = UnityEngine.Vector3.Lerp(defaultHandPoseFingers.distal.localPosition, targetHandPoseFingers.distal.localPosition, targetWeight);
    Quaternion localRotation3 = Quaternion.Lerp(defaultHandPoseFingers.distal.localRotation, targetHandPoseFingers.distal.localRotation, targetWeight);
    finger.distal.collider.transform.SetLocalPositionAndRotation(localPosition2, localRotation3);
  }

  public virtual void UpdateFinger(
    RagdollHand.Finger finger,
    HandPoseData.Pose.Finger defaultHandPoseFingers)
  {
    finger.proximal.collider.transform.SetLocalPositionAndRotation(defaultHandPoseFingers.proximal.localPosition, defaultHandPoseFingers.proximal.localRotation);
    finger.intermediate.collider.transform.SetLocalPositionAndRotation(defaultHandPoseFingers.intermediate.localPosition, defaultHandPoseFingers.intermediate.localRotation);
    finger.distal.collider.transform.SetLocalPositionAndRotation(defaultHandPoseFingers.distal.localPosition, defaultHandPoseFingers.distal.localRotation);
  }

  public void SetGripFromPose(HandPoseData handPoseData)
  {
    if (handPoseData == null)
    {
      if (this.defaultHandPoseData == null)
        this.ResetDefaultPose();
      handPoseData = this.defaultHandPoseData;
      if (handPoseData == null)
        return;
    }
    HandPoseData.Pose creaturePose = handPoseData.GetCreaturePose(this.ragdollHand.creature);
    if (creaturePose != null)
    {
      Transform grip = this.ragdollHand.grip;
      HandPoseData.Pose.Fingers fingers = creaturePose.GetFingers(this.ragdollHand.side);
      grip.SetLocalPositionAndRotation(fingers.gripLocalPosition, fingers.gripLocalRotation);
      grip.localScale = UnityEngine.Vector3.one;
    }
    else
      Debug.LogError((object) $"Could not find creature pose {handPoseData.id} for {this.ragdollHand.creature.data.name}");
  }

  public void SetDefaultPose(HandPoseData handPoseData)
  {
    if (handPoseData == null)
    {
      this.ResetDefaultPose();
    }
    else
    {
      this.defaultHandPoseData = handPoseData;
      this.defaultHandPoseFingers = this.defaultHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
    }
  }

  public void ResetDefaultPose()
  {
    this.defaultHandPoseData = Catalog.GetData<HandPoseData>(this.defaultHandPoseId);
    this.defaultHandPoseFingers = this.defaultHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
  }

  public void SetTargetWeight(float weight, bool lerpFingers = false)
  {
    this.targetWeight = weight;
    if (lerpFingers)
    {
      float ragdollDataFingerSpeed = this.ragdollHand.ragdoll.creature.data.ragdollData.fingerSpeed;
      float deltaTime = this.ragdollHand.ragdoll.creature.isPlayer ? Time.unscaledDeltaTime : Time.deltaTime;
      UpdateFingerLerp(ref this.thumbCloseWeight, new Action<float>(this.UpdatePoseThumb));
      UpdateFingerLerp(ref this.indexCloseWeight, new Action<float>(this.UpdatePoseIndex));
      UpdateFingerLerp(ref this.middleCloseWeight, new Action<float>(this.UpdatePoseMiddle));
      UpdateFingerLerp(ref this.ringCloseWeight, new Action<float>(this.UpdatePoseRing));
      UpdateFingerLerp(ref this.littleCloseWeight, new Action<float>(this.UpdatePoseLittle));

      void UpdateFingerLerp(ref float closeWeight, Action<float> updatePoseFunc)
      {
        closeWeight = Mathf.MoveTowards(closeWeight, weight, ragdollDataFingerSpeed * deltaTime);
        updatePoseFunc(closeWeight);
      }
    }
    else
    {
      this.UpdatePoseThumb(this.targetWeight);
      this.UpdatePoseIndex(this.targetWeight);
      this.UpdatePoseMiddle(this.targetWeight);
      this.UpdatePoseRing(this.targetWeight);
      this.UpdatePoseLittle(this.targetWeight);
    }
  }

  public void SetTargetPose(
    HandPoseData handPoseData,
    bool allowThumbTracking = false,
    bool allowIndexTracking = false,
    bool allowMiddleTracking = false,
    bool allowRingTracking = false,
    bool allowLittleTracking = false)
  {
    if (handPoseData == null)
    {
      this.ResetTargetPose();
    }
    else
    {
      this.targetHandPoseData = handPoseData;
      if (this.targetHandPoseData == null)
      {
        this.targetHandPoseFingers = (HandPoseData.Pose.Fingers) null;
        this.hasTargetHandPose = false;
        this.allowThumbTracking = this.allowIndexTracking = this.allowMiddleTracking = this.allowRingTracking = this.allowLittleTracking = false;
      }
      else
      {
        this.targetHandPoseFingers = this.targetHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
        this.hasTargetHandPose = true;
        this.allowThumbTracking = allowThumbTracking;
        this.allowIndexTracking = allowIndexTracking;
        this.allowMiddleTracking = allowMiddleTracking;
        this.allowRingTracking = allowRingTracking;
        this.allowLittleTracking = allowLittleTracking;
      }
    }
  }

  public void ResetTargetPose()
  {
    this.targetHandPoseData = Catalog.GetData<HandPoseData>(this.targetHandPoseId);
    if (this.targetHandPoseData == null)
    {
      this.targetHandPoseFingers = (HandPoseData.Pose.Fingers) null;
      this.hasTargetHandPose = false;
      this.allowThumbTracking = this.allowIndexTracking = this.allowMiddleTracking = this.allowRingTracking = this.allowLittleTracking = false;
    }
    else
    {
      this.targetHandPoseFingers = this.targetHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
      this.hasTargetHandPose = true;
      this.allowThumbTracking = this.allowIndexTracking = this.allowMiddleTracking = this.allowRingTracking = this.allowLittleTracking = true;
    }
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    if (!this.ragdollHand.initialized || !this.ragdollHand.ragdoll.creature.isPlayer)
      return;
    this.poseComplete = true;
    if (this.hasTargetHandPose)
    {
      float deltaTime = Time.deltaTime;
      PlayerControl.Hand hand = PlayerControl.GetHand(this.ragdollHand.side);
      float fingerSpeed = this.ragdollHand.ragdoll.creature.data.ragdollData.fingerSpeed;
      bool isGripping = this.ragdollHand.climb.isGripping;
      if (this.allowThumbTracking && !this.ragdollHand.climb.thumbContact)
      {
        float num = isGripping ? 1f : hand.thumbCurl;
        this.thumbCloseWeight = Mathf.MoveTowards(this.thumbCloseWeight, num, fingerSpeed * deltaTime);
        if (!Mathf.Approximately(this.thumbCloseWeight, num))
        {
          this.poseComplete = false;
          this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb, this.targetHandPoseFingers.thumb, this.thumbCloseWeight);
        }
      }
      if (this.allowIndexTracking && !this.ragdollHand.climb.indexContact)
      {
        float num = isGripping ? 1f : hand.indexCurl;
        this.indexCloseWeight = Mathf.MoveTowards(this.indexCloseWeight, num, fingerSpeed * deltaTime);
        if (!Mathf.Approximately(this.indexCloseWeight, num))
        {
          this.poseComplete = false;
          this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index, this.targetHandPoseFingers.index, this.indexCloseWeight);
        }
      }
      if (this.allowMiddleTracking && !this.ragdollHand.climb.middleContact)
      {
        float num = isGripping ? 1f : hand.middleCurl;
        this.middleCloseWeight = Mathf.MoveTowards(this.middleCloseWeight, num, fingerSpeed * deltaTime);
        if (!Mathf.Approximately(this.middleCloseWeight, num))
        {
          this.poseComplete = false;
          this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle, this.targetHandPoseFingers.middle, this.middleCloseWeight);
        }
      }
      if (this.allowRingTracking && !this.ragdollHand.climb.ringContact)
      {
        float num = isGripping ? 1f : hand.ringCurl;
        this.ringCloseWeight = Mathf.MoveTowards(this.ringCloseWeight, num, fingerSpeed * deltaTime);
        if (!Mathf.Approximately(this.ringCloseWeight, num))
        {
          this.poseComplete = false;
          this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring, this.targetHandPoseFingers.ring, this.ringCloseWeight);
        }
      }
      if (!this.allowLittleTracking || this.ragdollHand.climb.littleContact)
        return;
      float num1 = isGripping ? 1f : hand.littleCurl;
      this.littleCloseWeight = Mathf.MoveTowards(this.littleCloseWeight, num1, fingerSpeed * deltaTime);
      if (Mathf.Approximately(this.littleCloseWeight, num1))
        return;
      this.poseComplete = false;
      this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little, this.targetHandPoseFingers.little, this.littleCloseWeight);
    }
    else
    {
      if (this.allowThumbTracking && !this.ragdollHand.climb.thumbContact)
        this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb);
      if (this.allowIndexTracking && !this.ragdollHand.climb.indexContact)
        this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index);
      if (this.allowMiddleTracking && !this.ragdollHand.climb.middleContact)
        this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle);
      if (this.allowRingTracking && !this.ragdollHand.climb.ringContact)
        this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring);
      if (!this.allowLittleTracking || this.ragdollHand.climb.littleContact)
        return;
      this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little);
    }
  }

  protected void OnDrawGizmosSelected()
  {
    Gizmos.DrawWireSphere(this.ragdollHand.grip.position, this.ragdollHand.collisionUngrabRadius);
    foreach (RagdollHand.Finger finger in this.ragdollHand.fingers)
    {
      Gizmos.color = UnityEngine.Color.gray;
      Gizmos.DrawWireSphere(finger.distal.collider.transform.position, 1f / 1000f);
      Gizmos.DrawWireSphere(finger.intermediate.collider.transform.position, 1f / 1000f);
      Gizmos.DrawWireSphere(finger.proximal.collider.transform.position, 1f / 1000f);
      Gizmos.DrawWireSphere(finger.tip.position, 1f / 1000f);
      Gizmos.DrawLine(this.transform.position, finger.proximal.collider.transform.position);
      Gizmos.DrawLine(finger.proximal.collider.transform.position, finger.intermediate.collider.transform.position);
      Gizmos.DrawLine(finger.intermediate.collider.transform.position, finger.distal.collider.transform.position);
      Gizmos.DrawLine(finger.distal.collider.transform.position, finger.tip.position);
      Gizmos.color = UnityEngine.Color.blue;
      Gizmos.DrawRay(finger.tip.position, finger.tip.forward * 0.01f);
      Gizmos.color = UnityEngine.Color.green;
      Gizmos.DrawRay(finger.tip.position, finger.tip.up * 0.01f);
    }
    if (!(bool) (UnityEngine.Object) this.ragdollHand.grip)
      return;
    Gizmos.matrix = this.ragdollHand.grip.localToWorldMatrix;
    Gizmos.color = (UnityEngine.Color) Common.HueColourValue(HueColorName.Purple);
    Gizmos.DrawWireCube(new UnityEngine.Vector3(0.0f, 0.0f, 0.0f), new UnityEngine.Vector3(0.01f, 0.05f, 0.01f));
    Gizmos.DrawWireCube(new UnityEngine.Vector3(0.0f, 0.03f, 0.01f), new UnityEngine.Vector3(0.01f, 0.01f, 0.03f));
  }

  public float GetCloseWeight(HandPoseData.FingerType type)
  {
    switch (type)
    {
      case HandPoseData.FingerType.Thumb:
        return this.thumbCloseWeight;
      case HandPoseData.FingerType.Index:
        return this.indexCloseWeight;
      case HandPoseData.FingerType.Middle:
        return this.middleCloseWeight;
      case HandPoseData.FingerType.Ring:
        return this.ringCloseWeight;
      case HandPoseData.FingerType.Little:
        return this.littleCloseWeight;
      default:
        throw new ArgumentOutOfRangeException(nameof (type), (object) type, (string) null);
    }
  }
}
