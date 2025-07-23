// Decompiled with JetBrains decompiler
// Type: ThunderRoad.SkillTreeCrystal
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Pools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

#nullable disable
namespace ThunderRoad;

public class SkillTreeCrystal : ThunderBehaviour
{
  public RagdollHand hand;
  public HandlePose floatHandPose;
  [Header("Merge VFx")]
  public AnimationCurve glowCurve;
  public float timeGlowTransition = 1f;
  public float timeVfxTransition = 2f;
  public float timeVfxTransitionMin = 0.5f;
  [FormerlySerializedAs("mergeVfx")]
  public VisualEffect mergeVfxWindows;
  [FormerlySerializedAs("mergeVfxTarget")]
  public Transform mergeVfxTargetWindows;
  [FormerlySerializedAs("linkVfx")]
  public VisualEffect linkVfxWindows;
  [FormerlySerializedAs("linkVfxTarget")]
  public Transform linkVfxTargetWindows;
  public VisualEffect mergeVfxAndroid;
  public Transform mergeVfxTargetAndroid;
  public VisualEffect linkVfxAndroid;
  public Transform linkVfxTargetAndroid;
  protected VisualEffect mergeVfx;
  protected Transform mergeVfxTarget;
  protected VisualEffect linkVfx;
  protected Transform linkVfxTarget;
  public float linkMaxDistance = 2f;
  [Header("Skill tree")]
  public string treeName;
  [ColorUsage(true, true)]
  public UnityEngine.Color skillTreeEmissionColor = UnityEngine.Color.white;
  [Header("Custom")]
  public bool overrideCrystalColors;
  [ColorUsage(true, true)]
  public UnityEngine.Color baseColor;
  [ColorUsage(true, true)]
  public UnityEngine.Color internalColor;
  [ColorUsage(true, true)]
  public UnityEngine.Color animatedColor;
  [ColorUsage(true, true)]
  public UnityEngine.Color emissionColor;
  [ColorUsage(true, true)]
  public UnityEngine.Color linkVfxColor;
  [ColorUsage(true, true)]
  public UnityEngine.Color mergeVfxColor;
  public EffectData sparkleEffectData;
  public EffectInstance sparkleEffect;
  [NonSerialized]
  public EffectData hoverEffectData;
  public static Dictionary<string, HashSet<SkillTreeCrystal>> crystalsOfType = new Dictionary<string, HashSet<SkillTreeCrystal>>();
  public static List<SkillTreeCrystal> allCrystals = new List<SkillTreeCrystal>();
  [NonSerialized]
  public Item item;
  [NonSerialized]
  public SkillTreeReceptacle receptacle;
  [NonSerialized]
  public bool merging;
  [NonSerialized]
  protected Transform mergePoint;
  public EffectData mergeBeginEffectData;
  protected EffectInstance mergeBeginEffect;
  public EffectData mergeEffectData;
  public EffectData mergeCrystalEffectAndroidData;
  protected EffectInstance mergeEffect;
  protected EffectInstance mergeCrystalEffect;
  protected EffectInstance activateEffect;
  protected float mergeStartTime;
  protected SkillTreeCrystal mergingCrystal;
  public ItemModuleCrystal module;
  private MaterialInstance[] materialInstances;
  [NonSerialized]
  public bool mergeVfxActive;
  [NonSerialized]
  public bool linkVfxActive;
  private bool glowing;
  private float glowTime;
  private float glowIntensity;
  private float timeLerpLinkVfx;
  private static readonly int GlowProperty = Shader.PropertyToID("_Glow");
  private static readonly int CrystalColor = Shader.PropertyToID("_Color");
  private static readonly int InternalColor = Shader.PropertyToID("_InternalColor");
  private static readonly int AnimatedColor = Shader.PropertyToID("_InternalAnimatedColor");
  private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
  private static readonly int VertexLerpAmount = Shader.PropertyToID("_VertexLerpAmount");
  private static readonly int VertexLerpTarget = Shader.PropertyToID("_VertexLerpTarget");
  private static readonly int Intensity = Shader.PropertyToID(nameof (Intensity));
  public static bool isMerging;
  public bool isSocketed;

  public override ManagedLoops EnabledManagedLoops
  {
    get => ManagedLoops.FixedUpdate | ManagedLoops.Update;
  }

  private void Awake()
  {
    if (this.TryGetComponent<Item>(out this.item))
    {
      this.item.OnHeldActionEvent += new Item.HeldActionDelegate(this.HeldAction);
      this.item.OnDespawnEvent += new Item.SpawnEvent(this.CrystalDespawned);
      this.item.OnUngrabEvent += new Item.ReleaseDelegate(this.OnUnGrabEvent);
    }
    else
      Debug.LogError((object) $"SkillTreeCrystal: [{this.name}] - No Item component found");
    if (Common.IsWindows)
    {
      this.mergeVfx = this.mergeVfxWindows;
      this.mergeVfxTarget = this.mergeVfxTargetWindows;
      this.linkVfx = this.linkVfxWindows;
      this.linkVfxTarget = this.linkVfxTargetWindows;
    }
    else
    {
      this.mergeVfx = this.mergeVfxAndroid;
      this.mergeVfxTarget = this.mergeVfxTargetAndroid;
      this.linkVfx = this.linkVfxAndroid;
      this.linkVfxTarget = this.linkVfxTargetAndroid;
    }
    this.mergePoint = new GameObject("MergePoint").transform;
    this.mergePoint.SetParent(this.transform);
    MeshRenderer[] componentsInChildren = this.GetComponentsInChildren<MeshRenderer>(true);
    this.materialInstances = new MaterialInstance[componentsInChildren.Length];
    for (int index = 0; index < componentsInChildren.Length; ++index)
    {
      this.materialInstances[index] = componentsInChildren[index].GetOrAddComponent<MaterialInstance>();
      this.materialInstances[index].AcquireMaterials();
    }
    this.UpdateVertexLerp(0.0f);
  }

  /// Stop effects (and prevent merge in the early stages) on ungrab
  private void OnUnGrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
  {
    this.SetGlow(false);
    if ((UnityEngine.Object) this.hand != (UnityEngine.Object) null || !(bool) (UnityEngine.Object) this.mergingCrystal)
      return;
    this.DisconnectMergeVFX();
    this.mergingCrystal.DisconnectMergeVFX();
    this.mergingCrystal.mergingCrystal = (SkillTreeCrystal) null;
    this.mergingCrystal = (SkillTreeCrystal) null;
    this.mergeBeginEffect?.End();
    this.mergeBeginEffect = (EffectInstance) null;
  }

  /// Tidy up after ourselves
  private void CrystalDespawned(EventTime eventTime)
  {
    this.mergeVfx?.transform.parent.SetParent(this.transform);
    this.item.OnUngrabEvent -= new Item.ReleaseDelegate(this.OnUnGrabEvent);
    this.item.OnHeldActionEvent -= new Item.HeldActionDelegate(this.HeldAction);
    this.item.OnDespawnEvent -= new Item.SpawnEvent(this.CrystalDespawned);
    this.activateEffect?.Despawn();
    this.activateEffect = (EffectInstance) null;
    SkillTreeCrystal.crystalsOfType.RemoveFromKeyedList<string, HashSet<SkillTreeCrystal>, SkillTreeCrystal>(this.treeName, this);
    SkillTreeCrystal.allCrystals.Remove(this);
  }

  /// Test if we can merge, and set up the conditions for the merge to start
  public void TestMerge()
  {
    Item obj = this.item.mainHandler?.otherHand.grabbedHandle?.item;
    SkillTreeCrystal component;
    if ((UnityEngine.Object) obj == (UnityEngine.Object) null || (UnityEngine.Object) obj == (UnityEngine.Object) this.item || (UnityEngine.Object) this.mergingCrystal != (UnityEngine.Object) null || !obj.TryGetComponent<SkillTreeCrystal>(out component) || !component.treeName.Equals(this.treeName) || !this.glowing || !component.glowing)
      return;
    this.mergingCrystal = component;
    this.mergingCrystal.mergingCrystal = this;
    this.mergingCrystal.mergeStartTime = this.mergeStartTime = Time.time;
  }

  private void HeldAction(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
  {
    if (action != Interactable.Action.UseStart && action != Interactable.Action.UseStop || this.item.handlers.CountCheck((Func<int, bool>) (count => count > 1)))
      return;
    this.SetGlow(action == Interactable.Action.UseStart);
  }

  public bool HasOtherGlowingCrystal
  {
    get
    {
      Item obj = this.item.mainHandler?.otherHand.grabbedHandle?.item;
      SkillTreeCrystal component;
      return obj != null && (UnityEngine.Object) obj != (UnityEngine.Object) this.item && obj.TryGetComponent<SkillTreeCrystal>(out component) && component.treeName.Equals(this.treeName) && component.glowing;
    }
  }

  public void SetGlow(bool active)
  {
    if (this.glowing == active)
      return;
    this.glowing = active;
    this.ToggleSparkles(this.glowing);
    if (active && !this.HasOtherGlowingCrystal)
    {
      this.activateEffect?.End();
      this.activateEffect = this.module.activateEffectData?.Spawn(this.transform);
      this.activateEffect?.Play();
    }
    else
      this.activateEffect?.End();
  }

  /// Connect this crystal to its merge point
  private void ConnectMergeVFX(Transform target)
  {
    this.mergeVfxTarget.parent = target;
    this.mergeVfxTarget.localPosition = UnityEngine.Vector3.zero;
    this.mergeVfx.gameObject.SetActive(true);
    this.mergeVfxActive = true;
  }

  /// Stop all merge VFX
  private void DisconnectMergeVFX()
  {
    if (!this.mergeVfxActive && this.mergeBeginEffect == null)
      return;
    this.mergeVfx.gameObject.SetActive(false);
    this.mergeVfxActive = false;
    if (!(bool) (UnityEngine.Object) this.mergeVfxTarget)
      return;
    this.mergeVfxTarget.parent = this.mergeVfx.transform;
    this.mergeBeginEffect?.End();
    this.mergeBeginEffect = (EffectInstance) null;
    this.mergeCrystalEffect?.End();
    this.mergeCrystalEffect = (EffectInstance) null;
    this.mergeEffect?.End();
    this.mergeEffect = (EffectInstance) null;
    if ((UnityEngine.Object) this.mergingCrystal == (UnityEngine.Object) null)
      return;
    this.mergingCrystal.mergeBeginEffect?.End();
    this.mergingCrystal.mergeBeginEffect = (EffectInstance) null;
    this.mergingCrystal.mergeEffect?.End();
    this.mergingCrystal.mergeEffect = (EffectInstance) null;
    this.mergingCrystal.mergeCrystalEffect?.End();
    this.mergingCrystal.mergeCrystalEffect = (EffectInstance) null;
    this.mergingCrystal.DisconnectMergeVFX();
  }

  public void Init()
  {
    SkillTreeCrystal.crystalsOfType.AddToKeyedList<string, HashSet<SkillTreeCrystal>, SkillTreeCrystal>(this.treeName, this);
    SkillTreeCrystal.allCrystals.Add(this);
    this.UpdateVertexLerp(0.0f);
    if (!this.overrideCrystalColors)
      return;
    for (int index = 0; index < this.materialInstances.Length; ++index)
    {
      this.materialInstances[index].material.SetColor(SkillTreeCrystal.CrystalColor, this.baseColor);
      this.materialInstances[index].material.SetColor(SkillTreeCrystal.InternalColor, this.internalColor);
      this.materialInstances[index].material.SetColor(SkillTreeCrystal.AnimatedColor, this.animatedColor);
      this.materialInstances[index].material.SetColor(SkillTreeCrystal.EmissionColor, this.emissionColor);
    }
    this.linkVfx.SetVector4("Source Color", (Vector4) this.linkVfxColor);
    this.mergeVfx.SetVector4("Source Color", (Vector4) this.mergeVfxColor);
  }

  /// <summary>
  /// Returns true if the skill is on the same tree or on a combining tree
  /// </summary>
  /// <param name="skill"></param>
  /// <returns></returns>
  public bool IsSkillOnTree(SkillData skill)
  {
    return skill.primarySkillTreeId == this.treeName || skill.secondarySkillTreeId == this.treeName;
  }

  protected internal override void ManagedUpdate()
  {
    this.UpdateCrystalsLink();
    this.UpdateCrystalGlow();
    this.TestMerge();
  }

  /// <summary>
  /// Update link and merge VFX, this is also where the actual merge is triggered
  /// (because we want to sync up the VFX and the start of the merge)
  /// </summary>
  private void UpdateCrystalsLink()
  {
    this.linkVfxActive = false;
    bool flag = false;
    if ((UnityEngine.Object) this.hand == (UnityEngine.Object) null)
    {
      Item obj1 = (Item) null;
      if ((bool) (UnityEngine.Object) Player.currentCreature)
      {
        Item obj2 = (Item) null;
        Item obj3 = (Item) null;
        Item obj4 = Player.currentCreature.handRight?.grabbedHandle?.item;
        ItemModuleCrystal module1;
        if (obj4 != null && obj4.data.TryGetModule<ItemModuleCrystal>(out module1) && string.Equals(module1.treeName, this.treeName))
          obj3 = obj4;
        Item obj5 = Player.currentCreature.handLeft?.grabbedHandle?.item;
        ItemModuleCrystal module2;
        if (obj5 != null && obj5.data.TryGetModule<ItemModuleCrystal>(out module2) && string.Equals(module2.treeName, this.treeName))
          obj2 = obj5;
        if ((bool) (UnityEngine.Object) obj2 && (bool) (UnityEngine.Object) obj3)
        {
          flag = true;
          if ((UnityEngine.Object) obj2 == (UnityEngine.Object) this.item)
            obj1 = obj3;
          else if ((UnityEngine.Object) obj3 == (UnityEngine.Object) this.item)
            obj1 = obj2;
        }
        else
        {
          obj1 = obj2 ?? obj3;
          if ((UnityEngine.Object) obj1 == (UnityEngine.Object) this.item)
            obj1 = (Item) null;
        }
      }
      UnityEngine.Vector3? nullable = new UnityEngine.Vector3?();
      if ((UnityEngine.Object) obj1 != (UnityEngine.Object) null)
        nullable = new UnityEngine.Vector3?(obj1.transform.position);
      else if (!flag && this.IsCrystalSameTreeInInventory())
        nullable = new UnityEngine.Vector3?(Player.currentCreature.Center);
      if (nullable.HasValue && (double) UnityEngine.Vector3.Distance(this.transform.position, nullable.Value) < (double) this.linkMaxDistance)
      {
        this.linkVfxActive = true;
        if ((double) this.timeLerpLinkVfx < (double) this.timeVfxTransitionMin)
          this.timeLerpLinkVfx = this.timeVfxTransitionMin;
        this.linkVfxTarget.position = nullable.Value;
      }
    }
    if (this.mergeEffect != null)
      this.linkVfx.gameObject.SetActive(false);
    if ((bool) (UnityEngine.Object) this.mergeVfxTarget)
      this.mergePoint.transform.position = UnityEngine.Vector3.Lerp(this.mergeVfxTarget.position, this.transform.position, 0.5f);
    if ((UnityEngine.Object) this.mergingCrystal != (UnityEngine.Object) null && this.mergeBeginEffect == null)
    {
      this.mergeBeginEffect?.End();
      this.mergeBeginEffect = this.mergeBeginEffectData?.Spawn(this.mergePoint);
      this.mergeBeginEffect?.Play();
    }
    if ((UnityEngine.Object) this.mergingCrystal != (UnityEngine.Object) null && this.mergeEffect == null && (double) Time.time - (double) this.mergeStartTime > (double) ItemModuleCrystal.mergeEffectDelay)
    {
      this.mergeEffect?.End();
      this.mergeEffect = this.mergeEffectData?.Spawn(this.mergePoint);
      this.mergeCrystalEffect?.End();
      this.mergeCrystalEffect = this.mergeCrystalEffectAndroidData?.Spawn(this.transform);
      if (Common.IsAndroid)
      {
        this.mergeEffect?.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor.UnHDR(), this.skillTreeEmissionColor.UnHDR()));
        this.mergeCrystalEffect?.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor.UnHDR(), this.skillTreeEmissionColor.UnHDR()));
      }
      else
      {
        this.mergeEffect?.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor, this.skillTreeEmissionColor));
        this.mergeCrystalEffect?.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor, this.skillTreeEmissionColor));
      }
      this.mergeEffect?.Play();
      this.mergeCrystalEffect?.Play();
      this.ConnectMergeVFX(this.mergingCrystal.transform);
      this.mergingCrystal.ConnectMergeVFX(this.transform);
      this.Merge(this.mergingCrystal);
    }
    if (!(bool) (UnityEngine.Object) this.linkVfx)
      return;
    if (this.linkVfxActive)
    {
      if (!this.linkVfx.gameObject.activeSelf)
        this.linkVfx.gameObject.SetActive(true);
      if ((double) this.timeLerpLinkVfx < 1.0)
      {
        this.timeLerpLinkVfx += Time.deltaTime / this.timeVfxTransition;
        if ((double) this.timeLerpLinkVfx > 1.0)
          this.timeLerpLinkVfx = 1f;
      }
    }
    else if ((double) this.timeLerpLinkVfx > 0.0)
    {
      this.timeLerpLinkVfx -= Time.deltaTime / this.timeVfxTransition;
      if ((double) this.timeLerpLinkVfx <= 0.0)
      {
        this.timeLerpLinkVfx = 0.0f;
        float f = Mathf.InverseLerp(this.timeVfxTransitionMin, 1f, this.timeLerpLinkVfx);
        this.linkVfx.SetFloat(SkillTreeCrystal.Intensity, f);
        this.linkVfx.gameObject.SetActive(false);
      }
    }
    if (!this.linkVfx.gameObject.activeSelf)
      return;
    float f1 = Mathf.InverseLerp(this.timeVfxTransitionMin, 1f, this.timeLerpLinkVfx);
    this.linkVfx.SetFloat(SkillTreeCrystal.Intensity, f1);
  }

  private void UpdateCrystalGlow()
  {
    if (this.glowing)
    {
      if ((double) this.glowIntensity < 1.0)
      {
        this.glowIntensity += Time.deltaTime / this.timeGlowTransition;
        if ((double) this.glowIntensity > 1.0)
          this.glowIntensity = 1f;
      }
      this.item.Haptic((float) ((0.20000000298023224 + ((double) Mathf.Sin(Time.unscaledTime * 3f) + 1.0) / 2.0 * 0.800000011920929) * (double) this.glowIntensity * 0.5));
    }
    else if ((double) this.glowIntensity > 0.0)
    {
      this.glowIntensity -= Time.deltaTime / this.timeGlowTransition;
      if ((double) this.glowIntensity < 0.0)
      {
        this.glowIntensity = 0.0f;
        for (int index = 0; index < this.materialInstances.Length; ++index)
          this.materialInstances[index].material.SetFloat(SkillTreeCrystal.GlowProperty, 0.0f);
      }
    }
    if ((double) this.timeGlowTransition <= 0.0 || this.glowCurve == null)
      return;
    float num1 = this.glowCurve.Evaluate(this.glowTime) * this.glowIntensity * ItemModuleCrystal.glowMultiplier;
    if ((bool) (UnityEngine.Object) this.mergeVfxTarget)
    {
      float num2 = UnityEngine.Vector3.Distance(this.mergeVfxTarget.position, this.transform.position);
      num1 *= Mathf.Pow(2f, (float) (1.0 + (double) Mathf.InverseLerp(0.5f, 0.0f, num2) * 2.0));
    }
    this.glowTime += Time.deltaTime;
    for (int index = 0; index < this.materialInstances.Length; ++index)
      this.materialInstances[index].material.SetFloat(SkillTreeCrystal.GlowProperty, num1);
  }

  public void UpdateVertexLerp(float amount, UnityEngine.Vector3 target = default (UnityEngine.Vector3), bool isLocal = false)
  {
    for (int index = 0; index < this.materialInstances.Length; ++index)
    {
      this.materialInstances[index].material.SetFloat(SkillTreeCrystal.VertexLerpAmount, amount);
      this.materialInstances[index].material.SetVector(SkillTreeCrystal.VertexLerpTarget, (Vector4) (isLocal ? target : this.materialInstances[index].transform.InverseTransformPoint(target)));
    }
  }

  private bool IsCrystalSameTreeInInventory()
  {
    if ((UnityEngine.Object) this.item == (UnityEngine.Object) null || this.item.data == null || (UnityEngine.Object) Player.currentCreature == (UnityEngine.Object) null)
      return false;
    for (int index = 0; index < Player.currentCreature.container.contents.Count; ++index)
    {
      ItemModuleCrystal module;
      if (Player.currentCreature.container.contents[index] is ItemContent content && content.catalogData is ItemData catalogData && catalogData.type == ItemData.Type.Crystal && content.state == null && !(catalogData.category != this.item.data.category) && catalogData.TryGetModule<ItemModuleCrystal>(out module) && string.Equals(this.treeName, module.treeName))
        return true;
    }
    return false;
  }

  protected internal override void ManagedFixedUpdate() => base.ManagedUpdate();

  public void ToggleSparkles(bool enable)
  {
    if (enable)
    {
      this.sparkleEffect?.End();
      this.sparkleEffect = this.sparkleEffectData?.Spawn(this.transform);
      if (Common.IsAndroid)
        this.sparkleEffect?.SetMainGradient(Utils.CreateGradient(this.module.skillTreeData.color.UnHDR(), this.module.skillTreeData.emissionColor.UnHDR()));
      else
        this.sparkleEffect?.SetMainGradient(Utils.CreateGradient(this.module.skillTreeData.color, this.module.skillTreeData.emissionColor));
      this.sparkleEffect?.Play();
    }
    else
    {
      this.sparkleEffect.End();
      this.sparkleEffect = (EffectInstance) null;
    }
  }

  public void Merge(SkillTreeCrystal other)
  {
    if (other.treeName != this.treeName)
      return;
    SkillTreeCrystal.MergeCrystal(this, other);
  }

  /// Static method to merge two crystals
  public static void MergeCrystal(SkillTreeCrystal crystal1, SkillTreeCrystal crystal2)
  {
    if (crystal1.merging || crystal2.merging)
      return;
    int tier1 = crystal1.item.data.tier;
    int tier2 = crystal2.item.data.tier;
    int num1 = tier1 > tier2 ? 1 : 0;
    SkillTreeCrystal mainCrystal = num1 != 0 ? crystal1 : crystal2;
    SkillTreeCrystal otherCrystal = num1 != 0 ? crystal2 : crystal1;
    int num2 = num1 != 0 ? tier2 : tier1;
    ItemModuleCrystal module = mainCrystal.item.data.GetModule<ItemModuleCrystal>();
    ItemData mergedData = mainCrystal.item.data;
    string leftoverId = (string) null;
    ItemData outputData1;
    int leftoverCount = (string.IsNullOrEmpty(mainCrystal.module.higherTierCrystalId) || !Catalog.TryGetData<ItemData>(mainCrystal.module.higherTierCrystalId, out outputData1) ? otherCrystal.item.data.tier : tier1 + tier2 - outputData1.tier) * Catalog.gameData.mergeLeftoverShardMultiplier;
    for (int index = 0; index < num2; ++index)
    {
      ItemData outputData2;
      if (string.IsNullOrEmpty(module.higherTierCrystalId) || !Catalog.TryGetData<ItemData>(module.higherTierCrystalId, out outputData2))
      {
        leftoverId = module.shardId;
        break;
      }
      mergedData = outputData2;
      module = mergedData.GetModule<ItemModuleCrystal>();
    }
    if (mergedData == null)
    {
      Debug.LogError((object) "Encountered an unexpected error: no merge could be complete");
    }
    else
    {
      crystal1.merging = true;
      crystal2.merging = true;
      crystal1.item.AddNonStorableModifier((object) crystal2);
      crystal2.item.AddNonStorableModifier((object) crystal1);
      SkillTreeCrystal.isMerging = true;
      mainCrystal.StartCoroutine(SkillTreeCrystal.MergeCoroutine(mainCrystal, otherCrystal, mergedData, leftoverId, leftoverCount));
    }
  }

  /// <summary>
  /// Where the merge-y magic happens. Brings crystals close together, plays an effect, then calls FinishMerge once done.
  /// </summary>
  /// <returns></returns>
  private static IEnumerator MergeCoroutine(
    SkillTreeCrystal mainCrystal,
    SkillTreeCrystal otherCrystal,
    ItemData mergedData,
    string leftoverId,
    int leftoverCount)
  {
    StartCrystal(mainCrystal);
    StartCrystal(otherCrystal);
    Transform mergePoint = PoolUtils.GetTransformPoolManager().Get();
    mergePoint.transform.position = UnityEngine.Vector3.Lerp(Player.currentCreature.handLeft.grip.position, Player.currentCreature.handRight.grip.position, 0.5f);
    EffectInstance mergeCompleteEffect = (EffectInstance) null;
    yield return (object) Utils.LoopOver(new Action<float>(Loop), 3f, new System.Action(End));

    void Loop(float value)
    {
      mergePoint.transform.position = UnityEngine.Vector3.Lerp(Player.currentCreature.handLeft.grip.position, Player.currentCreature.handRight.grip.position, 0.5f);
      float lerpValue = Catalog.gameData.mergeDistanceCurve.Evaluate(value);
      if (mergeCompleteEffect == null && (double) value > 0.44999998807907104)
      {
        mergeCompleteEffect = mainCrystal.module.mergeStartEffectData?.Spawn(mergePoint);
        mergeCompleteEffect?.Play();
      }
      UpdateCrystal(mainCrystal, value, lerpValue);
      UpdateCrystal(otherCrystal, value, lerpValue);
    }

    void UpdateCrystal(SkillTreeCrystal crystal, float value, float lerpValue)
    {
      crystal.transform.SetPositionAndRotation(UnityEngine.Vector3.Lerp(crystal.transform.position, UnityEngine.Vector3.Lerp(crystal.hand.grip.position, mergePoint.position, lerpValue), Time.deltaTime * 5f), Quaternion.Lerp(crystal.transform.rotation, Quaternion.LookRotation(crystal.hand.PointDir, crystal.hand.ThumbDir), Time.deltaTime * 5f));
      crystal.UpdateVertexLerp(Mathf.InverseLerp(0.65f, 1f, value), mergePoint.position);
      crystal.mergeEffect?.SetIntensity(value);
      if ((double) value > 0.949999988079071)
        crystal.hand.HapticTick(lerpValue * Mathf.InverseLerp(1f, 0.95f, value));
      else
        crystal.hand.HapticTick(lerpValue);
    }

    void End()
    {
      if (mergeCompleteEffect != null)
        mergeCompleteEffect.onEffectFinished += (EffectInstance.EffectFinishEvent) (_ => PoolUtils.GetTransformPoolManager().Release(mergePoint));
      else
        PoolUtils.GetTransformPoolManager().Release(mergePoint);
      FinishCrystal(mainCrystal);
      FinishCrystal(otherCrystal);
      SkillTreeCrystal.FinishMerge(mainCrystal, otherCrystal, mergedData, leftoverId, leftoverCount);
    }

    static void StartCrystal(SkillTreeCrystal crystal)
    {
      crystal.hand = crystal.item.mainHandler;
      crystal.item.ForceUngrabAll();
      crystal.hand.caster.DisableSpellWheel((object) crystal);
      crystal.hand.caster.DisallowCasting((object) crystal);
      crystal.hand.SetBlockGrab(true);
      crystal.item.physicBody.isKinematic = true;
      crystal.item.AddNonStorableModifier((object) crystal);
      crystal.hand.poser.SetDefaultPose(crystal.floatHandPose.defaultHandPoseData);
      crystal.hand.poser.SetTargetPose(crystal.floatHandPose.targetHandPoseData);
      crystal.hand.poser.SetTargetWeight(0.0f);
    }

    static void FinishCrystal(SkillTreeCrystal crystal)
    {
      crystal.hand.SetBlockGrab(false);
      crystal.item.physicBody.isKinematic = false;
      crystal.item.RemoveNonStorableModifier((object) crystal);
      crystal.hand.caster.AllowSpellWheel((object) crystal);
      crystal.hand.caster.AllowCasting((object) crystal);
      crystal.hand.poser.ResetDefaultPose();
      crystal.hand.poser.ResetTargetPose();
      crystal.hand = (RagdollHand) null;
      crystal.SetGlow(false);
    }
  }

  /// <summary>
  /// Finish the merge, despawn old crystals, spawn new crystals and leftover shards.
  /// </summary>
  private static void FinishMerge(
    SkillTreeCrystal mainCrystal,
    SkillTreeCrystal otherCrystal,
    ItemData merged,
    string leftoversID,
    int leftovers)
  {
    ConfigurableJoint joint = (ConfigurableJoint) null;
    SkillTreeCrystal.isMerging = false;
    if (!mainCrystal.item.data.id.Equals(merged.id))
    {
      merged.SpawnAsync((Action<Item>) (crystalSpawn =>
      {
        mainCrystal.item.RemoveNonStorableModifier((object) otherCrystal);
        otherCrystal.item.RemoveNonStorableModifier((object) mainCrystal);
        otherCrystal.item.Despawn();
        mainCrystal.item.Despawn();
        mainCrystal = (SkillTreeCrystal) null;
        SkillTreeCrystal component = crystalSpawn.GetComponent<SkillTreeCrystal>();
        FloatInPlace(component);
        SkillTreeCrystal.LerpIn(component);
      }));
    }
    else
    {
      mainCrystal.item.RemoveNonStorableModifier((object) otherCrystal);
      otherCrystal.item.Despawn();
      mainCrystal.mergingCrystal = (SkillTreeCrystal) null;
      mainCrystal.UpdateVertexLerp(0.0f);
      mainCrystal.DisconnectMergeVFX();
      mainCrystal.mergingCrystal = (SkillTreeCrystal) null;
      mainCrystal.merging = false;
      FloatInPlace(mainCrystal);
      SkillTreeCrystal.LerpIn(mainCrystal);
    }
    Player.currentCreature.handLeft.HapticTick(oneFrameCooldown: true);
    Player.currentCreature.handRight.HapticTick(oneFrameCooldown: true);
    if (leftovers <= 0)
      return;
    RagdollHand handRight = Player.currentCreature.handRight;
    ItemData data = Catalog.GetData<ItemData>(leftoversID);
    if (data == null)
      return;
    UnityEngine.Vector3 pos = handRight.transform.position + UnityEngine.Vector3.up * 0.05f;
    for (int index = 0; index < leftovers; ++index)
      data.SpawnAsync((Action<Item>) (leftoverItemSpawn =>
      {
        leftoverItemSpawn.GetPhysicBody().useGravity = false;
        leftoverItemSpawn.OnGrabEvent += new Item.GrabDelegate(SkillTreeCrystal.ResetGravity);
        SkillTreeShard component = leftoverItemSpawn.GetComponent<SkillTreeShard>();
        component.StartCoroutine(SkillTreeCrystal.LeftoverShardRoutine(component, pos));
      }), new UnityEngine.Vector3?(pos + UnityEngine.Random.onUnitSphere * 0.2f), new Quaternion?(Quaternion.identity));

    void OnGrab(Handle handle, RagdollHand grabber)
    {
      handle.item.OnGrabEvent -= new Item.GrabDelegate(OnGrab);
      handle.item.RemovePhysicModifier((object) handle.item);
      AudioSource.PlayClipAtPoint(handle.item.audioContainerSnap.PickAudioClip(1), handle.item.transform.position);
      if (!(bool) (UnityEngine.Object) joint)
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) joint);
    }

    void FloatInPlace(SkillTreeCrystal crystal)
    {
      Rigidbody attachment = new GameObject("CrystalAttachmentPoint").AddComponent<Rigidbody>();
      attachment.isKinematic = true;
      attachment.transform.SetPositionAndRotation(UnityEngine.Vector3.Lerp(Player.currentCreature.handLeft.grip.position, Player.currentCreature.handRight.grip.position, 0.5f), Quaternion.identity);
      crystal.transform.SetPositionAndRotation(attachment.transform.position, attachment.transform.rotation);
      Coroutine routine = crystal.StartCoroutine(SkillTreeCrystal.AttachmentBobRoutine(attachment));
      EffectInstance effect = crystal.hoverEffectData?.Spawn(crystal.transform);
      effect?.Play();
      crystal.item.OnGrabEvent += new Item.GrabDelegate(OnGrabEvent);
      crystal.item.SetPhysicModifier((object) crystal.item, new float?(0.0f), 1f, 3f, 1f, -1f, (EffectData) null);
      Quaternion rotation = crystal.transform.rotation;
      crystal.transform.rotation = Quaternion.identity;
      joint = crystal.gameObject.AddComponent<ConfigurableJoint>();
      joint.autoConfigureConnectedAnchor = false;
      joint.connectedBody = attachment;
      joint.rotationDriveMode = RotationDriveMode.Slerp;
      ConfigurableJoint configurableJoint1 = joint;
      ConfigurableJoint configurableJoint2 = joint;
      JointDrive jointDrive1;
      joint.zDrive = jointDrive1 = new JointDrive()
      {
        positionSpring = 100f,
        positionDamper = 10f,
        maximumForce = 1000f
      };
      JointDrive jointDrive2;
      JointDrive jointDrive3 = jointDrive2 = jointDrive1;
      configurableJoint2.yDrive = jointDrive2;
      JointDrive jointDrive4 = jointDrive3;
      configurableJoint1.xDrive = jointDrive4;
      joint.slerpDrive = new JointDrive()
      {
        positionSpring = 5f,
        positionDamper = 4f,
        maximumForce = 1000f
      };
      joint.xMotion = joint.yMotion = joint.zMotion = joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
      crystal.transform.rotation = rotation;
      crystal.item.OnGrabEvent += new Item.GrabDelegate(OnGrab);

      void OnGrabEvent(Handle handle, RagdollHand grabber)
      {
        crystal.item.OnGrabEvent -= new Item.GrabDelegate(OnGrabEvent);
        crystal.StopCoroutine(routine);
        effect?.End();
        if (!(bool) (UnityEngine.Object) attachment)
          return;
        UnityEngine.Object.Destroy((UnityEngine.Object) attachment.gameObject);
      }
    }
  }

  public static void LerpIn(SkillTreeCrystal crystal)
  {
    crystal.item.SetOwner(Item.Owner.Player);
    UnityEngine.Vector3 position = crystal.transform.position;
    crystal.UpdateVertexLerp(1f, position);
    crystal.RunAfter((System.Action) (() => crystal.module.mergeCompleteEffectData?.Spawn(crystal.transform)?.Play()), 0.01f);
    crystal.LoopOver((Action<float>) (time => crystal.UpdateVertexLerp(1f - time, position)), 0.5f);
  }

  public static IEnumerator AttachmentBobRoutine(Rigidbody attachment)
  {
    UnityEngine.Vector3 startPos = attachment.transform.position;
    float startTime = Time.time;
    while (!((UnityEngine.Object) attachment == (UnityEngine.Object) null))
    {
      attachment.transform.SetPositionAndRotation(startPos + UnityEngine.Vector3.up * (Mathf.Sin((float) (((double) Time.time - (double) startTime) * 1.5)) * 0.1f), Quaternion.AngleAxis(Time.time * 30f, UnityEngine.Vector3.up));
      yield return (object) 0;
    }
  }

  protected static IEnumerator LeftoverShardRoutine(SkillTreeShard shard, UnityEngine.Vector3 pos)
  {
    shard.item.SetPhysicModifier((object) shard, new float?(0.0f), 1f, 3f);
    shard.item.AddExplosionForce(0.7f, pos, 1f, 0.0f, ForceMode.VelocityChange, (CollisionHandler) null);
    shard.item.AddForce(Player.local.head.transform.forward * 2f, ForceMode.VelocityChange, (CollisionHandler) null);
    shard.item.mainHandleLeft.SetTouch(false);
    shard.item.mainHandleLeft.SetTelekinesis(false);
    yield return (object) new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
    shard.item.RemovePhysicModifier((object) shard);
    Player.characterData.inventory.AddCurrencyValue(Currency.CrystalShard, 1f);
    shard.genericAttractionTarget = Player.currentCreature.ragdoll.targetPart.transform;
  }

  private static void ResetGravity(Handle handle, RagdollHand ragdollHand)
  {
    handle.item.GetPhysicBody().useGravity = true;
    handle.item.OnGrabEvent -= new Item.GrabDelegate(SkillTreeCrystal.ResetGravity);
  }

  public List<ValueDropdownItem<string>> GetAllSkillTreeID()
  {
    return Catalog.gameData != null ? Catalog.GetDropdownAllID(Category.SkillTree) : new List<ValueDropdownItem<string>>();
  }
}
