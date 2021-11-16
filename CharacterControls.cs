using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
//input system and interactions are needed to pull input directly from the input map
//for use in controls, would also need processors if using said inputsystem processors

//ensures script is attached to a character controller
[RequireComponent(typeof(CharacterController))]

public class CharacterControls : MonoBehaviour
{
    //sets up for character controller use, stores velocity, and checks for player on ground
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    //store terrain collider variable, worldposition for ray, and setup ray variable
    TerrainCollider terrainCollider;
    Vector3 worldPosition;
    Ray ray;

    //store action input values for use
    private Vector2 m_Look;
    private Vector2 m_Move;
    private Vector2 m_MouseLook;

    //defines controller default values
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;

    //camera transform location, used to indicate move direction in move script as based on camera
    private Transform cameraMainTransform;
    //main sceme camera defined for use in mouse controls
    private Camera cam;

    //reads assigned action map for input and converts into value for use in controls
    //these REQUIRE input defining on assigned object as well
    public void OnMove(InputAction.CallbackContext context)
    {
        m_Move = context.ReadValue<Vector2>();
    }

    //reads assigned action map for input and converts into value for use in controls
    //these REQUIRE input defining on assigned object as well
    public void OnLook(InputAction.CallbackContext context)
    {
        m_Look = context.ReadValue<Vector2>();
    }

    //reads assigned action map for input and converts into value for use in controls
    //these REQUIRE input defining on assigned object as well
    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (controller.isGrounded)
                {
                    Jump();
                }
                break;
        }
    }

    //reads assigned action map for input and converts into value for use in controls
    //these REQUIRE input defining on assigned object as well
    public void OnMouseLook(InputAction.CallbackContext context)
    {
        m_MouseLook = context.ReadValue<Vector2>();
    }


    //initializes systems, instantiates controller, camera transform,
    //and main camera for use before first update
    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        cameraMainTransform = Camera.main.transform;
        cam = Camera.main;

        //terrain collider information for mouselook ray
        terrainCollider = Terrain.activeTerrain.GetComponent<TerrainCollider>();

    }

    void Update()
    {

        //update look then movement
        MouseLook(m_MouseLook);
        Look(m_Look);
        Move(m_Move);

        //updates current position based on time and gravity
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        //checks if controller is on the ground, used for jump action
        //also stops gravity from being applied after reaching ground
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }
    }

    private void Move(Vector2 move)
    {
        //check if there is input to update, should prevent changes without input
        if (move.sqrMagnitude < 0.01)
            return;
        //Read movement value, convert to Vector3 without height input
        //then moves controller based on inputs
        Vector3 movement = new Vector3(move.x, 0, move.y);
        movement = cameraMainTransform.forward * movement.z + cameraMainTransform.right * movement.x;
        movement.y = 0f;
        controller.Move(movement * Time.deltaTime * playerSpeed);
    }

    private void Look(Vector2 look)
    {
        //Look Action - controllerinput
        Vector3 lookdir = new Vector3(look.x, 0, look.y);
        //check if there is input to update, should prevent changes without input
        if (lookdir.sqrMagnitude < 0.01)
            return;
        controller.transform.rotation = Quaternion.LookRotation(lookdir);
    }

    private void Jump()
    {
        //Jump Action - changes height of player
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

    }

    private void MouseLook(Vector2 mouselook)
    {
        //check if there is input to update, should prevent changes without input
        if (mouselook.sqrMagnitude < 0.01)
            return;
        ray = cam.ScreenPointToRay(mouselook);
        RaycastHit hitData;

        if(terrainCollider.Raycast(ray, out hitData, 1000))
        {
            worldPosition = hitData.point;
        }
        //stores a vector3 to use for .LookAt function(doesn't work otherwise) also ensures turning is relative to the player model height
        Vector3 lookat = new Vector3(worldPosition.x, controller.transform.position.y, worldPosition.z);
        controller.transform.LookAt(lookat);
    }


}
