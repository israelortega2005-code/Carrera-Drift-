using UnityEngine;

public class CamaraMultijugador : MonoBehaviour
{
    [Header("--- LOS AUTOS A SEGUIR ---")]
    public Transform jugador1;
    public Transform jugador2;

    [Header("--- CONFIGURACIÓN DEL ZOOM ---")]
    [Tooltip("El tamaño de la cámara cuando los autos están muy pegados")]
    public float zoomMinimo = 5f;
    [Tooltip("El tamaño máximo de la cámara cuando los autos se separan mucho")]
    public float zoomMaximo = 15f;
    [Tooltip("A qué distancia la cámara empieza a hacer el zoom hacia atrás")]
    public float limiteDistancia = 20f;
    [Tooltip("Qué tan suave y fluido es el movimiento (0 = brusco, 1 = lento)")]
    public float suavidad = 0.5f;

    private Camera camara;
    private Vector3 velocidadMovimiento;

    void Start()
    {
        camara = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // Si no hemos asignado los autos, la cámara no hace nada (evita errores)
        if (jugador1 == null || jugador2 == null) return;

        MoverCamaraAlCentro();
        AjustarZoomInteligente();
    }

    private void MoverCamaraAlCentro()
    {
        // Calculamos la mitad exacta de la distancia entre el Jugador 1 y el Jugador 2
        Vector3 puntoMedio = (jugador1.position + jugador2.position) / 2f;

        // Mantenemos la profundidad Z original de la cámara para que no atraviese el suelo
        Vector3 nuevaPosicion = new Vector3(puntoMedio.x, puntoMedio.y, transform.position.z);

        // Movemos la cámara hacia ese punto medio de forma suave
        transform.position = Vector3.SmoothDamp(transform.position, nuevaPosicion, ref velocidadMovimiento, suavidad);
    }

    private void AjustarZoomInteligente()
    {
        // Medimos qué tan lejos están los autos en metros de Unity
        float distanciaEntreAutos = Vector3.Distance(jugador1.position, jugador2.position);

        // Calculamos matemáticamente qué tamaño debería tener la cámara según esa distancia
        float zoomDeseado = Mathf.Lerp(zoomMinimo, zoomMaximo, distanciaEntreAutos / limiteDistancia);

        // Aplicamos el tamaño (zoom) de forma suave
        camara.orthographicSize = Mathf.Lerp(camara.orthographicSize, zoomDeseado, Time.deltaTime / suavidad);
    }
}