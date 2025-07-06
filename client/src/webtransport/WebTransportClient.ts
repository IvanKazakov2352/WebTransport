export class WebTransportClient {
  private transport!: WebTransport;
  private abortController: AbortController = new AbortController();

  constructor(url: string) {}
}
