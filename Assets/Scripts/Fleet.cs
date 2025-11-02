using UnityEngine;
using System.Collections.Generic;

public class Fleet : RelativisticEntity, ISelectable
{
    public string title { get; set; }
    public List<Ship> ships = new List<Ship>();

    protected override void Start()
    {
        SetRandomName();
        base.Start();
    }

    protected override void Update()
    {
        if (!isMoving) return;

        base.Update();
        foreach (var ship in ships)
        {
            ship.UsePower(timeElapsed);
            ship.transform.position = transform.position;
            if (!isMoving)
            {
                ship.EvaluateEndOfJourneyPower();
                ship.ResetAcceleration();
            }
        }

        if (!isMoving)
            EvaluateMinimumSpeeds();
    }

    public void AddShip(Ship ship)
    {
        if (isMoving) return;

        ships.Add(ship);

        EvaluateMinimumSpeeds();
    }

    public void RemoveShip(Ship ship)
    {
        if (isMoving) return;

        ships.Remove(ship);

        if (ships.Count == 0) return;

        EvaluateMinimumSpeeds();
    }

    public override void SetDestination(Vector3 newDestination)
    {
        foreach (Ship ship in ships)
            ship.acceleration = acceleration;

        base.SetDestination(newDestination);

        foreach (Ship ship in ships)
            ship.SetDestinationFromFleet(d, t1, t2, t3, ta1, ta2, ta3);
    }

    void EvaluateMinimumSpeeds()
    {
        // Max speed is limited by min acc, min power, max mass
        float minAcc = float.MaxValue;
        float maxMass = 0f;
        float minPower = float.MaxValue;
        Ship shipWithMaxMass = ships[0];
        Ship shipWithMinPower = ships[0];
        foreach (Ship ship in ships)
        {
            minAcc = Mathf.Min(minAcc, ship.acceleration);
            if (ship.mass > maxMass)
            {
                maxMass = Mathf.Max(maxMass, ship.mass);
                shipWithMaxMass = ship;
            }
            if (ship.power < minPower)
            {
                minPower = Mathf.Min(minPower, ship.power);
                shipWithMinPower = ship;
            }
        }

        acceleration = minAcc;
        maxWarp = Mathf.Min(shipWithMaxMass.maxWarp, shipWithMinPower.maxWarp);
    }

    void SetRandomName()
    {
        if (string.IsNullOrEmpty(title))
            title = "Default";
    }

    public string GetInfo()
    {
        return $"Number of Ships: {ships.Count}";
    }
}
