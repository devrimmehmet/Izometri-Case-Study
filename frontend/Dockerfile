# ── Build stage ──────────────────────────────────────────────────────────────
# Use node:22 (Debian) so @resvg/resvg-js NAPI linux-x64-gnu binaries install
# without requiring extra system packages.
FROM node:22 AS build

# Inject the current git commit SHA so each build produces a unique SW revision,
# guaranteeing browser cache is busted on every deploy.
ARG VITE_COMMIT_HASH=unknown
ENV VITE_COMMIT_HASH=${VITE_COMMIT_HASH}

WORKDIR /app

# 1. Install dependencies first (layer-cached when package.json is unchanged).
#    --ignore-scripts skips quasar prepare here; it needs quasar.config.ts which
#    arrives in the next COPY step.
#    npm install (not npm ci) because @resvg/resvg-js optional platform binaries
#    differ between Windows (dev) and Linux (CI/Docker), causing lockfile mismatch.
COPY package.json package-lock.json ./
RUN npm install --ignore-scripts

# 2. Copy full source (node_modules and dist excluded via .dockerignore).
COPY . .

# 3. quasar prepare needs quasar.config.ts → run after COPY.
#    prebuild (generate-icons.mjs) generates DMP PNG icons from icon-source.svg.
#    quasar build -m pwa outputs to dist/pwa/.
RUN npx quasar prepare && npm run build

# ── Final stage ───────────────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS final

COPY --from=build /app/dist/pwa /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=3s --retries=3 \
  CMD wget -qO- http://127.0.0.1/health/frontend || exit 1
