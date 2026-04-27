/**
 * Generates all required PWA + favicon icon sizes from icon-source.svg.
 * Runs automatically via the "prebuild" npm script before every quasar build.
 * Uses @resvg/resvg-js (pure NAPI, no system-level librsvg needed).
 */

import { Resvg } from '@resvg/resvg-js';
import { readFileSync, writeFileSync, mkdirSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));

const svgSource = readFileSync(join(__dirname, 'icon-source.svg'), 'utf8');

// Safari pinned-tab uses a single-colour (pure black) mask SVG.
const safariSvg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">
  <text x="256" y="326"
    font-family="'Arial Black','Arial Bold',Arial,sans-serif"
    font-size="208" font-weight="900"
    text-anchor="middle" fill="#000000" letter-spacing="-6">DMP</text>
</svg>`;

const pngIcons = [
  // Favicon sizes
  { path: 'public/icons/favicon-16x16.png',   size: 16  },
  { path: 'public/icons/favicon-32x32.png',   size: 32  },
  { path: 'public/icons/favicon-96x96.png',   size: 96  },
  { path: 'public/icons/favicon-128x128.png', size: 128 },
  // Apple touch icons — Quasar auto-injects all four in PWA HTML
  { path: 'public/icons/apple-icon-120x120.png', size: 120 },
  { path: 'public/icons/apple-icon-152x152.png', size: 152 },
  { path: 'public/icons/apple-icon-167x167.png', size: 167 },
  { path: 'public/icons/apple-icon-180x180.png', size: 180 },
  // MS tile
  { path: 'public/icons/ms-icon-310x310.png', size: 310 },
  // PWA manifest icons
  { path: 'public/icons/icon-128x128.png',   size: 128 },
  { path: 'public/icons/icon-192x192.png',   size: 192 },
  { path: 'public/icons/icon-256x256.png',   size: 256 },
  { path: 'public/icons/icon-384x384.png',   size: 384 },
  { path: 'public/icons/icon-512x512.png',   size: 512 },
  { path: 'public/icons/maskable_icon_x512.png', size: 512 },
  // Root favicon (PNG served as .ico — accepted by all modern browsers)
  { path: 'public/favicon.ico', size: 32 },
];

mkdirSync(join(__dirname, 'public', 'icons'), { recursive: true });

for (const { path: relPath, size } of pngIcons) {
  const resvg = new Resvg(svgSource, { fitTo: { mode: 'width', value: size } });
  const png = resvg.render().asPng();
  writeFileSync(join(__dirname, relPath), png);
  console.log(`✓ ${relPath} (${size}x${size})`);
}

// Safari pinned-tab: monochrome SVG mask (black DMP text, transparent bg)
const safariDest = join(__dirname, 'public/icons/safari-pinned-tab.svg');
writeFileSync(safariDest, safariSvg, 'utf8');
console.log('✓ public/icons/safari-pinned-tab.svg (monochrome mask)');

// Also keep SVG in public root so <link rel="icon" type="image/svg+xml"> resolves
const svgDest = join(__dirname, 'public/icon-source.svg');
writeFileSync(svgDest, svgSource, 'utf8');
console.log('✓ public/icon-source.svg (SVG favicon)');

console.log(`\nAll ${pngIcons.length + 2} icon assets generated from icon-source.svg`);
