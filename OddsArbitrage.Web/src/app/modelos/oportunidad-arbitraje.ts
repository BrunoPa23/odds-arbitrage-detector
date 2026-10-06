import { Cuota } from './cuota';
import { RepartoStake } from './reparto-stake';

export interface OportunidadArbitraje {
  partidoId: string;
  liga: string;
  equipoLocal: string;
  equipoVisitante: string;
  fechaInicio: string;

  // Mejor cuota disponible para cada resultado del mercado 1X2 (una por resultado)
  mejoresCuotas: Cuota[];

  // Suma de probabilidades implicitas de las mejores cuotas; hay arbitraje cuando es menor a 1
  sumaProbabilidadesImplicitas: number;

  // Ganancia garantizada expresada como porcentaje del stake total (ej. 2.5 equivale a 2.5 %)
  margenPorcentaje: number;

  // Como repartir el stake entre las mejores cuotas para asegurar la ganancia
  reparto: RepartoStake;
}
