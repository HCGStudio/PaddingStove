import { MantineProvider, createTheme } from "@mantine/core";
import "@mantine/core/styles.css";
import { createRoot } from "react-dom/client";

import { App } from "./App";

document.body.innerHTML = '<div id="app"></div>';

const theme = createTheme({});

const root = createRoot(document.getElementById("app")!);
root.render(
  <MantineProvider defaultColorScheme="light" theme={theme}>
    <App />
  </MantineProvider>,
);
