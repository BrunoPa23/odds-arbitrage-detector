import { Pipe, PipeTransform } from '@angular/core';

import { TipoMercado } from '../modelos';

// En handicap la linea lleva signo (+1.5 o -1.5); en totales es el numero tal cual
export function formatearLinea(linea: number | null, mercado: TipoMercado): string {
  if (linea === null) {
    return '';
  }
  return mercado === 'Handicap' && linea > 0 ? `+${linea}` : `${linea}`;
}

// Linea de un mercado para mostrar en una tabla; el mercado ganador no tiene linea
@Pipe({ name: 'linea' })
export class LineaPipe implements PipeTransform {
  transform(linea: number | null, mercado: TipoMercado): string {
    return linea === null ? '-' : formatearLinea(linea, mercado);
  }
}
