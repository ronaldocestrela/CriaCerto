window.plantelCache = (() => {
  const dbName = 'criacerto-plantel';
  const dbVersion = 1;
  const sowStore = 'sows';

  function openDb() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(dbName, dbVersion);
      request.onupgradeneeded = () => {
        const db = request.result;
        if (!db.objectStoreNames.contains(sowStore)) {
          const store = db.createObjectStore(sowStore, { keyPath: 'id' });
          store.createIndex('tagId', 'tagId', { unique: false });
          store.createIndex('reproductiveStatus', 'reproductiveStatus', { unique: false });
        }
      };
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }

  async function saveSows(sows) {
    const db = await openDb();
    const tx = db.transaction(sowStore, 'readwrite');
    const store = tx.objectStore(sowStore);
    store.clear();
    for (const sow of sows || []) {
      store.put({ ...sow, cachedAt: new Date().toISOString() });
    }
    await waitForTransaction(tx);
    db.close();
  }

  async function getSows(search, status) {
    const db = await openDb();
    const tx = db.transaction(sowStore, 'readonly');
    const request = tx.objectStore(sowStore).getAll();
    const rows = await waitForRequest(request);
    db.close();

    const query = (search || '').trim().toLowerCase();
    return rows.filter((row) => {
      const matchesSearch = !query ||
        (row.tagId || '').toLowerCase().includes(query) ||
        (row.nickname || '').toLowerCase().includes(query);
      const matchesStatus = !status || String(row.reproductiveStatus) === String(status);
      return matchesSearch && matchesStatus;
    });
  }

  function waitForRequest(request) {
    return new Promise((resolve, reject) => {
      request.onsuccess = () => resolve(request.result || []);
      request.onerror = () => reject(request.error);
    });
  }

  function waitForTransaction(tx) {
    return new Promise((resolve, reject) => {
      tx.oncomplete = () => resolve();
      tx.onerror = () => reject(tx.error);
      tx.onabort = () => reject(tx.error);
    });
  }

  return { saveSows, getSows };
})();
