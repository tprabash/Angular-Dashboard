import { Routes } from '@angular/router';

import { DashboardComponent } from '../../dashboard/dashboard.component';
import { EmployeesComponent } from '../../employees/employees.component';
import { DepartmentsComponent } from 'app/departments/departments.component';

export const AdminLayoutRoutes: Routes = [
    { path: 'dashboard',      component: DashboardComponent },
    { path: 'employees',   component: EmployeesComponent },
    { path: 'departments',   component: DepartmentsComponent },
];
