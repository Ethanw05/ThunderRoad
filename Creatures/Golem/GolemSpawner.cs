// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemSpawner
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace ThunderRoad;

public class GolemSpawner : ThunderBehaviour
{
  public string golemAddress;
  public WeakPointRandomizer arenaCrystalRandomizer;
  public bool spawnOnStart = true;
  public GolemSpawner.SpawnAction actionOnSpawn;
  [Header("Events")]
  public UnityEvent onStartWakeFull;
  public UnityEvent onStartWakeShortA;
  public UnityEvent onStartWakeShortB;
  public UnityEvent onGolemAwaken;
  public UnityEvent onGolemDefeat;
  public UnityEvent onGolemKill;
  public UnityEvent onGolemStun;
  public UnityEvent onCrystalGrabbed;
  public UnityEvent onCrystalUnGrabbed;
  [NonSerialized]
  public Golem golem;

  private void Start()
  {
    if (!this.spawnOnStart)
      return;
    this.SpawnGolem();
  }

  public void SpawnGolem()
  {
    GolemSpawner.SpawnGolem(this.golemAddress, this.actionOnSpawn, this.transform.position, this.transform.rotation, this.transform, this.arenaCrystalRandomizer, this);
  }

  public static void SpawnGolem(
    string address,
    GolemSpawner.SpawnAction spawnAction,
    UnityEngine.Vector3 position,
    Quaternion rotation,
    Transform parent,
    WeakPointRandomizer arenaCrystalRandomizer,
    GolemSpawner spawner)
  {
    Catalog.InstantiateAsync(address, position, rotation, parent, (Action<GameObject>) (golemPrefab =>
    {
      Golem componentInChildren = golemPrefab.GetComponentInChildren<Golem>();
      if ((UnityEngine.Object) spawner != (UnityEngine.Object) null)
        spawner.golem = componentInChildren;
      componentInChildren.transform.rotation = Quaternion.FromToRotation(componentInChildren.transform.up, UnityEngine.Vector3.up) * componentInChildren.transform.rotation;
      componentInChildren.spawner = spawner;
      componentInChildren.characterController.enableOverlapRecovery = false;
      componentInChildren.characterController.radius = 0.01f;
      componentInChildren.arenaCrystalRandomizer = arenaCrystalRandomizer;
      if (componentInChildren.crystals.IsNullOrEmpty())
        return;
      if (spawnAction == GolemSpawner.SpawnAction.Disable)
      {
        componentInChildren.gameObject.SetActive(false);
      }
      else
      {
        spawner.EnableGolem();
        if (spawnAction != GolemSpawner.SpawnAction.Wake)
          return;
        componentInChildren.RandomizeCrystalProtection();
        componentInChildren.TargetPlayer();
        componentInChildren.SetAwake(true);
      }
    }), nameof (GolemSpawner));
  }

  public void EnableGolem()
  {
    if (!this.golem.gameObject.activeInHierarchy)
      this.golem.gameObject.SetActive(true);
    this.golem.characterController.enableOverlapRecovery = false;
    this.golem.characterController.radius = 0.01f;
    this.golem.RandomizeCrystalProtection();
    this.golem.TargetPlayer();
  }

  public void WakeGolem()
  {
    if ((UnityEngine.Object) this.golem == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "No golem to wake!");
    }
    else
    {
      if (!this.golem.gameObject.activeInHierarchy)
        this.EnableGolem();
      this.golem.SetAwake(true);
    }
  }

  public void StunGolem()
  {
    if ((UnityEngine.Object) this.golem == (UnityEngine.Object) null || !this.golem.gameObject.activeInHierarchy)
      Debug.LogError((object) "No golem to stun!");
    else
      this.golem.Stun(this.golem.activeCrystalConfig.arenaCrystalMaxStun);
  }

  public void StunGolem(float time = 0.0f)
  {
    if ((UnityEngine.Object) this.golem == (UnityEngine.Object) null || !this.golem.gameObject.activeInHierarchy)
      Debug.LogError((object) "No golem to stun!");
    else
      this.golem.Stun(time);
  }

  public void DefeatGolem()
  {
    if ((UnityEngine.Object) this.golem == (UnityEngine.Object) null)
      Debug.LogError((object) "No golem to defeat!");
    else
      this.golem.Defeat();
  }

  public void StartWakeSequence(int num)
  {
    switch (num)
    {
      case 0:
        this.onStartWakeFull?.Invoke();
        break;
      case 1:
        this.onStartWakeShortA?.Invoke();
        break;
      case 2:
        this.onStartWakeShortB?.Invoke();
        break;
    }
  }

  public enum SpawnAction
  {
    None,
    Disable,
    Wake,
  }
}
