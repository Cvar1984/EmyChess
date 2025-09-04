using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Interactions
{
    public class PromotionButton : UdonSharpBehaviour
    {
        public PromotionPicker picker;
        public string promotionType;
        public Text availableCountLabel;

        /// <summary>
        /// Refresh the counter showing how many pieces of the <see cref="promotionType"/> are left to be spawned
        /// </summary>
        public void _RefreshCounter()
        {
            if (availableCountLabel == null)
            {
                Debug.LogWarning("[PromotionButton] availableCountLabel is null in _RefreshCounter.", this);
                return;
            }
            if (picker == null)
            {
                Debug.LogWarning("[PromotionButton] picker is null in _RefreshCounter.", this);
                availableCountLabel.text = "0";
                return;
            }
            if (picker.board == null)
            {
                Debug.LogWarning("[PromotionButton] picker.board is null in _RefreshCounter.", this);
                availableCountLabel.text = "0";
                return;
            }
            if (string.IsNullOrEmpty(promotionType))
            {
                Debug.LogWarning("[PromotionButton] promotionType is null or empty in _RefreshCounter.", this);
                availableCountLabel.text = "0";
                return;
            }
            availableCountLabel.text = picker.board.GetPiecesAvailableCount(promotionType).ToString();
        }

        public override void Interact()
        {
            if (picker == null)
            {
                Debug.LogWarning("[PromotionButton] picker is null in Interact.", this);
                return;
            }
            if (string.IsNullOrEmpty(promotionType))
            {
                Debug.LogWarning("[PromotionButton] promotionType is null or empty in Interact.", this);
                return;
            }
            picker.PromoteTo(promotionType);
        }
    }

}
