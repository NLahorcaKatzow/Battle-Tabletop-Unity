using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    public int generatedId;
    public PieceDataClass pieceData;
    public Vector2Int position;
    public GameObject selectedHalo;

    public bool isSelected = false;
    public bool isMoving = false;
    public bool isAttacking = false;
    public bool isEnemy = false;


    private void Awake()
    {
        TabletopController.Instance.OnPieceSelected += Selected;
    }

    private void OnDestroy()
    {
        TabletopController.Instance.OnPieceSelected -= Selected;
    }

    public void SetPieceData(PieceDataClass data)
    {
        generatedId = gameObject.GetInstanceID();
        pieceData = data;
    }

    public void SetPosition(Vector2Int pos)
    {
        position = pos;
    }

    public Vector2Int GetPosition()
    {
        return position;
    }

    public void MoveToPosition(Vector3 newPosition)
    {
        transform.DOMove(newPosition, 0.5f).SetEase(Ease.InOutSine).OnComplete(() => {
            Deselect();
        });
    }

    public void Selected(int id)
    {
        if (id != generatedId)
        {
            Deselect();
            return;
        }
        isSelected = true;
        Debug.Log($"Selected piece: {pieceData.name}");
        selectedHalo.SetActive(true);
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f);
        TabletopController.Instance.ShowPieceInfo(generatedId);
        TabletopController.Instance.ShowPieceActions(generatedId);
    }

    public void Deselect()
    {
        if (isSelected)
        {
            Debug.Log($"Unselected piece: {pieceData.name}");
            //TODO: Deseleccionar la pieza si estaba seleccionada
        }
        isSelected = false;
        selectedHalo.SetActive(false);
    }
    
    public void SetMovementBool(bool value)
    {
        isMoving = value;
    }
    
    public void SetAttackBool(bool value)
    {
        isAttacking = value;
    }
    public void SetEnemyBool(bool value)
    {
        isEnemy = value;
    }
    
    public bool GetMovementBool()
    {
        return isMoving;
    }
    
    public bool GetAttackBool()
    {
        return isAttacking;
    }
    
    public bool GetEnemyBool()
    {
        return isEnemy;
    }
}
