using UnityEngine;

public interface ISelectableShip : ISelectable
{
    string prefix { get; set; }
}

public class Ship : RelativisticEntity, ISelectableShip
{
    public string title { get; set; }
    public string prefix { get; set; }
    public float mass = 1f;  // Mass (tons)
    public float power = 1f; // Power currently stored by the warp core (exons)
    float maxPower; // Maximum power the warp core can store (exons)
    float maxAcceleration;
    // NOTE: 1 exon is the amount of power required to accelerate a mass of 1 ton to a factor of sqrt(3)/2 times c
    // In SI units this is tons * ly^2/y^2

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        SetRandomName();
        CalculateMaxWarp();
        maxPower = power;
        maxAcceleration = acceleration;
        base.Start();
        FindAnyObjectByType<Fleet>()?.AddShip(this);
        if (acceleration > 10f)
            Debug.LogWarning("Warning: the current algorithm is untested for large accelerations. Unintended behavior may occur.");
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!isMoving) return;

        base.Update();
        UsePower(timeElapsed);

        if (!isMoving)
        {
            EvaluateEndOfJourneyPower();
        }
    }

    float prevActualTime = 0f;
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

        float curActualTime = ComputeActualTime(deltaTime);

        power -= GaussianQuadrature(GetPower, prevActualTime, curActualTime);

        prevActualTime = curActualTime;
    }

    public void EvaluateEndOfJourneyPower()
    {
        prevActualTime = 0f;
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

    float ComputeActualTime(float t)
    {
        float dt = 0.001f;  // Small step size for accuracy

        // Compute total displacement using v_a(t)
        float apparentDisplacement = 0f;
        float apparentTime = 0f;
        while (apparentTime < t)
        {
            Vector3 va = GetApparentVelocity(apparentTime);
            apparentDisplacement += va.magnitude * dt;
            apparentTime += dt;
        }

        // Now find t such that integral v(t') dt' = displacement
        float displacement = 0f;
        float actualTime = 0f;
        float epsilon = 0.05f; // account for rounding errors
        while (displacement < apparentDisplacement)
        {
            Vector3 v = GetVelocity(actualTime);
            displacement += v.magnitude * dt;
            actualTime += dt;
            if (v == Vector3.zero && displacement > 0f)
            {
                if (Mathf.Abs(apparentDisplacement - displacement) > epsilon)
                {
                    Debug.LogError($"Actual time {actualTime} couldn't converge for apparent time {t}");
                    Debug.Log($"{displacement}, apparent {apparentDisplacement}");
                }
                break;
            }
        }

        return actualTime; // This is t
    }

    public string GetInfo()
    {
        return string.Join("\n",
            $"Mass: {mass}",
            $"Power: {Mathf.Max(0f, power):0.##}\n",
            $"Core Rating: {maxPower}\u03c7",
            $"Acceleration: {maxAcceleration}",
            $"Max Warp: {maxWarp*10:0.##}",
            $"Destination: {(target != null ? target.gameObject.name : "None")}");
    }
}
