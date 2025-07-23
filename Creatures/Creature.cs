// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Creature
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Manikin;
using ThunderRoad.Skill;
using UnityEngine;
using UnityEngine.Rendering;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Creatures/Creature")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Creature.html")]
public class Creature : ThunderEntity
{
  public static string secondWindId = "SecondWind";
  [Tooltip("The Creature ID of the creature.")]
  public string creatureId;
  [Tooltip("The animator the creature will use for animations")]
  public Animator animator;
  [Tooltip("The LOD Group for the creature meshes, if it has one.")]
  public LODGroup lodGroup;
  [Tooltip("The container the creature uses for its parts.")]
  public Container container;
  [Tooltip("The transform used for eye rotation")]
  public Transform centerEyes;
  [Tooltip("The offset used for the eye camera")]
  public UnityEngine.Vector3 eyeCameraOffset;
  [Tooltip("If the creature use a VFX renderer, put it here.")]
  public Renderer vfxRenderer;
  [NonSerialized]
  public Ragdoll ragdoll;
  [NonSerialized]
  public Brain brain;
  [NonSerialized]
  public Locomotion locomotion;
  [NonSerialized]
  public RagdollHand handLeft;
  [NonSerialized]
  public RagdollHand handRight;
  [NonSerialized]
  public Equipment equipment;
  [NonSerialized]
  public Mana mana;
  [NonSerialized]
  public FeetClimber climber;
  [NonSerialized]
  public RagdollFoot footLeft;
  [NonSerialized]
  public RagdollFoot footRight;
  [NonSerialized]
  public LightVolumeReceiver lightVolumeReceiver;
  [NonSerialized]
  public bool wasLoadedForCharacterSelect;
  [NonSerialized]
  public HashSet<string> heldCrystalImbues;
  [Tooltip("References the class to tell in-game skills if the player is airborne.")]
  public AirHelper airHelper;
  [Tooltip("References the class for armor SFX")]
  public ArmorSFX armorSFX;
  protected float waterEyesEnterUnderwaterTime;
  protected float waterLastDrowningTime;
  [NonSerialized]
  public bool eyesUnderwater;
  [Tooltip("Reference the jaw bone for creature speaking")]
  [Header("Speak")]
  public Transform jaw;
  [Tooltip("Max rotation of the jaw when it speaks.")]
  public UnityEngine.Vector3 jawMaxRotation = new UnityEngine.Vector3(0.0f, -30f, 0.0f);
  [Tooltip("When enabled, the creature blinks")]
  [Header("Head")]
  public bool autoEyeClipsActive = true;
  [Tooltip("Reference the eyes for blinking")]
  public List<CreatureEye> allEyes = new List<CreatureEye>();
  [Tooltip("Reference the eye animation clips")]
  public List<CreatureData.EyeClip> eyeClips = new List<CreatureData.EyeClip>();
  [Tooltip("Reference the meshes to hide when in first person.")]
  public List<SkinnedMeshRenderer> meshesToHideForFPV;
  [Tooltip("The height off the ground the creature needs to be before they play the fall animation")]
  [Header("Fall")]
  public float fallAliveAnimationHeight = 0.5f;
  [Tooltip("The height the creautre needs to fall before their ragdoll distabilizes")]
  public float fallAliveDestabilizeHeight = 3f;
  [Tooltip("The maximum velocity for the creatures body before it can stand up")]
  public float groundStabilizationMaxVelocity = 1f;
  [Tooltip("The minimum duration a creature is on the ground before getting up.")]
  public float groundStabilizationMinDuration = 3f;
  [Range(0.0f, 1f)]
  [Tooltip("How submerged the creature needs to be before they can start the swimming animations.")]
  public float swimFallAnimationRatio = 0.6f;
  [Tooltip("Toggle T Pose for the creature.")]
  public bool toogleTPose;
  [Header("Movement")]
  public bool stepEnabled;
  public float stepThreshold = 0.2f;
  public bool turnRelativeToHand = true;
  public float headMinAngle = 30f;
  public float headMaxAngle = 80f;
  public float handToBodyRotationMaxVelocity = 2f;
  public float handToBodyRotationMaxAngle = 30f;
  public float turnSpeed = 6f;
  public float ikLocomotionSpeedThreshold = 1f;
  public float ikLocomotionAngularSpeedThreshold = 30f;
  public FloatHandler detectionFOVModifier;
  public FloatHandler hitEnvironmentDamageModifier;
  public FloatHandler healthModifier;
  public static int hashDynamicOneShot;
  public static int hashDynamicLoop;
  public static int hashDynamicLoopAdd;
  public static int hashDynamicLoop3;
  public static int hashDynamicInterrupt;
  public static int hashDynamicSpeedMultiplier;
  public static int hashDynamicMirror;
  public static int hashDynamicUpperOneShot;
  public static int hashDynamicUpperLoop;
  public static int hashDynamicUpperMultiplier;
  public static int hashDynamicUpperMirror;
  public static int hashExitDynamic;
  public static int hashInvokeCallback;
  public static int hashIsBusy;
  public static int hashFeminity;
  public static int hashHeight;
  public static int hashFalling;
  public static int hashUnderwater;
  public static int hashGetUp;
  public static int hashTstance;
  public static int hashStaticIdle;
  public static int hashFreeHands;
  public static bool hashInitialized;
  public Creature.FallState fallState;
  [NonSerialized]
  public Morphology morphology;
  protected float groundStabilizeDuration;
  [NonSerialized]
  public float groundStabilizationLastTime;
  [NonSerialized]
  public float turnTargetAngle;
  [NonSerialized]
  public UnityEngine.Vector3 stepTargetPos;
  [NonSerialized]
  public CollisionInstance lastDamage;
  [NonSerialized]
  public ManikinPartList manikinParts;
  [NonSerialized]
  public ManikinLocations manikinLocations;
  [NonSerialized]
  public ManikinProperties manikinProperties;
  [NonSerialized]
  public ManikinLocations.JsonWardrobeLocations orgWardrobeLocations;
  [NonSerialized]
  private List<ManikinPart> headManikinPart = new List<ManikinPart>(10);
  [NonSerialized]
  public Player player;
  [NonSerialized]
  public List<Holder> holders;
  [NonSerialized]
  public List<Creature.RendererData> renderers = new List<Creature.RendererData>();
  [NonSerialized]
  public List<RevealDecal> revealDecals = new List<RevealDecal>();
  [NonSerialized]
  public CreatureMouthRelay mouthRelay;
  public static List<Creature> all = new List<Creature>();
  public static List<Creature> allActive = new List<Creature>();
  public static Action<Creature> onAllActiveRemoved;
  [NonSerialized]
  public static Dictionary<string, AnimatorBundle> creatureAnimatorControllers = new Dictionary<string, AnimatorBundle>();
  [NonSerialized]
  public bool isPlayer;
  [NonSerialized]
  public bool hidden;
  [NonSerialized]
  public bool holsterItemsHidden;
  [NonSerialized]
  public HashSet<string> heldImbueIDs;
  public int factionId;
  public GameData.Faction faction;
  [NonSerialized]
  public CreatureData data;
  [NonSerialized]
  public bool pooled;
  [NonSerialized]
  public WaveData.Group spawnGroup;
  [NonSerialized]
  public CreatureSpawner creatureSpawner;
  [NonSerialized]
  public bool countTowardsMaxAlive;
  [NonSerialized]
  public float spawnTime;
  [NonSerialized]
  public float lastInteractionTime;
  [NonSerialized]
  public Creature lastInteractionCreature;
  [NonSerialized]
  public float swimVerticalRatio;
  [NonSerialized]
  public CreatureData.EthnicGroup currentEthnicGroup;
  public bool initialized;
  public bool loaded;
  public bool isPlayingDynamicAnimation;
  protected System.Action dynamicAnimationendEndCallback;
  protected System.Action upperDynamicAnimationendEndCallback;
  protected AnimatorOverrideController animatorOverrideController;
  protected KeyValuePair<AnimationClip, AnimationClip>[] animationClipOverrides;
  [NonSerialized]
  public bool updateReveal;
  public bool isKilled;
  public static Creature.ReplaceClipIndexHolder clipIndex = new Creature.ReplaceClipIndexHolder();
  [NonSerialized]
  public bool isSwimming;
  public float animationDampTime = 0.1f;
  public float verticalDampTime = 0.5f;
  public float stationaryVelocityThreshold = 0.01f;
  public float turnAnimSpeed = 0.007f;
  public static int hashStrafe;
  public static int hashTurn;
  public static int hashSpeed;
  public static int hashVerticalSpeed;
  [NonSerialized]
  public SpawnableArea initialArea;
  [NonSerialized]
  public int areaSpawnerIndex = -1;
  [NonSerialized]
  public SpawnableArea currentArea;
  [NonSerialized]
  public bool isCulled;
  protected bool cullingDetectionEnabled;
  protected float cullingDetectionCycleSpeed = 1f;
  protected float cullingDetectionCycleTime;
  [Header("Health")]
  public float _currentHealth = 50f;
  [Header("Health")]
  public float _currentMaxHealth = 50f;
  public float resurrectMinHeal = 5f;
  public static bool meshRaycast = true;
  [NonSerialized]
  public float lastDamageTime;
  public Dictionary<object, float> damageMultipliers;
  private Dictionary<object, (float position, float rotation)> jointForceMultipliers;
  private float jointPosForceMult = 1f;
  private float jointRotForceMult = 1f;

  public Locomotion currentLocomotion => !this.isPlayer ? this.locomotion : this.player.locomotion;

  public event System.Action onEyesEnterUnderwater;

  public event System.Action onEyesExitUnderwater;

  public event Creature.FallEvent OnFallEvent;

  public event Creature.ForceSkillLoadEvent OnForceSkillLoadEvent;

  public event Creature.ForceSkillLoadEvent OnForceSkillUnloadEvent;

  public event Creature.ImbueChangeEvent OnHeldImbueChange;

  public event Creature.DespawnEvent OnDespawnEvent;

  public event Creature.ThrowEvent OnThrowEvent;

  public event Creature.ThisCreatureAttackEvent OnThisCreatureAttackEvent;

  public bool canPlayDynamicAnimation => (UnityEngine.Object) this.animatorOverrideController != (UnityEngine.Object) null;

  public event Creature.ZoneEvent OnZoneEvent;

  public event Creature.SimpleDelegate OnDataLoaded;

  public event Creature.SimpleDelegate OnHeightChanged;

  public Creature.State state
  {
    get
    {
      if (this.isKilled)
        return Creature.State.Dead;
      return this.ragdoll.state == Ragdoll.State.Destabilized || this.ragdoll.state == Ragdoll.State.Inert ? Creature.State.Destabilized : Creature.State.Alive;
    }
  }

  public bool HasMetal
  {
    get
    {
      CreatureData data = this.data;
      if ((data != null ? (data.hasMetal ? 1 : 0) : 0) != 0)
        return true;
      Ragdoll ragdoll = this.ragdoll;
      return ragdoll != null && ragdoll.hasMetalArmor;
    }
  }

  public List<ValueDropdownItem<string>> GetAllCreatureID()
  {
    return Catalog.GetDropdownAllID(Category.Creature);
  }

  public List<ValueDropdownItem<int>> GetAllFactionID()
  {
    return Catalog.gameData == null ? (List<ValueDropdownItem<int>>) null : Catalog.gameData.GetFactions();
  }

  protected void Awake()
  {
    this.detectionFOVModifier = new FloatHandler();
    this.hitEnvironmentDamageModifier = new FloatHandler();
    this.healthModifier = new FloatHandler();
    this.healthModifier.OnChangeEvent += new ValueHandler<float>.ChangeEvent(this.OnMaxHealthModifierChangeEvent);
    foreach (SkinnedMeshRenderer componentsInChild in this.GetComponentsInChildren<SkinnedMeshRenderer>())
      componentsInChild.updateWhenOffscreen = true;
    if (!(bool) (UnityEngine.Object) this.lodGroup)
      this.lodGroup = this.GetComponentInChildren<LODGroup>();
    if (!string.IsNullOrEmpty(this.creatureId) && this.creatureId != "None")
      this.data = Catalog.GetData<CreatureData>(this.creatureId);
    Creature.all.Add(this);
    this.ragdoll = this.GetComponentInChildren<Ragdoll>();
    this.brain = this.GetComponentInChildren<Brain>();
    this.equipment = this.GetComponentInChildren<Equipment>();
    if (!(bool) (UnityEngine.Object) this.container)
      this.container = this.GetComponentInChildren<Container>();
    this.locomotion = this.GetComponent<Locomotion>();
    this.mana = this.GetComponent<Mana>();
    this.climber = this.GetComponentInChildren<FeetClimber>();
    this.airHelper = this.GetOrAddComponent<AirHelper>();
    this.holders = new List<Holder>((IEnumerable<Holder>) this.GetComponentsInChildren<Holder>());
    this.heldCrystalImbues = new HashSet<string>();
    this.heldImbueIDs = new HashSet<string>();
    this.jointForceMultipliers = new Dictionary<object, (float, float)>();
    this.damageMultipliers = new Dictionary<object, float>();
    foreach (RagdollHand componentsInChild in this.GetComponentsInChildren<RagdollHand>())
    {
      if (componentsInChild.side == Side.Right)
        this.handRight = componentsInChild;
      if (componentsInChild.side == Side.Left)
        this.handLeft = componentsInChild;
    }
    foreach (RagdollFoot componentsInChild in this.GetComponentsInChildren<RagdollFoot>())
    {
      if (componentsInChild.side == Side.Right)
        this.footRight = componentsInChild;
      if (componentsInChild.side == Side.Left)
        this.footLeft = componentsInChild;
    }
    this.lightVolumeReceiver = this.GetComponent<LightVolumeReceiver>();
    if (!(bool) (UnityEngine.Object) this.lightVolumeReceiver)
      this.lightVolumeReceiver = this.gameObject.AddComponent<LightVolumeReceiver>();
    this.lightVolumeReceiver.initRenderersOnStart = false;
    this.lightVolumeReceiver.addMaterialInstances = false;
    if (!Creature.hashInitialized)
      this.InitAnimatorHashs();
    this.manikinLocations = this.GetComponentInChildren<ManikinLocations>();
    if ((bool) (UnityEngine.Object) this.manikinLocations)
      this.orgWardrobeLocations = this.manikinLocations.ToJson();
    this.manikinParts = this.GetComponentInChildren<ManikinPartList>();
    this.manikinProperties = this.GetComponentInChildren<ManikinProperties>();
    this.mouthRelay = this.GetComponentInChildren<CreatureMouthRelay>();
    this.turnTargetAngle = this.transform.rotation.eulerAngles.y;
    this.stepTargetPos = this.transform.position;
    if ((bool) (UnityEngine.Object) this.locomotion)
      this.gameObject.layer = GameManager.GetLayer(LayerName.BodyLocomotion);
    this.InitLocomotionAnimation();
    this.stepEnabled = true;
    this.animator.keepAnimatorStateOnDisable = true;
    this.waterHandler = new WaterHandler(true, true);
    this.waterHandler.OnWaterExit += new WaterHandler.SimpleDelegate(this.OnWaterExit);
    if (!(bool) (UnityEngine.Object) this.equipment)
      return;
    this.equipment.onCreaturePartChanged += new Equipment.OnCreaturePartChanged(this.UpdateManikinAfterHeadChange);
  }

  protected void InitAnimatorHashs()
  {
    Creature.hashFeminity = Animator.StringToHash("Feminity");
    Creature.hashHeight = Animator.StringToHash("Height");
    Creature.hashFalling = Animator.StringToHash("Falling");
    Creature.hashUnderwater = Animator.StringToHash("Underwater");
    Creature.hashGetUp = Animator.StringToHash("GetUp");
    Creature.hashIsBusy = Animator.StringToHash("IsBusy");
    Creature.hashTstance = Animator.StringToHash("TStance");
    Creature.hashStaticIdle = Animator.StringToHash("StaticIdle");
    Creature.hashFreeHands = Animator.StringToHash("FreeHands");
    Creature.hashDynamicOneShot = Animator.StringToHash("DynamicOneShot");
    Creature.hashDynamicLoop = Animator.StringToHash("DynamicLoop");
    Creature.hashDynamicLoopAdd = Animator.StringToHash("DynamicLoopAdd");
    Creature.hashDynamicLoop3 = Animator.StringToHash("DynamicLoop3");
    Creature.hashDynamicInterrupt = Animator.StringToHash("DynamicInterrupt");
    Creature.hashDynamicSpeedMultiplier = Animator.StringToHash("DynamicSpeedMultiplier");
    Creature.hashDynamicMirror = Animator.StringToHash("DynamicMirror");
    Creature.hashDynamicUpperOneShot = Animator.StringToHash("UpperBodyDynamicOneShot");
    Creature.hashDynamicUpperLoop = Animator.StringToHash("UpperBodyDynamicLoop");
    Creature.hashDynamicUpperMultiplier = Animator.StringToHash("UpperBodyDynamicSpeed");
    Creature.hashDynamicUpperMirror = Animator.StringToHash("UpperBodyDynamicMirror");
    Creature.hashExitDynamic = Animator.StringToHash("ExitDynamic");
    Creature.hashInvokeCallback = Animator.StringToHash("InvokeCallback");
    Creature.hashInitialized = true;
  }

  public RagdollHand GetHand(Side side) => side == Side.Left ? this.handLeft : this.handRight;

  public RagdollFoot GetFoot(Side side) => side == Side.Left ? this.footLeft : this.footRight;

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update | ManagedLoops.LateUpdate;

  protected internal override void ManagedUpdate()
  {
    if (!this.initialized)
      return;
    this.UpdateFall();
    this.UpdateWater();
    this.UpdateReveal();
    this.CheckInvokeDynamicCallback();
    this.UpdateLocomotionAnimation();
    this.UpdateDynamicAnimation();
    this.UpdateFacialAnimation();
  }

  public void ApplyAnimatorController(AnimatorBundle animatorBundle)
  {
    this.animator.applyRootMotion = false;
    this.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    this.animatorOverrideController = new AnimatorOverrideController(animatorBundle.controller);
    this.animatorOverrideController.name = "OverrideForDynamicAnimation";
    this.animator.runtimeAnimatorController = (RuntimeAnimatorController) this.animatorOverrideController;
    if (Creature.clipIndex == null)
      Creature.clipIndex = new Creature.ReplaceClipIndexHolder();
    this.animationClipOverrides = new KeyValuePair<AnimationClip, AnimationClip>[Creature.clipIndex.count];
    int index = 0;
    foreach (AnimationClip allClip in animatorBundle.GetAllClips())
    {
      if ((UnityEngine.Object) allClip == (UnityEngine.Object) null)
        Debug.LogError((object) ("Clip is null in " + animatorBundle.name));
      this.animationClipOverrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(allClip, allClip);
      ++index;
    }
  }

  protected void ResetTPose()
  {
    foreach (SkeletonBone skeletonBone in this.animator.avatar?.humanDescription.skeleton)
    {
      foreach (HumanBodyBones humanBoneId in Enum.GetValues(typeof (HumanBodyBones)))
      {
        if (humanBoneId != HumanBodyBones.LastBone)
        {
          Transform boneTransform = this.animator.GetBoneTransform(humanBoneId);
          if ((UnityEngine.Object) boneTransform != (UnityEngine.Object) null && skeletonBone.name == boneTransform.name)
          {
            boneTransform.localPosition = skeletonBone.position;
            boneTransform.localRotation = skeletonBone.rotation;
          }
        }
      }
    }
  }

  protected void UpdateReveal()
  {
    if (!this.updateReveal)
      return;
    this.updateReveal = false;
    for (int index = 0; index < this.renderers.Count; ++index)
    {
      Creature.RendererData renderer1 = this.renderers[index];
      bool? nullable;
      int num1;
      if (renderer1 == null)
      {
        num1 = 0;
      }
      else
      {
        nullable = renderer1.revealDecal?.UpdateOvertime();
        bool flag = true;
        num1 = nullable.GetValueOrDefault() == flag & nullable.HasValue ? 1 : 0;
      }
      bool flag1 = num1 != 0 || this.updateReveal;
      Creature.RendererData renderer2 = this.renderers[index];
      int num2;
      if (renderer2 == null)
      {
        num2 = 0;
      }
      else
      {
        nullable = renderer2.splitReveal?.UpdateOvertime();
        bool flag2 = true;
        num2 = nullable.GetValueOrDefault() == flag2 & nullable.HasValue ? 1 : 0;
      }
      bool flag3 = num2 != 0 || this.updateReveal;
      this.updateReveal = flag1 | flag3;
    }
  }

  protected void CheckInvokeDynamicCallback()
  {
    if (!this.animator.GetBool(Creature.hashInvokeCallback))
      return;
    this.animator.SetBool(Creature.hashInvokeCallback, false);
    if (this.dynamicAnimationendEndCallback == null || this.animator.GetInteger(Creature.hashDynamicOneShot) != 0)
      return;
    this.dynamicAnimationendEndCallback();
    this.dynamicAnimationendEndCallback = (System.Action) null;
  }

  public virtual float GetAnimatorHeightRatio() => this.animator.GetFloat(Creature.hashHeight);

  public void Heal(float healing) => this.Heal(healing, this);

  public bool TryAddSkill(string id) => this.TryAddSkill(Catalog.GetData<SkillData>(id));

  public bool TryRemoveSkill(string id) => this.TryRemoveSkill(Catalog.GetData<SkillData>(id));

  public void ResurrectMaxHealth() => this.Resurrect(this.maxHealth, this);

  public void Resurrect(float healing) => this.Resurrect(healing, this);

  public void SetFaction(int factionId)
  {
    this.factionId = factionId;
    this.faction = Catalog.gameData.GetFaction(factionId);
    if (this.faction != null)
      return;
    Debug.LogError((object) $"Faction ID {factionId.ToString()} could not be found for creature {this.data.id}");
    this.faction = Catalog.gameData.factions[0];
    this.factionId = Catalog.gameData.factions[0].id;
  }

  public void Damage(float amount) => this.Damage(amount, DamageType.Energy);

  public void Kill()
  {
    this.Kill(new CollisionInstance(new DamageStruct(DamageType.Energy, 99999f)));
  }

  public void Despawn(float delay)
  {
    if ((double) delay > 0.0 && !this.IsInvoking(nameof (Despawn)))
      this.Invoke(nameof (Despawn), delay);
    else
      this.Despawn();
  }

  public override void Despawn()
  {
    base.Despawn();
    EventManager.InvokeCreatureDespawn(this, EventTime.OnStart);
    if (this.OnDespawnEvent != null)
      this.OnDespawnEvent(EventTime.OnStart);
    if (this.currentArea != null && this.currentArea.IsSpawned)
      this.currentArea.SpawnedArea.UnRegisterCreature(this);
    this.ClearMultipliers();
    this.currentArea = (SpawnableArea) null;
    this.isCulled = false;
    if (this.initialArea != null && this.initialArea.isCreatureSpawnedExist != null && this.areaSpawnerIndex >= 0 && this.areaSpawnerIndex < this.initialArea.isCreatureSpawnedExist.Length)
      this.initialArea.isCreatureSpawnedExist[this.areaSpawnerIndex] = false;
    if ((bool) (UnityEngine.Object) this.brain)
      this.brain.Stop();
    if (!this.wasLoadedForCharacterSelect)
      this.UnloadSkills();
    if ((bool) (UnityEngine.Object) this.mana)
    {
      this.mana.casterLeft.UnloadSpell();
      this.mana.casterRight.UnloadSpell();
    }
    this.creatureSpawner = (CreatureSpawner) null;
    if ((bool) (UnityEngine.Object) this.player)
      this.player.ReleaseCreature();
    for (int index = 0; index < this.revealDecals.Count; ++index)
      this.revealDecals[index]?.Reset();
    if ((bool) (UnityEngine.Object) this.ragdoll && this.ragdoll.parts != null)
    {
      int count1 = this.ragdoll.parts.Count;
      for (int index1 = 0; index1 < count1; ++index1)
      {
        RagdollPart part = this.ragdoll.parts[index1];
        int count2 = part.handles.Count;
        for (int index2 = 0; index2 < count2; ++index2)
        {
          HandleRagdoll handle = part.handles[index2];
          for (int index3 = handle.handlers.Count - 1; index3 >= 0; --index3)
            handle.handlers[index3].UnGrab(false);
        }
        for (int index4 = part.collisionHandler.penetratedObjects.Count - 1; index4 >= 0; --index4)
        {
          foreach (Damager damager in part.collisionHandler.penetratedObjects[index4].damagers)
            damager.UnPenetrateAll();
        }
      }
    }
    if ((bool) (UnityEngine.Object) this.equipment)
      this.equipment.OnDespawn();
    if ((bool) (UnityEngine.Object) this.ragdoll)
      this.ragdoll.OnDespawn();
    foreach (Creature.RendererData renderer in this.renderers)
    {
      renderer.splitRenderer = (SkinnedMeshRenderer) null;
      if ((bool) (UnityEngine.Object) renderer.revealDecal)
        renderer.revealDecal.Reset();
    }
    foreach (Effect componentsInChild in this.GetComponentsInChildren<Effect>(true))
      componentsInChild.Despawn();
    Creature.allActive.Remove(this);
    if (Creature.onAllActiveRemoved != null)
      Creature.onAllActiveRemoved(this);
    this.turnRelativeToHand = true;
    this.isKilled = false;
    this.spawnGroup = (WaveData.Group) null;
    this.loaded = false;
    this.animator.keepAnimatorStateOnDisable = false;
    if (this.pooled)
    {
      if (this.data.removeMeshWhenPooled && (bool) (UnityEngine.Object) this.manikinLocations)
      {
        this.manikinLocations.RemoveAll();
        this.manikinLocations.UpdateParts();
      }
      this.Hide(false);
      CreatureData.ReturnToPool(this);
    }
    else
    {
      if ((bool) (UnityEngine.Object) this.manikinLocations)
      {
        this.manikinLocations.RemoveAll();
        this.manikinLocations.UpdateParts();
      }
      this.gameObject.SetActive(false);
      Catalog.ReleaseAsset<GameObject>(this.gameObject);
    }
    Creature.DespawnEvent onDespawnEvent = this.OnDespawnEvent;
    if (onDespawnEvent != null)
      onDespawnEvent(EventTime.OnEnd);
    EventManager.InvokeCreatureDespawn(this, EventTime.OnEnd);
  }

  public static int CompareByLastInteractionTime(Creature c1, Creature c2)
  {
    return c1.lastInteractionTime.CompareTo(c2.lastInteractionTime);
  }

  protected override void ManagedOnEnable()
  {
    base.ManagedOnEnable();
    Creature.allActive.Add(this);
    if ((UnityEngine.Object) AreaManager.Instance != (UnityEngine.Object) null)
      this.cullingDetectionEnabled = true;
    if ((bool) (UnityEngine.Object) this.ragdoll)
      this.ragdoll.OnCreatureEnable();
    if (!(bool) (UnityEngine.Object) this.brain)
      return;
    this.brain.OnCreatureEnable();
  }

  protected override void ManagedOnDisable()
  {
    base.ManagedOnDisable();
    if (GameManager.isQuitting)
      return;
    Creature.allActive.Remove(this);
    if (Creature.onAllActiveRemoved != null)
      Creature.onAllActiveRemoved(this);
    this.cullingDetectionEnabled = false;
    if ((bool) (UnityEngine.Object) this.brain)
      this.brain.OnCreatureDisable();
    if ((bool) (UnityEngine.Object) this.ragdoll)
      this.ragdoll.OnCreatureDisable();
    this.waterHandler.Reset();
  }

  private void OnDestroy() => Creature.all.Remove(this);

  protected override void Start()
  {
    base.Start();
    if (this.initialized)
      return;
    this.Init();
  }

  protected virtual void Init()
  {
    this.Load((EntityData) this.data);
    this.ragdoll.Init(this);
    if ((bool) (UnityEngine.Object) this.manikinParts)
    {
      this.manikinParts.disableRenderersDuringUpdate = true;
      this.manikinParts.UpdateParts_Completed += new ManikinPartList.UpdatePartsCompletedHandler(this.OnManikinChangedEvent);
    }
    else
      this.UpdateRenderers();
    this.InitCenterEyes();
    this.morphology = new Morphology(this.transform.InverseTransformPoint(this.centerEyes.position).y);
    if ((bool) (UnityEngine.Object) this.footRight)
      this.footRight.Init();
    if ((bool) (UnityEngine.Object) this.footLeft)
      this.footLeft.Init();
    this.RefreshMorphology();
    this.locomotion.SetCapsuleCollider(this.morphology.hipsHeight);
    if ((bool) (UnityEngine.Object) this.climber)
      this.climber.Init();
    if ((bool) (UnityEngine.Object) this.handRight)
      this.handRight.ResetGripPositionAndRotation();
    if ((bool) (UnityEngine.Object) this.handLeft)
      this.handLeft.ResetGripPositionAndRotation();
    if ((bool) (UnityEngine.Object) this.equipment)
      this.equipment.Init(this);
    if ((bool) (UnityEngine.Object) this.mana)
      this.mana.Init(this);
    if ((bool) (UnityEngine.Object) this.brain)
      this.brain.Init(this);
    this.autoEyeClipsActive = true;
    this.initialized = true;
    this.weakpoints = new List<Transform>();
  }

  public void InitCenterEyes()
  {
    if ((bool) (UnityEngine.Object) this.centerEyes)
      return;
    Transform boneTransform1 = this.animator.GetBoneTransform(HumanBodyBones.LeftEye);
    Transform boneTransform2 = this.animator.GetBoneTransform(HumanBodyBones.RightEye);
    if ((bool) (UnityEngine.Object) boneTransform1 && (bool) (UnityEngine.Object) boneTransform2)
    {
      this.centerEyes = new GameObject("CenterEyes").transform;
      this.centerEyes.SetParent(this.ragdoll.headPart.transform);
      this.centerEyes.position = (boneTransform1.position + boneTransform2.position) / 2f;
      this.centerEyes.rotation = this.transform.rotation;
    }
    else
      Debug.LogErrorFormat((UnityEngine.Object) this, "Cannot create centerEyes because HumanBodyBones LeftEye and RightEye are missing!");
  }

  public void SetSwimVertical(float ratio) => this.swimVerticalRatio = ratio;

  private void UpdateWater()
  {
    this.isSwimming = false;
    if (this.waterHandler != null)
    {
      if (Water.exist)
      {
        RagdollFoot footLeft = this.ragdoll.creature.footLeft;
        UnityEngine.Vector3 a = footLeft != null ? footLeft.transform.position : this.transform.position;
        RagdollFoot footRight = this.ragdoll.creature.footRight;
        UnityEngine.Vector3 b = footRight != null ? footRight.transform.position : this.transform.position;
        this.waterHandler.Update(UnityEngine.Vector3.Lerp(a, b, 0.5f), Mathf.Min(a.y, b.y), this.centerEyes.position.y, this.locomotion.colliderRadius, this.ragdoll.IsPhysicsEnabled() ? this.ragdoll.rootPart.physicBody.velocity : this.currentLocomotion.physicBody.velocity);
        if (this.waterHandler.inWater)
        {
          this.isSwimming = Catalog.gameData.water.swimmingEnabled && (double) this.waterHandler.submergedRatio >= (double) this.swimFallAnimationRatio;
          float dragMultiplier = this.data.waterLocomotionDragMultiplierCurve.Evaluate(this.waterHandler.submergedRatio);
          bool flag = !this.eyesUnderwater && (double) this.waterHandler.submergedRatio > 0.800000011920929;
          this.currentLocomotion.SetPhysicModifier((object) this, new float?(flag ? 0.0001f : Mathf.Lerp(1f, Catalog.gameData.water.minGravityLocomotion, this.waterHandler.submergedRatio)), dragMultiplier: dragMultiplier);
          this.currentLocomotion.SetSpeedModifier((object) this, jumpForceMultiplier: this.data.waterJumpForceMutiplierCurve.Evaluate(this.waterHandler.submergedRatio));
          if (this.isSwimming)
          {
            float num = Mathf.Clamp01(Mathf.Clamp01(this.waterHandler.submergedRatio - this.swimFallAnimationRatio) / Mathf.Clamp01(1f - this.swimFallAnimationRatio));
            if ((double) Mathf.Abs(this.swimVerticalRatio) > 0.0 && (!flag || (double) this.swimVerticalRatio < 0.0))
              this.currentLocomotion.physicBody.AddForce(UnityEngine.Vector3.up * (this.data.waterSwimUpForce * this.swimVerticalRatio * num * Time.deltaTime), ForceMode.VelocityChange);
          }
          if ((double) this.waterHandler.submergedRatio >= 1.0)
          {
            if (!this.eyesUnderwater)
            {
              this.waterEyesEnterUnderwaterTime = Time.time;
              this.eyesUnderwater = true;
              this.brain.isMuffled = true;
              System.Action eyesEnterUnderwater = this.onEyesEnterUnderwater;
              if (eyesEnterUnderwater != null)
                eyesEnterUnderwater();
            }
          }
          else if (this.eyesUnderwater)
          {
            this.eyesUnderwater = false;
            this.brain.isMuffled = this.CheckMuffled();
            System.Action eyesExitUnderwater = this.onEyesExitUnderwater;
            if (eyesExitUnderwater != null)
              eyesExitUnderwater();
          }
          if (this.state != Creature.State.Dead && this.eyesUnderwater && (!(bool) (UnityEngine.Object) this.player || !Player.invincibility) && (double) Time.time - (double) this.waterEyesEnterUnderwaterTime > (double) this.data.waterDrowningStartTime * (double) this.data.waterDrowningWarningRatio && (double) Time.time - (double) this.waterLastDrowningTime > (double) this.data.waterDrowningDamageInterval)
          {
            this.data.drownEffectData?.Spawn(this.jaw.position, this.jaw.rotation, this.jaw, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f)?.Play();
            if ((double) Time.time - (double) this.waterEyesEnterUnderwaterTime > (double) this.data.waterDrowningStartTime)
              this.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy, this.data.waterDrowningDamage)));
            this.waterLastDrowningTime = Time.time;
          }
        }
      }
      else
      {
        this.waterHandler.Reset();
        if (this.eyesUnderwater)
        {
          this.eyesUnderwater = false;
          this.brain.isMuffled = this.CheckMuffled();
          System.Action eyesExitUnderwater = this.onEyesExitUnderwater;
          if (eyesExitUnderwater != null)
            eyesExitUnderwater();
        }
      }
    }
    this.animator.SetBool(Creature.hashUnderwater, this.isSwimming);
  }

  private void OnWaterExit()
  {
    this.currentLocomotion.RemovePhysicModifier((object) this);
    this.currentLocomotion.RemoveSpeedModifier((object) this);
  }

  private bool CheckMuffled()
  {
    for (int index = 0; index < this.ragdoll.handlers.Count; ++index)
    {
      if (this.ragdoll.handlers[index].grabbedHandle is HandleRagdoll grabbedHandle && (grabbedHandle.handleRagdollData.behaviour == HandleRagdollData.Behaviour.Muffle || grabbedHandle.handleRagdollData.behaviour == HandleRagdollData.Behaviour.ChokeAndMuffle))
        return true;
    }
    return false;
  }

  private void ParentEyes()
  {
    foreach (CreatureEye allEye in this.allEyes)
    {
      foreach (CreatureEye.EyeMoveable eyePart in allEye.eyeParts)
        eyePart.ParentingFix();
    }
  }

  private void SetupManikin(PlayerSaveData playerCharacterData = null)
  {
    if (!(bool) (UnityEngine.Object) this.manikinProperties)
      Debug.LogWarning((object) ("ManikinProperties is missing on " + this.name));
    else if (playerCharacterData != null)
    {
      CreatureData.EthnicGroup ethnicGroupFromId = this.GetEthnicGroupFromId(playerCharacterData.customization.ethnicGroupId);
      this.SetEthnicGroup(ethnicGroupFromId);
      if ((double) playerCharacterData.customization.hairColor.a > 0.0)
      {
        this.SetColor(playerCharacterData.customization.hairColor, Creature.ColorModifier.Hair);
        this.SetColor(playerCharacterData.customization.hairSecondaryColor, Creature.ColorModifier.HairSecondary);
        this.SetColor(playerCharacterData.customization.hairSpecularColor, Creature.ColorModifier.HairSpecular);
      }
      else
      {
        List<UnityEngine.Color> hairColorsPrimary = ethnicGroupFromId.hairColorsPrimary;
        UnityEngine.Color color1 = hairColorsPrimary == null || hairColorsPrimary.Count <= 0 ? this.data.PickHairColorPrimary(ethnicGroupFromId) : ethnicGroupFromId.hairColorsPrimary[0];
        List<UnityEngine.Color> hairColorsSecondary = ethnicGroupFromId.hairColorsSecondary;
        UnityEngine.Color color2 = hairColorsSecondary == null || hairColorsSecondary.Count <= 0 ? this.data.PickHairColorSecondary(ethnicGroupFromId) : ethnicGroupFromId.hairColorsSecondary[0];
        List<UnityEngine.Color> hairColorsSpecular = ethnicGroupFromId.hairColorsSpecular;
        UnityEngine.Color color3 = hairColorsSpecular == null || hairColorsSpecular.Count <= 0 ? this.data.PickHairColorSecondary(ethnicGroupFromId) : ethnicGroupFromId.hairColorsSpecular[0];
        this.SetColor(color1, Creature.ColorModifier.Hair);
        this.SetColor(color3, Creature.ColorModifier.HairSecondary);
        this.SetColor(color2, Creature.ColorModifier.HairSpecular);
      }
      if ((double) playerCharacterData.customization.eyesIrisColor.a > 0.0)
      {
        this.SetColor(playerCharacterData.customization.eyesIrisColor, Creature.ColorModifier.EyesIris);
        this.SetColor(playerCharacterData.customization.eyesScleraColor, Creature.ColorModifier.EyesSclera);
      }
      else
      {
        List<UnityEngine.Color> eyesColorsIris = ethnicGroupFromId.eyesColorsIris;
        UnityEngine.Color color4 = eyesColorsIris == null || eyesColorsIris.Count <= 0 ? this.data.PickEyesColorIris(ethnicGroupFromId) : ethnicGroupFromId.eyesColorsIris[0];
        List<UnityEngine.Color> eyesColorsSclera = ethnicGroupFromId.eyesColorsSclera;
        UnityEngine.Color color5 = eyesColorsSclera == null || eyesColorsSclera.Count <= 0 ? this.data.PickEyesColorSclera(ethnicGroupFromId) : ethnicGroupFromId.eyesColorsSclera[0];
        this.SetColor(color4, Creature.ColorModifier.EyesIris);
        this.SetColor(color5, Creature.ColorModifier.EyesSclera);
      }
      if ((double) playerCharacterData.customization.skinColor.a > 0.0)
      {
        this.SetColor(playerCharacterData.customization.skinColor, Creature.ColorModifier.Skin);
      }
      else
      {
        List<UnityEngine.Color> skinColors = ethnicGroupFromId.skinColors;
        this.SetColor(skinColors == null || skinColors.Count <= 0 ? this.data.PickSkinColor(ethnicGroupFromId) : ethnicGroupFromId.skinColors[0], Creature.ColorModifier.Skin);
      }
    }
    else
    {
      if (this.data.ethnicGroups.Count <= 0)
        return;
      CreatureData.EthnicGroup ethnicGroupFromId = this.GetEthnicGroupFromId(this.data.ethnicityId);
      this.SetEthnicGroup(ethnicGroupFromId);
      List<UnityEngine.Color> hairColorsPrimary = ethnicGroupFromId.hairColorsPrimary;
      UnityEngine.Color color6 = hairColorsPrimary == null || hairColorsPrimary.Count <= 0 ? this.data.PickHairColorPrimary(ethnicGroupFromId) : ethnicGroupFromId.hairColorsPrimary[0];
      List<UnityEngine.Color> hairColorsSecondary = ethnicGroupFromId.hairColorsSecondary;
      UnityEngine.Color color7 = hairColorsSecondary == null || hairColorsSecondary.Count <= 0 ? this.data.PickHairColorSecondary(ethnicGroupFromId) : ethnicGroupFromId.hairColorsSecondary[0];
      List<UnityEngine.Color> hairColorsSpecular = ethnicGroupFromId.hairColorsSpecular;
      UnityEngine.Color color8 = hairColorsSpecular == null || hairColorsSpecular.Count <= 0 ? this.data.PickHairColorSecondary(ethnicGroupFromId) : ethnicGroupFromId.hairColorsSpecular[0];
      this.SetColor(color6, Creature.ColorModifier.Hair);
      this.SetColor(color7, Creature.ColorModifier.HairSecondary);
      this.SetColor(color8, Creature.ColorModifier.HairSpecular);
      List<UnityEngine.Color> eyesColorsIris = ethnicGroupFromId.eyesColorsIris;
      UnityEngine.Color color9 = eyesColorsIris == null || eyesColorsIris.Count <= 0 ? this.data.PickEyesColorIris(ethnicGroupFromId) : ethnicGroupFromId.eyesColorsIris[0];
      List<UnityEngine.Color> eyesColorsSclera = ethnicGroupFromId.eyesColorsSclera;
      UnityEngine.Color color10 = eyesColorsSclera == null || eyesColorsSclera.Count <= 0 ? this.data.PickEyesColorSclera(ethnicGroupFromId) : ethnicGroupFromId.eyesColorsSclera[0];
      this.SetColor(color9, Creature.ColorModifier.EyesIris);
      this.SetColor(color10, Creature.ColorModifier.EyesSclera);
      this.SetColor(this.data.PickSkinColor(ethnicGroupFromId), Creature.ColorModifier.Skin);
    }
  }

  private void LoadContainer(PlayerSaveData playerCharacterData = null, bool clearLinkedHolders = false)
  {
    if (!(bool) (UnityEngine.Object) this.container)
      return;
    if (clearLinkedHolders)
      this.container.ClearLinkedHolders();
    if (playerCharacterData != null)
    {
      List<ContainerContent> containerContentList = playerCharacterData.CloneInventory();
      if (!containerContentList.IsNullOrEmpty())
      {
        this.container.spawnOwner = Item.Owner.Player;
        this.container.Load(containerContentList);
        this.container.containerID = this.data.containerID;
        return;
      }
    }
    this.container.spawnOwner = Item.Owner.None;
    this.container.loadContent = Container.LoadContent.ContainerID;
    this.container.containerID = this.data.containerID;
    this.container.Load();
  }

  public void LoadDefaultSkills()
  {
    foreach (SkillData data in Catalog.GetDataList<SkillData>())
    {
      if (data.allowSkill && data.isDefaultSkill && !this.HasSkill(data))
      {
        if (data is SpellData spellData)
          this.container.AddSpellContent(spellData);
        else
          this.container.AddSkillContent(data);
      }
    }
  }

  public void LoadSkills()
  {
    if ((UnityEngine.Object) this.container == (UnityEngine.Object) null)
      return;
    foreach (SpellContent spellContent in this.container.contents.GetEnumerableContentsOfType<SpellContent>(true))
    {
      if (spellContent.data.allowSkill)
      {
        try
        {
          spellContent.data.OnSkillLoaded((SkillData) spellContent.data, this);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error loading skill {spellContent.data.id} for {this.data.id}, skipping. Exception below.");
          Debug.LogException(ex);
        }
      }
    }
    foreach (SkillContent skillContent in this.container.contents.GetEnumerableContentsOfType<SkillContent>(true))
    {
      if (skillContent.data.allowSkill)
      {
        try
        {
          skillContent.data.OnSkillLoaded(skillContent.data, this);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error loading skill {skillContent.data.id} for {this.data.id}, skipping. Exception below.");
          Debug.LogException(ex);
        }
      }
    }
    foreach (SpellContent spellContent in this.container.contents.GetEnumerableContentsOfType<SpellContent>(true))
    {
      if (spellContent.data.allowSkill)
      {
        try
        {
          spellContent.data.OnLateSkillsLoaded((SkillData) spellContent.data, this);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error late loading skill {spellContent.data.id} for {this.data.id}, skipping. Exception below.");
          Debug.LogException(ex);
        }
      }
    }
    foreach (SkillContent skillContent in this.container.contents.GetEnumerableContentsOfType<SkillContent>(true))
    {
      if (skillContent.data.allowSkill)
      {
        try
        {
          skillContent.data.OnLateSkillsLoaded(skillContent.data, this);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) $"Error late loading skill {skillContent.data.id} for {this.data.id}, skipping. Exception below.");
          Debug.LogException(ex);
        }
      }
    }
    try
    {
      if (!(bool) (UnityEngine.Object) this.mana)
        return;
      this.mana.InvokeOnSpellLoad((SpellData) this.handLeft.caster.telekinesis, this.handLeft.caster);
      this.mana.InvokeOnSpellLoad((SpellData) this.handRight.caster.telekinesis, this.handRight.caster);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Error loading telekinesis for {this.data.id}: {ex}");
    }
  }

  public void LogSkills()
  {
    List<string> values = new List<string>();
    foreach (SkillContent skillContent in this.container.contents.GetEnumerableContentsOfType<SkillContent>())
      values.Add(skillContent.data.id);
    if (values.Count == 0)
      Debug.Log((object) "Player has no skills");
    else
      Debug.Log((object) ("Loaded player skills:\n - " + string.Join("\n - ", (IEnumerable<string>) values)));
  }

  public void UnloadSkills()
  {
    if ((UnityEngine.Object) this.container == (UnityEngine.Object) null)
      return;
    foreach (SkillContent skillContent in this.container.contents.GetEnumerableContentsOfType<SkillContent>(true))
    {
      try
      {
        skillContent.data.OnSkillUnloaded(skillContent.data, this);
      }
      catch (NullReferenceException ex)
      {
        Debug.LogError((object) $"Caught NullReferenceException while unloading {skillContent.data.id}, skipping. Exception below.");
        Debug.LogException((Exception) ex);
      }
    }
  }

  public bool ForceLoadSkill(string id)
  {
    SkillData outputData;
    if (!Catalog.TryGetData<SkillData>(id, out outputData))
      return false;
    outputData.OnSkillLoaded(outputData, this);
    outputData.OnLateSkillsLoaded(outputData, this);
    Creature.ForceSkillLoadEvent forceSkillLoadEvent = this.OnForceSkillLoadEvent;
    if (forceSkillLoadEvent != null)
      forceSkillLoadEvent(outputData);
    return true;
  }

  public bool ForceUnloadSkill(string id)
  {
    SkillData outputData;
    if (!Catalog.TryGetData<SkillData>(id, out outputData))
      return false;
    outputData.OnSkillUnloaded(outputData, this);
    Creature.ForceSkillLoadEvent skillUnloadEvent = this.OnForceSkillUnloadEvent;
    if (skillUnloadEvent != null)
      skillUnloadEvent(outputData);
    return true;
  }

  public bool TryAddSkill(SkillData skill)
  {
    if (skill == null)
      return false;
    if (this.HasSkill(skill))
      return true;
    this.container?.AddSkillContent(skill);
    return true;
  }

  public bool TryRemoveSkill(SkillData skill)
  {
    if (skill == null)
      return false;
    if (!this.HasSkill(skill))
      return true;
    this.container?.RemoveContent(skill.id);
    return true;
  }

  public bool HasSkill(SkillData skill)
  {
    if (skill == null)
      return false;
    Container container = this.container;
    return container != null && container.HasSkillContent(skill);
  }

  public bool HasSkill(string id)
  {
    Container container = this.container;
    return container != null && container.HasSkillContent(id);
  }

  public bool TryGetSkill<T>(string id, out T skillData) where T : SkillData
  {
    skillData = default (T);
    Container container = this.container;
    return container != null && container.TryGetSkillContent<T>(id, out skillData);
  }

  public bool TryGetSkill<T>(SkillData data, out T skillData) where T : SkillData
  {
    skillData = default (T);
    Container container = this.container;
    return container != null && container.TryGetSkillContent<T>(data, out skillData);
  }

  public int CountSkillsOfTree(string tree, bool includeNonCore = false, bool includeDual = false)
  {
    int num = 0;
    for (int index = 0; index < this.container.contents.Count; ++index)
    {
      SkillData data = this.container.contents[index] is SkillContent content ? content.data : (SkillData) null;
      if (data != null && !data.isDefaultSkill && data.showInTree && (data.primarySkillTreeId == tree || data.secondarySkillTreeId == tree) && (includeDual || !data.IsCombinedSkill) && (includeNonCore || data.isTierBlocker))
        ++num;
    }
    return num;
  }

  public void AddForce(
    UnityEngine.Vector3 force,
    ForceMode forceMode,
    float nonRootMultiplier = 1f,
    CollisionHandler hitHandler = null)
  {
    if (this.isPlayer)
    {
      this.player.locomotion.physicBody.AddForce(force, forceMode);
    }
    else
    {
      List<RagdollPart> parts;
      if (hitHandler != null)
      {
        RagdollPart ragdollPart = hitHandler.ragdollPart;
        if (ragdollPart != null)
        {
          parts = ragdollPart.ragdollRegion.parts;
          goto label_6;
        }
      }
      parts = this.ragdoll.parts;
label_6:
      List<RagdollPart> ragdollPartList = parts;
      for (int index = 0; index < ragdollPartList.Count; ++index)
      {
        RagdollPart ragdollPart = ragdollPartList[index];
        if (!ragdollPart.isSliced)
          ragdollPart.physicBody?.AddForce(force * ((UnityEngine.Object) ragdollPart == (UnityEngine.Object) this.ragdoll.rootPart ? 1f : nonRootMultiplier), forceMode);
      }
    }
  }

  public override void AddForce(UnityEngine.Vector3 force, ForceMode forceMode, CollisionHandler hitHandler = null)
  {
    base.AddForce(force, forceMode, hitHandler);
    if (this.isPlayer)
    {
      this.player.locomotion.physicBody.AddForce(force, forceMode);
    }
    else
    {
      List<RagdollPart> parts;
      if (hitHandler != null)
      {
        RagdollPart ragdollPart = hitHandler.ragdollPart;
        if (ragdollPart != null)
        {
          parts = ragdollPart.ragdollRegion.parts;
          goto label_6;
        }
      }
      parts = this.ragdoll.parts;
label_6:
      List<RagdollPart> ragdollPartList = parts;
      for (int index = 0; index < ragdollPartList.Count; ++index)
      {
        RagdollPart ragdollPart = ragdollPartList[index];
        if (!ragdollPart.isSliced)
          ragdollPart.physicBody?.AddForce(force, forceMode);
      }
    }
  }

  public override void AddRadialForce(
    float force,
    UnityEngine.Vector3 origin,
    float upwardsModifier,
    ForceMode forceMode,
    CollisionHandler hitHandler = null)
  {
    if (this.isPlayer)
    {
      this.player.locomotion.physicBody.AddRadialForce(force, origin, upwardsModifier, forceMode);
    }
    else
    {
      List<RagdollPart> parts;
      if (hitHandler != null)
      {
        RagdollPart ragdollPart = hitHandler.ragdollPart;
        if (ragdollPart != null)
        {
          parts = ragdollPart.ragdollRegion.parts;
          goto label_6;
        }
      }
      parts = this.ragdoll.parts;
label_6:
      List<RagdollPart> ragdollPartList = parts;
      for (int index = 0; index < ragdollPartList.Count; ++index)
        ragdollPartList[index].physicBody?.AddRadialForce(force, origin, upwardsModifier, forceMode);
    }
  }

  public override void AddExplosionForce(
    float force,
    UnityEngine.Vector3 origin,
    float radius,
    float upwardsModifier,
    ForceMode forceMode,
    CollisionHandler hitHandler = null)
  {
    base.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode);
    if (this.isPlayer)
    {
      this.player.locomotion.physicBody.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode);
    }
    else
    {
      List<RagdollPart> parts;
      if (hitHandler != null)
      {
        RagdollPart ragdollPart = hitHandler.ragdollPart;
        if (ragdollPart != null)
        {
          parts = ragdollPart.ragdollRegion.parts;
          goto label_6;
        }
      }
      parts = this.ragdoll.parts;
label_6:
      List<RagdollPart> ragdollPartList = parts;
      for (int index = 0; index < ragdollPartList.Count; ++index)
        ragdollPartList[index].physicBody?.AddRadialForce(force, origin, upwardsModifier, forceMode);
    }
  }

  public void InvokeOnThrowEvent(RagdollHand hand, Handle handle)
  {
    Creature.ThrowEvent onThrowEvent = this.OnThrowEvent;
    if (onThrowEvent == null)
      return;
    onThrowEvent(hand, handle);
  }

  public void InvokeThisCreatureAttackEvent(
    Creature targetCreature,
    Transform targetTransform,
    BrainModuleAttack.AttackType type,
    BrainModuleAttack.AttackStage stage)
  {
    Creature.ThisCreatureAttackEvent creatureAttackEvent = this.OnThisCreatureAttackEvent;
    if (creatureAttackEvent == null)
      return;
    creatureAttackEvent(targetCreature, targetTransform, type, stage);
  }

  public void OnMaxHealthModifierChangeEvent(float oldValue, float newValue)
  {
    this.currentHealth = this.currentHealth / oldValue * newValue;
  }

  public void InvokeHealthChangeEvent(float health, float maxHealth)
  {
    if ((double) this.currentHealth > (double) maxHealth)
      this.currentHealth = maxHealth;
    Creature.HealthChangeEvent onHealthChange = this.OnHealthChange;
    if (onHealthChange == null)
      return;
    onHealthChange(health, maxHealth);
  }

  private void SetupData(
    CreatureData data,
    PlayerSaveData playerCharacterData = null,
    bool characterSelect = false)
  {
    Creature.SetupDataEvent onSetupDataEvent1 = this.OnSetupDataEvent;
    if (onSetupDataEvent1 != null)
      onSetupDataEvent1(EventTime.OnStart);
    this.SetFaction(data.factionId);
    this.maxHealth = (float) data.health;
    this.currentHealth = this.maxHealth;
    this.countTowardsMaxAlive = data.countTowardsMaxAlive;
    this.eyeClips.Clear();
    if (!string.IsNullOrEmpty(data.animatorBundleAddress))
    {
      AnimatorBundle animatorBundle;
      if (Creature.creatureAnimatorControllers.TryGetValue(data.animatorBundleAddress, out animatorBundle))
        this.ApplyAnimatorController(animatorBundle);
      else
        Debug.LogError((object) $"Controller with address \"{data.animatorBundleAddress}\" did not load!");
    }
    else
      Debug.LogWarning((object) "Field \"animatorControllerAddress\" is not set in CreatureData! If this creature should have dynamic animations, fix this or it will not work!");
    this.fallAliveDestabilizeHeight = data.ragdollData.fallAliveDestabilizeHeight;
    this.fallAliveAnimationHeight = data.ragdollData.fallAliveAnimationHeight;
    this.groundStabilizationMinDuration = data.ragdollData.groundStabilizationMinDuration;
    this.groundStabilizationMaxVelocity = data.ragdollData.groundStabilizationMaxVelocity;
    this.ragdoll.Load(data);
    this.LoadContainer(playerCharacterData, characterSelect);
    if ((bool) (UnityEngine.Object) this.equipment)
      this.equipment.Load(characterSelect);
    this.wasLoadedForCharacterSelect = characterSelect;
    if (!characterSelect && (bool) (UnityEngine.Object) this.mana)
      this.mana.Load();
    this.SetupManikin(playerCharacterData);
    this.locomotion.Init();
    if (characterSelect)
      return;
    this.SetVariable<bool>("IsPlayer", true);
    if (playerCharacterData != null)
      this.LoadDefaultSkills();
    this.LoadSkills();
    if (playerCharacterData != null)
      this.LogSkills();
    this.currentHealth = this.maxHealth;
    Creature.SetupDataEvent onSetupDataEvent2 = this.OnSetupDataEvent;
    if (onSetupDataEvent2 == null)
      return;
    onSetupDataEvent2(EventTime.OnEnd);
  }

  public virtual void Load(PlayerSaveData playerCharacterData = null, bool characterSelect = false)
  {
    this.Load(this.data, playerCharacterData, characterSelect);
  }

  public virtual IEnumerator LoadCoroutine(
    CreatureData data,
    PlayerSaveData playerCharacterData = null,
    bool characterSelect = false)
  {
    Creature creature = this;
    if (!creature.initialized)
      creature.Init();
    creature.ClearMultipliers();
    creature.data = data;
    Coroutine coroutine = creature.StartCoroutine(creature.brain.LoadCoroutine(data.brainId));
    creature.SetupData(data, playerCharacterData, characterSelect);
    yield return (object) coroutine;
    creature.ParentEyes();
    creature.ragdoll.UpdateMetalArmor();
    creature.loaded = true;
    Creature.SimpleDelegate onDataLoaded = creature.OnDataLoaded;
    if (onDataLoaded != null)
      onDataLoaded();
    yield return (object) null;
  }

  public void SetRandomEthnicGroup()
  {
    this.SetEthnicGroup(this.data.ethnicGroups[UnityEngine.Random.Range(0, this.data.ethnicGroups.Count)]);
  }

  public void SetEthnicGroupFromId(string ethnicGroupId = "Eradian")
  {
    this.SetEthnicGroup(this.GetEthnicGroupFromId(ethnicGroupId));
  }

  public virtual void Load(
    CreatureData data,
    PlayerSaveData playerCharacterData = null,
    bool characterSelect = false)
  {
    if (!this.initialized)
      this.Init();
    this.data = data;
    this.ClearMultipliers();
    this.SetupData(data, playerCharacterData, characterSelect);
    this.brain.Load(data.brainId);
    this.loaded = true;
    this.ParentEyes();
    this.ragdoll.UpdateMetalArmor();
    Creature.SimpleDelegate onDataLoaded = this.OnDataLoaded;
    if (onDataLoaded == null)
      return;
    onDataLoaded();
  }

  private void InitLocomotionAnimation()
  {
    Creature.hashStrafe = Animator.StringToHash("Strafe");
    Creature.hashTurn = Animator.StringToHash("Turn");
    Creature.hashSpeed = Animator.StringToHash("Speed");
    Creature.hashVerticalSpeed = Animator.StringToHash("VerticalSpeed");
  }

  private void UpdateLocomotionAnimation()
  {
    if (this.locomotion.capsuleCollider.enabled == this.isKilled)
      this.locomotion.capsuleCollider.enabled = !this.isKilled;
    if ((UnityEngine.Object) this.brain?.navMeshAgent != (UnityEngine.Object) null && !this.brain.navMeshAgent.enabled)
      this.brain.navMeshAgent.enabled = this.loaded;
    UnityEngine.Vector3 vector3 = this.transform.InverseTransformDirection(this.currentLocomotion.velocity);
    if (this.currentLocomotion.isGrounded && (double) this.currentLocomotion.horizontalSpeed + (double) Mathf.Abs(this.currentLocomotion.angularSpeed) > (double) this.stationaryVelocityThreshold || this.animator.GetBool(Creature.hashUnderwater))
    {
      float num1 = this.currentLocomotion.isGrounded ? 0.0f : Mathf.Clamp((float) (0.20000000298023224 + (double) vector3.y * (1.0 / (double) this.transform.lossyScale.y)), -0.6f, 0.7f);
      this.animator.SetFloat(Creature.hashVerticalSpeed, num1, this.animationDampTime, Time.deltaTime);
      float num2 = Mathf.Lerp(1f, 0.3f, Mathf.Pow(Mathf.Clamp01(num1 / -0.6f), 3f));
      this.animator.SetFloat(Creature.hashSpeed, vector3.z * (1f / this.transform.lossyScale.z) * num2, this.animationDampTime, Time.deltaTime);
      this.animator.SetFloat(Creature.hashStrafe, vector3.x * (1f / this.transform.lossyScale.x) * num2, this.animationDampTime, Time.deltaTime);
      this.animator.SetFloat(Creature.hashTurn, this.currentLocomotion.angularSpeed * (1f / this.transform.lossyScale.y) * this.turnAnimSpeed, this.animationDampTime, Time.deltaTime);
    }
    else
    {
      this.animator.SetFloat(Creature.hashStrafe, 0.0f, this.animationDampTime, Time.deltaTime);
      this.animator.SetFloat(Creature.hashTurn, 0.0f, this.animationDampTime, Time.deltaTime);
      this.animator.SetFloat(Creature.hashSpeed, 0.0f, this.animationDampTime, Time.deltaTime);
      float num = Mathf.Clamp((float) (0.20000000298023224 + (double) vector3.y * (1.0 / (double) this.transform.lossyScale.y)), -0.6f, 0.7f);
      this.animator.SetFloat(Creature.hashVerticalSpeed, num, this.verticalDampTime, Time.deltaTime);
    }
  }

  public void AnimatorMoveUpdate()
  {
    if (!this.animator.applyRootMotion)
      return;
    this.transform.rotation = this.animator.rootRotation;
    this.transform.position += this.animator.deltaPosition;
    this.animator.transform.localPosition = UnityEngine.Vector3.zero;
    this.animator.transform.localRotation = Quaternion.identity;
    this.ragdoll.AnimatorMoveUpdate();
  }

  protected internal override void ManagedLateUpdate()
  {
    if (!this.cullingDetectionEnabled || (UnityEngine.Object) AreaManager.Instance == (UnityEngine.Object) null || !this.initialized || !this.loaded || (bool) (UnityEngine.Object) this.equipment && this.equipment.GetPendingApparelLoading() > 0)
      return;
    foreach (Holder holder in this.holders)
    {
      if (holder.spawningItem)
        return;
    }
    if ((double) Time.time - (double) this.cullingDetectionCycleTime < (double) this.cullingDetectionCycleSpeed)
      return;
    this.cullingDetectionCycleTime = Time.time;
    if (this.currentArea == null)
    {
      SpawnableArea recursive = AreaManager.Instance.CurrentArea.FindRecursive(this.ragdoll.rootPart.transform.position);
      if (recursive != null)
      {
        this.currentArea = recursive;
        this.SetCull(!this.currentArea.IsSpawned || this.currentArea.SpawnedArea.isCulled);
        if (this.currentArea.IsSpawned)
          this.currentArea.SpawnedArea.RegisterCreature(this);
      }
    }
    else
    {
      SpawnableArea recursive = this.currentArea.FindRecursive(this.ragdoll.rootPart.transform.position);
      if (recursive == null)
      {
        if (this.currentArea.IsSpawned)
          this.currentArea.SpawnedArea.UnRegisterCreature(this);
        this.currentArea = recursive;
      }
      else if (this.currentArea != recursive)
      {
        if (this.currentArea.IsSpawned)
          this.currentArea.SpawnedArea.UnRegisterCreature(this);
        this.currentArea = recursive;
        this.SetCull(!this.currentArea.IsSpawned || this.currentArea.SpawnedArea.isCulled);
        if (this.currentArea.IsSpawned)
          this.currentArea.SpawnedArea.RegisterCreature(this);
      }
    }
    if (this.currentArea != AreaManager.Instance.CurrentArea)
      return;
    this.Hide(false);
  }

  public void SetCull(bool cull)
  {
    if (this.isPlayer || this.isCulled == cull)
      return;
    this.gameObject.SetActive(!cull);
    this.isCulled = cull;
    if (this.isCulled || !(bool) (UnityEngine.Object) this.manikinProperties)
      return;
    this.manikinProperties.UpdateProperties();
  }

  public void SetColor(UnityEngine.Color color, Creature.ColorModifier colorModifier, bool updateProperties = false)
  {
    if (!(bool) (UnityEngine.Object) this.manikinProperties)
      return;
    switch (colorModifier)
    {
      case Creature.ColorModifier.Hair:
        ManikinProperty manikinProperty1;
        if (this.TryGetManikinProperty("HairColor", out manikinProperty1))
        {
          this.manikinProperties.TryUpdateProperty(color, false, manikinProperty1.set);
          break;
        }
        break;
      case Creature.ColorModifier.HairSecondary:
        ManikinProperty manikinProperty2;
        if (this.TryGetManikinProperty("HairSecondaryColor", out manikinProperty2))
        {
          this.manikinProperties.TryUpdateProperty(color, false, manikinProperty2.set);
          break;
        }
        break;
      case Creature.ColorModifier.HairSpecular:
        ManikinProperty manikinProperty3;
        if (this.TryGetManikinProperty("HairSpecularColor", out manikinProperty3))
        {
          this.manikinProperties.TryUpdateProperty(color, false, manikinProperty3.set);
          break;
        }
        break;
      case Creature.ColorModifier.EyesIris:
        ManikinProperty manikinProperty4;
        if (this.TryGetManikinProperty("EyeIrisColor", out manikinProperty4))
        {
          this.manikinProperties.TryUpdateProperty(color, false, manikinProperty4.set);
          break;
        }
        break;
      case Creature.ColorModifier.EyesSclera:
        ManikinProperty manikinProperty5;
        if (this.TryGetManikinProperty("EyeScleraColor", out manikinProperty5))
        {
          this.manikinProperties.TryUpdateProperty(color, false, manikinProperty5.set);
          break;
        }
        break;
      case Creature.ColorModifier.Skin:
        ManikinProperty manikinProperty6;
        if (this.TryGetManikinProperty("SkinColor", out manikinProperty6))
        {
          this.manikinProperties.TryUpdateProperty(color, false, manikinProperty6.set);
          break;
        }
        break;
    }
    if (!updateProperties)
      return;
    this.manikinProperties.UpdateProperties();
  }

  public UnityEngine.Color GetColor(Creature.ColorModifier colorModifier)
  {
    if ((bool) (UnityEngine.Object) this.manikinProperties)
    {
      switch (colorModifier)
      {
        case Creature.ColorModifier.Hair:
          ManikinProperty manikinProperty1;
          if (this.TryGetManikinProperty("HairColor", out manikinProperty1))
            return new UnityEngine.Color(manikinProperty1.values[0], manikinProperty1.values[1], manikinProperty1.values[2], manikinProperty1.values[3]);
          break;
        case Creature.ColorModifier.HairSecondary:
          ManikinProperty manikinProperty2;
          if (this.TryGetManikinProperty("HairSecondaryColor", out manikinProperty2))
            return new UnityEngine.Color(manikinProperty2.values[0], manikinProperty2.values[1], manikinProperty2.values[2], manikinProperty2.values[3]);
          break;
        case Creature.ColorModifier.HairSpecular:
          ManikinProperty manikinProperty3;
          if (this.TryGetManikinProperty("HairSpecularColor", out manikinProperty3))
            return new UnityEngine.Color(manikinProperty3.values[0], manikinProperty3.values[1], manikinProperty3.values[2], manikinProperty3.values[3]);
          break;
        case Creature.ColorModifier.EyesIris:
          ManikinProperty manikinProperty4;
          if (this.TryGetManikinProperty("EyeIrisColor", out manikinProperty4))
            return new UnityEngine.Color(manikinProperty4.values[0], manikinProperty4.values[1], manikinProperty4.values[2], manikinProperty4.values[3]);
          break;
        case Creature.ColorModifier.EyesSclera:
          ManikinProperty manikinProperty5;
          if (this.TryGetManikinProperty("EyeScleraColor", out manikinProperty5))
            return new UnityEngine.Color(manikinProperty5.values[0], manikinProperty5.values[1], manikinProperty5.values[2], manikinProperty5.values[3]);
          break;
        case Creature.ColorModifier.Skin:
          ManikinProperty manikinProperty6;
          if (this.TryGetManikinProperty("SkinColor", out manikinProperty6))
            return new UnityEngine.Color(manikinProperty6.values[0], manikinProperty6.values[1], manikinProperty6.values[2], manikinProperty6.values[3]);
          break;
      }
    }
    Debug.LogWarning((object) $"Can't retrieve creature {colorModifier.ToString()}, manikin property maybe missing");
    return UnityEngine.Color.white;
  }

  public bool TryGetManikinProperty(string name, out ManikinProperty manikinProperty)
  {
    return this.TryGetManikinProperty(name, this.manikinProperties, out manikinProperty);
  }

  public bool TryGetManikinProperty(
    string name,
    ManikinProperties properties,
    out ManikinProperty manikinProperty)
  {
    foreach (ManikinProperty property in properties.properties)
    {
      if (property.set.name == name)
      {
        manikinProperty = property;
        return true;
      }
    }
    manikinProperty = new ManikinProperty();
    return false;
  }

  public bool IsFromWave() => this.spawnGroup != null;

  public bool IsEnemy(Creature creatureTarget, bool aliveOnly = false)
  {
    return !((UnityEngine.Object) creatureTarget == (UnityEngine.Object) this) && (!aliveOnly || creatureTarget.state != Creature.State.Dead) && this.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Passive && this.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && creatureTarget.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && (this.faction.attackBehaviour == GameData.Faction.AttackBehaviour.Agressive || this.factionId != creatureTarget.factionId);
  }

  public bool IsVisible()
  {
    foreach (Creature.RendererData renderer in this.renderers)
    {
      if (renderer.renderer.isVisible)
        return true;
    }
    return false;
  }

  public void Hide(bool hide)
  {
    if (this.isPlayer)
      return;
    if (!hide && (bool) (UnityEngine.Object) this.manikinParts && this.manikinParts.disableRenderersDuringUpdate && this.manikinParts.PendingHandles() > 0)
    {
      this.hidden = false;
    }
    else
    {
      foreach (Creature.RendererData renderer in this.renderers)
      {
        if ((bool) (UnityEngine.Object) renderer.splitRenderer)
          renderer.splitRenderer.enabled = !hide;
        else if ((bool) (UnityEngine.Object) renderer.renderer)
          renderer.renderer.enabled = !hide;
      }
      if (hide)
      {
        foreach (Holder holder in this.holders)
        {
          foreach (Item obj in holder.items)
            obj.Hide(true);
        }
      }
      else if (!this.holsterItemsHidden)
        this.HideItemsInHolders(false);
      this.hidden = hide;
    }
  }

  public void HideItemsInHolders(bool hide)
  {
    foreach (Holder holder in this.holders)
    {
      foreach (Item obj in holder.items)
        obj.Hide(hide);
    }
    this.holsterItemsHidden = hide;
  }

  public virtual void OnManikinChangedEvent(ManikinPart[] partsAdded)
  {
    this.UpdateRenderers();
    this.Hide(this.hidden);
    int count1 = this.equipment.wearableSlots.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      Wearable wearableSlot = this.equipment.wearableSlots[index1];
      if (!wearableSlot.IsEmpty())
      {
        foreach (Wearable.WearableEntry wardrobeLayer in wearableSlot.wardrobeLayers)
        {
          ItemContent equipmentOnLayer = wearableSlot.GetEquipmentOnLayer(wardrobeLayer.layer);
          ItemModuleWardrobe module;
          int index2;
          if (equipmentOnLayer != null && equipmentOnLayer.data.TryGetModule<ItemModuleWardrobe>(out module) && wearableSlot.TryGetLayerIndex(wardrobeLayer.layer, out index2))
          {
            List<Renderer> wornPartRenderers = wearableSlot.GetWornPartRenderers(index2);
            if (wornPartRenderers != null)
            {
              int count2 = wornPartRenderers.Count;
              for (int index3 = 0; index3 < count2; ++index3)
              {
                Renderer renderer = wornPartRenderers[index3];
                if (!((UnityEngine.Object) renderer == (UnityEngine.Object) null))
                {
                  switch (module.castShadows)
                  {
                    case ItemModuleWardrobe.CastShadows.None:
                      renderer.shadowCastingMode = ShadowCastingMode.Off;
                      continue;
                    case ItemModuleWardrobe.CastShadows.PlayerOnly:
                      renderer.shadowCastingMode = this.isPlayer ? ShadowCastingMode.On : ShadowCastingMode.Off;
                      continue;
                    case ItemModuleWardrobe.CastShadows.PlayerAndNPC:
                      renderer.shadowCastingMode = ShadowCastingMode.On;
                      continue;
                    default:
                      continue;
                  }
                }
              }
            }
          }
        }
      }
    }
  }

  public Renderer GetRendererForVFX()
  {
    if ((bool) (UnityEngine.Object) this.vfxRenderer)
      return this.vfxRenderer;
    foreach (Creature.RendererData renderer in this.renderers)
    {
      if (renderer.renderer.name.IndexOf("vfx", StringComparison.OrdinalIgnoreCase) >= 0)
        return (Renderer) renderer.renderer;
    }
    return (Renderer) null;
  }

  public bool HaveRendererData(SkinnedMeshRenderer skinnedMeshRenderer)
  {
    foreach (Creature.RendererData renderer in this.renderers)
    {
      if ((UnityEngine.Object) renderer.renderer == (UnityEngine.Object) skinnedMeshRenderer)
        return true;
    }
    return false;
  }

  public void UpdateRenderers()
  {
    this.renderers = new List<Creature.RendererData>();
    this.revealDecals = new List<RevealDecal>();
    if ((bool) (UnityEngine.Object) this.manikinParts)
    {
      ManikinPart[] allParts = this.manikinParts.GetAllParts();
      foreach (ManikinPart manikinPart in allParts)
      {
        MeshPart[] components = manikinPart.GetComponents<MeshPart>();
        MeshPart meshPart = components.Length != 0 ? components[0] : (MeshPart) null;
        if (components.Length > 1)
          Debug.LogError((object) $"Creature {this.name} has more than one mesh part on manikin part {manikinPart.name}");
        foreach (SkinnedMeshRenderer componentsInChild in manikinPart.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
          if (!((UnityEngine.Object) componentsInChild.sharedMesh == (UnityEngine.Object) null) && !this.HaveRendererData(componentsInChild))
          {
            int lod = -1;
            if (manikinPart is ManikinGroupPart)
              lod = this.GetLODFromManikinGroupPart(manikinPart as ManikinGroupPart, (Renderer) componentsInChild);
            bool flag = (bool) (UnityEngine.Object) meshPart && (UnityEngine.Object) meshPart.skinnedMeshRenderer != (UnityEngine.Object) null && ((UnityEngine.Object) meshPart.skinnedMeshRenderer == (UnityEngine.Object) componentsInChild || (UnityEngine.Object) componentsInChild.transform.parent == (UnityEngine.Object) meshPart.skinnedMeshRenderer.transform.parent);
            RevealDecal component = componentsInChild.GetComponent<RevealDecal>();
            this.renderers.Add(new Creature.RendererData(componentsInChild, lod, flag ? meshPart : (MeshPart) null, component, manikinPart));
            if ((bool) (UnityEngine.Object) component)
              this.revealDecals.Add(component);
          }
        }
      }
      this.UpdatePartsBlendShapes(allParts);
    }
    else
    {
      if ((bool) (UnityEngine.Object) this.lodGroup)
      {
        LOD[] loDs = this.lodGroup.GetLODs();
        for (int lod = 0; lod < loDs.Length; ++lod)
        {
          foreach (Renderer renderer in loDs[lod].renderers)
          {
            if (renderer is SkinnedMeshRenderer)
            {
              SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
              if (!((UnityEngine.Object) skinnedMeshRenderer.sharedMesh == (UnityEngine.Object) null) && !this.HaveRendererData(skinnedMeshRenderer))
              {
                MeshPart componentInParent = skinnedMeshRenderer.GetComponentInParent<MeshPart>();
                MeshPart meshPart = !(bool) (UnityEngine.Object) componentInParent || !((UnityEngine.Object) componentInParent.skinnedMeshRenderer == (UnityEngine.Object) skinnedMeshRenderer) ? (MeshPart) null : componentInParent;
                RevealDecal component = skinnedMeshRenderer.GetComponent<RevealDecal>();
                this.renderers.Add(new Creature.RendererData(skinnedMeshRenderer, lod, meshPart, component));
                if ((bool) (UnityEngine.Object) component)
                  this.revealDecals.Add(component);
              }
            }
          }
        }
      }
      foreach (SkinnedMeshRenderer componentsInChild in this.GetComponentsInChildren<SkinnedMeshRenderer>())
      {
        if (!((UnityEngine.Object) componentsInChild.sharedMesh == (UnityEngine.Object) null) && !this.HaveRendererData(componentsInChild))
        {
          MeshPart componentInParent = componentsInChild.GetComponentInParent<MeshPart>();
          MeshPart meshPart = !(bool) (UnityEngine.Object) componentInParent || !((UnityEngine.Object) componentInParent.skinnedMeshRenderer == (UnityEngine.Object) componentsInChild) ? (MeshPart) null : componentInParent;
          RevealDecal component = componentsInChild.GetComponent<RevealDecal>();
          this.renderers.Add(new Creature.RendererData(componentsInChild, -1, meshPart, component));
          if ((bool) (UnityEngine.Object) component)
            this.revealDecals.Add(component);
        }
      }
    }
    foreach (RagdollPart part in this.ragdoll.parts)
      part.UpdateRenderers();
    if (LightProbeVolume.Exists)
    {
      List<Renderer> providedRenderers = new List<Renderer>();
      foreach (Creature.RendererData renderer in this.renderers)
        providedRenderers.Add((Renderer) renderer.renderer);
      this.lightVolumeReceiver.SetRenderers(providedRenderers, false);
      this.lightVolumeReceiver.UpdateRenderers();
    }
    this.RefreshRenderers();
  }

  /// <summary>
  /// Update the current parts blends shapes from the current wardrobes
  /// </summary>
  /// <param name="allParts"></param>
  private void UpdatePartsBlendShapes(ManikinPart[] allParts)
  {
    ManikinWardrobeData[] currentWardrobesData = this.GetCurrentWardrobesData();
    if (currentWardrobesData == null || allParts == null)
      return;
    for (int index = 0; index < allParts.Length; ++index)
      allParts[index].UpdateBlendShapes(currentWardrobesData);
  }

  /// <summary>
  /// Return current wardrobes data by fetching into layers.
  /// </summary>
  /// <returns>wardrobes data</returns>
  private ManikinWardrobeData[] GetCurrentWardrobesData()
  {
    if (!(bool) (UnityEngine.Object) this.manikinLocations)
      return (ManikinWardrobeData[]) null;
    HashSet<ManikinWardrobeData> source = new HashSet<ManikinWardrobeData>();
    foreach (KeyValuePair<ManikinLocations.LocationKey, ManikinWardrobeData> partLocation in this.manikinLocations.partLocations)
      source.Add(partLocation.Value);
    return source.ToArray<ManikinWardrobeData>();
  }

  public void RefreshRenderers()
  {
    if ((bool) (UnityEngine.Object) this.equipment && this.equipment.GetPendingApparelLoading() > 0)
      return;
    this.headManikinPart.Clear();
    if ((bool) (UnityEngine.Object) this.manikinLocations)
      this.manikinLocations.GetPartsAtChannel("Head", this.headManikinPart);
    int count = this.renderers.Count;
    for (int index = 0; index < count; ++index)
    {
      Creature.RendererData renderer = this.renderers[index];
      if (this.headManikinPart.Count > 0 && this.headManikinPart.Contains(renderer.manikinPart) || this.meshesToHideForFPV.Contains(renderer.renderer))
        renderer.renderer.gameObject.layer = GameManager.GetLayer((bool) (UnityEngine.Object) this.player ? LayerName.FPVHide : LayerName.NPC);
      else
        renderer.renderer.gameObject.layer = GameManager.GetLayer((bool) (UnityEngine.Object) this.player ? LayerName.Avatar : LayerName.NPC);
      renderer.renderer.updateWhenOffscreen = this.ShouldUpdateWhenOffscreen(renderer);
      RagdollPart part = this.ragdoll.GetPart(RagdollPart.Type.Torso);
      renderer.renderer.probeAnchor = (bool) (UnityEngine.Object) part ? part.transform : this.ragdoll.rootPart.transform;
      if ((bool) (UnityEngine.Object) renderer.splitRenderer)
      {
        renderer.splitRenderer.gameObject.layer = GameManager.GetLayer((bool) (UnityEngine.Object) this.player ? LayerName.Avatar : LayerName.NPC);
        renderer.splitRenderer.updateWhenOffscreen = this.ShouldUpdateWhenOffscreen(renderer);
      }
    }
  }

  public int GetLODFromManikinGroupPart(ManikinGroupPart manikinGroupPart, Renderer renderer)
  {
    for (int index = 0; index < manikinGroupPart.partLODs.Count; ++index)
    {
      foreach (UnityEngine.Object renderer1 in manikinGroupPart.partLODs[index].renderers)
      {
        if (renderer1 == (UnityEngine.Object) renderer)
          return index;
      }
    }
    return -1;
  }

  public bool ShouldUpdateWhenOffscreen(Creature.RendererData rendererData)
  {
    return rendererData.lod <= QualitySettings.maximumLODLevel && rendererData.renderer.gameObject.layer != GameManager.GetLayer(LayerName.FPVHide) && ((bool) (UnityEngine.Object) this.player || this.state != Creature.State.Alive || this.ragdoll.isGrabbed);
  }

  public void UpdateHeldImbues()
  {
    HashSet<string> heldImbueIds = this.heldImbueIDs;
    HashSet<string> after = new HashSet<string>();
    this.heldCrystalImbues.Clear();
    Item obj1 = this.handLeft.grabbedHandle?.item;
    if (obj1 != null)
    {
      for (int index = 0; index < obj1.imbues.Count; ++index)
      {
        if (obj1.imbues[index].spellCastBase != null)
        {
          if (obj1.imbues[index].colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal)
            this.heldCrystalImbues.Add(obj1.imbues[index].spellCastBase.id);
          after.Add(obj1.imbues[index].spellCastBase.id);
        }
      }
    }
    Item obj2 = this.handRight.grabbedHandle?.item;
    if (obj2 != null)
    {
      for (int index = 0; index < obj2.imbues.Count; ++index)
      {
        if (obj2.imbues[index].spellCastBase != null)
        {
          if (obj2.imbues[index].colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal)
            this.heldCrystalImbues.Add(obj2.imbues[index].spellCastBase.id);
          after.Add(obj2.imbues[index].spellCastBase.id);
        }
      }
    }
    this.heldImbueIDs = after;
    Creature.ImbueChangeEvent onHeldImbueChange = this.OnHeldImbueChange;
    if (onHeldImbueChange == null)
      return;
    onHeldImbueChange(this, heldImbueIds, after);
  }

  /// <summary>Make a creature drop whatever they are holding</summary>
  /// <param name="creature"></param>
  public static void DisarmCreature(Creature creature)
  {
    if ((UnityEngine.Object) creature == (UnityEngine.Object) null)
      return;
    Creature.DisarmCreature(creature, Side.Left);
    Creature.DisarmCreature(creature, Side.Right);
  }

  /// <summary>
  /// Make a creature drop whatever they are holding in a particular side
  /// </summary>
  /// <param name="creature"></param>
  /// <param name="side"></param>
  public static void DisarmCreature(Creature creature, Side side)
  {
    if ((UnityEngine.Object) creature == (UnityEngine.Object) null)
      return;
    if (side == Side.Left)
    {
      if (!Creature.IsCreatureGrabbingHandle(creature, side))
        return;
      creature.handLeft.UnGrab(false);
    }
    else
    {
      if (!Creature.IsCreatureGrabbingHandle(creature, side))
        return;
      creature.handRight.UnGrab(false);
    }
  }

  /// <summary>Returns true if the creature is holding something</summary>
  /// <param name="creature"></param>
  /// <returns></returns>
  public static bool IsCreatureGrabbingHandle(Creature creature)
  {
    return Creature.IsCreatureGrabbingHandle(creature, Side.Left) || Creature.IsCreatureGrabbingHandle(creature, Side.Right);
  }

  /// <summary>
  /// Returns true if the creature is holding something in a particular side
  /// </summary>
  /// <param name="creature"></param>
  /// <param name="side"></param>
  /// <returns></returns>
  public static bool IsCreatureGrabbingHandle(Creature creature, Side side)
  {
    return side == Side.Left ? (UnityEngine.Object) creature?.handLeft.grabbedHandle != (UnityEngine.Object) null : (UnityEngine.Object) creature?.handRight.grabbedHandle != (UnityEngine.Object) null;
  }

  public static List<Creature> InRadius(
    UnityEngine.Vector3 position,
    float radius,
    Func<Creature, bool> filter = null,
    List<Creature> allocList = null)
  {
    if (allocList == null)
      allocList = new List<Creature>();
    float num = radius * radius;
    for (int index = 0; index < Creature.allActive.Count; ++index)
    {
      Creature creature = Creature.allActive[index];
      if ((filter != null ? (filter(creature) ? 1 : 0) : 1) != 0 && (double) (position - creature.ClosestPoint(position)).sqrMagnitude <= (double) num)
        allocList.Add(creature);
    }
    return allocList;
  }

  public static List<Creature> InRadiusNaive(
    UnityEngine.Vector3 position,
    float radius,
    Func<Creature, bool> filter = null,
    List<Creature> allocList = null)
  {
    if (allocList == null)
      allocList = new List<Creature>();
    float num = radius * radius;
    for (int index = 0; index < Creature.allActive.Count; ++index)
    {
      Creature creature = Creature.allActive[index];
      if ((filter != null ? (filter(creature) ? 1 : 0) : 1) != 0 && (double) (position - creature.Center).sqrMagnitude <= (double) num)
        allocList.Add(creature);
    }
    return allocList;
  }

  public static ThunderEntity AimAssist(
    UnityEngine.Vector3 position,
    UnityEngine.Vector3 direction,
    float maxDistance,
    float maxAngle,
    out Transform targetPoint,
    Func<Creature, bool> filter = null,
    CreatureType weakpointFilter = (CreatureType) 0,
    Creature ignoredCreature = null,
    float minDistance = 0.1f)
  {
    return Creature.AimAssist(new Ray(position, direction), maxDistance, maxAngle, out targetPoint, filter, weakpointFilter, ignoredCreature, minDistance);
  }

  public static ThunderEntity AimAssist(
    Ray ray,
    float maxDistance,
    float maxAngle,
    out Transform targetPoint,
    Func<Creature, bool> filter = null,
    CreatureType weakpointFilter = (CreatureType) 0,
    Creature ignoredCreature = null,
    float minDistance = 0.1f)
  {
    float num1 = minDistance * minDistance;
    float num2 = maxDistance * maxDistance;
    float num3 = float.PositiveInfinity;
    ThunderEntity thunderEntity = (ThunderEntity) null;
    targetPoint = (Transform) null;
    for (int index1 = 0; index1 < Creature.allActive.Count; ++index1)
    {
      Creature creature = Creature.allActive[index1];
      if ((filter != null ? (filter(creature) ? 1 : 0) : 1) != 0 && !((UnityEngine.Object) creature == (UnityEngine.Object) ignoredCreature))
      {
        UnityEngine.Vector3 from1 = creature.Center - ray.origin;
        if (weakpointFilter.HasFlag((Enum) creature.data.type) && creature != null)
        {
          List<Transform> weakpoints = creature.weakpoints;
          if (weakpoints != null && weakpoints.Count > 0)
          {
            for (int index2 = 0; index2 < creature.weakpoints.Count; ++index2)
            {
              Transform weakpoint = creature.weakpoints[index2];
              UnityEngine.Vector3 from2 = weakpoint.position - ray.origin;
              if ((double) from2.sqrMagnitude <= (double) num2 && (double) from2.sqrMagnitude >= (double) num1 && (double) UnityEngine.Vector3.Angle(from2, ray.direction) <= (double) maxAngle)
              {
                float magnitude = from2.magnitude;
                float sqrMagnitude = (ray.GetPoint(magnitude) - weakpoint.position).sqrMagnitude;
                if ((double) sqrMagnitude < (double) num3)
                {
                  num3 = sqrMagnitude;
                  thunderEntity = (ThunderEntity) creature;
                  targetPoint = weakpoint;
                }
              }
            }
            continue;
          }
        }
        if ((double) from1.sqrMagnitude <= (double) num2 && (double) from1.sqrMagnitude >= (double) num1 && (double) UnityEngine.Vector3.Angle(from1, ray.direction) <= (double) maxAngle)
        {
          float magnitude = from1.magnitude;
          float sqrMagnitude = (ray.GetPoint(magnitude) - creature.ClosestPoint(ray.GetPoint(magnitude))).sqrMagnitude;
          if ((double) sqrMagnitude < (double) num3)
          {
            num3 = sqrMagnitude;
            thunderEntity = (ThunderEntity) creature;
          }
        }
      }
    }
    if (targetPoint == null)
      targetPoint = ((Creature) thunderEntity)?.ragdoll.targetPart.transform;
    if (!((UnityEngine.Object) Golem.local == (UnityEngine.Object) null))
    {
      Golem local = Golem.local;
      if (local != null && !local.isKilled)
      {
        UnityEngine.Vector3 from3 = local.Center - ray.origin;
        if (weakpointFilter.HasFlag((Enum) CreatureType.Golem))
        {
          List<Transform> weakpoints = local.weakpoints;
          if (weakpoints != null && weakpoints.Count > 0)
          {
            for (int index = 0; index < local.weakpoints.Count; ++index)
            {
              Transform weakpoint = local.weakpoints[index];
              UnityEngine.Vector3 from4 = weakpoint.position - ray.origin;
              if ((double) from4.sqrMagnitude <= (double) num2 && (double) from4.sqrMagnitude >= (double) num1 && (double) UnityEngine.Vector3.Angle(from4, ray.direction) <= (double) maxAngle)
              {
                float magnitude = from4.magnitude;
                float sqrMagnitude = (ray.GetPoint(magnitude) - weakpoint.position).sqrMagnitude;
                if ((double) sqrMagnitude < (double) num3)
                {
                  num3 = sqrMagnitude;
                  thunderEntity = (ThunderEntity) local;
                  targetPoint = weakpoint;
                }
              }
            }
            goto label_31;
          }
        }
        if ((double) from3.sqrMagnitude <= (double) num2 && (double) from3.sqrMagnitude >= (double) num1 && (double) UnityEngine.Vector3.Angle(from3, ray.direction) <= (double) maxAngle)
        {
          float magnitude = from3.magnitude;
          if ((double) (ray.GetPoint(magnitude) - local.ClosestPoint(ray.GetPoint(magnitude))).sqrMagnitude < (double) num3)
            thunderEntity = (ThunderEntity) local;
        }
label_31:
        return thunderEntity;
      }
    }
    return thunderEntity;
  }

  public override void RefreshWeakPoints()
  {
    if (this.weakpoints.Count != 0)
      return;
    this.weakpoints.Add(this.ragdoll.headPart.transform);
  }

  public void Teleport(UnityEngine.Vector3 position, Quaternion rotation)
  {
    if (this.isPlayer)
      Debug.Log((object) "Teleporting the player via Creature.Teleport will not work as intended! You want Player.local.Teleport instead!");
    UnityEngine.Vector3 position1 = UnityEngine.Vector3.zero;
    Quaternion localRotation1 = Quaternion.identity;
    if ((bool) (UnityEngine.Object) this.handLeft.grabbedHandle && (bool) (UnityEngine.Object) this.handLeft.grabbedHandle.item)
    {
      position1 = this.transform.InverseTransformPoint(this.handLeft.grabbedHandle.item.transform.position);
      localRotation1 = this.transform.InverseTransformRotation(this.handLeft.grabbedHandle.item.transform.rotation);
    }
    UnityEngine.Vector3 position2 = UnityEngine.Vector3.zero;
    Quaternion localRotation2 = Quaternion.identity;
    if ((bool) (UnityEngine.Object) this.handRight.grabbedHandle && (bool) (UnityEngine.Object) this.handRight.grabbedHandle.item)
    {
      position2 = this.transform.InverseTransformPoint(this.handRight.grabbedHandle.item.transform.position);
      localRotation2 = this.transform.InverseTransformRotation(this.handRight.grabbedHandle.item.transform.rotation);
    }
    this.transform.position = position;
    this.transform.rotation = rotation;
    this.locomotion.prevPosition = position;
    this.locomotion.prevRotation = rotation;
    this.locomotion.velocity = UnityEngine.Vector3.zero;
    if ((bool) (UnityEngine.Object) this.handLeft.grabbedHandle && (bool) (UnityEngine.Object) this.handLeft.grabbedHandle.item)
    {
      this.handLeft.grabbedHandle.item.transform.position = this.transform.TransformPoint(position1);
      this.handLeft.grabbedHandle.item.transform.rotation = this.transform.TransformRotation(localRotation1);
    }
    if (!(bool) (UnityEngine.Object) this.handRight.grabbedHandle || !(bool) (UnityEngine.Object) this.handRight.grabbedHandle.item)
      return;
    this.handRight.grabbedHandle.item.transform.position = this.transform.TransformPoint(position2);
    this.handRight.grabbedHandle.item.transform.rotation = this.transform.TransformRotation(localRotation2);
  }

  public void SetHeight(CreatureData creatureData)
  {
    if (creatureData.adjustHeightToPlayer && Player.characterData != null)
    {
      this.SetHeight(Mathf.Clamp(UnityEngine.Random.Range(Player.characterData.calibration.height - creatureData.adjustHeightToPlayerDelta, Player.characterData.calibration.height + creatureData.adjustHeightToPlayerDelta), creatureData.randomMinHeight, creatureData.randomMaxHeight));
    }
    else
    {
      if ((double) creatureData.randomMinHeight <= 0.0 || (double) creatureData.randomMaxHeight <= 0.0)
        return;
      this.SetHeight(UnityEngine.Random.Range(creatureData.randomMinHeight, creatureData.randomMaxHeight));
    }
  }

  public void SetHeight(float height)
  {
    if ((double) height == 0.0)
      return;
    if (this.isActiveAndEnabled && this.initialized)
    {
      Ragdoll.State state = this.ragdoll.state;
      this.ragdoll.SetState(Ragdoll.State.Disabled);
      this.ragdoll.gameObject.SetActive(false);
      this.transform.localScale = UnityEngine.Vector3.one * (height / this.morphology.height);
      this.ragdoll.gameObject.SetActive(true);
      this.ragdoll.SetState(state);
      if (this.OnHeightChanged == null)
        return;
      this.OnHeightChanged();
    }
    else
    {
      if ((double) this.morphology.height == 0.0)
      {
        this.InitCenterEyes();
        this.morphology.eyesHeight = this.transform.InverseTransformPoint(this.centerEyes.position).y;
        this.morphology.height = Morphology.GetHeight(this.morphology.eyesHeight);
      }
      this.transform.localScale = UnityEngine.Vector3.one * (height / this.morphology.height);
    }
  }

  public float GetHeight() => this.morphology.height * this.transform.lossyScale.y;

  public float GetEyesHeight() => this.morphology.eyesHeight * this.transform.lossyScale.y;

  public bool IsAnimatorBusy()
  {
    return !(bool) (UnityEngine.Object) this.animator || this.animator.GetBool(Creature.hashIsBusy);
  }

  public void SetAnimatorBusy(bool active) => this.animator.SetBool(Creature.hashIsBusy, active);

  public void TryElectrocute(
    float power,
    float duration,
    bool forced,
    bool imbueHit,
    EffectData effectData = null)
  {
    if (!(bool) (UnityEngine.Object) this.brain || this.brain.instance == null)
      return;
    this.brain.instance.GetModule<BrainModuleElectrocute>()?.TryElectrocute(power, duration, forced, imbueHit, effectData);
  }

  public void StopShock()
  {
    if (!(bool) (UnityEngine.Object) this.brain || this.brain.instance == null)
      return;
    this.brain.instance.GetModule<BrainModuleElectrocute>()?.StopElectrocute();
  }

  public void MaxPush(Creature.PushType type, UnityEngine.Vector3 direction, RagdollPart.Type bodyPart = (RagdollPart.Type) 0)
  {
    if (this.TryBrainModulePush(type, direction, 0, true, bodyPart))
      return;
    this.PushFallback(type);
  }

  public void TryPush(
    Creature.PushType type,
    UnityEngine.Vector3 direction,
    int pushLevel,
    RagdollPart.Type bodyPart = (RagdollPart.Type) 0)
  {
    if (this.TryBrainModulePush(type, direction, pushLevel, bodyPart: bodyPart))
      return;
    this.PushFallback(type);
  }

  private bool TryBrainModulePush(
    Creature.PushType type,
    UnityEngine.Vector3 direction,
    int pushLevel,
    bool max = false,
    RagdollPart.Type bodyPart = (RagdollPart.Type) 0)
  {
    if ((bool) (UnityEngine.Object) this.brain && this.brain.instance != null)
    {
      BrainModuleHitReaction module = this.brain.instance.GetModule<BrainModuleHitReaction>();
      if (module != null)
      {
        module.TryPush(type, direction, max ? module.GetMaxPushLevel(type) : pushLevel, bodyPart);
        return true;
      }
    }
    return false;
  }

  public void ForceStagger(
    UnityEngine.Vector3 direction,
    BrainModuleHitReaction.PushBehaviour.Effect pushType,
    RagdollPart.Type bodyPart = RagdollPart.Type.Torso)
  {
    this.brain?.instance?.GetModule<BrainModuleHitReaction>()?.ForceStaggerBehaviour(direction, pushType, bodyPart);
  }

  private void PushFallback(Creature.PushType type)
  {
    if (type == Creature.PushType.Parry)
      return;
    if (this.ragdoll.standingUp)
      this.ragdoll.CancelGetUp();
    this.ragdoll.SetState(Ragdoll.State.Destabilized);
  }

  public void PlayAnimation(string id, System.Action endCallback = null)
  {
    AnimationData outputData;
    if (!Catalog.TryGetData<AnimationData>(id, out outputData))
      return;
    this.PlayAnimation(outputData, out AnimationData.Clip[] _, endCallback);
  }

  public void PlayAnimation(AnimationData animationData, System.Action endCallback = null)
  {
    this.PlayAnimation(animationData, out AnimationData.Clip[] _, endCallback);
  }

  public void PlayAnimation(
    AnimationData animationData,
    out AnimationData.Clip[] clips,
    System.Action endCallback = null)
  {
    AnimationData.Clip clip1 = animationData.Pick(AnimationData.Clip.Step.Start);
    AnimationData.Clip clip2 = animationData.Pick(AnimationData.Clip.Step.Loop);
    AnimationData.Clip clip3 = animationData.Pick(AnimationData.Clip.Step.End);
    clips = new AnimationData.Clip[3]{ clip1, clip2, clip3 };
    if (clip1 != null && clip2 != null && clip3 != null)
      this.PlayAnimation(clip1.animationClip, clip2.animationClip, clip3.animationClip, clip3.animationSpeed, endCallback);
    else if (clip1 != null)
    {
      this.PlayAnimation(clip1.animationClip, false, clip1.animationSpeed, endCallback);
    }
    else
    {
      if (clip2 == null)
        return;
      this.PlayAnimation(clip2.animationClip, true, clip2.animationSpeed, endCallback);
    }
  }

  public void PlayAnimation(
    AnimationClip animationClip,
    bool loop,
    float speedMultiplier = 1f,
    System.Action endCallback = null,
    bool mirror = false,
    bool exitAutomatically = true,
    bool overrideAllOthers = false)
  {
    if (this.isPlayingDynamicAnimation)
      this.StopAnimation(true);
    this.animator.ResetTrigger(Creature.hashDynamicInterrupt);
    this.animator.SetBool(Creature.hashDynamicLoop, false);
    this.animator.SetBool(Creature.hashDynamicLoop3, false);
    this.animator.SetFloat(Creature.hashDynamicSpeedMultiplier, speedMultiplier);
    this.animator.SetBool(Creature.hashDynamicMirror, mirror);
    this.animator.SetBool(Creature.hashExitDynamic, exitAutomatically);
    if ((bool) (UnityEngine.Object) this.animatorOverrideController)
    {
      if (loop)
      {
        this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicLoopClip, animationClip));
        this.animator.SetBool(Creature.hashDynamicLoop, true);
      }
      else
      {
        int integer = this.animator.GetInteger(Creature.hashDynamicOneShot);
        this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>(integer == 1 ? Creature.clipIndex.dynamicStartClipB : Creature.clipIndex.dynamicStartClipA, animationClip));
        this.animator.SetInteger(Creature.hashDynamicOneShot, integer == 1 ? 2 : 1);
      }
      this.dynamicAnimationendEndCallback = endCallback;
      this.SetAnimatorBusy(true);
      this.isPlayingDynamicAnimation = true;
    }
    else
      Debug.LogError((object) "No AnimatorOverrideController found on animator!");
  }

  public void PlayUpperAnimation(
    AnimationClip animationClip,
    bool loop,
    float speedMultiplier = 1f,
    System.Action endCallback = null,
    bool mirror = false,
    bool exitAutomatically = true)
  {
    this.animator.SetBool(Creature.hashDynamicUpperLoop, false);
    this.animator.SetFloat(Creature.hashDynamicUpperMultiplier, speedMultiplier);
    this.animator.SetBool(Creature.hashDynamicUpperMirror, mirror);
    this.animator.SetBool(Creature.hashExitDynamic, exitAutomatically);
    if ((bool) (UnityEngine.Object) this.animatorOverrideController)
    {
      if (loop)
      {
        this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>(Creature.clipIndex.upperBodyDynamicLoopClip, animationClip));
        this.animator.SetBool(Creature.hashDynamicUpperLoop, true);
      }
      else
      {
        int integer = this.animator.GetInteger(Creature.hashDynamicUpperOneShot);
        this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>(integer == 1 ? Creature.clipIndex.upperBodyDynamicClipB : Creature.clipIndex.upperBodyDynamicClipA, animationClip));
        this.animator.SetInteger(Creature.hashDynamicUpperOneShot, integer == 1 ? 2 : 1);
      }
      this.dynamicAnimationendEndCallback = endCallback;
    }
    else
      Debug.LogError((object) "No AnimatorOverrideController found on animator!");
  }

  public void PlayAnimation(
    AnimationClip startAnimationClip,
    AnimationClip loopAnimationClip,
    AnimationClip endAnimationClip,
    float speedMultiplier = 1f,
    System.Action endCallback = null,
    bool exitAutomatically = true)
  {
    if (this.isPlayingDynamicAnimation)
      this.StopAnimation(true);
    this.animator.ResetTrigger(Creature.hashDynamicInterrupt);
    this.animator.SetBool(Creature.hashDynamicLoop, false);
    this.animator.SetBool(Creature.hashDynamicLoop3, false);
    this.animator.SetFloat(Creature.hashDynamicSpeedMultiplier, speedMultiplier);
    this.animator.SetBool(Creature.hashExitDynamic, exitAutomatically);
    if ((bool) (UnityEngine.Object) this.animatorOverrideController)
    {
      this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicStartClipA, startAnimationClip), new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicLoopClip, loopAnimationClip), new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicEndClip, endAnimationClip));
      this.animator.SetBool(Creature.hashDynamicLoop3, true);
      this.dynamicAnimationendEndCallback = endCallback;
      this.SetAnimatorBusy(true);
      this.isPlayingDynamicAnimation = true;
    }
    else
      Debug.LogError((object) "No AnimatorOverrideController found on animator!");
  }

  public void PlayAnimationLoopAdd(AnimationClip animationLoopAddClip)
  {
    if (!this.isPlayingDynamicAnimation || !(bool) (UnityEngine.Object) this.animatorOverrideController)
      return;
    this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicLoopAddClip, animationLoopAddClip));
    this.animator.SetTrigger(Creature.hashDynamicLoopAdd);
  }

  public void PlayAnimationLoopAdd(AnimationData animationData)
  {
    AnimationData.Clip clip = animationData.Pick(AnimationData.Clip.Step.Start);
    if (clip == null)
      return;
    this.PlayAnimationLoopAdd(clip.animationClip);
  }

  public AnimationClip GetOverrideClip(int key) => this.animationClipOverrides[key].Value;

  public void UpdateOverrideClip(
    params KeyValuePair<int, AnimationClip>[] overrides)
  {
    foreach (KeyValuePair<int, AnimationClip> keyValuePair in overrides)
      this.animationClipOverrides[keyValuePair.Key] = new KeyValuePair<AnimationClip, AnimationClip>(this.animationClipOverrides[keyValuePair.Key].Key, keyValuePair.Value);
    this.animatorOverrideController.ApplyOverrides((IList<KeyValuePair<AnimationClip, AnimationClip>>) this.animationClipOverrides);
  }

  protected void UpdateDynamicAnimation()
  {
    if (!this.isPlayingDynamicAnimation || this.IsAnimatorBusy())
      return;
    this.isPlayingDynamicAnimation = false;
    if (this.dynamicAnimationendEndCallback == null)
      return;
    this.dynamicAnimationendEndCallback();
    this.dynamicAnimationendEndCallback = (System.Action) null;
  }

  public void StopAnimation(bool interrupt = false)
  {
    if (this.isPlayingDynamicAnimation)
    {
      this.isPlayingDynamicAnimation = false;
      if (interrupt)
        this.animator.SetTrigger(Creature.hashDynamicInterrupt);
      this.animator.SetBool(Creature.hashDynamicLoop, false);
      this.animator.SetBool(Creature.hashDynamicLoop3, false);
      this.animator.SetFloat(Creature.hashDynamicSpeedMultiplier, 1f);
      if (interrupt && this.dynamicAnimationendEndCallback != null)
      {
        this.dynamicAnimationendEndCallback();
        this.dynamicAnimationendEndCallback = (System.Action) null;
      }
    }
    if (!this.animator.GetBool(Creature.hashDynamicUpperLoop) && this.animator.GetInteger(Creature.hashDynamicUpperOneShot) == 0)
      return;
    if (interrupt)
      this.animator.SetTrigger(Creature.hashDynamicInterrupt);
    this.animator.SetBool(Creature.hashDynamicUpperLoop, false);
    this.animator.SetBool(Creature.hashDynamicUpperMirror, false);
    this.animator.SetFloat(Creature.hashDynamicUpperMultiplier, 1f);
  }

  public CreatureData.EyeClip GetEyeClip(string clipName)
  {
    int count = this.eyeClips.Count;
    for (int index = 0; index < count; ++index)
    {
      CreatureData.EyeClip eyeClip = this.eyeClips[index];
      if (!(eyeClip.clipName != clipName))
        return eyeClip;
    }
    return (CreatureData.EyeClip) null;
  }

  public void PlayEyeClip(string clipName) => this.PlayEyeClip(this.GetEyeClip(clipName));

  public void PlayEyeClip(CreatureData.EyeClip eyeClip)
  {
    if (eyeClip == null || eyeClip.active)
      return;
    if (eyeClip.playAutomaticallyWhileAlive && (double) eyeClip.nextPlayDelay < 0.0)
    {
      eyeClip.lastEndTime = Time.time;
      eyeClip.nextPlayDelay = UnityEngine.Random.Range(eyeClip.minMaxBetweenPlays.x, eyeClip.minMaxBetweenPlays.y);
    }
    else
    {
      if ((double) Time.time < (double) eyeClip.lastEndTime + (double) eyeClip.nextPlayDelay + (double) eyeClip.maxIndividualDelay)
        return;
      eyeClip.active = true;
      eyeClip.lastStartTime = Time.time;
      eyeClip.lastEndTime = Time.time + eyeClip.duration;
      eyeClip.nextPlayDelay = UnityEngine.Random.Range(eyeClip.minMaxBetweenPlays.x, eyeClip.minMaxBetweenPlays.y);
      eyeClip.maxIndividualDelay = 0.0f;
      if (eyeClip.affectedEyes.Count == 0)
      {
        foreach (CreatureEye allEye in this.allEyes)
        {
          if (string.IsNullOrEmpty(eyeClip.eyeTagFilter) || allEye.eyeTag.Contains(eyeClip.eyeTagFilter))
            eyeClip.affectedEyes.Add(allEye, 0.0f);
        }
      }
      if ((double) eyeClip.minMaxDelayPerEye.y <= 0.0)
        return;
      foreach (CreatureEye key in eyeClip.affectedEyes.Keys)
      {
        float b = UnityEngine.Random.Range(eyeClip.minMaxDelayPerEye.x, eyeClip.minMaxDelayPerEye.y);
        eyeClip.affectedEyes[key] = b;
        eyeClip.maxIndividualDelay = Mathf.Max(eyeClip.maxIndividualDelay, b);
      }
    }
  }

  protected void UpdateFacialAnimation() => this.UpdateEyesAnimation();

  protected void UpdateEyesAnimation()
  {
    if (this.isPlayer && ((!((UnityEngine.Object) Mirror.local != (UnityEngine.Object) null) || !Mirror.local.isRendering ? 0 : (Mirror.local.playerHeadVisible ? 1 : 0)) != 0 || (UnityEngine.Object) Spectator.local == (UnityEngine.Object) null || Spectator.local.state == Spectator.State.Auto || Spectator.local.state == Spectator.State.FPV))
    {
      foreach (CreatureEye allEye in this.allEyes)
      {
        allEye.closeAmount = 0.0f;
        allEye.SetClose();
      }
    }
    else
    {
      if (this.eyeClips.Count == 0)
      {
        foreach (CreatureData.EyeClip eyeClip in this.data.eyeClips)
          this.eyeClips.Add(eyeClip.Clone());
      }
      for (int index = 0; index < this.eyeClips.Count; ++index)
      {
        CreatureData.EyeClip eyeClip = this.eyeClips[index];
        if (!eyeClip.active)
        {
          if ((double) Time.time > (double) eyeClip.lastEndTime + (double) eyeClip.nextPlayDelay + (double) eyeClip.maxIndividualDelay && eyeClip.playAutomaticallyWhileAlive && this.autoEyeClipsActive)
            this.PlayEyeClip(eyeClip);
          else
            continue;
        }
        bool flag = true;
        foreach (CreatureEye key in eyeClip.affectedEyes.Keys)
        {
          if ((double) key.lastUpdateTime != (double) Time.time)
          {
            key.closeAmount = 0.0f;
            key.lastUpdateTime = Time.time;
          }
          float affectedEye = eyeClip.affectedEyes[key];
          if ((double) Time.time <= (double) eyeClip.lastEndTime + (double) affectedEye)
          {
            flag = false;
            float num = Mathf.InverseLerp(eyeClip.lastStartTime + affectedEye, eyeClip.lastEndTime + affectedEye, Time.time);
            float b = 1f - eyeClip.openCurve.Evaluate(num * (float) eyeClip.openCurve.length);
            key.closeAmount = Mathf.Max(key.closeAmount, b);
          }
          else
            key.closeAmount = Mathf.Max(key.closeAmount, 1f - eyeClip.openCurve.Evaluate((float) eyeClip.openCurve.length));
          key.SetClose();
        }
        if (flag)
          eyeClip.active = false;
      }
    }
  }

  public virtual void InvokeZoneEvent(Zone zone, bool enter) => this.OnZoneEvent(zone, enter);

  public float currentHealth
  {
    get => this._currentHealth;
    set
    {
      this._currentHealth = value;
      this.PreventHealthNaN();
      this.InvokeHealthChangeEvent(this._currentHealth, this.maxHealth);
    }
  }

  public float maxHealth
  {
    get => this._currentMaxHealth * (float) (ValueHandler<float>) this.healthModifier;
    set
    {
      float num = this.currentHealth / this.maxHealth;
      this._currentMaxHealth = value;
      this.currentHealth = this.maxHealth * num;
      this.PreventHealthNaN();
      this.InvokeHealthChangeEvent(this.currentHealth, this.maxHealth);
    }
  }

  public event Creature.SetupDataEvent OnSetupDataEvent;

  public event Creature.HealthChangeEvent OnHealthChange;

  public event Creature.HealEvent OnHealEvent;

  public event Creature.ResurrectEvent OnResurrectEvent;

  public event Creature.DamageEvent OnDamageEvent;

  public event Creature.KillEvent OnKillEvent;

  public void TestKill() => this.Kill();

  public void TestDamage() => this.Damage(5f);

  private void PreventHealthNaN()
  {
    if (float.IsNaN(this.currentHealth))
      this.currentHealth = 0.0f;
    if (!float.IsNaN(this.maxHealth))
      return;
    this.maxHealth = (float) this.data.health;
  }

  public float GetDamageMultiplier()
  {
    float damageMultiplier = 1f;
    foreach (float num in this.damageMultipliers.Values)
      damageMultiplier *= num;
    return damageMultiplier;
  }

  public void SetDamageMultiplier(object handler, float value)
  {
    this.damageMultipliers[handler] = value;
  }

  public bool RemoveDamageMultiplier(object handler) => this.damageMultipliers.Remove(handler);

  public void ClearMultipliers()
  {
    this.damageMultipliers.Clear();
    this.detectionFOVModifier.Clear();
    this.hitEnvironmentDamageModifier.Clear();
    this.healthModifier.Clear();
  }

  public void Damage(float amount, DamageType type = DamageType.Energy)
  {
    this.Damage(new CollisionInstance(new DamageStruct(type, amount)));
  }

  public void Damage(CollisionInstance collisionInstance)
  {
    Creature.DamageEvent onDamageEvent1 = this.OnDamageEvent;
    if (onDamageEvent1 != null)
      onDamageEvent1(collisionInstance, EventTime.OnStart);
    EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnStart);
    if (!collisionInstance.damageStruct.active || (double) collisionInstance.damageStruct.damage == 0.0)
      return;
    Brain brain = this.brain;
    if ((brain != null ? (brain.isAttacking ? 1 : 0) : 0) != 0 && !collisionInstance.damageStruct.sliceDone && (UnityEngine.Object) collisionInstance.damageStruct.hitRagdollPart != (UnityEngine.Object) null && (collisionInstance.damageStruct.hitRagdollPart.type == RagdollPart.Type.RightHand || collisionInstance.damageStruct.hitRagdollPart.type == RagdollPart.Type.LeftHand))
      return;
    if (this.isKilled)
    {
      Creature.DamageEvent onDamageEvent2 = this.OnDamageEvent;
      if (onDamageEvent2 != null)
        onDamageEvent2(collisionInstance, EventTime.OnEnd);
      EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnEnd);
    }
    else
    {
      this.lastInteractionTime = Time.time;
      Creature creature = collisionInstance.sourceColliderGroup?.collisionHandler.item?.lastHandler?.creature;
      if (!(bool) (UnityEngine.Object) creature)
        creature = collisionInstance.casterHand?.mana.creature;
      if ((bool) (UnityEngine.Object) creature)
        this.lastInteractionCreature = creature;
      this.lastDamage = collisionInstance;
      this.lastDamageTime = Time.time;
      float num1 = collisionInstance.damageStruct.damage;
      if ((bool) (UnityEngine.Object) this.player)
      {
        if (Player.invincibility)
        {
          Creature.DamageEvent onDamageEvent3 = this.OnDamageEvent;
          if (onDamageEvent3 != null)
            onDamageEvent3(collisionInstance, EventTime.OnEnd);
          EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnEnd);
          return;
        }
        if ((bool) (UnityEngine.Object) collisionInstance.damageStruct.damager)
        {
          num1 = Mathf.Clamp(num1 * collisionInstance.damageStruct.damager.data.GetTier(collisionInstance.damageStruct.damager.collisionHandler).playerDamageMultiplier, collisionInstance.damageStruct.damager.data.playerMinDamage, collisionInstance.damageStruct.damager.data.playerMaxDamage);
          if ((double) UnityEngine.Random.value < (double) collisionInstance.damageStruct.damager.data.GetTier(collisionInstance.damageStruct.damager.collisionHandler).gripDisableChance && (double) this.data.gripRecoverTime > (double) Time.fixedDeltaTime && (double) Time.time - (double) this.handRight.climb.lastGripDisableTime > (double) Time.fixedDeltaTime * 2.0 && (double) Time.time - (double) this.handLeft.climb.lastGripDisableTime > (double) Time.fixedDeltaTime * 2.0)
          {
            if ((double) UnityEngine.Random.value < 0.5)
            {
              if ((double) this.handLeft.climb.lastGripDisableTime < 0.0)
                this.handLeft.climb.DisableGripTemp(this.data.gripRecoverTime);
              else if ((double) UnityEngine.Random.value < 0.5)
                this.handRight.climb.DisableGripTemp(this.data.gripRecoverTime);
            }
            else if ((double) this.handRight.climb.lastGripDisableTime < 0.0)
              this.handRight.climb.DisableGripTemp(this.data.gripRecoverTime);
            else if ((double) UnityEngine.Random.value < 0.5)
              this.handLeft.climb.DisableGripTemp(this.data.gripRecoverTime);
          }
        }
      }
      if (!this.isPlayer && !this.ragdoll.IsPhysicsEnabled())
        num1 *= this.data.physicsOffDamageMult;
      float num2 = num1 * this.GetDamageMultiplier();
      if (GameManager.local.collisionDebug != GameManager.CollisionDebug.None)
        Debug.Log((object) $"{this.name} took damage. Current health: {this.currentHealth}. Damage: {num2}. Resulting health: {(ValueType) (float) ((double) this.currentHealth - (double) num2)}");
      if ((double) this.currentHealth - (double) num2 <= 0.0)
      {
        this.Kill(collisionInstance);
      }
      else
      {
        this.currentHealth -= num2;
        if ((double) this.currentHealth <= 0.0)
        {
          this.currentHealth = 0.0f;
          this.Kill(collisionInstance);
        }
        else
        {
          Creature.DamageEvent onDamageEvent4 = this.OnDamageEvent;
          if (onDamageEvent4 != null)
            onDamageEvent4(collisionInstance, EventTime.OnEnd);
          EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnEnd);
        }
      }
    }
  }

  public void Kill(CollisionInstance collisionInstance)
  {
    SkillSecondWind skillData;
    if (this.isKilled || this.TryGetSkill<SkillSecondWind>(Creature.secondWindId, out skillData) && skillData.ConsumeCharge(this, collisionInstance))
      return;
    EventManager.InvokeCreatureKill(this, this.player, collisionInstance, EventTime.OnStart);
    if (this.OnKillEvent != null)
      this.OnKillEvent(collisionInstance, EventTime.OnStart);
    Player player = this.player;
    int state = (int) this.state;
    if ((bool) (UnityEngine.Object) this.player || this.brain.instance == null || !this.brain.instance.isActive)
    {
      if ((bool) (UnityEngine.Object) this.handRight)
        this.handRight.TryRelease();
      if ((bool) (UnityEngine.Object) this.handLeft)
        this.handLeft.TryRelease();
    }
    if ((bool) (UnityEngine.Object) this.handLeft?.caster && this.handLeft.caster.telekinesis != null)
      this.handLeft.caster.telekinesis.TryRelease();
    if ((bool) (UnityEngine.Object) this.handRight?.caster && this.handRight.caster.telekinesis != null)
      this.handRight.caster.telekinesis.TryRelease();
    if ((bool) (UnityEngine.Object) this.player)
      this.player.ReleaseCreature();
    this.ragdoll.SetState(Ragdoll.State.Inert);
    this.RefreshFallState(Creature.FallState.None, true);
    this.lastInteractionTime = Time.time;
    if ((bool) (UnityEngine.Object) collisionInstance.sourceColliderGroup?.collisionHandler.item?.lastHandler)
      this.lastInteractionCreature = collisionInstance.sourceColliderGroup.collisionHandler.item.lastHandler.creature;
    this.isKilled = true;
    this.currentHealth = 0.0f;
    this.brain.Stop();
    this.autoEyeClipsActive = false;
    this.locomotion.ClearPhysicModifiers();
    this.locomotion.ClearSpeedModifiers();
    this.spawnGroup = (WaveData.Group) null;
    if ((bool) (UnityEngine.Object) player)
      Player.characterData.inventory.LoadCurrencies();
    if (this.OnKillEvent != null)
      this.OnKillEvent(collisionInstance, EventTime.OnEnd);
    EventManager.InvokeCreatureKill(this, player, collisionInstance, EventTime.OnEnd);
    if (this.initialArea == null)
      return;
    this.initialArea.OnCreatureKill(this.areaSpawnerIndex);
  }

  public void Heal(float heal, Creature healer)
  {
    if ((double) this.currentHealth == (double) this.maxHealth)
      return;
    if (this.OnHealEvent != null)
      this.OnHealEvent(heal, healer, EventTime.OnStart);
    EventManager.InvokeCreatureHeal(this, heal, healer, EventTime.OnStart);
    this.currentHealth += heal;
    if ((double) this.currentHealth >= (double) this.maxHealth)
      this.currentHealth = this.maxHealth;
    this.lastInteractionTime = Time.time;
    this.lastInteractionCreature = healer;
    if (this.OnHealEvent != null)
      this.OnHealEvent(heal, healer, EventTime.OnEnd);
    EventManager.InvokeCreatureHeal(this, heal, healer, EventTime.OnEnd);
  }

  public void Resurrect(float newHealth, Creature resurrector)
  {
    if (!this.isKilled)
      return;
    if (this.OnResurrectEvent != null)
      this.OnResurrectEvent(newHealth, resurrector, EventTime.OnStart);
    this.currentHealth = newHealth;
    this.isKilled = false;
    this.lastInteractionTime = Time.time;
    this.lastInteractionCreature = resurrector;
    this.ragdoll.SetState(Ragdoll.State.Destabilized, true);
    if (this.brain.instance != null)
      this.brain.instance.Start();
    this.autoEyeClipsActive = true;
    if (this.OnResurrectEvent == null)
      return;
    this.OnResurrectEvent(newHealth, resurrector, EventTime.OnEnd);
  }

  public void ToogleTPose()
  {
    this.toogleTPose = !this.toogleTPose;
    this.animator.SetBool(Creature.hashTstance, this.toogleTPose);
  }

  public void RefreshMorphology()
  {
    this.animator.SetBool(Creature.hashTstance, true);
    this.animator.Update(0.0f);
    UnityEngine.Vector3 vector3 = UnityEngine.Vector3.zero;
    if ((bool) (UnityEngine.Object) this.handLeft && (bool) (UnityEngine.Object) this.handRight)
    {
      vector3 = this.transform.InverseTransformPoint((this.handLeft.upperArmPart.bone.animation.position + this.handRight.upperArmPart.bone.animation.position) / 2f);
      this.morphology.armsLength = (float) (((double) UnityEngine.Vector3.Distance(this.transform.InverseTransformPoint(this.handLeft.upperArmPart.bone.animation.position), this.transform.InverseTransformPoint(this.handLeft.grip.position)) + (double) UnityEngine.Vector3.Distance(this.transform.InverseTransformPoint(this.handRight.upperArmPart.bone.animation.position), this.transform.InverseTransformPoint(this.handRight.grip.position))) / 2.0);
      this.morphology.armsHeight = vector3.y;
      this.morphology.armsSpacing = UnityEngine.Vector3.Distance(this.transform.InverseTransformPoint(this.handLeft.upperArmPart.bone.animation.position), this.transform.InverseTransformPoint(this.handRight.upperArmPart.bone.animation.position));
    }
    if ((bool) (UnityEngine.Object) this.footLeft && (bool) (UnityEngine.Object) this.footRight)
    {
      this.morphology.legsLength = (float) (((double) this.footLeft.GetCurrentLegDistance(Space.Self) + (double) this.footRight.GetCurrentLegDistance(Space.Self)) / 2.0);
      this.morphology.legsSpacing = UnityEngine.Vector3.Distance(this.transform.InverseTransformPoint((bool) (UnityEngine.Object) this.footLeft.upperLegBone ? this.footLeft.upperLegBone.position : this.footLeft.lowerLegBone.position), this.transform.InverseTransformPoint((bool) (UnityEngine.Object) this.footRight.upperLegBone ? this.footRight.upperLegBone.position : this.footRight.lowerLegBone.position));
    }
    this.morphology.eyesHeight = this.transform.InverseTransformPoint(this.centerEyes.position).y;
    this.morphology.headHeight = this.transform.InverseTransformPoint(this.ragdoll.headPart.bone.animation.position).y;
    this.morphology.headForward = Mathf.Abs(this.transform.InverseTransformPoint(this.ragdoll.headPart.bone.animation.position).z - vector3.z);
    this.morphology.eyesForward = Mathf.Abs(this.transform.InverseTransformPoint(this.centerEyes.position).z - vector3.z);
    this.morphology.hipsHeight = this.transform.InverseTransformPoint(this.ragdoll.rootPart.bone.animation.position).y;
    if (this.animator.isHuman)
    {
      this.morphology.chestHeight = this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.Chest).position).y;
      this.morphology.spineHeight = this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.Spine).position).y;
      this.morphology.upperLegsHeight = (float) (((double) this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position).y + (double) this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position).y) / 2.0);
      this.morphology.lowerLegsHeight = (float) (((double) this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position).y + (double) this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position).y) / 2.0);
      this.morphology.footHeight = (float) (((double) this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position).y + (double) this.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.RightFoot).position).y) / 2.0);
    }
    this.morphology.height = Morphology.GetHeight(this.morphology.eyesHeight);
    this.animator.SetBool(Creature.hashTstance, false);
  }

  public virtual void UpdateStep(
    UnityEngine.Vector3 position,
    float stepSpeedMultiplier = 1f,
    float stepThresholdMultiplier = 1f)
  {
    UnityEngine.Vector3 a = new UnityEngine.Vector3(position.x, this.transform.position.y, position.z);
    if ((double) UnityEngine.Vector3.Distance(a, this.transform.position) > (double) this.stepThreshold * (double) stepThresholdMultiplier)
      this.stepTargetPos = a;
    UnityEngine.Vector3 vector3 = (this.stepTargetPos - this.transform.position) * (this.locomotion.forwardSpeed * stepSpeedMultiplier);
    this.locomotion.moveDirection = new UnityEngine.Vector3(vector3.x, 0.0f, vector3.z);
  }

  public virtual void UpdateFall()
  {
    if (!this.loaded || this.state == Creature.State.Dead || (UnityEngine.Object) AreaManager.Instance != (UnityEngine.Object) null && AreaManager.Instance.CurrentArea != null && (this.currentArea == null || !this.currentArea.IsSpawned || !this.currentArea.SpawnedArea.initialized || this.currentArea.SpawnedArea.isCulled))
      return;
    if (this.currentLocomotion.isGrounded && this.state == Creature.State.Alive)
      this.RefreshFallState(Creature.FallState.None);
    else if (this.state == Creature.State.Destabilized)
    {
      if (this.ragdoll.SphereCastGround(this.locomotion.capsuleCollider.radius, this.morphology.hipsHeight, out RaycastHit _, out float _))
      {
        if ((double) this.ragdoll.rootPart.physicBody.velocity.magnitude < (double) this.groundStabilizationMaxVelocity)
        {
          this.groundStabilizeDuration += Time.deltaTime;
          if ((double) this.groundStabilizeDuration > (double) this.groundStabilizationMinDuration)
            this.RefreshFallState(Creature.FallState.StabilizedOnGround);
          else
            this.RefreshFallState(Creature.FallState.Stabilizing);
        }
        else
          this.RefreshFallState(Creature.FallState.NearGround);
      }
      else
        this.RefreshFallState(Creature.FallState.Falling);
    }
    else
    {
      float groundDistance;
      if (this.currentLocomotion.SphereCastGround(this.fallAliveDestabilizeHeight, out RaycastHit _, out groundDistance))
      {
        if ((double) groundDistance < (double) this.fallAliveAnimationHeight)
          this.RefreshFallState(Creature.FallState.NearGround);
        else
          this.RefreshFallState(Creature.FallState.Falling);
      }
      else
      {
        bool flag = false;
        float waterHeight;
        if (Water.exist && Water.current.TryGetWaterHeight(this.currentLocomotion.groundHit.point, out waterHeight))
          flag = (double) this.currentLocomotion.groundHit.point.y < (double) waterHeight - (double) this.morphology.height * (double) this.swimFallAnimationRatio;
        if (!(bool) (UnityEngine.Object) this.player && !flag && this.data.destabilizeOnFall)
          this.ragdoll.SetState(Ragdoll.State.Destabilized);
        this.RefreshFallState(Creature.FallState.Falling);
      }
    }
  }

  protected void RefreshFallState(Creature.FallState newState, bool force = false)
  {
    if (!force && this.fallState == newState)
      return;
    this.fallState = newState;
    switch (newState)
    {
      case Creature.FallState.None:
        this.animator.SetBool(Creature.hashFalling, false);
        this.groundStabilizeDuration = 0.0f;
        break;
      case Creature.FallState.NearGround:
        this.animator.SetBool(Creature.hashFalling, this.state != Creature.State.Alive);
        break;
      case Creature.FallState.Stabilizing:
        this.animator.SetBool(Creature.hashFalling, true);
        break;
      case Creature.FallState.StabilizedOnGround:
        this.groundStabilizationLastTime = Time.time;
        this.animator.SetBool(Creature.hashFalling, true);
        break;
      default:
        this.animator.SetBool(Creature.hashFalling, true);
        this.groundStabilizeDuration = 0.0f;
        break;
    }
    if (this.OnFallEvent == null)
      return;
    this.OnFallEvent(this.fallState);
  }

  public virtual void SetAnimatorHeightRatio(float height)
  {
    this.animator.SetFloat(Creature.hashHeight, height);
  }

  public virtual void SetGrabbedObjectLayer(LayerName layerName)
  {
    if ((bool) (UnityEngine.Object) this.handLeft?.grabbedHandle?.item)
      this.handLeft.grabbedHandle.item.SetColliderAndMeshLayer(GameManager.GetLayer(layerName));
    if (!(bool) (UnityEngine.Object) this.handRight?.grabbedHandle?.item)
      return;
    this.handRight.grabbedHandle.item.SetColliderAndMeshLayer(GameManager.GetLayer(layerName));
  }

  public virtual void RefreshCollisionOfGrabbedItems()
  {
    if ((bool) (UnityEngine.Object) this.handLeft?.grabbedHandle?.item)
      this.handLeft.grabbedHandle.item.RefreshCollision();
    if (!(bool) (UnityEngine.Object) this.handRight?.grabbedHandle?.item)
      return;
    this.handRight.grabbedHandle.item.RefreshCollision();
  }

  public bool TryGetVfxManikinPart(string channel, out ManikinSmrPart part)
  {
    part = (ManikinSmrPart) null;
    List<ManikinPart> partsAtChannel = this.manikinLocations.GetPartsAtChannel(channel);
    if (partsAtChannel == null || partsAtChannel.Count == 0)
      return false;
    for (int index = 0; index < partsAtChannel.Count; ++index)
    {
      ManikinPart manikinPart = partsAtChannel[index];
      if (manikinPart.isActiveAndEnabled && manikinPart is ManikinGroupPart manikinGroupPart)
      {
        foreach (ManikinPart childPart in manikinGroupPart.ChildParts)
        {
          if (childPart.isActiveAndEnabled && childPart is ManikinSmrPart manikinSmrPart && manikinSmrPart.GetSkinnedMeshRenderer().enabled)
          {
            part = manikinSmrPart;
            return true;
          }
        }
      }
    }
    return false;
  }

  public void SetBodyMaterials(UnityEngine.Material[] bodyMaterials)
  {
    if (bodyMaterials == null || bodyMaterials.Length == 0)
      return;
    for (int index = 0; index < bodyMaterials.Length; ++index)
    {
      ManikinProperty manikinProperty;
      if (this.TryGetManikinProperty($"EthnicGroupMaterialBodyLOD{index}", out manikinProperty))
        this.manikinProperties.TryUpdateProperty(bodyMaterials[index], manikinProperty.set, true);
    }
  }

  public void SetHandsMaterials(UnityEngine.Material[] handsMaterials)
  {
    if (handsMaterials == null || handsMaterials.Length == 0)
      return;
    for (int index = 0; index < handsMaterials.Length; ++index)
    {
      ManikinProperty manikinProperty;
      if (this.TryGetManikinProperty($"EthnicGroupMaterialHandsLOD{index}", out manikinProperty))
        this.manikinProperties.TryUpdateProperty(handsMaterials[index], manikinProperty.set, true);
    }
  }

  /// <summary>Return the current ethnic group from it's id</summary>
  /// <param name="id">Id of the ethnic group to seartch</param>
  /// <returns>The matching ethnic group if found, the first one if not</returns>
  public CreatureData.EthnicGroup GetEthnicGroupFromId(string id)
  {
    for (int index = 0; index < this.data.ethnicGroups.Count; ++index)
    {
      if (this.data.ethnicGroups[index].id == id)
        return this.data.ethnicGroups[index];
    }
    return this.data.ethnicGroups[0];
  }

  /// <summary>
  /// Sets the ethnic group of the creature.
  /// It will change its head wardrobe part if the current one doesn't match
  /// </summary>
  /// <param name="creatureDataEthnicGroup">Ethnic group to change to.</param>
  public void SetEthnicGroup(CreatureData.EthnicGroup creatureDataEthnicGroup)
  {
    if (creatureDataEthnicGroup == null)
      return;
    this.currentEthnicGroup = creatureDataEthnicGroup;
    ManikinWardrobeData wardrobeData = this.manikinLocations.GetWardrobeData("Head", ItemModuleWardrobe.GetLayer("Head", "Body"));
    ItemData itemData = (ItemData) null;
    foreach (ItemData data in Catalog.GetDataList<ItemData>())
    {
      ItemModuleWardrobe module = data.GetModule<ItemModuleWardrobe>();
      if (module != null && module.category == Equipment.WardRobeCategory.Body)
      {
        for (int index = 0; index < module.wardrobes.Count; ++index)
        {
          if ((UnityEngine.Object) module.wardrobes[index].manikinWardrobeData == (UnityEngine.Object) wardrobeData)
          {
            itemData = data;
            break;
          }
        }
      }
    }
    if (itemData == null)
      return;
    bool flag = false;
    if (creatureDataEthnicGroup.allowedHeadsIDs != null)
    {
      for (int index = 0; index < creatureDataEthnicGroup.allowedHeadsIDs.Count; ++index)
      {
        if (creatureDataEthnicGroup.allowedHeadsIDs[index] == itemData.id)
        {
          flag = true;
          break;
        }
      }
    }
    if (!flag && creatureDataEthnicGroup.allowedHeadsIDs != null)
    {
      ItemModuleWardrobe itemModuleWardrobe = (ItemModuleWardrobe) null;
      foreach (ItemData data in Catalog.GetDataList<ItemData>())
      {
        ItemModuleWardrobe module = data.GetModule<ItemModuleWardrobe>();
        if (module != null && module.category == Equipment.WardRobeCategory.Body)
        {
          ItemModuleWardrobe.CreatureWardrobe wardrobe = module.GetWardrobe(this);
          if (wardrobe != null && (UnityEngine.Object) wardrobe.manikinWardrobeData != (UnityEngine.Object) null && module.IsCompatible(this))
          {
            for (int index = 0; index < creatureDataEthnicGroup.allowedHeadsIDs.Count; ++index)
            {
              if (creatureDataEthnicGroup.allowedHeadsIDs[index] == module.itemData.id)
              {
                itemModuleWardrobe = module;
                break;
              }
            }
          }
        }
      }
      if (itemModuleWardrobe != null)
        this.equipment.EquipWardrobe(new ItemContent(itemModuleWardrobe.itemData));
    }
    this.SetBodyMaterials(new UnityEngine.Material[2]
    {
      creatureDataEthnicGroup.bodyMaterialLod0,
      creatureDataEthnicGroup.bodyMaterialLod1
    });
    this.SetHandsMaterials(new UnityEngine.Material[2]
    {
      creatureDataEthnicGroup.handsMaterialLod0,
      creatureDataEthnicGroup.handsMaterialLod1
    });
    this.manikinProperties.UpdateProperties();
  }

  /// <summary>
  /// Update the property of the creature when the head changed
  /// </summary>
  /// <param name="eventTime"></param>
  private void UpdateManikinAfterHeadChange(EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    this.manikinProperties.UpdateProperties();
    if (!this.isPlayer)
      return;
    foreach (ManikinPart manikinPart in this.manikinLocations.GetPartsAtChannel("Head"))
    {
      if (manikinPart.name.Contains("Eyes"))
      {
        foreach (Renderer renderer in manikinPart.GetRenderers())
          renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
      }
    }
  }

  public bool AddJointForceMultiplier(object handler, float position, float rotation)
  {
    (float position, float rotation) tuple1;
    if (this.jointForceMultipliers.TryGetValue(handler, out tuple1))
    {
      (float position, float rotation) tuple2 = tuple1;
      float num1 = position;
      float num2 = rotation;
      if ((double) tuple2.position == (double) num1 && (double) tuple2.rotation == (double) num2)
        return false;
    }
    this.jointForceMultipliers[handler] = (position, rotation);
    this.RefreshJointForceMultipliers();
    return true;
  }

  public void RemoveJointForceMultiplier(object handler)
  {
    if (!this.jointForceMultipliers.ContainsKey(handler))
      return;
    this.jointForceMultipliers.Remove(handler);
    this.RefreshJointForceMultipliers();
  }

  public void ClearJointForceMultipliers()
  {
    int num = this.jointForceMultipliers.Count > 0 ? 1 : 0;
    this.jointForceMultipliers.Clear();
    if (num == 0)
      return;
    this.RefreshJointForceMultipliers();
  }

  public void RefreshJointForceMultipliers()
  {
    this.jointPosForceMult = 1f;
    this.jointRotForceMult = 1f;
    foreach ((float position, float rotation) in this.jointForceMultipliers.Values)
    {
      this.jointPosForceMult *= position;
      this.jointRotForceMult *= rotation;
    }
    this.handLeft.grabbedHandle?.RefreshJointModifiers();
    this.handLeft.playerHand?.link.RefreshJointModifiers();
    this.handRight.grabbedHandle?.RefreshJointModifiers();
    this.handRight.playerHand?.link.RefreshJointModifiers();
  }

  public UnityEngine.Vector3 GetPositionJointConfig()
  {
    return new UnityEngine.Vector3(this.data.forcePositionSpringDamper.x * this.jointPosForceMult, this.data.forcePositionSpringDamper.y * Mathf.Max(1f, this.jointPosForceMult / 2f), this.jointPosForceMult);
  }

  public UnityEngine.Vector3 GetRotationJointConfig()
  {
    return new UnityEngine.Vector3(this.data.forceRotationSpringDamper.x * this.jointRotForceMult, this.data.forceRotationSpringDamper.y * Mathf.Max(1f, this.jointRotForceMult / 2f), this.jointRotForceMult);
  }

  public enum StaggerAnimation
  {
    Default,
    Parry,
    Head,
    Torso,
    Legs,
    FallGround,
    Riposte,
  }

  public enum PushType
  {
    Magic,
    Grab,
    Hit,
    Parry,
  }

  public enum FallState
  {
    None,
    Falling,
    NearGround,
    Stabilizing,
    StabilizedOnGround,
  }

  public delegate void FallEvent(Creature.FallState state);

  public delegate void ForceSkillLoadEvent(SkillData skill);

  public delegate void ImbueChangeEvent(
    Creature creature,
    HashSet<string> before,
    HashSet<string> after);

  public delegate void DespawnEvent(EventTime eventTime);

  public delegate void ThrowEvent(RagdollHand hand, Handle handle);

  public delegate void ThisCreatureAttackEvent(
    Creature targetCreature,
    Transform targetTransform,
    BrainModuleAttack.AttackType type,
    BrainModuleAttack.AttackStage stage);

  public class RendererData
  {
    public SkinnedMeshRenderer renderer;
    public SkinnedMeshRenderer splitRenderer;
    public MeshPart meshPart;
    public ManikinPart manikinPart;
    public RevealDecal revealDecal;
    public RevealDecal splitReveal;
    public int lod;

    public RendererData(
      SkinnedMeshRenderer renderer,
      int lod,
      MeshPart meshPart = null,
      RevealDecal revealDecal = null,
      ManikinPart manikinPart = null)
    {
      this.renderer = renderer;
      this.lod = lod;
      this.meshPart = meshPart;
      this.revealDecal = revealDecal;
      this.manikinPart = manikinPart;
    }
  }

  public delegate void ZoneEvent(Zone zone, bool enter);

  public delegate void SimpleDelegate();

  public enum State
  {
    Dead,
    Destabilized,
    Alive,
  }

  public enum ProtectToAim
  {
    Protect,
    Idle,
    Aim,
  }

  public enum AnimFootStep
  {
    Slow,
    Walk,
    Run,
  }

  public enum ColorModifier
  {
    Hair,
    HairSecondary,
    HairSpecular,
    EyesIris,
    EyesSclera,
    Skin,
  }

  public class ReplaceClipIndexHolder
  {
    public int count { get; protected set; }

    public int dynamicStartClipA { get; protected set; }

    public int dynamicStartClipB { get; protected set; }

    public int dynamicLoopClip { get; protected set; }

    public int dynamicLoopAddClip { get; protected set; }

    public int dynamicEndClip { get; protected set; }

    public int upperBodyDynamicClipA { get; protected set; }

    public int upperBodyDynamicClipB { get; protected set; }

    public int upperBodyDynamicLoopClip { get; protected set; }

    public int subStanceClipA { get; protected set; }

    public int subStanceClipB { get; protected set; }

    public int upperLeftGuard { get; protected set; }

    public int upperRightGuard { get; protected set; }

    public int leftGuard { get; protected set; }

    public int midGuard { get; protected set; }

    public int rightGuard { get; protected set; }

    public int lowerLeftGuard { get; protected set; }

    public int lowerRightGuard { get; protected set; }

    public ReplaceClipIndexHolder()
    {
      this.count = 0;
      this.dynamicStartClipA = this.count++;
      this.dynamicStartClipB = this.count++;
      this.dynamicLoopClip = this.count++;
      this.dynamicLoopAddClip = this.count++;
      this.dynamicEndClip = this.count++;
      this.upperBodyDynamicClipA = this.count++;
      this.upperBodyDynamicClipB = this.count++;
      this.upperBodyDynamicLoopClip = this.count++;
      this.subStanceClipA = this.count++;
      this.subStanceClipB = this.count++;
      this.upperLeftGuard = this.count++;
      this.upperRightGuard = this.count++;
      this.leftGuard = this.count++;
      this.midGuard = this.count++;
      this.rightGuard = this.count++;
      this.lowerLeftGuard = this.count++;
      this.lowerRightGuard = this.count++;
    }
  }

  public delegate void SetupDataEvent(EventTime eventTime);

  public delegate void HealthChangeEvent(float health, float maxHealth);

  public delegate void HealEvent(float heal, Creature healer, EventTime eventTime);

  public delegate void ResurrectEvent(float newHealth, Creature resurrector, EventTime eventTime);

  public delegate void DamageEvent(CollisionInstance collisionInstance, EventTime eventTime);

  public delegate void KillEvent(CollisionInstance collisionInstance, EventTime eventTime);
}
