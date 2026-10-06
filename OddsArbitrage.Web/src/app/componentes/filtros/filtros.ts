import { Component, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

export interface DeporteOpcion {
  clave: string;
  nombre: string;
}

export interface FiltrosSeleccionados {
  deporte: string;
  stake: number;
}

// Claves de deporte soportadas por The Odds API (subconjunto usado en el MVP)
const DEPORTES: DeporteOpcion[] = [
  { clave: 'soccer_epl', nombre: 'Futbol: Premier League' },
  { clave: 'soccer_spain_la_liga', nombre: 'Futbol: La Liga' },
  { clave: 'soccer_italy_serie_a', nombre: 'Futbol: Serie A' },
  { clave: 'soccer_germany_bundesliga', nombre: 'Futbol: Bundesliga' },
  { clave: 'basketball_nba', nombre: 'Basquet: NBA' },
];

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
