using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    [Header("Pengaturan Quest")]
    [SerializeField] private int targetKayu = 3;
    [SerializeField] private float batasWaktu = 60f;

    private int kayuTerkumpul = 0;
    private float waktuTersisa;
    private bool questSelesai = false;

    private void Start()
    {
        waktuTersisa = batasWaktu;

        Debug.Log("QUEST: Kumpulkan " + targetKayu + " kayu!");
        Debug.Log("Batas waktu: " + batasWaktu + " detik.");
    }

    private void Update()
    {
        if (questSelesai)
            return;

        waktuTersisa -= Time.deltaTime;

        if (waktuTersisa <= 0)
        {
            questSelesai = true;
            Debug.Log("QUEST GAGAL! Waktu habis.");
        }
    }

    public void TambahKayu(int jumlah)
    {
        if (questSelesai)
            return;

        kayuTerkumpul += jumlah;

        Debug.Log("Progress Quest: " +
            kayuTerkumpul + "/" + targetKayu);

        if (kayuTerkumpul >= targetKayu)
        {
            questSelesai = true;
            Debug.Log("QUEST SELESAI! Target kayu tercapai.");
        }
    }
}
