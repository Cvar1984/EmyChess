using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3;
using UnityEngine.UI;
using JetBrains.Annotations;

namespace Emychess.Interactions
{
    /// <summary>
    /// Behaviour that lets users spawn pieces while in anarchy mode
    /// </summary>
    public class PiecePlacer : UdonSharpBehaviour
    {
        public Board board;
        public bool white;
        [HideInInspector]
        public string currentType;
        [HideInInspector]
        public VRC_Pickup pickup;
        [Tooltip("The parent that holds the pieces that are part of the PiecePlacer interface")]
        public Transform pieceMeshParent;
        public Text availableText;
        public Text pieceCount;

        private string[] types = { "pawn", "knight", "rook", "bishop", "queen", "king" };
        //TODO should index be [UdonSynced] to sync the interface for everyone?
        private byte index = 0;

        public void _Setup()
        {
            index = 0;
            currentType = types[index];
            SetEnabled(true);
            _Refresh();
        }

        public void SetEnabled(bool enabled)
        {
            if (pickup == null)
            {
                pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
                if (pickup == null)
                {
                    Debug.LogWarning("[PiecePlacer] VRC_Pickup component missing!", this);
                    return;
                }
            }
            pickup.pickupable = enabled;
        }

        [PublicAPI] public void _NextType()
        {
            index = (byte)((index + 1) % types.Length);
            currentType = types[index];
            _Refresh();
        }
        [PublicAPI] public void _PreviousType()
        {
            index = (byte)(((index - 1) + types.Length) % types.Length);
            currentType = types[index];
            _Refresh();
        }
        public void _Refresh()
        {
            if (string.IsNullOrEmpty(currentType))
            {
                _Setup();
                return;
            }
            if (pieceMeshParent == null)
            {
                Debug.LogWarning("[PiecePlacer] pieceMeshParent is null in _Refresh.", this);
                return;
            }
            foreach (Transform child in pieceMeshParent)
            {
                if (child == null) continue;
                child.gameObject.SetActive(child.name == currentType);
            }
            if (availableText != null)
                availableText.text = "Available " + currentType + "s";
            else
                Debug.LogWarning("[PiecePlacer] availableText is null in _Refresh.", this);

            if (pieceCount != null && board != null)
                pieceCount.text = board.GetPiecesAvailableCount(currentType).ToString();
            else if (pieceCount == null)
                Debug.LogWarning("[PiecePlacer] pieceCount is null in _Refresh.", this);
            else if (board == null)
                Debug.LogWarning("[PiecePlacer] board is null in _Refresh.", this);
        }
        public override void OnDrop()
        {
            if (board == null)
            {
                Debug.LogWarning("[PiecePlacer] board is null in OnDrop.", this);
                return;
            }
            if (board.pieces_parent == null)
            {
                Debug.LogWarning("[PiecePlacer] board.pieces_parent is null in OnDrop.", this);
                return;
            }
            Vector3 pos = board.pieces_parent.InverseTransformPoint(transform.position);
            int x = (int)pos.x * -1 - 1;
            int y = (int)pos.z * -1 - 1;
            if (board.currentRules != null && board.currentRules.anarchy && board.GetPiecesAvailableCount(currentType) > 0)
            {
                Piece captured = board.GetPiece(x, y);
                if (captured != null) captured._Capture();
                Piece spawned = board._SpawnPiece(x, y, white, currentType);
                if (spawned != null)
                {
                    spawned.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(spawned.PieceMovedAudio));
                }
            }
            else if (board.currentRules == null)
            {
                Debug.LogWarning("[PiecePlacer] board.currentRules is null in OnDrop.", this);
            }
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _Refresh();
        }
        public void OnEnable()
        {
            _Setup();
        }
    }

}
