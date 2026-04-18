import { ICardContent } from "../types/SseEvent";
import { useCards } from "../utils/hooks";
import { CardTileDisplay } from "./CardTileDisplay";

export interface IDeckDisplayProps {
  cards: ICardContent[];
  title?: string;
}

export const DeckDisplay = ({ cards, title }: IDeckDisplayProps) => {
  const { data } = useCards("latest");

  const enriched =
    data &&
    cards
      .filter((c) => data[c.id])
      .map((c) => ({ ...c, card: data[c.id] }))
      .sort((a, b) => {
        const costCompare = a.card.cost - b.card.cost;
        const nameCompare =
          a.card.name > b.card.name ? 1 : a.card.name < b.card.name ? -1 : 0;
        return costCompare === 0 ? nameCompare : costCompare;
      });

  const remaining = enriched?.reduce((sum, c) => sum + c.count, 0) ?? 0;

  return (
    <div className="flex min-h-0 flex-1 flex-col overflow-hidden rounded-2xl border border-border-cream bg-ivory shadow-whisper">
      {title && (
        <div className="flex items-baseline justify-between border-b border-border-cream px-5 pb-3 pt-4">
          <h2 className="font-serif text-[20.8px] font-medium leading-tight text-near-black">
            {title}
          </h2>
          <span className="font-sans text-[12px] tracking-[0.12px] text-stone-gray">
            {remaining} cards left
          </span>
        </div>
      )}
      <div className="flex flex-col gap-1 overflow-y-auto px-4 pb-4 pt-3">
        {enriched && enriched.length > 0 ? (
          enriched.map((c) => (
            <CardTileDisplay
              key={c.id}
              id={c.id}
              cost={c.card.cost}
              name={c.card.name}
              rarity={c.card.rarity as any}
              count={c.count}
            />
          ))
        ) : (
          <div className="px-4 py-6 text-center font-sans text-[14px] text-stone-gray">
            No cards revealed yet.
          </div>
        )}
      </div>
    </div>
  );
};
