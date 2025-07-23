// Decompiled with JetBrains decompiler
// Type: ThunderRoad.RagdollPart
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Manikin;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/RagdollPart.html")]
[RequireComponent(typeof (CollisionHandler))]
[AddComponentMenu("ThunderRoad/Creatures/Ragdoll part")]
public class RagdollPart : ThunderBehaviour
{
  [Header("Part")]
  public Transform meshBone;
  public Transform[] linkedMeshBones;
  public RagdollPart.Type type;
  public RagdollPart.Section section;
  [SerializeField]
  private RagdollPart.Axis frontAxis = RagdollPart.Axis.Forwards;
  [SerializeField]
  private RagdollPart.Axis upAxis = RagdollPart.Axis.Left;
  public UnityEngine.Vector3 boneToChildDirection = UnityEngine.Vector3.left;
  public RagdollPart parentPart;
  public bool ignoreStaticCollision;
  [Header("Dismemberment")]
  public bool sliceAllowed;
  [Range(0.0f, 1f)]
  public float sliceParentAdjust = 0.5f;
  public float sliceWidth = 0.04f;
  public float sliceHeight;
  public float sliceThreshold = 0.5f;
  public UnityEngine.Material sliceFillMaterial;
  [Tooltip("Disable this part collider and slice the referenced child part on slice (usefull for necks)")]
  public RagdollPart sliceChildAndDisableSelf;
  public bool ripBreak;
  public float ripBreakForce = 3000f;
  [Header("Forces")]
  [Min(1E-05f)]
  public float handledMass = -1f;
  [Min(1E-05f)]
  public float ragdolledMass = -1f;
  public float springPositionMultiplier = 1f;
  public float damperPositionMultiplier = 1f;
  public float springRotationMultiplier = 1f;
  public float damperRotationMultiplier = 1f;
  public List<RagdollPart> ignoredParts;
  [NonSerialized]
  public CreatureData.PartData data;
  [NonSerialized]
  public bool initialized;
  [NonSerialized]
  public bool bodyDamagerIsAttack;
  [NonSerialized]
  public PhysicBody physicBody;
  [NonSerialized]
  public Ragdoll ragdoll;
  [NonSerialized]
  public Ragdoll.Region ragdollRegion;
  [NonSerialized]
  public ColliderGroup colliderGroup;
  [NonSerialized]
  public CollisionHandler collisionHandler;
  public Wearable wearable;
  [NonSerialized]
  public bool hasMetalArmor;
  [NonSerialized]
  public List<RagdollPart> childParts = new List<RagdollPart>();
  [NonSerialized]
  public List<Creature.RendererData> renderers = new List<Creature.RendererData>();
  [NonSerialized]
  public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
  [NonSerialized]
  public List<int> skinnedMeshRendererIndexes = new List<int>();
  [NonSerialized]
  public List<SkinnedMeshRenderer> meshpartSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
  [NonSerialized]
  public List<Creature.RendererData> meshpartRendererList = new List<Creature.RendererData>();
  [NonSerialized]
  public float standingMass = -1f;
  [NonSerialized]
  public Ragdoll.Bone bone;
  [NonSerialized]
  public bool isSliced;
  [NonSerialized]
  public Transform slicedMeshRoot;
  [NonSerialized]
  public Transform root;
  [NonSerialized]
  public UnityEngine.Vector3 rootOrgLocalPosition;
  [NonSerialized]
  public Quaternion rootOrgLocalRotation;
  [NonSerialized]
  public UnityEngine.Vector3 savedPosition;
  [NonSerialized]
  public Quaternion savedRotation;
  [NonSerialized]
  public List<HandleRagdoll> handles;
  [NonSerialized]
  public bool isGrabbed;
  [NonSerialized]
  public Damager bodyDamager;
  [NonSerialized]
  public CharacterJoint characterJoint;
  public RagdollPart.CharacterJointData orgCharacterJointData;
  [NonSerialized]
  public bool characterJointLocked;
  public int damagerTier;
  protected float breakForceMultiplier;

  public bool hasParent { get; protected set; }

  protected virtual void OnValidate()
  {
    if (!this.gameObject.activeInHierarchy)
      return;
    if ((UnityEngine.Object) this.parentPart == (UnityEngine.Object) null)
    {
      CharacterJoint component = this.GetComponent<CharacterJoint>();
      if ((bool) (UnityEngine.Object) component)
        this.parentPart = component.connectedBody.GetComponent<RagdollPart>();
    }
    if ((double) this.ragdolledMass >= 0.0)
      return;
    PhysicBody physicBody = this.GetPhysicBody();
    if ((object) physicBody == null)
      return;
    this.ragdolledMass = physicBody.mass;
  }

  public void SetAllowSlice(bool allow) => this.sliceAllowed = allow;

  public void SetPositionToBone()
  {
    this.transform.position = this.meshBone.position;
    this.transform.rotation = this.meshBone.rotation;
    this.transform.localScale = this.meshBone.localScale;
  }

  public void SetPositionToBoneLeaveChildren()
  {
    List<Transform> transformList = new List<Transform>();
    foreach (Transform componentsInChild in this.GetComponentsInChildren<Transform>())
    {
      transformList.Add(componentsInChild);
      componentsInChild.SetParent(this.transform.parent, true);
    }
    this.transform.position = this.meshBone.position;
    this.transform.rotation = this.meshBone.rotation;
    this.transform.localScale = this.meshBone.localScale;
    foreach (Transform transform in transformList)
      transform.SetParent(this.transform, true);
  }

  public void IgnoreParentPart()
  {
    if ((UnityEngine.Object) this.parentPart == (UnityEngine.Object) null)
      return;
    if (this.ignoredParts == null)
      this.ignoredParts = new List<RagdollPart>();
    if (this.ignoredParts.Contains(this.parentPart))
      return;
    this.ignoredParts.Add(this.parentPart);
  }

  public void IgnoreAllParts()
  {
    if (this.ignoredParts == null)
      this.ignoredParts = new List<RagdollPart>();
    this.ignoredParts.Clear();
    foreach (RagdollPart componentsInChild in this.GetComponentInParent<Ragdoll>().GetComponentsInChildren<RagdollPart>())
    {
      if (!((UnityEngine.Object) componentsInChild == (UnityEngine.Object) this))
        this.ignoredParts.Add(componentsInChild);
    }
  }

  public void FindBoneFromName()
  {
    this.ragdoll = this.GetComponentInParent<Ragdoll>();
    foreach (Transform componentsInChild in this.ragdoll.meshRig.GetComponentsInChildren<Transform>())
    {
      if (componentsInChild.name == this.name)
      {
        this.meshBone = componentsInChild;
        break;
      }
    }
  }

  public void AssignJointBodyFromParent()
  {
    if (!((UnityEngine.Object) this.parentPart != (UnityEngine.Object) null))
      return;
    CharacterJoint component = this.GetComponent<CharacterJoint>();
    if (component == null)
      return;
    PhysicBody physicBody = this.parentPart.physicBody;
    if ((object) physicBody == null)
      physicBody = this.parentPart.GetPhysicBody();
    component.SetConnectedPhysicBody(physicBody);
  }

  public virtual void GetSlicePositionAndDirection(out UnityEngine.Vector3 position, out UnityEngine.Vector3 direction)
  {
    direction = this.GetSliceDirection();
    position = this.meshBone.transform.position + direction * this.sliceHeight;
  }

  public virtual UnityEngine.Vector3 GetSliceDirection()
  {
    return !(bool) (UnityEngine.Object) this.parentPart ? this.meshBone.transform.TransformDirection(this.boneToChildDirection) : UnityEngine.Vector3.Lerp(this.meshBone.transform.TransformDirection(this.boneToChildDirection), this.parentPart.meshBone.transform.TransformDirection(this.parentPart.boneToChildDirection), this.sliceParentAdjust);
  }

  protected virtual void OnDrawGizmosSelected()
  {
  }

  public UnityEngine.Vector3 forwardDirection => this.AxisToDirection(this.frontAxis);

  public UnityEngine.Vector3 upDirection => this.AxisToDirection(this.upAxis);

  public event RagdollPart.TouchActionDelegate OnTouchActionEvent;

  public event RagdollPart.HandlerEvent OnGrabbed;

  public event RagdollPart.HandlerEvent OnUngrabbed;

  public event RagdollPart.TKHandlerEvent OnTKGrab;

  public event RagdollPart.TKHandlerEvent OnTKRelease;

  public event RagdollPart.HeldActionDelegate OnHeldActionEvent;

  public virtual void OnRagdollEnable()
  {
  }

  public virtual void OnRagdollDisable()
  {
  }

  private UnityEngine.Vector3 AxisToDirection(RagdollPart.Axis axis)
  {
    switch (axis)
    {
      case RagdollPart.Axis.Right:
        return this.transform.right;
      case RagdollPart.Axis.Left:
        return -this.transform.right;
      case RagdollPart.Axis.Up:
        return this.transform.up;
      case RagdollPart.Axis.Down:
        return -this.transform.up;
      case RagdollPart.Axis.Forwards:
        return this.transform.forward;
      case RagdollPart.Axis.Backwards:
        return -this.transform.forward;
      default:
        return this.transform.forward;
    }
  }

  public bool SafeSlice()
  {
    RagdollPart.Type type = this.type;
    bool flag;
    switch (type)
    {
      case RagdollPart.Type.Head:
        flag = this.parentPart.TrySlice();
        break;
      case RagdollPart.Type.Neck:
        flag = this.TrySlice();
        break;
      case RagdollPart.Type.LeftArm:
      case RagdollPart.Type.RightArm:
      case RagdollPart.Type.LeftLeg:
      case RagdollPart.Type.RightLeg:
        flag = this.ragdoll.GetPart(type).TrySlice();
        break;
      default:
        flag = false;
        break;
    }
    return flag;
  }

  public bool TrySlice()
  {
    if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment) || (UnityEngine.Object) this == (UnityEngine.Object) this.ragdoll.rootPart)
      return false;
    if ((bool) (UnityEngine.Object) this.sliceChildAndDisableSelf)
    {
      if (!this.ragdoll.TrySlice(this.sliceChildAndDisableSelf))
        return false;
      this.isSliced = true;
      this.ragdoll.isSliced = true;
      foreach (Collider collider in this.colliderGroup.colliders)
        collider.enabled = false;
      foreach (Interactable handle in this.handles)
        handle.SetTouch(false);
      this.FixedCharJointLimit();
      this.characterJointLocked = true;
    }
    else if (!this.ragdoll.TrySlice(this))
      return false;
    return true;
  }

  protected bool HasMetalArmor()
  {
    ItemContent[] equipmentOnPart = this.ragdoll?.creature?.equipment?.GetEquipmentOnPart(this.type);
    if (equipmentOnPart != null)
    {
      foreach (ContainerContent<ItemData, ItemContent> containerContent in equipmentOnPart)
      {
        ItemData data = containerContent.data;
        int num;
        if (data == null)
        {
          num = 0;
        }
        else
        {
          bool? isMetal = data.GetModule<ItemModuleWardrobe>()?.isMetal;
          bool flag = true;
          num = isMetal.GetValueOrDefault() == flag & isMetal.HasValue ? 1 : 0;
        }
        if (num != 0)
          return true;
      }
    }
    return false;
  }

  protected virtual void Awake()
  {
    this.physicBody = this.gameObject.GetPhysicBody();
    this.physicBody.isKinematic = true;
    this.standingMass = this.physicBody.mass;
    if ((double) this.ragdolledMass < 0.0)
      this.ragdolledMass = this.physicBody.mass;
    this.colliderGroup = this.GetComponentInChildren<ColliderGroup>();
    this.collisionHandler = this.GetComponent<CollisionHandler>();
    this.handles = new List<HandleRagdoll>((IEnumerable<HandleRagdoll>) this.GetComponentsInChildren<HandleRagdoll>());
    this.bodyDamager = this.collisionHandler.gameObject.GetComponent<Damager>();
    if (!(bool) (UnityEngine.Object) this.bodyDamager)
      this.bodyDamager = this.collisionHandler.gameObject.AddComponent<Damager>();
    this.bodyDamager.colliderGroup = this.colliderGroup;
    this.bodyDamager.direction = Damager.Direction.All;
    this.collisionHandler.damagers.Add(this.bodyDamager);
    this.hasParent = (UnityEngine.Object) this.parentPart != (UnityEngine.Object) null;
    if (!this.hasParent)
      return;
    RagdollPartJointFixer component;
    if (!this.TryGetComponent<RagdollPartJointFixer>(out component))
      component = this.gameObject.AddComponent<RagdollPartJointFixer>();
    if (component.initialized)
      return;
    component.SetPart(this);
  }

  public void LinkParent()
  {
    if (!((UnityEngine.Object) this.parentPart != (UnityEngine.Object) null) || this.parentPart.childParts.Contains(this))
      return;
    this.parentPart.childParts.Add(this);
  }

  public virtual void Init(Ragdoll ragdoll)
  {
    if ((UnityEngine.Object) this.meshBone == (UnityEngine.Object) null)
    {
      Debug.LogError((object) ("Mesh bone is not set on part " + this.name));
    }
    else
    {
      this.ragdoll = ragdoll;
      this.root = this.transform.parent;
      this.rootOrgLocalPosition = this.transform.localPosition;
      this.rootOrgLocalRotation = this.transform.localRotation;
      this.characterJoint = this.GetComponent<CharacterJoint>();
      if ((bool) (UnityEngine.Object) this.characterJoint)
        this.orgCharacterJointData = new RagdollPart.CharacterJointData(this.characterJoint);
      foreach (RagdollPart ignoredPart in this.ignoredParts)
      {
        foreach (Collider componentsInChild1 in this.GetComponentsInChildren<Collider>(true))
        {
          foreach (Collider componentsInChild2 in ignoredPart.GetComponentsInChildren<Collider>(true))
            Physics.IgnoreCollision(componentsInChild1, componentsInChild2, true);
        }
      }
      this.collisionHandler.OnCollisionStartEvent += new CollisionHandler.CollisionEvent(this.CollisionStart);
      this.collisionHandler.OnCollisionStopEvent += new CollisionHandler.CollisionEvent(this.CollisionEnd);
      ragdoll.OnGrabEvent += new Ragdoll.GrabEvent(this.Grabbed);
      ragdoll.OnUngrabEvent += new Ragdoll.UngrabEvent(this.Ungrabbed);
      ragdoll.OnTelekinesisGrabEvent += new Ragdoll.TelekinesisGrabEvent(this.TKGrabbed);
      ragdoll.OnTelekinesisReleaseEvent += new Ragdoll.TelekinesisReleaseEvent(this.TKReleased);
      this.RefreshLayer();
      this.initialized = true;
    }
  }

  private void Grabbed(RagdollHand ragdollHand, HandleRagdoll handleRagdoll)
  {
    if (!((UnityEngine.Object) handleRagdoll.ragdollPart == (UnityEngine.Object) this))
      return;
    RagdollPart.HandlerEvent onGrabbed = this.OnGrabbed;
    if (onGrabbed == null)
      return;
    onGrabbed(ragdollHand, handleRagdoll);
  }

  private void Ungrabbed(RagdollHand ragdollHand, HandleRagdoll handleRagdoll, bool lastHandler)
  {
    if (!((UnityEngine.Object) handleRagdoll.ragdollPart == (UnityEngine.Object) this))
      return;
    RagdollPart.HandlerEvent onUngrabbed = this.OnUngrabbed;
    if (onUngrabbed == null)
      return;
    onUngrabbed(ragdollHand, handleRagdoll);
  }

  private void TKGrabbed(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll)
  {
    if (!((UnityEngine.Object) handleRagdoll.ragdollPart == (UnityEngine.Object) this))
      return;
    RagdollPart.TKHandlerEvent onTkGrab = this.OnTKGrab;
    if (onTkGrab == null)
      return;
    onTkGrab(spellTelekinesis, handleRagdoll);
  }

  private void TKReleased(
    SpellTelekinesis spellTelekinesis,
    HandleRagdoll handleRagdoll,
    bool lastHandler)
  {
    if (!((UnityEngine.Object) handleRagdoll.ragdollPart == (UnityEngine.Object) this))
      return;
    RagdollPart.TKHandlerEvent onTkRelease = this.OnTKRelease;
    if (onTkRelease == null)
      return;
    onTkRelease(spellTelekinesis, handleRagdoll);
  }

  private void CollisionStart(CollisionInstance collisionInstance)
  {
    this.ragdoll.CollisionStartStop(collisionInstance, this, true);
  }

  private void CollisionEnd(CollisionInstance collisionInstance)
  {
    this.ragdoll.CollisionStartStop(collisionInstance, this, false);
  }

  public virtual void Load() => this.SetBodyDamagerToDefault();

  public bool UpdateMetalArmor()
  {
    this.hasMetalArmor = this.HasMetalArmor();
    return this.hasMetalArmor;
  }

  public void UpdateRenderers()
  {
    this.renderers.Clear();
    this.skinnedMeshRenderers.Clear();
    this.skinnedMeshRendererIndexes.Clear();
    this.meshpartSkinnedMeshRenderers.Clear();
    this.meshpartRendererList.Clear();
    ManikinPart[] source = (ManikinPart[]) null;
    if ((bool) (UnityEngine.Object) this.ragdoll.creature.manikinLocations)
      source = this.ragdoll.creature.manikinLocations.GetPartsOnBoneInLayerOrder(this.bone.boneHashes);
    for (int index = 0; index < this.ragdoll.creature.renderers.Count; ++index)
    {
      Creature.RendererData renderer = this.ragdoll.creature.renderers[index];
      bool flag1 = (UnityEngine.Object) renderer.meshPart != (UnityEngine.Object) null;
      bool flag2 = source == null || ((IEnumerable<ManikinPart>) source).Contains<ManikinPart>(renderer.manikinPart);
      if (!flag2 && renderer.manikinPart is ManikinGroupPart manikinPart)
        flag2 = manikinPart.PartOfBones(this.bone.boneHashes);
      if (flag2 && (UnityEngine.Object) renderer.revealDecal != (UnityEngine.Object) null)
      {
        if ((bool) (UnityEngine.Object) renderer.renderer)
        {
          this.skinnedMeshRenderers.Add(flag1 ? renderer.renderer : (SkinnedMeshRenderer) null);
          this.skinnedMeshRendererIndexes.Add(this.renderers.Count);
          if (flag1 && (UnityEngine.Object) renderer.meshPart.skinnedMeshRenderer == (UnityEngine.Object) renderer.renderer)
          {
            this.meshpartSkinnedMeshRenderers.Add(renderer.renderer);
            this.meshpartRendererList.Add(renderer);
          }
        }
        if ((bool) (UnityEngine.Object) renderer.splitRenderer)
        {
          this.skinnedMeshRenderers.Add(flag1 ? renderer.splitRenderer : (SkinnedMeshRenderer) null);
          this.skinnedMeshRendererIndexes.Add(this.renderers.Count);
        }
        this.renderers.Add(renderer);
      }
    }
  }

  public void AnimatorMoveUpdate()
  {
    if (!this.isSliced)
      return;
    Animator animator = this.ragdoll.creature.animator;
    this.transform.position -= animator.deltaPosition;
    Quaternion quaternion = Quaternion.Inverse(animator.deltaRotation);
    this.transform.position = animator.transform.TransformPoint(quaternion * animator.transform.InverseTransformPoint(this.transform.position));
    this.transform.rotation = Quaternion.LookRotation(animator.transform.TransformPoint(quaternion * animator.transform.InverseTransformPoint(this.transform.position + this.transform.forward)) - this.transform.position, animator.transform.TransformPoint(quaternion * animator.transform.InverseTransformPoint(this.transform.position + this.transform.up)) - this.transform.position);
  }

  public virtual void OnHeldAction(
    RagdollHand ragdollHand,
    HandleRagdoll handle,
    Interactable.Action action)
  {
    this.ragdoll.OnHeldAction(ragdollHand, this, handle, action);
    RagdollPart.HeldActionDelegate onHeldActionEvent = this.OnHeldActionEvent;
    if (onHeldActionEvent == null)
      return;
    onHeldActionEvent(ragdollHand, handle, action);
  }

  public virtual void OnTouchAction(
    RagdollHand ragdollHand,
    Interactable interactable,
    Interactable.Action action)
  {
    this.ragdoll.OnTouchAction(ragdollHand, this, interactable, action);
    RagdollPart.TouchActionDelegate touchActionEvent = this.OnTouchActionEvent;
    if (touchActionEvent == null)
      return;
    touchActionEvent(ragdollHand, interactable, action);
  }

  public virtual void SetBodyDamagerToDefault() => this.SetBodyDamager(false);

  public virtual void SetBodyDamagerToAttack() => this.SetBodyDamager(true);

  private void SetBodyDamager(bool isAttack)
  {
    DamagerData damagerData = isAttack ? this.data?.bodyAttackDamagerData : this.data?.bodyDamagerData;
    if (damagerData == null)
    {
      damagerData = this.ragdoll.creature.data.ragdollData.bodyDefaultDamagerData;
      isAttack = false;
    }
    this.bodyDamager.Load(damagerData, this.collisionHandler);
    this.collisionHandler.SortDamagers();
    this.bodyDamagerIsAttack = isAttack;
    this.RefreshLayer();
  }

  public virtual void RefreshLayer()
  {
    if (this.bodyDamagerIsAttack)
      this.SetLayer(LayerName.PlayerHandAndFoot);
    else if (!this.isSliced && !this.isGrabbed && (this.ragdoll.state == Ragdoll.State.Standing || this.ragdoll.state == Ragdoll.State.NoPhysic || this.ragdoll.state == Ragdoll.State.Kinematic || this.ragdoll.state == Ragdoll.State.Disabled))
    {
      if ((bool) (UnityEngine.Object) this.ragdoll.creature.player)
        this.SetLayer(LayerName.Avatar);
      else
        this.SetLayer(this.ignoreStaticCollision ? LayerName.ItemAndRagdollOnly : LayerName.NPC);
    }
    else if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration) && this.ragdoll.creature.isKilled)
      this.SetLayer(LayerName.LocomotionOnly);
    else
      this.SetLayer(LayerName.Ragdoll);
  }

  public void ResetCharJointLimit()
  {
    if (this.characterJointLocked || !(bool) (UnityEngine.Object) this.characterJoint)
      return;
    this.characterJoint.lowTwistLimit = this.characterJoint.lowTwistLimit with
    {
      limit = this.orgCharacterJointData.lowTwistLimit.limit
    };
    this.characterJoint.highTwistLimit = this.characterJoint.highTwistLimit with
    {
      limit = this.orgCharacterJointData.highTwistLimit.limit
    };
    this.characterJoint.swing1Limit = this.characterJoint.swing1Limit with
    {
      limit = this.orgCharacterJointData.swing1Limit.limit
    };
    this.characterJoint.swing2Limit = this.characterJoint.swing2Limit with
    {
      limit = this.orgCharacterJointData.swing2Limit.limit
    };
  }

  public void DisableCharJointLimit()
  {
    if (this.characterJointLocked || !(bool) (UnityEngine.Object) this.characterJoint)
      return;
    this.characterJoint.lowTwistLimit = this.characterJoint.lowTwistLimit with
    {
      limit = -180f
    };
    this.characterJoint.highTwistLimit = this.characterJoint.highTwistLimit with
    {
      limit = 180f
    };
    this.characterJoint.swing1Limit = this.characterJoint.swing1Limit with
    {
      limit = 180f
    };
    this.characterJoint.swing2Limit = this.characterJoint.swing2Limit with
    {
      limit = 180f
    };
  }

  public void FixedCharJointLimit()
  {
    if (this.characterJointLocked || !(bool) (UnityEngine.Object) this.characterJoint)
      return;
    this.characterJoint.lowTwistLimit = this.characterJoint.lowTwistLimit with
    {
      limit = 0.0f
    };
    this.characterJoint.highTwistLimit = this.characterJoint.highTwistLimit with
    {
      limit = 0.0f
    };
    this.characterJoint.swing1Limit = this.characterJoint.swing1Limit with
    {
      limit = 0.0f
    };
    this.characterJoint.swing2Limit = this.characterJoint.swing2Limit with
    {
      limit = 0.0f
    };
  }

  public void EnableCharJointBreakForce(float multiplier = 1f)
  {
    if (this.characterJointLocked || !Damager.dismembermentEnabled || !this.ripBreak || this.isSliced || !(bool) (UnityEngine.Object) this.characterJoint)
      return;
    this.breakForceMultiplier = multiplier;
    this.characterJoint.breakForce = (float) ((double) this.ripBreakForce * (double) multiplier * 1.0) / Time.timeScale;
    SpellPowerSlowTime.OnTimeScaleChangeEvent -= new SpellPowerSlowTime.TimeScaleChangeEvent(this.UpdateJointBreakForceToTimeScale);
    SpellPowerSlowTime.OnTimeScaleChangeEvent += new SpellPowerSlowTime.TimeScaleChangeEvent(this.UpdateJointBreakForceToTimeScale);
  }

  public void ResetCharJointBreakForce()
  {
    if (this.characterJointLocked || !Damager.dismembermentEnabled || !this.ripBreak || !(bool) (UnityEngine.Object) this.characterJoint)
      return;
    SpellPowerSlowTime.OnTimeScaleChangeEvent -= new SpellPowerSlowTime.TimeScaleChangeEvent(this.UpdateJointBreakForceToTimeScale);
    this.breakForceMultiplier = 1f;
    this.characterJoint.breakForce = this.breakForceMultiplier = this.orgCharacterJointData.breakForce;
  }

  public void UpdateJointBreakForceToTimeScale(SpellPowerSlowTime slowTime, float scale)
  {
    if (this.characterJointLocked || !Damager.dismembermentEnabled || !this.ripBreak || !(bool) (UnityEngine.Object) this.characterJoint)
      return;
    this.characterJoint.breakForce = (float) ((double) this.ripBreakForce * (double) this.breakForceMultiplier * 1.0) / scale;
  }

  public void CreateCharJoint(bool resetPosition)
  {
    if (this.characterJointLocked)
      return;
    this.DestroyCharJoint();
    this.characterJoint = this.orgCharacterJointData.CreateJoint(this.gameObject, resetPosition);
  }

  public void DestroyCharJoint()
  {
    if (this.characterJointLocked || !(bool) (UnityEngine.Object) this.characterJoint)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.characterJoint);
  }

  protected void OnJointBreak()
  {
    if (!this.TrySlice() || !this.data.sliceForceKill)
      return;
    this.ragdoll.creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy, float.PositiveInfinity))
    {
      damageStruct = {
        hitRagdollPart = this
      }
    });
  }

  public float GetPinSpringPosition()
  {
    return this.ragdoll.springPositionForce * this.springPositionMultiplier;
  }

  public float GetPinDamperPosition()
  {
    return this.ragdoll.springPositionForce * this.damperPositionMultiplier;
  }

  public float GetPinSpringRotation()
  {
    return this.ragdoll.springRotationForce * this.springRotationMultiplier;
  }

  public float GetPinDamperRotation()
  {
    return this.ragdoll.springRotationForce * this.damperRotationMultiplier;
  }

  public bool HasCollider(Collider collider)
  {
    for (int index = 0; index < this.colliderGroup.colliders.Count; ++index)
    {
      if ((UnityEngine.Object) collider == (UnityEngine.Object) this.colliderGroup.colliders[index])
        return true;
    }
    return false;
  }

  public void SetLayer(int layer)
  {
    this.gameObject.layer = layer;
    for (int index = 0; index < this.colliderGroup.colliders.Count; ++index)
    {
      if (!((UnityEngine.Object) this.colliderGroup.colliders[index] == (UnityEngine.Object) null) && !((UnityEngine.Object) this.colliderGroup.colliders[index].gameObject == (UnityEngine.Object) null))
        this.colliderGroup.colliders[index].gameObject.layer = layer;
    }
  }

  public void SetLayer(LayerName layerName) => this.SetLayer(GameManager.GetLayer(layerName));

  public Bounds GetWorldBounds()
  {
    Bounds worldBounds = new Bounds(this.transform.position, UnityEngine.Vector3.zero);
    for (int index = 0; index < this.renderers.Count; ++index)
      worldBounds.Encapsulate(this.renderers[index].renderer.bounds);
    return worldBounds;
  }

  public bool IsPenetrated()
  {
    return this.collisionHandler != null && this.collisionHandler.penetratedObjects.Count > 0;
  }

  [Flags]
  public enum Type
  {
    Head = 1,
    Neck = 2,
    Torso = 4,
    LeftArm = 8,
    RightArm = 16, // 0x00000010
    LeftHand = 32, // 0x00000020
    RightHand = 64, // 0x00000040
    LeftLeg = 128, // 0x00000080
    RightLeg = 256, // 0x00000100
    LeftFoot = 512, // 0x00000200
    RightFoot = 1024, // 0x00000400
    LeftWing = 2048, // 0x00000800
    RightWing = 4096, // 0x00001000
    Tail = 8192, // 0x00002000
  }

  public enum Section
  {
    Full,
    Lower,
    Mid,
    Upper,
  }

  public enum Axis
  {
    Right,
    Left,
    Up,
    Down,
    Forwards,
    Backwards,
  }

  public delegate void TouchActionDelegate(
    RagdollHand ragdollHand,
    Interactable interactable,
    Interactable.Action action);

  public delegate void HandlerEvent(RagdollHand ragdollHand, HandleRagdoll handle);

  public delegate void TKHandlerEvent(SpellTelekinesis spellTelekinesis, HandleRagdoll handle);

  public delegate void HeldActionDelegate(
    RagdollHand ragdollHand,
    HandleRagdoll handle,
    Interactable.Action action);

  public class CharacterJointData
  {
    public UnityEngine.Vector3 localPosition;
    public Quaternion localRotation;
    public Rigidbody connectedBody;
    public UnityEngine.Vector3 anchor;
    public UnityEngine.Vector3 axis;
    public bool autoConfigureConnectedAnchor;
    public UnityEngine.Vector3 connectedAnchor;
    public UnityEngine.Vector3 swingAxis;
    public SoftJointLimitSpring twistLimitSpring;
    public SoftJointLimit lowTwistLimit;
    public SoftJointLimit highTwistLimit;
    public SoftJointLimitSpring swingLimitSpring;
    public SoftJointLimit swing1Limit;
    public SoftJointLimit swing2Limit;
    public bool enableProjection;
    public float projectionDistance;
    public float projectionAngle;
    public float breakForce;
    public float breakTorque;
    public bool enableCollision;
    public bool enablePreprocessing;
    public float massScale;
    public float connectedMassScale;

    public CharacterJointData(CharacterJoint characterJoint)
    {
      this.localPosition = characterJoint.connectedBody.transform.InverseTransformPoint(characterJoint.transform.position);
      this.localRotation = Quaternion.Inverse(characterJoint.connectedBody.transform.rotation) * characterJoint.transform.rotation;
      this.connectedBody = characterJoint.connectedBody;
      this.anchor = characterJoint.anchor;
      this.axis = characterJoint.axis;
      this.autoConfigureConnectedAnchor = characterJoint.autoConfigureConnectedAnchor;
      this.connectedAnchor = characterJoint.connectedAnchor;
      this.swingAxis = characterJoint.swingAxis;
      this.twistLimitSpring = characterJoint.twistLimitSpring;
      this.lowTwistLimit = characterJoint.lowTwistLimit;
      this.highTwistLimit = characterJoint.highTwistLimit;
      this.swingLimitSpring = characterJoint.swingLimitSpring;
      this.swing1Limit = characterJoint.swing1Limit;
      this.swing2Limit = characterJoint.swing2Limit;
      this.enableProjection = characterJoint.enableProjection;
      this.projectionDistance = characterJoint.projectionDistance;
      this.projectionAngle = characterJoint.projectionAngle;
      this.breakForce = characterJoint.breakForce;
      this.breakTorque = characterJoint.breakTorque;
      this.enableCollision = characterJoint.enableCollision;
      this.enablePreprocessing = characterJoint.enablePreprocessing;
      this.massScale = characterJoint.massScale;
      this.connectedMassScale = characterJoint.connectedMassScale;
    }

    public CharacterJoint CreateJoint(GameObject gameobject, bool resetPosition = true)
    {
      if (resetPosition)
      {
        gameobject.transform.position = this.connectedBody.transform.TransformPoint(this.localPosition);
        gameobject.transform.rotation = this.connectedBody.transform.rotation * this.localRotation;
      }
      CharacterJoint joint = gameobject.AddComponent<CharacterJoint>();
      joint.anchor = this.anchor;
      joint.axis = this.axis;
      joint.autoConfigureConnectedAnchor = this.autoConfigureConnectedAnchor;
      joint.connectedAnchor = this.connectedAnchor;
      joint.swingAxis = this.swingAxis;
      joint.twistLimitSpring = this.twistLimitSpring;
      joint.lowTwistLimit = this.lowTwistLimit;
      joint.highTwistLimit = this.highTwistLimit;
      joint.swingLimitSpring = this.swingLimitSpring;
      joint.swing1Limit = this.swing1Limit;
      joint.swing2Limit = this.swing2Limit;
      joint.enableProjection = this.enableProjection;
      joint.projectionDistance = this.projectionDistance;
      joint.projectionAngle = this.projectionAngle;
      joint.breakForce = this.breakForce;
      joint.breakTorque = this.breakTorque;
      joint.enableCollision = this.enableCollision;
      joint.enablePreprocessing = this.enablePreprocessing;
      joint.massScale = this.massScale;
      joint.connectedMassScale = this.connectedMassScale;
      joint.connectedBody = this.connectedBody;
      return joint;
    }
  }
}
