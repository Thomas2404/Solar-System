using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityObject : MonoBehaviour
{
    public static List<GravityObject> GravityObjects;
    private GravityObject[] attractions;
    public Rigidbody rb;
    [SerializeField]
    public Vector3 initialVelocity;

    public Vector3 velocity { get; private set; }

    private bool startAttraction;

    private void Awake()
    {
        velocity = initialVelocity;
    }

    private void OnEnable()
    {
        if (GravityObjects == null)
        {
            GravityObjects = new List<GravityObject>();
        }
        GravityObjects.Add(this);
    }

    private void OnDisable()
    {
        GravityObjects.Remove(this);
    }

    private void FixedUpdate()
    {
        Attract();

        for (int i = 0; i < GravityObjects.Count; i++)
        {
            rb.MovePosition(rb.position + velocity * Universal.physicsTimeStep);
        }
    }

    void Attract()
    {
        foreach (GravityObject body in GravityObjects)
        {
            if (body != this)
            {
                Rigidbody attractedBody = body.rb;
                
                Vector3 direction = (attractedBody.position - rb.position);
                float distance = (attractedBody.position - rb.position).sqrMagnitude;
                    
                if (distance == 0f) return;
                        
                Vector3 acceleration = direction * (Universal.gravitationalConstant * (rb.mass * attractedBody.mass) / distance);
                velocity += acceleration * Universal.physicsTimeStep;
            }
        }
    }
}
 