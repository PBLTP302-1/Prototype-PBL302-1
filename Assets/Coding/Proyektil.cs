using UnityEngine;

public class Proyektil : MonoBehaviour
{
    [Header("Pengaturan Proyektil")]
    [SerializeField, Min(0.01f)]
    private float kecepatan = 8f;

    [SerializeField, Min(0)]
    private int damage = 2;

    [Header("Pemeriksaan Jalur")]
    [SerializeField, Min(0f)]
    private float toleransiJalurY = 0.25f;

    [Header("Referensi Tilemap")]
    [SerializeField]
    private KotakSetting kotakSetting;

    private Vector2 arah = Vector2.right;

    // Posisi Y jalur saat proyektil mulai digunakan.
    private float posisiJalurY;

    public void AturProyektil(
        int damageBaru,
        float kecepatanBaru,
        Vector2 arahBaru)
    {
        damage = damageBaru;
        kecepatan = kecepatanBaru;

        if (arahBaru.sqrMagnitude > 0f)
        {
            arah = arahBaru.normalized;
        }
    }

    private void Start()
    {
        // Simpan jalur awal proyektil.
        posisiJalurY = transform.position.y;

        if (kotakSetting == null)
        {
            kotakSetting =
                FindObjectOfType<KotakSetting>();
        }

        if (kotakSetting == null)
        {
            Debug.LogError(
                "KotakSetting tidak ditemukan!",
                this
            );
        }
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(arah * kecepatan * Time.deltaTime);

        PeriksaBatasTilemap();
    }

    private void PeriksaBatasTilemap()
    {
        if (kotakSetting == null)
            return;

        Vector2Int cell =
            kotakSetting.WorldToCell(transform.position);

        // Hapus proyektil jika mencapai kolom paling kanan.
        if (cell.x >= kotakSetting.Panjang - 1)
        {
            Destroy(gameObject);
            return;
        }

        // Hapus proyektil jika berada di luar area aman.
        KotakSetting.DataUbin data =
            kotakSetting.GetCellData(cell);

        if (data == null || !data.aman)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MusuhHealth musuh =
            other.GetComponentInParent<MusuhHealth>();

        // Abaikan objek yang bukan musuh.
        if (musuh == null)
            return;

        // Hitung selisih jalur Y proyektil dan musuh.
        float selisihY = Mathf.Abs(
            posisiJalurY - musuh.transform.position.y
        );

        // Abaikan musuh yang berada di jalur berbeda.
        if (selisihY > toleransiJalurY)
            return;

        // Musuh berada di jalur yang sama.
        musuh.TerimaDamage(damage);

        Destroy(gameObject);
    }
}