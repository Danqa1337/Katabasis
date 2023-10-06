using UnityEngine;

public class AnatomyPartSlots : MonoBehaviour
{
    public EquipSlot[] equipSlots;
    public Canvas canvas;
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }
}
