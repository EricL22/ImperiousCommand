using System.Collections.Generic;
using UnityEngine;

public class RelatShip : RelatEnt, ISelectableShip
{
    public string title { get; set; }
    public string prefix { get; set; }
    public float mass = 1f;  // Mass (tons)
    public float power = 1f; // Power currently stored by the warp core (exons)
    public float obsPower = 0f; // Power reported by the ship
    float maxPower; // Maximum power the warp core can store (exons)
    float maxAcceleration;
    // NOTE: 1 exon is the amount of power required to accelerate a mass of 1 ton to a factor of sqrt(3)/2 times c
    // In SI units this is tons * ly^2/y^2
    private readonly Queue<LightPacket<float>> packetQueue = new Queue<LightPacket<float>>();
    bool powVal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        SetRandomName();
        CalculateMaxWarp();
        maxPower = power;
        maxAcceleration = acceleration;
        base.Start();
        //FindAnyObjectByType<Fleet>()?.AddShip(this);
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (isMoving) powVal = false;

        base.Update();
        UsePower(timeElapsed);

        if (!isMoving && !powVal)
        {
            EvaluateEndOfJourneyPower();
            powVal = true;
        }

        float t = Time.time;

        // Create a new light packet from the object's current true position
        packetQueue.Enqueue(new LightPacket<float>(truePosition, t, power));

        // Check if any packets have "arrived"
        while (packetQueue.Count > 0 && packetQueue.Peek().arrivalTime <= t)
        {
            LightPacket<float> packet = packetQueue.Dequeue();
            obsPower = packet.payload;
        }
    }

    public void UsePower(float deltaTime)
    {
        // decrease power over time; eventually, we need to refuel
        float p(float t)
        {
            return Mathf.Pow(acceleration, 2) * Mathf.Pow(c, 4) * mass * t /
                (Mathf.Pow(Mathf.Pow(acceleration * t, 2) + Mathf.Pow(c, 2), 2) *
                Mathf.Pow(Mathf.Sqrt(1 - Mathf.Pow(acceleration * t / c, 2) / (Mathf.Pow(acceleration * t / c, 2) + 1)), 3));
        }
        float GetPower(float t)
        {
            if (t >= 0 && t < t1)
                return p(t);
            else if (t >= t2 && t <= t3)
                return p(t3 - t);
            return 0f;
        }
        power -= GetPower(deltaTime) * Time.deltaTime;
    }

    public void EvaluateEndOfJourneyPower()
    {
        if (power < 0.01f)
        {
            if (power < -0.1f)
            {
                Debug.LogError("Warning: Power has decreased to a non-negligible negative amount.");
                return;
            }
            Debug.Log($"The {title} ran out of power!");
            power = 0f;
            maxWarp = 0f;
        }
        else
            CalculateMaxWarp();

        if (maxWarp < 0.1f && maxWarp > 0f)
            Debug.Log($"The {title} is low on fuel.");
    }

    public void Refuel()
    {
        power = maxPower;
        CalculateMaxWarp();
    }

    public void ResetAcceleration()
    {
        acceleration = maxAcceleration;
    }

    public void SetDestinationFromFleet(Vector3 d, float t1, float t2, float t3, float ta1, float ta2, float ta3)
    {
        this.d = d;
        p = transform.position;
        this.t1 = t1;
        this.t2 = t2;
        this.t3 = t3;
        this.ta1 = ta1;
        this.ta2 = ta2;
        this.ta3 = ta3;
    }

    void SetRandomName()
    {
        prefix = PlayerData.instance.countries[0].shipPrefix;
        title = NameGenerator.GenerateShipName(PlayerData.instance.countries[0].nameListId);
    }

    void CalculateMaxWarp()
    {
        // Set the max warp factor depending on power: note that half power is used for acceleration/deceleration
        maxWarp = Mathf.Sqrt(1 - Mathf.Pow(mass / (power / 2 / Mathf.Pow(c, 2) + mass), 2));
    }

    public string GetInfo()
    {
        return string.Join("\n",
            $"Mass: {mass}",
            $"Power: {Mathf.Max(0f, obsPower):0.##}\n",
            $"Core Rating: {maxPower}\u03c7",
            $"Acceleration: {maxAcceleration}",
            $"Max Warp: {maxWarp * 10:0.##}",
            $"Destination: {(target != null ? target.gameObject.name : "None")}");
    }
}
