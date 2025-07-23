// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Equipment
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Manikin;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Equipment.html")]
[AddComponentMenu("ThunderRoad/Creatures/Equipment")]
public class Equipment : MonoBehaviour
{
  public bool canSwapExistingArmour = true;
  public bool equipWardrobesOnLoad = true;
  public bool armourEditModeEnabled;
  [NonSerialized]
  public Creature creature;
  [NonSerialized]
  public Renderer leftSelectedPart;
  [NonSerialized]
  public Renderer rightSelectedPart;
  [NonSerialized]
  public Wearable lastSelectedWearable;
  [NonSerialized]
  public readonly List<Wearable> wearableSlots = new List<Wearable>();
  public Equipment.OnCreaturePartChanged onCreaturePartChanged;
  public static Equipment.OnCreaturePartChanged onAnyCreaturePartChanged;
  protected Handle heldLeft;
  protected Handle heldRight;
  protected Coroutine waitPartUpdateCoroutine;
  private Dictionary<Holder, Holder.HolderDelegate> holderSnapHandlers = new Dictionary<Holder, Holder.HolderDelegate>();
  private Dictionary<Holder, Holder.HolderDelegate> holderUnSnapHandlers = new Dictionary<Holder, Holder.HolderDelegate>();

  /// <summary>Invoked when armour has been equipped.</summary>
  public Equipment.OnArmourEquipped OnArmourEquippedEvent { get; set; }

  /// <summary>Invoked when armour has been unequipped.</summary>
  public Equipment.OnArmourUnEquipped OnArmourUnEquippedEvent { get; set; }

  private void Awake()
  {
    EventManager.onItemSpawnEquip += new EventManager.OnItemSpawnEquip(this.OnItemSpawnEquip);
  }

  public void Init(Creature creature)
  {
    this.creature = creature;
    creature.container.OnContentRemoveEvent += new Container.ContentChangeEvent(this.OnContainerContentRemoved);
    if ((bool) (UnityEngine.Object) creature.manikinParts)
      creature.manikinParts.UpdateParts_Completed += new ManikinPartList.UpdatePartsCompletedHandler(this.OnUpdatePartCompleted);
    this.ManageHolsterHandlers(true);
  }

  public void Load(bool reEquipAllWardrobes = false)
  {
    if (!this.equipWardrobesOnLoad)
      return;
    this.EquipAllWardrobes(false, reEquipAllWardrobes);
  }

  private void OnItemSpawnEquip(Item item)
  {
    if (!this.creature.isPlayer)
      return;
    if (item.data.type == ItemData.Type.Wardrobe)
    {
      ItemModuleWardrobe.CreatureWardrobe wardrobe = item.data.GetModule<ItemModuleWardrobe>().GetWardrobe(this.creature);
      if (wardrobe.creatureName != this.creature.data.name)
        return;
      for (int index = 0; index < this.wearableSlots.Count; ++index)
      {
        Wearable wearableSlot = this.wearableSlots[index];
        if (wearableSlot.IsItemCompatible(wardrobe))
        {
          wearableSlot.EquipItem(item);
          break;
        }
      }
    }
    else
    {
      if (!item.data.HasModule<ItemModuleWardrobe>())
        return;
      Debug.LogWarning((object) $"Cannot equip {item.data.id} because it's type is not Wardrobe");
    }
  }

  public void OnDespawn()
  {
    this.ManageHolsterHandlers(false);
    if ((bool) (UnityEngine.Object) this.GetHeldItem(Side.Left))
      this.GetHeldItem(Side.Left).Despawn();
    if ((bool) (UnityEngine.Object) this.GetHeldItem(Side.Right))
      this.GetHeldItem(Side.Right).Despawn();
    foreach (Holder holder in this.creature.holders)
    {
      for (int index = 0; index < holder.items.Count; ++index)
        holder.items[index].Despawn();
    }
    EventManager.onItemSpawnEquip -= new EventManager.OnItemSpawnEquip(this.OnItemSpawnEquip);
    this.UnequipAllWardrobes();
  }

  protected void OnContainerContentRemoved(ContainerContent content, EventTime eventTime)
  {
    if (!(content is ItemContent itemContent) || itemContent.data.type != ItemData.Type.Wardrobe || eventTime != EventTime.OnStart || !(itemContent.state is ContentStateWorn))
      return;
    this.UnequipWardrobe(itemContent);
  }

  public void SetWearablesState(bool active)
  {
    for (int index = 0; index < this.wearableSlots.Count; ++index)
      this.wearableSlots[index].SetTouch(active);
  }

  public void UnequipAllWardrobes(bool updateParts = false, bool updateWornState = true)
  {
    if (!(bool) (UnityEngine.Object) this.creature.manikinParts)
      return;
    this.creature.manikinParts.disableRenderersDuringUpdate = true;
    if (updateParts && this.waitPartUpdateCoroutine != null)
      this.StopCoroutine(this.waitPartUpdateCoroutine);
    if (updateWornState)
    {
      foreach (ItemOrTableContent<ItemData, ItemContent> itemOrTableContent in this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content => content.HasState<ContentStateWorn>())))
        itemOrTableContent.state = (ContentState) null;
    }
    this.creature.manikinLocations.FromJson(this.creature.orgWardrobeLocations);
    if (!updateParts)
      return;
    this.UpdateParts();
  }

  public void EquipAllWardrobes(bool bodyOnly, bool reEquipAllWardrobes = true)
  {
    if (!(bool) (UnityEngine.Object) this.creature.manikinParts)
      return;
    this.creature.manikinParts.disableRenderersDuringUpdate = true;
    if (reEquipAllWardrobes)
      this.UnequipAllWardrobes(updateWornState: !reEquipAllWardrobes);
    foreach (ItemContent itemContent in this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content =>
    {
      ItemModuleWardrobe module;
      if (!content.data.TryGetModule<ItemModuleWardrobe>(out module))
        return false;
      return !bodyOnly || module.category == Equipment.WardRobeCategory.Body;
    })))
    {
      if (itemContent.HasState<ContentStateWorn>())
        this.EquipWardrobe(itemContent, false);
    }
    this.UpdateParts();
  }

  public void EquipWardrobe(ItemContent itemContent, bool updateParts = true)
  {
    if (!(bool) (UnityEngine.Object) this.creature.manikinParts)
      return;
    if (itemContent.data.type != ItemData.Type.Wardrobe)
    {
      Debug.LogError((object) $"Cannot wear {itemContent.referenceID} because it's not a Wardrobe");
    }
    else
    {
      ItemModuleWardrobe module1;
      if (!itemContent.data.TryGetModule<ItemModuleWardrobe>(out module1))
      {
        Debug.LogError((object) $"Cannot wear {itemContent.referenceID} because it doesn't have an itemModuleWardrobe");
      }
      else
      {
        ItemModuleWardrobe.CreatureWardrobe wardrobe;
        if (!module1.TryGetWardrobe(this.creature, out wardrobe) || (UnityEngine.Object) wardrobe.manikinWardrobeData == (UnityEngine.Object) null)
          return;
        if (updateParts)
          this.creature.manikinParts.disableRenderersDuringUpdate = false;
        for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
        {
          ItemContent wornContent = this.GetWornContent(wardrobe.manikinWardrobeData.channels[index], wardrobe.manikinWardrobeData.layers[index]);
          if (wornContent != null)
            this.UnequipWardrobe(wornContent, false);
        }
        foreach (ItemModule module2 in itemContent.data.modules)
        {
          ApparelModuleType result;
          if (module2 is ItemModuleApparel itemModuleApparel && Enum.TryParse<ApparelModuleType>(wardrobe.manikinWardrobeData.channels[0], true, out result))
            itemModuleApparel.OnEquip(this.creature, result, wardrobe);
        }
        itemContent.state = (ContentState) new ContentStateWorn();
        this.creature.manikinLocations.AddPart(wardrobe.manikinWardrobeData);
        if (updateParts)
          this.UpdateParts();
        this.creature.mana?.OnApparelChangeEvent();
        this.creature.armorSFX?.CalculateEffectData();
      }
    }
  }

  public void UnequipWardrobe(ItemContent itemContent, bool updateParts = true)
  {
    if (!(bool) (UnityEngine.Object) this.creature.manikinParts)
      return;
    if (itemContent.data.type != ItemData.Type.Wardrobe)
    {
      Debug.LogError((object) $"Cannot unwear {itemContent.referenceID} because it's not a Wardrobe");
    }
    else
    {
      ItemModuleWardrobe module1;
      if (!itemContent.data.TryGetModule<ItemModuleWardrobe>(out module1))
      {
        Debug.LogError((object) $"Cannot unwear {itemContent.referenceID} because it doesn't have an itemModuleWardrobe");
      }
      else
      {
        ItemModuleWardrobe.CreatureWardrobe wardrobe;
        if (!module1.TryGetWardrobe(this.creature, out wardrobe) || (UnityEngine.Object) wardrobe.manikinWardrobeData == (UnityEngine.Object) null)
        {
          Debug.LogError((object) $"Cannot unwear {itemContent.referenceID} because it doesn't have a wardrobeData or wardrobeData.manikinWardrobeData");
        }
        else
        {
          if (updateParts)
            this.creature.manikinParts.disableRenderersDuringUpdate = false;
          bool flag = false;
          for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
          {
            if (this.GetWornContent(wardrobe.manikinWardrobeData.channels[index], wardrobe.manikinWardrobeData.layers[index]) != null)
            {
              this.creature.manikinLocations.RemovePart(wardrobe.manikinWardrobeData.channels[index], wardrobe.manikinWardrobeData.layers[index]);
              flag = true;
            }
          }
          itemContent.state = (ContentState) null;
          if (flag)
          {
            foreach (ItemModule module2 in itemContent.data.modules)
            {
              ApparelModuleType result;
              if (module2 is ItemModuleApparel itemModuleApparel && Enum.TryParse<ApparelModuleType>(wardrobe.manikinWardrobeData.channels[0], true, out result))
                itemModuleApparel.OnUnequip(this.creature, result, wardrobe);
            }
          }
          else
            Debug.LogWarning((object) $"Cannot unwear {itemContent.referenceID} because it was already not worn");
          if (updateParts)
            this.UpdateParts();
          this.creature.mana?.OnApparelChangeEvent();
          this.creature.armorSFX?.CalculateEffectData();
        }
      }
    }
  }

  public ItemContent[] GetWornContents()
  {
    return this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content => content.HasState<ContentStateWorn>())).ToArray<ItemContent>();
  }

  public ItemContent GetWornContent(string channel, int layer)
  {
    foreach (ItemContent wornContent in this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content => content.HasState<ContentStateWorn>())))
    {
      ItemModuleWardrobe module = wornContent.data.GetModule<ItemModuleWardrobe>();
      if (module != null)
      {
        ItemModuleWardrobe.CreatureWardrobe wardrobe = module.GetWardrobe(this.creature);
        if (wardrobe != null)
        {
          ManikinWardrobeData manikinWardrobeData = wardrobe.manikinWardrobeData;
          if (!((UnityEngine.Object) manikinWardrobeData == (UnityEngine.Object) null))
          {
            for (int index = 0; index < manikinWardrobeData.channels.Length; ++index)
            {
              string channel1 = manikinWardrobeData.channels[index];
              int layer1 = manikinWardrobeData.layers[index];
              string str = channel;
              if (channel1 == str && layer1 == layer)
                return wornContent;
            }
          }
        }
      }
    }
    return (ItemContent) null;
  }

  /// <summary>
  /// Get the lowest layer of equipment on the target manikin channel.
  /// </summary>
  public ItemContent GetWornContentLowerLayer(string channel, params string[] onlyLayers)
  {
    int[] source = new int[onlyLayers.Length];
    for (int index = 0; index < onlyLayers.Length; ++index)
      source[index] = ItemModuleWardrobe.GetLayer(channel, onlyLayers[index]);
    int num = int.MaxValue;
    ItemContent contentLowerLayer = (ItemContent) null;
    foreach (ItemContent itemContent in this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content => content.HasState<ContentStateWorn>())))
    {
      ItemModuleWardrobe module;
      if (itemContent.data.TryGetModule<ItemModuleWardrobe>(out module))
      {
        ItemModuleWardrobe.CreatureWardrobe wardrobe = module.GetWardrobe(this.creature);
        if (wardrobe != null && (UnityEngine.Object) wardrobe.manikinWardrobeData != (UnityEngine.Object) null)
        {
          for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
          {
            if (wardrobe.manikinWardrobeData.channels[index] == channel && (onlyLayers == null || onlyLayers.Length == 0 || ((IEnumerable<int>) source).Contains<int>(wardrobe.manikinWardrobeData.layers[index])) && wardrobe.manikinWardrobeData.layers[index] < num)
            {
              num = wardrobe.manikinWardrobeData.layers[index];
              contentLowerLayer = itemContent;
            }
          }
        }
      }
    }
    return contentLowerLayer;
  }

  /// <summary>
  /// Get all equipment on a specific manikin channel, filtered by layers
  /// </summary>
  public ItemContent[] GetWornContentsLowerLayer(string channel, params string[] onlyLayers)
  {
    int[] source = new int[onlyLayers.Length];
    for (int index = 0; index < onlyLayers.Length; ++index)
      source[index] = ItemModuleWardrobe.GetLayer(channel, onlyLayers[index]);
    List<ItemContent> itemContentList = new List<ItemContent>();
    foreach (ItemContent itemContent in this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content => content.HasState<ContentStateWorn>())))
    {
      ItemModuleWardrobe module;
      if (itemContent.data.TryGetModule<ItemModuleWardrobe>(out module))
      {
        ItemModuleWardrobe.CreatureWardrobe wardrobe = module.GetWardrobe(this.creature);
        if (wardrobe != null && (UnityEngine.Object) wardrobe.manikinWardrobeData != (UnityEngine.Object) null)
        {
          for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
          {
            if (wardrobe.manikinWardrobeData.channels[index] == channel && (onlyLayers == null || onlyLayers.Length == 0 || ((IEnumerable<int>) source).Contains<int>(wardrobe.manikinWardrobeData.layers[index])))
            {
              itemContentList.Add(itemContent);
              break;
            }
          }
        }
      }
    }
    return itemContentList.ToArray();
  }

  /// <summary>Get all equipment on a specific manikin channel.</summary>
  public ItemContent[] GetWornContentsLowerLayer(string channel)
  {
    List<ItemContent> itemContentList = new List<ItemContent>();
    foreach (ItemContent itemContent in this.creature.container.contents.GetEnumerableContentsOfType<ItemContent>(true, (Func<ItemContent, bool>) (content => content.HasState<ContentStateWorn>())))
    {
      ItemModuleWardrobe module;
      if (itemContent.data.TryGetModule<ItemModuleWardrobe>(out module))
      {
        ItemModuleWardrobe.CreatureWardrobe wardrobe = module.GetWardrobe(this.creature);
        if (wardrobe != null && (UnityEngine.Object) wardrobe.manikinWardrobeData != (UnityEngine.Object) null)
        {
          for (int index = 0; index < wardrobe.manikinWardrobeData.channels.Length; ++index)
          {
            if (wardrobe.manikinWardrobeData.channels[index] == channel)
            {
              itemContentList.Add(itemContent);
              break;
            }
          }
        }
      }
    }
    return itemContentList.ToArray();
  }

  /// <summary>Get the equipment on a specific part.</summary>
  public ItemContent[] GetEquipmentOnPart(RagdollPart.Type partType)
  {
    Wearable wearable = this.creature.ragdoll.GetPart(partType).wearable;
    if (!((UnityEngine.Object) wearable == (UnityEngine.Object) null))
      return this.GetWornContentsLowerLayer(wearable.wardrobeChannel);
    Debug.LogWarning((object) $"Part {partType} on {this.creature.name} does not contain a wearable!");
    return (ItemContent[]) null;
  }

  public void UpdateParts()
  {
    if (this.waitPartUpdateCoroutine != null)
      this.StopCoroutine(this.waitPartUpdateCoroutine);
    this.waitPartUpdateCoroutine = this.StartCoroutine(this.WaitPartUpdateCoroutine());
  }

  protected IEnumerator WaitPartUpdateCoroutine()
  {
    Equipment.OnCreaturePartChanged creaturePartChanged1 = this.onCreaturePartChanged;
    if (creaturePartChanged1 != null)
      creaturePartChanged1(EventTime.OnStart);
    Equipment.OnCreaturePartChanged creaturePartChanged2 = Equipment.onAnyCreaturePartChanged;
    if (creaturePartChanged2 != null)
      creaturePartChanged2(EventTime.OnStart);
    while (this.GetPendingApparelLoading() > 0)
      yield return (object) Yielders.EndOfFrame;
    this.creature.manikinLocations.UpdateParts();
  }

  public int GetPendingApparelLoading()
  {
    return this.creature?.manikinParts?.PendingHandles().GetValueOrDefault();
  }

  protected void OnUpdatePartCompleted(ManikinPart[] partsAdded)
  {
    Equipment.OnCreaturePartChanged creaturePartChanged1 = this.onCreaturePartChanged;
    if (creaturePartChanged1 != null)
      creaturePartChanged1(EventTime.OnEnd);
    Equipment.OnCreaturePartChanged creaturePartChanged2 = Equipment.onAnyCreaturePartChanged;
    if (creaturePartChanged2 == null)
      return;
    creaturePartChanged2(EventTime.OnEnd);
  }

  public event Equipment.HeldWeaponReceivedHit OnHeldWeaponHitEvent;

  public void HeldWeaponHit(CollisionInstance collisionInstance)
  {
    if (this.OnHeldWeaponHitEvent == null)
      return;
    this.OnHeldWeaponHitEvent(collisionInstance);
  }

  public event Equipment.HeldItemsChanged OnHeldItemsChangeEvent;

  public void HeldItemsChange()
  {
    if (this.OnHeldItemsChangeEvent != null)
      this.OnHeldItemsChangeEvent(this.heldRight?.item, this.heldLeft?.item, this.GetHeldItem(Side.Right), this.GetHeldItem(Side.Left));
    this.creature.animator.SetInteger(Creature.hashFreeHands, ((UnityEngine.Object) this.GetHeldHandle(Side.Right) == (UnityEngine.Object) null ? 1 : 0) + ((UnityEngine.Object) this.GetHeldHandle(Side.Left) == (UnityEngine.Object) null ? 2 : 0));
  }

  public event Equipment.HolsterInteracted OnHolsterInteractedEvent;

  protected void ManageHolsterHandlers(bool add)
  {
    foreach (Holder holder1 in this.creature.holders)
    {
      Holder holder = holder1;
      this.holderSnapHandlers[holder] = new Holder.HolderDelegate(HolderSnap);
      this.holderUnSnapHandlers[holder] = new Holder.HolderDelegate(HolderUnsnap);
      if (add)
      {
        holder.Snapped += this.holderSnapHandlers[holder];
        holder.UnSnapped += this.holderUnSnapHandlers[holder];
      }
      else
      {
        holder.Snapped -= this.holderSnapHandlers[holder];
        holder.UnSnapped -= this.holderUnSnapHandlers[holder];
      }

      void HolderSnap(Item item) => HolderChange(item, true);

      void HolderUnsnap(Item item) => HolderChange(item, false);

      void HolderChange(Item item, bool add)
      {
        Equipment.HolsterInteracted holsterInteractedEvent = this.OnHolsterInteractedEvent;
        if (holsterInteractedEvent == null)
          return;
        holsterInteractedEvent(holder, item, add);
      }
    }
  }

  public Handle GetHeldHandle(Side handSide)
  {
    switch (handSide)
    {
      case Side.Right:
        return this.creature.handRight?.grabbedHandle;
      case Side.Left:
        return this.creature.handLeft?.grabbedHandle;
      default:
        return (Handle) null;
    }
  }

  public Item GetHeldItem(Side handSide) => this.GetHeldHandle(handSide)?.item;

  public Item GetHeldWeapon(Side handSide)
  {
    Item heldItem = this.GetHeldItem(handSide);
    ItemData.Type? type1;
    if (heldItem != null)
    {
      type1 = heldItem.data?.type;
      ItemData.Type type2 = ItemData.Type.Weapon;
      if (type1.GetValueOrDefault() == type2 & type1.HasValue)
        goto label_5;
    }
    if (heldItem != null)
    {
      type1 = heldItem.data?.type;
      ItemData.Type type3 = ItemData.Type.Shield;
      if (type1.GetValueOrDefault() == type3 & type1.HasValue)
        goto label_5;
    }
    return (Item) null;
label_5:
    return heldItem;
  }

  public bool CheckWeapon(Item weapon, Equipment.WeaponDrawInfo info, Transform target = null)
  {
    if ((UnityEngine.Object) weapon == (UnityEngine.Object) null || weapon.data.moduleAI == null)
      return false;
    if (info.checkAmmo)
    {
      Item heldWeapon1 = this.GetHeldWeapon(Side.Left);
      bool flag1 = (bool) (UnityEngine.Object) heldWeapon1 && heldWeapon1.data.slot == weapon.data.moduleAI.rangedWeaponData?.ammoType;
      Item heldWeapon2 = this.GetHeldWeapon(Side.Right);
      bool flag2 = (bool) (UnityEngine.Object) heldWeapon2 && heldWeapon2.data.slot == weapon.data.moduleAI.rangedWeaponData?.ammoType;
      if (!(bool) (UnityEngine.Object) this.GetQuiverAmmo(weapon.data.moduleAI.rangedWeaponData?.ammoType) && !flag1 && !flag2)
        return false;
    }
    if (weapon.data.moduleAI.rangedWeaponData != null && (UnityEngine.Object) target != (UnityEngine.Object) null && (double) (target.position - this.creature.transform.position).magnitude < (double) weapon.data.moduleAI.rangedWeaponData.tooCloseDistance || weapon.data.moduleAI.primaryClass != info.weaponClass && weapon.data.moduleAI.secondaryClass != info.weaponClass)
      return false;
    return weapon.data.moduleAI.weaponHandling == info.weaponHandling || weapon.data.moduleAI.secondaryHandling == info.weaponHandling;
  }

  public List<Item> GetAllHolsteredItems()
  {
    List<Item> allHolsteredItems = new List<Item>();
    List<Holder> holderList = new List<Holder>();
    holderList.AddRange((IEnumerable<Holder>) this.creature.holders);
    while (holderList.Count > 0)
    {
      foreach (Item obj in holderList[0].items)
      {
        allHolsteredItems.Add(obj);
        if (obj.childHolders.Count > 0)
          holderList.AddRange((IEnumerable<Holder>) obj.childHolders);
      }
      holderList.RemoveAt(0);
    }
    return allHolsteredItems;
  }

  public List<Item> GetHolsterWeapons()
  {
    List<Item> holsterWeapons = new List<Item>();
    foreach (Holder holder in this.creature.holders)
    {
      foreach (Item obj in holder.items)
      {
        if (obj.data != null && (obj.data.type == ItemData.Type.Weapon || obj.data.type == ItemData.Type.Weapon))
          holsterWeapons.Add(obj);
      }
    }
    return holsterWeapons;
  }

  public void GetBestMatch(
    Equipment.WeaponDrawInfo info,
    Side side,
    out Item set,
    Item disallowed = null,
    Transform target = null)
  {
    float num = 0.0f;
    set = (Item) null;
    int count1 = this.creature.holders.Count;
    for (int index1 = 0; index1 < count1; ++index1)
    {
      Holder holder = this.creature.holders[index1];
      int count2 = holder.items.Count;
      for (int index2 = 0; index2 < count2; ++index2)
      {
        Item weapon1 = holder.items[index2];
        if (!((UnityEngine.Object) disallowed == (UnityEngine.Object) weapon1))
        {
          if (this.CheckWeapon(weapon1, info, target))
          {
            Handle mainHandle = weapon1.GetMainHandle(side);
            if ((double) mainHandle.reach > (double) num)
            {
              num = mainHandle.reach;
              set = weapon1;
            }
          }
          if (weapon1.childHolders.Count > 0 && info.checkInHolder)
          {
            foreach (Holder childHolder in weapon1.childHolders)
            {
              foreach (Item weapon2 in childHolder.items)
              {
                if (!((UnityEngine.Object) disallowed == (UnityEngine.Object) weapon2) && this.CheckWeapon(weapon2, info, target))
                {
                  Handle mainHandle = weapon2.GetMainHandle(side);
                  if ((double) mainHandle.reach > (double) num)
                  {
                    num = mainHandle.reach;
                    set = weapon2;
                  }
                }
              }
            }
          }
        }
      }
    }
  }

  public Holder GetQuiverAmmo(string ammoType, bool requireContents = true)
  {
    foreach (Holder holder in this.creature.holders)
    {
      foreach (Item obj in holder.items)
      {
        if (obj.data.type == ItemData.Type.Quiver && obj.childHolders.Count > 0)
        {
          Holder childHolder = obj.childHolders[0];
          if (childHolder.data.slots.Contains(ammoType) && (!requireContents || childHolder.items.Count > 0))
            return childHolder;
        }
      }
    }
    return (Holder) null;
  }

  public Holder GetHolder(Holder.DrawSlot drawSlot)
  {
    foreach (Holder holder in this.creature.holders)
    {
      if (holder.drawSlot == drawSlot)
        return holder;
    }
    return (Holder) null;
  }

  public Holder GetFreeDrawHolder(
    Side side,
    bool hipsIsPriority,
    string slot = null,
    Holder ignoreHolder = null)
  {
    if (side == Side.Right)
    {
      Holder holder1 = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.HipsRight : Holder.DrawSlot.BackRight);
      if ((bool) (UnityEngine.Object) holder1 && holder1.HasSlotFree() && (UnityEngine.Object) holder1 != (UnityEngine.Object) ignoreHolder && (slot == null || holder1.data.SlotAllowed(slot)))
        return holder1;
      Holder holder2 = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.BackRight : Holder.DrawSlot.HipsRight);
      if ((bool) (UnityEngine.Object) holder2 && holder2.HasSlotFree() && (UnityEngine.Object) holder2 != (UnityEngine.Object) ignoreHolder && (slot == null || holder2.data.SlotAllowed(slot)))
        return holder2;
    }
    else
    {
      Holder holder3 = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.HipsLeft : Holder.DrawSlot.BackLeft);
      if ((bool) (UnityEngine.Object) holder3 && holder3.HasSlotFree() && (UnityEngine.Object) holder3 != (UnityEngine.Object) ignoreHolder && (slot == null || holder3.data.SlotAllowed(slot)))
        return holder3;
      Holder holder4 = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.BackLeft : Holder.DrawSlot.HipsLeft);
      if ((bool) (UnityEngine.Object) holder4 && holder4.HasSlotFree() && (UnityEngine.Object) holder4 != (UnityEngine.Object) ignoreHolder && (slot == null || holder4.data.SlotAllowed(slot)))
        return holder4;
    }
    return (Holder) null;
  }

  public Holder GetFirstFreeHolder(string slot = null, Holder ignoreHolder = null)
  {
    foreach (Holder holder in this.creature.holders)
    {
      if (!((UnityEngine.Object) holder == (UnityEngine.Object) ignoreHolder) && holder.HasSlotFree() && (slot == null || holder.data.SlotAllowed(slot)))
        return holder;
    }
    return (Holder) null;
  }

  public delegate void OnArmourEquipped(Wearable slot, Item item);

  public delegate void OnArmourUnEquipped(Wearable slot, Item item);

  public class WeaponDrawInfo
  {
    public ItemModuleAI.WeaponClass weaponClass;
    public ItemModuleAI.WeaponHandling weaponHandling;
    public bool checkAmmo;
    public bool checkInHolder;
  }

  public enum WardRobeCategory
  {
    Apparel,
    Body,
  }

  public delegate void OnCreaturePartChanged(EventTime eventTime);

  public delegate void HeldWeaponReceivedHit(CollisionInstance collisionInstance);

  public delegate void HeldItemsChanged(
    Item oldRightHand,
    Item oldLeftHand,
    Item newRightHand,
    Item newLeftHand);

  public delegate void HolsterInteracted(Holder holder, Item holsteredItem, bool added);
}
