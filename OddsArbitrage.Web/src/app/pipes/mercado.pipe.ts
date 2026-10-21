import { Pipe, PipeTransform } from '@angular/core';

import { MERCADOS } from '../datos/mercados';
import { TipoMercado } from '../modelos';

// Nombre legible de un mercado (ej. Ganador pasa a "Ganador (1X2)")
@Pipe({ name: 'mercado' })
export class MercadoPipe implements PipeTransform {
  transform(mercado: TipoMercado): string {
    return MERCADOS.find((m) => m.valor === mercado)?.nombre ?? mercado;
  }
}
