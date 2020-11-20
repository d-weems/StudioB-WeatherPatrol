using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;
	[SerializeField] private LayerMask m_WhatIsSwingable;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;							// A position marking where to check if the player is grounded.
	[SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Transform m_FrontCheck;

	const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private bool m_TouchingFront;
	const float k_FrontRadius = .2f;
	private Rigidbody2D m_Rigidbody2D;
	private HingeJoint2D m_HingeJoint2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private bool m_WallSliding = false;
	public float k_WallSlideSpeed;
	private bool m_WallJumping = false;
	public float m_xWallForce;
	public float m_yWallForce;
	public float m_WallJumpTime;
	private bool m_CanSwing = false;
	private bool m_IsSwinging = false;
	private GameObject m_SwingPoint;
	private Vector3 velocity = Vector3.zero;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		m_HingeJoint2D = GetComponent<HingeJoint2D>();
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

		// Check for swing points slightly above the player
		m_CanSwing = false;
		m_SwingPoint = null;

		Collider2D[] headColliders = Physics2D.OverlapCircleAll(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsSwingable);
		for (int i = 0; i < headColliders.Length; i++)
		{
			if (headColliders[i].gameObject != gameObject)
				m_CanSwing = true;
				m_SwingPoint = headColliders[i].gameObject;
		}
	}


	public void Move(float move, bool jump)
	{
		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{
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

		CheckWallJump(move, jump);
		CheckHookSwing(move, jump);
		
	}

	private void CheckHookSwing(float move, bool jump)
	{
		if(!m_Grounded && m_CanSwing && Input.GetKey(KeyCode.J))
		{
			m_HingeJoint2D.enabled = true;
			m_HingeJoint2D.connectedBody = m_SwingPoint.GetComponent<Rigidbody2D>();
			m_HingeJoint2D.connectedAnchor = new Vector2(0.0f, 0.0f);
			m_Rigidbody2D.freezeRotation= false;
			m_IsSwinging = true;
		}

		if(m_IsSwinging && Input.GetKey(KeyCode.J))
		{
			if(m_HingeJoint2D.jointAngle >= m_HingeJoint2D.limits.max || m_HingeJoint2D.jointAngle <= m_HingeJoint2D.limits.min)
			{
				JointMotor2D motor = m_HingeJoint2D.motor;
				motor.motorSpeed *= -1;
				m_HingeJoint2D.motor = motor;
			}
		}

		if(m_IsSwinging && !Input.GetKey(KeyCode.J))
		{
			m_HingeJoint2D.enabled = false;
			m_Rigidbody2D.rotation = 0.0f;
			m_Rigidbody2D.freezeRotation= true;
			m_IsSwinging = false;
		}
	}

	private void CheckWallJump(float move, bool jump)
	{
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