import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
baseurl='https://localhost:5001/api/';
private CurrentUserSource = new BehaviorSubject<User | null>(null);
CurrentUser$=this.CurrentUserSource.asObservable();

  constructor(private http : HttpClient) { }

  login(model : any){
    return this.http.post<User>(this.baseurl + 'account/login',model).pipe(
      map((response : User)=> {
        const user= response;
        if(user)
        {
          localStorage.setItem('user', JSON.stringify(user))
          this.CurrentUserSource.next(user);
        }
      })
    )
  }

  setCurrentUser(user : User){
    this.CurrentUserSource.next(user);
  }

  logout(){
    localStorage.removeItem('user');
    this.CurrentUserSource.next(null);
  }
}
