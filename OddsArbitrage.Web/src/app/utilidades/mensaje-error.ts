import { HttpErrorResponse } from '@angular/common/http';

// La api responde los errores como ProblemDetails; se muestra su title si viene
export function mensajeDeError(err: HttpErrorResponse, porDefecto: string): string {
  const titulo = err.error?.title as string | undefined;
  return titulo ? titulo : porDefecto;
}
