import { test, expect } from "@playwright/test";
import { readFile } from "node:fs/promises";

let browserErrors;
test.beforeEach(async ({ page }) => {
  browserErrors = [];
  page.on("pageerror", (error) => browserErrors.push(error.message));
  page.on("console", (message) => {
    if (message.type() === "error" && message.text().includes("circuit")) {
      browserErrors.push(message.text());
    }
  });
});
test.afterEach(() => expect(browserErrors).toEqual([]));

async function createEntry(page, title, { tags = "testing", code = "dotnet test", language = "text" } = {}) {
  await page.goto("/entries/new");
  await page.getByLabel("Title", { exact: false }).fill(title);
  await page.getByLabel("The error or problem", { exact: false }).fill("An unexpected error");
  await page.getByLabel("What fixed it", { exact: false }).fill("A repeatable solution");
  await page.getByLabel("Tags", { exact: false }).fill(tags);
  await page.getByLabel("Code snippet", { exact: false }).fill(code);
  await page.getByLabel("Code language", { exact: true }).selectOption(language);
  await page.getByRole("button", { name: "Save entry", exact: true }).click();
  await expect(page.getByRole("heading", { name: title, exact: true })).toBeVisible();
}

test("starts empty and only adds examples when requested", async ({ page, request }) => {
  await expect(await request.get("/health")).toBeOK();
  await page.goto("/");
  await expect(page.getByRole("heading", { name: "Start with your next “finally fixed it.”" })).toBeVisible();
  await expect(page.locator("article")).toHaveCount(0);
  await page.getByRole("button", { name: "Or load 3 example entries" }).click();
  await expect(page.locator("article")).toHaveCount(3);
  await page.reload();
  await expect(page.locator("article")).toHaveCount(3);
  await page.getByLabel("Search entries").fill("configuration indexer");
  await expect(page.locator("article")).toHaveCount(1);
  await page.getByRole("button", { name: "Clear filters" }).click();
  await expect(page.locator("article")).toHaveCount(3);
  await page.getByRole("button", { name: "#git", exact: true }).click();
  await expect(page.locator("article")).toHaveCount(1);
  await expect(page.getByRole("heading", { name: "Fix a typo in the last Git commit" })).toBeVisible();
  await page.getByRole("button", { name: "All tags", exact: true }).click();
  await page.getByLabel("Sort entries").selectOption("Title");
  await expect(page.locator("article h2").first()).toHaveText("Fix a typo in the last Git commit");
  await page.getByLabel("Search entries").fill("no-such-fix-xyz");
  await expect(page.getByRole("heading", { name: "No fixes found." })).toBeVisible();
});

test("creates, copies, edits, and deletes a fix", async ({ page, context }) => {
  await context.grantPermissions(["clipboard-read", "clipboard-write"]);
  await createEntry(page, "Browser CRUD fix", { tags: "CSharp, testing, csharp", code: "  dotnet test\n" });
  await expect(page.getByText("#csharp", { exact: true })).toHaveCount(1);
  await page.getByRole("button", { name: "Copy code", exact: true }).click();
  await expect(page.getByRole("status")).toHaveText("Code copied to clipboard.");
  await expect.poll(() => page.evaluate(() => navigator.clipboard.readText())).toBe("  dotnet test\n");
  await page.getByRole("link", { name: "Edit entry" }).click();
  await page.getByLabel("Title", { exact: false }).fill("Updated browser fix");
  await page.getByLabel("What fixed it", { exact: false }).fill("An improved solution");
  await page.getByRole("button", { name: "Save entry", exact: true }).click();
  await expect(page.getByRole("heading", { name: "Updated browser fix" })).toBeVisible();
  await expect(page.getByText("An improved solution", { exact: true })).toBeVisible();
  await page.getByRole("button", { name: "Delete entry", exact: true }).click();
  await expect(page.getByRole("dialog")).toBeVisible();
  await page.getByRole("button", { name: "Keep entry" }).click();
  await expect(page.getByRole("dialog")).not.toBeVisible();
  await page.getByRole("button", { name: "Delete entry", exact: true }).click();
  await page.getByRole("button", { name: "Delete permanently" }).click();
  await expect(page).toHaveURL("/");
  await page.getByLabel("Search entries").fill("Updated browser fix");
  await expect(page.getByRole("heading", { name: "No fixes found." })).toBeVisible();
});

test("validates required fields, tag limits, and reference links", async ({ page }) => {
  await page.goto("/entries/new");
  await page.getByRole("button", { name: "Save entry", exact: true }).click();
  await expect(page.getByText("Give this fix a title.", { exact: true })).toBeVisible();
  await expect(page.getByText("Describe the error or problem.", { exact: true })).toBeVisible();
  await expect(page.getByText("Write down what fixed it.", { exact: true })).toBeVisible();
  await page.getByLabel("Title", { exact: false }).fill("Validation fix");
  await page.getByLabel("The error or problem", { exact: false }).fill("Error");
  await page.getByLabel("What fixed it", { exact: false }).fill("Fix");
  await page.getByLabel("Tags", { exact: false }).fill("a,b,c,d,e,f,g,h,i");
  await page.getByLabel("Reference link", { exact: false }).fill("javascript:alert(1)");
  await page.getByRole("button", { name: "Save entry", exact: true }).click();
  await expect(page.getByText("Use up to 8 tags, each 24 characters or fewer.")).toBeVisible();
  await expect(page.getByText("Use a full http:// or https:// reference link.")).toBeVisible();
  await expect(page).toHaveURL("/entries/new");
});

test("renders user content as text and opens valid references safely", async ({ page }) => {
  const title = "<img src=x onerror=alert(1)>";
  await createEntry(page, title, { code: "<script>alert('hello')</script>" });
  await expect(page.locator("article img")).toHaveCount(0);
  await expect(page.locator("article script")).toHaveCount(0);
  await expect(page.locator("pre code")).toHaveText("<script>alert('hello')</script>");
  await page.getByRole("link", { name: "Edit entry" }).click();
  await page.getByLabel("Reference link", { exact: false }).fill("https://learn.microsoft.com/dotnet/");
  await page.getByRole("button", { name: "Save entry", exact: true }).click();
  const reference = page.getByRole("link", { name: "https://learn.microsoft.com/dotnet/" });
  await expect(reference).toHaveAttribute("target", "_blank");
  await expect(reference).toHaveAttribute("rel", "noopener noreferrer");
});

test("protects unsaved edits when navigating away", async ({ page }) => {
  await page.goto("/entries/new");
  await page.getByLabel("Title", { exact: false }).fill("Unsaved fix");
  await page.getByLabel("The error or problem", { exact: false }).focus();
  page.once("dialog", (dialog) => dialog.dismiss());
  await page.getByRole("link", { name: "Back to notebook" }).click();
  await expect(page).toHaveURL("/entries/new");
  await expect(page.getByLabel("Title", { exact: false })).toHaveValue("Unsaved fix");
  page.once("dialog", (dialog) => dialog.accept());
  await page.getByRole("link", { name: "Back to notebook" }).click();
  await expect(page).toHaveURL("/");
});

test("mobile layout fits, long code scrolls, and delete dialog supports Escape", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await createEntry(page, "Mobile fix", { code: "x".repeat(300) });
  await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
  await page.getByRole("button", { name: "Delete entry", exact: true }).click();
  await expect(page.getByRole("dialog")).toBeVisible();
  await expect(page.getByRole("button", { name: "Keep entry" })).toBeFocused();
  await page.keyboard.press("Escape");
  await expect(page.getByRole("dialog")).not.toBeVisible();
  await page.getByRole("link", { name: "Back to notebook" }).click();
  await expect(page.getByRole("heading", { name: "Your fixes, all in one place." })).toBeVisible();
  await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});

test("handles missing entries and unknown routes", async ({ page }) => {
  await page.goto("/entries/00000000-0000-0000-0000-000000000001");
  await expect(page.getByRole("heading", { name: "This entry isn’t here." })).toBeVisible();
  await page.goto("/entries/00000000-0000-0000-0000-000000000001/edit");
  await expect(page.getByRole("heading", { name: "This entry isn’t here." })).toBeVisible();
  await page.goto("/missing-page");
  await expect(page.getByRole("heading", { name: "A page out of place." })).toBeVisible();
});

test("pins entries to the top and keeps pins after editing", async ({ page }) => {
  await createEntry(page, "ZZZ pinned fix");
  await page.getByRole("button", { name: "Pin ZZZ pinned fix", exact: true }).click();
  await expect(page.getByRole("button", { name: "Unpin ZZZ pinned fix", exact: true })).toHaveAttribute("aria-pressed", "true");
  await page.getByRole("link", { name: "Edit entry" }).click();
  await page.getByLabel("What fixed it", { exact: false }).fill("Updated pinned solution");
  await page.getByRole("button", { name: "Save entry", exact: true }).click();
  await expect(page.getByRole("button", { name: "Unpin ZZZ pinned fix", exact: true })).toBeVisible();
  await page.getByRole("link", { name: "Back to notebook" }).click();
  await page.getByLabel("Sort entries").selectOption("Title");
  await expect(page.locator("article h2").first()).toHaveText("ZZZ pinned fix");
  await page.reload();
  await expect(page.locator("article h2").first()).toHaveText("ZZZ pinned fix");
  await page.getByRole("button", { name: "Unpin ZZZ pinned fix", exact: true }).click();
  await expect(page.getByRole("button", { name: "Pin ZZZ pinned fix", exact: true })).toHaveAttribute("aria-pressed", "false");
});


test("exports a real backup and restores a deleted pinned entry without duplicates", async ({ page }) => {
  await createEntry(page, "Backup browser fix", { code: "  preserve this\n" });
  const originalUrl = page.url();
  await page.getByRole("button", { name: "Pin Backup browser fix", exact: true }).click();
  await page.getByRole("link", { name: "Back to notebook" }).click();
  const pendingDownload = page.waitForEvent("download");
  await page.getByRole("button", { name: "Export backup", exact: true }).click();
  const download = await pendingDownload;
  const bytes = await readFile(await download.path());
  const backup = JSON.parse(bytes.toString());
  expect(backup.format).toBe("patchbook");
  expect(backup.entries.find(entry => entry.title === "Backup browser fix").isPinned).toBe(true);
  await page.goto(originalUrl);
  await page.getByRole("button", { name: "Delete entry", exact: true }).click();
  await page.getByRole("button", { name: "Delete permanently" }).click();
  await page.getByLabel("Import backup", { exact: true }).setInputFiles({ name: "backup.json", mimeType: "application/json", buffer: bytes });
  await expect(page.getByRole("status")).toContainText("Imported 1 entries.");
  await page.getByLabel("Import backup", { exact: true }).setInputFiles({ name: "backup.json", mimeType: "application/json", buffer: bytes });
  await expect(page.getByRole("status")).toContainText("Imported 0 entries.");
  await page.goto(originalUrl);
  await expect(page.getByRole("heading", { name: "Backup browser fix", exact: true })).toBeVisible();
  await expect(page.getByRole("button", { name: "Unpin Backup browser fix", exact: true })).toBeVisible();
  await expect(page.locator("pre code")).toHaveText("  preserve this\n");
});

test("invalid and oversized backup files keep the notebook intact", async ({ page }) => {
  await page.goto("/");
  await expect(page.locator("article").first()).toBeVisible();
  const count = await page.locator("article").count();
  await page.getByLabel("Import backup", { exact: true }).setInputFiles({ name: "broken.json", mimeType: "application/json", buffer: Buffer.from("not JSON") });
  await expect(page.getByRole("status")).toContainText("Choose a valid Patchbook JSON export.");
  await expect(page.locator("article")).toHaveCount(count);
  await page.getByLabel("Import backup", { exact: true }).setInputFiles({ name: "large.json", mimeType: "application/json", buffer: Buffer.alloc(2 * 1024 * 1024 + 1) });
  await expect(page.getByRole("status")).toContainText("2 MB or smaller");
  await expect(page.locator("article")).toHaveCount(count);
});


test("highlights C# with line numbers while copying only the original code", async ({ page, context }) => {
  await context.grantPermissions(["clipboard-read", "clipboard-write"]);
  const code = 'var answer = 42;\nConsole.WriteLine("hello");';
  await createEntry(page, "Highlighted C# fix", { code, language: "csharp" });
  await expect(page.locator("pre code .hljs-keyword").first()).toBeVisible();
  await expect(page.locator("pre[aria-hidden=true]")).toHaveText("1\n2");
  await expect(page.locator("pre code")).toHaveJSProperty("textContent", code);
  await expect.poll(() => page.locator("pre code").evaluate(element =>
    getComputedStyle(element.querySelector(".hljs-keyword")).color !== getComputedStyle(element).color)).toBe(true);
  await page.getByRole("button", { name: "Copy code", exact: true }).click();
  await expect.poll(() => page.evaluate(() => navigator.clipboard.readText())).toBe(code);
});

test("HTML highlighting keeps markup inert and fits on mobile", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  const code = '<img src=x onerror=alert(1)>\n<script>alert("hello")</script>';
  await createEntry(page, "Highlighted HTML fix", { code, language: "html" });
  await expect(page.locator("pre code .hljs-tag").first()).toBeVisible();
  await expect(page.locator("article img, article script")).toHaveCount(0);
  await expect(page.locator("pre code")).toHaveJSProperty("textContent", code);
  await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});


test("combines language, tag, and search filters and shows solution matches", async ({ page }) => {
  await createEntry(page, "Search preview fix", { tags: "searchtest", language: "csharp" });
  await page.getByRole("link", { name: "Edit entry" }).click();
  await page.getByLabel("What fixed it", { exact: false }).fill("a".repeat(220) + "needle42" + "z".repeat(220));
  await page.getByRole("button", { name: "Save entry", exact: true }).click();
  await page.getByRole("link", { name: "Back to notebook" }).click();
  await page.getByLabel("Filter by language").selectOption("csharp");
  await page.getByRole("button", { name: "#searchtest", exact: true }).click();
  await page.getByLabel("Search entries").fill("NEEDLE42");
  await expect(page.locator("article")).toHaveCount(1);
  await expect(page.locator("article mark")).toHaveText("needle42");
  await expect(page.locator("article p")).toContainText("Fix:");
  await page.getByLabel("Filter by language").selectOption("bash");
  await expect(page.getByRole("heading", { name: "No fixes found." })).toBeVisible();
  await page.getByRole("button", { name: "Clear filters", exact: true }).first().click();
  await expect(page.getByLabel("Filter by language")).toHaveValue("");
  await expect(page.getByLabel("Search entries")).toHaveValue("");
  await expect(page.getByRole("button", { name: "All tags", exact: true })).toHaveAttribute("aria-pressed", "true");
});

test("highlights literal HTML search text safely and fits filter controls on mobile", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto("/");
  await page.getByLabel("Search entries").fill("<img");
  await expect(page.locator("article")).toHaveCount(2);
  await expect(page.locator("article mark").first()).toHaveText("<img");
  await expect(page.locator("article img")).toHaveCount(0);
  await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
});
