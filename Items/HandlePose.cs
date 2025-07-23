// Decompiled with JetBrains decompiler
// Type: ThunderRoad.HandlePose
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/HandlePose.html")]
public class HandlePose : MonoBehaviour
{
  [Tooltip("References the handle this handpose is attached to.")]
  public Handle handle;
  [Tooltip("Depicts which hand this handpose directs to.")]
  public Side side;
  [CatalogPicker(new Category[] {Category.HandPose})]
  [Tooltip("ID of the handpose that is set to default if the target weight is zero.")]
  public string defaultHandPoseId = "HandleDefault";
  [Tooltip("A per-HandlePose override for the Handle's SpellOrbTarget")]
  public Transform spellOrbTarget;
  [NonSerialized]
  public HandPoseData defaultHandPoseData;
  protected HandPoseData.Pose defaultHandPose;
  [Range(0.0f, 1f)]
  [Tooltip("Blends the \"Default\" handpose and the \"Target\" handpose, allowing you to create more unique and fitting handposes without needing to create new ones.")]
  public float targetWeight;
  [NonSerialized]
  public float lastTargetWeight = -1f;
  [CatalogPicker(new Category[] {Category.HandPose})]
  [Tooltip("ID of the handpose that is used to blend against the default handpose. Handpose that is used if the target weight is one.")]
  public string targetHandPoseId;
  [NonSerialized]
  public HandPoseData targetHandPoseData;
  protected HandPoseData.Pose targetHandPose;

  private void Awake()
  {
    if ((bool) (UnityEngine.Object) this.handle)
      return;
    this.handle = this.GetComponentInParent<Handle>();
  }

  private void Start() => this.LoadHandPosesData();

  public void LoadHandPosesData()
  {
    this.defaultHandPoseData = Catalog.GetData<HandPoseData>(this.defaultHandPoseId) ?? Catalog.GetData<HandPoseData>("HandleDefault");
    this.targetHandPoseData = Catalog.GetData<HandPoseData>(this.targetHandPoseId) ?? Catalog.GetData<HandPoseData>("HandleDefault");
  }

  public void UpdateName()
  {
    if (!(bool) (UnityEngine.Object) this.handle)
      return;
    if (this.side == Side.Right)
    {
      this.name = "OrientRight" + ((UnityEngine.Object) this.handle.orientationDefaultRight == (UnityEngine.Object) this ? "_Default" : "");
    }
    else
    {
      if (this.side != Side.Left)
        return;
      this.name = "OrientLeft" + ((UnityEngine.Object) this.handle.orientationDefaultLeft == (UnityEngine.Object) this ? "_Default" : "");
    }
  }

  protected void GizmoDrawFinger(
    HandPoseData.Pose.Finger finger,
    HandPoseData.Pose.Finger finger2,
    Matrix4x4 matrix)
  {
    UnityEngine.Vector3 vector3_1 = finger.proximal.localPosition;
    Quaternion quaternion1 = finger.proximal.localRotation;
    UnityEngine.Vector3 vector3_2 = finger.intermediate.localPosition;
    Quaternion quaternion2 = finger.intermediate.localRotation;
    UnityEngine.Vector3 vector3_3 = finger.distal.localPosition;
    Quaternion quaternion3 = finger.distal.localRotation;
    UnityEngine.Vector3 vector3_4 = finger.tipLocalPosition;
    if (finger2 != null)
    {
      vector3_1 = UnityEngine.Vector3.Lerp(finger.proximal.localPosition, finger2.proximal.localPosition, this.targetWeight);
      quaternion1 = Quaternion.Lerp(finger.proximal.localRotation, finger2.proximal.localRotation, this.targetWeight);
      vector3_2 = UnityEngine.Vector3.Lerp(finger.intermediate.localPosition, finger2.intermediate.localPosition, this.targetWeight);
      quaternion2 = Quaternion.Lerp(finger.intermediate.localRotation, finger2.intermediate.localRotation, this.targetWeight);
      vector3_3 = UnityEngine.Vector3.Lerp(finger.distal.localPosition, finger2.distal.localPosition, this.targetWeight);
      quaternion3 = Quaternion.Lerp(finger.distal.localRotation, finger2.distal.localRotation, this.targetWeight);
      vector3_4 = UnityEngine.Vector3.Lerp(finger.tipLocalPosition, finger2.tipLocalPosition, this.targetWeight);
    }
    Gizmos.matrix = matrix;
    Gizmos.DrawWireSphere(vector3_1, 3f / 500f);
    if (this.CheckQuaternion(quaternion1))
    {
      Gizmos.matrix *= Matrix4x4.TRS(vector3_1, quaternion1, UnityEngine.Vector3.one);
      Gizmos.DrawWireSphere(vector3_2, 0.005f);
      Gizmos.DrawLine(UnityEngine.Vector3.zero, vector3_2);
    }
    if (this.CheckQuaternion(quaternion2))
    {
      Gizmos.matrix *= Matrix4x4.TRS(vector3_2, quaternion2, UnityEngine.Vector3.one);
      Gizmos.DrawWireSphere(vector3_3, 3f / 1000f);
      Gizmos.DrawLine(UnityEngine.Vector3.zero, vector3_3);
    }
    if (!this.CheckQuaternion(quaternion3))
      return;
    Gizmos.matrix *= Matrix4x4.TRS(vector3_3, quaternion3, UnityEngine.Vector3.one);
    Gizmos.DrawWireSphere(vector3_4, 1f / 500f);
    Gizmos.DrawLine(UnityEngine.Vector3.zero, vector3_4);
  }

  protected bool CheckQuaternion(Quaternion quaternion)
  {
    return (double) quaternion.x + (double) quaternion.y + (double) quaternion.z + (double) quaternion.w != 0.0;
  }
}
