import { Component, input, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe, PercentPipe } from '@angular/common';

import { OportunidadArbitraje } from '../../modelos';

@Component({
  imports: [CurrencyPipe, DatePipe, DecimalPipe, PercentPipe],
  selector: 'app-tabla-oportunidades',
  styleUrl: './tabla-oportunidades.scss',
  templateUrl: './tabla-oportunidades.html',
})
export class TablaOportunidades {
  readonly oportunidades = input<OportunidadArbitraje[]>([]);

  protected readonly partidoIdExpandido = signal<string | null>(null);

  protected estaExpandida(partidoId: string): boolean {
    return this.partidoIdExpandido() === partidoId;
  }

  protected alternarExpansion(partidoId: string): void {
    this.partidoIdExpandido.set(this.estaExpandida(partidoId) ? null : partidoId);
  }

  // Clase del badge segun el margen de ganancia de la oportunidad
  protected claseMargen(margenPorcentaje: number): string {
    if (margenPorcentaje >= 3) {
      return 'badge badge--alto';
    }
    if (margenPorcentaje >= 1) {
      return 'badge badge--medio';
    }
    return 'badge badge--bajo';
  }
}
