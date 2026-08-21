import { Routes } from '@angular/router';

import { AppShell } from './layout/app-shell/app-shell';

export const routes: Routes = [
  {
    path: '',
    component: AppShell,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/dashboard/pages/dashboard/dashboard')
            .then(m => m.Dashboard)
      },

      {
        path: 'products',
        loadComponent: () =>
          import('./features/products/pages/product-list/product-list')
            .then(m => m.ProductList)
      },
      {
        path: 'products/new',
        loadComponent: () =>
          import('./features/products/pages/product-form/product-form')
            .then(m => m.ProductForm)
      },
      {
        path: 'products/:id/edit',
        loadComponent: () =>
          import('./features/products/pages/product-form/product-form')
            .then(m => m.ProductForm)
      },

      {
        path: 'invoices',
        loadComponent: () =>
          import('./features/invoices/pages/invoice-list/invoice-list')
            .then(m => m.InvoiceList)
      },
      {
        path: 'invoices/new',
        loadComponent: () =>
          import('./features/invoices/pages/invoice-form/invoice-form')
            .then(m => m.InvoiceForm)
      },
      {
        path: 'invoices/:id/edit',
        loadComponent: () =>
          import('./features/invoices/pages/invoice-form/invoice-form')
            .then(m => m.InvoiceForm)
      },
      {
        path: 'invoices/:id',
        loadComponent: () =>
          import('./features/invoices/pages/invoice-details/invoice-details')
            .then(m => m.InvoiceDetails)
      }
    ]
  },

  {
    path: '**',
    redirectTo: ''
  }
];
