using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
[RequireComponent(typeof(Collider))]
public class PickUpBase : MonoBehaviour
{

    [SerializeField] private int id;
    [SerializeField] private Collider col;

    public void Awake()
    {
        col = GetComponent<Collider>();
        col.isTrigger = true; // funciona con trigger por defecto
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
        Debug.Log("OnPickUp");
        
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
    
    
    // Método utilitario para que un tile/pickup detecte la entrada de una pieza
    void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Piece"))
        {
            OnPickUp(other.gameObject.GetComponent<PieceController>());
        }
    }
}
