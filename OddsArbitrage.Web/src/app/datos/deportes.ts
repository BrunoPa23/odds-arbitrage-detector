export interface DeporteOpcion {
  clave: string;
  nombre: string;
}

// Claves de deporte soportadas por The Odds API (subconjunto usado en el MVP)
export const DEPORTES: DeporteOpcion[] = [
  { clave: 'soccer_epl', nombre: 'Futbol: Premier League' },
  { clave: 'soccer_spain_la_liga', nombre: 'Futbol: La Liga' },
  { clave: 'soccer_italy_serie_a', nombre: 'Futbol: Serie A' },
  { clave: 'soccer_germany_bundesliga', nombre: 'Futbol: Bundesliga' },
  { clave: 'basketball_nba', nombre: 'Basquet: NBA' },
];
