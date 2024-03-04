using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;    
    public bool player;

    public void CreateTank(int index = -1)
    {
        if (!Preferences.Editing)
        {                    
            // Create the spawned object
            GameObject creation;
            if (prefab.GetComponent<PlayerInput>() != null)
            {
                InputDevice chosenInputDevice;
                if (Gamepad.all.Count > index)
                {
                    chosenInputDevice = Gamepad.all[index];                    
                }
                else
                {
                    chosenInputDevice = Keyboard.current;
                }                
                creation = PlayerInput.Instantiate(prefab, -1, null, -1, chosenInputDevice).gameObject;                
            }
            else
            {
                creation = Instantiate(prefab);
            }
            
            // Position it
            creation.transform.parent = transform;
            Vector3 pos = creation.transform.localPosition;
            pos.x = 0;
            pos.y = 0;
            creation.transform.localPosition = pos;            
            creation.transform.parent = Arena.currentArena.transform;

            // Update the block
            Block block = transform.parent.GetComponent<Block>();
            block.tile.surfaceObject = Tile.SurfaceObject.none;
            block.Reconfigure();

            // If this is a player tank, inform the player tank of its index and set its color
            if (creation.TryGetComponent(out PlayerTank playerTank))
            {
                playerTank.playerIndex = index;

                if (Preferences.Multiplayer)
                {
                    playerTank.tank.SetAccentMaterial(GameManager.current.multiplayerMaterials[index]);
                }
                else
                {
                    playerTank.tank.SetAccentMaterial(GameManager.current.singlePlayerMaterial);
                }
            }

            // Destroy the spawner
            Destroy(gameObject);
        }  
    }
}
