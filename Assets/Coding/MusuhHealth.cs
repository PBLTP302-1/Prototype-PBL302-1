using UnityEngine;

public class MusuhHealth : MonoBehaviour
{
    [Header("Status HP")]
    [SerializeField, Min(1)]
    private int hpMaksimum = 10;

    private int hpSekarang;

    public int HPSekarang => hpSekarang;

    private void Awake()
    {
        hpSekarang = hpMaksimum;
    }

    public void TerimaDamage(int damage)
    {
        if (damage <= 0)
            return;

        hpSekarang -= damage;

        Debug.Log(
            name + " menerima damage " + damage +
            ". HP tersisa: " + hpSekarang
        );

        if (hpSekarang <= 0)
        {
            Destroy(gameObject);
        }
    }
}
