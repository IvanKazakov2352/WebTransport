import { MessageTypes } from "./MessageTypes";

export interface ITransportEvent {
  sessionId: string;
  payload: Uint8Array<ArrayBufferLike> | null;
  messageType: MessageTypes;
}

export class TransportEvent implements ITransportEvent {
  public sessionId: string;
  public messageType: MessageTypes;
  public payload: Uint8Array<ArrayBufferLike> | null = null;

  constructor(
    sessionId: string, 
    messageType: MessageTypes, 
    payload?: Uint8Array<ArrayBufferLike> | null
  ) {
    this.sessionId = sessionId
    this.messageType = messageType
    if(payload !== null && payload !== undefined) {
      this.payload = payload
    }
  }
}
