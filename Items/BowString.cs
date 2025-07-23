// Decompiled with JetBrains decompiler
// Type: ThunderRoad.BowString
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/BowString.html")]
public class BowString : ThunderBehaviour
{
  [Header("Draw and animation")]
  public Animation animation;
  [Tooltip("This allows you to adjust the animation time so that the pink line matches where your bow is drawn to better.")]
  public AnimationCurve pullCurve = AnimationCurve.Linear(0.0f, 0.0f, 1f, 1f);
  [Tooltip("Defines how far your bow string can be pulled (in meters). This gets set automatically by the auto-configure, but can be manually adjusted if you feel it's wrong.")]
  public float stringDrawLength = 0.5f;
  [Tooltip("Set the minimum speed for the bow to fire an arrow.")]
  public float minFireVelocity = 4f;
  [Range(0.0f, 0.1f)]
  [Tooltip("Defines the minimum distance the handle has to move to register a pull happening.")]
  public float minPull = 0.01f;
  [Tooltip("As the pull difficulty increases, the player's hand will become weaker. Allows you to make it \"tougher\" to achieve full draw.")]
  public AnimationCurve pullDifficultyByDraw = new AnimationCurve(new Keyframe[2]
  {
    new Keyframe(0.0f, 0.0f),
    new Keyframe(1f, 1f)
  });
  [Header("Resting and nocking")]
  public Transform restLeft;
  public Transform restRight;
  public string ammoCategory = "Arrow";
  [Tooltip("Allow the player to always grab the string, even if the bow itself isn't grabbed. Defaults to false.")]
  public bool stringAlwaysGrabbable;
  [Tooltip("Sets whether or not arrows can be nocked when holding the non-main handle. Defaults to true.")]
  public bool nockOnlyMainHandle = true;
  [Tooltip("Defines whether or not to drop the arrow when the bow is ungrabbed. If set to false, the bow can hold an arrow even when not held.")]
  public bool loseNockOnUngrab = true;
  [Tooltip("If true, allows arrows to drop out of the bow. If false, prevents arrows from falling out of the bow.")]
  public bool allowOverdraw = true;
  [Tooltip("Plays when the bow is released and the arrow gets fired.")]
  [Header("Audio")]
  public AudioContainer audioContainerShoot;
  [Tooltip("Plays on a loop while the player is pulling the string back.")]
  public AudioContainer audioContainerDraw;
  [Tooltip("Plays on loop while the string is moving (Either being pulled back, or snapping forward when released)")]
  public AudioClip audioClipString;
  protected bool setupFinished;
  protected float currentTargetRatio;
  private string clipName;
  private bool _blockStringRelease;
  [NonSerialized]
  public Transform currentRest;
  protected AudioSource audioSourceString;
  protected AudioSource audioSourceShoot;
  protected UnityEngine.Vector3 stringCorrectionVelocity;
  protected UnityEngine.Vector3 maxArrowVelocity;
  protected UnityEngine.Vector3? lastStringLocal;
  protected UnityEngine.Vector3? stringReleaseLocal;
  protected UnityEngine.Vector3? arrowUnnockForward;
  protected float arrowUnnockZ;
  protected UnityEngine.Vector3 arrowShootDirection;
  protected float previousPull;
  protected float stepPull;
  protected bool drawAudioPlayed;
  protected bool checkJointsOnNull;

  public Item item { get; protected set; }

  public PhysicBody pb { get; protected set; }

  public ConfigurableJoint stringJoint { get; protected set; }

  public Handle stringHandle { get; protected set; }

  public UnityEngine.Vector3 orgBowStringPos { get; protected set; }

  public ItemModuleBow module { get; protected set; }

  private void TryAssignReferences()
  {
    if ((UnityEngine.Object) this.item == (UnityEngine.Object) null)
      this.item = this.GetComponentInParent<Item>();
    if (this.pb == (PhysicBody) null)
      this.pb = this.gameObject.GetPhysicBody();
    if (this.pb == (PhysicBody) null)
      this.pb = this.gameObject.AddComponent<Rigidbody>().AsPhysicBody();
    if ((UnityEngine.Object) this.stringJoint == (UnityEngine.Object) null)
      this.stringJoint = this.GetComponent<ConfigurableJoint>();
    if ((UnityEngine.Object) this.stringJoint == (UnityEngine.Object) null)
      this.stringJoint = this.gameObject.AddComponent<ConfigurableJoint>();
    if ((UnityEngine.Object) this.stringHandle == (UnityEngine.Object) null)
      this.stringHandle = this.GetComponent<Handle>() ?? this.GetComponentInChildren<Handle>();
    if (!((UnityEngine.Object) this.stringHandle == (UnityEngine.Object) null))
      return;
    Debug.LogError((object) $"Could not assign Handle reference! Make sure that this BowString component ({this.gameObject.name}) has a Handle on it, or has a Handle as a child object!");
  }

  private void JointSetup(bool init, float allowance = 0.0f)
  {
    if (init && (!this.setupFinished || !Application.isPlaying))
    {
      this.SetStringTargetRatio(0.0f);
      this.orgBowStringPos = this.pb.transform.localPosition;
      ItemModuleBow module = this.module;
      this.SetStringSpring(module != null ? module.stringSpring : 500f);
    }
    this.stringJoint.SetConnectedPhysicBody(this.item.gameObject.GetPhysicBody());
    this.stringJoint.autoConfigureConnectedAnchor = false;
    this.stringJoint.configuredInWorldSpace = false;
    this.stringJoint.anchor = UnityEngine.Vector3.zero;
    this.stringJoint.linearLimit = new SoftJointLimit()
    {
      limit = (float) (0.5 * ((double) this.stringDrawLength + (double) allowance)),
      contactDistance = 0.01f
    };
    this.stringJoint.xMotion = ConfigurableJointMotion.Locked;
    this.stringJoint.yMotion = ConfigurableJointMotion.Locked;
    this.stringJoint.zMotion = ConfigurableJointMotion.Limited;
    this.stringJoint.angularXMotion = ConfigurableJointMotion.Locked;
    this.stringJoint.angularYMotion = ConfigurableJointMotion.Locked;
    this.stringJoint.angularZMotion = ConfigurableJointMotion.Locked;
    this.stringJoint.connectedAnchor = this.orgBowStringPos - new UnityEngine.Vector3(0.0f, 0.0f, (float) (0.5 * (double) this.stringDrawLength + 0.5 * (double) allowance));
    this.setupFinished = true;
  }

  public void SetStringTargetRatio(float targetRatio)
  {
    this.currentTargetRatio = targetRatio;
    this.stringJoint.targetPosition = new UnityEngine.Vector3(0.0f, 0.0f, -0.5f * this.stringDrawLength) + new UnityEngine.Vector3(0.0f, 0.0f, targetRatio * this.stringDrawLength);
  }

  public void SetStringSpring(float spring)
  {
    this.stringJoint.zDrive = this.stringJoint.zDrive with
    {
      positionSpring = spring
    };
  }

  public bool blockStringRelease
  {
    get
    {
      int num = this._blockStringRelease ? 1 : 0;
      this._blockStringRelease = false;
      return num != 0;
    }
    set => this._blockStringRelease = value;
  }

  public void SetMinFireVelocity(float fireVelocity) => this.minFireVelocity = fireVelocity;

  public void ReleaseString() => this.TryReleaseString();

  public bool TryReleaseString()
  {
    if (this.stringHandle.handlers.Count > 0)
      return false;
    if ((double) this.currentTargetRatio > 0.0)
      this.SetStringTargetRatio(0.0f);
    this.SetLimitBounciness(0.33f);
    this.releasePullRatio = this.currentPullRatio;
    if ((bool) (UnityEngine.Object) this.loadedArrow)
    {
      this.SetBowHandleDrives((double) this.currentPullRatio > 0.0 ? UnityEngine.Vector2.zero : UnityEngine.Vector2.one);
      this.ResetStringHandleDrive();
      if ((UnityEngine.Object) this.audioContainerShoot != (UnityEngine.Object) null)
        this.audioSourceShoot.PlayOneShot(this.audioContainerShoot.PickAudioClip(), Mathf.InverseLerp(this.module.audioShootMinPull, 1f, this.currentPullRatio));
      this.maxArrowVelocity = UnityEngine.Vector3.zero;
      this.lastStringLocal = new UnityEngine.Vector3?(this.item.transform.InverseTransformPoint(this.transform.position));
      this.stringReleaseLocal = new UnityEngine.Vector3?(this.item.transform.InverseTransformPoint(this.transform.position));
      this.arrowShootDirection = this.loadedArrow.transform.forward;
      this.loadedArrow.mainCollisionHandler.OnCollisionStartEvent += new CollisionHandler.CollisionEvent(this.ArrowHitObject);
    }
    BowString.StringReleaseDelegate onStringReleased = this.onStringReleased;
    if (onStringReleased != null)
      onStringReleased(this.currentPullRatio, this.loadedArrow);
    return true;
  }

  public void SpawnAndAttachArrow(string arrowID)
  {
    if ((UnityEngine.Object) this.loadedArrow != (UnityEngine.Object) null)
      return;
    Catalog.GetData<ItemData>(arrowID)?.SpawnAsync((Action<Item>) (projectile => this.NockArrow(projectile.GetMainHandle(Side.Right), ignoreHandling: true)));
  }

  public void RemoveArrow(bool despawn)
  {
    if ((UnityEngine.Object) this.loadedArrow == (UnityEngine.Object) null)
      return;
    Item loadedArrow = this.loadedArrow;
    this.DetachArrow(BowString.DetachType.Both);
    if (!despawn)
      return;
    loadedArrow.Despawn();
  }

  public bool isPulling { get; protected set; }

  public float pullDistance { get; protected set; }

  public float currentPullRatio { get; protected set; }

  public float releasePullRatio { get; protected set; }

  public Item loadedArrow { get; protected set; }

  public BowString.ArrowState arrowState { get; protected set; }

  public ConfigurableJoint nockJoint { get; protected set; }

  public ConfigurableJoint restJoint { get; protected set; }

  public event BowString.GrabNockEvent onArrowAdded;

  public event BowString.GrabNockEvent onStringGrabbed;

  public event BowString.GrabNockEvent onStringUngrabbed;

  public event BowString.StringReleaseDelegate onStringReleased;

  public event BowString.StringReleaseDelegate onStringSnap;

  public event BowString.UnnockingEvent onArrowRemoved;

  protected void Awake()
  {
    if ((bool) (UnityEngine.Object) this.animation && (bool) (UnityEngine.Object) this.animation.clip)
      this.clipName = this.animation.clip.name;
    this.SetNormalizedTime(0.0f);
    this.TryAssignReferences();
    this.item.OnDataLoaded += new Item.LoadDelegate(this.OnItemDataLoaded);
    this.item.mainHandleLeft.Grabbed += new Handle.GrabEvent(this.OnBowGrabbed);
    this.item.mainHandleRight.Grabbed += new Handle.GrabEvent(this.OnBowGrabbed);
    this.item.mainHandleLeft.UnGrabbed += new Handle.GrabEvent(this.OnBowUngrabbed);
    this.item.mainHandleRight.UnGrabbed += new Handle.GrabEvent(this.OnBowUngrabbed);
    this.item.OnSnapEvent += new Item.HolderDelegate(this.OnBowSnapped);
    this.stringHandle.Grabbed += new Handle.GrabEvent(this.OnStringGrab);
    this.stringHandle.UnGrabbed += new Handle.GrabEvent(this.OnStringUnGrab);
    this.pb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    this.orgBowStringPos = this.pb.transform.localPosition;
    this.audioSourceString = this.AddAndConfigureAudioSource();
    if ((UnityEngine.Object) this.audioClipString != (UnityEngine.Object) null)
      this.audioSourceString.clip = this.audioClipString;
    this.audioSourceShoot = this.AddAndConfigureAudioSource();
  }

  private void Start() => this.JointSetup(true);

  private AudioSource AddAndConfigureAudioSource()
  {
    AudioSource audioSource = this.gameObject.AddComponent<AudioSource>();
    audioSource.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Effect);
    audioSource.playOnAwake = false;
    if (AudioSettings.GetSpatializerPluginName() != null)
      audioSource.spatialize = true;
    audioSource.spatialBlend = 1f;
    audioSource.dopplerLevel = 0.0f;
    return audioSource;
  }

  private void OnItemDataLoaded()
  {
    this.module = this.item.data.GetModule<ItemModuleBow>();
    this.ResetStringSpring();
    if (this.stringAlwaysGrabbable)
      return;
    this.stringHandle.SetTouchPersistent(false);
  }

  private void OnStringGrab(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    if (!(bool) (UnityEngine.Object) this.loadedArrow && this.module.spawnArrow)
      this.SpawnAndAttachArrow(this.module.arrowProjectileID);
    this.SetLimitBounciness(0.0f);
    this.SetStringSpring(this.module.stringSpring);
    this.loadedArrow?.IgnoreRagdollCollision(ragdollHand.ragdoll);
    BowString.GrabNockEvent onStringGrabbed = this.onStringGrabbed;
    if (onStringGrabbed == null)
      return;
    onStringGrabbed(this.loadedArrow);
  }

  private void OnStringUnGrab(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    BowString.GrabNockEvent onStringUngrabbed = this.onStringUngrabbed;
    if (onStringUngrabbed != null)
      onStringUngrabbed(this.loadedArrow);
    int num = (double) this.currentPullRatio <= (double) this.minPull ? 0 : (!this.blockStringRelease ? 1 : 0);
    if ((bool) (UnityEngine.Object) this.loadedArrow)
    {
      this.loadedArrow.lastHandler = ragdollHand;
      if (this.arrowState == BowString.ArrowState.Nocked)
        this.DetachArrow(BowString.DetachType.Unnock);
    }
    if (num != 0 && this.TryReleaseString())
      EventManager.InvokeBowRelease(ragdollHand.creature, this);
    if (num != 0 && !((UnityEngine.Object) this.loadedArrow == (UnityEngine.Object) null))
      return;
    this.ResetBowHandleDrives();
  }

  private void OnTriggerEnter(Collider collider)
  {
    Handle component;
    if ((bool) (UnityEngine.Object) this.loadedArrow || !((UnityEngine.Object) Player.currentCreature.handRight.grabbedHandle?.item == (UnityEngine.Object) this.item) && !((UnityEngine.Object) Player.currentCreature.handLeft.grabbedHandle?.item == (UnityEngine.Object) this.item) || !collider.TryGetComponent<Handle>(out component) || component.handlers.Count <= 0 || !(bool) (UnityEngine.Object) component.item || !(component.item.data.slot == this.ammoCategory) || !((UnityEngine.Object) Player.currentCreature.handRight.grabbedHandle?.item == (UnityEngine.Object) component.item) && !((UnityEngine.Object) Player.currentCreature.handLeft.grabbedHandle?.item == (UnityEngine.Object) component.item))
      return;
    Handle arrowHandle = component;
    if ((UnityEngine.Object) component != (UnityEngine.Object) component.item.mainHandleLeft && (UnityEngine.Object) component != (UnityEngine.Object) component.item.mainHandleRight)
      arrowHandle = component.item.GetMainHandle(component.handlers[0].side);
    this.NockArrow(arrowHandle);
  }

  public void ResetStringSpring() => this.SetStringSpring(this.module.stringSpring);

  public void SetLimitBounciness(float bounciness)
  {
    this.stringJoint.linearLimit = this.stringJoint.linearLimit with
    {
      bounciness = bounciness
    };
  }

  public void SetJointMotion(ConfigurableJointMotion configurableJointMotion)
  {
    this.stringJoint.xMotion = configurableJointMotion;
    this.stringJoint.yMotion = configurableJointMotion;
    this.stringJoint.angularXMotion = configurableJointMotion;
    this.stringJoint.angularYMotion = configurableJointMotion;
    this.stringJoint.angularZMotion = configurableJointMotion;
  }

  protected void SetBowHandleDrives(UnityEngine.Vector2 rotationStrength, UnityEngine.Vector2? positionStrength = null)
  {
    if (!positionStrength.HasValue)
      positionStrength = new UnityEngine.Vector2?(UnityEngine.Vector2.one);
    foreach (Handle handle in this.item.handles)
      handle.SetJointDrive(positionStrength.Value, rotationStrength);
  }

  protected void ResetBowHandleDrives()
  {
    foreach (Handle handle in this.item.handles)
      handle.RefreshJointDrive();
  }

  protected void SetStringHandleDrive(UnityEngine.Vector2 positionStrength)
  {
    this.stringHandle.SetJointDrive(positionStrength, UnityEngine.Vector2.zero);
  }

  protected void ResetStringHandleDrive() => this.stringHandle.RefreshJointDrive();

  public override ManagedLoops EnabledManagedLoops
  {
    get => ManagedLoops.FixedUpdate | ManagedLoops.Update;
  }

  protected internal override void ManagedUpdate()
  {
    if (!(bool) (UnityEngine.Object) this.stringHandle || (bool) (UnityEngine.Object) this.item.holder)
      return;
    float num = this.orgBowStringPos.z - this.transform.localPosition.z;
    if ((double) num > (double) this.minPull)
    {
      this.currentPullRatio = num / this.stringDrawLength;
      this.SetNormalizedTime(this.currentPullRatio);
      if (this.stringHandle.handlers.Count <= 0 && (double) this.releasePullRatio <= (double) this.minPull)
        return;
      if ((double) Mathf.Abs(this.currentPullRatio - this.stepPull) > (double) Catalog.gameData.haptics.bowDrawPeriod)
      {
        float currentPullRatio = this.currentPullRatio;
        this.audioSourceString.volume = 0.1f + currentPullRatio;
        this.audioSourceString.pitch = (float) (1.0 + (double) currentPullRatio / 5.0);
        if (!this.audioSourceString.isPlaying && (UnityEngine.Object) this.audioClipString != (UnityEngine.Object) null)
          this.audioSourceString.Play();
        if (this.stringHandle.handlers.Count > 0 && (bool) (UnityEngine.Object) this.stringHandle.handlers[0].playerHand)
          PlayerControl.GetHand(this.stringHandle.handlers[0].playerHand.side).HapticShort(Catalog.gameData.haptics.bowDrawIntensity);
        if (this.item.mainHandleRight.handlers.Count > 0 && (bool) (UnityEngine.Object) this.item.mainHandleRight.handlers[0].playerHand)
          PlayerControl.handRight.HapticShort(Catalog.gameData.haptics.bowDrawIntensity * 0.5f);
        else if (this.item.mainHandleLeft.handlers.Count > 0 && (bool) (UnityEngine.Object) this.item.mainHandleLeft.handlers[0].playerHand)
          PlayerControl.handLeft.HapticShort(Catalog.gameData.haptics.bowDrawIntensity * 0.5f);
        if ((UnityEngine.Object) this.audioClipString != (UnityEngine.Object) null)
          this.audioSourceString.PlayOneShot(this.audioSourceString.clip);
        this.stepPull = this.currentPullRatio;
      }
      if ((bool) (UnityEngine.Object) this.loadedArrow && this.arrowState == (BowString.ArrowState.Nocked | BowString.ArrowState.Rested) && !this.drawAudioPlayed && (double) this.currentPullRatio - (double) this.previousPull > (double) this.module.audioDrawPullSpeed)
      {
        if ((UnityEngine.Object) this.audioContainerDraw != (UnityEngine.Object) null)
          this.audioSourceShoot.PlayOneShot(this.audioContainerDraw.PickAudioClip(), 1f);
        this.drawAudioPlayed = true;
      }
      this.previousPull = this.currentPullRatio;
      if (this.isPulling)
        return;
      this.OnPullStart();
      this.isPulling = true;
    }
    else
    {
      this.drawAudioPlayed = false;
      this.currentPullRatio = 0.0f;
      this.stepPull = 0.0f;
      this.SetNormalizedTime(0.0f);
      if (this.isPulling)
      {
        this.OnPullEnd();
        this.isPulling = false;
      }
      this.releasePullRatio = 0.0f;
    }
  }

  protected void FixStringBouncy(float deltaTime)
  {
    if (!((UnityEngine.Object) Player.currentCreature != (UnityEngine.Object) null))
      return;
    this.stringCorrectionVelocity = UnityEngine.Vector3.zero;
    if (((UnityEngine.Object) Player.currentCreature.GetHand(Side.Right)?.grabbedHandle == (UnityEngine.Object) this.stringHandle || (UnityEngine.Object) Player.currentCreature.GetHand(Side.Left)?.grabbedHandle == (UnityEngine.Object) this.stringHandle) && Player.currentCreature.currentLocomotion.isGrounded)
      this.stringCorrectionVelocity = this.transform.forward * UnityEngine.Vector3.Dot(this.transform.forward, Player.currentCreature.currentLocomotion.physicBody.velocity);
    this.transform.position += this.stringCorrectionVelocity * deltaTime;
  }

  protected internal override void ManagedFixedUpdate()
  {
    if (((UnityEngine.Object) this.loadedArrow == (UnityEngine.Object) null || (UnityEngine.Object) this.loadedArrow != (UnityEngine.Object) null && (UnityEngine.Object) this.restJoint == (UnityEngine.Object) null && (UnityEngine.Object) this.nockJoint == (UnityEngine.Object) null) && this.checkJointsOnNull)
    {
      bool flag = false;
      ConfigurableJoint[] components = this.GetComponents<ConfigurableJoint>();
      if (components.Length > 1)
      {
        for (int index1 = components.Length - 1; index1 >= 0; --index1)
        {
          if ((UnityEngine.Object) components[index1] != (UnityEngine.Object) this.stringJoint)
          {
            for (int index2 = components[index1].GetConnectedPhysicBody().gameObject.GetComponents<ConfigurableJoint>().Length - 1; index2 >= 0; --index2)
            {
              PhysicBody connectedPhysicBody = components[index1].GetConnectedPhysicBody();
              if (connectedPhysicBody == this.pb || connectedPhysicBody == this.item.physicBody)
                UnityEngine.Object.Destroy((UnityEngine.Object) components[index2]);
            }
            UnityEngine.Object.Destroy((UnityEngine.Object) components[index1]);
            flag = true;
          }
        }
      }
      this.arrowState = BowString.ArrowState.None;
      if ((bool) (UnityEngine.Object) this.loadedArrow)
      {
        this.loadedArrow.OnGrabEvent -= new Item.GrabDelegate(this.OnNockedArrowGrabbed);
        this.loadedArrow.OnTelekinesisGrabEvent -= new Item.TelekinesisDelegate(this.OnNockedArrowTeleGrabbed);
        this.loadedArrow.OnDespawnEvent -= new Item.SpawnEvent(this.OnNockedArrowDespawn);
      }
      this.SetBowHandleDrives((double) this.currentPullRatio > 0.0 ? UnityEngine.Vector2.zero : UnityEngine.Vector2.one);
      this.loadedArrow = (Item) null;
      this.checkJointsOnNull = false;
      if (flag)
        Debug.LogWarning((object) $"Bowstring on bow [ {this.item.name} ] had an error with detaching arrow; The joints have been removed, loaded arrow cleared, and string reset");
    }
    if ((UnityEngine.Object) Player.currentCreature != (UnityEngine.Object) null && ((UnityEngine.Object) Player.local?.GetHand(Side.Right)?.ragdollHand?.grabbedHandle == (UnityEngine.Object) this.stringHandle || (UnityEngine.Object) Player.local?.GetHand(Side.Left)?.ragdollHand?.grabbedHandle == (UnityEngine.Object) this.stringHandle))
      this.SetStringHandleDrive(new UnityEngine.Vector2(Mathf.Clamp(1f - this.pullDifficultyByDraw.Evaluate(this.currentPullRatio), 1E-05f, 1f), 1f));
    bool hasValue = this.lastStringLocal.HasValue;
    if (this.stringHandle.handlers.Count == 0)
    {
      if (!hasValue)
        this.ResetBowHandleDrives();
      else
        this.SetBowHandleDrives(UnityEngine.Vector2.zero, new UnityEngine.Vector2?((1f - this.currentPullRatio) * UnityEngine.Vector2.one));
    }
    this.FixStringBouncy(Time.fixedDeltaTime);
    if (!(bool) (UnityEngine.Object) this.loadedArrow)
      return;
    if (this.loadedArrow.isPenetrating)
    {
      this.DetachArrow(BowString.DetachType.Both);
    }
    else
    {
      if (this.stringHandle.handlers.Count == 0 & hasValue)
      {
        this.maxArrowVelocity = (double) this.loadedArrow.physicBody.velocity.sqrMagnitude >= (double) this.maxArrowVelocity.sqrMagnitude ? this.loadedArrow.physicBody.velocity : this.maxArrowVelocity;
        if ((double) this.currentPullRatio > (double) this.minPull)
        {
          this.item.transform.RotateAroundPivot(this.currentRest.position, Quaternion.FromToRotation(this.GetCurrentStringRestDirection(), this.GetPreviousStringRestDirection()));
          this.lastStringLocal = new UnityEngine.Vector3?(this.item.transform.InverseTransformPoint(this.transform.position));
        }
      }
      if (this.arrowState == BowString.ArrowState.Rested)
      {
        float toArrowTransform = this.GetZFromRestToArrowTransform(this.loadedArrow.mainHandleRight.transform);
        if (this.arrowUnnockForward.HasValue)
          this.loadedArrow.transform.RotateAroundPivot(this.currentRest.position, Quaternion.FromToRotation(this.loadedArrow.transform.forward, UnityEngine.Vector3.Slerp(this.arrowShootDirection, this.arrowUnnockForward.Value, toArrowTransform / this.arrowUnnockZ)));
        if ((double) toArrowTransform < 0.0)
          return;
        this.DetachArrow(BowString.DetachType.Fire);
      }
      else
      {
        if ((double) this.GetZFromRestToArrowTransform(this.loadedArrow.GetDefaultHolderPoint().anchor) > 0.0 || !((UnityEngine.Object) this.restJoint != (UnityEngine.Object) null) || !this.allowOverdraw)
          return;
        this.DetachArrow(BowString.DetachType.Unrest);
      }
    }
  }

  protected UnityEngine.Vector3 GetCurrentStringRestDirection()
  {
    return (this.currentRest.position - this.transform.position).normalized;
  }

  protected UnityEngine.Vector3 GetPreviousStringRestDirection()
  {
    return this.lastStringLocal.HasValue ? (this.currentRest.position - this.item.transform.TransformPoint(this.lastStringLocal.Value)).normalized : this.loadedArrow.flyDirRef.forward;
  }

  public float GetZFromRestToArrowTransform(Transform transform)
  {
    return this.loadedArrow.transform.InverseTransformPoint(transform.position).z - this.loadedArrow.transform.InverseTransformPoint(this.currentRest.position).z;
  }

  public void SpawnAndAttachArrow(string arrowID, Side? forceSide = null)
  {
    if ((UnityEngine.Object) this.loadedArrow != (UnityEngine.Object) null)
      return;
    Catalog.GetData<ItemData>(arrowID)?.SpawnAsync((Action<Item>) (projectile => this.NockArrow(projectile.GetMainHandle(Side.Right), ignoreHandling: true, forceSide: forceSide)));
  }

  public void NockArrow(
    Handle arrowHandle,
    HandlePose handleOrientation = null,
    bool ignoreHandling = false,
    Side? forceSide = null)
  {
    if ((UnityEngine.Object) arrowHandle != (UnityEngine.Object) arrowHandle.item.mainHandleRight && (UnityEngine.Object) arrowHandle != (UnityEngine.Object) arrowHandle.item.mainHandleLeft || !ignoreHandling && (!arrowHandle.item.IsHanded() || this.nockOnlyMainHandle && !arrowHandle.IsHanded()))
      return;
    arrowHandle.SetTouch(false);
    RagdollHand ragdollHand1 = this.item.IsHanded() ? this.item.mainHandler : (RagdollHand) null;
    if (this.item.mainHandleRight.IsHanded())
      ragdollHand1 = this.item.mainHandleRight.handlers[0];
    else if (this.item.mainHandleLeft.IsHanded())
      ragdollHand1 = this.item.mainHandleLeft.handlers[0];
    this.currentRest = (double) this.restRight.position.y > (double) this.restLeft.position.y ? this.restRight : this.restLeft;
    if ((UnityEngine.Object) ragdollHand1 != (UnityEngine.Object) null)
    {
      this.currentRest = ragdollHand1.side == Side.Right ? this.restRight : this.restLeft;
      if ((double) UnityEngine.Vector3.Dot(ragdollHand1.gripInfo.orientation.transform.up, ragdollHand1.grabbedHandle.transform.up) < 0.0)
        this.currentRest = ragdollHand1.side == Side.Right ? this.restLeft : this.restRight;
    }
    if (forceSide.HasValue)
    {
      Side? nullable = forceSide;
      Side side = Side.Right;
      this.currentRest = nullable.GetValueOrDefault() == side & nullable.HasValue ? this.restRight : this.restLeft;
    }
    this.loadedArrow = arrowHandle.item;
    this.loadedArrow.transform.MoveAlign(arrowHandle.transform, this.transform);
    this.loadedArrow.transform.rotation = Quaternion.LookRotation((this.currentRest.position - this.transform.position).normalized, this.transform.up) * Quaternion.FromToRotation(this.loadedArrow.transform.forward, arrowHandle.transform.forward);
    this.loadedArrow.DisallowDespawn = true;
    this.loadedArrow.OnGrabEvent += new Item.GrabDelegate(this.OnNockedArrowGrabbed);
    this.loadedArrow.OnTelekinesisGrabEvent += new Item.TelekinesisDelegate(this.OnNockedArrowTeleGrabbed);
    this.nockJoint = this.pb.gameObject.AddComponent<ConfigurableJoint>();
    this.nockJoint.configuredInWorldSpace = false;
    this.nockJoint.autoConfigureConnectedAnchor = false;
    this.nockJoint.SetConnectedPhysicBody(this.loadedArrow.physicBody);
    this.nockJoint.connectedAnchor = arrowHandle.transform.localPosition;
    this.nockJoint.xMotion = ConfigurableJointMotion.Locked;
    this.nockJoint.yMotion = ConfigurableJointMotion.Locked;
    this.nockJoint.zMotion = ConfigurableJointMotion.Locked;
    this.nockJoint.angularZMotion = ConfigurableJointMotion.Locked;
    this.restJoint = this.loadedArrow.physicBody.gameObject.AddComponent<ConfigurableJoint>();
    this.restJoint.SetConnectedPhysicBody(this.item.physicBody);
    this.restJoint.anchor = UnityEngine.Vector3.zero;
    this.restJoint.autoConfigureConnectedAnchor = false;
    this.restJoint.connectedAnchor = this.currentRest.localPosition;
    this.restJoint.xMotion = ConfigurableJointMotion.Locked;
    this.restJoint.yMotion = ConfigurableJointMotion.Locked;
    RagdollHand ragdollHand2 = (RagdollHand) null;
    if (arrowHandle.handlers.Count > 0)
      ragdollHand2 = arrowHandle.handlers[0];
    bool withTrigger = ragdollHand2 != null && ragdollHand2.grabbedWithTrigger;
    for (int index = this.loadedArrow.handlers.Count - 1; index >= 0; --index)
      this.loadedArrow.handlers[index].UnGrab(false);
    this.loadedArrow.IgnoreObjectCollision(this.item);
    this.loadedArrow.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem));
    this.loadedArrow.physicBody.sleepThreshold = 0.0f;
    if ((UnityEngine.Object) handleOrientation == (UnityEngine.Object) null)
      ragdollHand2?.GrabRelative(this.stringHandle, withTrigger);
    else
      ragdollHand2?.Grab(this.stringHandle, (bool) (UnityEngine.Object) handleOrientation, withTrigger);
    this.arrowState = BowString.ArrowState.Nocked | BowString.ArrowState.Rested;
    this.loadedArrow.OnDespawnEvent += new Item.SpawnEvent(this.OnNockedArrowDespawn);
    BowString.GrabNockEvent onArrowAdded = this.onArrowAdded;
    if (onArrowAdded != null)
      onArrowAdded(this.loadedArrow);
    this.checkJointsOnNull = true;
  }

  public void SetNormalizedTime(float time)
  {
    this.animation[this.clipName].speed = 0.0f;
    this.animation[this.clipName].normalizedTime = this.pullCurve.Evaluate(time);
    this.animation.Play(this.clipName);
  }

  private void OnPullStart()
  {
    if (this.stringHandle.handlers.Count <= 0)
      return;
    EventManager.InvokeBowDraw(this.stringHandle.handlers[0].creature, this);
  }

  private void OnPullEnd()
  {
    if (!this.stringHandle.IsHanded() && (double) this.releasePullRatio > (double) this.minPull)
    {
      if ((bool) (UnityEngine.Object) this.loadedArrow)
      {
        UnityEngine.Vector3 vector3 = this.maxArrowVelocity - this.item.physicBody.velocity;
        if ((double) vector3.sqrMagnitude > (double) this.minFireVelocity * (double) this.minFireVelocity)
        {
          vector3 = this.loadedArrow.physicBody.velocity;
          float ratio = Utils.CalculateRatio(vector3.magnitude, this.minFireVelocity, 5f * this.module.velocityMultiplier, 0.1f, 1f);
          if ((bool) (UnityEngine.Object) this.item.leftPlayerHand)
            PlayerControl.handLeft.HapticPlayClip(Catalog.gameData.haptics.bowShoot, ratio);
          else if ((bool) (UnityEngine.Object) this.item.rightPlayerHand)
            PlayerControl.handRight.HapticPlayClip(Catalog.gameData.haptics.bowShoot, ratio);
          Item loadedArrow = this.loadedArrow;
          this.DetachArrow(BowString.DetachType.Unnock);
          loadedArrow.physicBody.velocity = this.maxArrowVelocity;
          if (this.arrowState == BowString.ArrowState.Rested && (UnityEngine.Object) this.loadedArrow != (UnityEngine.Object) null)
          {
            this.arrowUnnockForward = new UnityEngine.Vector3?(this.loadedArrow.transform.forward);
            this.arrowUnnockZ = this.GetZFromRestToArrowTransform(this.loadedArrow.mainHandleRight.transform);
            goto label_10;
          }
          goto label_10;
        }
      }
      this.lastStringLocal = new UnityEngine.Vector3?();
label_10:
      this.ResetBowHandleDrives();
      BowString.StringReleaseDelegate onStringSnap = this.onStringSnap;
      if (onStringSnap != null)
        onStringSnap(this.releasePullRatio, this.loadedArrow);
    }
    if (this.stringHandle.handlers.Count <= 0)
      return;
    EventManager.InvokeBowRelease(this.stringHandle.handlers[0].creature, this);
  }

  private void OnBowGrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
  {
    if (this.stringAlwaysGrabbable || eventTime != EventTime.OnEnd)
      return;
    this.loadedArrow?.IgnoreRagdollCollision(ragdollHand.ragdoll);
    this.stringHandle.SetTouchPersistent(true);
  }

  private void OnBowUngrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    if (this.loseNockOnUngrab)
      this.DetachArrow(BowString.DetachType.Both);
    if (this.stringAlwaysGrabbable)
      return;
    if ((bool) (UnityEngine.Object) ragdollHand.otherHand.grabbedHandle && (UnityEngine.Object) ragdollHand.otherHand.grabbedHandle == (UnityEngine.Object) this.stringHandle)
      ragdollHand.otherHand.UnGrab(false);
    this.stringHandle.SetTouchPersistent(false);
  }

  private void OnBowSnapped(Holder holder)
  {
    if (this.loseNockOnUngrab)
      this.DetachArrow(BowString.DetachType.Both);
    this.pb.velocity = UnityEngine.Vector3.zero;
    this.pb.angularVelocity = UnityEngine.Vector3.zero;
  }

  private void OnNockedArrowGrabbed(Handle handle, RagdollHand ragdollHand)
  {
    this.DetachArrow(BowString.DetachType.Both);
  }

  private void OnNockedArrowTeleGrabbed(Handle handle, SpellTelekinesis teleGrabber)
  {
    this.DetachArrow(BowString.DetachType.Both);
  }

  private void ArrowHitObject(CollisionInstance collisionInstance)
  {
    PhysicBody physicBody = collisionInstance.targetCollider.GetPhysicBody();
    RagdollPart component;
    if ((object) physicBody != null && (physicBody == this.pb || physicBody == this.item.physicBody || physicBody.gameObject.TryGetComponent<RagdollPart>(out component) && (UnityEngine.Object) component.ragdoll.creature == (UnityEngine.Object) this.item.mainHandler?.creature))
      return;
    this.DetachArrow(BowString.DetachType.Both);
  }

  private void OnNockedArrowDespawn(EventTime eventTime)
  {
    this.DetachArrow(BowString.DetachType.Both);
  }

  public void DetachArrow(BowString.DetachType detachType)
  {
    if ((UnityEngine.Object) this.loadedArrow == (UnityEngine.Object) null)
      return;
    bool fired = detachType == BowString.DetachType.Fire;
    if (fired)
      detachType = BowString.DetachType.Both;
    if ((detachType == BowString.DetachType.Unnock || detachType == BowString.DetachType.Both) && (UnityEngine.Object) this.nockJoint != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.nockJoint);
      this.nockJoint = (ConfigurableJoint) null;
      this.loadedArrow.mainHandleRight.SetTouch(true);
      this.loadedArrow.mainHandleLeft.SetTouch(true);
    }
    if ((detachType == BowString.DetachType.Unrest || detachType == BowString.DetachType.Both) && (UnityEngine.Object) this.restJoint != (UnityEngine.Object) null)
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this.restJoint);
      this.restJoint = (ConfigurableJoint) null;
    }
    this.arrowState = (BowString.ArrowState) (0 | ((UnityEngine.Object) this.restJoint != (UnityEngine.Object) null ? 2 : 0) | ((UnityEngine.Object) this.nockJoint != (UnityEngine.Object) null ? 1 : 0));
    if (fired)
    {
      if (this.arrowState != BowString.ArrowState.None)
      {
        Debug.LogError((object) $"Error on bow {this.item.name}: Attempted to fire arrow but the arrow is still partially connected.");
        return;
      }
      UnityEngine.Vector3 forward = this.loadedArrow.flyDirRef.forward;
      float magnitude = this.maxArrowVelocity.magnitude;
      this.loadedArrow.physicBody.velocity = forward * magnitude;
      this.loadedArrow.physicBody.angularVelocity = UnityEngine.Vector3.zero;
      this.loadedArrow.Throw(this.module.velocityMultiplier, Item.FlyDetection.Forced);
      this.loadedArrow.lastHandler = this.item.lastHandler;
      float ratio = Utils.CalculateRatio(magnitude, this.minFireVelocity, 5f * this.module.velocityMultiplier, 0.1f, 1f);
      if ((bool) (UnityEngine.Object) this.item.leftPlayerHand)
        PlayerControl.handLeft.HapticPlayClip(Catalog.gameData.haptics.bowShoot, ratio);
      else if ((bool) (UnityEngine.Object) this.item.rightPlayerHand)
        PlayerControl.handRight.HapticPlayClip(Catalog.gameData.haptics.bowShoot, ratio);
      EventManager.InvokeBowFire(this.item.mainHandler, this, this.loadedArrow);
    }
    if (this.arrowState != BowString.ArrowState.None)
      return;
    this.lastStringLocal = new UnityEngine.Vector3?();
    this.stringReleaseLocal = new UnityEngine.Vector3?();
    this.arrowUnnockForward = new UnityEngine.Vector3?();
    this.loadedArrow.DisallowDespawn = false;
    this.loadedArrow.physicBody.sleepThreshold = this.loadedArrow.orgSleepThreshold;
    this.loadedArrow.mainCollisionHandler.OnCollisionStartEvent -= new CollisionHandler.CollisionEvent(this.ArrowHitObject);
    this.loadedArrow.OnGrabEvent -= new Item.GrabDelegate(this.OnNockedArrowGrabbed);
    this.loadedArrow.OnTelekinesisGrabEvent -= new Item.TelekinesisDelegate(this.OnNockedArrowTeleGrabbed);
    this.ResetBowHandleDrives();
    this.loadedArrow.OnDespawnEvent -= new Item.SpawnEvent(this.OnNockedArrowDespawn);
    BowString.UnnockingEvent onArrowRemoved = this.onArrowRemoved;
    if (onArrowRemoved != null)
      onArrowRemoved(this.loadedArrow, fired);
    this.loadedArrow = (Item) null;
  }

  protected override void ManagedOnDisable()
  {
    if (GameManager.isQuitting)
      return;
    this.DetachArrow(BowString.DetachType.Both);
    this.transform.localPosition = this.orgBowStringPos;
    this.pb.velocity = UnityEngine.Vector3.zero;
    this.pb.angularVelocity = UnityEngine.Vector3.zero;
  }

  [Flags]
  public enum ArrowState
  {
    None = 0,
    Nocked = 1,
    Rested = 2,
  }

  public delegate void GrabNockEvent(Item arrow);

  public delegate void StringReleaseDelegate(float pullRatio, Item arrow);

  public delegate void UnnockingEvent(Item arrow, bool fired);

  public enum DetachType
  {
    Unnock,
    Unrest,
    Fire,
    Both,
  }
}
