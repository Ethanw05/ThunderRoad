// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Golem
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;

#nullable disable
namespace ThunderRoad;

public class Golem : GolemController
{
  public static Golem local;
  [Header("AI")]
  public NavMeshAgent navMeshAgent;
  public float cycleTime = 0.5f;
  public float forwardAngle = 30f;
  public float stopMoveDistance = 5.5f;
  public float stopMoveSphereHeight = 1f;
  public float minHeadToTargetDistance = 20f;
  private UnityEngine.Vector2 velocity;
  private UnityEngine.Vector2 smoothDeltaPosition;
  private float animationDampTime = 0.1f;
  [Header("Awake")]
  public bool awakeWhenTargetClose = true;
  public float awakeTargetDistance = 30f;
  protected float awakeTime;
  [Header("Melee")]
  public bool allowMelee = true;
  public float meleeMaxAttackDistance = 7f;
  [FormerlySerializedAs("abilityCooldownInMelee")]
  public float abilityCooldown = 15f;
  public UnityEngine.Vector2 meleeMinMaxCooldown = new UnityEngine.Vector2(1f, 3f);
  public List<Golem.MeleeAttackRange> attackRanges;
  private bool setMeleeCooldown;
  private float nextMeleeAttackTime;
  public bool blockActionWhileClimbed = true;
  public bool allowClimbReact;
  public AudioSource climbReactWarningAudio;
  public float climbReactWarningTime = 2f;
  public float climbReactTime = 5f;
  public float resetClimbedTime = 1f;
  public List<GolemAbility> climbReacts;
  private bool wasClimbed;
  private float climbTime;
  private int climbReactIntensity;
  private float lastReactWarningTime;
  [Header("Crystals")]
  public WeakPointRandomizer golemCrystalRandomizer;
  public WeakPointRandomizer arenaCrystalRandomizer;
  public Golem.CrystalConfig defaultConfig;
  public Golem.CrystalConfig tier1Config;
  public Golem.CrystalConfig tier2Config;
  public Golem.CrystalConfig tier3Config;
  public bool rampageOnCrystalBreak = true;
  public List<GolemAbility> crystalBreakReactions = new List<GolemAbility>();
  public float normalRampageChance = 0.4f;
  public float rampageDamageMult = 2f;
  public float rampageForceMult = 1.5f;
  public string shardItemId = "CrystalShard";
  protected ItemData shardItemData;
  protected int shardsToDrop;
  [Range(1f, 10f)]
  public int crystalsBrokenToWake = 1;
  private int crystalsBrokenDuringStun;
  public float shieldDisableDelay;
  [ColorUsage(true, true)]
  public UnityEngine.Color defeatedEmissionColor;
  public bool autoDefeatOnStart;
  public bool autoDefeatAndKillOnStart;
  [NonSerialized]
  public List<GolemCrystal> crystals = new List<GolemCrystal>();
  [NonSerialized]
  public List<GolemCrystal> linkedArenaCrystals = new List<GolemCrystal>();
  protected int disableArenaCrystalShieldIndex;
  private Quaternion climbInitialRelativeRotation;

  public Golem.CrystalConfig activeCrystalConfig
  {
    get
    {
      switch (this.tier)
      {
        case Golem.Tier.Tier1:
          return this.tier1Config;
        case Golem.Tier.Tier2:
          return this.tier2Config;
        case Golem.Tier.Tier3:
          return this.tier3Config;
        default:
          return this.defaultConfig;
      }
    }
  }

  public List<ValueDropdownItem<string>> GetAllItemID() => Catalog.GetDropdownAllID(Category.Item);

  public bool isProtected
  {
    get
    {
      List<GolemCrystal> linkedArenaCrystals = this.linkedArenaCrystals;
      return (linkedArenaCrystals != null ? __nonvirtual (linkedArenaCrystals.Count) : 0) > 0;
    }
  }

  public int crystalsLeft => this.crystals.Count;

  public event Golem.GolemCrystalBreak OnGolemCrystalBreak;

  public event Golem.GolemCrystalBreak OnArenaCrystalBreak;

  public static event System.Action OnLocalGolemSet;

  public override void SetAwake(bool awake)
  {
    base.SetAwake(awake);
    this.awakeTime = Time.time;
  }

  public virtual void SetAttackTarget(Transform targetTransform)
  {
    if (this.isAwake && !this.isStunned)
      this.LookAt(targetTransform);
    this.attackTarget = targetTransform;
  }

  public void Rampage()
  {
    this.Rampage(this.TargetInMeleeRange() ? RampageType.Melee : RampageType.Ranged);
  }

  public void Rampage(RampageType type = RampageType.Melee)
  {
    if (type == RampageType.None)
    {
      Debug.LogWarning((object) "Can't rampage with type none!");
    }
    else
    {
      this.StopStun();
      this.StopDeploy();
      this.hitDamageMultiplier = this.rampageDamageMult;
      this.hitForceMultiplier = this.rampageForceMult;
      if (type == RampageType.Melee)
        this.RampageMelee((System.Action) (() =>
        {
          this.hitDamageMultiplier = 1f;
          this.hitForceMultiplier = 1f;
        }));
      else
        this.RampageRanged((System.Action) (() =>
        {
          this.hitDamageMultiplier = 1f;
          this.hitForceMultiplier = 1f;
        }));
      this.navMeshAgent.updateRotation = false;
    }
  }

  public virtual void Defeat()
  {
    if (this.crystals.IsNullOrEmpty())
      return;
    this.BreakArenaCrystals(this.linkedArenaCrystals.Count);
    this.BreakCrystals(this.crystalsLeft);
    this.spawner?.onGolemDefeat?.Invoke();
  }

  public void BreakCrystals(int num = 1)
  {
    if (this.crystals.IsNullOrEmpty())
      return;
    for (int index = 0; index < num; ++index)
      this.crystals[this.crystals.Count - 1].Break();
  }

  public void BreakArenaCrystals(int num = 1)
  {
    if (this.linkedArenaCrystals.IsNullOrEmpty())
      return;
    for (int index = 0; index < num; ++index)
      this.linkedArenaCrystals[this.linkedArenaCrystals.Count - 1].Break();
  }

  protected override void OnValidate()
  {
    base.OnValidate();
    if (!(bool) (UnityEngine.Object) this.navMeshAgent)
      this.navMeshAgent = this.GetComponentInChildren<NavMeshAgent>();
    if ((bool) (UnityEngine.Object) this.golemCrystalRandomizer)
      return;
    this.golemCrystalRandomizer = this.GetComponentInChildren<WeakPointRandomizer>();
  }

  protected override void Awake()
  {
    Golem.local = this;
    string str;
    Golem.Tier tier;
    switch (int.Parse(Level.current.options == null || !Level.current.options.TryGetValue(LevelOption.GolemTier.Get(), out str) ? "4" : str))
    {
      case 1:
        tier = Golem.Tier.Tier1;
        break;
      case 2:
        tier = Golem.Tier.Tier2;
        break;
      case 3:
        tier = Golem.Tier.Tier3;
        break;
      case 4:
        tier = Golem.Tier.Any;
        break;
      default:
        tier = this.tier;
        break;
    }
    this.tier = tier;
    System.Action onLocalGolemSet = Golem.OnLocalGolemSet;
    if (onLocalGolemSet != null)
      onLocalGolemSet();
    EventManager.onPossess += new EventManager.PossessEvent(this.OnPlayerPossess);
    this.nextMeleeAttackTime = Time.time;
    this.lastAbilityTime = Time.time;
    this.climbTime = Time.time;
    this.awakeTime = Time.time;
    this.navMeshAgent.updatePosition = false;
    this.navMeshAgent.updateRotation = false;
    this.RandomizeCrystals();
    base.Awake();
    this.shardItemData = Catalog.GetData<ItemData>(this.shardItemId);
    if (!this.autoDefeatOnStart && !this.autoDefeatAndKillOnStart)
      return;
    this.DelayedAction(0.5f, new System.Action(this.Defeat));
    if (!this.autoDefeatAndKillOnStart)
      return;
    this.DelayedAction(7f, (System.Action) (() =>
    {
      Time.timeScale = 0.2f;
      this.Kill();
    }));
  }

  public GolemAbilityType GetValidAbilityType()
  {
    if (this.isClimbed && this.allowClimbReact)
      return GolemAbilityType.Climb;
    return this.allowMelee && this.TargetInMeleeRange() ? GolemAbilityType.Melee : GolemAbilityType.Ranged;
  }

  public GolemAbility GetRandomAbilityOfType(GolemAbilityType type)
  {
    GolemAbility result;
    return !this.abilities.WeightedFilteredSelectInPlace<GolemAbility>((Func<GolemAbility, bool>) (ability => ability.Allow((GolemController) this) && ability.type == type), (Func<GolemAbility, float>) (ability => ability.weight), out result) && !this.abilities.WeightedFilteredSelectInPlace<GolemAbility>((Func<GolemAbility, bool>) (ability => ability.Allow((GolemController) this) && ability.type == type), (Func<GolemAbility, float>) (ability => ability.weight), out result) ? (GolemAbility) null : result;
  }

  private void RandomizeCrystals()
  {
    this.shardsToDrop = UnityEngine.Random.Range((int) this.activeCrystalConfig.minMaxShardsDropped.x, (int) this.activeCrystalConfig.minMaxShardsDropped.y + 1);
    foreach (Transform transform in this.golemCrystalRandomizer.RandomizeWeakPoints(this.activeCrystalConfig.golemCrystals).Shuffle<Transform>())
    {
      GolemCrystal crystal = transform.GetComponentInChildren<GolemCrystal>();
      if ((UnityEngine.Object) crystal == (UnityEngine.Object) null)
      {
        Debug.LogError((object) $"Weak point {transform.GetPathFrom(transform.root)} is not a golem crystal!");
      }
      else
      {
        crystal.onBreak?.AddListener((UnityAction) (() => this.CrystalBreak(crystal)));
        this.crystals.Add(crystal);
        Rigidbody component;
        if (crystal.TryGetComponent<Rigidbody>(out component))
          this.bodyParts.Add(component);
      }
    }
    this.RefreshWeakPoints();
  }

  public void RandomizeCrystalProtection()
  {
    if ((UnityEngine.Object) this.arenaCrystalRandomizer != (UnityEngine.Object) null)
    {
      int num = Mathf.Min(this.activeCrystalConfig.golemCrystals, this.activeCrystalConfig.arenaCrystals);
      int index = 0;
      foreach (Transform transform in this.arenaCrystalRandomizer.RandomizeWeakPoints(this.activeCrystalConfig.arenaCrystals).Shuffle<Transform>())
      {
        GolemCrystal arenaCrystal = transform.GetComponentInChildren<GolemCrystal>();
        if ((UnityEngine.Object) arenaCrystal == (UnityEngine.Object) null)
          Debug.LogError((object) $"Arena crystal {transform.GetPathFrom(transform.root)} is not a simple breakable!");
        else if (index < num)
        {
          GolemCrystal crystal = this.crystals[index];
          arenaCrystal.onShieldDisable.AddListener((UnityAction) (() => crystal.linkEffect.gameObject.SetActive(true)));
          arenaCrystal.onBreak.AddListener((UnityAction) (() =>
          {
            this.linkedArenaCrystals.Remove(arenaCrystal);
            crystal.DisableShield();
            Golem.GolemCrystalBreak arenaCrystalBreak = this.OnArenaCrystalBreak;
            if (arenaCrystalBreak != null)
              arenaCrystalBreak(arenaCrystal);
            this.Stun(this.activeCrystalConfig.arenaCrystalMaxStun);
          }));
          arenaCrystal.EnableShield();
          crystal.linkEffectTarget.SetParentOrigin(arenaCrystal.transform, new UnityEngine.Vector3?(UnityEngine.Vector3.zero), new Quaternion?(Quaternion.identity));
          this.linkedArenaCrystals.Add(arenaCrystal);
          ++index;
        }
      }
    }
    for (int index = 0; index < this.crystals.Count; ++index)
      this.crystals[index].EnableShield();
  }

  protected override void Start()
  {
    base.Start();
    this.SetAttackTarget(this.attackTarget);
  }

  protected override void ManagedOnEnable()
  {
    base.ManagedOnEnable();
    this.InvokeRepeating("OnCycle", this.cycleTime, this.cycleTime);
  }

  protected override void ManagedOnDisable()
  {
    base.ManagedOnDisable();
    this.CancelInvoke("OnCycle");
  }

  private void OnDestroy()
  {
    EventManager.onPossess -= new EventManager.PossessEvent(this.OnPlayerPossess);
  }

  protected virtual void OnPlayerPossess(Creature creature, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    this.SetAttackTarget(creature.ragdoll.targetPart.transform);
  }

  protected override IEnumerator WakeCoroutine()
  {
    yield return (object) base.WakeCoroutine();
    for (int index = 0; index < this.crystals.Count; ++index)
    {
      bool flag = index < this.linkedArenaCrystals.Count;
      if (!flag || index <= 0)
        (flag ? this.linkedArenaCrystals[index] : this.crystals[index]).DisableShield();
    }
  }

  public void RemoveNextShield(float delay)
  {
    if (this.linkedArenaCrystals.Count <= 0)
      return;
    if ((double) delay <= 0.0)
      this.linkedArenaCrystals[0].DisableShield();
    else
      this.DelayedAction(delay, (System.Action) (() => this.linkedArenaCrystals[0].DisableShield()));
  }

  public override void RefreshWeakPoints()
  {
    base.RefreshWeakPoints();
    this.weakpoints = new List<Transform>();
    for (int index = 0; index < this.crystals.Count; ++index)
      this.weakpoints.Add(this.crystals[index].transform.Find("Target") ?? this.crystals[index].transform);
  }

  public virtual void TargetPlayer()
  {
    Transform transform = Player.currentCreature?.ragdoll?.targetPart?.transform;
    if (transform == null)
      return;
    this.SetAttackTarget(transform);
  }

  protected internal override void ManagedUpdate()
  {
    base.ManagedUpdate();
    if (this.isKilled)
      return;
    bool flag1 = (UnityEngine.Object) this.attackTarget != (UnityEngine.Object) null && this.attackTarget.position.PointInRadius(this.transform.position + new UnityEngine.Vector3(0.0f, this.stopMoveSphereHeight, 0.0f), this.stopMoveDistance);
    if (!flag1)
      this.climbReactIntensity = 0;
    if (((!this.isStunned ? 0 : (this.waitStunApproach ? 1 : 0)) & (flag1 ? 1 : 0)) != 0)
    {
      this.stunEndTime = Time.time + this.activeCrystalConfig.arenaCrystalNearbyStun;
      this.waitStunApproach = false;
    }
    this.navMeshAgent.updateRotation = false;
    if (this.isDefeated && !this.stunInProgress)
    {
      this.Stun(0.0f, (System.Action) null, (System.Action) (() => this.UnlockFacePlate(true)), (System.Action) null);
    }
    else
    {
      if (this.isAwake && !this.isBusy && (bool) (UnityEngine.Object) this.attackTarget)
      {
        UnityEngine.Vector3 rhs = (this.navMeshAgent.nextPosition - this.transform.position) with
        {
          y = 0.0f
        };
        UnityEngine.Vector2 b = new UnityEngine.Vector2(UnityEngine.Vector3.Dot(this.transform.right, rhs), UnityEngine.Vector3.Dot(this.transform.forward, rhs));
        float t = Mathf.Min(1f, Time.deltaTime / 0.1f);
        this.smoothDeltaPosition = UnityEngine.Vector2.Lerp(this.smoothDeltaPosition, b, t);
        this.velocity = this.smoothDeltaPosition / Time.deltaTime;
        if ((double) this.navMeshAgent.remainingDistance <= (double) this.navMeshAgent.stoppingDistance)
          this.velocity = UnityEngine.Vector2.Lerp(UnityEngine.Vector2.zero, this.velocity, this.navMeshAgent.remainingDistance / this.navMeshAgent.stoppingDistance);
        bool flag2 = (double) this.navMeshAgent.remainingDistance <= (double) this.navMeshAgent.stoppingDistance;
        bool active = (double) this.velocity.magnitude > 0.5 && !flag2 && !flag1;
        bool flag3 = this.IsSightable(this.attackTarget, 1000f, this.forwardAngle);
        if ((double) this.navMeshAgent.destination.ToXZ().DistanceSqr(this.attackTarget.position.ToXZ()) > (double) this.navMeshAgent.stoppingDistance)
          active = !flag3 || !this.eyeTransform.position.ToXZ().PointInRadius(this.attackTarget.position.ToXZ(), this.minHeadToTargetDistance);
        bool flag4 = this.animatorEvent.rightFootPlanted && this.animatorEvent.leftFootPlanted;
        if (!active && !flag4)
          active = true;
        if (this.isClimbed)
        {
          active = false;
          if (!this.wasClimbed)
            this.climbTime = Time.time;
          if ((double) Time.time > (double) this.climbTime + ((double) this.climbReactTime - (double) this.climbReactWarningTime) && (double) this.lastReactWarningTime < (double) this.climbTime)
          {
            this.lastReactWarningTime = Time.time;
            this.climbReactWarningAudio.Play();
          }
          this.wasClimbed = true;
        }
        else if ((double) Time.time > (double) this.climbTime + (double) this.resetClimbedTime)
          this.wasClimbed = false;
        this.SetMove(active);
        if ((double) rhs.magnitude > (double) this.navMeshAgent.radius / 2.0)
          this.transform.position = UnityEngine.Vector3.Lerp(this.animator.rootPosition, this.navMeshAgent.nextPosition, t);
        if (active && !this.isBusy)
          this.navMeshAgent.updateRotation = !flag4;
      }
      this.currentAbility?.OnUpdate();
      if (!this.isAwake || !(bool) (UnityEngine.Object) this.attackTarget)
        return;
      this.UpdateSwingEffects();
    }
  }

  public override void OnAnimatorMove()
  {
    base.OnAnimatorMove();
    if (this.isKilled)
      return;
    this.navMeshAgent.nextPosition = this.animator.rootPosition;
  }

  public override void Kill()
  {
    this.Defeat();
    base.Kill();
    this.spawner?.onGolemKill?.Invoke();
  }

  public override void Stun(
    float duration = 0.0f,
    System.Action onStunStart = null,
    System.Action onStunnedBegin = null,
    System.Action onStunnedEnd = null)
  {
    base.Stun(duration, onStunStart, onStunnedBegin, onStunnedEnd);
    this.crystalsBrokenDuringStun = 0;
    this.navMeshAgent.updateRotation = false;
    this.LookAt((Transform) null);
    this.spawner?.onGolemStun?.Invoke();
  }

  public void RampageRanged(System.Action callback)
  {
    GolemAbility result;
    if (this.crystalBreakReactions.WeightedFilteredSelectInPlace<GolemAbility>((Func<GolemAbility, bool>) (each =>
    {
      if (each.rampageType != RampageType.Ranged)
        return false;
      return each.abilityTier == Golem.Tier.Any || this.tier == Golem.Tier.Any || each.abilityTier.HasFlagNoGC(this.tier);
    }), (Func<GolemAbility, float>) (_ => 1f), out result))
      this.StartAbility(result, callback);
    else
      this.RampageMelee(callback);
  }

  public void RampageMelee(System.Action callback)
  {
    GolemAbility result;
    if ((double) UnityEngine.Random.value > (double) this.normalRampageChance && this.crystalBreakReactions.WeightedFilteredSelectInPlace<GolemAbility>((Func<GolemAbility, bool>) (each =>
    {
      if (each.rampageType != RampageType.Melee)
        return false;
      return each.abilityTier == Golem.Tier.Any || this.tier == Golem.Tier.Any || each.abilityTier.HasFlagNoGC(this.tier);
    }), (Func<GolemAbility, float>) (_ => 1f), out result))
      this.StartAbility(result, callback);
    else
      this.PerformAttackMotion(GolemController.AttackMotion.Rampage);
  }

  protected void CrystalBreak(GolemCrystal crystal)
  {
    float f = (float) this.shardsToDrop / (float) this.crystals.Count;
    int num = UnityEngine.Random.Range(Mathf.FloorToInt(f), Mathf.CeilToInt(f) + 1);
    this.shardsToDrop -= num;
    for (int index = 0; index < num; ++index)
      this.shardItemData?.SpawnAsync(new Action<Item>(this.OnShardSpawn), new UnityEngine.Vector3?(crystal.transform.position), new Quaternion?(crystal.transform.rotation));
    Player.currentCreature.mana.RegenFocus(Player.currentCreature.mana.MaxFocus);
    this.crystals.Remove(crystal);
    if (this.crystals.IsNullOrEmpty())
    {
      this.EndAbility();
      this.isDefeated = true;
      this.ChangeState(GolemController.State.Defeated);
      this.characterController.enabled = false;
      this.targetHeadEmissionColor = this.defeatedEmissionColor;
      if (!this.animator.GetBool(GolemController.stunHash) || !this.stunInProgress)
        this.Stun(0.0f, (System.Action) null, (System.Action) (() => this.UnlockFacePlate(true)), (System.Action) null);
      else
        this.UnlockFacePlate(true);
    }
    else
    {
      if (this.animator.GetBool(GolemController.stunStartedHash))
      {
        if (++this.crystalsBrokenDuringStun >= this.crystalsBrokenToWake)
        {
          this.StopStun();
          this.StartCoroutine(this.RampageAfterStun());
        }
      }
      else if (this.state != GolemController.State.Rampage)
      {
        this.InvokeRampageState();
        this.Rampage();
      }
      this.crystalBreakEvent?.Invoke();
      Golem.GolemCrystalBreak golemCrystalBreak = this.OnGolemCrystalBreak;
      if (golemCrystalBreak != null)
        golemCrystalBreak(crystal);
    }
    this.RefreshWeakPoints();
    this.RemoveNextShield(this.shieldDisableDelay);
  }

  public void OnShardSpawn(Item item)
  {
    SkillTreeShard component = item.GetComponent<SkillTreeShard>();
    item.SetOwner(Item.Owner.Player);
    component.OnAbsorbEvent -= new SkillTreeShard.AbsorbEvent(this.OnShardAbsorb);
    component.OnAbsorbEvent += new SkillTreeShard.AbsorbEvent(this.OnShardAbsorb);
    component.FlyToPlayer();
  }

  public void OnShardAbsorb(SkillTreeShard shard)
  {
    (string str, float value) = ItemModuleConvertToCurrency.GetValue(shard.item);
    Player.characterData.inventory.AddCurrencyValue(str, value);
  }

  private IEnumerator RampageAfterStun()
  {
    Golem golem = this;
    while (golem.animator.GetBool(GolemController.stunStartedHash))
      yield return (object) null;
    if (!golem.isDefeated)
      golem.Rampage();
  }

  public override void Deploy(
    float duration = 0.0f,
    System.Action onDeployStart = null,
    System.Action onDeployedBegin = null,
    System.Action onDeployedEnd = null)
  {
    base.Deploy(duration, onDeployStart, onDeployedBegin, onDeployedEnd);
    this.navMeshAgent.updateRotation = false;
    this.LookAt(this.attackTarget);
  }

  public void OnCycle()
  {
    if (this.isKilled)
      return;
    if ((bool) (UnityEngine.Object) this.attackTarget)
    {
      if (this.awakeWhenTargetClose && !this.isAwake)
      {
        if (!this.transform.position.PointInRadius(this.attackTarget.position, this.awakeTargetDistance))
          return;
        this.LookAt(this.attackTarget);
        this.lastAbilityTime = Time.time;
        this.nextMeleeAttackTime = Time.time;
        this.headAimConstraint.weight = 1f;
        this.SetAwake(true);
      }
      this.navMeshAgent.SetDestination(this.attackTarget.position);
      bool flag = true;
      if (this.wasClimbed)
      {
        if (this.blockActionWhileClimbed)
          flag = false;
        if (this.allowClimbReact && !this.isBusy && this.isClimbed && (double) Time.time > (double) this.climbTime + (double) this.climbReactTime)
        {
          flag = false;
          this.TryClimbReact();
        }
      }
      if (flag && (UnityEngine.Object) this.currentAbility == (UnityEngine.Object) null && !this.isBusy && !this.inAttackMotion && !this.deployInProgress && !this.stunInProgress)
      {
        GolemAbilityType validAbilityType = this.GetValidAbilityType();
        if (this.allowMelee && this.TargetInMeleeRange())
        {
          if (this.setMeleeCooldown)
          {
            this.nextMeleeAttackTime = Time.time + UnityEngine.Random.Range(this.meleeMinMaxCooldown.x, this.meleeMinMaxCooldown.y);
            this.setMeleeCooldown = false;
          }
          if (validAbilityType == GolemAbilityType.Melee && (double) Time.time - (double) this.lastAbilityTime > (double) this.abilityCooldown)
          {
            GolemAbility randomAbilityOfType = this.GetRandomAbilityOfType(GolemAbilityType.Melee);
            if (randomAbilityOfType != null)
            {
              this.StartAbility(randomAbilityOfType);
              goto label_24;
            }
          }
          if ((double) Time.time >= (double) this.nextMeleeAttackTime && this.TryMeleeAttackTarget())
          {
            this.setMeleeCooldown = true;
            this.LookAt((Transform) null);
            this.navMeshAgent.updateRotation = false;
          }
        }
        else if ((double) Time.time - (double) this.lastAbilityTime > (double) this.abilityCooldown)
        {
          GolemAbility randomAbilityOfType = this.GetRandomAbilityOfType(validAbilityType);
          if (randomAbilityOfType != null)
            this.StartAbility(randomAbilityOfType);
        }
      }
label_24:
      this.currentAbility?.OnCycle(this.cycleTime);
      if (this.isBusy)
        return;
      this.LookAt(this.attackTarget);
    }
    else
      this.SetMove(false);
  }

  private bool TargetInMeleeRange()
  {
    return this.transform.position.PointInRadius(this.attackTarget.position, this.meleeMaxAttackDistance);
  }

  public void TryClimbReact()
  {
    if (this.climbReacts.IsNullOrEmpty())
    {
      Debug.LogError((object) "No climb reacts!");
    }
    else
    {
      GolemAbility ability = (GolemAbility) null;
      int num = this.climbReactIntensity + 1;
      if (num > 0)
      {
        for (int index = 0; index < this.climbReacts.Count; ++index)
        {
          if (this.climbReacts[index].Allow((GolemController) this))
          {
            ability = this.climbReacts[index];
            --num;
          }
          if (num == 0)
            break;
        }
      }
      if ((UnityEngine.Object) ability == (UnityEngine.Object) null)
      {
        Debug.LogError((object) "No available climb react!");
      }
      else
      {
        ++this.climbReactIntensity;
        this.StartAbility(ability, (System.Action) (() =>
        {
          if (this.isClimbed)
            return;
          --this.climbReactIntensity;
        }));
        this.wasClimbed = false;
        this.climbTime += this.climbReactTime;
      }
    }
  }

  public bool TryMeleeAttackTarget()
  {
    float targetDistance = UnityEngine.Vector3.Distance(this.transform.position.ToXZ(), this.attackTarget.position.ToXZ());
    float targetAngle = UnityEngine.Vector3.SignedAngle(this.transform.forward, this.attackTarget.position.ToXZ() - this.transform.position.ToXZ(), UnityEngine.Vector3.up);
    bool flag = false;
    GolemController.AttackMotion attack = GolemController.AttackMotion.Rampage;
    foreach (Golem.MeleeAttackRange attackRange in this.attackRanges)
    {
      flag = attackRange.TryUseRange(targetAngle, targetDistance, out attack, this.lastAttackMotion);
      if (flag)
        break;
    }
    if (flag)
    {
      this.PerformAttackMotion(attack);
      this.climbReactIntensity = 0;
    }
    return flag;
  }

  public delegate void GolemCrystalBreak(GolemCrystal crystal);

  [Serializable]
  public class CrystalConfig
  {
    public int golemCrystals = 8;
    public int arenaCrystals = 3;
    public float arenaCrystalNearbyStun = 10f;
    public float arenaCrystalMaxStun = 30f;
    public UnityEngine.Vector2 minMaxShardsDropped = new UnityEngine.Vector2(8f, 16f);
  }

  [Serializable]
  public class AttackRange
  {
    public UnityEngine.Vector2 angleMinMax;
    public UnityEngine.Vector2 distanceMinMax;

    public virtual string dynamicTitle => "Attack Range";

    public bool CheckAngleDistance(float targetAngle, float targetDistance)
    {
      return (double) targetAngle >= (double) this.angleMinMax.x && (double) targetAngle <= (double) this.angleMinMax.y && (double) targetDistance >= (double) this.distanceMinMax.x && (double) targetDistance <= (double) this.distanceMinMax.y;
    }
  }

  [Flags]
  public enum Tier
  {
    Any = 0,
    Tier1 = 1,
    Tier2 = 2,
    Tier3 = 4,
  }

  [Serializable]
  public class MeleeAttackRange : Golem.AttackRange
  {
    [Space]
    public Golem.MeleeAttackRange.WeightedAttack[] attackOptions;

    public override string dynamicTitle
    {
      get
      {
        string dynamicTitle = Utils.IsNullOrEmpty(this.attackOptions) ? "None" : "";
        int index = 0;
        while (true)
        {
          int num = index;
          Golem.MeleeAttackRange.WeightedAttack[] attackOptions = this.attackOptions;
          int length = attackOptions != null ? attackOptions.Length : 0;
          if (num < length)
          {
            Golem.MeleeAttackRange.WeightedAttack attackOption = this.attackOptions[index];
            dynamicTitle = $"{dynamicTitle}{attackOption.attack.ToString()} ({attackOption.weight.ToString("0.00")})";
            if (index != this.attackOptions.Length - 1)
              dynamicTitle += ", ";
            ++index;
          }
          else
            break;
        }
        return dynamicTitle;
      }
    }

    public bool TryUseRange(
      float targetAngle,
      float targetDistance,
      out GolemController.AttackMotion attack,
      GolemController.AttackMotion lastAttack = GolemController.AttackMotion.Rampage)
    {
      attack = this.attackOptions[UnityEngine.Random.Range(0, this.attackOptions.Length)].attack;
      Golem.MeleeAttackRange.WeightedAttack result;
      if (!((IEnumerable<Golem.MeleeAttackRange.WeightedAttack>) this.attackOptions).WeightedFilteredSelectInPlace<Golem.MeleeAttackRange.WeightedAttack>((Func<Golem.MeleeAttackRange.WeightedAttack, bool>) (at => at.attack != lastAttack), (Func<Golem.MeleeAttackRange.WeightedAttack, float>) (at => at.weight), out result))
        return false;
      attack = result.attack;
      return this.CheckAngleDistance(targetAngle, targetDistance);
    }

    [Serializable]
    public class WeightedAttack
    {
      public GolemController.AttackMotion attack;
      public float weight = 1f;
    }
  }

  [Serializable]
  public class InflictedStatus
  {
    public string data;
    public float duration = 3f;
    public float parameter;

    private List<ValueDropdownItem<string>> GetAllStatuses
    {
      get => Catalog.GetDropdownAllID<StatusData>();
    }
  }
}
