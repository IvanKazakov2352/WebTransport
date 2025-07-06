export interface IWebTransportClient {
  initConnection(): Promise<void>;
  destroyConnection(): void;
}
