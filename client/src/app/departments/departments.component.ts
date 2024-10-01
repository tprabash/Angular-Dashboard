import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Department } from 'app/_models/department.model';
import { DepartmentService } from 'app/_services/department.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-departments',
  templateUrl: './departments.component.html',
  styleUrls: ['./departments.component.css']
})
export class DepartmentsComponent implements OnInit {

  departments: Department[] = [];
  departmentForm: FormGroup;

  constructor(
    private departmentService: DepartmentService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {
    this.departmentForm = this.fb.group({
      id: [0],
      departmentName: ['', [Validators.required]],
      departmentCode: ['', [Validators.required]]
    });
  }

  onEdit(department: any) {
    this.departmentForm.patchValue({
      id: department.id,
      departmentName: department.departmentName,
      departmentCode: department.departmentCode
    });
  }

  onDelete(departmentId: number) {
    const confirmed = window.confirm('Are you sure you want to delete this department?');
  
    if (confirmed) {
      this.departmentService.deleteDepartment(departmentId).subscribe(
        () => {
          this.departments = this.departments.filter(department => department.id !== departmentId);
          this.toastr.success('Department deleted successfully!');
        },
        error => {
          console.error('Error deleting department', error);
          this.toastr.error('Error deleting department!');
        }
      );
    }
  }

  onSubmit() {
    if (this.departmentForm.valid) {
      const department: Department = this.departmentForm.value;

      if (department.id === 0) {
        this.departmentService.addDepartment(department).subscribe(
          newDepartment => {
            this.departments.push(newDepartment);
            this.resetForm();
            this.toastr.success('Department added successfully!');
          },
          error => {
            console.error('Error adding department', error);
            this.toastr.error('Error adding department!');
          }
        );
      } else {
        this.departmentService.updateDepartment(department).subscribe(
          updatedDepartment => {
            const index = this.departments.findIndex(d => d.id === updatedDepartment.id);
            if (index !== -1) {
              this.departments[index] = updatedDepartment;
            }
            this.resetForm();
            this.toastr.success('Department updated successfully!');
          },
          error => {
            console.error('Error updating department', error);
            this.toastr.error('Error updating department!');
          }
        );
      }
    }
  }

  resetForm() {
    this.departmentForm.reset({
      id: 0,
      departmentName: '',
      departmentCode: ''
    });
  }

  ngOnInit() {
    this.departmentService.getDepartments().subscribe(
      data => {
        this.departments = data;
      },
      error => {
        console.error('Error fetching department data', error);
      }
    );
  }
}
