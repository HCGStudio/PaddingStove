import { createRoot } from "react-dom/client";

import { App } from "./App";
import "./global.css";

document.body.innerHTML = '<div id="app"></div>';

const root = createRoot(document.getElementById("app")!);
root.render(<App />);
