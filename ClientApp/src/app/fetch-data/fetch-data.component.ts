import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {
  public UserPrompt: UserPrompt;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<UserPrompt>(baseUrl + 'currentContent').subscribe(result => {
      this.UserPrompt = result;
    }, error => console.error(error));
  }
}

interface UserPrompt {
  Question: string;
  Answers: string[];
}
