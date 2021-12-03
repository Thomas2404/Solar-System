using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
public class OrbitLines : MonoBehaviour {

    public int numSteps = 1000;
    public float timeStep = 0.01f;
    public bool usePhysicsTimeStep;

    public bool relativeToBody;
    public GravityObject centralBody;
    public float width = 100;
    public bool useThickLines;
    
    //When play is pressed, hide the orbits.
    void Start () {
        if (Application.isPlaying) {
            HideOrbits ();
        }
    }

    //When something is updated, if the app isn't playing, draw orbits.
    void Update () {

        if (!Application.isPlaying) {
            DrawOrbits ();
        }
    }

    void DrawOrbits ()
    {
        //Make an array of all GravityObject objects
        GravityObject[] bodies = FindObjectsOfType<GravityObject> ();
        //Make a local array, virtualBodies with the same length as bodies.
        var virtualBodies = new VirtualBody[bodies.Length];
        //Make another local array, this time of Vector3s with the same length as bodies.
        var drawPoints = new Vector3[bodies.Length][];
        var collisionPoints = new List<Vector3>();
        var bodyRadius = new List<float>();
        int referenceFrameIndex = 0;
        Vector3 referenceBodyInitialPosition = Vector3.zero;

        // Initialize virtual bodies (don't want to move the actual bodies)
        for (int i = 0; i < virtualBodies.Length; i++) {
            virtualBodies[i] = new VirtualBody (bodies[i]);
            //numSteps is the number of iterations to do, set in the inspector. Default is 1000.
            drawPoints[i] = new Vector3[numSteps];

            if (bodies[i] == centralBody && relativeToBody) {
                referenceFrameIndex = i;
                referenceBodyInitialPosition = virtualBodies[i].position;
            }
        }

        // Simulate for every iteration.
        for (int step = 0; step < numSteps; step++) {
            //If using relativeToBody, reference is the body, else it's nothing.
            Vector3 referenceBodyPosition = (relativeToBody) ? virtualBodies[referenceFrameIndex].position : Vector3.zero;
            // Update velocities for every attraction object
            for (int i = 0; i < virtualBodies.Length; i++) {
                virtualBodies[i].velocity += CalculateAcceleration (i, virtualBodies) * timeStep;
            }
            // Update positions
            for (int i = 0; i < virtualBodies.Length; i++) {
                Vector3 newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                virtualBodies[i].position = newPos;
                if (relativeToBody) {
                    var referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                    newPos -= referenceFrameOffset;
                }
                if (relativeToBody && i == referenceFrameIndex) {
                    newPos = referenceBodyInitialPosition;
                }

                drawPoints[i][step] = newPos;
            }
        }

        for (int step = 0; step < numSteps; step++)
        {
            for (int length = 0; length < virtualBodies.Length; length++)
            {
                Vector3 bodyPosition = drawPoints[length][step];
                for (int i = 0; i < drawPoints.Length; i++)
                {
                    
                    if (virtualBodies[length] != virtualBodies[i])
                    {
                         Vector3 secondBodyPostion = drawPoints[i][step];
                         //Debug.Log("Distance: " + Vector3.Distance(bodyPosition, secondBodyPostion));
                         //Debug.Log("Radius: " + virtualBodies[length].radius + virtualBodies[i].radius);
                        
                         if (Vector3.Distance(bodyPosition, secondBodyPostion) < virtualBodies[length].radius + virtualBodies[i].radius)
                         {
                             collisionPoints.Add(bodyPosition);
                             bodyRadius.Add(virtualBodies[length].radius);
                         }
                    }
                }
            }
        }

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++) {
            var pathColour = bodies[bodyIndex].gameObject.GetComponentInChildren<MeshRenderer> ().sharedMaterial.color; //

            if (useThickLines) {
                var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer> ();
                lineRenderer.enabled = true;
                lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                lineRenderer.SetPositions (drawPoints[bodyIndex]);
                lineRenderer.startColor = pathColour;
                lineRenderer.endColor = pathColour;
                lineRenderer.widthMultiplier = width;
            } else {
                for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++) {
                    Debug.DrawLine (drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
                }
                
                //for every point in collision point, draw a circle
                for (int i = 0; i < collisionPoints.Count; i++)
                {
                    var boxPositions = new Vector3[5];
                   
                    boxPositions[0] = collisionPoints[i] + (Vector3.left + Vector3.back) / 2;
                    boxPositions[1] = collisionPoints[i] + (Vector3.left + Vector3.forward) / 2;
                    boxPositions[2] = collisionPoints[i] + (Vector3.right + Vector3.forward) / 2;
                    boxPositions[3] = collisionPoints[i] + (Vector3.right + Vector3.back) / 2;
                    boxPositions[4] = collisionPoints[i] + (Vector3.left + Vector3.back) / 2;

                    for (int j = 0; j < boxPositions.Length - 1; j++)
                    {
                        Debug.DrawLine(boxPositions[j], boxPositions[j + 1], Color.red);
                    }
                }

                // Hide renderer
                var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer> ();
                if (lineRenderer) {
                    lineRenderer.enabled = false;
                }
            }

        }
    }

    Vector3 CalculateAcceleration (int i, VirtualBody[] virtualBodies) {
        Vector3 acceleration = Vector3.zero;
        for (int j = 0; j < virtualBodies.Length; j++) {
            if (i == j) {
                continue;
            }
            Vector3 direction = (virtualBodies[j].position - virtualBodies[i].position);
            float distance = (virtualBodies[j].position - virtualBodies[i].position).sqrMagnitude;
            acceleration += direction * (Universal.gravitationalConstant * (virtualBodies[j].mass * virtualBodies[i].mass) / distance);
        }
        return acceleration;
    }

    void HideOrbits () {
        GravityObject[] bodies = FindObjectsOfType<GravityObject> ();

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++)
        {
            var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
    }

    void OnValidate () {
        if (usePhysicsTimeStep) {
            timeStep = Universal.physicsTimeStep;
        }
    }
    
    class VirtualBody {
        public Vector3 position;
        public Vector3 velocity;
        public float mass;
        public float radius;

        public VirtualBody (GravityObject body) {
            position = body.transform.position;
            velocity = body.initialVelocity;
            mass = body.rb.mass;
            radius = body.radius;
        }
    }
}