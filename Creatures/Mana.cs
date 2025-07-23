// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Mana
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Mana.html")]
[RequireComponent(typeof (Creature))]
[AddComponentMenu("ThunderRoad/Creatures/Mana")]
public class Mana : ThunderBehaviour
{
  public float _currentFocus = 30f;
  public float _baseMaxFocus = 30f;
  public float focusRegen = 2f;
  public float focusRegenPerSkill;
  public FloatHandler focusConsumptionMult;
  public float maxFocusPerSkill = 0.05f;
  public float minFocus = 10f;
  protected bool focusReady = true;
  protected bool focusFull = true;
  [NonSerialized]
  public Creature creature;
  [NonSerialized]
  public SpellCaster casterLeft;
  [NonSerialized]
  public SpellCaster casterRight;
  public static bool infiniteFocus;
  public static bool fastCast;
  [NonSerialized]
  public List<SpellData> spells = new List<SpellData>();
  [NonSerialized]
  public List<SpellPowerData> spellPowerInstances = new List<SpellPowerData>();
  public AudioClip noManaSound;
  [NonSerialized]
  protected float itemChargeSpeedMult = 1f;
  [NonSerialized]
  public float focusRegenMult = 1f;
  [NonSerialized]
  public float _maxFocusMult = 1f;
  [NonSerialized]
  public Transform mergePoint;
  public bool mergeActive;
  public bool mergeCompleted;
  public float mergeHandsDistance;
  [NonSerialized]
  public bool mergeCastLoaded;
  [NonSerialized]
  public SpellMergeData mergeInstance;
  [NonSerialized]
  public SpellMergeData mergeData;
  public float overlapRadius = 5f;
  public float overlapMinDelay = 0.5f;
  public LayerMask overlapMask;
  public int overlapCount;
  public Collider[] overlapColliders;
  protected float overlapLastTime;
  protected bool initialized;
  public FloatHandler chargeSpeedMult;
  public SavedSpells tempSavedSpells;

  public event Mana.FocusChangeEvent OnFocusChange;

  public float currentFocus
  {
    get => this._currentFocus;
    set
    {
      this._currentFocus = value;
      Mana.FocusChangeEvent onFocusChange = this.OnFocusChange;
      if (onFocusChange == null)
        return;
      onFocusChange(this._currentFocus, this.MaxFocus);
    }
  }

  public float baseMaxFocus
  {
    get => this._baseMaxFocus;
    set
    {
      this._baseMaxFocus = value;
      Mana.FocusChangeEvent onFocusChange = this.OnFocusChange;
      if (onFocusChange == null)
        return;
      onFocusChange(this._currentFocus, this.MaxFocus);
    }
  }

  public float MaxFocus => this.baseMaxFocus * (1f + this.maxFocusMult);

  public float maxFocusMult
  {
    get => this._maxFocusMult;
    set
    {
      this._maxFocusMult = value;
      Mana.FocusChangeEvent onFocusChange = this.OnFocusChange;
      if (onFocusChange == null)
        return;
      onFocusChange(this._currentFocus, this.MaxFocus);
    }
  }

  public event Mana.SpellLoadEvent OnSpellLoadEvent;

  public event Mana.SpellLoadEvent OnSpellUnloadEvent;

  public event Mana.ImbueLoadEvent OnImbueLoadEvent;

  public event Mana.ImbueLoadEvent OnImbueUnloadEvent;

  public event Mana.PowerUseEvent OnPowerUseEvent;

  public event Mana.MergeCastEvent OnMergeCastStep;

  public SpellCaster GetCaster(Side side) => side == Side.Left ? this.casterLeft : this.casterRight;

  private void Awake()
  {
    this.creature = this.GetComponent<Creature>();
    this.mergePoint = new GameObject("SpellMerge").transform;
    this.mergePoint.SetParent(this.transform, false);
    this.overlapColliders = new Collider[200];
    this.focusConsumptionMult = new FloatHandler();
  }

  public void Init(Creature creature)
  {
    this.casterRight = creature.handRight.caster;
    this.casterLeft = creature.handLeft.caster;
    this.initialized = true;
  }

  public void Load()
  {
    this.chargeSpeedMult = new FloatHandler();
    this.baseMaxFocus = this.currentFocus = this.creature.data.focus;
    this.focusRegen = this.creature.data.focusRegen;
    this.focusRegenPerSkill = this.creature.data.focusRegenPerSkill;
    this.maxFocusPerSkill = this.creature.data.maxFocusPerSkill;
    this.overlapRadius = this.creature.data.overlapRadius;
    this.overlapMinDelay = this.creature.data.overlapMinDelay;
    this.overlapMask = this.creature.data.overlapMask;
    this.focusReady = this.focusFull = true;
    this.RemoveSpell();
    this.creature.container.OnContentAddEvent += new Container.ContentChangeEvent(this.OnContainerContentAdd);
    this.creature.container.OnContentRemoveEvent += new Container.ContentChangeEvent(this.OnContainerContentRemove);
    foreach (ContainerContent<SpellData, SpellContent> containerContent in this.creature.container.contents.GetEnumerableContentsOfType<SpellContent>(true))
      this.AddSpell(containerContent.data);
    this.RefreshMultipliers();
  }

  public void OnContainerContentAdd(ContainerContent content, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    switch (content)
    {
      case SpellContent spellContent:
        if (spellContent.data != null)
        {
          this.TempUnloadSpells();
          this.creature.ForceLoadSkill(spellContent.data.id);
          this.AddSpell(spellContent.data);
          this.TempReloadSpells();
          break;
        }
        break;
      case SkillContent skillContent:
        if (skillContent.data != null)
        {
          Imbue.TempUnloadAll();
          this.TempUnloadSpells();
          this.creature.ForceLoadSkill(skillContent.data.id);
          this.TempReloadSpells();
          Imbue.TempReloadAll();
          break;
        }
        break;
    }
    this.RefreshMultipliers();
  }

  public void OnContainerContentRemove(ContainerContent content, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    switch (content)
    {
      case SpellContent spellContent:
        if (spellContent.data != null)
        {
          this.RemoveSpell(spellContent.data.id);
          this.creature.ForceUnloadSkill(spellContent.data.id);
          this.TempUnloadSpells();
          this.TempReloadSpells();
          break;
        }
        break;
      case SkillContent skillContent:
        if (skillContent.data != null)
        {
          Imbue.TempUnloadAll();
          this.TempUnloadSpells();
          this.creature.ForceUnloadSkill(skillContent.data.id);
          this.TempReloadSpells();
          Imbue.TempReloadAll();
          break;
        }
        break;
    }
    this.RefreshMultipliers();
  }

  public float GetHandsIntensity()
  {
    return (float) (((double) this.casterLeft.intensity + (double) this.casterRight.intensity) * 0.5);
  }

  public SpellPowerSlowTime GetPowerSlowTime()
  {
    foreach (SpellPowerData spellPowerInstance in this.spellPowerInstances)
    {
      if (spellPowerInstance is SpellPowerSlowTime)
        return spellPowerInstance as SpellPowerSlowTime;
    }
    return (SpellPowerSlowTime) null;
  }

  public void OnHandGrabChangeEvent() => this.RefreshMultipliers();

  public void OnApparelChangeEvent() => this.RefreshMultipliers();

  public void RefreshMultipliers()
  {
    this.itemChargeSpeedMult = 1f;
    this.focusRegenMult = 1f;
    this.maxFocusMult = 1f;
    if (!this.creature.isPlayer)
      return;
    this.casterLeft.chargeSpeedMult.ClearByType<ItemModuleGlove>();
    this.casterRight.chargeSpeedMult.ClearByType<ItemModuleGlove>();
    this.creature.handLeft.playerHand?.link.ClearJointModifiersByType<ItemModuleGlove>();
    this.creature.handRight.playerHand?.link.ClearJointModifiersByType<ItemModuleGlove>();
    foreach (ItemData itemData in this.creature.container.GetAllWardrobe())
    {
      this.itemChargeSpeedMult += itemData.data.spellChargeSpeedPlayerMultiplier - 1f;
      this.focusRegenMult += itemData.data.focusRegenMultiplier - 1f;
      ItemModuleGlove module;
      if (itemData.data.TryGetModule<ItemModuleGlove>(out module))
      {
        this.GetCaster(module.side).chargeSpeedMult.Add((object) module, module.chargeSpeed);
        this.creature.GetHand(module.side).playerHand?.link.SetAllJointModifiers((object) module, module.strength);
      }
    }
    Item obj1 = this.creature.handLeft.grabbedHandle?.item;
    if (obj1 != null)
    {
      this.itemChargeSpeedMult += (float) ((this.creature.isPlayer ? (double) obj1.data.spellChargeSpeedPlayerMultiplier : (double) obj1.data.spellChargeSpeedNPCMultiplier) - 1.0);
      this.focusRegenMult += obj1.data.focusRegenMultiplier - 1f;
    }
    Item obj2 = this.creature.handRight.grabbedHandle?.item;
    if (obj2 != null)
    {
      this.itemChargeSpeedMult += (float) ((this.creature.isPlayer ? (double) obj2.data.spellChargeSpeedPlayerMultiplier : (double) obj2.data.spellChargeSpeedNPCMultiplier) - 1.0);
      this.focusRegenMult += obj2.data.focusRegenMultiplier - 1f;
    }
    this.focusRegenMult += (float) this.creature.CountSkillsOfTree("Mind") * this.focusRegenPerSkill;
    this.maxFocusMult += (float) this.creature.CountSkillsOfTree("Mind") * this.maxFocusPerSkill;
    this.creature.healthModifier.Add((object) this, (float) (1.0 + (double) this.creature.CountSkillsOfTree("Body") * (double) this.creature.data.maxHealthPerSkill));
  }

  public float ChargeSpeedMultiplier
  {
    get => this.itemChargeSpeedMult * (float) (ValueHandler<float>) this.chargeSpeedMult;
  }

  public SavedSpells UnloadSpells()
  {
    SpellCastData spellInstance1 = this.casterLeft.spellInstance;
    if (spellInstance1 != null)
      this.casterLeft.UnloadSpell();
    SpellCastData spellInstance2 = this.casterRight.spellInstance;
    if (spellInstance2 != null)
      this.casterRight.UnloadSpell();
    SpellTelekinesis telekinesis1 = this.casterLeft.telekinesis;
    if (telekinesis1 != null)
    {
      telekinesis1.Unload();
      this.InvokeOnSpellUnload((SpellData) telekinesis1, this.casterLeft);
    }
    SpellTelekinesis telekinesis2 = this.casterRight.telekinesis;
    if (telekinesis2 != null)
    {
      telekinesis2.Unload();
      this.InvokeOnSpellUnload((SpellData) telekinesis2, this.casterRight);
    }
    if (this.mergeData != null)
    {
      if (this.mergeInstance != null)
      {
        this.mergeInstance.Unload();
        this.InvokeOnSpellUnload((SpellData) this.mergeInstance);
      }
      this.mergeInstance = (SpellMergeData) null;
      this.mergeData = (SpellMergeData) null;
      this.mergeCastLoaded = false;
    }
    return new SavedSpells(spellInstance1, spellInstance2, telekinesis1, telekinesis2);
  }

  public void LoadSpells(SavedSpells saved)
  {
    if (this.mergeData != null)
    {
      if (this.mergeInstance != null)
      {
        this.mergeInstance.Unload();
        this.InvokeOnSpellUnload((SpellData) this.mergeInstance);
      }
      this.mergeInstance = (SpellMergeData) null;
      this.mergeData = (SpellMergeData) null;
      this.mergeCastLoaded = false;
    }
    if (saved.left != null)
      this.casterLeft.LoadSpell(saved.left);
    if (saved.right != null)
      this.casterRight.LoadSpell(saved.right);
    if (saved.tkLeft != null)
    {
      saved.tkLeft.Load(this.casterLeft);
      this.InvokeOnSpellLoad((SpellData) saved.tkLeft, this.casterLeft);
    }
    if (saved.tkRight == null)
      return;
    saved.tkRight.Load(this.casterRight);
    this.InvokeOnSpellLoad((SpellData) saved.tkRight, this.casterRight);
  }

  public void TempUnloadSpells() => this.tempSavedSpells = this.UnloadSpells();

  public void TempReloadSpells() => this.LoadSpells(this.tempSavedSpells);

  protected override void ManagedOnDisable()
  {
  }

  public void AddSpell(SpellData spellData)
  {
    switch (spellData)
    {
      case SpellPowerData spellPowerData2:
        SpellPowerData spellPowerData1 = spellPowerData2.Clone();
        try
        {
          spellPowerData1.Load(this);
        }
        catch (NullReferenceException ex)
        {
          Debug.LogError((object) $"Caught NullReferenceException while loading power {spellPowerData1.id}, skipping. Exception below.");
          Debug.LogException((Exception) ex);
        }
        this.spellPowerInstances.Add(spellPowerData1);
        break;
      case SpellTelekinesis spellTelekinesis:
        this.casterLeft.telekinesis = spellTelekinesis.Clone();
        this.casterRight.telekinesis = spellTelekinesis.Clone();
        try
        {
          this.casterLeft.telekinesis.Load(this.casterLeft);
          this.casterRight.telekinesis.Load(this.casterRight);
        }
        catch (NullReferenceException ex)
        {
          Debug.LogError((object) "Caught NullReferenceException while loading Telekinesis, skipping. Exception below.");
          Debug.LogException((Exception) ex);
        }
        this.InvokeOnSpellLoad((SpellData) this.casterLeft.telekinesis, this.casterLeft);
        this.InvokeOnSpellLoad((SpellData) this.casterRight.telekinesis, this.casterRight);
        break;
      default:
        this.spells.Add(spellData.Clone() as SpellData);
        break;
    }
  }

  public void RemoveSpell(string spellId = null)
  {
    if (this.casterLeft.telekinesis != null)
    {
      if (this.casterLeft.telekinesis.id == spellId)
      {
        try
        {
          this.casterLeft.telekinesis.Unload();
        }
        catch (NullReferenceException ex)
        {
          Debug.LogError((object) "Caught NullReferenceException while unloading left Telekinesis spell, skipping. Exception below.");
          Debug.LogException((Exception) ex);
        }
        this.casterLeft.telekinesis = (SpellTelekinesis) null;
      }
    }
    if (this.casterRight.telekinesis != null)
    {
      if (this.casterRight.telekinesis.id == spellId)
      {
        try
        {
          this.casterRight.telekinesis.Unload();
        }
        catch (NullReferenceException ex)
        {
          Debug.LogError((object) "Caught NullReferenceException while unloading left Telekinesis spell, skipping. Exception below.");
          Debug.LogException((Exception) ex);
        }
        this.casterRight.telekinesis = (SpellTelekinesis) null;
      }
    }
    for (int index = this.spells.Count - 1; index >= 0; --index)
    {
      if (spellId == null || this.spells[index].id == spellId)
      {
        if (this.mergeData != null)
        {
          if (this.mergeData.id == spellId)
          {
            try
            {
              this.mergeInstance?.Unload();
            }
            catch (NullReferenceException ex)
            {
              Debug.LogError((object) $"Caught NullReferenceException while unloading merge spell {this.mergeData.id}, skipping. Exception below.");
              Debug.LogException((Exception) ex);
            }
            this.mergeInstance = (SpellMergeData) null;
            this.mergeData = (SpellMergeData) null;
          }
        }
        if (this.casterLeft.spellInstance != null && this.casterLeft.spellInstance.id == spellId)
          this.casterLeft.UnloadSpell();
        if (this.casterRight.spellInstance != null && this.casterRight.spellInstance.id == spellId)
          this.casterRight.UnloadSpell();
        this.spells.RemoveAt(index);
      }
    }
    for (int index = this.spellPowerInstances.Count - 1; index >= 0; --index)
    {
      if (spellId == null || !(this.spellPowerInstances[index].id != spellId))
      {
        try
        {
          this.spellPowerInstances[index].Unload();
        }
        catch (NullReferenceException ex)
        {
          Debug.LogError((object) $"Caught NullReferenceException while unloading spell {this.spellPowerInstances[index].id}, skipping. Exception below.");
          Debug.LogException((Exception) ex);
        }
        this.spellPowerInstances.RemoveAt(index);
      }
    }
  }

  public void ClearSpells()
  {
    if ((bool) (UnityEngine.Object) this.casterLeft)
    {
      this.casterLeft.UnloadSpell();
      this.casterLeft.telekinesis = (SpellTelekinesis) null;
    }
    if ((bool) (UnityEngine.Object) this.casterRight)
    {
      this.casterRight.UnloadSpell();
      this.casterRight.telekinesis = (SpellTelekinesis) null;
    }
    if (this.mergeInstance != null)
      this.mergeInstance.Unload();
    foreach (SpellPowerData spellPowerInstance in this.spellPowerInstances)
      spellPowerInstance.Unload();
    this.spellPowerInstances.Clear();
    this.spells.Clear();
  }

  public void UsePower(bool active)
  {
    Mana.PowerUseEvent onPowerUseEvent1 = this.OnPowerUseEvent;
    if (onPowerUseEvent1 != null)
      onPowerUseEvent1(active, EventTime.OnStart);
    foreach (SpellPowerData spellPowerInstance in this.spellPowerInstances)
      spellPowerInstance.Use(active);
    Mana.PowerUseEvent onPowerUseEvent2 = this.OnPowerUseEvent;
    if (onPowerUseEvent2 == null)
      return;
    onPowerUseEvent2(active, EventTime.OnEnd);
  }

  public void InvokeMergeStep(SpellMergeData merge, Mana.CastStep step)
  {
    if (merge == null)
    {
      Debug.LogError((object) "Merge step shouldn't be invoked with a null merge!");
    }
    else
    {
      if (this.OnMergeCastStep == null)
        return;
      foreach (Delegate invocation in this.OnMergeCastStep.GetInvocationList())
      {
        if (invocation is Mana.MergeCastEvent mergeCastEvent)
        {
          try
          {
            mergeCastEvent(merge, step);
          }
          catch (Exception ex)
          {
            Debug.LogError((object) $"Error during OnMergeCastStep event: {ex}");
          }
        }
      }
    }
  }

  public override ManagedLoops EnabledManagedLoops
  {
    get => ManagedLoops.FixedUpdate | ManagedLoops.Update;
  }

  protected internal override void ManagedFixedUpdate()
  {
    if (!this.initialized)
      return;
    if (this.mergeActive)
      this.mergeInstance.FixedUpdate();
    this.casterLeft.ManaFixedUpdate();
    this.casterRight.ManaFixedUpdate();
  }

  protected internal override void ManagedUpdate()
  {
    if (!this.initialized)
      return;
    this.UpdateRegen();
    this.UpdateMerge();
    this.UpdatePowers();
    if (this.mergeActive)
      this.mergeInstance.Update();
    this.casterLeft.ManaUpdate();
    this.casterRight.ManaUpdate();
  }

  private void UpdatePowers()
  {
    if (this.spellPowerInstances.IsNullOrEmpty())
      return;
    foreach (SpellPowerData spellPowerInstance in this.spellPowerInstances)
      spellPowerInstance.Update();
  }

  private void UpdateRegen()
  {
    if (!(bool) (UnityEngine.Object) this.creature.player || this.creature.state == Creature.State.Dead || Mana.infiniteFocus || (double) this.focusRegen <= 0.0 || (double) this.currentFocus >= (double) this.MaxFocus)
      return;
    this.currentFocus = Mathf.Clamp(this.currentFocus + this.focusRegen * ((double) this.creature.player.locomotion.moveDirection.magnitude > 0.0 ? 0.5f : 1f) * this.focusRegenMult * Time.deltaTime, 0.0f, this.MaxFocus);
    if (this.focusReady)
    {
      if ((double) this.currentFocus < (double) this.minFocus)
        this.focusReady = false;
    }
    else if ((double) this.currentFocus >= (double) this.minFocus)
    {
      this.creature.data.focusReadyEffect?.Spawn(this.transform).Play();
      this.focusReady = true;
    }
    if (this.focusFull)
    {
      if ((double) this.currentFocus >= (double) this.MaxFocus)
        return;
      this.focusFull = false;
    }
    else
    {
      if ((double) this.currentFocus < (double) this.MaxFocus)
        return;
      this.creature.data.focusFullEffect?.Spawn(this.transform).Play();
      this.focusFull = true;
    }
  }

  private void UpdateMerge()
  {
    if (this.mergeCastLoaded)
    {
      SpellCaster casterLeft = this.casterLeft;
      if (casterLeft != null && casterLeft.allowCasting && (double) casterLeft.intensity > 0.0)
      {
        SpellCaster casterRight = this.casterRight;
        if (casterRight != null && casterRight.allowCasting && (double) casterRight.intensity > 0.0 && this.mergeInstance.CanMerge() && !this.casterLeft.grabbedFire && !this.casterRight.grabbedFire)
        {
          this.mergeHandsDistance = UnityEngine.Vector3.Distance(this.casterLeft.magicSource.position, this.casterRight.magicSource.position);
          if (!this.mergeActive && (double) UnityEngine.Vector3.Angle(this.casterLeft.magicSource.forward, this.casterRight.magicSource.position - this.casterLeft.magicSource.position) < (double) this.mergeData.handEnterAngle && (double) UnityEngine.Vector3.Angle(this.casterRight.magicSource.forward, this.casterLeft.magicSource.position - this.casterRight.magicSource.position) < (double) this.mergeData.handEnterAngle && (double) this.mergeHandsDistance < (double) this.mergeData.handEnterDistance && this.casterLeft.isFiring && this.casterRight.isFiring)
            this.mergeActive = true;
          if (!this.mergeActive)
            return;
          this.mergePoint.position = UnityEngine.Vector3.Lerp(this.mergePoint.position, UnityEngine.Vector3.Lerp(this.casterLeft.magicSource.position, this.casterRight.magicSource.position, 0.5f), (double) this.mergeData.effectLerpFactor == 0.0 ? 1f : Time.deltaTime * this.mergeData.effectLerpFactor);
          this.mergePoint.rotation.SetLookRotation(this.mergePoint.position - this.creature.ragdoll.headPart.bone.animation.position, this.creature.transform.up);
          if ((double) UnityEngine.Vector3.Angle(this.casterLeft.magicSource.forward, this.casterRight.magicSource.position - this.casterLeft.magicSource.position) > (double) this.mergeData.handExitAngle || (double) UnityEngine.Vector3.Angle(this.casterRight.magicSource.forward, this.casterLeft.magicSource.position - this.casterRight.magicSource.position) > (double) this.mergeData.handExitAngle || (double) this.mergeHandsDistance > (double) this.mergeData.handExitDistance)
          {
            this.mergeActive = false;
            this.mergeCompleted = false;
            this.mergeInstance.Merge(false);
            return;
          }
          if (this.mergeCompleted || (double) UnityEngine.Vector3.Distance(this.casterLeft.Orb.position, this.casterRight.Orb.position) >= (double) this.mergeData.handCompletedDistance)
            return;
          this.mergeCompleted = true;
          this.casterLeft.Fire(false);
          this.casterRight.Fire(false);
          this.mergeInstance.Merge(true);
          this.mergeInstance.FireAxis(this.casterLeft.fireAxis, Side.Left);
          this.mergeInstance.FireAxis(this.casterLeft.fireAxis, Side.Right);
          return;
        }
      }
    }
    if (!this.mergeActive)
      return;
    this.mergeActive = false;
    this.mergeCompleted = false;
    this.mergeInstance?.Merge(false);
  }

  public bool TryLoadMerge()
  {
    if (this.casterLeft.spellInstance != null && this.casterRight.spellInstance != null)
    {
      foreach (SpellMergeData spellMergeData in this.creature.container.contents.GetEnumerableContentCatalogDatasOfType<SpellContent, SpellData>((Func<SpellContent, SpellData, bool>) ((_, data) => data is SpellMergeData)))
      {
        if (!(spellMergeData.leftSpellId != this.casterLeft.spellInstance.id) && !(spellMergeData.rightSpellId != this.casterRight.spellInstance.id) || !(spellMergeData.rightSpellId != this.casterLeft.spellInstance.id) && !(spellMergeData.leftSpellId != this.casterRight.spellInstance.id))
        {
          this.mergeData = spellMergeData;
          this.mergeInstance = this.mergeData.Clone() as SpellMergeData;
          this.mergeInstance?.Load(this);
          this.InvokeOnSpellLoad((SpellData) this.mergeInstance);
          this.mergeCastLoaded = true;
          return true;
        }
      }
    }
    return false;
  }

  public void UnloadMerge()
  {
    if (this.mergeInstance != null)
    {
      if (this.mergeActive)
      {
        this.mergeActive = false;
        this.mergeCompleted = false;
        this.mergeInstance.Merge(false);
      }
      this.mergeInstance.Unload();
      this.InvokeOnSpellUnload((SpellData) this.mergeInstance);
    }
    this.mergeInstance = (SpellMergeData) null;
    this.mergeCastLoaded = false;
    this.mergeData = (SpellMergeData) null;
  }

  public void OnSpellChange()
  {
    this.UnloadMerge();
    if (this.TryLoadMerge())
      return;
    this.mergeCastLoaded = false;
    this.mergeData = (SpellMergeData) null;
    this.mergeInstance = (SpellMergeData) null;
  }

  public bool RegenFocus(float focusToRegen)
  {
    if (Mana.infiniteFocus)
      return true;
    this.currentFocus = Mathf.Clamp(this.currentFocus + focusToRegen, 0.0f, this.MaxFocus);
    return true;
  }

  public bool ConsumeFocus(float focusToConsume)
  {
    if ((double) focusToConsume < 0.0)
    {
      Debug.LogError((object) "Focus to consume is negative, redirecting it to regen focus instead. Update the method call where you are trying to consume negative");
      return this.RegenFocus(-focusToConsume);
    }
    if (Mana.infiniteFocus)
      return true;
    float num = this.currentFocus - focusToConsume * (float) (ValueHandler<float>) this.focusConsumptionMult;
    if ((double) num <= 0.0)
      return false;
    this.currentFocus = Mathf.Clamp(num, 0.0f, this.MaxFocus);
    return true;
  }

  public bool CanConsumeFocus(float focusToConsume)
  {
    return Mana.infiniteFocus || (double) this.currentFocus - (double) focusToConsume * (double) (float) (ValueHandler<float>) this.focusConsumptionMult > 0.0;
  }

  public void CastOverlapSphere()
  {
    if ((double) Time.time - (double) this.overlapLastTime <= (double) this.overlapMinDelay)
      return;
    this.overlapCount = Physics.OverlapSphereNonAlloc(this.creature.ragdoll.headPart.bone.animation.position, this.overlapRadius, this.overlapColliders, (int) this.overlapMask);
    this.overlapLastTime = Time.time;
  }

  public bool TryGetSpell<T>(string id, out T spell) where T : SpellData
  {
    for (int index = 0; index < this.spells.Count; ++index)
    {
      if (this.spells[index].id == id && this.spells[index] is T spell1)
      {
        spell = spell1;
        return true;
      }
    }
    spell = default (T);
    return false;
  }

  public void InvokeOnSpellLoad(SpellData spellInstance, SpellCaster spellCaster = null)
  {
    Mana.SpellLoadEvent onSpellLoadEvent = this.OnSpellLoadEvent;
    if (onSpellLoadEvent == null)
      return;
    onSpellLoadEvent(spellInstance, spellCaster);
  }

  public void InvokeOnSpellUnload(SpellData spellInstance, SpellCaster spellCaster = null)
  {
    Mana.SpellLoadEvent spellUnloadEvent = this.OnSpellUnloadEvent;
    if (spellUnloadEvent == null)
      return;
    spellUnloadEvent(spellInstance, spellCaster);
  }

  public void InvokeOnImbueLoad(SpellCastCharge spell, Imbue imbue)
  {
    Mana.ImbueLoadEvent onImbueLoadEvent = this.OnImbueLoadEvent;
    if (onImbueLoadEvent == null)
      return;
    onImbueLoadEvent(spell, imbue);
  }

  public void InvokeOnImbueUnload(SpellCastCharge spell, Imbue imbue)
  {
    Mana.ImbueLoadEvent imbueUnloadEvent = this.OnImbueUnloadEvent;
    if (imbueUnloadEvent == null)
      return;
    imbueUnloadEvent(spell, imbue);
  }

  public delegate void FocusChangeEvent(float focus, float maxFocus);

  public delegate void SpellLoadEvent(SpellData spellInstance, SpellCaster caster);

  public delegate void ImbueLoadEvent(SpellCastCharge spellData, Imbue imbue);

  public delegate void PowerUseEvent(bool active, EventTime eventTime);

  public delegate void MergeCastEvent(SpellMergeData merge, Mana.CastStep step);

  public enum CastStep
  {
    MergeStart,
    MergeCharged,
    MergeFireStart,
    MergeFireEnd,
    MergeStop,
  }
}
