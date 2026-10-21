import { HttpErrorResponse } from '@angular/common/http';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { DEPORTES } from '../../datos/deportes';
import { MERCADOS } from '../../datos/mercados';
import { OportunidadArbitraje, TipoMercado } from '../../modelos';
import { LineaPipe } from '../../pipes/linea.pipe';
import { MercadoPipe } from '../../pipes/mercado.pipe';
import { ResultadoPipe } from '../../pipes/resultado.pipe';
import { SurebetsService } from '../../servicios/surebets.service';
import { mensajeDeError } from '../../utilidades/mensaje-error';

@Component({
  imports: [FormsModule, CurrencyPipe, DatePipe, DecimalPipe, LineaPipe, MercadoPipe, ResultadoPipe],
  selector: 'app-pagina-surebets',
  styleUrl: './surebets.scss',
  templateUrl: './surebets.html',
})
export class Surebets {
  private readonly surebetsService = inject(SurebetsService);

  protected readonly deportes = DEPORTES;
  protected readonly mercados = MERCADOS;

  // Filtros: solo se consulta la api al presionar Buscar, para no gastar cuota en cada cambio
  protected readonly deporte = signal(DEPORTES[0].clave);
  protected readonly mercado = signal<TipoMercado | null>(null);
  protected readonly margenMinimo = signal<number | null>(null);
  protected readonly stake = signal(100);

  protected readonly surebets = signal<OportunidadArbitraje[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly haBuscado = signal(false);
  protected readonly claveExpandida = signal<string | null>(null);

  // Un partido puede tener varias surebets (una por mercado y linea)
  protected clave(surebet: OportunidadArbitraje): string {
    return `${surebet.partidoId}|${surebet.mercado}|${surebet.linea ?? ''}`;
  }

  protected estaExpandida(surebet: OportunidadArbitraje): boolean {
    return this.claveExpandida() === this.clave(surebet);
  }

  protected alternarExpansion(surebet: OportunidadArbitraje): void {
    this.claveExpandida.set(this.estaExpandida(surebet) ? null : this.clave(surebet));
  }

  protected claseMargen(margenPorcentaje: number): string {
    if (margenPorcentaje >= 3) {
      return 'badge badge--alto';
    }
    if (margenPorcentaje >= 1) {
      return 'badge badge--medio';
    }
    return 'badge badge--bajo';
  }

  protected onBuscar(): void {
    this.cargando.set(true);
    this.error.set(null);
    this.claveExpandida.set(null);

    this.surebetsService
      .obtenerSurebets({
        deporte: this.deporte(),
        mercado: this.mercado(),
        // Un input numerico vacio deja el filtro sin minimo
        margenMinimo: this.margenMinimo() ?? null,
        stake: this.stake(),
      })
      .subscribe({
        next: (surebets) => {
          this.surebets.set(surebets);
          this.cargando.set(false);
          this.haBuscado.set(true);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(mensajeDeError(err, 'Ocurrio un error inesperado al consultar las surebets.'));
          this.cargando.set(false);
          this.haBuscado.set(true);
        },
      });
  }
}
