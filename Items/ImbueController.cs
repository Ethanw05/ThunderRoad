// Decompiled with JetBrains decompiler
// Type: ThunderRoad.ImbueController
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Items/Imbue Controller")]
[DisallowMultipleComponent]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ImbueController.html")]
public class ImbueController : ThunderBehaviour
{
  public ColliderGroup imbueGroup;
  [Range(-100f, 100f)]
  public float imbueRate;
  [Range(0.0f, 100f)]
  public float imbueMaxPercent = 50f;
  public string imbueSpellId;

  public SpellCastCharge imbueSpell { get; protected set; }

  public SpellCaster.ImbueObject imbueObject { get; protected set; }

  private void OnValidate() => this.TryAssignGroup();

  private void TryAssignGroup()
  {
    if (this.imbueGroup != null)
      return;
    this.imbueGroup = this.GetComponent<ColliderGroup>();
  }

  public void SetImbueID(string newID)
  {
    this.imbueSpellId = newID;
    if (this.imbueSpell == null || !(newID != this.imbueSpell.id))
      return;
    this.imbueSpell = (SpellCastCharge) null;
  }

  public void SetImbueRate(float newRate) => this.imbueRate = newRate;

  public void SetImbueMaxPercent(float newMax) => this.imbueMaxPercent = newMax;

  public void ClearImbueID() => this.SetImbueID(string.Empty);

  public void ImbueUseStart() => this.ImbueUse(true);

  public void ImbueUseEnd() => this.ImbueUse(false);

  private void Awake() => this.TryAssignGroup();

  private void Start() => this.SetImbueID(this.imbueSpellId);

  protected void ImbueUse(bool start)
  {
    RagdollHand hand = Player.currentCreature?.GetHand(Side.Right) ?? Creature.all[0].GetHand(Side.Right);
    CollisionHandler collisionHandler1 = this.imbueGroup.collisionHandler;
    if ((collisionHandler1 != null ? (collisionHandler1.isItem ? 1 : 0) : 0) != 0)
      hand = this.imbueGroup.collisionHandler.item.mainHandler ?? this.imbueGroup.collisionHandler.item.lastHandler;
    CollisionHandler collisionHandler2 = this.imbueGroup.collisionHandler;
    if ((collisionHandler2 != null ? (collisionHandler2.isRagdollPart ? 1 : 0) : 0) != 0)
    {
      RagdollPart ragdollPart = this.imbueGroup.collisionHandler.ragdollPart;
      foreach (RagdollHand handler in ragdollPart.ragdoll.handlers)
      {
        if (handler.grabbedHandle is HandleRagdoll grabbedHandle && (Object) grabbedHandle.ragdollPart == (Object) ragdollPart)
        {
          hand = handler;
          break;
        }
      }
      if (hand == null)
        hand = ragdollPart.ragdoll.creature.GetHand(Side.Right) ?? ragdollPart.ragdoll.creature.GetHand(Side.Left);
    }
    if ((Object) hand == (Object) null)
      Debug.LogError((object) "You are activating an ImbueController's ImbueUseStart or ImbueUseEnd method before any creatures exist! This shouldn't happen ever.\nThe method is aborting to prevent further errors.");
    else
      this.imbueObject.colliderGroup.imbue.OnCrystalUse(hand, start);
  }

  public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;

  protected internal override void ManagedUpdate()
  {
    if ((Object) this.imbueGroup == (Object) null)
      return;
    if (this.imbueObject == null || (Object) this.imbueObject.colliderGroup != (Object) this.imbueGroup)
      this.imbueObject = new SpellCaster.ImbueObject(this.imbueGroup);
    if (!string.IsNullOrEmpty(this.imbueSpellId) && (this.imbueSpell == null || this.imbueSpell.id != this.imbueSpellId))
      this.imbueSpell = Catalog.GetData<SpellCastCharge>(this.imbueSpellId);
    if (this.imbueSpell == null && this.imbueObject.colliderGroup.imbue.spellCastBase != null)
      this.imbueSpell = this.imbueObject.colliderGroup.imbue.spellCastBase;
    if (this.imbueSpell == null || (double) this.imbueRate < 0.0 && (double) this.imbueObject.colliderGroup.imbue.energy <= 0.0 || (double) this.imbueRate > 0.0 && (double) this.imbueObject.colliderGroup.imbue.energy >= (double) this.imbueObject.colliderGroup.imbue.maxEnergy * ((double) this.imbueMaxPercent / 100.0) && this.imbueSpellId == this.imbueObject.colliderGroup.imbue.spellCastBase.id)
      return;
    this.imbueObject.colliderGroup.imbue.Transfer(this.imbueSpell, (float) ((double) this.imbueSpell.imbueRate * (double) this.imbueRate * (1.0 / (double) this.imbueObject.colliderGroup.modifier.imbueRate)) * Time.deltaTime);
  }
}
