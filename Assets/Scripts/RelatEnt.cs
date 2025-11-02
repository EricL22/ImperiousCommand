using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RelatEnt : MonoBehaviour
{
    protected const int c = 1;    // Speed of Light (ly/y)

    public Vector3 arrivalOffset; // Offset position when the entity arrives
    protected Vector3 d;  // displacement vector from t = 0 to t = t3
    protected Vector3 p;  // initial position
    public float acceleration = 10f; // Acceleration (ly/y^2)
    public float maxWarp = 0.1f; // Maximum warp factor

    public bool isMoving { get; protected set; }
    public Transform target;
    public Transform currentLocation;
    protected float timeElapsed = 0f;
    public Vector3 truePosition;
    internal Transform referenceTransform { private get; set; }

    private readonly Queue<LightPacket<Vector3>> packetQueue = new Queue<LightPacket<Vector3>>();

    protected virtual void Start()
    {
        referenceTransform = GameManager.instance.referenceTransform;
        if (target != null)
            SetDestination(target.position);
    }

    protected virtual void Update()
    {
        timeElapsed += Time.deltaTime;
        Move(timeElapsed);

        float t = Time.time;

        // Create a new light packet from the object's current true position
        packetQueue.Enqueue(new LightPacket<Vector3>(truePosition, t, truePosition));

        // Check if any packets have "arrived"
        while (packetQueue.Count > 0 && packetQueue.Peek().arrivalTime <= t)
        {
            LightPacket<Vector3> packet = packetQueue.Dequeue();
            transform.position = packet.payload;
        }
    }

    public float t1, t2, t3, ta1, ta2, ta3;
    // position function
    float x(float t)
    {
        return Mathf.Pow(c, 2) / acceleration * (Mathf.Sqrt(1 + Mathf.Pow(acceleration * t / c, 2)) - 1);
    }
    // derivative of the position function
    float xp(float t)
    {
        return acceleration * t / Mathf.Sqrt(1 + Mathf.Pow(acceleration * t / c, 2));
    }

    public virtual void Move(float deltaTime)
    {
        if (!isMoving) return;

        Vector3 dx = GetVelocity(deltaTime);

        truePosition += dx * Time.deltaTime;
        //Debug.Log($"Velocity: {dx.x}");

        // Check if close to destination
        if (dx.magnitude == 0f)
        {
            isMoving = false;
            currentLocation = target;
            target = null;
            //transform.position = currentLocation.position + arrivalOffset;
        }

        //Vector3 actualVelocity = GetVelocity(deltaTime);
        //velocity = actualVelocity.magnitude;
    }

    public virtual void SetDestination(Vector3 newDestination)
    {
        if (isMoving) return;
        if (maxWarp == 0f)
        {
            Debug.LogWarning("Attempted to set a destination with maximum speed of 0!");
            return;
        }
        if ((transform.position - newDestination).magnitude < 0.1f)
        {
            Debug.LogWarning("Attempted to set a destination while already there!");
            return;
        }

        timeElapsed = 0f;
        transform.position = currentLocation.position;
        truePosition = transform.position;
        d = newDestination - transform.position;
        p = transform.position;

        // evaluate transition times
        float clampedWarpTime = maxWarp * c / acceleration * Mathf.Sqrt(1 / (1 - Mathf.Pow(maxWarp / c, 2)));
        // take the minimum of the time it takes to cover a distance D/2 and the time it takes to reach maxWarp
        t1 = Mathf.Min(c / acceleration * Mathf.Sqrt(Mathf.Pow(1 + acceleration * d.magnitude / (2 * Mathf.Pow(c, 2)), 2) - 1),
            clampedWarpTime);
        t2 = t1 + (d.magnitude - 2 * x(t1)) / xp(t1);
        t3 = t1 + t2;

        isMoving = true;
    }

    public void SetDestination(Transform newTarget)
    {
        if (isMoving || target == currentLocation) return;

        target = newTarget;
        SetDestination(target.position);
    }

    // for testing, assume we are moving for one second
    internal void SetDestination(Vector3 v, Vector3 x)
    {
        d = v - x;
    }

    // VERY COMPLEX Mathematical Stuff Follows. Proceed at your own peril.

    protected virtual Vector3 GetVelocity(float t)
    {
        float dx = 0f;

        // Phase 1: Acceleration
        if (t >= 0 && t < t1)
            dx = xp(t);

        // Phase 2: Coasting
        else if (t >= t1 && t < t2)
            dx = xp(t1);

        // Phase 3: Deceleration
        else if (t >= t2 && t <= t3)
            dx = -xp(t - t3);

        return d.normalized * dx;
    }

    public Vector3 GetPosition(float t)
    {
        float dx = 0f;

        // Phase 1: Acceleration
        if (t >= 0 && t < t1)
            dx = x(t);

        // Phase 2: Coasting
        else if (t >= t1 && t < t2)
            dx = x(t1) + xp(t1) * (t - t1);

        // Phase 3: Deceleration
        else if (t >= t2 && t <= t3)
            dx = d.magnitude - x(t - t3);

        return p + d.normalized * dx;
    }
}
