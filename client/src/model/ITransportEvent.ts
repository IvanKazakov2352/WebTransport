export interface ITransportEvent<T> {
  sessionId: string;
  payload: T | null;
}
