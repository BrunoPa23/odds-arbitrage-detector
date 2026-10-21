import { TipoMercado } from '../modelos';

export interface MercadoOpcion {
  valor: TipoMercado;
  nombre: string;
}

// Mercados que entiende la api; totales y handicap solo traen datos si la api los consulta
export const MERCADOS: MercadoOpcion[] = [
  { valor: 'Ganador', nombre: 'Ganador (1X2)' },
  { valor: 'Totales', nombre: 'Totales (mas o menos)' },
  { valor: 'Handicap', nombre: 'Handicap' },
];
