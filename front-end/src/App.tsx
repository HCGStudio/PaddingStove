import { makeStyles } from "@griffel/react";
import { Button, Text } from "@mantine/core";
import React, { useEffect } from "react";

import { DeckDisplay } from "./compoments/DeckDisplay";
import { BoardUpdateEvent, SseEvent } from "./types/SseEvent";
import { useDevices } from "./utils/hooks";

const useStyles = makeStyles({
  wrapper: {
    display: "flex",
    flexDirection: "column",
    rowGap: "10px",
    justifyContent: "center",
    alignItems: "center",
    height: "100%",
    "> button": {
      width: " 80%",
    },
  },
});

export const App = () => {
  const styles = useStyles();

  const [deviceId, setDeviceId] = React.useState<string>("");
  const [board, setBoard] = React.useState<BoardUpdateEvent | undefined>();

  const { data: devices } = useDevices();

  useEffect(() => {
    if (deviceId) {
      const eventSource = new EventSource(`/api/tracker/${deviceId}`);
      eventSource.onmessage = (e: MessageEvent<string>) => {
        console.log(e.data);
        const data = JSON.parse(e.data) as SseEvent;
        if (data.$type === "boardUpdate") {
          setBoard(data);
        }
      };
    }
  }, [deviceId]);

  console.log(board, 111);

  return deviceId ? (
    board && board.state === "running" ? (
      <DeckDisplay cards={board.playerDeck} />
    ) : (
      <div className={styles.wrapper}>
        <Text size="xl">Waiting for Game Start</Text>
      </div>
    )
  ) : (
    <div className={styles.wrapper}>
      <Text size="xl">Choose Your Device</Text>
      {devices?.map((d) => (
        <Button
          key={d.id}
          variant="filled"
          disabled={d.deviceType !== "iPad"}
          onClick={() => setDeviceId(d.id)}
        >
          {d.deviceName ?? d.id}({d.deviceType})
        </Button>
      )) ?? <Text>No Devices</Text>}
    </div>
  );
};
