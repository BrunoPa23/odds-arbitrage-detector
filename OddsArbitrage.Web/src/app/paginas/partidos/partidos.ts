import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';

import { DEPORTES } from '../../datos/deportes';
import { Partido } from '../../modelos';
import { PartidosService } from '../../servicios/partidos.service';

@Component({
  imports: [FormsModule, DatePipe, DecimalPipe],
  selector: 'app-pagina-partidos',
  styleUrl: './partidos.scss',
  templateUrl: './partidos.html',
})
export class Partidos {
  private readonly partidosService = inject(PartidosService);

  protected readonly deportes = DEPORTES;
  protected readonly deporte = signal(DEPORTES[0].clave);

  protected readonly partidos = signal<Partido[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly haBuscado = signal(false);

  protected onBuscar(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.partidosService.obtenerPartidos(this.deporte()).subscribe({
      next: (partidos) => {
        this.partidos.set(partidos);
        this.cargando.set(false);
        this.haBuscado.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set((err.error?.title as string | undefined) ?? 'Ocurrio un error inesperado al consultar los partidos.');
        this.cargando.set(false);
        this.haBuscado.set(true);
      },
    });
  }
}
