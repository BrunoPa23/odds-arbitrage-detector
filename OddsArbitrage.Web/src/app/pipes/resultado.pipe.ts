import { Pipe, PipeTransform } from '@angular/core';

import { ResultadoCuota, TipoMercado } from '../modelos';
import { formatearLinea } from './linea.pipe';

// Resultado con su linea, tal como se apuesta (ej. "Mas de 2.5" o "Visitante +1.5")
@Pipe({ name: 'resultado' })
export class ResultadoPipe implements PipeTransform {
  transform(resultado: ResultadoCuota, mercado: TipoMercado, linea: number | null): string {
    if (linea === null) {
      return resultado;
    }
    if (mercado === 'Totales') {
      return `${resultado} de ${formatearLinea(linea, mercado)}`;
    }
    return `${resultado} ${formatearLinea(linea, mercado)}`;
  }
}
