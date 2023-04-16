import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-Test',
  templateUrl: './Test.component.html',
  styleUrls: ['./Test.component.css']
})

export class TestComponent {
  public result: TestModel | undefined;
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string) {
  }
  ngOnInit() {
    this.http.get<TestModel>(this.baseUrl + 'api/TestTwo').subscribe(result => {
      this.result = result;
    }, error => console.error(error));
  }
}


interface TestModel {
  id: number;
  name: string;
}
