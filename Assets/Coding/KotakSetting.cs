using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class KotakSetting : MonoBehaviour
{
    public class DataUbin
    {
        public bool aman;
    }

    private DataUbin[,] datakotak;
    private Tilemap tanah;
    private Grid garis;
    private Transform visualRoot;

    [Header("Ukuran Medan")]
    public int Panjang = 19;
    public int Lebar = 7;

    [Header("Bentuk Tile")]
    public Tile[] BentukTanah;
    public Tile[] BentukDinding;

    [Header("Pengaturan Jajargenjang")]
    [Tooltip("Pergeseran horizontal setiap baris, dalam unit Grid.")]
    [Min(0f)]
    public float GeserPerBaris = 0.18f;

    [Header("Objek Awal")]
    public MovOrg Orang;

    [Header("Tampilan")]
    public int SortingOrderVisual = 0;

    private readonly Dictionary<Texture2D, Material> materialCache =
        new Dictionary<Texture2D, Material>();

    // Mengubah koordinat cell menjadi posisi pusat cell di dunia.
    public Vector3 CelltoWorld(Vector2Int cell)
    {
        if (garis == null)
        {
            Debug.LogError("Grid tidak ditemukan!", this);
            return Vector3.zero;
        }

        Vector3 ukuran = garis.cellSize;

        float x = (cell.x + 0.5f) * ukuran.x
                + (cell.y + 0.5f) * GeserPerBaris;

        float y = (cell.y + 0.5f) * ukuran.y;

        return garis.transform.TransformPoint(
            new Vector3(x, y, 0f)
        );
    }

    // Mengubah posisi dunia menjadi koordinat cell.
    public Vector2Int WorldToCell(Vector3 posisi)
    {
        if (garis == null)
        {
            Debug.LogError("Grid tidak ditemukan!", this);
            return Vector2Int.zero;
        }

        Vector3 lokal =
            garis.transform.InverseTransformPoint(posisi);

        Vector3 ukuran = garis.cellSize;

        if (ukuran.x <= 0f || ukuran.y <= 0f)
        {
            Debug.LogError("Ukuran Grid tidak valid!", this);
            return Vector2Int.zero;
        }

        int y = Mathf.FloorToInt(lokal.y / ukuran.y);

        float xTanpaGeser =
            lokal.x - (lokal.y / ukuran.y) * GeserPerBaris;

        int x = Mathf.FloorToInt(xTanpaGeser / ukuran.x);

        return new Vector2Int(x, y);
    }

    // Mengambil data cell jika koordinatnya berada dalam batas arena.
    public DataUbin GetCellData(Vector2Int cell)
    {
        if (datakotak == null)
            return null;

        if (cell.x < 0 || cell.x >= Panjang ||
            cell.y < 0 || cell.y >= Lebar)
        {
            return null;
        }

        return datakotak[cell.x, cell.y];
    }

    private void Awake()
    {
        tanah = GetComponentInChildren<Tilemap>();

        if (tanah == null)
        {
            Debug.LogError("Tilemap tidak ditemukan!", this);
            enabled = false;
            return;
        }

        garis = GetComponentInChildren<Grid>();

        if (garis == null)
            garis = GetComponentInParent<Grid>();

        if (garis == null)
        {
            Debug.LogError("Grid tidak ditemukan!", this);
            enabled = false;
            return;
        }

        if (Panjang <= 0 || Lebar <= 0 ||
            BentukTanah == null || BentukTanah.Length == 0 ||
            BentukDinding == null || BentukDinding.Length == 0)
        {
            Debug.LogError(
                "Periksa ukuran medan dan array Tile di Inspector!",
                this
            );

            enabled = false;
            return;
        }

        datakotak = new DataUbin[Panjang, Lebar];

        for (int y = 0; y < Lebar; y++)
        {
            for (int x = 0; x < Panjang; x++)
            {
                bool petakPinggir =
                    x == 0 || y == 0 ||
                    x == Panjang - 1 ||
                    y == Lebar - 1;

                datakotak[x, y] = new DataUbin
                {
                    aman = !petakPinggir
                };
            }
        }
    }

    private void Start()
    {
        TilemapRenderer tileRenderer =
            tanah.GetComponent<TilemapRenderer>();

        if (tileRenderer != null)
            tileRenderer.enabled = false;

        // Visual digambar oleh mesh, bukan TilemapRenderer.
        // Collider Tilemap juga dimatikan karena tidak mengikuti
        // bentuk visual yang miring.
        TilemapCollider2D tileCollider =
            tanah.GetComponent<TilemapCollider2D>();

        if (tileCollider != null)
            tileCollider.enabled = false;

        BuatVisualMedan();

        if (Orang != null)
            Orang.Spawn(this, new Vector2Int(1, 1));
    }

    private void BuatVisualMedan()
    {
        Transform lama =
            garis.transform.Find("VisualJajargenjang");

        if (lama != null)
        {
            lama.gameObject.SetActive(false);
            Destroy(lama.gameObject);
        }

        GameObject root = new GameObject("VisualJajargenjang");

        visualRoot = root.transform;
        visualRoot.SetParent(garis.transform, false);
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        Vector3 ukuran = garis.cellSize;

        for (int y = 0; y < Lebar; y++)
        {
            for (int x = 0; x < Panjang; x++)
            {
                Tile tile;

                if (!datakotak[x, y].aman)
                {
                    tile = BentukDinding[
                        Random.Range(0, BentukDinding.Length)
                    ];
                }
                else
                {
                    tile = BentukTanah[
                        Random.Range(0, BentukTanah.Length)
                    ];
                }

                tanah.SetTile(new Vector3Int(x, y, 0), tile);

                if (tile == null || tile.sprite == null)
                {
                    Debug.LogWarning(
                        $"Tile di cell ({x}, {y}) tidak memiliki Sprite.",
                        this
                    );
                    continue;
                }

                BuatVisualTile(x, y, ukuran, tile);
            }
        }
    }

    private void BuatVisualTile(
        int x, int y, Vector3 ukuran, Tile tile)
    {
        Sprite sprite = tile.sprite;
        Texture2D tekstur = sprite.texture;

        if (!materialCache.TryGetValue(tekstur, out Material material))
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogError(
                    "Shader Sprites/Default tidak ditemukan!",
                    this
                );
                return;
            }

            material = new Material(shader);
            material.mainTexture = tekstur;
            materialCache.Add(tekstur, material);
        }

        GameObject objek = new GameObject($"TileVisual_{x}_{y}");
        objek.transform.SetParent(visualRoot, false);

        MeshFilter filter = objek.AddComponent<MeshFilter>();
        MeshRenderer renderer = objek.AddComponent<MeshRenderer>();

        renderer.sharedMaterial = material;
        renderer.sortingOrder = SortingOrderVisual;

        Mesh mesh = new Mesh();
        mesh.name = $"MeshTile_{x}_{y}";

        float kiri = x * ukuran.x + y * GeserPerBaris;
        float bawah = y * ukuran.y;

        // Setiap petak memiliki empat sudut.
        // Bagian atas bergeser ke kanan sehingga petak miring.
        mesh.vertices = new Vector3[]
        {
            new Vector3(kiri, bawah, 0f),
            new Vector3(kiri + ukuran.x, bawah, 0f),
            new Vector3(
                kiri + ukuran.x + GeserPerBaris,
                bawah + ukuran.y, 0f),
            new Vector3(
                kiri + GeserPerBaris,
                bawah + ukuran.y, 0f)
        };

        // Menggunakan area tekstur sprite.
        // Sprite yang menggunakan tight packing dalam atlas
        // mungkin membutuhkan penanganan UV khusus.
        Rect rect = sprite.textureRect;

        float uMin = rect.xMin / tekstur.width;
        float uMax = rect.xMax / tekstur.width;
        float vMin = rect.yMin / tekstur.height;
        float vMax = rect.yMax / tekstur.height;

        mesh.uv = new Vector2[]
        {
            new Vector2(uMin, vMin),
            new Vector2(uMax, vMin),
            new Vector2(uMax, vMax),
            new Vector2(uMin, vMax)
        };

        mesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        Color warna = tile.color;

        mesh.colors = new Color[]
        {
            warna, warna, warna, warna
        };

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;
    }

    private void OnDestroy()
    {
        foreach (Material material in materialCache.Values)
        {
            if (material != null)
                Destroy(material);
        }

        materialCache.Clear();
    }
}