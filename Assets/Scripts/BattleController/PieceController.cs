using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using DamageNumbersPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class PieceController : MonoBehaviour, IDamageable
{
    public int generatedId;
    public PieceDataClass pieceData;
    public Vector2Int position;
    public GameObject selectedHalo;
    public Slider damageSlider;
    public DamageNumber damageNumberPrefab;
    public GameObject render;
    public int currentHealth;

    public bool isSelected = false;
    public bool isMoving = false;
    public bool isAttacking = false;
    public bool isEnemy = false;

    public bool IsAlive => currentHealth > 0;


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
        currentHealth = pieceData.maxHealth;

        render = RenderController.Instance.InstantiatePieceByMovementType(data.movementType, transform.position, transform);
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
        Debug.Log("PieceController: Deselecting piece: " + pieceData.name);
        isSelected = false;
        selectedHalo.SetActive(false);
    }


    public void TakeDamage(int amount)
    {
        if (amount == 0)
        {
            Debug.Log("PieceController: Damage is 0, skipping");
            return;
        }
        Debug.Log("PieceController: Health: " + currentHealth + " Max Health: " + pieceData.maxHealth);
        Debug.Log("PieceController: Applying damage: " + amount + " relative damage: " + (float)(currentHealth - amount) / (float)pieceData.maxHealth);
        damageSlider.transform.parent.gameObject.SetActive(true);
        damageNumberPrefab.Spawn(Camera.main.WorldToScreenPoint(transform.position), amount);
        currentHealth -= amount;
        damageSlider.value = (float)currentHealth / (float)pieceData.maxHealth;

        var rend = render.GetComponentInChildren<Renderer>();
        TweenUtils.Flash(rend, "_Mid_Color", Color.red, 0.3f);


        render.transform.DOShakePosition(0.3f, 0.3f, 10, 90, false, false);
        if (currentHealth <= 0)
        {
            damageSlider.value = 0;
            DestroyPiece(() =>
            {
                TabletopController.Instance.RecalculateAllPiecesLife();
            });
        }

    }

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            Debug.Log("PieceController: Heal amount is 0 or negative, skipping");
            return;
        }

        if (!IsAlive)
        {
            Debug.Log("PieceController: Cannot heal dead piece");
            return;
        }

        Debug.Log("PieceController: Healing for: " + amount);
        currentHealth += amount;
        
        // Cap health at max health
        if (currentHealth > pieceData.maxHealth)
        {
            currentHealth = pieceData.maxHealth;
        }
        
        damageSlider.transform.parent.gameObject.SetActive(true);
        damageSlider.value = (float)currentHealth / (float)pieceData.maxHealth;

        var rend = render.GetComponentInChildren<Renderer>();
        TweenUtils.Flash(rend, "_Mid_Color", Color.green, 0.3f);
    }

    public void DestroyPiece(UnityAction onComplete = null)
    {

        Debug.Log($"Destroying piece: {pieceData.name}");
        transform.DOScale(0, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            currentHealth = 0;
            onComplete?.Invoke();
        });
    }

    public void RevivePiece()
    {
        currentHealth = pieceData.maxHealth;
        damageSlider.value = 1.0f;
        
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
