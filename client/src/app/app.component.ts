import { Component, OnDestroy, OnInit } from '@angular/core';
import { decode, encode } from "@msgpack/msgpack"

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  public wtUrl: string = 'https://localhost:5001/wt';
  public abortController: AbortController = new AbortController();
  
  public async initWebTransport(): Promise<void> {
    try {
      const wt = new WebTransport(this.wtUrl)
      await wt.ready
      console.log("WebTransport connection established")
      const stream = await wt.createBidirectionalStream()
      console.log("Created BidirectionalStream")
      const reader = stream.readable.getReader()
      const writer = stream.writable.getWriter()
      writer.write(encode("PING"))

      while(true) {
        const { value, done } = await reader.read()
        if(done) break
        console.log(decode(value))
      }
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
