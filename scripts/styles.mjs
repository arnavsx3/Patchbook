import { readFile, writeFile } from "node:fs/promises";
import { watch } from "node:fs";
import postcss from "postcss";
import tailwind from "@tailwindcss/postcss";

const input = "src/Patchbook/Styles/app.css";
const output = "src/Patchbook/wwwroot/app.css";
const processor = postcss([tailwind({ optimize: true })]);

async function build() {
  const result = await processor.process(await readFile(input, "utf8"), {
    from: input,
    to: output,
  });
  await writeFile(output, result.css);
  console.log("Tailwind styles built.");
}

await build();

if (process.argv.includes("--watch")) {
  let pending;
  let building = Promise.resolve();
  for (const folder of ["src/Patchbook/Components", "src/Patchbook/Styles"]) {
    watch(folder, { recursive: true }, () => {
      clearTimeout(pending);
      pending = setTimeout(() => {
        building = building.then(build).catch((error) => {
          console.error(error);
          process.exit(1);
        });
      }, 100);
    });
  }
}
