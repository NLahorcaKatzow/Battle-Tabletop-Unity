// HealthPickup.cs
using UnityEngine;
using System.Linq;

public class HealthPickup : PickUpBase
{
    [SerializeField] int healthAmount = 20;

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
