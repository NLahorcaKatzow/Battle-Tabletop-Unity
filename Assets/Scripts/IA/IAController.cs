using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class IAController : MonoBehaviour
{
    public enum Side { White, Black }

    public static IAController Instance;
    void Awake() { Instance = this; }

    // ===================== RENDERS DE OPCIONES =====================
    [Header("Prefabs · Renders de IA")]
    [Tooltip("Prefab para renderizar CASILLAS DE MOVIMIENTO posibles")]
    public GameObject moveRender;     // <- pediste 'MoveRender'
    [Tooltip("Prefab para renderizar CASILLAS DE ATAQUE posibles")]
    public GameObject attackRender;   // <- pediste 'attackRender'

    [Tooltip("Opcional: padre para organizar los renders en el hierarchy")]
    public Transform rendersParent;

    [Tooltip("Offset vertical si usás plano XZ (elevar los prefabs)")]
    public float renderYOffset = 0.02f;

    // Listas para almacenar instancias y poder borrarlas
    public readonly List<GameObject> moveRenderInstances = new List<GameObject>();
    public readonly List<GameObject> attackRenderInstances = new List<GameObject>();

    void ClearAllRenderHints()
    {
        for (int i = 0; i < moveRenderInstances.Count; i++)
            if (moveRenderInstances[i]) Destroy(moveRenderInstances[i]);
        for (int i = 0; i < attackRenderInstances.Count; i++)
            if (attackRenderInstances[i]) Destroy(attackRenderInstances[i]);

        moveRenderInstances.Clear();
        attackRenderInstances.Clear();
    }

    void SpawnRender(GameObject prefab, Vector3 worldPos, List<GameObject> bucket)
    {
        if (!prefab) return;
        if (planeXZ) worldPos += Vector3.up * renderYOffset;
        var go = Instantiate(prefab, worldPos, Quaternion.identity, rendersParent);
        bucket.Add(go);
        go.SetActive(false);
    }

    void RenderHintsForPiece(
        MovementType type,
        Vector2Int start,
        int boardW, int boardH,
        Side side,
        HashSet<Vector2Int> playerPositions,
        int pawnStartRank = -999 // si no querés doble paso, dejalo inválido
    )
    {
        ClearAllRenderHints();

        // 1) Todos los MOVIMIENTOS posibles desde 'start'
        var moveSquares = new HashSet<Vector2Int>(GetLegalMoves(type, start, boardW, boardH, side, pawnStartRank));

        // 2) Todos los ATAQUES posibles desde la posición actual...
        var attackSquares = new HashSet<Vector2Int>(GetAttackableTargetsFrom(type, start, boardW, boardH, side, playerPositions));

        // ...y además desde cada casilla de movimiento (mover + atacar)
        foreach (var m in moveSquares)
        {
            // IMPORTANTE: asumimos que 'start' queda vacío y 'm' ocupado por la pieza al atacar
            var fromAfterMove = new HashSet<Vector2Int>(GetAttackableTargetsFrom(type, m, boardW, boardH, side, playerPositions));
            attackSquares.UnionWith(fromAfterMove);
        }

        // Instanciar renders
        foreach (var c in moveSquares)
            SpawnRender(moveRender, GridToWorld(c), moveRenderInstances);

        foreach (var c in attackSquares)
            SpawnRender(attackRender, GridToWorld(c), attackRenderInstances);
    }

    // ===================== LOOP PRINCIPAL IA =====================
    public IEnumerator Initialize(List<PieceDataGO> enemyPieces, List<PieceDataGO> playerPieces)
    {
        var boardSizeX = ResourceController.Instance.gameConfigs.gameSettings.boardSizeX;
        var boardSizeY = ResourceController.Instance.gameConfigs.gameSettings.boardSizeY;

        // Cache de posiciones de jugador para calcular "casillas atacables"
        HashSet<Vector2Int> PlayerPositions()
        {
            var set = new HashSet<Vector2Int>();
            for (int i = 0; i < playerPieces.Count; i++)
                set.Add(playerPieces[i].pieceController.position);
            return set;
        }

        for (int i = 0; i < enemyPieces.Count; i++)
        {
            var randomPlayerPiece = playerPieces.Where(x => !x.pieceController.isDead).OrderBy(x => Random.value).ToList();
            var enemyPiece = enemyPieces[i];

            Side side = (enemyPiece.pieceController.position.x % 2 == 0 && enemyPiece.pieceController.position.y % 2 == 0) ? Side.Black : Side.White;

            RenderHintsForPiece(
                enemyPiece.pieceController.pieceData.movementType,
                enemyPiece.pieceController.position,
                boardSizeX, boardSizeY,
                side,
                PlayerPositions()
            );

            // Pequeña pausa para que el jugador lo vea antes de que actúe la IA
            yield return new WaitForSeconds(0.35f);

            foreach (var playerPiece in randomPlayerPiece)
            {
                Vector2Int moveTo, attackTo;
                var canMove = TryPlanMoveAndAttack(
                    enemyPiece.pieceController.pieceData.movementType,
                    enemyPiece.pieceController.position,
                    playerPiece.pieceController.position,
                    boardSizeX,
                    boardSizeY,
                    out moveTo,
                    out attackTo
                );
                if (!canMove)
                {
                    Debug.Log("#IAController#: No can move or attack, setting attack and movement to true");
                    continue;
                }
                moveRenderInstances.ForEach(instance => instance.SetActive(true));
                // --- GIZMOS: registrar plan para visualizar ---
                PushDebugPlan(
                    enemyPiece.pieceController,
                    enemyPiece.pieceController.position,
                    playerPiece.pieceController.position,
                    moveTo, attackTo, canMove
                );

                if (canMove)
                {
                    Debug.Log("#IAController#: Moving to: " + moveTo.x + ", " + moveTo.y);
                    enemyPiece.pieceController.MoveToPosition(moveTo);
                    yield return new WaitForSeconds(1f);
                    attackRenderInstances.ForEach(instance => instance.SetActive(true));
                    yield return new WaitForSeconds(1f);
                    Debug.Log("#IAController#: Attacking to: " + attackTo.x + ", " + attackTo.y);
                    TabletopController.Instance.AttackPiece(attackTo.x, attackTo.y, enemyPiece.pieceController.pieceData.damage);
                    enemyPiece.pieceController.SetAttackBool(true);
                }

                // Al terminar el turno de esta pieza, limpiar los renders
                ClearAllRenderHints();

                yield return new WaitForSeconds(1f);
                TabletopController.Instance.CalculateMovements();
                break;
            }
            if (!enemyPiece.pieceController.GetAttackBool())
            {
                Debug.Log("#IAController#: No can move or attack, setting attack and movement to true");
                enemyPiece.pieceController.SetAttackBool(true);
                enemyPiece.pieceController.SetMovementBool(true);
                TabletopController.Instance.CalculateMovements();
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public void FinishTurn()
    {
        StopAllCoroutines();
    }

    // ===================== MOVIMIENTO / ATAQUE POSIBLES =====================
    // Offsets estáticos
    private static readonly Vector2Int[] KnightOffsets = new Vector2Int[]
    {
        new Vector2Int(1,2), new Vector2Int(2,1), new Vector2Int(-1,2), new Vector2Int(-2,1),
        new Vector2Int(1,-2), new Vector2Int(2,-1), new Vector2Int(-1,-2), new Vector2Int(-2,-1)
    };
    private static readonly Vector2Int[] KingOffsets = new Vector2Int[]
    {
        new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1),
        new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
    };

    private static bool InBounds(Vector2Int p, int w, int h) => p.x >= 0 && p.y >= 0 && p.x < w && p.y < h;
    private static int Abs(int v) => v < 0 ? -v : v;
    private static int Sign(int v) => v > 0 ? 1 : (v < 0 ? -1 : 0);
    private static int Manhattan(Vector2Int a, Vector2Int b) => Abs(a.x - b.x) + Abs(a.y - b.y);
    private static int Chebyshev(Vector2Int a, Vector2Int b) => Mathf.Max(Abs(a.x - b.x), Abs(a.y - b.y));

    // Casilla vacía y dentro del tablero (para destinos de movimiento)
    private static bool Valid(Vector2Int p, int w, int h)
    {
        if (!InBounds(p, w, h)) return false;
        return TabletopController.Instance.IsPositionEmpty(p);
    }

    private static bool IsKnightNeighbor(Vector2Int a, Vector2Int b)
    {
        int dx = Abs(a.x - b.x), dy = Abs(a.y - b.y);
        return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
    }

    private static IEnumerable<Vector2Int> KnightMoves(Vector2Int s, int w, int h)
    {
        foreach (var o in KnightOffsets)
        {
            var p = s + o;
            if (Valid(p, w, h)) yield return p;
        }
    }

    private static IEnumerable<Vector2Int> KingNeighbors(Vector2Int s, int w, int h)
    {
        foreach (var o in KingOffsets)
        {
            var p = s + o;
            if (Valid(p, w, h)) yield return p;
        }
    }

    // ===== Helpers de raycast en grilla (rook/bishop/queen) =====
    private static bool TryGetStep(Vector2Int from, Vector2Int to, out Vector2Int step)
    {
        int dx = Sign(to.x - from.x);
        int dy = Sign(to.y - from.y);
        bool rookAligned = (dx == 0) ^ (dy == 0);
        bool bishopAligned = (dx != 0 && dy != 0 && Abs(to.x - from.x) == Abs(to.y - from.y));
        if (!rookAligned && !bishopAligned)
        {
            step = Vector2Int.zero;
            return false;
        }
        step = new Vector2Int(dx, dy);
        return true;
    }

    // ¿Libre el camino EXCLUYENDO 'from' y 'to'? (permite asumir algunas celdas vacías)
    private static bool IsLineClearExclusive(Vector2Int from, Vector2Int to, HashSet<Vector2Int> assumeEmpty = null)
    {
        if (!TryGetStep(from, to, out var step)) return false;
        var p = from + step;
        while (p != to)
        {
            bool assumed = (assumeEmpty != null && assumeEmpty.Contains(p));
            if (!assumed && !TabletopController.Instance.IsPositionEmpty(p)) return false;
            p += step;
        }
        return true;
    }

    // Primer bloqueador EXCLUYENDO 'from' y 'to'
    private static bool TryFindFirstBlockerExclusive(Vector2Int from, Vector2Int to, out Vector2Int blocker)
    {
        blocker = from;
        if (!TryGetStep(from, to, out var step)) return false;
        var p = from + step;
        while (p != to)
        {
            if (!TabletopController.Instance.IsPositionEmpty(p))
            {
                blocker = p;
                return true;
            }
            p += step;
        }
        return false;
    }

    // Un paso "alejándose" (opuesto al step), si está libre.
    private static bool TryFindRetreatSquare(Vector2Int start, Vector2Int awayStep, int w, int h, out Vector2Int retreat)
    {
        retreat = start;
        if (awayStep == Vector2Int.zero) return false;
        var p = start + awayStep;
        if (!InBounds(p, w, h)) return false;
        if (!TabletopController.Instance.IsPositionEmpty(p)) return false;
        retreat = p;
        return true;
    }

    // --- Candidatos por pieza (para planear) ---
    private static bool TryRookCandidate(Vector2Int start, Vector2Int target, int w, int h, out Vector2Int moveTo)
    {
        moveTo = start;
        if (start.x == target.x || start.y == target.y) return true;
        var c1 = new Vector2Int(target.x, start.y);
        var c2 = new Vector2Int(start.x, target.y);
        bool ok1 = Valid(c1, w, h), ok2 = Valid(c2, w, h);
        if (!ok1 && !ok2) return false;
        if (ok1 && ok2) moveTo = (Manhattan(start, c1) <= Manhattan(start, c2)) ? c1 : c2;
        else moveTo = ok1 ? c1 : c2;
        return true;
    }

    private static bool TryBishopCandidate(Vector2Int start, Vector2Int target, int w, int h, out Vector2Int moveTo)
    {
        moveTo = start;
        if (Abs(start.x - target.x) == Abs(start.y - target.y)) return true;
        if (((start.x + start.y) & 1) != ((target.x + target.y) & 1)) return false;

        int cs = start.y - start.x;
        int ds = start.y + start.x;
        int ct = target.y - target.x;
        int dt = target.y + target.x;
        var cand = new List<Vector2Int>();

        int num1 = dt - cs;
        if ((num1 & 1) == 0)
        {
            int x1 = num1 / 2;
            int y1 = x1 + cs;
            var p1 = new Vector2Int(x1, y1);
            if (Valid(p1, w, h)) cand.Add(p1);
        }
        int num2 = ds - ct;
        if ((num2 & 1) == 0)
        {
            int x2 = num2 / 2;
            int y2 = -x2 + ds;
            var p2 = new Vector2Int(x2, y2);
            if (Valid(p2, w, h)) cand.Add(p2);
        }
        if (cand.Count == 0) return false;

        moveTo = cand[0];
        int best = Manhattan(start, moveTo);
        for (int i = 1; i < cand.Count; i++)
        {
            int d = Manhattan(start, cand[i]);
            if (d < best) { best = d; moveTo = cand[i]; }
        }
        return true;
    }

    private static bool TryQueenCandidate(Vector2Int start, Vector2Int target, int w, int h, out Vector2Int moveTo)
    {
        moveTo = start;
        if (start.x == target.x || start.y == target.y || Abs(start.x - target.x) == Abs(start.y - target.y)) return true;
        Vector2Int r, b;
        bool okR = TryRookCandidate(start, target, w, h, out r);
        bool okB = TryBishopCandidate(start, target, w, h, out b);
        if (!okR && !okB) return false;
        if (okR && okB) moveTo = (Manhattan(start, r) <= Manhattan(start, b)) ? r : b;
        else moveTo = okR ? r : b;
        return true;
    }

    private static bool TryKnightCandidate(Vector2Int start, Vector2Int target, int w, int h, out Vector2Int moveTo)
    {
        moveTo = start;
        if (IsKnightNeighbor(start, target)) return true;
        Vector2Int best = start;
        int bestD = int.MaxValue;
        bool found = false;
        foreach (var p in KnightMoves(start, w, h))
        {
            if (IsKnightNeighbor(p, target))
            {
                int d = Manhattan(start, p);
                if (d < bestD) { bestD = d; best = p; found = true; }
            }
        }
        if (!found) return false;
        moveTo = best;
        return true;
    }

    private static bool TryKingCandidate(Vector2Int start, Vector2Int target, int w, int h, out Vector2Int moveTo)
    {
        moveTo = start;
        if (Chebyshev(start, target) == 1) return true;
        Vector2Int best = start;
        int bestD = int.MaxValue;
        bool found = false;
        foreach (var p in KingNeighbors(start, w, h))
        {
            if (Chebyshev(p, target) == 1)
            {
                int d = Manhattan(start, p);
                if (d < bestD) { bestD = d; best = p; found = true; }
            }
        }
        if (!found) return false;
        moveTo = best;
        return true;
    }

    private static bool TryPawnCandidate(Vector2Int start, Vector2Int target, int w, int h, Side side, int pawnStartRank, out Vector2Int moveTo)
    {
        moveTo = start;
        int fwd = (side == Side.White) ? +1 : -1;

        if (Abs(target.x - start.x) == 1 && (target.y - start.y) == fwd) return true;

        var pA = new Vector2Int(target.x - 1, target.y - fwd);
        var pB = new Vector2Int(target.x + 1, target.y - fwd);
        bool okA = Valid(pA, w, h);
        bool okB = Valid(pB, w, h);

        var candidates = new List<Vector2Int>();
        if (okA) candidates.Add(pA);
        if (okB) candidates.Add(pB);
        if (candidates.Count == 0) return false;

        Vector2Int best = start;
        int bestD = int.MaxValue;
        bool found = false;
        foreach (var p in candidates)
        {
            if (p.x != start.x) continue;
            int dySigned = (p.y - start.y) * fwd;
            bool oneStep = (dySigned == 1);
            bool twoStep = (dySigned == 2) && (pawnStartRank == start.y);
            if (oneStep || twoStep)
            {
                int d = Abs(p.y - start.y);
                if (d < bestD) { bestD = d; best = p; found = true; }
            }
        }
        if (!found) return false;
        moveTo = best;
        return true;
    }

    /// <summary>
    /// Devuelve true si es posible "mover y atacar" en el mismo turno según la pieza.
    /// moveTo: casilla adonde moverse; attackTo: por defecto 'target' o bloqueador si hay fallback.
    /// Nota: valida in-bounds; el target puede estar ocupado.
    /// </summary>
    public static bool TryPlanMoveAndAttack(
        MovementType type,
        Vector2Int start,
        Vector2Int target,
        int boardWidth,
        int boardHeight,
        out Vector2Int moveTo,
        out Vector2Int attackTo,
        Side side = Side.White,
        int pawnStartRank = -999 // setear si querés doble paso del peón
    )
    {
        moveTo = start;
        attackTo = target;

        if (!InBounds(start, boardWidth, boardHeight) || !InBounds(target, boardWidth, boardHeight) || start == target)
            return false;

        bool can = false;
        switch (type)
        {
            case MovementType.TOWER: can = TryRookCandidate(start, target, boardWidth, boardHeight, out moveTo); break;
            case MovementType.BISHOP: can = TryBishopCandidate(start, target, boardWidth, boardHeight, out moveTo); break;
            case MovementType.QUEEN: can = TryQueenCandidate(start, target, boardWidth, boardHeight, out moveTo); break;
            case MovementType.HORSE: can = TryKnightCandidate(start, target, boardWidth, boardHeight, out moveTo); break;
            case MovementType.KING: can = TryKingCandidate(start, target, boardWidth, boardHeight, out moveTo); break;
            case MovementType.PAWN: can = TryPawnCandidate(start, target, boardWidth, boardHeight, side, pawnStartRank, out moveTo); break;
        }

        // --- Fallback "alejarse y atacar al bloqueador" para piezas deslizantes ---
        if (type == MovementType.TOWER || type == MovementType.BISHOP || type == MovementType.QUEEN)
        {
            if (can && moveTo != start)
            {
                if (!IsLineClearExclusive(start, moveTo))
                {
                    if (TryFindFirstBlockerExclusive(start, moveTo, out var blocker)
                        && TryGetStep(start, moveTo, out var step)
                        && TryFindRetreatSquare(start, -step, boardWidth, boardHeight, out var retreat))
                    {
                        moveTo = retreat;
                        attackTo = blocker;
                        return true;
                    }
                    return false;
                }
                return true;
            }

            if (!can)
            {
                if (type == MovementType.TOWER)
                {
                    Vector2Int c1 = new Vector2Int(target.x, start.y);
                    Vector2Int c2 = new Vector2Int(start.x, target.y);
                    var pick = c1;
                    if (!InBounds(c1, boardWidth, boardHeight)) pick = c2;
                    else if (InBounds(c2, boardWidth, boardHeight) && Manhattan(start, c2) < Manhattan(start, c1)) pick = c2;

                    if (InBounds(pick, boardWidth, boardHeight)
                        && TryFindFirstBlockerExclusive(start, pick, out var blocker)
                        && TryGetStep(start, pick, out var step)
                        && TryFindRetreatSquare(start, -step, boardWidth, boardHeight, out var retreat))
                    {
                        moveTo = retreat;
                        attackTo = blocker;
                        return true;
                    }
                }
                else if (type == MovementType.BISHOP || type == MovementType.QUEEN)
                {
                    if (((start.x + start.y) & 1) == ((target.x + target.y) & 1))
                    {
                        int cs = start.y - start.x;
                        int ds = start.y + start.x;
                        int ct = target.y - target.x;
                        int dt = target.y + target.x;
                        var cands = new List<Vector2Int>();

                        int num1 = dt - cs; // s(+1) ∩ t(-1)
                        if ((num1 & 1) == 0)
                        {
                            int x1 = num1 / 2;
                            int y1 = x1 + cs;
                            var p1 = new Vector2Int(x1, y1);
                            if (InBounds(p1, boardWidth, boardHeight)) cands.Add(p1);
                        }
                        int num2 = ds - ct; // s(-1) ∩ t(+1)
                        if ((num2 & 1) == 0)
                        {
                            int x2 = num2 / 2;
                            int y2 = -x2 + ds;
                            var p2 = new Vector2Int(x2, y2);
                            if (InBounds(p2, boardWidth, boardHeight)) cands.Add(p2);
                        }

                        if (cands.Count > 0)
                        {
                            var pick = cands[0];
                            if (cands.Count == 2 && Manhattan(start, cands[1]) < Manhattan(start, pick)) pick = cands[1];

                            if (TryFindFirstBlockerExclusive(start, pick, out var blocker)
                                && TryGetStep(start, pick, out var step)
                                && TryFindRetreatSquare(start, -step, boardWidth, boardHeight, out var retreat))
                            {
                                moveTo = retreat;
                                attackTo = blocker;
                                return true;
                            }
                        }
                    }

                    if (type == MovementType.QUEEN)
                    {
                        Vector2Int c1 = new Vector2Int(target.x, start.y);
                        Vector2Int c2 = new Vector2Int(start.x, target.y);
                        var pick = c1;
                        if (!InBounds(c1, boardWidth, boardHeight)) pick = c2;
                        else if (InBounds(c2, boardWidth, boardHeight) && Manhattan(start, c2) < Manhattan(start, c1)) pick = c2;

                        if (InBounds(pick, boardWidth, boardHeight)
                            && TryFindFirstBlockerExclusive(start, pick, out var blocker)
                            && TryGetStep(start, pick, out var step)
                            && TryFindRetreatSquare(start, -step, boardWidth, boardHeight, out var retreat))
                        {
                            moveTo = retreat;
                            attackTo = blocker;
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        // Piezas no deslizantes o caso simple
        return can;
    }

    // ===================== GENERADORES DE MOV/ATAQUE PARA RENDERS =====================
    IEnumerable<Vector2Int> GetLegalMoves(MovementType type, Vector2Int start, int w, int h, Side side, int pawnStartRank)
    {
        switch (type)
        {
            case MovementType.TOWER:
                foreach (var p in RayEmpty(start, new Vector2Int(1, 0), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(-1, 0), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(0, 1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(0, -1), w, h)) yield return p;
                yield break;

            case MovementType.BISHOP:
                foreach (var p in RayEmpty(start, new Vector2Int(1, 1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(-1, 1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(1, -1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(-1, -1), w, h)) yield return p;
                yield break;

            case MovementType.QUEEN:
                foreach (var p in RayEmpty(start, new Vector2Int(1, 0), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(-1, 0), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(0, 1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(0, -1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(1, 1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(-1, 1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(1, -1), w, h)) yield return p;
                foreach (var p in RayEmpty(start, new Vector2Int(-1, -1), w, h)) yield return p;
                yield break;

            case MovementType.HORSE:
                foreach (var p in KnightMoves(start, w, h)) yield return p;
                yield break;

            case MovementType.KING:
                foreach (var p in KingNeighbors(start, w, h)) yield return p;
                yield break;

            case MovementType.PAWN:
                {
                    int fwd = (side == Side.White) ? +1 : -1;
                    var one = new Vector2Int(start.x, start.y + fwd);
                    if (Valid(one, w, h)) yield return one;

                    if (pawnStartRank == start.y)
                    {
                        var two = new Vector2Int(start.x, start.y + 2 * fwd);
                        // Para doble paso, ambas casillas deben estar libres:
                        if (Valid(one, w, h) && Valid(two, w, h)) yield return two;
                    }
                    yield break;
                }
        }
    }

    IEnumerable<Vector2Int> RayEmpty(Vector2Int from, Vector2Int step, int w, int h)
    {
        var p = from + step;
        while (InBounds(p, w, h) && TabletopController.Instance.IsPositionEmpty(p))
        {
            yield return p;
            p += step;
        }
    }

    IEnumerable<Vector2Int> GetAttackableTargetsFrom(
        MovementType type,
        Vector2Int from,
        int w, int h,
        Side side,
        HashSet<Vector2Int> playerPositions)
    {
        // Atacables = casillas OCUPADAS por el jugador que cumplan regla de ataque de la pieza
        foreach (var target in playerPositions)
        {
            if (!InBounds(target, w, h)) continue;

            switch (type)
            {
                case MovementType.TOWER:
                    {
                        if (from.x == target.x || from.y == target.y)
                            if (IsLineClearExclusive(from, target)) yield return target;
                        break;
                    }
                case MovementType.BISHOP:
                    {
                        if (Abs(from.x - target.x) == Abs(from.y - target.y))
                            if (IsLineClearExclusive(from, target)) yield return target;
                        break;
                    }
                case MovementType.QUEEN:
                    {
                        bool aligned = (from.x == target.x || from.y == target.y) || (Abs(from.x - target.x) == Abs(from.y - target.y));
                        if (aligned && IsLineClearExclusive(from, target)) yield return target;
                        break;
                    }
                case MovementType.HORSE:
                    {
                        if (IsKnightNeighbor(from, target)) yield return target;
                        break;
                    }
                case MovementType.KING:
                    {
                        if (Chebyshev(from, target) == 1) yield return target;
                        break;
                    }
                case MovementType.PAWN:
                    {
                        int fwd = (side == Side.White) ? +1 : -1;
                        if (Abs(target.x - from.x) == 1 && (target.y - from.y) == fwd)
                            yield return target;
                        break;
                    }
            }
        }
    }

    // ====================== GIZMOS DEBUG ======================
    [Header("Gizmos · Toggle")]
    [SerializeField] bool gizmosEnabled = true;
    [SerializeField] bool gizmosOnlyWhenSelected = false;

    [Header("Gizmos · Tablero")]
    [SerializeField] bool drawBoardGrid = true;
    [SerializeField] Vector3 boardOrigin = Vector3.zero; // esquina de la celda (0,0)
    [SerializeField] float cellSize = 1f;
    [SerializeField] bool planeXZ = false; // true: XZ (3D); false: XY (2D)

    [Header("Gizmos · Estilo")]
    [SerializeField] float pointRadius = 0.16f;
    [SerializeField] float arrowHeadSize = 0.25f;
    [SerializeField] Color startColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] Color moveColor = new Color(0f, 0.75f, 1f, 0.95f);
    [SerializeField] Color attackColor = new Color(1f, 0.25f, 0.25f, 0.95f);
    [SerializeField] Color gridColor = new Color(1f, 1f, 1f, 0.15f);
    [SerializeField] Color failColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    struct DebugPlan
    {
        public PieceController piece;
        public MovementType type;
        public Vector2Int start;
        public Vector2Int moveTo;
        public Vector2Int attackTo;
        public bool ok;
        public string label;
    }
    readonly List<DebugPlan> _debugPlans = new List<DebugPlan>();

    /// <summary> Llamá a esto luego de calcular tu plan para visualizarlo. </summary>
    public void PushDebugPlan(
        PieceController piece,
        Vector2Int start,
        Vector2Int target,
        Vector2Int moveTo,
        Vector2Int attackTo,
        bool ok,
        string label = null)
    {
        _debugPlans.Add(new DebugPlan
        {
            piece = piece,
            type = piece.pieceData.movementType,
            start = start,
            moveTo = moveTo,
            attackTo = attackTo,
            ok = ok,
            label = label ?? $"{piece.pieceData.movementType}"
        });
    }

    /// <summary> Limpia todos los planes dibujados. </summary>
    public void ClearDebugPlans() => _debugPlans.Clear();

    void OnDrawGizmos()
    {
        if (!gizmosEnabled || gizmosOnlyWhenSelected) return;
        DrawGizmosInternal();
    }
    void OnDrawGizmosSelected()
    {
        if (!gizmosEnabled || !gizmosOnlyWhenSelected) return;
        DrawGizmosInternal();
    }

    void DrawGizmosInternal()
    {
        if (drawBoardGrid && ResourceController.Instance != null)
        {
            var cfg = ResourceController.Instance.gameConfigs.gameSettings;
            DrawBoardGrid(cfg.boardSizeX, cfg.boardSizeY);
        }
        foreach (var p in _debugPlans) DrawPlan(p);
    }

    // ---------- Helpers de dibujo ----------
    void DrawBoardGrid(int w, int h)
    {
        Gizmos.color = gridColor;
        Vector3 A(int x, int y) => GridToWorld(new Vector2Int(x, y));
        for (int x = 0; x <= w; x++) Gizmos.DrawLine(A(x, 0) + EdgeOffset(), A(x, h) + EdgeOffset());
        for (int y = 0; y <= h; y++) Gizmos.DrawLine(A(0, y) + EdgeOffset(), A(w, y) + EdgeOffset());
    }

    void DrawPlan(DebugPlan p)
    {
        var startPos = GridToWorld(p.start);
        var movePos = GridToWorld(p.moveTo);
        var attackPos = GridToWorld(p.attackTo);

        var cStart = p.ok ? startColor : failColor;
        var cMove = p.ok ? moveColor : failColor;
        var cAttack = p.ok ? attackColor : failColor;

        Gizmos.color = cStart;
        DrawDisc(startPos, pointRadius * 0.9f);
        DrawWireSquare(startPos, cellSize * 0.35f);

        Gizmos.color = cMove;
        if (p.moveTo != p.start)
        {
            DrawArrow(startPos, movePos, arrowHeadSize);
            DrawDisc(movePos, pointRadius);
#if UNITY_EDITOR
            Handles.color = cMove;
            Handles.Label(movePos + LabelOffset(), $"move {p.moveTo.x},{p.moveTo.y}");
#endif
        }

        Gizmos.color = cAttack;
        DrawArrow(movePos, attackPos, arrowHeadSize * 0.9f);
        DrawCross(attackPos, cellSize * 0.35f);
#if UNITY_EDITOR
        Handles.color = cAttack;
        Handles.Label(attackPos + LabelOffset(), $"attack {p.attackTo.x},{p.attackTo.y} ({p.label})");
#endif
    }

    // ----- primitivas -----
    Vector3 GridToWorld(Vector2Int c)
    {
        Vector3 newPos = TabletopController.Instance.GetGrid().GetCellCenterWorld(new Vector3Int(c.x, 0, c.y));
        return newPos;
    }

    Vector3 EdgeOffset() => Vector3.zero;

    void DrawDisc(Vector3 pos, float r)
    {
        if (planeXZ) DrawCylinder(pos, r, 0.01f);
        else Gizmos.DrawSphere(pos, r);
    }

    void DrawWireSquare(Vector3 center, float half)
    {
        Vector3 a, b, c, d;
        if (planeXZ)
        {
            a = center + new Vector3(-half, 0, -half);
            b = center + new Vector3(half, 0, -half);
            c = center + new Vector3(half, 0, half);
            d = center + new Vector3(-half, 0, half);
        }
        else
        {
            a = center + new Vector3(-half, -half, 0);
            b = center + new Vector3(half, -half, 0);
            c = center + new Vector3(half, half, 0);
            d = center + new Vector3(-half, half, 0);
        }
        Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, c); Gizmos.DrawLine(c, d); Gizmos.DrawLine(d, a);
    }

    void DrawCross(Vector3 center, float size)
    {
        Vector3 u, v;
        if (planeXZ) { u = new Vector3(size, 0, 0); v = new Vector3(0, 0, size); }
        else { u = new Vector3(size, 0, 0); v = new Vector3(0, size, 0); }
        Gizmos.DrawLine(center - u, center + u);
        Gizmos.DrawLine(center - v, center + v);
    }

    void DrawArrow(Vector3 from, Vector3 to, float head)
    {
        Gizmos.DrawLine(from, to);
        var dir = (to - from);
        var len = dir.magnitude;
        if (len < 1e-4f) return;
        dir /= len;
        Vector3 right, up;
        if (planeXZ) { up = Vector3.up; right = Vector3.Cross(up, dir).normalized; }
        else { up = Vector3.forward; right = Vector3.Cross(dir, up).normalized; }
        var p1 = to - dir * head + right * head * 0.6f;
        var p2 = to - dir * head - right * head * 0.6f;
        Gizmos.DrawLine(to, p1);
        Gizmos.DrawLine(to, p2);
    }

    void DrawCylinder(Vector3 center, float radius, float height)
    {
        var top = center + Vector3.up * (height * 0.5f);
        var bot = center - Vector3.up * (height * 0.5f);
        Gizmos.DrawLine(top, bot);
        const int seg = 20;
        Vector3 prevTop = top + new Vector3(radius, 0, 0);
        Vector3 prevBot = bot + new Vector3(radius, 0, 0);
        for (int i = 1; i <= seg; i++)
        {
            float ang = i * Mathf.PI * 2f / seg;
            Vector3 p = new Vector3(Mathf.Cos(ang) * radius, 0, Mathf.Sin(ang) * radius);
            Vector3 nTop = top + p;
            Vector3 nBot = bot + p;
            Gizmos.DrawLine(prevTop, nTop);
            Gizmos.DrawLine(prevBot, nBot);
            prevTop = nTop;
            prevBot = nBot;
        }
    }

    Vector3 LabelOffset() => planeXZ ? new Vector3(0, 0.02f, 0) : new Vector3(0, 0.02f, 0);
    // ==================== FIN GIZMOS DEBUG ====================
}
