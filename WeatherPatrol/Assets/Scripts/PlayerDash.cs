using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    private Rigidbody2D rb;

    public float dashSpeed;
	private float dashTime;
	public float startDashTime;
    private int direction;
    private bool isDashing = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashTime = startDashTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDashing)
        {
            if (dashTime <= 0) 
            {
                direction = 0;
                dashTime = startDashTime;
                rb.velocity = Vector2.zero;
                isDashing = false;
            }
            else
            {
                dashTime -= Time.deltaTime;

                if (direction == 1)
                {
                    rb.velocity += Vector2.right * dashSpeed;
                }
                else if (direction == 2)
                {
                    rb.velocity += Vector2.left * dashSpeed;
                }
            }
        }
    }

    public void Dash(CharacterController2D controller)
    {
        isDashing = true;
        if (controller.FacingRight())
        {
            direction = 1;
        } 
        else 
        {
            direction = 2;
        }
    }
}
