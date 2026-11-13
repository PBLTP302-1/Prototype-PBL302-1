
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Prefab Defense")]
    public GameObject prefabDefense;

    [Header("Gambar Defense di UI")]
    public Image gambarDefense;

    [Header("Referensi")]
    public KotakSetting kotakSetting;
    private InventoryUIManager inventoryManager;

    [Header("Tampilan Cooldown")]
    public Image cooldownOverlay;
    public TMP_Text teksCooldown;

    [Min(0f)]
    public float cooldownDurasi = 3f;

    private bool sedangCooldown;
    private float waktuCooldownSelesai;

    private GameObject objekDrag;
    private Camera kameraUtama;

    [Header("Tampilan Objek Saat Drag")]
    public string sortingLayerSaatDrag = "Default";
    public int sortingOrderSaatDrag = 100;

    private SpriteRenderer[] rendererDrag;
    private string[] sortingLayerAsli;
    private int[] sortingOrderAsli;

    // =====================================================
    // INISIALISASI
    // =====================================================

    public void Inisialisasi(
        InventoryUIManager manager,
        KotakSetting arena)
    {
        inventoryManager = manager;
        kotakSetting = arena;
        kameraUtama = Camera.main;

        PerbaruiTampilanCooldown(0f);
    }

    // =====================================================
    // MENAMPILKAN IKON DARI PREFAB
    // =====================================================

    public void PerbaruiIkonDefense()
    {
        if (gambarDefense == null)
        {
            Debug.LogError(
                "Image ikon belum dihubungkan pada " + name,
                this
            );
            return;
        }

        if (prefabDefense == null)
        {
            gambarDefense.sprite = null;
            gambarDefense.enabled = false;
            return;
        }

        // Cari SpriteRenderer pada prefab dan objek anaknya.
        SpriteRenderer sumberSprite =
            prefabDefense.GetComponentInChildren<SpriteRenderer>(true);

        if (sumberSprite == null || sumberSprite.sprite == null)
        {
            Debug.LogError(
                "SpriteRenderer atau sprite tidak ditemukan pada prefab: "
                + prefabDefense.name,
                prefabDefense
            );

            gambarDefense.sprite = null;
            gambarDefense.enabled = false;
            return;
        }

        // Tampilkan sprite prefab pada Image UI.
        gambarDefense.sprite = sumberSprite.sprite;
        gambarDefense.preserveAspect = true;
        gambarDefense.color = Color.white;
        gambarDefense.enabled = true;
    }

    // =====================================================
    // COOLDOWN
    // =====================================================

    private void Update()
    {
        if (!sedangCooldown)
            return;

        float sisaWaktu = waktuCooldownSelesai - Time.time;

        if (sisaWaktu <= 0f)
        {
            sedangCooldown = false;
            PerbaruiTampilanCooldown(0f);
            return;
        }

        PerbaruiTampilanCooldown(sisaWaktu);
    }

    // =====================================================
    // MEMULAI DRAG
    // =====================================================

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (sedangCooldown)
            return;

        if (prefabDefense == null ||
            kotakSetting == null ||
            inventoryManager == null)
        {
            Debug.LogWarning(
                "Prefab defense atau referensi slot belum lengkap.",
                this
            );
            return;
        }

        InventoryDragDrop prefabData =
            prefabDefense.GetComponent<InventoryDragDrop>();

        if (prefabData == null)
        {
            Debug.LogWarning(
                "Prefab defense harus memiliki InventoryDragDrop.",
                prefabDefense
            );
            return;
        }

        if (!inventoryManager.CukupResource(prefabDefense))
            return;

        if (!CobaAmbilPosisiDunia(
            eventData.position, out Vector3 posisiDunia))
            return;

        // Buat instance dari prefab saat drag dimulai.
        objekDrag = Instantiate(
            prefabDefense,
            posisiDunia,
            prefabDefense.transform.rotation
        );

        InventoryDragDrop dataDrag =
            objekDrag.GetComponent<InventoryDragDrop>();

        dataDrag.objekInventory = true;
        dataDrag.kotakSetting = kotakSetting;
        dataDrag.resourceManager = inventoryManager.resourceManager;

        // Nonaktifkan collider selama proses drag.
        Collider2D[] colliders =
            objekDrag.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
            col.enabled = false;

        // Simpan sorting asli.
        rendererDrag =
            objekDrag.GetComponentsInChildren<SpriteRenderer>(true);

        sortingLayerAsli = new string[rendererDrag.Length];
        sortingOrderAsli = new int[rendererDrag.Length];

        for (int i = 0; i < rendererDrag.Length; i++)
        {
            sortingLayerAsli[i] =
                rendererDrag[i].sortingLayerName;

            sortingOrderAsli[i] =
                rendererDrag[i].sortingOrder;

            rendererDrag[i].sortingLayerName =
                sortingLayerSaatDrag;

            rendererDrag[i].sortingOrder =
                sortingOrderSaatDrag + i;
        }
    }

    // =====================================================
    // MENGIKUTI MOUSE
    // =====================================================

    public void OnDrag(PointerEventData eventData)
    {
        if (objekDrag == null)
            return;

        if (!CobaAmbilPosisiDunia(
            eventData.position, out Vector3 posisiDunia))
            return;

        objekDrag.transform.position = posisiDunia;
    }

    // =====================================================
    // SELESAI DRAG
    // =====================================================

    public void OnEndDrag(PointerEventData eventData)
    {
        if (objekDrag == null)
            return;

        if (!CobaAmbilPosisiDunia(
            eventData.position, out Vector3 posisiDunia))
        {
            HapusObjekDrag();
            return;
        }

        Vector2Int cellTujuan =
            kotakSetting.WorldToCell(posisiDunia);

        KotakSetting.DataUbin dataPetak =
            kotakSetting.GetCellData(cellTujuan);

        // Petak harus valid dan bisa ditempati.
        if (dataPetak == null || !dataPetak.aman)
        {
            HapusObjekDrag();
            return;
        }

        // Jangan izinkan dua defense pada petak yang sama.
        if (inventoryManager.PetakSudahTerisi(cellTujuan))
        {
            HapusObjekDrag();
            return;
        }

        // Periksa resource sebelum menggunakannya.
        if (!inventoryManager.CukupResource(prefabDefense))
        {
            HapusObjekDrag();
            return;
        }

        if (!inventoryManager.GunakanResource(prefabDefense))
        {
            HapusObjekDrag();
            return;
        }

        // Tempatkan tepat di tengah petak.
        objekDrag.transform.position =
            kotakSetting.CelltoWorld(cellTujuan);

        InventoryDragDrop dataDefense =
            objekDrag.GetComponent<InventoryDragDrop>();

        dataDefense.objekInventory = false;
        dataDefense.kotakSetting = kotakSetting;
        dataDefense.resourceManager =
            inventoryManager.resourceManager;

        // Aktifkan kembali collider setelah berhasil dipasang.
        Collider2D[] colliders =
            objekDrag.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
            col.enabled = true;

        PulihkanSortingAsli();
        MulaiCooldown();

        // Jangan hancurkan objek yang sudah dipasang.
        objekDrag = null;
    }

    // =====================================================
    // KONVERSI POSISI LAYAR KE DUNIA
    // =====================================================

    private bool CobaAmbilPosisiDunia(
        Vector2 posisiLayar,
        out Vector3 posisiDunia)
    {
        posisiDunia = Vector3.zero;

        if (kameraUtama == null)
            kameraUtama = Camera.main;

        if (kameraUtama == null)
            return false;

        float kedalaman =
            Mathf.Abs(kameraUtama.transform.position.z);

        Vector3 posisiLayar3D = new Vector3(
            posisiLayar.x,
            posisiLayar.y,
            kedalaman
        );

        posisiDunia =
            kameraUtama.ScreenToWorldPoint(posisiLayar3D);

        posisiDunia.z = 0f;

        return true;
    }

    // =====================================================
    // HAPUS OBJEK DRAG YANG GAGAL DIPASANG
    // =====================================================

    private void HapusObjekDrag()
    {
        if (objekDrag != null)
            Destroy(objekDrag);

        objekDrag = null;
        rendererDrag = null;
        sortingLayerAsli = null;
        sortingOrderAsli = null;
    }

    // =====================================================
    // PULIHKAN SORTING
    // =====================================================

    private void PulihkanSortingAsli()
    {
        if (rendererDrag == null)
            return;

        for (int i = 0; i < rendererDrag.Length; i++)
        {
            if (rendererDrag[i] == null)
                continue;

            rendererDrag[i].sortingLayerName =
                sortingLayerAsli[i];

            rendererDrag[i].sortingOrder =
                sortingOrderAsli[i];
        }

        rendererDrag = null;
        sortingLayerAsli = null;
        sortingOrderAsli = null;
    }

    // =====================================================
    // COOLDOWN SLOT
    // =====================================================

    private void MulaiCooldown()
    {
        sedangCooldown = true;
        waktuCooldownSelesai = Time.time + cooldownDurasi;

        PerbaruiTampilanCooldown(cooldownDurasi);
    }

    private void PerbaruiTampilanCooldown(float sisaWaktu)
    {
        if (cooldownOverlay != null)
            cooldownOverlay.enabled = sisaWaktu > 0f;

        if (teksCooldown != null)
        {
            teksCooldown.text = sisaWaktu > 0f
                ? Mathf.CeilToInt(sisaWaktu).ToString()
                : "";

            teksCooldown.enabled = sisaWaktu > 0f;
        }
    }

    private void OnDisable()
    {
        if (objekDrag != null)
            Destroy(objekDrag);

        objekDrag = null;
    }
}
