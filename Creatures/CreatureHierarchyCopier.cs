// Decompiled with JetBrains decompiler
// Type: ThunderRoad.CreatureHierarchyCopier
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class CreatureHierarchyCopier : MonoBehaviour
{
  public Transform copyFrom;
  public Transform fromRig;
  public Transform copyTo;
  public Transform toRig;
  public Transform ragdollParts;
  public bool copyMeshParts = true;
  public bool copyReveal = true;
  public bool reassignPartBones = true;
  public bool alignParts = true;

  private void Awake()
  {
    if (!Application.isPlaying)
      return;
    Object.Destroy((Object) this);
  }

  private void Copy()
  {
    foreach (Transform transform1 in this.copyFrom)
    {
      if (!((Object) transform1 == (Object) this.fromRig))
      {
        MeshPart component1 = transform1.GetComponent<MeshPart>();
        Transform transform2 = this.copyTo.Find(transform1.name);
        if ((Object) transform2 == (Object) null)
        {
          transform2 = Object.Instantiate<GameObject>(transform1.gameObject).transform;
          transform2.name = transform1.name;
          for (int index = transform2.childCount - 1; index >= 0; --index)
            Object.DestroyImmediate((Object) transform2.GetChild(index).gameObject);
        }
        transform2.parent = this.copyTo;
        if (this.copyMeshParts && (bool) (Object) component1)
        {
          MeshPart component2;
          if (!transform2.TryGetComponent<MeshPart>(out component2))
            component2 = transform2.gameObject.AddComponent<MeshPart>();
          component2.skinnedMeshRenderer = this.copyTo.FindChildRecursive(component1.skinnedMeshRenderer.name).GetComponent<SkinnedMeshRenderer>();
          component2.defaultPhysicMaterial = component1.defaultPhysicMaterial;
          component2.idMap = component1.idMap;
        }
        foreach (Transform transform3 in transform1)
        {
          RevealDecal component3 = transform3.GetComponent<RevealDecal>();
          Transform transform4 = this.copyTo.Find(transform3.name);
          transform4.parent = transform2;
          if (this.copyReveal && (bool) (Object) component3)
          {
            RevealDecal component4;
            if (!transform4.TryGetComponent<RevealDecal>(out component4))
              component4 = transform4.gameObject.AddComponent<RevealDecal>();
            component4.type = component3.type;
            component4.maskHeight = component3.maskHeight;
            component4.maskWidth = component3.maskWidth;
          }
        }
      }
    }
    if (!this.reassignPartBones)
      return;
    foreach (Component ragdollPart in this.ragdollParts)
    {
      RagdollPart component = ragdollPart.GetComponent<RagdollPart>();
      component.meshBone = this.toRig.FindChildRecursive(component.meshBone.name);
      if (this.alignParts)
      {
        component.transform.position = component.meshBone.position;
        component.transform.rotation = component.meshBone.rotation;
      }
      for (int index = 0; index < component.linkedMeshBones.Length; ++index)
        component.linkedMeshBones[index] = this.toRig.FindChildRecursive(component.linkedMeshBones[index].name);
    }
  }
}
