# Informe de Desarrollo: Carrera Drift 2D

Este documento detalla el desarrollo del proyecto de un juego de carreras multijugador local con mecánicas de derrape, creado en Unity 6. A continuación se justifican las decisiones técnicas, de diseño y la implementación de los distintos sistemas del juego.

## Selección de la Plataforma y Despliegue
Elegimos la plataforma de PC para este desarrollo debido a la naturaleza multijugador local del proyecto, donde un jugador utiliza las teclas WASD y el otro las flechas direccionales. Un teclado físico garantiza la lectura simultánea de comandos sin el retraso o las limitaciones de una pantalla táctil, algo crucial para ejecutar maniobras de derrape precisas. El proyecto cuenta con una versión compilada estable y funcional, generada sin errores de dependencias ni problemas de compatibilidad.

## Motor de Renderización y Recursos Visuales
Implementamos Universal Render Pipeline enfocado en dos dimensiones para aprovechar un renderizado moderno y optimizado. Los materiales y los sprites de los vehículos mantienen un estilo limpio y coherente con el entorno de la pista. 

Para la interfaz gráfica, el lienzo principal está configurado en modo de superposición de pantalla. Esto asegura que los menús de inicio, las pantallas de victoria y el contador de vueltas se escalen correctamente en cualquier resolución, manteniendo la información legible sin interferir visualmente con la acción de la pista.

## Lógica del Juego y Herramientas del Motor
El proyecto implementa un ciclo de juego completo que abarca el menú de inicio, el desarrollo de la carrera y la pantalla de fin de juego. Toda esta lógica fue estructurada en base a scripts modulares y limpios. 

El controlador principal del juego gestiona los estados de la partida y la pausa del tiempo sin romper el flujo de la interfaz. Por otro lado, el contador de vueltas evalúa las condiciones de victoria de manera independiente al detectar los cruces por la meta. Se utilizaron elementos prefabricados para los vehículos, lo que facilitó su instanciación y el ajuste de sus variables desde el editor. Toda la jerarquía de la escena se mantuvo organizada para separar la interfaz de los elementos del mundo.

## Implementación de Físicas
Utilizamos un enfoque basado en físicas bidimensionales mediante cuerpos rígidos y colisionadores. Al ser un juego con vista superior, la gravedad se fijó en cero, delegando el control del roce natural a la amortiguación lineal y angular del motor de Unity.

El script de control de los autos simula la pérdida de agarre calculando constantemente los vectores de velocidad frontal y lateral, lo que permite realizar derrapes controlados al presionar el freno de mano. Respecto a la interacción con el entorno, la línea de meta funciona mediante un activador invisible. El sistema detecta limpiamente qué vehículo cruza leyendo su respectiva etiqueta de jugador, sumando la vuelta sin generar un bloqueo físico en la pista.

## Integración de Sonido
El proyecto incluye un sistema de audio funcional y equilibrado. Configuramos una fuente de audio general para reproducir la pista musical principal en bucle continuo desde el inicio del nivel.

Además, integramos eventos de audio dinámicos controlados por código. Destaca el efecto de sonido de aceleración de los motores, el cual se activa exclusivamente cuando el jugador presiona las teclas de movimiento y se silencia al soltarlas, brindando una respuesta auditiva realista. Los volúmenes fueron ajustados directamente en el editor para asegurar que la música de fondo no sature y permita que los efectos mecánicos de los vehículos destaquen durante la carrera.