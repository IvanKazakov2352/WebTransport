import { Component, OnDestroy, OnInit } from '@angular/core';
import { decode, encode } from '@msgpack/msgpack';
import { asyncScheduler, interval, Subject, takeUntil } from 'rxjs';
import { ITransportEvent } from '../model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  public abortController: AbortController = new AbortController()
  public transport: WebTransport = new WebTransport(
    new URL("https://localhost:5001/wt")
  );
  public reader: ReadableStreamDefaultReader<Uint8Array> | null = null;
  public writer: WritableStreamDefaultWriter<Uint8Array> | null = null;
  public destroySubject: Subject<void> = new Subject<void>()

  public async ngOnInit(): Promise<void> {
    try {
      console.log('WebTransport init connection');
      await this.transport.ready
      console.log('WebTransport connection established');

      const stream = await this.transport.createBidirectionalStream();
      this.reader = stream.readable.getReader() as ReadableStreamDefaultReader<Uint8Array>
      this.writer = stream.writable.getWriter() as WritableStreamDefaultWriter<Uint8Array>
      this.pingInterval()

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

  public pingInterval(): void {
    interval(5000, asyncScheduler)
      .pipe(
        takeUntil(this.destroySubject)
      )
      .subscribe(async () => {
        if(this.writer) {
          await this.writer.write(encode('PING'))
        }
      })
  }

  public async readFunction(): Promise<void> {
    while(!this.abortController.signal.aborted) {
      const { value, done } = await this.reader!.read()
      if(done) break
      if(value && value.length) {
        const data = decode(value) as ITransportEvent<string>
        console.log(data)
      }
    }
  }

  public async closeConnection(): Promise<void> {
    if(!this.abortController.signal.aborted) {
      this.abortController.abort()
    }
    this.destroySubject.next()
    this.destroySubject.complete()

    if(this.writer && this.reader) {
      await this.reader.cancel()
      await this.writer.close()
      this.writer = null;
      this.reader = null;
    }

    this.transport.close({
      closeCode: 101,
      reason: "Connection close"
    })
    const closed = await this.transport.closed
    
    console.log('closed code', closed.closeCode)
    console.log('closed reason', closed.reason)
  }

  public async ngOnDestroy(): Promise<void> {
    await this.closeConnection()
  }
}
