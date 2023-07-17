import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { WorkItem } from '../Interfaces/workitem';


@Injectable({
  providedIn: 'root'
})
export class AppService {
  private apiUrl = 'https://localhost:7056/TFS';

  constructor(private http: HttpClient) { }

  getWorkItems(): Observable<WorkItem[]> {
    var result = this.http.get<WorkItem[]>(this.apiUrl);
    return result;
  }

  updateWorkItems(itemData: any[]): Observable<any> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post<any>(this.apiUrl, itemData, { headers });
  }
}
