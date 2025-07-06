import { Component, OnDestroy, OnInit } from '@angular/core';
import { WebTransportClient } from '../webtransport';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  public webTransportClient: WebTransportClient | null = null;

  public ngOnInit(): void {
    this.webTransportClient = new WebTransportClient(
      new URL("https://localhost:5001/wt")
    );
  }

  public ngOnDestroy(): void {
    if(this.webTransportClient) {
      this.webTransportClient.destroyConnection();
      this.webTransportClient = null;
    }
  }
}
