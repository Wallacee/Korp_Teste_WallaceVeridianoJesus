import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { HttpErrorResponse } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { ProductList } from './product-list';
import { ProductApiService } from '../../services/product-api.service';
import { NotificationService } from '../../../../shared/services/notification.service';
import { Product } from '../../models/product.model';

describe('ProductList', () => {

  let component: ProductList;
  let fixture: ComponentFixture<ProductList>;

  let productApiService: {
    search: ReturnType<typeof vi.fn>;
    delete: ReturnType<typeof vi.fn>;
  };

  let notificationService: {
    success: ReturnType<typeof vi.fn>;
    warning: ReturnType<typeof vi.fn>;
    error: ReturnType<typeof vi.fn>;
  };

  let dialog: {
    open: ReturnType<typeof vi.fn>;
  };

  const products: Product[] = [
    {
      id: '11111111-1111-1111-1111-111111111111',
      code: 'SURF-001',
      description: 'Prancha Shortboard',
      stock: 10,
      createdAtUtc: '2026-08-20T10:00:00Z'
    } as Product,

    {
      id: '22222222-2222-2222-2222-222222222222',
      code: 'SURF-002',
      description: 'Prancha Longboard',
      stock: 5,
      createdAtUtc: '2026-08-20T11:00:00Z'
    } as Product
  ];

  beforeEach(async () => {

    productApiService = {
      search: vi.fn(),
      delete: vi.fn()
    };

    notificationService = {
      success: vi.fn(),
      warning: vi.fn(),
      error: vi.fn()
    };

    dialog = {
      open: vi.fn()
    };

    productApiService.search.mockReturnValue(
      of({
        items: products,
        totalCount: 2
      })
    );

    await TestBed.configureTestingModule({
      imports: [
        ProductList
      ],

      providers: [
        provideRouter([]),

        {
          provide: ProductApiService,
          useValue: productApiService
        },

        {
          provide: NotificationService,
          useValue: notificationService
        },

        {
          provide: MatDialog,
          useValue: dialog
        }
      ]
    }).compileComponents();

    fixture =
      TestBed.createComponent(ProductList);

    component =
      fixture.componentInstance;
  });

  it('deve carregar a primeira página de produtos ao iniciar', () => {

    fixture.detectChanges();

    expect(
      productApiService.search
    ).toHaveBeenCalledTimes(1);

    expect(
      productApiService.search
    ).toHaveBeenCalledWith({
      search: '',
      page: 1,
      pageSize: 10,
      sortBy: 'code',
      sortDirection: 'asc'
    });

    expect(
      component.products()
    ).toEqual(products);

    expect(
      component.totalCount()
    ).toBe(2);

    expect(
      component.isLoading()
    ).toBe(false);
  });

  it('deve pesquisar produtos após o debounce', async () => {

    fixture.detectChanges();

    productApiService.search.mockClear();

    component.pageIndex = 3;

    component.searchControl.setValue(
      'prancha'
    );

    await new Promise(resolve =>
      setTimeout(resolve, 450)
    );

    expect(
      component.pageIndex
    ).toBe(0);

    expect(
      productApiService.search
    ).toHaveBeenCalledTimes(1);

    expect(
      productApiService.search
    ).toHaveBeenCalledWith({
      search: 'prancha',
      page: 1,
      pageSize: 10,
      sortBy: 'code',
      sortDirection: 'asc'
    });
  });

  it('não deve pesquisar antes do tempo de debounce', async () => {

    fixture.detectChanges();

    productApiService.search.mockClear();

    component.searchControl.setValue(
      'surf'
    );

    await new Promise(resolve =>
      setTimeout(resolve, 200)
    );

    expect(
      productApiService.search
    ).not.toHaveBeenCalled();
  });

  it('não deve pesquisar novamente quando o valor não mudar', async () => {

    fixture.detectChanges();

    productApiService.search.mockClear();

    component.searchControl.setValue(
      'surf'
    );

    await new Promise(resolve =>
      setTimeout(resolve, 450)
    );

    expect(
      productApiService.search
    ).toHaveBeenCalledTimes(1);

    component.searchControl.setValue(
      'surf'
    );

    await new Promise(resolve =>
      setTimeout(resolve, 450)
    );

    expect(
      productApiService.search
    ).toHaveBeenCalledTimes(1);
  });

  it('deve carregar a página selecionada', () => {

    fixture.detectChanges();

    productApiService.search.mockClear();

    component.onPageChange({
      pageIndex: 2,
      pageSize: 20,
      length: 100,
      previousPageIndex: 1
    });

    expect(
      component.pageIndex
    ).toBe(2);

    expect(
      component.pageSize
    ).toBe(20);

    expect(
      productApiService.search
    ).toHaveBeenCalledWith({
      search: '',
      page: 3,
      pageSize: 20,
      sortBy: 'code',
      sortDirection: 'asc'
    });
  });

  it('deve ordenar produtos no servidor', () => {

    fixture.detectChanges();

    productApiService.search.mockClear();

    component.pageIndex = 4;

    component.onSortChange({
      active: 'description',
      direction: 'desc'
    });

    expect(
      component.pageIndex
    ).toBe(0);

    expect(
      component.sortBy
    ).toBe('description');

    expect(
      component.sortDirection
    ).toBe('desc');

    expect(
      productApiService.search
    ).toHaveBeenCalledWith({
      search: '',
      page: 1,
      pageSize: 10,
      sortBy: 'description',
      sortDirection: 'desc'
    });
  });

  it('deve voltar para ordenação padrão quando sort for removido', () => {

    fixture.detectChanges();

    productApiService.search.mockClear();

    component.sortBy = 'stock';
    component.sortDirection = 'desc';

    component.onSortChange({
      active: 'stock',
      direction: ''
    });

    expect(
      component.sortBy
    ).toBe('code');

    expect(
      component.sortDirection
    ).toBe('asc');

    expect(
      component.pageIndex
    ).toBe(0);
  });

  it('deve limpar a lista quando ocorrer erro ao carregar produtos', () => {

    productApiService.search.mockReturnValue(
      throwError(
        () => new HttpErrorResponse({
          status: 500
        })
      )
    );

    fixture.detectChanges();

    expect(
      component.products()
    ).toEqual([]);

    expect(
      component.totalCount()
    ).toBe(0);

    expect(
      component.isLoading()
    ).toBe(false);
  });

  it('não deve excluir produto quando usuário cancelar', () => {

    fixture.detectChanges();

    dialog.open.mockReturnValue({
      afterClosed: () => of(false)
    });

    component.deleteProduct(
      products[0]
    );

    expect(
      dialog.open
    ).toHaveBeenCalled();

    expect(
      productApiService.delete
    ).not.toHaveBeenCalled();
  });

  it('deve excluir produto e recarregar a lista após confirmação', () => {

    fixture.detectChanges();

    productApiService.search.mockClear();

    dialog.open.mockReturnValue({
      afterClosed: () => of(true)
    });

    productApiService.delete.mockReturnValue(
      of(void 0)
    );

    component.deleteProduct(
      products[0]
    );

    expect(
      productApiService.delete
    ).toHaveBeenCalledTimes(1);

    expect(
      productApiService.delete
    ).toHaveBeenCalledWith(
      products[0].id
    );

    expect(
      notificationService.success
    ).toHaveBeenCalledWith(
      'Produto excluído com sucesso.'
    );

    expect(
      productApiService.search
    ).toHaveBeenCalledTimes(1);
  });

  it('deve exibir warning quando produto estiver vinculado a nota fiscal', () => {

    fixture.detectChanges();

    dialog.open.mockReturnValue({
      afterClosed: () => of(true)
    });

    productApiService.delete.mockReturnValue(
      throwError(
        () => new HttpErrorResponse({
          status: 409,

          error: {
            detail:
              'O produto SURF-001 não pode ser excluído porque está vinculado a uma ou mais notas fiscais.'
          }
        })
      )
    );

    component.deleteProduct(
      products[0]
    );

    expect(
      notificationService.warning
    ).toHaveBeenCalledWith(
      'O produto SURF-001 não pode ser excluído porque está vinculado a uma ou mais notas fiscais.'
    );

    expect(
      notificationService.error
    ).not.toHaveBeenCalled();
  });

  it('deve exibir erro quando não for possível excluir o produto', () => {

    fixture.detectChanges();

    dialog.open.mockReturnValue({
      afterClosed: () => of(true)
    });

    productApiService.delete.mockReturnValue(
      throwError(
        () => new HttpErrorResponse({
          status: 500,

          error: {
            detail:
              'Erro inesperado ao excluir produto.'
          }
        })
      )
    );

    component.deleteProduct(
      products[0]
    );

    expect(
      notificationService.error
    ).toHaveBeenCalledWith(
      'Erro inesperado ao excluir produto.'
    );
  });
});
