using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Emychess.Interactions
{
    public class SendEventOnInteract : UdonSharpBehaviour
    {
        public UdonBehaviour targetBehaviour;
        public string eventName;
        public override void Interact()
        {
            if (targetBehaviour == null)
            {
                Debug.LogWarning("[SendEventOnInteract] targetBehaviour is null in Interact.", this);
                return;
            }
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("[SendEventOnInteract] eventName is null or empty in Interact.", this);
                return;
            }
            targetBehaviour.SendCustomEvent(eventName);
        }
    }

}
