import { defineConfig, devices } from "@playwright/test";

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
    command: "dotnet run --project src/Patchbook --configuration Release --no-build --no-launch-profile --urls http://127.0.0.1:5081",
    url: "http://127.0.0.1:5081/health",
    reuseExistingServer: false,
    timeout: 60000,
    env: { ASPNETCORE_ENVIRONMENT: "Development" },
  },
});
