import { CasaDeApuestas } from './casa-de-apuestas';
import { ResultadoPartido } from './resultado-partido';

export interface Cuota {
  casa: CasaDeApuestas;
  resultado: ResultadoPartido;

  // Cuota en formato decimal (ej. 2.10)
  valor: number;

  ultimaActualizacion: string;
}
