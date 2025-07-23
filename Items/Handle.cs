// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Handle
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Handle")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Handle.html")]
public class Handle : Interactable
{
  [Range(-1f, 1f)]
  [Tooltip("Define where the handle is automatically grabbed along the axis length")]
  public float defaultGrabAxisRatio;
  public UnityEngine.Vector3 ikAnchorOffset;
  [NonSerialized]
  public List<HandlePose> orientations = new List<HandlePose>();
  [Tooltip("The default handpose to be grabbed (Right Hand)")]
  public HandlePose orientationDefaultRight;
  [Tooltip("The default handpose to be grabbed (Left Hand)")]
  public HandlePose orientationDefaultLeft;
  [Tooltip("When linked handle is grabbed, ungrip this handle.")]
  public Handle releaseHandle;
  [Tooltip("Handle will only activate when the linked handle is grabbed.")]
  public Handle activateHandle;
  [Tooltip("NPCs will try to grab this handle if their brain logic tells them to.")]
  public Handle AIGrabHandle;
  [Tooltip("When ticked, no sound will play when the handle is grabbed")]
  public bool silentGrab;
  [Tooltip("When ticked, the player will ungrab the handle when grounded (touching the ground)")]
  public bool forceAutoDropWhenGrounded;
  [Tooltip("The default handpose to be grabbed (Right Hand)")]
  public bool ignoreClimbingForceOverride;
  [Tooltip("If the player can cast spells while holding this handle, this transform defines where the orb will appear.")]
  public Transform spellOrbTarget;
  [Tooltip("Lets AI know how far the item is away from the player. You can use the button to calculate this automatically, so long as a ColliderGroup is set up with sufficient colliders.")]
  public float reach = 0.5f;
  [Tooltip("(Optional)Disables listed colliders once the handle is grabbed.")]
  public List<Collider> handOverlapColliders;
  [Tooltip("(Optional) Allows you to add a custom rigidbody to the handle. (Do not reference item!)")]
  public Rigidbody customRigidBody;
  [Tooltip("(Optional) When player hand reaches the top of the handle via slide, it will switch to listed handle once the top is reached.")]
  public Handle slideToUpHandle;
  [Tooltip("(Optional) When player hand reaches the bottom of the handle via slide, it will switch to listed handle once the bottom is reached.")]
  public Handle slideToBottomHandle;
  [Tooltip("(Optional) Offset of the bottom and top slide up handles. Can switch handle when reaching 0.2 meters away from the top/bottom, for example.")]
  public float slideToHandleOffset = 0.01f;
  [Tooltip("Allows you to enable/disable handle sliding (Only works on handles with a length greater than 0!)")]
  public Handle.SlideBehavior slideBehavior;
  [Tooltip("(Optional) When you slide up the handle, and the axis length is 0, sliding will instead snap to the referenced handle.")]
  public Handle moveToHandle;
  [Tooltip("Axis Position for the \"Move To Handle\" handle")]
  public float moveToHandleAxisPos;
  [Tooltip("When this box is checked, hand poses will update whenever the target weight changes or whenever the pose data changes.")]
  public bool updatePosesAutomatically;
  [Header("Here, you can add a list of orientations for the handle. Once done, you can click the update button for the new orientations, and it will do it automatically.\n\nThis field is old/obsolete, and is only optional.")]
  public List<Handle.Orientation> allowedOrientations = new List<Handle.Orientation>();
  [Header("Controller axis override")]
  public bool redirectControllerAxis;
  public UnityEvent<float> controllerAxisOutputX = new UnityEvent<float>();
  public UnityEvent<float> controllerAxisOutputY = new UnityEvent<float>();
  [NonSerialized]
  public HandleData data;
  public static bool holdGrip = false;
  public static Side dominantHand = Side.Right;
  public static Handle.TwoHandedMode twoHandedMode = Handle.TwoHandedMode.AutoFront;
  public static float globalPositionSpringMultiplier = 1f;
  public static float globalPositionDamperMultiplier = 1f;
  public static float globalRotationSpringMultiplier = 1f;
  public static float globalRotationDamperMultiplier = 1f;
  public static bool globalForceXYZ = false;
  protected UnityEngine.Vector2 lastPositionMultiplier = new UnityEngine.Vector2(1f, 1f);
  protected UnityEngine.Vector2 lastRotationMultiplier = new UnityEngine.Vector2(1f, 1f);
  [NonSerialized]
  public Item item;
  [NonSerialized]
  public PhysicBody physicBody;
  [NonSerialized]
  public bool playerJointActive;
  protected bool forcePlayerJoint;
  protected List<object> forcePlayerJointHandlers = new List<object>();
  protected float positionSpringMultiplier = 1f;
  protected float positionDamperMultiplier = 1f;
  protected float rotationSpringMultiplier = 1f;
  protected float rotationDamperMultiplier = 1f;
  [NonSerialized]
  public List<Handle.JointModifier> jointModifiers = new List<Handle.JointModifier>();
  public List<RagdollHand> handlers;
  protected int handlersCount;
  [NonSerialized]
  public List<SpellCaster> telekinesisHandlers = new List<SpellCaster>();
  [NonSerialized]
  public System.Action<HandleData> onDataLoaded;
  [NonSerialized]
  public bool justGrabbed;
  protected float slideForce;

  public event Handle.GrabEvent Grabbed;

  public event Handle.GrabEvent UnGrabbed;

  public event Handle.TkEvent TkGrabbed;

  public event Handle.TkEvent TkUnGrabbed;

  public event Handle.SlideEvent SlidingStateChange;

  public event Handle.SwitchHandleEvent SlideToOtherHandle;

  public event Interactable.ActionDelegate OnHeldActionEvent;

  public bool IsTkGrabbed
  {
    get
    {
      List<SpellCaster> telekinesisHandlers = this.telekinesisHandlers;
      return telekinesisHandlers != null && telekinesisHandlers.Count > 0;
    }
  }

  public SpellCaster MainTkHandler
  {
    get => !this.IsTkGrabbed ? (SpellCaster) null : this.telekinesisHandlers[0];
  }

  public virtual void CheckOrientations()
  {
    this.orientations = new List<HandlePose>((IEnumerable<HandlePose>) this.GetComponentsInChildren<HandlePose>());
    if (this.orientations.Count != 0)
      return;
    if (this.allowedOrientations.Count > 0)
    {
      foreach (Handle.Orientation allowedOrientation in this.allowedOrientations)
      {
        if (allowedOrientation.allowedHand == Handle.HandSide.Both || allowedOrientation.allowedHand == Handle.HandSide.Right)
        {
          HandlePose handlePose = this.AddOrientation(Side.Right, allowedOrientation.positionOffset, Quaternion.Euler(allowedOrientation.rotation));
          if ((UnityEngine.Object) this.orientationDefaultRight == (UnityEngine.Object) null && (allowedOrientation.isDefault == Handle.HandSide.Both || allowedOrientation.isDefault == Handle.HandSide.Right))
            this.orientationDefaultRight = handlePose;
        }
        if (allowedOrientation.allowedHand == Handle.HandSide.Both || allowedOrientation.allowedHand == Handle.HandSide.Left)
        {
          HandlePose handlePose = this.AddOrientation(Side.Left, allowedOrientation.positionOffset, Quaternion.Euler(allowedOrientation.rotation));
          if ((UnityEngine.Object) this.orientationDefaultLeft == (UnityEngine.Object) null && (allowedOrientation.isDefault == Handle.HandSide.Both || allowedOrientation.isDefault == Handle.HandSide.Left))
            this.orientationDefaultLeft = handlePose;
        }
      }
    }
    else
    {
      this.orientationDefaultRight = this.AddOrientation(Side.Right, UnityEngine.Vector3.zero, Quaternion.identity);
      this.orientationDefaultLeft = this.AddOrientation(Side.Left, UnityEngine.Vector3.zero, Quaternion.identity);
    }
  }

  public virtual HandlePose AddOrientation(Side side, UnityEngine.Vector3 position, Quaternion rotation)
  {
    GameObject gameObject = new GameObject("Orient");
    gameObject.transform.SetParent(this.transform);
    gameObject.transform.localPosition = position;
    gameObject.transform.localRotation = rotation;
    gameObject.transform.localScale = UnityEngine.Vector3.one;
    HandlePose handlePose = gameObject.AddComponent<HandlePose>();
    handlePose.side = side;
    this.orientations.Add(handlePose);
    return handlePose;
  }

  public virtual float GetDefaultAxisLocalPosition()
  {
    return (double) this.axisLength == 0.0 ? 0.0f : this.defaultGrabAxisRatio * (this.axisLength / 2f);
  }

  public virtual UnityEngine.Vector3 GetDefaultAxisPosition(Side side)
  {
    return this.transform.TransformPoint(0.0f, this.GetDefaultAxisLocalPosition(), 0.0f);
  }

  public virtual HandlePose GetDefaultOrientation(Side side)
  {
    if (side == Side.Right && (bool) (UnityEngine.Object) this.orientationDefaultRight)
      return this.orientationDefaultRight;
    if (side == Side.Left && (bool) (UnityEngine.Object) this.orientationDefaultLeft)
      return this.orientationDefaultLeft;
    Debug.LogError((object) $"No default orientation found! Please check the prefab {this.transform.parent.name}/{this.name}");
    return (HandlePose) null;
  }

  public virtual HandlePose GetNearestOrientation(Transform grip, Side side)
  {
    float num1 = float.NegativeInfinity;
    HandlePose nearestOrientation = (HandlePose) null;
    foreach (HandlePose orientation in this.orientations)
    {
      if (orientation.side == side)
      {
        float num2 = UnityEngine.Vector3.Dot(grip.forward, orientation.transform.rotation * UnityEngine.Vector3.forward) + UnityEngine.Vector3.Dot(grip.up, orientation.transform.rotation * UnityEngine.Vector3.up);
        if ((double) num2 > (double) num1)
        {
          num1 = num2;
          nearestOrientation = orientation;
        }
      }
    }
    return nearestOrientation;
  }

  public virtual bool IsAllowed(Side side)
  {
    foreach (HandlePose orientation in this.orientations)
    {
      if (side == orientation.side)
        return true;
    }
    return false;
  }

  public virtual void CalculateReach()
  {
    float num = 0.0f;
    foreach (Component componentsInChild1 in this.GetComponentInParent<Item>().GetComponentsInChildren<ColliderGroup>())
    {
      foreach (Collider componentsInChild2 in componentsInChild1.GetComponentsInChildren<Collider>())
      {
        float y = this.transform.InverseTransformPoint(componentsInChild2.ClosestPointOnBounds(this.transform.position + this.transform.up.normalized * 10f)).y;
        if ((double) y > (double) num)
          num = y;
      }
    }
    this.reach = num - this.GetDefaultAxisLocalPosition();
  }

  public virtual void SetUpdatePoses(bool active) => this.updatePosesAutomatically = active;

  public virtual void Release()
  {
    for (int index = this.handlersCount - 1; index >= 0; --index)
      this.handlers[index].UnGrab(false);
  }

  protected virtual void ForcePlayerGrab()
  {
    if (!(bool) (UnityEngine.Object) Player.local)
      return;
    PlayerHand playerHand = (PlayerHand) null;
    if (!(bool) (UnityEngine.Object) Player.local.handRight.ragdollHand.grabbedHandle)
      playerHand = Player.local.handRight;
    if (!(bool) (UnityEngine.Object) Player.local.handLeft.ragdollHand.grabbedHandle)
      playerHand = Player.local.handLeft;
    if (!(bool) (UnityEngine.Object) playerHand || !(bool) (UnityEngine.Object) playerHand.ragdollHand)
      return;
    playerHand.ragdollHand.Grab(this);
  }

  protected override void Awake()
  {
    this.CheckOrientations();
    base.Awake();
    this.physicBody = this.gameObject.GetPhysicBodyInParent();
    if (this.physicBody == (PhysicBody) null)
      Debug.LogError((object) $"Handle could not find a physic body in parent! {this.transform.parent.name}/{this.name}", (UnityEngine.Object) this.gameObject);
    this.gameObject.layer = GameManager.GetLayer(LayerName.TouchObject);
    this.handlers = new List<RagdollHand>();
    this.telekinesisHandlers = new List<SpellCaster>();
    this.item = this.GetComponentInParent<Item>();
    if (!(bool) (UnityEngine.Object) this.activateHandle)
      return;
    this.SetActivateHandle(this.activateHandle);
  }

  public void SetActivateHandle(Handle newActivateHandle)
  {
    if ((bool) (UnityEngine.Object) this.activateHandle)
    {
      this.activateHandle.Grabbed -= new Handle.GrabEvent(this.OnActivateHandleGrabbed);
      this.activateHandle.UnGrabbed -= new Handle.GrabEvent(this.OnActivateHandleUnGrabbed);
      this.SetTouchPersistent(true);
    }
    if (!(bool) (UnityEngine.Object) newActivateHandle)
      return;
    this.SetTouchPersistent(false);
    newActivateHandle.Grabbed += new Handle.GrabEvent(this.OnActivateHandleGrabbed);
    newActivateHandle.UnGrabbed += new Handle.GrabEvent(this.OnActivateHandleUnGrabbed);
    this.activateHandle = newActivateHandle;
  }

  protected void OnActivateHandleGrabbed(
    RagdollHand ragdollHand,
    Handle handle,
    EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    this.SetTouchPersistent(true);
  }

  protected void OnActivateHandleUnGrabbed(
    RagdollHand ragdollHand,
    Handle handle,
    EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    this.SetTouchPersistent(false);
  }

  public override void Load(InteractableData interactableData)
  {
    if (!(interactableData is HandleData))
    {
      Debug.LogError((object) "Trying to load wrong data type");
    }
    else
    {
      base.Load((InteractableData) (interactableData as HandleData));
      this.data = base.data as HandleData;
      if (this.data.disableOnStart)
      {
        this.SetTouchPersistent(false);
        this.SetTelekinesis(false);
      }
      else
        this.SetTelekinesis(this.data.allowTelekinesis);
      System.Action<HandleData> onDataLoaded = this.onDataLoaded;
      if (onDataLoaded == null)
        return;
      onDataLoaded(base.data as HandleData);
    }
  }

  public virtual void SetTelekinesis(bool active)
  {
    if (active)
    {
      if (this.IsAllowed(Side.Left) && this.IsAllowed(Side.Right))
      {
        if (this is HandleRagdoll)
          this.gameObject.tag = HandleData.tagTkRagdoll;
        else
          this.gameObject.tag = HandleData.tagTkDefault;
      }
      else if (!this.IsAllowed(Side.Left) && !this.IsAllowed(Side.Right))
        this.gameObject.tag = "Untagged";
      else if (!this.IsAllowed(Side.Left))
      {
        if (this is HandleRagdoll)
          this.gameObject.tag = HandleData.tagTkRagdollLeft;
        else
          this.gameObject.tag = HandleData.tagTkDefaultLeft;
      }
      else
      {
        if (this.IsAllowed(Side.Right))
          return;
        if (this is HandleRagdoll)
          this.gameObject.tag = HandleData.tagTkRagdollRight;
        else
          this.gameObject.tag = HandleData.tagTkDefaultRight;
      }
    }
    else
    {
      if (this.IsTkGrabbed)
      {
        for (int index = this.telekinesisHandlers.Count - 1; index >= 0; --index)
          this.telekinesisHandlers[index]?.telekinesis?.TryRelease();
      }
      if (this.telekinesisHandlers.Count != 0)
        return;
      this.gameObject.tag = "Untagged";
    }
  }

  protected virtual bool HoldGripToGrab() => this.data.forceHoldGripToGrab || Handle.holdGrip;

  public override void OnTouchStart(RagdollHand ragdollHand)
  {
    if ((bool) (UnityEngine.Object) this.item && (bool) (UnityEngine.Object) this.item.holder && !this.item.holder.GrabFromHandle())
      this.item.holder.OnTouchStart(ragdollHand);
    else
      base.OnTouchStart(ragdollHand);
  }

  public override void OnTouchStay(RagdollHand ragdollHand)
  {
    base.OnTouchStay(ragdollHand);
    if (!Highlighter.isEnabled || (double) this.axisLength <= 0.0)
      return;
    Highlighter.GetSide(ragdollHand.side).axisPosition = this.GetNearestAxisPosition(ragdollHand.grip.position);
  }

  public override void OnTouchEnd(RagdollHand ragdollHand)
  {
    if ((bool) (UnityEngine.Object) this.item && (bool) (UnityEngine.Object) this.item.holder && !this.item.holder.GrabFromHandle())
      this.item.holder.OnTouchEnd(ragdollHand);
    else
      base.OnTouchEnd(ragdollHand);
  }

  public override Interactable.InteractionResult CheckInteraction(RagdollHand ragdollHand)
  {
    if ((bool) (UnityEngine.Object) this.item && !this.item.gameObject.activeInHierarchy && (UnityEngine.Object) this.item.holder == (UnityEngine.Object) null)
      return new Interactable.InteractionResult(ragdollHand, false);
    if ((bool) (UnityEngine.Object) ragdollHand.grabbedHandle)
      return new Interactable.InteractionResult(ragdollHand, false);
    if (!this.IsAllowed(ragdollHand.side))
      return new Interactable.InteractionResult(ragdollHand, false, this.data.warnIfNotAllowed, LocalizationManager.Instance.GetLocalizedString("Default", "NotAllowed"), LocalizationManager.Instance.GetLocalizedString("Default", "NotAllowedHand"), new UnityEngine.Color?(UnityEngine.Color.red));
    if (this.data.disabledOnSnap && (bool) (UnityEngine.Object) this.item?.holder)
      return new Interactable.InteractionResult(ragdollHand, false);
    if (this is HandleRagdoll handleRagdoll && (UnityEngine.Object) handleRagdoll.ragdollPart == (UnityEngine.Object) ragdollHand)
      return new Interactable.InteractionResult(ragdollHand, false);
    if ((bool) (UnityEngine.Object) this.item && this.item.IsHanded())
    {
      TextData.Item localizedTextItem = LocalizationManager.Instance.GetLocalizedTextItem(this.item.data.localizationId);
      string hintTitle = localizedTextItem != null ? localizedTextItem.name : this.item.data.displayName;
      string hintDesignation = (double) this.item.data.value > 0.0 ? $"{this.item.data.tierString} / {this.item.OwnerString}" : this.item.data.tierString;
      if (this.IsHanded())
      {
        if ((double) this.axisLength > 0.0 || this.data.forceAllowTwoHanded)
        {
          if ((bool) (UnityEngine.Object) this.item.leftPlayerHand || (bool) (UnityEngine.Object) this.item.rightPlayerHand)
            return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle, hintDesignation);
          if (this.data.allowSteal)
            return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle, hintDesignation);
        }
        else if (this.data.allowSwap)
        {
          Handle grabbedHandle = ragdollHand.otherHand?.grabbedHandle;
          if ((bool) (UnityEngine.Object) grabbedHandle && (UnityEngine.Object) grabbedHandle.item == (UnityEngine.Object) this.item)
            return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle, hintDesignation);
        }
      }
      else
      {
        if ((bool) (UnityEngine.Object) this.item.leftPlayerHand || (bool) (UnityEngine.Object) this.item.rightPlayerHand)
          return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle, hintDesignation);
        if (this.data.allowSteal)
          return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle, hintDesignation);
      }
      return new Interactable.InteractionResult(ragdollHand, false);
    }
    string hintTitle1;
    string hintDesignation1;
    if ((UnityEngine.Object) this.item != (UnityEngine.Object) null)
    {
      if (string.IsNullOrEmpty(this.item.data.localizationId))
        Debug.LogWarning((object) $"Item {this.item.data.id} has no localization ID set!");
      TextData.Item localizedTextItem = LocalizationManager.Instance.GetLocalizedTextItem(this.item.data.localizationId);
      hintTitle1 = localizedTextItem != null ? localizedTextItem.name : this.item.data.displayName;
      hintDesignation1 = $"{this.item.data.tierString} / {this.item.OwnerString}";
    }
    else
    {
      if (!string.IsNullOrEmpty(this.data.localizationId))
      {
        TextData.Item localizedTextItem = LocalizationManager.Instance.GetLocalizedTextItem(this.data.localizationId);
        hintTitle1 = localizedTextItem == null ? LocalizationManager.Instance.GetLocalizedString("Default", this.data.localizationId) ?? this.data.highlightDefaultTitle : localizedTextItem.name;
      }
      else
        hintTitle1 = this.data.highlightDefaultTitle;
      hintDesignation1 = this.data.highlightDefaultDesignation;
    }
    return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle1, hintDesignation1);
  }

  public override bool TryTouchAction(RagdollHand ragdollHand, Interactable.Action action)
  {
    if ((bool) (UnityEngine.Object) this.item)
      this.item.OnTouchAction(ragdollHand, (Interactable) this, action);
    if (this is HandleRagdoll handleRagdoll)
      handleRagdoll.ragdollPart.OnTouchAction(ragdollHand, (Interactable) this, action);
    base.TryTouchAction(ragdollHand, action);
    if (action == Interactable.Action.Grab && (!this.data.disablePinchGrab || !PlayerControl.GetHand(ragdollHand.side).pinchPressed || PlayerControl.GetHand(ragdollHand.side).gripPressed) && !(bool) (UnityEngine.Object) ragdollHand.grabbedHandle)
    {
      Interactable.InteractionResult interactionResult1 = this.CheckInteraction(ragdollHand);
      if (interactionResult1.isInteractable)
      {
        if ((bool) (UnityEngine.Object) this.item?.holder)
        {
          Interactable.InteractionResult interactionResult2 = this.item.holder.CheckInteraction(ragdollHand);
          if (interactionResult2.isInteractable)
          {
            ragdollHand.GrabRelative(this);
            this.justGrabbed = true;
            return true;
          }
          if (interactionResult2.showHint)
            return true;
        }
        else
        {
          ragdollHand.GrabRelative(this);
          this.justGrabbed = true;
          return true;
        }
      }
      else if (interactionResult1.showHint)
        return true;
    }
    return false;
  }

  public virtual bool HeldActionAvailable(RagdollHand ragdollHand, Interactable.Action action)
  {
    if (action == Interactable.Action.Grab)
      this.justGrabbed = false;
    if (action != Interactable.Action.Ungrab || !this.justGrabbed || this.HoldGripToGrab())
      return true;
    this.justGrabbed = false;
    return false;
  }

  public virtual void HeldAction(RagdollHand ragdollHand, Interactable.Action action)
  {
    if (action == Interactable.Action.Ungrab)
      ragdollHand.UnGrab(true);
    if ((bool) (UnityEngine.Object) this.item)
      this.item.OnHeldAction(ragdollHand, this, action);
    if (this is HandleRagdoll handle)
      handle.ragdollPart.OnHeldAction(ragdollHand, handle, action);
    Interactable.ActionDelegate onHeldActionEvent = this.OnHeldActionEvent;
    if (onHeldActionEvent != null)
      onHeldActionEvent(ragdollHand, action);
    Interactable.Action action1 = GameManager.options.invertUseAndSlide ? Interactable.Action.UseStart : Interactable.Action.AlternateUseStart;
    Interactable.Action action2 = GameManager.options.invertUseAndSlide ? Interactable.Action.UseStop : Interactable.Action.AlternateUseStop;
    if ((double) this.axisLength > 0.0)
    {
      if (action == action1 && this.slideBehavior != Handle.SlideBehavior.DisallowSlide)
        this.SetSliding(ragdollHand, true);
      if (action != action2)
        return;
      this.SetSliding(ragdollHand, false);
    }
    else
    {
      if (!(bool) (UnityEngine.Object) this.moveToHandle || action != action1 || this.handlersCount <= 0)
        return;
      ragdollHand.UnGrab(false);
      ragdollHand.Grab(this.moveToHandle, this.moveToHandle.GetDefaultOrientation(ragdollHand.side), this.moveToHandleAxisPos);
    }
  }

  public virtual void SetSliding(RagdollHand ragdollHand, bool active)
  {
    if (this.SlidingStateChange != null)
      this.SlidingStateChange(ragdollHand, active, this, ragdollHand.gripInfo.axisPosition, EventTime.OnStart);
    if (active && !ragdollHand.gripInfo.isSliding)
    {
      if (!this.data.allowSlidingWithBothHand && ragdollHand.otherHand?.gripInfo != null && (UnityEngine.Object) ragdollHand.otherHand.grabbedHandle == (UnityEngine.Object) this && ragdollHand.otherHand.gripInfo.isSliding)
        return;
      ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.grabbedHandle.transform.position);
      ragdollHand.gripInfo.isSliding = true;
      ragdollHand.gripInfo.lastSlidePosition = ragdollHand.gripInfo.transform.position;
      this.slideForce = 0.0f;
      this.RefreshAllJointDrives();
      this.UpdateSliding();
    }
    else if (ragdollHand.gripInfo.isSliding)
    {
      if (this.SlidingStateChange != null)
        this.SlidingStateChange(ragdollHand, false, this, ragdollHand.gripInfo.axisPosition, EventTime.OnStart);
      ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
      ragdollHand.gripInfo.isSliding = false;
      ragdollHand.lastSliding = (Handle) null;
      this.RefreshAllJointDrives();
      this.UpdateSliding();
    }
    if (this.SlidingStateChange == null)
      return;
    this.SlidingStateChange(ragdollHand, active, this, ragdollHand.gripInfo.axisPosition, EventTime.OnEnd);
  }

  public virtual void FixedUpdateHandle(RagdollHand ragdollHand)
  {
    if (!this.initialized)
      return;
    Handle.GripInfo gripInfo = ragdollHand.gripInfo;
    if (gripInfo != null && gripInfo.hasPlayerJoint)
    {
      ConfigurableJoint playerJoint = gripInfo.playerJoint;
      PlayerHand playerHand = ragdollHand.playerHand;
      Player player = playerHand.player;
      Transform transform = player.transform;
      playerJoint.targetPosition = transform.InverseTransformPoint(playerHand.grip.position) - playerJoint.anchor;
      playerJoint.targetRotation = Quaternion.Inverse(transform.rotation) * playerHand.grip.rotation;
      playerJoint.anchor = transform.InverseTransformPoint(player.GetShoulderCenter());
    }
    this.UpdateSliding(ragdollHand);
  }

  public virtual void UpdateHandle(RagdollHand ragdollHand)
  {
    if (!this.initialized || !this.handlers.Contains(ragdollHand))
      return;
    this.UpdateAutoRotate(ragdollHand);
    this.UpdatePoses(ragdollHand);
    this.UpdateRedirectControllerAxis(ragdollHand);
  }

  protected virtual void UpdateRedirectControllerAxis(RagdollHand ragdollHand)
  {
    if (!this.initialized || !this.redirectControllerAxis || !(bool) (UnityEngine.Object) ragdollHand.playerHand)
      return;
    PlayerControl.Hand hand = PlayerControl.GetHand(ragdollHand.side);
    this.controllerAxisOutputX?.Invoke(hand.JoystickAxis.x);
    this.controllerAxisOutputY?.Invoke(hand.JoystickAxis.y);
  }

  protected virtual void UpdateAutoRotate(RagdollHand ragdollHand)
  {
    if (!this.initialized || !this.data.rotateAroundAxis)
      return;
    UnityEngine.Vector3 to = UnityEngine.Vector3.ProjectOnPlane(ragdollHand.grip.transform.position - ragdollHand.bone.animation.position, ragdollHand.gripInfo.transform.up);
    float num = UnityEngine.Vector3.SignedAngle(ragdollHand.grip.forward, to, ragdollHand.grip.up);
    UnityEngine.Vector3 forward = UnityEngine.Vector3.ProjectOnPlane(ragdollHand.gripInfo.transform.position - ragdollHand.lowerArmPart.bone.animation.position, ragdollHand.gripInfo.transform.up);
    ragdollHand.gripInfo.transform.rotation = Quaternion.LookRotation(forward, ragdollHand.gripInfo.transform.up);
    ragdollHand.gripInfo.transform.Rotate(0.0f, -num, 0.0f, Space.Self);
  }

  protected virtual void UpdateSliding()
  {
    if ((double) this.axisLength == 0.0)
      return;
    int handlersCount = this.handlersCount;
    for (int index = 0; index < handlersCount; ++index)
      this.UpdateSliding(this.handlers[index]);
  }

  protected virtual void UpdateSliding(RagdollHand ragdollHand)
  {
    if (!this.initialized || (double) this.axisLength == 0.0 || !(bool) (UnityEngine.Object) ragdollHand.playerHand)
      return;
    if (ragdollHand.gripInfo.hasEffect)
    {
      float num = Mathf.InverseLerp(this.data.slideFxMinVelocity, this.data.slideFxMaxVelocity, (ragdollHand.gripInfo.transform.position - ragdollHand.gripInfo.lastSlidePosition).magnitude / Time.fixedDeltaTime);
      ragdollHand.gripInfo.effectInstance.SetSpeed(num);
      ragdollHand.gripInfo.lastSlidePosition = ragdollHand.gripInfo.transform.position;
    }
    if (!ragdollHand.gripInfo.isSliding)
      return;
    UnityEngine.Vector3 vector3_1 = ragdollHand.gripInfo.orientation.transform.position - this.transform.position;
    float nearestAxisPosition = this.GetNearestAxisPosition(ragdollHand.playerHand.grip.position - vector3_1);
    float ratio = Utils.CalculateRatio(GameManager.options.invertUseAndSlide ? PlayerControl.GetHand(ragdollHand.side).useAxis : PlayerControl.GetHand(ragdollHand.side).alternateUseAxis, 0.2f, 1f, this.data.slideMaxDamper, this.data.slideMinDamper);
    ragdollHand.gripInfo.transform.position = this.GetNearestPositionAlongAxis(ragdollHand.playerHand.grip.position - vector3_1) + vector3_1;
    ragdollHand.gripInfo.joint.connectedAnchor = ragdollHand.gripInfo.playerJoint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
    JointDrive yDrive = ragdollHand.gripInfo.joint.yDrive;
    if ((double) nearestAxisPosition >= (double) this.axisLength / 2.0 || (double) nearestAxisPosition <= -((double) this.axisLength / 2.0))
    {
      yDrive.positionSpring = ragdollHand.creature.GetPositionJointConfig().x * this.data.positionSpringMultiplier;
      yDrive.positionDamper = ragdollHand.creature.GetPositionJointConfig().y * this.data.positionDamperMultiplier;
    }
    else
    {
      yDrive.positionSpring = 0.0f;
      yDrive.positionDamper = ratio;
    }
    ragdollHand.gripInfo.joint.yDrive = yDrive;
    if ((double) this.data.slideMotorMaxForce > 0.0 && (bool) (UnityEngine.Object) ragdollHand.playerHand)
    {
      UnityEngine.Vector3 vector3_2 = (double) this.transform.up.y < 0.0 ? this.transform.up : -this.transform.up;
      if (this.data.slideMotorDir == HandleData.SlideMotorDirection.Down)
        vector3_2 = -this.transform.up;
      if (this.data.slideMotorDir == HandleData.SlideMotorDirection.Up)
        vector3_2 = this.transform.up;
      this.slideForce = Mathf.Clamp(this.slideForce + this.data.slideMotorAcceleration, 0.0f, this.data.slideMotorMaxForce);
      ragdollHand.playerHand.player.locomotion.physicBody.AddForce(vector3_2 * this.slideForce, ForceMode.Force);
    }
    if ((double) nearestAxisPosition > (double) this.axisLength / 2.0 - (double) this.slideToHandleOffset)
    {
      if (!(bool) (UnityEngine.Object) this.slideToUpHandle || !((UnityEngine.Object) ragdollHand.lastSliding != (UnityEngine.Object) this.slideToUpHandle))
        return;
      this.SlideHandToOtherHandle(ragdollHand, this.slideToUpHandle);
    }
    else if ((double) nearestAxisPosition < -((double) this.axisLength / 2.0) + (double) this.slideToHandleOffset)
    {
      if (!(bool) (UnityEngine.Object) this.slideToBottomHandle || !((UnityEngine.Object) ragdollHand.lastSliding != (UnityEngine.Object) this.slideToBottomHandle))
        return;
      this.SlideHandToOtherHandle(ragdollHand, this.slideToBottomHandle);
    }
    else
      ragdollHand.lastSliding = (Handle) null;
  }

  protected virtual void UpdatePoses(RagdollHand ragdollHand)
  {
    if (!this.initialized || !this.updatePosesAutomatically)
      return;
    HandlePose orientation = ragdollHand.gripInfo.orientation;
    if (Mathf.Approximately(orientation.targetWeight, orientation.lastTargetWeight))
      return;
    ragdollHand.poser.SetTargetWeight(orientation.targetWeight);
    orientation.lastTargetWeight = orientation.targetWeight;
  }

  public virtual void SlideHandToOtherHandle(RagdollHand hand, Handle target, bool silent = true)
  {
    if (this.SlideToOtherHandle != null)
      this.SlideToOtherHandle(hand, this, target, EventTime.OnStart);
    hand.UnGrab(false);
    hand.lastSliding = this;
    bool silentGrab = target.silentGrab;
    if (silent)
      target.silentGrab = true;
    hand.GrabRelative(target);
    target.silentGrab = silentGrab;
    target.SetSliding(hand, target.slideBehavior == Handle.SlideBehavior.KeepSlide);
    if (this.SlideToOtherHandle == null)
      return;
    this.SlideToOtherHandle(hand, this, target, EventTime.OnEnd);
  }

  public virtual bool IsHanded() => this.handlers != null && this.handlersCount > 0;

  public virtual void OnTelekinesisGrab(SpellTelekinesis spellTelekinesis)
  {
    this.telekinesisHandlers.Add(spellTelekinesis.spellCaster);
    if ((bool) (UnityEngine.Object) this.item)
    {
      if ((bool) (UnityEngine.Object) this.item.holder)
        this.item.holder.UnSnap(this.item);
      foreach (CollisionHandler collisionHandler in this.item.collisionHandlers)
        collisionHandler.SetPhysicModifier((object) this, new float?(spellTelekinesis.gravity ? 1f : 0.0f), drag: spellTelekinesis.drag, angularDrag: spellTelekinesis.angularDrag);
      this.item.physicBody.sleepThreshold = 0.0f;
      this.item.StopThrowing();
      this.item.StopFlying();
      this.item.IgnoreIsMoving();
      this.item.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
      this.item.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.telekinesis;
      this.item.SetCenterOfMass(this.transform.localPosition + new UnityEngine.Vector3(0.0f, this.GetDefaultAxisLocalPosition(), 0.0f));
      this.item.isTelekinesisGrabbed = true;
      if (this.IsTkGrabbed)
      {
        this.item.tkHandlers.Add(spellTelekinesis.spellCaster);
        this.item.OnTelekinesisGrab(this, spellTelekinesis);
        this.item.lastHandler = spellTelekinesis.spellCaster.ragdollHand;
      }
    }
    Handle.TkEvent tkGrabbed = this.TkGrabbed;
    if (tkGrabbed == null)
      return;
    tkGrabbed(this, spellTelekinesis);
  }

  public virtual void OnTelekinesisRelease(
    SpellTelekinesis spellTelekinesis,
    bool tryThrow,
    out bool throwing,
    bool isGrabbing)
  {
    throwing = false;
    this.telekinesisHandlers.Remove(spellTelekinesis.spellCaster);
    if ((bool) (UnityEngine.Object) this.item)
    {
      this.item.tkHandlers.Remove(spellTelekinesis.spellCaster);
      bool flag1 = true;
      foreach (Handle handle in this.item.handles)
      {
        if (handle.IsTkGrabbed)
        {
          flag1 = false;
          break;
        }
      }
      if (flag1)
      {
        foreach (CollisionHandler collisionHandler in this.item.collisionHandlers)
        {
          foreach (Handle handle in this.item.handles)
            collisionHandler.RemovePhysicModifier((object) handle);
        }
        this.item.ResetCenterOfMass();
        this.item.isTelekinesisGrabbed = false;
        this.item.OnTelekinesisRelease(this, spellTelekinesis, tryThrow, isGrabbing);
        if (tryThrow)
        {
          UnityEngine.Vector3 from = Player.local.transform.rotation * PlayerControl.GetHand(spellTelekinesis.spellCaster.ragdollHand.side).GetHandVelocity() * (1f / Time.timeScale);
          bool flag2 = false;
          if ((double) from.sqrMagnitude > (double) SpellCaster.throwMinHandVelocity * (double) SpellCaster.throwMinHandVelocity)
          {
            if (this.item.isPenetrating)
            {
              for (int index = this.item.mainCollisionHandler.collisions.Length - 1; index >= 0; --index)
              {
                CollisionInstance collision = this.item.mainCollisionHandler.collisions[index];
                if (!((UnityEngine.Object) collision?.damageStruct.penetrationJoint == (UnityEngine.Object) null))
                {
                  UnityEngine.Vector3 to = -collision.damageStruct.damager.transform.forward;
                  if (collision.damageStruct.damager.type == Damager.Type.Pierce && (double) UnityEngine.Vector3.Angle(from, to) < 40.0)
                  {
                    flag2 = true;
                    collision.damageStruct.damager.UnPenetrate(collision);
                  }
                }
              }
            }
            this.item.physicBody.velocity = from.normalized * 0.01f;
            this.item.physicBody.AddForce(from.normalized * (float) ((double) spellTelekinesis.pushDefaultForce * (double) this.item.distantGrabThrowRatio * (flag2 ? 0.5 : 1.0)), ForceMode.VelocityChange);
            this.item.Throw(spellTelekinesis.throwMultiplier, this.item.HasFlag(ItemFlags.Throwable) || !spellTelekinesis.spinMode ? Item.FlyDetection.Forced : Item.FlyDetection.Disabled);
            if (spellTelekinesis.clearFloatingOnThrow)
              this.item.Clear("Floating");
            throwing = true;
          }
        }
      }
    }
    Handle.TkEvent tkUnGrabbed = this.TkUnGrabbed;
    if (tkUnGrabbed == null)
      return;
    tkUnGrabbed(this, spellTelekinesis);
  }

  public void ReleaseAllTkHandlers()
  {
    if (!this.IsTkGrabbed)
      return;
    for (int index = this.telekinesisHandlers.Count - 1; index >= 0; --index)
      this.telekinesisHandlers[index].telekinesis.TryRelease();
  }

  public virtual void OnGrab(
    RagdollHand ragdollHand,
    float axisPosition,
    HandlePose handlePose,
    bool teleportToHand = false)
  {
    if (this.physicBody == (PhysicBody) null)
      Debug.LogError((object) $"Handle {this.name} has no physic body", (UnityEngine.Object) this.gameObject);
    else if ((UnityEngine.Object) handlePose == (UnityEngine.Object) null)
      Debug.LogError((object) $"There is no handle pose assigned to {this.name}! This causes arms to break.");
    else if ((UnityEngine.Object) handlePose.handle == (UnityEngine.Object) null)
    {
      Debug.LogError((object) $"Handle is not assigned on HandlePose {handlePose.name}, or the handle reference was set incorrectly and has been despawned! This causes arms to break.");
    }
    else
    {
      Handle.GrabEvent grabbed = this.Grabbed;
      if (grabbed != null)
        grabbed(ragdollHand, this, EventTime.OnStart);
      if ((bool) (UnityEngine.Object) this.item?.holder)
      {
        if (this.item.holder.data.grabTeleport == HolderData.GrabTeleport.Enabled)
          teleportToHand = true;
        else if (this.item.holder.data.grabTeleport == HolderData.GrabTeleport.IfParentHolder && (bool) (UnityEngine.Object) this.item.holder.parentHolder)
          teleportToHand = true;
        this.item.holder.UnSnap(this.item);
      }
      if ((bool) (UnityEngine.Object) this.item && this.item.isTelekinesisGrabbed)
      {
        foreach (Handle handle in this.item.handles)
          handle.ReleaseAllTkHandlers();
      }
      if ((bool) (UnityEngine.Object) this.releaseHandle)
      {
        Handle component = this.releaseHandle.GetComponent<Handle>();
        for (int index = component.handlersCount - 1; index >= 0; --index)
          component.handlers[index].TryRelease();
      }
      if ((double) this.axisLength == 0.0 && !this.data.forceAllowTwoHanded)
        this.Release();
      if (handlePose.defaultHandPoseData == null || handlePose.targetHandPoseData == null)
        handlePose.LoadHandPosesData();
      ragdollHand.poser.SetGripFromPose(handlePose.defaultHandPoseData);
      ragdollHand.poser.SetDefaultPose(handlePose.defaultHandPoseData);
      ragdollHand.poser.SetTargetPose(handlePose.targetHandPoseData);
      ragdollHand.poser.SetTargetWeight(handlePose.targetWeight);
      Physics.IgnoreCollision(ragdollHand.touchCollider, this.touchCollider, true);
      if (this.data.disableHandCollider)
      {
        ragdollHand.simplifiedCollider.enabled = false;
        if (ragdollHand.ForeArmColliders != null)
        {
          for (int index = 0; index < ragdollHand.ForeArmColliders.Count; ++index)
          {
            if ((bool) (UnityEngine.Object) ragdollHand.ForeArmColliders[index])
              ragdollHand.ForeArmColliders[index].enabled = false;
          }
        }
      }
      Highlighter.GetSide(ragdollHand.side).Hide();
      ragdollHand.gripInfo = this.CreateGripPoint(ragdollHand, axisPosition, handlePose);
      if ((bool) (UnityEngine.Object) ragdollHand.playerHand && (ragdollHand.creature.ragdoll.state == Ragdoll.State.NoPhysic || ragdollHand.creature.ragdoll.state == Ragdoll.State.Kinematic))
      {
        UnityEngine.Vector3 position = this.physicBody.transform.position;
        Quaternion rotation = this.physicBody.transform.rotation;
        ragdollHand.creature.ragdoll.ik.SetHandAnchor(ragdollHand.side, ragdollHand.gripInfo.ikAnchor);
        ragdollHand.gripInfo.ikAnchor.position = ragdollHand.gripInfo.transform.TransformPointUnscaled(ragdollHand.grip.InverseTransformPointUnscaled(ragdollHand.transform.position) + new UnityEngine.Vector3(ragdollHand.side == Side.Right ? this.ikAnchorOffset.x : -this.ikAnchorOffset.x, this.ikAnchorOffset.y, this.ikAnchorOffset.z));
        ragdollHand.gripInfo.ikAnchor.localRotation = Quaternion.Inverse(ragdollHand.grip.rotation) * ragdollHand.transform.rotation;
        this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.playerHand.grip.transform);
        if ((bool) (UnityEngine.Object) ragdollHand.gripInfo.joint)
        {
          Debug.LogError((object) "gripInfo.joint already exist");
          UnityEngine.Object.Destroy((UnityEngine.Object) ragdollHand.gripInfo.joint);
        }
        ragdollHand.gripInfo.joint = ragdollHand.playerHand.grip.gameObject.AddComponent<ConfigurableJoint>();
        ragdollHand.gripInfo.joint.anchor = UnityEngine.Vector3.zero;
        ragdollHand.gripInfo.joint.autoConfigureConnectedAnchor = false;
        ragdollHand.gripInfo.joint.rotationDriveMode = this.data.rotationDrive != HandleData.RotationDrive.Slerp || Handle.globalForceXYZ ? RotationDriveMode.XYAndZ : RotationDriveMode.Slerp;
        if ((bool) (UnityEngine.Object) this.customRigidBody)
        {
          ragdollHand.gripInfo.joint.connectedAnchor = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
          ragdollHand.gripInfo.joint.connectedBody = this.customRigidBody;
        }
        else
        {
          ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
          ragdollHand.gripInfo.joint.SetConnectedPhysicBody(this.physicBody);
        }
        this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.playerHand.player.locomotion.physicBody.transform);
        ragdollHand.gripInfo.playerJoint = ragdollHand.playerHand.player.locomotion.physicBody.gameObject.AddComponent<ConfigurableJoint>();
        ragdollHand.gripInfo.playerJoint.enableCollision = true;
        ragdollHand.gripInfo.playerJoint.autoConfigureConnectedAnchor = false;
        ragdollHand.gripInfo.playerJoint.anchor = UnityEngine.Vector3.zero;
        if ((bool) (UnityEngine.Object) this.customRigidBody)
        {
          ragdollHand.gripInfo.playerJoint.connectedAnchor = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
          ragdollHand.gripInfo.playerJoint.connectedBody = this.customRigidBody;
        }
        else
        {
          ragdollHand.gripInfo.playerJoint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
          ragdollHand.gripInfo.playerJoint.SetConnectedPhysicBody(this.physicBody);
        }
        this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.playerHand.grip.transform);
        ragdollHand.gripInfo.hasPlayerJoint = true;
        if (!teleportToHand)
        {
          this.physicBody.transform.position = position;
          this.physicBody.transform.rotation = rotation;
        }
        ragdollHand.gripInfo.type = Handle.GripInfo.Type.PlayerJoint;
      }
      else if ((bool) (UnityEngine.Object) ragdollHand.otherHand?.grabbedHandle && (UnityEngine.Object) ragdollHand.otherHand.grabbedHandle.item == (UnityEngine.Object) this.item)
      {
        ragdollHand.creature.ragdoll.ik.SetHandAnchor(ragdollHand.side, ragdollHand.gripInfo.ikAnchor);
        ragdollHand.gripInfo.ikAnchor.position = ragdollHand.gripInfo.transform.TransformPointUnscaled(ragdollHand.grip.InverseTransformPointUnscaled(ragdollHand.transform.position) + new UnityEngine.Vector3(ragdollHand.side == Side.Right ? this.ikAnchorOffset.x : -this.ikAnchorOffset.x, this.ikAnchorOffset.y, this.ikAnchorOffset.z));
        ragdollHand.gripInfo.ikAnchor.localRotation = Quaternion.Inverse(ragdollHand.grip.rotation) * ragdollHand.transform.rotation;
        ragdollHand.gripInfo.type = Handle.GripInfo.Type.IKOnly;
      }
      else if (ragdollHand.creature.ragdoll.state == Ragdoll.State.NoPhysic || ragdollHand.creature.ragdoll.state == Ragdoll.State.Kinematic || ragdollHand.creature.ragdoll.state == Ragdoll.State.Disabled)
        this.Attach(ragdollHand, false);
      else
        this.Attach(ragdollHand, true);
      if (this.data.setCenterOfMassTohandle)
      {
        if ((bool) (UnityEngine.Object) this.customRigidBody)
          this.customRigidBody.centerOfMass = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
        else
          this.physicBody.centerOfMass = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
      }
      if ((bool) (UnityEngine.Object) ragdollHand.playerHand)
      {
        ragdollHand.playerHand.player.locomotion.OnGroundEvent += new Locomotion.GroundEvent(this.OnPlayerLocomotionGroundEvent);
        ragdollHand.playerHand.player.locomotion.OnFlyEvent += new Locomotion.FlyEvent(this.OnPlayerLocomotionFlyEvent);
      }
      ragdollHand.creature.currentLocomotion.SetSpeedModifier((object) this.data.handlerLocomotionSpeedMultiplier);
      if (this.data.grabEffectData != null)
      {
        ragdollHand.gripInfo.effectInstance = this.data.grabEffectData.Spawn(ragdollHand.gripInfo.transform.position, ragdollHand.gripInfo.transform.rotation, ragdollHand.gripInfo.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
        ragdollHand.gripInfo.effectInstance.SetIntensity(0.0f);
        ragdollHand.gripInfo.effectInstance.Play(skipStarts: this.silentGrab);
        ragdollHand.gripInfo.hasEffect = true;
      }
      if (this.redirectControllerAxis)
      {
        if (ragdollHand.side == PlayerControl.local.locomotionController)
        {
          Player.local.locomotion.allowMove = false;
        }
        else
        {
          Player.local.locomotion.allowTurn = false;
          Player.local.locomotion.allowJump = false;
          Player.local.locomotion.allowCrouch = false;
        }
      }
      if (!this.handlers.Contains(ragdollHand))
        this.handlers.Add(ragdollHand);
      this.handlersCount = this.handlers.Count;
      ragdollHand.grabbedHandle = this;
      ragdollHand.lastHandle = this;
      if ((bool) (UnityEngine.Object) this.item)
        this.item.OnGrab(this, ragdollHand);
      this.RefreshAllJointDrives();
      this.UpdateAutoRotate(ragdollHand);
      if (this.Grabbed == null)
        return;
      this.Grabbed(ragdollHand, this, EventTime.OnEnd);
    }
  }

  public virtual void Attach(RagdollHand ragdollHand, bool usePhysic)
  {
    if ((bool) (UnityEngine.Object) ragdollHand.gripInfo.joint)
      UnityEngine.Object.Destroy((UnityEngine.Object) ragdollHand.gripInfo.joint);
    ragdollHand.gripInfo.physicBody = ragdollHand.physicBody;
    if ((bool) (UnityEngine.Object) this.item && this.physicBody == this.item.physicBody)
      this.physicBody.transform.SetParent((Transform) null, true);
    if (usePhysic)
    {
      this.physicBody.isKinematic = false;
      this.MoveAndAlignToHand(ragdollHand);
      ragdollHand.gripInfo.joint = ragdollHand.physicBody.gameObject.AddComponent<ConfigurableJoint>();
      ragdollHand.gripInfo.joint.autoConfigureConnectedAnchor = false;
      ragdollHand.gripInfo.joint.anchor = ragdollHand.transform.InverseTransformPoint(ragdollHand.grip.position);
      if ((bool) (UnityEngine.Object) this.customRigidBody)
      {
        ragdollHand.gripInfo.joint.connectedBody = this.customRigidBody;
        ragdollHand.gripInfo.joint.connectedAnchor = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
      }
      else
      {
        ragdollHand.gripInfo.joint.SetConnectedPhysicBody(this.physicBody);
        ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
      }
      ragdollHand.gripInfo.joint.xMotion = ConfigurableJointMotion.Locked;
      ragdollHand.gripInfo.joint.yMotion = ConfigurableJointMotion.Locked;
      ragdollHand.gripInfo.joint.zMotion = ConfigurableJointMotion.Locked;
      ragdollHand.gripInfo.joint.angularXMotion = ConfigurableJointMotion.Locked;
      ragdollHand.gripInfo.joint.angularYMotion = ConfigurableJointMotion.Locked;
      ragdollHand.gripInfo.joint.angularZMotion = ConfigurableJointMotion.Locked;
      ragdollHand.gripInfo.type = Handle.GripInfo.Type.HandJoint;
    }
    else
    {
      this.physicBody.isKinematic = true;
      ragdollHand.gripInfo.type = Handle.GripInfo.Type.HandSync;
    }
  }

  public virtual void MoveAndAlignToHand(RagdollHand ragdollHand)
  {
    UnityEngine.Vector3 position = ragdollHand.transform.InverseTransformPointUnscaled(ragdollHand.grip.position);
    Quaternion localRotation = ragdollHand.transform.InverseTransformRotation(ragdollHand.grip.rotation);
    this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.transform.TransformPoint(position), ragdollHand.transform.TransformRotation(localRotation));
  }

  protected void OnPlayerLocomotionGroundEvent(
    Locomotion locomotion,
    UnityEngine.Vector3 groundPoint,
    UnityEngine.Vector3 velocity,
    Collider groundCollider)
  {
    this.RefreshJointDrive();
  }

  protected void OnPlayerLocomotionFlyEvent(Locomotion locomotion) => this.RefreshJointDrive();

  public virtual void OnUnGrab(RagdollHand ragdollHand, bool throwing)
  {
    if (this.UnGrabbed != null)
      this.UnGrabbed(ragdollHand, this, EventTime.OnStart);
    ragdollHand.poser.ResetDefaultPose();
    ragdollHand.poser.ResetTargetPose();
    ragdollHand.poser.SetTargetWeight(0.0f);
    ragdollHand.ResetGripPositionAndRotation();
    Physics.IgnoreCollision(ragdollHand.touchCollider, this.touchCollider, false);
    if (this.data.disableHandCollider && !(bool) (UnityEngine.Object) ragdollHand.playerHand)
    {
      ragdollHand.simplifiedCollider.enabled = true;
      if (ragdollHand.ForeArmColliders != null)
      {
        for (int index = 0; index < ragdollHand.ForeArmColliders.Count; ++index)
        {
          if ((bool) (UnityEngine.Object) ragdollHand.ForeArmColliders[index])
            ragdollHand.ForeArmColliders[index].enabled = true;
        }
      }
    }
    if ((bool) (UnityEngine.Object) ragdollHand.playerHand)
    {
      ragdollHand.playerHand.player.locomotion.OnGroundEvent -= new Locomotion.GroundEvent(this.OnPlayerLocomotionGroundEvent);
      ragdollHand.playerHand.player.locomotion.OnFlyEvent -= new Locomotion.FlyEvent(this.OnPlayerLocomotionFlyEvent);
    }
    if ((bool) (UnityEngine.Object) this.item && !this.item.IsHanded(this) && (bool) (UnityEngine.Object) this.physicBody.rigidBody)
      this.item.transform.SetParent((Transform) null, true);
    bool flag = false;
    if (ragdollHand.gripInfo.type == Handle.GripInfo.Type.PlayerJoint)
      ragdollHand.creature.ragdoll.ik.SetHandAnchor(ragdollHand.side, ragdollHand.transform);
    else if (ragdollHand.gripInfo.type == Handle.GripInfo.Type.HandSync)
    {
      this.physicBody.isKinematic = false;
      flag = true;
    }
    if (this.data.setCenterOfMassTohandle)
    {
      if ((bool) (UnityEngine.Object) this.customRigidBody)
        this.customRigidBody.ResetCenterOfMass();
      else if ((bool) (UnityEngine.Object) this.item)
        this.item.ResetCenterOfMass();
    }
    if ((UnityEngine.Object) ragdollHand.otherHand?.grabbedHandle == (UnityEngine.Object) null)
      ragdollHand.creature.currentLocomotion.RemoveSpeedModifier((object) this);
    if (this.redirectControllerAxis)
    {
      if (ragdollHand.side == PlayerControl.local.locomotionController)
      {
        Player.local.locomotion.allowMove = true;
      }
      else
      {
        Player.local.locomotion.allowTurn = true;
        Player.local.locomotion.allowJump = true;
        Player.local.locomotion.allowCrouch = true;
      }
      this.controllerAxisOutputX?.Invoke(0.0f);
      this.controllerAxisOutputY?.Invoke(0.0f);
    }
    ragdollHand.gripInfo.Destroy();
    this.handlers.Remove(ragdollHand);
    this.handlersCount = this.handlers.Count;
    ragdollHand.gripInfo = (Handle.GripInfo) null;
    ragdollHand.lastHandle = this;
    ragdollHand.grabbedHandle = (Handle) null;
    if ((bool) (UnityEngine.Object) this.item)
    {
      this.item.OnUnGrab(this, ragdollHand, throwing);
      if (flag)
        this.item.IgnoreRagdollCollision(ragdollHand.ragdoll, RagdollPart.Type.LeftArm | RagdollPart.Type.RightArm | RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand);
    }
    this.RefreshAllJointDrives();
    if (this.UnGrabbed == null)
      return;
    this.UnGrabbed(ragdollHand, this, EventTime.OnEnd);
  }

  public virtual Handle.GripInfo CreateGripPoint(
    RagdollHand ragdollHand,
    float axisPosition,
    HandlePose orientation)
  {
    Handle.GripInfo gripPoint = new Handle.GripInfo(this.transform);
    gripPoint.ragdollHand = ragdollHand;
    gripPoint.orientation = orientation;
    gripPoint.axisPosition = axisPosition;
    gripPoint.transform.position = orientation.transform.position + orientation.handle.transform.up * (axisPosition * orientation.handle.transform.lossyScale.y);
    gripPoint.transform.rotation = orientation.transform.rotation;
    gripPoint.ikAnchor = new GameObject("Anchor").transform;
    gripPoint.ikAnchor.SetParentOrigin(gripPoint.transform);
    return gripPoint;
  }

  public virtual void RefreshAllJointDrives()
  {
    if ((bool) (UnityEngine.Object) this.item)
    {
      foreach (Handle handle in this.item.handles)
        handle.RefreshJointDrive();
    }
    else
      this.RefreshJointDrive();
  }

  public virtual void RefreshJointDrive()
  {
    if (!this.IsHanded())
      return;
    if (this.handlersCount == 1)
    {
      RagdollHand handler = this.handlers[0];
      if ((bool) (UnityEngine.Object) this.item && this.item.IsTwoHanded())
      {
        UnityEngine.Vector2 vector2 = new UnityEngine.Vector2(handler.creature.data.forceRotationSpringDamper2HMult.x * this.data.rotationSpring2hMultiplier, handler.creature.data.forceRotationSpringDamper2HMult.y * this.data.rotationDamper2hMultiplier);
        if ((bool) (UnityEngine.Object) handler.otherHand?.grabbedHandle && (bool) (UnityEngine.Object) handler.otherHand.grabbedHandle.item && (UnityEngine.Object) handler.otherHand.grabbedHandle.item == (UnityEngine.Object) this.item)
        {
          if (this.data.dominantWhenTwoHanded && handler.otherHand.grabbedHandle.data.dominantWhenTwoHanded)
            this.SetJointConfig(handler, handler.creature.data.forcePositionSpringDamper2HMult, handler.side == Handle.dominantHand ? vector2 : UnityEngine.Vector2.zero, this.data.rotationDrive);
          else
            this.SetJointConfig(handler, handler.creature.data.forcePositionSpringDamper2HMult, this.data.dominantWhenTwoHanded ? vector2 : UnityEngine.Vector2.zero, this.data.rotationDrive);
        }
        else
          this.SetJointConfig(handler, handler.creature.data.forcePositionSpringDamper2HMult, this.data.dominantWhenTwoHanded ? vector2 : UnityEngine.Vector2.zero, this.data.rotationDrive);
      }
      else
        this.SetJointConfig(handler, UnityEngine.Vector2.one, UnityEngine.Vector2.one, this.data.rotationDrive);
    }
    if (this.handlersCount == 2)
    {
      RagdollHand handler1 = this.handlers[0];
      RagdollHand handler2 = this.handlers[1];
      if (this.data.dominantWhenTwoHanded && Handle.twoHandedMode != Handle.TwoHandedMode.Position)
      {
        if ((double) UnityEngine.Vector3.Dot(handler1.gripInfo.transform.up, handler2.gripInfo.transform.up) > 0.0)
        {
          if ((double) UnityEngine.Vector3.Dot(handler1.gripInfo.transform.up, this.transform.up) > 0.0)
          {
            switch (Handle.twoHandedMode)
            {
              case Handle.TwoHandedMode.AutoFront:
                if ((double) this.GetNearestAxisPosition(handler1.gripInfo.transform.position) > (double) this.GetNearestAxisPosition(handler2.gripInfo.transform.position))
                {
                  this.SetJointToTwoHanded(handler1.side);
                  break;
                }
                this.SetJointToTwoHanded(handler2.side);
                break;
              case Handle.TwoHandedMode.AutoRear:
                if ((double) this.GetNearestAxisPosition(handler1.gripInfo.transform.position) > (double) this.GetNearestAxisPosition(handler2.gripInfo.transform.position))
                {
                  this.SetJointToTwoHanded(handler2.side);
                  break;
                }
                this.SetJointToTwoHanded(handler1.side);
                break;
              default:
                this.SetJointToTwoHanded(Handle.dominantHand);
                break;
            }
          }
          else
          {
            switch (Handle.twoHandedMode)
            {
              case Handle.TwoHandedMode.AutoFront:
                if ((double) this.GetNearestAxisPosition(handler1.gripInfo.transform.position) < (double) this.GetNearestAxisPosition(handler2.gripInfo.transform.position))
                {
                  this.SetJointToTwoHanded(handler1.side);
                  break;
                }
                this.SetJointToTwoHanded(handler2.side);
                break;
              case Handle.TwoHandedMode.AutoRear:
                if ((double) this.GetNearestAxisPosition(handler1.gripInfo.transform.position) < (double) this.GetNearestAxisPosition(handler2.gripInfo.transform.position))
                {
                  this.SetJointToTwoHanded(handler2.side);
                  break;
                }
                this.SetJointToTwoHanded(handler1.side);
                break;
              default:
                this.SetJointToTwoHanded(Handle.dominantHand);
                break;
            }
          }
        }
        else
          this.SetJointToTwoHanded(Handle.dominantHand, 0.1f);
      }
      else
        this.SetJointToTwoHanded(Handle.dominantHand, 0.1f);
    }
    if (this.handlersCount <= 2)
      return;
    Debug.LogError((object) "More than 2 handler is not supported right now");
  }

  public virtual void SetJointToTwoHanded(Side dominantSide, float rotationMultiplier = 1f)
  {
    RagdollHand handler1 = this.handlers.First<RagdollHand>();
    RagdollHand handler2 = this.handlers.Last<RagdollHand>();
    this.SetJointConfig(handler1, handler1.creature.data.forceSpringDamper2HNoDomMult, handler1.side == dominantSide ? handler1.creature.data.forceRotationSpringDamper2HMult * rotationMultiplier : UnityEngine.Vector2.zero, this.data.useWYZWhenTwoHanded ? HandleData.RotationDrive.WYZ : this.data.rotationDrive);
    this.SetJointConfig(handler2, handler2.creature.data.forceSpringDamper2HNoDomMult, handler2.side == dominantSide ? handler2.creature.data.forceRotationSpringDamper2HMult * rotationMultiplier : UnityEngine.Vector2.zero, this.data.useWYZWhenTwoHanded ? HandleData.RotationDrive.WYZ : this.data.rotationDrive);
  }

  public virtual void SetJointDrive(UnityEngine.Vector2 positionMultiplier, UnityEngine.Vector2 rotationMultiplier)
  {
    foreach (RagdollHand handler in this.handlers)
      this.SetJointConfig(handler, positionMultiplier, rotationMultiplier, this.data.rotationDrive);
  }

  public virtual void SetForcePlayerJointModifier(object handler, bool active)
  {
    if (active)
    {
      if (!this.forcePlayerJointHandlers.Contains(handler))
        this.forcePlayerJointHandlers.Add(handler);
      if (this.forcePlayerJoint)
        return;
      this.forcePlayerJoint = true;
      this.RefreshJointDrive();
    }
    else
    {
      for (int index = 0; index < this.forcePlayerJointHandlers.Count; ++index)
      {
        if (this.forcePlayerJointHandlers[index] == handler)
        {
          this.forcePlayerJointHandlers.RemoveAtIgnoreOrder<object>(index);
          --index;
        }
      }
      if (this.forcePlayerJointHandlers.Count != 0 || this.forcePlayerJoint == active || !this.forcePlayerJoint)
        return;
      this.forcePlayerJoint = false;
      this.RefreshJointDrive();
    }
  }

  public virtual void SetJointModifier(
    object handler,
    float positionSpringMultiplier = 1f,
    float positionDamperMultiplier = 1f,
    float rotationSpringMultiplier = 1f,
    float rotationDamperMultiplier = 1f)
  {
    Handle.JointModifier jointModifier = (Handle.JointModifier) null;
    for (int index = 0; index < this.jointModifiers.Count; ++index)
    {
      if (this.jointModifiers[index].handler == handler)
      {
        jointModifier = this.jointModifiers[index];
        break;
      }
    }
    if (jointModifier == null)
    {
      jointModifier = new Handle.JointModifier()
      {
        handler = handler
      };
      this.jointModifiers.Add(jointModifier);
    }
    jointModifier.positionSpringMultiplier = positionSpringMultiplier;
    jointModifier.positionDamperMultiplier = positionDamperMultiplier;
    jointModifier.rotationSpringMultiplier = rotationSpringMultiplier;
    jointModifier.rotationDamperMultiplier = rotationDamperMultiplier;
    this.RefreshJointModifiers();
  }

  public virtual void RemoveJointModifier(object handler)
  {
    for (int index = 0; index < this.jointModifiers.Count; ++index)
    {
      if (this.jointModifiers[index].handler == handler)
      {
        this.jointModifiers.RemoveAtIgnoreOrder<Handle.JointModifier>(index);
        --index;
      }
    }
    this.RefreshJointModifiers();
  }

  public virtual void ClearJointModifiers()
  {
    this.jointModifiers.Clear();
    this.RefreshJointModifiers();
  }

  public virtual void RefreshJointModifiers()
  {
    this.positionSpringMultiplier = 1f;
    this.positionDamperMultiplier = 1f;
    this.rotationSpringMultiplier = 1f;
    this.rotationDamperMultiplier = 1f;
    foreach (Handle.JointModifier jointModifier in this.jointModifiers)
    {
      if ((double) jointModifier.positionSpringMultiplier >= 0.0 && (double) jointModifier.positionSpringMultiplier != 1.0)
        this.positionSpringMultiplier *= jointModifier.positionSpringMultiplier;
      if ((double) jointModifier.positionDamperMultiplier >= 0.0 && (double) jointModifier.positionDamperMultiplier != 1.0)
        this.positionDamperMultiplier *= jointModifier.positionDamperMultiplier;
      if ((double) jointModifier.rotationSpringMultiplier >= 0.0 && (double) jointModifier.rotationSpringMultiplier != 1.0)
        this.rotationSpringMultiplier *= jointModifier.rotationSpringMultiplier;
      if ((double) jointModifier.rotationDamperMultiplier >= 0.0 && (double) jointModifier.rotationDamperMultiplier != 1.0)
        this.rotationDamperMultiplier *= jointModifier.rotationDamperMultiplier;
    }
    this.RefreshJointDrive();
  }

  public virtual void SetJointConfig(
    RagdollHand handler,
    UnityEngine.Vector2 positionMultiplier,
    UnityEngine.Vector2 rotationMultiplier,
    HandleData.RotationDrive rotationDrive)
  {
    if (!(bool) (UnityEngine.Object) handler.creature.player)
      return;
    if (handler.creature.player.locomotion.isGrounded && !this.data.forceClimbing && !this.forcePlayerJoint && (!(bool) (UnityEngine.Object) this.item || !this.item.data.grabAndGripClimb))
    {
      if ((bool) (UnityEngine.Object) handler.gripInfo.joint)
        this.SetJointConfig(handler.gripInfo.joint, handler, positionMultiplier, rotationMultiplier, handler.creature.data.forceMaxPosition, handler.creature.data.forceMaxRotation, rotationDrive);
      if ((bool) (UnityEngine.Object) handler.gripInfo.playerJoint)
        this.SetJointConfig(handler.gripInfo.playerJoint, handler, UnityEngine.Vector2.zero, UnityEngine.Vector2.zero, 0.0f, 0.0f, rotationDrive);
      this.playerJointActive = false;
    }
    else
    {
      if ((bool) (UnityEngine.Object) handler.gripInfo.joint)
        this.SetJointConfig(handler.gripInfo.joint, handler, UnityEngine.Vector2.zero, UnityEngine.Vector2.zero, 0.0f, 0.0f, rotationDrive);
      if ((bool) (UnityEngine.Object) handler.gripInfo.playerJoint)
      {
        if (this.ignoreClimbingForceOverride)
          this.SetJointConfig(handler.gripInfo.playerJoint, handler, positionMultiplier, rotationMultiplier, handler.creature.data.forceMaxPosition, handler.creature.data.forceMaxRotation, rotationDrive, handler.playerHand.player.creature.morphology.GetArmCenterToFingerTipLenght());
        else
          this.SetJointConfig(handler.gripInfo.playerJoint, handler, handler.creature.data.climbingForcePositionSpringDamperMult * positionMultiplier, rotationMultiplier, handler.creature.data.climbingForceMaxPosition, handler.creature.data.climbingForceMaxRotation, rotationDrive, handler.playerHand.player.creature.morphology.GetArmCenterToFingerTipLenght());
      }
      this.playerJointActive = true;
    }
  }

  public virtual void SetJointConfig(
    ConfigurableJoint joint,
    RagdollHand handler,
    UnityEngine.Vector2 positionMultiplier,
    UnityEngine.Vector2 rotationMultiplier,
    float maxPositionForce,
    float maxRotationForce,
    HandleData.RotationDrive rotationDrive,
    float limit = 0.0f)
  {
    JointDrive jointDrive1 = new JointDrive();
    JointDrive jointDrive2 = new JointDrive();
    JointDrive jointDrive3 = new JointDrive();
    jointDrive1.positionSpring = handler.creature.GetPositionJointConfig().x * this.data.positionSpringMultiplier * positionMultiplier.x * Handle.globalPositionSpringMultiplier * this.positionSpringMultiplier;
    jointDrive1.positionDamper = handler.creature.GetPositionJointConfig().y * this.data.positionDamperMultiplier * positionMultiplier.y * Handle.globalPositionDamperMultiplier * this.positionDamperMultiplier;
    jointDrive1.maximumForce = maxPositionForce * positionMultiplier.x * handler.creature.GetRotationJointConfig().z;
    joint.xDrive = jointDrive1;
    joint.yDrive = jointDrive1;
    joint.zDrive = jointDrive1;
    jointDrive2.positionSpring = handler.creature.GetRotationJointConfig().x * this.data.rotationSpringMultiplier * rotationMultiplier.x * Handle.globalRotationSpringMultiplier * this.rotationSpringMultiplier;
    jointDrive2.positionDamper = handler.creature.GetRotationJointConfig().y * this.data.rotationDamperMultiplier * rotationMultiplier.y * Handle.globalRotationDamperMultiplier * this.rotationDamperMultiplier;
    jointDrive2.maximumForce = maxRotationForce * rotationMultiplier.x * handler.creature.GetRotationJointConfig().z;
    joint.angularXDrive = rotationDrive == HandleData.RotationDrive.X || rotationDrive == HandleData.RotationDrive.WYZ || Handle.globalForceXYZ ? jointDrive2 : jointDrive3;
    joint.angularYZDrive = rotationDrive == HandleData.RotationDrive.YZ || rotationDrive == HandleData.RotationDrive.WYZ || Handle.globalForceXYZ ? jointDrive2 : jointDrive3;
    joint.slerpDrive = rotationDrive != HandleData.RotationDrive.Slerp || Handle.globalForceXYZ ? jointDrive3 : jointDrive2;
    joint.rotationDriveMode = rotationDrive != HandleData.RotationDrive.Slerp || Handle.globalForceXYZ ? RotationDriveMode.XYAndZ : RotationDriveMode.Slerp;
    if ((double) limit > 0.0)
    {
      joint.xMotion = ConfigurableJointMotion.Limited;
      joint.yMotion = ConfigurableJointMotion.Limited;
      joint.zMotion = ConfigurableJointMotion.Limited;
      SoftJointLimit softJointLimit = new SoftJointLimit()
      {
        limit = limit
      };
      joint.linearLimit = softJointLimit;
    }
    else
    {
      joint.xMotion = ConfigurableJointMotion.Free;
      joint.yMotion = ConfigurableJointMotion.Free;
      joint.zMotion = ConfigurableJointMotion.Free;
    }
    joint.angularXMotion = ConfigurableJointMotion.Free;
    joint.angularYMotion = ConfigurableJointMotion.Free;
    joint.angularZMotion = ConfigurableJointMotion.Free;
    this.lastPositionMultiplier = positionMultiplier;
    this.lastRotationMultiplier = rotationMultiplier;
  }

  protected override void OnDrawGizmosSelected()
  {
    base.OnDrawGizmosSelected();
    Gizmos.color = UnityEngine.Color.grey;
    Gizmos.DrawWireSphere(new UnityEngine.Vector3(0.0f, this.GetDefaultAxisLocalPosition(), 0.0f), this.reach);
    Gizmos.color = UnityEngine.Color.yellow;
    Gizmos.DrawWireSphere(new UnityEngine.Vector3(0.0f, this.GetDefaultAxisLocalPosition(), 0.0f), 0.03f);
  }

  public new enum HandSide
  {
    None,
    Right,
    Left,
    Both,
  }

  public enum SlideBehavior
  {
    CanSlide,
    KeepSlide,
    DisallowSlide,
  }

  [Serializable]
  public class Orientation
  {
    public UnityEngine.Vector3 rotation;
    public UnityEngine.Vector3 positionOffset;
    public Handle.HandSide allowedHand = Handle.HandSide.Both;
    public Handle.HandSide isDefault;

    public Orientation(
      UnityEngine.Vector3 position,
      UnityEngine.Vector3 rotation,
      Handle.HandSide allowedHand,
      Handle.HandSide isDefault)
    {
      this.rotation = rotation;
      this.positionOffset = position;
      this.allowedHand = allowedHand;
      this.isDefault = isDefault;
    }
  }

  public enum TwoHandedMode
  {
    Position,
    Dominant,
    AutoFront,
    AutoRear,
  }

  public delegate void GrabEvent(RagdollHand ragdollHand, Handle handle, EventTime eventTime);

  public delegate void TkEvent(Handle handle, SpellTelekinesis spellTelekinesis);

  [Serializable]
  public class JointModifier
  {
    [NonSerialized]
    public object handler;
    public float positionSpringMultiplier;
    public float positionDamperMultiplier;
    public float rotationSpringMultiplier;
    public float rotationDamperMultiplier;

    public JointModifier()
    {
    }

    public JointModifier(
      object handler,
      float positionSpringMultiplier,
      float positionDamperMultiplier,
      float rotationSpringMultiplier,
      float rotationDamperMultiplier)
    {
      this.handler = handler;
      this.positionSpringMultiplier = positionSpringMultiplier;
      this.positionDamperMultiplier = positionDamperMultiplier;
      this.rotationSpringMultiplier = rotationSpringMultiplier;
      this.rotationDamperMultiplier = rotationDamperMultiplier;
    }
  }

  public delegate void SlideEvent(
    RagdollHand ragdollHand,
    bool sliding,
    Handle handle,
    float position,
    EventTime eventTime);

  public delegate void SwitchHandleEvent(
    RagdollHand ragdollHand,
    Handle oldHandle,
    Handle newHandle,
    EventTime eventTime);

  [Serializable]
  public class GripInfo
  {
    public GameObject gameObject;
    public Transform transform;
    public Transform ikAnchor;
    public UnityEngine.Vector3 lastSlidePosition;
    public RagdollHand ragdollHand;
    public float axisPosition;
    public HandlePose orientation;
    public PhysicBody physicBody;
    public ConfigurableJoint joint;
    public ConfigurableJoint playerJoint;
    public EffectInstance effectInstance;
    public bool hasEffect;
    public bool isSliding;
    public bool hasPlayerJoint;
    public Handle.GripInfo.Type type;

    public GripInfo(Transform parent)
    {
      this.gameObject = new GameObject("GripPoint");
      this.transform = this.gameObject.transform;
      this.transform.SetParent(parent);
      this.transform.localPosition = UnityEngine.Vector3.zero;
      this.transform.localRotation = Quaternion.identity;
      this.transform.localScale = UnityEngine.Vector3.one;
    }

    public Transform SpellOrbTarget
    {
      get
      {
        if ((UnityEngine.Object) this.orientation?.spellOrbTarget != (UnityEngine.Object) null)
          return this.orientation.spellOrbTarget;
        return !((UnityEngine.Object) this.orientation?.handle.spellOrbTarget != (UnityEngine.Object) null) ? (Transform) null : this.orientation.handle.spellOrbTarget;
      }
    }

    public void Destroy()
    {
      if ((bool) (UnityEngine.Object) this.joint)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.joint);
      if ((bool) (UnityEngine.Object) this.playerJoint)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.playerJoint);
      this.hasPlayerJoint = false;
      if (this.hasEffect)
      {
        this.effectInstance.onEffectFinished += new EffectInstance.EffectFinishEvent(this.OnEffectFinishedEvent);
        this.effectInstance.End();
        this.hasEffect = false;
      }
      else
        UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }

    private void OnEffectFinishedEvent(EffectInstance effectInstance)
    {
      if (this.effectInstance != effectInstance)
        return;
      effectInstance.onEffectFinished -= new EffectInstance.EffectFinishEvent(this.OnEffectFinishedEvent);
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }

    public enum Type
    {
      None,
      PlayerJoint,
      HandJoint,
      HandSync,
      IKOnly,
    }
  }
}
