import clsx from "clsx";

export interface ICardTileDisplayProps {
  id: string;
  cost: number;
  name: string;
  rarity: "COMMON" | "RARE" | "EPIC" | "LEGENDARY";
  count: number;
}

const rarityClass: Record<ICardTileDisplayProps["rarity"], string> = {
  COMMON: "bg-[#858585]",
  RARE: "bg-[#315376]",
  EPIC: "bg-[#644C82]",
  LEGENDARY: "bg-[#855C25]",
};

const tileTextShadow = {
  textShadow:
    "rgb(0,0,0) -1px -1px 0px, rgb(0,0,0) 1px -1px 0px, rgb(0,0,0) -1px 1px 0px, rgb(0,0,0) 1px 1px 0px",
};

const getBackgroundImage = (id: string) =>
  `linear-gradient(65deg, rgb(49,49,30), rgb(49,49,49) calc(100% - 110px), rgba(49,49,49,0) calc(100% - 46px), rgba(49,49,49,0)), url("https://art.hearthstonejson.com/v1/tiles/${id}.png")`;

export const CardTileDisplay = ({
  id,
  cost,
  name,
  rarity,
  count,
}: ICardTileDisplayProps) => (
  <div className="relative flex w-full overflow-hidden rounded-lg bg-dark-surface font-sans font-medium shadow-ring-deep">
    <div
      className={clsx(
        "relative h-[34px] w-[34px] flex-none border-r border-black/80 text-center text-[1.3em] leading-[34px] text-white",
        rarityClass[rarity],
      )}
      style={tileTextShadow}
    >
      {cost}
    </div>
    <div
      className="relative flex-1 overflow-hidden bg-no-repeat px-1.5 text-left"
      style={{
        backgroundImage: getBackgroundImage(id),
        backgroundPosition: "right center",
        backgroundSize: "contain",
      }}
    >
      <span
        className="block h-[34px] truncate text-[0.8em] font-medium leading-[34px] text-white"
        style={tileTextShadow}
      >
        {name}
      </span>
      {count <= 0 && (
        <div className="pointer-events-none absolute inset-0 z-[100] bg-black/65" />
      )}
    </div>
    {count > 1 ? (
      <div className="w-6 flex-none border-l border-dark-surface bg-near-black text-center text-[1.1em] leading-[34px] text-coral">
        {count}
      </div>
    ) : rarity === "LEGENDARY" ? (
      <div className="w-6 flex-none border-l border-dark-surface bg-near-black text-center text-[1.1em] leading-[34px] text-coral">
        ★
      </div>
    ) : null}
  </div>
);
