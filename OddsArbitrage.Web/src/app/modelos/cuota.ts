import { CasaDeApuestas } from './casa-de-apuestas';
import { ResultadoPartido } from './resultado-partido';
import { TipoMercado } from './tipo-mercado';

export interface Cuota {
  casa: CasaDeApuestas;
  mercado: TipoMercado;

  // Punto del mercado (ej. 2.5 en totales); null en el mercado ganador
  linea: number | null;

  resultado: ResultadoPartido;

  // Cuota en formato decimal (ej. 2.10)
  valor: number;

  ultimaActualizacion: string;
}
