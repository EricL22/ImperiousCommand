using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ApparentVelocityTests
{
    private GameObject testObject;
    private Ship shipComponent;
    private Transform referenceTransform;

    // SetUp method runs before each test to initialize GameObject and components
    [SetUp]
    public void SetUp()
    {
        // Create a new GameObject and attach the Ship component
        testObject = new GameObject("TestShip");
        shipComponent = testObject.AddComponent<Ship>(); // Attach Ship component

        referenceTransform = new GameObject("ReferenceTransform").transform;
        referenceTransform.position = Vector3.zero; // Set to (0, 0, 0)
        shipComponent.referenceTransform = referenceTransform;
    }

    [Test]
    public void TestComputeApparentVelocity()
    {
        Vector3[] testPositions =
        {
            new Vector3(0, 0, 0),   // Object at origin, moving left
            new Vector3(0, 0, 0),   // Object at origin, moving right
            new Vector3(-10, 0, 0), // Object left of origin, moving right
            new Vector3(10, 0, 0),  // Object right of origin, moving left
        };

        Vector3[] testVelocities =
        {
            new Vector3(-1, 0, 0), // Moving left
            new Vector3(1, 0, 0), // Moving right
            new Vector3(0.99f, 0, 0),  // Moving right
            new Vector3(-0.99f, 0, 0)  // Moving left
        };

        Vector3[] expectedApparentVelocities =
        {
            new Vector3(-0.5f, 0, 0),
            new Vector3(0.5f, 0, 0),
            new Vector3(99, 0, 0),
            new Vector3(-99, 0, 0)
        };

        for (int i = 0; i < testPositions.Length; i++)
        {
            Vector3 v = testVelocities[i];
            Vector3 x = testPositions[i];
            shipComponent.SetDestination(v, x);
            Vector3 va = shipComponent.ComputeApparentVelocity(v, x);

            // Assert that the computed apparent velocity matches the expected one
            Assert.AreEqual(expectedApparentVelocities[i], va, $"Test case {i + 1} failed: Expected {expectedApparentVelocities[i]}, but got {va}");
        }
    }

    // TearDown method runs after each test to clean up
    [TearDown]
    public void TearDown()
    {
        // Clean up the GameObject after each test
        Object.Destroy(testObject);
        Object.Destroy(referenceTransform.gameObject);
    }
}
