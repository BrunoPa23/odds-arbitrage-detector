import { CasaDeApuestas } from './casa-de-apuestas';
import { ResultadoCuota } from './resultado-cuota';
import { TipoMercado } from './tipo-mercado';

// Cuota claramente por encima del consenso sin margen de las casas (value bet).
// No es ganancia garantizada: es una ventaja esperada si el consenso es correcto
export interface CuotaDesfasada {
  partidoId: string;
  liga: string;
  equipoLocal: string;
  equipoVisitante: string;
  fechaInicio: string;
  mercado: TipoMercado;

  // Linea de la propia cuota tal como se apuesta; null en el mercado ganador
  linea: number | null;

  resultado: ResultadoCuota;
  casa: CasaDeApuestas;

  // Cuota decimal que ofrece la casa
  valor: number;

  ultimaActualizacion: string;

  // Probabilidad del resultado segun el consenso sin margen (entre 0 y 1)
  probabilidadJusta: number;

  // Cuota que corresponde a la probabilidad justa: 1 / probabilidad
  cuotaJusta: number;

  // Ganancia esperada por unidad apostada en porcentaje (ej. 4.5 equivale a 4.5 %)
  ventajaPorcentaje: number;

  // Casas que forman el consenso
  casasConsenso: number;
}
