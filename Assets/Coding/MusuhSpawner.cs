using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusuhSpawner : MonoBehaviour
{
    [Header("Referensi")]
    [SerializeField]
    private KotakSetting kotakSetting;

    [SerializeField]
    private GameObject prefabMusuh;

    [Header("Array Koordinat Spawn")]
    [SerializeField]
    private Vector2Int[] petakSpawn;

    [Header("Pengaturan Spawn")]
    [SerializeField, Min(0.1f)]
    private float intervalSpawn = 3f;

    [SerializeField, Min(1)]
    private int maksimumMusuh = 5;

    [Header("Tampilan")]
    [SerializeField]
    private int sortingOrderMusuh = 10;

    private readonly List<GameObject> daftarMusuh =
        new List<GameObject>();

    private void Start()
    {
        if (kotakSetting == null)
        {
            Debug.LogError(
                "KotakSetting belum dihubungkan!",
                this
            );
            return;
        }

        if (prefabMusuh == null)
        {
            Debug.LogError(
                "Prefab Musuh belum dihubungkan!",
                this
            );
            return;
        }

        if (petakSpawn == null || petakSpawn.Length == 0)
        {
            Debug.LogError(
                "Array petakSpawn masih kosong!",
                this
            );
            return;
        }

        StartCoroutine(SpawnBerkala());
    }

    private IEnumerator SpawnBerkala()
    {
        while (true)
        {
            daftarMusuh.RemoveAll(musuh => musuh == null);

            if (daftarMusuh.Count < maksimumMusuh)
                SpawnMusuh();

            yield return new WaitForSeconds(intervalSpawn);
        }
    }

    private void SpawnMusuh()
    {
        // Pilih koordinat spawn secara acak.
        Vector2Int cell = petakSpawn[
            Random.Range(0, petakSpawn.Length)
        ];

        KotakSetting.DataUbin data =
            kotakSetting.GetCellData(cell);

        if (data == null || !data.aman)
        {
            Debug.LogWarning(
                "Koordinat spawn tidak valid atau tidak aman: " + cell,
                this
            );
            return;
        }

        // Ambil pusat petak menggunakan konversi arena jajargenjang.
        Vector3 posisiSpawn = kotakSetting.CelltoWorld(cell);

        GameObject musuh = Instantiate(
            prefabMusuh,
            posisiSpawn,
            Quaternion.identity
        );

        // Atur urutan render agar musuh terlihat di depan arena.
        SpriteRenderer[] semuaSprite =
            musuh.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in semuaSprite)
            sr.sortingOrder = sortingOrderMusuh;

        daftarMusuh.Add(musuh);

        Debug.Log(
            "Musuh spawn pada petak " + cell +
            " di posisi " + posisiSpawn,
            musuh
        );
    }
}