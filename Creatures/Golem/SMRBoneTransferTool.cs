// Decompiled with JetBrains decompiler
// Type: ThunderRoad.SMRBoneTransferTool
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class SMRBoneTransferTool : MonoBehaviour
{
  public GameObject sourceHierarchy;
  public GameObject targetHierarchy;

  public void ReBone()
  {
    Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
    foreach (SkinnedMeshRenderer componentsInChild in this.sourceHierarchy.GetComponentsInChildren<SkinnedMeshRenderer>())
    {
      foreach (Transform bone in componentsInChild.bones)
        dictionary[bone.name] = bone;
    }
    foreach (SkinnedMeshRenderer componentsInChild in this.targetHierarchy.GetComponentsInChildren<SkinnedMeshRenderer>())
    {
      Transform transform;
      if (dictionary.TryGetValue(componentsInChild.rootBone.name, out transform))
        componentsInChild.rootBone = transform;
      else
        Debug.LogError((object) ("failed to get bone: " + componentsInChild.rootBone.name));
      Transform[] bones = componentsInChild.bones;
      for (int index = 0; index < bones.Length; ++index)
      {
        string name = bones[index].name;
        if (!dictionary.TryGetValue(name, out bones[index]))
          Debug.LogError((object) ("failed to get bone: " + name));
      }
      componentsInChild.bones = bones;
    }
  }
}
