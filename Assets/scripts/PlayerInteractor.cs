
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    public FirstPersonMover mover;

    private Exhibit nearby;
    private GameObject openPanel;

    private void OnTriggerEnter(Collider other)
    {
        Exhibit e = other.GetComponent<Exhibit>();
        if (e == null) return;

        nearby = e;
        nearby.ShowPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Exhibit e = other.GetComponent<Exhibit>();
        if (e == null || e != nearby) return;

        nearby.ShowPrompt(false);
        nearby = null;
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        if (openPanel != null)
        {
            openPanel.SetActive(false);
            openPanel = null;
            mover.canMove = true;
            return;
        }

        if (nearby == null) return;

        openPanel = nearby.panel;
        openPanel.SetActive(true);
        mover.canMove = false;
    }
}
