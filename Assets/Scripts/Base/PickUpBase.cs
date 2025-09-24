using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using Unity.VisualScripting;
[RequireComponent(typeof(Collider))]
public class PickUpBase : MonoBehaviour
{

    [SerializeField] protected int id;
    [SerializeField] protected PickUpRarity rarity;
    [SerializeField] protected string namePickUp;
    [SerializeField] protected string description;
    [SerializeField] protected Collider col;
    [SerializeField] protected FloatingAnimation floatingAnimation;

    public void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true; // funciona con trigger por defecto
        InitializeFloatingAnimation();
    }
    
    protected virtual void Start()
    {
        // Configurar animación basada en la rareza si existe
        if (floatingAnimation != null)
        {
            ConfigureFloatingByRarity(rarity);
        }
    }
    
    private void InitializeFloatingAnimation()
    {
        // Inicializar la animación flotante si existe
        if (floatingAnimation == null)
        {
        this.AddComponent<FloatingAnimation>();
            floatingAnimation = GetComponent<FloatingAnimation>();
        }
    }
    
    protected void ConfigureFloatingByRarity(PickUpRarity rarity)
    {
        if (floatingAnimation == null) return;
        
        switch (rarity)
        {
            case PickUpRarity.Common:
                floatingAnimation.SetFloatDistance(0.1f);
                floatingAnimation.SetFloatDuration(1f);
                break;
            case PickUpRarity.Rare:
                floatingAnimation.SetFloatDistance(0.1f);
                floatingAnimation.SetFloatDuration(1f);
                break;
            case PickUpRarity.Epic:
                floatingAnimation.SetFloatDistance(0.15f);
                floatingAnimation.SetFloatDuration(1f);
                break;
            case PickUpRarity.Legendary:
                floatingAnimation.SetFloatDistance(0.2f);
                floatingAnimation.SetFloatDuration(1f);
                break;
        }
    }
    
    protected void UpdateRarity(PickUpRarity newRarity)
    {
        rarity = newRarity;
        ConfigureFloatingByRarity(rarity);
    }
    public void PickUp()
    {
        Debug.Log("PickUp");
    }
    
    public virtual bool CanPickUp()
    {
        Debug.Log("CanPickUp");
        return true;
    }
    
    public void OnPickUp(PieceController pieceController)
    {
        if(!CanPickUp()) return;
        Debug.Log("OnPickUp: " + pieceController.name);
        
        ApplyEffect(pieceController);
        
        transform.DOScale(0, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
    
    public virtual void ApplyEffect(PieceController pieceController)
    {
        Debug.Log("ApplyEffect");
    }
    
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Piece"))
        {   Debug.Log("OnTriggerEnter: " + other.gameObject.name);
            OnPickUp(other.gameObject.GetComponent<PieceController>());
        }
    }
}
