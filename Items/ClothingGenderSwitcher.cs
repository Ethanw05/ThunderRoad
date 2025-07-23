// Decompiled with JetBrains decompiler
// Type: ThunderRoad.ClothingGenderSwitcher
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

[RequireComponent(typeof (Item))]
[AddComponentMenu("ThunderRoad/Items/Clothing Gender Switcher")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ClothingGenderSwitcher.html")]
public class ClothingGenderSwitcher : MonoBehaviour
{
  [Tooltip("Male wearable item (Select item, not model)")]
  public GameObject maleModel;
  [Tooltip("Main left handle of the male item.")]
  public Handle mainMaleHandleRight;
  [Tooltip("Main right handle of item.")]
  public Handle mainMaleHandleLeft;
  [Space]
  [Tooltip("Female wearable item(Select item, not model")]
  public GameObject femaleModel;
  [Tooltip("Main left handle of the female item.")]
  public Handle mainFemaleHandleRight;
  [Tooltip("Main right handle of the female item.")]
  public Handle mainFemaleHandleLeft;

  public void Refresh()
  {
    if ((bool) (Object) Player.currentCreature)
      this.SetGender(Player.currentCreature.data.gender);
    else
      this.SetGender(CreatureData.Gender.Male);
  }

  private void OnEnable()
  {
    EventManager.onPossess += new EventManager.PossessEvent(this.OnPossessionEvent);
    if ((Object) this.maleModel == (Object) null)
      Debug.LogWarning((object) $"Item {this.GetComponentInParent<Item>().data.id}'s ClothingGenderSwitcher is missing its male model.");
    if (!((Object) this.femaleModel == (Object) null))
      return;
    Debug.LogWarning((object) $"Item {this.GetComponentInParent<Item>().data.id}'s ClothingGenderSwitcher is missing its female model.");
  }

  private void OnDisable()
  {
    EventManager.onPossess -= new EventManager.PossessEvent(this.OnPossessionEvent);
  }

  private void OnPossessionEvent(Creature creature, EventTime eventTime)
  {
    if (eventTime != EventTime.OnEnd)
      return;
    this.SetGender(Player.currentCreature.data.gender);
  }

  public void SetModelActive(bool enabled)
  {
    this.SetMaleModelActive(enabled);
    this.SetFemaleModelActive(enabled);
  }

  public void SetMaleModelActive(bool enabled)
  {
    if ((bool) (Object) this.maleModel)
      this.maleModel.SetActive(enabled);
    else
      Debug.LogErrorFormat((Object) this, "ClothingGenderSwitcher - maleModel variable is not assigned!");
  }

  public void SetFemaleModelActive(bool enabled)
  {
    if ((bool) (Object) this.femaleModel)
      this.femaleModel.SetActive(enabled);
    else
      Debug.LogErrorFormat((Object) this, "ClothingGenderSwitcher - femaleModel variable is not assigned!");
  }

  public void SetGender(CreatureData.Gender gender)
  {
    Item component = this.GetComponent<Item>();
    if (gender == CreatureData.Gender.Female)
    {
      this.SetMaleModelActive(false);
      this.SetFemaleModelActive(true);
      if (!((Object) component != (Object) null))
        return;
      component.mainHandleLeft = (Object) this.mainFemaleHandleLeft != (Object) null ? this.mainFemaleHandleLeft : component.mainHandleLeft;
      component.mainHandleRight = (Object) this.mainFemaleHandleRight != (Object) null ? this.mainFemaleHandleRight : component.mainHandleRight;
    }
    else
    {
      this.SetMaleModelActive(true);
      this.SetFemaleModelActive(false);
      if (!((Object) component != (Object) null))
        return;
      component.mainHandleLeft = (Object) this.mainMaleHandleLeft != (Object) null ? this.mainMaleHandleLeft : component.mainHandleLeft;
      component.mainHandleRight = (Object) this.mainMaleHandleRight != (Object) null ? this.mainMaleHandleRight : component.mainHandleRight;
    }
  }
}
