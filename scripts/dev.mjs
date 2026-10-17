import { spawn } from "node:child_process";

const children = [];
let stopping = false;

function stop(code = 0) {
  if (stopping) return;
  stopping = true;
  process.exitCode = code;
  for (const child of children) child.kill("SIGTERM");
}

function start(command, args) {
  const child = spawn(command, args, { stdio: "inherit" });
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

start("npm", ["run", "css:watch"]);
start("dotnet", ["watch", "--project", "src/Patchbook", "--non-interactive"]);
