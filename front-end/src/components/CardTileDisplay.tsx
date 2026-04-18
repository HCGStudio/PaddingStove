import { makeStyles, mergeClasses } from "@griffel/react";
import { Text } from "@mantine/core";

export interface ICardTileDisplayProps {
  id: string;
  cost: number;
  name: string;
  rarity: "COMMON" | "RARE" | "EPIC" | "LEGENDARY";
  count: number;
}

const useStyles = makeStyles({
  wrapper: {
    display: "flex",
    overflow: "hidden",
    position: "relative",
    width: "100%",
    backgroundColor: "gray",
    border: "1px solid black",
    fontWeight: "bold",
  },
  gem: {
    fontSize: "1.3em",
    flex: "0 0 auto",
    height: "34px",
    width: "34px",
    lineHeight: "34px",
    textAlign: "center",
    float: "left",
    position: "relative",
    borderRight: "solid 1px black",
    color: "white",
    textShadow:
      "rgb(0, 0, 0) -1px -1px 0px, rgb(0, 0, 0) 1px -1px 0px, rgb(0, 0, 0) -1px 1px 0px, rgb(0, 0, 0) 1px 1px 0px",
  },
  COMMON: {
    backgroundColor: "#858585",
  },
  RARE: {
    backgroundColor: "#315376",
  },
  EPIC: {
    backgroundColor: "#644C82",
  },
  LEGENDARY: {
    backgroundColor: "#855C25",
  },
  tile: {
    textAlign: "left",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
    padding: "0px 6px",
    flex: "1 0 0px",
    overflow: "hidden",
    backgroundColor: "#313131",
    backgroundPosition: "right center",
    backgroundSize: "contain",
    backgroundRepeat: "no-repeat",
  },
  count: {
    flex: "0 0 auto",
    fontSize: "1.1em",
    color: "gold",
    textAlign: "center",
    lineHeight: "34px",
    width: "24px",
    backgroundColor: "#313131",
    borderLeft: "solid 1px black",
  },
  tileText: {
    fontSize: "0.8em",
    height: "34px",
    lineHeight: "34px",
    color: "white",
    fontWeight: "bold",
    textShadow:
      "rgb(0, 0, 0) -1px -1px 0px, rgb(0, 0, 0) 1px -1px 0px, rgb(0, 0, 0) -1px 1px 0px, rgb(0, 0, 0) 1px 1px 0px",
  },
  greyOutOverlay: {
    position: "absolute",
    top: "0",
    left: "0",
    width: "100%",
    height: "100%",
    backgroundColor: "rgba(0, 0, 0, 0.65)",
    zIndex: 100,
  },
  overlayHide: {
    display: "none",
  },
});

const getBackGroundImage = (id: string) =>
  `linear-gradient(
  65deg,
  rgb(49, 49, 30),
  rgb(49, 49, 49) calc(100% - 110px),
  rgba(49, 49, 49, 0) calc(100% - 46px),
  rgba(49, 49, 49, 0)),
  url("https://art.hearthstonejson.com/v1/tiles/${id}.png")`;

export const CardTileDisplay = ({
  id,
  cost,
  name,
  rarity,
  count,
}: ICardTileDisplayProps) => {
  const styles = useStyles();
  return (
    <div className={styles.wrapper}>
      <div className={mergeClasses(styles.gem, styles[rarity])}>{cost}</div>
      <div
        className={styles.tile}
        style={{ backgroundImage: getBackGroundImage(id) }}
      >
        <Text className={styles.tileText}>{name}</Text>
        <div
          className={mergeClasses(
            styles.greyOutOverlay,
            count > 0 ? styles.overlayHide : undefined,
          )}
        />
      </div>
      {count > 1 ? (
        <div className={styles.count}>{count}</div>
      ) : rarity === "LEGENDARY" ? (
        <div className={styles.count}>★</div>
      ) : null}
    </div>
  );
};
