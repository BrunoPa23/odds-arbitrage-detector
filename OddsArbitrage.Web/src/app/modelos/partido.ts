import { Cuota } from './cuota';

export interface Partido {
  id: string;
  liga: string;
  equipoLocal: string;
  equipoVisitante: string;
  fechaInicio: string;
  cuotas: Cuota[];
}
