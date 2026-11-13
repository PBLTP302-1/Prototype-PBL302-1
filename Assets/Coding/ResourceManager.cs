using System.Collections;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Pengaturan Sumber Daya")]
    [SerializeField, Min(0)]
    private int sumberDayaAwal = 5;

    [SerializeField, Min(1)]
    private int sumberDayaMaksimum = 10;

    [Header("Regenerasi")]
    [SerializeField, Min(0.1f)]
    private float intervalRegenerasi = 3f;

    [SerializeField, Min(1)]
    private int jumlahRegenerasi = 1;

    private int sumberDayaSaatIni;

    public int SumberDayaSaatIni => sumberDayaSaatIni;
    public int SumberDayaMaksimum => sumberDayaMaksimum;

    private void Start()
    {
        sumberDayaSaatIni = Mathf.Clamp(
            sumberDayaAwal,
            0,
            sumberDayaMaksimum
        );

        StartCoroutine(RegenerasiSumberDaya());
    }

    // Periksa apakah sumber daya mencukupi
    public bool CukupSumberDaya(int biaya)
    {
        return sumberDayaSaatIni >= biaya;
    }

    // Gunakan sumber daya
    public bool GunakanSumberDaya(int biaya)
    {
        if (biaya < 0)
            return false;

        if (!CukupSumberDaya(biaya))
            return false;

        sumberDayaSaatIni -= biaya;

        Debug.Log(
            "Sumber daya digunakan: " + biaya +
            ". Sisa: " + sumberDayaSaatIni
        );

        return true;
    }

    // Tambahkan sumber daya
    public void TambahSumberDaya(int jumlah)
    {
        if (jumlah <= 0)
            return;

        sumberDayaSaatIni = Mathf.Min(
            sumberDayaSaatIni + jumlah,
            sumberDayaMaksimum
        );
    }

    // Regenerasi otomatis
    private IEnumerator RegenerasiSumberDaya()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                intervalRegenerasi
            );

            TambahSumberDaya(jumlahRegenerasi);

            Debug.Log(
                "Sumber daya: " +
                sumberDayaSaatIni + "/" +
                sumberDayaMaksimum
            );
        }
    }
}