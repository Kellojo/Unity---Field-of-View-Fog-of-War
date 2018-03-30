using UnityEngine;

public class Hideable : MonoBehaviour, IHideable {

    public void OnFOVEnter() {
        GetComponent<Renderer>().enabled = true;
    }
    public void OnFOVLeave() {
        GetComponent<Renderer>().enabled = false;
    }
}
