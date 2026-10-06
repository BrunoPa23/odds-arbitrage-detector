import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OportunidadArbitraje } from '../../modelos';
import { TablaOportunidades } from './tabla-oportunidades';

function crearOportunidadMock(partidoId: string): OportunidadArbitraje {
  return {
    partidoId,
    liga: 'Premier League',
    equipoLocal: 'Equipo A',
    equipoVisitante: 'Equipo B',
    fechaInicio: '2026-10-10T20:00:00Z',
    mejoresCuotas: [],
    sumaProbabilidadesImplicitas: 0.95,
    margenPorcentaje: 5,
    reparto: {
      stakeTotal: 100,
      retornoGarantizado: 105,
      gananciaGarantizada: 5,
      apuestas: [
        {
          cuota: {
            casa: { clave: 'bet365', nombre: 'Bet365' },
            resultado: 'Local',
            valor: 2.1,
            ultimaActualizacion: '2026-10-10T19:00:00Z',
          },
          stake: 50,
          retorno: 105,
        },
      ],
    },
  };
}

describe('TablaOportunidades', () => {
  let fixture: ComponentFixture<TablaOportunidades>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TablaOportunidades],
    }).compileComponents();

    fixture = TestBed.createComponent(TablaOportunidades);
  });

  it('deberia mostrar el mensaje vacio cuando no hay oportunidades', () => {
    fixture.componentRef.setInput('oportunidades', []);
    fixture.detectChanges();

    const texto = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(texto).toContain('No hay oportunidades');
  });

  it('deberia renderizar una fila por cada oportunidad recibida', () => {
    const mock = [crearOportunidadMock('p1'), crearOportunidadMock('p2')];
    fixture.componentRef.setInput('oportunidades', mock);
    fixture.detectChanges();

    const filas = (fixture.nativeElement as HTMLElement).querySelectorAll('.fila-oportunidad');
    expect(filas.length).toBe(2);
  });

  it('deberia mostrar el reparto de stake al expandir una fila', () => {
    const mock = [crearOportunidadMock('p1')];
    fixture.componentRef.setInput('oportunidades', mock);
    fixture.detectChanges();

    let detalle = (fixture.nativeElement as HTMLElement).querySelector('.fila-detalle');
    expect(detalle).toBeNull();

    const fila = (fixture.nativeElement as HTMLElement).querySelector<HTMLElement>('.fila-oportunidad');
    fila?.click();
    fixture.detectChanges();

    detalle = (fixture.nativeElement as HTMLElement).querySelector('.fila-detalle');
    expect(detalle).not.toBeNull();
    expect(detalle?.textContent).toContain('Bet365');
  });
});
