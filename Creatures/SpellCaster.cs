// Decompiled with JetBrains decompiler
// Type: ThunderRoad.SpellCaster
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

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/SpellCaster")]
[AddComponentMenu("ThunderRoad/Creatures/Spell caster")]
public class SpellCaster : MonoBehaviour
{
  public Transform fire;
  public Transform magicSource;
  public Transform rayDir;
  public FloatHandler chargeSpeedMult;
  public static float throwMinHandVelocity = 1f;
  [NonSerialized]
  public bool allowCasting = true;
  public BoolHandler disallowCasting;
  public BoolHandler disallowSpellWheel;
  [NonSerialized]
  private Transform orb;
  [NonSerialized]
  public float intensity;
  [NonSerialized]
  public UnityEngine.Vector3 magicOffset;
  [NonSerialized]
  public bool parryable;
  [NonSerialized]
  public bool isFiring;
  [NonSerialized]
  public bool isMerging;
  [NonSerialized]
  public bool grabbedFire;
  [NonSerialized]
  public bool isSpraying;
  [NonSerialized]
  public float fireAxis;
  [NonSerialized]
  public List<EffectInstance> fingerEffectInstances = new List<EffectInstance>();
  [NonSerialized]
  public float fireTime;
  [NonSerialized]
  public SpellTelekinesis telekinesis;
  [NonSerialized]
  public Mana mana;
  [NonSerialized]
  public RagdollHand ragdollHand;
  [NonSerialized]
  public Side side;
  [NonSerialized]
  public SpellCaster other;
  [NonSerialized]
  public SpellCastData spellInstance;
  protected float magicFollowSpeed = 50f;
  protected float mergeAttractSpeed = 10f;
  protected float imbueTransferMinIntensity = 0.1f;
  public Trigger imbueTrigger;
  [NonSerialized]
  public List<SpellCaster.ImbueObject> imbueObjects = new List<SpellCaster.ImbueObject>();

  public void DisallowCasting(object handler)
  {
    this.disallowCasting.Add(handler);
    this.allowCasting = !(bool) (ValueHandler<bool>) this.disallowCasting;
    if (!this.isFiring)
      return;
    this.Fire(false);
  }

  public void AllowCasting(object handler)
  {
    this.disallowCasting.Remove(handler);
    this.allowCasting = !(bool) (ValueHandler<bool>) this.disallowCasting;
  }

  public void ClearDisallowCasting()
  {
    this.disallowCasting.Clear();
    this.allowCasting = true;
  }

  public bool allowSpellWheel { get; protected set; } = true;

  public Transform Orb
  {
    get
    {
      if ((UnityEngine.Object) this.orb == (UnityEngine.Object) null)
        this.orb = new GameObject(nameof (Orb) + this.ragdollHand.side.ToString()).transform;
      return this.orb;
    }
  }

  public event SpellCaster.TriggerImbueEvent OnTriggerImbueEvent;

  public event SpellCaster.CastEvent OnSpellCastStep;

  private void OnEnable()
  {
    PlayerControl.GetHand(this.side).OnButtonPressEvent += new PlayerControl.Hand.ButtonEvent(this.OnControllerButtonPress);
  }

  private void OnDisable()
  {
    PlayerControl.GetHand(this.side).OnButtonPressEvent -= new PlayerControl.Hand.ButtonEvent(this.OnControllerButtonPress);
  }

  private void Awake()
  {
    this.disallowCasting = new BoolHandler(false);
    this.disallowSpellWheel = new BoolHandler(false);
    this.chargeSpeedMult = new FloatHandler();
    this.mana = this.GetComponentInParent<Mana>();
    if (!(bool) (UnityEngine.Object) this.mana)
    {
      Debug.LogError((object) "Spellcaster can't work without a mana component on the creature");
      this.gameObject.SetActive(false);
    }
    else
    {
      this.ragdollHand = this.GetComponentInParent<RagdollHand>();
      this.ragdollHand.OnGrabEvent += new RagdollHand.GrabEvent(this.OnHandGrab);
      this.ragdollHand.OnUnGrabEvent += new RagdollHand.UnGrabEvent(this.OnHandUnGrab);
      this.side = this.ragdollHand.side;
      foreach (SpellCaster componentsInChild in this.mana.GetComponentsInChildren<SpellCaster>())
      {
        if ((UnityEngine.Object) componentsInChild != (UnityEngine.Object) this)
        {
          this.other = componentsInChild;
          break;
        }
      }
      this.Orb.SetParent(this.magicSource);
      this.imbueTrigger = new GameObject("ImbueTrigger").AddComponent<Trigger>();
      Transform transform = this.imbueTrigger.transform;
      transform.SetParent(this.magicSource);
      transform.SetLocalPositionAndRotation(UnityEngine.Vector3.zero, Quaternion.identity);
      this.imbueTrigger.SetCallback(new Trigger.CallBack(this.OnTriggerImbue));
      this.imbueTrigger.SetLayer(GameManager.GetLayer(LayerName.DroppedItem));
    }
  }

  protected void OnControllerButtonPress(PlayerControl.Hand.Button button, bool pressed)
  {
    if (((!this.isFiring ? 0 : (button == PlayerControl.Hand.Button.Grip ? 1 : 0)) & (pressed ? 1 : 0)) == 0 || !(this.spellInstance is SpellCastCharge spellInstance) || !spellInstance.endOnGrip)
      return;
    spellInstance.InvokeOnGripEndEvent();
    this.Fire(false);
  }

  public void OnHandGrab(
    Side side,
    Handle handle,
    float axisPosition,
    HandlePose orientation,
    EventTime eventTime)
  {
    if (eventTime == EventTime.OnStart && !handle.data.allowSpellFire)
      this.DisallowCasting((object) handle);
    if (eventTime != EventTime.OnEnd)
      return;
    this.mana.OnHandGrabChangeEvent();
  }

  public void OnHandUnGrab(Side side, Handle handle, bool throwing, EventTime eventTime)
  {
    if (eventTime == EventTime.OnStart)
      this.AllowCasting((object) handle);
    if (eventTime != EventTime.OnEnd)
      return;
    this.mana.OnHandGrabChangeEvent();
  }

  public void InvokeCastStep(SpellCastData spell, SpellCaster.CastStep step)
  {
    if (spell == null)
    {
      Debug.LogError((object) "Cast step shouldn't be invoked with a null spell!");
    }
    else
    {
      if (this.OnSpellCastStep == null)
        return;
      foreach (Delegate invocation in this.OnSpellCastStep.GetInvocationList())
      {
        if (invocation is SpellCaster.CastEvent castEvent)
        {
          try
          {
            castEvent(spell, step);
          }
          catch (Exception ex)
          {
            Debug.LogError((object) $"Error during OnSpellCastStep event: {ex}");
          }
        }
      }
    }
  }

  public void SpawnFingersEffect(
    EffectData effectData,
    bool play = false,
    float intensity = 1f,
    Transform target = null)
  {
    if (effectData == null)
      return;
    this.StopFingersEffect();
    SpawnFingerEffect(this.ragdollHand.fingerThumb.tip);
    SpawnFingerEffect(this.ragdollHand.fingerIndex.tip);
    SpawnFingerEffect(this.ragdollHand.fingerMiddle.tip);
    SpawnFingerEffect(this.ragdollHand.fingerRing.tip);
    SpawnFingerEffect(this.ragdollHand.fingerLittle.tip);

    void SpawnFingerEffect(Transform tip)
    {
      EffectInstance effectInstance = effectData.Spawn(tip.position, tip.rotation, tip, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
      this.fingerEffectInstances.Add(effectInstance);
      effectInstance.SetSource(tip);
      effectInstance.SetTarget(target);
      effectInstance.SetIntensity(intensity);
      if (!play)
        return;
      effectInstance.Play();
    }
  }

  public void SetFingersEffect(float intensity)
  {
    int count = this.fingerEffectInstances.Count;
    for (int index = 0; index < count; ++index)
      this.fingerEffectInstances[index].SetIntensity(intensity);
  }

  public void StopFingersEffect()
  {
    int count = this.fingerEffectInstances.Count;
    for (int index = 0; index < count; ++index)
      this.fingerEffectInstances[index].End();
    this.fingerEffectInstances.Clear();
  }

  public void DisableSpellWheel(object handler)
  {
    this.disallowSpellWheel.Add(handler);
    this.allowSpellWheel = !(bool) (ValueHandler<bool>) this.disallowSpellWheel;
  }

  public void AllowSpellWheel(object handler)
  {
    this.disallowSpellWheel.Remove(handler);
    this.allowSpellWheel = !(bool) (ValueHandler<bool>) this.disallowSpellWheel;
  }

  public void SetMagicOffset(UnityEngine.Vector3 offset, bool switchForHands = false)
  {
    if (switchForHands && this.ragdollHand.side == Side.Left)
      this.magicOffset = new UnityEngine.Vector3(-offset.x, offset.y, offset.z);
    else
      this.magicOffset = offset;
  }

  public void LoadSpell(SpellCastData spellCastData)
  {
    SpellCastData spellCastData1 = spellCastData.Clone();
    this.UnloadSpell();
    if (spellCastData1 != null)
    {
      this.spellInstance = spellCastData1;
      try
      {
        this.spellInstance.Load(this);
      }
      catch (NullReferenceException ex)
      {
        Debug.LogError((object) $"Caught NullReferenceException while loading spell {this.spellInstance.id}, skipping. Exception below.");
        Debug.LogException((Exception) ex);
      }
      float radius = 0.0f;
      if (this.spellInstance is SpellCastCharge spellInstance && spellInstance.imbueEnabled)
        radius = spellInstance.imbueRadius;
      this.imbueTrigger.SetRadius(radius);
    }
    this.RefreshWater();
    this.mana.InvokeOnSpellLoad((SpellData) this.spellInstance, this);
    this.mana.OnSpellChange();
  }

  public void UnloadSpell()
  {
    if (this.spellInstance != null)
    {
      try
      {
        this.spellInstance.Unload();
      }
      catch (NullReferenceException ex)
      {
        Debug.LogError((object) $"Caught NullReferenceException while unloading spell {this.spellInstance.id}, skipping. Exception below.");
        Debug.LogException((Exception) ex);
      }
    }
    this.Fire(false);
    this.mana.InvokeOnSpellUnload((SpellData) this.spellInstance, this);
    this.mana.UnloadMerge();
    this.spellInstance = (SpellCastData) null;
  }

  public void RefreshWater()
  {
    if (!(this.spellInstance is SpellCastCharge spellInstance))
      return;
    if (this.ragdollHand.waterHandler.inWater)
    {
      if (spellInstance.allowUnderwater)
        return;
      this.DisallowCasting((object) this.ragdollHand.waterHandler);
    }
    else
      this.AllowCasting((object) this.ragdollHand.waterHandler);
  }

  protected void OnTriggerImbue(Collider other, bool enter)
  {
    SpellCaster.TriggerImbueEvent triggerImbueEvent = this.OnTriggerImbueEvent;
    if (triggerImbueEvent != null)
      triggerImbueEvent(other, enter);
    ColliderGroup rootGroup = other.GetComponentInParent<ColliderGroup>()?.RootGroup;
    if (!(bool) (UnityEngine.Object) rootGroup)
      return;
    if (enter)
    {
      if (rootGroup.modifier.imbueType == ColliderGroupData.ImbueType.None || this.spellInstance is SpellCastCharge spellInstance && !spellInstance.imbueAllowMetal && rootGroup.modifier.imbueType == ColliderGroupData.ImbueType.Metal || !string.IsNullOrEmpty(rootGroup.imbueCustomSpellID) && !string.Equals(this.spellInstance.id, rootGroup.imbueCustomSpellID, StringComparison.OrdinalIgnoreCase) || rootGroup.modifier.imbueType == ColliderGroupData.ImbueType.Custom && (rootGroup.imbueCustomSpellID == null || !string.Equals(this.spellInstance.id, rootGroup.imbueCustomSpellID, StringComparison.OrdinalIgnoreCase)))
        return;
      for (int index = this.imbueObjects.Count - 1; index >= 0; --index)
      {
        if ((UnityEngine.Object) this.imbueObjects[index].colliderGroup == (UnityEngine.Object) rootGroup)
          return;
      }
      rootGroup.gameObject.GetComponentInParent<Item>();
      this.imbueObjects.Add(new SpellCaster.ImbueObject(rootGroup));
    }
    else
    {
      for (int index = this.imbueObjects.Count - 1; index >= 0; --index)
      {
        SpellCaster.ImbueObject imbueObject = this.imbueObjects[index];
        if (!((UnityEngine.Object) imbueObject.colliderGroup != (UnityEngine.Object) rootGroup))
        {
          imbueObject.effectInstance?.End();
          this.imbueObjects.RemoveAt(index);
        }
      }
    }
  }

  public void Fire(bool active)
  {
    if (active && !this.allowCasting)
      return;
    bool grabbedHandle1 = (bool) (UnityEngine.Object) this.ragdollHand.grabbedHandle;
    if (grabbedHandle1)
    {
      this.grabbedFire = false;
      if (!this.ragdollHand.grabbedHandle.data.allowSpellFire)
        return;
    }
    else if (((!(bool) (UnityEngine.Object) this.mana.creature.player ? 0 : (this.mana.creature.player.isLocal ? 1 : 0)) & (active ? 1 : 0)) != 0 && PlayerControl.GetHand(this.ragdollHand.side).gripPressed)
      return;
    if (this.isFiring == active || this.spellInstance == null)
      return;
    this.InvokeCastStep(this.spellInstance, active ? SpellCaster.CastStep.CastStart : SpellCaster.CastStep.CastStop);
    this.isFiring = active;
    if (this.isFiring)
    {
      if (grabbedHandle1)
        this.ragdollHand.grabbedHandle.UnGrabbed += new Handle.GrabEvent(this.OnUngrabFire);
      this.grabbedFire = grabbedHandle1;
      this.Orb.SetParent(this.mana.transform);
      this.Orb.SetPositionAndRotation(this.magicSource.position, this.magicSource.rotation);
      this.Orb.transform.localScale = UnityEngine.Vector3.one;
      this.fireTime = Time.time;
      this.magicOffset = UnityEngine.Vector3.zero;
    }
    else
    {
      if (grabbedHandle1)
        this.ragdollHand.grabbedHandle.UnGrabbed -= new Handle.GrabEvent(this.OnUngrabFire);
      this.grabbedFire = false;
      Transform spellOrbTarget = this.ragdollHand.gripInfo?.SpellOrbTarget;
      if (spellOrbTarget != null)
      {
        this.Orb.SetParent(spellOrbTarget);
        this.Orb.SetLocalPositionAndRotation(UnityEngine.Vector3.zero, Quaternion.identity);
      }
      else
      {
        Transform orb1 = this.Orb;
        Handle grabbedHandle2 = this.ragdollHand.grabbedHandle;
        Transform p = grabbedHandle2 == null || !grabbedHandle2.data.allowSpellFire || !grabbedHandle2.data.offsetInHandleSpace ? ((bool) (UnityEngine.Object) this.ragdollHand.playerHand ? this.magicSource : this.mana.transform) : this.ragdollHand.grabbedHandle.transform;
        orb1.SetParent(p);
        Transform orb2 = this.Orb;
        Handle grabbedHandle3 = this.ragdollHand.grabbedHandle;
        int num;
        if (grabbedHandle3 == null)
        {
          num = 0;
        }
        else
        {
          bool? offsetInHandleSpace = grabbedHandle3.data?.offsetInHandleSpace;
          bool flag = true;
          num = offsetInHandleSpace.GetValueOrDefault() == flag & offsetInHandleSpace.HasValue ? 1 : 0;
        }
        UnityEngine.Vector3 position = num != 0 ? this.ragdollHand.grabbedHandle.transform.TransformPoint(this.magicOffset) : this.magicSource.TransformPoint(this.magicOffset);
        Quaternion rotation = this.magicSource.rotation;
        orb2.SetPositionAndRotation(position, rotation);
      }
      this.Orb.transform.localScale = UnityEngine.Vector3.one;
      for (int index = this.imbueObjects.Count - 1; index >= 0; --index)
        this.imbueObjects[index].effectInstance?.End();
      this.imbueObjects.Clear();
      this.intensity = 0.0f;
    }
    this.imbueTrigger.SetActive(this.isFiring);
    EventManager.InvokeSpellUsed(this.spellInstance.id, this.ragdollHand.creature, this.side);
    this.spellInstance.Fire(active);
  }

  public void FireAxis(float value)
  {
    this.fireAxis = value;
    if (this.isFiring)
      this.spellInstance.FireAxis(value);
    if (!this.mana.mergeActive)
      return;
    this.mana.mergeInstance.FireAxis(value, this.ragdollHand.side);
  }

  private void OnUngrabFire(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
  {
    this.Orb.SetParent(this.mana.transform);
    if (eventTime != EventTime.OnEnd)
      return;
    this.Fire(false);
    handle.UnGrabbed -= new Handle.GrabEvent(this.OnUngrabFire);
  }

  public void ManaFixedUpdate()
  {
    if (!this.isFiring)
      return;
    this.spellInstance.FixedUpdateCaster();
  }

  public void ManaUpdate()
  {
    this.telekinesis?.Update();
    if (this.isFiring)
    {
      this.UpdateManaPosition();
      this.UpdateImbueEffects();
    }
    else if (this.mana.mergeActive)
      this.Orb.position = UnityEngine.Vector3.MoveTowards(this.Orb.position, this.mana.mergePoint.position, this.magicFollowSpeed * Time.deltaTime);
    this.spellInstance?.UpdateCaster();
  }

  private void UpdateManaPosition()
  {
    UnityEngine.Vector3 position1;
    Quaternion rotation;
    if (this.mana.mergeActive)
    {
      position1 = UnityEngine.Vector3.Lerp(this.Orb.position, this.mana.mergePoint.position, this.mergeAttractSpeed * Time.deltaTime);
      rotation = Quaternion.LookRotation(this.mana.creature.transform.forward);
    }
    else
    {
      Transform spellOrbTarget = this.ragdollHand.gripInfo?.SpellOrbTarget;
      if (spellOrbTarget != null)
      {
        position1 = spellOrbTarget.position;
        rotation = spellOrbTarget.rotation;
      }
      else
      {
        UnityEngine.Vector3 position2 = this.Orb.position;
        Handle grabbedHandle = this.ragdollHand.grabbedHandle;
        int num;
        if (grabbedHandle == null)
        {
          num = 0;
        }
        else
        {
          bool? offsetInHandleSpace = grabbedHandle.data?.offsetInHandleSpace;
          bool flag = true;
          num = offsetInHandleSpace.GetValueOrDefault() == flag & offsetInHandleSpace.HasValue ? 1 : 0;
        }
        UnityEngine.Vector3 target = num != 0 ? this.ragdollHand.grabbedHandle.transform.TransformPoint(this.magicOffset) : this.magicSource.TransformPoint(this.magicOffset);
        double maxDistanceDelta = (double) this.magicFollowSpeed * (double) Time.deltaTime;
        position1 = UnityEngine.Vector3.MoveTowards(position2, target, (float) maxDistanceDelta);
        rotation = this.magicSource.rotation;
      }
    }
    this.Orb.SetPositionAndRotation(position1, rotation);
  }

  public float ChargeRatio
  {
    get
    {
      if (!(this.spellInstance is SpellCastCharge spellInstance))
        return 0.0f;
      return !this.grabbedFire ? spellInstance.currentCharge : spellInstance.currentCharge / ((double) spellInstance.grabbedFireMaxCharge == 0.0 ? 1f : spellInstance.grabbedFireMaxCharge);
    }
  }

  private void UpdateImbueEffects()
  {
    if (!(this.spellInstance is SpellCastCharge spellInstance) || !spellInstance.imbueEnabled)
      return;
    int count = this.imbueObjects.Count;
    Transform transform = this.mana.creature.transform;
    for (int index = 0; index < count; ++index)
    {
      SpellCaster.ImbueObject imbueObject = this.imbueObjects[index];
      imbueObject.colliderGroup.imbue.Transfer(spellInstance, spellInstance.imbueRate * this.ChargeRatio / (float) count * Time.unscaledDeltaTime);
      if (imbueObject.effectInstance == null && (UnityEngine.Object) imbueObject.colliderGroup.parentGroup == (UnityEngine.Object) null)
      {
        EffectInstance effectInstance = spellInstance.imbueTransferEffectData.Spawn(transform.position, transform.rotation, transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
        Renderer renderer = (bool) (UnityEngine.Object) imbueObject.colliderGroup.imbueEffectRenderer ? imbueObject.colliderGroup.imbueEffectRenderer : imbueObject.colliderGroup.imbueEmissionRenderer;
        if ((bool) (UnityEngine.Object) renderer)
        {
          effectInstance.SetSource(this.Orb.transform);
          effectInstance.SetRenderer(renderer, false);
          effectInstance.SetTarget(renderer.transform);
          effectInstance.Play();
        }
        else
          Debug.LogError((object) "Can't play imbue effect, there is no imbueEffectRenderer and imbueEmissionRenderer on the prefab");
        imbueObject.effectInstance = effectInstance;
      }
      float num = this.imbueTransferMinIntensity;
      if ((double) imbueObject.colliderGroup.imbue.energy != (double) imbueObject.colliderGroup.imbue.maxEnergy)
        num = this.ChargeRatio / (float) count;
      imbueObject.effectInstance?.SetIntensity(num);
    }
  }

  public UnityEngine.Vector3 GetShootDirection()
  {
    return this.magicSource.transform.TransformDirection(UnityEngine.Vector3.forward);
  }

  private void OnDrawGizmos()
  {
    this.spellInstance?.DrawGizmos();
    this.telekinesis?.DrawGizmos();
  }

  private void OnDrawGizmosSelected()
  {
    this.spellInstance?.DrawGizmosSelected();
    this.telekinesis?.DrawGizmosSelected();
  }

  public delegate void TriggerImbueEvent(Collider other, bool enter);

  public delegate void CastEvent(SpellCastData spell, SpellCaster.CastStep step);

  public enum CastStep
  {
    CastStart,
    ChargeStart,
    ChargeSprayable,
    ChargeThrowable,
    ChargeThrow,
    SprayStart,
    SprayEnd,
    ChargeStop,
    CastStop,
    MergeStart,
    MergeCharged,
    MergeFireStart,
    MergeFireEnd,
    MergeStop,
  }

  public class ImbueObject
  {
    public ColliderGroup colliderGroup;
    public EffectInstance effectInstance;

    public ImbueObject(ColliderGroup colliderGroup) => this.colliderGroup = colliderGroup;
  }
}
