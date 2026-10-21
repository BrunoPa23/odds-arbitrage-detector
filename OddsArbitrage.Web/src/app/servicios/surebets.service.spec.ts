import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { SurebetsService } from './surebets.service';

describe('SurebetsService', () => {
  let service: SurebetsService;
  let httpMock: HttpTestingController;
  const url = `${environment.apiUrl}/surebets`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(SurebetsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('deberia enviar deporte, mercado, margen minimo y stake', () => {
    service
      .obtenerSurebets({ deporte: 'soccer_epl', mercado: 'Totales', margenMinimo: 1.5, stake: 250 })
      .subscribe((respuesta) => expect(respuesta).toEqual([]));

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.method).toBe('GET');
    expect(peticion.request.params.get('deporte')).toBe('soccer_epl');
    expect(peticion.request.params.get('mercado')).toBe('Totales');
    expect(peticion.request.params.get('margenMinimo')).toBe('1.5');
    expect(peticion.request.params.get('stake')).toBe('250');
    peticion.flush([]);
  });

  it('no deberia enviar mercado ni margen minimo cuando son null', () => {
    service
      .obtenerSurebets({ deporte: 'basketball_nba', mercado: null, margenMinimo: null, stake: 100 })
      .subscribe();

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.params.has('mercado')).toBe(false);
    expect(peticion.request.params.has('margenMinimo')).toBe(false);
    expect(peticion.request.params.get('deporte')).toBe('basketball_nba');
    peticion.flush([]);
  });

  it('deberia enviar margen minimo 0 porque es un filtro valido', () => {
    service
      .obtenerSurebets({ deporte: 'soccer_epl', mercado: null, margenMinimo: 0, stake: 100 })
      .subscribe();

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.params.get('margenMinimo')).toBe('0');
    peticion.flush([]);
  });

  it('deberia propagar el ProblemDetails cuando la api responde 502', () => {
    service
      .obtenerSurebets({ deporte: 'soccer_epl', mercado: null, margenMinimo: null, stake: 100 })
      .subscribe({
        next: () => {
          throw new Error('no deberia emitir un valor exitoso');
        },
        error: (error) => {
          expect(error.status).toBe(502);
          expect(error.error.title).toBe('No se pudieron obtener las cuotas de The Odds API');
        },
      });

    httpMock
      .expectOne((req) => req.url === url)
      .flush(
        { title: 'No se pudieron obtener las cuotas de The Odds API' },
        { status: 502, statusText: 'Bad Gateway' },
      );
  });
});
