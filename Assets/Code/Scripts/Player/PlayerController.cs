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

  // Possible attributes
  private int height;
  private int strength;
  private int agility;
  private int perception;
  private int intellect;

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
    lookSpeed = 7f;
  }

  private void Start() {

    GameInputHandler.Instance.JumpPerformed += Player_OnJumpPerformed;
    GameInputHandler.Instance.CrouchPerformed += Player_OnCrouchPerformed;

    GameInputHandler.Instance.SprintPerformed += Player_OnSprintPerformed;

    GameInputHandler.Instance.DivePerformed += Player_OnDivePerformed;

    GameInputHandler.Instance.SlidePerformed += Player_OnSlidePerformed;
  }


  bool logInputEvents = false;
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

  private void Update() {
    // PlayerLook();
    PlayerLook(true);
    PlayerMovement();

  }

  void LateUpdate() {

  }

  private void FixedUpdate() {

  }

  private void PlayerLook() {
    (float, float) lookAxis = gameInputHandler.GetVerticalAndHorizontalLookAxis();

    if (logInputEvents) {
      Debug.Log("Vertical: " + lookAxis.Item1);
      Debug.Log("Horizontal: " + lookAxis.Item2);
    }

    // multiply axis values by look speed.
    float verticalAxisMovement = lookAxis.Item1 * lookSpeed;
    float horizontalAxisMovement = lookAxis.Item2 * lookSpeed;

    // get current values for the player gameObject rotation, convert from quaternion to euler angle for ease of use
    Quaternion currentRotation = transform.rotation;
    Vector3 currentEulerAngles = currentRotation.eulerAngles;

    Debug.Log("1: " + verticalAxisMovement);
    Debug.Log("2: " + horizontalAxisMovement);

    currentEulerAngles.x += -verticalAxisMovement * Time.deltaTime; // around x axis, which is up & down rotation || NEED TO FIX THIS INVERSION, NATURAL NUMBER IS INVERTED
    currentEulerAngles.y += horizontalAxisMovement * Time.deltaTime; // around y axis, which is left & right rotation

    currentRotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, currentEulerAngles.z);
    transform.rotation = currentRotation;
  }

  float verticalAxisMovement = 0;
  float horizontalAxisMovement = 0;
  private void PlayerLook(bool performLerpLogic) {
    (float, float) lookAxis = gameInputHandler.GetVerticalAndHorizontalLookAxis();

    // multiply axis values by look speed.
    verticalAxisMovement = Mathf.Lerp(verticalAxisMovement, lookAxis.Item1, 10f * Time.deltaTime);  // does this make a filter for the input value? also what is this snappiness value? 
    horizontalAxisMovement = Mathf.Lerp(horizontalAxisMovement, lookAxis.Item2, 10f * Time.deltaTime); // does this make a filter for the input value? also what is this snappiness value?

    // get current values for the player gameObject rotation, convert from quaternion to euler angle for ease of use
    Quaternion currentRotation = transform.rotation;
    Vector3 currentRotationEulerAngles = currentRotation.eulerAngles;
    Vector3 toRotationVector = Vector3.zero;

    toRotationVector.x = (currentRotationEulerAngles.x + -verticalAxisMovement); // around x axis, which is up & down rotation || NEED TO FIX THIS INVERSION, NATURAL NUMBER IS INVERTED
    toRotationVector.y = (currentRotationEulerAngles.y + horizontalAxisMovement); // around y axis, which is left & right rotation

    Quaternion toRotationQuaternion = Quaternion.Euler(toRotationVector.x, toRotationVector.y, toRotationVector.z);

    // Vector3 eulerAngleRotateValue = Vector3.Lerp(currentRotationEulerAngles, toRotationVector, lookSpeed * Time.deltaTime);
    // Debug.Log($"z axis: {toRotationVector.z} & currentRotationEulerAngles z {currentRotationEulerAngles.z}");
    // transform.Rotate(toRotationVector);

    currentRotation = Quaternion.Lerp(currentRotation, toRotationQuaternion, lookSpeed * Time.deltaTime);
    transform.rotation = currentRotation;
  }

  private void PlayerMovement() {
    Vector2 playerMovementInput = gameInputHandler.GetMovementFromKeyboard();

  }
}