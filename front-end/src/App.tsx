import clsx from "clsx";
import React, { useEffect } from "react";
import { Separator } from "radix-ui";

import { DeckDisplay } from "./components/DeckDisplay";
import { BoardUpdateEvent, SseEvent } from "./types/SseEvent";
import { installLogConfig, useDevices } from "./utils/hooks";

export const App = () => {
  const [deviceId, setDeviceId] = React.useState<string>("");
  const [board, setBoard] = React.useState<BoardUpdateEvent | undefined>();

  const { data: devices } = useDevices();

  useEffect(() => {
    if (deviceId) {
      const eventSource = new EventSource(`/api/tracker/${deviceId}`);
      eventSource.onmessage = (e: MessageEvent<string>) => {
        const data = JSON.parse(e.data) as SseEvent;
        if (data.$type === "boardUpdate") {
          setBoard(data);
        }
      };
      return () => eventSource.close();
    }
  }, [deviceId]);

  if (deviceId && board && board.state === "running") {
    return (
      <div className="flex min-h-full flex-col bg-parchment text-near-black">
        <div className="flex w-full flex-1 flex-row gap-4 p-5">
          <DeckDisplay title="You" cards={board.playerDeck} />
          <DeckDisplay title="Opponent" cards={board.opponentDeck} />
        </div>
      </div>
    );
  }

  if (deviceId) {
    return (
      <Shell>
        <Card>
          <Eyebrow>PaddingStove</Eyebrow>
          <h1 className="font-serif text-[36px] font-medium leading-tight text-near-black">
            <span className="mr-2 inline-block h-2 w-2 rounded-full bg-terracotta align-middle animate-pulse-dot" />
            Waiting for game start
          </h1>
          <p className="font-sans text-[17px] leading-relaxed text-olive-gray">
            Listening to your iPad. Open Hearthstone and begin a match — the
            tracker will appear here as soon as the first card is drawn.
          </p>
        </Card>
      </Shell>
    );
  }

  return (
    <Shell>
      <Card>
        <Eyebrow>PaddingStove</Eyebrow>
        <h1 className="font-serif text-[36px] font-medium leading-tight text-near-black">
          Choose your device
        </h1>
        <p className="font-sans text-[17px] leading-relaxed text-olive-gray">
          Select an iPad from the list below to begin tracking your Hearthstone
          matches.
        </p>
        <Separator.Root className="h-px w-full bg-border-cream" />
        {devices === undefined ? (
          <EmptyState>Looking for devices…</EmptyState>
        ) : devices.length === 0 ? (
          <EmptyState>
            No devices detected. Connect an iPad over Wi-Fi sync and refresh.
          </EmptyState>
        ) : (
          <div className="flex flex-col gap-2.5">
            {devices.map((d, idx) => {
              const isIpad = d.deviceType === "iPad";
              const isPrimary = isIpad && idx === 0;
              return (
                <div key={d.id} className="flex flex-col gap-1.5">
                  <DeviceButton
                    primary={isPrimary}
                    disabled={!isIpad}
                    onClick={() => setDeviceId(d.id)}
                  >
                    <span className="font-sans text-[16px] font-medium">
                      {d.deviceName ?? d.id}
                    </span>
                    <span
                      className={clsx(
                        "font-sans text-[12px] tracking-[0.12px]",
                        isPrimary ? "text-parchment/85" : "text-stone-gray",
                      )}
                    >
                      {d.deviceType}
                      {!isIpad ? " · unsupported" : ""}
                    </span>
                  </DeviceButton>
                  {isIpad ? <InstallLogConfigAction deviceId={d.id} /> : null}
                </div>
              );
            })}
          </div>
        )}
      </Card>
    </Shell>
  );
};

const Shell = ({ children }: { children: React.ReactNode }) => (
  <div className="flex min-h-full flex-col bg-parchment text-near-black">
    <div className="flex flex-1 flex-col items-center justify-center px-6 py-12">
      {children}
    </div>
  </div>
);

const Card = ({ children }: { children: React.ReactNode }) => (
  <div className="flex w-full max-w-[520px] flex-col gap-5 rounded-2xl border border-border-cream bg-ivory p-8 shadow-whisper">
    {children}
  </div>
);

const Eyebrow = ({ children }: { children: React.ReactNode }) => (
  <div className="-mb-1 font-sans text-[10px] uppercase tracking-[0.5px] text-stone-gray">
    {children}
  </div>
);

const EmptyState = ({ children }: { children: React.ReactNode }) => (
  <div className="px-2 py-3 text-center font-sans text-[15px] text-stone-gray">
    {children}
  </div>
);

interface DeviceButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  primary?: boolean;
}

const DeviceButton = ({
  primary,
  className,
  children,
  ...rest
}: DeviceButtonProps) => (
  <button
    type="button"
    className={clsx(
      "flex w-full cursor-pointer flex-col items-start gap-0.5 rounded-xl border-0 px-4 py-3.5 text-left font-sans text-[16px] font-medium transition-shadow duration-150",
      primary
        ? "bg-terracotta text-ivory shadow-ring-terracotta hover:bg-coral"
        : "bg-sand text-charcoal-warm shadow-ring-warm hover:shadow-ring-deep",
      "disabled:cursor-not-allowed disabled:opacity-55",
      className,
    )}
    {...rest}
  >
    {children}
  </button>
);

type InstallStatus =
  | { kind: "idle" }
  | { kind: "running" }
  | { kind: "success" }
  | { kind: "error"; message: string };

const InstallLogConfigAction = ({ deviceId }: { deviceId: string }) => {
  const [status, setStatus] = React.useState<InstallStatus>({ kind: "idle" });

  const onClick = async () => {
    setStatus({ kind: "running" });
    try {
      const result = await installLogConfig(deviceId);
      setStatus(
        result.success
          ? { kind: "success" }
          : { kind: "error", message: result.error ?? "Install failed." },
      );
    } catch (e) {
      setStatus({
        kind: "error",
        message: e instanceof Error ? e.message : "Install failed.",
      });
    }
  };

  return (
    <div className="flex items-center justify-between px-1 font-sans text-[12px] text-stone-gray">
      <button
        type="button"
        onClick={onClick}
        disabled={status.kind === "running"}
        className="cursor-pointer border-0 bg-transparent p-0 text-left underline decoration-stone-gray/40 underline-offset-2 hover:text-charcoal-warm disabled:cursor-not-allowed disabled:opacity-60"
      >
        {status.kind === "running"
          ? "Installing log.config…"
          : "Install log.config on Hearthstone"}
      </button>
      {status.kind === "success" ? (
        <span className="text-charcoal-warm">Installed.</span>
      ) : status.kind === "error" ? (
        <span className="text-terracotta" title={status.message}>
          Failed
        </span>
      ) : null}
    </div>
  );
};
