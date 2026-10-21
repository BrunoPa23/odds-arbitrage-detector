import { LineaPipe } from './linea.pipe';
import { MercadoPipe } from './mercado.pipe';
import { ResultadoPipe } from './resultado.pipe';

describe('MercadoPipe', () => {
  const pipe = new MercadoPipe();

  it('deberia devolver el nombre legible de cada mercado', () => {
    expect(pipe.transform('Ganador')).toBe('Ganador (1X2)');
    expect(pipe.transform('Totales')).toBe('Totales (mas o menos)');
    expect(pipe.transform('Handicap')).toBe('Handicap');
  });
});

describe('LineaPipe', () => {
  const pipe = new LineaPipe();

  it('deberia mostrar un guion cuando el mercado no tiene linea', () => {
    expect(pipe.transform(null, 'Ganador')).toBe('-');
  });

  it('deberia agregar el signo + a las lineas positivas de handicap', () => {
    expect(pipe.transform(1.5, 'Handicap')).toBe('+1.5');
    expect(pipe.transform(-1.5, 'Handicap')).toBe('-1.5');
    expect(pipe.transform(0, 'Handicap')).toBe('0');
  });

  it('deberia dejar la linea de totales sin signo', () => {
    expect(pipe.transform(2.5, 'Totales')).toBe('2.5');
  });
});

describe('ResultadoPipe', () => {
  const pipe = new ResultadoPipe();

  it('deberia devolver el resultado solo en el mercado ganador', () => {
    expect(pipe.transform('Empate', 'Ganador', null)).toBe('Empate');
  });

  it('deberia describir totales como mas o menos de la linea', () => {
    expect(pipe.transform('Mas', 'Totales', 2.5)).toBe('Mas de 2.5');
    expect(pipe.transform('Menos', 'Totales', 2.5)).toBe('Menos de 2.5');
  });

  it('deberia mostrar la linea con signo en handicap', () => {
    expect(pipe.transform('Visitante', 'Handicap', 1.5)).toBe('Visitante +1.5');
    expect(pipe.transform('Local', 'Handicap', -1.5)).toBe('Local -1.5');
  });
});
