import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { OportunidadArbitraje } from '../../modelos';
import { Surebets } from './surebets';

function crearSurebetTotales(partidoId: string, linea: number): OportunidadArbitraje {
  const cuota = (casa: string, resultado: 'Mas' | 'Menos', valor: number) => ({
    casa: { clave: casa, nombre: casa.toUpperCase() },
    mercado: 'Totales' as const,
    linea,
    resultado,
    valor,
    ultimaActualizacion: '2026-10-20T10:00:00Z',
  });

  return {
    partidoId,
    liga: 'EPL',
    equipoLocal: 'Arsenal',
    equipoVisitante: 'Chelsea',
    fechaInicio: '2026-10-25T15:00:00Z',
    mercado: 'Totales',
    linea,
    mejoresCuotas: [cuota('casad', 'Mas', 2.1), cuota('casab', 'Menos', 1.95)],
    sumaProbabilidadesImplicitas: 0.988,
    margenPorcentaje: 1.11,
    reparto: {
      stakeTotal: 100,
      retornoGarantizado: 101.11,
      gananciaGarantizada: 1.11,
      apuestas: [
        { cuota: cuota('casad', 'Mas', 2.1), stake: 48.15, retorno: 101.12 },
        { cuota: cuota('casab', 'Menos', 1.95), stake: 51.85, retorno: 101.11 },
      ],
    },
  };
}

describe('Surebets', () => {
  let fixture: ComponentFixture<Surebets>;
  let httpMock: HttpTestingController;
  const url = `${environment.apiUrl}/surebets`;

  const elemento = () => fixture.nativeElement as HTMLElement;
  const buscar = () => {
    elemento().querySelector<HTMLButtonElement>('.filtros button')!.click();
    fixture.detectChanges();
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Surebets],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(Surebets);
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

  it('deberia enviar los filtros elegidos y mostrar el estado de carga', () => {
    fixture.componentInstance['mercado'].set('Totales');
    fixture.componentInstance['margenMinimo'].set(1);

    buscar();

    expect(elemento().textContent).toContain('Buscando surebets...');
    expect(elemento().querySelector<HTMLButtonElement>('.filtros button')!.disabled).toBe(true);

    const peticion = httpMock.expectOne((req) => req.url === url);
    expect(peticion.request.params.get('deporte')).toBe('soccer_epl');
    expect(peticion.request.params.get('mercado')).toBe('Totales');
    expect(peticion.request.params.get('margenMinimo')).toBe('1');
    expect(peticion.request.params.get('stake')).toBe('100');
    peticion.flush([]);
  });

  it('deberia renderizar una fila por surebet aunque sean del mismo partido', () => {
    buscar();
    httpMock
      .expectOne((req) => req.url === url)
      .flush([crearSurebetTotales('p1', 2.5), crearSurebetTotales('p1', 3.5)]);
    fixture.detectChanges();

    const filas = elemento().querySelectorAll('.fila-surebet');
    expect(filas.length).toBe(2);
    expect(filas[0].textContent).toContain('Totales (mas o menos)');
    expect(filas[0].textContent).toContain('2.5');
    expect(filas[0].textContent).toContain('1.11%');
  });

  it('deberia mostrar el reparto de stake al expandir una fila', () => {
    buscar();
    httpMock.expectOne((req) => req.url === url).flush([crearSurebetTotales('p1', 2.5)]);
    fixture.detectChanges();

    expect(elemento().querySelector('.fila-detalle')).toBeNull();

    elemento().querySelector<HTMLElement>('.fila-surebet')!.click();
    fixture.detectChanges();

    const detalle = elemento().querySelector('.fila-detalle');
    expect(detalle?.textContent).toContain('Mas de 2.5');
    expect(detalle?.textContent).toContain('CASAB');
    expect(detalle?.textContent).toContain('$48.15');
  });

  it('deberia mostrar el estado vacio cuando no hay surebets', () => {
    buscar();
    httpMock.expectOne((req) => req.url === url).flush([]);
    fixture.detectChanges();

    expect(elemento().querySelector('.estado--vacio')?.textContent).toContain('No hay surebets');
  });

  it('deberia mostrar el title del ProblemDetails cuando la api falla', () => {
    buscar();
    httpMock
      .expectOne((req) => req.url === url)
      .flush({ title: 'The Odds API no respondio a tiempo' }, { status: 504, statusText: 'Gateway Timeout' });
    fixture.detectChanges();

    expect(elemento().querySelector('.estado--error')?.textContent).toContain(
      'The Odds API no respondio a tiempo',
    );
  });
});
