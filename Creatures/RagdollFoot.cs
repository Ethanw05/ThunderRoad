// Decompiled with JetBrains decompiler
// Type: ThunderRoad.RagdollFoot
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Creatures/Ragdoll foot")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RagdollFoot")]
public class RagdollFoot : RagdollPart
{
  public Side side;
  public Transform grip;
  [Tooltip("detected automatically for humanoid")]
  [Header("Bones (non-humanoid creature only)")]
  public Transform upperLegBone;
  [Tooltip("detected automatically for humanoid")]
  public Transform lowerLegBone;
  [Tooltip("detected automatically for humanoid")]
  public Transform toesBone;
  [NonSerialized]
  public Transform toesAnchor;
  [NonSerialized]
  public PlayerFoot playerFoot;

  protected override void OnValidate()
  {
    base.OnValidate();
    if (!this.gameObject.activeInHierarchy)
      return;
    this.grip = this.transform.Find("Grip");
    if ((bool) (UnityEngine.Object) this.grip)
      return;
    this.grip = new GameObject("Grip").transform;
    this.grip.SetParent(this.transform);
  }

  protected override void OnDrawGizmosSelected()
  {
    base.OnDrawGizmosSelected();
    if (!(bool) (UnityEngine.Object) this.grip)
      return;
    Gizmos.matrix = this.grip.transform.localToWorldMatrix;
    Common.DrawGizmoArrow(UnityEngine.Vector3.zero, UnityEngine.Vector3.forward * 0.05f, (UnityEngine.Color) Common.HueColourValue(HueColorName.Purple), 0.05f, 10f);
    Common.DrawGizmoArrow(UnityEngine.Vector3.zero, UnityEngine.Vector3.up * 0.025f, (UnityEngine.Color) Common.HueColourValue(HueColorName.Green), 0.025f);
  }

  public virtual void Init()
  {
    if ((bool) (UnityEngine.Object) this.toesBone)
      this.toesBone = this.ragdoll.GetBone(this.toesBone).animation;
    if ((bool) (UnityEngine.Object) this.upperLegBone)
      this.upperLegBone = this.ragdoll.GetBone(this.upperLegBone).animation;
    if ((bool) (UnityEngine.Object) this.lowerLegBone)
      this.lowerLegBone = this.ragdoll.GetBone(this.lowerLegBone).animation;
    if (this.side == Side.Left)
    {
      if (!(bool) (UnityEngine.Object) this.toesBone)
        this.toesBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftToes);
      if (!(bool) (UnityEngine.Object) this.upperLegBone)
        this.upperLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
      if (!(bool) (UnityEngine.Object) this.lowerLegBone)
        this.lowerLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
    }
    else if (this.side == Side.Right)
    {
      if (!(bool) (UnityEngine.Object) this.toesBone)
        this.toesBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightToes);
      if (!(bool) (UnityEngine.Object) this.upperLegBone)
        this.upperLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
      if (!(bool) (UnityEngine.Object) this.lowerLegBone)
        this.lowerLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
    }
    if (!(bool) (UnityEngine.Object) this.toesBone)
      return;
    this.toesAnchor = new GameObject("ToesAnchor").transform;
    this.toesAnchor.SetParent(this.transform);
    this.toesAnchor.position = this.toesBone.position;
    this.toesAnchor.rotation = this.toesBone.rotation;
  }

  public float GetCurrentLegDistance(Space space)
  {
    return space == Space.Self ? UnityEngine.Vector3.Distance(this.ragdoll.creature.transform.InverseTransformPoint((bool) (UnityEngine.Object) this.upperLegBone ? this.upperLegBone.position : this.lowerLegBone.position), this.ragdoll.creature.transform.InverseTransformPoint(this.bone.animation.position)) : UnityEngine.Vector3.Distance((bool) (UnityEngine.Object) this.upperLegBone ? this.upperLegBone.position : this.lowerLegBone.position, this.bone.animation.position);
  }

  public float GetLegLenght(Space space)
  {
    return space != Space.Self ? this.ragdoll.creature.morphology.legsLength * this.ragdoll.creature.transform.localScale.y : this.ragdoll.creature.morphology.legsLength;
  }

  public override void RefreshLayer()
  {
    if ((bool) (UnityEngine.Object) this.playerFoot && (bool) this.playerFoot.footTracker)
      this.SetLayer(LayerName.ItemAndRagdollOnly);
    else
      base.RefreshLayer();
  }
}
