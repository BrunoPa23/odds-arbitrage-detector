import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { Partido } from '../modelos';

@Injectable({ providedIn: 'root' })
export class PartidosService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/partidos`;

  // GET /api/partidos?deporte=soccer_epl
  obtenerPartidos(deporte: string): Observable<Partido[]> {
    const params = new HttpParams().set('deporte', deporte);
    return this.http.get<Partido[]>(this.baseUrl, { params });
  }
}
