using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityObject : MonoBehaviour
{
    public static List<GravityObject> GravityObjects;
    private GravityObject centralBody;
    public Rigidbody rb;
    private Rigidbody centralBodyRb;
    [SerializeField]
    public Vector3 initialVelocity;
    public float radius;
    
    public Vector3 velocity { get; private set; }
    private Vector3 centralBodyInitialPostion;
    public GameObject objectWithOrbitLines;
    private OrbitLines refScript;
    private Vector3 newPos;
    private int count;
    private bool relativeToBody;
    

    private void Awake()
    {
        velocity = initialVelocity;

        refScript = objectWithOrbitLines.GetComponent<OrbitLines>();

        centralBody = refScript.centralBody;
        relativeToBody = refScript.relativeToBody;
        centralBodyRb = getRb(centralBody);
        centralBodyInitialPostion = centralBodyRb.position;
        
        radius = rb.mass / 10;
        gameObject.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);
    }

    private void OnEnable()
    {
        if (GravityObjects == null)
        {
            GravityObjects = new List<GravityObject>();
        }
        GravityObjects.Add(this);

        Debug.Log(relativeToBody);
    }

    private Rigidbody getRb(GravityObject gravityObject) {
        return gravityObject.rb;
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
            count = i;
            rb.MovePosition(newPos);
        }
    }

    void Attract()
    {
        foreach (GravityObject body in GravityObjects)
        {
            if (collision)
            {
                velocity += otherVelocity;
                collision = false;
            }
            if (body != this)
            {
                Rigidbody attractedBody = body.rb;
                
                Vector3 direction = (attractedBody.position - rb.position);
                float distance = (attractedBody.position - rb.position).sqrMagnitude;
                    
                if (distance == 0f) return;
                        
                Vector3 acceleration = direction * (Universal.gravitationalConstant * (rb.mass * attractedBody.mass) / distance);
                velocity += acceleration * Universal.physicsTimeStep;
                
                if (relativeToBody && refScript.centralBody != null) {
                     var centralBodyOffset = centralBodyRb.position - centralBodyInitialPostion;
                     newPos = rb.position + velocity * Universal.physicsTimeStep;
                     newPos -= centralBodyOffset;
            
                }
            }

            if (!relativeToBody)
            {
                newPos = rb.position + velocity * Universal.physicsTimeStep;
            }
        }
    }
    Dictionary<GravityObject, float> collisions;
    private bool collision;
    private Vector3 otherVelocity;

    private void OnTriggerEnter(Collider other)
    {
        if (relativeToBody && other.gameObject == refScript.centralBody)
        {
            relativeToBody = false;
        }

        Rigidbody collisionRb = other.GetComponent<GravityObject>().rb;
        if (rb.mass > collisionRb.mass)
        {
            rb.mass += collisionRb.mass;
            radius = rb.mass / 10;
            
            gameObject.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);

            collision = true;
            otherVelocity = collisionRb.velocity;

            Destroy(other.gameObject);
        }
    }
}
 