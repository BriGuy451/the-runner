// To do: Get PlayerVisual to not rotate unless force to when coming into contact with slope, understand how the FPCC.cs code is not overwriting the children positions with Rotate.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
  private bool isFalling = false;

  /* Jump Logic Variables */
  private float jumpBaseY;
  private float jumpApex;
  private bool risingFlag;
  private bool fallingFlag;

  [SerializeField] public Transform _theOrb;

  private LayerMask groundLayer = 64;

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
    lastPosition = transform.position;
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

  Vector3 castOGizmo;
  bool backgroundFinished = false;
  private void Update() {
    // if (!backgroundFinished)
    //   MyFirstAwaitable();
    PlayerLook();
    PlayerMovement();

    TestGroundable();

    // isGrounded();
    // CalculateAngleBetweenThisAndTheOrb(_theOrb, transform);
  }

  void OnDrawGizmos() {
    if (castOGizmo != null) {
      Gizmos.color = Color.red;
      Gizmos.DrawRay(castOGizmo, transform.up * -1 * 1.5f);
      Gizmos.DrawRay(castOGizmo, (transform.up * -1 * 1.5f) + (transform.forward * .5f));
      Gizmos.DrawRay(castOGizmo, (transform.up * -1 * 1.5f) + (transform.forward * .5f * -1));
    }

    Gizmos.color = Color.blue;
    Gizmos.DrawRay(_theOrb.position, Vector3.down * 100f); // line straight down

    Gizmos.color = Color.yellow;
    Gizmos.DrawRay(_theOrb.position, transform.position); // playerOrbAngleRay

    // Gizmos.color = Color.black;
    // Gizmos.DrawRay(_theOrb.position - transform.position, transform.up * 30); //orbPlayerTransformUpRay

    // Debug.Log($"Player Orb Angle Ray: {playerOrbAngleRay}");
    // Debug.Log($"Orb Player Transform.Up Ray {orbPlayerTransformUpRay}");
  }

  private void CalculateAngleBetweenThisAndTheOrb(Transform orbTransform, Transform playerPosition) {
    Vector3 orbPosition = orbTransform.position;
    Vector3 currentPlayerPosition = playerPosition.transform.position;

    float angleBetweenPlayerAndTheOrb = Vector3.Angle(orbPosition, currentPlayerPosition);
    float originAngleBetweenTheOrb = Vector3.Angle(orbPosition - currentPlayerPosition, playerPosition.transform.up);

    Debug.Log($"Angle Between Player and The Orb (Raw) {angleBetweenPlayerAndTheOrb}");
    Debug.Log($"Angle Between Rotation Player Up and The Orb Minus Player Position (Processed) {originAngleBetweenTheOrb}");
  }

  private bool isGrounded() {
    bool groundedFlag = false;

    RaycastHit groundableHit;
    Vector3 castOrigin = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    float rayCastDistance = .5f;

    if (Physics.Raycast(castOrigin, transform.up * -1, out groundableHit, rayCastDistance, groundLayer)) {
      groundedFlag = true;

      Debug.Log("We hit the ground baby.");
      Debug.Log(groundableHit);
      Transform collidedObjectTransform = groundableHit.transform;
      Debug.Log(collidedObjectTransform.position);
      Debug.Log(collidedObjectTransform.rotation);
    }

    castOGizmo = castOrigin;

    return groundedFlag;
  }

  private void TestGroundable() {
    Vector3 castOrigin = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    castOGizmo = castOrigin;

    Ray centerBottomRay = new Ray(castOrigin, transform.up * -1 * 1.5f);
    Ray frontBottomRay = new Ray(castOrigin, (transform.up * -1 * 1.5f) + (transform.forward * .5f));
    Ray backBottomRay = new Ray(castOrigin, (transform.up * -1 * 1.5f) + (transform.forward * .5f * -1));

    List<GroundableSurfaceImpact> groundableSurfaceImpactsList = new List<GroundableSurfaceImpact>();

    GroundableSurfaceImpact centerBottomRayImpact = PerformRayCastAndCreateGroundableSurfaceImpactObject(centerBottomRay);
    GroundableSurfaceImpact frontBottomRayImpact = PerformRayCastAndCreateGroundableSurfaceImpactObject(frontBottomRay);
    GroundableSurfaceImpact backBottomRayImpact = PerformRayCastAndCreateGroundableSurfaceImpactObject(backBottomRay);

    if (centerBottomRayImpact != null) { Debug.Log($"Center: {centerBottomRayImpact.GetInstanceId()} Rotation: {centerBottomRayImpact.GetRotation()}"); }
    if (frontBottomRayImpact != null) { Debug.Log($"Front: {frontBottomRayImpact.GetInstanceId()} Rotation: {frontBottomRayImpact.GetRotation()}"); }
    if (backBottomRayImpact != null) { Debug.Log($"Back: {backBottomRayImpact.GetInstanceId()} Rotation: {backBottomRayImpact.GetRotation()}"); }

  }

  private GroundableSurfaceImpact PerformRayCastAndCreateGroundableSurfaceImpactObject(Ray rayToCast) {
    GroundableSurfaceImpact groundableSurfaceImpact = null;

    RaycastHit isHit;
    if (Physics.Raycast(rayToCast, out isHit, groundLayer)) {
      int gameObjectInstanceId = isHit.transform.gameObject.GetInstanceID();

      Vector3 gameObjectPosition = isHit.transform.position;
      Vector3 gameObjectRotation = isHit.transform.eulerAngles;
      Vector3 gameObjectScale = isHit.transform.localScale;

      Vector3 impactPoint = isHit.point;

      Collider impactCollider = isHit.collider;

      groundableSurfaceImpact = new GroundableSurfaceImpact(gameObjectInstanceId, gameObjectPosition, gameObjectRotation, gameObjectScale, impactPoint, impactCollider);
    }

    return groundableSurfaceImpact;
  }

  private async void MyFirstAwaitable() {
    List<int> integerList = new List<int>();

    int i = 0;
    while (i < 10) {
      i++;
      integerList.Add(i);
      Debug.Log("Yielding");
      await Task.Yield();
      backgroundFinished = true;
    }

    Debug.Log("Background Function Finish");
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
    // Debug.Log($"PlayerMovementInput: X = {playerMovementInput.x} | Z = {playerMovementInput.y}");
    //movementSpeed = walkSpeed (set to default)
    float movementSpeed = walkSpeed;


    float playerPositionY = lastPosition.y; // default y value that the player is usually at, but what about y changing functionality like jumping, diving, standing on elevated terrain, falling from elevated terrain
    float playerScaleY = 1f; // default scale, but what about scale change functionality like slide, dive, crouch

    float xMoveInput = playerMovementInput.x;
    float zMoveInput = playerMovementInput.y; // this is the z

    Vector3 transformRightWithDistance = xMoveInput != 0f ? transform.right * xMoveInput : Vector3.zero;
    Vector3 transformForwardWithDistance = zMoveInput != 0f ? transform.forward * zMoveInput : Vector3.zero;

    Vector3 currentPlayerPosition = lastPosition;
    Vector3 nextPlayerPosition = currentPlayerPosition + transformForwardWithDistance + transformRightWithDistance;

    nextPlayerPosition.y = playerPositionY;

    if (!currentPlayerPosition.Equals(nextPlayerPosition)) { movementAction = MovementEnum.MovementAction.WALKING; }

    if (isCrouched) {
      //currentState is crouched
      characterState = MovementEnum.CharacterState.CROUCHING;

      movementSpeed = crouchSpeed;

      playerScaleY /= 2f;

      // float crouchingPositionY = playerPositionY - .5f; //.5f is half the size of the scale height. Should sit the player on groudable surface.

      if (!Mathf.Approximately(playerScaleY, playerScale.y)) {
        // nextPlayerPosition.y = crouchingPositionY;
        // nextPlayerPosition = Vector3.Lerp(currentPlayerPosition, nextPlayerPosition, 4f * Time.deltaTime);

        Vector3 playerScaleWithValueChange = new Vector3(playerScale.x, playerScaleY, playerScale.z);
        playerScale = Vector3.Lerp(playerScale, playerScaleWithValueChange, 4f * Time.deltaTime); // Need to lerp from this scale to next scale
        transform.localScale = playerScale;
      }

      //any camera logic?
    }

    if (isJumping) {
      movementAction = MovementEnum.MovementAction.JUMPING;
      //jumpHeight = .85f
      //if jumpApex is null
      //jumpBaseY is currentPlayerY; risingFlag = true ; fallingFlag = false
      float jumpHeight = .85f;
      float jumpingPlayerPositionY = 0f;
      if (this.jumpApex.Equals(0.0f)) {
        this.jumpApex = playerPositionY + jumpHeight;
        this.jumpBaseY = playerPositionY;
        this.risingFlag = true;
      }
      else if (this.risingFlag) {
        //if jumpFlag
        //rising, playerPositionY set (mathf lerp value)
        //if at apex
        //risingFlag = false ; fallingFlag = true, playerPositionY set (mathf lerp value); falling to jumpBaseY or first collider surface (physics raycast until a surface is hit, keep that y)

        float interpolatedPlayerY = Mathf.Lerp(this.jumpBaseY, this.jumpApex, 2f * Time.deltaTime);
        jumpingPlayerPositionY = interpolatedPlayerY;

        if (interpolatedPlayerY >= jumpApex) {
          jumpApex = 0;
          risingFlag = false;
          fallingFlag = true;
        }

      }
      else if (this.fallingFlag) {
        //once ground is hit risingFlag & fallingFlag are false & jumpApex is reset to null & playerPositionY set to +1 on surface (didn't use isGrounded)

        float interpolatedPlayerY = Mathf.Lerp(playerPositionY, jumpBaseY, 2f * Time.deltaTime);
        jumpingPlayerPositionY = interpolatedPlayerY;

        if (interpolatedPlayerY <= jumpBaseY) {
          jumpBaseY = 0;
          risingFlag = false;
          fallingFlag = false;
          isJumping = false;
        }
      }

      playerPositionY = jumpingPlayerPositionY;
      nextPlayerPosition.y = playerPositionY;
    }

    if (isSprinting) {
      movementAction = MovementEnum.MovementAction.SPRINTING;
      movementSpeed = sprintSpeed;
      //logic to see if player is moving forward or other directions
      //movementSpeed = sprintSpeed based on movement direction
      //can do a check to see if the speed before this one was slower or faster to give the feeling of locomotion

      if (isDiving) {
        //need some form of timer / delay before this can be activated
        //scale is cut in half
        //yPosition is raised with the .85 meter unit
        //how to I increase the jump angle, derived by the y and z (maybe can use cos, sin, tan :) )
      }
      if (isSliding) {
        //need some form of timer / delay before this can be activated
        //scale is cut in half
        //yPosition is cut in half
        //movementSpeed needs to gradually taper off (possible use of async await)
      }
    }


    // Debug.Log($"Movement Speed ${movementSpeed}");
    if (!Mathf.Approximately(currentPlayerPosition.x, nextPlayerPosition.x) && !Mathf.Approximately(currentPlayerPosition.z, nextPlayerPosition.z)) {
      nextPlayerPosition = Vector3.Lerp(currentPlayerPosition, nextPlayerPosition, movementSpeed * Time.deltaTime);
    }

    transform.position = nextPlayerPosition;

    //this will allow me to have last frame, current frame, and next frame player position data
    lastPosition = nextPlayerPosition;
  }

  private bool logInputEvents = true;
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
    isSprinting = true;
    if (logInputEvents)
      Debug.Log("OnSprintPerformed isSprinting: " + isSprinting);
  }
  private void Player_OnSprintCancelled(object sender, EventArgs e) {
    isSprinting = false;
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