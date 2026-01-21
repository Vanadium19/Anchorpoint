using UnityEngine;

public sealed class BuildMenuView : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            Toggle();
    }

    private void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}