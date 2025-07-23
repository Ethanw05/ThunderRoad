// Decompiled with JetBrains decompiler
// Type: ThunderRoad.BoneColliderToRagdollParts
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class BoneColliderToRagdollParts : MonoBehaviour
{
  public Transform rig;
  public bool addParts = true;
  public List<Transform> partsWithSubBones = new List<Transform>();

  public void Transfer()
  {
    Dictionary<Transform, List<Collider>> dictionary1 = new Dictionary<Transform, List<Collider>>();
    List<Collider> colliderList1 = new List<Collider>();
    colliderList1.AddRange((IEnumerable<Collider>) this.rig.GetComponentsInChildren<Collider>());
    foreach (Transform partsWithSubBone in this.partsWithSubBones)
    {
      Collider[] componentsInChildren = partsWithSubBone.GetComponentsInChildren<Collider>();
      dictionary1.Add(partsWithSubBone, new List<Collider>());
      foreach (Collider collider in componentsInChildren)
      {
        dictionary1[partsWithSubBone].Add(collider);
        colliderList1.Remove(collider);
      }
    }
    foreach (Collider collider in colliderList1)
    {
      Transform parent = collider.transform.parent;
      List<Collider> colliderList2;
      if (!dictionary1.TryGetValue(parent, out colliderList2))
      {
        colliderList2 = new List<Collider>();
        dictionary1.Add(parent, colliderList2);
      }
      colliderList2.Add(collider);
    }
    Dictionary<Transform, RagdollPart> dictionary2 = new Dictionary<Transform, RagdollPart>();
    foreach (KeyValuePair<Transform, List<Collider>> keyValuePair in dictionary1)
    {
      Transform orAddTransform = this.transform.FindOrAddTransform(keyValuePair.Key.name, keyValuePair.Key.position, new Quaternion?(keyValuePair.Key.rotation), new UnityEngine.Vector3?(keyValuePair.Key.lossyScale));
      foreach (Component component in keyValuePair.Value)
        component.transform.parent = orAddTransform;
      if (this.addParts)
      {
        RagdollPart ragdollPart = orAddTransform.gameObject.AddComponent<RagdollPart>();
        ragdollPart.meshBone = keyValuePair.Key;
        dictionary2[keyValuePair.Key] = ragdollPart;
      }
    }
    foreach (KeyValuePair<Transform, RagdollPart> keyValuePair in dictionary2)
    {
      RagdollPart ragdollPart1 = keyValuePair.Value;
      for (Transform parent = keyValuePair.Key.parent; (Object) parent != (Object) null; parent = parent.parent)
      {
        RagdollPart ragdollPart2;
        if (dictionary2.TryGetValue(parent, out ragdollPart2))
        {
          ragdollPart1.parentPart = ragdollPart2;
          break;
        }
      }
    }
  }
}
