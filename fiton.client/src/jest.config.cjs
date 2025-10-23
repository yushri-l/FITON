export default {
  testEnvironment: "jsdom",
  transform: {
    "^.+\\.(js|jsx|ts|tsx)$": ["@swc/jest"],
  },
  moduleFileExtensions: ["js", "jsx", "ts", "tsx", "json"],
  setupFilesAfterEnv: ["<rootDir>/src/setupTests.js"],
  testPathIgnorePatterns: ["/node_modules/", "/dist/"],
};
