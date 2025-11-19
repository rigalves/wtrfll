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
  if (new URL(request.url).pathname.startsWith('/api') || new URL(request.url).pathname.startsWith('/realtime')) {
    return;
  }
  const isSameOrigin = new URL(request.url).origin === self.location.origin;
  event.respondWith(
    caches.match(request).then((cachedResponse) => {
      if (cachedResponse) {
        return cachedResponse;
      }
      return fetch(request)
        .then((response) => {
          if (!response || response.status !== 200 || response.type !== 'basic' || !isSameOrigin) {
            return response;
          }
          const responseToCache = response.clone();
          caches.open(CACHE_NAME).then(async (cache) => {
            try {
              await cache.put(request, responseToCache);
            } catch (error) {
              if (error?.name === 'InvalidAccessError') {
                await cache.delete(request).catch(() => undefined);
                await cache.put(request, responseToCache).catch(() =>
                  console.warn('[wtrfll-sw] Unable to update cache entry', request.url, error),
                );
              } else {
                console.warn('[wtrfll-sw] Failed to update cache', request.url, error);
              }
            }
          });
          return response;
        })
        .catch(() => caches.match('/'));
    }),
  );
});
