using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 10f;
    private Vector3 velocity;

    private void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        //playerInput.Normalize();
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        //velocity += acceleration * Time.deltaTime;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        //if (velocity.x < desiredVelocity.x)
        //{
        //    velocity.x += Mathf.Min(velocity.x + maxSpeedChange, desiredVelocity.x);
        //}
        //else if (velocity.x > desiredVelocity.x)
        //{
        //    velocity.x = Mathf.Max(velocity.x - maxSpeedChange, desiredVelocity.x);
        //}

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        Vector3 displacement = velocity * Time.deltaTime;
        transform.localPosition += displacement;
    }
}
