import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Department } from 'app/_models/department.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {

  private apiUrl = 'https://localhost:7032/api/Department';

  constructor(private http: HttpClient) { }

  getDepartments(): Observable<Department[]> {
    return this.http.get<Department[]>(`${this.apiUrl}/GetDepartment`);
  }

  addDepartment(department: Department): Observable<Department> {
    return this.http.post<Department>(`${this.apiUrl}/AddDepartment`, department);
  }
  
  updateDepartment(department: Department): Observable<Department> {
    return this.http.patch<Department>(`${this.apiUrl}/UpdateDepartment`, department);
  }

  deleteDepartment(departmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/DeleteDepartment/${departmentId}`);
  }

}
