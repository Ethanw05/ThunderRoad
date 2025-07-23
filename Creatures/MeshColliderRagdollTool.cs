// Decompiled with JetBrains decompiler
// Type: ThunderRoad.MeshColliderRagdollTool
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class MeshColliderRagdollTool : MonoBehaviour
{
  public PhysicMaterial defaultMaterial;

  public void SetColliders()
  {
    SkinnedMeshRenderer[] componentsInChildren = this.GetComponentsInChildren<SkinnedMeshRenderer>();
    foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
    {
      MeshCollider component;
      if (!skinnedMeshRenderer.TryGetComponent<MeshCollider>(out component))
        component = skinnedMeshRenderer.gameObject.AddComponent<MeshCollider>();
      component.sharedMesh = skinnedMeshRenderer.sharedMesh;
      component.convex = true;
      component.material = this.defaultMaterial;
    }
    for (int index = componentsInChildren.Length - 1; index >= 0; --index)
      Object.Destroy((Object) componentsInChildren[index]);
  }
}
