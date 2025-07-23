// Decompiled with JetBrains decompiler
// Type: ThunderRoad.WeakPointRandomizer
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class WeakPointRandomizer : MonoBehaviour
{
  public List<WeakPointRandomizer.Group> groups;

  public List<Transform> RandomizeWeakPoints(int targetCount, bool ignoreMinimumPerController = false)
  {
    this.groups.OrderByDescending<WeakPointRandomizer.Group, int>((Func<WeakPointRandomizer.Group, int>) (group => group.priority));
    List<Transform> transformList = new List<Transform>();
    for (int index = 0; index < this.groups.Count; ++index)
      transformList.AddRange((IEnumerable<Transform>) this.groups[index].GetMinRandomlyPickedPoints());
    if (transformList.Count < targetCount)
    {
      while (transformList.Count < targetCount)
      {
        WeakPointRandomizer.Group result;
        if (!this.groups.WeightedFilteredSelectInPlace<WeakPointRandomizer.Group>((Func<WeakPointRandomizer.Group, bool>) (group => (double) group.selectedCount < (double) group.minMaxWeakPoints.y), (Func<WeakPointRandomizer.Group, float>) (group => (float) group.priority), out result))
        {
          Debug.LogError((object) "Can't add more weak points without violating weak point controller rules!");
          break;
        }
        transformList.Add(result.GetNewRandPoint());
      }
    }
    if (transformList.Count > targetCount)
    {
      while (transformList.Count > targetCount)
      {
        WeakPointRandomizer.Group result;
        if (!this.groups.WeightedFilteredSelectInPlace<WeakPointRandomizer.Group>((Func<WeakPointRandomizer.Group, bool>) (group => group.selectedCount > 1), (Func<WeakPointRandomizer.Group, float>) (group => (float) group.selectedCount), out result) && !ignoreMinimumPerController)
        {
          Debug.LogError((object) "Couldn't reduce number of weak points without violating weak point controller rules!");
          break;
        }
        transformList.Remove(result.RandRemove());
      }
    }
    for (int index = 0; index < this.groups.Count; ++index)
      this.groups[index].RemoveAllNonSelected();
    return transformList;
  }

  [Serializable]
  public class Group
  {
    public Transform parent;
    public int priority;
    public UnityEngine.Vector2 minMaxWeakPoints;
    private bool initialized;
    private List<Transform> allWeakPointOptions;
    private List<Transform> shuffled;
    private List<Transform> selected;

    public int selectedCount => this.selected.Count;

    private void Init()
    {
      this.allWeakPointOptions = new List<Transform>();
      this.shuffled = new List<Transform>();
      this.selected = new List<Transform>();
      foreach (Transform transform in this.parent)
      {
        if ((UnityEngine.Object) transform.GetComponentInChildren<SimpleBreakable>(true) != (UnityEngine.Object) null)
          this.shuffled.Add(transform);
      }
      if ((double) this.shuffled.Count < (double) this.minMaxWeakPoints.x)
        Debug.LogError((object) $"All childs of {this.parent.name} without simple breakables have been ignored, which leaves it without enough for the specified minimum weakpoint count!");
      this.allWeakPointOptions.AddRange((IEnumerable<Transform>) this.shuffled);
      this.shuffled.Shuffle<Transform>();
      this.initialized = true;
    }

    public List<Transform> GetMinRandomlyPickedPoints()
    {
      if (!this.initialized)
        this.Init();
      int num = Mathf.Min((int) this.minMaxWeakPoints.x, this.shuffled.Count);
      for (int index = 0; index < num; ++index)
      {
        this.selected.Add(this.shuffled[0]);
        this.shuffled.RemoveAt(0);
      }
      return this.selected;
    }

    public Transform GetNewRandPoint()
    {
      if (!this.initialized)
        this.Init();
      Transform newRandPoint = this.shuffled[0];
      this.selected.Add(newRandPoint);
      this.shuffled.Remove(newRandPoint);
      return newRandPoint;
    }

    public Transform RandRemove()
    {
      if (!this.initialized)
        this.Init();
      Transform transform = this.selected[UnityEngine.Random.Range(0, this.selected.Count)];
      this.shuffled.Add(transform);
      this.selected.Remove(transform);
      return transform;
    }

    public void RemoveAllNonSelected()
    {
      if (!this.initialized)
        this.Init();
      for (int index = this.allWeakPointOptions.Count - 1; index >= 0; --index)
      {
        if (!this.selected.Contains(this.allWeakPointOptions[index]))
          this.allWeakPointOptions[index].gameObject.SetActive(false);
      }
    }
  }
}
