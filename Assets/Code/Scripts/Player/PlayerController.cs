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


  private void Awake() {

  }

  private void Start() {

  }

  private void Update() {
    gameInputHandler.GetLookAxis();
    gameInputHandler.GetMovementFromKeyboard();
  }

  private void FixedUpdate() {

  }
}
