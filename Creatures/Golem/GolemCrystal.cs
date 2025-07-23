// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemCrystal
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace ThunderRoad;

public class GolemCrystal : SimpleBreakable
{
  [Header("Protection")]
  public GameObject shield;
  public UnityEvent onShieldEnable;
  public UnityEvent onShieldDisable;
  public Transform linkEffect;
  public Transform linkEffectTarget;
  public VfxPlayer passiveVfxPlayer;
  public string hitEffectID;
  public string emissiveEffectID;
  public float emissiveToggleTime = 0.5f;
  public List<Renderer> emissiveRenderers = new List<Renderer>();
  private bool shieldActive;
  private SimpleBreakable.DamageType originalAllowedTypes;
  private EffectData hitEffectData;
  private EffectData emissiveEffectData;
  private List<EffectInstance> emissiveEffects = new List<EffectInstance>();
  private float currentEmissive;
  private float targetEmissive = 1f;

  protected override void Awake()
  {
    base.Awake();
    this.originalAllowedTypes = this.allowedDamageTypes;
  }

  private void Start()
  {
    this.passiveVfxPlayer?.Stop();
    this.hitEffectData = Catalog.GetData<EffectData>(this.hitEffectID, false);
    this.emissiveEffectData = Catalog.GetData<EffectData>(this.emissiveEffectID, false);
    for (int index = 0; index < this.emissiveRenderers.Count; ++index)
    {
      if (!((Object) this.emissiveRenderers[index] == (Object) null))
      {
        EffectInstance effectInstance = this.emissiveEffectData.Spawn(this.transform.position, this.transform.rotation, this.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f);
        effectInstance.SetRenderer(this.emissiveRenderers[index], true);
        effectInstance.SetIntensity(1f);
        effectInstance.Play();
        this.emissiveEffects.Add(effectInstance);
      }
    }
  }

  public virtual void EnableShield()
  {
    if (this.shieldActive)
      return;
    this.shieldActive = true;
    this.onShieldEnable?.Invoke();
    this.shield?.SetActive(true);
    this.allowedDamageTypes = SimpleBreakable.DamageType.None;
    this.passiveVfxPlayer?.Stop();
    this.targetEmissive = 0.0f;
  }

  public virtual void DisableShield()
  {
    if (!this.shieldActive)
      return;
    this.shieldActive = false;
    this.onShieldDisable?.Invoke();
    this.shield?.SetActive(false);
    this.allowedDamageTypes = this.originalAllowedTypes;
    this.passiveVfxPlayer?.Play();
    this.targetEmissive = 1f;
  }

  protected override bool EnterCollision(UnityEngine.Collision collision)
  {
    if (!base.EnterCollision(collision))
      return false;
    if (this.hitEffectData != null)
      this.hitEffectData.Spawn(collision.GetContact(0).point, Quaternion.LookRotation(Random.insideUnitSphere), (Transform) null, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f).Play();
    return true;
  }

  private void LateUpdate()
  {
    this.currentEmissive = Mathf.MoveTowards(this.currentEmissive, this.targetEmissive, Time.deltaTime / this.emissiveToggleTime);
    foreach (EffectInstance emissiveEffect in this.emissiveEffects)
      emissiveEffect.SetIntensity(this.currentEmissive);
  }
}
