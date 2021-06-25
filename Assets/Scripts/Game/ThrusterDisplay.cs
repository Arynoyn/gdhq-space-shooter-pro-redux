using UnityEngine;
using UnityEngine.UI;

public class ThrusterDisplay : MonoBehaviour
{
    [SerializeField] private Slider thrusterBar;

    private void Start()
    {
        if (thrusterBar == null) { thrusterBar = GetComponent<Slider>(); }
        if (thrusterBar == null) { Debug.Log("Thruster Bar missing from Thruster Display!"); }
    }

    public void SetCharge(int charge)
    {
        thrusterBar.value = charge;
    }
    
    public void SetMaxCharge(int charge)
    {
        thrusterBar.maxValue = charge;
    }
}
