import { defineConfig } from "vitest/config";
import { fileURLToPath } from "url";
import { URL } from "url";

export default defineConfig({
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  test: {
    environment: "jsdom",
    setupFiles: ["./vitest.setup.ts"],
    globals: true,
    css: true,
    coverage: {
      provider: "v8",
      reportsDirectory: "./coverage",
      reporter: ["text", "html", "lcov"],
      exclude: ["**/*.d.ts", "**/node_modules/**", "**/.next/**"],
    },
  },
});


