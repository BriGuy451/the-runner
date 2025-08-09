namespace InputSystem {
  using System;
  using System.Collections.Generic;
  using UnityEngine;

  public class GameInputHandler : MonoBehaviour {

    public static GameInputHandler Instance { get; private set; }

    private InputSystem_Actions inputActions;

    public event EventHandler JumpPerformed;

    // public event EventHandler sprintEvents;
    // private delegate void SprintEvents(object sender, EventArgs e);

    public event EventHandler CrouchPerformed;
    public event EventHandler CrouchCancelled;

    // public event EventHandler SprintStarted;
    public event EventHandler SprintPerformed;
    public event EventHandler SprintCanceled;

    // public event EventHandler DiveStarted;
    public event EventHandler DivePerformed;
    // public event EventHandler DiveCanceled;

    // public event EventHandler SlideStarted;
    public event EventHandler SlidePerformed;
    // public event EventHandler SlideCanceled;


    private void Awake() {
      Instance = this;

      Cursor.lockState = CursorLockMode.Locked;

      inputActions = new InputSystem_Actions();
      inputActions.Enable();

      inputActions.Player.Crouch.performed += Player_CrouchPerformed;
      inputActions.Player.Crouch.performed += Player_CrouchCancelled;

      inputActions.Player.Sprint.performed += Player_SprintPerformed;
      inputActions.Player.Sprint.canceled += Player_SprintCanceled;
      // inputActions.Player.Sprint.started += Player_SprintStarted;

      inputActions.Player.Jump.performed += Player_JumpPerformed;

      inputActions.Player.Dive.performed += Player_DivePerformed;
      // inputActions.Player.Dive.started += Player_DiveStarted;
      // inputActions.Player.Dive.canceled += Player_DiveCanceled;

      inputActions.Player.Slide.performed += Player_SlidePerformed;
      // inputActions.Player.Slide.started += Player_SlideStarted;
      // inputActions.Player.Slide.canceled += Player_SlideCanceled;
    }

    // Crouch Events (Start, Performed, Canceled)
    private void Player_CrouchPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
      CrouchPerformed?.Invoke(this, EventArgs.Empty);
    }
    private void Player_CrouchCancelled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
      CrouchCancelled?.Invoke(this, EventArgs.Empty);
    }
    // Sprint Events (Start, Performed, Canceled)
    private void Player_SprintPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
      SprintPerformed?.Invoke(this, EventArgs.Empty);
    }
    private void Player_SprintCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
      SprintCanceled?.Invoke(this, EventArgs.Empty);
    }
    // private void Player_SprintStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
    //   SprintStarted?.Invoke(this, EventArgs.Empty);
    // }


    // Jump Events (Start, Performed, Canceled)
    private void Player_JumpPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
      JumpPerformed?.Invoke(this, EventArgs.Empty);
    }

    // Dive Events (Start, Performed, Canceled)
    private void Player_DivePerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
      DivePerformed?.Invoke(this, EventArgs.Empty);
    }

    // Slide Events (Start, Performed, Canceled)
    private void Player_SlidePerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
      SlidePerformed?.Invoke(this, EventArgs.Empty);
    }

    // private void Player_DiveStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
    //   DiveStarted?.Invoke(this, EventArgs.Empty);
    // }
    // private void Player_DiveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
    //   DiveCanceled?.Invoke(this, EventArgs.Empty);
    // }

    // private void Player_SlideStarted(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
    //   SlideStarted?.Invoke(this, EventArgs.Empty);
    // }
    // private void Player_SlideCanceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
    //   SlideCanceled?.Invoke(this, EventArgs.Empty);
    // }

    public (float, float) GetVerticalAndHorizontalLookAxis() {
      float verticalAxis = inputActions.Player.LookVertical.ReadValue<float>();
      float horizontalAxis = inputActions.Player.LookHorizontal.ReadValue<float>();

      return (verticalAxis, horizontalAxis);
    }

    public Vector2 GetMovementFromKeyboard() {
      Vector2 movementActionValue = inputActions.Player.Walk.ReadValue<Vector2>();

      return movementActionValue;
    }

  }
}