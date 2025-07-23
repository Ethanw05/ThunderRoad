// Decompiled with JetBrains decompiler
// Type: ThunderRoad.RagdollHand
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RagdollHand")]
[AddComponentMenu("ThunderRoad/Creatures/Ragdoll hand")]
public class RagdollHand : RagdollPart
{
  [Header("Hand")]
  public Side side;
  public RagdollPart lowerArmPart;
  public RagdollPart upperArmPart;
  public bool meshFixedScale = true;
  public UnityEngine.Vector3 meshGlobalScale = UnityEngine.Vector3.one;
  public UnityEngine.Vector3 axisThumb = UnityEngine.Vector3.up;
  public UnityEngine.Vector3 axisPalm = UnityEngine.Vector3.left;
  public Collider touchCollider;
  public WristStats wristStats;
  public RagdollHandPoser poser;
  [Header("Fingers")]
  public RagdollHand.Finger fingerThumb = new RagdollHand.Finger();
  public RagdollHand.Finger fingerIndex = new RagdollHand.Finger();
  public RagdollHand.Finger fingerMiddle = new RagdollHand.Finger();
  public RagdollHand.Finger fingerRing = new RagdollHand.Finger();
  public RagdollHand.Finger fingerLittle = new RagdollHand.Finger();
  public List<RagdollHand.Finger> fingers = new List<RagdollHand.Finger>();
  public Collider palmCollider;
  public Collider simplifiedCollider;
  [NonSerialized]
  public bool fingerMeshParentedToAnim;
  [NonSerialized]
  public bool tempIgnoreOverlap;
  private List<Collider> foreArmColliders;
  [NonSerialized]
  public RagdollHand otherHand;
  [NonSerialized]
  public Creature creature;
  [NonSerialized]
  public Transform grip;
  [NonSerialized]
  public SpellCaster caster;
  [NonSerialized]
  public PlayerHand playerHand;
  public RagdollHandClimb climb;
  public RagdollHandSwim swim;
  protected UnityEngine.Vector3 orgGripLocalPosition;
  protected Quaternion orgGripLocalRotation;
  [NonSerialized]
  public WaterHandler waterHandler;
  [NonSerialized]
  public RagdollHand.ControlPose controlPose;
  public static float punchDetectionAngleThreshold = 20f;
  public static float punchDetectionThreshold = 3f;
  public static float punchStopThreshold = 0.5f;
  [NonSerialized]
  public bool punching;
  [Header("Interactor")]
  public Interactable nearestInteractable;
  public List<Interactable> touchedInteractables;
  [NonSerialized]
  public bool grabBlocked;
  [NonSerialized]
  public bool grabbedWithTrigger;
  public Handle grabbedHandle;
  public Handle.GripInfo gripInfo;
  public Handle lastSliding;
  public Handle lastHandle;
  public bool isHandlingSameObject;
  public float collisionUngrabRadius = 0.13f;
  public float collisionUngrabMinDelay = 0.25f;
  protected Coroutine handOverlapCoroutine;
  protected bool forceTriggerCheck;
  protected Coroutine disableTriggerCheck;
  protected bool pauseCheck;
  protected Handle disabledGrabbedHandle;
  public Handle editorGrabTarget;

  public event RagdollHand.PunchEvent OnPunchStartEvent;

  public event RagdollHand.PunchEvent OnPunchEndEvent;

  public event RagdollHand.PunchHitEvent OnPunchHitEvent;

  /// <summary>The colliders of the linked forearm</summary>
  public List<Collider> ForeArmColliders
  {
    get
    {
      if (this.foreArmColliders != null)
        return this.foreArmColliders;
      if (!(bool) (UnityEngine.Object) this.lowerArmPart)
        return (List<Collider>) null;
      ColliderGroup colliderGroup = this.lowerArmPart.colliderGroup;
      if (!(bool) (UnityEngine.Object) colliderGroup)
        return (List<Collider>) null;
      this.foreArmColliders = colliderGroup.colliders;
      return this.foreArmColliders;
    }
    set => this.foreArmColliders = value;
  }

  public UnityEngine.Vector3 PalmDir => -this.transform.forward;

  public UnityEngine.Vector3 PointDir => -this.transform.right;

  public UnityEngine.Vector3 ThumbDir
  {
    get => this.side != Side.Right ? -this.transform.up : this.transform.up;
  }

  public RagdollHand.Finger GetFinger(HandPoseData.FingerType type)
  {
    switch (type)
    {
      case HandPoseData.FingerType.Thumb:
        return this.fingerThumb;
      case HandPoseData.FingerType.Index:
        return this.fingerIndex;
      case HandPoseData.FingerType.Middle:
        return this.fingerMiddle;
      case HandPoseData.FingerType.Ring:
        return this.fingerRing;
      case HandPoseData.FingerType.Little:
        return this.fingerLittle;
      default:
        throw new ArgumentOutOfRangeException(nameof (type), (object) type, (string) null);
    }
  }

  public event RagdollHand.ControlPoseChangeEvent OnControlPoseChangeEvent;

  protected override void OnValidate()
  {
    base.OnValidate();
    if (!this.gameObject.activeInHierarchy)
      return;
    this.grip = this.transform.Find("Grip");
    if (!(bool) (UnityEngine.Object) this.grip)
      this.grip = this.CreateDefaultGrip();
    if ((UnityEngine.Object) this.creature == (UnityEngine.Object) null)
      this.creature = this.GetComponentInParent<Creature>();
    if (!((UnityEngine.Object) this.poser == (UnityEngine.Object) null))
      return;
    this.poser = this.GetComponent<RagdollHandPoser>();
  }

  public void MirrorFingersToOtherHand()
  {
    if (!(bool) (UnityEngine.Object) this.creature)
      this.creature = this.GetComponentInParent<Creature>();
    foreach (RagdollHand componentsInChild in this.creature.GetComponentsInChildren<RagdollHand>())
    {
      if ((UnityEngine.Object) componentsInChild != (UnityEngine.Object) this)
      {
        this.otherHand = componentsInChild;
        break;
      }
    }
    UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.otherHand.palmCollider.gameObject);
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.palmCollider.gameObject, this.otherHand.transform);
    gameObject.name = this.palmCollider.name;
    gameObject.transform.MirrorChilds(new UnityEngine.Vector3(1f, -1f, 1f));
    foreach (Transform componentsInChild in gameObject.GetComponentsInChildren<Transform>())
      componentsInChild.localScale = UnityEngine.Vector3.one;
    this.otherHand.SetupFingers();
  }

  public virtual void SetGripToDefaultPosition()
  {
    Transform transform = this.transform.Find("Grip");
    if ((bool) (UnityEngine.Object) transform)
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) transform.gameObject);
    this.CreateDefaultGrip();
  }

  public virtual Transform CreateDefaultGrip()
  {
    Transform transform = new GameObject("Grip").transform;
    transform.transform.SetParent(this.transform);
    transform.transform.localScale = UnityEngine.Vector3.one;
    if (this.side == Side.Left)
    {
      transform.transform.localPosition = new UnityEngine.Vector3(-0.042f, -0.01f, 3f / 1000f);
      transform.transform.localRotation = Quaternion.Euler(0.0f, 220f, -90f);
    }
    if (this.side == Side.Right)
    {
      transform.transform.localPosition = new UnityEngine.Vector3(0.042f, -0.01f, 3f / 1000f);
      transform.transform.localRotation = Quaternion.Euler(0.0f, 140f, 90f);
    }
    return transform;
  }

  public virtual void SetupFingers()
  {
    if (!(bool) (UnityEngine.Object) this.creature)
      this.creature = this.GetComponentInParent<Creature>();
    this.fingers = new List<RagdollHand.Finger>();
    this.palmCollider = this.transform.Find("Palm")?.GetComponent<Collider>();
    if ((UnityEngine.Object) this.palmCollider == (UnityEngine.Object) null)
    {
      this.palmCollider = (Collider) new GameObject("Palm").AddComponent<BoxCollider>();
      this.palmCollider.transform.SetParentOrigin(this.transform);
      (this.palmCollider as BoxCollider).size = new UnityEngine.Vector3(0.1f, 0.1f, 0.03f);
      this.palmCollider.gameObject.AddComponent<ColliderGroup>();
    }
    this.fingerThumb.proximal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightThumbProximal : HumanBodyBones.LeftThumbProximal);
    if (!(bool) (UnityEngine.Object) this.fingerThumb.proximal.mesh)
      Debug.LogError((object) "Could not find ThumbProximal bone on animator");
    this.fingerThumb.intermediate.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightThumbIntermediate : HumanBodyBones.LeftThumbIntermediate);
    if (!(bool) (UnityEngine.Object) this.fingerThumb.intermediate.mesh)
      Debug.LogError((object) "Could not find ThumbIntermediate bone on animator");
    this.fingerThumb.distal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightThumbDistal : HumanBodyBones.LeftThumbDistal);
    if (!(bool) (UnityEngine.Object) this.fingerThumb.distal.mesh)
      Debug.LogError((object) "Could not find ThumbDistal bone on animator");
    this.SetupFinger(this.fingerThumb, "Thumb");
    if ((bool) (UnityEngine.Object) this.fingerThumb.proximal.mesh)
      this.fingers.Add(this.fingerThumb);
    this.fingerIndex.proximal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightIndexProximal : HumanBodyBones.LeftIndexProximal);
    if (!(bool) (UnityEngine.Object) this.fingerIndex.proximal.mesh)
      Debug.LogError((object) "Could not find IndexProximal bone on animator");
    this.fingerIndex.intermediate.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightIndexIntermediate : HumanBodyBones.LeftIndexIntermediate);
    if (!(bool) (UnityEngine.Object) this.fingerIndex.intermediate.mesh)
      Debug.LogError((object) "Could not find IndexIntermediate bone on animator");
    this.fingerIndex.distal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightIndexDistal : HumanBodyBones.LeftIndexDistal);
    if (!(bool) (UnityEngine.Object) this.fingerIndex.distal.mesh)
      Debug.LogError((object) "Could not find IndexDistal bone on animator");
    this.SetupFinger(this.fingerIndex, "Index");
    if ((bool) (UnityEngine.Object) this.fingerIndex.proximal.mesh)
      this.fingers.Add(this.fingerIndex);
    this.fingerMiddle.proximal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.LeftMiddleProximal);
    if (!(bool) (UnityEngine.Object) this.fingerMiddle.proximal.mesh)
      Debug.LogError((object) "Could not find MiddleProximal bone on animator");
    this.fingerMiddle.intermediate.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightMiddleIntermediate : HumanBodyBones.LeftMiddleIntermediate);
    if (!(bool) (UnityEngine.Object) this.fingerMiddle.intermediate.mesh)
      Debug.LogError((object) "Could not find MiddleIntermediate bone on animator");
    this.fingerMiddle.distal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightMiddleDistal : HumanBodyBones.LeftMiddleDistal);
    if (!(bool) (UnityEngine.Object) this.fingerMiddle.distal.mesh)
      Debug.LogError((object) "Could not find MiddleDistal bone on animator");
    this.SetupFinger(this.fingerMiddle, "Middle");
    if ((bool) (UnityEngine.Object) this.fingerMiddle.proximal.mesh)
      this.fingers.Add(this.fingerMiddle);
    this.fingerRing.proximal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightRingProximal : HumanBodyBones.LeftRingProximal);
    if (!(bool) (UnityEngine.Object) this.fingerRing.proximal.mesh)
      Debug.LogError((object) "Could not find RingProximal bone on animator");
    this.fingerRing.intermediate.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightRingIntermediate : HumanBodyBones.LeftRingIntermediate);
    if (!(bool) (UnityEngine.Object) this.fingerRing.intermediate.mesh)
      Debug.LogError((object) "Could not find RingIntermediate bone on animator");
    this.fingerRing.distal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightRingDistal : HumanBodyBones.LeftRingDistal);
    if (!(bool) (UnityEngine.Object) this.fingerRing.distal.mesh)
      Debug.LogError((object) "Could not find RingDistal bone on animator");
    this.SetupFinger(this.fingerRing, "Ring");
    if ((bool) (UnityEngine.Object) this.fingerRing.proximal.mesh)
      this.fingers.Add(this.fingerRing);
    this.fingerLittle.proximal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightLittleProximal : HumanBodyBones.LeftLittleProximal);
    if (!(bool) (UnityEngine.Object) this.fingerLittle.proximal.mesh)
      Debug.LogError((object) "Could not find LittleProximal bone on animator");
    this.fingerLittle.intermediate.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightLittleIntermediate : HumanBodyBones.LeftLittleIntermediate);
    if (!(bool) (UnityEngine.Object) this.fingerLittle.intermediate.mesh)
      Debug.LogError((object) "Could not find LittleIntermediate bone on animator");
    this.fingerLittle.distal.mesh = this.creature.animator.GetBoneTransform(this.side == Side.Right ? HumanBodyBones.RightLittleDistal : HumanBodyBones.LeftLittleDistal);
    if (!(bool) (UnityEngine.Object) this.fingerLittle.distal.mesh)
      Debug.LogError((object) "Could not find LittleDistal bone on animator");
    this.SetupFinger(this.fingerLittle, "Little");
    if (!(bool) (UnityEngine.Object) this.fingerLittle.proximal.mesh)
      return;
    this.fingers.Add(this.fingerLittle);
  }

  protected virtual void SetupFinger(RagdollHand.Finger finger, string name)
  {
    if (!(bool) (UnityEngine.Object) finger.proximal.collider)
      finger.proximal.collider = this.palmCollider.transform.Find(name + "Proximal")?.GetComponent<CapsuleCollider>();
    if (!(bool) (UnityEngine.Object) finger.proximal.collider)
    {
      finger.proximal.collider = new GameObject(name + "Proximal").AddComponent<CapsuleCollider>();
      finger.proximal.collider.radius = 0.01f;
      finger.proximal.collider.height = 0.05f;
      finger.proximal.collider.direction = 0;
      finger.proximal.collider.transform.SetParent(this.palmCollider.transform);
    }
    Transform transform1 = finger.proximal.collider.transform;
    transform1.SetPositionAndRotation(finger.proximal.mesh.position, finger.proximal.mesh.rotation);
    finger.proximal.colliderTransform = transform1;
    if (!(bool) (UnityEngine.Object) finger.intermediate.collider)
      finger.intermediate.collider = transform1.Find(name + "Intermediate")?.GetComponent<CapsuleCollider>();
    if (!(bool) (UnityEngine.Object) finger.intermediate.collider)
    {
      finger.intermediate.collider = new GameObject(name + "Intermediate").AddComponent<CapsuleCollider>();
      finger.intermediate.collider.radius = 0.01f;
      finger.intermediate.collider.height = 0.05f;
      finger.intermediate.collider.direction = 0;
      finger.intermediate.collider.transform.SetParent(transform1);
    }
    Transform transform2 = finger.intermediate.collider.transform;
    transform2.SetPositionAndRotation(finger.intermediate.mesh.position, finger.intermediate.mesh.rotation);
    finger.intermediate.colliderTransform = transform2;
    if (!(bool) (UnityEngine.Object) finger.distal.collider)
      finger.distal.collider = transform2.Find(name + "Distal")?.GetComponent<CapsuleCollider>();
    if (!(bool) (UnityEngine.Object) finger.distal.collider)
    {
      finger.distal.collider = new GameObject(name + "Distal").AddComponent<CapsuleCollider>();
      finger.distal.collider.radius = 0.01f;
      finger.distal.collider.height = 0.05f;
      finger.distal.collider.direction = 0;
      finger.distal.collider.transform.SetParent(transform2);
    }
    Transform transform3 = finger.distal.collider.transform;
    transform3.SetPositionAndRotation(finger.distal.mesh.position, finger.distal.mesh.rotation);
    finger.distal.colliderTransform = transform3;
    string str = name + "Tip";
    finger.tip = transform3.Find(str);
    if (!(bool) (UnityEngine.Object) finger.tip)
    {
      finger.tip = new GameObject(str).transform;
      finger.tip.SetParent(transform3);
    }
    finger.tip.localRotation = Quaternion.identity;
    finger.tip.localPosition = UnityEngine.Vector3.zero;
  }

  public static List<RagdollHand.GrabState> UnGrabAllAndSaveState(List<Handle> handles)
  {
    List<RagdollHand.GrabState> grabStateList = new List<RagdollHand.GrabState>();
    for (int index1 = handles.Count - 1; index1 >= 0; --index1)
    {
      for (int index2 = handles[index1].handlers.Count - 1; index2 >= 0; --index2)
      {
        grabStateList.Add(new RagdollHand.GrabState(handles[index1].handlers[index2]));
        handles[index1].handlers[index2].UnGrab(false);
      }
    }
    return grabStateList;
  }

  public static void GrabFromSavedStates(List<RagdollHand.GrabState> grabStates)
  {
    foreach (RagdollHand.GrabState grabState in grabStates)
      grabState.ragdollHand.Grab(grabState.handle, grabState.orientation, grabState.gripAxisPosition);
  }

  protected override void Awake()
  {
    base.Awake();
    this.caster = this.GetComponentInChildren<SpellCaster>();
    this.wristStats = this.parentPart.GetComponentInChildren<WristStats>();
    this.grip = this.transform.Find("Grip");
    if (!(bool) (UnityEngine.Object) this.grip)
      this.grip = this.CreateDefaultGrip();
    this.orgGripLocalPosition = this.grip.localPosition;
    this.orgGripLocalRotation = this.grip.localRotation;
    this.touchedInteractables = new List<Interactable>();
    this.touchCollider.isTrigger = true;
    this.touchCollider.gameObject.layer = GameManager.GetLayer(LayerName.Touch);
    this.touchCollider.enabled = false;
    this.waterHandler = new WaterHandler(true, false);
    this.waterHandler.OnWaterEnter += new WaterHandler.SimpleDelegate(this.OnWaterEnter);
    this.waterHandler.OnWaterExit += new WaterHandler.SimpleDelegate(this.OnWaterExit);
    this.collisionHandler.OnCollisionStartEvent += new CollisionHandler.CollisionEvent(this.OnHandCollision);
  }

  private void OnHandCollision(CollisionInstance hit)
  {
    RagdollHand.PunchHitEvent onPunchHitEvent = this.OnPunchHitEvent;
    if (onPunchHitEvent == null)
      return;
    onPunchHitEvent(this, hit, this.Fist);
  }

  private bool Fist
  {
    get
    {
      return (bool) (UnityEngine.Object) this.playerHand && this.playerHand.controlHand.gripPressed && this.playerHand.controlHand.usePressed;
    }
  }

  protected override void ManagedOnDisable() => this.waterHandler.Reset();

  public override ManagedLoops EnabledManagedLoops
  {
    get => ManagedLoops.FixedUpdate | ManagedLoops.Update | ManagedLoops.LateUpdate;
  }

  protected internal override void ManagedFixedUpdate()
  {
    if (!this.initialized || !(bool) (UnityEngine.Object) this.playerHand)
      return;
    this.climb.FixedUpdate();
    this.swim.FixedUpdate();
    if ((bool) (UnityEngine.Object) this.grabbedHandle)
      this.grabbedHandle.FixedUpdateHandle(this);
    UnityEngine.Vector3 vector3 = this.Velocity();
    if (!this.punching)
    {
      if (!this.Fist || (double) UnityEngine.Vector3.Angle(vector3, this.PointDir) >= (double) RagdollHand.punchDetectionAngleThreshold || (double) vector3.sqrMagnitude <= (double) RagdollHand.punchDetectionThreshold * (double) RagdollHand.punchDetectionThreshold)
        return;
      this.punching = true;
      RagdollHand.PunchEvent onPunchStartEvent = this.OnPunchStartEvent;
      if (onPunchStartEvent == null)
        return;
      onPunchStartEvent(this, vector3);
    }
    else
    {
      if (this.Fist && (double) vector3.sqrMagnitude >= (double) RagdollHand.punchStopThreshold * (double) RagdollHand.punchStopThreshold)
        return;
      this.punching = false;
      RagdollHand.PunchEvent onPunchEndEvent = this.OnPunchEndEvent;
      if (onPunchEndEvent == null)
        return;
      onPunchEndEvent(this, vector3);
    }
  }

  protected internal override void ManagedUpdate()
  {
    if (!this.initialized)
      return;
    this.UpdateClimb();
    this.UpdateWater();
    if ((bool) (UnityEngine.Object) this.grabbedHandle)
      this.grabbedHandle.UpdateHandle(this);
    this.UpdatePoseInformation();
  }

  private void UpdatePoseInformation()
  {
    RagdollHand.ControlPose controlPose = this.controlPose;
    this.controlPose = !((UnityEngine.Object) this.grabbedHandle != (UnityEngine.Object) null) ? (!((UnityEngine.Object) this.playerHand != (UnityEngine.Object) null) || this.playerHand.controlHand == null ? RagdollHand.ControlPose.Open : (RagdollHand.ControlPose) (1 + (this.playerHand.controlHand.gripPressed ? 1 : 0) + (this.playerHand.controlHand.usePressed ? 2 : 0))) : RagdollHand.ControlPose.HandPose;
    if (controlPose == this.controlPose || this.OnControlPoseChangeEvent == null)
      return;
    this.OnControlPoseChangeEvent(this.side, controlPose, this.controlPose);
  }

  protected internal override void ManagedLateUpdate()
  {
    if (this.initialized && !this.pauseCheck && this.touchedInteractables.Count > 0)
      this.CheckInteractablesStillTouched();
    this.pauseCheck = false;
  }

  public override void Init(Ragdoll ragdoll)
  {
    base.Init(ragdoll);
    this.creature = ragdoll.creature;
    this.creature.ragdoll.OnStateChange += new Ragdoll.StateChange(this.OnRagdollStateChange);
    RenderPipelineManager.beginContextRendering += new Action<ScriptableRenderContext, List<Camera>>(this.UpdateHandleSync);
    foreach (RagdollHand componentsInChild in ragdoll.GetComponentsInChildren<RagdollHand>())
    {
      if ((UnityEngine.Object) componentsInChild != (UnityEngine.Object) this)
      {
        this.otherHand = componentsInChild;
        break;
      }
    }
    this.climb = new RagdollHandClimb(this);
    this.swim.Init(this);
    this.SetFingerColliders(false);
  }

  private void UpdateHandleSync(ScriptableRenderContext arg1, List<Camera> arg2)
  {
    if (this.ragdoll.creature.isPlayer || !(bool) (UnityEngine.Object) this.grabbedHandle || this.gripInfo.type != Handle.GripInfo.Type.HandSync)
      return;
    this.grabbedHandle.MoveAndAlignToHand(this);
  }

  public void InitAfterBoneInit()
  {
    foreach (RagdollHand.Finger finger in this.fingers)
    {
      if ((bool) (UnityEngine.Object) finger.proximal.mesh)
        finger.proximal.animation = this.ragdoll.GetBone(finger.proximal.mesh).animation;
      else
        Debug.LogError((object) "A finger mesh reference is missing, did you setup fingers on RagdollHand?");
      if ((bool) (UnityEngine.Object) finger.intermediate.mesh)
        finger.intermediate.animation = this.ragdoll.GetBone(finger.intermediate.mesh).animation;
      else
        Debug.LogError((object) "A finger mesh reference is missing, did you setup fingers on RagdollHand?");
      if ((bool) (UnityEngine.Object) finger.distal.mesh)
        finger.distal.animation = this.ragdoll.GetBone(finger.distal.mesh).animation;
      else
        Debug.LogError((object) "A finger mesh reference is missing, did you setup fingers on RagdollHand?");
    }
    this.RefreshFingerMeshParent(true);
  }

  public override void Load()
  {
    base.Load();
    if (!(bool) (UnityEngine.Object) this.poser)
      return;
    this.poser.ResetDefaultPose();
    this.poser.ResetTargetPose();
  }

  private void OnWaterEnter()
  {
    this.caster.RefreshWater();
    this.swim.OnWaterEnter();
  }

  private void UpdateWater()
  {
    if (!(bool) (UnityEngine.Object) this.playerHand)
      return;
    this.waterHandler.Update(this.grip.position, this.grip.position.y - this.collisionUngrabRadius * 0.5f, this.grip.position.y + this.collisionUngrabRadius * 0.5f, this.collisionUngrabRadius, (bool) (UnityEngine.Object) this.grabbedHandle ? this.grabbedHandle.physicBody.velocity : this.physicBody.velocity);
    if (!this.waterHandler.inWater)
      return;
    if (this.climb.isGripping)
      this.playerHand.link.RemoveJointModifier((object) this);
    else
      this.playerHand.link.SetJointModifier((object) this, this.creature.data.waterHandSpringMultiplierCurve.Evaluate(this.waterHandler.submergedRatio), locomotionVelocityCorrectionMultiplier: this.creature.data.waterHandLocomotionVelocityCorrectionMultiplier);
  }

  private void OnWaterExit()
  {
    if ((bool) (UnityEngine.Object) this.playerHand)
      this.playerHand.link.RemoveJointModifier((object) this);
    this.caster.RefreshWater();
    this.swim.OnWaterExit();
  }

  /// <summary>
  /// Play a single haptic tick on the hand. Only works if this is the player's hand
  /// </summary>
  public void HapticTick(float intensity = 1f, bool oneFrameCooldown = false)
  {
    if (!(bool) (UnityEngine.Object) this.playerHand)
      return;
    this.playerHand.controlHand.HapticShort(intensity, oneFrameCooldown);
  }

  /// <summary>
  /// Play a haptic clip based on an animation curve over a duration
  /// </summary>
  public void PlayHapticClipOver(AnimationCurve curve, float duration)
  {
    if (!(bool) (UnityEngine.Object) this.playerHand)
      return;
    this.StartCoroutine(this.HapticPlayer(curve, duration));
  }

  protected IEnumerator HapticPlayer(AnimationCurve curve, float duration)
  {
    float time = Time.time;
    while ((double) Time.time - (double) time < (double) duration)
    {
      this.HapticTick(curve.Evaluate((Time.time - time) / duration));
      yield return (object) 0;
    }
  }

  /// <summary>
  /// Return the velocity of the hand. Only works if this is the player's hand.
  /// </summary>
  public UnityEngine.Vector3 Velocity()
  {
    if (!this.creature.isPlayer)
      return UnityEngine.Vector3.zero;
    try
    {
      return Player.local.transform.rotation * this.playerHand.controlHand.GetHandVelocity();
    }
    catch (NullReferenceException ex)
    {
      return UnityEngine.Vector3.zero;
    }
  }

  public void SetFingerColliders(bool enabled)
  {
    this.palmCollider.enabled = enabled;
    foreach (RagdollHand.Finger finger in this.fingers)
    {
      finger.distal.collider.enabled = enabled;
      finger.intermediate.collider.enabled = enabled;
      finger.proximal.collider.enabled = enabled;
    }
    this.simplifiedCollider.enabled = !enabled;
  }

  public override void OnRagdollEnable()
  {
    base.OnRagdollEnable();
    if (!(bool) (UnityEngine.Object) this.disabledGrabbedHandle)
      return;
    this.disabledGrabbedHandle.item.gameObject.SetActive(true);
    this.Grab(this.disabledGrabbedHandle, true);
    this.disabledGrabbedHandle = (Handle) null;
  }

  public override void OnRagdollDisable()
  {
    base.OnRagdollDisable();
    if (this.creature.gameObject.activeInHierarchy || !(bool) (UnityEngine.Object) this.grabbedHandle || !(bool) (UnityEngine.Object) this.grabbedHandle.item)
      return;
    this.disabledGrabbedHandle = this.grabbedHandle;
    this.UnGrab(false);
    this.disabledGrabbedHandle.item.gameObject.SetActive(false);
  }

  protected void OnRagdollStateChange(
    Ragdoll.State previousState,
    Ragdoll.State newState,
    Ragdoll.PhysicStateChange physicStateChange,
    EventTime eventTime)
  {
    if (eventTime == EventTime.OnStart && (bool) (UnityEngine.Object) this.grabbedHandle && !Ragdoll.IsPhysicalState(newState))
    {
      for (int index = this.grabbedHandle.handlers.Count - 1; index >= 0; --index)
      {
        if (this.grabbedHandle.handlers[index].gripInfo.type == Handle.GripInfo.Type.PlayerJoint)
          this.grabbedHandle.handlers[index].UnGrab(false);
      }
    }
    if (eventTime != EventTime.OnEnd)
      return;
    if ((bool) (UnityEngine.Object) this.grabbedHandle && (this.gripInfo.type == Handle.GripInfo.Type.HandJoint || this.gripInfo.type == Handle.GripInfo.Type.HandSync) && physicStateChange != Ragdoll.PhysicStateChange.None)
    {
      bool flag = Ragdoll.IsPhysicalState(newState);
      this.grabbedHandle.Attach(this, flag);
      this.grabbedHandle.item?.SetColliders(flag);
    }
    this.RefreshFingerMeshParent(true);
  }

  public void RefreshFingerMeshParent(bool force = false)
  {
    if (!(bool) (UnityEngine.Object) this.poser)
      this.AttachFingerMeshBoneToAnimation(force);
    else if (!this.ragdoll.creature.isPlayer && !(bool) (UnityEngine.Object) this.grabbedHandle)
      this.AttachFingerMeshBoneToAnimation(force);
    else
      this.AttachFingerMeshBoneToRagdoll(force);
  }

  public void AttachFingerMeshBoneToRagdoll(bool force = false)
  {
    if (!force && !this.fingerMeshParentedToAnim)
      return;
    foreach (RagdollHand.Finger finger in this.fingers)
    {
      finger.proximal.mesh.SetParentOrigin(finger.proximal.collider.transform);
      finger.intermediate.mesh.SetParentOrigin(finger.intermediate.collider.transform);
      finger.distal.mesh.SetParentOrigin(finger.distal.collider.transform);
    }
    this.fingerMeshParentedToAnim = false;
  }

  public void AttachFingerMeshBoneToAnimation(bool force = false)
  {
    if (!force && this.fingerMeshParentedToAnim)
      return;
    foreach (RagdollHand.Finger finger in this.fingers)
    {
      finger.proximal.mesh.SetParentOrigin(finger.proximal.animation);
      finger.intermediate.mesh.SetParentOrigin(finger.intermediate.animation);
      finger.distal.mesh.SetParentOrigin(finger.distal.animation);
    }
    this.fingerMeshParentedToAnim = true;
  }

  public void ResetGripPositionAndRotation()
  {
    this.grip.localPosition = this.orgGripLocalPosition;
    this.grip.localRotation = this.orgGripLocalRotation;
    this.grip.localScale = UnityEngine.Vector3.one;
  }

  public float GetArmLenghtRatio(bool XZOnly)
  {
    UnityEngine.Vector3 position = this.bone.animation.position;
    return XZOnly ? this.upperArmPart.bone.animation.InverseTransformPoint(new UnityEngine.Vector3(position.x, this.upperArmPart.bone.animation.position.y, position.z)).magnitude : this.upperArmPart.bone.animation.InverseTransformPoint(position).magnitude / this.creature.morphology.armsLength;
  }

  private void OnTriggerEnter(Collider other)
  {
    this.OnInteractorTriggerEnter(other);
    this.climb.OnTriggerEnter(other);
  }

  private void OnTriggerStay(Collider other)
  {
    this.OnInteractorTriggerStay(other);
    this.climb.OnTriggerStay(other);
  }

  private void OnTriggerExit(Collider other) => this.OnInteractorTriggerExit(other);

  protected void OnCollisionEnter(UnityEngine.Collision collision)
  {
    if (!this.ragdoll.creature.isPlayer)
      return;
    this.climb.OnCollisionEnter(collision);
  }

  protected void OnCollisionStay(UnityEngine.Collision collision)
  {
    if (!this.ragdoll.creature.isPlayer)
      return;
    this.climb.OnCollisionStay(collision);
  }

  protected void UpdateClimb() => this.climb.Update();

  public event RagdollHand.TouchHoverEvent OnTouchNewInteractable;

  public event RagdollHand.HoverEndEvent OnStopTouchInteractable;

  public event RagdollHand.GrabEvent OnGrabEvent;

  public event RagdollHand.UnGrabEvent OnUnGrabEvent;

  private void OnInteractorTriggerEnter(Collider other)
  {
    if (other.gameObject.layer != GameManager.GetLayer(LayerName.TouchObject) && other.gameObject.layer != GameManager.GetLayer(LayerName.Touch))
      return;
    this.pauseCheck = true;
    Interactable component;
    if (other.TryGetComponent<Interactable>(out component) && component.CanTouch(this) && !this.TouchedInteractablesContain(component))
    {
      if (component is Holder holder)
      {
        Item parentItem = holder.parentItem;
        if (parentItem != null && (UnityEngine.Object) parentItem == (UnityEngine.Object) this.grabbedHandle?.item)
          goto label_8;
      }
      Interactable.InteractionResult interactionResult = component.CheckInteraction(this);
      if (interactionResult.isInteractable || interactionResult.showHint)
        this.touchedInteractables.Add(component);
      RagdollHand.TouchHoverEvent touchNewInteractable = this.OnTouchNewInteractable;
      if (touchNewInteractable != null)
        touchNewInteractable(this.side, component, interactionResult);
    }
label_8:
    this.CheckTouch();
  }

  private void OnInteractorTriggerExit(Collider other)
  {
    if (other.gameObject.layer != GameManager.GetLayer(LayerName.TouchObject) && other.gameObject.layer != GameManager.GetLayer(LayerName.Touch))
      return;
    Interactable component;
    if (other.TryGetComponent<Interactable>(out component) && this.TouchedInteractablesContain(component))
    {
      this.touchedInteractables.Remove(component);
      RagdollHand.HoverEndEvent touchInteractable = this.OnStopTouchInteractable;
      if (touchInteractable != null)
        touchInteractable(this.side, component);
    }
    this.CheckTouch();
  }

  private void OnInteractorTriggerStay(Collider other)
  {
    if (other.gameObject.layer != GameManager.GetLayer(LayerName.TouchObject) && other.gameObject.layer != GameManager.GetLayer(LayerName.Touch))
      return;
    this.pauseCheck = true;
    if (this.forceTriggerCheck)
    {
      this.OnTriggerEnter(other);
      if (this.ragdoll.creature.isPlayer)
      {
        if (this.disableTriggerCheck != null)
          return;
        this.disableTriggerCheck = this.StartCoroutine(this.WaitDisableTriggerCheck());
      }
      else
        this.forceTriggerCheck = false;
    }
    else
      this.CheckTouch();
  }

  protected IEnumerator WaitDisableTriggerCheck()
  {
    yield return (object) Yielders.EndOfFrame;
    this.forceTriggerCheck = false;
    this.disableTriggerCheck = (Coroutine) null;
  }

  protected bool TouchedInteractablesContain(Interactable interactable)
  {
    foreach (UnityEngine.Object touchedInteractable in this.touchedInteractables)
    {
      if (touchedInteractable == (UnityEngine.Object) interactable)
        return true;
    }
    return false;
  }

  private void CheckTouch()
  {
    if (this.touchedInteractables.Count > 0)
    {
      if ((UnityEngine.Object) this.caster?.telekinesis?.targetHandle != (UnityEngine.Object) null)
        this.caster.telekinesis.StopTargeting();
      for (int index = this.touchedInteractables.Count - 1; index >= 0; --index)
      {
        if ((UnityEngine.Object) this.touchedInteractables[index] == (UnityEngine.Object) null)
          this.touchedInteractables.RemoveAt(index);
      }
    }
    if (this.touchedInteractables.Count == 0)
    {
      if (!(bool) (UnityEngine.Object) this.nearestInteractable)
        return;
      this.nearestInteractable.OnTouchEnd(this);
      this.nearestInteractable = (Interactable) null;
    }
    else if (this.touchedInteractables.Count == 1)
    {
      if ((UnityEngine.Object) this.nearestInteractable != (UnityEngine.Object) this.touchedInteractables[0])
      {
        if ((bool) (UnityEngine.Object) this.nearestInteractable)
          this.nearestInteractable.OnTouchEnd(this);
        this.nearestInteractable = this.touchedInteractables[0];
        if ((bool) (UnityEngine.Object) this.nearestInteractable)
          this.nearestInteractable.OnTouchStart(this);
      }
      if (!(bool) (UnityEngine.Object) this.nearestInteractable)
        return;
      this.nearestInteractable.OnTouchStay(this);
    }
    else
    {
      Interactable interactable = this.touchedInteractables[0];
      if (this.touchedInteractables.Count > 1)
      {
        float num1 = (((double) interactable.axisLength > 0.0 ? interactable.GetNearestPositionAlongAxis(this.GetReferencePoint(interactable).position) : interactable.transform.position) - this.GetReferencePoint(interactable).position).sqrMagnitude + interactable.artificialDistance;
        int count = this.touchedInteractables.Count;
        for (int index = 1; index < count; ++index)
        {
          Interactable touchedInteractable = this.touchedInteractables[index];
          if (touchedInteractable.data == null || !touchedInteractable.data.ignoreWhenTouchConflict)
          {
            float num2 = (((double) touchedInteractable.axisLength > 0.0 ? touchedInteractable.GetNearestPositionAlongAxis(this.GetReferencePoint(touchedInteractable).position) : touchedInteractable.transform.position) - this.GetReferencePoint(touchedInteractable).position).sqrMagnitude + touchedInteractable.artificialDistance;
            if ((double) num2 < (double) num1)
            {
              interactable = touchedInteractable;
              num1 = num2;
            }
          }
        }
      }
      if ((UnityEngine.Object) interactable != (UnityEngine.Object) this.nearestInteractable)
      {
        this.nearestInteractable?.OnTouchEnd(this);
        this.nearestInteractable = interactable;
        this.nearestInteractable.OnTouchStart(this);
      }
      foreach (Interactable touchedInteractable in this.touchedInteractables)
        touchedInteractable.OnTouchStay(this);
    }
  }

  public void RefreshTouch()
  {
    this.nearestInteractable?.OnTouchEnd(this);
    this.nearestInteractable?.OnTouchStart(this);
  }

  protected Transform GetReferencePoint(Interactable interactable)
  {
    if (interactable.data != null)
    {
      if (interactable.data.referencePoint == InteractableData.ReferencePoint.Grip)
        return this.grip;
      if (interactable.data.referencePoint == InteractableData.ReferencePoint.IndexTip)
        return this.fingerIndex.tip;
    }
    return this.transform;
  }

  public void CheckInteractablesStillTouched()
  {
    bool flag = false;
    for (int i = this.touchedInteractables.Count - 1; i >= 0; i--)
    {
      Interactable touchedInteractable = this.touchedInteractables[i];
      if ((UnityEngine.Object) touchedInteractable == (UnityEngine.Object) null || this.InteractableStoppedTouching(touchedInteractable, false))
      {
        this.RemoveInteractable(touchedInteractable, (System.Action) (() => this.touchedInteractables.RemoveAt(i)));
        flag = true;
      }
    }
    if (!flag)
      return;
    this.CheckTouch();
  }

  public bool InteractableStoppedTouching(Interactable interactable, bool remove = true)
  {
    if (!(bool) (UnityEngine.Object) interactable || !this.touchedInteractables.Contains(interactable))
      return false;
    bool flag = (UnityEngine.Object) interactable.touchCollider == (UnityEngine.Object) null || !interactable.touchCollider.enabled || !interactable.touchCollider.gameObject.activeInHierarchy;
    if (!flag)
      flag = !this.touchCollider.bounds.Intersects(interactable.touchCollider.bounds);
    if (!flag)
      return false;
    if (remove)
      this.RemoveInteractable(interactable, (System.Action) (() => this.touchedInteractables.Remove(interactable)));
    return true;
  }

  public void RemoveInteractable(Interactable interactable, System.Action remove)
  {
    remove();
    if (!(bool) (UnityEngine.Object) interactable)
      return;
    interactable.OnTouchEnd(this);
    RagdollHand.HoverEndEvent touchInteractable = this.OnStopTouchInteractable;
    if (touchInteractable != null)
      touchInteractable(this.side, interactable);
    if (!((UnityEngine.Object) interactable == (UnityEngine.Object) this.nearestInteractable))
      return;
    this.nearestInteractable = (Interactable) null;
  }

  public virtual void ClearTouch()
  {
    foreach (Interactable touchedInteractable in this.touchedInteractables)
      touchedInteractable.OnTouchEnd(this);
    this.touchedInteractables.Clear();
    this.nearestInteractable = (Interactable) null;
    this.forceTriggerCheck = true;
  }

  public virtual void SetBlockGrab(bool active, bool blockTK = true)
  {
    if (blockTK && this.caster?.telekinesis != null)
      this.caster.telekinesis.tkBlocked = active;
    if (active)
    {
      this.ClearTouch();
      this.touchCollider.enabled = false;
      this.grabBlocked = true;
    }
    else
    {
      this.touchCollider.enabled = true;
      this.grabBlocked = false;
    }
  }

  public virtual bool IsGrabbingOrTK() => this.IsGrabbingOrTK(out Handle _);

  public virtual bool IsGrabbingOrTK(out Handle handle)
  {
    handle = this.grabbedHandle;
    if ((UnityEngine.Object) handle != (UnityEngine.Object) null)
      return true;
    handle = this.caster?.telekinesis?.catchedHandle;
    return (UnityEngine.Object) handle != (UnityEngine.Object) null;
  }

  public virtual bool IsGrabbingOrTK(Handle handle)
  {
    Handle handle1;
    return this.IsGrabbingOrTK(out handle1) && (UnityEngine.Object) handle1 == (UnityEngine.Object) handle;
  }

  public virtual bool IsGrabbingOrTK(ThunderEntity entity)
  {
    Handle handle;
    return this.IsGrabbingOrTK(out handle) && (handle is HandleRagdoll handleRagdoll && (UnityEngine.Object) handleRagdoll.ragdollPart.ragdoll.creature == (UnityEngine.Object) entity || (UnityEngine.Object) handle.item == (UnityEngine.Object) entity);
  }

  public virtual bool IsTK() => this.IsTK(out Handle _);

  public virtual bool IsTK(out Handle handle)
  {
    handle = this.caster?.telekinesis?.catchedHandle;
    return (UnityEngine.Object) handle != (UnityEngine.Object) null;
  }

  public virtual ThunderEntity GetAttachedEntity()
  {
    if ((UnityEngine.Object) this.grabbedHandle?.item != (UnityEngine.Object) null)
      return (ThunderEntity) this.grabbedHandle.item;
    if (this.climb.isGripping)
    {
      Item gripItem = this.climb.gripItem;
      if (gripItem != null)
        return (ThunderEntity) gripItem;
      Creature creature = this.climb.gripRagdollPart?.ragdoll?.creature;
      if (creature != null)
        return (ThunderEntity) creature;
      return this.climb.gripPhysicBody?.gameObject.GetComponentInParent<ThunderEntity>();
    }
    if (this.grabbedHandle is HandleRagdoll grabbedHandle1)
      return (ThunderEntity) grabbedHandle1.ragdollPart.ragdoll.creature;
    RagdollPart component1 = (RagdollPart) null;
    Handle grabbedHandle2 = this.grabbedHandle;
    int num;
    if (grabbedHandle2 == null)
    {
      num = 0;
    }
    else
    {
      bool? component2 = grabbedHandle2.physicBody?.gameObject?.TryGetComponent<RagdollPart>(out component1);
      bool flag = true;
      num = component2.GetValueOrDefault() == flag & component2.HasValue ? 1 : 0;
    }
    return num != 0 ? (ThunderEntity) component1.ragdoll.creature : (ThunderEntity) null;
  }

  public virtual bool IsAttachedToEntity(out ThunderEntity heldEntity)
  {
    heldEntity = this.GetAttachedEntity();
    return (UnityEngine.Object) heldEntity != (UnityEngine.Object) null;
  }

  public virtual bool IsAttachedToEntity(ThunderEntity entity)
  {
    ThunderEntity heldEntity;
    return this.IsAttachedToEntity(out heldEntity) && (UnityEngine.Object) heldEntity == (UnityEngine.Object) entity;
  }

  public virtual bool TryAction(Interactable.Action action)
  {
    if ((bool) (UnityEngine.Object) this.nearestInteractable && this.nearestInteractable.TryTouchAction(this, action))
      return true;
    if (!(bool) (UnityEngine.Object) this.grabbedHandle || !this.grabbedHandle.HeldActionAvailable(this, action))
      return false;
    this.grabbedHandle.HeldAction(this, action);
    return true;
  }

  public virtual void EditorGrab() => this.Grab(this.editorGrabTarget);

  public virtual void EditorUngrab()
  {
    if (!(bool) (UnityEngine.Object) this.grabbedHandle)
      return;
    this.UnGrab(true);
  }

  /// <summary>
  /// Grab object relative to current hand grip position and rotation. Warning: Need to be used by local player only to be accurate!
  /// </summary>
  /// <param name="handle">Handle to grab</param>
  public virtual void GrabRelative(Handle handle, bool withTrigger = false)
  {
    this.Grab(handle, handle.GetNearestOrientation(this.grip, this.side), (double) handle.axisLength > 0.0 ? handle.GetNearestAxisPosition(this.grip.position) : 0.0f, handle.data.alwaysTeleportToHand, withTrigger);
  }

  public virtual void Grab(Handle handle)
  {
    this.Grab(handle, handle.GetDefaultOrientation(this.side), handle.GetDefaultAxisLocalPosition(), handle.data.alwaysTeleportToHand);
  }

  public virtual void Grab(Handle handle, bool teleportToHand, bool withTrigger = false)
  {
    this.Grab(handle, handle.GetDefaultOrientation(this.side), handle.GetDefaultAxisLocalPosition(), teleportToHand, withTrigger);
  }

  public virtual void Grab(
    Handle handle,
    HandlePose orientation,
    float axisPosition,
    bool teleportToHand = false,
    bool withTrigger = false)
  {
    if (handle.handlers.Contains(this))
      Debug.LogError((object) "Trying to Grab two time with the same ragdollHand!");
    else if ((UnityEngine.Object) orientation == (UnityEngine.Object) null)
    {
      Debug.LogError((object) $"Trying to grab a handle with no orientation. Cannot grab {handle.name}!");
    }
    else
    {
      this.grabbedWithTrigger = withTrigger;
      if (this.OnGrabEvent != null)
        this.OnGrabEvent(this.side, handle, axisPosition, orientation, EventTime.OnStart);
      if ((UnityEngine.Object) this.playerHand != (UnityEngine.Object) null)
      {
        WheelMenu wheelMenu = this.side == Side.Left ? (WheelMenu) WheelMenuSpell.left : (WheelMenu) WheelMenuSpell.right;
        if (wheelMenu.isShown && !handle.data.allowSpellMenu)
          wheelMenu.Hide();
      }
      handle.OnGrab(this, axisPosition, orientation, teleportToHand);
      this.RefreshTwoHanded();
      this.ClearTouch();
      if (this.handOverlapCoroutine != null)
        this.StopCoroutine(this.handOverlapCoroutine);
      this.creature.UpdateHeldImbues();
      this.RefreshFingerMeshParent();
      RagdollHand.GrabEvent onGrabEvent = this.OnGrabEvent;
      if (onGrabEvent == null)
        return;
      onGrabEvent(this.side, handle, axisPosition, orientation, EventTime.OnEnd);
    }
  }

  public virtual void UnGrab(bool throwing)
  {
    if ((bool) (UnityEngine.Object) this.grabbedHandle)
    {
      this.grabbedWithTrigger = false;
      Handle grabbedHandle = this.grabbedHandle;
      if ((bool) (UnityEngine.Object) grabbedHandle.item && this.gameObject.activeInHierarchy)
        this.StartCoroutine(this.PreventPierceSelf(grabbedHandle.item));
      if (this.OnUnGrabEvent != null)
        this.OnUnGrabEvent(this.side, grabbedHandle, throwing, EventTime.OnStart);
      grabbedHandle.OnUnGrab(this, throwing);
      this.RefreshTwoHanded();
      this.ClearTouch();
      if ((bool) (UnityEngine.Object) this.playerHand && (bool) grabbedHandle.physicBody && !this.tempIgnoreOverlap)
      {
        if (this.handOverlapCoroutine != null)
          this.StopCoroutine(this.handOverlapCoroutine);
        this.handOverlapCoroutine = this.StartCoroutine(this.HandOverlapCoroutine(grabbedHandle));
      }
      this.lastSliding = (Handle) null;
      this.creature.UpdateHeldImbues();
      this.RefreshFingerMeshParent();
      if (this.OnUnGrabEvent != null)
        this.OnUnGrabEvent(this.side, grabbedHandle, throwing, EventTime.OnEnd);
      if (!throwing)
        return;
      this.creature.InvokeOnThrowEvent(this, grabbedHandle);
    }
    else
      Debug.LogError((object) "Trying to ungrab nothing!");
  }

  private IEnumerator PreventPierceSelf(Item item)
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    RagdollHand part = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      item.AllowPenetration((RagdollPart) part);
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    item.PreventPenetration((RagdollPart) part);
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = (object) Yielders.ForSeconds(1f);
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }

  private IEnumerator HandOverlapCoroutine(Handle handle)
  {
    RagdollHand ragdollHand = this;
    if ((bool) (UnityEngine.Object) handle.item)
    {
      RagdollPart.Type type = ragdollHand.type | ragdollHand.parentPart.type;
      for (int index = handle.item.handlers.Count - 1; index >= 0; --index)
      {
        RagdollHand handler = handle.item.handlers[index];
        if (!((UnityEngine.Object) handler.ragdoll != (UnityEngine.Object) ragdollHand.ragdoll))
          type = type | handler.type | handler.parentPart.type;
      }
      handle.item.IgnoreRagdollCollision(ragdollHand.ragdoll, ~type);
    }
    else if (handle is HandleRagdoll)
    {
      foreach (Collider collider in ragdollHand.colliderGroup.colliders)
        (handle as HandleRagdoll).ragdollPart.ragdoll.IgnoreCollision(collider, true);
    }
    foreach (Collider handOverlapCollider in handle.handOverlapColliders)
    {
      foreach (Collider collider in ragdollHand.colliderGroup.colliders)
        Physics.IgnoreCollision(handOverlapCollider, collider, true);
    }
    LayerMask ungrabLayerMask = ragdollHand.playerHand.collisionUngrabLayerMask;
    yield return (object) Yielders.ForSeconds(ragdollHand.collisionUngrabMinDelay);
    bool isOverlapped = true;
    while (isOverlapped)
    {
      isOverlapped = Physics.CheckSphere(ragdollHand.grip.position, ragdollHand.collisionUngrabRadius, (int) ungrabLayerMask, QueryTriggerInteraction.Ignore);
      yield return (object) Yielders.FixedUpdate;
    }
    if ((bool) (UnityEngine.Object) handle.item)
      handle.item.RefreshCollision();
    else if (handle is HandleRagdoll)
    {
      foreach (Collider collider in ragdollHand.colliderGroup.colliders)
        (handle as HandleRagdoll).ragdollPart.ragdoll.IgnoreCollision(collider, false);
    }
    foreach (Collider handOverlapCollider in handle.handOverlapColliders)
    {
      foreach (Collider collider in ragdollHand.colliderGroup.colliders)
        Physics.IgnoreCollision(handOverlapCollider, collider, false);
    }
  }

  public virtual bool TryRelease()
  {
    if (!(bool) (UnityEngine.Object) this.grabbedHandle)
      return false;
    Handle grabbedHandle = this.grabbedHandle;
    this.UnGrab(false);
    if ((bool) (UnityEngine.Object) grabbedHandle.item)
      grabbedHandle.item.isFlying = false;
    return true;
  }

  protected virtual void RefreshTwoHanded()
  {
    if ((bool) (UnityEngine.Object) this.grabbedHandle && (bool) (UnityEngine.Object) this.otherHand?.grabbedHandle)
    {
      if ((bool) (UnityEngine.Object) this.grabbedHandle.item && (bool) (UnityEngine.Object) this.otherHand.grabbedHandle.item && (UnityEngine.Object) this.grabbedHandle.item == (UnityEngine.Object) this.otherHand.grabbedHandle.item)
      {
        this.isHandlingSameObject = this.otherHand.isHandlingSameObject = true;
        return;
      }
      if ((UnityEngine.Object) this.grabbedHandle == (UnityEngine.Object) this.otherHand.grabbedHandle)
      {
        this.isHandlingSameObject = this.otherHand.isHandlingSameObject = true;
        return;
      }
    }
    this.isHandlingSameObject = false;
    if (!(bool) (UnityEngine.Object) this.otherHand)
      return;
    this.otherHand.isHandlingSameObject = false;
  }

  public delegate void PunchEvent(RagdollHand hand, UnityEngine.Vector3 velocity);

  public delegate void PunchHitEvent(RagdollHand hand, CollisionInstance hit, bool fist);

  [Serializable]
  public class Finger
  {
    public RagdollHand.Finger.Bone proximal = new RagdollHand.Finger.Bone();
    public RagdollHand.Finger.Bone intermediate = new RagdollHand.Finger.Bone();
    public RagdollHand.Finger.Bone distal = new RagdollHand.Finger.Bone();
    public Transform tip;

    [Serializable]
    public class Bone
    {
      public Transform mesh;
      public Transform animation;
      public CapsuleCollider collider;
      public Transform colliderTransform;
    }
  }

  public enum ControlPose
  {
    HandPose,
    Open,
    Point,
    Trigger,
    Fist,
  }

  public delegate void ControlPoseChangeEvent(
    Side side,
    RagdollHand.ControlPose previousPose,
    RagdollHand.ControlPose newPose);

  public class GrabState
  {
    public RagdollHand ragdollHand;
    public Handle handle;
    public float gripAxisPosition;
    public HandlePose orientation;

    public GrabState(RagdollHand ragdollHand)
    {
      this.ragdollHand = ragdollHand;
      this.handle = ragdollHand.grabbedHandle;
      this.gripAxisPosition = ragdollHand.gripInfo != null ? ragdollHand.gripInfo.axisPosition : 0.0f;
      this.orientation = ragdollHand.gripInfo != null ? ragdollHand.gripInfo.orientation : (HandlePose) null;
    }
  }

  public delegate void TouchHoverEvent(
    Side side,
    Interactable interactable,
    Interactable.InteractionResult interactionResult);

  public delegate void HoverEndEvent(Side side, Interactable interactable);

  public delegate void GrabEvent(
    Side side,
    Handle handle,
    float axisPosition,
    HandlePose orientation,
    EventTime eventTime);

  public delegate void UnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventTime);
}
