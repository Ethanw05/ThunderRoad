// Decompiled with JetBrains decompiler
// Type: ThunderRoad.AIFireable
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace ThunderRoad;

[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Exclude/AIFireball.html")]
public class AIFireable : MonoBehaviour
{
  public Transform aimTransform;
  [Header("Fireable events")]
  public UnityEvent aimEvent;
  public UnityEvent fireEvent;
  public UnityEvent reloadEvent;

  public Item item { get; protected set; }
}
