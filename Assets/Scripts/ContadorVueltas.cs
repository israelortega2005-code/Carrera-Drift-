using UnityEngine;
using TMPro;

public class ContadorVueltas : MonoBehaviour
{
    public int totalVueltas = 3;
    private int vueltasJugador1 = 0;
    private int vueltasJugador2 = 0;

    public TextMeshProUGUI textoVueltasJ1;
    public TextMeshProUGUI textoVueltasJ2;
    public ControladorJuego controladorJuego;

    private float tiempoInicio;

    private void Start()
    {
        tiempoInicio = Time.time;
        ActualizarTextos();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Acepta tanto "Player" como "Player1"
        if (collision.CompareTag("Player") || collision.CompareTag("Player1"))
        {
            vueltasJugador1++;
            ActualizarTextos();

            if (vueltasJugador1 >= totalVueltas)
            {
                GuardarTiempo();
                if (controladorJuego != null)
                {
                    controladorJuego.ActivarVictoria();
                }
            }
        }
        else if (collision.CompareTag("Player2"))
        {
            vueltasJugador2++;
            ActualizarTextos();

            if (vueltasJugador2 >= totalVueltas)
            {
                GuardarTiempo();
                if (controladorJuego != null)
                {
                    controladorJuego.ActivarGameOver();
                }
            }
        }
    }

    private void ActualizarTextos()
    {
        if (textoVueltasJ1 != null) textoVueltasJ1.text = "J1 Vueltas: " + vueltasJugador1 + "/" + totalVueltas;
        if (textoVueltasJ2 != null) textoVueltasJ2.text = "J2 Vueltas: " + vueltasJugador2 + "/" + totalVueltas;
    }

    private void GuardarTiempo()
    {
        float tiempoTranscurrido = Time.time - tiempoInicio;
        PlayerPrefs.SetFloat("TiempoFinal", tiempoTranscurrido);
        PlayerPrefs.Save();
    }
}