// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemBlast
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Blast config")]
public class GolemBlast : GolemAbility
{
  public GolemController.AttackMotion motion;
  public UnityEngine.Vector3 blastLocalPosition;
  public bool attachToBone;
  public HumanBodyBones blastLinkedBone = HumanBodyBones.UpperChest;
  public float blastRadius = 5f;
  public string blastEffectID = "";
  public UnityEngine.Vector3 effectEulers = UnityEngine.Vector3.zero;
  public bool kickPlayerOff;
  public bool damageBreakables;
  public LayerMask blastMask;
  public float blastDamage = 5f;
  public float blastForce = 5f;
  public float blastForceUpwardMult = 1.5f;
  public ForceMode blastForceMode = ForceMode.VelocityChange;
  public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();
  public Golem.AttackRange attackRange;
  [NonSerialized]
  public EffectData blastEffectData;
  [NonSerialized]
  public Transform blastReference;

  private void OnValidate()
  {
    if (!(this.attackRange is Golem.MeleeAttackRange attackRange))
      return;
    this.attackRange = new Golem.AttackRange()
    {
      angleMinMax = attackRange.angleMinMax,
      distanceMinMax = attackRange.distanceMinMax
    };
  }

  public override bool Allow(GolemController golem)
  {
    float targetDistance = UnityEngine.Vector3.Distance(golem.transform.position.ToXZ(), golem.attackTarget.position.ToXZ());
    float targetAngle = UnityEngine.Vector3.SignedAngle(golem.transform.forward, golem.attackTarget.position.ToXZ() - golem.transform.position.ToXZ(), UnityEngine.Vector3.up);
    if (!base.Allow(golem) || this.type != GolemAbilityType.Climb && golem.lastAttackMotion == this.motion)
      return false;
    return this.type != GolemAbilityType.Melee || this.attackRange.CheckAngleDistance(targetAngle, targetDistance);
  }

  public override void Begin(GolemController golem)
  {
    base.Begin(golem);
    this.blastReference = golem.transform;
    if (this.attachToBone)
      this.blastReference = golem.animator.GetBoneTransform(this.blastLinkedBone);
    if (this.blastEffectData == null)
      this.blastEffectData = Catalog.GetData<EffectData>(this.blastEffectID);
    golem.PerformAttackMotion(this.motion, new System.Action(((GolemAbility) this).End));
  }

  public override void AbilityStep(int step)
  {
    base.AbilityStep(step);
    UnityEngine.Vector3 position = this.blastReference.TransformPoint(this.blastLocalPosition);
    if (this.kickPlayerOff && this.golem.isClimbed)
      this.golem.ForceUngripClimbers(true, true);
    this.blastEffectData.Spawn(position, Quaternion.Euler(this.effectEulers), (Transform) null, (CollisionInstance) null, true, (ColliderGroup) null, false, 1f, 1f).Play();
    GolemBlast.Explosion(position, this.blastRadius, this.blastMask, this.blastDamage, this.appliedStatuses, this.blastForce, this.blastForceUpwardMult, this.blastForceMode, this.damageBreakables, (System.Action) null);
  }

  public static void Explosion(
    UnityEngine.Vector3 position,
    float radius,
    LayerMask layerMask,
    float damage,
    List<Golem.InflictedStatus> statuses,
    float force,
    float upwardsMult,
    ForceMode forceMode,
    bool hitBreakables,
    System.Action endCallback)
  {
    Action<Breakable, PhysicBody[]> breakHandler = (Action<Breakable, PhysicBody[]>) null;
    EventManager.OnItemBrokenEnd += new EventManager.BreakEndDelegate(BreakableBroken);
    HashSet<GameObject> gameObjectSet = new HashSet<GameObject>();
    foreach (Component component1 in Physics.OverlapSphere(position, radius, (int) layerMask, QueryTriggerInteraction.Ignore))
    {
      PhysicBody physicBody = component1.GetPhysicBody();
      if ((object) physicBody != null)
      {
        ThunderEntity component2 = (ThunderEntity) null;
        if ((UnityEngine.Object) physicBody.gameObject == (UnityEngine.Object) Player.local.gameObject)
        {
          component2 = (ThunderEntity) Player.currentCreature;
        }
        else
        {
          RagdollPart component3;
          if (physicBody.gameObject.TryGetComponent<RagdollPart>(out component3))
            component2 = (ThunderEntity) component3.ragdoll.creature;
          else
            physicBody.gameObject.TryGetComponent<ThunderEntity>(out component2);
        }
        if (!((UnityEngine.Object) component2 == (UnityEngine.Object) null) && !gameObjectSet.Contains(component2.gameObject))
        {
          if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
          {
            gameObjectSet.Add(component2.gameObject);
            component2.AddExplosionForce(force, position, radius, upwardsMult, forceMode);
            if (component2 is Creature creature)
            {
              creature.Damage(damage);
              foreach (Golem.InflictedStatus statuse in statuses)
                creature.Inflict(statuse.data, (object) "Golem explosion", statuse.duration, (object) statuse.parameter);
            }
            int num1;
            Breakable breakable;
            if (component2 is Item obj)
            {
              breakable = obj.breakable;
              num1 = breakable != null ? 1 : 0;
            }
            else
              num1 = 0;
            int num2 = hitBreakables ? 1 : 0;
            if ((num1 & num2) != 0 && !breakable.contactBreakOnly && (double) force * (double) force >= (double) breakable.instantaneousBreakDamage)
            {
              breakHandler = (Action<Breakable, PhysicBody[]>) ((broken, pieces) =>
              {
                if ((UnityEngine.Object) broken != (UnityEngine.Object) breakable)
                  return;
                foreach (PhysicBody piece in pieces)
                  piece.AddExplosionForce(force, position, radius, upwardsMult, forceMode);
              });
              breakable.Break();
            }
          }
          else
          {
            gameObjectSet.Add(physicBody.gameObject);
            physicBody.AddExplosionForce(force, position, radius, upwardsMult, forceMode);
          }
        }
      }
    }
    if (endCallback == null)
      return;
    endCallback();

    void BreakableBroken(Breakable breakable, PhysicBody[] pieces)
    {
      Action<Breakable, PhysicBody[]> action = breakHandler;
      if (action == null)
        return;
      action(breakable, pieces);
    }
  }
}
