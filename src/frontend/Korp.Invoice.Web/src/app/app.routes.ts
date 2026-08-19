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
        path: 'products',
        component: ProductList
      }
    ]
  }
];
