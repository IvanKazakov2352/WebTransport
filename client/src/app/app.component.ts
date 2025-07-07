import { Component, OnDestroy, OnInit } from '@angular/core';
import { decode, encode } from '@msgpack/msgpack';
import { asyncScheduler, interval, Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  public abortController: AbortController = new AbortController()
  public transport: WebTransport = new WebTransport(new URL("https://localhost:5001/wt"));
  public reader!: ReadableStreamDefaultReader<any>;
  public writer!: WritableStreamDefaultWriter<any>;
  public destroySubject: Subject<void> = new Subject<void>()

  public async ngOnInit(): Promise<void> {
    try {
      console.log('WebTransport init connection');
      await this.transport.ready
      this.pingPong()
      console.log('WebTransport connection established');

      const stream = await this.transport.createBidirectionalStream();
      this.reader = stream.readable.getReader()
      this.writer = stream.writable.getWriter()

      await this.readFunction()
    } catch (error) {
      const errorMessage = `
        WebTransport connection error.
        Reason: ${(error as any).message}.
        Source: ${(error as any).source}.
        Error code: ${(error as any).streamErrorCode}.
      `;
      console.error(errorMessage)
    }
  }

  public pingPong(): void {
    interval(5000, asyncScheduler)
      .pipe(takeUntil(this.destroySubject))
      .subscribe(async () => {
        await this.writer.write(encode('PING'))
      })
  }

  public async readFunction(): Promise<void> {
    while(!this.abortController.signal.aborted) {
      const { value, done } = await this.reader.read()
      if(done) break
      console.log(decode(value))
    }
  }

  public async closeConnection(): Promise<void> {
    if(!this.abortController.signal.aborted) {
      this.abortController.abort()
    }
    this.destroySubject.next()

    await this.reader.cancel()
    await this.writer.close()

    this.transport.close({
      closeCode: 101,
      reason: "Ручное закрытие соединения"
    })
    const closed = await this.transport.closed
    
    console.log('closed code', closed.closeCode)
    console.log('closed reason', closed.reason)
  }

  public ngOnDestroy(): void {
    this.abortController.abort()
    this.destroySubject.next()
    this.destroySubject.complete()
  }
}
