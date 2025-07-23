// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Item
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ThunderRoad.Pools;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Item.html")]
[AddComponentMenu("ThunderRoad/Items/Item")]
public class Item : ThunderEntity
{
  public static List<Item> all = new List<Item>();
  public static List<Item> allActive = new List<Item>();
  public static List<Item> allThrowed = new List<Item>();
  public static HashSet<Item> allMoving = new HashSet<Item>();
  public static List<Item> allTk = new List<Item>();
  public static List<Item> allWorldAttached = new List<Item>();
  public static List<ItemContent> potentialLostItems = new List<ItemContent>();
  [Tooltip("The Item ID of the item specified in the Catalog")]
  public string itemId;
  [Tooltip("Specifies the Holder Point of the item. This specifies the position and rotation of the item when held in a holder, such as on player hips and back. The Z axis/blue arrow specifies towards the floor.")]
  public Transform holderPoint;
  [Tooltip("Specifies the spawn point of the item. This specifies the position and rotation of the item when spawned, it's mostly used for itemSpawner and spawning the item via an item book")]
  public Transform spawnPoint;
  [Tooltip("Can add additional holderpoints for different interactables.\n \nFor Items on the Item Rack, the anchor must be named HolderRackTopAnchor, or alternatively for the bow rack, HolderRackTopAnchorBow, and HolderRackSideAnchor for Shield rack")]
  public List<Item.HolderPoint> additionalHolderPoints = new List<Item.HolderPoint>();
  [Tooltip("Shows the point that AI will try to block with when they are holding the item.")]
  public Transform parryPoint;
  [Tooltip("Specifies what handle is grabbed by default for the Right Hand")]
  public Handle mainHandleRight;
  [Tooltip("Specifies what handle is grabbed by default for the Left Hand")]
  public Handle mainHandleLeft;
  [Tooltip("Used to point in direction when thrown.\nZ-Axis/Blue Arrow points forwards.")]
  public Transform flyDirRef;
  [Tooltip("States the Preview for the item.")]
  public Preview preview;
  [Tooltip("Tick if the item is attached to the world and not spawned via item spawner or item book.")]
  public bool worldAttached;
  [Tooltip("Tick if the item should keep its parent when it loads into the scene.")]
  public bool keepParent;
  [Tooltip("Radius to depict how close this item needs to be to a creature before the creatures' collision is enabled.")]
  public float creaturePhysicToggleRadius = 2f;
  [Tooltip("Allows user to adjust the center of mass on an object.\nIf unticked, this is automatically adjusted. When ticked, adds a custom gizmo to adjust.\n \nUse this if weight on the item is acting strange.")]
  public bool useCustomCenterOfMass;
  [Tooltip("Position of Center of Mass (if ticked)")]
  public UnityEngine.Vector3 customCenterOfMass;
  [Tooltip("Used for balance adjustment on a weapon.\n \nUse this if swinging weapons are strange. Adjust the Capsule collider to the width of the weapon.")]
  public bool customInertiaTensor;
  [Tooltip("Collider of the Custom Inertia Tensor")]
  public CapsuleCollider customInertiaTensorCollider;
  [Tooltip("Allows a custom reference to be able to reference specific gameobjects and scripts in External code.")]
  public List<CustomReference> customReferences = new List<CustomReference>();
  [Tooltip("When ticked, item is automatically set as \"Thrown\" when spawned.")]
  public bool forceThrown;
  [Tooltip("Forces layer of mesh when an item is spawned.\n\n(Items will have their layer automatically applied when spawned, unless this is set)")]
  public int forceMeshLayer = -1;
  private Item.Owner _owner;
  [NonSerialized]
  public bool isUsed = true;
  [NonSerialized]
  public List<Renderer> renderers = new List<Renderer>();
  [NonSerialized]
  public List<FxController> fxControllers = new List<FxController>();
  [NonSerialized]
  public List<FxModule> fxModules = new List<FxModule>();
  [NonSerialized]
  public List<RevealDecal> revealDecals = new List<RevealDecal>();
  [NonSerialized]
  public List<ColliderGroup> colliderGroups = new List<ColliderGroup>();
  [NonSerialized]
  public List<Collider> disabledColliders = new List<Collider>();
  [NonSerialized]
  public List<HingeEffect> effectHinges = new List<HingeEffect>();
  [NonSerialized]
  public List<WhooshPoint> whooshPoints = new List<WhooshPoint>();
  [HideInInspector]
  public LightVolumeReceiver lightVolumeReceiver;
  public AudioSource audioSource;
  [NonSerialized]
  public List<CollisionHandler> collisionHandlers = new List<CollisionHandler>();
  [NonSerialized]
  public List<Handle> handles = new List<Handle>();
  [NonSerialized]
  public PhysicBody physicBody;
  [NonSerialized]
  public Breakable breakable;
  [NonSerialized]
  public List<ParryTarget> parryTargets = new List<ParryTarget>();
  [NonSerialized]
  public Holder holder;
  [NonSerialized]
  private ClothingGenderSwitcher clothingGenderSwitcher;
  [NonSerialized]
  public bool allowGrip = true;
  [NonSerialized]
  public bool hasSlash;
  [NonSerialized]
  public bool hasMetal;
  [NonSerialized]
  public List<ColliderGroup> metalColliderGroups;
  [NonSerialized]
  public FloatHandler sliceAngleMultiplier;
  public FloatHandler damageMultiplier;
  public IntAddHandler pushLevelMultiplier;
  [NonSerialized]
  public Container linkedContainer;
  [NonSerialized]
  public List<ContentCustomData> contentCustomData;
  [NonSerialized]
  public CollisionHandler mainCollisionHandler;
  [NonSerialized]
  public UnityEngine.Vector3 customInertiaTensorPos;
  [NonSerialized]
  public Quaternion customInertiaTensorRot;
  [NonSerialized]
  public bool updateReveal;
  [NonSerialized]
  public bool loaded;
  [NonSerialized]
  public bool loadedItemModules;
  [NonSerialized]
  public bool isSwaping;
  [NonSerialized]
  public bool ignoreGravityPush;
  [NonSerialized]
  public HashSet<Zone> zones = new HashSet<Zone>();
  [NonSerialized]
  public AsyncOperationHandle<GameObject> addressableHandle;
  protected HashSet<object> isNotStorableModifiers = new HashSet<object>();
  [NonSerialized]
  public SpawnableArea currentArea;
  [NonSerialized]
  public bool isCulled;
  [NonSerialized]
  public bool isHidden;
  protected bool isRegistered;
  protected bool cullingDetectionEnabled;
  protected float cullingDetectionCycleSpeed = 1f;
  protected float cullingDetectionCycleTime;
  [NonSerialized]
  public ItemData data;
  [NonSerialized]
  public ItemSpawner spawner;
  [NonSerialized]
  public List<RagdollHand> handlers = new List<RagdollHand>();
  [NonSerialized]
  public List<SpellCaster> tkHandlers = new List<SpellCaster>();
  [NonSerialized]
  public RagdollHand mainHandler;
  [NonSerialized]
  public List<Imbue> imbues = new List<Imbue>();
  [NonSerialized]
  public PlayerHand leftPlayerHand;
  [NonSerialized]
  public PlayerHand rightPlayerHand;
  [NonSerialized]
  public RagdollHand leftNpcHand;
  [NonSerialized]
  public RagdollHand rightNpcHand;
  [NonSerialized]
  public bool handlerArmGrabbed;
  [NonSerialized]
  public RagdollHand lastHandler;
  public float snapPitchRange = 0.05f;
  public List<Holder> childHolders = new List<Holder>();
  [Header("Holder")]
  public List<ItemData.CustomSnap> customSnaps = new List<ItemData.CustomSnap>();
  [Header("Fly")]
  [Tooltip("When ticked, item is automatically set as \"Thrown\" when spawned.")]
  public bool flyFromThrow;
  [Tooltip("Speed of which the item rotates when thrown")]
  public float flyRotationSpeed = 2f;
  [Tooltip("Angle offset of the z-axis arrow when thrown.")]
  public float flyThrowAngle;
  public bool isTelekinesisGrabbed;
  public bool isThrowed;
  public bool isFlying;
  public bool isFlyingBackwards;
  public bool isMoving;
  public bool wasMoving;
  public bool isGripped;
  public bool isBrokenPiece;
  [NonSerialized]
  public bool isCollidersOn = true;
  public Ragdoll ignoredRagdoll;
  public Item ignoredItem;
  public Collider ignoredCollider;
  public bool disableSnap;
  public AudioContainer audioContainerSnap;
  public AudioContainer audioContainerInventory;
  [Header("Telekinesis")]
  public float distantGrabSafeDistance = 1f;
  public bool distantGrabSpinEnabled = true;
  public float distantGrabThrowRatio = 1f;
  public static Action<Item> OnItemSpawn;
  public static Action<Item> OnItemDespawn;
  public static Action<Item, Holder> OnItemSnap;
  public static Action<Item, Holder> OnItemUnSnap;
  public static Action<Item, Handle, RagdollHand> OnItemGrab;
  public static Action<Item, Handle, RagdollHand> OnItemUngrab;
  public Action<Item.Owner, Item.Owner> onOwnerChange;
  public static Action<Item, Item.Owner, Item.Owner> onAnyOwnerChange;
  public UIInventory.ItemDelegate OnItemStored;
  public UIInventory.ItemDelegate OnItemRetrieved;
  protected Zone zone;
  public bool isPooled;
  public bool DisallowDespawn;
  public bool parryActive;
  public bool isPenetrating;
  public static string parryMagicTag = "ParryMagic";
  public float spawnTime;
  public float lastInteractionTime;
  [NonSerialized]
  public List<ItemMagnet> magnets = new List<ItemMagnet>();
  public Action<AudioContainer> OnSnapAudioLoaded;
  public Action<AudioContainer> OnInventoryAudioLoaded;
  [NonSerialized]
  public float orgSleepThreshold;
  [NonSerialized]
  public float orgMass;
  [NonSerialized]
  public float totalCombinedMass = -1f;
  private bool ignoreIsMoving;
  private Coroutine imbueDecreaseRoutine;
  [NonSerialized]
  public bool trackVelocity;
  [NonSerialized]
  public float lastUpdateTime;
  [NonSerialized]
  public UnityEngine.Vector3 lastLinearVelocity;
  [NonSerialized]
  public UnityEngine.Vector3 lastAngularVelocity;
  [NonSerialized]
  public UnityEngine.Vector3 lastPosition;
  [NonSerialized]
  public UnityEngine.Vector3 lastEulers;
  [NonSerialized]
  public UnityEngine.Vector3 spawnPosition;
  [NonSerialized]
  public Quaternion spawnRotation;
  [NonSerialized]
  private Dictionary<Transform, (UnityEngine.Vector3, Quaternion)> spawnSkinnedBonesTransforms;
  [NonSerialized]
  public LayerName forcedItemLayer;
  private HashSet<RagdollPart> penetrateNotAllowedParts = new HashSet<RagdollPart>();
  private Coroutine ignoreRagdollCollisionRoutine;
  [NonSerialized]
  public bool despawning;
  private bool despawned;
  [NonSerialized]
  public bool fellOutOfBounds;

  public List<ValueDropdownItem<string>> GetAllItemID() => Catalog.GetDropdownAllID(Category.Item);

  [Tooltip("Determines the owner of the item. This is generally set at runtime to work with the shop and buying items.")]
  public Item.Owner owner => this._owner;

  public string OwnerString
  {
    get
    {
      Item.Owner owner = this.owner;
      switch (owner)
      {
        case Item.Owner.None:
          return LocalizationManager.Instance.GetLocalizedString("Default", "UnownedItem");
        case Item.Owner.Player:
          return LocalizationManager.Instance.GetLocalizedString("Default", "OwnedItem");
        case Item.Owner.Shopkeeper:
          return LocalizationManager.Instance.GetLocalizedString("Default", "ShopItem");
        default:
          throw new SwitchExpressionException((object) owner);
      }
    }
  }

  public void SetHolderPointToCenterOfMass()
  {
    this.holderPoint.transform.position = this.transform.TransformPoint((bool) this.physicBody ? this.physicBody.centerOfMass : this.GetComponent<Rigidbody>().centerOfMass);
  }

  public bool storeBlocked => this.isNotStorableModifiers.Count > 0;

  public bool HasFlag(ItemFlags flag) => this.data.HasFlag(flag);

  public bool HasCustomData<T>() where T : ContentCustomData
  {
    if (this.contentCustomData == null)
      return false;
    int count = this.contentCustomData.Count;
    for (int index = 0; index < count; ++index)
    {
      ContentCustomData contentCustomData = this.contentCustomData[index];
      if (contentCustomData != null && contentCustomData is T)
        return true;
    }
    return false;
  }

  public bool TryGetCustomData<T>(out T customData) where T : ContentCustomData
  {
    if (this.contentCustomData == null)
    {
      customData = default (T);
      return false;
    }
    int count = this.contentCustomData.Count;
    for (int index = 0; index < count; ++index)
    {
      ContentCustomData contentCustomData = this.contentCustomData[index];
      if (contentCustomData != null && contentCustomData is T)
      {
        customData = contentCustomData as T;
        return true;
      }
    }
    customData = default (T);
    return false;
  }

  public void AddCustomData<T>(T customData) where T : ContentCustomData
  {
    if (this.HasCustomData<T>())
      return;
    if (this.contentCustomData == null)
      this.contentCustomData = new List<ContentCustomData>();
    this.contentCustomData.Add((ContentCustomData) customData);
  }

  /// <summary>
  /// Replace current content custom data with the given list.
  /// </summary>
  /// <param name="newContentCustomData">Content list to replace contentCustomData with.</param>
  public void OverrideCustomData(List<ContentCustomData> newContentCustomData)
  {
    this.contentCustomData = newContentCustomData;
    Item.OverrideContentCustomDataEvent contentCustomDataEvent = this.OnOverrideContentCustomDataEvent;
    if (contentCustomDataEvent == null)
      return;
    contentCustomDataEvent(this.contentCustomData);
  }

  /// <summary>
  /// Removes all occurrences of the ContentCustomData with the type T
  /// </summary>
  /// <typeparam name="T">Type of the ContentCustomData to remove</typeparam>
  public void RemoveCustomData<T>() where T : ContentCustomData
  {
    if (this.contentCustomData == null)
      return;
    for (int index = this.contentCustomData.Count - 1; index >= 0; --index)
    {
      ContentCustomData contentCustomData = this.contentCustomData[index];
      if (contentCustomData is T)
        this.contentCustomData.Remove(contentCustomData);
    }
  }

  public void AddNonStorableModifier(object handler)
  {
    if (this.isNotStorableModifiers.Contains(handler))
      return;
    this.isNotStorableModifiers.Add(handler);
  }

  public void RemoveNonStorableModifier(object handler)
  {
    if (!this.isNotStorableModifiers.Contains(handler))
      return;
    this.isNotStorableModifiers.Remove(handler);
  }

  public void AddNonStorableModifierInvokable(UnityEngine.Object handler)
  {
    this.AddNonStorableModifier((object) handler);
  }

  public void RemoveNonStorableModifierInvokable(UnityEngine.Object handler)
  {
    this.RemoveNonStorableModifier((object) handler);
  }

  public void ClearNonStorableModifiers() => this.isNotStorableModifiers.Clear();

  protected virtual void OnValidate()
  {
    if (Application.isPlaying || !this.gameObject.activeInHierarchy)
      return;
    this.SetupDefaultComponents();
  }

  public void SetupDefaultComponents()
  {
    if (this.physicBody == (PhysicBody) null && (UnityEngine.Object) this.GetComponent<Rigidbody>() == (UnityEngine.Object) null && (UnityEngine.Object) this.GetComponent<ArticulationBody>() == (UnityEngine.Object) null)
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding Rigidbody to " + this.itemId));
      this.gameObject.AddComponent<Rigidbody>();
    }
    if (!(bool) (UnityEngine.Object) this.holderPoint)
      this.holderPoint = this.transform.Find("HolderPoint");
    if (!(bool) (UnityEngine.Object) this.holderPoint)
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding HolderPoint to " + this.itemId));
      this.holderPoint = new GameObject("HolderPoint").transform;
      this.holderPoint.SetParent(this.transform, false);
    }
    if (!(bool) (UnityEngine.Object) this.parryPoint)
      this.parryPoint = this.transform.Find("ParryPoint");
    if (!(bool) (UnityEngine.Object) this.parryPoint)
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding ParryPoint to " + this.itemId));
      this.parryPoint = new GameObject("ParryPoint").transform;
      this.parryPoint.SetParent(this.transform, false);
    }
    if (!(bool) (UnityEngine.Object) this.spawnPoint)
      this.spawnPoint = this.transform.Find("SpawnPoint");
    if (!(bool) (UnityEngine.Object) this.spawnPoint)
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding SpawnPoint to " + this.itemId));
      this.spawnPoint = new GameObject("SpawnPoint").transform;
      this.spawnPoint.SetParent(this.transform, false);
    }
    this.preview = this.GetComponentInChildren<Preview>();
    if (!(bool) (UnityEngine.Object) this.preview && (bool) (UnityEngine.Object) this.transform.Find("Preview"))
      this.preview = this.transform.Find("Preview").gameObject.AddComponent<Preview>();
    if (!(bool) (UnityEngine.Object) this.preview)
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding Preview to " + this.itemId));
      this.preview = new GameObject("Preview").AddComponent<Preview>();
      this.preview.transform.SetParent(this.transform, false);
    }
    Transform transform = this.transform.Find("Whoosh");
    if ((bool) (UnityEngine.Object) transform && !(bool) (UnityEngine.Object) transform.GetComponent<WhooshPoint>())
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding WhooshPoint to " + this.itemId));
      transform.gameObject.AddComponent<WhooshPoint>();
    }
    if (!(bool) (UnityEngine.Object) this.mainHandleRight)
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding Handle Right to " + this.itemId));
      foreach (Handle componentsInChild in this.GetComponentsInChildren<Handle>())
      {
        if (componentsInChild.IsAllowed(Side.Right))
        {
          this.mainHandleRight = componentsInChild;
          break;
        }
      }
    }
    if (!(bool) (UnityEngine.Object) this.mainHandleLeft)
    {
      Debug.LogWarning((object) ("[DefaultComponents] Adding Handle Right to " + this.itemId));
      foreach (Handle componentsInChild in this.GetComponentsInChildren<Handle>())
      {
        if (componentsInChild.IsAllowed(Side.Left))
        {
          this.mainHandleLeft = componentsInChild;
          break;
        }
      }
    }
    if (!(bool) (UnityEngine.Object) this.mainHandleRight)
      this.mainHandleRight = this.GetComponentInChildren<Handle>();
    this.physicBody = this.gameObject.GetPhysicBody();
    if (this.useCustomCenterOfMass)
      this.physicBody.centerOfMass = this.customCenterOfMass;
    else
      this.physicBody.ResetCenterOfMass();
    if (this.customInertiaTensor)
    {
      if ((UnityEngine.Object) this.customInertiaTensorCollider == (UnityEngine.Object) null)
      {
        this.customInertiaTensorCollider = new GameObject("InertiaTensorCollider").AddComponent<CapsuleCollider>();
        this.customInertiaTensorCollider.transform.SetParent(this.transform, false);
        this.customInertiaTensorCollider.radius = 0.05f;
        this.customInertiaTensorCollider.direction = 2;
      }
      this.customInertiaTensorCollider.enabled = false;
      this.customInertiaTensorCollider.isTrigger = true;
      this.customInertiaTensorCollider.gameObject.layer = 2;
    }
    if (!this.TryGetComponent<LightVolumeReceiver>(out this.lightVolumeReceiver))
      this.lightVolumeReceiver = this.gameObject.AddComponent<LightVolumeReceiver>();
    if ((bool) (UnityEngine.Object) this.audioSource)
      return;
    this.audioSource = this.gameObject.AddComponent<AudioSource>();
  }

  public Bounds GetWorldBounds()
  {
    Bounds worldBounds = new Bounds(this.transform.position, UnityEngine.Vector3.zero);
    for (int index = 0; index < this.renderers.Count; ++index)
      worldBounds.Encapsulate(this.renderers[index].bounds);
    return worldBounds;
  }

  public Bounds GetLocalBounds()
  {
    Bounds bounds = new Bounds(UnityEngine.Vector3.zero, UnityEngine.Vector3.zero);
    RecurseEncapsulate(this.transform, ref bounds);
    return bounds;

    void RecurseEncapsulate(Transform child, ref Bounds bounds)
    {
      Transform transform = child.transform;
      MeshFilter component1;
      if (child.TryGetComponent<MeshFilter>(out component1) && (UnityEngine.Object) component1?.sharedMesh != (UnityEngine.Object) null)
      {
        Bounds bounds1 = component1.sharedMesh.bounds;
        UnityEngine.Vector3 position1 = child.TransformPoint(bounds1.center - bounds1.extents);
        UnityEngine.Vector3 position2 = child.TransformPoint(bounds1.center + bounds1.extents);
        bounds.Encapsulate(this.transform.InverseTransformPoint(position1));
        bounds.Encapsulate(this.transform.InverseTransformPoint(position2));
      }
      else
      {
        SkinnedMeshRenderer component2;
        if (child.TryGetComponent<SkinnedMeshRenderer>(out component2))
        {
          Bounds localBounds = component2.localBounds;
          UnityEngine.Vector3 position3 = child.TransformPoint(localBounds.center - localBounds.extents);
          UnityEngine.Vector3 position4 = child.TransformPoint(localBounds.center + localBounds.extents);
          bounds.Encapsulate(this.transform.InverseTransformPoint(position3));
          bounds.Encapsulate(this.transform.InverseTransformPoint(position4));
        }
        else
          bounds.Encapsulate(this.transform.InverseTransformPoint(transform.position));
      }
      for (int index = 0; index < transform.childCount; ++index)
        RecurseEncapsulate(transform.GetChild(index), ref bounds);
    }
  }

  public UnityEngine.Vector3 GetLocalCenter()
  {
    ItemData data = this.data;
    int preferredItemCenter = data != null ? (int) data.preferredItemCenter : 0;
    if (preferredItemCenter == 0)
    {
      PhysicBody physicBody = this.physicBody;
      if (((object) physicBody != null ? (physicBody.IsSleeping() ? 1 : 0) : 0) != 0)
        this.physicBody.WakeUp();
    }
    UnityEngine.Vector3 localCenter;
    switch (preferredItemCenter)
    {
      case 0:
        if (this.physicBody != (PhysicBody) null)
        {
          localCenter = this.physicBody.centerOfMass;
          break;
        }
        goto default;
      case 1:
        localCenter = UnityEngine.Vector3.zero;
        break;
      case 2:
        localCenter = this.GetLocalBounds().center;
        break;
      case 3:
        if ((UnityEngine.Object) (this.mainHandleLeft ?? this.mainHandleRight) != (UnityEngine.Object) null)
        {
          localCenter = this.transform.InverseTransformPoint((this.mainHandleLeft ?? this.mainHandleRight).transform.position);
          break;
        }
        goto default;
      case 4:
        if ((UnityEngine.Object) this.holderPoint != (UnityEngine.Object) null)
        {
          localCenter = this.transform.InverseTransformPoint(this.holderPoint.transform.position);
          break;
        }
        goto default;
      default:
        localCenter = UnityEngine.Vector3.zero;
        break;
    }
    return localCenter;
  }

  public void Haptic(float intensity, bool oneFrameCooldown = false)
  {
    int count = this.handlers.Count;
    for (int index = 0; index < count; ++index)
      this.handlers[index].playerHand?.controlHand.HapticShort(intensity, oneFrameCooldown);
  }

  public void HapticClip(PcmData pcmData, GameData.HapticClip fallbackClip)
  {
    int count = this.handlers.Count;
    for (int index = 0; index < count; ++index)
      this.handlers[index].playerHand?.controlHand.Haptic(pcmData, fallbackClip);
  }

  public bool TryGetCustomReference<T>(string name, out T custom) where T : Component
  {
    custom = this.GetCustomReference<T>(name, false);
    return (UnityEngine.Object) custom != (UnityEngine.Object) null;
  }

  public T GetCustomReference<T>(string name, bool printError = true) where T : Component
  {
    CustomReference customReference = this.customReferences.Find((Predicate<CustomReference>) (cr => cr.name == name));
    if (customReference != null)
    {
      if (customReference.transform is T)
        return (T) customReference.transform;
      return typeof (T) == typeof (Transform) ? customReference.transform.transform as T : customReference.transform.GetComponent<T>();
    }
    if (printError)
      Debug.LogError((object) $"[{this.itemId}] Cannot find item custom reference {name}");
    return default (T);
  }

  public Transform GetCustomReference(string name, bool printError = true)
  {
    return this.GetCustomReference<Transform>(name, printError);
  }

  protected virtual void Awake()
  {
    Item.all.Add(this);
    this.damageMultiplier = new FloatHandler();
    this.sliceAngleMultiplier = new FloatHandler();
    this.pushLevelMultiplier = new IntAddHandler();
    Transform transform = this.transform;
    Breakable component;
    if (this.TryGetComponent<Breakable>(out component))
      transform = component.unbrokenObjectsHolder.transform;
    this.clothingGenderSwitcher = transform.GetComponentInChildren<ClothingGenderSwitcher>();
    this.clothingGenderSwitcher?.SetModelActive(true);
    transform.GetComponentsInChildren<Renderer>(this.renderers);
    int count1 = this.renderers.Count;
    for (int index = 0; index < count1; ++index)
    {
      Renderer renderer = this.renderers[index];
      if (renderer.enabled)
      {
        switch (renderer)
        {
          case SkinnedMeshRenderer _:
          case MeshRenderer _:
            continue;
        }
      }
      this.renderers.RemoveAtIgnoreOrder<Renderer>(index);
      --index;
      --count1;
    }
    transform.GetComponentsInChildren<FxController>(this.fxControllers);
    int count2 = this.fxControllers.Count;
    for (int index = 0; index < count2; ++index)
    {
      FxController fxController = this.fxControllers[index];
      if (!fxController.gameObject.activeInHierarchy || !fxController.enabled)
      {
        this.fxControllers.RemoveAtIgnoreOrder<FxController>(index);
        --index;
        --count2;
      }
    }
    transform.GetComponentsInChildren<FxModule>(this.fxModules);
    int count3 = this.fxModules.Count;
    for (int index = 0; index < count3; ++index)
    {
      FxModule fxModule = this.fxModules[index];
      if (!fxModule.gameObject.activeInHierarchy || !fxModule.enabled || (UnityEngine.Object) fxModule.GetComponentInParent<FxController>() != (UnityEngine.Object) null)
      {
        this.fxModules.RemoveAtIgnoreOrder<FxModule>(index);
        --index;
        --count3;
      }
    }
    this.waterHandler = new WaterHandler(true, false);
    this.waterHandler.OnWaterEnter += new WaterHandler.SimpleDelegate(this.OnWaterEnter);
    this.waterHandler.OnWaterExit += new WaterHandler.SimpleDelegate(this.OnWaterExit);
    if (!this.TryGetComponent<LightVolumeReceiver>(out this.lightVolumeReceiver))
      this.lightVolumeReceiver = this.gameObject.AddComponent<LightVolumeReceiver>();
    this.lightVolumeReceiver.initRenderersOnStart = false;
    this.lightVolumeReceiver.volumeDetection = LightVolumeReceiver.VolumeDetection.DynamicTrigger;
    this.lightVolumeReceiver.SetRenderers(this.renderers);
    transform.GetComponentsInChildren<RevealDecal>(this.revealDecals);
    transform.GetComponentsInChildren<WhooshPoint>(this.whooshPoints);
    transform.GetComponentsInChildren<HingeEffect>(this.effectHinges);
    transform.GetComponentsInChildren<ParryTarget>(this.parryTargets);
    transform.GetComponentsInChildren<Handle>(this.handles);
    transform.GetComponentsInChildren<Holder>(true, this.childHolders);
    this.SetPhysicBodyAndMainCollisionHandler();
    if ((bool) (UnityEngine.Object) this.customInertiaTensorCollider)
      this.CalculateCustomInertiaTensor();
    this.imbues = new List<Imbue>();
    if (!(bool) (UnityEngine.Object) this.audioSource)
      this.audioSource = this.gameObject.AddComponent<AudioSource>();
    this.audioSource.spatialBlend = 1f;
    this.audioSource.dopplerLevel = 0.0f;
    this.audioSource.playOnAwake = false;
    this.audioSource.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Effect);
    if (AudioSettings.GetSpatializerPluginName() != null)
      this.audioSource.spatialize = true;
    this.orgSleepThreshold = this.physicBody.sleepThreshold;
    this.handlers = new List<RagdollHand>();
    if (this.worldAttached)
      Item.allWorldAttached.Add(this);
    this.clothingGenderSwitcher?.Refresh();
  }

  public void SetPhysicBodyAndMainCollisionHandler()
  {
    if (this.physicBody != (PhysicBody) null)
      return;
    this.physicBody = this.gameObject.GetPhysicBody();
    if (this.useCustomCenterOfMass)
      this.physicBody.centerOfMass = this.customCenterOfMass;
    Transform transform = this.transform;
    this.metalColliderGroups = new List<ColliderGroup>();
    transform.GetComponentsInChildren<ColliderGroup>(this.colliderGroups);
    transform.GetComponentsInChildren<CollisionHandler>(this.collisionHandlers);
    if (this.collisionHandlers.Count == 0)
    {
      CollisionHandler collisionHandler = this.gameObject.AddComponent<CollisionHandler>();
      this.collisionHandlers.Add(collisionHandler);
      int count = this.colliderGroups.Count;
      for (int index = 0; index < count; ++index)
      {
        ColliderGroup colliderGroup = this.colliderGroups[index];
        if (colliderGroup.isMetal)
          this.hasMetal = true;
        this.metalColliderGroups.Add(colliderGroup);
        colliderGroup.collisionHandler = collisionHandler;
      }
    }
    if (!(bool) (UnityEngine.Object) GameManager.local)
      return;
    int count1 = this.collisionHandlers.Count;
    for (int index = 0; index < count1; ++index)
    {
      CollisionHandler collisionHandler = this.collisionHandlers[index];
      collisionHandler.OnCollisionStartEvent += new CollisionHandler.CollisionEvent(this.OnCollisionStartEvent);
      if (!(bool) collisionHandler.physicBody)
        collisionHandler.physicBody = collisionHandler.gameObject.GetPhysicBody();
      if (collisionHandler.physicBody == this.physicBody)
      {
        this.mainCollisionHandler = collisionHandler;
        break;
      }
    }
  }

  protected override void ManagedOnEnable()
  {
    base.ManagedOnEnable();
    Item.allActive.Add(this);
    if (!((UnityEngine.Object) this.holder == (UnityEngine.Object) null) || !((UnityEngine.Object) AreaManager.Instance != (UnityEngine.Object) null) || AreaManager.Instance.CurrentArea == null)
      return;
    this.cullingDetectionEnabled = true;
    this.cullingDetectionCycleTime = Time.time;
    this.CheckCurrentArea();
  }

  protected override void ManagedOnDisable()
  {
    base.ManagedOnDisable();
    Item.allActive.Remove(this);
    this.cullingDetectionEnabled = false;
    this.waterHandler.Reset();
    this.ClearZones();
  }

  private void OnWaterEnter()
  {
  }

  private void UpdateWater()
  {
    if (this.loaded && Water.exist && !this.physicBody.isKinematic && (this.isMoving || this.isFlying || this.isThrowed || this.IsHanded() || this.physicBody.HasMeaningfulVelocity()))
    {
      float radius = this.data.waterSampleMinRadius;
      UnityEngine.Vector3 vector3_1 = UnityEngine.Vector3.zero;
      UnityEngine.Vector3 vector = UnityEngine.Vector3.zero;
      int count1 = this.parryTargets.Count;
      UnityEngine.Vector3 velocity;
      if (count1 > 0)
      {
        vector3_1.y = float.PositiveInfinity;
        for (int index = 0; index < count1; ++index)
        {
          ParryTarget parryTarget = this.parryTargets[index];
          Transform transform = parryTarget.transform;
          UnityEngine.Vector3 position = transform.position;
          UnityEngine.Vector3 vector3_2 = transform.up * parryTarget.length;
          UnityEngine.Vector3 vector3_3 = position + vector3_2;
          UnityEngine.Vector3 vector3_4 = position - vector3_2;
          if ((double) vector3_4.y < (double) vector3_1.y)
          {
            vector3_1 = vector3_4;
            vector = vector3_3;
          }
          if ((double) vector3_3.y < (double) vector3_1.y)
          {
            vector3_1 = vector3_3;
            vector = vector3_4;
          }
        }
        UnityEngine.Vector3 lastPointVelocity = this.collisionHandlers[0].CalculateLastPointVelocity(vector3_1);
        velocity = lastPointVelocity;
        UnityEngine.Vector3 normalized = lastPointVelocity.normalized;
        radius = Mathf.Max(UnityEngine.Vector3.Distance(UnityEngine.Vector3.ProjectOnPlane(vector3_1, normalized), UnityEngine.Vector3.ProjectOnPlane(vector, normalized)) * 0.25f, this.data.waterSampleMinRadius);
      }
      else
      {
        UnityEngine.Vector3 position = this.holderPoint.position;
        vector3_1.x = position.x;
        vector3_1.y = position.y + radius;
        vector3_1.z = position.z;
        vector.x = position.x;
        vector.y = position.y - radius;
        vector.z = position.z;
        velocity = this.physicBody.velocity;
      }
      this.waterHandler.Update(vector3_1, vector3_1.y, vector.y, radius, velocity);
      if (this.waterHandler.inWater)
      {
        float drag = this.data.waterDragMultiplierCurve.Evaluate(this.waterHandler.submergedRatio);
        this.SetPhysicModifier((object) this, new float?(Mathf.Lerp(1f, Catalog.gameData.water.minGravityItem, this.waterHandler.submergedRatio)), 1f, drag, drag * 0.1f, -1f, (EffectData) null);
        int count2 = this.handlers.Count;
        for (int index = 0; index < count2; ++index)
        {
          RagdollHand handler = this.handlers[index];
          if (handler.creature.isPlayer)
          {
            float num = this.data.waterHandSpringMultiplierCurve.Evaluate(this.waterHandler.submergedRatio);
            handler.grabbedHandle.SetJointModifier((object) this, num, rotationSpringMultiplier: num);
          }
        }
      }
    }
    if (this.waterHandler.inWater)
    {
      int count = this.colliderGroups.Count;
      float num = Catalog.gameData.water.imbueDepletionRate * Time.deltaTime;
      for (int index = 0; index < count; ++index)
      {
        ColliderGroup colliderGroup = this.colliderGroups[index];
        if ((double) colliderGroup.transform.position.y < (double) this.waterHandler.waterSurfacePosition.y)
        {
          Imbue imbue = colliderGroup.imbue;
          if ((bool) (UnityEngine.Object) imbue && imbue.spellCastBase != null && (double) imbue.energy > 0.0)
            imbue.ConsumeInstant(num * colliderGroup.data.GetModifier(colliderGroup).waterLossRateMultiplier);
        }
      }
    }
    if (!this.waterHandler.inWater || Water.exist && this.loaded)
      return;
    this.waterHandler.Reset();
  }

  private void OnWaterExit()
  {
    int count = this.handlers.Count;
    for (int index = 0; index < count; ++index)
    {
      RagdollHand handler = this.handlers[index];
      if (handler.creature.isPlayer)
        handler.grabbedHandle.RemoveJointModifier((object) this);
    }
    this.RemovePhysicModifier((object) this);
  }

  public void SwapWith(string itemID) => this.SwapWith(itemID, true, (Action<Item, Item>) null);

  public void ForceUngrabAll()
  {
    for (int index = this.handlers.Count - 1; index >= 0; --index)
      this.handlers[index].UnGrab(false);
  }

  public (string valueType, float value) GetValue(bool skipOverride = false)
  {
    ContentCustomDataValueOverride customData;
    return !skipOverride && this.TryGetCustomData<ContentCustomDataValueOverride>(out customData) ? (customData.valueType.IsNullOrEmptyOrWhitespace() ? this.data.valueType : customData.valueType, customData.value) : (this.data.valueType, this.data.value);
  }

  public void SetValue(float value)
  {
    ContentCustomDataValueOverride customData;
    if (this.TryGetCustomData<ContentCustomDataValueOverride>(out customData))
      customData.value = value;
    else
      this.AddCustomData<ContentCustomDataValueOverride>(new ContentCustomDataValueOverride(value));
  }

  public void SetOwner(Item.Owner owner)
  {
    if (this._owner == owner || this.data.allowedStorage <= (ItemData.Storage) 0)
      return;
    Item.Owner owner1 = this._owner;
    this._owner = owner;
    Action<Item.Owner, Item.Owner> onOwnerChange = this.onOwnerChange;
    if (onOwnerChange != null)
      onOwnerChange(owner1, this._owner);
    Action<Item, Item.Owner, Item.Owner> onAnyOwnerChange = Item.onAnyOwnerChange;
    if (onAnyOwnerChange == null)
      return;
    onAnyOwnerChange(this, owner1, this._owner);
  }

  public void ClearValueOverride() => this.RemoveCustomData<ContentCustomDataValueOverride>();

  public void SwapWith(string itemID, bool transferCustomData, Action<Item, Item> callback)
  {
    if (this.isSwaping)
      return;
    ItemData itemData = Catalog.GetData<ItemData>(itemID);
    if (itemData == null)
      return;
    this.isSwaping = true;
    itemData.SpawnAsync((Action<Item>) (newItem =>
    {
      this.isSwaping = false;
      if ((UnityEngine.Object) newItem == (UnityEngine.Object) null)
      {
        Debug.LogWarning((object) "New item is null, can't swap.");
      }
      else
      {
        newItem.transform.position = this.transform.position;
        newItem.transform.rotation = this.transform.rotation;
        if (itemData.allowedStorage > (ItemData.Storage) 0)
          newItem.SetOwner(this.owner);
        if ((bool) (UnityEngine.Object) this.holder)
        {
          Holder holder = this.holder;
          holder.UnSnap(this);
          holder.Snap(newItem);
        }
        else if (this.handlers.Count > 0 && newItem.handles.Count > 0)
        {
          for (int index = this.handlers.Count - 1; index >= 0; --index)
          {
            RagdollHand handler = this.handlers[index];
            float axisPosition = handler.gripInfo.axisPosition;
            Handle handle = newItem.handles.ElementAtOrDefault<Handle>(this.handles.IndexOf(handler.grabbedHandle));
            if ((UnityEngine.Object) handle == (UnityEngine.Object) null || !handle.IsAllowed(handler.side))
            {
              Debug.LogWarning((object) "Item swap could not find same handle index on new item");
              handle = newItem.GetMainHandle(handler.side);
            }
            HandlePose orientation = handle.orientations.ElementAtOrDefault<HandlePose>(handler.grabbedHandle.orientations.IndexOf(handler.gripInfo.orientation));
            if ((UnityEngine.Object) orientation == (UnityEngine.Object) null || orientation.side != handler.side)
            {
              Debug.LogWarning((object) "Item swap could not find same hand pose index on new item");
              orientation = handle.GetDefaultOrientation(handler.side);
            }
            handler.TryRelease();
            handler.Grab(handle, orientation, axisPosition, true);
          }
        }
        List<Damager> piercedDamagers = new List<Damager>();
        for (int index = 0; index < this.collisionHandlers.Count; ++index)
        {
          if (this.collisionHandlers[index].penetratedObjects.Count > 0)
            this.collisionHandlers[index].RemoveAllPenetratedObjects(out piercedDamagers);
        }
        if (piercedDamagers.Count > 0)
        {
          for (int index = 0; index < piercedDamagers.Count; ++index)
          {
            PhysicBody physicBody = piercedDamagers[index].collisionHandler.item?.physicBody;
            if ((object) physicBody != null)
              newItem.gameObject.AddComponent<FixedJoint>().SetConnectedPhysicBody(physicBody);
          }
          newItem.StartCoroutine(TryPierceAgain());
        }
        Action<Item, Item> action = callback;
        if (action != null)
          action(this, newItem);
        this.Despawn();

        IEnumerator TryPierceAgain()
        {
          yield return (object) Yielders.EndOfFrame;
          foreach (UnityEngine.Object component in newItem.GetComponents<FixedJoint>())
            UnityEngine.Object.Destroy(component);
          List<(Transform, PhysicBody)> bodyTransforms = new List<(Transform, PhysicBody)>()
          {
            (newItem.physicBody.transform, newItem.physicBody)
          };
          for (int index = 0; index < piercedDamagers.Count; ++index)
          {
            if (piercedDamagers[index].type == Damager.Type.Pierce)
              piercedDamagers[index].TryPierceItems(bodyTransforms);
          }
        }
      }
    }), pooled: this.isPooled, customDataList: transferCustomData ? this.contentCustomData : (List<ContentCustomData>) null);
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update | ManagedLoops.LateUpdate;

  protected internal override void ManagedLateUpdate()
  {
    if (!this.cullingDetectionEnabled || (UnityEngine.Object) this.holder != (UnityEngine.Object) null || (UnityEngine.Object) AreaManager.Instance == (UnityEngine.Object) null || AreaManager.Instance.CurrentArea == null)
      return;
    if (this.currentArea == null)
      this.CheckCurrentArea();
    if (this.currentArea != null && !this.isRegistered && this.currentArea.IsSpawned)
    {
      this.currentArea.SpawnedArea.RegisterItem(this);
      this.isRegistered = true;
    }
    if ((double) Time.time - (double) this.cullingDetectionCycleTime < (double) this.cullingDetectionCycleSpeed)
      return;
    this.cullingDetectionCycleTime = Time.time;
    this.CheckCurrentArea();
  }

  public void CheckCurrentArea()
  {
    if ((UnityEngine.Object) this.holder != (UnityEngine.Object) null || (UnityEngine.Object) AreaManager.Instance == (UnityEngine.Object) null || AreaManager.Instance.CurrentArea == null)
      return;
    if (this.currentArea == null)
    {
      SpawnableArea recursive = AreaManager.Instance.CurrentArea.FindRecursive(this.transform.position);
      if (recursive == null)
        return;
      this.currentArea = recursive;
      if (this.currentArea.IsSpawned)
      {
        this.currentArea.SpawnedArea.RegisterItem(this);
        this.isRegistered = true;
      }
      else
        this.isRegistered = false;
    }
    else
    {
      SpawnableArea recursive = this.currentArea.FindRecursive(this.transform.position);
      if (recursive == null)
      {
        if (this.currentArea.IsSpawned)
          this.currentArea.SpawnedArea.UnRegisterItem(this);
        this.isRegistered = false;
        this.currentArea = recursive;
      }
      else
      {
        if (this.currentArea == recursive)
          return;
        if (this.currentArea.IsSpawned)
          this.currentArea.SpawnedArea.UnRegisterItem(this);
        this.isRegistered = false;
        this.currentArea = recursive;
        if (!this.currentArea.IsSpawned)
          return;
        this.currentArea.SpawnedArea.RegisterItem(this);
        this.isRegistered = true;
      }
    }
  }

  public void UnRegisterArea()
  {
    if (this.currentArea == null || !this.currentArea.IsSpawned)
      return;
    this.currentArea.SpawnedArea.UnRegisterItem(this);
    this.isRegistered = false;
  }

  public void SetCull(bool cull, bool checkChange = true)
  {
    if (checkChange && this.isCulled == cull)
      return;
    this.isCulled = cull;
    if (!this.loaded)
      return;
    if (!(bool) (UnityEngine.Object) Level.master || !this.IsHanded())
      this.gameObject.SetActive(!cull);
    if (this.OnCullEvent == null)
      return;
    this.OnCullEvent(this.isCulled);
  }

  [ContextMenu("Convert to spawner")]
  public void CreateItemSpawnerFromItem()
  {
    if ((UnityEngine.Object) this.GetComponentInChildren<RopeSimple>() != (UnityEngine.Object) null)
      return;
    GameObject gameObject1 = this.gameObject;
    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(new GameObject("itemSpawnerRoot"), this.holderPoint.position, this.holderPoint.rotation, gameObject1.transform.parent);
    ItemSpawner itemSpawner = gameObject2.AddComponent<ItemSpawner>();
    itemSpawner.name = this.itemId + "_Spawner";
    itemSpawner.itemId = this.itemId;
    itemSpawner.spawnOnStart = false;
    try
    {
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) gameObject1);
    }
    catch (Exception ex)
    {
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) gameObject2);
    }
  }

  protected override void Start()
  {
    base.Start();
    this.CheckDestroyedMeshRenderers();
    if ((bool) (UnityEngine.Object) GameManager.local && (UnityEngine.Object) this.transform.root != (UnityEngine.Object) this.transform && !(bool) (UnityEngine.Object) this.holder && !this.keepParent)
      this.transform.SetParent((Transform) null, true);
    if (!(bool) (UnityEngine.Object) GameManager.local)
    {
      this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.DroppedItem));
    }
    else
    {
      ItemData outputData = this.data;
      if (outputData == null && !string.IsNullOrEmpty(this.itemId) && this.itemId != "None")
      {
        if (Catalog.TryGetData<ItemData>(this.itemId, out outputData))
          this.Load(outputData);
        else
          Debug.LogError((object) $"Unable to load itemData {this.itemId} for item {this.name}");
      }
      if (this.data != null && !GameManager.CheckContentActive(this.data.sensitiveContent, this.data.sensitiveFilterBehaviour))
        this.Despawn();
      this.CacheSpawnTransformation();
    }
  }

  /// <summary>
  /// Cache the position and rotation of the item (implicitly during spawn)
  /// Does the same for child skinned meshes
  /// </summary>
  public void CacheSpawnTransformation()
  {
    Transform transform = this.transform;
    this.spawnPosition = transform.position;
    this.spawnRotation = transform.rotation;
    foreach (SkinnedMeshRenderer componentsInChild in this.GetComponentsInChildren<SkinnedMeshRenderer>())
    {
      if (this.spawnSkinnedBonesTransforms == null)
        this.spawnSkinnedBonesTransforms = new Dictionary<Transform, (UnityEngine.Vector3, Quaternion)>();
      for (int index = 0; index < componentsInChild.bones.Length; ++index)
      {
        Transform bone = componentsInChild.bones[index];
        if ((bool) (UnityEngine.Object) bone)
        {
          if (this.spawnSkinnedBonesTransforms.ContainsKey(bone))
            this.spawnSkinnedBonesTransforms[bone] = (bone.position, bone.rotation);
          else
            this.spawnSkinnedBonesTransforms.Add(bone, (bone.position, bone.rotation));
        }
      }
    }
  }

  public void CheckDestroyedMeshRenderers()
  {
    bool flag = false;
    for (int index = this.renderers.Count - 1; index >= 0; --index)
    {
      if (!(bool) (UnityEngine.Object) this.renderers[index])
      {
        this.renderers.RemoveAt(index);
        flag = true;
      }
    }
    if (!flag)
      return;
    this.lightVolumeReceiver.SetRenderers(this.renderers);
  }

  public void Hide(bool hide)
  {
    this.isHidden = hide;
    if (this.renderers != null)
    {
      int count = this.renderers.Count;
      for (int index = 0; index < count; ++index)
      {
        Renderer renderer = this.renderers[index];
        if ((UnityEngine.Object) renderer != (UnityEngine.Object) null)
          renderer.enabled = !hide;
      }
    }
    if (this.fxControllers != null)
    {
      int count = this.fxControllers.Count;
      for (int index = 0; index < count; ++index)
      {
        FxController fxController = this.fxControllers[index];
        if ((UnityEngine.Object) fxController != (UnityEngine.Object) null)
          fxController.enabled = !hide;
      }
    }
    if (this.fxModules != null)
    {
      int count = this.fxModules.Count;
      for (int index = 0; index < count; ++index)
      {
        FxModule fxModule = this.fxModules[index];
        if ((UnityEngine.Object) fxModule != (UnityEngine.Object) null)
          fxModule.enabled = !hide;
      }
    }
    if (this.childHolders == null)
      return;
    int count1 = this.childHolders.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      Holder childHolder = this.childHolders[index1];
      if ((UnityEngine.Object) childHolder != (UnityEngine.Object) null && childHolder.items != null)
      {
        int count2 = childHolder.items.Count;
        for (int index2 = 0; index2 < count2; ++index2)
        {
          Item obj = childHolder.items[index2];
          if ((UnityEngine.Object) obj != (UnityEngine.Object) null)
            obj.Hide(hide);
        }
      }
    }
  }

  public void SetCustomInertiaTensor()
  {
    if (this.customInertiaTensorPos == UnityEngine.Vector3.zero)
      this.CalculateCustomInertiaTensor();
    if (!(bool) this.physicBody)
      this.physicBody = this.gameObject.GetPhysicBody();
    this.physicBody.inertiaTensor = this.customInertiaTensorPos;
    this.physicBody.inertiaTensorRotation = this.customInertiaTensorRot;
  }

  public virtual void ResetInertiaTensor()
  {
    if (!(bool) this.physicBody)
      this.physicBody = this.gameObject.GetPhysicBody();
    this.physicBody.ResetInertiaTensor();
  }

  public void CalculateCustomInertiaTensor()
  {
    if (!(bool) this.physicBody)
      this.physicBody = this.gameObject.GetPhysicBody();
    if (!(bool) (UnityEngine.Object) this.customInertiaTensorCollider)
    {
      Debug.LogWarning((object) ("Cannot calculate custom inertia tensor because no custom collider has been set on " + this.itemId));
      this.physicBody.ResetInertiaTensor();
    }
    else
    {
      List<Collider> colliderList = new List<Collider>();
      foreach (Collider componentsInChild in this.physicBody.gameObject.GetComponentsInChildren<Collider>())
      {
        if (!componentsInChild.isTrigger && !((UnityEngine.Object) this.customInertiaTensorCollider == (UnityEngine.Object) componentsInChild))
        {
          componentsInChild.enabled = false;
          colliderList.Add(componentsInChild);
        }
      }
      this.customInertiaTensorCollider.enabled = true;
      this.customInertiaTensorCollider.isTrigger = false;
      this.physicBody.ResetInertiaTensor();
      this.customInertiaTensorPos = this.physicBody.inertiaTensor;
      this.customInertiaTensorRot = this.physicBody.inertiaTensorRotation;
      this.customInertiaTensorCollider.isTrigger = true;
      this.customInertiaTensorCollider.enabled = false;
      int count = colliderList.Count;
      for (int index = 0; index < count; ++index)
        colliderList[index].enabled = true;
    }
  }

  public event Item.CullEvent OnCullEvent;

  public event Item.SpawnEvent OnSpawnEvent;

  public event Item.SpawnEvent OnDespawnEvent;

  private static void InvokeOnItemSpawn(Item item)
  {
    if (Item.OnItemSpawn == null)
      return;
    foreach (Delegate invocation in Item.OnItemSpawn.GetInvocationList())
    {
      if (invocation is Action<Item> action)
      {
        try
        {
          action(item);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during OnItemSpawn: {ex}");
        }
      }
    }
  }

  private static void InvokeOnItemDespawn(Item item)
  {
    if (Item.OnItemDespawn == null)
      return;
    foreach (Delegate invocation in Item.OnItemDespawn.GetInvocationList())
    {
      if (invocation is Action<Item> action)
      {
        try
        {
          action(item);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during OnItemDespawn: {ex}");
        }
      }
    }
  }

  private static void InvokeOnItemSnap(Item item, Holder holder)
  {
    if (Item.OnItemSnap == null)
      return;
    foreach (Delegate invocation in Item.OnItemSnap.GetInvocationList())
    {
      if (invocation is Action<Item, Holder> action)
      {
        try
        {
          action(item, holder);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during OnItemSnap: {ex}");
        }
      }
    }
  }

  private static void InvokeOnItemUnSnap(Item item, Holder holder)
  {
    if (Item.OnItemUnSnap == null)
      return;
    foreach (Delegate invocation in Item.OnItemUnSnap.GetInvocationList())
    {
      if (invocation is Action<Item, Holder> action)
      {
        try
        {
          action(item, holder);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during OnItemUnSnap: {ex}");
        }
      }
    }
  }

  private static void InvokeOnItemGrab(Item item, Handle handle, RagdollHand ragdollHand)
  {
    if (Item.OnItemGrab == null)
      return;
    foreach (Delegate invocation in Item.OnItemGrab.GetInvocationList())
    {
      if (invocation is Action<Item, Handle, RagdollHand> action)
      {
        try
        {
          action(item, handle, ragdollHand);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during OnItemGrab: {ex}");
        }
      }
    }
  }

  private static void InvokeOnItemUngrab(Item item, Handle handle, RagdollHand ragdollHand)
  {
    if (Item.OnItemUngrab == null)
      return;
    foreach (Delegate invocation in Item.OnItemUngrab.GetInvocationList())
    {
      if (invocation is Action<Item, Handle, RagdollHand> action)
      {
        try
        {
          action(item, handle, ragdollHand);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during OnItemUngrab: {ex}");
        }
      }
    }
  }

  public event Item.ImbuesChangeEvent OnImbuesChangeEvent;

  public event Item.ContainerEvent OnContainerAddEvent;

  public event Item.ZoneEvent OnZoneEvent;

  public void InvokeOnItemStored(UIInventory inventory, ItemContent itemContent)
  {
    UIInventory.ItemDelegate onItemStored = this.OnItemStored;
    if (onItemStored == null)
      return;
    onItemStored(inventory, itemContent, this);
  }

  public void InvokeOnItemRetrieved(UIInventory inventory, ItemContent itemContent)
  {
    UIInventory.ItemDelegate onItemRetrieved = this.OnItemRetrieved;
    if (onItemRetrieved == null)
      return;
    onItemRetrieved(inventory, itemContent, this);
  }

  public event Item.DamageReceivedDelegate OnDamageReceivedEvent;

  public event Item.GrabDelegate OnGrabEvent;

  public event Item.ReleaseDelegate OnHandleReleaseEvent;

  public event Item.ReleaseDelegate OnUngrabEvent;

  public event Item.HolderDelegate OnSnapEvent;

  public event Item.HolderDelegate OnUnSnapEvent;

  public event Item.ThrowingDelegate OnThrowEvent;

  public event Item.ThrowingDelegate OnFlyStartEvent;

  public event Item.ThrowingDelegate OnFlyEndEvent;

  public event Item.TelekinesisDelegate OnTelekinesisGrabEvent;

  public event Item.TelekinesisReleaseDelegate OnTelekinesisReleaseEvent;

  public event Item.TelekinesisTemporalDelegate OnTelekinesisRepelEvent;

  public event Item.TelekinesisTemporalDelegate OnTelekinesisPullEvent;

  public event Item.TelekinesisSpinEvent OnTKSpinStart;

  public event Item.TelekinesisSpinEvent OnTKSpinEnd;

  public event Item.TouchActionDelegate OnTouchActionEvent;

  public event Item.HeldActionDelegate OnHeldActionEvent;

  public event Item.MagnetDelegate OnMagnetCatchEvent;

  public event Item.MagnetDelegate OnMagnetReleaseEvent;

  public event Item.BreakStartDelegate OnBreakStart;

  public event Item.LoadDelegate OnDataLoaded;

  public void InvokeOnDataLoaded()
  {
    if (this.OnDataLoaded == null)
      return;
    foreach (Delegate invocation in this.OnDataLoaded.GetInvocationList())
    {
      if (invocation is Item.LoadDelegate loadDelegate)
      {
        try
        {
          loadDelegate();
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during onGameLoad event: {ex}");
        }
      }
    }
  }

  public event Item.OverrideContentCustomDataEvent OnOverrideContentCustomDataEvent;

  public event Item.IgnoreRagdollCollisionEvent OnIgnoreRagdollCollision;

  public event Item.IgnoreItemCollisionEvent OnIgnoreItemCollision;

  public event Item.IgnoreColliderCollisionEvent OnIgnoreColliderCollision;

  public event Item.SetCollidersEvent OnSetCollidersEvent;

  public event Item.SetColliderLayerEvent OnSetColliderLayerEvent;

  public LayerName forcedLayer
  {
    get
    {
      if (this.forcedItemLayer != LayerName.None)
        return this.forcedItemLayer;
      ItemData data = this.data;
      LayerName? nullable;
      if ((data != null ? (data.diffForceLayerWhenHeld ? 1 : 0) : 0) != 0)
      {
        List<RagdollHand> handlers = this.handlers;
        if ((handlers != null ? __nonvirtual (handlers.Count) : 0) > 0)
        {
          nullable = this.data?.forceLayerHeld;
          goto label_6;
        }
      }
      nullable = this.data?.forceLayer;
label_6:
      return nullable.GetValueOrDefault();
    }
  }

  public Handle GetMainHandle(Side side)
  {
    return side != Side.Right ? this.mainHandleLeft : this.mainHandleRight;
  }

  public void ForceLayer(LayerName layer)
  {
    this.forcedItemLayer = layer;
    this.SetColliderAndMeshLayer(GameManager.GetLayer(layer));
  }

  public virtual bool TryGetData(out ItemData data, bool printError = true)
  {
    data = this.data;
    int num = this.data != null ? 1 : 0;
    if (!(num == 0 & printError))
      return num != 0;
    Debug.LogError((object) $"Something tried to access the item data of item [ {this.name} ], but the item data is null! An error occurred and prevented the item data from properly loading.");
    return num != 0;
  }

  public virtual void Load(ItemData itemData)
  {
    if (this.loaded)
      return;
    if (this.data == null)
      this.LoadData(itemData);
    this.LoadInteractable(itemData);
    this.LoadModules();
    this.RefreshTotalItemMass();
    this.RefreshItemHasSlash();
    this.loaded = true;
    this.SetCull(this.isCulled, false);
    this.InvokeOnDataLoaded();
  }

  public void LoadData(ItemData itemData)
  {
    this.itemId = itemData.id;
    this.data = itemData.Clone() as ItemData;
    this.Load((EntityData) this.data);
    if ((bool) (UnityEngine.Object) this.customInertiaTensorCollider)
      this.SetCustomInertiaTensor();
    if (itemData.overrideMassAndDrag)
    {
      if ((bool) (UnityEngine.Object) this.mainCollisionHandler)
        this.mainCollisionHandler.SetPhysicBody(itemData.mass, itemData.drag, itemData.angularDrag);
      else
        Debug.LogErrorFormat((UnityEngine.Object) this, $"Item {this.name} have no mainCollisionHandler!");
    }
    this.flyRotationSpeed = itemData.flyRotationSpeed;
    this.flyThrowAngle = itemData.flyThrowAngle;
    this.distantGrabSafeDistance = itemData.telekinesisSafeDistance;
    this.distantGrabSpinEnabled = itemData.HasFlag(ItemFlags.Spinnable);
    this.distantGrabThrowRatio = itemData.telekinesisThrowRatio;
    this.customSnaps = itemData.customSnaps;
    this.forcedItemLayer = LayerName.None;
    if ((bool) (UnityEngine.Object) this.audioContainerSnap)
      Catalog.ReleaseAsset<AudioContainer>(this.audioContainerSnap);
    if (itemData.snapAudioContainerLocation != null)
      Catalog.LoadAssetAsync<AudioContainer>((object) itemData.snapAudioContainerLocation, (Action<AudioContainer>) (value =>
      {
        this.audioContainerSnap = value;
        Action<AudioContainer> onSnapAudioLoaded = this.OnSnapAudioLoaded;
        if (onSnapAudioLoaded == null)
          return;
        onSnapAudioLoaded(this.audioContainerSnap);
      }), itemData.id);
    if ((bool) (UnityEngine.Object) this.audioContainerInventory)
      Catalog.ReleaseAsset<AudioContainer>(this.audioContainerInventory);
    if (itemData.inventoryAudioContainerLocation != null)
      Catalog.LoadAssetAsync<AudioContainer>((object) itemData.inventoryAudioContainerLocation, (Action<AudioContainer>) (value =>
      {
        this.audioContainerInventory = value;
        if ((UnityEngine.Object) this.audioContainerInventory != (UnityEngine.Object) null)
        {
          Action<AudioContainer> inventoryAudioLoaded = this.OnInventoryAudioLoaded;
          if (inventoryAudioLoaded == null)
            return;
          inventoryAudioLoaded(this.audioContainerInventory);
        }
        else
          Debug.LogWarning((object) ("Inventory audio container is null for item " + itemData.id));
      }), itemData.id);
    int count1 = itemData.effectHinges.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ItemData.EffectHinge effectHinge1 = itemData.effectHinges[index1];
      int count2 = this.effectHinges.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        HingeEffect effectHinge2 = this.effectHinges[index2];
        if (effectHinge2.name == effectHinge1.transformName)
          effectHinge2.Load(effectHinge1.effectId, effectHinge1.minTorque, effectHinge1.maxTorque);
      }
    }
    int count3 = itemData.whooshs.Count;
    for (int index3 = 0; index3 < count3; ++index3)
    {
      ItemData.Whoosh whooshData = itemData.whooshs[index3];
      int count4 = this.whooshPoints.Count;
      for (int index4 = 0; index4 < count4; ++index4)
      {
        WhooshPoint whooshPoint = this.whooshPoints[index4];
        if (whooshPoint.name == whooshData.transformName)
        {
          EffectData data = Catalog.GetData<EffectData>(whooshData.effectId);
          if (data != null)
            whooshPoint.Load(data, whooshData);
        }
      }
    }
    int count5 = this.colliderGroups.Count;
    for (int index5 = 0; index5 < count5; ++index5)
    {
      ColliderGroup colliderGroup1 = this.colliderGroups[index5];
      int count6 = itemData.colliderGroups.Count;
      for (int index6 = 0; index6 < count6; ++index6)
      {
        ItemData.ColliderGroup colliderGroup2 = itemData.colliderGroups[index6];
        if (colliderGroup1.name == colliderGroup2.transformName)
        {
          if (colliderGroup2.colliderGroupData != null)
            colliderGroup1.Load(colliderGroup2.colliderGroupData);
          else
            Debug.LogWarning((object) ("ColliderGroupData is null for " + colliderGroup2.transformName));
        }
      }
    }
    int count7 = this.collisionHandlers.Count;
    for (int index7 = 0; index7 < count7; ++index7)
    {
      CollisionHandler collisionHandler = this.collisionHandlers[index7];
      int count8 = collisionHandler.damagers.Count;
      for (int index8 = 0; index8 < count8; ++index8)
      {
        Damager damager1 = collisionHandler.damagers[index8];
        bool flag = false;
        int count9 = itemData.damagers.Count;
        for (int index9 = 0; index9 < count9; ++index9)
        {
          ItemData.Damager damager2 = itemData.damagers[index9];
          if (damager1.name == damager2.transformName)
          {
            flag = true;
            damager1.Load(damager2.damagerData, collisionHandler);
          }
        }
        if (!flag)
          Debug.LogWarning((object) $"Damager '{damager1.name}' on item {this.data.id} did not load any DamagerData!", (UnityEngine.Object) this);
      }
      collisionHandler.SortDamagers();
    }
    int count10 = this.collisionHandlers.Count;
    for (int index = 0; index < count10; ++index)
    {
      CollisionHandler collisionHandler = this.collisionHandlers[index];
      bool flag = collisionHandler.collisions == null;
      if (!flag)
        collisionHandler.StopCollisions();
      if (this.data.collisionMaxOverride > 0)
      {
        if (flag || collisionHandler.collisions.Length != this.data.collisionMaxOverride)
          collisionHandler.SetMaxCollision(this.data.collisionMaxOverride);
      }
      else if (flag || collisionHandler.collisions.Length != Catalog.gameData.maxObjectCollision)
        collisionHandler.SetMaxCollision(Catalog.gameData.maxObjectCollision);
      collisionHandler.checkMinVelocity = !this.data.collisionNoMinVelocityCheck;
      collisionHandler.enterOnly = this.data.collisionEnterOnly;
    }
    this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.DroppedItem));
  }

  public void LoadInteractable(ItemData itemData)
  {
    foreach (Interactable interactable1 in !(bool) (UnityEngine.Object) this.breakable ? this.GetComponentsInChildren<Interactable>() : this.breakable.unbrokenObjectsHolder.GetComponentsInChildren<Interactable>())
    {
      int count = itemData.Interactables.Count;
      for (int index = 0; index < count; ++index)
      {
        ItemData.Interactable interactable2 = itemData.Interactables[index];
        if (!(interactable1.name != interactable2.transformName))
        {
          interactable1.interactableId = interactable2.interactableId;
          InteractableData outputData;
          if (Catalog.TryGetData<InteractableData>(interactable2.interactableId, out outputData))
          {
            InteractableData interactableData = outputData.Clone() as InteractableData;
            interactable1.Load(interactableData);
            break;
          }
          Debug.LogWarning((object) $"Interactable '{interactable1.name}' on item {this.data.id} did not load any InteractableData with interactableId: {interactable2.interactableId}!");
        }
      }
      if (interactable1.data == null)
        interactable1.TryLoadFromID();
      if (interactable1.data == null)
        Debug.LogWarning((object) $"Interactable '{interactable1.name}' on item {this.data.id} did not load any InteractableData. It may not have a matching interactable in the itemData or no ID in the prefab", (UnityEngine.Object) this);
    }
  }

  public void LoadModules()
  {
    if (this.loadedItemModules || this.data.modules == null)
      return;
    int count = this.data.modules.Count;
    for (int index = 0; index < count; ++index)
      this.data.modules[index].OnItemLoaded(this);
    this.loadedItemModules = true;
  }

  protected internal override void ManagedUpdate()
  {
    this.CheckIfItemIsMoving();
    if (this.isMoving || this.isFlying)
    {
      bool flag1 = (bool) (UnityEngine.Object) this.lastHandler && !this.lastHandler.creature.isPlayer;
      bool flag2 = false;
      if (flag1)
        flag2 = (bool) (UnityEngine.Object) this.lastHandler.creature.brain.currentTarget && !this.lastHandler.creature.brain.currentTarget.isPlayer && !this.lastHandler.creature.brain.currentTarget.ragdoll.IsPhysicsEnabled();
      bool flag3 = false;
      if ((bool) (UnityEngine.Object) Player.currentCreature & flag1 & flag2)
        flag3 = (double) UnityEngine.Vector3.Distance(Player.currentCreature.transform.position, this.transform.position) < ((double) Creature.allActive[0]?.ragdoll?.physicTogglePlayerRadius ?? 5.0);
      if (flag1 & flag2 && !flag3)
        this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingObjectOnly));
      else
        this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
      if (this.isFlying && (bool) (UnityEngine.Object) this.flyDirRef)
      {
        UnityEngine.Vector3 to = this.isFlyingBackwards ? -this.flyDirRef.forward : this.flyDirRef.forward;
        double num1 = (double) UnityEngine.Vector3.SignedAngle(UnityEngine.Vector3.ProjectOnPlane(this.physicBody.velocity.normalized, this.flyDirRef.right), to, this.flyDirRef.right);
        UnityEngine.Vector3 velocity = this.physicBody.velocity;
        float num2 = UnityEngine.Vector3.SignedAngle(UnityEngine.Vector3.ProjectOnPlane(velocity.normalized, this.flyDirRef.up), to, this.flyDirRef.up);
        UnityEngine.Vector3 vector3_1 = this.transform.InverseTransformDirection(this.flyDirRef.up);
        UnityEngine.Vector3 vector3_2 = this.transform.InverseTransformDirection(this.flyDirRef.right);
        UnityEngine.Vector3 vector3_3 = (float) -num1 * vector3_2 + -num2 * vector3_1;
        Transform transform = this.transform;
        UnityEngine.Vector3 zero = UnityEngine.Vector3.zero;
        UnityEngine.Vector3 b = vector3_3;
        double flyRotationSpeed = (double) this.flyRotationSpeed;
        velocity = this.physicBody.velocity;
        double magnitude = (double) velocity.magnitude;
        double t = flyRotationSpeed * magnitude * (double) Time.deltaTime;
        UnityEngine.Vector3 eulers = UnityEngine.Vector3.Slerp(zero, b, (float) t);
        transform.Rotate(eulers, Space.Self);
      }
    }
    this.UpdateWater();
    this.UpdateLastSpeeds();
    if ((UnityEngine.Object) this.lastHandler != (UnityEngine.Object) null && this.physicBody.IsSleeping() || this.waterHandler.inWater && !this.physicBody.HasMeaningfulVelocity())
    {
      this.StopThrowing();
      this.StopFlying();
    }
    int count = this.whooshPoints.Count;
    for (int index = 0; index < count; ++index)
      this.whooshPoints[index].UpdateWhooshPoint();
    this.wasMoving = this.isMoving;
  }

  /// <summary>
  /// Check if the item is currently moving by looking at its rigidbody velocity, and if moving is not ignored.
  /// </summary>
  private void CheckIfItemIsMoving()
  {
    this.isMoving = !this.ignoreIsMoving && !this.isThrowed && (this.isFlying || !this.physicBody.IsSleeping() && this.physicBody.HasMeaningfulVelocity());
    if (this.isMoving)
    {
      if (this.wasMoving)
        return;
      Item.allMoving.Add(this);
    }
    else
    {
      if (!this.wasMoving)
        return;
      Item.allMoving.Remove(this);
    }
  }

  public void RefreshTotalItemMass()
  {
    this.totalCombinedMass = this.physicBody.mass;
    Breakable component;
    this.TryGetComponent<Breakable>(out component);
    foreach (PhysicBody physicBodiesInChild in this.gameObject.GetPhysicBodiesInChildren(true))
    {
      if (physicBodiesInChild.gameObject.activeInHierarchy && !physicBodiesInChild.isKinematic && !(physicBodiesInChild == this.physicBody) && (!(bool) (UnityEngine.Object) component || !component.allSubBodies.Contains(physicBodiesInChild)))
        this.totalCombinedMass += physicBodiesInChild.mass;
    }
  }

  public void RefreshItemHasSlash()
  {
    this.hasSlash = false;
    foreach (CollisionHandler collisionHandler in this.collisionHandlers)
    {
      foreach (Damager damager in collisionHandler.damagers)
      {
        if (damager.type == Damager.Type.Slash)
        {
          this.hasSlash = true;
          break;
        }
      }
      if (this.hasSlash)
        break;
    }
  }

  public virtual void UpdateReveal()
  {
    if (!this.updateReveal)
      return;
    this.updateReveal = false;
    int count = this.revealDecals.Count;
    for (int index = 0; index < count; ++index)
    {
      if ((UnityEngine.Object) this.revealDecals[index] != (UnityEngine.Object) null)
        this.updateReveal = this.revealDecals[index].UpdateOvertime() || this.updateReveal;
    }
  }

  protected void OnCollisionStartEvent(CollisionInstance collisionInstance) => this.StopFlying();

  public void OnDamageReceived(CollisionInstance collisionInstance)
  {
    if (this.OnDamageReceivedEvent == null)
      return;
    this.OnDamageReceivedEvent(collisionInstance);
  }

  public bool IsVisible()
  {
    int count = this.renderers.Count;
    for (int index = 0; index < count; ++index)
    {
      if (this.renderers[index].isVisible)
        return true;
    }
    return false;
  }

  public void SetParryMagic(bool active)
  {
    int count1 = this.colliderGroups.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ColliderGroup colliderGroup = this.colliderGroups[index1];
      if (colliderGroup.modifier.imbueType != ColliderGroupData.ImbueType.None)
      {
        int count2 = colliderGroup.colliders.Count;
        for (int index2 = 0; index2 < count2; ++index2)
        {
          Collider collider = colliderGroup.colliders[index2];
          if (active)
            collider.tag = Item.parryMagicTag;
          else
            collider.tag = "Untagged";
        }
      }
    }
    this.parryActive = active;
  }

  public void SetColliders(bool active, bool force = false)
  {
    if (!force && active == this.isCollidersOn)
      return;
    if (active)
    {
      foreach (Collider disabledCollider in this.disabledColliders)
        disabledCollider.enabled = true;
      this.disabledColliders.Clear();
    }
    else
    {
      foreach (ColliderGroup colliderGroup in this.colliderGroups)
      {
        foreach (Collider collider in colliderGroup.colliders)
        {
          if (!((UnityEngine.Object) collider == (UnityEngine.Object) this.customInertiaTensorCollider) && collider.enabled)
          {
            collider.enabled = false;
            this.disabledColliders.Add(collider);
          }
        }
      }
    }
    this.isCollidersOn = active;
    Item.SetCollidersEvent setCollidersEvent = this.OnSetCollidersEvent;
    if (setCollidersEvent == null)
      return;
    setCollidersEvent(this, active, force);
  }

  public void ForceMainHandler(RagdollHand ragdollHand) => this.mainHandler = ragdollHand;

  public bool IsFree
  {
    get
    {
      return !this.IsHeld() && (UnityEngine.Object) this.holder == (UnityEngine.Object) null && !this.isGripped && !this.isTelekinesisGrabbed;
    }
  }

  public bool IsHeld() => this.IsHanded();

  public bool IsHeldByPlayer
  {
    get
    {
      if (!this.isGripped)
      {
        RagdollHand mainHandler = this.mainHandler;
        int num;
        if (mainHandler == null)
        {
          num = 0;
        }
        else
        {
          bool? isPlayer = mainHandler.creature?.isPlayer;
          bool flag = true;
          num = isPlayer.GetValueOrDefault() == flag & isPlayer.HasValue ? 1 : 0;
        }
        if (num == 0)
          return this.isTelekinesisGrabbed;
      }
      return true;
    }
  }

  public virtual bool IsHanded() => this.handlers.Count > 0;

  public virtual bool IsHanded(Handle ignoreHandle)
  {
    int count = this.handles.Count;
    for (int index = 0; index < count; ++index)
    {
      Handle handle = this.handles[index];
      if (handle.IsHanded() && (UnityEngine.Object) handle != (UnityEngine.Object) ignoreHandle)
        return true;
    }
    return false;
  }

  public virtual bool IsHanded(Side side)
  {
    int count = this.handlers.Count;
    for (int index = 0; index < count; ++index)
    {
      if (this.handlers[index].side == side)
        return true;
    }
    return false;
  }

  public virtual bool IsTwoHanded(List<Handle> validHandles = null)
  {
    if (validHandles == null)
      validHandles = this.handles;
    int count1 = validHandles.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      Handle validHandle = validHandles[index1];
      int count2 = validHandle.handlers.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        Handle grabbedHandle = validHandle.handlers[index2].otherHand?.grabbedHandle;
        if ((bool) (UnityEngine.Object) grabbedHandle && (UnityEngine.Object) grabbedHandle.item == (UnityEngine.Object) this)
          return true;
      }
    }
    return false;
  }

  public virtual void GetPositionRotationRelativeToHand(
    out UnityEngine.Vector3 handLocalPos,
    out Quaternion handLocalRot,
    RagdollHand hand,
    Handle handle = null,
    HandlePose handlePose = null,
    float? axisRatio = null)
  {
    if (this.handlers.Contains(hand))
    {
      handLocalPos = hand.transform.InverseTransformPoint(this.transform.position);
      handLocalRot = hand.transform.InverseTransformRotation(this.transform.rotation);
    }
    else
    {
      if ((UnityEngine.Object) handle == (UnityEngine.Object) null)
        handle = this.GetMainHandle(hand.side);
      if ((UnityEngine.Object) handlePose == (UnityEngine.Object) null)
        handlePose = handle.GetNearestOrientation(hand.grip, hand.side);
      if (!axisRatio.HasValue)
        axisRatio = new float?(handle.GetNearestAxisPosition(hand.transform.position));
      Handle.GripInfo gripPoint = handle.CreateGripPoint(hand, axisRatio.Value, handlePose);
      handLocalPos = gripPoint.transform.InverseTransformPoint(this.transform.position);
      handLocalRot = gripPoint.transform.InverseTransformRotation(this.transform.rotation);
    }
  }

  public virtual void StopFlying()
  {
    if (!this.isFlying)
      return;
    this.isFlying = false;
    this.isFlyingBackwards = false;
    this.RefreshCollision();
    Item.ThrowingDelegate onFlyEndEvent = this.OnFlyEndEvent;
    if (onFlyEndEvent == null)
      return;
    onFlyEndEvent(this);
  }

  public virtual void StopThrowing()
  {
    if (!this.isThrowed || this.forceThrown)
      return;
    this.isThrowed = false;
    this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.DroppedItem));
    this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.dropped;
    if (Item.allThrowed.Contains(this))
      Item.allThrowed.Remove(this);
    this.lastInteractionTime = Time.time;
    this.lastHandler = (RagdollHand) null;
  }

  public void SetCenterOfMass(UnityEngine.Vector3 localPosition)
  {
    this.physicBody.centerOfMass = localPosition;
  }

  private void UpdateLastSpeeds()
  {
    if (!this.trackVelocity || !this.physicBody.isKinematic && !this.physicBody.HasMeaningfulVelocity())
      return;
    Transform transform = this.transform;
    UnityEngine.Vector3 eulerAngles = transform.eulerAngles;
    UnityEngine.Vector3 position = transform.position;
    if (this.lastPosition != position || this.lastEulers != eulerAngles)
    {
      float num = (float) (1.0 / ((double) Time.time - (double) this.lastUpdateTime));
      if (this.physicBody.isKinematic || (UnityEngine.Object) this.mainHandler != (UnityEngine.Object) null && this.mainHandler.gripInfo.type == Handle.GripInfo.Type.HandSync)
      {
        this.lastLinearVelocity = (position - this.lastPosition) * num;
        this.lastAngularVelocity = (eulerAngles - this.lastEulers) * ((float) Math.PI / 180f * num);
      }
      else
      {
        this.lastLinearVelocity = this.physicBody.velocity;
        this.lastAngularVelocity = this.physicBody.angularVelocity;
      }
      this.lastUpdateTime = Time.time;
    }
    this.lastPosition = position;
    this.lastEulers = eulerAngles;
  }

  /// <summary>
  /// Gets the item's velocity at the given world space position. Used to determine impact velocity for both real and manufactured collisions.
  /// </summary>
  public UnityEngine.Vector3 GetItemPointVelocity(UnityEngine.Vector3 worldSpacePosition, bool useCalculated = false)
  {
    return !useCalculated ? this.physicBody.GetPointVelocity(worldSpacePosition) : this.transform.TransformDirection(UnityEngine.Vector3.Cross(this.lastAngularVelocity, this.transform.InverseTransformPoint(worldSpacePosition) - this.physicBody.centerOfMass)) + this.lastLinearVelocity;
  }

  public void InvokeMagnetCatchEvent(ItemMagnet itemMagnet, EventTime eventTime)
  {
    this.magnets.Add(itemMagnet);
    if (this.OnMagnetCatchEvent == null)
      return;
    this.OnMagnetCatchEvent(itemMagnet, eventTime);
  }

  public void InvokeMagnetReleaseEvent(ItemMagnet itemMagnet, EventTime eventTime)
  {
    this.magnets.Remove(itemMagnet);
    if (this.OnMagnetReleaseEvent == null)
      return;
    this.OnMagnetReleaseEvent(itemMagnet, eventTime);
  }

  public void ResetCenterOfMass()
  {
    if (this.useCustomCenterOfMass)
      this.physicBody.centerOfMass = this.customCenterOfMass;
    else
      this.physicBody.ResetCenterOfMass();
  }

  public virtual void OnTouchAction(
    RagdollHand ragdollHand,
    Interactable interactable,
    Interactable.Action action)
  {
    if (this.OnTouchActionEvent == null)
      return;
    this.OnTouchActionEvent(ragdollHand, interactable, action);
  }

  public virtual void OnHeldAction(
    RagdollHand ragdollHand,
    Handle handle,
    Interactable.Action action)
  {
    Interactable.Action action1 = GameManager.options.invertUseAndSlide ? Interactable.Action.AlternateUseStart : Interactable.Action.UseStart;
    Interactable.Action action2 = GameManager.options.invertUseAndSlide ? Interactable.Action.AlternateUseStop : Interactable.Action.UseStop;
    bool flag1 = action == action1;
    bool flag2 = action == action2;
    if (flag1 | flag2)
    {
      bool active = false;
      if (flag1)
        active = true;
      if (flag2)
        active = false;
      int count = this.imbues.Count;
      for (int index = 0; index < count; ++index)
      {
        Imbue imbue = this.imbues[index];
        if (imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal)
          imbue.OnCrystalUse(ragdollHand, active);
      }
    }
    if (this.OnHeldActionEvent == null)
      return;
    this.OnHeldActionEvent(ragdollHand, handle, action);
  }

  public void RefreshAllowTelekinesis()
  {
    if (this.IsHanded() || this.isGripped)
    {
      int count = this.handles.Count;
      for (int index = 0; index < count; ++index)
        this.handles[index].SetTelekinesis(false);
    }
    else
    {
      int count = this.handles.Count;
      for (int index = 0; index < count; ++index)
      {
        Handle handle = this.handles[index];
        if (handle.data == null)
        {
          Debug.LogWarning((object) $"Handle({handle.name}) on Item '{this.transform.name}' contains no 'data', defaulting telekenesis to false. [EXPAND FOR MORE INFO]\n\nThis can happen when one of your handles isn't defined in the JSON file, in this case {handle.name} is not defined in your 'Interactables' array, please add it to fix this warning.");
          handle.SetTelekinesis(false);
        }
        else
          handle.SetTelekinesis(handle.data.allowTelekinesis);
      }
    }
  }

  public virtual void OnTelekinesisGrab(Handle handle, SpellTelekinesis teleGrabber)
  {
    this.RefreshCollision();
    if (!Item.allTk.Contains(this))
      Item.allTk.Add(this);
    Item.TelekinesisDelegate telekinesisGrabEvent = this.OnTelekinesisGrabEvent;
    if (telekinesisGrabEvent != null)
      telekinesisGrabEvent(handle, teleGrabber);
    EventManager.InvokeSpellUsed("Telekinesis", Player.local.creature, teleGrabber.spellCaster.ragdollHand.side);
  }

  public virtual void OnTelekinesisRelease(
    Handle handle,
    SpellTelekinesis teleGrabber,
    bool tryThrow,
    bool isGrabbing)
  {
    this.RefreshCollision();
    if (Item.allTk.Contains(this))
      Item.allTk.Remove(this);
    Item.TelekinesisReleaseDelegate telekinesisReleaseEvent = this.OnTelekinesisReleaseEvent;
    if (telekinesisReleaseEvent == null)
      return;
    telekinesisReleaseEvent(handle, teleGrabber, tryThrow, isGrabbing);
  }

  public virtual void OnGrab(Handle handle, RagdollHand ragdollHand)
  {
    if (this.handlers.Count == 0)
      this.mainHandler = ragdollHand;
    this.handlers.Add(ragdollHand);
    this.StopThrowing();
    this.StopFlying();
    this.IgnoreIsMoving();
    this.lastHandler = ragdollHand;
    this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.grabbed;
    this.physicBody.sleepThreshold = 0.0f;
    this.RefreshCollision();
    this.RefreshAllowTelekinesis();
    if (this.parryActive)
      this.SetParryMagic(false);
    this.SetColliders(ragdollHand.ragdoll.IsPhysicsEnabled(true));
    this.lastInteractionTime = Time.time;
    Item.GrabDelegate onGrabEvent = this.OnGrabEvent;
    if (onGrabEvent != null)
      onGrabEvent(handle, ragdollHand);
    EventManager.InvokeItemGrab(handle, ragdollHand);
    Item.InvokeOnItemGrab(this, handle, ragdollHand);
  }

  public virtual void OnUnGrab(Handle handle, RagdollHand ragdollHand, bool throwing)
  {
    this.handlers.Remove(ragdollHand);
    if (this.handlers.Count == 0)
    {
      this.mainHandler = (RagdollHand) null;
      this.SetColliders(true);
    }
    else
      this.mainHandler = this.handlers[0];
    this.RefreshCollision(throwing);
    if (this.handlers.Count == 0)
    {
      this.physicBody.sleepThreshold = this.orgSleepThreshold;
      this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.dropped;
      if (throwing)
      {
        bool flag = (bool) (UnityEngine.Object) ragdollHand.playerHand && (double) PlayerControl.GetHand(ragdollHand.side).GetHandVelocity().magnitude * (1.0 / (double) Time.timeScale) > (double) Catalog.gameData.throwVelocity;
        this.Throw(flag ? Catalog.gameData.throwMultiplier * this.data.throwMultiplier : 1f, this.data.HasFlag(ItemFlags.Throwable) & flag ? Item.FlyDetection.CheckAngle : Item.FlyDetection.Disabled);
      }
      Item.ReleaseDelegate onUngrabEvent = this.OnUngrabEvent;
      if (onUngrabEvent != null)
        onUngrabEvent(handle, ragdollHand, throwing);
    }
    this.RefreshAllowTelekinesis();
    if (!this.IsHanded() && this.parryActive)
      this.SetParryMagic(false);
    this.lastInteractionTime = Time.time;
    Item.ReleaseDelegate handleReleaseEvent = this.OnHandleReleaseEvent;
    if (handleReleaseEvent != null)
      handleReleaseEvent(handle, ragdollHand, throwing);
    EventManager.InvokeItemRelease(handle, ragdollHand, throwing);
    Item.InvokeOnItemUngrab(this, handle, ragdollHand);
  }

  public virtual void Throw(float throwMultiplier = 1f, Item.FlyDetection flyDetection = Item.FlyDetection.CheckAngle)
  {
    Item.ThrowingDelegate onThrowEvent = this.OnThrowEvent;
    if (onThrowEvent != null)
      onThrowEvent(this);
    this.isFlying = false;
    this.isThrowed = true;
    this.ignoreIsMoving = false;
    if (flyDetection == Item.FlyDetection.CheckAngle && (bool) (UnityEngine.Object) this.flyDirRef && (double) this.flyRotationSpeed > 0.0)
    {
      if ((double) this.flyThrowAngle > 0.0)
      {
        if ((double) UnityEngine.Vector3.Angle(this.physicBody.velocity.normalized, this.flyDirRef.forward) < (double) this.flyThrowAngle)
          this.isFlying = true;
        else if (this.data.allowFlyBackwards && (double) UnityEngine.Vector3.Angle(this.physicBody.velocity.normalized, -this.flyDirRef.forward) < (double) this.flyThrowAngle)
          this.isFlying = true;
      }
      else
        this.isFlying = true;
    }
    if (flyDetection == Item.FlyDetection.Forced)
      this.isFlying = true;
    this.isFlyingBackwards = this.isFlying && (bool) (UnityEngine.Object) this.flyDirRef && this.data.allowFlyBackwards && (double) UnityEngine.Vector3.Dot(this.physicBody.velocity.normalized, -this.flyDirRef.forward) > (double) UnityEngine.Vector3.Dot(this.physicBody.velocity.normalized, this.flyDirRef.forward);
    if ((double) throwMultiplier > 1.0)
      this.physicBody.velocity *= Mathf.Clamp(throwMultiplier, 1f, float.PositiveInfinity);
    this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
    this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.throwed;
    if (!Item.allThrowed.Contains(this))
      Item.allThrowed.Add(this);
    if (!this.isFlying || this.OnFlyStartEvent == null)
      return;
    this.OnFlyStartEvent(this);
  }

  /// <summary>
  /// Adds a recoil force to the item, and a lingering force if the item is held after the initial force is applied.
  /// </summary>
  /// <param name="forcePosAndDir">The position and rotation for the force. This can be any transform and can even be parented to this item.</param>
  /// <param name="initialForce">The amount of force to apply, relative to the force position and direction transform.</param>
  /// <param name="initialForceMode">How to apply initial force, usually impulse or velocity change works best here.</param>
  /// <param name="lingeringForce">How much force should be applied to prevent "weapon snapping". Also relative to the force position and direction transform.</param>
  /// <param name="lingerCurve">How much of that initial force to apply over time. Should usually be an ease-in-east-out curve or straight 1-to-0 line.</param>
  public virtual void AddRecoil(
    Transform forcePosAndDir,
    UnityEngine.Vector3 initialForce,
    ForceMode initialForceMode,
    UnityEngine.Vector3 lingeringForce,
    AnimationCurve lingerCurve = null,
    bool temporaryTransform = false)
  {
    if (lingerCurve == null)
      lingerCurve = AnimationCurve.EaseInOut(0.0f, 1f, 1f, 0.0f);
    this.StartCoroutine(this.LingeringForce(forcePosAndDir, initialForce, initialForceMode, lingeringForce, lingerCurve, temporaryTransform));
  }

  private IEnumerator LingeringForce(
    Transform forcePosAndDir,
    UnityEngine.Vector3 initialForce,
    ForceMode initialForceMode,
    UnityEngine.Vector3 lingeringForce,
    AnimationCurve lingerCurve,
    bool temporaryTransform)
  {
    float startTime = Time.time;
    float curveDuration = lingerCurve.GetLastTime();
    float endTime = startTime + curveDuration;
    if ((double) lingerCurve.GetLastValue() != 0.0)
      Debug.LogWarning((object) "Recoil linger curve doesn't end at value 0! The item may snap suddenly when recoil ends.");
    this.physicBody.AddForceAtPosition(forcePosAndDir.right * initialForce.x + forcePosAndDir.up * initialForce.y + forcePosAndDir.forward * initialForce.z, forcePosAndDir.position, initialForceMode);
    while ((double) Time.time < (double) endTime)
    {
      this.physicBody.AddForceAtPosition((forcePosAndDir.right * lingeringForce.x + forcePosAndDir.up * lingeringForce.y + forcePosAndDir.forward * lingeringForce.z) * lingerCurve.Evaluate(Mathf.Clamp(Time.time - startTime, 0.0f, curveDuration)), forcePosAndDir.position, ForceMode.Force);
      yield return (object) Yielders.FixedUpdate;
      if ((UnityEngine.Object) this.mainHandler == (UnityEngine.Object) null || this.mainHandler.gripInfo.type != Handle.GripInfo.Type.PlayerJoint && this.mainHandler.gripInfo.type != Handle.GripInfo.Type.HandJoint)
        break;
    }
    if (temporaryTransform)
      UnityEngine.Object.Destroy((UnityEngine.Object) forcePosAndDir.gameObject);
  }

  public virtual void OnSnap(Holder holder, bool silent = false)
  {
    silent = ((silent ? 1 : 0) | (!((UnityEngine.Object) Level.current != (UnityEngine.Object) null) ? 0 : (!Level.current.loaded ? 1 : 0))) != 0;
    this.holder = holder;
    this.StopThrowing();
    this.StopFlying();
    this.IgnoreIsMoving();
    this.RefreshCollision();
    this.ToggleImbueDrainOnSnap(true);
    if (!silent && (bool) (UnityEngine.Object) this.audioSource)
    {
      if ((double) this.snapPitchRange > 0.0)
        this.audioSource.pitch = UnityEngine.Random.Range(1f - this.snapPitchRange, 1f + this.snapPitchRange);
      if ((bool) (UnityEngine.Object) this.audioContainerSnap)
        this.audioSource.PlayOneShot(this.audioContainerSnap.PickAudioClip(0));
      else if ((bool) (UnityEngine.Object) holder.audioContainer)
        this.audioSource.PlayOneShot(holder.audioContainer.PickAudioClip(0));
    }
    if ((UnityEngine.Object) AreaManager.Instance != (UnityEngine.Object) null && AreaManager.Instance.CurrentArea != null)
    {
      Creature creature = holder.GetRootHolder().creature;
      if ((bool) (UnityEngine.Object) creature)
      {
        this.UnRegisterArea();
        this.Hide(creature.hidden);
      }
      else
      {
        Item parentItem = holder.GetRootHolder().parentItem;
        if ((bool) (UnityEngine.Object) parentItem)
        {
          this.UnRegisterArea();
          this.Hide(parentItem.isHidden);
        }
        else
          this.CheckCurrentArea();
      }
    }
    Item.HolderDelegate onSnapEvent = this.OnSnapEvent;
    if (onSnapEvent != null)
      onSnapEvent(holder);
    EventManager.InvokeHolderSnap(holder);
    Item.InvokeOnItemSnap(this, holder);
  }

  public virtual void OnUnSnap(Holder holder, bool silent = false)
  {
    silent = ((silent ? 1 : 0) | (!((UnityEngine.Object) Level.current != (UnityEngine.Object) null) ? 0 : (!Level.current.loaded ? 1 : 0))) != 0;
    this.ToggleImbueDrainOnSnap(false);
    this.holder = (Holder) null;
    if (!silent && (bool) (UnityEngine.Object) this.audioSource)
    {
      if ((double) this.snapPitchRange > 0.0)
        this.audioSource.pitch = UnityEngine.Random.Range(1f - this.snapPitchRange, 1f + this.snapPitchRange);
      if ((bool) (UnityEngine.Object) this.audioContainerSnap)
        this.audioSource.PlayOneShot(this.audioContainerSnap.PickAudioClip(1));
      else if ((bool) (UnityEngine.Object) holder.audioContainer)
        this.audioSource.PlayOneShot(holder.audioContainer.PickAudioClip(1));
    }
    Item.HolderDelegate onUnSnapEvent = this.OnUnSnapEvent;
    if (onUnSnapEvent != null)
      onUnSnapEvent(holder);
    EventManager.InvokeHolderUnsnap(holder);
    Item.InvokeOnItemUnSnap(this, holder);
  }

  /// <summary>
  /// Start imbue decreasing on snap
  /// Stop imbue decreasing on un-snap
  /// </summary>
  /// <param name="snapped">Did the item snap or un-snap ?</param>
  private void ToggleImbueDrainOnSnap(bool snapped)
  {
    if (this.data == null || !this.data.drainImbueOnSnap || !this.gameObject.activeInHierarchy)
      return;
    if (snapped)
    {
      this.imbueDecreaseRoutine = this.StartCoroutine(this.DecreaseImbueRoutine(this.data.imbueEnergyOverTimeOnSnap));
    }
    else
    {
      if (this.imbueDecreaseRoutine == null)
        return;
      this.StopCoroutine(this.imbueDecreaseRoutine);
    }
  }

  /// <summary>
  /// Decrease imbues energy gradually according to the given curve
  /// </summary>
  /// <param name="imbueEnergyOverTimeOnSnap">Curve to follow for energy decrease</param>
  /// <returns></returns>
  private IEnumerator DecreaseImbueRoutine(AnimationCurve imbueEnergyOverTimeOnSnap)
  {
    if (!Imbue.infiniteImbue && this.imbues != null && this.imbues.Count > 0)
    {
      float t = 0.0f;
      float totalTime = imbueEnergyOverTimeOnSnap.GetLastTime();
      float[] onSnapEnergyValues = new float[this.imbues.Count];
      int count1 = this.imbues.Count;
      for (int index = 0; index < count1; ++index)
        onSnapEnergyValues[index] = this.imbues[index].energy;
      while ((double) t < (double) totalTime)
      {
        int count2 = this.imbues.Count;
        for (int index = 0; index < count2; ++index)
          this.imbues[index].SetEnergyInstant(onSnapEnergyValues[index] * Mathf.Clamp01(imbueEnergyOverTimeOnSnap.Evaluate(t)));
        t += Time.deltaTime;
        yield return (object) Yielders.EndOfFrame;
      }
      int count3 = this.imbues.Count;
      for (int index = 0; index < count3; ++index)
      {
        float t1 = 1f - Mathf.Clamp01(imbueEnergyOverTimeOnSnap.Evaluate(totalTime));
        this.imbues[index].SetEnergyInstant(Mathf.Lerp(onSnapEnergyValues[index], 0.0f, t1));
      }
    }
  }

  public virtual void SetMeshLayer(int layer)
  {
    this.gameObject.layer = this.forceMeshLayer >= 0 ? this.forceMeshLayer : layer;
    int layer1 = Common.GetLayer(LayerName.UI);
    int count = this.renderers.Count;
    for (int index = 0; index < count; ++index)
    {
      Renderer renderer = this.renderers[index];
      if (!((UnityEngine.Object) renderer == (UnityEngine.Object) null) && renderer.gameObject.layer != layer1)
        renderer.gameObject.layer = this.forceMeshLayer >= 0 ? this.forceMeshLayer : layer;
    }
  }

  public int currentPhysicsLayer { get; protected set; }

  public virtual void SetColliderAndMeshLayer(int layer, bool force = false)
  {
    if (this.currentPhysicsLayer == layer && !force)
      return;
    this.currentPhysicsLayer = layer;
    this.SetColliderLayer(layer);
    this.SetMeshLayer(layer);
  }

  public virtual void SetColliderLayer(int layer)
  {
    if ((UnityEngine.Object) this.gameObject == (UnityEngine.Object) null)
      return;
    if (this.colliderGroups.CountCheck((Func<int, bool>) (count => count > 0)))
    {
      int count1 = this.colliderGroups.Count;
      for (int index1 = 0; index1 < count1; ++index1)
      {
        ColliderGroup colliderGroup = this.colliderGroups[index1];
        if (colliderGroup.colliders == null)
          colliderGroup.GroupSetup();
        if (colliderGroup.colliders != null)
        {
          int count2 = colliderGroup.colliders.Count;
          for (int index2 = 0; index2 < count2; ++index2)
          {
            Collider collider = colliderGroup.colliders[index2];
            if (!((UnityEngine.Object) collider.gameObject == (UnityEngine.Object) null) && collider.gameObject.layer != Common.GetLayer(LayerName.UI))
            {
              if (this.data != null)
                collider.gameObject.layer = this.forcedLayer != LayerName.None ? Common.GetLayer(this.forcedLayer) : layer;
              else
                collider.gameObject.layer = layer;
            }
          }
        }
      }
    }
    else
    {
      Debug.LogError((object) $"Item {this.name} ({this.data.id}) has no collider groups on it! This is a potential problem.");
      foreach (Collider componentsInChild in this.GetComponentsInChildren<Collider>())
      {
        if (componentsInChild.gameObject.layer != Common.GetLayer(LayerName.Touch) && componentsInChild.gameObject.layer != Common.GetLayer(LayerName.UI))
          componentsInChild.gameObject.layer = layer;
      }
    }
    Item.SetColliderLayerEvent colliderLayerEvent = this.OnSetColliderLayerEvent;
    if (colliderLayerEvent == null)
      return;
    colliderLayerEvent(this, layer);
  }

  public virtual void GetFurthestDamagerCollider(
    out Damager furthestDamager,
    out Collider furthestCollider,
    UnityEngine.Vector3? origin = null,
    bool ignoreHandleDamagers = true)
  {
    if (!origin.HasValue)
    {
      ref UnityEngine.Vector3? local = ref origin;
      Handle mainHandleRight = this.mainHandleRight;
      UnityEngine.Vector3 vector3 = mainHandleRight != null ? mainHandleRight.transform.position : this.transform.position;
      local = new UnityEngine.Vector3?(vector3);
    }
    furthestDamager = (Damager) null;
    int count1 = this.collisionHandlers.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      int count2 = this.mainCollisionHandler.damagers.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        Damager damager = this.collisionHandlers[index1].damagers[index2];
        if (((!((UnityEngine.Object) furthestDamager != (UnityEngine.Object) null) ? 0 : (damager.data.id.Contains("Handle") ? 1 : 0)) & (ignoreHandleDamagers ? 1 : 0)) == 0 && ((UnityEngine.Object) furthestDamager == (UnityEngine.Object) null || furthestDamager.data.id.Contains("Handle") & ignoreHandleDamagers || (double) (damager.transform.position - origin.Value).sqrMagnitude > (double) (furthestDamager.transform.position - origin.Value).sqrMagnitude))
          furthestDamager = damager;
      }
    }
    if ((UnityEngine.Object) furthestDamager == (UnityEngine.Object) null)
    {
      furthestCollider = (Collider) null;
    }
    else
    {
      furthestCollider = furthestDamager.colliderOnly;
      if (!((UnityEngine.Object) furthestCollider == (UnityEngine.Object) null))
        return;
      furthestCollider = furthestDamager.colliderGroup.colliders[0];
      int count3 = furthestDamager.colliderGroup.colliders.Count;
      for (int index = 1; index < count3; ++index)
      {
        Collider collider = furthestDamager.colliderGroup.colliders[index];
        UnityEngine.Vector3 vector3 = collider.transform.position - origin.Value;
        double sqrMagnitude1 = (double) vector3.sqrMagnitude;
        vector3 = furthestCollider.transform.position - origin.Value;
        double sqrMagnitude2 = (double) vector3.sqrMagnitude;
        if (sqrMagnitude1 > sqrMagnitude2)
          furthestCollider = collider;
      }
    }
  }

  [ContextMenu("RefreshCollision")]
  protected virtual void RefreshCollisionTest() => this.RefreshCollision();

  public virtual void RefreshCollision(bool throwing = false)
  {
    this.handlerArmGrabbed = false;
    this.leftPlayerHand = (PlayerHand) null;
    this.rightPlayerHand = (PlayerHand) null;
    this.leftNpcHand = (RagdollHand) null;
    this.rightNpcHand = (RagdollHand) null;
    this.ResetRagdollCollision();
    this.ResetObjectCollision();
    int count1 = this.handlers.Count;
    for (int index = 0; index < count1; ++index)
    {
      RagdollHand handler = this.handlers[index];
      if ((bool) (UnityEngine.Object) handler.playerHand)
      {
        if (handler.playerHand.side == Side.Left)
          this.leftPlayerHand = handler.playerHand;
        else
          this.rightPlayerHand = handler.playerHand;
      }
      else if (handler.side == Side.Left)
        this.leftNpcHand = handler;
      else
        this.rightNpcHand = handler;
    }
    int count2 = this.tkHandlers.Count;
    for (int index = 0; index < count2; ++index)
    {
      SpellCaster tkHandler = this.tkHandlers[index];
      if ((bool) (UnityEngine.Object) tkHandler.ragdollHand.playerHand)
      {
        if (tkHandler.ragdollHand.playerHand.side == Side.Left)
          this.leftPlayerHand = tkHandler.ragdollHand.playerHand;
        else
          this.rightPlayerHand = tkHandler.ragdollHand.playerHand;
      }
      else if (tkHandler.ragdollHand.side == Side.Left)
        this.leftNpcHand = tkHandler.ragdollHand;
      else
        this.rightNpcHand = tkHandler.ragdollHand;
    }
    if ((bool) (UnityEngine.Object) Player.local?.handLeft.ragdollHand && (UnityEngine.Object) Player.local.handLeft.ragdollHand.climb.gripItem == (UnityEngine.Object) this)
      this.leftPlayerHand = Player.local.handLeft;
    if ((bool) (UnityEngine.Object) Player.local?.handRight.ragdollHand && (UnityEngine.Object) Player.local.handRight.ragdollHand.climb.gripItem == (UnityEngine.Object) this)
      this.rightPlayerHand = Player.local.handRight;
    if ((bool) (UnityEngine.Object) this.rightPlayerHand || (bool) (UnityEngine.Object) this.leftPlayerHand)
    {
      RagdollPart.Type ignoredParts1 = RagdollPart.Type.LeftFoot | RagdollPart.Type.RightFoot;
      if (this.data.playerGrabAndGripChangeLayer)
        this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
      if (this.isTelekinesisGrabbed)
      {
        if ((bool) (UnityEngine.Object) this.rightPlayerHand || (bool) (UnityEngine.Object) this.leftPlayerHand)
        {
          if ((bool) (UnityEngine.Object) this.rightPlayerHand && (bool) (UnityEngine.Object) this.leftPlayerHand)
            this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand | ignoredParts1);
          else if ((bool) (UnityEngine.Object) this.rightPlayerHand)
          {
            this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.LeftHand | ignoredParts1);
          }
          else
          {
            if (!(bool) (UnityEngine.Object) this.leftPlayerHand)
              return;
            this.IgnoreRagdollCollision(this.leftPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.RightHand | ignoredParts1);
          }
        }
        else
        {
          if (!(bool) (UnityEngine.Object) this.rightNpcHand && !(bool) (UnityEngine.Object) this.leftNpcHand)
            return;
          if ((bool) (UnityEngine.Object) this.rightNpcHand && (bool) (UnityEngine.Object) this.leftNpcHand)
            this.IgnoreRagdollCollision(this.rightNpcHand.creature.ragdoll, RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand);
          else if ((bool) (UnityEngine.Object) this.rightNpcHand)
          {
            this.IgnoreRagdollCollision(this.rightNpcHand.creature.ragdoll, RagdollPart.Type.LeftHand);
          }
          else
          {
            if (!(bool) (UnityEngine.Object) this.leftNpcHand)
              return;
            this.IgnoreRagdollCollision(this.leftNpcHand.creature.ragdoll, RagdollPart.Type.RightHand);
          }
        }
      }
      else if (Player.selfCollision)
      {
        RagdollPart.Type type1 = RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand;
        RagdollPart.Type type2 = RagdollPart.Type.RightArm | RagdollPart.Type.RightHand;
        RagdollPart.Type ignoredParts2 = ~(type1 | type2);
        if ((bool) (UnityEngine.Object) this.rightPlayerHand && (bool) (UnityEngine.Object) this.leftPlayerHand)
          this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, ignoredParts2);
        else if ((bool) (UnityEngine.Object) this.rightPlayerHand)
          this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, type1 | ignoredParts2);
        else if ((bool) (UnityEngine.Object) this.leftPlayerHand)
          this.IgnoreRagdollCollision(this.leftPlayerHand.ragdollHand.creature.ragdoll, type2 | ignoredParts2);
        if (!this.isPenetrating)
          return;
        int count3 = this.collisionHandlers.Count;
        for (int index1 = 0; index1 < count3; ++index1)
        {
          CollisionHandler collisionHandler = this.collisionHandlers[index1];
          for (int index2 = collisionHandler.collisions.Length - 1; index2 >= 0; --index2)
          {
            CollisionInstance collision = collisionHandler.collisions[index2];
            if (collision.active && collision.damageStruct.active && collision.damageStruct.penetration != DamageStruct.Penetration.None && (bool) (UnityEngine.Object) collision.damageStruct.hitRagdollPart && collision.damageStruct.hitRagdollPart.ragdoll.creature.isPlayer)
            {
              int count4 = collision.sourceColliderGroup.colliders.Count;
              for (int index3 = 0; index3 < count4; ++index3)
              {
                Collider collider = collision.sourceColliderGroup.colliders[index3];
                if ((bool) (UnityEngine.Object) collision.damageStruct.hitRagdollPart)
                  collision.damageStruct.hitRagdollPart.ragdoll.IgnoreCollision(collider, true);
                else
                  Physics.IgnoreCollision(collision.targetCollider, collider, true);
              }
            }
          }
        }
      }
      else if ((bool) (UnityEngine.Object) this.rightPlayerHand && (bool) (UnityEngine.Object) this.leftPlayerHand)
        this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, ignoredParts1);
      else if ((bool) (UnityEngine.Object) this.rightPlayerHand)
      {
        this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.LeftHand | ignoredParts1);
      }
      else
      {
        if (!(bool) (UnityEngine.Object) this.leftPlayerHand)
          return;
        this.IgnoreRagdollCollision(this.leftPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.RightHand | ignoredParts1);
      }
    }
    else if ((bool) (UnityEngine.Object) this.rightNpcHand || (bool) (UnityEngine.Object) this.leftNpcHand)
    {
      if ((bool) (UnityEngine.Object) this.rightNpcHand && (this.rightNpcHand.creature.state != Creature.State.Alive || this.rightNpcHand.creature.ragdoll.standingUp) || (bool) (UnityEngine.Object) this.leftNpcHand && (this.leftNpcHand.creature.state != Creature.State.Alive || this.leftNpcHand.creature.ragdoll.standingUp))
        this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
      else if ((bool) (UnityEngine.Object) this.rightNpcHand && (this.rightNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.RightHand).isGrabbed || this.rightNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.RightArm).isGrabbed))
      {
        this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
        this.handlerArmGrabbed = true;
      }
      else if ((bool) (UnityEngine.Object) this.leftNpcHand && (this.leftNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.LeftHand).isGrabbed || this.leftNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).isGrabbed))
      {
        this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
        this.handlerArmGrabbed = true;
      }
      else
      {
        RagdollHand ragdollHand = (bool) (UnityEngine.Object) this.rightNpcHand ? this.rightNpcHand : this.leftNpcHand;
        this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.ItemAndRagdollOnly));
        if (ragdollHand.ragdoll.allowSelfDamage)
          this.IgnoreRagdollCollision(ragdollHand.creature.ragdoll, (bool) (UnityEngine.Object) this.rightNpcHand ? RagdollPart.Type.RightHand : RagdollPart.Type.LeftHand);
        else
          this.IgnoreRagdollCollision(ragdollHand.creature.ragdoll);
        if (!(bool) (UnityEngine.Object) ragdollHand.otherHand?.grabbedHandle)
          return;
        this.IgnoreObjectCollision(ragdollHand.otherHand.grabbedHandle.item);
      }
    }
    else if ((bool) (UnityEngine.Object) this.holder)
    {
      this.ResetRagdollCollision();
      this.ResetObjectCollision();
      this.SetColliderAndMeshLayer(GameManager.GetLayer(throwing ? LayerName.MovingItem : LayerName.DroppedItem));
    }
    else
      this.SetColliderAndMeshLayer(GameManager.GetLayer(throwing ? LayerName.MovingItem : LayerName.DroppedItem));
  }

  public virtual void IgnoreRagdollCollision(Ragdoll ragdoll)
  {
    this.ResetRagdollCollision();
    int count1 = this.colliderGroups.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ColliderGroup colliderGroup = this.colliderGroups[index1];
      int count2 = colliderGroup.colliders.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        Collider collider = colliderGroup.colliders[index2];
        ragdoll.IgnoreCollision(collider, true);
      }
    }
    this.ignoredRagdoll = ragdoll;
  }

  public virtual void IgnoreRagdollCollision(Ragdoll ragdoll, RagdollPart.Type ignoredParts)
  {
    if (this.ignoreRagdollCollisionRoutine != null)
    {
      this.StopCoroutine(this.ignoreRagdollCollisionRoutine);
      this.ignoreRagdollCollisionRoutine = (Coroutine) null;
    }
    int count1 = this.colliderGroups.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ColliderGroup colliderGroup = this.colliderGroups[index1];
      int count2 = colliderGroup.colliders.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        Collider collider = colliderGroup.colliders[index2];
        ragdoll.IgnoreCollision(collider, true, ignoredParts);
      }
    }
    Item.IgnoreRagdollCollisionEvent ragdollCollision = this.OnIgnoreRagdollCollision;
    if (ragdollCollision != null)
      ragdollCollision(this, ragdoll, true, ignoredParts);
    this.ignoredRagdoll = ragdoll;
  }

  public virtual void IgnoreRagdollCollision(Ragdoll ragdoll, float duration)
  {
    if (this.ignoreRagdollCollisionRoutine != null)
      this.StopCoroutine(this.ignoreRagdollCollisionRoutine);
    this.ignoreRagdollCollisionRoutine = this.StartCoroutine(this.IgnoreRagdollCollisionRoutine(ragdoll, duration));
  }

  public IEnumerator IgnoreRagdollCollisionRoutine(Ragdoll ragdoll, float duration)
  {
    this.IgnoreRagdollCollision(ragdoll);
    yield return (object) new WaitForSeconds(duration);
    this.ResetRagdollCollision();
  }

  public virtual void ResetRagdollCollision()
  {
    if (!(bool) (UnityEngine.Object) this.ignoredRagdoll)
      return;
    if (this.ignoreRagdollCollisionRoutine != null)
    {
      this.StopCoroutine(this.ignoreRagdollCollisionRoutine);
      this.ignoreRagdollCollisionRoutine = (Coroutine) null;
    }
    int count1 = this.colliderGroups.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ColliderGroup colliderGroup = this.colliderGroups[index1];
      int count2 = colliderGroup.colliders.Count;
      for (int index2 = 0; index2 < count2; ++index2)
        this.ignoredRagdoll.IgnoreCollision(colliderGroup.colliders[index2], false);
    }
    Item.IgnoreRagdollCollisionEvent ragdollCollision = this.OnIgnoreRagdollCollision;
    if (ragdollCollision != null)
      ragdollCollision(this, this.ignoredRagdoll, false, (RagdollPart.Type) 0);
    this.ignoredRagdoll = (Ragdoll) null;
  }

  public virtual void IgnoreObjectCollision(Item item)
  {
    this.ResetObjectCollision();
    this.IgnoreItemCollision(item);
    this.ignoredItem = item;
  }

  public virtual void IgnoreItemCollision(Item item, bool ignore = true)
  {
    int count1 = this.colliderGroups.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ColliderGroup colliderGroup1 = this.colliderGroups[index1];
      int count2 = colliderGroup1.colliders.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        Collider collider1 = colliderGroup1.colliders[index2];
        int count3 = item.colliderGroups.Count;
        for (int index3 = 0; index3 < count3; ++index3)
        {
          ColliderGroup colliderGroup2 = item.colliderGroups[index3];
          int count4 = colliderGroup2.colliders.Count;
          for (int index4 = 0; index4 < count4; ++index4)
          {
            Collider collider2 = colliderGroup2.colliders[index4];
            Physics.IgnoreCollision(collider1, collider2, ignore);
          }
        }
      }
    }
    Item.IgnoreItemCollisionEvent ignoreItemCollision = this.OnIgnoreItemCollision;
    if (ignoreItemCollision == null)
      return;
    ignoreItemCollision(this, item, ignore);
  }

  public virtual void ResetObjectCollision()
  {
    if (!(bool) (UnityEngine.Object) this.ignoredItem)
      return;
    this.IgnoreItemCollision(this.ignoredItem, false);
    this.ignoredItem = (Item) null;
  }

  public virtual void IgnoreColliderCollision(Collider targetCollider)
  {
    this.ResetColliderCollision();
    int count1 = this.colliderGroups.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ColliderGroup colliderGroup = this.colliderGroups[index1];
      int count2 = colliderGroup.colliders.Count;
      for (int index2 = 0; index2 < count2; ++index2)
        Physics.IgnoreCollision(colliderGroup.colliders[index2], targetCollider, true);
    }
    Item.IgnoreColliderCollisionEvent colliderCollision = this.OnIgnoreColliderCollision;
    if (colliderCollision != null)
      colliderCollision(this, targetCollider, true);
    this.ignoredCollider = targetCollider;
  }

  public virtual void ResetColliderCollision()
  {
    if (!(bool) (UnityEngine.Object) this.ignoredCollider)
      return;
    int count1 = this.colliderGroups.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      ColliderGroup colliderGroup = this.colliderGroups[index1];
      int count2 = colliderGroup.colliders.Count;
      for (int index2 = 0; index2 < count2; ++index2)
        Physics.IgnoreCollision(colliderGroup.colliders[index2], this.ignoredCollider, false);
    }
    Item.IgnoreColliderCollisionEvent colliderCollision = this.OnIgnoreColliderCollision;
    if (colliderCollision != null)
      colliderCollision(this, this.ignoredCollider, true);
    this.ignoredCollider = (Collider) null;
  }

  public virtual bool CanPenetratePart(RagdollPart part)
  {
    return this.penetrateNotAllowedParts.Contains(part);
  }

  public virtual void PreventPenetration(RagdollPart part)
  {
    this.penetrateNotAllowedParts.Add(part);
  }

  public virtual void AllowPenetration(RagdollPart part)
  {
    this.penetrateNotAllowedParts.Remove(part);
  }

  public virtual bool FullyUnpenetrate()
  {
    if (!this.isPenetrating)
      return false;
    for (int index1 = 0; index1 < this.collisionHandlers.Count; ++index1)
    {
      CollisionHandler collisionHandler = this.collisionHandlers[index1];
      for (int index2 = 0; index2 < collisionHandler.damagers.Count; ++index2)
        collisionHandler.damagers[index2].UnPenetrateAll();
    }
    return true;
  }

  public void InvokeZoneEvent(Zone zone, bool enter)
  {
    Item.ZoneEvent onZoneEvent = this.OnZoneEvent;
    if (onZoneEvent != null)
      onZoneEvent(zone, enter);
    if (enter)
      this.zones.Add(zone);
    else
      this.zones.Remove(zone);
  }

  private void OnTriggerEnter(Collider other)
  {
    Creature component;
    if (other.gameObject.layer != GameManager.GetLayer(LayerName.BodyLocomotion) || !other.gameObject.CompareTag("DefenseCollider") || !(bool) (UnityEngine.Object) this.lastHandler || this.lastHandler.creature.isPlayer || !((UnityEngine.Object) this.lastHandler.creature.brain.currentTarget != (UnityEngine.Object) null) || this.lastHandler.creature.brain.currentTarget.isPlayer || this.lastHandler.creature.brain.currentTarget.ragdoll.IsPhysicsEnabled() || !other.transform.parent.TryGetComponent<Creature>(out component) || !((UnityEngine.Object) component != (UnityEngine.Object) this.lastHandler.creature) || component == null || component.ragdoll.IsPhysicsEnabled() || (component != null ? (component.state != 0 ? 1 : 0) : 1) == 0)
      return;
    Damager furthestDamager;
    Collider furthestCollider;
    this.GetFurthestDamagerCollider(out furthestDamager, out furthestCollider);
    RagdollPart closestPart;
    Collider closestCollider;
    int materialHash;
    component.ragdoll.GetClosestPartColliderAndMatHash(furthestCollider != null ? furthestCollider.transform.position : this.transform.position, out closestPart, out closestCollider, out materialHash, true);
    UnityEngine.Vector3 velocity = (double) this.physicBody.velocity.sqrMagnitude > 0.0 ? this.physicBody.velocity : this.mainCollisionHandler.lastLinearVelocity;
    int num = (int) CollisionInstance.FakeCollision(furthestDamager?.colliderGroup, closestPart.colliderGroup, furthestCollider, closestCollider, velocity, furthestCollider != null ? furthestCollider.transform.position : this.transform.position, closestCollider.transform.position, -velocity.normalized, new float?(), new int?(), new int?(materialHash), 1f, 0.1f);
  }

  private void OnDestroy()
  {
    if ((!this.despawned || this.fellOutOfBounds) && this._owner == Item.Owner.Player)
    {
      Debug.LogError((object) $"Item {this.itemId} on {this.gameObject.name} was lost, sending the item to player stash", (UnityEngine.Object) this.gameObject);
      Item.potentialLostItems.Add(new ItemContent(this, customDataList: this.contentCustomData));
    }
    if (GameManager.isQuitting)
      return;
    Item.all.Remove(this);
    if (Item.allThrowed.Contains(this))
      Item.allThrowed.Remove(this);
    if (Item.allMoving.Contains(this))
      Item.allMoving.Remove(this);
    if (Item.allTk.Contains(this))
      Item.allTk.Remove(this);
    if (!Item.allWorldAttached.Contains(this))
      return;
    Item.allWorldAttached.Remove(this);
  }

  public virtual void InvokeTKSpinEvent(
    Handle held,
    bool spinning,
    EventTime eventTime,
    bool start)
  {
    if (start && this.OnTKSpinStart != null)
    {
      this.OnTKSpinStart(held, spinning, eventTime);
    }
    else
    {
      if (this.OnTKSpinEnd == null)
        return;
      this.OnTKSpinEnd(held, spinning, eventTime);
    }
  }

  /// <summary>
  /// Invoked when the TK held item is repeled away from the player
  /// </summary>
  /// <param name="handle"></param>
  /// <param name="spellTelekinesis"></param>
  /// 
  ///             /// <param name="eventTime"></param>
  public virtual void InvokeOnTKRepel(
    Handle handle,
    SpellTelekinesis spellTelekinesis,
    EventTime eventTime)
  {
    Item.TelekinesisTemporalDelegate telekinesisRepelEvent = this.OnTelekinesisRepelEvent;
    if (telekinesisRepelEvent == null)
      return;
    telekinesisRepelEvent(handle, spellTelekinesis, eventTime);
  }

  /// <summary>
  /// Invoked when the TK held item is pulled towards the player
  /// </summary>
  /// <param name="handle"></param>
  /// <param name="spellTelekinesis"></param>
  /// <param name="eventTime"></param>
  public virtual void InvokeOnTKPull(
    Handle handle,
    SpellTelekinesis spellTelekinesis,
    EventTime eventTime)
  {
    Item.TelekinesisTemporalDelegate telekinesisPullEvent = this.OnTelekinesisPullEvent;
    if (telekinesisPullEvent == null)
      return;
    telekinesisPullEvent(handle, spellTelekinesis, eventTime);
  }

  /// <summary>
  /// Invoke the OnContainerAddEvent event from another class
  /// </summary>
  /// <param name="container">Container in which the item was added</param>
  public virtual void InvokeOnContainerAddEvent(Container container)
  {
    Item.ContainerEvent containerAddEvent = this.OnContainerAddEvent;
    if (containerAddEvent == null)
      return;
    containerAddEvent(container);
  }

  public virtual void InvokeBreakStartEvent(Breakable breakable)
  {
    Item.BreakStartDelegate onBreakStart = this.OnBreakStart;
    if (onBreakStart == null)
      return;
    onBreakStart(breakable);
  }

  public void OnSpawn(List<ContentCustomData> contentCustomDataList, Item.Owner owner)
  {
    if (contentCustomDataList != null)
      this.OverrideCustomData(contentCustomDataList);
    this._owner = owner;
    this.spawnTime = this.lastInteractionTime = Time.time;
    this.damageMultiplier?.Clear();
    this.sliceAngleMultiplier?.Clear();
    this.pushLevelMultiplier?.Clear();
    this.CheckCurrentArea();
    Item.InvokeOnItemSpawn(this);
    this.InvokeOnSpawnEvent(EventTime.OnEnd);
  }

  private void InvokeOnSpawnEvent(EventTime eventTime)
  {
    if (this.OnSpawnEvent == null)
      return;
    foreach (Delegate invocation in this.OnSpawnEvent.GetInvocationList())
    {
      if (invocation is Item.SpawnEvent spawnEvent)
      {
        try
        {
          spawnEvent(eventTime);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during Item OnSpawnEvent: {ex}");
        }
      }
    }
  }

  private void InvokeOnDespawnEvent(EventTime eventTime)
  {
    if (this.OnDespawnEvent == null)
      return;
    foreach (Delegate invocation in this.OnDespawnEvent.GetInvocationList())
    {
      if (invocation is Item.SpawnEvent spawnEvent)
      {
        try
        {
          spawnEvent(eventTime);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error during Item OnDespawnEvent: {ex}");
        }
      }
    }
  }

  public void Despawn(float delay)
  {
    if ((double) delay > 0.0 && !this.IsInvoking(nameof (Despawn)))
      this.Invoke(nameof (Despawn), delay);
    else
      this.Despawn();
  }

  [ContextMenu("Despawn")]
  public override void Despawn()
  {
    base.Despawn();
    if ((UnityEngine.Object) this == (UnityEngine.Object) null)
      return;
    this.despawning = true;
    Item.InvokeOnItemDespawn(this);
    this.InvokeOnDespawnEvent(EventTime.OnStart);
    if (this.currentArea != null)
    {
      if (this.currentArea.IsSpawned)
        this.currentArea.SpawnedArea.UnRegisterItem(this);
      this.isRegistered = false;
      this.currentArea = (SpawnableArea) null;
    }
    this.SetCull(false);
    if ((bool) (UnityEngine.Object) this.audioContainerSnap)
      Catalog.ReleaseAsset<AudioContainer>(this.audioContainerSnap);
    if ((bool) (UnityEngine.Object) this.audioContainerInventory)
      Catalog.ReleaseAsset<AudioContainer>(this.audioContainerInventory);
    if ((bool) (UnityEngine.Object) this.holder)
      this.holder.UnSnap(this);
    for (int index = this.handlers.Count - 1; index >= 0; --index)
      this.handlers[index].UnGrab(false);
    if (this.isGripped)
    {
      for (int index = Creature.allActive.Count - 1; index >= 0; --index)
      {
        Creature creature = Creature.allActive[index];
        if ((UnityEngine.Object) creature.handRight.climb.gripItem == (UnityEngine.Object) this)
          creature.handRight.climb.UnGrip();
        if ((UnityEngine.Object) creature.handLeft.climb.gripItem == (UnityEngine.Object) this)
          creature.handLeft.climb.UnGrip();
      }
    }
    if (this.isTelekinesisGrabbed)
    {
      int count = this.handles.Count;
      for (int index = 0; index < count; ++index)
        this.handles[index].ReleaseAllTkHandlers();
    }
    int count1 = this.collisionHandlers.Count;
    for (int index = 0; index < count1; ++index)
      this.collisionHandlers[index].ClearPhysicModifiers();
    int count2 = this.collisionHandlers.Count;
    for (int index1 = 0; index1 < count2; ++index1)
    {
      CollisionHandler collisionHandler = this.collisionHandlers[index1];
      int count3 = collisionHandler.damagers.Count;
      for (int index2 = 0; index2 < count3; ++index2)
        collisionHandler.damagers[index2].UnPenetrateAll();
      for (int index3 = collisionHandler.penetratedObjects.Count - 1; index3 >= 0; --index3)
      {
        CollisionHandler penetratedObject = collisionHandler.penetratedObjects[index3];
        int count4 = penetratedObject.damagers.Count;
        for (int index4 = 0; index4 < count4; ++index4)
          penetratedObject.damagers[index4].UnPenetrateAll();
      }
    }
    int count5 = this.imbues.Count;
    for (int index = 0; index < count5; ++index)
      this.imbues[index]?.UnloadCurrentSpell();
    foreach (Holder componentsInChild in this.GetComponentsInChildren<Holder>())
    {
      for (int index = componentsInChild.items.Count - 1; index >= 0; --index)
        componentsInChild.items[index].Despawn();
    }
    foreach (Effect componentsInChild in this.GetComponentsInChildren<Effect>(true))
    {
      try
      {
        componentsInChild.Despawn();
      }
      catch (NullReferenceException ex)
      {
        Debug.LogError((object) $"Could not despawn item {this.name} (instance of {this.data.id}) because effect {componentsInChild} ({componentsInChild?.module?.rootData?.id}) despawn threw NRE.");
        Debug.LogException((Exception) ex);
      }
    }
    int count6 = this.revealDecals.Count;
    for (int index = 0; index < count6; ++index)
    {
      RevealDecal revealDecal = this.revealDecals[index];
      if ((bool) (UnityEngine.Object) revealDecal.revealMaterialController)
        revealDecal.revealMaterialController.Reset();
    }
    if (Item.allWorldAttached.Contains(this))
      Item.allWorldAttached.Remove(this);
    this.StopThrowing();
    this.StopFlying();
    if (this.physicBody != (PhysicBody) null)
      this.physicBody.velocity = UnityEngine.Vector3.zero;
    this.loaded = false;
    if (this.isPooled)
    {
      this.Hide(false);
      this.ReturnToPool();
    }
    else
    {
      this.gameObject.SetActive(false);
      if (this.addressableHandle.IsValid())
        Addressables.ReleaseInstance(this.addressableHandle);
      else
        UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
    }
    if (this.imbueDecreaseRoutine != null)
      this.StopCoroutine(this.imbueDecreaseRoutine);
    this.despawning = false;
    this.despawned = true;
    this.InvokeOnDespawnEvent(EventTime.OnEnd);
  }

  public void ReturnToPool()
  {
    this.transform.SetParent(ItemData.poolRoot);
    this.transform.localPosition = UnityEngine.Vector3.zero;
    this.transform.localRotation = Quaternion.identity;
    this.transform.localScale = UnityEngine.Vector3.one;
    this._owner = Item.Owner.None;
    this.gameObject.SetActive(false);
  }

  public Item.HolderPoint GetHolderPoint(string holderPoint)
  {
    Item.HolderPoint holderPoint1 = this.additionalHolderPoints.Find((Predicate<Item.HolderPoint>) (x => x.anchorName == holderPoint));
    if (holderPoint1 != null)
      return holderPoint1;
    if (!string.IsNullOrEmpty(holderPoint))
      Debug.LogWarning((object) $"HolderPoint {holderPoint} not found on item {this.name} : returning default HolderPoint.");
    return this.GetDefaultHolderPoint();
  }

  public ItemData.CustomSnap GetCustomSnap(string holderName)
  {
    foreach (ItemData.CustomSnap customSnap in this.customSnaps)
    {
      if (customSnap.holderName == holderName)
        return customSnap;
    }
    return (ItemData.CustomSnap) null;
  }

  public Item.HolderPoint GetDefaultHolderPoint()
  {
    return new Item.HolderPoint(this.holderPoint, "Default");
  }

  /// <summary>
  /// Assign the position and rotation of the item to the spawning cached values.
  /// Stops the physic body from moving.
  /// </summary>
  public void ResetToSpawningTransformation()
  {
    Transform transform = this.transform;
    transform.position = this.spawnPosition;
    transform.rotation = this.spawnRotation;
    if (this.spawnSkinnedBonesTransforms != null)
    {
      foreach (KeyValuePair<Transform, (UnityEngine.Vector3, Quaternion)> skinnedBonesTransform in this.spawnSkinnedBonesTransforms)
      {
        Transform key = skinnedBonesTransform.Key;
        if ((bool) (UnityEngine.Object) skinnedBonesTransform.Key)
        {
          (UnityEngine.Vector3 vector3, Quaternion quaternion) = skinnedBonesTransform.Value;
          key.position = vector3;
          key.rotation = quaternion;
        }
      }
    }
    if (!(bool) this.physicBody)
      return;
    this.physicBody.velocity = UnityEngine.Vector3.zero;
    this.physicBody.angularVelocity = UnityEngine.Vector3.zero;
  }

  public void SetPhysicModifier(
    object handler,
    float? gravityRatio = null,
    float massRatio = 1f,
    float drag = -1f,
    float angularDrag = -1f,
    float sleepThreshold = -1f,
    EffectData effectData = null)
  {
    int count = this.collisionHandlers.Count;
    for (int index = 0; index < count; ++index)
      this.collisionHandlers[index].SetPhysicModifier(handler, gravityRatio, massRatio, drag, angularDrag, sleepThreshold, effectData);
  }

  public void ClearPhysicModifiers()
  {
    int count = this.collisionHandlers.Count;
    for (int index = 0; index < count; ++index)
      this.collisionHandlers[index].ClearPhysicModifiers();
  }

  public new void RemovePhysicModifier(object handler)
  {
    int count = this.collisionHandlers.Count;
    for (int index = 0; index < count; ++index)
      this.collisionHandlers[index].RemovePhysicModifier(handler);
  }

  public void IgnoreIsMoving() => this.ignoreIsMoving = true;

  public void ClearZones()
  {
    List<Zone> list = LazyListPool<Zone>.Instance.Get(this.zones.Count);
    foreach (Zone zone in this.zones)
      list.Add(zone);
    foreach (Zone zone in list)
      zone.RemoveItem(this);
    LazyListPool<Zone>.Instance.Return(list);
  }

  public override void AddForce(UnityEngine.Vector3 force, ForceMode forceMode, CollisionHandler handler = null)
  {
    base.AddForce(force, forceMode);
    this.physicBody.AddForce(force, forceMode);
  }

  public override void AddRadialForce(
    float force,
    UnityEngine.Vector3 origin,
    float upwardsModifier,
    ForceMode forceMode,
    CollisionHandler handler = null)
  {
    base.AddRadialForce(force, origin, upwardsModifier, forceMode);
    this.physicBody.AddRadialForce(force, origin, upwardsModifier, forceMode);
  }

  public override void AddExplosionForce(
    float force,
    UnityEngine.Vector3 origin,
    float radius,
    float upwardsModifier,
    ForceMode forceMode,
    CollisionHandler handler = null)
  {
    base.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode);
    this.physicBody.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode);
  }

  public void InvokeOnImbuesChange(
    SpellData spellData,
    float amount,
    float change,
    EventTime time)
  {
    Item.ImbuesChangeEvent imbuesChangeEvent = this.OnImbuesChangeEvent;
    if (imbuesChangeEvent != null)
      imbuesChangeEvent();
    this.mainHandler?.creature?.UpdateHeldImbues();
  }

  public enum Owner
  {
    None,
    Player,
    Shopkeeper,
  }

  [Serializable]
  public class IconMarker
  {
    public string damagerId;
    public UnityEngine.Vector2 position;
    public float directionAngle;
    public Damager.Direction direction;

    public IconMarker(
      string damagerId,
      UnityEngine.Vector2 position,
      Damager.Direction direction,
      float directionAngle)
    {
      this.damagerId = damagerId;
      this.position = position;
      this.direction = direction;
      this.directionAngle = directionAngle;
    }
  }

  public delegate void CullEvent(bool culled);

  public delegate void SpawnEvent(EventTime eventTime);

  public delegate void ImbuesChangeEvent();

  public delegate void ContainerEvent(Container container);

  public delegate void ZoneEvent(Zone zone, bool enter);

  public delegate void DamageReceivedDelegate(CollisionInstance collisionInstance);

  public delegate void GrabDelegate(Handle handle, RagdollHand ragdollHand);

  public delegate void ReleaseDelegate(Handle handle, RagdollHand ragdollHand, bool throwing);

  public delegate void HolderDelegate(Holder holder);

  public delegate void ThrowingDelegate(Item item);

  public delegate void TelekinesisDelegate(Handle handle, SpellTelekinesis teleGrabber);

  public delegate void TelekinesisReleaseDelegate(
    Handle handle,
    SpellTelekinesis teleGrabber,
    bool tryThrow,
    bool isGrabbing);

  public delegate void TelekinesisTemporalDelegate(
    Handle handle,
    SpellTelekinesis teleGrabber,
    EventTime eventTime);

  public delegate void TelekinesisSpinEvent(Handle held, bool spinning, EventTime eventTime);

  public delegate void TouchActionDelegate(
    RagdollHand ragdollHand,
    Interactable interactable,
    Interactable.Action action);

  public delegate void HeldActionDelegate(
    RagdollHand ragdollHand,
    Handle handle,
    Interactable.Action action);

  public delegate void MagnetDelegate(ItemMagnet itemMagnet, EventTime eventTime);

  public delegate void BreakStartDelegate(Breakable breakable);

  public delegate void LoadDelegate();

  public delegate void OverrideContentCustomDataEvent(List<ContentCustomData> contentCustomData);

  public delegate void IgnoreRagdollCollisionEvent(
    Item item,
    Ragdoll ragdoll,
    bool ignore,
    RagdollPart.Type ignoredParts);

  public delegate void IgnoreItemCollisionEvent(Item item, Item other, bool ignore);

  public delegate void IgnoreColliderCollisionEvent(Item item, Collider other, bool ignore);

  public delegate void SetCollidersEvent(Item item, bool active, bool force);

  public delegate void SetColliderLayerEvent(Item item, int layer);

  public enum FlyDetection
  {
    Disabled,
    CheckAngle,
    Forced,
  }

  [Serializable]
  public class HolderPoint
  {
    public Transform anchor;
    public string anchorName;

    public HolderPoint(Transform t, string s)
    {
      this.anchor = t;
      this.anchorName = s;
    }
  }
}
