export type KeepAliveEvent = {
  $type: "keepalive";
};

export interface ICardContent {
  id: string;
  count: number;
}
export type BoardUpdateEvent = {
  $type: "boardUpdate";
  state: "running" | "idle";
  playerDeck: ICardContent[];
  opponentDeck: ICardContent[];
};

export type SseEvent = KeepAliveEvent | BoardUpdateEvent;
