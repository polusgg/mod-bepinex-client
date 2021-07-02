using UnityEngine;

namespace PolusGG.Behaviours.Ino {
    public class PolusPointOfInterest : MonoBehaviour {
        private static readonly int UILayer = LayerMask.NameToLayer("UI");
        private ArrowBehaviour arrow;
        private PolusNetworkTransform cnt;

        private void Start() {
            arrow = gameObject.AddComponent<ArrowBehaviour>();
            cnt = GetComponent<PolusNetworkTransform>();
        }

        private void Update() {
            if (arrow.image == null) {
                arrow.image = GetComponent<PolusGraphic>().renderer;
            }

            gameObject.layer = UILayer;

            // if these stupid arrows aren't pointing to the point they should, they're pointing to 20, 20
            arrow.target = transform.parent ? transform.parent.position : new Vector3(20, 20);
        }
    }
}