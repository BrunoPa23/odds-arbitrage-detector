import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { CuotaDesfasada, TipoMercado } from '../modelos';

export interface FiltrosCuotasDesfasadas {
  deporte: string;

  // null pide todos los mercados consultados por la api
  mercado: TipoMercado | null;

  // Ventaja minima en porcentaje; null usa el umbral configurado en la api
  ventajaMinima: number | null;
}

@Injectable({ providedIn: 'root' })
export class CuotasDesfasadasService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/value-bets`;

  // GET /api/value-bets?deporte=soccer_epl&mercado=Ganador&ventajaMinima=3
  obtenerCuotasDesfasadas(filtros: FiltrosCuotasDesfasadas): Observable<CuotaDesfasada[]> {
    let params = new HttpParams().set('deporte', filtros.deporte);
    if (filtros.mercado) {
      params = params.set('mercado', filtros.mercado);
    }
    if (filtros.ventajaMinima !== null) {
      params = params.set('ventajaMinima', filtros.ventajaMinima);
    }
    return this.http.get<CuotaDesfasada[]>(this.baseUrl, { params });
  }
}
