// Decompiled with JetBrains decompiler
// Type: GolemIK
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
public class GolemIK : MonoBehaviour
{
  public float alphaMoveSpeed = 3f;
  public float angleMoveSpeed = 5f;
  public Animator animator;
  [Range(0.0f, 1f)]
  public float rightHandIKWeight;
  [Range(0.0f, 1f)]
  public float rightHandAlpha;
  [Range(0.0f, 1f)]
  public float rightHandAlphaTarget;
  public bool rightArmCheckGround = true;
  [Range(-90f, 90f)]
  public float rightHandAngle;
  public Vector3 localRightOffset = Vector3.zero;
  [Range(0.0f, 1f)]
  public float leftHandIkWeight;
  [Range(0.0f, 1f)]
  public float leftHandAlpha;
  [Range(0.0f, 1f)]
  public float leftHandAlphaTarget;
  public bool leftUseAngle = true;
  [Range(-90f, 90f)]
  public float leftHandAngle;
  public Vector3 localLeftOffset = Vector3.zero;
  public bool debugAnimStretchCurve;
  public AnimationCurve timeStretchCurve;
  public string multParameterName;
  public float angle;
  [Range(-2f, 2f)]
  public float armThickness;
  public bool rightHandAlignToGround;
  public bool leftHandAlignToGround;

  private void Start()
  {
    if ((bool) (Object) this.animator)
      return;
    this.animator = this.GetComponent<Animator>();
  }

  private void Update()
  {
    if (!(bool) (Object) this.animator)
      return;
    this.rightHandAlpha = Mathf.MoveTowards(this.rightHandAlpha, this.rightHandAlphaTarget, Time.deltaTime * this.alphaMoveSpeed);
    this.leftHandAlpha = Mathf.MoveTowards(this.leftHandAlpha, this.leftHandAlphaTarget, Time.deltaTime * this.alphaMoveSpeed);
    if (!this.debugAnimStretchCurve)
      return;
    this.animator.SetFloat(this.multParameterName, this.timeStretchCurve.Evaluate(this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f));
  }

  private void OnAnimatorIK(int layerIndex)
  {
    Vector3 localRightOffset = this.localRightOffset;
    this.GetOffsetForHand(ref localRightOffset, ref this.rightHandAngle, this.rightHandAlignToGround, AvatarIKGoal.RightHand);
    Vector3 offset = this.animator.transform.rotation * this.localLeftOffset;
    this.GetOffsetForHand(ref offset, ref this.leftHandAngle, this.leftHandAlignToGround, AvatarIKGoal.LeftHand);
    this.SetHandIK(AvatarIKGoal.RightHand, this.rightHandIKWeight, this.rightHandAlpha, localRightOffset);
    this.SetHandIK(AvatarIKGoal.LeftHand, this.leftHandIkWeight, this.leftHandAlpha, offset);
  }

  private void GetOffsetForHand(
    ref Vector3 offset,
    ref float handAngle,
    bool alignToGround,
    AvatarIKGoal goal)
  {
    offset = this.animator.transform.rotation * offset;
    if (!this.rightArmCheckGround)
      return;
    Transform boneTransform1;
    Transform boneTransform2;
    if (goal == AvatarIKGoal.RightHand)
    {
      boneTransform1 = this.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
      boneTransform2 = this.animator.GetBoneTransform(HumanBodyBones.RightHand);
    }
    else
    {
      boneTransform1 = this.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
      boneTransform2 = this.animator.GetBoneTransform(HumanBodyBones.LeftHand);
    }
    Vector3 position1 = boneTransform2.position with
    {
      y = boneTransform1.position.y
    };
    float maxDistance = 5f;
    UnityEngine.RaycastHit hitInfo;
    Physics.Raycast(position1, this.transform.up * -1f, out hitInfo, maxDistance, 1);
    this.angle = 0.0f;
    if ((bool) (Object) hitInfo.collider)
    {
      Vector3 vector3_1 = boneTransform2.position + boneTransform2.forward * this.armThickness;
      Vector3 vector3_2 = boneTransform1.position + boneTransform2.forward * this.armThickness;
      Vector3 vector3_3 = vector3_1 - boneTransform1.position;
      if ((double) vector3_1.y < (double) hitInfo.point.y)
      {
        float num = Mathf.Sqrt(Mathf.Pow(vector3_3.magnitude, 2f) - Mathf.Pow(hitInfo.distance, 2f));
        Vector3 vector3_4 = Vector3.ProjectOnPlane(vector3_3.normalized, this.transform.up).normalized * num;
        this.angle = Vector3.Angle(vector3_3.normalized, (hitInfo.point + vector3_4 - boneTransform1.position).normalized);
      }
      else if ((double) vector3_1.y > (double) hitInfo.point.y & alignToGround)
      {
        Vector3 vector3_5 = hitInfo.point - boneTransform1.position;
        this.angle = -Vector3.Angle(vector3_3.normalized, vector3_5.normalized);
      }
    }
    handAngle = this.FInterpTo(handAngle, this.angle, Time.deltaTime, this.angleMoveSpeed);
    Vector3 vector3_6 = boneTransform1.InverseTransformPoint(boneTransform2.position);
    Vector3 position2 = Quaternion.AngleAxis(-handAngle, Vector3.forward) * vector3_6;
    Vector3 vector3_7 = boneTransform1.TransformPoint(position2);
    offset = vector3_7 - boneTransform2.position;
  }

  protected float FInterpTo(float Current, float Target, float DeltaTime, float InterpSpeed)
  {
    if ((double) InterpSpeed <= 0.0)
      return Target;
    float f = Target - Current;
    if ((double) Mathf.Abs(f) < 0.0001)
      return Target;
    float num = f * Mathf.Clamp(DeltaTime * InterpSpeed, 0.0f, 1f);
    return Current + num;
  }

  private void SetHandIK(AvatarIKGoal goal, float weight, float alpha, Vector3 offset)
  {
    Vector3 position = this.animator.GetBoneTransform(goal == AvatarIKGoal.RightHand ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand).position;
    Vector3 goalPosition = Vector3.Lerp(position, position + offset, alpha);
    this.animator.SetIKPosition(goal, goalPosition);
    this.animator.SetIKPositionWeight(goal, weight);
  }

  public void ActivateRightHandIK() => this.rightHandAlignToGround = true;

  public void DeactivateRightHandIK() => this.rightHandAlignToGround = false;

  public void ActivateLeftHandIK() => this.leftHandAlignToGround = true;

  public void DeactivateLeftHandIK() => this.leftHandAlignToGround = false;
}
