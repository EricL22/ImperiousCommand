using UnityEngine;

public abstract class RelativisticEntity : MonoBehaviour
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
    //public float velocity;
    internal Transform referenceTransform { private get; set; }

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
    }

    public float t1, t2, t3, ta1, ta2, ta3;
    // position function
    protected float x(float t)
    {
        return Mathf.Pow(c, 2) / acceleration * (Mathf.Sqrt(1 + Mathf.Pow(acceleration * t / c, 2)) - 1);
    }
    // derivative of the position function
    protected float xp(float t)
    {
        return acceleration * t / Mathf.Sqrt(1 + Mathf.Pow(acceleration * t / c, 2));
    }

    public virtual void Move(float deltaTime)
    {
        if (!isMoving) return;

        Vector3 dx = GetApparentVelocity(deltaTime);

        transform.position += dx * Time.deltaTime;
        //Debug.Log($"Velocity: {dx.x}");

        // Check if close to destination
        if (dx.magnitude == 0f)
        {
            isMoving = false;
            currentLocation = target;
            target = null;
            transform.position = currentLocation.position + arrivalOffset;
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
        d = newDestination - transform.position;
        p = transform.position;

        // evaluate transition times
        float clampedWarpTime = maxWarp * c / acceleration * Mathf.Sqrt(1 / (1 - Mathf.Pow(maxWarp / c, 2)));
        // take the minimum of the time it takes to cover a distance D/2 and the time it takes to reach maxWarp
        t1 = Mathf.Min(c / acceleration * Mathf.Sqrt(Mathf.Pow(1 + acceleration * d.magnitude / (2 * Mathf.Pow(c, 2)), 2) - 1),
            clampedWarpTime);
        t2 = t1 + (d.magnitude - 2 * x(t1)) / xp(t1);
        t3 = t1 + t2;

        // should be stretched for vR > 0, compressed for vR < 0
        ta1 = ComputeApparentTime(t1);
        ta2 = ComputeApparentTimeRange(t1, t2, ta1);
        ta3 = ComputeApparentTimeRangeShifted(t2, t3, ta2);

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

    Vector3 GetScaledVelocity(float t)
    {
        float dx = 0f;

        // Phase 1: Acceleration
        if (t >= 0 && t < ta1)
            dx = xp(t);

        // Phase 2: Coasting
        else if (t >= ta1 && t < ta2)
            dx = xp(ta1);

        // Phase 3: Deceleration
        else if (t >= ta2 && t <= ta3)
            dx = -xp(t - ta3);

        return d.normalized * dx;
    }

    Vector3 GetScaledPosition(float t)
    {
        float dx = 0f;

        // Phase 1: Acceleration
        if (t >= 0 && t < ta1)
            dx = x(t);

        // Phase 2: Coasting
        else if (t >= ta1 && t < ta2)
            dx = x(ta1) + xp(ta1) * (t - ta1);

        // Phase 3: Deceleration
        else if (t >= ta2 && t <= ta3)
            dx = d.magnitude - x(t - ta3);

        return p + d.normalized * dx;
    }

    protected Vector3 GetApparentVelocity(float t)
    {
        Vector3 dx = Vector3.zero;

        // Phase 1: Acceleration
        if (t >= 0 && t < ta1)
            dx = ComputeApparentVelocity(GetScaledVelocity(t), GetScaledPosition(t));

        // Phase 2: Coasting
        else if (t >= ta1 && t < ta2)
            dx = ComputeApparentVelocity(GetScaledVelocity(ta1), GetScaledPosition(t));

        // Phase 3: Deceleration
        else if (t >= ta2 && t <= ta3)
            dx = ComputeApparentVelocity(GetScaledVelocity(ta3 - t), GetScaledPosition(t));

        return dx;
    }

    // make it internal for testing
    internal Vector3 ComputeApparentVelocity(Vector3 v, Vector3 x)
    {
        // Calculate the direction from the object to the reference (reference transform is at origin)
        Vector3 r = x - referenceTransform.position;

        Vector3 rHat;
        if (r == Vector3.zero)
            rHat = v.normalized;
        else
            rHat = r.normalized;  // Radial direction from object to reference

        // Compute the radial velocity component (scalar)
        float vR = Vector3.Dot(v, rHat);  // Projection of velocity onto radial direction

        // Compute the transformed radial velocity component
        decimal denom = 1 + (decimal) vR / c;   // we are unit-testing for a precision of 0.01f, which requires a decimal type
        float v_aR = vR / (float) denom;

        // Compute the tangential component (perpendicular to radial direction)
        Vector3 vT = v - vR * rHat; // Tangential velocity is the leftover part

        // Combine the transformed radial component and unchanged tangential component
        Vector3 v_a = vT + v_aR * rHat;

        return v_a.magnitude * d.normalized;
    }

    // This function is used to compute the apparent time ta1 for increasing velocity 0 < t < ta1
    float ComputeApparentTime(float t)
    {
        // Function for velocity magnitude (this will be used for integration)
        float VelocityMagnitudeAtTime(float t)
        {
            Vector3 v = GetVelocity(t);
            return v.magnitude;
        }

        // Compute total displacement using Gaussian Quadrature
        float displacement = GaussianQuadrature(VelocityMagnitudeAtTime, 0f, t);

        // Function for apparent velocity magnitude (for the second integral)
        float ApparentVelocityMagnitudeAtTime(float ta)
        {
            Vector3 va = ComputeApparentVelocity(d.normalized * xp(ta), p + d.normalized * x(ta));
            return va.magnitude;
        }

        // Find t_a such that integral v_a(t') dt' = displacement
        float apparentDisplacement = 0f;
        float apparentTime = 0f;

        // Use Gaussian Quadrature for apparent displacement calculation
        while (apparentDisplacement < displacement)
        {
            apparentDisplacement = GaussianQuadrature(ApparentVelocityMagnitudeAtTime, 0f, apparentTime);
            apparentTime += 0.0005f; // Increase time slightly to move forward
        }

        return apparentTime; // This is t_a
    }

    // Another function is needed for the constant velocity in ta1 < t < ta2
    float ComputeApparentTimeRange(float t1, float t2, float ta1)
    {
        // Function for velocity magnitude (this will be used for integration)
        float VelocityMagnitudeAtTime(float t)
        {
            Vector3 v = GetVelocity(t);
            return v.magnitude;
        }

        // Displacement between t1 and t2
        float displacement = GaussianQuadrature(VelocityMagnitudeAtTime, t1, t2);

        // Apparent displacement calculation
        float apparentDisplacement = 0f;
        float apparentTime = ta1;

        // Function for apparent velocity magnitude (for the second integral)
        float ApparentVelocityMagnitudeAtTime(float t)
        {
            Vector3 va = ComputeApparentVelocity(d.normalized * xp(ta1), p + d.normalized * (x(ta1) + xp(ta1) * (t - ta1)));
            return va.magnitude;
        }

        // Find t_a2 such that integral v_a(t') dt' = displacement starting from ta1
        while (apparentDisplacement < displacement)
        {
            // Approximate the apparent displacement integral using Gaussian Quadrature
            apparentDisplacement = GaussianQuadrature(ApparentVelocityMagnitudeAtTime, ta1, apparentTime);

            apparentTime += 0.0005f;  // Increase the apparent time step slightly for the next iteration
        }

        return apparentTime; // This is t_a2
    }

    // Another function is needed for the decreasing velocity in ta2 < t < ta3
    float ComputeApparentTimeRangeShifted(float t2, float t3, float ta2)
    {
        // Function for velocity magnitude (this will be used for integration)
        float VelocityMagnitudeAtTime(float t)
        {
            Vector3 v = GetVelocity(t);
            return v.magnitude;
        }

        // Function for apparent velocity magnitude (this will be used for integration)
        float ApparentVelocityMagnitudeAtTime(float t, float apparentTime)
        {
            Vector3 va = ComputeApparentVelocity(d.normalized * -xp(t - apparentTime), p + d.normalized * (d.magnitude - x(t - apparentTime)));
            return va.magnitude;
        }

        // Compute total displacement between t2 and t3 using Gaussian Quadrature
        float displacement = GaussianQuadrature(VelocityMagnitudeAtTime, t2, t3);

        // Now find t_a3 such that integral v_a(t') dt' = displacement starting from t_a2
        float apparentDisplacement = 0f;
        float apparentTime = ta2;

        // Use a loop with Gaussian Quadrature to find the correct t_a3
        while (apparentDisplacement < displacement)
        {
            // We need to calculate the integral of the apparent velocity between ta2 and the current test apparent time
            apparentDisplacement = GaussianQuadrature(t => ApparentVelocityMagnitudeAtTime(t, apparentTime), ta2, apparentTime);

            // If the apparent displacement is less than the actual displacement, increase apparentTime
            if (apparentDisplacement < displacement)
            {
                apparentTime += 0.0005f;  // Increase the test apparent time slightly
            }
        }

        return apparentTime; // This is t_a3
    }

    // 8-point Gaussian Quadrature for a definite integral on [-1, 1]
    protected float GaussianQuadrature(System.Func<float, float> f, float a, float b)
    {
        // 8-point Gaussian quadrature nodes and weights for the interval [-1, 1]
        float[] nodes = new float[] {
            -0.1834346424956498f, 0.1834346424956498f,
            -0.5255324099163290f, 0.5255324099163290f,
            -0.7966664774136267f, 0.7966664774136267f,
            -0.9491079123427585f, 0.9491079123427585f
        };
        float[] weights = new float[] {
            0.3626837833783620f, 0.3626837833783620f,
            0.3137066458778873f, 0.3137066458778873f,
            0.2223810344533745f, 0.2223810344533745f,
            0.1012285362903763f, 0.1012285362903763f
        };

        // Transform the interval [a, b] to [-1, 1]
        float halfLength = (b - a) / 2.0f;
        float midPoint = (b + a) / 2.0f;

        float sum = 0.0f;

        // Evaluate the integrand at the nodes and sum the weighted results
        for (int i = 0; i < 8; i++)
        {
            float transformedNode = midPoint + halfLength * nodes[i];
            sum += weights[i] * f(transformedNode);
        }

        // Scale the result by half the length of the interval
        return sum * halfLength;
    }
}
