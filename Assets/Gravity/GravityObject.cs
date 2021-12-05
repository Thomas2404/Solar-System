using System;
using System.Collections.Generic;
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
    

    private void Awake()
    {
        velocity = initialVelocity;

        refScript = objectWithOrbitLines.GetComponent<OrbitLines>();

        centralBody = refScript.centralBody;
        centralBodyRb = getRb(centralBody);
        centralBodyInitialPostion = centralBodyRb.position;
    }

    private void OnEnable()
    {
        if (GravityObjects == null)
        {
            GravityObjects = new List<GravityObject>();
        }
        GravityObjects.Add(this);
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
            rb.MovePosition(this.newPos);
        }
    }

    void Attract()
    {
        foreach (GravityObject body in GravityObjects)
        {
            if (body != this)
            {
                if (refScript.relativeToBody) {
                    Vector3 referenceBodyPosition = centralBodyRb.position;
                }

                Rigidbody attractedBody = body.rb;
                
                Vector3 direction = (attractedBody.position - rb.position);
                float distance = (attractedBody.position - rb.position).sqrMagnitude;
                    
                if (distance == 0f) return;
                        
                Vector3 acceleration = direction * (Universal.gravitationalConstant * (rb.mass * attractedBody.mass) / distance);
                velocity += acceleration * Universal.physicsTimeStep;
                
                if (refScript.relativeToBody) {
                     var centralBodyOffset = centralBodyRb.position - centralBodyInitialPostion;
                     this.newPos -= centralBodyOffset;
            
                }
            }
            this.newPos = rb.position + velocity * Universal.physicsTimeStep;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}
 