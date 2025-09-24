// HealthPickup.cs
using UnityEngine;
using System.Linq;

public class HealthPickUp : PickUpBase
{
    [SerializeField] int healthAmount = 20;

    public void SetPickUpData(HealthPickUpClass pickUpData)
    {
        healthAmount = pickUpData.healAmount;
        id = pickUpData.id;
        namePickUp = pickUpData.name;
        description = pickUpData.description;
        
        // Actualizar rareza y configurar animación automáticamente
        UpdateRarity(pickUpData.rarity);
    }

    public override void ApplyEffect(PieceController picker)
    {
        
        var dmg = picker.GetComponentsInParent<MonoBehaviour>().OfType<IDamageable>().FirstOrDefault();
        if (dmg != null)
        {
            dmg.Heal(healthAmount);
            Debug.Log($"{picker.name} recogió pocion de curacion + {healthAmount} HP");
            return;
        }
    }
}
