// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemAnimatorEvent
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace ThunderRoad;

public class GolemAnimatorEvent : MonoBehaviour
{
  [Header("References")]
  public GolemController golem;
  public Animator animator;
  public List<AudioSource> audioSources;
  public List<ParticleSystem> particleSystems;
  [Header("Feet")]
  public bool rightFootPlanted;
  public bool leftFootPlanted;
  [Header("Events")]
  public UnityEvent onLeftFootPlant;
  public UnityEvent onRightFootPlant;
  public Action<bool> onEnableHitbox;
  public static int blendIdHash;
  public static int rightFootHash;
  protected Dictionary<string, AudioSource> keyedAudios = new Dictionary<string, AudioSource>();
  protected Dictionary<string, ParticleSystem> keyedParticles = new Dictionary<string, ParticleSystem>();

  private void OnValidate()
  {
    if (!(bool) (UnityEngine.Object) this.animator)
      this.animator = this.GetComponent<Animator>();
    if (!(bool) (UnityEngine.Object) this.golem)
      this.golem = this.GetComponentInParent<GolemController>();
    if (this.audioSources.IsNullOrEmpty())
    {
      this.audioSources = new List<AudioSource>();
      this.audioSources.AddRange((IEnumerable<AudioSource>) this.golem.GetComponentsInChildren<AudioSource>());
    }
    if (!this.particleSystems.IsNullOrEmpty())
      return;
    this.particleSystems = new List<ParticleSystem>();
    this.particleSystems.AddRange((IEnumerable<ParticleSystem>) this.golem.GetComponentsInChildren<ParticleSystem>());
  }

  private void Awake()
  {
    this.InitAnimationParametersHashes();
    for (int index = 0; index < this.audioSources.Count; ++index)
    {
      if (!this.keyedAudios.ContainsKey(this.audioSources[index].name) && !((UnityEngine.Object) this.audioSources[index].GetComponentInParent<GolemCrystal>() != (UnityEngine.Object) null))
        this.keyedAudios.Add(this.audioSources[index].name, this.audioSources[index]);
    }
    for (int index = 0; index < this.particleSystems.Count; ++index)
    {
      if (!this.keyedAudios.ContainsKey(this.particleSystems[index].name) && !((UnityEngine.Object) this.particleSystems[index].GetComponentInParent<GolemCrystal>() != (UnityEngine.Object) null))
        this.keyedParticles[this.particleSystems[index].name] = this.particleSystems[index];
    }
  }

  private void InitAnimationParametersHashes()
  {
    GolemAnimatorEvent.blendIdHash = Animator.StringToHash("BlendID");
    GolemAnimatorEvent.rightFootHash = Animator.StringToHash("RightFoot");
  }

  private void OnAnimatorMove()
  {
    if (this.golem.animatorIsRoot)
      return;
    this.golem.OnAnimatorMove();
  }

  public void RightPlant(UnityEngine.AnimationEvent e)
  {
    double f = (double) this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
    float num1 = e != null ? e.floatParameter : 0.0f;
    float num2 = Mathf.Round((float) f);
    int num3;
    if (e != null)
    {
      AnimatorStateInfo animatorStateInfo = e.animatorStateInfo;
      int fullPathHash1 = animatorStateInfo.fullPathHash;
      animatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
      int fullPathHash2 = animatorStateInfo.fullPathHash;
      num3 = fullPathHash1 == fullPathHash2 ? 1 : 0;
    }
    else
      num3 = 1;
    if (num3 == 0 || (double) Mathf.Abs(num1 - num2) > (double) Mathf.Epsilon)
      return;
    this.animator.SetBool(GolemAnimatorEvent.rightFootHash, false);
    this.rightFootPlanted = true;
    this.onRightFootPlant?.Invoke();
  }

  public void RightUnplant(UnityEngine.AnimationEvent e)
  {
    float f = this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
    if ((double) Mathf.Abs((e != null ? e.floatParameter : 0.0f) - Mathf.Round(f)) > (double) Mathf.Epsilon)
      return;
    this.rightFootPlanted = false;
  }

  public void LeftPlant(UnityEngine.AnimationEvent e)
  {
    double f = (double) this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
    float num1 = e != null ? e.floatParameter : 0.0f;
    float num2 = Mathf.Round((float) f);
    int num3;
    if (e != null)
    {
      AnimatorStateInfo animatorStateInfo = e.animatorStateInfo;
      int fullPathHash1 = animatorStateInfo.fullPathHash;
      animatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
      int fullPathHash2 = animatorStateInfo.fullPathHash;
      num3 = fullPathHash1 == fullPathHash2 ? 1 : 0;
    }
    else
      num3 = 1;
    if (num3 == 0 || (double) Mathf.Abs(num1 - num2) > (double) Mathf.Epsilon)
      return;
    this.animator.SetBool(GolemAnimatorEvent.rightFootHash, true);
    this.leftFootPlanted = true;
    this.onLeftFootPlant?.Invoke();
  }

  public void LeftUnplant(UnityEngine.AnimationEvent e)
  {
    float f = this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
    if ((double) Mathf.Abs((e != null ? e.floatParameter : 0.0f) - Mathf.Round(f)) > (double) Mathf.Epsilon)
      return;
    this.leftFootPlanted = false;
  }

  private void ActivateAbilityStep(UnityEngine.AnimationEvent e)
  {
    this.golem.currentAbility?.TryAbilityStep(e);
  }

  private void StartTurnTo(UnityEngine.AnimationEvent e)
  {
  }

  private void StopTurnTo(UnityEngine.AnimationEvent e)
  {
  }

  private void EnableHitbox(UnityEngine.AnimationEvent e)
  {
    Action<bool> onEnableHitbox = this.onEnableHitbox;
    if (onEnableHitbox == null)
      return;
    onEnableHitbox(true);
  }

  private void DisableHitbox(UnityEngine.AnimationEvent e)
  {
    Action<bool> onEnableHitbox = this.onEnableHitbox;
    if (onEnableHitbox == null)
      return;
    onEnableHitbox(false);
  }

  private void EffectOn(UnityEngine.AnimationEvent e)
  {
  }

  private void EffectOff(UnityEngine.AnimationEvent e) => Debug.Log((object) nameof (EffectOff));

  private void PlayAudioSource(UnityEngine.AnimationEvent e)
  {
    AudioSource audioSource;
    if (!this.keyedAudios.TryGetValue(e.stringParameter, out audioSource))
      return;
    audioSource.Play();
  }

  private void PlayParticleEffect(UnityEngine.AnimationEvent e)
  {
    ParticleSystem particleSystem;
    if (!this.keyedParticles.TryGetValue(e.stringParameter, out particleSystem))
    {
      Debug.LogWarning((object) $"Golem attempted to {(e.intParameter == 0 ? "stop" : "play")} particle effect {e.stringParameter}, but no particle system of that name could be found.");
    }
    else
    {
      switch (e.intParameter)
      {
        case 0:
          particleSystem.Stop();
          break;
        case 1:
          particleSystem.Play();
          break;
      }
    }
  }
}
