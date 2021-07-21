using UnityEngine;

namespace KartGame.KartSystems {

    public class KeyboardInput : BaseInput
    {
        public string Horizontal = "Horizontal";
        public string Vertical = "Vertical";

        public override Vector2 GenerateInput() {
            //Debug.Log("KeyboardInput:" + Input.GetAxis(Horizontal) + " " + Input.GetAxis(Vertical));
            return new Vector2 {
                x = Input.GetAxis(Horizontal),
                y = Input.GetAxis(Vertical)
            };
            
        }
    }
}
