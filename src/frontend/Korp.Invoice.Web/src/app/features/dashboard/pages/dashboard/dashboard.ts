import {
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
  inject,
  signal
} from '@angular/core';

import { RouterLink } from '@angular/router';

import {
  forkJoin,
  finalize,
  map,
  of,
  switchMap
} from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import {
  Chart,
  ChartConfiguration,
  registerables
} from 'chart.js';

import { DashboardApiService } from '../../services/dashboard-api.service';

import {
  BillingDashboardSummary,
  DailyConsumption,
  TopProduct
} from '../../models/billing-dashboard.model';

import {
  InventoryDashboardSummary
} from '../../models/inventory-dashboard.model';

import { ProductApiService } from '../../../products/services/product-api.service';
import { Product } from '../../../products/models/product.model';

import { NotificationService } from '../../../../shared/services/notification.service';

Chart.register(...registerables);

interface TopProductView {
  productId: string;
  code: string;
  description: string;
  quantity: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,

  imports: [
    RouterLink,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],

  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard implements OnDestroy {

  private readonly dashboardApiService =
    inject(DashboardApiService);

  private readonly productApiService =
    inject(ProductApiService);

  private readonly notification =
    inject(NotificationService);

  readonly isLoading = signal(false);

  readonly billingSummary =
    signal<BillingDashboardSummary | null>(null);

  readonly inventorySummary =
    signal<InventoryDashboardSummary | null>(null);

  readonly consumption =
    signal<DailyConsumption[]>([]);

  readonly topProducts =
    signal<TopProductView[]>([]);

  private consumptionCanvas?: ElementRef<HTMLCanvasElement>;
  private topProductsCanvas?: ElementRef<HTMLCanvasElement>;

  private consumptionChartInstance?: Chart<'line'>;
  private topProductsChartInstance?: Chart<'bar'>;

  @ViewChild('consumptionChart')
  set consumptionChartElement(
    element: ElementRef<HTMLCanvasElement> | undefined
  ) {
    this.consumptionCanvas = element;

    if (element) {
      this.renderConsumptionChart();
    }
  }

  @ViewChild('topProductsChart')
  set topProductsChartElement(
    element: ElementRef<HTMLCanvasElement> | undefined
  ) {
    this.topProductsCanvas = element;

    if (element) {
      this.renderTopProductsChart();
    }
  }

  constructor() {
    this.loadDashboard();
  }

  loadDashboard(): void {

    this.destroyCharts();

    this.isLoading.set(true);

    forkJoin({
      billing:
        this.dashboardApiService
          .getBillingSummary(),

      inventory:
        this.dashboardApiService
          .getInventorySummary(),

      consumption:
        this.dashboardApiService
          .getDailyConsumption(30),

      topProducts:
        this.dashboardApiService
          .getTopProducts(5)
    })
      .pipe(

        switchMap(result => {

          const productIds =
            result.topProducts
              .map(item => item.productId);

          if (productIds.length === 0) {

            return of({
              ...result,
              products: [] as Product[]
            });
          }

          return this.productApiService
            .getByIds(productIds)
            .pipe(

              map(products => ({
                ...result,
                products
              }))

            );
        }),

        finalize(() => {
          this.isLoading.set(false);
        })
      )
      .subscribe({

        next: result => {

          this.billingSummary.set(
            result.billing
          );

          this.inventorySummary.set(
            result.inventory
          );

          this.consumption.set(
            result.consumption
          );

          this.topProducts.set(
            this.mapTopProducts(
              result.topProducts,
              result.products
            )
          );

          /*
           * Não renderizamos os gráficos aqui.
           *
           * O isLoading ainda controla a existência
           * dos canvas no template.
           *
           * Quando o Angular recriar os canvas,
           * os setters do @ViewChild serão chamados
           * e renderizarão os gráficos.
           */
        },

        error: error => {

          console.error(
            'Erro ao carregar dashboard:',
            error
          );

          this.notification.error(
            'Não foi possível carregar os indicadores da visão geral.'
          );
        }
      });
  }

  private mapTopProducts(
    data: TopProduct[],
    products: Product[]
  ): TopProductView[] {

    const productMap =
      new Map<string, Product>(
        products.map(product => [
          product.id,
          product
        ])
      );

    return data.map(item => {

      const product =
        productMap.get(item.productId);

      return {
        productId: item.productId,

        code:
          product?.code ??
          '-',

        description:
          product?.description ??
          'Produto não encontrado',

        quantity:
          item.quantity
      };
    });
  }

  private renderConsumptionChart(): void {

    const canvas =
      this.consumptionCanvas?.nativeElement;

    if (!canvas) {
      return;
    }

    this.consumptionChartInstance?.destroy();

    const data =
      this.consumption();

    const configuration:
      ChartConfiguration<'line'> = {

      type: 'line',

      data: {

        labels: data.map(item =>
          new Date(item.date)
            .toLocaleDateString(
              'pt-BR',
              {
                day: '2-digit',
                month: '2-digit'
              }
            )
        ),

        datasets: [
          {
            label:
              'Unidades consumidas',

            data:
              data.map(
                item => item.quantity
              ),

            borderColor:
              '#2563eb',

            backgroundColor:
              'rgba(37, 99, 235, 0.08)',

            fill: true,

            tension: 0.35,

            borderWidth: 2,

            pointRadius: 3,

            pointHoverRadius: 5,

            pointBackgroundColor:
              '#2563eb'
          }
        ]
      },

      options: {

        responsive: true,

        maintainAspectRatio: false,

        interaction: {
          intersect: false,
          mode: 'index'
        },

        plugins: {

          legend: {
            display: false
          },

          tooltip: {
            displayColors: false
          }
        },

        scales: {

          x: {

            grid: {
              display: false
            },

            ticks: {
              color: '#94a3b8',
              maxRotation: 0
            }
          },

          y: {

            beginAtZero: true,

            grid: {
              color: '#f1f5f9'
            },

            ticks: {
              color: '#94a3b8',
              precision: 0
            }
          }
        }
      }
    };

    this.consumptionChartInstance =
      new Chart(
        canvas,
        configuration
      );
  }

  private renderTopProductsChart(): void {

    const canvas =
      this.topProductsCanvas?.nativeElement;

    if (!canvas) {
      return;
    }

    this.topProductsChartInstance?.destroy();

    const data =
      this.topProducts();

    const configuration:
      ChartConfiguration<'bar'> = {

      type: 'bar',

      data: {

        labels:
          data.map(
            item => item.code
          ),

        datasets: [
          {
            label:
              'Quantidade utilizada',

            data:
              data.map(
                item => item.quantity
              ),

            backgroundColor:
              '#2563eb',

            borderRadius: 6,

            barThickness: 22
          }
        ]
      },

      options: {

        indexAxis: 'y',

        responsive: true,

        maintainAspectRatio: false,

        plugins: {

          legend: {
            display: false
          },

          tooltip: {

            callbacks: {

              title: context => {

                const index =
                  context[0].dataIndex;

                return (
                  data[index]?.description ??
                  ''
                );
              }
            }
          }
        },

        scales: {

          x: {

            beginAtZero: true,

            grid: {
              color: '#f1f5f9'
            },

            ticks: {
              color: '#94a3b8',
              precision: 0
            }
          },

          y: {

            grid: {
              display: false
            },

            ticks: {
              color: '#64748b'
            }
          }
        }
      }
    };

    this.topProductsChartInstance =
      new Chart(
        canvas,
        configuration
      );
  }

  private destroyCharts(): void {

    this.consumptionChartInstance?.destroy();

    this.consumptionChartInstance =
      undefined;

    this.topProductsChartInstance?.destroy();

    this.topProductsChartInstance =
      undefined;
  }

  ngOnDestroy(): void {

    this.destroyCharts();
  }
}
