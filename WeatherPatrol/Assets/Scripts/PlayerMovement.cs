using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

	public CharacterController2D controller;
	public PlayerDash dashScript;

	public float runSpeed = 40f;

	float horizontalMove = 0f;
	bool jump = false;
    bool dash = false;

    // public Animator animator;
    // public SpriteRenderer sprite;
	
	// Update is called once per frame
	void Update () 
    {
        
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
		Debug.Log(horizontalMove);

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

		if (Input.GetKeyDown(KeyCode.J))
		{
			dash = true;
		}
        
	}

	void FixedUpdate ()
	{
		// Move our character
		controller.Move(horizontalMove * Time.fixedDeltaTime, jump);
		jump = false;
		
		if (dash)
		{
			dashScript.Dash(controller);
			dash = false;
		}
	}
}