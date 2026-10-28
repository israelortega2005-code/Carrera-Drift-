using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorJuego : MonoBehaviour
{
    public GameObject panelMenuInicio;
    public GameObject panelGameOver;
    public GameObject panelVictoria;

    void Start()
    {
        // El tiempo fluye normal para que los autos no se queden pegados.
        Time.timeScale = 1f;

        if (panelMenuInicio) panelMenuInicio.SetActive(true);
        if (panelGameOver) panelGameOver.SetActive(false);
        if (panelVictoria) panelVictoria.SetActive(false);
    }

    public void IniciarJuego()
    {
        if (panelMenuInicio) panelMenuInicio.SetActive(false);
    }

    public void ActivarGameOver()
    {
        Time.timeScale = 0f;
        if (panelGameOver) panelGameOver.SetActive(true);
    }

    public void ActivarVictoria()
    {
        Time.timeScale = 0f;
        if (panelVictoria) panelVictoria.SetActive(true);
    }

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}