// Decompiled with JetBrains decompiler
// Type: ThunderRoad.Container
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[AddComponentMenu("ThunderRoad/Container")]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Container.html")]
public class Container : MonoBehaviour
{
  public Container.LoadContent loadContent;
  public string loadPlayerContainerID;
  public string containerID;
  public bool loadOnStart = true;
  public Item.Owner spawnOwner;
  public List<Holder> linkedHolders = new List<Holder>();
  public List<ContainerContent> contents = new List<ContainerContent>();
  public bool allowStackItem;
  [NonSerialized]
  public bool contentLoaded;
  [NonSerialized]
  public Creature creature;

  public List<ValueDropdownItem<string>> GetAllContainerID()
  {
    return Catalog.GetDropdownAllID(Category.Container);
  }

  public event Container.ContentLoadedEvent OnContentLoadedEvent;

  public event Container.ContentChangeEvent OnContentAddEvent;

  public event Container.ContentChangeEvent OnContentRemoveEvent;

  /// <summary>
  /// Called when the quantity field of a container is set to a higher one
  /// </summary>
  public event Container.ContentChangeEvent OnContentQuantityIncreaseEvent;

  /// <summary>
  /// Called when the quantity field of a container is set to a lower one
  /// </summary>
  public event Container.ContentChangeEvent OnContentQuantityDecreaseEvent;

  protected void Awake()
  {
    if (!(bool) (UnityEngine.Object) GameManager.local)
      this.enabled = false;
    else
      this.creature = this.GetComponentInParent<Creature>();
  }

  protected void Start()
  {
    if ((bool) (UnityEngine.Object) this.creature || !this.loadOnStart)
      return;
    EventManager.onLevelLoad += new EventManager.LevelLoadEvent(this.OnLevelLoad);
  }

  private void OnLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
  {
    if (eventTime == EventTime.OnStart)
      return;
    try
    {
      if (this.enabled)
      {
        if (this.gameObject.activeInHierarchy)
          this.Load();
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) $"Error loading container on level load: {ex}");
    }
    EventManager.onLevelLoad -= new EventManager.LevelLoadEvent(this.OnLevelLoad);
  }

  public void Load()
  {
    List<ContainerContent> contents;
    if (!string.IsNullOrEmpty(this.loadPlayerContainerID) && Player.characterData.TryGetContainer(this.loadPlayerContainerID, out contents))
    {
      this.Load(contents);
    }
    else
    {
      switch (this.loadContent)
      {
        case Container.LoadContent.ContainerID:
          this.LoadFromContainerId();
          break;
        case Container.LoadContent.PlayerInventory:
          this.LoadFromPlayerInventory();
          break;
        default:
          this.contents = new List<ContainerContent>();
          this.LoadContents();
          break;
      }
    }
  }

  public void Load(List<ContainerContent> contents)
  {
    List<ContainerContent> containerContentList = Container.CloneContents(contents);
    if (!this.allowStackItem)
    {
      this.contents = containerContentList ?? new List<ContainerContent>();
    }
    else
    {
      this.contents = new List<ContainerContent>();
      if (containerContentList != null)
      {
        int count = containerContentList.Count;
        for (int index = 0; index < count; ++index)
          this.AddContent<ContainerContent>(containerContentList[index], true);
      }
    }
    this.LoadContents();
  }

  public void LoadFromContainerId()
  {
    ContainerData outputData;
    if (!string.IsNullOrEmpty(this.containerID) && this.containerID != "None" && Catalog.TryGetData<ContainerData>(this.containerID, out outputData))
      this.contents = outputData.GetClonedContents() ?? new List<ContainerContent>();
    this.LoadContents();
  }

  public void LoadFromPlayerInventory()
  {
    if (Player.characterData != null)
      this.contents = Player.characterData.CloneInventory() ?? new List<ContainerContent>();
    this.LoadContents();
  }

  public static bool ItemExist(string containerID, ItemData itemData)
  {
    List<ContainerContent> containerContents;
    if (Container.TryGetContent(containerID, out containerContents))
    {
      foreach (ContainerContent containerContent in containerContents)
      {
        if (itemData == containerContent.catalogData)
          return true;
      }
    }
    return false;
  }

  public static bool TryGetContent(string containerID, out List<ContainerContent> containerContents)
  {
    List<ContainerContent> contents;
    if (Player.characterData.TryGetContainer(containerID, out contents))
    {
      containerContents = contents;
      return true;
    }
    ContainerData outputData;
    if (Catalog.TryGetData<ContainerData>(containerID, out outputData))
    {
      containerContents = outputData.containerContents;
      return true;
    }
    containerContents = (List<ContainerContent>) null;
    return false;
  }

  public void ClearLinkedHolders()
  {
    foreach (Holder linkedHolder in this.linkedHolders)
    {
      for (int index = linkedHolder.items.Count - 1; index >= 0; --index)
        linkedHolder.items[index].Despawn();
    }
  }

  private void LoadContents()
  {
    List<ItemContent> itemContentList = new List<ItemContent>();
    int num = 0;
    foreach (ItemContent itemContent in this.contents.GetContentsOfType<ItemContent>())
    {
      ItemContent content = itemContent;
      if (content != null)
      {
        if (content.state is ContentStateHolder state1)
        {
          foreach (Holder linkedHolder in this.linkedHolders)
          {
            Holder holder = linkedHolder;
            if (holder.name == state1.holderName)
            {
              content.Spawn((Action<Item>) (item =>
              {
                item.linkedContainer = this;
                holder.Snap(item, true);
                this.contents.Remove((ContainerContent) content);
              }), this.spawnOwner);
              ++num;
              break;
            }
          }
        }
        if (content.state is ContentStatePlaced state2 && Level.current?.data?.id == state2.levelId)
          itemContentList.Add(content);
      }
    }
    if (!itemContentList.IsNullOrEmpty())
    {
      List<ItemContent> list = itemContentList.OrderBy<ItemContent, float>((Func<ItemContent, float>) (c => (c.state as ContentStatePlaced).lastSpawnTime)).ToList<ItemContent>();
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.AppendLine($"Spawning persistent items from container {this.name}. Max items: {Catalog.gameData.platformParameters.maxHomeItem}");
      foreach (ItemContent itemContent in list)
      {
        ItemContent content = itemContent;
        ContentStatePlaced contentStatePlaced = content.state as ContentStatePlaced;
        if (contentStatePlaced != null)
        {
          if (PlayerSaveData.playerStashContainerName != this.loadPlayerContainerID || contentStatePlaced.levelId != Player.characterData.mode.data.levelHome || num < Catalog.gameData.platformParameters.maxHomeItem)
          {
            content.Spawn((Action<Item>) (item =>
            {
              item.linkedContainer = this;
              item.DisallowDespawn = true;
              item.transform.SetPositionAndRotation(contentStatePlaced.position, contentStatePlaced.rotation);
              item.physicBody.isKinematic = contentStatePlaced.kinematic;
              this.contents.Remove((ContainerContent) content);
            }), this.spawnOwner);
            ++num;
            stringBuilder.AppendLine($"[{num}] {content.data.id}");
          }
          else
          {
            stringBuilder.AppendLine($"[Max Items Hit] {content.data.id} cannot spawn");
            content.state = (ContentState) null;
          }
        }
      }
      Debug.Log((object) stringBuilder.ToString());
    }
    this.contentLoaded = true;
    Container.ContentLoadedEvent contentLoadedEvent = this.OnContentLoadedEvent;
    if (contentLoadedEvent == null)
      return;
    contentLoadedEvent();
  }

  public List<ContainerContent> CloneContents() => Container.CloneContents(this.contents);

  public static List<ContainerContent> CloneContents(List<ContainerContent> contents)
  {
    List<ContainerContent> containerContentList = new List<ContainerContent>();
    foreach (ContainerContent content in contents)
    {
      if (content != null)
        containerContentList.Add(content.Clone());
    }
    return containerContentList;
  }

  public ContainerContent AddDataContent<T>(T data, string type = "data") where T : CatalogData, IContainerLoadable<T>
  {
    if ((object) data != null)
      return this.AddContent<ContainerContent>(data.InstanceContent());
    Debug.LogWarning((object) $"Can't add null {type} to inventory!");
    return (ContainerContent) null;
  }

  public bool HasDataContent<T>(T data) where T : CatalogData, IContainerLoadable<T>
  {
    return (object) data != null && this.contents.Exists((Predicate<ContainerContent>) (content => content.referenceID == ((T) data).id));
  }

  public bool TryGetDataContent<T>(string id, out T instance) where T : CatalogData
  {
    instance = default (T);
    if (string.IsNullOrEmpty(id))
      return false;
    instance = this.contents.Find((Predicate<ContainerContent>) (content => content.referenceID == id))?.catalogData as T;
    return (object) instance != null;
  }

  public List<ItemData> GetAllWardrobe()
  {
    List<ItemData> allWardrobe = new List<ItemData>();
    for (int index = 0; index < this.contents.Count; ++index)
    {
      if (this.contents[index].catalogData is ItemData catalogData && catalogData.type == ItemData.Type.Wardrobe)
        allWardrobe.Add(catalogData);
    }
    return allWardrobe;
  }

  public T AddDataContent<T, J>(J data, string type = "data")
    where T : ContainerContent<J, T>
    where J : CatalogData, IContainerLoadable<J>
  {
    return (T) this.AddDataContent<J>(data, type);
  }

  public ItemContent AddItemContent(
    Item item,
    bool despawnItem,
    ContentState state = null,
    List<ContentCustomData> customDataList = null)
  {
    item.InvokeOnContainerAddEvent(this);
    ItemContent itemContent = this.AddItemContent(item.data, state, customDataList != null ? customDataList : item.contentCustomData);
    if (!despawnItem)
      return itemContent;
    item.Despawn();
    return itemContent;
  }

  public ItemContent AddItemContent(
    string itemId,
    ContentState state = null,
    List<ContentCustomData> customDataList = null)
  {
    return this.AddItemContent(Catalog.GetData<ItemData>(itemId), state, customDataList);
  }

  public ItemContent AddItemContent(
    ItemData itemData,
    ContentState state = null,
    List<ContentCustomData> customDataList = null)
  {
    if (itemData != null)
      return this.AddContent<ItemContent>(new ItemContent(itemData.id, state, customDataList));
    Debug.LogWarning((object) "Can't add null item to inventory!");
    return (ItemContent) null;
  }

  public ItemContent AddItemContent(ItemContent itemContent)
  {
    return this.AddContent<ItemContent>(itemContent);
  }

  public bool HasSkillContent(string skillID)
  {
    return this.HasSkillContent(Catalog.GetData<SkillData>(skillID));
  }

  public bool HasSkillContent(SkillData skillData) => this.HasDataContent<SkillData>(skillData);

  public bool TryGetSkillContent<T>(string skillID, out T skillData) where T : SkillData
  {
    return this.TryGetDataContent<T>(skillID, out skillData);
  }

  public bool TryGetSkillContent<T>(SkillData skill, out T skillData) where T : SkillData
  {
    return this.TryGetDataContent<T>(skill.id, out skillData);
  }

  public SpellContent AddSpellContent(string spellID)
  {
    return this.AddSpellContent(Catalog.GetData<SpellData>(spellID));
  }

  public SpellContent AddSpellContent(SpellData spellData)
  {
    return (SpellContent) this.AddDataContent<SpellData>(spellData, "spell");
  }

  public SkillContent AddSkillContent(string skillID)
  {
    return this.AddSkillContent(Catalog.GetData<SkillData>(skillID));
  }

  public SkillContent AddSkillContent(SkillData skillData)
  {
    return (SkillContent) this.AddDataContent<SkillData>(skillData, "skill");
  }

  protected T AddContent<T>(T content, bool isSilent = false) where T : ContainerContent
  {
    if ((object) content == null)
      return default (T);
    if (!isSilent)
    {
      Container.ContentChangeEvent onContentAddEvent = this.OnContentAddEvent;
      if (onContentAddEvent != null)
        onContentAddEvent((ContainerContent) content, EventTime.OnStart);
    }
    if (this.allowStackItem && content is ItemContent itemContent1 && itemContent1.data.isStackable && itemContent1.state == null)
    {
      ItemContent itemContent = (ItemContent) null;
      int count = this.contents.Count;
      for (int index = 0; index < count; ++index)
      {
        if (this.contents[index] is ItemContent content1 && content1.referenceID == itemContent1.referenceID && content1.state == null)
        {
          itemContent = content1;
          break;
        }
      }
      if (itemContent != null)
        itemContent.quantity += itemContent1.quantity;
      else
        this.contents.Add((ContainerContent) content);
    }
    else
      this.contents.Add((ContainerContent) content);
    if (!isSilent)
    {
      Container.ContentChangeEvent onContentAddEvent = this.OnContentAddEvent;
      if (onContentAddEvent != null)
        onContentAddEvent((ContainerContent) content, EventTime.OnEnd);
    }
    return content;
  }

  /// <summary>
  /// Used to set the quantity field of some content. Calls the increase / decrease hooks if value is different</summary>
  /// <param name="content">Content to change</param>
  /// <param name="newQuantity">Quantity to set</param>
  public void SetContentQuantity(ContainerContent content, int newQuantity)
  {
    int num = 0;
    if (content is ItemContent itemContent)
    {
      num = itemContent.quantity;
      if (num > newQuantity)
      {
        Container.ContentChangeEvent quantityDecreaseEvent = this.OnContentQuantityDecreaseEvent;
        if (quantityDecreaseEvent != null)
          quantityDecreaseEvent(content, EventTime.OnStart);
      }
      if (num < newQuantity)
      {
        Container.ContentChangeEvent quantityIncreaseEvent = this.OnContentQuantityIncreaseEvent;
        if (quantityIncreaseEvent != null)
          quantityIncreaseEvent(content, EventTime.OnStart);
      }
      itemContent.SetQuantity(newQuantity);
    }
    if (content is TableContent tableContent)
    {
      num = tableContent.quantity;
      if (num > newQuantity)
      {
        Container.ContentChangeEvent quantityDecreaseEvent = this.OnContentQuantityDecreaseEvent;
        if (quantityDecreaseEvent != null)
          quantityDecreaseEvent(content, EventTime.OnStart);
      }
      if (num < newQuantity)
      {
        Container.ContentChangeEvent quantityIncreaseEvent = this.OnContentQuantityIncreaseEvent;
        if (quantityIncreaseEvent != null)
          quantityIncreaseEvent(content, EventTime.OnStart);
      }
      tableContent.SetQuantity(newQuantity);
    }
    if (num > newQuantity)
    {
      Container.ContentChangeEvent quantityDecreaseEvent = this.OnContentQuantityDecreaseEvent;
      if (quantityDecreaseEvent != null)
        quantityDecreaseEvent(content, EventTime.OnEnd);
    }
    if (num >= newQuantity)
      return;
    Container.ContentChangeEvent quantityIncreaseEvent1 = this.OnContentQuantityIncreaseEvent;
    if (quantityIncreaseEvent1 == null)
      return;
    quantityIncreaseEvent1(content, EventTime.OnEnd);
  }

  public void RemoveContent(string referenceID, int count = 0, bool removeIfQuantityNull = true)
  {
    int num = count;
    for (int index = this.contents.Count - 1; index >= 0; --index)
    {
      ContainerContent content = this.contents[index];
      if (!(content.referenceID != referenceID))
      {
        Container.ContentChangeEvent contentRemoveEvent1 = this.OnContentRemoveEvent;
        if (contentRemoveEvent1 != null)
          contentRemoveEvent1(content, EventTime.OnStart);
        if (content is ItemContent itemContent && num > 0)
        {
          if (itemContent.quantity > num)
          {
            itemContent.quantity -= num;
            num = 0;
          }
          else
          {
            num -= itemContent.quantity;
            if (removeIfQuantityNull)
              this.contents.RemoveAt(index);
            else
              itemContent.quantity = 0;
          }
        }
        if (count <= 0)
          this.contents.RemoveAt(index);
        Container.ContentChangeEvent contentRemoveEvent2 = this.OnContentRemoveEvent;
        if (contentRemoveEvent2 != null)
          contentRemoveEvent2(content, EventTime.OnEnd);
        if (count > 0 && num == 0)
          break;
      }
    }
  }

  public void RemoveContent(ContainerContent content)
  {
    for (int index = this.contents.Count - 1; index >= 0; --index)
    {
      if (this.contents[index] == content)
      {
        Container.ContentChangeEvent contentRemoveEvent1 = this.OnContentRemoveEvent;
        if (contentRemoveEvent1 != null)
          contentRemoveEvent1(content, EventTime.OnStart);
        this.contents.RemoveAt(index);
        Container.ContentChangeEvent contentRemoveEvent2 = this.OnContentRemoveEvent;
        if (contentRemoveEvent2 == null)
          break;
        contentRemoveEvent2(content, EventTime.OnEnd);
        break;
      }
    }
  }

  public bool IsPlayersContainer()
  {
    return (UnityEngine.Object) this.creature != (UnityEngine.Object) null && (UnityEngine.Object) this.creature == (UnityEngine.Object) Player.currentCreature;
  }

  public enum LoadContent
  {
    None,
    ContainerID,
    PlayerInventory,
  }

  public delegate void ContentLoadedEvent();

  public delegate void ContentChangeEvent(ContainerContent content, EventTime eventTime);
}
