import { Component, OnInit } from '@angular/core';
import { EmployeeService } from '../_services/employee.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Employee } from 'app/_models/employee.model';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-employees',
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.css']
})
export class EmployeesComponent implements OnInit {

  employees: Employee[] = [];
  employeeForm: FormGroup;

  constructor(
    private employeeService: EmployeeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {
    this.employeeForm = this.fb.group({
      id: [0],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      dob: ['', Validators.required],
      age: ['', Validators.required],
      salary: ['', Validators.required],
      departmentName: ['', Validators.required],
      departmentCode: ['']
    });
  }

  // On edit action, populate form
  onEdit(employee: any) {
    this.employeeForm.patchValue({
      id: employee.id,
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      dob: employee.dob,
      age: employee.age,
      salary: employee.salary,
      departmentName: employee.departmentName,
      departmentCode: employee.departmentCode
    });
    this.toastr.info('Editing Employee', 'Info');
  }

  // On delete action
  onDelete(employeeId: number) {
    this.employees = this.employees.filter(emp => emp.id !== employeeId);
    this.toastr.error('Employee deleted successfully', 'Deleted');
  }

  // On form submit
  onSubmit() {
    if (this.employeeForm.valid) {
      const newEmployee: Employee = this.employeeForm.value;

      this.employeeService.addEmployee(newEmployee).subscribe(
        employee => {
          this.employees.push(employee);
          this.employeeForm.reset();
          this.toastr.success('Employee added successfully', 'Success');
        },
        error => {
          console.error('Error adding employee', error);
          this.toastr.error('Error adding employee', 'Error');
        }
      );
    } else {
      this.toastr.warning('Please fill all required fields', 'Validation Warning');
    }
  }

  // Fetch all employees on component initialization
  ngOnInit() {
    this.employeeService.getEmployees().subscribe(
      data => {
        this.employees = data;
        this.toastr.success('Employees fetched successfully', 'Success');
      },
      error => {
        console.error('Error fetching employee data', error);
        this.toastr.error('Error fetching employee data', 'Error');
      }
    );
  }
}
