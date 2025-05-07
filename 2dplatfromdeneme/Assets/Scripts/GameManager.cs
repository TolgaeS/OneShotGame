using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

       public void GameOver()
    {
        Debug.Log("Oyun Bitti!");
        // GameOver işlemleri (ekranı gösterme, zaman durdurma, vb.)
        Time.timeScale = 0;  // Oyunu durdur
    }
}
