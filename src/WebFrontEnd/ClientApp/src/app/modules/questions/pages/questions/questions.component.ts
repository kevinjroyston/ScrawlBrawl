import { Component} from '@angular/core';

const questions = [
  {question: "What is ScrawlBrawl?", answer: "ScrawlBrawl is a game built for everyone by anyone. Put your wordsmithing and drawing skills to the test with one of many game modes, perfect for small gatherings and huge events alike." },
  {question: "How can I help?", answer: "We are taking in contributers! Contact us through one of our socials." }
]

@Component({
  selector: 'questions',
  templateUrl: './questions.component.html',
  styleUrls: ['./questions.component.scss']
})
export class QuestionsComponent {

    questions: any[]

    constructor() {
      this.questions = questions;
    }
}