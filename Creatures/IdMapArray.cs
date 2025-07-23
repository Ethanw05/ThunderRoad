// Decompiled with JetBrains decompiler
// Type: ThunderRoad.IdMapArray
// Assembly: ThunderRoad, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 440ED6B9-B010-414D-A078-366B18988020
// Assembly location: A:\BAS DEV\ThunderRoad.dll
// XML documentation location: A:\BAS DEV\ThunderRoad.xml

using System;
using UnityEngine;

#nullable disable
namespace ThunderRoad;

[PreferBinarySerialization]
public class IdMapArray : ScriptableObject
{
  [Tooltip("The path to the ID map texture.")]
  public string idMapPath;
  [Tooltip("The factor to scale the ID map down by.")]
  public int scale = 2;
  public NibbleArray nibbleArray;
  public int originalWidth;
  public int originalHeight;
  public int width;
  public int height;
  private int estimatedIDMapSize;
  private int estimatedIDMapArraySize;
  public Texture2D debugTexture;

  public int GetIdAtUV(float ux, float uy)
  {
    if (this.nibbleArray == null || this.nibbleArray.Length == 0)
    {
      Debug.LogError((object) "ID map array is null.");
      return -1;
    }
    int num = (int) ((double) ux * (double) this.width);
    return (int) this.nibbleArray[(int) ((double) uy * (double) this.height) * this.width + num];
  }

  public void ConvertArrayToTexture()
  {
    if (this.nibbleArray == null || this.nibbleArray.Length == 0)
    {
      Debug.LogError((object) "ID map array is null.");
    }
    else
    {
      Catalog.EditorLoadAllJson();
      Texture2D texture2D = new Texture2D(this.width, this.height);
      for (int y = 0; y < this.height; ++y)
      {
        for (int x = 0; x < this.width; ++x)
        {
          try
          {
            byte nibble = this.nibbleArray[y * this.width + x];
            UnityEngine.Color idMapColor = Catalog.gameData.GetIDMapColor((int) nibble);
            texture2D.SetPixel(x, y, idMapColor);
          }
          catch (Exception ex)
          {
            Debug.LogError((object) $"Error setting pixel at {x}, {y}: {ex.Message}");
          }
        }
      }
      texture2D.Apply();
      this.debugTexture = texture2D;
      this.estimatedIDMapSize = this.originalWidth * this.originalHeight * 3;
      this.estimatedIDMapArraySize = this.nibbleArray.Length / 2;
    }
  }

  private void OnValidate() => this.debugTexture = (Texture2D) null;
}
