import { CasaDeApuestas } from './casa-de-apuestas';
import { ResultadoCuota } from './resultado-cuota';
import { TipoMercado } from './tipo-mercado';

export interface Cuota {
  casa: CasaDeApuestas;
  mercado: TipoMercado;

  // Punto del mercado (ej. 2.5 en totales); null en el mercado ganador
  linea: number | null;

  resultado: ResultadoCuota;

  // Cuota en formato decimal (ej. 2.10)
  valor: number;

  ultimaActualizacion: string;
}
