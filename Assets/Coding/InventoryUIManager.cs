
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Pengaturan Slot")]
    public InventorySlotUI prefabSlot;
    public Transform parentSlot;

    [Min(1)]
    public int jumlahSlot = 7;

    [Header("Daftar Prefab Defense")]
    public GameObject[] daftarPrefabDefense;

    [Header("Referensi Sistem")]
    public KotakSetting kotakSetting;
    public ResourceManager resourceManager;

    private void Start()
    {
        if (parentSlot == null)
        {
            Debug.LogError("Parent Slot belum diisi!", this);
            return;
        }

        if (prefabSlot == null)
        {
            Debug.LogError("Prefab Slot belum diisi!", this);
            return;
        }

        if (kotakSetting == null)
            kotakSetting = FindFirstObjectByType<KotakSetting>();

        if (kotakSetting == null)
        {
            Debug.LogError("KotakSetting tidak ditemukan!", this);
            return;
        }

        // Cari slot yang sudah ada agar tidak menggandakan slot.
        InventorySlotUI[] slotAwal =
            parentSlot.GetComponentsInChildren<InventorySlotUI>(true);

        InventorySlotUI slotPertama = null;

        foreach (InventorySlotUI slot in slotAwal)
        {
            if (slot.transform.parent != parentSlot)
                continue;

            if (slotPertama == null)
                slotPertama = slot;
            else
                Destroy(slot.gameObject);
        }

        if (slotPertama == null)
            slotPertama = Instantiate(prefabSlot, parentSlot);

        AturSlot(slotPertama, 0);

        for (int i = 1; i < jumlahSlot; i++)
        {
            InventorySlotUI slotBaru =
                Instantiate(prefabSlot, parentSlot);

            AturSlot(slotBaru, i);
        }
    }

    // =====================================================
    // MENGATUR PREFAB DAN IKON SETIAP SLOT
    // =====================================================

    private void AturSlot(InventorySlotUI slot, int indeks)
    {
        slot.gameObject.name = "SlotInventory_" + (indeks + 1);

        slot.Inisialisasi(this, kotakSetting);

        if (daftarPrefabDefense != null &&
            indeks < daftarPrefabDefense.Length)
        {
            slot.prefabDefense = daftarPrefabDefense[indeks];
        }
        else
        {
            slot.prefabDefense = null;
        }

        // Penting: jalankan setelah prefabDefense diisi.
        slot.PerbaruiIkonDefense();
    }

    // =====================================================
    // CEK RESOURCE
    // =====================================================

    public bool CukupResource(GameObject prefabDefense)
    {
        if (resourceManager == null || prefabDefense == null)
            return false;

        InventoryDragDrop dataDefense =
            prefabDefense.GetComponent<InventoryDragDrop>();

        if (dataDefense == null)
        {
            Debug.LogWarning(
                "Prefab defense tidak memiliki InventoryDragDrop!",
                prefabDefense
            );
            return false;
        }

        return resourceManager.CukupSumberDaya(
            dataDefense.biayaSumberDaya
        );
    }

    // =====================================================
    // GUNAKAN RESOURCE
    // =====================================================

    public bool GunakanResource(GameObject prefabDefense)
    {
        if (resourceManager == null || prefabDefense == null)
            return false;

        InventoryDragDrop dataDefense =
            prefabDefense.GetComponent<InventoryDragDrop>();

        if (dataDefense == null)
            return false;

        if (!resourceManager.CukupSumberDaya(
            dataDefense.biayaSumberDaya))
        {
            return false;
        }

        resourceManager.GunakanSumberDaya(
            dataDefense.biayaSumberDaya
        );

        return true;
    }

    // =====================================================
    // CEK PETAK SUDAH TERISI
    // =====================================================

    public bool PetakSudahTerisi(Vector2Int cellTujuan)
    {
        if (kotakSetting == null)
            return true;

        InventoryDragDrop[] semuaDefense =
            FindObjectsOfType<InventoryDragDrop>();

        foreach (InventoryDragDrop defense in semuaDefense)
        {
            if (defense == null)
                continue;

            if (!defense.ObjekSudahDiletakkan)
                continue;

            if (!defense.gameObject.activeInHierarchy)
                continue;

            if (defense.kotakSetting != kotakSetting)
                continue;

            Vector2Int cellDefense =
                kotakSetting.WorldToCell(defense.transform.position);

            if (cellDefense == cellTujuan)
                return true;
        }

        return false;
    }
}
