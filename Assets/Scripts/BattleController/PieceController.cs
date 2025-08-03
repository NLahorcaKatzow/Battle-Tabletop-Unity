using DG.Tweening;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PieceController : MonoBehaviour
{
    public int generatedId;
    public PieceDataClass pieceData;
    public Vector2Int position;

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



    public void Selected(int id)
    {
        if (id != generatedId)
        {
            Deselect();
            return;
        }
        Debug.Log($"Selected piece: {pieceData.name}");
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f);
        TabletopController.Instance.ShowPieceInfo(generatedId);
        TabletopController.Instance.ShowPieceActions(generatedId);
    }

    public void Deselect()
    {
        Debug.Log($"Unselected piece: {pieceData.name}");
    }
}
