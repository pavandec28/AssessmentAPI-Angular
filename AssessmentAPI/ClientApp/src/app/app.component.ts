import { Component } from '@angular/core';
import { WorkItem } from '../Interfaces/workitem';
import { AppService } from './app.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  data: WorkItem[] = [];
  title = 'TFSApp';

  constructor(private appService: AppService) {

  }

  ngOnInit(): void {
    this.initializeEmployees();
  }

  initializeEmployees(): void {
    this.appService.getWorkItems().
      subscribe((response: WorkItem[]) => {
        this.data = response;
        console.log(this.data);
      });
  }

  saveAll(): void {

    this.appService.updateWorkItems(this.data).subscribe(
      (response: any) => {
        console.log('Item created:', response);
        // Perform any additional actions after successful creation
      },
      (error: any) => {
        console.error('Error creating item:', error);
        // Handle error responses
      }
    );
  }
}
