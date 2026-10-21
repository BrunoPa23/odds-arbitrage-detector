# Changelog

Todos los cambios relevantes de este proyecto se documentan en este archivo.

El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/) y el proyecto usa [Versionado Semántico](https://semver.org/lang/es/).

## [Sin publicar]

## [0.2.0] - 2026-10-21

Fase S: surebets y cuotas desfasadas más allá del 1X2.

### Agregado

- Mercados totales (`totals`) y hándicap (`spreads`) además del ganador (`h2h`), con línea y resultado por mercado (#7).
- Opción `TheOddsApi:Mercados` para elegir qué mercados se consultan; por defecto solo `h2h`, porque cada mercado extra multiplica el costo de cada consulta (#7).
- Detector de surebets en mercados de dos resultados que agrupa por línea y empareja el hándicap del local con la línea opuesta del visitante (#8).
- Interfaz común `IDetectorSurebets` y buscador que une los detectores de todos los mercados (#8).
- Endpoint `GET /api/surebets` con filtros por deporte, mercado, margen mínimo y stake (#8).
- Probabilidad justa sin margen por el método proporcional y consenso promediado entre casas, con un mínimo de casas configurable (#9).
- Detector de cuotas desfasadas (ventaja esperada `cuota × p_justa - 1`) y endpoint `GET /api/value-bets` con umbral configurable en `ValueBets:VentajaMinimaPorcentaje` (#9).
- Páginas de surebets y de cuotas desfasadas en Angular, con filtros, detalle expandible del reparto de stake y navegación entre páginas (#10).
- Modo demo (`TheOddsApi:ModoDemo`, `MODO_DEMO` en Docker Compose y perfil `demo` de `dotnet run`) que sirve un fixture versionado de partidos ficticios sin API key y sin llamar a The Odds API.
- Tests de integración con `WebApplicationFactory` para surebets, cuotas desfasadas y los endpoints del MVP, sobre el modo demo.
- Explicación matemática en el README (probabilidad implícita, condición de surebet, reparto de stake, probabilidad justa y ventaja esperada) y capturas del dashboard.

### Cambiado

- `TheOddsApi:ApiKey` solo es obligatoria fuera del modo demo.
- El job de Docker del CI levanta el stack en modo demo y verifica que `/api/surebets` responde con datos.

## [0.1.0] - 2026-10-10

MVP: detección de arbitraje en el mercado 1X2.

### Agregado

- Solución ASP.NET Core (.NET 10) con modelo de dominio `Partido`, `Cuota` y `CasaDeApuestas` (#1).
- Cliente de The Odds API con configuración por opciones, cache en memoria para cuidar la cuota de 500 requests al mes y endpoint `GET /api/partidos` (#1, #2).
- Algoritmo de arbitraje en el 1X2: probabilidad implícita, mejores cuotas entre casas y reparto óptimo de stake, con endpoint `GET /api/oportunidades` (#3).
- Tests unitarios del algoritmo con xUnit y CI en GitHub Actions (#4).
- Frontend en Angular con filtros por deporte, tabla de oportunidades con detalle del reparto y listado de partidos (#5).
- Docker Compose con la API y la web servida por nginx, y job de CI que construye y prueba las imágenes (#6).
