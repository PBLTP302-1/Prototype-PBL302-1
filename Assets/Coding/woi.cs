using System.Collections;
using UnityEngine;

public class woi : MonoBehaviour
{
    public bool tahan;
    public Vector2 posisiawal;

    // Referensi ke KotakSetting
    public KotakSetting kotakSetting;

    [Header("Pengaturan Cooldown")]
    [SerializeField] private float cooldownDurasi = 3f;

    private bool tersentuh = false;
    private bool sedangDrag = false;
    private bool sedangCooldown = false;

    private Transform target;
    private SpriteRenderer spriteRenderer;
    private Color warnaAwal;

    private void Start()
    {
        posisiawal = transform.position;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            warnaAwal = spriteRenderer.color;
        }
        else
        {
            Debug.LogError(
                "SpriteRenderer tidak ditemukan pada objek!"
            );
        }
    }

    private void OnMouseDown()
    {
        // Tidak bisa didrag saat cooldown
        if (sedangCooldown)
            return;

        tahan = true;
        sedangDrag = true;
    }

    private void OnMouseUp()
    {
        tahan = false;
    }

    // ==============================
    // MENYENTUH OBJECT TARGET
    // ==============================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("target"))
        {
            tersentuh = true;
            target = other.transform;

            Debug.Log("kenak target");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("target"))
        {
            // Pastikan target yang keluar adalah target aktif
            if (target == other.transform)
            {
                tersentuh = false;
                target = null;
            }

            Debug.Log("keluar target");
        }
    }

    private void Update()
    {
        // ==============================
        // DRAG
        // ==============================

        if (Input.GetMouseButton(0) &&
            tahan && !sedangCooldown)
        {
            Vector2 mouseposition =
                Camera.main.ScreenToWorldPoint(
                    Input.mousePosition
                );

            transform.position = mouseposition;
        }

        // ==============================
        // MOUSE DILEPAS
        // ==============================

        if (Input.GetMouseButtonUp(0) && sedangDrag)
        {
            sedangDrag = false;
            tahan = false;

            Vector2 posisiTujuan;

            bool posisiValid = false;

            // 1. CEK TARGET
            if (tersentuh && target != null)
            {
                posisiTujuan = target.position;
                posisiValid = true;
            }

            // 2. CEK TILEMAP
            else if (handleRealsePosition(transform.position))
            {
                Vector2Int cell =
                    kotakSetting.WorldToCell(
                        transform.position
                    );

                posisiTujuan =
                    kotakSetting.CelltoWorld(cell);

                posisiValid = true;
            }
            else
            {
                posisiTujuan = posisiawal;
            }

            // ==============================
            // JIKA POSISI VALID
            // ==============================

            if (posisiValid)
            {
                // Buat salinan di posisi tujuan
                GameObject salinan = Instantiate(
                    gameObject,
                    posisiTujuan,
                    transform.rotation
                );

                // Salinan tujuan tidak bisa didrag lagi
                woi scriptSalinan =
                    salinan.GetComponent<woi>();

                if (scriptSalinan != null)
                {
                    scriptSalinan.enabled = false;
                }

                // Objek asli kembali ke posisi awal
                transform.position = posisiawal;

                // Mulai cooldown pada objek asli
                StartCoroutine(Cooldown());
            }
            else
            {
                // Posisi tidak valid: kembali ke awal
                transform.position = posisiawal;
            }

            // Bersihkan status target
            tersentuh = false;
            target = null;
        }
    }

    // =========================================
    // CEK APAKAH TILE BOLEH DITEMPATI
    // =========================================

    public bool handleRealsePosition(Vector2 posisi)
    {
        if (kotakSetting == null)
        {
            Debug.LogError(
                "KotakSetting belum diisi!"
            );

            return false;
        }

        Vector2Int cell =
            kotakSetting.WorldToCell(posisi);

        KotakSetting.DataUbin data =
            kotakSetting.GetCellData(cell);

        if (data != null && data.aman)
        {
            return true;
        }

        return false;
    }

    // =========================================
    // SISTEM COOLDOWN
    // =========================================

    private IEnumerator Cooldown()
    {
        sedangCooldown = true;

        // Ubah objek asli menjadi hitam
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.black;
        }

        // Tunggu sesuai durasi di Inspector
        yield return new WaitForSeconds(cooldownDurasi);

        // Cooldown selesai, kembalikan warna awal
        if (spriteRenderer != null)
        {
            spriteRenderer.color = warnaAwal;
        }

        sedangCooldown = false;

        Debug.Log("Cooldown selesai, objek siap digunakan.");
    }
}
