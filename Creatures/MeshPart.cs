// Decompiled with JetBrains decompiler
// Type: ThunderRoad.MeshPart
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using ThunderRoad.Manikin;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[DisallowMultipleComponent]
[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/MeshPart.html")]
public class MeshPart : MonoBehaviour
{
  [Tooltip("References the Skinned Mesh Renderer that creates the collider for the part.\n\nIt is recommended, if your parts have LODs, to use the lowest LOD for this, as it will be more performant.")]
  public SkinnedMeshRenderer skinnedMeshRenderer;
  [Tooltip("This defines the default physics material if your part does not have an ID mesh. If it does have an ID mesh, it is recommended to set this to \"Flesh\".")]
  public PhysicMaterial defaultPhysicMaterial;
  [Tooltip("The ID map texture. This is used to determine the physics material used for armor detection for the part.")]
  public Texture2D idMap;
  [Tooltip("For better performance, it is recommended to use the convert button to set the ID map to an ID Map array, for better performance.")]
  public IdMapArray idMapArray;
  [Tooltip("The factor to scale the ID map down by.")]
  public int scale = 4;
  [NonSerialized]
  public ManikinPart manikinPart;
  [NonSerialized]
  public int defaultPhysicMaterialHash;

  private void OnValidate()
  {
    this.scale = this.scale < 1 ? 1 : Mathf.ClosestPowerOfTwo(this.scale);
  }

  private void Start()
  {
    this.manikinPart = this.GetComponentInParent<ManikinPart>();
    if (!(bool) (UnityEngine.Object) this.defaultPhysicMaterial)
      return;
    this.defaultPhysicMaterialHash = Animator.StringToHash(this.defaultPhysicMaterial.name + " (Instance)");
  }
}
