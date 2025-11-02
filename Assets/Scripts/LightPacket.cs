using UnityEngine;

public class LightPacket<T>
{
    const int c = 1;           // Speed of Light (ly/y)
    public float arrivalTime;  // When it will arrive (computed)
    public T payload;          // The information being sent

    public LightPacket(Vector3 origin, float emissionTime, T payload)
    {
        this.payload = payload;
        arrivalTime = emissionTime + Vector3.Distance(origin, GameManager.instance.referenceTransform.position) / c;
    }
}
