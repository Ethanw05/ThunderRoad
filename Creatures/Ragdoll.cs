// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Ragdoll
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using RainyReignGames.MeshDismemberment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Reveal;
using ThunderRoad.Skill.SpellPower;
using Unity.Collections;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Ragdoll")]
[AddComponentMenu("ThunderRoad/Creatures/Ragdoll")]
public class Ragdoll : ThunderBehaviour
{
  public Transform meshRig;
  public Transform meshRootBone;
  [Header("Parts")]
  public RagdollPart headPart;
  public RagdollPart leftUpperArmPart;
  public RagdollPart rightUpperArmPart;
  public RagdollPart targetPart;
  public RagdollPart rootPart;
  [Header("Default forces")]
  public float springPositionForce = 1000f;
  public float damperPositionForce = 50f;
  public float maxPositionForce = 1000f;
  public float springRotationForce = 800f;
  public float damperRotationForce = 50f;
  public float maxRotationForce = 100f;
  [Header("Destabilized")]
  public float destabilizedSpringRotationMultiplier = 0.5f;
  public float destabilizedDamperRotationMultiplier = 0.1f;
  public float destabilizedGroundSpringRotationMultiplier = 0.2f;
  [Header("HipsAttached")]
  public float hipsAttachedSpringPositionMultiplier = 1f;
  public float hipsAttachedDamperPositionMultiplier;
  public float hipsAttachedSpringRotationMultiplier = 1f;
  public float hipsAttachedDamperRotationMultiplier;
  [Header("StandUp")]
  public AnimationCurve standUpCurve;
  public float standUpFromGrabDuration = 1f;
  public float preStandUpDuration = 3f;
  public float preStandUpRatio = 0.7f;
  [Header("Player arm")]
  public float playerArmPositionSpring = 5000f;
  public float playerArmPositionDamper = 40f;
  public float playerArmRotationSpring = 1000f;
  public float playerArmRotationDamper = 40f;
  public float playerArmMaxPositionForce = 3000f;
  public float playerArmMaxRotationForce = 250f;
  [Header("Collision")]
  public float collisionEffectMinDelay = 0.2f;
  public float collisionMinVelocity = 2f;
  [NonSerialized]
  public float lastCollisionEffectTime;
  [Header("Misc")]
  public bool allowSelfDamage;
  public bool grippable = true;
  [NonSerialized]
  public Creature creature;
  [Header("Physic toggle")]
  public bool physicToggle;
  public float physicTogglePlayerRadius = 5f;
  public float physicToggleRagdollRadius = 3f;
  public float physicEnabledDuration = 2f;
  public float lastPhysicToggleTime;
  [NonSerialized]
  public bool shouldEnablePhysic;
  public static bool playerPhysicBody;
  [NonSerialized]
  public Transform animatorRig;
  [NonSerialized]
  public float totalMass;
  public Ragdoll.State state = Ragdoll.State.Disabled;
  public bool hipsAttached;
  public List<RagdollPart> parts;
  [NonSerialized]
  public List<Ragdoll.Bone> bones = new List<Ragdoll.Bone>();
  [NonSerialized]
  public Ragdoll.Region rootRegion;
  [NonSerialized]
  public List<Ragdoll.Region> ragdollRegions = new List<Ragdoll.Region>();
  [NonSerialized]
  public IkController ik;
  [NonSerialized]
  public HumanoidFullBodyIK humanoidIk;
  [NonSerialized]
  public List<RagdollHand> handlers = new List<RagdollHand>();
  [NonSerialized]
  public List<SpellCaster> tkHandlers = new List<SpellCaster>();
  [NonSerialized]
  public bool isGrabbed;
  [NonSerialized]
  public bool isTkGrabbed;
  [NonSerialized]
  public bool isSliced;
  [NonSerialized]
  public bool charJointBreakEnabled;
  public BoolHandler forcePhysic;
  [NonSerialized]
  public List<Ragdoll.PhysicModifier> physicModifiers = new List<Ragdoll.PhysicModifier>();
  [NonSerialized]
  public bool initialized;
  [NonSerialized]
  public bool standingUp;
  [NonSerialized]
  public float standStartTime;
  protected Coroutine getUpCoroutine;
  private ConfigurableJoint stabilizationJoint;
  private GameObject stabilizationJointFollowObject;
  private List<Ragdoll.StabilizationJointQueue> stabilizationJointQueue = new List<Ragdoll.StabilizationJointQueue>();
  private Dismemberment dismemberment;
  [NonSerialized]
  public bool hasMetalArmor;
  [NonSerialized]
  public bool meshRaycast = true;

  public ConfigurableJoint StabilizationJoint => this.stabilizationJoint;

  public event Ragdoll.StateChange OnStateChange;

  public event Ragdoll.SliceEvent OnSliceEvent;

  public event Ragdoll.TouchActionDelegate OnTouchActionEvent;

  public event Ragdoll.HeldActionDelegate OnHeldActionEvent;

  public event Ragdoll.TelekinesisGrabEvent OnTelekinesisGrabEvent;

  public event Ragdoll.TelekinesisReleaseEvent OnTelekinesisReleaseEvent;

  public event Ragdoll.GrabEvent OnGrabEvent;

  public event Ragdoll.UngrabEvent OnUngrabEvent;

  public event Ragdoll.ContactEvent OnContactStartEvent;

  public event Ragdoll.ContactEvent OnContactStopEvent;

  public bool sliceRunning { get; private set; }

  public void AutoCreateMeshColliders()
  {
    Dictionary<Transform, List<SkinnedMeshRenderer>> dictionary = new Dictionary<Transform, List<SkinnedMeshRenderer>>();
    foreach (SkinnedMeshRenderer componentsInChild1 in this.meshRig.parent.GetComponentsInChildren<SkinnedMeshRenderer>())
    {
      MeshCollider meshCollider = new GameObject(componentsInChild1.name + "_Collider").AddComponent<MeshCollider>();
      meshCollider.sharedMesh = componentsInChild1.sharedMesh;
      meshCollider.convex = true;
      meshCollider.transform.parent = this.transform;
      if (componentsInChild1.bones.Length == 0)
        Debug.LogError((object) (componentsInChild1.name + " has no associated bones and won't be automatically moved to the ragdoll!"));
      if (componentsInChild1.bones.Length == 1)
        meshCollider.transform.parent = this.GetPart(componentsInChild1.bones[0]).transform;
      if (componentsInChild1.bones.Length != 0)
      {
        UnityEngine.Mesh sharedMesh = componentsInChild1.sharedMesh;
        if (sharedMesh.GetBonesPerVertex().Length == 0)
          break;
        NativeArray<BoneWeight1> allBoneWeights = sharedMesh.GetAllBoneWeights();
        int index1 = -1;
        float num = 0.0f;
        float[] numArray = new float[componentsInChild1.bones.Length];
        for (int index2 = 0; index2 < allBoneWeights.Length; ++index2)
        {
          BoneWeight1 boneWeight1 = allBoneWeights[index2];
          numArray[boneWeight1.boneIndex] += boneWeight1.weight;
          if ((double) numArray[boneWeight1.boneIndex] > (double) num)
          {
            num = numArray[boneWeight1.boneIndex];
            index1 = boneWeight1.boneIndex;
          }
        }
        Transform bone = componentsInChild1.bones[index1];
        Debug.Log((object) ("Associated bone: " + bone.name));
        foreach (RagdollPart componentsInChild2 in this.GetComponentsInChildren<RagdollPart>())
        {
          if ((UnityEngine.Object) componentsInChild2.meshBone == (UnityEngine.Object) bone)
          {
            meshCollider.transform.parent = componentsInChild2.transform;
            break;
          }
        }
      }
    }
  }

  public void AutoAssignParentPartsByBones()
  {
    Dictionary<Transform, RagdollPart> dictionary = new Dictionary<Transform, RagdollPart>();
    foreach (RagdollPart componentsInChild in this.GetComponentsInChildren<RagdollPart>())
    {
      if ((UnityEngine.Object) componentsInChild.meshBone != (UnityEngine.Object) null)
        dictionary[componentsInChild.meshBone] = componentsInChild;
    }
    foreach (KeyValuePair<Transform, RagdollPart> keyValuePair in dictionary)
    {
      RagdollPart ragdollPart1 = keyValuePair.Value;
      for (Transform parent = keyValuePair.Key.parent; (UnityEngine.Object) parent != (UnityEngine.Object) null; parent = parent.parent)
      {
        RagdollPart ragdollPart2;
        if (dictionary.TryGetValue(parent, out ragdollPart2))
        {
          ragdollPart1.parentPart = ragdollPart2;
          break;
        }
      }
    }
  }

  private void Awake()
  {
    this.parts = new List<RagdollPart>();
    this.SetOrderedPartList();
  }

  private RagdollPart AddPartOrdered(RagdollPart part)
  {
    this.parts.Add(part);
    foreach (RagdollPart componentsInChild in this.GetComponentsInChildren<RagdollPart>())
    {
      if ((UnityEngine.Object) componentsInChild.parentPart == (UnityEngine.Object) part)
      {
        RagdollPart ragdollPart = this.AddPartOrdered(componentsInChild);
        if ((bool) (UnityEngine.Object) ragdollPart)
          this.parts.Add(ragdollPart);
      }
    }
    return (RagdollPart) null;
  }

  private void SetOrderedPartList()
  {
    this.parts.Clear();
    foreach (RagdollPart componentsInChild in this.GetComponentsInChildren<RagdollPart>())
      componentsInChild.LinkParent();
    this.AddDrillRecursive(this.rootPart);
  }

  private void AddDrillRecursive(RagdollPart part)
  {
    this.parts.Add(part);
    for (int index = 0; index < part.childParts.Count; ++index)
      this.AddDrillRecursive(part.childParts[index]);
  }

  public void Init(Creature creature)
  {
    this.creature = creature;
    creature.OnFallEvent += new Creature.FallEvent(this.OnFallEvent);
    GameObject destination = new GameObject("Animator");
    destination.transform.SetParentOrigin(this.transform);
    UnityEngine.Object.Instantiate<Transform>(this.meshRig, destination.transform).name = this.meshRig.name;
    SkinnedMeshRenderer skinnedMeshRenderer = new GameObject("SkinnedMesh").AddComponent<SkinnedMeshRenderer>();
    skinnedMeshRenderer.transform.SetParent(destination.transform);
    skinnedMeshRenderer.rootBone = this.meshRootBone;
    this.dismemberment = new Dismemberment((bool) (UnityEngine.Object) creature.lodGroup ? creature.lodGroup.gameObject : creature.gameObject);
    this.dismemberment.Completed += new EventHandler<Dismemberment.CompletedEventArgs>(this.OnSlice);
    Animator animator = destination.AddComponent<Animator>();
    animator.keepAnimatorStateOnDisable = true;
    animator.applyRootMotion = creature.animator.applyRootMotion;
    animator.runtimeAnimatorController = creature.animator.runtimeAnimatorController;
    animator.avatar = creature.animator.avatar;
    animator.cullingMode = creature.animator.cullingMode;
    animator.updateMode = creature.animator.updateMode;
    creature.animator.enabled = false;
    this.ik = creature.animator.GetComponent<IkController>();
    if ((bool) (UnityEngine.Object) this.ik)
    {
      IkController ikController = Common.CloneComponent((Component) this.ik, destination, true) as IkController;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.ik);
      this.ik = ikController;
    }
    CreatureAnimatorEventReceiver component1 = creature.animator.GetComponent<CreatureAnimatorEventReceiver>();
    if ((bool) (UnityEngine.Object) component1)
    {
      Common.CloneComponent((Component) component1, destination, true);
      UnityEngine.Object.Destroy((UnityEngine.Object) component1);
    }
    this.humanoidIk = creature.animator.GetComponent<HumanoidFullBodyIK>();
    if ((bool) (UnityEngine.Object) this.humanoidIk)
    {
      HumanoidFullBodyIK humanoidFullBodyIk = Common.CloneComponent((Component) this.humanoidIk, destination, true) as HumanoidFullBodyIK;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.humanoidIk);
      this.humanoidIk = humanoidFullBodyIk;
    }
    this.animatorRig = this.FindTransformAtSamePath(animator.transform, this.meshRig, creature.animator.transform);
    creature.animator = animator;
    creature.animator.logWarnings = false;
    creature.animator.name = "Animator";
    foreach (RagdollPart part in this.parts)
      part.Init(this);
    this.bones = new List<Ragdoll.Bone>();
    foreach (Transform componentsInChild in this.meshRig.GetComponentsInChildren<Transform>())
    {
      Transform transformAtSamePath = this.FindTransformAtSamePath(this.animatorRig, componentsInChild, this.meshRig);
      if ((bool) (UnityEngine.Object) transformAtSamePath)
        this.bones.Add(new Ragdoll.Bone(creature, componentsInChild, transformAtSamePath, this.GetPart(componentsInChild)));
      else
        Debug.LogErrorFormat((UnityEngine.Object) componentsInChild, $"Could not find animation transform at same path for bone {componentsInChild.name}, did you set the correct meshRig and meshRootBone?");
    }
    this.ResetRegions();
    if ((bool) (UnityEngine.Object) creature.handLeft)
    {
      creature.handLeft.InitAfterBoneInit();
      WristRelaxer component2 = creature.handLeft.GetComponent<WristRelaxer>();
      if ((bool) (UnityEngine.Object) component2)
        component2.Init();
    }
    if ((bool) (UnityEngine.Object) creature.handRight)
    {
      creature.handRight.InitAfterBoneInit();
      WristRelaxer component3 = creature.handRight.GetComponent<WristRelaxer>();
      if ((bool) (UnityEngine.Object) component3)
        component3.Init();
    }
    this.totalMass = 0.0f;
    foreach (Ragdoll.Bone bone in this.bones)
    {
      if ((bool) (UnityEngine.Object) bone.animation.parent)
      {
        bone.parent = this.GetBone(bone.animation.parent);
        if (bone.parent != null)
          bone.parent.childs.Add(bone);
      }
      bone.hasChildAnimationJoint = (bool) (UnityEngine.Object) bone.animation.GetComponentInChildren<ConfigurableJoint>();
      if ((bool) (UnityEngine.Object) bone.part)
        this.totalMass += bone.part.physicBody.mass;
      this.SetMeshBone(bone);
      bone.mesh.name += "_Mesh";
    }
    this.ik = animator.GetComponent<IkController>();
    if ((bool) (UnityEngine.Object) this.ik)
      this.ik.Setup();
    if (creature.data != null)
    {
      RagdollMassScalar component4;
      if (this.TryGetComponent<RagdollMassScalar>(out component4))
        UnityEngine.Object.Destroy((UnityEngine.Object) component4);
      RagdollMassScalar.ScaleMass<RagdollPart>(this.parts, creature.data.ragdollData.standingMass / RagdollMassScalar.GetTotalMass<RagdollPart>(this.parts, new Func<RagdollPart, float>(GetStandingMass)), new Func<RagdollPart, float>(GetStandingMass), (Action<RagdollPart, float>) ((part, mass) =>
      {
        part.physicBody.mass = mass;
        part.standingMass = mass;
      }));
      RagdollMassScalar.ScaleMass<RagdollPart>(this.parts, creature.data.ragdollData.handledMass / RagdollMassScalar.GetTotalMass<RagdollPart>(this.parts, new Func<RagdollPart, float>(GetHandledMass)), new Func<RagdollPart, float>(GetHandledMass), (Action<RagdollPart, float>) ((part, mass) => part.handledMass = mass));
      RagdollMassScalar.ScaleMass<RagdollPart>(this.parts, creature.data.ragdollData.ragdolledMass / RagdollMassScalar.GetTotalMass<RagdollPart>(this.parts, new Func<RagdollPart, float>(GetRagdolledMass)), new Func<RagdollPart, float>(GetRagdolledMass), (Action<RagdollPart, float>) ((part, mass) => part.ragdolledMass = mass));
      foreach (CreatureData.PartData part1 in creature.data.ragdollData.parts)
      {
        foreach (RagdollPart part2 in this.parts)
        {
          if (part1.bodyPartTypes == (RagdollPart.Type) 0 || part1.bodyPartTypes.HasFlagNoGC(part2.type))
            part2.data = part1;
        }
      }
    }
    PhysicsToggleManager.Local.UpdatePhysicToggle(this.creature, true, true, true);
    this.initialized = true;

    static float GetStandingMass(RagdollPart part) => part.standingMass;

    static float GetHandledMass(RagdollPart part) => part.handledMass;

    static float GetRagdolledMass(RagdollPart part) => part.ragdolledMass;
  }

  public void ResetRegions()
  {
    this.rootRegion = new Ragdoll.Region(this.parts, true);
    if (this.ragdollRegions == null)
    {
      this.ragdollRegions = new List<Ragdoll.Region>()
      {
        this.rootRegion
      };
    }
    else
    {
      this.ragdollRegions.Clear();
      this.ragdollRegions.Add(this.rootRegion);
    }
  }

  public void UpdateMetalArmor()
  {
    this.hasMetalArmor = false;
    foreach (RagdollPart part in this.parts)
    {
      if (part.UpdateMetalArmor())
        this.hasMetalArmor = true;
    }
  }

  public void Load(CreatureData data)
  {
    this.destabilizedSpringRotationMultiplier = data.ragdollData.destabilizedSpringRotationMultiplier;
    this.destabilizedDamperRotationMultiplier = data.ragdollData.destabilizedDamperRotationMultiplier;
    this.destabilizedGroundSpringRotationMultiplier = data.ragdollData.destabilizedGroundSpringRotationMultiplier;
    foreach (RagdollPart part in this.parts)
      part.Load();
    this.DisableCharJointBreakForce();
  }

  public void OnDespawn()
  {
    this.ResetRegions();
    this.DisableCharJointBreakForce();
    foreach (RagdollPart part in this.parts)
    {
      if (part.isSliced)
      {
        if ((bool) (UnityEngine.Object) part.slicedMeshRoot)
          UnityEngine.Object.Destroy((UnityEngine.Object) part.slicedMeshRoot.gameObject);
        if ((bool) (UnityEngine.Object) part.bone.meshSplit)
          UnityEngine.Object.Destroy((UnityEngine.Object) part.bone.meshSplit.gameObject);
        part.slicedMeshRoot = (Transform) null;
        part.bone.meshSplit = (Transform) null;
        if (part.isSliced)
        {
          if ((bool) (UnityEngine.Object) part.sliceChildAndDisableSelf)
          {
            foreach (Collider collider in part.colliderGroup.colliders)
              collider.enabled = true;
            foreach (Interactable handle in part.handles)
              handle.SetTouch(true);
          }
          LightVolumeReceiver component = part.GetComponent<LightVolumeReceiver>();
          if ((bool) (UnityEngine.Object) component)
            UnityEngine.Object.Destroy((UnityEngine.Object) component);
        }
        part.characterJointLocked = false;
        part.isSliced = false;
        part.CreateCharJoint(true);
      }
    }
    this.sliceRunning = false;
    this.isSliced = false;
    this.ClearPhysicModifiers();
    this.forcePhysic.Clear();
  }

  public void SetColliders(bool active)
  {
    foreach (RagdollPart part in this.parts)
    {
      foreach (Collider collider in part.colliderGroup.colliders)
        collider.enabled = active;
    }
  }

  public void CancelVelocity()
  {
    foreach (RagdollPart part in this.parts)
      part.physicBody.velocity = UnityEngine.Vector3.zero;
  }

  public void MultiplyVelocity(float amount)
  {
    foreach (RagdollPart part in this.parts)
      part.physicBody.velocity *= amount;
  }

  public void EnableJointLimit()
  {
    foreach (RagdollPart part in this.parts)
      part.ResetCharJointLimit();
  }

  public void DisableJointLimit()
  {
    foreach (RagdollPart part in this.parts)
      part.DisableCharJointLimit();
  }

  protected void SetAnimationBoneToRig(Ragdoll.Bone bone)
  {
    if (bone.parent == null)
      return;
    if (bone.hasChildAnimationJoint)
    {
      bone.animation.SetParent(bone.parent.animation);
      bone.animation.localPosition = bone.orgLocalPosition;
      bone.animation.localRotation = bone.orgLocalRotation;
      bone.animation.localScale = UnityEngine.Vector3.one;
    }
    else
      this.SetAnimationBoneToPart(bone, true);
  }

  protected void SetAnimationBoneToPart(Ragdoll.Bone bone, bool resetNoPartBone = false)
  {
    if (bone.parent == null)
      return;
    if ((bool) (UnityEngine.Object) bone.parent.part)
      bone.animation.SetParent(bone.parent.part.transform);
    else if ((UnityEngine.Object) bone.parent.animation.GetComponentInParent<Animator>() == (UnityEngine.Object) this.creature.animator && bone.parent.parent != null && (bool) (UnityEngine.Object) bone.parent.parent.part)
      bone.animation.SetParent(bone.parent.parent.part.transform);
    else
      bone.animation.SetParent(bone.parent.animation);
    if (resetNoPartBone && !(bool) (UnityEngine.Object) bone.part)
    {
      bone.animation.localPosition = bone.orgLocalPosition;
      bone.animation.localRotation = bone.orgLocalRotation;
    }
    bone.animation.localScale = UnityEngine.Vector3.one;
  }

  protected void SetAnimationBoneToRoot(Ragdoll.Bone bone, bool resetToOrgPosition)
  {
    if (bone.parent != null)
      bone.animation.SetParent(bone.parent.animation, true);
    else
      bone.animation.SetParent(this.animatorRig, true);
    if (resetToOrgPosition)
    {
      bone.animation.localPosition = bone.orgLocalPosition;
      bone.animation.localRotation = bone.orgLocalRotation;
    }
    bone.animation.localScale = UnityEngine.Vector3.one;
  }

  protected void SetMeshBone(Ragdoll.Bone bone, bool forceParentMesh = false, bool parentAnimation = false)
  {
    if ((bool) (UnityEngine.Object) bone.part)
    {
      if (bone.part.isSliced)
        return;
      bone.mesh.SetParent(parentAnimation ? bone.animation : bone.part.transform);
      bone.mesh.localPosition = UnityEngine.Vector3.zero;
      bone.mesh.localRotation = Quaternion.identity;
    }
    else if (bone.parent != null && bone.hasChildAnimationJoint | forceParentMesh)
    {
      bone.mesh.SetParent(bone.parent.mesh, true);
      bone.mesh.localPosition = bone.orgLocalPosition;
      bone.mesh.localRotation = bone.orgLocalRotation;
    }
    else
    {
      bone.mesh.SetParent(bone.animation);
      bone.mesh.localPosition = UnityEngine.Vector3.zero;
      bone.mesh.localRotation = Quaternion.identity;
    }
    bone.mesh.localScale = UnityEngine.Vector3.one;
  }

  public HandleRagdoll GetHandle(string name)
  {
    foreach (RagdollPart part in this.creature.ragdoll.parts)
    {
      foreach (HandleRagdoll handle in part.handles)
      {
        if (handle.name == name)
          return handle;
      }
    }
    return (HandleRagdoll) null;
  }

  public void OnCreatureEnable()
  {
    this.forcePhysic = new BoolHandler(false);
    this.forcePhysic.OnChangeEvent += new ValueHandler<bool>.ChangeEvent(this.OnForcePhysicChange);
    this.lastPhysicToggleTime = Time.time;
    if ((bool) (UnityEngine.Object) this.creature && this.creature.initialized)
    {
      if (this.creature.loaded)
      {
        if (this.state == Ragdoll.State.Standing || this.state == Ragdoll.State.NoPhysic)
          PhysicsToggleManager.Local.UpdatePhysicToggle(this.creature, true, true, true);
        else
          this.SetState(this.state, true, true);
      }
      else
        PhysicsToggleManager.Local.UpdatePhysicToggle(this.creature, true, true, true);
    }
    foreach (RagdollPart part in this.parts)
      part.OnRagdollEnable();
    if (!this.standingUp)
      return;
    this.CancelGetUp();
  }

  private void OnForcePhysicChange(bool oldValue, bool newValue)
  {
    if (!newValue || this.state != Ragdoll.State.NoPhysic)
      return;
    this.SetState(Ragdoll.State.Standing);
    foreach (RagdollPart part in this.parts)
      part.collisionHandler.RemovePhysicModifier((object) this);
  }

  public void OnCreatureDisable()
  {
    foreach (RagdollPart part in this.parts)
      part.OnRagdollDisable();
    if (!(bool) (UnityEngine.Object) this.creature || !this.creature.initialized)
      return;
    if (this.creature.loaded)
    {
      foreach (Ragdoll.Bone bone in this.bones)
        this.SetAnimationBoneToRig(bone);
    }
    else
      this.SetState(Ragdoll.State.Disabled, true);
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    if (!this.initialized || !((UnityEngine.Object) this.creature != (UnityEngine.Object) null) || this.creature.isPlayer || !this.creature.data.destabilizeOnFall || (double) UnityEngine.Vector3.Angle(UnityEngine.Vector3.up, this.creature.locomotion.transform.up) <= 0.5)
      return;
    this.SetState(Ragdoll.State.Destabilized);
    this.creature.locomotion.physicBody.MoveRotation(Quaternion.FromToRotation(UnityEngine.Vector3.forward, -UnityEngine.Vector3.up) * Quaternion.LookRotation(UnityEngine.Vector3.up));
  }

  public void AnimatorMoveUpdate()
  {
    if (this.ragdollRegions.Count <= 1)
      return;
    for (int index = this.ragdollRegions.Count - 1; index > 0; --index)
    {
      if (this.ragdollRegions[index] != this.rootRegion)
      {
        foreach (RagdollPart part in this.ragdollRegions[index].parts)
          part.AnimatorMoveUpdate();
      }
    }
  }

  private void TogglePhysic()
  {
    if (this.state == Ragdoll.State.NoPhysic)
    {
      this.SetState(Ragdoll.State.Standing);
    }
    else
    {
      if (this.state != Ragdoll.State.Standing)
        return;
      this.SetState(Ragdoll.State.NoPhysic);
    }
  }

  public static bool IsPhysicalState(Ragdoll.State state, bool kinematicIsPhysic = false)
  {
    return kinematicIsPhysic ? state == Ragdoll.State.Standing || state == Ragdoll.State.Destabilized || state == Ragdoll.State.Inert || state == Ragdoll.State.Frozen || state == Ragdoll.State.Kinematic : state == Ragdoll.State.Standing || state == Ragdoll.State.Destabilized || state == Ragdoll.State.Inert || state == Ragdoll.State.Frozen;
  }

  public bool IsPhysicsEnabled(bool kinematicIsPhysic = false)
  {
    return Ragdoll.IsPhysicalState(this.state, kinematicIsPhysic);
  }

  public static bool IsAnimatedState(Ragdoll.State state, bool destabilizedIsAnimated = false)
  {
    return destabilizedIsAnimated ? state == Ragdoll.State.Standing || state == Ragdoll.State.Kinematic || state == Ragdoll.State.NoPhysic || state == Ragdoll.State.Destabilized : state == Ragdoll.State.Standing || state == Ragdoll.State.Kinematic || state == Ragdoll.State.NoPhysic;
  }

  public bool IsAnimationEnabled(bool destabilizedIsAnimated = false)
  {
    return Ragdoll.IsAnimatedState(this.state, destabilizedIsAnimated);
  }

  public void SetState(Ragdoll.State newState) => this.SetState(newState, false, false);

  public void SetState(Ragdoll.State newState, bool force) => this.SetState(newState, force, false);

  public void SetState(Ragdoll.State newState, bool force, bool resetPartJointIfPhysic)
  {
    if (!force && this.state == newState)
      return;
    Ragdoll.PhysicStateChange physicStateChange = Ragdoll.PhysicStateChange.None;
    if (!Ragdoll.IsPhysicalState(this.state) && Ragdoll.IsPhysicalState(newState))
      physicStateChange = Ragdoll.PhysicStateChange.ParentingToPhysic;
    else if (Ragdoll.IsPhysicalState(this.state) && !Ragdoll.IsPhysicalState(newState))
      physicStateChange = Ragdoll.PhysicStateChange.PhysicToParenting;
    if (this.OnStateChange != null)
      this.OnStateChange(this.state, newState, physicStateChange, EventTime.OnStart);
    if (physicStateChange == Ragdoll.PhysicStateChange.PhysicToParenting || force && !Ragdoll.IsPhysicalState(newState))
    {
      if ((bool) (UnityEngine.Object) this.creature.lodGroup && this.creature.gameObject.activeSelf)
        this.creature.lodGroup.transform.SetParentOrigin(this.transform);
      foreach (RagdollPart part in this.parts)
      {
        part.collisionHandler.RemovePhysicModifier((object) this);
        part.physicBody.isKinematic = true;
        part.DisableCharJointLimit();
        part.bone.SetPinPositionForce(0.0f, 0.0f, 0.0f);
        part.bone.SetPinRotationForce(0.0f, 0.0f, 0.0f);
        part.bone.animationJoint.gameObject.SetActive(false);
        foreach (HandleRagdoll handle in part.handles)
        {
          UnityEngine.Vector3 localPosition = handle.transform.localPosition;
          Quaternion localRotation = handle.transform.localRotation;
          handle.transform.SetParent(part.bone.animation.transform, false);
          handle.transform.localPosition = localPosition;
          handle.transform.localRotation = localRotation;
        }
      }
    }
    else if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || force && Ragdoll.IsPhysicalState(newState))
    {
      if ((bool) (UnityEngine.Object) this.creature.lodGroup)
        this.creature.lodGroup.transform.SetParentOrigin(this.rootPart.transform);
      foreach (RagdollPart part in this.parts)
      {
        if (resetPartJointIfPhysic)
        {
          part.gameObject.SetActive(false);
          part.bone.animationJoint.gameObject.SetActive(false);
        }
        foreach (HandleRagdoll handle in part.handles)
        {
          UnityEngine.Vector3 localPosition = handle.transform.localPosition;
          Quaternion localRotation = handle.transform.localRotation;
          handle.transform.SetParent(part.transform);
          handle.transform.localPosition = localPosition;
          handle.transform.localRotation = localRotation;
        }
        foreach (ThunderBehaviour holder in part.collisionHandler.holders)
          holder.transform.SetParent(part.transform, false);
      }
    }
    switch (newState)
    {
      case Ragdoll.State.Inert:
      case Ragdoll.State.Destabilized:
        this.CancelGetUp(false);
        this.creature.animator.enabled = newState == Ragdoll.State.Destabilized;
        this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        foreach (RagdollPart part in this.parts)
        {
          if ((bool) (UnityEngine.Object) part.bone.fixedJoint)
            UnityEngine.Object.Destroy((UnityEngine.Object) part.bone.fixedJoint);
          part.collisionHandler.RemovePhysicModifier((object) this);
          if (newState == Ragdoll.State.Inert || (UnityEngine.Object) part == (UnityEngine.Object) this.rootPart)
          {
            part.bone.SetPinPositionForce(0.0f, 0.0f, 0.0f);
            part.bone.SetPinRotationForce(0.0f, 0.0f, 0.0f);
          }
          else
          {
            part.bone.SetPinPositionForce(0.0f, 0.0f, 0.0f);
            part.bone.SetPinRotationForce(this.springRotationForce * this.destabilizedSpringRotationMultiplier, this.damperRotationForce * this.destabilizedDamperRotationMultiplier, this.maxRotationForce);
          }
        }
        if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || force && Ragdoll.IsPhysicalState(newState))
        {
          this.SavePartsPosition();
          this.ResetPartsToOrigin();
          foreach (Ragdoll.Bone bone in this.bones)
          {
            if (bone.parent != null)
            {
              if (bone.hasChildAnimationJoint)
              {
                bone.animation.SetParent(bone.parent.animation);
                bone.animation.localPosition = bone.orgLocalPosition;
                bone.animation.localRotation = bone.orgLocalRotation;
                bone.animation.localScale = UnityEngine.Vector3.one;
              }
              else
                this.SetAnimationBoneToPart(bone);
            }
            this.SetMeshBone(bone);
          }
          foreach (RagdollPart part in this.parts)
          {
            part.gameObject.SetActive(true);
            part.bone.animationJoint.gameObject.SetActive(true);
          }
          this.LoadPartsPosition();
        }
        foreach (Ragdoll.Bone bone in this.bones)
          this.SetAnimationBoneToPart(bone);
        this.creature.locomotion.enabled = false;
        this.creature.SetAnimatorHeightRatio(0.0f);
        break;
      case Ragdoll.State.Frozen:
        this.CancelGetUp(false);
        this.creature.animator.enabled = false;
        this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        foreach (RagdollPart part in this.parts)
        {
          part.collisionHandler.RemovePhysicModifier((object) this);
          part.bone.SetPinPositionForce(0.0f, 0.0f, 0.0f);
          part.bone.SetPinRotationForce(0.0f, 0.0f, 0.0f);
        }
        if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || force && Ragdoll.IsPhysicalState(newState))
        {
          this.SavePartsPosition();
          foreach (Ragdoll.Bone bone in this.bones)
          {
            if (bone.parent != null)
              this.SetAnimationBoneToPart(bone);
          }
          this.ResetPartsToOrigin();
          foreach (Ragdoll.Bone bone in this.bones)
          {
            if (bone.parent != null)
              this.SetMeshBone(bone);
          }
          foreach (RagdollPart part in this.parts)
          {
            part.gameObject.SetActive(true);
            part.bone.animationJoint.gameObject.SetActive(true);
          }
          this.LoadPartsPosition();
        }
        else
        {
          foreach (Ragdoll.Bone bone in this.bones)
          {
            if (bone.parent != null)
              this.SetAnimationBoneToPart(bone);
          }
        }
        foreach (RagdollPart part in this.parts)
        {
          if ((bool) (UnityEngine.Object) part.characterJoint && !(bool) (UnityEngine.Object) part.bone.fixedJoint)
          {
            part.bone.fixedJoint = part.characterJoint.gameObject.AddComponent<FixedJoint>();
            part.bone.fixedJoint.connectedBody = part.characterJoint.connectedBody;
          }
        }
        this.creature.locomotion.enabled = false;
        this.creature.SetAnimatorHeightRatio(1f);
        break;
      case Ragdoll.State.Standing:
        this.creature.animator.enabled = true;
        this.creature.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        foreach (RagdollPart part in this.parts)
        {
          if ((bool) (UnityEngine.Object) part.bone.fixedJoint)
            UnityEngine.Object.Destroy((UnityEngine.Object) part.bone.fixedJoint);
          if (!part.isSliced)
            part.collisionHandler.SetPhysicModifier((object) this, new float?(0.0f));
          if (part.isSliced)
            part.collisionHandler.RemovePhysicModifier((object) this);
          if (this.hipsAttached)
          {
            if ((UnityEngine.Object) part == (UnityEngine.Object) this.rootPart)
            {
              part.bone.SetPinPositionForce(this.springPositionForce * this.hipsAttachedSpringPositionMultiplier, this.damperPositionForce * this.hipsAttachedDamperPositionMultiplier, this.maxPositionForce * this.hipsAttachedSpringPositionMultiplier);
              part.bone.SetPinRotationForce(this.springRotationForce * this.hipsAttachedSpringRotationMultiplier, this.damperRotationForce * this.hipsAttachedDamperRotationMultiplier, this.maxRotationForce * this.hipsAttachedSpringRotationMultiplier);
            }
            else
            {
              part.bone.SetPinPositionForce(0.0f, 0.0f, 0.0f);
              part.bone.SetPinRotationForce(this.springRotationForce, this.damperRotationForce, this.maxRotationForce);
            }
          }
          else
          {
            part.bone.SetPinPositionForce(this.springPositionForce, this.damperPositionForce, this.maxPositionForce);
            part.bone.SetPinRotationForce(this.springRotationForce, this.damperRotationForce, this.maxRotationForce);
          }
        }
        if (physicStateChange == Ragdoll.PhysicStateChange.ParentingToPhysic || force && Ragdoll.IsPhysicalState(newState))
        {
          this.SavePartsPosition();
          this.ResetPartsToOrigin();
          foreach (Ragdoll.Bone bone in this.bones)
          {
            this.SetAnimationBoneToRig(bone);
            this.SetMeshBone(bone);
          }
          foreach (RagdollPart part in this.parts)
          {
            part.gameObject.SetActive(true);
            part.bone.animationJoint.gameObject.SetActive(true);
          }
          this.LoadPartsPosition();
        }
        else
        {
          foreach (Ragdoll.Bone bone in this.bones)
            this.SetAnimationBoneToRig(bone);
        }
        this.creature.locomotion.enabled = true;
        this.creature.SetAnimatorHeightRatio(1f);
        break;
      case Ragdoll.State.Kinematic:
        this.CancelGetUp(false);
        foreach (RagdollPart part in this.parts)
        {
          if ((bool) (UnityEngine.Object) part.bone.fixedJoint)
            UnityEngine.Object.Destroy((UnityEngine.Object) part.bone.fixedJoint);
          foreach (ThunderBehaviour holder in part.collisionHandler.holders)
            holder.transform.SetParent(part.transform, false);
          part.gameObject.SetActive(true);
        }
        foreach (Ragdoll.Bone bone in this.bones)
        {
          this.SetAnimationBoneToRoot(bone, true);
          this.SetMeshBone(bone);
        }
        foreach (RagdollPart part in this.parts)
        {
          part.transform.SetParent(part.bone.animation.transform);
          part.transform.localPosition = UnityEngine.Vector3.zero;
          part.transform.localRotation = Quaternion.identity;
          part.transform.localScale = UnityEngine.Vector3.one;
        }
        this.creature.animator.enabled = true;
        this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        this.creature.locomotion.enabled = true;
        this.creature.SetAnimatorHeightRatio(1f);
        break;
      case Ragdoll.State.NoPhysic:
        this.CancelGetUp(false);
        foreach (RagdollPart part in this.parts)
        {
          if ((bool) (UnityEngine.Object) part.bone.fixedJoint)
            UnityEngine.Object.Destroy((UnityEngine.Object) part.bone.fixedJoint);
          foreach (ThunderBehaviour holder in part.collisionHandler.holders)
            holder.transform.SetParent(part.bone.animation.transform, false);
          part.gameObject.SetActive(false);
        }
        foreach (Ragdoll.Bone bone in this.bones)
        {
          this.SetAnimationBoneToRoot(bone, true);
          this.SetMeshBone(bone, parentAnimation: true);
        }
        foreach (RagdollPart part in this.parts)
        {
          part.transform.SetParent(part.bone.animation.transform);
          part.transform.localPosition = UnityEngine.Vector3.zero;
          part.transform.localRotation = Quaternion.identity;
          part.transform.localScale = UnityEngine.Vector3.one;
        }
        this.creature.animator.enabled = true;
        this.creature.animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        this.creature.locomotion.enabled = true;
        this.creature.SetAnimatorHeightRatio(1f);
        break;
      case Ragdoll.State.Disabled:
        this.CancelGetUp(false);
        foreach (RagdollPart part in this.parts)
        {
          if ((bool) (UnityEngine.Object) part.bone.fixedJoint)
            UnityEngine.Object.Destroy((UnityEngine.Object) part.bone.fixedJoint);
          foreach (ThunderBehaviour holder in part.collisionHandler.holders)
            holder.transform.SetParent(part.transform, false);
          part.gameObject.SetActive(false);
        }
        foreach (Ragdoll.Bone bone in this.bones)
        {
          this.SetAnimationBoneToRoot(bone, true);
          this.SetMeshBone(bone, true);
        }
        foreach (RagdollPart part in this.parts)
        {
          part.transform.SetParent(part.root);
          part.transform.localPosition = part.rootOrgLocalPosition;
          part.transform.localRotation = part.rootOrgLocalRotation;
          part.transform.localScale = UnityEngine.Vector3.one;
        }
        this.creature.animator.enabled = false;
        this.creature.animator.cullingMode = AnimatorCullingMode.CullCompletely;
        this.creature.locomotion.enabled = false;
        this.creature.SetAnimatorHeightRatio(1f);
        break;
    }
    Ragdoll.State state = this.state;
    this.state = newState;
    if (this.creature.data != null && this.creature.data.gender != CreatureData.Gender.None)
      this.creature.animator.SetFloat(Creature.hashFeminity, this.creature.data.gender == CreatureData.Gender.Female ? 1f : 0.0f);
    UnityEngine.Vector3 position = this.creature.transform.position;
    Quaternion rotation = this.creature.transform.rotation;
    this.creature.animator.Update(0.0f);
    this.creature.transform.position = position;
    this.creature.transform.rotation = rotation;
    this.RefreshPartsLayer();
    this.RefreshPartJointAndCollision();
    if (this.creature.initialized)
      this.creature.RefreshRenderers();
    this.creature.RefreshCollisionOfGrabbedItems();
    if (this.OnStateChange == null)
      return;
    this.OnStateChange(state, newState, physicStateChange, EventTime.OnEnd);
  }

  public void SavePartsPosition()
  {
    foreach (RagdollPart part in this.parts)
    {
      part.savedPosition = part.transform.position;
      part.savedRotation = part.transform.rotation;
    }
  }

  public void LoadPartsPosition()
  {
    foreach (RagdollPart part in this.parts)
    {
      part.transform.position = part.savedPosition;
      part.transform.rotation = part.savedRotation;
    }
  }

  public void ResetPartsToOrigin(bool isKinematic = false)
  {
    foreach (RagdollPart part in this.parts)
    {
      part.physicBody.isKinematic = isKinematic;
      if ((UnityEngine.Object) part.transform.parent != (UnityEngine.Object) part.root)
        part.transform.SetParent(part.root);
      part.transform.localPosition = part.rootOrgLocalPosition;
      part.transform.localRotation = part.rootOrgLocalRotation;
      part.transform.localScale = UnityEngine.Vector3.one;
    }
  }

  public void RefreshPartJointAndCollision()
  {
    foreach (RagdollPart part in this.parts)
    {
      part.ResetCharJointLimit();
      part.collisionHandler.active = this.state == Ragdoll.State.Inert || this.state == Ragdoll.State.Destabilized || this.state == Ragdoll.State.Frozen;
      foreach (HandleRagdoll handle in part.handles)
        handle.RefreshJointAndCollision();
    }
  }

  protected void OnFallEvent(Creature.FallState fallState)
  {
    if (this.state != Ragdoll.State.Destabilized)
      return;
    if (fallState == Creature.FallState.StabilizedOnGround || fallState == Creature.FallState.Stabilizing)
    {
      foreach (RagdollPart part in this.parts)
      {
        if (!((UnityEngine.Object) part == (UnityEngine.Object) this.rootPart))
          part.bone.SetPinRotationForce(this.springRotationForce * this.destabilizedGroundSpringRotationMultiplier, this.damperRotationForce * this.destabilizedDamperRotationMultiplier, this.maxRotationForce);
      }
    }
    else
    {
      foreach (RagdollPart part in this.parts)
      {
        if (!((UnityEngine.Object) part == (UnityEngine.Object) this.rootPart))
          part.bone.SetPinRotationForce(this.springRotationForce * this.destabilizedSpringRotationMultiplier, this.damperRotationForce * this.destabilizedDamperRotationMultiplier, this.maxRotationForce);
      }
    }
  }

  public void InvokeTelekinesisGrabEvent(
    SpellTelekinesis spellTelekinesis,
    HandleRagdoll handleRagdoll)
  {
    if (this.OnTelekinesisGrabEvent == null)
      return;
    this.OnTelekinesisGrabEvent(spellTelekinesis, handleRagdoll);
  }

  public void InvokeTelekinesisReleaseEvent(
    SpellTelekinesis spellTelekinesis,
    HandleRagdoll handleRagdoll,
    bool lastHandler)
  {
    if (this.OnTelekinesisReleaseEvent == null)
      return;
    this.OnTelekinesisReleaseEvent(spellTelekinesis, handleRagdoll, lastHandler);
  }

  public void InvokeGrabEvent(RagdollHand ragdollHand, HandleRagdoll handleRagdoll)
  {
    if (this.OnGrabEvent == null)
      return;
    this.OnGrabEvent(ragdollHand, handleRagdoll);
  }

  public void InvokeUngrabEvent(
    RagdollHand ragdollHand,
    HandleRagdoll handleRagdoll,
    bool lastHandler)
  {
    if (this.OnUngrabEvent == null)
      return;
    this.OnUngrabEvent(ragdollHand, handleRagdoll, lastHandler);
  }

  public bool SphereCastGround(
    float sphereRadius,
    float castLenght,
    out UnityEngine.RaycastHit raycastHit,
    out float groundDistance)
  {
    if (Physics.SphereCast(new Ray(new UnityEngine.Vector3(this.creature.ragdoll.rootPart.transform.position.x, this.creature.ragdoll.rootPart.transform.position.y + sphereRadius, this.creature.ragdoll.rootPart.transform.position.z), UnityEngine.Vector3.down), sphereRadius, out raycastHit, castLenght, (int) ThunderRoadSettings.current.groundLayer))
    {
      groundDistance = raycastHit.distance - sphereRadius;
      return true;
    }
    groundDistance = 0.0f;
    return false;
  }

  public virtual void StandUp()
  {
    this.CancelGetUp();
    this.creature.brain.instance.GetModule<BrainModuleHitReaction>()?.StopStagger();
    this.getUpCoroutine = this.StartCoroutine(this.StandUpCoroutine());
  }

  protected virtual IEnumerator StandUpCoroutine()
  {
    Ragdoll handler = this;
    handler.standingUp = true;
    handler.standStartTime = Time.time;
    if (!handler.isGrabbed && !handler.isTkGrabbed)
    {
      handler.SetBodyPositionToHips();
      if ((double) handler.rootPart.physicBody.transform.forward.y > 0.0)
        handler.creature.animator.SetInteger(Creature.hashGetUp, 1);
      else
        handler.creature.animator.SetInteger(Creature.hashGetUp, 2);
    }
    else
      handler.SetBodyPositionToHead();
    handler.creature.locomotion.capsuleCollider.radius = 0.0f;
    handler.creature.locomotion.enabled = true;
    handler.creature.SetAnimatorHeightRatio(1f);
    if ((bool) (UnityEngine.Object) handler.creature)
      handler.creature.enabled = true;
    handler.SetState(Ragdoll.State.Standing);
    handler.RemovePhysicModifier((object) handler);
    handler.SetPinForceMultiplier(0.0f, 0.0f, 0.0f, 0.0f);
    bool standUp = false;
    float elapsedTime = 0.0f;
    if (!handler.isGrabbed && !handler.isTkGrabbed)
    {
      while ((double) elapsedTime < (double) handler.preStandUpDuration)
      {
        float num = handler.standUpCurve.Evaluate(elapsedTime / handler.preStandUpDuration);
        handler.SetPinForceMultiplier(num, num, num, num);
        elapsedTime += Time.deltaTime;
        if (!standUp && (double) elapsedTime / (double) handler.preStandUpDuration > (double) handler.preStandUpRatio)
        {
          if (!handler.isGrabbed && !handler.isTkGrabbed)
            handler.creature.animator.SetInteger(Creature.hashGetUp, 0);
          standUp = true;
        }
        yield return (object) null;
      }
    }
    else
    {
      while ((double) elapsedTime < (double) handler.standUpFromGrabDuration)
      {
        float num = handler.standUpCurve.Evaluate(elapsedTime / handler.standUpFromGrabDuration);
        handler.SetPinForceMultiplier(num, num, num, num);
        elapsedTime += Time.deltaTime;
        yield return (object) null;
      }
    }
    handler.ResetPinForce();
    handler.SetState(Ragdoll.State.Standing);
    if (!handler.isGrabbed && !handler.isTkGrabbed)
    {
      while (handler.creature.animator.GetBool(Creature.hashIsBusy))
      {
        handler.creature.locomotion.capsuleCollider.radius = Mathf.Lerp(0.0f, handler.creature.locomotion.colliderRadius, handler.creature.animator.GetCurrentAnimatorStateInfo(3).normalizedTime);
        yield return (object) null;
      }
    }
    else
    {
      handler.creature.locomotion.capsuleCollider.radius = handler.creature.locomotion.colliderRadius;
      if (handler.isGrabbed)
        handler.creature.locomotion.StartShrinkCollider();
    }
    handler.creature.RefreshRenderers();
    handler.getUpCoroutine = (Coroutine) null;
    handler.standingUp = false;
  }

  public virtual void CancelGetUp(bool resetState = true)
  {
    if (this.getUpCoroutine != null)
    {
      this.StopCoroutine(this.getUpCoroutine);
      this.creature.animator.SetInteger(Creature.hashGetUp, 0);
      this.creature.locomotion.capsuleCollider.radius = this.creature.locomotion.colliderRadius;
      this.getUpCoroutine = (Coroutine) null;
      if (resetState)
        this.SetState(Ragdoll.State.Destabilized, true);
    }
    this.standingUp = false;
  }

  public virtual void SetBodyPositionToHips()
  {
    UnityEngine.Vector3 forward = (double) this.rootPart.physicBody.transform.forward.y <= 0.0 ? new UnityEngine.Vector3(-this.rootPart.transform.right.x, 0.0f, -this.rootPart.transform.right.z) : new UnityEngine.Vector3(this.rootPart.transform.right.x, 0.0f, this.rootPart.transform.right.z);
    foreach (ThunderBehaviour part in this.parts)
      part.transform.SetParent((Transform) null, true);
    this.creature.locomotion.physicBody.velocity = UnityEngine.Vector3.zero;
    this.creature.locomotion.physicBody.angularVelocity = UnityEngine.Vector3.zero;
    this.creature.transform.position = this.rootPart.transform.position;
    UnityEngine.RaycastHit raycastHit;
    if (this.creature.locomotion.SphereCastGround(10f, out raycastHit, out float _))
      this.creature.transform.position = raycastHit.point;
    this.creature.transform.rotation = Quaternion.LookRotation(forward, UnityEngine.Vector3.up);
    foreach (RagdollPart part in this.parts)
      part.transform.SetParent(part.root, true);
  }

  public virtual void SetBodyPositionToHead()
  {
    foreach (ThunderBehaviour part in this.parts)
      part.transform.SetParent((Transform) null, true);
    this.creature.transform.position = this.headPart.transform.position;
    UnityEngine.RaycastHit raycastHit;
    if (this.creature.locomotion.SphereCastGround(10f, out raycastHit, out float _))
      this.creature.transform.position = raycastHit.point;
    this.creature.transform.rotation = Quaternion.LookRotation(this.headPart.transform.forward.ToXZ(), UnityEngine.Vector3.up);
    foreach (RagdollPart part in this.parts)
      part.transform.SetParent(part.root, true);
  }

  /// <summary>
  /// Adds a stabilization joint connected to the root of the ragdoll
  /// If the stabilization joint already exists, it will take any further requests to enable it and add it to a queue.
  /// When one component removes the joint it will look for the next one to take over
  /// </summary>
  /// <param name="owningObject">The object to which the joint will be applied</param>
  /// <param name="relativeObject">The object relative to which this root will be placed</param>
  public virtual void AddStabilizationJoint(
    GameObject owningObject,
    Ragdoll.StabilizationJointSettings jointSettings = null)
  {
    if ((UnityEngine.Object) this.stabilizationJoint == (UnityEngine.Object) null)
    {
      this.stabilizationJointFollowObject = owningObject;
      GameObject gameObject = new GameObject("stabilizationJoint");
      gameObject.transform.position = this.stabilizationJointFollowObject.transform.position;
      gameObject.transform.rotation = this.stabilizationJointFollowObject.transform.rotation;
      Rigidbody stabilityRB = gameObject.AddComponent<Rigidbody>();
      this.stabilizationJoint = gameObject.AddComponent<ConfigurableJoint>();
      this.stabilizationJoint.connectedBody = this.rootPart.physicBody.rigidBody;
      if (jointSettings == null)
        return;
      this.SetSettings(jointSettings, stabilityRB);
    }
    else if ((UnityEngine.Object) this.stabilizationJointFollowObject == (UnityEngine.Object) null)
    {
      this.stabilizationJointFollowObject = owningObject;
      this.stabilizationJoint.transform.position = this.stabilizationJointFollowObject.transform.position;
      this.stabilizationJoint.transform.rotation = this.stabilizationJointFollowObject.transform.rotation;
      Rigidbody component = this.stabilizationJoint.GetComponent<Rigidbody>();
      if (jointSettings == null)
        return;
      this.SetSettings(jointSettings, component);
    }
    else
      this.stabilizationJointQueue.Add(new Ragdoll.StabilizationJointQueue(owningObject, jointSettings));
  }

  private void SetSettings(Ragdoll.StabilizationJointSettings jointSettings, Rigidbody stabilityRB)
  {
    stabilityRB.isKinematic = jointSettings.isKinematic;
    stabilityRB.useGravity = false;
    this.stabilizationJoint.configuredInWorldSpace = jointSettings.configuredInWorldSpace;
    this.stabilizationJoint.autoConfigureConnectedAnchor = jointSettings.autoConfigureConnectedAnchor;
    this.stabilizationJoint.axis = jointSettings.axis;
    this.stabilizationJoint.angularXMotion = jointSettings.angularXMotion;
    this.stabilizationJoint.angularYMotion = jointSettings.angularYMotion;
    this.stabilizationJoint.angularZMotion = jointSettings.angularZMotion;
    this.stabilizationJoint.angularYZLimitSpring = jointSettings.angularYZLimitSpring;
    this.stabilizationJoint.angularZLimit = jointSettings.angularZLimit;
    this.stabilizationJoint.angularYLimit = jointSettings.angularYLimit;
    this.stabilizationJoint.angularXLimitSpring = jointSettings.angularXLimitSpring;
    SoftJointLimit softJointLimit = new SoftJointLimit();
    softJointLimit.limit = -jointSettings.angularXLimit.limit;
    softJointLimit.bounciness = jointSettings.angularXLimit.bounciness;
    softJointLimit.contactDistance = jointSettings.angularXLimit.contactDistance;
    this.stabilizationJoint.highAngularXLimit = jointSettings.angularXLimit;
    this.stabilizationJoint.lowAngularXLimit = softJointLimit;
    this.stabilizationJoint.angularYZDrive = jointSettings.angularYZDrive;
    this.stabilizationJoint.angularXDrive = jointSettings.angularXDrive;
  }

  /// <summary>
  /// When this ragdoll already possess a stabilization joint
  /// </summary>
  /// <returns></returns>
  public bool HasStabilizationJointEnabled() => (UnityEngine.Object) this.stabilizationJoint != (UnityEngine.Object) null;

  /// <summary>
  /// When this ragdoll already possess a stabilization joint
  /// </summary>
  /// <returns></returns>
  public bool HasStabilizationJoint(GameObject owningObject)
  {
    return (bool) (UnityEngine.Object) this.stabilizationJoint && (UnityEngine.Object) this.stabilizationJointFollowObject == (UnityEngine.Object) owningObject;
  }

  /// <summary>
  /// Removes the joint from this body and if there's a queue, adds it to the next item
  /// </summary>
  public virtual void RemoveStabilizationJoint(GameObject owningObject)
  {
    if (this.stabilizationJointQueue.Count == 0)
    {
      if (!this.HasStabilizationJoint(owningObject))
        return;
      this.stabilizationJointFollowObject = (GameObject) null;
      this.stabilizationJoint.connectedBody = (Rigidbody) null;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.stabilizationJoint.gameObject);
      this.stabilizationJoint = (ConfigurableJoint) null;
    }
    else if (this.HasStabilizationJoint(owningObject))
    {
      this.stabilizationJointFollowObject = (GameObject) null;
      this.AddStabilizationJoint(this.stabilizationJointQueue[0].owningObject, this.stabilizationJointQueue[0].settings);
      this.stabilizationJointQueue.RemoveAt(0);
    }
    else
      this.stabilizationJointQueue.RemoveAll((Predicate<Ragdoll.StabilizationJointQueue>) (item => (UnityEngine.Object) item.owningObject == (UnityEngine.Object) owningObject || (UnityEngine.Object) item.owningObject == (UnityEngine.Object) null));
  }

  public Ragdoll.Bone GetBone(Transform meshOrAnimBone)
  {
    foreach (Ragdoll.Bone bone in this.bones)
    {
      if ((UnityEngine.Object) bone.animation == (UnityEngine.Object) meshOrAnimBone || (UnityEngine.Object) bone.mesh == (UnityEngine.Object) meshOrAnimBone)
        return bone;
    }
    return (Ragdoll.Bone) null;
  }

  public RagdollPart GetPart(Transform meshBone)
  {
    if ((UnityEngine.Object) meshBone == (UnityEngine.Object) null)
      return (RagdollPart) null;
    int count = this.parts.Count;
    for (int index = 0; index < count; ++index)
    {
      RagdollPart part = this.parts[index];
      if ((UnityEngine.Object) part.meshBone == (UnityEngine.Object) meshBone)
        return part;
    }
    return (RagdollPart) null;
  }

  public RagdollPart GetPart(RagdollPart.Type partTypes)
  {
    return this.GetPart(partTypes, RagdollPart.Section.Full);
  }

  public RagdollPart GetPart(RagdollPart.Type partTypes, RagdollPart.Section section = RagdollPart.Section.Full)
  {
    int count = this.parts.Count;
    for (int index = 0; index < count; ++index)
    {
      RagdollPart part = this.parts[index];
      if (partTypes.HasFlagNoGC(part.type) && (section == RagdollPart.Section.Full || part.section == section))
        return part;
    }
    return (RagdollPart) null;
  }

  public RagdollPart GetPartByName(string name)
  {
    int count = this.parts.Count;
    for (int index = 0; index < count; ++index)
    {
      RagdollPart part = this.parts[index];
      if (part.name.Equals(name))
        return part;
    }
    return (RagdollPart) null;
  }

  public void GetClosestPartColliderAndMatHash(
    UnityEngine.Vector3 origin,
    out RagdollPart closestPart,
    out Collider closestCollider,
    out int materialHash,
    bool includeApparel = false,
    UnityEngine.Vector2? randomPos = null)
  {
    closestPart = this.parts[0];
    UnityEngine.Vector3 vector3;
    for (int index = 1; index < this.parts.Count; ++index)
    {
      RagdollPart part = this.parts[index];
      vector3 = part.transform.position - origin;
      double sqrMagnitude1 = (double) vector3.sqrMagnitude;
      vector3 = closestPart.transform.position - origin;
      double sqrMagnitude2 = (double) vector3.sqrMagnitude;
      if (sqrMagnitude1 < sqrMagnitude2)
        closestPart = part;
    }
    closestCollider = closestPart.colliderGroup.colliders[0];
    for (int index = 1; index < closestPart.colliderGroup.colliders.Count; ++index)
    {
      Collider collider = closestPart.colliderGroup.colliders[index];
      vector3 = collider.transform.position - origin;
      double sqrMagnitude3 = (double) vector3.sqrMagnitude;
      vector3 = closestCollider.transform.position - origin;
      double sqrMagnitude4 = (double) vector3.sqrMagnitude;
      if (sqrMagnitude3 < sqrMagnitude4)
        closestCollider = collider;
    }
    materialHash = Animator.StringToHash(closestCollider.material.name);
    if (!includeApparel)
      return;
    UnityEngine.Vector2 vector2 = randomPos.HasValue ? randomPos.Value : new UnityEngine.Vector2(UnityEngine.Random.value, UnityEngine.Random.value);
    MaterialData materialData = (MaterialData) null;
    foreach (Creature.RendererData renderer in closestPart.renderers)
    {
      if ((UnityEngine.Object) renderer.meshPart?.idMap != (UnityEngine.Object) null)
      {
        MaterialData material = MaterialData.GetMaterial(renderer.meshPart.idMap.GetPixel(Mathf.RoundToInt(vector2.x * (float) renderer.meshPart.idMap.width), Mathf.RoundToInt(vector2.y * (float) renderer.meshPart.idMap.height)));
        if (materialData == null || material != null && material.apparelProtectionLevel > materialData.apparelProtectionLevel)
          materialData = material;
      }
    }
    if (materialData == null)
      return;
    materialHash = materialData.physicMaterialHash;
  }

  protected Transform FindTransformAtSamePath(
    Transform transform,
    Transform targetTransform,
    Transform targetRoot)
  {
    string gameObjectPath = this.GetGameObjectPath(targetTransform, targetRoot);
    return gameObjectPath == null ? transform : transform.Find(gameObjectPath);
  }

  protected string GetGameObjectPath(Transform target, Transform root)
  {
    if ((UnityEngine.Object) target == (UnityEngine.Object) root)
      return (string) null;
    string str = "/" + target.name;
    while ((UnityEngine.Object) target.parent != (UnityEngine.Object) null && !((UnityEngine.Object) target.parent == (UnityEngine.Object) root))
    {
      target = target.parent;
      str = $"/{target.name}{str}";
    }
    return str.Remove(0, 1);
  }

  private void OnDrawGizmosSelected()
  {
    foreach (Ragdoll.Bone bone in this.bones)
    {
      if (bone.parent != null && (bool) (UnityEngine.Object) bone.part)
      {
        Gizmos.color = UnityEngine.Color.red;
        Gizmos.DrawLine(bone.parent.animation.position, bone.animation.position);
      }
    }
  }

  public void SetPartsLayer(int layer)
  {
    foreach (RagdollPart part in this.parts)
      part.SetLayer(layer);
  }

  public void SetPartsLayer(LayerName layerName)
  {
    foreach (RagdollPart part in this.parts)
      part.SetLayer(layerName);
  }

  public void RefreshPartsLayer()
  {
    foreach (RagdollPart part in this.parts)
      part.RefreshLayer();
  }

  public virtual void SetPinForceMultiplier(
    float springMultiplier,
    float damperMultiplier,
    float posMaxForceMult,
    float rotMaxForceMult,
    bool jointLimits = true,
    bool noRoot = false,
    RagdollPart.Type partTypes = (RagdollPart.Type) 0,
    Ragdoll.Region affectedRegion = null)
  {
    if (affectedRegion == null)
      affectedRegion = this.rootRegion;
    float spring1 = this.springRotationForce * springMultiplier;
    float spring2 = this.springPositionForce * springMultiplier;
    float damper1 = this.damperPositionForce * damperMultiplier;
    float damper2 = this.damperRotationForce * damperMultiplier;
    float maxForce1 = this.maxPositionForce * posMaxForceMult;
    float maxForce2 = this.maxRotationForce * rotMaxForceMult;
    int count = this.parts.Count;
    for (int index = 0; index < count; ++index)
    {
      RagdollPart part = this.parts[index];
      if ((!noRoot || !((UnityEngine.Object) part == (UnityEngine.Object) this.rootPart)) && (partTypes == (RagdollPart.Type) 0 || partTypes.HasFlagNoGC(part.type)))
      {
        part.bone.SetPinPositionForce(spring2, damper1, maxForce1);
        part.bone.SetPinRotationForce(spring1, damper2, maxForce2, affectedRegion);
        if (jointLimits)
          part.ResetCharJointLimit();
        else
          part.DisableCharJointLimit();
      }
    }
  }

  public virtual void SetPinForce(
    float posSpring,
    float posDamper,
    float rotSpring,
    float rotDamper,
    float posMaxForce,
    float rotMaxForce,
    bool jointLimits = true,
    bool noRoot = false,
    RagdollPart.Type partTypes = (RagdollPart.Type) 0,
    Ragdoll.Region affectedRegion = null)
  {
    if (affectedRegion == null)
      affectedRegion = this.rootRegion;
    foreach (RagdollPart part in this.parts)
    {
      if ((!noRoot || !((UnityEngine.Object) part == (UnityEngine.Object) this.rootPart)) && (partTypes == (RagdollPart.Type) 0 || partTypes.HasFlagNoGC(part.type)))
      {
        part.bone.SetPinPositionForce(posSpring, posDamper, posMaxForce);
        part.bone.SetPinRotationForce(rotSpring, rotDamper, rotMaxForce, affectedRegion);
        if (jointLimits)
          part.ResetCharJointLimit();
        else
          part.DisableCharJointLimit();
      }
    }
  }

  public void ResetPinForce(bool jointLimits = true, bool noRoot = false, RagdollPart.Type partTypes = (RagdollPart.Type) 0)
  {
    foreach (RagdollPart part in this.parts)
    {
      if ((!noRoot || !((UnityEngine.Object) part == (UnityEngine.Object) this.rootPart)) && (partTypes == (RagdollPart.Type) 0 || partTypes.HasFlagNoGC(part.type)))
      {
        part.bone.ResetPinForce();
        if (jointLimits)
          part.ResetCharJointLimit();
        else
          part.DisableCharJointLimit();
      }
    }
  }

  public void SetPhysicModifier(
    object handler,
    float? gravityRatio = null,
    float massRatio = 1f,
    float drag = -1f,
    float angularDrag = -1f,
    EffectData effectData = null)
  {
    foreach (RagdollPart part in this.parts)
      part.collisionHandler.SetPhysicModifier(handler, gravityRatio, massRatio, drag, angularDrag);
    Ragdoll.PhysicModifier physicModifier1 = this.physicModifiers.FirstOrDefault<Ragdoll.PhysicModifier>((Func<Ragdoll.PhysicModifier, bool>) (p => p.handler == handler));
    if (physicModifier1 != null)
    {
      if (effectData == null || effectData == physicModifier1.effectData || physicModifier1.effectInstance == null)
        return;
      physicModifier1.effectInstance.End();
      physicModifier1.effectInstance = effectData.Spawn(this.rootPart.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
      physicModifier1.effectInstance.SetRenderer(this.creature.GetRendererForVFX(), false);
      physicModifier1.effectInstance.Play();
    }
    else
    {
      Ragdoll.PhysicModifier physicModifier2 = new Ragdoll.PhysicModifier(handler, effectData);
      if (effectData != null)
      {
        physicModifier2.effectInstance = effectData.Spawn(this.rootPart.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
        physicModifier2.effectInstance.SetRenderer(this.creature.GetRendererForVFX(), false);
        physicModifier2.effectInstance.Play();
      }
      this.physicModifiers.Add(physicModifier2);
    }
  }

  public void RemovePhysicModifier(object handler)
  {
    int count = this.parts.Count;
    for (int index = 0; index < count; ++index)
      this.parts[index].collisionHandler.RemovePhysicModifier(handler);
    for (int index = 0; index < this.physicModifiers.Count; ++index)
    {
      Ragdoll.PhysicModifier physicModifier = this.physicModifiers[index];
      if (physicModifier.handler == handler)
      {
        if (physicModifier.effectInstance != null)
          physicModifier.effectInstance.End();
        this.physicModifiers.RemoveAtIgnoreOrder<Ragdoll.PhysicModifier>(index);
        --index;
      }
    }
  }

  public void ClearPhysicModifiers()
  {
    int count = this.parts.Count;
    for (int index = 0; index < count; ++index)
      this.parts[index].collisionHandler.ClearPhysicModifiers();
    for (int index = 0; index < this.physicModifiers.Count; ++index)
    {
      Ragdoll.PhysicModifier physicModifier = this.physicModifiers[index];
      if (physicModifier.effectInstance != null)
        physicModifier.effectInstance.End();
    }
    this.physicModifiers.Clear();
  }

  public void EnableCharJointBreakForce(float multiplier = 1f)
  {
    foreach (RagdollPart part in this.parts)
      part.EnableCharJointBreakForce(multiplier);
    this.charJointBreakEnabled = true;
  }

  public void DisableCharJointBreakForce()
  {
    foreach (RagdollPart part in this.parts)
      part.ResetCharJointBreakForce();
    this.charJointBreakEnabled = false;
  }

  public virtual void IgnoreCollision(
    Collider collider,
    bool ignore,
    RagdollPart.Type ignoredParts = (RagdollPart.Type) 0)
  {
    foreach (RagdollPart part in this.parts)
    {
      if (!ignoredParts.HasFlagNoGC(part.type))
      {
        foreach (Collider collider1 in part.colliderGroup.colliders)
        {
          if (!((UnityEngine.Object) collider1 == (UnityEngine.Object) null) && !((UnityEngine.Object) collider == (UnityEngine.Object) null))
            Physics.IgnoreCollision(collider1, collider, ignore);
        }
      }
    }
  }

  public virtual void IgnoreCollision(Ragdoll otherRagdoll, bool ignore)
  {
    foreach (RagdollPart part1 in this.parts)
    {
      foreach (Collider collider1 in part1.colliderGroup.colliders)
      {
        foreach (RagdollPart part2 in otherRagdoll.parts)
        {
          foreach (Collider collider2 in part2.colliderGroup.colliders)
          {
            if (!((UnityEngine.Object) collider1 == (UnityEngine.Object) null) && !((UnityEngine.Object) collider2 == (UnityEngine.Object) null))
              Physics.IgnoreCollision(collider1, collider2, ignore);
          }
        }
      }
    }
  }

  public void ForBothHands(Action<RagdollHand> action)
  {
    action(this.creature.handLeft);
    action(this.creature.handRight);
  }

  public bool TrySlice(RagdollPart slicedPart)
  {
    Ragdoll.SliceEvent onSliceEvent = this.OnSliceEvent;
    if (onSliceEvent != null)
      onSliceEvent(slicedPart, EventTime.OnStart);
    EventManager.InvokeRagdollSlice(slicedPart, EventTime.OnStart);
    if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment) || slicedPart.isSliced || (UnityEngine.Object) slicedPart == (UnityEngine.Object) this.rootPart)
      return false;
    if (!(bool) (UnityEngine.Object) slicedPart.sliceFillMaterial)
      Debug.LogError((object) "Slice fill material is null!");
    this.StartCoroutine(this.SliceCoroutine(slicedPart));
    return true;
  }

  public virtual void OnHeldAction(
    RagdollHand ragdollHand,
    RagdollPart part,
    HandleRagdoll handle,
    Interactable.Action action)
  {
    Ragdoll.HeldActionDelegate onHeldActionEvent = this.OnHeldActionEvent;
    if (onHeldActionEvent == null)
      return;
    onHeldActionEvent(ragdollHand, part, handle, action);
  }

  public virtual void OnTouchAction(
    RagdollHand ragdollHand,
    RagdollPart part,
    Interactable interactable,
    Interactable.Action action)
  {
    Ragdoll.TouchActionDelegate touchActionEvent = this.OnTouchActionEvent;
    if (touchActionEvent == null)
      return;
    touchActionEvent(ragdollHand, part, interactable, action);
  }

  public void CollisionStartStop(
    CollisionInstance collisionInstance,
    RagdollPart part,
    bool active)
  {
    if (active)
    {
      Ragdoll.ContactEvent contactStartEvent = this.OnContactStartEvent;
      if (contactStartEvent == null)
        return;
      contactStartEvent(collisionInstance, part);
    }
    else
    {
      Ragdoll.ContactEvent contactStopEvent = this.OnContactStopEvent;
      if (contactStopEvent == null)
        return;
      contactStopEvent(collisionInstance, part);
    }
  }

  private IEnumerator SliceCoroutine(RagdollPart slicedPart)
  {
    Ragdoll handler = this;
    while (handler.sliceRunning)
      yield return (object) null;
    handler.sliceRunning = true;
    slicedPart.DestroyCharJoint();
    if ((bool) (UnityEngine.Object) slicedPart.bone.fixedJoint)
      UnityEngine.Object.Destroy((UnityEngine.Object) slicedPart.bone.fixedJoint);
    foreach (Ragdoll.Bone child in slicedPart.bone.childs)
    {
      if ((bool) (UnityEngine.Object) child.part)
      {
        child.SetPinPositionForce(0.0f, 0.0f, 0.0f);
        child.SetPinRotationForce(0.0f, 0.0f, 0.0f);
        child.part.collisionHandler.RemovePhysicModifier((object) handler);
      }
    }
    if (slicedPart.data != null)
    {
      UnityEngine.Vector3 direction;
      slicedPart.GetSlicePositionAndDirection(out UnityEngine.Vector3 _, out direction);
      if ((double) slicedPart.data.sliceSeparationForce > 0.0 && !slicedPart.physicBody.isKinematic)
      {
        slicedPart.physicBody.velocity *= slicedPart.data.sliceVelocityMultiplier;
        slicedPart.physicBody.AddForce(direction * slicedPart.data.sliceSeparationForce, ForceMode.VelocityChange);
      }
    }
    List<Ragdoll.Bone> allChilds = slicedPart.bone.GetAllChilds();
    yield return (object) handler.dismemberment.DoRip(slicedPart.bone.mesh, allChilds.Select<Ragdoll.Bone, Transform>((Func<Ragdoll.Bone, Transform>) (b => b.mesh)).ToArray<Transform>(), slicedPart.sliceThreshold, slicedPart.sliceFillMaterial);
  }

  private void OnSlice(object sender, Dismemberment.CompletedEventArgs args)
  {
    if (!args.successful)
    {
      this.sliceRunning = false;
    }
    else
    {
      Ragdoll.Bone bone1 = this.GetBone(args.sourceBoneToSplit);
      bone1.meshSplit = args.splitBone;
      bone1.part.slicedMeshRoot = args.splitGameObject.transform;
      bone1.part.slicedMeshRoot.SetParentOrigin(bone1.part.transform);
      this.ragdollRegions.Add(bone1.part.ragdollRegion.SplitFromRegion(bone1.part));
      for (int index1 = 0; index1 < args.sourceBonesToSplit.Length; ++index1)
      {
        Ragdoll.Bone bone2 = this.GetBone(args.sourceBonesToSplit[index1]);
        if ((bool) (UnityEngine.Object) bone2.part)
        {
          if (!bone2.part.isSliced)
          {
            bone2.part.isSliced = true;
            bone2.part.RefreshLayer();
            bone2.SetPinPositionForce(0.0f, 0.0f, 0.0f);
            bone2.SetPinRotationForce(0.0f, 0.0f, 0.0f);
            bone2.part.collisionHandler.RemovePhysicModifier((object) this);
            for (int index2 = 0; index2 < bone2.part.renderers.Count; ++index2)
            {
              foreach (SkinnedMeshRenderer sourceRenderer in args.sourceRenderers)
              {
                if (index2 < bone2.part.renderers.Count && index1 < args.sourceRenderers.Length && (UnityEngine.Object) bone2.part.renderers[index2].renderer == (UnityEngine.Object) args.sourceRenderers[index1])
                {
                  bone2.part.skinnedMeshRenderers.Add(args.splitRenderers[index1]);
                  bone2.part.skinnedMeshRendererIndexes.Add(index2);
                }
              }
            }
          }
          else
            break;
        }
        bone2.meshSplit = args.splitBones[index1];
        bone2.meshSplit.name += "_Split";
        bone2.meshSplit.SetParent((UnityEngine.Object) bone2.mesh.parent == (UnityEngine.Object) bone2.parent.mesh ? bone2.parent.meshSplit : bone2.mesh.parent);
        bone2.meshSplit.localPosition = bone2.mesh.localPosition;
        bone2.meshSplit.localRotation = bone2.mesh.localRotation;
        bone2.meshSplit.localScale = bone2.mesh.localScale;
        if (index1 == 0)
        {
          bone2.mesh.SetParent(bone1.parent.mesh, true);
          bone2.mesh.localPosition = bone1.orgLocalPosition;
          bone2.mesh.localRotation = bone1.orgLocalRotation;
          bone2.mesh.localScale = UnityEngine.Vector3.one;
        }
        else
        {
          bone2.mesh.SetParent(bone2.parent.mesh, true);
          bone2.mesh.localPosition = bone2.orgLocalPosition;
          bone2.mesh.localRotation = bone2.orgLocalRotation;
          bone2.mesh.localScale = UnityEngine.Vector3.one;
        }
      }
      foreach (Creature.RendererData renderer in this.creature.renderers)
      {
        for (int index = 0; index < args.sourceRenderers.Length; ++index)
        {
          if ((UnityEngine.Object) renderer.renderer == (UnityEngine.Object) args.sourceRenderers[index])
          {
            renderer.splitRenderer = args.splitRenderers[index];
            if ((bool) (UnityEngine.Object) renderer.revealDecal && (bool) (UnityEngine.Object) renderer.revealDecal.revealMaterialController)
            {
              RevealMaterialController materialController = renderer.splitRenderer.gameObject.AddComponent<RevealMaterialController>();
              materialController.CopySettingsFrom(renderer.revealDecal.revealMaterialController);
              renderer.splitReveal = renderer.splitRenderer.gameObject.AddComponent<RevealDecal>();
              renderer.splitReveal.maskWidth = renderer.revealDecal.maskWidth;
              renderer.splitReveal.maskHeight = renderer.revealDecal.maskHeight;
              renderer.splitReveal.type = renderer.revealDecal.type;
              renderer.splitReveal.revealMaterialController = materialController;
            }
            if ((bool) (UnityEngine.Object) this.creature.manikinParts)
              this.creature.manikinParts.SetPartDirty(renderer.manikinPart, true);
          }
        }
      }
      if (bone1.part.data != null)
      {
        UnityEngine.Vector3 position1;
        UnityEngine.Vector3 direction;
        bone1.part.GetSlicePositionAndDirection(out position1, out direction);
        if (bone1.part.data.sliceParentEffectData != null)
        {
          EffectInstance effectInstance = bone1.part.data.sliceParentEffectData.Spawn(position1, Quaternion.LookRotation(direction), bone1.parent.mesh.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
          effectInstance.SetIntensity(1f);
          effectInstance.Play();
        }
        if (bone1.part.data.sliceChildEffectData != null)
        {
          UnityEngine.Vector3 position2 = position1;
          UnityEngine.Vector3 forward = -direction;
          if ((bool) (UnityEngine.Object) bone1.part?.slicedMeshRoot)
          {
            position2 = bone1.part.slicedMeshRoot.position;
            forward = bone1.part.slicedMeshRoot.right;
          }
          EffectInstance effectInstance = bone1.part.data.sliceChildEffectData.Spawn(position2, Quaternion.LookRotation(forward), bone1.meshSplit.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
          effectInstance.SetIntensity(1f);
          effectInstance.Play();
        }
      }
      if ((bool) (UnityEngine.Object) Level.current && (bool) (UnityEngine.Object) AreaManager.Instance)
      {
        LightVolumeReceiver lightVolumeReceiver = bone1.part.gameObject.AddComponent<LightVolumeReceiver>();
        lightVolumeReceiver.addMaterialInstances = false;
        lightVolumeReceiver.initRenderersOnStart = false;
        lightVolumeReceiver.currentLightProbeVolume = this.creature.lightVolumeReceiver.currentLightProbeVolume;
        lightVolumeReceiver.SetRenderers(new List<Renderer>((IEnumerable<Renderer>) args.splitRenderers), false);
        lightVolumeReceiver.UpdateRenderers();
        this.creature.lightVolumeReceiver.UpdateRenderers();
      }
      this.sliceRunning = false;
      this.forcePhysic.Add((object) this);
      Ragdoll.SliceEvent onSliceEvent = this.OnSliceEvent;
      if (onSliceEvent != null)
        onSliceEvent(bone1.part, EventTime.OnEnd);
      EventManager.InvokeRagdollSlice(bone1.part, EventTime.OnEnd);
    }
  }

  public bool HasPenetratedPart()
  {
    int count = this.parts.Count;
    for (int index = 0; index < count; ++index)
    {
      if (this.parts[index].IsPenetrated())
        return true;
    }
    return false;
  }

  public enum State
  {
    Inert,
    Destabilized,
    Frozen,
    Standing,
    Kinematic,
    NoPhysic,
    Disabled,
  }

  [Serializable]
  public class Region
  {
    public List<RagdollPart> parts;

    public Region(List<RagdollPart> partsList, bool copy)
    {
      this.parts = copy ? partsList.ToList<RagdollPart>() : partsList;
      for (int index = 0; index < this.parts.Count; ++index)
        this.parts[index].ragdollRegion = this;
    }

    public Ragdoll.Region SplitFromRegion(RagdollPart part)
    {
      List<RagdollPart> partsList = new List<RagdollPart>();
      List<RagdollPart> ragdollPartList = new List<RagdollPart>()
      {
        part
      };
      while (ragdollPartList.Count > 0)
      {
        RagdollPart ragdollPart = ragdollPartList[0];
        partsList.Add(ragdollPart);
        this.parts.Remove(ragdollPart);
        List<RagdollPart> childParts = ragdollPart.childParts;
        // ISSUE: explicit non-virtual call
        if ((childParts != null ? (__nonvirtual (childParts.Count) > 0 ? 1 : 0) : 0) != 0)
          ragdollPartList.AddRange((IEnumerable<RagdollPart>) ragdollPart.childParts);
        ragdollPartList.RemoveAt(0);
      }
      return new Ragdoll.Region(partsList, false);
    }
  }

  public class PhysicModifier
  {
    public object handler;
    public EffectData effectData;
    [NonSerialized]
    public EffectInstance effectInstance;

    public PhysicModifier(object handler, EffectData effectData = null)
    {
      this.handler = handler;
      this.effectData = effectData;
    }
  }

  /// <summary>
  /// Helper class to keep the line of which objects would like to apply a stabilization joint.
  /// </summary>
  private class StabilizationJointQueue
  {
    public GameObject owningObject;
    public Ragdoll.StabilizationJointSettings settings;

    public StabilizationJointQueue(
      GameObject owningObject,
      Ragdoll.StabilizationJointSettings settings)
    {
      this.owningObject = owningObject;
      this.settings = settings;
    }
  }

  /// <summary>
  /// Helper class to assign necessary settings to the joint,
  /// nearly identical to the stabilization joint, but additional stuff might be needed sometimes
  /// </summary>
  public class StabilizationJointSettings
  {
    public UnityEngine.Vector3 axis = UnityEngine.Vector3.zero;
    public ConfigurableJointMotion angularXMotion = ConfigurableJointMotion.Free;
    public ConfigurableJointMotion angularYMotion = ConfigurableJointMotion.Free;
    public ConfigurableJointMotion angularZMotion = ConfigurableJointMotion.Free;
    public bool configuredInWorldSpace;
    public bool autoConfigureConnectedAnchor = true;
    public SoftJointLimit angularXLimit;
    public SoftJointLimitSpring angularXLimitSpring;
    public SoftJointLimit angularYLimit;
    public SoftJointLimit angularZLimit;
    public SoftJointLimitSpring angularYZLimitSpring;
    public JointDrive angularYZDrive;
    public JointDrive angularXDrive;
    public bool isKinematic;
    public GameObject relativeObject;
  }

  public enum PhysicStateChange
  {
    None,
    ParentingToPhysic,
    PhysicToParenting,
  }

  public delegate void StateChange(
    Ragdoll.State previousState,
    Ragdoll.State newState,
    Ragdoll.PhysicStateChange physicStateChange,
    EventTime eventTime);

  public delegate void SliceEvent(RagdollPart ragdollPart, EventTime eventTime);

  public delegate void TouchActionDelegate(
    RagdollHand ragdollHand,
    RagdollPart part,
    Interactable interactable,
    Interactable.Action action);

  public delegate void HeldActionDelegate(
    RagdollHand ragdollHand,
    RagdollPart part,
    HandleRagdoll handle,
    Interactable.Action action);

  public delegate void TelekinesisGrabEvent(
    SpellTelekinesis spellTelekinesis,
    HandleRagdoll handleRagdoll);

  public delegate void TelekinesisReleaseEvent(
    SpellTelekinesis spellTelekinesis,
    HandleRagdoll handleRagdoll,
    bool lastHandler);

  public delegate void GrabEvent(RagdollHand ragdollHand, HandleRagdoll handleRagdoll);

  public delegate void UngrabEvent(
    RagdollHand ragdollHand,
    HandleRagdoll handleRagdoll,
    bool lastHandler);

  public delegate void ContactEvent(CollisionInstance collisionInstance, RagdollPart ragdollPart);

  [Serializable]
  public class Bone
  {
    public int[] boneHashes;
    public Transform mesh;
    public Transform animation;
    public Transform meshSplit;
    public ConfigurableJoint animationJoint;
    public FixedJoint fixedJoint;
    public RagdollPart part;
    public Ragdoll.Bone parent;
    public List<Ragdoll.Bone> childs;
    public bool hasChildAnimationJoint;
    public UnityEngine.Vector3 orgLocalPosition;
    public Quaternion orgLocalRotation;
    public UnityEngine.Vector3 orgCreatureLocalPosition;
    public Quaternion orgCreatureLocalRotation;

    public Bone(Creature creature, Transform mesh, Transform animation, RagdollPart part)
    {
      this.mesh = mesh;
      this.animation = animation;
      this.part = part;
      this.orgLocalPosition = animation.localPosition;
      this.orgLocalRotation = animation.localRotation;
      this.orgCreatureLocalPosition = creature.transform.InverseTransformPoint(animation.position);
      this.orgCreatureLocalRotation = Quaternion.Inverse(animation.rotation) * creature.transform.rotation;
      this.childs = new List<Ragdoll.Bone>();
      if ((bool) (UnityEngine.Object) part)
      {
        this.boneHashes = new int[part.linkedMeshBones.Length + 1];
        this.boneHashes[0] = Animator.StringToHash(mesh.name);
        for (int index = 0; index < part.linkedMeshBones.Length; ++index)
          this.boneHashes[index + 1] = Animator.StringToHash(part.linkedMeshBones[index].name);
        GameObject gameObject = new GameObject("AnimAnchor");
        gameObject.transform.SetParentOrigin(this.animation);
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        rigidbody.detectCollisions = false;
        this.animationJoint = gameObject.AddComponent<ConfigurableJoint>();
        part.transform.SetPositionAndRotation(animation.position, animation.rotation);
        this.animationJoint.autoConfigureConnectedAnchor = false;
        this.animationJoint.connectedAnchor = UnityEngine.Vector3.zero;
        this.animationJoint.SetConnectedPhysicBody(part.physicBody);
        part.bone = this;
        part.OnGrabbed += new RagdollPart.HandlerEvent(this.PartGrabbed);
        part.OnUngrabbed += new RagdollPart.HandlerEvent(this.PartUngrabbed);
        part.OnTKGrab += new RagdollPart.TKHandlerEvent(this.PartTKGrab);
        part.OnTKRelease += new RagdollPart.TKHandlerEvent(this.PartTKRelease);
      }
      else
      {
        this.boneHashes = new int[1];
        this.boneHashes[0] = Animator.StringToHash(mesh.name);
      }
    }

    private void PartGrabbed(RagdollHand ragdollHand, HandleRagdoll handle)
    {
      this.UpdatePartMass();
    }

    private void PartUngrabbed(RagdollHand ragdollHand, HandleRagdoll handle)
    {
      this.UpdatePartMass();
    }

    private void PartTKGrab(SpellTelekinesis spellTelekinesis, HandleRagdoll handle)
    {
      this.UpdatePartMass();
    }

    private void PartTKRelease(SpellTelekinesis spellTelekinesis, HandleRagdoll handle)
    {
      this.UpdatePartMass();
    }

    private void UpdatePartMass()
    {
      List<RagdollHand> handlers = this.part.ragdoll.handlers;
      // ISSUE: explicit non-virtual call
      int count1 = handlers != null ? __nonvirtual (handlers.Count) : 0;
      List<SpellCaster> tkHandlers = this.part.ragdoll.tkHandlers;
      // ISSUE: explicit non-virtual call
      int count2 = tkHandlers != null ? __nonvirtual (tkHandlers.Count) : 0;
      if (count1 + count2 > 0)
        this.part.physicBody.mass = this.part.handledMass;
      else
        this.part.physicBody.mass = Mathf.Lerp(this.part.ragdolledMass, this.part.standingMass, Mathf.Clamp01(this.animationJoint.xDrive.positionSpring / this.part.ragdoll.springPositionForce));
    }

    public List<Ragdoll.Bone> GetAllChilds()
    {
      List<Ragdoll.Bone> allChilds = new List<Ragdoll.Bone>();
      allChilds.Add(this);
      foreach (Ragdoll.Bone child in this.childs)
        allChilds.AddRange((IEnumerable<Ragdoll.Bone>) child.GetAllChilds());
      return allChilds;
    }

    public float GetAnimationBoneHeight()
    {
      return this.part.ragdoll.creature.animator.transform.InverseTransformPointUnscaled(this.animation.position).y;
    }

    public void SetPinPositionForce(float spring, float damper, float maxForce)
    {
      if (this.part.isSliced)
      {
        spring = 0.0f;
        damper = 0.0f;
        maxForce = 0.0f;
      }
      JointDrive jointDrive = new JointDrive();
      jointDrive.positionSpring = spring;
      jointDrive.positionDamper = damper;
      jointDrive.maximumForce = maxForce;
      this.animationJoint.xDrive = jointDrive;
      this.animationJoint.yDrive = jointDrive;
      this.animationJoint.zDrive = jointDrive;
      this.UpdatePartMass();
    }

    public void SetPinRotationForce(
      float spring,
      float damper,
      float maxForce,
      Ragdoll.Region region = null)
    {
      if (region == null)
        region = this.part.ragdoll.rootRegion;
      if (this.part.ragdollRegion != region)
      {
        spring = 0.0f;
        damper = 0.0f;
        maxForce = 0.0f;
      }
      this.animationJoint.rotationDriveMode = RotationDriveMode.Slerp;
      this.animationJoint.slerpDrive = new JointDrive()
      {
        positionSpring = spring,
        positionDamper = damper,
        maximumForce = maxForce
      };
    }

    public void ResetPinForce()
    {
      if (this.part.isSliced)
        return;
      this.SetPinPositionForce(this.part.ragdoll.springPositionForce, this.part.ragdoll.damperPositionForce, this.part.ragdoll.maxPositionForce);
      this.SetPinRotationForce(this.part.ragdoll.springRotationForce, this.part.ragdoll.damperRotationForce, this.part.ragdoll.maxRotationForce);
    }
  }
}
