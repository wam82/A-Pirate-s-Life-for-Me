using UnityEngine;

public class TriggerTester : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object has entered: " + other.transform.tag);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Object has exited: " + other.transform.tag);
    }
}
