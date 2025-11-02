using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DestinationSetter : MonoBehaviour
{
    private Button setDestinationButton;
    private RelativisticEntity selectedEntity;

    private void Start()
    {
        setDestinationButton = GetComponent<Button>();
        setDestinationButton.interactable = false;
        selectedEntity = SelectionManager.instance.selectedObject.GetComponent<RelativisticEntity>();
    }

    private void Update()
    {
        if (selectedEntity != null)
        {
            // Disable button if the entity is moving
            setDestinationButton.interactable = !selectedEntity.isMoving;
        }
    }

    public void OnSetDestinationButtonClicked()
    {
        if (selectedEntity != null && !selectedEntity.isMoving && selectedEntity.maxWarp > 0.1f)
        {
            // Enable star selection mode
            StartCoroutine(WaitForStarClick());
            SelectionManager.instance.interrupted = true;
        }
    }

    private System.Collections.IEnumerator WaitForStarClick()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    yield break; // Prevent selecting UI elements

                // Raycast to detect clicked star
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    Star star = hit.collider.GetComponent<Star>() ?? hit.collider.GetComponentInParent<Star>();
                    if (star != null)
                    {
                        selectedEntity.SetDestination(star.transform);
                        SelectionManager.instance.interrupted = false;
                        yield break;
                    }
                }
            }
            yield return null;
        }
    }
}
