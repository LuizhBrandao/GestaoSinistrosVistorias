import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { App } from './app';
import { ChecklistAvariasComponent } from './components/checklist-avarias/checklist-avarias.component';
import { TipoAcaoAvaria } from './models/vistoria.models';

describe('App & ChecklistAvarias', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App, ChecklistAvariasComponent],
      providers: [provideHttpClient()]
    }).compileComponents();
  });

  it('deve inicializar a aplicação com sucesso', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('ChecklistAvarias: deve somar subtotais e total geral via Signals', () => {
    const fixture = TestBed.createComponent(ChecklistAvariasComponent);
    const component = fixture.componentInstance;

    component.avarias.set([
      { peca: 'Capô', tipoAcao: TipoAcaoAvaria.Substituir, custoMaoDeObra: 500, custoPecas: 1500 },
      { peca: 'Farol', tipoAcao: TipoAcaoAvaria.Substituir, custoMaoDeObra: 200, custoPecas: 800 }
    ]);

    expect(component.totalMaoDeObra()).toBe(700);
    expect(component.totalPecas()).toBe(2300);
    expect(component.totalGeral()).toBe(3000);
  });

  it('ChecklistAvarias: deve acionar sinal de Perda Total quando reparo for >= 75% da FIPE', () => {
    const fixture = TestBed.createComponent(ChecklistAvariasComponent);
    const component = fixture.componentInstance;

    // FIPE R$ 100.000 -> 75% = R$ 75.000
    fixture.componentRef.setInput('valorFipe', 100000);

    // Orçamento R$ 80.000 (>= 75%)
    component.avarias.set([
      { peca: 'Monobloco', tipoAcao: TipoAcaoAvaria.Substituir, custoMaoDeObra: 30000, custoPecas: 50000 }
    ]);

    expect(component.isPerdaTotal()).toBe(true);
    expect(component.percentualFipe()).toBe(80);
  });
});
