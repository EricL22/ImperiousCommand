using System.Collections.Generic;
using UnityEngine;

public abstract class RelatEnt : RelativisticEntity
{
    public Vector3 truePosition;

    private readonly Queue<LightPacket<Vector3>> packetQueue = new Queue<LightPacket<Vector3>>();

    protected override void Update()
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

    public override void Move(float deltaTime)
    {
        if (!isMoving) return;

        Vector3 dx = GetVelocity(deltaTime);

        truePosition += dx * Time.deltaTime;

        // Check if close to destination
        if (dx.magnitude == 0f)
        {
            isMoving = false;
            currentLocation = target;
            target = null;
            //transform.position = currentLocation.position + arrivalOffset;
        }
    }

    public override void SetDestination(Vector3 newDestination)
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
}
