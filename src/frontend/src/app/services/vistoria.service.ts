import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ConcluirVistoriaRequest, FotoEvidencia, VistoriaPendente } from '../models/vistoria.models';

@Injectable({
  providedIn: 'root'
})
export class VistoriaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:3000/api/vistorias';

  obterVistoriasPendentes(): Observable<VistoriaPendente[]> {
    return this.http.get<VistoriaPendente[]>(`${this.baseUrl}/pendentes`);
  }

  uploadFoto(file: File): Observable<FotoEvidencia> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<FotoEvidencia>(`${this.baseUrl}/upload`, formData);
  }

  concluirVistoria(vistoriaId: string, payload: ConcluirVistoriaRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/${vistoriaId}/concluir`, payload);
  }
}
