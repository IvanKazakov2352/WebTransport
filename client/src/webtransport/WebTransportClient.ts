import { wait } from '../utils';
import { IWebTransportClient } from './IWebTransportClient';
import { IWebTransportClientOptions } from './IWebTransportClientOptions';

export class WebTransportClient implements IWebTransportClient {
  private transport: WebTransport | null = null;
  private abortController: AbortController = new AbortController();
  private url: URL | null = null;
  private pingPongInterval: number = 25000;
  private maxRetries: number = 5;
  private attempts: number = 0;
  private retryExponentialBackoff: number = 5000;
  public isConnectedWebTransport: boolean = false;

  constructor(url: URL, options?: IWebTransportClientOptions) {
    this.url = url
    if (options && options.pingPongInterval) {
      this.pingPongInterval = options.pingPongInterval;
    }
    if(options && options.maxRetries) {
      this.maxRetries = options.maxRetries;
    }
    if(options && options.retryExponentialBackoff) {
      this.retryExponentialBackoff = options.retryExponentialBackoff;
    }
    this.initConnection()
  }

  public destroyConnection(): void {
    if (this.transport && !this.transport.closed) {
      this.abortController.abort();
      this.transport.close();
      this.transport = null;
      this.attempts = 0;
      this.url = null;
      this.isConnectedWebTransport = false
      console.log('WebTransport connection destroy');
    }
  }

  public async initConnection(): Promise<void> {
    try {
      if(!this.url) {
        throw new Error("Missing URL for WebTransport connection")
      }
      console.log('WebTransport init connection');
      this.transport = new WebTransport(this.url);
      await this.transport.ready;
      this.isConnectedWebTransport = true
      console.log('WebTransport connection established');
    } catch (error) {
      const errorMessage = `
        WebTransport connection error.
        Reason: ${(error as any).message}.
        Source: ${(error as any).source}.
        Error code: ${(error as any).streamErrorCode}.
      `;
      console.error(errorMessage)
      this.isConnectedWebTransport = false
      //await this.retryConnection()
    }
  }

  private async retryConnection(): Promise<void> {
    try {
      this.attempts += 1
      console.log(`New reconnection attempt #${this.attempts}`)
      await this.initConnection()
    } catch (err) {
      if (this.attempts < this.maxRetries) {
        const delay = this.retryExponentialBackoff * Math.pow(2, this.attempts - 1);
        console.log(`Next reconnection attempt in ${delay} ms`)
        await wait(delay);
        await this.retryConnection()
      } else {
        console.error('The number of reconnection attempts has expired')
        throw err
      }
    }
  }
}
