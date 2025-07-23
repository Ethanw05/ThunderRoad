// Decompiled with JetBrains decompiler
// Type: ThunderRoad.GolemMeleeDamager
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

public class GolemMeleeDamager : MonoBehaviour
{
  public Rigidbody rigidbody;
  public GolemController golemController;
  public GolemAnimatorEvent golemAnimatorEvent;
  public List<Collider> colliders;
  public bool disableColliderDuringAttack = true;
  public float hitDamage = 20f;
  public float hitForce = 10f;
  public float hitForceUpward = 3f;
  public bool shieldBlocksDamage = true;
  public bool shieldBlocksForce;
  public float shieldBlockRadius = 0.75f;
  public AudioSource blockAudio;
  public UnityEvent onHit;
  protected bool hitBoxEnabled;
  protected UnityEngine.Vector3 lastPosition;
  protected List<Creature> hitCreatures = new List<Creature>();

  private void OnEnable()
  {
    this.golemAnimatorEvent.onEnableHitbox += new Action<bool>(this.OnEnableHitbox);
    this.golemController.OnGolemStateChange += new GolemController.GolemStateChange(this.GolemStateChange);
  }

  private void GolemStateChange(GolemController.State newState)
  {
    if (newState == GolemController.State.Active || newState == GolemController.State.Rampage)
      return;
    this.OnEnableHitbox(false);
  }

  private void OnDisable()
  {
    this.golemAnimatorEvent.onEnableHitbox -= new Action<bool>(this.OnEnableHitbox);
    this.golemController.OnGolemStateChange -= new GolemController.GolemStateChange(this.GolemStateChange);
  }

  private void OnEnableHitbox(bool hitBoxEnabled)
  {
    if (this.disableColliderDuringAttack)
    {
      foreach (Collider collider in this.colliders)
        collider.enabled = !hitBoxEnabled;
    }
    this.hitBoxEnabled = hitBoxEnabled;
    this.hitCreatures.Clear();
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!this.hitBoxEnabled || !(bool) (UnityEngine.Object) other.attachedRigidbody || this.golemController.isClimbed)
      return;
    Creature componentInParent = other.attachedRigidbody.GetComponentInParent<Creature>();
    if (this.hitCreatures.Contains(componentInParent) || !(bool) (UnityEngine.Object) componentInParent)
      return;
    UnityEngine.Vector3 velocity = (this.transform.position - this.lastPosition) / Time.deltaTime;
    bool flag = false;
    if (this.shieldBlocksDamage || this.shieldBlocksForce)
    {
      flag = this.ShieldIsBlocking(other.attachedRigidbody.transform.position, velocity, componentInParent, Side.Right) || this.ShieldIsBlocking(other.attachedRigidbody.transform.position, velocity, componentInParent, Side.Left);
      if (flag && !this.blockAudio.isPlaying)
        this.blockAudio.Play();
    }
    if (!flag || !this.shieldBlocksForce)
    {
      componentInParent.currentLocomotion.physicBody.velocity = UnityEngine.Vector3.zero;
      componentInParent.currentLocomotion.physicBody.AddForce((velocity.normalized.ToXZ() * this.hitForce + new UnityEngine.Vector3(0.0f, this.hitForceUpward, 0.0f)) * this.golemController.hitForceMultiplier, ForceMode.VelocityChange);
    }
    if ((!flag || !this.shieldBlocksDamage) && (double) this.hitDamage > 0.0)
      componentInParent.Damage(this.hitDamage * this.golemController.hitDamageMultiplier, DamageType.Blunt);
    this.onHit.Invoke();
    this.hitCreatures.Add(componentInParent);
  }

  private bool ShieldIsBlocking(
    UnityEngine.Vector3 bodyPosition,
    UnityEngine.Vector3 velocity,
    Creature creature,
    Side side)
  {
    bool flag = false;
    Item obj = creature.GetHand(side)?.grabbedHandle?.item;
    if ((UnityEngine.Object) obj == (UnityEngine.Object) null || obj.data.type != ItemData.Type.Shield || (double) UnityEngine.Vector3.Dot(obj.parryPoint.forward, velocity) > 0.0)
      return flag;
    UnityEngine.Vector3 vector3 = bodyPosition - velocity;
    float enter;
    if ((double) vector3.DistanceSqr(bodyPosition) < (double) vector3.DistanceSqr(obj.parryPoint.position) || !new Plane(obj.parryPoint.forward, obj.parryPoint.position).Raycast(new Ray(vector3, velocity.normalized), out enter))
      return flag;
    UnityEngine.Vector3 vectorB = vector3 + velocity.normalized * enter;
    flag = obj.parryPoint.position.PointInRadius(vectorB, this.shieldBlockRadius);
    return flag;
  }

  private void Update() => this.lastPosition = this.transform.position;
}
