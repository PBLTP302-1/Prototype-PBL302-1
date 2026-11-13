using UnityEngine;

public class MusuhGerak : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField]
    private KotakSetting kotakSetting;

    [Header("Pengaturan Gerakan")]
    [SerializeField, Min(0f)]
    private float kecepatan = 2f;

    [Header("Pengaturan Serangan")]
    [SerializeField, Min(0)]
    private int damageMusuh = 1;

    [Tooltip("Jeda antarserangan dalam detik.")]
    [SerializeField, Min(0.1f)]
    private float intervalSerangan = 1f;

    private int barisMusuh;
    private float waktuSeranganBerikutnya;

    private InventoryDragDrop targetSaatIni;

    private void Start()
    {
        if (kotakSetting == null)
            kotakSetting = FindFirstObjectByType<KotakSetting>();

        if (kotakSetting == null)
        {
            Debug.LogError("KotakSetting tidak ditemukan!", this);
            enabled = false;
            return;
        }

        // Catat baris grid tempat musuh mulai.
        barisMusuh =
            kotakSetting.WorldToCell(transform.position).y;
    }

    private void Update()
    {
        if (kotakSetting == null)
            return;

        // Berhenti selama masih menyentuh pertahanan yang valid.
        if (TargetMasihValid())
            return;

        targetSaatIni = null;

        Vector3 posisiBerikutnya = transform.position;
        posisiBerikutnya.x -= kecepatan * Time.deltaTime;

        Vector2Int cellBerikutnya =
            kotakSetting.WorldToCell(posisiBerikutnya);

        // Musuh tidak boleh berpindah ke baris lain.
        if (cellBerikutnya.y != barisMusuh)
        {
            Destroy(gameObject);
            return;
        }

        KotakSetting.DataUbin data =
            kotakSetting.GetCellData(cellBerikutnya);

        // Hilangkan musuh jika petak tidak aman atau di luar arena.
        if (data == null || !data.aman)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = posisiBerikutnya;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (kotakSetting == null)
            return;

        InventoryDragDrop objek =
            other.GetComponentInParent<InventoryDragDrop>();

        if (objek == null || !objek.ObjekSudahDiletakkan)
            return;

        Vector2Int cellMusuh =
            kotakSetting.WorldToCell(transform.position);

        Vector2Int cellPertahanan =
            kotakSetting.WorldToCell(objek.transform.position);

        // Hanya serang pertahanan di baris grid yang sama.
        if (cellMusuh.y != cellPertahanan.y)
            return;

        if (targetSaatIni != objek)
        {
            targetSaatIni = objek;
            waktuSeranganBerikutnya = Time.time;
        }

        if (Time.time >= waktuSeranganBerikutnya)
        {
            objek.TerimaDamage(damageMusuh);

            waktuSeranganBerikutnya =
                Time.time + intervalSerangan;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        InventoryDragDrop objek =
            other.GetComponentInParent<InventoryDragDrop>();

        if (objek != null && objek == targetSaatIni)
            targetSaatIni = null;
    }

    private bool TargetMasihValid()
    {
        if (targetSaatIni == null ||
            !targetSaatIni.ObjekSudahDiletakkan ||
            kotakSetting == null)
        {
            return false;
        }

        Vector2Int cellMusuh =
            kotakSetting.WorldToCell(transform.position);

        Vector2Int cellPertahanan =
            kotakSetting.WorldToCell(
                targetSaatIni.transform.position
            );

        if (cellMusuh.y != cellPertahanan.y)
            return false;

        Collider2D colliderMusuh =
            GetComponent<Collider2D>();

        Collider2D colliderPertahanan =
            targetSaatIni.GetComponent<Collider2D>();

        if (colliderMusuh == null ||
            colliderPertahanan == null)
        {
            return false;
        }

        return colliderMusuh.IsTouching(colliderPertahanan);
    }
}