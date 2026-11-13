using UnityEngine;
using UnityEngine.Tilemaps;

public class InventoryTilemap : MonoBehaviour
{
    [Header("Ukuran Inventory")]
    [Min(1)] public int Kolom = 5;
    [Min(1)] public int Baris = 1;

    [Header("Tilemap Inventory")]
    [SerializeField] private Tilemap tilemapInventory;
    [SerializeField] private Grid grid;
    [SerializeField] private TileBase tileSlot;

    [Header("Objek Sumber Inventory")]
    public GameObject[] objekInventory;

    [Header("Posisi Awal Cell")]
    public Vector2Int cellAwal = Vector2Int.zero;

    private void Awake()
    {
        // Ambil referensi otomatis jika belum diisi
        if (tilemapInventory == null)
            tilemapInventory = GetComponentInChildren<Tilemap>();

        if (grid == null)
        {
            grid = GetComponentInParent<Grid>();

            if (grid == null)
                grid = GetComponentInChildren<Grid>();
        }

        // Pastikan komponen tersedia
        if (tilemapInventory == null || grid == null)
        {
            Debug.LogError(
                "Tilemap atau Grid Inventory belum ditemukan!",
                this
            );
            return;
        }

        // Buat slot inventory
        BuatSlot();

        // Tempatkan objek sumber pada slot
        TempatkanObjek();
    }

    private void BuatSlot()
    {
        if (tileSlot == null)
        {
            Debug.LogError("Tile Slot belum diisi!", this);
            return;
        }

        tilemapInventory.ClearAllTiles();

        for (int y = 0; y < Baris; y++)
        {
            for (int x = 0; x < Kolom; x++)
            {
                Vector3Int cell = new Vector3Int(
                    cellAwal.x + x,
                    cellAwal.y + y,
                    0
                );

                tilemapInventory.SetTile(cell, tileSlot);
            }
        }
    }

    private void TempatkanObjek()
    {
        if (objekInventory == null)
            return;

        int jumlahSlot = Kolom * Baris;

        if (objekInventory.Length > jumlahSlot)
        {
            Debug.LogWarning(
                "Jumlah objek melebihi jumlah slot inventory!",
                this
            );
        }

        for (int i = 0;
             i < Mathf.Min(objekInventory.Length, jumlahSlot);
             i++)
        {
            GameObject objek = objekInventory[i];

            if (objek == null)
                continue;

            // Urutan slot: kiri ke kanan, lalu baris berikutnya
            int x = i % Kolom;
            int y = i / Kolom;

            Vector3Int cell = new Vector3Int(
                cellAwal.x + x,
                cellAwal.y + y,
                0
            );

            // Letakkan objek di tengah slot
            objek.transform.position =
                grid.GetCellCenterWorld(cell);
        }
    }
}