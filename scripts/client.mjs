import { build, context } from "esbuild";

const options = {
  entryPoints: ["src/Patchbook/Client/app.js"],
  outfile: "src/Patchbook/wwwroot/app.js",
  bundle: true,
  minify: true,
  format: "iife",
  target: "es2022",
  logLevel: "info",
};

if (process.argv.includes("--watch")) {
  const builder = await context(options);
  await builder.watch();
} else {
  await build(options);
}
