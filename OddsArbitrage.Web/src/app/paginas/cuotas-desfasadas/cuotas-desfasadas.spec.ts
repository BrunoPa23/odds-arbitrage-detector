import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { CuotaDesfasada } from '../../modelos';
import { CuotasDesfasadas } from './cuotas-desfasadas';

function crearCuotaDesfasada(casa: string, ventajaPorcentaje: number): CuotaDesfasada {
  return {
    partidoId: 'p1',
    liga: 'EPL',
    equipoLocal: 'Arsenal',
    equipoVisitante: 'Chelsea',
    fechaInicio: '2026-10-25T15:00:00Z',
    mercado: 'Handicap',
    linea: 1.5,
    resultado: 'Visitante',
    casa: { clave: casa, nombre: casa.toUpperCase() },
    valor: 1.72,
    ultimaActualizacion: '2026-10-20T10:00:00Z',
    probabilidadJusta: 0.608949,
    cuotaJusta: 1.642,
    ventajaPorcentaje,
    casasConsenso: 4,
  };
}

describe('CuotasDesfasadas', () => {
  let fixture: ComponentFixture<CuotasDesfasadas>;
  let httpMock: HttpTestingController;
  const url = `${environment.apiUrl}/value-bets`;

  const elemento = () => fixture.nativeElement as HTMLElement;
  const buscar = () => {
    elemento().querySelector<HTMLButtonElement>('.filtros button')!.click();
    fixture.detectChanges();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CuotasDesfasadas],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(CuotasDesfasadas);
    httpMock = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('no deberia consultar la api hasta presionar Buscar', () => {
    httpMock.expectNone((req) => req.url === url);
    expect(elemento().textContent).toContain('presiona "Buscar"');
  });

  it('deberia enviar solo el deporte cuando no se eligen mercado ni ventaja', () => {
    buscar();

    expect(elemento().textContent).toContain('Buscando cuotas desfasadas...');

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.params.keys()).toEqual(['deporte']);
    peticion.flush([]);
  });

  it('deberia enviar mercado y ventaja minima cuando se eligen', () => {
    fixture.componentInstance['mercado'].set('Handicap');
    fixture.componentInstance['ventajaMinima'].set(4);

    buscar();

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.params.get('mercado')).toBe('Handicap');
    expect(peticion.request.params.get('ventajaMinima')).toBe('4');
    peticion.flush([]);
  });

  it('deberia mostrar casa, resultado, cuota, cuota justa y ventaja de cada cuota', () => {
    buscar();
    httpMock
      .expectOne((req) => req.url === url)
      .flush([crearCuotaDesfasada('casad', 4.74), crearCuotaDesfasada('casac', 3.1)]);
    fixture.detectChanges();

    const filas = elemento().querySelectorAll('.fila-cuota');
    expect(filas.length).toBe(2);

    const texto = filas[0].textContent ?? '';
    expect(texto).toContain('Arsenal vs Chelsea');
    expect(texto).toContain('CASAD');
    expect(texto).toContain('Visitante +1.5');
    expect(texto).toContain('1.72');
    expect(texto).toContain('1.642');
    expect(texto).toContain('+4.74%');
    expect(texto).toContain('4 casas');
  });

  it('deberia mostrar el estado vacio cuando no hay cuotas desfasadas', () => {
    buscar();
    httpMock.expectOne((req) => req.url === url).flush([]);
    fixture.detectChanges();

    expect(elemento().querySelector('.estado--vacio')?.textContent).toContain(
      'No hay cuotas desfasadas',
    );
  });

  it('deberia mostrar el title del ProblemDetails cuando la api rechaza los filtros', () => {
    buscar();
    httpMock
      .expectOne((req) => req.url === url)
      .flush({ title: 'One or more validation errors occurred.' }, { status: 400, statusText: 'Bad Request' });
    fixture.detectChanges();

    expect(elemento().querySelector('.estado--error')?.textContent).toContain(
      'One or more validation errors occurred.',
    );
  });

  it('deberia usar un mensaje generico si el error no trae title', () => {
    buscar();
    httpMock
      .expectOne((req) => req.url === url)
      .flush(null, { status: 500, statusText: 'Internal Server Error' });
    fixture.detectChanges();

    expect(elemento().querySelector('.estado--error')?.textContent).toContain(
      'Ocurrio un error inesperado',
    );
  });
});
