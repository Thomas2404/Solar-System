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
    private bool relativeToBody;
    

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

        relativeToBody = refScript.relativeToBody;
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
                     newPos -= centralBodyOffset;
            
                }
            }
            newPos = rb.position + velocity * Universal.physicsTimeStep;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (relativeToBody && other.gameObject == refScript.centralBody)
        {
            relativeToBody = false;
        }
        Destroy(gameObject);
    }
}
 