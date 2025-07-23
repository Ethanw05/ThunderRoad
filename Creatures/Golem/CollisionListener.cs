// Decompiled with JetBrains decompiler
// Type: ThunderRoad.CollisionListener
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using UnityEngine;

#nullable disable
namespace ThunderRoad;

public class CollisionListener : MonoBehaviour
{
  public event CollisionListener.CollisionEvent OnCollisionEnterEvent;

  public event CollisionListener.CollisionEvent OnCollisionExitEvent;

  private void OnCollisionEnter(UnityEngine.Collision other)
  {
    CollisionListener.CollisionEvent collisionEnterEvent = this.OnCollisionEnterEvent;
    if (collisionEnterEvent == null)
      return;
    collisionEnterEvent(other);
  }

  private void OnCollisionExit(UnityEngine.Collision other)
  {
    CollisionListener.CollisionEvent collisionExitEvent = this.OnCollisionExitEvent;
    if (collisionExitEvent == null)
      return;
    collisionExitEvent(other);
  }

  public delegate void CollisionEvent(UnityEngine.Collision other);
}
