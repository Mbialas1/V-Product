import { Component, AfterViewInit } from '@angular/core';
import { NgForOf } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductHelper } from '../ShoppingCart/shoppingCart.component';

@Component({
  selector: 'app-AddItem',
  templateUrl: './item.component.html',
  styleUrls: ['./item.component.css'],
})
export class ItemComponent implements AfterViewInit {
  constructor(private http: HttpClient) {

  }
  products: Product[] = [];
  categories: any[] = [];
  selectedCategory?: Category;
  selectedProduct?: Product;

  category = new Category(this.http);
  product = new Product(this.http, this.categories);

  private hiddeAllForms(): void{
    this.category.showFormForAdd = false;
    this.category.showFormForDelete = false;
    this.product.showFormForAdd = false;
    this.product.showFormForDelete = false;
  }

  protected displayCategoryForAdd(): void {
    this.hiddeAllForms();
    this.category.showFormForAdd = true;
  }

  protected displayProductForAdd(): void {
    this.hiddeAllForms();
    this.product.showFormForAdd = true;
  }

  protected displayCategoryForDelete(): void {
    this.hiddeAllForms();
    this.category.showFormForDelete = true;
  }

  protected displayProductForDelete(): void {
    this.hiddeAllForms();
    this.product.showFormForDelete = true;
  }

  loadCategories = (): Observable<Category[]> => this.http.get<Category[]>('api/category/GetAllCategory');

  ngAfterViewInit(): void {
    this.loadCategories().subscribe((categories: Category[]) => {
      this.categories = categories;
    });
  }

  addCategory(selectedCategory?: Category): void {

    if (!selectedCategory) {
      alert("Dont choose category!");
      return;
    }

    if (this.product.chooseCategories.find(category => category.id === selectedCategory.id)) {
      alert("Category exist in list!");
      return;
    }

    this.product.chooseCategories.push(selectedCategory);
  }

  trackByAddCategory(index: number, category: Category): Category {
    return category;
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
}

export class Category {
  name: string = '';
  id: number = 0;
  showFormForAdd: boolean = false;
  showFormForDelete: boolean = false;
  items = {
    name: '',
    id: 0
  };

  constructor(private http: HttpClient) { }

  setItems() : void {
    this.items.name = this.name;
  }

  submitNewCategory(): void {
    this.setItems();

    if (!this.name || this.name.trim() === "") {
      alert("Name of category is empty!");
      return;
    }

    this.http.post('api/category/AddNewCategory', this.items).subscribe((response) => {
      console.log('Dodano nowy element:', response);
      this.name = '';
    }, (error) => {
      console.error('Something wrong => ', error);
    });
  }

  submitDeleteCategory(event: Event, select: HTMLSelectElement): void {

    if (select.options.selectedIndex === -1) {
      alert("Choose category");
      return;
    }

    event.preventDefault();
    const selectValue = select.value;
    const index = selectValue.indexOf(":");
    const selectedCategoryId = Number(selectValue.slice(index + 1));
    this.http.delete(`api/category/DeleteCategoryById?categoryId=${selectedCategoryId}`).subscribe((response) => {
      console.log('Delete succes!:', response);
      select.options.remove(select.selectedIndex);
    }, (error) => {
      console.error('Something wrong => ', error);
    });
  }
}

export class Product {

  showFormForAdd: boolean = false;
  showFormForDelete: boolean = false;
  name: string = '';
  id: number = 0;
  price: number = 0.00;
  calories: number = 0;
  categories: any[] = [];
  categoryId: number = 0;
  chooseCategories: Category[] = [];

  items = {
    name: '',
    price: 0,
    calories: 0,
    categoriesIds: [] as number[]
  }

  constructor(private http: HttpClient, _categories: any[]) { this.categories = _categories; }

  setItems(): void {
    this.items.name = this.name;
    this.items.price = this.price;
    this.items.calories = this.calories;
    this.items.categoriesIds = this.getIdsChooseCategories();
  }

  getIdsChooseCategories(): number[] {
    if (this.chooseCategories.length < 1) {
      throw new Error("Non choose category for product");
    }

    let categoriesListIds: number[] = [];

    for (let i = 0; i < this.chooseCategories.length; i++) {
      let id = this.chooseCategories[i].id;
      categoriesListIds.push(id);
    }

    console.log(this.chooseCategories);
    return categoriesListIds;
  }

  submitNewProduct(): void {
    this.setItems();

    if (this.name.trim() === '' || this.chooseCategories.length < 1) {
      alert("Name and category is required for add product");
      return;
    }
 
    this.http.post('api/product/AddNewProduct', this.items).subscribe((response) => {
      console.log('Dodano nowy element:', response);
      this.name = '';
      this.price = 0;
      this.calories = 0;
      this.chooseCategories = [];
    }, (error) => {
      console.error('Something wrong => ', error);
    });
  }

  submitDeleteProduct(event: Event, select: HTMLSelectElement): void {

    if (select.options.selectedIndex === -1) {
      alert("Choose product for remove");
      return;
    }

    event.preventDefault();
    const selectValue = select.value;
    const index = selectValue.indexOf(":");
    const selectedProductId = Number(selectValue.slice(index + 1));
    this.http.delete(`api/product/DeleteProduct?productId=${selectedProductId}`).subscribe((response) => {
      console.log('Delete succes!:', response);
      select.options.remove(select.selectedIndex);
    }, (error) => {
      console.error('Something wrong => ', error);
    });
  }
}
