// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemBeam
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Skill.Spell;
using ThunderRoad.Skill.SpellMerge;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Beam config")]
public class GolemBeam : GolemAbility
{
  public static float lastBeamTime;
  [Header("Beam")]
  public float hitRange = 30f;
  public float radius = 1f;
  public float headSweepRange = 5f;
  public bool lockSweep;
  public LayerMask beamRaycastMask;
  public float beamMaxDistance = 50f;
  public float beamStartMaxAngle = 30f;
  public float beamAngleSoftMax = 60f;
  public float beamAngleHardMax = 80f;
  [Header("Effects")]
  public string chargeEffectID = "";
  public string beamEffectID = "";
  public bool stopChargeOnShoot;
  [Header("Impact")]
  public float damagePerSecond = 5f;
  public float damagePeriodTime = 0.1f;
  public bool blockable = true;
  public bool deflectOnBlock = true;
  public float appliedForce;
  public float blockedPushForce;
  public ForceMode appliedForceMode;
  public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();
  [Header("Timing")]
  public float beamCooldownDuration = 20f;
  public float beamStopDelayTargetLost = 3f;
  public UnityEngine.Vector2 beamingMinMaxDuration = new UnityEngine.Vector2(5f, 10f);
  [Header("Trail")]
  public bool leaveMoltenTrail;
  public string moltenBeamSpellId = "PlasmaBeam";
  [NonSerialized]
  public GolemBeam.State state;
  [NonSerialized]
  public EffectData chargeEffectData;
  [NonSerialized]
  public EffectData beamEffectData;
  [NonSerialized]
  public SpellMergeMoltenBeam moltenBeamSpell;
  [NonSerialized]
  public bool sweepSide;
  protected float headSweepRangeMultiplier = 1f;
  protected EffectInstance chargeEffect;
  protected EffectInstance beamEffect;
  protected EffectInstance deflectedBeamEffect;
  protected Transform deflectPoint;
  protected float lastTrailSpawn;
  protected FlameWall lastFlameWall;
  protected float beamTargetLostDuration;
  protected LookMode sweepMode;
  private float lastDeflect;
  private UnityEngine.Vector3 lastDeflectDirection;

  private List<ValueDropdownItem<string>> GetAllSpellIDs() => Catalog.GetDropdownAllID<SpellData>();

  public override bool HeadshotInterruptable => true;

  public override bool OverrideLook
  {
    get
    {
      switch (this.state)
      {
        case GolemBeam.State.Deploying:
        case GolemBeam.State.Firing:
          if ((UnityEngine.Object) this.golem != (UnityEngine.Object) null)
          {
            LookMode lookMode = this.golem.lookMode;
            return (lookMode == LookMode.HorizontalSweep ? 0 : (lookMode != LookMode.VerticalSweep ? 1 : 0)) == 0;
          }
          break;
      }
      return false;
    }
  }

  public override bool Allow(GolemController golem)
  {
    return base.Allow(golem) && (double) Time.time - (double) GolemBeam.lastBeamTime > (double) this.beamCooldownDuration && golem.IsSightable(golem.attackTarget, this.beamMaxDistance, this.beamStartMaxAngle);
  }

  public override void Begin(GolemController golem)
  {
    base.Begin(golem);
    GolemBeam.lastBeamTime = Time.time;
    if (this.chargeEffectData == null)
      this.chargeEffectData = Catalog.GetData<EffectData>(this.chargeEffectID);
    if (this.beamEffectData == null)
      this.beamEffectData = Catalog.GetData<EffectData>(this.beamEffectID);
    if ((UnityEngine.Object) this.deflectPoint == (UnityEngine.Object) null)
      this.deflectPoint = golem.transform.FindOrAddTransform("DeflectPoint", UnityEngine.Vector3.zero);
    if (this.leaveMoltenTrail && this.moltenBeamSpell == null)
      this.moltenBeamSpell = Catalog.GetData<SpellData>(this.moltenBeamSpellId) as SpellMergeMoltenBeam;
    golem.weakpoints.Add(golem.headCrystalBody.transform);
    this.beamTargetLostDuration = 0.0f;
    this.sweepMode = (LookMode) UnityEngine.Random.Range(0, 3);
    switch (this.sweepMode)
    {
      case LookMode.Follow:
      case LookMode.HorizontalSweep:
        this.sweepSide = UnityEngine.Random.Range(0, 2) != 0;
        break;
      case LookMode.VerticalSweep:
        this.sweepSide = false;
        break;
    }
    float duration = UnityEngine.Random.Range(this.beamingMinMaxDuration.x, this.beamingMinMaxDuration.y);
    golem.Deploy(duration, new System.Action(this.OnDeployStart), new System.Action(this.OnDeployed), new System.Action(this.OnDeployEnd));
  }

  public void OnDeployStart()
  {
    this.state = GolemBeam.State.Deploying;
    this.golem.lookMode = this.sweepMode;
    this.lockSweep = true;
    this.golem.headLookSpeedMultiplier = this.sweepMode == LookMode.Follow ? 0.5f : 1f;
    this.BeamCharge();
  }

  public void OnDeployed()
  {
    if (this.state != GolemBeam.State.Deploying)
      return;
    this.lockSweep = false;
    this.state = GolemBeam.State.Firing;
    this.BeamStartLoop();
  }

  public void OnDeployEnd() => this.End();

  public override void OnCycle(float delta)
  {
    base.OnCycle(delta);
    if (this.golem.IsSightable(this.golem.attackTarget, this.beamMaxDistance, this.beamAngleHardMax))
    {
      this.beamTargetLostDuration = 0.0f;
    }
    else
    {
      this.beamTargetLostDuration += delta;
      if ((double) this.beamTargetLostDuration <= (double) this.beamStopDelayTargetLost)
        return;
      this.End();
    }
  }

  public override void OnUpdate()
  {
    base.OnUpdate();
    switch (this.state)
    {
      case GolemBeam.State.Firing:
        this.BeamUpdate();
        break;
    }
  }

  public override void LookAt()
  {
    base.LookAt();
    this.golem.headIktarget.rotation = Quaternion.LookRotation(this.golem.lookingTarget.position - this.golem.transform.position, UnityEngine.Vector3.up);
    UnityEngine.Vector3 vector3 = this.golem.lookingTarget.position + (this.golem.lookMode == LookMode.VerticalSweep ? this.golem.headIktarget.transform.up : this.golem.headIktarget.transform.right) * ((this.sweepSide ? this.headSweepRange : -this.headSweepRange) * this.headSweepRangeMultiplier);
    this.golem.headIktarget.transform.position = UnityEngine.Vector3.MoveTowards(this.golem.headIktarget.transform.position, vector3, this.golem.headLookSpeed * this.golem.headLookSpeedMultiplier * Time.deltaTime);
    if (!this.lockSweep && this.golem.headIktarget.position.PointInRadius(vector3, 0.1f))
      this.sweepSide = !this.sweepSide;
    this.golem.eyeTransform.rotation = Quaternion.LookRotation(this.golem.headIktarget.position - this.golem.eyeTransform.transform.position, UnityEngine.Vector3.up);
  }

  public void BeamUpdate()
  {
    bool deflected = false;
    if (this.golem.WithinForwardCone(this.golem.attackTarget, 1000f, this.beamAngleHardMax))
      this.FireBeam(this.golem.eyeTransform.position, this.radius, this.golem.eyeTransform.forward, this.hitRange, this.beamRaycastMask, true, out deflected);
    else
      this.End();
    if (deflected || this.deflectedBeamEffect == null)
      return;
    this.deflectedBeamEffect.Stop();
    this.deflectedBeamEffect = (EffectInstance) null;
  }

  public void FireBeam(
    UnityEngine.Vector3 start,
    float radius,
    UnityEngine.Vector3 direction,
    float beamLength,
    LayerMask mask,
    bool tryDeflect,
    out bool deflected)
  {
    deflected = false;
    RaycastHit hitInfo;
    if (!Physics.SphereCast(start, radius, direction, out hitInfo, beamLength, (int) mask, QueryTriggerInteraction.Ignore))
      return;
    PhysicBody physicBody = hitInfo.GetPhysicBody();
    if ((object) physicBody != null)
    {
      Creature component1;
      if (physicBody == Player.local.locomotion.physicBody)
      {
        component1 = Player.currentCreature;
      }
      else
      {
        RagdollPart component2;
        if (physicBody.gameObject.TryGetComponent<RagdollPart>(out component2))
          component1 = component2.ragdoll.creature;
        else
          physicBody.gameObject.TryGetComponent<Creature>(out component1);
      }
      if ((UnityEngine.Object) component1 != (UnityEngine.Object) null)
      {
        if ((double) Time.time >= (double) component1.lastDamageTime + (double) this.damagePeriodTime)
        {
          component1.Damage(this.damagePerSecond * this.damagePeriodTime);
          foreach (Golem.InflictedStatus appliedStatuse in this.appliedStatuses)
            component1.Inflict(appliedStatuse.data, (object) this, appliedStatuse.duration, (object) appliedStatuse.parameter);
        }
        component1.AddForce(this.golem.eyeTransform.forward * this.appliedForce, this.appliedForceMode, 1f, (CollisionHandler) null);
      }
      else
      {
        Item component3;
        if (physicBody.gameObject.TryGetComponent<Item>(out component3))
        {
          if ((UnityEngine.Object) component3.breakable != (UnityEngine.Object) null)
            component3.breakable.Break();
          else if (tryDeflect && component3.IsHeldByPlayer)
          {
            UnityEngine.Vector3 deflectNormal = this.GetDeflectNormal(beamLength, component3, hitInfo.normal);
            UnityEngine.Vector3 b = UnityEngine.Vector3.Reflect(direction, deflectNormal);
            this.lastDeflectDirection = (double) Time.unscaledTime - (double) this.lastDeflect > 0.5 ? b : UnityEngine.Vector3.Slerp(this.lastDeflectDirection, b, Time.unscaledDeltaTime * 5f);
            this.lastDeflect = Time.unscaledTime;
            deflected = true;
            this.deflectPoint.SetParent(hitInfo.collider.transform);
            this.deflectPoint.SetPositionAndRotation(hitInfo.point, Quaternion.LookRotation(this.lastDeflectDirection));
            if (this.deflectedBeamEffect == null)
            {
              this.deflectedBeamEffect = this.beamEffectData.Spawn(this.deflectPoint, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f, typeof (EffectModuleAudio));
              this.deflectedBeamEffect.Play();
            }
            this.FireBeam(hitInfo.point, radius, this.lastDeflectDirection, beamLength, mask, false, out bool _);
          }
        }
        this.deflectPoint.SetParent((Transform) null);
        physicBody.rigidBody.GetComponentInParent<SimpleBreakable>()?.Break();
      }
    }
    else
    {
      if (!this.leaveMoltenTrail)
        return;
      this.TrySpawnTrail(hitInfo, this.golem.eyeTransform.forward);
    }
  }

  public virtual void BeamCharge()
  {
    this.chargeEffect = this.chargeEffectData.Spawn(this.golem.eyeTransform);
    this.chargeEffect.Play();
  }

  public virtual void BeamStartLoop()
  {
    if (this.stopChargeOnShoot && this.chargeEffect != null)
    {
      this.chargeEffect.End();
      this.chargeEffect = (EffectInstance) null;
    }
    this.beamEffect?.End();
    this.beamEffect = this.beamEffectData.Spawn(this.golem.eyeTransform);
    this.beamEffect.Play();
    if (this.beamEffect == null)
      return;
    foreach (EffectParticle effectParticle in this.beamEffect.effects.OfType<EffectParticle>())
    {
      effectParticle.rootParticleSystem.collision.collidesWith = this.beamRaycastMask;
      foreach (EffectParticleChild child in effectParticle.childs)
        child.particleSystem.collision.collidesWith = this.beamRaycastMask;
    }
  }

  public override void OnEnd()
  {
    base.OnEnd();
    GolemBeam.lastBeamTime = Time.time;
    this.state = GolemBeam.State.Finished;
    if (this.golem.isDeployed)
      this.golem.StopDeploy();
    this.golem.headLookSpeedMultiplier = 1f;
    this.golem.weakpoints.Remove(this.golem.headCrystalBody.transform);
    if (this.chargeEffect != null)
    {
      this.chargeEffect.End();
      this.chargeEffect = (EffectInstance) null;
    }
    if (this.beamEffect != null)
    {
      this.beamEffect.End();
      this.beamEffect = (EffectInstance) null;
    }
    if (this.deflectedBeamEffect == null)
      return;
    this.deflectedBeamEffect.End();
    this.deflectedBeamEffect = (EffectInstance) null;
  }

  private UnityEngine.Vector3 GetDeflectNormal(float beamLength, Item item, UnityEngine.Vector3 defaultNormal)
  {
    List<RaycastHit> raycastHitList = new List<RaycastHit>();
    for (int index = 0; index < 3; ++index)
    {
      RaycastHit hit;
      if (this.Raycast(Quaternion.AngleAxis(120f * (float) index, UnityEngine.Vector3.forward) * UnityEngine.Vector3.up * 0.01f, beamLength, out hit) && (UnityEngine.Object) hit.rigidbody?.GetComponent<CollisionHandler>()?.item == (UnityEngine.Object) item)
        raycastHitList.Add(hit);
    }
    switch (raycastHitList.Count)
    {
      case 0:
        return defaultNormal;
      case 3:
        Plane plane;
        ref Plane local = ref plane;
        RaycastHit raycastHit = raycastHitList[0];
        UnityEngine.Vector3 point1 = raycastHit.point;
        raycastHit = raycastHitList[1];
        UnityEngine.Vector3 point2 = raycastHit.point;
        raycastHit = raycastHitList[2];
        UnityEngine.Vector3 point3 = raycastHit.point;
        local = new Plane(point1, point2, point3);
        if ((double) UnityEngine.Vector3.Dot(plane.normal, this.golem.eyeTransform.forward) > 0.0)
          plane.Flip();
        return plane.normal;
      default:
        UnityEngine.Vector3 vector3 = defaultNormal;
        for (int index = 0; index < raycastHitList.Count; ++index)
          vector3 += raycastHitList[index].normal;
        return vector3 / (float) (raycastHitList.Count + 1);
    }
  }

  private bool Raycast(UnityEngine.Vector3 offset, float distance, out RaycastHit hit)
  {
    return Physics.Raycast(this.golem.eyeTransform.position + this.golem.eyeTransform.TransformPoint(offset), this.golem.eyeTransform.forward, out hit, distance, (int) this.beamRaycastMask, QueryTriggerInteraction.Ignore);
  }

  public void TrySpawnTrail(RaycastHit hit, UnityEngine.Vector3 direction)
  {
    if ((UnityEngine.Object) hit.rigidbody != (UnityEngine.Object) null)
      return;
    float num1 = Time.time - this.lastTrailSpawn;
    float num2 = (UnityEngine.Object) this.lastFlameWall != (UnityEngine.Object) null ? (this.lastFlameWall.transform.position - hit.point).sqrMagnitude : float.PositiveInfinity;
    if ((UnityEngine.Object) this.lastFlameWall != (UnityEngine.Object) null && (double) num1 < (double) this.moltenBeamSpell.trailMaxDelay && ((double) num1 < (double) this.moltenBeamSpell.trailMinDelay || (double) num2 < (double) this.moltenBeamSpell.trailMinDistance * (double) this.moltenBeamSpell.trailMinDistance) && (double) num2 < (double) this.moltenBeamSpell.trailMaxDistance * (double) this.moltenBeamSpell.trailMaxDistance)
      return;
    this.lastFlameWall = this.moltenBeamSpell.SpawnTrail(hit.point, Quaternion.LookRotation(UnityEngine.Vector3.Cross(hit.normal, UnityEngine.Random.onUnitSphere), hit.normal));
    this.lastFlameWall.ignorePlayer = false;
    this.lastFlameWall.statusData = Catalog.GetData<StatusData>(this.appliedStatuses[0].data);
    this.lastFlameWall.statusDuration = this.appliedStatuses[0].duration;
    this.lastFlameWall.heatPerSecond = this.appliedStatuses[0].parameter;
    this.lastTrailSpawn = Time.time;
  }

  public enum State
  {
    Deploying,
    Firing,
    Finished,
  }
}
