import { spawn } from "node:child_process";

const children = [];
let stopping = false;

function stop(code = 0) {
  if (stopping) return;
  stopping = true;
  process.exitCode = code;
  function signalChildren(signal) {
    for (const child of children) {
      if (!child.pid) continue;
      try {
        if (process.platform === "win32") child.kill(signal);
        else process.kill(-child.pid, signal);
      } catch (error) {
        if (error.code !== "ESRCH") console.error(error.message);
      }
    }
  }
  // dotnet watch handles Ctrl+C as shutdown; SIGTERM can leave it watching.
  signalChildren("SIGINT");
  setTimeout(() => signalChildren("SIGKILL"), 2000).unref();
}

function start(command, args) {
  const child = spawn(command, args, {
    stdio: "inherit",
    detached: process.platform !== "win32",
  });
  children.push(child);
  child.on("error", (error) => {
    console.error(error.message);
    stop(1);
  });
  child.on("exit", (code) => {
    if (!stopping) stop(code ?? 1);
  });
}

process.on("SIGINT", () => stop());
process.on("SIGTERM", () => stop());

start(process.execPath, ["scripts/styles.mjs", "--watch"]);
start(process.execPath, ["scripts/client.mjs", "--watch"]);
start("dotnet", ["watch", "--project", "src/Patchbook", "--non-interactive"]);
