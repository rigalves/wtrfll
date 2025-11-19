const CACHE_NAME = 'wtrfll-cache-v2';
const CORE_ASSETS = ['/', '/index.html', '/manifest.webmanifest'];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches
      .open(CACHE_NAME)
      .then(async (cache) => {
        const uniqueAssets = Array.from(new Set(CORE_ASSETS));
        for (const asset of uniqueAssets) {
          const existing = await cache.match(asset);
          if (existing) {
            continue;
          }
          try {
            await cache.add(asset);
          } catch (error) {
            console.warn('[wtrfll-sw] Failed to cache asset', asset, error);
          }
        }
      })
      .then(() => self.skipWaiting()),
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches
      .keys()
      .then((keys) => Promise.all(keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key))))
      .then(() => self.clients.claim()),
  );
});

self.addEventListener('fetch', (event) => {
  const { request } = event;
  if (request.method !== 'GET') {
    return;
  }
  const url = new URL(request.url);
  if (url.origin !== self.location.origin) {
    return;
  }
  if (url.pathname.startsWith('/api') || url.pathname.startsWith('/realtime')) {
    return;
  }
  const isNavigation = request.mode === 'navigate';
  const isCoreAsset = CORE_ASSETS.includes(url.pathname);
  if (!isNavigation && !isCoreAsset) {
    return;
  }
  event.respondWith(
    caches.match(request).then((cachedResponse) => {
      if (cachedResponse) {
        return cachedResponse;
      }
      return fetch(request).catch(() => caches.match('/'));
    }),
  );
});
