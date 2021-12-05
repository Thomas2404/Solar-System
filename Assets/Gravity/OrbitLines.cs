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
    public float thetaScale = 0.01f;
    public bool stopAfterCollision;

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
        Dictionary<VirtualBody, bool> collisions = new Dictionary<VirtualBody, bool>();
        Dictionary<VirtualBody, int> collisionStep = new Dictionary<VirtualBody, int>();
        int virtualBodiesLength = virtualBodies.Length;

        // Initialize virtual bodies (don't want to move the actual bodies)
        for (int i = 0; i < virtualBodies.Length; i++) {
            virtualBodies[i] = new VirtualBody (bodies[i]);
            //numSteps is the number of iterations to do, set in the inspector. Default is 1000.
            drawPoints[i] = new Vector3[numSteps];

            if (bodies[i] == centralBody && relativeToBody) {
                referenceFrameIndex = i;
                referenceBodyInitialPosition = virtualBodies[i].position;
            }
            
            collisions.Add(virtualBodies[i], false);
        }
        
        // Simulate for every iteration.
        for (int step = 0; step < numSteps; step++) {
            //If using relativeToBody, reference is the body, else it's nothing.
            Vector3 referenceBodyPosition = (relativeToBody) ? virtualBodies[referenceFrameIndex].position : Vector3.zero;
            // Update velocities for every attraction object
            for (int i = 0; i < virtualBodies.Length; i++) {
                virtualBodies[i].velocity += CalculateAcceleration (i, virtualBodies, collisions) * timeStep;
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
            
            

            if (collisionPoints.Count < virtualBodies.Length)
            {
                for (int length = 0; length < virtualBodies.Length; length++)
                {
                    Vector3 bodyPosition = drawPoints[length][step];
                    for (int i = 0; i < drawPoints.Length; i++)
                    {
                        if (virtualBodies[length] != virtualBodies[i] && collisions[virtualBodies[length]] == false)
                        {
                            Vector3 secondBodyPostion = drawPoints[i][step];
                            if (Vector3.Distance(bodyPosition, secondBodyPostion) < virtualBodies[length].radius + virtualBodies[i].radius)
                            {
                                if (!collisionStep.ContainsKey(virtualBodies[i]) || !(step - collisionStep[virtualBodies[i]] > 5))
                                {
                                    collisionPoints.Add(bodyPosition);
                                    bodyRadius.Add(virtualBodies[length].radius);
                                    collisions[virtualBodies[length]] = true;
                                    collisionStep[virtualBodies[length]] = step;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Draw paths
        for (int bodyIndex = 0; bodyIndex < virtualBodiesLength; bodyIndex++) {
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

                if (stopAfterCollision)
                {
                    for (int i = 0; i < drawPoints[bodyIndex].Length; i++)
                    {
                        if (collisionPoints.Contains(drawPoints[bodyIndex][i]))
                        {
                            Array.Resize(ref drawPoints[bodyIndex], i);
                        }
                    }
                }
                
                for (int i = 0; i < drawPoints[bodyIndex].Length - 1; i++) {
                    
                    Debug.DrawLine (drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
                }
                
                //for every point in collision point, draw a circle
                for (int i = 0; i < collisionPoints.Count; i++)
                {

                    var radius = bodyRadius[i];
                    var size = (int) ((2.0f * Mathf.PI) / thetaScale) + 1;
                    var theta = 0f;
                    var circlePositions = new Vector3[size];
                    Vector3 pos;

                    for (int j = 0; j < size; j++)
                    {
                        theta += (2.0f * Mathf.PI * thetaScale);
                        float x = (radius / 3) * Mathf.PI * Mathf.Cos(theta);
                        float z = (radius / 3) * Mathf.PI * Mathf.Sin(theta);
                        x += collisionPoints[i].x;
                        z += collisionPoints[i].z;
                        pos = new Vector3(x, 0, z);
                        circlePositions[j] = pos;
                    }

                    for (int j = 0; j < circlePositions.Length - 1; j++)
                    {
                        Debug.DrawLine(circlePositions[j], circlePositions[j + 1], Color.red);
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

    Vector3 CalculateAcceleration (int i, VirtualBody[] virtualBodies, Dictionary<VirtualBody, bool> collisions) {
        Vector3 acceleration = Vector3.zero;
        for (int j = 0; j < virtualBodies.Length; j++) {
            if (i == j) {
                continue;
            }

            if (!collisions[virtualBodies[j]])
            {
                Vector3 direction = (virtualBodies[j].position - virtualBodies[i].position);
                float distance = (virtualBodies[j].position - virtualBodies[i].position).sqrMagnitude;
                acceleration += direction * (Universal.gravitationalConstant * (virtualBodies[j].mass * virtualBodies[i].mass) / distance);
            }
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