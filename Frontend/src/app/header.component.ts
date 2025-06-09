import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
imports: [CommonModule, RouterModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})

export class HeaderComponent implements OnInit{
  isAdminUser = false;
  ngOnInit(): void {
    console.log("Is Admin calling");
    this.isAdminUser = this.isAdmin();
    console.log("Is Admin after calling "+this.isAdminUser);
  }
  get isLoggedIn() {
    return !!localStorage.getItem('jwt');
  }
   isAdmin() {
    // Example: check for admin role in JWT (assumes JWT payload contains 'role')
    const token = localStorage.getItem('jwt');
    console.log("Token exist "+token);
    if (!token) return false;
    
try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      var isAdminRole = false;
      Object.entries(payload).forEach(([key, value]) => {
        console.log("value data :: "+value);
        if(value == "Admin") {
          localStorage.setItem('role', "Admin");
          isAdminRole = true;
          return;
        }
        
      });
      return isAdminRole;
    } catch {
      return false;
    }
  }
  logout() {
    localStorage.removeItem('jwt');
    window.location.href = '/login';
  }
}
