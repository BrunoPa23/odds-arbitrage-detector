import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'oportunidades' },
  {
    path: 'oportunidades',
    loadComponent: () => import('./paginas/oportunidades/oportunidades').then((m) => m.Oportunidades),
  },
  {
    path: 'partidos',
    loadComponent: () => import('./paginas/partidos/partidos').then((m) => m.Partidos),
  },
];
