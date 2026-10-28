using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI txtGanador;
    public TextMeshProUGUI txtTiempo;

    private void Start()
    {
        // Recuperar el ganador guardado
        if (txtGanador != null)
        {
            txtGanador.text = PlayerPrefs.GetString("Ganador", "¡Carrera Finalizada!");
        }

        // Recuperar y formatear el tiempo
        if (txtTiempo != null)
        {
            float tiempoFinal = PlayerPrefs.GetFloat("TiempoFinal", 0f);
            int minutos = Mathf.FloorToInt(tiempoFinal / 60f);
            int segundos = Mathf.FloorToInt(tiempoFinal % 60f);
            txtTiempo.text = string.Format("Tiempo Final: {0:00}:{1:00}", minutos, segundos);
        }
    }

    // Método para volver a cargar la pista de juego
    public void Reintentar()
    {
        SceneManager.LoadScene("Level1");
    }
}