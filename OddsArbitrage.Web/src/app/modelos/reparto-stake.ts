import { Cuota } from './cuota';

export interface ApuestaRecomendada {
  cuota: Cuota;

  // Monto a apostar en esta cuota
  stake: number;

  // Monto que se recibe si este resultado ocurre (stake x valor de la cuota)
  retorno: number;
}

export interface RepartoStake {
  stakeTotal: number;
  apuestas: ApuestaRecomendada[];

  // Retorno minimo entre todas las apuestas; es lo que se cobra sin importar el resultado
  retornoGarantizado: number;
  gananciaGarantizada: number;
}
