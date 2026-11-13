using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovOrg : MonoBehaviour
{
    private KotakSetting Kotak;
    private Vector2Int Posisipetak;

    public void Spawn(KotakSetting setting, Vector2Int posisi)
    {
        Kotak = setting;
        Posisipetak = posisi;

        transform.position = Kotak.CelltoWorld(Posisipetak);
    }

    private void Update()
    {
        Vector2Int targetPetak = Posisipetak;
        bool bergerak = false;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            targetPetak.y += 1;
            bergerak = true;
        }
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            targetPetak.y -= 1;
            bergerak = true;
        }
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            targetPetak.x += 1;
            bergerak = true;
        }
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            targetPetak.x -= 1;
            bergerak = true;
        }
        if (bergerak)
        {
            KotakSetting.DataUbin data = Kotak.GetCellData(targetPetak);
            {
                if (data != null && data.aman)
                {
                    Posisipetak = targetPetak;
                    transform.position = Kotak.CelltoWorld(Posisipetak);
                }
            }
            
        }
    }
    
   
}
