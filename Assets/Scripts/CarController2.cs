using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RivalController : MonoBehaviour
{
    [Header("--- MOTOR Y RENDIMIENTO (JUGADOR 2) ---")]
    [Tooltip("Fuerza con la que el auto acelera hacia adelante")]
    public float fuerzaAceleracion = 25f;
    [Tooltip("Velocidad máxima que puede alcanzar el auto en rectas")]
    public float velocidadMaxima = 20f;
    [Tooltip("Fuerza de frenado y marcha atrás")]
    public float fuerzaFrenado = 15f;
    [Tooltip("Fricción que frena el auto lentamente al soltar el acelerador")]
    public float desaceleracionLibre = 0.98f;

    [Header("--- DIRECCIÓN Y MANEJO ---")]
    [Tooltip("Qué tan rápido giran las llantas en conducción normal")]
    public float sensibilidadGiro = 3.5f;
    [Tooltip("Sensibilidad extra al tirar el freno de mano para hacer trompos")]
    public float sensibilidadGiroDrift = 5.5f;
    [Tooltip("El auto necesita velocidad para poder girar. Esto controla ese límite")]
    public float velocidadParaGiroMaximo = 8f;

    [Header("--- FÍSICAS DE DERRAPE (DRIFT) ---")]
    [Tooltip("Agarre lateral normal. 1 = Pegado al piso, 0 = Resbala como hielo")]
    [Range(0f, 1f)] public float agarreLateralNormal = 0.9f;
    [Tooltip("Agarre lateral al usar freno de mano. Valores bajos = Más resbalo lateral")]
    [Range(0f, 1f)] public float agarreLateralDrift = 0.05f;
    [Tooltip("Cuánto impulso frontal pierde el auto al derrapar fuerte")]
    public float penalizacionVelocidadDrift = 0.95f;

    [Header("--- EFECTOS VISUALES ---")]
    public ParticleSystem humoDerrape;
    public TrailRenderer marcasLlantas;
    [Tooltip("Velocidad lateral mínima para que las llantas lloren (suelten humo) automáticamente")]
    public float umbralDerrapeVisual = 3f;

    // --- Variables Internas Privadas ---
    private float inputAceleracion;
    private float inputGiro;
    private bool usandoFrenoMano;
    private float anguloRotacionActual;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Configuramos el Rigidbody por código para asegurar las físicas correctas
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
    }

    void FixedUpdate()
    {
        AplicarFuerzaMotor();
        AplicarFisicasDeDireccion();
        AplicarAgarreYDerrape();
    }

    private void CapturarInputs()
    {
        // Controles estrictos SOLO para el Jugador 2 (Flechas) - ¡Sin letras!
        if (Input.GetKey(KeyCode.UpArrow)) inputAceleracion = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) inputAceleracion = -1f;
        else inputAceleracion = 0f;

        if (Input.GetKey(KeyCode.RightArrow)) inputGiro = 1f; // Flecha Derecha = Gira a la derecha (Positivo)
        else if (Input.GetKey(KeyCode.LeftArrow)) inputGiro = -1f; // Flecha Izquierda = Gira a la izquierda (Negativo)
        else inputGiro = 0f;

        // Freno de mano con la tecla Shift Izquierdo (o Derecho, el que prefieras, aquí está el Izquierdo)
        usandoFrenoMano = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    private void AplicarFuerzaMotor()
    {
        // Calcular si el auto va hacia adelante o en reversa en base a donde mira
        float velocidadFrontal = Vector2.Dot(transform.right, rb.linearVelocity);

        // Aceleración hacia adelante (Flecha Arriba)
        if (inputAceleracion > 0)
        {
            if (rb.linearVelocity.magnitude < velocidadMaxima)
            {
                rb.AddForce(transform.right * inputAceleracion * fuerzaAceleracion, ForceMode2D.Force);
            }
        }
        // Frenado o Reversa (Flecha Abajo)
        else if (inputAceleracion < 0)
        {
            rb.AddForce(transform.right * inputAceleracion * fuerzaFrenado, ForceMode2D.Force);
        }
        // Si no apretamos nada, el auto pierde velocidad poco a poco (inercia)
        else
        {
            rb.linearVelocity = rb.linearVelocity * desaceleracionLibre;
        }
    }

    private void AplicarFisicasDeDireccion()
    {
        // El auto solo puede girar si se está moviendo, imitando la realidad
        float factorVelocidad = Mathf.Clamp01(rb.linearVelocity.magnitude / velocidadParaGiroMaximo);

        // Invertir controles si vamos en reversa para no marear al jugador
        float direccionVelocidad = Vector2.Dot(rb.linearVelocity, transform.right);
        float multiplicadorReversa = (direccionVelocidad < 0 && inputAceleracion < 0) ? -1f : 1f;

        // Si tenemos el freno de mano, la dirección se vuelve más nerviosa
        float sensibilidadActual = usandoFrenoMano ? sensibilidadGiroDrift : sensibilidadGiro;

        // Calcular la rotación suave y aplicarla (eliminando los tirones robóticos)
        anguloRotacionActual -= inputGiro * sensibilidadActual * factorVelocidad * multiplicadorReversa;
        rb.MoveRotation(anguloRotacionActual);
    }

    private void AplicarAgarreYDerrape()
    {
        // Separamos la inercia del auto en dos ejes: Hacia adelante y Lateral
        Vector2 velocidadAdelante = transform.right * Vector2.Dot(rb.linearVelocity, transform.right);
        Vector2 velocidadLateral = transform.up * Vector2.Dot(rb.linearVelocity, transform.up);

        // Si tiramos freno de mano, el agarre lateral (tracción) baja drásticamente
        float factorAgarreActual = usandoFrenoMano ? agarreLateralDrift : agarreLateralNormal;

        // Aquí ocurre la magia del derrape: eliminamos solo una parte de la fuerza lateral
        velocidadLateral = velocidadLateral * (1f - factorAgarreActual);

        // Penalizamos ligeramente la velocidad frontal al derrapar para obligar a controlar los frenos
        if (usandoFrenoMano && rb.linearVelocity.magnitude > 5f)
        {
            velocidadAdelante = velocidadAdelante * penalizacionVelocidadDrift;
        }

        // Combinamos las fuerzas y se las devolvemos al auto
        rb.linearVelocity = velocidadAdelante + velocidadLateral;
    }

    private void GestionarEfectosVisuales()
    {
        // Calculamos matemáticamente cuánto se está deslizando el auto de lado
        float derrapeLateral = Vector2.Dot(rb.linearVelocity, transform.up);

        // ¿Estamos resbalando de lado lo suficiente como para quemar llanta?
        bool deslizandoPorFisica = Mathf.Abs(derrapeLateral) > umbralDerrapeVisual;

        // Se encienden los efectos si resbalamos por inercia o si estamos forzando un trompo
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

    // --- SISTEMA DE COLISIONES Y CASTIGO ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si el auto choca contra los muros (pasto/llantas), pierde el 70% de su inercia
        rb.linearVelocity = rb.linearVelocity * 0.3f;

        // Cortamos el derrape visual inmediatamente al chocar
        DetenerEfectosVisuales();
    }
}