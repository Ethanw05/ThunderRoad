// Decompiled with JetBrains decompiler
// Type: ThunderRoad.CreatureMouthRelay
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

/// <summary>
/// This script allows events based around a creatures mouth(s) to be triggered and hooked into.
/// </summary>
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/CreatureMouthRelay.html")]
[RequireComponent(typeof (Rigidbody))]
public class CreatureMouthRelay : ThunderBehaviour
{
  [Tooltip("How big is the detection radius?")]
  public float mouthRadius = 0.05f;
  [Tooltip("Can this mouth receive food/liquid?")]
  public bool isMouthActive = true;
  [Tooltip("If enabled this relay will only be active if the current creature is the player.")]
  public bool playerOnly;
  [NonSerialized]
  public CollisionHandler collisionHandler;
  protected float mouthEnterableTime = -1f;
  protected Transform orgTransform;
  protected UnityEngine.Vector3 orgLocalPosition;
  protected Quaternion orgLocalRotation;
  [SerializeField]
  private GameObject liquidReciever;
  private SphereCollider zone;
  /// <summary>The creature this relay is for.</summary>
  public Creature creature;
  /// <summary>
  /// Invoked when a particle collides.
  /// 
  /// This gets invoked from LiquidReciever, here just for the sake of consistency.
  /// </summary>
  public CreatureMouthRelay.OnParticleCollide OnParticleCollideEvent;

  /// <summary>Invoked when an object touches the mouth.</summary>
  public event CreatureMouthRelay.OnObjectTouchMouth OnObjectTouchMouthEvent;

  /// <summary>Invoked when an object touches the mouth.</summary>
  public event CreatureMouthRelay.OnObjectLeaveMouth OnObjectLeaveMouthEvent;

  /// <summary>Invoked when an item touches the mouth.</summary>
  public event CreatureMouthRelay.OnItemTouchMouth OnItemTouchMouthEvent;

  /// <summary>Invoked when an item leaves the mouth.</summary>
  public event CreatureMouthRelay.OnItemLeaveMouth OnItemLeaveMouthEvent;

  /// <summary>
  /// Invoked each frame when the relay updates, its useful for relay hooks like the LiquidReciever to reduce the update call count.
  /// </summary>
  public event CreatureMouthRelay.OnRelayUpdate OnRelayUpdateEvent;

  private void Awake()
  {
    this.collisionHandler = this.GetComponentInParent<CollisionHandler>();
    this.creature = this.GetComponentInParent<Creature>();
    this.zone = this.gameObject.AddComponent<SphereCollider>();
    this.zone.isTrigger = true;
    this.zone.radius = this.mouthRadius;
    foreach (Collider componentsInChild in this.creature.GetComponentsInChildren<Collider>())
      Physics.IgnoreCollision(componentsInChild, (Collider) this.zone, true);
    if ((UnityEngine.Object) this.liquidReciever != (UnityEngine.Object) null)
    {
      this.liquidReciever.AddComponent<SphereCollider>().radius = this.mouthRadius;
      this.liquidReciever.SetLayerRecursively(GameManager.GetLayer(LayerName.LiquidFlow));
    }
    this.orgTransform = this.transform.parent;
    this.orgLocalPosition = this.transform.localPosition;
    this.orgLocalRotation = this.transform.localRotation;
    if (!this.playerOnly || this.creature.isPlayer)
      return;
    this.isMouthActive = false;
  }

  protected override void ManagedOnEnable()
  {
    this.zone.enabled = true;
    this.collisionHandler.enabled = true;
  }

  private void Start()
  {
    if (!((UnityEngine.Object) this.collisionHandler != (UnityEngine.Object) null) || !((UnityEngine.Object) this.collisionHandler.ragdollPart != (UnityEngine.Object) null))
      return;
    this.transform.SetParent(this.creature.transform, true);
    this.creature.ragdoll.OnStateChange += new Ragdoll.StateChange(this.OnRagdollStateChange);
    this.creature.OnDespawnEvent += new Creature.DespawnEvent(this.OnCreatureDespawn);
    this.creature.OnKillEvent += new Creature.KillEvent(this.OnCreatureDied);
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    if (!(bool) (UnityEngine.Object) this.collisionHandler || !this.collisionHandler.isRagdollPart || this.creature.ragdoll.state != Ragdoll.State.NoPhysic && this.creature.ragdoll.state != Ragdoll.State.Kinematic)
      return;
    this.transform.SetPositionAndRotation(this.orgTransform.TransformPoint(this.orgLocalPosition), this.orgTransform.rotation * this.orgLocalRotation);
  }

  /// <summary>Invoked when this creature dies.</summary>
  protected void OnCreatureDied(CollisionInstance collisionInstance, EventTime eventTime)
  {
    this.zone.enabled = false;
    this.liquidReciever?.SetActive(false);
  }

  /// <summary>Invoked when this creature is despawned.</summary>
  protected void OnCreatureDespawn(EventTime eventTime)
  {
    if (eventTime != EventTime.OnStart)
      return;
    this.transform.SetParent(this.orgTransform, true);
    this.transform.localPosition = this.orgLocalPosition;
    this.transform.localRotation = this.orgLocalRotation;
    this.gameObject.SetActive(this.collisionHandler.isRagdollPart && this.collisionHandler.ragdollPart.ragdoll.creature.isPlayer);
  }

  /// <summary>Invoked when the state of this ragdoll changes.</summary>
  protected void OnRagdollStateChange(
    Ragdoll.State previousState,
    Ragdoll.State newState,
    Ragdoll.PhysicStateChange physicStateChange,
    EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    if (newState == Ragdoll.State.NoPhysic || newState == Ragdoll.State.Kinematic)
    {
      if (!((UnityEngine.Object) this.transform.parent != (UnityEngine.Object) this.collisionHandler.ragdollPart.ragdoll.creature.transform))
        return;
      this.transform.SetParent(this.collisionHandler.ragdollPart.ragdoll.creature.transform, true);
      this.transform.position = this.orgTransform.transform.TransformPoint(this.orgLocalPosition);
      this.transform.rotation = this.orgTransform.transform.rotation * this.orgLocalRotation;
      this.gameObject.SetActive(this.collisionHandler.ragdollPart.ragdoll.creature.isPlayer);
    }
    else
    {
      if (!((UnityEngine.Object) this.transform.parent != (UnityEngine.Object) this.orgTransform))
        return;
      this.transform.SetParent(this.orgTransform, true);
      this.transform.localPosition = this.orgLocalPosition;
      this.transform.localRotation = this.orgLocalRotation;
      this.gameObject.SetActive(true);
    }
  }

  public void DisableTemporarily(float disableTime)
  {
    this.mouthEnterableTime = Time.time + disableTime;
  }

  private void OnTriggerEnter(Collider collider)
  {
    if (!this.isMouthActive && (double) this.mouthEnterableTime <= (double) Time.time)
      return;
    CreatureMouthRelay.OnObjectTouchMouth objectTouchMouthEvent = this.OnObjectTouchMouthEvent;
    if (objectTouchMouthEvent != null)
      objectTouchMouthEvent(collider.gameObject);
    Item componentInParent = collider.GetComponentInParent<Item>();
    if (!((UnityEngine.Object) componentInParent != (UnityEngine.Object) null))
      return;
    if (componentInParent.data != null)
      componentInParent.data.GetModule<ItemModuleMouthTouch>()?.OnMouthTouch(componentInParent, this);
    CreatureMouthRelay.OnItemTouchMouth itemTouchMouthEvent = this.OnItemTouchMouthEvent;
    if (itemTouchMouthEvent == null)
      return;
    itemTouchMouthEvent(componentInParent);
  }

  private void OnTriggerExit(Collider collider)
  {
    if (!this.isMouthActive)
      return;
    CreatureMouthRelay.OnObjectLeaveMouth objectLeaveMouthEvent = this.OnObjectLeaveMouthEvent;
    if (objectLeaveMouthEvent != null)
      objectLeaveMouthEvent(collider.gameObject);
    Item componentInParent = collider.GetComponentInParent<Item>();
    if (!((UnityEngine.Object) componentInParent != (UnityEngine.Object) null))
      return;
    CreatureMouthRelay.OnItemLeaveMouth itemLeaveMouthEvent = this.OnItemLeaveMouthEvent;
    if (itemLeaveMouthEvent == null)
      return;
    itemLeaveMouthEvent(componentInParent);
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = UnityEngine.Color.blue;
    Gizmos.DrawWireSphere(this.transform.position, this.mouthRadius);
  }

  public delegate void OnParticleCollide(GameObject other);

  public delegate void OnObjectTouchMouth(GameObject gameObject);

  public delegate void OnObjectLeaveMouth(GameObject gameObject);

  public delegate void OnItemTouchMouth(Item item);

  public delegate void OnItemLeaveMouth(Item item);

  public delegate void OnRelayUpdate();
}
