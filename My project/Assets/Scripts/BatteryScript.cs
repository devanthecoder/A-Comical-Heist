using UnityEngine;
using UnityEngine.UI;
public class BatteryScript : MonoBehaviour
{
    public Slider BatterySlider; // Reference to the Text component to display battery life
    public Slider BlockedOutSlider; // Reference to the Text component to display battery life
    public float batteryLife = 100f; // Battery life in seconds
    public float blockedLife = 0;
    public float replenishRate = 5f; // Rate at which the battery replenishes (negative value for depletion)
    private float currentBatteryLife;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentBatteryLife = batteryLife;
    }

    void Update()
    {
        if (currentBatteryLife > 0)
        {
            currentBatteryLife += replenishRate * Time.deltaTime;
        }
        if (currentBatteryLife > (batteryLife - blockedLife))
        {
            currentBatteryLife = batteryLife - blockedLife;
        }
        BatterySlider.value = currentBatteryLife;
        BlockedOutSlider.value = blockedLife;
    }

    public void DepleteBattery(float amount)
    {
        currentBatteryLife -= amount;
        if (currentBatteryLife <= 0)
        {
            currentBatteryLife = 0;
        }
    }

    public float GetBatteryLife()
    {
        return currentBatteryLife;
    }

    public void BlockBatteryLife(float amount)
    {
        blockedLife = amount;
    }
}
