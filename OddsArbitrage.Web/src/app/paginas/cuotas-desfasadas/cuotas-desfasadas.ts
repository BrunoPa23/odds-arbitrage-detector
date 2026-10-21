import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe, DecimalPipe, PercentPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { DEPORTES } from '../../datos/deportes';
import { MERCADOS } from '../../datos/mercados';
import { CuotaDesfasada, TipoMercado } from '../../modelos';
import { MercadoPipe } from '../../pipes/mercado.pipe';
import { ResultadoPipe } from '../../pipes/resultado.pipe';
import { CuotasDesfasadasService } from '../../servicios/cuotas-desfasadas.service';
import { mensajeDeError } from '../../utilidades/mensaje-error';

@Component({
  imports: [FormsModule, DatePipe, DecimalPipe, PercentPipe, MercadoPipe, ResultadoPipe],
  selector: 'app-pagina-cuotas-desfasadas',
  styleUrl: './cuotas-desfasadas.scss',
  templateUrl: './cuotas-desfasadas.html',
})
export class CuotasDesfasadas {
  private readonly cuotasDesfasadasService = inject(CuotasDesfasadasService);

  protected readonly deportes = DEPORTES;
  protected readonly mercados = MERCADOS;

  // Filtros: solo se consulta la api al presionar Buscar, para no gastar cuota en cada cambio
  protected readonly deporte = signal(DEPORTES[0].clave);
  protected readonly mercado = signal<TipoMercado | null>(null);
  protected readonly ventajaMinima = signal<number | null>(null);

  protected readonly cuotas = signal<CuotaDesfasada[]>([]);
  protected readonly cargando = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly haBuscado = signal(false);

  protected clave(cuota: CuotaDesfasada): string {
    return `${cuota.partidoId}|${cuota.mercado}|${cuota.linea ?? ''}|${cuota.resultado}|${cuota.casa.clave}`;
  }

  protected claseVentaja(ventajaPorcentaje: number): string {
    if (ventajaPorcentaje >= 6) {
      return 'badge badge--alto';
    }
    if (ventajaPorcentaje >= 3) {
      return 'badge badge--medio';
    }
    return 'badge badge--bajo';
  }

  protected onBuscar(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.cuotasDesfasadasService
      .obtenerCuotasDesfasadas({
        deporte: this.deporte(),
        mercado: this.mercado(),
        // Un input numerico vacio deja que la api use su umbral configurado
        ventajaMinima: this.ventajaMinima() ?? null,
      })
      .subscribe({
        next: (cuotas) => {
          this.cuotas.set(cuotas);
          this.cargando.set(false);
          this.haBuscado.set(true);
        },
        error: (err: HttpErrorResponse) => {
          this.error.set(
            mensajeDeError(err, 'Ocurrio un error inesperado al consultar las cuotas desfasadas.'),
          );
          this.cargando.set(false);
          this.haBuscado.set(true);
        },
      });
  }
}
