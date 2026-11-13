using UnityEngine;

public class InventoryDragDrop : MonoBehaviour
{
    // =========================================================
    // 1. REFERENSI SCRIPT LAIN
    // =========================================================

    [Header("Referensi")]
    public KotakSetting kotakSetting; // Mengakses data dan posisi petak arena.
    public ResourceManager resourceManager; // Mengakses sistem resource pemain.


    // =========================================================
    // 2. PENGATURAN INVENTORY DAN BIAYA
    // =========================================================

    [Header("Pengaturan Inventory")]
    public int biayaSumberDaya = 2; // Biaya untuk memasang satu defense.
    public float cooldownDurasi = 3f; // Waktu tunggu sebelum inventory bisa digunakan lagi.

    public bool objekInventory = true; // True jika objek masih menjadi item inventory.

    public bool ObjekSudahDiletakkan
    {
        get { return !objekInventory; } // True jika objek sudah dipasang di arena.
    }

    private Vector3 posisiAwal; // Menyimpan posisi awal objek inventory.
    private bool sedangDrag = false; // Menandai apakah objek sedang ditarik.
    private bool sedangCooldown = false; // Menandai apakah inventory sedang cooldown.
    private float waktuCooldownSelesai; // Waktu berakhirnya cooldown.
    private Camera kameraUtama; // Kamera yang digunakan untuk membaca posisi mouse.


    // =========================================================
    // 3. PENGATURAN HP DEFENSE
    // =========================================================

    [Header("Pengaturan HP")]
    public int hpMaksimum = 20; // Batas maksimum HP defense.
    public int HPSekarang; // HP defense saat ini.


    // =========================================================
    // 4. PENGATURAN SERANGAN DEFENSE
    // =========================================================

    [Header("Pengaturan Serangan")]
    public int damage = 2; // Damage yang diberikan kepada musuh.
    public float jedaTembak = 1f; // Jeda antara dua tembakan.
    public float kecepatanProyektil = 8f; // Kecepatan proyektil.

    public GameObject prefabProyektil; // Prefab proyektil yang ditembakkan.
    public Transform titikTembak; // Titik keluarnya proyektil.

    private float waktuTembakBerikutnya; // Waktu defense boleh menembak lagi.


    // =========================================================
    // 5. REFERENSI MUSUH YANG SEDANG DITARGET
    // =========================================================

    private MusuhHealth targetMusuh; // Menyimpan musuh yang sedang menjadi target.


    // =========================================================
    // 6. INISIALISASI OBJEK
    // =========================================================

    private void Start()
    {
        // Simpan posisi awal untuk mengembalikan objek inventory.
        posisiAwal = transform.position;

        // Isi HP dengan nilai maksimum saat objek mulai aktif.
        HPSekarang = hpMaksimum;

        // Ambil kamera utama jika referensinya belum tersedia.
        kameraUtama = Camera.main;

        // Cari KotakSetting secara otomatis jika belum diisi di Inspector.
        if (kotakSetting == null)
            kotakSetting = FindFirstObjectByType<KotakSetting>();
    }


    // =========================================================
    // 7. INPUT MOUSE DAN COOLDOWN
    // =========================================================

    private void Update()
    {
        // Periksa apakah cooldown sudah selesai.
        if (sedangCooldown && Time.time >= waktuCooldownSelesai)
        {
            sedangCooldown = false;
        }

        // Defense yang sudah terpasang tidak bisa ditarik dari arena.
        // Klik kanan digunakan untuk menghancurkan defense tersebut.
        if (ObjekSudahDiletakkan)
        {
            if (Input.GetMouseButtonDown(1))
                HancurkanJikaTerpasang();

            // Defense di arena tetap dapat menjalankan fungsi serangan.
            UpdateSerangan();
            return;
        }

        // Jika objek masih di inventory dan sedang ditarik,
        // ikuti posisi mouse.
        if (sedangDrag && kameraUtama != null)
        {
            Vector3 posisiMouse = kameraUtama.ScreenToWorldPoint(
                Input.mousePosition
            );

            // Pertahankan posisi Z agar tidak berpindah dari bidang 2D.
            posisiMouse.z = transform.position.z;

            // Pindahkan objek mengikuti mouse.
            transform.position = posisiMouse;
        }
    }


    // =========================================================
    // 8. MULAI MENARIK OBJEK DARI INVENTORY
    // =========================================================

    private void OnMouseDown()
    {
        // Objek yang sudah dipasang tidak boleh ditarik kembali.
        if (ObjekSudahDiletakkan)
            return;

        // Jangan mulai drag ketika inventory masih cooldown.
        if (sedangCooldown)
            return;

        // Pastikan referensi resource tersedia.
        if (resourceManager == null)
        {
            Debug.LogWarning("ResourceManager belum diatur.", this);
            return;
        }

        // Pastikan resource pemain cukup untuk memasang defense.
        if (!resourceManager.CukupSumberDaya(biayaSumberDaya))
            return;

        // Mulai proses drag.
        sedangDrag = true;
    }


    // =========================================================
    // 9. MELEPAS OBJEK DAN MEMASANG DEFENSE
    // =========================================================

    private void OnMouseUp()
    {
        // Abaikan jika objek tidak sedang ditarik.
        if (!sedangDrag)
            return;

        // Hentikan proses drag.
        sedangDrag = false;

        // Jalankan pemeriksaan dan proses penempatan.
        LepaskanObjek();
    }


    private void LepaskanObjek()
    {
        // Pastikan arena dan sistem resource tersedia.
        if (kotakSetting == null || resourceManager == null)
        {
            // Batalkan penempatan dan kembalikan objek.
            transform.position = posisiAwal;
            return;
        }

        // Ubah posisi objek menjadi koordinat petak arena.
        Vector2Int cellTujuan =
            kotakSetting.WorldToCell(transform.position);

        // Ambil data petak yang dituju.
        KotakSetting.DataUbin dataPetak =
            kotakSetting.GetCellData(cellTujuan);

        // Batalkan jika petak berada di luar arena.
        if (dataPetak == null)
        {
            transform.position = posisiAwal;
            return;
        }

        // Batalkan jika petak tidak aman untuk ditempati.
        if (!dataPetak.aman)
        {
            transform.position = posisiAwal;
            return;
        }

        // =====================================================
        // PERBAIKAN BUG: CEK APAKAH PETAK SUDAH DITEMPATI
        // =====================================================

        // Jika sudah ada defense pada petak tujuan,
        // batalkan penempatan sebelum resource dikurangi.
        if (PetakSudahTerisi(cellTujuan))
        {
            // Kembalikan objek inventory ke posisi semula.
            transform.position = posisiAwal;

            // Hentikan proses agar tidak membuat defense baru.
            return;
        }

        // =====================================================
        // CEK RESOURCE SEBELUM MEMASANG DEFENSE
        // =====================================================

        // Periksa kembali resource sebelum melakukan transaksi.
        if (!resourceManager.CukupSumberDaya(biayaSumberDaya))
        {
            transform.position = posisiAwal;
            return;
        }

        // Tentukan posisi tengah petak menggunakan sistem grid.
        Vector3 posisiPetak = kotakSetting.CelltoWorld(cellTujuan);

        // Buat salinan defense pada posisi petak yang valid.
        GameObject defenseBaru = Instantiate(
            gameObject,
            posisiPetak,
            transform.rotation
        );

        // Ambil script dari salinan defense yang baru dibuat.
        InventoryDragDrop scriptDefenseBaru =
            defenseBaru.GetComponent<InventoryDragDrop>();

        // Tandai salinan sebagai defense yang sudah terpasang.
        if (scriptDefenseBaru != null)
        {
            scriptDefenseBaru.objekInventory = false;

            // Sinkronkan referensi yang diperlukan.
            scriptDefenseBaru.kotakSetting = kotakSetting;
            scriptDefenseBaru.resourceManager = resourceManager;

            // Pastikan HP salinan dimulai dari maksimum.
            scriptDefenseBaru.HPSekarang =
                scriptDefenseBaru.hpMaksimum;
        }

        // Kurangi resource setelah objek berhasil dibuat.
        resourceManager.GunakanSumberDaya(biayaSumberDaya);

        // Kembalikan objek sumber ke posisi inventory.
        transform.position = posisiAwal;

        // Aktifkan cooldown untuk item inventory ini.
        sedangCooldown = true;
        waktuCooldownSelesai = Time.time + cooldownDurasi;
    }


    // =========================================================
    // 10. MEMERIKSA APAKAH PETAK SUDAH TERISI DEFENSE
    // =========================================================

    private bool PetakSudahTerisi(Vector2Int cellTujuan)
    {
        // Cari semua objek defense yang memiliki script ini.
        InventoryDragDrop[] semuaDefense =
            FindObjectsOfType<InventoryDragDrop>();

        // Periksa setiap defense satu per satu.
        foreach (InventoryDragDrop defense in semuaDefense)
        {
            // Abaikan objek yang masih berada di inventory.
            if (!defense.ObjekSudahDiletakkan)
                continue;

            // Abaikan objek yang sedang menjalankan pemeriksaan.
            if (defense == this)
                continue;

            // Abaikan objek yang sudah tidak aktif.
            if (!defense.gameObject.activeInHierarchy)
                continue;

            // Ubah posisi defense yang sudah terpasang
            // menjadi koordinat petak arena.
            Vector2Int cellDefense =
                kotakSetting.WorldToCell(defense.transform.position);

            // Jika koordinatnya sama, petak sudah ditempati.
            if (cellDefense == cellTujuan)
                return true;
        }

        // Tidak ditemukan defense lain pada petak tersebut.
        return false;
    }


    // =========================================================
    // 11. MENERIMA DAMAGE DARI MUSUH
    // =========================================================

    public void TerimaDamage(int jumlahDamage)
    {
        // Hanya defense yang sudah terpasang yang menerima damage.
        if (!ObjekSudahDiletakkan)
            return;

        // Abaikan damage yang tidak positif.
        if (jumlahDamage <= 0)
            return;

        // Kurangi HP defense.
        HPSekarang -= jumlahDamage;

        // Hancurkan defense jika HP habis.
        if (HPSekarang <= 0)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // 12. MENGHANCURKAN DEFENSE DENGAN KLIK KANAN
    // =========================================================

    public void HancurkanJikaTerpasang()
    {
        // Pastikan yang dihancurkan memang defense di arena.
        if (ObjekSudahDiletakkan)
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // 13. MENENTUKAN TARGET MUSUH DAN MENEMBAK
    // =========================================================

    private void UpdateSerangan()
    {
        // Pastikan objek ini merupakan defense yang terpasang.
        if (!ObjekSudahDiletakkan)
            return;

        // Pastikan prefab proyektil sudah diatur.
        if (prefabProyektil == null)
            return;

        // Tunggu hingga jeda tembak selesai.
        if (Time.time < waktuTembakBerikutnya)
            return;

        // Cari musuh terdekat yang berada di sebelah kanan
        // dan pada baris petak yang sama dengan defense.
        targetMusuh = CariMusuhSatuJalur();

        // Jangan menembak jika tidak ada target yang sesuai.
        if (targetMusuh == null)
            return;

        // Tentukan posisi awal proyektil.
        Vector3 posisiTembak = titikTembak != null
            ? titikTembak.position
            : transform.position;

        // Buat proyektil baru.
        GameObject proyektilBaru = Instantiate(
            prefabProyektil,
            posisiTembak,
            Quaternion.identity
        );

        // Atur damage, kecepatan, dan arah gerak proyektil.
        Proyektil scriptProyektil =
            proyektilBaru.GetComponent<Proyektil>();

        if (scriptProyektil != null)
        {
            scriptProyektil.AturProyektil(
                damage,
                kecepatanProyektil,
                Vector2.right
            );
        }

        // Tentukan waktu tembak berikutnya.
        waktuTembakBerikutnya = Time.time + jedaTembak;
    }


    // =========================================================
    // 14. MENCARI MUSUH PADA BARIS PETAK YANG SAMA
    // =========================================================

    private MusuhHealth CariMusuhSatuJalur()
    {
        // Ambil semua musuh yang memiliki script MusuhHealth.
        MusuhHealth[] semuaMusuh =
            FindObjectsOfType<MusuhHealth>();

        // Simpan kandidat musuh terdekat.
        MusuhHealth musuhTerdekat = null;

        // Jarak awal dibuat sangat besar agar kandidat pertama
        // yang memenuhi syarat bisa dipilih.
        float jarakTerdekat = float.MaxValue;

        // Tentukan baris petak defense.
        Vector2Int cellPertahanan =
            kotakSetting.WorldToCell(transform.position);

        // Periksa setiap musuh.
        foreach (MusuhHealth musuh in semuaMusuh)
        {
            // Abaikan referensi musuh yang sudah tidak valid.
            if (musuh == null)
                continue;

            // Abaikan musuh yang berada di sebelah kiri defense.
            if (musuh.transform.position.x <= transform.position.x)
                continue;

            // Tentukan baris petak musuh.
            Vector2Int cellMusuh =
                kotakSetting.WorldToCell(musuh.transform.position);

            // Abaikan musuh yang berada pada baris berbeda.
            if (cellMusuh.y != cellPertahanan.y)
                continue;

            // Hitung jarak horizontal menuju musuh.
            float jarak = musuh.transform.position.x
                        - transform.position.x;

            // Pilih musuh terdekat di antara kandidat yang valid.
            if (jarak < jarakTerdekat)
            {
                jarakTerdekat = jarak;
                musuhTerdekat = musuh;
            }
        }

        // Kembalikan musuh terdekat atau null jika tidak ada.
        return musuhTerdekat;
    }
}