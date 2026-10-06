import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import { OportunidadArbitraje } from '../modelos';

@Injectable({ providedIn: 'root' })
export class OportunidadesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/oportunidades`;

  // GET /api/oportunidades?deporte=soccer_epl&stake=100
  obtenerOportunidades(deporte: string, stake: number): Observable<OportunidadArbitraje[]> {
    const params = new HttpParams().set('deporte', deporte).set('stake', stake);
    return this.http.get<OportunidadArbitraje[]>(this.baseUrl, { params });
  }
}
