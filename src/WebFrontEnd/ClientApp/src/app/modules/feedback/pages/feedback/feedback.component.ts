import { Component, Inject, ViewEncapsulation } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';


@Component({
  selector: 'feedback',
  templateUrl: './feedback.component.html',
  styleUrls: ['./feedback.component.css']
})
export class FeedbackComponent {

  private httpOptions = {
    headers: new HttpHeaders({
        'Content-Type': 'application/json',
    })
};
  constructor(private http: HttpClient) {

    console.log("PLEASE WORK");
  }

  onSubmitFeedback(text: string) {
    let body = {feedback: text}
    this.http.post("feedback", body, this.httpOptions).subscribe(
      data => {
          console.log("POST Request is successful ", data);
      },
      error => {
          console.log("Error", error);
          // TODO: show this string to user.
      });
  }
}
