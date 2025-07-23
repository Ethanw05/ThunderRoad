// Decompiled with JetBrains decompiler
// Type: ThunderRoad.WristRelaxer
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Creatures/Wrist relaxer")]
public class WristRelaxer : MonoBehaviour
{
  public Transform armTwistBone;
  public Transform upperArmBone;
  public Transform lowerArmBone;
  public Transform handBone;
  [Tooltip("The weight of relaxing the twist")]
  [Range(0.0f, 1f)]
  public float weight = 1f;
  [Tooltip("The weight of relaxing the arm of this Transform (UMA)")]
  [Range(0.0f, 1f)]
  public float weightArm = 0.5f;
  [Tooltip("If 0.5, will be twisted half way from parent to child. If 1, the twist angle will be locked to the child and will rotate with along with it.")]
  [Range(0.0f, 1f)]
  public float parentChildCrossfade = 0.5f;
  [Range(-180f, 180f)]
  [Tooltip("Rotation offset around the twist axis.")]
  public float twistAngleOffset;
  protected bool noTwistBone;
  protected RagdollHand ragdollHand;
  private Transform lowerArmBoneMesh;
  private UnityEngine.Vector3 twistAxis = UnityEngine.Vector3.right;
  private UnityEngine.Vector3 axis = UnityEngine.Vector3.forward;
  private UnityEngine.Vector3 axisRelativeToParentDefault;
  private UnityEngine.Vector3 axisRelativeToChildDefault;

  private void OnValidate()
  {
    if (!this.gameObject.activeInHierarchy || (bool) (Object) this.upperArmBone && (bool) (Object) this.lowerArmBone && (bool) (Object) this.handBone && (bool) (Object) this.armTwistBone)
      return;
    RagdollHand componentInParent1 = this.GetComponentInParent<RagdollHand>();
    Creature componentInParent2 = this.GetComponentInParent<Creature>();
    if (!(bool) (Object) componentInParent2 || !(bool) (Object) componentInParent2.animator || !(bool) (Object) componentInParent1)
      return;
    if (!(bool) (Object) this.upperArmBone)
      this.upperArmBone = componentInParent2.animator.GetBoneTransform(componentInParent1.side == Side.Right ? HumanBodyBones.RightUpperArm : HumanBodyBones.LeftUpperArm);
    if (!(bool) (Object) this.lowerArmBone)
      this.lowerArmBone = componentInParent2.animator.GetBoneTransform(componentInParent1.side == Side.Right ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm);
    if (!(bool) (Object) this.armTwistBone)
      this.armTwistBone = componentInParent2.animator.GetBoneTransform(componentInParent1.side == Side.Right ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm).GetChild(0);
    if ((bool) (Object) this.handBone)
      return;
    this.handBone = componentInParent2.animator.GetBoneTransform(componentInParent1.side == Side.Right ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
  }

  protected void Awake() => this.ragdollHand = this.GetComponentInParent<RagdollHand>();

  public void Init()
  {
    this.ragdollHand.creature.ragdoll.ik.OnPostIKUpdateEvent += new IkController.LateUpdateDelegate(this.OnPostIKUpdate);
    this.armTwistBone = this.ragdollHand.ragdoll.GetBone(this.armTwistBone).animation;
    this.upperArmBone = this.ragdollHand.ragdoll.GetBone(this.upperArmBone).animation;
    this.lowerArmBoneMesh = this.lowerArmBone;
    this.lowerArmBone = this.ragdollHand.ragdoll.GetBone(this.lowerArmBone).animation;
    this.handBone = this.ragdollHand.ragdoll.GetBone(this.handBone).animation;
    this.twistAxis = this.lowerArmBone.InverseTransformDirection(this.handBone.position - this.lowerArmBone.position);
    this.axis = new UnityEngine.Vector3(this.twistAxis.y, this.twistAxis.z, this.twistAxis.x);
    UnityEngine.Vector3 vector3 = this.lowerArmBone.rotation * this.axis;
    this.axisRelativeToParentDefault = Quaternion.Inverse(this.upperArmBone.rotation) * vector3;
    this.axisRelativeToChildDefault = Quaternion.Inverse(this.handBone.rotation) * vector3;
    if (!((Object) this.armTwistBone == (Object) this.handBone))
      return;
    Debug.LogWarningFormat((Object) this, "No twist bone found on skeleton, wristRelaxer cannot work correctly when grabbing objects");
    this.noTwistBone = true;
  }

  private void OnPostIKUpdate()
  {
    if (!(bool) (Object) this.ragdollHand || (double) this.weight <= 0.0 || this.ragdollHand.isSliced || !this.gameObject.activeInHierarchy || this.noTwistBone && (bool) (Object) this.ragdollHand.grabbedHandle)
      return;
    Quaternion rotation = this.lowerArmBone.rotation;
    Quaternion quaternion1 = Quaternion.AngleAxis(this.twistAngleOffset, rotation * this.twistAxis);
    Quaternion quaternion2 = quaternion1 * rotation;
    UnityEngine.Vector3 vector3_1 = UnityEngine.Vector3.Slerp(quaternion1 * this.upperArmBone.rotation * this.axisRelativeToParentDefault, quaternion1 * this.handBone.rotation * this.axisRelativeToChildDefault, this.parentChildCrossfade);
    UnityEngine.Vector3 vector3_2 = Quaternion.Inverse(Quaternion.LookRotation(quaternion2 * this.axis, quaternion2 * this.twistAxis)) * vector3_1;
    float num = Mathf.Atan2(vector3_2.x, vector3_2.z) * 57.29578f;
    this.armTwistBone.localRotation = Quaternion.AngleAxis(num * this.weight, this.twistAxis);
    this.lowerArmBoneMesh.localRotation = Quaternion.AngleAxis(num * this.weightArm, this.twistAxis);
  }
}
