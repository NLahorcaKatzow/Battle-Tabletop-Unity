using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using DamageNumbersPro;

public class PieceController : MonoBehaviour
{
    public int generatedId;
    public PieceDataClass pieceData;
    public Vector2Int position;
    public GameObject selectedHalo;
    public TimedSliderUI damageSlider;
    public DamageNumber damageNumberPrefab;

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

    public void SetPieceData(PieceDataClass data, bool isEnemy = false)
    {
        generatedId = gameObject.GetInstanceID();
        pieceData = data;
        this.isEnemy = isEnemy;
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
        transform.DOMove(newPosition, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
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
        if (!isEnemy) TabletopController.Instance.ShowPieceActions(generatedId);
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


    public void ApplyDamage(int damage)
    {
        var sliderDamage = Instantiate(damageSlider, UIController.GetTransform()).GetComponent<TimedSliderUI>();
        sliderDamage.Spawn(Camera.main.WorldToScreenPoint(transform.position), 0.5f, (pieceData.health)/pieceData.maxHealth, (pieceData.health - damage)/pieceData.maxHealth, Ease.OutSine);
        damageNumberPrefab.Spawn(Camera.main.WorldToScreenPoint(transform.position), damage);
        pieceData.health -= damage;
        if (pieceData.health <= 0)
        {
            DestroyPiece();
        }

    }

    public void DestroyPiece()
    {
        Debug.Log($"Destroying piece: {pieceData.name}");
        transform.DOScale(0, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            TabletopController.Instance.DestroyPiece(generatedId);
        });
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
