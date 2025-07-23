// Decompiled with JetBrains decompiler
// Type: ThunderRoad.CreatureEye
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/CreatureEye.html")]
[AddComponentMenu("ThunderRoad/Creatures/Creature Eye")]
public class CreatureEye : MonoBehaviour
{
  [Range(0.0f, 1f)]
  public float closeAmount;
  [NonSerialized]
  public float lastUpdateTime;
  public string eyeTag = "";
  public List<CreatureEye.EyeMoveable> eyeParts = new List<CreatureEye.EyeMoveable>();

  public void SetKeyframes()
  {
    foreach (CreatureEye.EyeMoveable eyePart in this.eyeParts)
      eyePart.curves.SetKeyframeByRotation(eyePart.transform, this.closeAmount);
  }

  public void SetClose()
  {
    foreach (CreatureEye.EyeMoveable eyePart in this.eyeParts)
    {
      if ((!Application.isPlaying || eyePart.isFixed) && (bool) (UnityEngine.Object) eyePart.transform)
        eyePart.transform.localRotation = eyePart.curves.TimeToQuaternion(this.closeAmount);
    }
  }

  [Serializable]
  public class EyeMoveable
  {
    public string name;
    public Transform transform;
    public CreatureEye.EyeMoveable.RotationCurves curves;

    public bool isFixed { get; private set; }

    public void ParentingFix()
    {
      if (this.isFixed)
        return;
      this.isFixed = true;
      this.transform = this.transform.parent;
    }

    [Serializable]
    public class RotationCurves
    {
      public AnimationCurve closeCurveX;
      public AnimationCurve closeCurveY;
      public AnimationCurve closeCurveZ;
      public AnimationCurve closeCurveW;

      public Quaternion TimeToQuaternion(float t)
      {
        return new Quaternion(this.closeCurveX.Evaluate(t), this.closeCurveY.Evaluate(t), this.closeCurveZ.Evaluate(t), this.closeCurveW.Evaluate(t));
      }

      public void SetKeyframeByRotation(Transform pull, float t)
      {
        this.AddOrMoveKeyframe(this.closeCurveX, t, pull.localRotation.x);
        this.AddOrMoveKeyframe(this.closeCurveY, t, pull.localRotation.y);
        this.AddOrMoveKeyframe(this.closeCurveZ, t, pull.localRotation.z);
        this.AddOrMoveKeyframe(this.closeCurveW, t, pull.localRotation.w);
      }

      public void AddOrMoveKeyframe(AnimationCurve curve, float t, float v)
      {
        if (curve.AddKey(t, v) != -1)
          return;
        Keyframe keyframe = new Keyframe();
        foreach (Keyframe key in curve.keys)
        {
          if ((double) key.time == (double) t)
          {
            keyframe = key;
            break;
          }
        }
        keyframe.value = v;
      }

      public void ClearCurves()
      {
        this.closeCurveX = new AnimationCurve();
        this.closeCurveY = new AnimationCurve();
        this.closeCurveZ = new AnimationCurve();
        this.closeCurveW = new AnimationCurve();
      }
    }
  }
}
