import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../environments/environment';
import { OportunidadArbitraje } from '../modelos';
import { OportunidadesService } from './oportunidades.service';

describe('OportunidadesService', () => {
  let service: OportunidadesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(OportunidadesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('deberia crearse', () => {
    expect(service).toBeTruthy();
  });

  it('deberia pedir las oportunidades con los parametros deporte y stake', () => {
    const oportunidadesMock: OportunidadArbitraje[] = [];

    service.obtenerOportunidades('soccer_epl', 100).subscribe((respuesta) => {
      expect(respuesta).toEqual(oportunidadesMock);
    });

    const peticion = httpMock.expectOne(
      (req) => req.url === `${environment.apiUrl}/oportunidades`
        && req.params.get('deporte') === 'soccer_epl'
        && req.params.get('stake') === '100',
    );

    expect(peticion.request.method).toBe('GET');
    peticion.flush(oportunidadesMock);
  });

  it('deberia propagar el error cuando la api responde 502', () => {
    service.obtenerOportunidades('soccer_epl', 100).subscribe({
      next: () => {
        throw new Error('no deberia emitir un valor exitoso');
      },
      error: (error) => {
        expect(error.status).toBe(502);
        expect(error.error.title).toBe('No se pudieron obtener las cuotas de The Odds API');
      },
    });

    const peticion = httpMock.expectOne((req) => req.url === `${environment.apiUrl}/oportunidades`);
    peticion.flush(
      { title: 'No se pudieron obtener las cuotas de The Odds API' },
      { status: 502, statusText: 'Bad Gateway' },
    );
  });
});
