// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemThrow
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Throw config")]
public class GolemThrow : GolemAbility
{
  public static float lastThrowTime;
  public string summonEffectID;
  public string throwObjectID;
  public string objectEffectID;
  public AnimationCurve objectEffectIntensityCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1f, 1f);
  public LayerMask objectSpawnRaycastMask;
  public float throwVelocity = 5f;
  public float throwCooldownDuration = 20f;
  public float throwMaxDistance = 50f;
  public float throwMaxAngle = 30f;
  public float gravityMultiplier = 1f;
  public Side grabArmSide;
  public UnityEngine.Vector3 holdPosition = UnityEngine.Vector3.zero;
  public float holdForce;
  public float holdDamper;
  public float explosionRadius = 10f;
  public float explosionDamage = 20f;
  public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();
  public float explosionForce = 20f;
  public ForceMode forceMode = ForceMode.Impulse;
  public float upwardForceMult = 1f;
  public LayerMask explosionLayerMask;
  public string explosionEffectID;
  [NonSerialized]
  public EffectData summonEffectData;
  [NonSerialized]
  public ItemData throwObjectData;
  [NonSerialized]
  public EffectData objectEffectData;
  [NonSerialized]
  public EffectData explosionEffectData;
  protected ConfigurableJoint holdJoint;
  protected Item throwingObject;
  protected Transform part;

  private List<ValueDropdownItem<string>> GetAllItemID() => Catalog.GetDropdownAllID(Category.Item);

  private List<ValueDropdownItem<string>> GetAllEffectID()
  {
    return Catalog.GetDropdownAllID(Category.Effect);
  }

  public override bool Allow(GolemController golem)
  {
    return base.Allow(golem) && (double) Time.time - (double) GolemThrow.lastThrowTime > (double) this.throwCooldownDuration && golem.IsSightable(golem.attackTarget, this.throwMaxDistance, this.throwMaxAngle);
  }

  public override void Begin(GolemController golem)
  {
    base.Begin(golem);
    GolemThrow.lastThrowTime = Time.time;
    golem.PerformAttackMotion(GolemController.AttackMotion.Throw, new System.Action(((GolemAbility) this).End));
    this.part = golem.GetHand(this.grabArmSide);
    if (this.summonEffectData == null)
      this.summonEffectData = Catalog.GetData<EffectData>(this.summonEffectID);
    if (this.throwObjectData == null)
      this.throwObjectData = Catalog.GetData<ItemData>(this.throwObjectID);
    if (this.objectEffectData == null)
      this.objectEffectData = Catalog.GetData<EffectData>(this.objectEffectID);
    if (this.explosionEffectData != null)
      return;
    this.explosionEffectData = Catalog.GetData<EffectData>(this.explosionEffectID);
  }

  public override void AbilityStep(int step)
  {
    base.AbilityStep(step);
    switch (step)
    {
      case 1:
        this.ConjureObject();
        break;
      case 2:
        this.ReleaseObject();
        break;
    }
  }

  public override void OnEnd()
  {
    base.OnEnd();
    GolemThrow.lastThrowTime = Time.time;
    this.ReleaseObject(false);
  }

  public virtual void ConjureObject()
  {
    UnityEngine.Vector3 vector3 = this.part.transform.TransformPoint(this.holdPosition);
    UnityEngine.Vector3 spawnPoint = vector3;
    RaycastHit hit;
    bool hasHitpoint = Physics.Raycast(this.part.transform.position, (this.part.transform.TransformPoint(this.holdPosition) - this.part.transform.position).normalized, out hit, 10f, (int) this.objectSpawnRaycastMask, QueryTriggerInteraction.Ignore);
    this.throwObjectData?.SpawnAsync((Action<Item>) (item =>
    {
      if (hasHitpoint)
      {
        this.summonEffectData?.Spawn(hit.point, Quaternion.identity, (Transform) null, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f).Play();
        spawnPoint = hit.point;
      }
      item.transform.position = spawnPoint;
      item.DisallowDespawn = true;
      this.throwingObject = item;
      this.throwingObject.SetPhysicModifier((object) this, new float?(this.gravityMultiplier), 1f, -1f, -1f, -1f, (EffectData) null);
      ItemMagicAreaProjectile component;
      if (item.TryGetComponent<ItemMagicAreaProjectile>(out component))
      {
        component.explosionEffectData = this.explosionEffectData;
        component.OnHit -= new ItemMagicAreaProjectile.Hit(this.ObjectCollide);
        component.OnHit += new ItemMagicAreaProjectile.Hit(this.ObjectCollide);
        component.areaRadius = this.explosionRadius;
        component.guidance = GuidanceMode.NonGuided;
        component.guidanceFunc = (Func<UnityEngine.Vector3>) null;
        component.speed = this.throwVelocity;
        component.effectIntensityCurve = this.objectEffectIntensityCurve;
        component.Fire(UnityEngine.Vector3.zero, this.objectEffectData);
        component.doExplosion = false;
      }
      else
      {
        if (this.objectEffectData != null)
          item.StartCoroutine(this.ForceObjectEffect(item, this.objectEffectData));
        if ((UnityEngine.Object) item.breakable != (UnityEngine.Object) null)
        {
          item.OnBreakStart -= new Item.BreakStartDelegate(this.ItemBreak);
          item.OnBreakStart += new Item.BreakStartDelegate(this.ItemBreak);
        }
        else
        {
          item.mainCollisionHandler.OnCollisionStartEvent -= new CollisionHandler.CollisionEvent(this.ObjectCollide);
          item.mainCollisionHandler.OnCollisionStartEvent += new CollisionHandler.CollisionEvent(this.ObjectCollide);
        }
      }
      Rigidbody componentInParent = this.part.GetComponentInParent<Rigidbody>();
      this.holdJoint = componentInParent.gameObject.AddComponent<ConfigurableJoint>();
      this.holdJoint.autoConfigureConnectedAnchor = false;
      this.holdJoint.SetConnectedPhysicBody(item.physicBody);
      this.holdJoint.anchor = componentInParent.transform.InverseTransformPoint(this.part.transform.TransformPoint(this.holdPosition));
      this.holdJoint.connectedAnchor = UnityEngine.Vector3.zero;
      this.holdJoint.targetPosition = UnityEngine.Vector3.zero;
      JointDrive jointDrive = new JointDrive()
      {
        positionSpring = this.holdForce,
        positionDamper = this.holdDamper,
        maximumForce = float.PositiveInfinity
      };
      this.holdJoint.xDrive = jointDrive;
      this.holdJoint.yDrive = jointDrive;
      this.holdJoint.zDrive = jointDrive;
      Collider[] componentsInChildren = componentInParent.GetComponentsInChildren<Collider>();
      foreach (Collider componentsInChild in item.GetComponentsInChildren<Collider>())
      {
        Collider collider = componentsInChild;
        collider.enabled = false;
        if ((UnityEngine.Object) hit.collider != (UnityEngine.Object) null)
        {
          Physics.IgnoreCollision(collider, hit.collider);
          item.DelayedAction(0.5f, (System.Action) (() => Physics.IgnoreCollision(collider, hit.collider, false)));
        }
        foreach (Collider collider1 in componentsInChildren)
          Physics.IgnoreCollision(collider1, collider, true);
      }
      LightProbeVolume closestLightMapVolume;
      int volumeFromPosition = (int) LightVolumeReceiver.GetVolumeFromPosition(item.lightVolumeReceiver.transform.position, out closestLightMapVolume, this.golem.GetComponentInParent<Area>());
      item.lightVolumeReceiver.TriggerEnter((Collider) closestLightMapVolume.BoxCollider);
    }), new UnityEngine.Vector3?(vector3), new Quaternion?(this.golem.transform.rotation));
  }

  public virtual void ReleaseObject(bool launch = true)
  {
    if ((UnityEngine.Object) this.throwingObject == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) this.holdJoint != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.holdJoint);
    if (launch)
    {
      UnityEngine.Vector3 target = this.golem.transform.position + this.golem.transform.forward * 15f;
      if ((double) Mathf.Abs(UnityEngine.Vector3.SignedAngle(this.golem.transform.forward, (this.golem.attackTarget.position.ToXZ() - this.golem.transform.position.ToXZ()).normalized, this.golem.transform.up)) <= 45.0)
      {
        Ragdoll componentInParent = this.golem.attackTarget?.GetComponentInParent<Ragdoll>();
        if (componentInParent != null)
          target = componentInParent.targetPart.transform.position;
      }
      UnityEngine.Vector3 launchVector;
      this.throwingObject.physicBody.CalculateBodyLaunchVector(target, out launchVector, this.throwVelocity, this.gravityMultiplier);
      this.throwingObject.physicBody.velocity = launchVector;
    }
    Collider[] array = this.throwingObject.GetComponentsInChildren<Collider>();
    int index1 = 0;
    while (true)
    {
      int num = index1;
      Collider[] colliderArray = array;
      int length = colliderArray != null ? colliderArray.Length : 0;
      if (num < length)
      {
        array[index1].enabled = true;
        ++index1;
      }
      else
        break;
    }
    this.throwingObject.RunAfter((System.Action) (() =>
    {
      Collider[] componentsInChildren = this.part.GetComponentInParent<Rigidbody>().GetComponentsInChildren<Collider>();
      int index2 = 0;
      while (true)
      {
        int num = index2;
        Collider[] colliderArray = array;
        int length = colliderArray != null ? colliderArray.Length : 0;
        if (num < length)
        {
          foreach (Collider collider1 in componentsInChildren)
            Physics.IgnoreCollision(collider1, array[index2], false);
          ++index2;
        }
        else
          break;
      }
    }), 0.5f);
    ItemMagicAreaProjectile component;
    if (this.throwingObject.TryGetComponent<ItemMagicAreaProjectile>(out component))
      component.doExplosion = true;
    else
      this.throwingObject.Throw(flyDetection: Item.FlyDetection.Forced);
  }

  private void ObjectCollide(CollisionInstance collision)
  {
    CollisionHandler collisionHandler = collision.sourceColliderGroup?.collisionHandler;
    if (collisionHandler != null)
    {
      ItemMagicAreaProjectile component;
      if ((bool) (UnityEngine.Object) collisionHandler.item && collisionHandler.item.TryGetComponent<ItemMagicAreaProjectile>(out component))
        component.OnHit -= new ItemMagicAreaProjectile.Hit(this.ObjectCollide);
      collisionHandler.OnCollisionStartEvent -= new CollisionHandler.CollisionEvent(this.ObjectCollide);
    }
    collision.targetCollider.attachedRigidbody?.GetComponentInParent<Golem>()?.StaggerImpact(collision.contactPoint);
    GolemBlast.Explosion(collision.contactPoint, this.explosionRadius, this.explosionLayerMask, this.explosionDamage, this.appliedStatuses, this.explosionForce, this.upwardForceMult, this.forceMode, true, (System.Action) (() => this.throwingObject = (Item) null));
  }

  private void ItemBreak(Breakable breakable)
  {
    breakable.LinkedItem.OnBreakStart -= new Item.BreakStartDelegate(this.ItemBreak);
    UnityEngine.Collision breakingCollision = breakable.breakingCollision;
    if (breakingCollision != null)
    {
      ContactPoint contact = breakable.breakingCollision.GetContact(0);
      breakingCollision.collider?.GetComponentInParent<Golem>()?.StaggerImpact(contact.point);
      this.explosionEffectData?.Spawn(contact.point, Quaternion.LookRotation(UnityEngine.Vector3.Cross(contact.normal, UnityEngine.Vector3.forward), contact.normal), (Transform) null, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f)?.Play();
    }
    GolemBlast.Explosion(breakable.transform.position, this.explosionRadius, this.explosionLayerMask, this.explosionDamage, this.appliedStatuses, this.explosionForce, this.upwardForceMult, this.forceMode, true, (System.Action) (() => this.throwingObject = (Item) null));
  }

  protected IEnumerator ForceObjectEffect(Item item, EffectData effectData)
  {
    EffectInstance effect = effectData?.Spawn(item.transform, (CollisionInstance) null, true, (ColliderGroup) null, false, 0.0f, 1f);
    if (effect != null)
    {
      effect.Play();
      item.OnDespawnEvent += (Item.SpawnEvent) (time =>
      {
        if (time != EventTime.OnStart)
          return;
        effect?.End();
      });
      item.OnBreakStart += (Item.BreakStartDelegate) (_ => effect?.End());
      while ((bool) (UnityEngine.Object) item && !item.despawning && effect.isPlaying)
      {
        effect.SetIntensity(item.Velocity.magnitude.RemapClamp01(3f, 15f));
        yield return (object) 0;
      }
    }
  }
}
