import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TablaOportunidades } from './tabla-oportunidades';

describe('TablaOportunidades', () => {
  let component: TablaOportunidades;
  let fixture: ComponentFixture<TablaOportunidades>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TablaOportunidades],
    }).compileComponents();

    fixture = TestBed.createComponent(TablaOportunidades);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
