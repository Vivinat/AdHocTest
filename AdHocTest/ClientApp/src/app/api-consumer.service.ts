import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ApiConsumerService {
  private baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  getPlants(query: string): Observable<any> {
    return this.http.get<any>(
      `${this.baseUrl}/plants?query=${encodeURIComponent(query)}`
    );
  }
}
