# Odds Arbitrage Detector

Detecta oportunidades de arbitraje entre casas de apuestas, comparando cuotas del mismo partido en distintas casas en el mercado 1X2 (local, empate, visitante). Un arbitraje existe cuando la suma de las probabilidades implicitas de las mejores cuotas es menor al 100%; en ese caso, repartiendo el stake de forma proporcional se asegura una ganancia sin importar el resultado.

Proyecto personal de portafolio para practicar C#, ASP.NET Core, Entity Framework y Angular.

## Arquitectura

```
odds-arbitrage-detector/
├── OddsArbitrage.Api/   API REST en ASP.NET Core (.NET 10)
└── OddsArbitrage.Web/   Frontend en Angular (standalone components)
```

- **OddsArbitrage.Api**: consume cuotas reales de The Odds API, calcula probabilidad implicita por cuota, detecta arbitrajes en el mercado 1X2 y expone los endpoints:
  - `GET /api/partidos?deporte={clave}`: partidos y sus cuotas por casa de apuestas.
  - `GET /api/oportunidades?deporte={clave}&stake={monto}`: oportunidades de arbitraje detectadas, con el reparto de stake recomendado.
- **OddsArbitrage.Web**: consume esos endpoints y muestra filtros por deporte, la tabla de oportunidades (con detalle expandible del reparto) y el listado de partidos con sus cuotas.

## Como correr la API

Requisitos: .NET 10 SDK.

```bash
cd OddsArbitrage.Api
dotnet run
```

Por defecto queda disponible en `https://localhost:7279` (y `http://localhost:5162`); puede variar segun `Properties/launchSettings.json`.

### Configuracion de la API

La clave de The Odds API nunca se guarda en el repositorio. Se configura con user secrets (desarrollo) o una variable de entorno (otros ambientes):

```bash
cd OddsArbitrage.Api
dotnet user-secrets set "TheOddsApi:ApiKey" "tu-api-key"
```

O, como variable de entorno:

```bash
# PowerShell
$env:TheOddsApi__ApiKey = "tu-api-key"
```

El plan gratuito de The Odds API tiene un limite de 500 requests al mes; el servicio usa cache en memoria (`TheOddsApi:MinutosCache`) para no gastar cuota en cada consulta.

Los origenes permitidos por CORS para el frontend se configuran en `appsettings.json`, bajo `Cors:OrigenesPermitidos` (por defecto incluye `http://localhost:4200`).

## Como correr el frontend

Requisitos: Node.js LTS.

```bash
cd OddsArbitrage.Web
npm install
npm start
```

Queda disponible en `http://localhost:4200`. El proxy configurado en `proxy.conf.json` redirige las llamadas a `/api` hacia la API en `https://localhost:7279`, asi que conviene tener la API corriendo antes de abrir el frontend.

Para compilar en modo produccion:

```bash
npm run build
```

Para correr los tests unitarios:

```bash
npm test
```

## Estado del proyecto

Desarrollo por sprints cortos (metodologia agil, backlog MoSCoW). MVP funcional: backend con datos reales de The Odds API, algoritmo de deteccion de arbitraje y frontend en Angular que muestra las oportunidades y los partidos.

Pendiente (post MVP): alertas en tiempo real (SignalR), historico en base de datos, jobs automaticos en background, tests de integracion, Docker y CI/CD.
