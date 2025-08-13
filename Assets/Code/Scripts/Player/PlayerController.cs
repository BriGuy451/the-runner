using System;
using InputSystem;
using UnityEngine;

public class PlayerController : MonoBehaviour {

  [Header("Movement Properties")]
  /* Player Speed Properties */
  // Definite movement properties
  [SerializeField] private float walkSpeed;
  [SerializeField] private float sprintSpeed;
  [SerializeField] private float crouchSpeed;

  [Space(10)]
  // Possible movement properties
  [SerializeField] private float slideSpeed;
  [SerializeField] private float jumpSpeed;
  [SerializeField] private float diveSpeed;

  [Space(5)]
  // Possible look properties
  [SerializeField] private float lookSpeed;

  [Header("Attribute Properties")]
  /* Player Attribute Properties */
  // Definite attributes
  [SerializeField] private int health;

  [Header("Player Orientation Properties")]
  /* Player Orientation */
  [SerializeField] private Vector3 playerPosition;
  [SerializeField] private Quaternion playerRotation;
  [SerializeField] private Vector3 playerScale;
  [SerializeField] private Vector3 playerDirection;
  [Range(0, 180)] public int playerAngle;

  /* Reference Variables */
  [SerializeField] private GameInputHandler gameInputHandler;
  private Camera currentCamera;
  private Camera lastCamera; // just an experiment thought; have a copy of the camera data from the last frame.
  private Vector3 lastPosition = Vector3.zero;
  private Vector3 lastLookDirection = Vector3.zero;
  private float lastSpeed;
  private int lastAngle;
  private MovementEnum.MovementAction movementAction;
  private MovementEnum.CharacterState characterState;

  /* Functionality Memory Variables */
  private bool isJumping = false;
  private bool isCrouched = false;
  private bool isSprinting = false;
  private bool isDiving = false;
  private bool isSliding = false;
  private bool isWalking = false;

  private void Awake() {
    // speed initialization 
    lookSpeed = 7f;
    walkSpeed = 5f;
    sprintSpeed = 9f;
    crouchSpeed = 3f;
    slideSpeed = 7f;
    jumpSpeed = 5f;
    diveSpeed = 5f;

    // positional initialization
    playerPosition = transform.position;
    playerRotation = transform.rotation;
    playerScale = transform.localScale;

    // camera reference
    currentCamera = GetComponentInChildren<Camera>();

    // state initialization
    characterState = MovementEnum.CharacterState.STANDING;
    movementAction = MovementEnum.MovementAction.IDLE;
  }

  private void Start() {

    GameInputHandler.Instance.JumpPerformed += Player_OnJumpPerformed;

    GameInputHandler.Instance.CrouchPerformed += Player_OnCrouchPerformed;
    GameInputHandler.Instance.CrouchCancelled += Player_OnCrouchCancelled;

    GameInputHandler.Instance.SprintPerformed += Player_OnSprintPerformed;
    GameInputHandler.Instance.SprintCanceled += Player_OnSprintCancelled;

    GameInputHandler.Instance.DivePerformed += Player_OnDivePerformed;

    GameInputHandler.Instance.SlidePerformed += Player_OnSlidePerformed;
  }

  private void Update() {
    // PlayerLook();
    PlayerMovement();
  }

  /* Problems with PlayerLook: 
    1) There are no constraints on up or down rotation angle.
    2) How to get a little dot cursor in the middle.
  */
  private float verticalAxisLookMovement = 0;
  private float horizontalAxisLookMovement = 0;
  private void PlayerLook() {
    (float, float) lookAxis = gameInputHandler.GetVerticalAndHorizontalLookAxis();

    // filter raw input data with linear interpolation to create a smoother value transition [low filter pass, makes data less noisy]
    float mouseSnappiness = 10; // this number makes the mouse movement tighter for interpolate value in the lerp
    verticalAxisLookMovement = Mathf.Lerp(verticalAxisLookMovement, lookAxis.Item1, mouseSnappiness * Time.deltaTime);  // does this make a filter for the input value? also what is this snappiness value? 
    horizontalAxisLookMovement = Mathf.Lerp(horizontalAxisLookMovement, lookAxis.Item2, mouseSnappiness * Time.deltaTime); // does this make a filter for the input value? also what is this snappiness value?

    // get current values for the player gameObject rotation, convert from quaternion to euler angle for ease of use
    Quaternion currentRotation = playerRotation;
    Vector3 currentRotationEulerAngles = currentRotation.eulerAngles;
    Vector3 toRotationVector = Vector3.zero;

    toRotationVector.x = currentRotationEulerAngles.x + -verticalAxisLookMovement; // around x axis, which is up & down rotation || NEED TO FIX THIS INVERSION, NATURAL NUMBER IS INVERTED
    toRotationVector.y = currentRotationEulerAngles.y + horizontalAxisLookMovement; // around y axis, which is left & right rotation

    Quaternion toRotationQuaternion = Quaternion.Euler(toRotationVector.x, toRotationVector.y, toRotationVector.z); // convert modified euler angles to quaternion type to pass to Quaternion Lerp function.

    // lerp rotation and assign the rotation change to the transform of the player game object.
    currentRotation = Quaternion.Lerp(currentRotation, toRotationQuaternion, lookSpeed * Time.deltaTime);
    transform.rotation = currentRotation;

    // update playerRotation variable with current rotation values
    playerRotation = currentRotation;
  }

  private void PlayerMovement() {
    Vector2 playerMovementInput = gameInputHandler.GetMovementFromKeyboard();
    Debug.Log($"PlayerMovementInput: X = {playerMovementInput.x} | Z = {playerMovementInput.y}");
  }

  private bool logInputEvents = false;
  private void Player_OnCrouchPerformed(object sender, EventArgs e) {
    isCrouched = true;
    if (logInputEvents)
      Debug.Log("OnCrouchPerformed isCrouched: " + isCrouched);
  }
  private void Player_OnCrouchCancelled(object sender, EventArgs e) {
    isCrouched = false;
    if (logInputEvents)
      Debug.Log("OnCrouchCancelled isCrouched: " + isCrouched);
  }

  private void Player_OnSprintPerformed(object sender, EventArgs e) {
    if (isWalking) isSprinting = true;
    if (logInputEvents)
      Debug.Log("OnSprintPerformed isSprinting: " + isSprinting);
  }
  private void Player_OnSprintCancelled(object sender, EventArgs e) {
    if (isWalking) isSprinting = false;
    if (logInputEvents)
      Debug.Log("OnSprintCancelled isSprinting: " + isSprinting);
  }

  private void Player_OnDivePerformed(object sender, EventArgs e) {
    if (!isDiving && isSprinting) isDiving = true;
    if (logInputEvents)
      Debug.Log("OnDivePerformed isDiving: " + isDiving);
  }

  private void Player_OnSlidePerformed(object sender, EventArgs e) {
    if (!isSliding && isSprinting) isSliding = true;
    if (logInputEvents)
      Debug.Log("OnSlidePerformed isSliding: " + isSliding);
  }

  private void Player_OnJumpPerformed(object sender, EventArgs e) {
    if (!isJumping) isJumping = true;
    if (logInputEvents)
      Debug.Log("OnJumpPerformed isJumping: " + isJumping);
  }
}