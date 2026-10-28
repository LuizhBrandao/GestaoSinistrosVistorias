import { CommonModule } from '@angular/common';
import { Component, computed, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ItemAvaria, TipoAcaoAvaria } from '../../models/vistoria.models';

@Component({
  selector: 'app-checklist-avarias',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="card border-0 shadow-sm rounded-3 mb-4">
      <div class="card-header bg-white py-3 border-bottom d-flex justify-content-between align-items-center">
        <div>
          <h5 class="fw-bold mb-0"><i class="bi bi-tools text-primary me-2"></i>Checklist de Avarias por Peças</h5>
          <span class="text-muted small">Adicione cada componente avariado identificando ação e orçamento.</span>
        </div>
        <button type="button" class="btn btn-sm btn-outline-primary" (click)="adicionarItem()">
          <i class="bi bi-plus-circle me-1"></i> Adicionar Peça
        </button>
      </div>

      <div class="card-body">
        <!-- Tabela Dinâmica Reativa -->
        <div class="table-responsive mb-3">
          <table class="table table-bordered table-sm align-middle">
            <thead class="table-light">
              <tr>
                <th style="width: 35%;">Peça / Componente</th>
                <th style="width: 25%;">Ação Recomendada</th>
                <th style="width: 18%;">Mão de Obra (R$)</th>
                <th style="width: 18%;">Peças / Peças Novas (R$)</th>
                <th style="width: 4%;" class="text-center">Ação</th>
              </tr>
            </thead>
            <tbody>
              @for (item of avarias(); track $index) {
                <tr>
                  <td>
                    <input type="text" [(ngModel)]="item.peca" (ngModelChange)="atualizar()"
                           class="form-control form-control-sm" placeholder="Ex: Capô, Farol, Para-choque..." />
                  </td>
                  <td>
                    <select [(ngModel)]="item.tipoAcao" (ngModelChange)="atualizar()" class="form-select form-select-sm">
                      <option [ngValue]="1">Recuperar / Funilaria</option>
                      <option [ngValue]="2">Substituir Peça Nova</option>
                      <option [ngValue]="3">Apenas Pintura</option>
                    </select>
                  </td>
                  <td>
                    <input type="number" [(ngModel)]="item.custoMaoDeObra" (ngModelChange)="atualizar()"
                           min="0" step="50" class="form-control form-control-sm text-end" />
                  </td>
                  <td>
                    <input type="number" [(ngModel)]="item.custoPecas" (ngModelChange)="atualizar()"
                           min="0" step="50" class="form-control form-control-sm text-end" />
                  </td>
                  <td class="text-center">
                    <button type="button" class="btn btn-sm btn-outline-danger border-0 p-1"
                            (click)="removerItem($index)" [disabled]="avarias().length <= 1">
                      <i class="bi bi-trash"></i>
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Indicador Reativo com Angular Signals de Perda Parcial vs. Perda Total -->
        <div class="row g-3">
          <div class="col-md-4">
            <div class="p-3 bg-light rounded text-center">
              <span class="small text-muted d-block">Subtotal Mão de Obra</span>
              <strong class="fs-5 text-dark">R$ {{ totalMaoDeObra().toFixed(2) }}</strong>
            </div>
          </div>
          <div class="col-md-4">
            <div class="p-3 bg-light rounded text-center">
              <span class="small text-muted d-block">Subtotal Peças</span>
              <strong class="fs-5 text-secondary">R$ {{ totalPecas().toFixed(2) }}</strong>
            </div>
          </div>
          <div class="col-md-4">
            <div class="p-3 rounded text-center border" [ngClass]="isPerdaTotal() ? 'bg-danger-subtle border-danger' : 'bg-primary-subtle border-primary'">
              <span class="small d-block fw-semibold" [ngClass]="isPerdaTotal() ? 'text-danger' : 'text-primary'">
                Total do Orçamento Pericial ({{ percentualFipe().toFixed(1) }}% FIPE)
              </span>
              <strong class="fs-5" [ngClass]="isPerdaTotal() ? 'text-danger' : 'text-primary'">
                R$ {{ totalGeral().toFixed(2) }}
              </strong>
            </div>
          </div>
        </div>

        @if (isPerdaTotal()) {
          <div class="alert alert-danger mt-3 mb-0 d-flex align-items-center gap-2 small">
            <i class="bi bi-exclamation-octagon-fill fs-5"></i>
            <div>
              <strong>Indicativo de Indenização Integral (Perda Total):</strong> O orçamento de reparos atingiu ou superou 75% da Tabela FIPE do veículo (R$ {{ (valorFipe() * 0.75).toFixed(2) }}).
            </div>
          </div>
        }
      </div>
    </div>
  `
})
export class ChecklistAvariasComponent {
  public valorFipe = input<number>(100000);
  public avariasChanged = output<ItemAvaria[]>();

  public avarias = signal<ItemAvaria[]>([
    { peca: 'Para-choque Dianteiro', tipoAcao: TipoAcaoAvaria.Recuperar, custoMaoDeObra: 600, custoPecas: 400 },
    { peca: 'Grade do Radiador', tipoAcao: TipoAcaoAvaria.Substituir, custoMaoDeObra: 200, custoPecas: 1200 }
  ]);

  // Angular Signals: Computação reativa com zero overhead de detecção de mudanças
  public totalMaoDeObra = computed(() =>
    this.avarias().reduce((acc, item) => acc + (Number(item.custoMaoDeObra) || 0), 0)
  );

  public totalPecas = computed(() =>
    this.avarias().reduce((acc, item) => acc + (Number(item.custoPecas) || 0), 0)
  );

  public totalGeral = computed(() =>
    this.totalMaoDeObra() + this.totalPecas()
  );

  public percentualFipe = computed(() => {
    const fipe = this.valorFipe();
    return fipe > 0 ? (this.totalGeral() / fipe) * 100 : 0;
  });

  public isPerdaTotal = computed(() =>
    this.percentualFipe() >= 75.0
  );

  adicionarItem(): void {
    this.avarias.update(lista => [
      ...lista,
      { peca: '', tipoAcao: TipoAcaoAvaria.Substituir, custoMaoDeObra: 0, custoPecas: 0 }
    ]);
    this.atualizar();
  }

  removerItem(index: number): void {
    this.avarias.update(lista => lista.filter((_, i) => i !== index));
    this.atualizar();
  }

  atualizar(): void {
    this.avariasChanged.emit(this.avarias());
  }
}
