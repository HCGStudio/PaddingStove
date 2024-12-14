import useSWR from "swr";

export type CardClass =
  | "DEATHKNIGHT"
  | "DRUID"
  | "HUNTER"
  | "MAGE"
  | "PALADIN"
  | "PRIEST"
  | "ROGUE"
  | "SHAMAN"
  | "WARLOCK"
  | "WARRIOR"
  | "DREAM"
  | "NEUTRAL"
  | "WHIZBANG"
  | "DEMONHUNTER";

export interface ICardDbInfo {
  dbfId: number;
  id: string;
  name: string;
  rarity: string;
  cost: number;
  set: string;
  text: string;
  cardClass: CardClass;
}

export interface IDeviceInfo {
  id: string;
  canConnect: boolean;
  deviceName?: string;
  deviceType?: string;
}

const fetcher = (url: string) => fetch(url).then((r) => r.json());

const cardsFetcher = (url: string) => fetcher(url).then(mapCardDbInfo);

const mapCardDbInfo = (data: ICardDbInfo[]) => {
  let result: Record<string, ICardDbInfo> = {};
  for (const item of data) {
    result[item.id] = item;
  }
  return result;
};

export const useDevices = () => useSWR<IDeviceInfo[]>("/api/device", fetcher);

export const useCards = (version: string) =>
  useSWR<Record<string, ICardDbInfo>>(
    `https://api.hearthstonejson.com/v1/${version}/zhCN/cards.json`,
    cardsFetcher,
  );
