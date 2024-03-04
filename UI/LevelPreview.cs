using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LevelPreview : MonoBehaviour
{    
    public void GeneratePreview(Level level)
    {        
        if (TryGetComponent(out Image image))
        {             
            int n = level.Width;

            // Checking dimensions are n x n

            if (level.Width != n) { Debug.LogError("Trying to create preview of level with incorrect width"); }
            if (level.Height != n) { Debug.LogError("Trying to create preview of level with incorrect height"); }

            // Creating the texture

            Texture2D texture = new Texture2D(n, n, TextureFormat.ARGB32, false);

            // Reading the walls

            Color transparent = new Color(0, 0, 0, 0);

            for (int i = 0; i < n; i++)
            {                
                for (int j = 0; j < n; j++)
                {
                    texture.SetPixel(i, j, level.tileMatrix[n-j-1][i].closed ? transparent : Color.white);                
                }
            }        

            // Set filter mode to point

            texture.filterMode = FilterMode.Point;

            // Applying changes made above

            texture.Apply();       

            // Update the image sprite

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);        
            image.sprite = sprite;  
        }
        else
        {
            Debug.LogError("No image component on level preview");
        }      
    }
}

// byte[] bytes = texture.EncodeToPNG();         
// File.WriteAllBytes(Application.dataPath + "/Levels/Multiplayer Images/" + levelName + ".png", bytes);
