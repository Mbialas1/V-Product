import { Component, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Category, Product } from '../AddItem/item.component';
import { ItemComponent } from '../AddItem/item.component';
import { Observable } from 'rxjs';
import { NgForOf } from '@angular/common';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ShoppingCart',
  templateUrl: './shoppingCart.component.html',
  styleUrls: ['./shoppingCart.component.css'],
})
export class ShoppingCartComponent implements AfterViewInit {

  categories: Category[] = [];
  products: Product[] = [];
  productsInShoppingCart: ProductInShoppingCart[] = [];

  sumCalories: number = 0;
  amount: number = 0;

  selectedCategory: Category | null = null;
  selectedProduct: Product | null = null;

  constructor(private http: HttpClient) { }

  ngAfterViewInit(): void {
    var itemComponent = new ItemComponent(this.http);
    itemComponent.loadCategories().subscribe((categories: Category[]) => {
      this.categories = categories;
    });
  }

  getProductsFromCategory(event: any) {
    let idCategory: number | undefined = this.selectedCategory?.id;
    if (idCategory && idCategory > 0) {
      let helper = new ProductHelper(this.http);
      helper.getProductsFromCategoryByIdCategory(idCategory).subscribe((products: Product[]) => {
        this.products = products;
      })
    }
  }

  AddProductToList(): void {
    if (!this.selectedCategory || !this.selectedProduct) {
      alert("Select category and product!");
      return;
    }
    let product: ProductInShoppingCart = new ProductInShoppingCart(this.selectedProduct, this.selectedCategory);
    this.productsInShoppingCart.push(product);
    this.sumCalories += product.product?.calories ?? 0;
    this.amount += product.product?.price ?? 0;
  }

  trackByAddProduct(index: number, productCat: ProductInShoppingCart): ProductInShoppingCart {
    return productCat;
  }
}

export class ProductInShoppingCart{

  product?: Product;
  category?: Category;
  constructor(product: Product, category: Category) {
    this.product = product;
    this.category = category;
  }
}

export class ProductHelper {

  constructor(private http: HttpClient) { }

  getProductsFromCategoryByIdCategory(idCat: number): Observable<Product[]> {
    return this.http.get<Product[]>(`api/product/GetProductsByCategory?categoryId=${idCat}`);
  }
}
