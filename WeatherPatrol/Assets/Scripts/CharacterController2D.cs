using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Transform m_FrontCheck;
	[SerializeField] private Collider2D m_CrouchDisableCollider;				// A collider that will be disabled when crouching

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private bool m_TouchingFront;
	const float k_FrontRadius = .2f;
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private bool m_WallSliding = false;
	public float k_WallSlideSpeed;
	private bool m_WallJumping = false;
	public float m_xWallForce;
	public float m_yWallForce;
	public float m_WallJumpTime;
	private Vector3 velocity = Vector3.zero;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
	}


	private void FixedUpdate()
	{
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		Collider2D[] groundColliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < groundColliders.Length; i++)
		{
			if (groundColliders[i].gameObject != gameObject)
				m_Grounded = true;
		}

		// Check for walls in front of the player to slide down and wall jump off of
		m_TouchingFront = false;

		Collider2D[] frontColliders = Physics2D.OverlapCircleAll(m_FrontCheck.position, k_FrontRadius, m_WhatIsGround);
		for (int i = 0; i < frontColliders.Length; i++)
		{
			if (frontColliders[i].gameObject != gameObject)
				m_TouchingFront = true;
		}
	}


	public void Move(float move, bool crouch, bool jump)
	{
		Debug.Log(m_Grounded);
		// // If crouching, check to see if the character can stand up
		// if (!crouch)
		// {
		// 	// If the character has a ceiling preventing them from standing up, keep them crouching
		// 	if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
		// 	{
		// 		crouch = true;
		// 	}
		// }

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// // If crouching
			// if (crouch)
			// {
			// 	// Reduce the speed by the crouchSpeed multiplier
			// 	move *= m_CrouchSpeed;

			// 	// Disable one of the colliders when crouching
			// 	if (m_CrouchDisableCollider != null)
			// 		m_CrouchDisableCollider.enabled = false;
			// } else
			// {
			// 	// Enable the collider when not crouching
			// 	if (m_CrouchDisableCollider != null)
			// 		m_CrouchDisableCollider.enabled = true;
			// }

			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);

			// If the input is moving the player right and the player is facing left...
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...
		if (m_Grounded && jump)
		{
			// Add a vertical force to the player.
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}

		// New WallJumping Tech //
		if (m_TouchingFront && !m_Grounded && move != 0)
		{
			
			m_WallSliding = true;
		}
		else
		{
			m_WallSliding = false;
		}

		if(m_WallSliding)
		{
			Vector3 targetVelocity = new Vector2(move * 10f, Mathf.Clamp(m_Rigidbody2D.velocity.y, -k_WallSlideSpeed, float.MaxValue));
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);

			if(jump)
			{
				StartCoroutine(WallJumpTime());
			}
		}

		if(m_WallJumping)
		{
			Vector3 targetVelocity = new Vector2(m_xWallForce * -move * 10f, m_yWallForce);
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, m_MovementSmoothing);
		}
	}

	private IEnumerator WallJumpTime()
	{
		m_WallJumping = true;
		yield return new WaitForSeconds(m_WallJumpTime);
		m_WallJumping = false;
	}


	private void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

    public bool isGrounded()
    {
        return m_Grounded;
    }

    public bool FacingRight()
    {
        return m_FacingRight;
    }

	public bool isTouchingFront()
	{
		return m_TouchingFront;
	}
}