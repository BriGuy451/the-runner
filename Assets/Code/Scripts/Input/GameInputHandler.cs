namespace InputSystem {
  using UnityEngine;

  public class GameInputHandler : MonoBehaviour {

    private InputSystem_Actions inputActions;

    private void Awake() {
      Cursor.lockState = CursorLockMode.Confined;
      inputActions = new InputSystem_Actions();
      inputActions.Enable();
    }

    public (float, float) GetLookAxis() {
      float verticalAxis = inputActions.Player.LookVertical.ReadValue<float>();
      float horizontalAxis = inputActions.Player.LookHorizontal.ReadValue<float>();

      Debug.Log("Vertical Axis: " + verticalAxis);
      Debug.Log("Horizontal Axis: " + horizontalAxis);

      return (verticalAxis, horizontalAxis);
    }

    public Vector2 GetMovementFromKeyboard() {
      Vector2 movementActionValue = inputActions.Player.Walk.ReadValue<Vector2>();

      Debug.Log("Movement Action Value: " + movementActionValue);

      return movementActionValue;
    }


  }
}