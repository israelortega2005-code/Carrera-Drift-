using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("--- LA CÁMARA Y EL RIVAL ---")]
    [Tooltip("Arrastra tu Main Camera aquí")]
    public Camera camaraPrincipal;
    [Tooltip("Arrastra tu auto azul (Rival) aquí")]
    public Transform jugador2;

    // NUEVO: Aquí conectaremos tu parlante del motor
    [Header("--- SONIDOS ---")]
    public AudioSource sonidoMotor;

    [Header("--- MOTOR Y RENDIMIENTO (JUGADOR 1) ---")]
    public float fuerzaAceleracion = 25f;
    public float velocidadMaxima = 20f;
    public float fuerzaFrenado = 15f;
    public float desaceleracionLibre = 0.98f;

    [Header("--- DIRECCIÓN Y MANEJO ---")]
    public float sensibilidadGiro = 3.5f;
    public float sensibilidadGiroDrift = 5.5f;
    public float velocidadParaGiroMaximo = 8f;

    [Header("--- FÍSICAS DE DERRAPE (DRIFT) ---")]
    [Range(0f, 1f)] public float agarreLateralNormal = 0.9f;
    [Range(0f, 1f)] public float agarreLateralDrift = 0.05f;
    public float penalizacionVelocidadDrift = 0.95f;

    [Header("--- EFECTOS VISUALES ---")]
    public ParticleSystem humoDerrape;
    public TrailRenderer marcasLlantas;
    public float umbralDerrapeVisual = 3f;

    private float inputAceleracion;
    private float inputGiro;
    private bool usandoFrenoMano;
    private float anguloRotacionActual;
    private Rigidbody2D rb;

    private Vector3 velocidadMovimientoCamara;
    private float zoomMinimo = 5f;
    private float zoomMaximo = 15f;
    private float limiteDistancia = 20f;
    private float suavidadCamara = 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
        anguloRotacionActual = transform.eulerAngles.z;
        DetenerEfectosVisuales();
    }

    void Update()
    {
        CapturarInputs();
        GestionarEfectosVisuales();

        // NUEVO: Activamos la función que controla el ruido del motor cada segundo
        ControlarSonidoMotor();
    }

    void FixedUpdate()
    {
        AplicarFuerzaMotor();
        AplicarFisicasDeDireccion();
        AplicarAgarreYDerrape();
    }

    void LateUpdate()
    {
        ControlarCamaraInteligente();
    }

    private void CapturarInputs()
    {
        if (Input.GetKey(KeyCode.W)) inputAceleracion = 1f;
        else if (Input.GetKey(KeyCode.S)) inputAceleracion = -1f;
        else inputAceleracion = 0f;

        if (Input.GetKey(KeyCode.D)) inputGiro = 1f;
        else if (Input.GetKey(KeyCode.A)) inputGiro = -1f;
        else inputGiro = 0f;

        usandoFrenoMano = Input.GetKey(KeyCode.Space);
    }

    // NUEVO: Esta es la lógica que enciende y apaga el parlante
    private void ControlarSonidoMotor()
    {
        // Si no hemos conectado un parlante, no hace nada para evitar errores
        if (sonidoMotor == null) return;

        // Si el jugador presiona W o S (el input es diferente de 0)
        if (inputAceleracion != 0)
        {
            // Y si el sonido NO está sonando ya, lo encendemos
            if (!sonidoMotor.isPlaying)
            {
                sonidoMotor.Play();
            }
        }
        else // Si soltó el acelerador
        {
            // Apagamos el motor
            if (sonidoMotor.isPlaying)
            {
                sonidoMotor.Stop();
            }
        }
    }

    private void AplicarFuerzaMotor()
    {
        float velocidadFrontal = Vector2.Dot(transform.right, rb.linearVelocity);

        if (inputAceleracion > 0)
        {
            if (rb.linearVelocity.magnitude < velocidadMaxima)
            {
                rb.AddForce(transform.right * inputAceleracion * fuerzaAceleracion, ForceMode2D.Force);
            }
        }
        else if (inputAceleracion < 0)
        {
            rb.AddForce(transform.right * inputAceleracion * fuerzaFrenado, ForceMode2D.Force);
        }
        else
        {
            rb.linearVelocity = rb.linearVelocity * desaceleracionLibre;
        }
    }

    private void AplicarFisicasDeDireccion()
    {
        float factorVelocidad = Mathf.Clamp01(rb.linearVelocity.magnitude / velocidadParaGiroMaximo);
        float direccionVelocidad = Vector2.Dot(rb.linearVelocity, transform.right);
        float multiplicadorReversa = (direccionVelocidad < 0 && inputAceleracion < 0) ? -1f : 1f;
        float sensibilidadActual = usandoFrenoMano ? sensibilidadGiroDrift : sensibilidadGiro;

        anguloRotacionActual -= inputGiro * sensibilidadActual * factorVelocidad * multiplicadorReversa;
        rb.MoveRotation(anguloRotacionActual);
    }

    private void AplicarAgarreYDerrape()
    {
        Vector2 velocidadAdelante = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        Vector2 velocidadLateral = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);
        float factorAgarreActual = usandoFrenoMano ? agarreLateralDrift : agarreLateralNormal;

        velocidadLateral = velocidadLateral * (1f - factorAgarreActual);

        if (usandoFrenoMano && rb.linearVelocity.magnitude > 5f)
        {
            velocidadAdelante = velocidadAdelante * penalizacionVelocidadDrift;
        }

        rb.linearVelocity = velocidadAdelante + velocidadLateral;
    }

    private void GestionarEfectosVisuales()
    {
        float derrapeLateral = Vector2.Dot(rb.linearVelocity, transform.up);
        bool deslizandoPorFisica = Mathf.Abs(derrapeLateral) > umbralDerrapeVisual;
        bool debeEmitir = deslizandoPorFisica || (usandoFrenoMano && rb.linearVelocity.magnitude > 5f && Mathf.Abs(inputGiro) > 0.1f);

        if (debeEmitir)
        {
            if (humoDerrape != null && !humoDerrape.isPlaying) humoDerrape.Play();
            if (marcasLlantas != null) marcasLlantas.emitting = true;
        }
        else
        {
            if (humoDerrape != null && humoDerrape.isPlaying) humoDerrape.Stop();
            if (marcasLlantas != null) marcasLlantas.emitting = false;
        }
    }

    private void DetenerEfectosVisuales()
    {
        if (humoDerrape != null) humoDerrape.Stop();
        if (marcasLlantas != null) marcasLlantas.emitting = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        rb.linearVelocity = rb.linearVelocity * 0.3f;
        DetenerEfectosVisuales();
    }

    private void ControlarCamaraInteligente()
    {
        if (camaraPrincipal == null || jugador2 == null) return;
        Vector3 puntoMedio = (transform.position + jugador2.position) / 2f;
        Vector3 nuevaPosicion = new Vector3(puntoMedio.x, puntoMedio.y, camaraPrincipal.transform.position.z);
        camaraPrincipal.transform.position = Vector3.SmoothDamp(camaraPrincipal.transform.position, nuevaPosicion, ref velocidadMovimientoCamara, suavidadCamara);
        float distanciaEntreAutos = Vector3.Distance(transform.position, jugador2.position);
        float zoomDeseado = Mathf.Lerp(zoomMinimo, zoomMaximo, distanciaEntreAutos / limiteDistancia);
        camaraPrincipal.orthographicSize = Mathf.Lerp(camaraPrincipal.orthographicSize, zoomDeseado, Time.deltaTime / suavidadCamara);
    }
}