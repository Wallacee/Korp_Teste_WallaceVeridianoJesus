import { Routes } from '@angular/router';
import { ProductList } from './features/products/pages/product-list/product-list';

export const routes: Routes = [
  {
    path: 'products',
    component: ProductList
  },
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full'
  }
];
