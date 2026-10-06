import { Component, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { DEPORTES } from '../../datos/deportes';

export interface FiltrosSeleccionados {
  deporte: string;
  stake: number;
}

@Component({
  imports: [FormsModule],
  selector: 'app-filtros',
  styleUrl: './filtros.scss',
  templateUrl: './filtros.html',
})
export class Filtros {
  protected readonly deportes = DEPORTES;

  protected readonly deporte = signal(DEPORTES[0].clave);
  protected readonly stake = signal(100);

  readonly buscar = output<FiltrosSeleccionados>();

  protected onBuscar(): void {
    this.buscar.emit({ deporte: this.deporte(), stake: this.stake() });
  }
}
