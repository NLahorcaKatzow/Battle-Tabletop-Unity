using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using DamageNumbersPro;
using UnityEngine.Events;

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
    public bool isDead = false;


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

    public void MoveToPosition(Vector2Int newPosition, UnityAction onComplete = null, float easeTime = 0.5f)
    {
        transform.DOMove(TabletopController.Instance.GetGrid()
        .GetCellCenterWorld(new Vector3Int(newPosition.x, 0, newPosition.y)), easeTime)
        .SetEase(Ease.InOutSine).OnComplete(() =>
        {
            SetPosition(newPosition);
            SetMovementBool(true);
            Deselect();
            onComplete?.Invoke();
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
        sliderDamage.Spawn(Camera.main.WorldToScreenPoint(transform.position), 0.5f, (pieceData.health) / pieceData.maxHealth, (pieceData.health - damage) / pieceData.maxHealth, Ease.OutSine);
        damageNumberPrefab.Spawn(Camera.main.WorldToScreenPoint(transform.position), damage);
        pieceData.health -= damage;
        GetComponent<Material>().DOColor(Color.red, 0.3f).SetLoops(1, LoopType.Yoyo);
        transform.DOShakePosition(0.3f, 0.3f, 10, 90, false, false);
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
            isDead = true;
        });
    }

    public void RevivePiece()
    {
        isDead = false;
        transform.DOScale(1, 0.5f).SetEase(Ease.InOutSine);
    }

    public void SetPieceInactive()
    {
        isMoving = true;
        isAttacking = true;
    }

    public void SetPieceActive()
    {
        isMoving = false;
        isAttacking = false;
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
