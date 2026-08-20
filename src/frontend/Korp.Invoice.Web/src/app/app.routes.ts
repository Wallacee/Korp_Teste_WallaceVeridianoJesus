import { Routes } from '@angular/router';

import { AppShell } from './layout/app-shell/app-shell';
import { ProductList } from './features/products/pages/product-list/product-list';

export const routes: Routes = [
  {
    path: '',
    component: AppShell,
    children: [
      {
        path: '',
        redirectTo: 'products',
        pathMatch: 'full'
      },
      {
        path: 'products/new',
        loadComponent: () => import('./features/products/pages/product-form/product-form').then(m => m.ProductForm)
      },
      {
        path: 'products/:id/edit',
        loadComponent: () => import('./features/products/pages/product-form/product-form').then(m => m.ProductForm)
      },
      {
        path: 'invoices',
        loadComponent: () => import('./features/invoices/pages/invoice-list/invoice-list').then(m => m.InvoiceList)
      },
      {
        path: 'invoices/new',
        loadComponent: () =>
          import('./features/invoices/pages/invoice-form/invoice-form')
            .then(m => m.InvoiceForm)
      },
      {
        path: 'invoices',
        loadComponent: () =>
          import('./features/invoices/pages/invoice-list/invoice-list')
            .then(m => m.InvoiceList)
      },
      {
        path: 'products',
        component: ProductList
      }
    ]
  }
];
