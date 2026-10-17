import { defineConfig, devices } from "@playwright/test";
import { fileURLToPath } from "node:url";

export default defineConfig({
  testDir: "./tests/browser",
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: 0,
  use: {
    ...devices["Desktop Chrome"],
    baseURL: "http://127.0.0.1:5081",
    trace: "retain-on-failure",
  },
  webServer: {
    command: "dotnet Patchbook.dll --urls http://127.0.0.1:5081",
    cwd: fileURLToPath(new URL("./artifacts/publish", import.meta.url)),
    url: "http://127.0.0.1:5081/health",
    reuseExistingServer: false,
    timeout: 60000,
    env: { ASPNETCORE_ENVIRONMENT: "Production" },
  },
});
