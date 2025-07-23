// Decompiled with JetBrains decompiler
// Type: ThunderRoad.WeakPointController
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

[RequireComponent(typeof (RagdollPart))]
public class WeakPointController : MonoBehaviour
{
  public int priority;
  public List<Transform> weakPoints;
  public Vector2Int minMaxActive = new Vector2Int(1, 1);
  [NonSerialized]
  public int added;
  public List<Transform> selected;
  private List<Transform> shuffled;

  public static void SelectWeakPoints(
    Ragdoll ragdoll,
    int targetCount,
    Action<List<Item>> callback,
    bool ignoreMinimumPerController = false)
  {
    WeakPointController[] componentsInChildren = ragdoll.GetComponentsInChildren<WeakPointController>();
    ((IEnumerable<WeakPointController>) componentsInChildren).OrderByDescending<WeakPointController, int>((Func<WeakPointController, int>) (wpc => wpc.priority));
    List<Transform> transformList = new List<Transform>();
    for (int index = 0; index < componentsInChildren.Length; ++index)
    {
      WeakPointController weakPointController = componentsInChildren[index];
      transformList.AddRange((IEnumerable<Transform>) weakPointController.GetMinRandomlyPickedPoints());
    }
    if (transformList.Count < targetCount)
    {
      while (transformList.Count < targetCount)
      {
        WeakPointController result;
        if (!((IEnumerable<WeakPointController>) componentsInChildren).WeightedFilteredSelectInPlace<WeakPointController>((Func<WeakPointController, bool>) (wpc => wpc.selected.Count < wpc.minMaxActive.y), (Func<WeakPointController, float>) (wpc => (float) wpc.priority), out result))
        {
          Debug.LogError((object) "Can't add more weak points without violating weak point controller rules!");
          break;
        }
        transformList.Add(result.GetNewRandPoint());
      }
    }
    if (ignoreMinimumPerController && transformList.Count > targetCount)
    {
      while (transformList.Count > targetCount)
      {
        WeakPointController result;
        if (!((IEnumerable<WeakPointController>) componentsInChildren).WeightedFilteredSelectInPlace<WeakPointController>((Func<WeakPointController, bool>) (wpc => wpc.selected.Count > 1), (Func<WeakPointController, float>) (wpc => (float) wpc.selected.Count), out result))
        {
          Debug.LogError((object) "Couldn't reduce number of weak points without violating weak point controller rules!");
          break;
        }
        transformList.Remove(result.RandRemove());
      }
    }
    List<Item> objList = new List<Item>();
    foreach (Component component in transformList)
    {
      Item componentInChildren = component.GetComponentInChildren<Item>();
      if ((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null || (UnityEngine.Object) componentInChildren.breakable == (UnityEngine.Object) null)
        Debug.LogError((object) "Something went wrong, a weak point is not configured with a breakable item!");
      else
        objList.Add(componentInChildren);
    }
    for (int index = 0; index < componentsInChildren.Length; ++index)
      componentsInChildren[index].RemoveAllNonSelected();
    if (callback == null)
      return;
    callback(objList);
  }

  public List<Transform> GetMinRandomlyPickedPoints()
  {
    if (this.shuffled.IsNullOrEmpty())
      this.Init();
    for (int index = 0; index < Mathf.Min(this.minMaxActive.x, this.shuffled.Count); ++index)
    {
      this.selected.Add(this.shuffled[0]);
      this.shuffled.RemoveAt(0);
    }
    return this.selected;
  }

  public Transform GetNewRandPoint()
  {
    Transform newRandPoint = this.shuffled[0];
    this.selected.Add(newRandPoint);
    this.shuffled.Remove(newRandPoint);
    return newRandPoint;
  }

  public Transform RandRemove()
  {
    Transform transform = this.selected[UnityEngine.Random.Range(0, this.selected.Count)];
    this.shuffled.Add(transform);
    this.selected.Remove(transform);
    return transform;
  }

  public void RemoveAllNonSelected()
  {
    for (int index = this.weakPoints.Count - 1; index >= 0; --index)
    {
      if (!this.selected.Contains(this.weakPoints[index]))
      {
        foreach (ThunderEntity componentsInChild in this.weakPoints[index].GetComponentsInChildren<Item>())
          componentsInChild.Despawn();
        UnityEngine.Object.Destroy((UnityEngine.Object) this.weakPoints[index].gameObject);
      }
    }
  }

  private void Start() => this.Init();

  private void Init()
  {
    this.selected = new List<Transform>();
    this.shuffled = new List<Transform>();
    this.shuffled.AddRange((IEnumerable<Transform>) this.weakPoints);
    this.shuffled.Shuffle<Transform>();
    for (int index = this.shuffled.Count - 1; index >= 0; --index)
    {
      Item componentInChildren = this.shuffled[index].GetComponentInChildren<Item>();
      if (componentInChildren == null || !((UnityEngine.Object) componentInChildren.breakable != (UnityEngine.Object) null))
        this.shuffled.RemoveAt(index);
    }
    if (this.shuffled.Count >= this.minMaxActive.x)
      return;
    Debug.LogError((object) (this.name + ": Non-breakable weak points have been discarded! There are not enough weak points to meet this controller's minimum!"));
  }

  public void AddAllChildBreakableItemsAsWeakPoints(int includeParents = 0)
  {
    foreach (ThunderBehaviour componentsInChild in this.GetComponentsInChildren<Breakable>())
    {
      Transform transform = componentsInChild.transform;
      if (includeParents > 0)
      {
        for (int index = 0; index < includeParents; ++index)
          transform = transform.parent;
      }
      this.weakPoints.Add(transform);
    }
  }

  public void AttachWeakPoints()
  {
    Rigidbody component = this.GetComponent<Rigidbody>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "No RB on controller!");
    }
    else
    {
      foreach (ThunderBehaviour componentsInChild in this.GetComponentsInChildren<Item>())
        componentsInChild.gameObject.AddComponent<FixedJoint>().connectedBody = component;
    }
  }
}
