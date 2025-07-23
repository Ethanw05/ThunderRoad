// Decompiled with JetBrains decompiler
// Type: ThunderRoad.RagdollMassScalar
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (Ragdoll))]
public class RagdollMassScalar : MonoBehaviour
{
  public RagdollMassScalar.PhysicBodyScalar standing = new RagdollMassScalar.PhysicBodyScalar();
  public RagdollMassScalar.HandledScalar handled = new RagdollMassScalar.HandledScalar();
  public RagdollMassScalar.RagdolledScalar ragdolled = new RagdollMassScalar.RagdolledScalar();

  private void Awake()
  {
    if (!Application.isPlaying)
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void OnValidate()
  {
    this.standing.OnValidate(this.transform);
    this.handled.OnValidate(this.transform);
    this.ragdolled.OnValidate(this.transform);
  }

  private void OnTransformChildrenChanged()
  {
    this.standing.ChildrenChanged(this.transform);
    this.handled.ChildrenChanged(this.transform);
    this.ragdolled.ChildrenChanged(this.transform);
  }

  private void OnDrawGizmos()
  {
    this.standing.UpdateMasses();
    this.handled.UpdateMasses();
    this.ragdolled.UpdateMasses();
  }

  public static float GetTotalMass<T>(List<T> bodies, Func<T, float> getPartMass)
  {
    float totalMass = 0.0f;
    foreach (T body in bodies)
      totalMass += getPartMass(body);
    return totalMass;
  }

  public static void ScaleMass<T>(
    List<T> bodies,
    float scale,
    Func<T, float> getPartMass,
    Action<T, float> setPartMass)
  {
    foreach (T body in bodies)
      setPartMass(body, getPartMass(body) * scale);
  }

  [Serializable]
  public abstract class MassScalar<T>
  {
    [Delayed]
    public float totalMass = -1f;
    [HideInInspector]
    [NonSerialized]
    protected float lastTotalMass = -1f;
    protected List<T> bodies;
    protected bool blockValidate;

    public void PopulateBodies(Transform ragdoll)
    {
      this.bodies = new List<T>();
      foreach (RagdollPart componentsInChild in ragdoll.GetComponentsInChildren<RagdollPart>())
        this.bodies.Add(this.GetBodyFromPart(componentsInChild));
    }

    public abstract T GetBodyFromPart(RagdollPart part);

    public abstract void SetMass(T t, float mass);

    public abstract float GetMass(T t);

    public abstract void DefaultMassesFallback();

    public void OnValidate(Transform ragdoll)
    {
      if (this.blockValidate)
        return;
      this.blockValidate = true;
      this.PopulateBodies(ragdoll);
      if (Application.isPlaying)
        return;
      if ((double) this.totalMass < 0.0)
      {
        this.totalMass = RagdollMassScalar.GetTotalMass<T>(this.bodies, new Func<T, float>(this.GetMass));
        this.lastTotalMass = this.totalMass;
      }
      if ((double) this.lastTotalMass < 0.0)
        this.lastTotalMass = this.totalMass;
      if (!this.totalMass.IsApproximately(this.lastTotalMass))
      {
        RagdollMassScalar.ScaleMass<T>(this.bodies, this.totalMass / this.lastTotalMass, new Func<T, float>(this.GetMass), new Action<T, float>(this.SetMass));
        this.lastTotalMass = this.totalMass;
      }
      this.blockValidate = false;
    }

    public void ChildrenChanged(Transform ragdoll) => this.PopulateBodies(ragdoll);

    public void UpdateMasses()
    {
      this.blockValidate = true;
      float totalMass = RagdollMassScalar.GetTotalMass<T>(this.bodies, new Func<T, float>(this.GetMass));
      if (!totalMass.IsApproximately(this.totalMass))
        this.totalMass = totalMass;
      if ((double) this.totalMass < 0.0)
        this.DefaultMassesFallback();
      this.blockValidate = false;
    }
  }

  [Serializable]
  public class PhysicBodyScalar : RagdollMassScalar.MassScalar<PhysicBody>
  {
    public override PhysicBody GetBodyFromPart(RagdollPart part) => part.GetPhysicBody();

    public override float GetMass(PhysicBody t) => t.mass;

    public override void SetMass(PhysicBody t, float mass) => t.mass = mass;

    public override void DefaultMassesFallback()
    {
    }
  }

  [Serializable]
  public class HandledScalar : RagdollMassScalar.MassScalar<RagdollPart>
  {
    public override RagdollPart GetBodyFromPart(RagdollPart part) => part;

    public override float GetMass(RagdollPart t) => t.handledMass;

    public override void SetMass(RagdollPart t, float mass) => t.handledMass = mass;

    public override void DefaultMassesFallback()
    {
      this.blockValidate = true;
      this.totalMass = 0.0f;
      foreach (RagdollPart body in this.bodies)
      {
        body.handledMass = body.GetPhysicBody().mass;
        this.totalMass += body.GetPhysicBody().mass;
      }
      this.blockValidate = false;
    }
  }

  [Serializable]
  public class RagdolledScalar : RagdollMassScalar.MassScalar<RagdollPart>
  {
    public override RagdollPart GetBodyFromPart(RagdollPart part) => part;

    public override float GetMass(RagdollPart t) => t.ragdolledMass;

    public override void SetMass(RagdollPart t, float mass) => t.ragdolledMass = mass;

    public override void DefaultMassesFallback()
    {
    }
  }
}
