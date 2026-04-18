import { makeStyles } from "@griffel/react";
import { Text } from "@mantine/core";

import { ICardContent } from "../types/SseEvent";
import { useCards } from "../utils/hooks";
import { CardTileDisplay } from "./CardTileDisplay";

export interface IDeckDisplayProps {
  cards: ICardContent[];
  title?: string;
}

const useStyles = makeStyles({
  wrapper: {
    display: "flex",
    flexDirection: "column",
    height: "auto",
    alignItems: "center",
    overflowY: "auto",
    margin: "10px 20px",
    flex: "1 1 0",
    "> div": {
      width: "220px",
    },
  },
  title: {
    marginBottom: "6px",
  },
});

export const DeckDisplay = ({ cards, title }: IDeckDisplayProps) => {
  const styles = useStyles();
  const { data } = useCards("latest");
  return (
    <div className={styles.wrapper}>
      {title && (
        <Text className={styles.title} size="lg" fw={700}>
          {title}
        </Text>
      )}
      {data &&
        cards
          .filter((c) => data[c.id])
          .map((c) => {
            return { ...c, card: data[c.id] };
          })
          .sort((a, b) => {
            const costCompare = a.card.cost - b.card.cost;
            const nameCompare =
              a.card.name > b.card.name
                ? 1
                : a.card.name < b.card.name
                  ? -1
                  : 0;
            return costCompare === 0 ? nameCompare : costCompare;
          })
          .map((c) => (
            <CardTileDisplay
              key={c.id}
              id={c.id}
              cost={c.card.cost}
              name={c.card.name}
              rarity={c.card.rarity as any}
              count={c.count}
            />
          ))}
    </div>
  );
};
