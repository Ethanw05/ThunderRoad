// Decompiled with JetBrains decompiler
// Type: ThunderRoad.IKControllerFIK
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Creatures/IK Controller")]
public class IKControllerFIK : IkController
{
  [Header("Final IK")]
  public AnimationCurve stretchArmsCurve;
  public AnimationCurve stretchLegsCurve;
  public float headTorsoBendWeightMultiplier = 0.5f;
  protected HandPoser handPoserLeft;
  protected HandPoser handPoserRight;
  protected FingerRig leftFingerRig;
  protected FingerRig rightFingerRig;

  public VRIK vrik { get; protected set; }

  public BipedIKCustom bipedIk { get; protected set; }

  public FullBodyBipedIK fullBodyBipedIk { get; protected set; }

  public FBBIKHeadEffector headEffector { get; protected set; }

  public override void Setup()
  {
    this.vrik = this.gameObject.AddComponent<VRIK>();
    this.vrik.solver.locomotion.weight = 0.0f;
    this.vrik.solver.locomotion.footDistance = 0.25f;
    this.vrik.solver.locomotion.stepThreshold = 0.25f;
    this.vrik.solver.locomotion.angleThreshold = 60f;
    this.vrik.solver.plantFeet = false;
    this.vrik.solver.spine.positionWeight = 0.0f;
    this.vrik.solver.spine.rotationWeight = 0.0f;
    this.vrik.solver.spine.pelvisPositionWeight = 0.0f;
    this.vrik.solver.spine.pelvisRotationWeight = 0.0f;
    this.vrik.solver.spine.rotateChestByHands = 0.0f;
    this.vrik.solver.spine.maxRootAngle = 180f;
    this.vrik.solver.spine.maintainPelvisPosition = 0.0f;
    this.vrik.solver.spine.bodyPosStiffness = 0.5f;
    this.vrik.solver.spine.bodyRotStiffness = 0.2f;
    this.vrik.solver.spine.neckStiffness = 0.0f;
    this.vrik.solver.leftArm.positionWeight = 0.0f;
    this.vrik.solver.leftArm.rotationWeight = 0.0f;
    this.vrik.solver.leftArm.stretchCurve = this.stretchArmsCurve;
    this.vrik.solver.rightArm.positionWeight = 0.0f;
    this.vrik.solver.rightArm.rotationWeight = 0.0f;
    this.vrik.solver.rightArm.stretchCurve = this.stretchArmsCurve;
    IKSolverVR.Arm rightArm1 = this.vrik.solver.rightArm;
    RagdollHand handRight1 = this.creature.handRight;
    UnityEngine.Vector3 vector3_1 = handRight1 != null ? handRight1.axisPalm : UnityEngine.Vector3.left;
    rightArm1.wristToPalmAxis = vector3_1;
    IKSolverVR.Arm rightArm2 = this.vrik.solver.rightArm;
    RagdollHand handRight2 = this.creature.handRight;
    UnityEngine.Vector3 vector3_2 = handRight2 != null ? handRight2.axisThumb : UnityEngine.Vector3.up;
    rightArm2.palmToThumbAxis = vector3_2;
    IKSolverVR.Arm leftArm1 = this.vrik.solver.leftArm;
    RagdollHand handLeft1 = this.creature.handLeft;
    UnityEngine.Vector3 vector3_3 = handLeft1 != null ? handLeft1.axisPalm : UnityEngine.Vector3.left;
    leftArm1.wristToPalmAxis = vector3_3;
    IKSolverVR.Arm leftArm2 = this.vrik.solver.leftArm;
    RagdollHand handLeft2 = this.creature.handLeft;
    UnityEngine.Vector3 vector3_4 = handLeft2 != null ? handLeft2.axisThumb : UnityEngine.Vector3.down;
    leftArm2.palmToThumbAxis = vector3_4;
    this.vrik.solver.leftLeg.positionWeight = 0.0f;
    this.vrik.solver.leftLeg.rotationWeight = 0.0f;
    this.vrik.solver.leftLeg.swivelOffset = -20f;
    this.vrik.solver.leftLeg.bendToTargetWeight = 0.0f;
    this.vrik.solver.leftLeg.stretchCurve = this.stretchLegsCurve;
    this.vrik.solver.rightLeg.positionWeight = 0.0f;
    this.vrik.solver.rightLeg.rotationWeight = 0.0f;
    this.vrik.solver.rightLeg.swivelOffset = 0.0f;
    this.vrik.solver.rightLeg.bendToTargetWeight = 0.0f;
    this.vrik.solver.rightLeg.stretchCurve = this.stretchLegsCurve;
    this.vrik.GetIKSolver().OnPreUpdate += new IKSolver.UpdateDelegate(((IkController) this).PreIKUpdate);
    this.vrik.GetIKSolver().OnPostUpdate += new IKSolver.UpdateDelegate(((IkController) this).PostIKUpdate);
    this.vrik.enabled = false;
    this.bipedIk = this.gameObject.AddComponent<BipedIKCustom>();
    this.bipedIk.references.root = this.transform;
    this.bipedIk.references.pelvis = this.creature.animator.GetBoneTransform(HumanBodyBones.Hips);
    Transform boneTransform1 = this.creature.animator.GetBoneTransform(HumanBodyBones.Chest);
    int length = 2 + ((bool) (Object) boneTransform1 ? 1 : 0);
    this.bipedIk.references.spine = new Transform[length];
    this.bipedIk.references.spine[0] = this.creature.animator.GetBoneTransform(HumanBodyBones.Spine);
    if ((bool) (Object) boneTransform1)
      this.bipedIk.references.spine[1] = boneTransform1;
    this.bipedIk.references.spine[length - 1] = this.creature.animator.GetBoneTransform(HumanBodyBones.Neck);
    Transform boneTransform2 = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftEye);
    Transform boneTransform3 = this.creature.animator.GetBoneTransform(HumanBodyBones.RightEye);
    if ((bool) (Object) boneTransform2 && (bool) (Object) boneTransform3)
    {
      this.bipedIk.references.eyes = new Transform[2];
      this.bipedIk.references.eyes[0] = boneTransform2;
      this.bipedIk.references.eyes[1] = boneTransform3;
    }
    else if ((bool) (Object) boneTransform2 || (bool) (Object) boneTransform3)
    {
      this.bipedIk.references.eyes = new Transform[1];
      this.bipedIk.references.eyes[0] = boneTransform2 ?? boneTransform3;
    }
    this.bipedIk.solvers.lookAt.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.lookAt.bodyWeight = 0.6f;
    this.bipedIk.solvers.lookAt.headWeight = 1f;
    this.bipedIk.solvers.lookAt.eyesWeight = 0.25f;
    this.bipedIk.solvers.lookAt.clampWeight = 0.7f;
    this.bipedIk.solvers.lookAt.clampWeightHead = 0.8f;
    this.bipedIk.solvers.lookAt.clampWeightEyes = 0.95f;
    this.bipedIk.references.head = this.creature.animator.GetBoneTransform(HumanBodyBones.Head);
    this.bipedIk.references.leftUpperArm = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
    this.bipedIk.references.leftForearm = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
    this.bipedIk.references.leftHand = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftHand);
    this.bipedIk.references.rightUpperArm = this.creature.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
    this.bipedIk.references.rightForearm = this.creature.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
    this.bipedIk.references.rightHand = this.creature.animator.GetBoneTransform(HumanBodyBones.RightHand);
    this.bipedIk.references.leftThigh = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
    this.bipedIk.references.leftCalf = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
    this.bipedIk.references.leftFoot = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
    this.bipedIk.references.rightThigh = this.creature.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
    this.bipedIk.references.rightCalf = this.creature.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
    this.bipedIk.references.rightFoot = this.creature.animator.GetBoneTransform(HumanBodyBones.RightFoot);
    this.bipedIk.solvers.aim.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.leftFoot.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.leftFoot.IKRotationWeight = 0.0f;
    this.bipedIk.solvers.rightFoot.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.rightFoot.IKRotationWeight = 0.0f;
    this.bipedIk.solvers.leftHand.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.leftHand.IKRotationWeight = 0.0f;
    this.bipedIk.solvers.rightHand.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.rightHand.IKRotationWeight = 0.0f;
    this.bipedIk.solvers.spine.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.aim.IKPositionWeight = 0.0f;
    this.bipedIk.solvers.pelvis.positionWeight = 0.0f;
    this.bipedIk.solvers.pelvis.rotationWeight = 0.0f;
    this.bipedIk.OnPreUpdateEvent += new BipedIKCustom.UpdateDelegate(((IkController) this).PreIKUpdate);
    this.bipedIk.OnPostUpdateEvent += new BipedIKCustom.UpdateDelegate(((IkController) this).PostIKUpdate);
    this.bipedIk.enabled = false;
    this.fullBodyBipedIk = this.gameObject.AddComponent<FullBodyBipedIK>();
    this.fullBodyBipedIk.references = new BipedReferences();
    BipedReferences.AutoDetectReferences(ref this.fullBodyBipedIk.references, this.transform, new BipedReferences.AutoDetectParams(true, false));
    this.fullBodyBipedIk.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(this.fullBodyBipedIk.references);
    this.fullBodyBipedIk.solver.SetToReferences(this.fullBodyBipedIk.references, this.fullBodyBipedIk.solver.rootNode);
    this.fullBodyBipedIk.GetIKSolver().OnPreUpdate += new IKSolver.UpdateDelegate(((IkController) this).PreIKUpdate);
    this.fullBodyBipedIk.GetIKSolver().OnPostUpdate += new IKSolver.UpdateDelegate(((IkController) this).PostIKUpdate);
    this.fullBodyBipedIk.solver.leftArmMapping.weight = 0.0f;
    this.fullBodyBipedIk.solver.rightArmMapping.weight = 0.0f;
    this.fullBodyBipedIk.enabled = false;
    this.headEffector = new GameObject("HeadEffector").AddComponent<FBBIKHeadEffector>();
    this.headEffector.transform.SetParent(this.transform);
    this.headEffector.transform.localPosition = UnityEngine.Vector3.zero;
    this.headEffector.transform.localRotation = Quaternion.identity;
    this.headEffector.ik = this.fullBodyBipedIk;
    this.headEffector.bendBones = new FBBIKHeadEffector.BendBone[3];
    this.headEffector.bendBones[0] = new FBBIKHeadEffector.BendBone(this.creature.animator.GetBoneTransform(HumanBodyBones.Spine), 1f);
    this.headEffector.bendBones[1] = new FBBIKHeadEffector.BendBone(this.creature.animator.GetBoneTransform(HumanBodyBones.Chest), 1f);
    this.headEffector.bendBones[2] = new FBBIKHeadEffector.BendBone(this.creature.animator.GetBoneTransform(HumanBodyBones.Neck), 1f);
    this.headEffector.CCDBones = new Transform[3];
    this.headEffector.CCDBones[0] = this.creature.animator.GetBoneTransform(HumanBodyBones.Spine);
    this.headEffector.CCDBones[1] = this.creature.animator.GetBoneTransform(HumanBodyBones.Chest);
    this.headEffector.CCDBones[2] = this.creature.animator.GetBoneTransform(HumanBodyBones.Neck);
    this.headEffector.CCDWeight = 0.0f;
    this.headEffector.positionWeight = 0.0f;
    this.headEffector.rotationWeight = 0.0f;
    this.headEffector.bendWeight = 0.0f;
    this.headEffector.OnPostHeadEffectorFK += new IKSolver.UpdateDelegate(((IkController) this).PostIKUpdate);
    this.headEffector.enabled = false;
    this.initialized = true;
  }

  protected Transform AddFingerTip(Transform bone, float tipDistance)
  {
    Transform transform = new GameObject("tip").transform;
    transform.SetParent(bone);
    transform.localPosition = new UnityEngine.Vector3(tipDistance, 0.0f, 0.0f);
    transform.localRotation = Quaternion.identity;
    return transform;
  }

  public override void SetFullbody(bool active)
  {
    base.SetFullbody(active);
    this.RefreshState();
  }

  protected void RefreshState()
  {
    if ((bool) (Object) this.eyesTarget || this.headEnabled || this.hipsEnabled || this.handLeftEnabled || this.handRightEnabled || this.footLeftEnabled || this.footRightEnabled || this.shoulderLeftEnabled || this.shoulderRightEnabled)
    {
      if ((bool) (Object) this.creature.player)
        this.SetState(IKControllerFIK.State.Player);
      else if (this.fullbody || this.headEnabled || this.shoulderLeftEnabled || this.shoulderRightEnabled)
        this.SetState(IKControllerFIK.State.FullBody);
      else
        this.SetState(IKControllerFIK.State.Default);
    }
    else
    {
      this.fullbody = false;
      this.SetState(IKControllerFIK.State.Disabled);
    }
  }

  protected void SetState(IKControllerFIK.State state)
  {
    this.bipedIk.enabled = state == IKControllerFIK.State.Default;
    this.fullBodyBipedIk.enabled = state == IKControllerFIK.State.FullBody;
    if ((bool) (Object) this.vrik)
      this.vrik.enabled = state == IKControllerFIK.State.Player;
    this.turnBodyByHeadAndHands = state == IKControllerFIK.State.Player;
  }

  /// LOCOMOTION
  public override float GetLocomotionWeight() => this.vrik.solver.locomotion.weight;

  public override void SetLocomotionWeight(float weight)
  {
    this.vrik.solver.locomotion.weight = weight;
  }

  public override void AddLocomotionDeltaPosition(UnityEngine.Vector3 delta)
  {
    this.vrik.solver.locomotion.AddDeltaPosition(delta);
  }

  public override void AddLocomotionDeltaRotation(Quaternion delta, UnityEngine.Vector3 pivot)
  {
    this.vrik.solver.locomotion.AddDeltaRotation(delta, pivot);
  }

  /// EYES
  public override float GetLookAtWeight() => this.bipedIk.solvers.lookAt.IKPositionWeight;

  public override void SetLookAtTarget(Transform target)
  {
    if ((bool) (Object) target)
    {
      this.bipedIk.solvers.lookAt.target = target;
      this.bipedIk.solvers.lookAt.IKPositionWeight = 1f;
      this.eyesTarget = target;
    }
    else
    {
      this.bipedIk.solvers.lookAt.target = (Transform) null;
      this.bipedIk.solvers.lookAt.IKPositionWeight = 0.0f;
      this.eyesTarget = (Transform) null;
    }
    this.RefreshState();
  }

  public override void SetLookAtWeight(float weight)
  {
    this.bipedIk.solvers.lookAt.IKPositionWeight = weight;
  }

  public override void SetLookAtBodyWeight(float weight, float clamp)
  {
    this.bipedIk.solvers.lookAt.bodyWeight = weight;
    this.bipedIk.solvers.lookAt.clampWeight = clamp;
  }

  public override void SetLookAtHeadWeight(float weight, float clamp)
  {
    this.bipedIk.solvers.lookAt.headWeight = weight;
    this.bipedIk.solvers.lookAt.clampWeightHead = clamp;
  }

  public override void SetLookAtEyesWeight(float weight, float clamp)
  {
    this.bipedIk.solvers.lookAt.eyesWeight = weight;
    this.bipedIk.solvers.lookAt.clampWeightEyes = clamp;
  }

  /// HEAD
  public override void SetHeadAnchor(Transform anchor)
  {
    if ((bool) (Object) anchor)
    {
      this.headEffector.transform.SetParent(anchor);
      this.headEffector.transform.localPosition = UnityEngine.Vector3.zero;
      this.headEffector.transform.localRotation = Quaternion.identity;
      this.headEffector.enabled = true;
      this.vrik.solver.spine.headTarget = this.headEffector.transform;
      this.SetHeadState(true, true);
      this.SetHeadWeight(1f, 1f);
      this.headTarget = this.headEffector.transform;
    }
    else
    {
      this.headEffector.transform.SetParent(this.transform);
      this.headEffector.transform.localPosition = UnityEngine.Vector3.zero;
      this.headEffector.transform.localRotation = Quaternion.identity;
      this.headEffector.enabled = false;
      this.vrik.solver.spine.headTarget = (Transform) null;
      this.SetHeadState(false, false);
      this.SetHeadWeight(0.0f, 0.0f);
      this.headTarget = (Transform) null;
    }
    this.RefreshState();
  }

  public override void SetHeadState(bool positionEnabled, bool rotationEnabled)
  {
    this.vrik.solver.spine.positionWeight = positionEnabled ? 1f : 0.0f;
    this.vrik.solver.spine.rotationWeight = rotationEnabled ? 1f : 0.0f;
    this.headEffector.positionWeight = positionEnabled ? 1f : 0.0f;
    this.headEffector.rotationWeight = rotationEnabled ? 1f : 0.0f;
    this.headEffector.enabled = positionEnabled | rotationEnabled;
    this.headEnabled = positionEnabled || rotationEnabled;
    this.RefreshState();
  }

  public override void SetHeadWeight(float positionWeight, float rotationWeight)
  {
    this.vrik.solver.spine.positionWeight = positionWeight;
    this.vrik.solver.spine.rotationWeight = rotationWeight;
    this.headEffector.positionWeight = positionWeight;
    this.headEffector.rotationWeight = rotationWeight;
    this.headEffector.bendWeight = rotationWeight * this.headTorsoBendWeightMultiplier;
  }

  public override float GetHeadWeight() => this.vrik.solver.spine.positionWeight;

  /// HIPS
  public override void SetHipsAnchor(Transform anchor)
  {
    if ((bool) (Object) anchor)
    {
      this.vrik.solver.spine.pelvisTarget = anchor;
      this.SetHipsState(true);
      this.SetHipsWeight(1f);
      this.hipsTarget = anchor;
    }
    else
    {
      this.vrik.solver.spine.pelvisTarget = (Transform) null;
      this.SetHipsState(false);
      this.SetHipsWeight(0.0f);
      this.hipsTarget = (Transform) null;
    }
    this.RefreshState();
  }

  public override void SetHipsState(bool enabled)
  {
    this.vrik.solver.spine.pelvisPositionWeight = enabled ? 1f : 0.0f;
    this.vrik.solver.spine.pelvisRotationWeight = enabled ? 1f : 0.0f;
    this.hipsEnabled = enabled;
    this.RefreshState();
  }

  public override void SetHipsWeight(float value)
  {
    this.vrik.solver.spine.pelvisPositionWeight = value;
    this.vrik.solver.spine.pelvisRotationWeight = value;
  }

  public override float GetHipsWeight() => this.vrik.solver.spine.pelvisPositionWeight;

  /// UPPER ARM / SHOULDER
  public override void SetShoulderAnchor(Side side, Transform anchor)
  {
    if ((bool) (Object) anchor)
    {
      if (side == Side.Left)
      {
        this.fullBodyBipedIk.solver.leftShoulderEffector.target = anchor;
        this.shoulderLeftTarget = anchor;
      }
      if (side == Side.Right)
      {
        this.fullBodyBipedIk.solver.rightShoulderEffector.target = anchor;
        this.shoulderLeftTarget = anchor;
      }
      this.SetShoulderState(side, true, true);
      this.SetShoulderWeight(side, 1f, 1f);
    }
    else
    {
      if (side == Side.Left)
      {
        this.fullBodyBipedIk.solver.leftShoulderEffector.target = (Transform) null;
        this.shoulderLeftTarget = (Transform) null;
      }
      if (side == Side.Right)
      {
        this.fullBodyBipedIk.solver.rightShoulderEffector.target = (Transform) null;
        this.shoulderRightTarget = (Transform) null;
      }
      this.SetShoulderState(side, false, false);
      this.SetShoulderWeight(side, 0.0f, 0.0f);
    }
    this.RefreshState();
  }

  public override void SetShoulderState(Side side, bool positionEnabled, bool rotationEnabled)
  {
    if (side == Side.Left)
    {
      this.fullBodyBipedIk.solver.leftShoulderEffector.positionWeight = positionEnabled ? 1f : 0.0f;
      this.fullBodyBipedIk.solver.leftShoulderEffector.rotationWeight = rotationEnabled ? 1f : 0.0f;
      this.shoulderLeftEnabled = positionEnabled || rotationEnabled;
    }
    if (side == Side.Right)
    {
      this.fullBodyBipedIk.solver.rightShoulderEffector.positionWeight = positionEnabled ? 1f : 0.0f;
      this.fullBodyBipedIk.solver.rightShoulderEffector.rotationWeight = rotationEnabled ? 1f : 0.0f;
      this.shoulderLeftEnabled = positionEnabled || rotationEnabled;
    }
    this.RefreshState();
  }

  public override void SetShoulderWeight(Side side, float positionWeight, float rotationWeight)
  {
    if (side == Side.Left)
    {
      this.fullBodyBipedIk.solver.leftShoulderEffector.positionWeight = positionWeight;
      this.fullBodyBipedIk.solver.leftShoulderEffector.rotationWeight = rotationWeight;
    }
    if (side != Side.Right)
      return;
    this.fullBodyBipedIk.solver.rightShoulderEffector.positionWeight = positionWeight;
    this.fullBodyBipedIk.solver.rightShoulderEffector.rotationWeight = rotationWeight;
  }

  /// HANDS
  public override void SetHandAnchor(Side side, Transform anchor, Quaternion palmRotation)
  {
    if ((bool) (Object) anchor)
    {
      if (side == Side.Left)
      {
        this.vrik.solver.leftArm.target = anchor;
        this.bipedIk.solvers.leftHand.target = anchor;
        this.fullBodyBipedIk.solver.leftHandEffector.target = anchor;
        this.handLeftTarget = anchor;
      }
      if (side == Side.Right)
      {
        this.vrik.solver.rightArm.target = anchor;
        this.bipedIk.solvers.rightHand.target = anchor;
        this.fullBodyBipedIk.solver.rightHandEffector.target = anchor;
        this.handRightTarget = anchor;
      }
      this.SetHandState(side, true, true);
      this.SetHandWeight(side, 1f, 1f);
    }
    else
    {
      if (side == Side.Left)
      {
        this.vrik.solver.leftArm.target = (Transform) null;
        this.bipedIk.solvers.leftHand.target = (Transform) null;
        this.handLeftTarget = (Transform) null;
      }
      if (side == Side.Right)
      {
        this.vrik.solver.rightArm.target = (Transform) null;
        this.bipedIk.solvers.rightHand.target = (Transform) null;
        this.handRightTarget = (Transform) null;
      }
      this.SetHandState(side, false, false);
      this.SetHandWeight(side, 0.0f, 0.0f);
    }
    this.RefreshState();
  }

  public override void SetHandState(Side side, bool positionEnabled, bool rotationEnabled)
  {
    if (side == Side.Left)
    {
      this.vrik.solver.leftArm.positionWeight = positionEnabled ? 1f : 0.0f;
      this.vrik.solver.leftArm.rotationWeight = rotationEnabled ? 1f : 0.0f;
      this.bipedIk.solvers.leftHand.IKPositionWeight = positionEnabled ? 1f : 0.0f;
      this.bipedIk.solvers.leftHand.IKRotationWeight = rotationEnabled ? 1f : 0.0f;
      this.fullBodyBipedIk.solver.leftHandEffector.positionWeight = positionEnabled ? 1f : 0.0f;
      this.fullBodyBipedIk.solver.leftHandEffector.rotationWeight = rotationEnabled ? 1f : 0.0f;
      this.handLeftEnabled = positionEnabled || rotationEnabled;
    }
    if (side == Side.Right)
    {
      this.vrik.solver.rightArm.positionWeight = positionEnabled ? 1f : 0.0f;
      this.vrik.solver.rightArm.rotationWeight = rotationEnabled ? 1f : 0.0f;
      this.bipedIk.solvers.rightHand.IKPositionWeight = positionEnabled ? 1f : 0.0f;
      this.bipedIk.solvers.rightHand.IKRotationWeight = rotationEnabled ? 1f : 0.0f;
      this.fullBodyBipedIk.solver.rightHandEffector.positionWeight = positionEnabled ? 1f : 0.0f;
      this.fullBodyBipedIk.solver.rightHandEffector.rotationWeight = rotationEnabled ? 1f : 0.0f;
      this.handRightEnabled = positionEnabled || rotationEnabled;
    }
    this.RefreshState();
  }

  public override void SetHandWeight(Side side, float positionWeight, float rotationWeight)
  {
    if (side == Side.Left)
    {
      this.vrik.solver.leftArm.positionWeight = positionWeight;
      this.vrik.solver.leftArm.rotationWeight = rotationWeight;
      this.bipedIk.solvers.leftHand.IKPositionWeight = positionWeight;
      this.bipedIk.solvers.leftHand.IKRotationWeight = rotationWeight;
      this.fullBodyBipedIk.solver.leftHandEffector.positionWeight = positionWeight;
      this.fullBodyBipedIk.solver.leftHandEffector.rotationWeight = rotationWeight;
    }
    if (side != Side.Right)
      return;
    this.vrik.solver.rightArm.positionWeight = positionWeight;
    this.vrik.solver.rightArm.rotationWeight = rotationWeight;
    this.bipedIk.solvers.rightHand.IKPositionWeight = positionWeight;
    this.bipedIk.solvers.rightHand.IKRotationWeight = rotationWeight;
    this.fullBodyBipedIk.solver.rightHandEffector.positionWeight = positionWeight;
    this.fullBodyBipedIk.solver.rightHandEffector.rotationWeight = rotationWeight;
  }

  public override float GetHandPositionWeight(Side side)
  {
    return (bool) (Object) this.creature.player ? (side == Side.Left ? this.vrik.solver.leftArm.positionWeight : this.vrik.solver.rightArm.positionWeight) : (side == Side.Left ? this.bipedIk.solvers.leftHand.IKPositionWeight : this.bipedIk.solvers.rightHand.IKPositionWeight);
  }

  public override float GetHandRotationWeight(Side side)
  {
    return (bool) (Object) this.creature.player ? (side == Side.Left ? this.vrik.solver.leftArm.rotationWeight : this.vrik.solver.rightArm.rotationWeight) : (side == Side.Left ? this.bipedIk.solvers.leftHand.IKRotationWeight : this.bipedIk.solvers.rightHand.IKRotationWeight);
  }

  /// FOOTS
  public override void SetFootAnchor(Side side, Transform anchor, Quaternion toesRotation)
  {
    if ((bool) (Object) anchor)
    {
      if (side == Side.Left)
      {
        this.vrik.solver.leftLeg.target = anchor;
        this.bipedIk.solvers.leftFoot.target = anchor;
        this.footLeftTarget = anchor;
      }
      if (side == Side.Right)
      {
        this.vrik.solver.rightLeg.target = anchor;
        this.bipedIk.solvers.rightFoot.target = anchor;
        this.footRightTarget = anchor;
      }
      this.SetFootState(side, true);
      this.SetFootWeight(side, 1f, 1f);
    }
    else
    {
      if (side == Side.Left)
      {
        this.vrik.solver.leftLeg.target = (Transform) null;
        this.bipedIk.solvers.leftFoot.target = (Transform) null;
        this.footLeftTarget = (Transform) null;
      }
      if (side == Side.Right)
      {
        this.vrik.solver.rightLeg.target = (Transform) null;
        this.bipedIk.solvers.rightFoot.target = (Transform) null;
        this.footRightTarget = (Transform) null;
      }
      this.SetFootState(side, false);
      this.SetFootWeight(side, 0.0f, 0.0f);
    }
    this.RefreshState();
  }

  public override void SetFootPull(Side side, float value)
  {
  }

  public override void SetFootState(Side side, bool active)
  {
    if (side == Side.Left)
    {
      this.vrik.solver.leftLeg.positionWeight = active ? 1f : 0.0f;
      this.vrik.solver.leftLeg.rotationWeight = active ? 1f : 0.0f;
      this.bipedIk.solvers.leftFoot.IKPositionWeight = active ? 1f : 0.0f;
      this.bipedIk.solvers.leftFoot.IKRotationWeight = active ? 1f : 0.0f;
      this.footLeftEnabled = active;
    }
    if (side == Side.Right)
    {
      this.vrik.solver.rightLeg.positionWeight = active ? 1f : 0.0f;
      this.vrik.solver.rightLeg.rotationWeight = active ? 1f : 0.0f;
      this.bipedIk.solvers.rightFoot.IKPositionWeight = active ? 1f : 0.0f;
      this.bipedIk.solvers.rightFoot.IKRotationWeight = active ? 1f : 0.0f;
      this.footRightEnabled = active;
    }
    this.RefreshState();
  }

  public override void SetFootWeight(Side side, float positionWeight, float rotationWeight)
  {
    if (side == Side.Left)
    {
      this.vrik.solver.leftLeg.positionWeight = positionWeight;
      this.vrik.solver.leftLeg.rotationWeight = rotationWeight;
      this.bipedIk.solvers.leftFoot.IKPositionWeight = positionWeight;
      this.bipedIk.solvers.leftFoot.IKRotationWeight = rotationWeight;
    }
    if (side != Side.Right)
      return;
    this.vrik.solver.rightLeg.positionWeight = positionWeight;
    this.vrik.solver.rightLeg.rotationWeight = rotationWeight;
    this.bipedIk.solvers.rightFoot.IKPositionWeight = positionWeight;
    this.bipedIk.solvers.rightFoot.IKRotationWeight = rotationWeight;
  }

  public override void SetKneeAnchor(Side side, Transform anchor)
  {
    if ((bool) (Object) anchor)
    {
      if (side == Side.Left)
      {
        this.vrik.solver.leftLeg.bendGoal = anchor;
        this.bipedIk.solvers.leftFoot.bendGoal = anchor;
        this.kneeLeftHint = anchor;
      }
      if (side == Side.Right)
      {
        this.vrik.solver.rightLeg.bendGoal = anchor;
        this.bipedIk.solvers.rightFoot.bendGoal = anchor;
        this.kneeRightHint = anchor;
      }
    }
    else
    {
      if (side == Side.Left)
      {
        this.vrik.solver.leftLeg.bendGoal = (Transform) null;
        this.bipedIk.solvers.leftFoot.bendGoal = (Transform) null;
        this.kneeLeftHint = (Transform) null;
      }
      if (side == Side.Right)
      {
        this.vrik.solver.rightLeg.bendGoal = (Transform) null;
        this.bipedIk.solvers.rightFoot.bendGoal = (Transform) null;
        this.kneeRightHint = (Transform) null;
      }
    }
    this.RefreshState();
  }

  public override void SetKneeWeight(Side side, float weight)
  {
    if (side == Side.Left)
    {
      this.vrik.solver.leftLeg.bendToTargetWeight = weight;
      this.bipedIk.solvers.leftFoot.bendModifierWeight = weight;
    }
    if (side != Side.Right)
      return;
    this.vrik.solver.rightLeg.bendToTargetWeight = weight;
    this.bipedIk.solvers.rightFoot.bendModifierWeight = weight;
  }

  public override IkController.FootBoneTarget GetFootBoneTarget()
  {
    return IkController.FootBoneTarget.Toes;
  }

  public enum State
  {
    Disabled,
    Default,
    FullBody,
    Player,
  }
}
