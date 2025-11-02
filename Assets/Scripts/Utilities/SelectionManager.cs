using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public interface ISelectable
{
    string title { get; set; }
    string GetInfo();
}

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager instance;
    public GameObject infoPanel;  // The UI panel to show
    public TextMeshProUGUI prefixText;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI infoText;  // Text to show the object's info
    public GameObject selectedObject;
    public bool interrupted;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !interrupted)  // Left-click
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                ISelectable interactable = hit.collider.GetComponent<ISelectable>() ??
                    hit.collider.GetComponentInParent<ISelectable>();
                if (interactable != null)
                    selectedObject = hit.collider.GetComponent<ISelectable>() != null
                        ? hit.collider.gameObject
                        : hit.collider.transform.parent.gameObject;
                else
                    selectedObject = null;
            }
            else
                selectedObject = null;
        }
        if (selectedObject != null)
        {
            ISelectable selectedSelectable = selectedObject.GetComponent<ISelectable>();
            if (selectedSelectable is ISelectableShip selectedShip)
                ShowInfoPanel(selectedSelectable.title, selectedSelectable.GetInfo(), selectedShip.prefix);
            else
                ShowInfoPanel(selectedSelectable.title, selectedSelectable.GetInfo());
        }
        else
            HideInfoPanel();
    }

    public void ShowInfoPanel(string title, string info, string prefix = "")
    {
        prefixText.text = prefix;
        titleText.text = title;
        infoText.text = info;
        infoPanel.SetActive(true);
    }

    public void HideInfoPanel()
    {
        infoPanel.SetActive(false);
    }
}
