import { Component, OnDestroy, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  public baseUrl: string = 'https://localhost:500/';
  public abortController: AbortController = new AbortController();

  public async initWebTransport(): Promise<void> {
    try {
      const wt = new WebTransport(this.baseUrl + "wt")
      await wt.ready
      console.log("WebTransport connection established")

      const stream = await wt.createUnidirectionalStream()
      
      stream.abort()
      stream.close()
      wt.close()
    } catch (error) {
      const msg = `Transport initialization failed.
                Reason: ${(error as any).message}.
                Source: ${(error as any).source}.
                Error code: ${(error as any).streamErrorCode}.`;
      console.log(msg);
    }
  }

  public async ngOnInit(): Promise<void> {
    await this.initWebTransport();
  }

  public ngOnDestroy(): void {
    this.abortController.abort();
  }
}
