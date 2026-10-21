import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { OportunidadArbitraje, TipoMercado } from '../modelos';

export interface FiltrosSurebets {
  deporte: string;

  // null pide todos los mercados consultados por la api
  mercado: TipoMercado | null;

  // Margen minimo en porcentaje; null no filtra
  margenMinimo: number | null;

  stake: number;
}

@Injectable({ providedIn: 'root' })
export class SurebetsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/surebets`;

  // GET /api/surebets?deporte=soccer_epl&mercado=Totales&margenMinimo=1&stake=100
  obtenerSurebets(filtros: FiltrosSurebets): Observable<OportunidadArbitraje[]> {
    let params = new HttpParams().set('deporte', filtros.deporte).set('stake', filtros.stake);
    if (filtros.mercado) {
      params = params.set('mercado', filtros.mercado);
    }
    if (filtros.margenMinimo !== null) {
      params = params.set('margenMinimo', filtros.margenMinimo);
    }
    return this.http.get<OportunidadArbitraje[]>(this.baseUrl, { params });
  }
}
