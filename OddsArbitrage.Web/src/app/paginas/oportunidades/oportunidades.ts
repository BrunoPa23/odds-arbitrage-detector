import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';

import { Filtros, FiltrosSeleccionados } from '../../componentes/filtros/filtros';
import { TablaOportunidades } from '../../componentes/tabla-oportunidades/tabla-oportunidades';
import { OportunidadArbitraje } from '../../modelos';
import { OportunidadesService } from '../../servicios/oportunidades.service';

@Component({
  imports: [Filtros, TablaOportunidades],
  selector: 'app-pagina-oportunidades',
  styleUrl: './oportunidades.scss',
  templateUrl: './oportunidades.html',
})
export class Oportunidades {
  private readonly oportunidadesService = inject(OportunidadesService);

  protected readonly oportunidades = signal<OportunidadArbitraje[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly haBuscado = signal(false);

  protected onBuscar(filtros: FiltrosSeleccionados): void {
    this.cargando.set(true);
    this.error.set(null);

    this.oportunidadesService.obtenerOportunidades(filtros.deporte, filtros.stake).subscribe({
      next: (oportunidades) => {
        this.oportunidades.set(oportunidades);
        this.cargando.set(false);
        this.haBuscado.set(true);
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(this.mensajeDeError(err));
        this.cargando.set(false);
        this.haBuscado.set(true);
      },
    });
  }

  private mensajeDeError(err: HttpErrorResponse): string {
    const titulo = err.error?.title as string | undefined;
    if (titulo) {
      return titulo;
    }
    return 'Ocurrio un error inesperado al consultar las oportunidades.';
  }
}
