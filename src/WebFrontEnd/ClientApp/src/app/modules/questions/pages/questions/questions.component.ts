import { Component} from '@angular/core';

const questions = [
  {question: "What is ScrawlBrawl?", answer: "ScrawlBrawl is a game built for everyone by anyone. <br><br>Put your wordsmithing and drawing skills to the test with one of many game modes, perfect for small gatherings and huge events alike." },
  {question: "Where can I play?", answer: "Currently, you can play via online browser or your phone." },
  {question: "How do I host?", answer: "In the top right, click on \"Lobby\"<br><br>Create an account or sign in with an account<br><br>Start up the game viewer and enter the lobby code<br><br>Share lobby code with friends<br><br>On the lobby page, select a game<br><br>Configure game settings<br><br>Start the game!" },
  {question: "How do I play?", answer: "In the top right, click on \"Play\"<br><br>Make a nickname and draw a doodle to represent yourself. You can save your doodle with the star icon on the right for future use through clicking the photo album button.<br><br>The lobby host will provide a lobby code to use for joining their game." },
  {question: "How do I use the drawing canvas?", answer: "Use the slider for the size of your brush or eraser<br><br>You can change the color of your brush with the color swapper square<br><br>There is a paint bucket for a color fill option<br><br>The back arrow allows you to undo your last action<br><br>Use the trashcan to clear your canvas<br><br>You can use the gallery (photo album icon) for saved drawings<br><br>You can use the star icon to save drawings either for your profile or for future scrawl brawls" },
  {question: "How do I play each game?", answer: "You can watch our demo video for a full tutorial on the homepage <a href=https://www.scrawlbrawl.tv>here</a><br><br>If you go into the lobby page, you'll find full instructions for each game as well on the homepage <a href=https://www.scrawlbrawl.tv>here</a>" },
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