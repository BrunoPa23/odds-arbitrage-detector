import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { CuotasDesfasadasService } from './cuotas-desfasadas.service';

describe('CuotasDesfasadasService', () => {
  let service: CuotasDesfasadasService;
  let httpMock: HttpTestingController;
  const url = `${environment.apiUrl}/value-bets`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(CuotasDesfasadasService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('deberia pedir /value-bets con deporte, mercado y ventaja minima', () => {
    service
      .obtenerCuotasDesfasadas({ deporte: 'soccer_epl', mercado: 'Handicap', ventajaMinima: 5 })
      .subscribe((respuesta) => expect(respuesta).toEqual([]));

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.method).toBe('GET');
    expect(peticion.request.params.get('deporte')).toBe('soccer_epl');
    expect(peticion.request.params.get('mercado')).toBe('Handicap');
    expect(peticion.request.params.get('ventajaMinima')).toBe('5');
    peticion.flush([]);
  });

  it('deberia omitir mercado y ventaja minima para usar los valores de la api', () => {
    service
      .obtenerCuotasDesfasadas({ deporte: 'soccer_epl', mercado: null, ventajaMinima: null })
      .subscribe();

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.params.keys()).toEqual(['deporte']);
    peticion.flush([]);
  });

  it('deberia propagar el error de validacion de la api', () => {
    service
      .obtenerCuotasDesfasadas({ deporte: 'soccer_epl', mercado: null, ventajaMinima: 500 })
      .subscribe({
        next: () => {
          throw new Error('no deberia emitir un valor exitoso');
        },
        error: (error) => expect(error.status).toBe(400),
      });

    httpMock
      .expectOne((req) => req.url === url)
      .flush(
        { title: 'One or more validation errors occurred.' },
        { status: 400, statusText: 'Bad Request' },
      );
  });
});
