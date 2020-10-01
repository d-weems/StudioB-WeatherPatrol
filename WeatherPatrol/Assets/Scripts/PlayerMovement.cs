using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;

	public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;
	bool crouch = false;
    

    // public Animator animator;
    // public SpriteRenderer sprite;
	
	// Update is called once per frame
	void Update () 
    {
        
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        // if (Input.GetButtonDown("Crouch"))
        // {
        // 	crouch = true;
        // } else if (Input.GetButtonUp("Crouch"))
        // {
        // 	crouch = false;
        // }
        
	}

	void FixedUpdate ()
	{
        // animator.SetFloat("speed", Mathf.Abs(horizontalMove));
        // animator.SetBool("isGrounded", controller.isGrounded());
		// Move our character
		controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
		jump = false;
	}
}