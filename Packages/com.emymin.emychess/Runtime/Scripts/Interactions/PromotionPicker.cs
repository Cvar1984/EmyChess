using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Interactions
{
    /// <summary>
    /// Behaviour for the promotion interface, show with <see cref="Setup(Piece)"/>, hide with <see cref="_Hide"/>
    /// </summary>
    public class PromotionPicker : UdonSharpBehaviour
    {
        public Board board;
        public GameObject piecesButtonsParent;
        [HideInInspector]
        public Piece targetPiece;
        private PromotionButton[] promotionButtons;

        public void Start()
        {
            if (piecesButtonsParent == null)
            {
                Debug.LogWarning("[PromotionPicker] piecesButtonsParent is null in Start.", this);
                promotionButtons = new PromotionButton[0];
                return;
            }
            var parentTransform = piecesButtonsParent.transform;
            promotionButtons = new PromotionButton[parentTransform.childCount];
            for (int i = 0; i < parentTransform.childCount; i++)
            {
                GameObject child = parentTransform.GetChild(i).gameObject;
                var udonBehaviour = child.GetComponent(typeof(UdonBehaviour));
                if (udonBehaviour == null)
                {
                    Debug.LogWarning("[PromotionPicker] UdonBehaviour missing on child in Start.", this);
                    continue;
                }
                PromotionButton button = udonBehaviour as PromotionButton;
                if (button == null)
                {
                    Debug.LogWarning("[PromotionPicker] PromotionButton missing on child in Start.", this);
                    continue;
                }
                promotionButtons[i] = button;
            }
        }
        /// <summary>
        /// Function that shows the promotion interface above the specified piece
        /// </summary>
        /// <param name="piece"></param>
        public void Setup(Piece piece)
        {
            if (piece == null)
            {
                Debug.LogWarning("[PromotionPicker] piece is null in Setup.", this);
                return;
            }
            if (board == null)
            {
                Debug.LogWarning("[PromotionPicker] board is null in Setup.", this);
                return;
            }
            if (piecesButtonsParent == null)
            {
                Debug.LogWarning("[PromotionPicker] piecesButtonsParent is null in Setup.", this);
                return;
            }
            targetPiece = piece;
            board._AllPiecesUngrabbable();
            transform.localPosition = new Vector3(-(targetPiece.x + .5f), 0, -(targetPiece.y + .5f));
            transform.localRotation = targetPiece.white ? Quaternion.identity : Quaternion.identity * Quaternion.Euler(0, 180f, 0);
            piecesButtonsParent.SetActive(true);
            if (promotionButtons != null)
            {
                foreach (PromotionButton button in promotionButtons)
                {
                    if (button != null)
                        button._RefreshCounter();
                }
            }
        }
        public void _PromotionDone()
        {
            _Hide();
            if (board == null || board.chessManager == null)
            {
                Debug.LogWarning("[PromotionPicker] board or chessManager is null in _PromotionDone.", this);
                return;
            }
            board.chessManager._EndTurn();
        }
        public void _Hide()
        {
            if (piecesButtonsParent != null)
                piecesButtonsParent.SetActive(false);
            else
                Debug.LogWarning("[PromotionPicker] piecesButtonsParent is null in _Hide.", this);
        }
        public void PromoteTo(string type)
        {
            if (targetPiece == null)
            {
                Debug.LogWarning("[PromotionPicker] targetPiece is null in PromoteTo.", this);
                return;
            }
            if (board == null)
            {
                Debug.LogWarning("[PromotionPicker] board is null in PromoteTo.", this);
                return;
            }
            bool white = targetPiece.white;

            if (string.IsNullOrEmpty(type))
            {
                Debug.LogWarning("[PromotionPicker] type is null or empty in PromoteTo.", this);
                return;
            }

            if (type == "pawn")
            {
                _PromotionDone();
                return;
            }
            if (board.GetPiecesAvailableCount(type) > 0)
            {
                int x = targetPiece.x;
                int y = targetPiece.y;
                targetPiece._ReturnToPool();
                board._SpawnPiece(x, y, white, type);
                _PromotionDone();
            }
            else
            {
                Debug.LogWarning("[PromotionPicker] No available pieces for promotion type: " + type, this);
            }
            Debug.Log("[ChessManager] " + (white ? "White " : "Black ") + targetPiece.type + " promoted to " + type);
        }
        
    }

}
