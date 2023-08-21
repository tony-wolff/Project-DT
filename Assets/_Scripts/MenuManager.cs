using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public Transform head;
    [SerializeField]
    GameObject acessibilityMenu;
    public float distance = 2;

    private void OnEnable() {
        ControllerInputManager.onMenuPressed += ActivateMenu;
    }

    private void OnDisable() {
        ControllerInputManager.onMenuPressed -= ActivateMenu;
    }

    void ActivateMenu()
    {
        Debug.Log("activate menu");
        if(!acessibilityMenu.gameObject.activeSelf)
        {
            acessibilityMenu.transform.position = head.position + new Vector3(head.forward.x, 0, head.forward.z).normalized * distance; //set te menu in the direction fo the user
            acessibilityMenu.transform.LookAt(new Vector3(head.position.x, acessibilityMenu.transform.position.y, head.position.z)); //rotate the transform so that it points to the user
            acessibilityMenu.transform.forward *= -1; //menu in the right orientation, otherwise it's reversed and upside down
            acessibilityMenu.gameObject.SetActive(true);
        }
        else
            acessibilityMenu.gameObject.SetActive(false);
    }
}
